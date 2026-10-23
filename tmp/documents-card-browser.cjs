// Isolated production-preview QA. All API responses are synthetic; no live database.
const { chromium } = require('C:/Users/USER7/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/.pnpm/playwright-core@1.61.1/node_modules/playwright-core');
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const origin = 'http://127.0.0.1:4175';
const output = path.resolve(__dirname, '../plan');
const statusOnly = process.argv.includes('--status-only');
const user = { id: 99999, fullName: 'مستخدم المعاينة', username: 'ui-preview', role: 'رافع الوثائق', branchId: 1, isActive: true };
const permissions = ['System.Admin', 'File.Documents.Upload', 'File.View.Sensitive'];
const statuses = [
  { statusID: 7, statusName: 'تحت المراجعة', color: 'info' },
  { statusID: 1, statusName: 'جديد', color: 'outline' },
  { statusID: 2, statusName: 'نشط', color: 'success' },
  { statusID: 3, statusName: 'موقوف', color: 'destructive' },
  { statusID: 4, statusName: 'مغلق', color: 'secondary' },
  { statusID: 5, statusName: 'ناقص', color: 'warning' },
  { statusID: 6, statusName: 'معلق', color: 'warning' },
  { statusID: 8, statusName: 'مرفوض', color: 'destructive' },
];
const names = ['محمد أحمد عبدالله عبدالكريم عبدالرحمن صالح', 'علي صالح أحمد', 'عبدالله محمد سالم'];
const rows = Array.from({ length: 20 }, (_, i) => ({
  fileID: i + 1, fileNumber: `2026-${String(4985 - i).padStart(5, '0')}`,
  retireeFullName: names[i % 3], nationalID: null, rankName: null,
  fileStatusName: statuses[i % 8].statusName,
  missingDocumentsCount: i % 3 === 1 ? 2 : 0, hasDocuments: i % 3 !== 0,
  lastUploadedByUserID: i % 3 ? 1 : null, lastUploadedByUserName: i % 3 ? 'مدخل الوثائق الأول' : null,
  statusID: statuses[i % 8].statusID,
}));
function dataFor(url) {
  const pathname = new URL(url).pathname.replace(/^\/api/, '');
  if (pathname === '/auth/me/permissions') return permissions;
  if (pathname === '/auth/me') return user;
  if (pathname === '/retirement-files/document-upload-stats') return { totalFiles: 4984, filesWithDocuments: 3900, filesWithoutDocuments: 1084, filesWithMissing: 126, totalMissingDocuments: 250 };
  if (pathname === '/retirement-files/uploader-stats') return [{ userID: 1, fullName: 'مدخل الوثائق الأول', userName: 'preview' }];
  if (pathname === '/file-statuses') return statuses;
  if (pathname === '/retirement-files') return { items: rows, totalPages: 2, totalCount: 40 };
  if (pathname === '/notifications/unread-count') return 0;
  if (pathname === '/notifications') return { items: [], totalCount: 0, totalPages: 0, page: 1, pageSize: 20 };
  return [];
}
async function main() {
  const browser = await chromium.launch({ executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe', headless: true });
  const measurements = [], exceptions = [], requests = [];
  try {
    for (const width of statusOnly ? [360] : [320, 360, 640, 1280]) {
      for (const theme of statusOnly ? ['light'] : ['light', 'dark']) {
        const context = await browser.newContext({ viewport: { width, height: 900 }, isMobile: width < 640, hasTouch: width < 640, reducedMotion: 'reduce' });
        try {
          const token = `${Buffer.from('{}').toString('base64url')}.${Buffer.from(JSON.stringify({ exp: Math.floor(Date.now()/1000)+86400 })).toString('base64url')}.preview`;
          await context.addInitScript(({token,user,permissions,theme}) => {
            localStorage.setItem('auth_token', token);
            localStorage.setItem('auth_user', JSON.stringify(user));
            localStorage.setItem('auth_permissions', JSON.stringify(permissions));
            localStorage.setItem('theme-storage', JSON.stringify({state:{isDark:theme==='dark'},version:0}));
          }, {token,user,permissions,theme});
          await context.route('**/api/**', async route => {
            requests.push(route.request().url());
            await route.fulfill({ json: { isSuccess: true, data: dataFor(route.request().url()), message: '' } });
          });
          await context.route('**/hubs/**', route => route.fulfill({ json: { error: 'الاتصال الفوري غير مستخدم في المعاينة' } }));
          const page = await context.newPage();
          page.on('pageerror', error => exceptions.push({width,theme,message:error.message}));
          await page.goto(`${origin}/documents`);
          await page.locator('main [data-index]').first().waitFor({ state: 'attached', timeout: 15000 });
          const statusPaint = await page.locator('main [data-index]').first().getByText('تحت المراجعة',{exact:true}).evaluate(el=>getComputedStyle(el).backgroundColor);
          const statusChannels = statusPaint.match(/[0-9.]+/g).map(Number);
          const blueStatusBadge = statusChannels[2] > statusChannels[0] && (statusChannels.length < 4 || statusChannels[3] > 0);
          if (statusOnly) {
            const passed = blueStatusBadge;
            fs.writeFileSync(path.join(output,'documents-status-repro.json'),JSON.stringify({statusID:7,colorFieldPresent:false,background:statusPaint,passed},null,2));
            assert.ok(passed, `UnderReview (statusID=7) must have a blue badge without fileStatusColor; got ${statusPaint}`);
            console.log('PASS: UnderReview badge is blue with the real API field shape.');
            return;
          }
          assert.ok(blueStatusBadge, 'UnderReview is blue when the API omits a color field');
          const mobile = width < 640;
          if (mobile) {
            assert.equal(await page.locator('main select:visible').count(), 0, 'filters initially folded');
            if (width === 360) await page.screenshot({ path: path.join(output,`documents-card-${width}-${theme}.png`) });
            await page.getByRole('button', {name:'الفلاتر',exact:true}).click();
            await page.locator('main select:visible').first().waitFor();
            if (width===360) await page.screenshot({path:path.join(output,`documents-card-${width}-${theme}-expanded.png`)});
          }
          const result = await page.evaluate(() => {
            const main = document.querySelector('main');
            const visible = el => el.getBoundingClientRect().width > 0 && el.getBoundingClientRect().height > 0 && !el.closest('[inert]');
            const selects = [...main.querySelectorAll('select')].filter(visible);
            const cards = [...main.querySelectorAll('[data-index]')].filter(visible);
            const controls = [...main.querySelectorAll('button,input,select,h1,h2,bdi')].filter(visible).filter(el=>!el.closest('table'));
            return { width:innerWidth,theme:document.documentElement.classList.contains('dark')?'dark':'light',pageWidth:document.documentElement.scrollWidth,mainWidth:main.clientWidth,mainScrollWidth:main.scrollWidth,
              overflow:controls.filter(el=>el.getBoundingClientRect().left < -1 || el.getBoundingClientRect().right > innerWidth+1).map(el=>el.textContent.trim()),
              smallTargets:controls.filter(el=>['BUTTON','INPUT','SELECT'].includes(el.tagName)&&el.getBoundingClientRect().height<43).map(el=>el.textContent.trim()),
              selects:selects.map(el=>({text:el.selectedOptions[0].text,emptyOptions:[...el.options].filter(o=>o.value==='').length,top:el.getBoundingClientRect().top,bottom:el.getBoundingClientRect().bottom})),
              cards:cards.slice(0,3).map(el=>{const heading=el.querySelector('h2');const name=heading.getBoundingClientRect();const number=el.querySelector('bdi').getBoundingClientRect();const badge=heading.parentElement.querySelector('span').getBoundingClientRect();const row=heading.parentElement.nextElementSibling;const uploader=row.children[1];return {background:getComputedStyle(el).backgroundColor,actions:[...el.querySelectorAll('button')].map(b=>b.textContent.trim()),nameTop:name.top,nameBottom:name.bottom,numberTop:number.top,numberBottom:number.bottom,statusTop:badge.top,statusBottom:badge.bottom,uploaderBesideNumber:!uploader||Math.abs(uploader.getBoundingClientRect().top-row.children[0].getBoundingClientRect().top)<1,fieldsOnOneLine:[...row.children].every(p=>getComputedStyle(p).whiteSpace==='nowrap'&&p.getBoundingClientRect().height<=parseFloat(getComputedStyle(p).lineHeight)+1),documentLabelClipped:getComputedStyle(el.querySelector('.sr-only')).clip==='rect(0px, 0px, 0px, 0px)',dividers:el.querySelectorAll('.border-t').length};}),
              watermarkZ:getComputedStyle(document.querySelector('.watermark-bg'),'::before').zIndex };
          });
          measurements.push(result);
          fs.writeFileSync(path.join(output,'documents-card-browser-results.json'),JSON.stringify({status:'running',measurements,exceptions},null,2));
          assert.equal(result.theme,theme);
          assert.equal(result.pageWidth,width, 'no page overflow');
          assert.ok(result.mainScrollWidth <= result.mainWidth+1, 'no main overflow');
          assert.deepEqual(result.overflow,[],'controls fit');
          assert.deepEqual(result.selects.map(s=>s.emptyOptions),[1,0,1]);
          assert.deepEqual(result.selects.map(s=>s.text),['جميع الحالات','الكل','جميع المدخلين']);
          assert.equal(result.selects[0].top,result.selects[1].top,'statuses side by side');
          assert.ok(result.selects[2].top > result.selects[0].bottom,'uploader below');
          assert.equal(result.watermarkZ,'-1');
          if (mobile) {
            assert.deepEqual(result.smallTargets,[],'mobile touch targets');
            assert.equal(new Set(result.cards.map(c=>c.background)).size,3,'three document background colors');
            for (const card of result.cards) {
              assert.deepEqual(card.actions,['رفع الوثائق','طباعة الوثائق']);
              assert.ok(card.nameTop < card.numberTop,'name above file number');
              assert.ok(card.statusBottom > card.nameTop && card.statusTop < card.nameBottom,'file status beside name');
              assert.ok(card.uploaderBesideNumber,'uploader beside file number');
              assert.ok(card.fieldsOnOneLine,'file number and uploader never wrap');
              assert.ok(card.documentLabelClipped,'document status text is hidden visually');
              assert.equal(card.dividers,0,'no inner divider');
            }
            const fields = page.locator('main select:visible');
            await fields.nth(1).selectOption('missing');
            await page.waitForFunction(()=>document.querySelectorAll('main [data-index]').length===7);
            await page.getByRole('button',{name:/الفلاتر/}).click();
            assert.equal(await page.locator('main select:visible').count(),0,'all fields fold together');
            await page.getByRole('button',{name:/الفلاتر/}).click();
            assert.equal(await fields.nth(1).inputValue(),'missing','folding preserves selection');
            await fields.nth(1).selectOption('all');
            await page.waitForFunction(()=>document.querySelectorAll('main [data-index]').length===20);
            const print = page.locator('main [data-index]').first().getByRole('button',{name:'طباعة الوثائق',exact:true});
            for (const key of ['Enter','Space']) {
              const before=requests.filter(url=>url.includes('/documents/by-file/')).length;
              await print.focus();
              await page.keyboard.press(key);
              await page.waitForFunction(()=>document.body.textContent.includes('لا توجد وثائق للطباعة'));
              assert.equal(new URL(page.url()).pathname,'/documents','print does not activate card');
              assert.equal(requests.filter(url=>url.includes('/documents/by-file/')).length,before+1,'single print activation');
            }
          } else {
            assert.ok(await page.getByRole('button',{name:'تقرير الوثائق الناقصة',exact:true}).count()>0,'desktop report retained');
            if(width===1280) await page.screenshot({path:path.join(output,`documents-card-${width}-${theme}.png`)});
          }
        } finally { await context.close(); }
      }
    }
    assert.deepEqual(exceptions,[],'no unhandled browser errors');
    const result={status:'passed',data:'synthetic API responses; no live database',measurements,exceptions};
    fs.writeFileSync(path.join(output,'documents-card-browser-results.json'),JSON.stringify(result,null,2));
    fs.writeFileSync(path.join(output,'documents-status-browser-results.json'),JSON.stringify(result,null,2));
    fs.writeFileSync(path.join(output,'documents-ui-browser-results.json'),JSON.stringify(result,null,2));
    for(const width of [360,1280])for(const theme of ['light','dark'])fs.copyFileSync(path.join(output,`documents-card-${width}-${theme}.png`),path.join(output,`documents-ui-${width}-${theme}.png`));
    console.log(`PASS: ${measurements.length} viewport/theme combinations; card colors, actions, order, spacing, filters and keyboard.`);
  } finally { await browser.close(); }
}
main().catch(error=>{console.error(error);process.exitCode=1;});
