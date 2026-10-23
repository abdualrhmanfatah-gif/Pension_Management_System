// Diagnostic harness: actual component callbacks, mocked hooks/API, deterministic timers.
// No HTTP requests, database writes, browser access, or changes to application files.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const ts = require('typescript');

const sourceRoot = path.resolve(__dirname, '../../src');
const sameDeps = (a, b) => a && b && a.length === b.length && a.every((v, i) => Object.is(v, b[i]));
const deferred = () => {
  let resolve, reject;
  const promise = new Promise((a, b) => { resolve = a; reject = b; });
  return { promise, resolve, reject };
};
const drain = async () => { for (let i = 0; i < 20; i++) await Promise.resolve(); };

async function fixture(options = {}) {
  let active, timerId = 0;
  const timers = new Map();
  const calls = { uploads: [], refresh: [], close: [], confirms: [], saved: 0, warnings: [], queries: [], ownerRetries: 0 };
  const queryState = { isError: false, isPending: false };
  const drafts = options.drafts ?? new Map();
  const auth = { user: { id: options.userId ?? 11 } };
  const listeners = new Map();
  const api = {
    upload: async (fd) => ({ documentID: calls.uploads.length }),
    refresh: async () => ({ isSuccess: true, data: { processedDocuments: 1, skippedDocumentIds: [] } }),
    owners: async () => [],
  };
  const slot = (init) => {
    const index = active.cursor++;
    if (!active.slots[index]) active.slots[index] = init();
    return active.slots[index];
  };
  const react = {
    memo: (fn) => fn,
    useState(init) {
      const record = active;
      const state = slot(() => ({ value: typeof init === 'function' ? init() : init }));
      return [state.value, (next) => {
        const value = typeof next === 'function' ? next(state.value) : next;
        if (!Object.is(value, state.value)) { state.value = value; record.dirty = true; }
      }];
    },
    useRef: (value) => slot(() => ({ current: value })),
    useId: () => react.useRef(`audit-${active.cursor}`).current,
    useMemo(fn, deps) {
      const memo = slot(() => ({}));
      if (!sameDeps(memo.deps, deps)) { memo.value = fn(); memo.deps = deps; }
      return memo.value;
    },
    useCallback: (fn, deps) => react.useMemo(() => fn, deps),
    useEffect(fn, deps) {
      const effect = slot(() => ({}));
      if (!sameDeps(effect.deps, deps)) {
        active.effects.push(() => { effect.cleanup?.(); effect.cleanup = fn(); });
        effect.deps = deps;
      }
    },
  };
  const ui = new Proxy({
    useConfirm: () => ({
      confirm: async (options) => { calls.confirms.push(options.title); return true; },
      ConfirmDialogElement: null,
    }),
  }, { get: (target, key) => key in target ? target[key] : String(key) });
  const jsx = (type, props) => ({ type, props: props ?? {} });
  const deps = {
    react,
    'react/jsx-runtime': { jsx, jsxs: jsx, Fragment: 'Fragment' },
    '@tanstack/react-query': { useQuery: (options) => {
      calls.queries.push(options);
      return {
        ...queryState,
        data: [{ value: 'retiree', label: 'المتقاعد' }],
        refetch: async () => { calls.ownerRetries++; },
      };
    } },
    'lucide-react': new Proxy({}, { get: (_target, key) => `icon:${String(key)}` }),
    sonner: { toast: { error() {}, success() {}, warning: (text) => calls.warnings.push(text) } },
    '@/api/documents': { documentsApi: { upload: (...args) => { calls.uploads.push(args); return api.upload(...args); } } },
    '@/api/retirement': { retirementApi: {
      refreshAfterUpload: (...args) => { calls.refresh.push(args); return api.refresh(...args); },
      getAgents: (...args) => api.owners(...args),
      getGuardians: (...args) => api.owners(...args),
      getEntitledPersons: (...args) => api.owners(...args),
    } },
    '@/components/ui': ui,
    '@/components/ui/Dialog': ui,
    '@/constants': { BADGE_VARIANT: {} },
    '@/constants/shortcuts': { useDialogKeyboard() {} },
    '@/hooks/useDialogReset': { useDialogReset() {} },
    '@/lib/errorUtils': { normalizeApiError: (error) => ({ message: error.message ?? 'فشل' }) },
    '@/lib/utils': { cn: (...args) => args.join(' ') },
    '@/lib/document-scanner/quality': { QUALITY_FLAG_LABELS: {} },
    '@/stores/authStore': { useAuthStore: (selector) => selector(auth) },
    './CameraScanDialog': { CameraScanDialog: 'CameraScanDialog' },
    './ErrorAlert': { ErrorAlert: 'ErrorAlert' },
    './FormDialogFooter': { FormDialogFooter: 'FormDialogFooter' },
  };
  function load(file) {
    const mod = { exports: {} };
    const code = ts.transpileModule(fs.readFileSync(file, 'utf8').replaceAll('import.meta.env.DEV', 'false'), {
      compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.CommonJS, jsx: ts.JsxEmit.ReactJSX },
    }).outputText;
    vm.runInNewContext(code, {
      exports: mod.exports, module: mod,
      require(name) {
        if (name in deps) return deps[name];
        if (name === './bulkUploadDuplicate') return load(path.join(sourceRoot, 'components/domain/retirement/bulkUploadDuplicate.ts'));
        throw new Error(`Unmocked dependency: ${name}`);
      },
      File, FormData, AbortController, URL, crypto, console,
      navigator: { onLine: true },
      setTimeout: (fn, delay) => { const id = ++timerId; timers.set(id, { fn, delay }); return id; },
      clearTimeout: (id) => timers.delete(id),
      requestAnimationFrame: () => ++timerId,
      cancelAnimationFrame() {},
      window: { history: { state: null, pushState() {}, back() {} },
        addEventListener(name, fn) { listeners.set(name, fn); },
        removeEventListener(name) { listeners.delete(name); } },
    }, { filename: file });
    return mod.exports;
  }
  function instance(component, props) {
    const record = { slots: [], cursor: 0, effects: [], dirty: true, props, tree: null };
    record.render = () => {
      for (let i = 0; i < 15; i++) {
        record.cursor = 0; record.effects = []; record.dirty = false; active = record;
        record.tree = component(record.props);
        for (const effect of record.effects) effect();
        if (!record.dirty) return record.tree;
      }
      throw new Error('Render loop');
    };
    record.render();
    return record;
  }
  function nodes(tree, type) {
    if (Array.isArray(tree)) return tree.flatMap((child) => nodes(child, type));
    if (!tree || typeof tree !== 'object') return [];
    const found = tree.type === type || tree.type?.name === type ? [tree] : [];
    if (typeof tree.type === 'function') return found.concat(nodes(tree.type(tree.props), type));
    return found.concat(nodes(tree.props?.children, type));
  }
  const storage = load(path.join(sourceRoot, 'lib/bulkUploadDraft.ts'));
  deps['@/lib/bulkUploadDraft'] = { ...storage,
    loadBulkUploadDraft: async (key) => {
      if (options.restoreError) throw new Error('storage unavailable');
      const draft = drafts.get(key);
      return draft && draft.expiresAt > Date.now() ? draft : null;
    },
    saveBulkUploadDraft: async (draft, key) => {
      if (options.saveError) throw new Error('quota exceeded');
      if (draft) drafts.set(key, structuredClone(draft)); else drafts.delete(key);
    },
  };
  const root = instance(load(path.join(sourceRoot, 'components/domain/retirement/BulkUploadDialog.tsx')).BulkUploadDialog, {
    open: true, fileId: options.fileId ?? 7, docTypes: [{ id: 1, name: 'وثيقة' }],
    onOpenChange: (value) => { calls.close.push(value); root.props.open = value; },
    onSaved: () => { calls.saved++; },
  });
  const add = (files) => { nodes(root.tree, 'FileUploader')[0].props.onChange(files); root.render(); };
  const setTypes = () => {
    nodes(root.tree, 'NativeSelect').filter((node) => node.props.options[0]?.value === '1').forEach((node) => node.props.onChange('1'));
    root.render();
  };
  const submit = () => root.tree.props.onSubmit();
  const submitButton = () => nodes(root.tree.props.footer, 'Button').find((node) => node.props.type === 'submit');
  const cancel = () => nodes(root.tree.props.footer, 'Button').find((node) => node.props.type === 'button').props.onClick();
  const flush = async () => { await drain(); root.render(); await drain(); root.render(); };
  await flush();
  return { root, api, calls, nodes, add, setTypes, submit, submitButton, cancel, timers, instance, load, queryState,
    drafts, auth, flush, listeners };
}

