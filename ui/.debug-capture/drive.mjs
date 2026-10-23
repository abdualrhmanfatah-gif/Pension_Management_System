import assert from 'node:assert/strict';
import { writeFile } from 'node:fs/promises';

const page = await (await fetch('http://127.0.0.1:9231/json/new?about:blank', { method: 'PUT' })).json();
const ws = new WebSocket(page.webSocketDebuggerUrl);
await new Promise(r => ws.addEventListener('open', r, { once: true }));
let id = 0;
const pending = new Map();
const mobile = process.argv.includes('--mobile');
const cancelCrop = process.argv.includes('--cancel-crop');
ws.addEventListener('message', e => {
  const msg = JSON.parse(e.data);
  if (msg.id) {
    const p = pending.get(msg.id);
    pending.delete(msg.id);
    if (msg.error) p.reject(new Error(JSON.stringify(msg.error))); else p.resolve(msg.result);
  }
  if (msg.method === 'Runtime.exceptionThrown') console.log('BROWSER ERROR', JSON.stringify(msg.params.exceptionDetails));
});
const send = (method, params = {}) => new Promise((resolve, reject) => {
  if (method !== 'Runtime.evaluate') console.log('CDP', method);
  const next = ++id;
  pending.set(next, { resolve, reject });
  ws.send(JSON.stringify({ id: next, method, params }));
  const timer = setTimeout(() => reject(new Error('CDP timeout: ' + method)), 15000);
  timer.unref();
});
const evaluate = async expression => {
  const result = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  if (result.exceptionDetails) throw new Error(JSON.stringify(result.exceptionDetails));
  return result.result.value;
};
const waitFor = async (expression, timeout = 30000) => {
  const deadline = Date.now() + timeout;
  while (Date.now() < deadline) {
    if (await evaluate(expression)) return;
    await new Promise(r => setTimeout(r, 200));
  }
  throw new Error('Timeout: ' + expression + '\n' + await evaluate('document.body.innerText'));
};
const click = async label => {
  const bounds = await evaluate(`(() => { const b = [...document.querySelectorAll('button')].find(b => b.innerText.trim().startsWith(${JSON.stringify(label)}) && b.getBoundingClientRect().width > 0 && !b.disabled); if (!b) throw Error('button missing'); const r = b.getBoundingClientRect(); return {x: r.x+r.width/2, y: r.y+r.height/2}; })()`);
  await send('Input.dispatchMouseEvent', { type: 'mousePressed', button: 'left', clickCount: 1, ...bounds });
  await send('Input.dispatchMouseEvent', { type: 'mouseReleased', button: 'left', clickCount: 1, ...bounds });
};
try {
  await send('Runtime.enable');
  await send('Emulation.setDeviceMetricsOverride', { width: mobile ? 390 : 1280, height: mobile ? 844 : 1000, deviceScaleFactor: 1, mobile });
  if (mobile) {
    await send('Emulation.setUserAgentOverride', {userAgent: 'Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Mobile Safari/537.36'});
    await send('Page.setInterceptFileChooserDialog', {enabled: true});
  }
  await send('Page.navigate', { url: 'http://127.0.0.1:5179/.debug-capture/index.html' });
  await waitFor(`document.body.innerText.includes('لم تُضف ملفات بعد')`);
  for (let i = 1; i <= 2; i++) {
    // Feed a distinct camera image through the real native-camera change handler.
    await evaluate(`(async () => {
      const canvas = document.createElement('canvas'); canvas.width = 800; canvas.height = 1000;
      const ctx = canvas.getContext('2d'); ctx.fillStyle = '${'#f0f0f0'}'; ctx.fillRect(0,0,800,1000);
      ctx.fillStyle = '#000'; ctx.font = '60px sans-serif'; ctx.fillText('Document ${i}', 100, 150);
      const blob = await new Promise(r => canvas.toBlob(r, 'image/jpeg'));
      const transfer = new DataTransfer(); transfer.items.add(new File([blob], 'capture_${i}.jpg', { type:'image/jpeg' }));
      const input = [...document.querySelectorAll('input[capture]')].at(${mobile ? 0 : -1}); input.files = transfer.files; input.dispatchEvent(new Event('change', {bubbles:true}));
    })()`);
    await waitFor(`document.body.innerText.includes('تأكيد التعديل') && [...document.querySelectorAll('canvas')].some(i => i.width > 0 && i.getBoundingClientRect().width > 0)`);
    await new Promise(r => setTimeout(r, 600));
    await click(cancelCrop && i === 1 ? 'إلغاء' : 'تأكيد التعديل');
    await waitFor(`document.body.innerText.includes('إنهاء التصوير')`, 5000);
    console.log('Before finish', await evaluate(`({rows: [...document.querySelectorAll('button[aria-label^="حذف "]')].map(b=>b.getAttribute('aria-label'))})`));
    await new Promise(r => setTimeout(r, 250));
    await click(mobile && i === 1 ? 'تصوير مرفق آخر' : 'إنهاء التصوير');
    await new Promise(r => setTimeout(r, 600));
    const rows = await evaluate(`([...document.querySelectorAll('button[aria-label^="حذف "]')].map(b=>b.getAttribute('aria-label')))`);
    console.log('After finish', rows);
    assert.equal(rows.length, i, 'Ending photography must preserve every accepted attachment');
    assert.ok(rows.some(r => r.includes('capture_' + i + (cancelCrop && i === 1 ? '.jpg' : '_scanned.jpg'))), 'Latest photo must stay in list');
  }
  await writeFile(new URL('./result.png', import.meta.url), Buffer.from((await send('Page.captureScreenshot')).data, 'base64'));
  console.log('Row locations', await evaluate(`([...document.querySelectorAll('button[aria-label^="حذف "]')].map(b=>({name:b.getAttribute('aria-label'), rect:b.getBoundingClientRect().toJSON()})))`));
  if (process.argv.includes('--scroll-probe')) {
    await evaluate(`([...document.querySelectorAll('button[aria-label^="معاينة "]')].at(-1).scrollIntoView({block:'nearest'}))`);
    await new Promise(r=>setTimeout(r,250));
  }
  const latestPhotoVisible = await evaluate(`(() => {
    const preview = [...document.querySelectorAll('button[aria-label^="معاينة "]')].at(-1);
    const r = preview.getBoundingClientRect();
    const hit = document.elementFromPoint(r.x+r.width/2, r.y+r.height/2);
    return preview === hit || preview.contains(hit);
  })()`);
  assert.ok(latestPhotoVisible, 'The latest accepted photo must be visible after ending photography');
  console.log('PASS: both accepted camera photos remain after ending photography');
} catch (error) {
  await writeFile(new URL('./failure.png', import.meta.url), Buffer.from((await send('Page.captureScreenshot')).data, 'base64'));
  throw error;
} finally {
  ws.close();
}
