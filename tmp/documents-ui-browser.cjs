// Browser verification against the production preview, with synthetic API responses.
// Run: node tmp/documents-ui-browser.cjs
const { spawn } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');

const origin = 'http://127.0.0.1:4175';
const output = path.resolve(__dirname, '../plan');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
const user = { id: 99999, fullName: 'مستخدم المعاينة', username: 'ui-preview', role: 'رافع الوثائق', branchId: 1, isActive: true };
const permissions = ['System.Admin', 'File.Documents.Upload', 'File.View.Sensitive'];
const names = ['محمد أحمد عبدالله عبدالكريم عبدالرحمن صالح', 'علي صالح أحمد', 'عبدالله محمد سالم'];
const rows = Array.from({ length: 20 }, (_, i) => ({
  fileID: i + 1, fileNumber: `2026-${String(4985 - i).padStart(5, '0')}`,
  retireeFullName: names[i % names.length], nationalID: null, rankName: null,
  fileStatusName: i % 2 ? 'جديد' : 'تحت المراجعة', fileStatusColor: i % 2 ? 'outline' : 'warning',
  missingDocumentsCount: i % 3 === 1 ? 2 : 0, hasDocuments: i % 3 !== 0,
  lastUploadedByUserID: i % 3 ? 1 : null, lastUploadedByUserName: i % 3 ? 'مدخل الوثائق الأول' : null,
  statusID: i % 2 ? 1 : 7,
}));

function responseFor(url) {
  const pathname = new URL(url).pathname.replace(/^\/api/, '');
  if (pathname === '/auth/me/permissions') return permissions;
  if (pathname === '/auth/me') return user;
  if (pathname === '/retirement-files/document-upload-stats') return { totalFiles: 4984, filesWithDocuments: 3900, filesWithoutDocuments: 1084, filesWithMissing: 126, totalMissingDocuments: 250 };
  if (pathname === '/retirement-files/uploader-stats') return [{ userID: 1, fullName: 'مدخل الوثائق الأول', userName: 'preview' }];
  if (pathname === '/file-statuses') return [{ statusID: 1, statusName: 'جديد' }, { statusID: 7, statusName: 'تحت المراجعة' }];
  if (pathname === '/retirement-files') return { items: rows, totalPages: 2, totalCount: 40 };
  if (pathname === '/notifications/unread-count') return 0;
  if (pathname === '/notifications') return { items: [], totalCount: 0, totalPages: 0, page: 1, pageSize: 20 };
  return [];
}

