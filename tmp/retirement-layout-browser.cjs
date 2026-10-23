// Isolated UI verification with synthetic API responses; never touches live data.
const { chromium } = require('C:/Users/USER7/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/.pnpm/playwright-core@1.61.1/node_modules/playwright-core');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const origin = 'http://127.0.0.1:4175';
const output = path.resolve(__dirname, '../plan');
const statuses = [
  {statusID:7,statusName:'تحت المراجعة'}, {statusID:1,statusName:'جديد'},
  {statusID:2,statusName:'نشط'}, {statusID:3,statusName:'موقوف'},
  {statusID:4,statusName:'مغلق'}, {statusID:5,statusName:'ناقص'},
  {statusID:6,statusName:'معلق'}, {statusID:8,statusName:'مرفوض'},
];
const names=['محمد أحمد عبدالله عبدالكريم عبدالرحمن صالح','علي صالح أحمد','عبدالله محمد سالم'];
const files=Array.from({length:25},(_,i)=>({fileID:i+1,fileNumber:`2026-${4985-i}`,retireeFullName:names[i%3],nationalID:'0102030405',militaryNumber:`12345${i}`,rankName:'مساعد أول',fileStatusName:statuses[i%8].statusName,statusID:statuses[i%8].statusID,fileOpenDate:null,hasDocuments:i%3!==0,missingDocumentsCount:i%3===1?2:0,lastUploadedByUserName:i%3?'أحمد عبدالرحمن محمد علي مدخل الملفات':null,reviewStatus:i===2?1:0}));
const retirees=Array.from({length:50},(_,i)=>({retireeID:i+1,fullName:names[i%3],nationalID:'0102030405',militaryNumber:`12345${i}`,rankName:'مساعد أول',rankId:1,categoryId:1,gender:1,governorate:'عدن',phone:null,linkNumber:null,isAlive:i%2===0,fileCount:i%2,fileID:i%2?i+1:null}));
const adminPermissions=['System.Admin','Retiree.View','Retiree.Delete','File.View.Sensitive','File.Create'];
const user={id:99999,fullName:'مستخدم المعاينة',username:'ui-preview',role:'مدير',branchId:1,isActive:true};
const observations=[],exceptions=[],requests=[];
function dataFor(url,permissions) {
  const uri=new URL(url), p=uri.pathname.replace(/^\/api/,'');
  if(p==='/auth/me/permissions')return permissions;
  if(p==='/auth/me')return user;
  if(p==='/file-statuses')return statuses;
  if(p==='/lookups/retiree-categories')return [{id:1,name:'ضباط'},{id:2,name:'أفراد'}];
  if(p==='/lookups/governorates')return [{id:1,name:'عدن'},{id:2,name:'تعز'}];
  if(p==='/hr/HrReferenceData/ranks')return [{rankID:1,rankName:'مساعد أول'}];
  if(p==='/retirees')return {items:retirees,totalCount:100,page:Number(uri.searchParams.get('page')||1),pageSize:50};
  if(p==='/retirement-files/for-data-entry')return {items:files.slice(0,20).map(f=>({fileID:f.fileID,fileNumber:f.fileNumber,retireeFullName:f.retireeFullName,nationalID:f.nationalID,fileStatusName:f.fileStatusName,fileOpenDate:null,statusID:null})),totalCount:40,page:Number(uri.searchParams.get('page')||1),pageSize:20};
  if(p==='/retirement-files')return {items:files,totalCount:50,page:Number(uri.searchParams.get('page')||1),pageSize:25,hasMore:true,nextCursor:26};
  if(p==='/notifications/unread-count')return 0;
  if(p==='/notifications')return {items:[],totalCount:0,page:1,pageSize:20};
  return [];
}
async function main(){
  const browser=await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true});
  try{
    const cases=[];
    for(const route of ['/files','/retirees'])for(const width of [320,360,640,1280])for(const theme of ['light','dark'])cases.push({route,width,theme,permissions:adminPermissions,mode:'admin'});
    cases.push({route:'/files',width:360,theme:'light',permissions:['File.Create','File.Edit.BasicData'],mode:'data-entry'});
    cases.push({route:'/retirees',width:360,theme:'light',permissions:['Retiree.View','File.View.Sensitive'],mode:'readonly'});
    for(const spec of cases){
      const {route,width,theme,permissions,mode}=spec;
      const context=await browser.newContext({viewport:{width,height:900},isMobile:width<640,hasTouch:width<640,reducedMotion:'reduce'});
      try{
        const token=`${Buffer.from('{}').toString('base64url')}.${Buffer.from(JSON.stringify({exp:Math.floor(Date.now()/1000)+86400})).toString('base64url')}.preview`;
        await context.addInitScript(({token,user,permissions,theme})=>{
          localStorage.setItem('auth_token',token);localStorage.setItem('auth_user',JSON.stringify(user));localStorage.setItem('auth_permissions',JSON.stringify(permissions));localStorage.setItem('theme-storage',JSON.stringify({state:{isDark:theme==='dark'},version:0}));
        },{token,user,permissions,theme});
        await context.route('**/api/**',async r=>{
          requests.push({mode,route,url:r.request().url(),method:r.request().method()});
          assert.equal(r.request().method(),'GET','verification must never mutate data');
          await r.fulfill({json:{isSuccess:true,data:dataFor(r.request().url(),permissions),message:''}});
        });
        await context.route('**/hubs/**',r=>r.fulfill({json:{error:'الاتصال الفوري غير مستخدم في المعاينة'}}));
        const page=await context.newPage();
        page.on('pageerror',e=>exceptions.push({...spec,message:e.message}));
        await page.goto(origin+route);
        const mobile=width<640;
        await page.locator(mobile?'main [data-index]':'main tbody tr').first().waitFor({timeout:20000});
        assert.equal(await page.getByText('حدث خطأ غير متوقع',{exact:true}).count(),0,'no error boundary');
        if(mode==='admin'&&(width===360||width===1280))await page.screenshot({path:path.join(output,`${route==='/files'?'retirement-files':'retirees-layout'}-${width}-${theme}.png`)});
        if(mode!=='data-entry'){
          const fields=page.locator('main select:visible');
          assert.equal(await fields.count(),mobile?0:route==='/files'?5:4,'mobile filters folded and desktop filters visible');
          if(mobile)await page.getByRole('button',{name:'الفلاتر',exact:true}).click();
          await fields.first().waitFor();
          const expected=route==='/files'?5:4;
          assert.equal(await fields.count(),expected);
          assert.equal(await fields.evaluateAll(elements=>elements.every(el=>[...el.options].filter(o=>o.value==='').length===1)),true,'one all-option per filter');
          if(mobile){
            const positions=await fields.evaluateAll(elements=>elements.map(e=>e.getBoundingClientRect().top));
            assert.equal(positions[0],positions[1],'first two fields in one row');
            assert.equal(positions[2],positions[3],'next two fields in one row');
            const parameter=route==='/files'?'statusId':'categoryId';
            const chosen=route==='/files'?'7':'1';
            await Promise.all([page.waitForRequest(r=>new URL(r.url()).pathname.endsWith(route==='/files'?'/retirement-files':'/retirees')&&new URL(r.url()).searchParams.get(parameter)===chosen),fields.first().selectOption(chosen)]);
            await page.getByRole('button',{name:/^الفلاتر/}).click();
            assert.equal(await fields.count(),0,'fields fold together');
            await page.getByRole('button',{name:/^الفلاتر/}).click();
            assert.equal(await fields.first().inputValue(),chosen,'selection preserved');
            await page.getByRole('button',{name:'مسح الفلاتر',exact:true}).filter({visible:true}).click();
            assert.equal(await fields.first().inputValue(),'','clear filters');
            if(width===360&&mode==='admin')await page.screenshot({path:path.join(output,`${route==='/files'?'retirement-files':'retirees-layout'}-${width}-${theme}-expanded.png`)});
            await page.getByRole('button',{name:/^الفلاتر/}).click();
          }
        }
        const measured=await page.evaluate(()=>{
          const main=document.querySelector('main'), visible=e=>e.getBoundingClientRect().width>0&&e.getBoundingClientRect().height>0;
          const cards=[...main.querySelectorAll('[data-index]')].filter(visible);
          return {pageWidth:document.documentElement.scrollWidth,width:innerWidth,mainWidth:main.clientWidth,mainScrollWidth:main.scrollWidth,
            smallTargets:[...main.querySelectorAll('button,input,select')].filter(visible).filter(e=>e.getBoundingClientRect().height<43).map(e=>e.textContent.trim()),
            cards:cards.slice(0,3).map(e=>{const h=e.querySelector('h2'),badge=h.parentElement.querySelector('span'),n=e.querySelector('bdi'),line=h.parentElement.nextElementSibling,uploader=line.children[1];return {background:getComputedStyle(e).backgroundColor,nameTop:h.getBoundingClientRect().top,nameBottom:h.getBoundingClientRect().bottom,statusTop:badge.getBoundingClientRect().top,statusBottom:badge.getBoundingClientRect().bottom,numberTop:n.getBoundingClientRect().top,metadataOneLine:[...line.children].every(p=>getComputedStyle(p).whiteSpace==='nowrap'&&p.getBoundingClientRect().height<=parseFloat(getComputedStyle(p).lineHeight)+1),uploaderBeside:!uploader||Math.abs(uploader.getBoundingClientRect().top-line.children[0].getBoundingClientRect().top)<1,dividers:e.querySelectorAll('.border-t').length,buttons:[...e.querySelectorAll('button')].map(b=>b.textContent.trim()),badgeColor:getComputedStyle(badge).backgroundColor};})};
        });
        observations.push({...spec,permissions:undefined,...measured});
        fs.writeFileSync(path.join(output,'retirement-files-browser-results.json'),JSON.stringify({status:'running',observations,exceptions},null,2));
        assert.equal(measured.pageWidth,width,'no horizontal page overflow');
        assert.ok(measured.mainScrollWidth<=measured.mainWidth+1,'no main overflow');
        if(mobile){
          assert.deepEqual(measured.smallTargets,[],'touch targets at least 44px');
          for(const card of measured.cards){
            assert.ok(card.nameTop<card.numberTop,'name above number');
            assert.ok(card.statusBottom>card.nameTop&&card.statusTop<card.nameBottom,'status beside name');
            assert.ok(card.metadataOneLine,'numbers and uploader stay on one line');
            assert.ok(card.uploaderBeside,'uploader beside number');
            assert.equal(card.dividers,0,'no inner divider');
          }
          if(route==='/files'){
            const channels=measured.cards[0].badgeColor.match(/[0-9.]+/g).map(Number);
            assert.ok(channels[2]>channels[0]&&(channels.length<4||channels[3]>0),'UnderReview blue, including data-entry endpoint without statusID');
            if(mode==='admin'){
              assert.equal(new Set(measured.cards.map(c=>c.background)).size,3,'document tint has three states');
              assert.equal(await page.locator('main [data-index]').first().locator('button').count(),0,'no redundant view/report button');
            }
          }else if(mode==='admin'){
            assert.equal(new Set(measured.cards.map(c=>c.background)).size,2,'life state colors');
            const more=page.locator('main [data-index]').first().getByRole('button',{name:'إجراءات المتقاعد'});
            await more.click();
            await page.getByRole('menuitem',{name:'حذف المتقاعد',exact:true}).click();
            await page.getByRole('dialog').waitFor();
            assert.equal(new URL(page.url()).pathname,'/retirees','portal menu must not navigate card');
            await page.getByRole('button',{name:'إلغاء',exact:true}).click();
            assert.equal(new URL(page.url()).pathname,'/retirees');
          }else{
            await page.locator('main [data-index]').first().getByRole('button',{name:'إجراءات المتقاعد'}).click();
            assert.equal(await page.getByRole('menuitem',{name:'حذف المتقاعد',exact:true}).count(),0,'delete permission respected');
            assert.equal(await page.getByRole('button',{name:'إنشاء ملف',exact:true}).count(),0,'create permission respected');
            await page.keyboard.press('Escape');
            await page.getByRole('menu').waitFor({state:'hidden'});
          }
          const input=page.locator('main input[type="search"]');
          await input.waitFor({state:'visible'});
          await Promise.all([page.waitForRequest(r=>new URL(r.url()).searchParams.get('search')==='محمد'),input.fill('محمد')]).catch(async error=>{
            await page.screenshot({path:path.join(output,'retirement-layout-failure.png')});
            console.error({spec,value:await input.inputValue(),lastRequests:requests.slice(-8)});
            throw error;
          });
          await input.fill('');
          if(route==='/files'){
            const card=page.locator('main [data-index]').first();
            await card.focus();await page.keyboard.press('Enter');
            await page.waitForURL('**/files/1');
          }
        }
      }finally{await context.close();}
    }
    assert.deepEqual(exceptions,[],'no runtime exceptions');
    const result={status:'passed',data:'synthetic API responses; no live database',observations,exceptions};
    fs.writeFileSync(path.join(output,'retirement-files-browser-results.json'),JSON.stringify(result,null,2));
    fs.writeFileSync(path.join(output,'retirees-layout-browser-results.json'),JSON.stringify({...result,observations:observations.filter(o=>o.route==='/retirees')},null,2));
    console.log(`PASS: ${observations.length} cases, filters, keyboard, permissions, shared cards and no runtime errors.`);
  }finally{await browser.close();}
}
main().catch(e=>{console.error(e);process.exitCode=1;});