const makeFile = (name, content) => new File([content], name, { type: 'image/jpeg' });
async function check(name, run) {
  try { await run(); console.log(`PASS ${name}`); }
  catch (error) { console.log(`FAIL ${name}: ${error.message}`); process.exitCode = 1; }
}

(async () => {
  await check('control: normal upload refreshes actual document IDs once and reports success', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    await f.submit(); f.root.render();
    assert.equal(f.calls.uploads.length, 1);
    assert.deepEqual(Array.from(f.calls.refresh[0][1]), [1]);
    assert.equal(f.calls.saved, 1);
    assert.equal(f.nodes(f.root.tree, 'BulkFileRow')[0].props.item.status, 'success');
  });
  await check('control: failed finalization can be retried without uploading successful documents again', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    f.api.refresh = async () => { throw new Error('finalization failed'); };
    await f.submit(); f.root.render();
    assert.equal(f.submitButton().props.disabled, false);
    f.api.refresh = async () => ({ isSuccess: true, data: { processedDocuments: 1, skippedDocumentIds: [] } });
    await f.submit(); f.root.render();
    assert.equal(f.calls.uploads.length, 1);
    assert.equal(f.calls.refresh.length, 2);
    assert.equal(f.calls.saved, 1);
  });
  await check('control: a scope-matching duplicate response recovers its ID and finalizes it', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    f.api.upload = async () => { throw { response: { status: 409, data: { data: {
      documentID: 101, fileID: 7, documentTypeID: 1,
      retireeID: null, guardianID: null, agentID: null, entitledID: null,
    } } } }; };
    await f.submit(); f.root.render();
    assert.deepEqual(Array.from(f.calls.refresh[0][1]), [101]);
    assert.equal(f.calls.saved, 1);
  });
  await check('distinct content with the same filename and size must be accepted', async () => {
    const f = await fixture();
    f.add([makeFile('image.jpg', 'AAA'), makeFile('image.jpg', 'BBB')]);
    assert.equal(f.nodes(f.root.tree, 'BulkFileRow').length, 2);
  });
  await check('adding another file must prevent automatic close and loss of pending files', async () => {
    const f = await fixture();
    f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    await f.submit(); f.root.render();
    assert.equal(f.calls.saved, 1);
    f.add(makeFile('new.jpg', 'BBBB'));
    for (const timer of f.timers.values()) if (timer.delay === 1400) timer.fn();
    f.root.render();
    assert.equal(f.calls.close.length, 0, 'auto-close fired with a new pending file');
  });
  await check('cancel followed by removing remaining files must still allow finalization of successful uploads', async () => {
    const f = await fixture(); const slow = deferred();
    f.api.upload = (fd) => fd.get('File').name === 'second.jpg' ? slow.promise : Promise.resolve({ documentID: 101 });
    f.add([makeFile('first.jpg', 'AAA'), makeFile('second.jpg', 'BBBB')]); f.setTypes();
    const run = f.submit(); await drain(); f.root.render();
    assert.equal(f.nodes(f.root.tree, 'BulkFileRow').filter((node) => node.props.item.status === 'success').length, 1);
    f.cancel(); slow.reject(new Error('canceled')); await run; f.root.render();
    const remaining = f.nodes(f.root.tree, 'BulkFileRow').find((node) => node.props.item.fileName === 'second.jpg');
    remaining.props.onRemove(remaining.props.item.id); f.root.render();
    assert.equal(f.calls.refresh.length, 0, 'scenario must reproduce cancellation before finalization');
    assert.equal(f.submitButton().props.disabled, false, 'finalization is required but submit remains disabled');
    await f.submit(); f.root.render();
    assert.equal(f.calls.uploads.length, 2, 'successful documents must not be uploaded again');
    assert.deepEqual(Array.from(f.calls.refresh[0][1]), [101]);
    assert.equal(f.calls.saved, 1);
  });
  await check('closing a pending batch must ask for confirmation only once', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA'));
    const base = f.instance(f.load(path.join(sourceRoot, 'components/ui/BaseFormDialog.tsx')).BaseFormDialog, f.root.tree.props);
    f.nodes(base.tree, 'Dialog')[0].props.onOpenChange(false);
    await drain();
    assert.equal(f.calls.confirms.length, 1, JSON.stringify(f.calls.confirms));
  });
  await check('closing a completed batch must not ask about unsaved changes', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    await f.submit(); f.root.render();
    const base = f.instance(f.load(path.join(sourceRoot, 'components/ui/BaseFormDialog.tsx')).BaseFormDialog, f.root.tree.props);
    f.nodes(base.tree, 'Dialog')[0].props.onOpenChange(false); await drain();
    assert.equal(f.calls.confirms.length, 0);
    assert.equal(f.calls.close.length, 1);
  });
  await check('owner lookup must not silently succeed when all three owner endpoints fail', async () => {
    const f = await fixture();
    f.api.owners = async () => { throw new Error('network unavailable'); };
    await assert.rejects(f.calls.queries[0].queryFn());
  });
  await check('owner lookup failures block uploads and expose a retry action', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    assert.equal(f.submitButton().props.disabled, false);
    f.queryState.isError = true; f.root.render();
    assert.equal(f.submitButton().props.disabled, true);
    const owner = f.nodes(f.root.tree, 'NativeSelect').find((node) => node.props.options[0]?.value === 'retiree');
    assert.equal(owner.props.disabled, true);
    await f.submit(); assert.equal(f.calls.uploads.length, 0);
    const retry = f.nodes(f.root.tree, 'Button').find((node) => node.props.children === 'إعادة تحميل أصحاب الوثائق');
    assert.ok(retry); retry.props.onClick();
    assert.equal(f.calls.ownerRetries, 1);
  });
  await check('finalization must not report complete success when the server skipped the uploaded document', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    f.api.refresh = async () => ({ isSuccess: true, data: { processedDocuments: 0, skippedDocumentIds: [1] } });
    await f.submit(); f.root.render();
    assert.equal([...f.timers.values()].some((timer) => timer.delay === 1400), false,
      'auto-close is scheduled although finalization skipped the only uploaded document');
    assert.equal(f.nodes(f.root.tree, 'BulkFileRow')[0].props.item.status, 'error');
    assert.equal(f.calls.saved, 0);
    f.api.refresh = async () => ({ isSuccess: true, data: { processedDocuments: 1, skippedDocumentIds: [] } });
    await f.submit(); f.root.render();
    assert.equal(f.calls.uploads.length, 2);
    assert.deepEqual(Array.from(f.calls.refresh[1][1]), [2]);
    assert.equal(f.calls.saved, 1);
  });
  await check('a canceled old run must not reset uploads in a reopened dialog', async () => {
    const f = await fixture(); const old = deferred(); const next = deferred();
    f.api.upload = (fd) => fd.get('File').name === 'old.jpg' ? old.promise : next.promise;
    f.add(makeFile('old.jpg', 'AAA')); f.setTypes();
    const oldRun = f.submit(); await drain(); f.root.render();
    f.root.props.open = false; f.root.render();
    f.root.props.open = true; f.root.render();
    await f.flush();
    for (const row of f.nodes(f.root.tree, 'BulkFileRow')) row.props.onRemove(row.props.item.id);
    f.root.render();
    f.add(makeFile('new.jpg', 'BBBB')); f.setTypes();
    const newRun = f.submit(); await drain(); f.root.render();
    old.reject(new Error('canceled')); await oldRun; f.root.render();
    assert.equal(f.root.tree.props.isLoading, true);
    assert.equal(f.nodes(f.root.tree, 'BulkFileRow')[0].props.item.status, 'uploading');
    next.resolve({ documentID: 202 }); await newRun; f.root.render();
    assert.deepEqual(Array.from(f.calls.refresh[0][1]), [202]);
  });
  await check('an already queued auto-close must recheck pending files', async () => {
    const f = await fixture(); f.add(makeFile('first.jpg', 'AAA')); f.setTypes();
    await f.submit(); f.root.render();
    const queuedClose = [...f.timers.values()].find((timer) => timer.delay === 1400);
    assert.ok(queuedClose);
    f.add(makeFile('pending.jpg', 'BBBB'));
    queuedClose.fn(); f.root.render();
    assert.equal(f.calls.close.length, 0);
  });
  await check('reload restores file bytes, owner, type, notes and quality confirmation', async () => {
    const f = await fixture(); f.add(makeFile('retained.jpg', 'ABC')); f.setTypes();
    const row = f.nodes(f.root.tree, 'BulkFileRow')[0];
    row.props.onUpdateField(row.props.item.id, { ownerEntity: 'agent_9', notes: 'ملاحظة', needsReview: true,
      reviewConfirmed: true, qualityFlags: ['blur'] });
    f.root.render(); await f.flush();
    const g = await fixture({ drafts: f.drafts });
    const restored = g.nodes(g.root.tree, 'BulkFileRow')[0].props.item;
    assert.equal(await restored.file.text(), 'ABC');
    assert.equal(restored.fileName, 'retained.jpg');
    assert.equal(restored.ownerEntity, 'agent_9');
    assert.equal(restored.documentTypeID, 1);
    assert.equal(restored.notes, 'ملاحظة');
    assert.equal(restored.reviewConfirmed, true);
    assert.deepEqual(Array.from(restored.qualityFlags), ['blur']);
  });
  await check('interrupted batch restores successful IDs and retries only the unfinished file', async () => {
    const f = await fixture();
    f.add([makeFile('saved.jpg', 'A'), makeFile('retry.jpg', 'B')]); f.setTypes();
    f.api.upload = async (fd) => {
      if (fd.get('File').name === 'retry.jpg') throw new Error('network failed');
      return { documentID: 42 };
    };
    f.api.refresh = async () => { throw new Error('network failed'); };
    await f.submit(); f.root.render(); await f.flush();
    const g = await fixture({ drafts: f.drafts });
    g.api.upload = async () => ({ documentID: 43 });
    await g.submit(); g.root.render(); await g.flush();
    assert.equal(g.calls.uploads.length, 1);
    assert.equal(g.calls.uploads[0][0].get('File').name, 'retry.jpg');
    assert.deepEqual(Array.from(g.calls.refresh[0][1]).sort(), [42, 43]);
    assert.equal(g.drafts.size, 0, 'completed batches must release local file storage');
  });
  await check('drafts expire after one hour without extending their expiry on restore', async () => {
    const f = await fixture(); f.add(makeFile('expiry.jpg', 'A')); await f.flush();
    const draft = f.drafts.get('11:7');
    assert.ok(Math.abs(draft.expiresAt - Date.now() - 3600000) < 1000);
    const g = await fixture({ drafts: f.drafts }); await g.flush();
    assert.equal(g.drafts.get('11:7').expiresAt, draft.expiresAt);
    draft.expiresAt = Date.now() - 1;
    const h = await fixture({ drafts: f.drafts });
    assert.equal(h.nodes(h.root.tree, 'BulkFileRow').length, 0);
  });
  await check('another account or retirement file cannot restore the previous draft', async () => {
    const f = await fixture(); f.add(makeFile('private.jpg', 'A')); await f.flush();
    for (const options of [{ userId: 12 }, { fileId: 8 }]) {
      const g = await fixture({ ...options, drafts: f.drafts });
      assert.equal(g.nodes(g.root.tree, 'BulkFileRow').length, 0);
    }
    f.auth.user = { id: 12 }; f.root.render(); await f.flush();
    assert.equal(f.nodes(f.root.tree, 'BulkFileRow').length, 0);
  });
  await check('offline retains the batch and blocks requests until connection returns', async () => {
    const f = await fixture(); f.add(makeFile('offline.jpg', 'A')); f.setTypes();
    f.listeners.get('offline')(); f.root.render(); await f.flush();
    assert.equal(f.submitButton().props.disabled, true);
    await f.submit(); assert.equal(f.calls.uploads.length, 0);
    assert.equal(f.drafts.get('11:7').files.length, 1);
    f.listeners.get('online')(); f.root.render();
    assert.equal(f.submitButton().props.disabled, false);
    assert.equal(f.calls.uploads.length, 0, 'reconnect must not upload without a user action');
  });
  await check('storage failures expose an Arabic warning and still allow uploading', async () => {
    const f = await fixture({ restoreError: true, saveError: true });
    f.add(makeFile('quota.jpg', 'A')); f.setTypes(); await f.flush();
    const warning = f.nodes(f.root.tree, 'div').find((node) => node.props.role === 'alert');
    assert.ok(JSON.stringify(warning).includes('تعذر الحفظ المحلي'));
    assert.equal(f.submitButton().props.disabled, false);
    await f.submit(); assert.equal(f.calls.uploads.length, 1);
  });
})().catch((error) => { console.error(error); process.exitCode = 2; });

