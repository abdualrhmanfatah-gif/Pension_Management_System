// Diagnostic only: runs the actual uploadAll callback with controlled HTTP replies.
// No network, credentials, database writes, or production source changes.
// Models deferred uploads + exactly one final refresh per batch.
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import vm from 'node:vm';

const require = createRequire(new URL('../../ui/package.json', import.meta.url));
const ts = require('typescript');
const path = new URL('../../ui/src/components/domain/retirement/BulkUploadDialog.tsx', import.meta.url);
const source = ts.createSourceFile(path.pathname, readFileSync(path, 'utf8'), ts.ScriptTarget.Latest, true, ts.ScriptKind.TSX);
let callback;
function visit(node) {
    if (ts.isVariableDeclaration(node) && node.name.getText(source) === 'uploadAll') {
        assert.ok(ts.isCallExpression(node.initializer));
        callback = node.initializer.arguments[0].getText(source);
    }
    ts.forEachChild(node, visit);
}
visit(source);
assert.ok(callback, 'Actual uploadAll callback must exist');
const js = ts.transpileModule(`globalThis.runUpload = ${callback}`, {
    compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.None },
}).outputText;

async function scenario(count, responseMs, finalMs) {
    let now = 0, peak = 0, active = 0, saved = 0, nextId = 0;
    let files = Array.from({ length: count }, (_, i) => ({
        id: String(i + 1), status: 'pending', documentTypeID: 1,
        ownerEntity: 'retiree', notes: '', file: new Blob(['synthetic']),
    }));
    const requests = [], pending = [], timers = [];
    let refreshCalls = 0, refreshIds = null, refreshAt = -1;
    let refreshPending = null;
    const context = vm.createContext({
        AbortController, FormData, fileId: 10,
        isUploadingRef: { current: false }, filesRef: { current: files },
        uploadedIdsRef: { current: new Map() },
        abortRef: { current: null }, isMountedRef: { current: true },
        setUploading() {}, setResults() {},
        setFiles(update) { files = update(files); },
        toast: { error() {}, success() {} },
        onSaved() { saved++; }, onOpenChange() {},
        normalizeApiError() { throw new Error('Unexpected failed request'); },
        setTimeout(fn, delay) { timers.push({ at: now, delay }); },
        documentsApi: { upload(fd, signal, opts) {
            assert.equal(opts?.deferPostProcessing, true, 'uploads must defer file-level work');
            active++; peak = Math.max(peak, active);
            requests.push({ start: now });
            return new Promise(resolve => pending.push({ end: now + responseMs, resolve, id: ++nextId }));
        } },
        retirementApi: { refreshAfterUpload(fid, ids) {
            assert.equal(fid, 10);
            refreshCalls++;
            refreshIds = [...ids].sort((a, b) => a - b);
            refreshAt = now;
            return new Promise(resolve => { refreshPending = resolve; });
        } },
    });
    vm.runInContext(js, context);
    let done = false;
    const run = context.runUpload().then(() => { done = true; });
    for (let turns = 0; !done && turns < 1000; turns++) {
        await new Promise(resolve => setImmediate(resolve));
        if (pending.length) {
            now = Math.min(...pending.map(p => p.end));
            for (let i = pending.length - 1; i >= 0; i--) {
                if (pending[i].end === now) {
                    const [request] = pending.splice(i, 1);
                    active--; request.resolve({ documentID: request.id });
                }
            }
            continue;
        }
        if (refreshPending) {
            now = Math.max(now, refreshAt + finalMs);
            const resolve = refreshPending;
            refreshPending = null;
            resolve({ data: { processedDocuments: count } });
        }
    }
    assert.ok(done, 'Callback did not settle');
    await run;
    assert.equal(requests.length, count);
    assert.equal(refreshCalls, 1, 'Exactly one final refresh per batch');
    assert.deepEqual(refreshIds, Array.from({ length: count }, (_, i) => i + 1));
    assert.ok(files.every(f => f.status === 'success'));
    assert.equal(saved, 1, 'onSaved once per batch, only after final refresh');
    assert.equal(peak, Math.min(3, count));
    assert.equal(now, Math.ceil(count / 3) * responseMs + finalMs);
    assert.deepEqual(timers, [{ at: now, delay: 1500 }]);
    console.log(JSON.stringify({ count, simulatedResponseMs: responseMs, simulatedFinalMs: finalMs,
        peak, simulatedBatchMs: now, starts: requests.map(r => r.start), closeDelayMs: timers[0].delay }));
}
for (const count of [1, 3, 4, 6, 12]) await scenario(count, 1000, 500);
console.log('PASS: actual callback; deferred uploads; one final refresh; controlled responses, times are simulated, not production measurements.');
