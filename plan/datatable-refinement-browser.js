async (page) => {
  const assert = (condition, message) => { if (!condition) throw new Error(message); };
  const mount = async () => {
    const React = (await import('/node_modules/.vite/deps/react.js')).default;
    const ReactDOM = (await import('/node_modules/.vite/deps/react-dom_client.js')).default;
    const {DataTable} = await import('/src/components/ui/DataTable.tsx');
    const {CrudPageLayout} = await import('/src/components/ui/CrudPageLayout.tsx');
    for (const child of document.body.children) if (child instanceof HTMLElement) child.style.display = 'none';
    const host = document.createElement('main');
    host.id = 'datatable-refinement-preview';
    host.style.cssText = 'padding:16px;max-width:100%';
    document.body.append(host);
    const names = ['يحيى إسماعيل جابر قبيصي', 'محمد ناجي حسين صالح', 'محمد عبدالله حسين عسيلي'];
    const headers = [['batchNumber','رقم الكشف'], ['serialNumber','التسلسلي'], ['fullName','الاسم'], ['gender','الجنس'], ['militaryNumber','الرقم العسكري'], ['rankName','الرتبة'], ['unitName','الوحدة'], ['enrollmentDate','تاريخ الالتحاق'], ['retirementDate','تاريخ التقاعد'], ['salary','المعاش'], ['categoryName','الفئة'], ['linkNumber','رقم الربط'], ['status','الحالة'], ['actions','']];
    function Fixture() {
      const [state, setState] = React.useState({page:1, mode:'standard', layout:false, pinned:false});
      window.__tableFixture = {state, update:patch=>setState(s=>({...s,...patch}))};
      const columns = headers.map(([key,header])=>({key,header,render:row=>key==='actions'?React.createElement('button',{type:'button'},'عرض'):React.createElement('span',{className:key==='fullName'?'font-bold':undefined},row[key])}));
      const data = state.mode==='empty'?[]:Array.from({length:20},(_,i)=>({id:i+1,batchNumber:2,serialNumber:897-i,fullName:names[i%3],gender:'ذكر',militaryNumber:6013373+i,rankName:'مساعد ثاني',unitName:'تقاعد أمن عدن',enrollmentDate:'1968/4/28',retirementDate:'2003/6/30',salary:'44,816',categoryName:'المتقاعدين',linkNumber:8833+i,status:'معالج'}));
      const props = {columns,data,rowKey:r=>r.id,loading:state.mode==='loading',stickyColumnKey:state.pinned?'fullName':undefined,mobileScrollMode:'contained',pagination:{page:state.page,pageSize:20,total:14963,cursor:state.mode==='cursor'?100:undefined,hasMore:true,onPageChange:page=>setState(s=>({...s,page})),onCursorPrev:()=>setState(s=>({...s,page:s.page-1,cursorAction:'prev'})),onCursorNext:()=>setState(s=>({...s,page:s.page+1,cursorAction:'next'}))}};
      return state.layout?React.createElement(CrudPageLayout,{...props,title:'كشف الإحالة'}):React.createElement(DataTable,{...props,maxHeight:'440px'});
    }
    ReactDOM.createRoot(host).render(React.createElement(Fixture));
  };
  const load = async p => { await p.goto('http://127.0.0.1:5178/'); await p.evaluate(mount); await p.locator('#datatable-refinement-preview table').waitFor({state:'attached'}); };
  const update = async (p, patch) => {await p.evaluate(patch=>window.__tableFixture.update(patch),patch);await p.waitForFunction(patch=>Object.entries(patch).every(([key,value])=>window.__tableFixture.state[key]===value),patch);};
  const results = {captures:[]};
  await page.setViewportSize({width:1280,height:900});
  await load(page);
  await page.emulateMedia({reducedMotion:'reduce'});
  results.nowrap = await page.locator('tr[data-row-index="0"] td').nth(2).evaluate(el=>{const range=document.createRange();range.selectNodeContents(el.querySelector('span')||el);return {lines:new Set([...range.getClientRects()].map(r=>Math.round(r.top))).size,rowHeight:el.parentElement.getBoundingClientRect().height};});
  assert(results.nowrap.lines===1,'The four-part name wraps across lines.');
  for (const theme of ['light','dark']) {
    await page.evaluate(theme=>document.documentElement.classList.toggle('dark',theme==='dark'),theme);
    await page.screenshot({path:`F:/Projects/Pension_Management_System/plan/datatable-refinement-1280-${theme}.png`,fullPage:true,animations:'disabled'});
    const overflow = await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth);
    assert(!overflow,'Desktop document overflows horizontally.');
    results.captures.push({width:1280,theme,overflow});
  }
  await update(page,{layout:true,pinned:true});
  results.frame = await page.getByRole('region',{name:'بيانات الجدول'}).evaluate(el=>{const root=el.parentElement.parentElement;return{tableBorder:getComputedStyle(root).borderTopWidth,cardBorder:getComputedStyle(root.parentElement).borderTopWidth,pinned:getComputedStyle(el.querySelector('tbody td:nth-child(3)')).position};});
  assert([results.frame.tableBorder,results.frame.cardBorder].filter(border=>parseFloat(border)>0).length===1,'CrudPageLayout should have exactly one frame.');
  assert(results.frame.pinned==='sticky','CrudPageLayout does not forward the pinned column.');
  await update(page,{layout:false,mode:'cursor',page:3});
  await page.getByRole('button',{name:'الصفحة التالية',exact:true}).click();
  results.cursor = await page.evaluate(()=>({page:window.__tableFixture.state.page,action:window.__tableFixture.state.cursorAction}));
  assert(results.cursor.page===4&&results.cursor.action==='next','Cursor navigation callback changed.');
  await update(page,{mode:'empty'});
  results.empty = await page.getByText('لا توجد بيانات',{exact:true}).first().isVisible();
  assert(results.empty,'Empty state is missing.');
  await update(page,{mode:'loading'});
  results.loading = await page.locator('#datatable-refinement-preview .animate-pulse').count();
  assert(results.loading>0,'Loading skeletons are missing.');
  const touchContext = await page.context().browser().newContext({viewport:{width:360,height:900},hasTouch:true,isMobile:true});
  try {
    const touchPage = await touchContext.newPage();
    await load(touchPage);
    await touchPage.emulateMedia({reducedMotion:'reduce'});
    for (const theme of ['light','dark']) {
      await touchPage.evaluate(theme=>document.documentElement.classList.toggle('dark',theme==='dark'),theme);
      await touchPage.screenshot({path:`F:/Projects/Pension_Management_System/plan/datatable-refinement-360-${theme}.png`,fullPage:true,animations:'disabled'});
      const overflow = await touchPage.evaluate(()=>document.documentElement.scrollWidth>innerWidth);
      const next = await touchPage.getByRole('button',{name:'الصفحة التالية',exact:true}).boundingBox();
      assert(!overflow,'Mobile document overflows horizontally.');
      assert(next.width>=43&&next.height>=43,'Touch pagination targets are too small.');
      results.captures.push({width:360,theme,overflow,paginationTarget:{width:next.width,height:next.height}});
    }
    await touchPage.getByRole('button',{name:'الصفحة التالية',exact:true}).tap();
    results.mobileNextPage = await touchPage.evaluate(()=>window.__tableFixture.state.page);
    assert(results.mobileNextPage===2,'Mobile next-page button failed.');
    results.mobileCards = await touchPage.locator('#datatable-refinement-preview [data-index]').count();
    assert(results.mobileCards>0&&results.mobileCards<20,'Mobile card virtualization is missing.');
  } finally {
    await touchContext.close();
  }
  return results;
}