async function main() {
  const browser = spawn('C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', [
    '--headless=new', '--disable-gpu', '--no-first-run', '--no-default-browser-check',
    '--remote-debugging-port=9337', `--user-data-dir=${path.resolve(__dirname, 'documents-ui-browser-profile')}`, 'about:blank',
  ], { stdio: 'ignore', windowsHide: true });
  let ws;
  const pending = new Map();
  const requests = [], exceptions = [], measurements = [];
  let sequence = 0;
  try {
    let tabs;
    for (let i = 0; i < 80; i++) {
      try { tabs = await (await fetch('http://127.0.0.1:9337/json/list')).json(); if (tabs.some(t => t.type === 'page')) break; } catch {}
      await delay(100);
    }
    assert.ok(tabs, 'Edge debugging endpoint started');
    ws = new WebSocket(tabs.find(t => t.type === 'page').webSocketDebuggerUrl);
    await new Promise((resolve, reject) => { ws.onopen = resolve; ws.onerror = reject; });
    const send = (method, params = {}) => new Promise((resolve, reject) => {
      const id = ++sequence;
      const timer = setTimeout(() => { pending.delete(id); reject(new Error(`CDP timeout: ${method}`)); }, 20000);
      pending.set(id, { resolve, reject, timer });
      ws.send(JSON.stringify({ id, method, params }));
    });
    ws.onmessage = async event => {
      const msg = JSON.parse(event.data);
      if (msg.id) {
        const item = pending.get(msg.id);
        if (item) { pending.delete(msg.id); clearTimeout(item.timer); msg.error ? item.reject(new Error(JSON.stringify(msg.error))) : item.resolve(msg.result); }
      }
      if (msg.method === 'Runtime.exceptionThrown') exceptions.push(msg.params.exceptionDetails.text + ': ' + (msg.params.exceptionDetails.exception?.description || ''));
      if (msg.method === 'Fetch.requestPaused') {
        const { requestId, request } = msg.params;
        requests.push({ url: request.url, method: request.method });
        const body = request.url.includes('/hubs/')
          ? { error: 'الاتصال الفوري غير مستخدم في المعاينة' }
          : { isSuccess: true, data: responseFor(request.url), message: '' };
        try { await send('Fetch.fulfillRequest', { requestId, responseCode: 200, responseHeaders: [{ name: 'Content-Type', value: 'application/json' }, { name: 'Access-Control-Allow-Origin', value: origin }, { name: 'Access-Control-Allow-Credentials', value: 'true' }], body: Buffer.from(JSON.stringify(body)).toString('base64') }); } catch {}
      }
    };
    const evaluate = async expression => {
      const result = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
      if (result.exceptionDetails) throw new Error(result.exceptionDetails.exception?.description || result.exceptionDetails.text);
      return result.result.value;
    };
    const waitFor = async expression => {
      for (let i = 0; i < 80; i++) { if (await evaluate(expression)) return; await delay(100); }
      throw new Error(`Browser condition failed: ${expression}; ${await evaluate('document.body.innerText.slice(0,1200)')}`);
    };
    await send('Page.enable');
    await send('Runtime.enable');
    await send('Fetch.enable', { patterns: [{ urlPattern: '*/api/*' }, { urlPattern: '*/hubs/*' }] });
    const token = `${Buffer.from('{}').toString('base64url')}.${Buffer.from(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 86400 })).toString('base64url')}.preview`;
    await send('Page.addScriptToEvaluateOnNewDocument', { source: `localStorage.setItem('auth_token',${JSON.stringify(token)});localStorage.setItem('auth_user',${JSON.stringify(JSON.stringify(user))});localStorage.setItem('auth_permissions',${JSON.stringify(JSON.stringify(permissions))});window.visibleSelects=()=>[...document.querySelectorAll('main select')].filter(el=>el.getBoundingClientRect().width>0);` });
    await send('Page.navigate', { url: `${origin}/documents` });
    await waitFor("document.querySelector('main h1')?.textContent.includes('رفع الوثائق') && document.querySelectorAll('main select').length === 3");

    for (const width of [320, 360, 400, 640, 768, 1280]) {
      for (const theme of ['light', 'dark']) {
        await send('Emulation.setDeviceMetricsOverride', { width, height: 900, deviceScaleFactor: 1, mobile: false });
        await evaluate(`localStorage.setItem('theme-storage', JSON.stringify({state:{isDark:${theme === 'dark'}},version:0}))`);
        await send('Page.reload');
        await waitFor("document.querySelector('main h1')?.textContent.includes('رفع الوثائق') && document.querySelector('main select')?.options.length === 3");
        if (width < 640) {
          assert.equal(await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim().startsWith('الفلاتر')).getAttribute('aria-expanded')"), 'false', 'mobile filters initially collapsed');
          await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim().startsWith('الفلاتر')).click()");
        }
        await delay(450);
        const data = await evaluate(`(() => {
          const main = document.querySelector('main');
          const visible = el => el.getBoundingClientRect().width > 0 && el.getBoundingClientRect().height > 0 && !el.closest('[inert]');
          const rects = [...main.querySelectorAll('button,input,select,h1,h2,bdi')].filter(visible).filter(el=>!el.closest('table')).map(el => ({tag:el.tagName,text:el.textContent?.trim(),left:el.getBoundingClientRect().left,right:el.getBoundingClientRect().right,height:el.getBoundingClientRect().height}));
          return {width:innerWidth,theme:document.documentElement.classList.contains('dark')?'dark':'light',pageWidth:document.documentElement.scrollWidth,mainWidth:main.clientWidth,mainScrollWidth:main.scrollWidth,overflow:rects.filter(r => r.left < -1 || r.right > innerWidth + 1),smallTargets:[...main.querySelectorAll('button,input,select')].filter(visible).filter(el => el.getBoundingClientRect().height < 43).map(el=>el.textContent.trim()),selects:window.visibleSelects().map(el=>({value:el.value,text:el.selectedOptions[0].text,emptyOptions:[...el.options].filter(o=>o.value==='').length})),watermarkZ:getComputedStyle(document.querySelector('.watermark-bg'),'::before').zIndex,internalMobileScroll:[...main.querySelectorAll('div')].filter(visible).filter(el=>getComputedStyle(el).overflowY==='auto' && el.scrollHeight > el.clientHeight + 2).length};
        })()`);
        measurements.push(data);
        fs.writeFileSync(path.join(output, 'documents-ui-browser-results.json'), JSON.stringify({ measurements, exceptions, status:'running' },null,2));
        assert.equal(data.theme, theme, `theme at ${width}`);
        assert.equal(data.pageWidth, width, `page overflow at ${width} ${theme}`);
        assert.ok(data.mainScrollWidth <= data.mainWidth + 1, `main overflow at ${width} ${theme}`);
        assert.deepEqual(data.overflow, [], `clipped controls at ${width} ${theme}`);
        if (width < 640) assert.deepEqual(data.smallTargets, [], `touch heights at ${width} ${theme}`);
        assert.deepEqual(data.selects.map(s=>s.emptyOptions), [1,0,1]);
        assert.equal(data.selects[0].text, 'جميع الحالات');
        assert.equal(data.selects[1].text, 'الكل');
        assert.equal(data.selects[2].text, 'جميع المدخلين');
        assert.ok(await evaluate("Math.abs(window.visibleSelects()[0].getBoundingClientRect().top-window.visibleSelects()[1].getBoundingClientRect().top)<1"), 'both selects stay in one row');
        assert.ok(await evaluate("window.visibleSelects()[2].getBoundingClientRect().top > window.visibleSelects()[0].getBoundingClientRect().bottom"), 'uploader stays below both status filters');
        assert.equal(data.watermarkZ, '-1');
        if (width < 640) assert.equal(data.internalMobileScroll, 0);
        if ([360,1280].includes(width)) {
          const shot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `documents-ui-${width}-${theme}.png`), Buffer.from(shot.data, 'base64'));
        }
      }
    }

    await send('Emulation.setDeviceMetricsOverride', { width: 360, height: 900, deviceScaleFactor: 1, mobile: false });
    await send('Page.reload');
    await waitFor("[...document.querySelectorAll('main button')].some(b=>b.textContent.trim()==='طباعة الوثائق')");
    for (const [key, code, vk] of [['Enter','Enter',13],[' ','Space',32]]) {
      const before = requests.filter(r => r.url.includes('/documents/by-file/')).length;
      await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim()==='طباعة الوثائق').focus()");
      await send('Input.dispatchKeyEvent', { type:'keyDown', key, code, windowsVirtualKeyCode:vk });
      await send('Input.dispatchKeyEvent', { type:'keyUp', key, code, windowsVirtualKeyCode:vk });
      await delay(250);
      assert.equal(await evaluate('location.pathname'), '/documents', 'print keyboard activation must not navigate the card');
      assert.equal(requests.filter(r => r.url.includes('/documents/by-file/')).length, before + 1, 'print control activates exactly once');
    }
    await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim()==='الإحصائيات').click()");
    await delay(400);
    assert.equal(await evaluate("document.querySelector('[aria-controls=\"document-upload-statistics\"]').getAttribute('aria-expanded')"), 'true');
    await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim().startsWith('الفلاتر')).click()");
    await delay(300);
    await evaluate("(() => { const f=window.visibleSelects()[1]; f.value='missing';f.dispatchEvent(new Event('change',{bubbles:true})); })()");
    assert.equal(await evaluate("window.visibleSelects()[1].selectedOptions[0].text"), 'ناقصة');
    assert.equal(await evaluate("document.querySelectorAll('main [data-index]').length"), 7, 'missing filter updates mobile cards');
    await evaluate("(() => { const f=window.visibleSelects()[1]; f.value='all';f.dispatchEvent(new Event('change',{bubbles:true})); })()");
    assert.equal(await evaluate("document.querySelectorAll('main [data-index]').length"), 20, 'all filter restores mobile cards');
    await evaluate("const s=window.visibleSelects()[0];s.value='7';s.dispatchEvent(new Event('change',{bubbles:true}))");
    await waitFor("window.visibleSelects()[0].value === '7'");
    await delay(200);
    assert.ok(requests.some(r=>new URL(r.url).searchParams.get('statusId')==='7'), 'status filter forwarded to API');
    await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim().startsWith('الفلاتر')).click()");
    await delay(300);
    assert.equal(await evaluate('window.visibleSelects().length'), 0, 'mobile filter fields fold together');
    await evaluate("[...document.querySelectorAll('main button')].find(b=>b.textContent.trim().startsWith('الفلاتر')).click()");
    await delay(300);
    assert.equal(await evaluate('window.visibleSelects()[0].value'), '7', 'selection preserved across folding');
    assert.deepEqual(exceptions, [], 'uncaught browser errors');
    fs.writeFileSync(path.join(output, 'documents-ui-browser-results.json'), JSON.stringify({ api:'synthetic responses; no live database calls', measurements, keyboard:['Enter','Space'], exceptions, status:'passed' },null,2));
    console.log(`PASS: ${measurements.length} viewport/theme combinations; selects, overflow, touch heights, keyboard, stats and filters.`);
  } finally {
    if (ws?.readyState === WebSocket.OPEN) { ws.send(JSON.stringify({id:++sequence,method:'Browser.close'})); ws.close(); }
    for (const item of pending.values()) clearTimeout(item.timer);
    browser.kill();
  }
}
main().catch(error => { console.error(error); process.exitCode=1; });
