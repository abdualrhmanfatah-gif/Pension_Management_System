import fs from 'node:fs';
const directory = 'plan/file-audit-2026-10-18';
const read = name => JSON.parse(fs.readFileSync(`${directory}/${name}.json`, 'utf8'));
const front = read('frontend'), back = read('backend'), dup = read('duplicates'), inventory = read('inventory');
const code = value => '`' + value + '`';
const link = (file, line) => `[${file}${line ? ':'+line : ''}](../../${file}${line ? '#L'+line : ''})`;
const table = (headers, rows) => [headers.join(' | '), headers.map(()=>'---').join(' | '), ...rows.map(r=>r.join(' | '))].map(s=>'| '+s+' |').join('\n');
const reasons = {
  'ui/src/components/common/ActionButtons.tsx': 'لا مستورد؛ مكوّن إجراءات عام غير مرتبط بالتطبيق.',
  'ui/src/components/common/FilterBar.tsx': 'لا مستورد؛ لا يظهر في شجرة الصفحات الحالية.',
  'ui/src/components/domain/hr/CreateOrderDialog.tsx': 'مصدّر فقط من ملف تجميع غير مستخدم؛ لا مستهلك فعلي.',
  'ui/src/components/domain/investment/AccountDialog.tsx': 'مصدّر فقط من ملف تجميع غير مستخدم.',
  'ui/src/components/domain/investment/TransactionDialog.tsx': 'مراجعُه محصورة في ملف التجميع وTransactionsTab غير المرتبطين بالتطبيق.',
  'ui/src/components/domain/retirement/EndRecipientDialog.tsx': 'مراجعُه محصورة في ملف التجميع وتبويب المستلمين القديم غير المرتبط بالتطبيق.',
  'ui/src/components/domain/salaries/EntitlementFormDialog.tsx': 'يستهلكه EntitlementsTab غير المرتبط بالتطبيق وملف تجميع غير مستخدم.',
  'ui/src/components/domain/salaries/EntitlementsTab.tsx': 'مصدّر من ملف تجميع غير مستخدم؛ لا صفحة حالية تستورده.',
  'ui/src/components/ui/print/PrintField.tsx': 'مصدّر من print/index.ts غير المستخدم؛ بقية مكونات الطباعة لها استيرادات مباشرة مستقلة.',
  'ui/src/components/ui/print/PrintSectionHeader.tsx': 'مصدّر من print/index.ts غير المستخدم؛ لا مستهلك فعلي.',
  'ui/src/hooks/useDialogNavigation.ts': 'لا مستورد؛ يعتمد وحده على dialogLayers.ts.',
  'ui/src/hooks/useKeyboardNavigation.ts': 'لا مستورد.',
  'ui/src/hooks/usePensionStatistics.ts': 'لا مستورد.',
  'ui/src/lib/dialogLayers.ts': 'المستهلك الوحيد useDialogNavigation.ts غير المستخدم.',
  'ui/src/pages/achievements/AchievementObstacleDetailPage.tsx': 'لا استيراد ولا مسار في App.tsx؛ الصفحة الفعلية تستخدم AchievementObstacleDetailDialog.',
  'ui/src/pages/investment/tabs/TransactionsTab.tsx': 'غير مستورد من ProjectDetailPage أو أي صفحة أخرى.',
  'ui/src/pages/retirement-files/tabs/RecipientsTab.tsx': 'نسخة تبويب غير مستخدمة؛ FileWorkflowManager.tsx:34 يستورد components/domain/salaries/RecipientsTab مباشرة.',
  'ui/src/types/api/fiscal-period.ts': 'لا مستورد؛ تعريفات محاسبية موازية للأنواع المستعملة في types/accounting.ts.',
  'ui/src/types/api/journal-entry.ts': 'لا مستورد؛ تعريفات محاسبية موازية للأنواع المستعملة في types/accounting.ts.',
  'ui/src/types/api/retirement-file.ts': 'لا مستورد؛ يوجد مسار أنواع مستخدم في types/retirement.ts؛ الحقول ليست متطابقة بالكامل.',
  'ui/src/types/api/salary-batch.ts': 'لا مستورد؛ مسار الأنواع المستخدم types/salaries.ts؛ العقود ليست قابلة للاستبدال آليًا.',
  'ui/src/types/financial-entitlements.ts': 'يستهلكه فقط EntitlementFormDialog.tsx غير المستخدم.'
};
const ordinary = front.dead.filter(x=>!x.file.endsWith('/index.ts'));
const barrels = front.dead.filter(x=>x.file.endsWith('/index.ts'));
const extraBackend = [
  ['PensionMS.Infrastructure/Data/EncryptedValueConverter.cs',10,'لا استخدام للصنف خارج ملفه، ولا ربط له عبر HasConversion أو خاصية ValueConverter. وجوده لا يثبت تفعيل تشفير الحقول.'],
  ['PensionMS.Infrastructure/Data/Seeders/JournalEntryTypeSeeder.cs',5,'لا استدعاء للصنف أو SeedAsync الخاصة به؛ غير موجود ضمن PrimaryDataSeeder.SeedAllAsync. يلزم تقرير هل المقصود تفعيله أم إزالته.'],
  ['PensionMS.Core/Helpers/DataMasking.cs',6,'لا مراجع للصنف أو الدوال MaskNationalId وMaskBankAccount وMaskPhone؛ ليست دوال امتداد.'],
  ['PensionMS.Core/Constants/DocumentStatus.cs',6,'غير مستخدم؛ نفس الثوابت Provided/Missing/Expired في DocumentStatusConstants.cs المستخدم فعليًا.'],
  ['PensionMS.Core/Constants/LeaseStatus.cs',6,'غير مستخدم؛ نفس الثوابت Active/Expired/Terminated في LeaseStatusConstants.cs المستخدم فعليًا.'],
  ['PensionMS.Core/Constants/FileReviewStatusConstants.cs',3,'غير مستخدم؛ يكرر القيم 0–3 الموجودة في enum FileReviewStatus داخل Entities/CommonEnums.cs.']
];
const duplicateRows = dup.exact.map((g,i)=>{
  const kind = g.files.some(f=>f.includes('RetireeStatementPrint/')) ? 'صور داخل مصدر الواجهة بلا مراجع' : g.files.some(f=>f.includes('/uploads/')) ? 'بيانات رفع؛ ارتباطاتها قد تكون في قاعدة البيانات' : 'أصل مشترك بين الواجهة والخادم؛ استخدامه مؤكد في NotificationProvider.tsx';
  return [i+1,g.files.map(f=>link(f)).join('<br>'),g.bytes.toLocaleString('en-US'),kind];
});
const assetCandidates = [...front.assetRefs,...front.sourceAssetRefs].filter(x=>!x.references.length);
const cloneRows = dup.exactFunctionClones.map(group=>[code(group[0].name),group.map(f=>link(f.file,f.line)).join('<br>')]);
const report = `# فحص الملفات المكررة وغير المستخدمة

تاريخ الفحص: 18 أكتوبر 2026، وفق النسخة المحلية الحالية بما فيها التغييرات غير المحفوظة في Git. أُجري فحص وكتابة أدلة فقط؛ لم يُحذف أو يُعدّل أي ملف من كود التطبيق.

## النتيجة

${table(['البند','النتيجة'],[
  ['الملفات المشمولة بالجرد',inventory.files.length.toString()],
  ['ملفات الكود C#/TS/TSX/JS ضمن التطبيق',inventory.appCode.toString()],
  ['ملفات C# المفحوصة، بما فيها الاختبارات',back.fileCount.toString()],
  ['ملفات مصدر الواجهة في فحص الوصول، باستثناء الاختبارات والقصص والتعريفات المحيطية',front.sourceCount.toString()],
  ['ملفات واجهة لا تصلها الاستيرادات من التطبيق أو الاختبارات أو أدوات الواجهة',front.dead.length+' = '+ordinary.length+' ملفًا وظيفيًا/تعريفيًا و'+barrels.length+' ملف تجميع index.ts'],
  ['ملفات خلفية غير مستهلكة وقت التشغيل، عالية الثقة','11: أربع خدمات مواطن + مهمة خلفية + ستة ملفات مساعدة/ثوابت'],
  ['مجموعات ملفات متطابقة بايتًا، بعد استبعاد مخرجات البناء والمتصفح',dup.exact.length.toString()],
  ['ملفات مصدر كود متطابقة بالكامل بايتًا أو بعد حذف المسافات','لم يُعثر على أزواج ضمن نطاق الفحص'],
  ['أبرز أزواج التشابه في الكود','زوجان، إضافة إلى تكرار ثوابت ومجموعات دوال'],
  ['أصول واجهة بلا مرجع نصي في المصدر/الإعدادات',assetCandidates.length.toString()],
  ['حزم dependencies في ui/package.json بلا استيراد مباشر',front.unimportedDependencies.length+' مرشحًا للمراجعة، لا للحذف التلقائي']
])}

هذه أعداد نتائج مؤكدة أو مرشحة بالمنهج المذكور، وليست إثباتًا أن كل ما عداها مستخدم. ملفات التجميع قد تجعل ملفًا قابلًا للوصول عبر export رغم أن رموزه لا تُستعمل؛ لم يُجرَ تحليل إزالة الشيفرة الميتة على مستوى كل export.

## طريقة الفحص وحدوده

1. جرد الملفات الحالية عبر ${code('git ls-files --cached --others --exclude-standard')}، واستبعاد المسارات المحذوفة فعليًا. استُبعدت مجلدات الأدوات والمهارات وplan وtmp ومخرجات bin/obj/dist/publish وملفات المتصفح. تُحترم .gitignore؛ الملفات المتجاهلة غير المتتبعة خارج هذا الجرد.
2. مقارنة SHA-256 لمحتوى الملفات؛ وفحص مساعد لتطابق ملفات الكود بعد إزالة المسافات. نتيجة التطابق البايتي منفصلة عن التشابه المنطقي.
3. تحليل AST باستخدام TypeScript 5.9 المثبت محليًا، وحل المسارات وفق tsconfig.app.json، وتتبع import وexport وimport() وrequire() وnew URL للـWorker؛ ثم المرور من مدخل ui/index.html إلى src/main.tsx وباقي التطبيق، ومن مداخل الاختبارات وأدوات الواجهة. لم يُعثر على import() بمسار متغير أو import.meta.glob في المصدر. راجعت أيضًا مراجع المرشحين في الاختبارات التي تقرأ الملفات كنصوص.
4. مراجعة رموز C# ومستهلكي الواجهات، وسلسلة تسجيل DI الفعلية، والاختبارات، والاكتشاف التلقائي في ASP.NET وEF. التحليل الآلي الخلفي نصي محافظ، ثم خضعت النتائج المذكورة هنا للمراجعة؛ ملف backend.json يحتوي مرشحين خامًا لا يمثلون قائمة حذف.
5. قياس التشابه بكتل من أربعة أسطر منسقة، ونسبة Dice لتقاطع الكتل الفريدة؛ النسبة مؤشر تشابه وليست نسبة الأسطر القابلة للحذف. كذلك قورنت الدوال العليا بطباعة AST مع إزالة التعليقات.
6. البحث عن مراجع الأصول في المصدر والإعدادات، وليس في قاعدة البيانات أو حركة الشبكة الفعلية. خصوصًا ملفات uploads والروابط الخارجية، لا يثبت غياب مرجع نصي أنها غير مستخدمة.

ملاحظة على الحالة الحالية: تسجيل الخدمات انتقل من الأسطر المباشرة في Program.cs إلى ${link('PensionMS.API/Program.cs',73)} ثم ${link('PensionMS.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs',7)}. لذلك لم يُعتمد غياب اسم الخدمة من Program.cs وحده. ويوجد مشروع PensionMS.IntegrationTests حاليًا وقد شمله البحث، رغم أن بعض وصف المشروع أقدم من هذه البنية.

## ملفات الواجهة غير المرتبطة بالتطبيق

درجة الثقة عالية في غياب استخدامها عبر سلسلة الاستيراد الحالية. بعض الملفات تستورد بعضها داخل مجموعة معزولة؛ لذلك لا يكفي البحث عن الملفات ذات صفر مستوردين فقط. هذه الملفات ما زالت ضمن مصدر TypeScript وقد يفحصها البناء؛ «غير مستخدم» هنا يعني غير مرتبط بمداخل التشغيل والاختبار.

${table(['الملف','عدد الأسطر التقريبي','الدليل/الملاحظة'],ordinary.map(x=>[link(x.file),x.lines,reasons[x.file]??'لا مسار وصول من المداخل المفحوصة.']))}

### ملفات التجميع غير المستوردة

${table(['الملف','الدليل'],barrels.map(x=>[link(x.file),'لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة.']))}

لا يُحذف مجلد كامل اعتمادًا على عدم استخدام index.ts: مثل BankFormDialog وPrintLayout وغيرها تُستورد مباشرة من ملفاتها.

### تكرار وظيفي يستحق الانتباه

${link('ui/src/pages/retirement-files/tabs/RecipientsTab.tsx',27)} و${link('ui/src/components/domain/salaries/RecipientsTab.tsx',31)} يؤديان دور تبويب مستلمي المعاش، لكنهما مختلفان في السلوك والعقود وليسَا نسختين متطابقتين. المستورد في ${link('ui/src/pages/retirement-files/FileWorkflowManager.tsx',34)} هو الثاني. الأول غير مرتبط بالتطبيق، ومعه EndRecipientDialog. كذلك AchievementObstacleDetailPage غير مستوردة بينما AchievementObstaclesPage تستخدم AchievementObstacleDetailDialog. يجب مقارنة الوظائف قبل إزالة أي نسخة كي لا تُفقد معالجة موجودة فقط في النسخة القديمة.

## ملفات الخلفية غير المستخدمة وقت التشغيل

### خدمات غير مسجلة

${table(['الملف','الدليل'],back.inactiveServices.map(x=>[link(x.file,x.line),x.symbol==='PledgeExpiryCheckerService'?'غير مسجل كـHostedService؛ المرجع الخارجي الوحيد اختبار يثبت عدم تسجيله. البديل العامل ExpiredPledgesJob.':'لا مرجع إلى التنفيذ خارج الملف، ولا تسجيل DI أو استهلاك إنتاجي لواجهته. الاختبار يثبت غياب تسجيل الواجهة.']))}

الدليل المشترك: ${link('PensionMS.Infrastructure/DependencyInjection/CitizenServiceCollectionExtensions.cs',9)} يسجل SmsService وOtpService فقط، و${link('PensionMS.Infrastructure/DependencyInjection/BackgroundJobsServiceCollectionExtensions.cs',8)} يسجل ExpiredPledgesJob وNotificationOutboxProcessor فقط. اختبار ${link('PensionMS.Tests/DependencyInjectionTests.cs',38)} يتحقق صراحةً من هذه الحدود.

PledgeExpiryCheckerService يتقاطع وظيفيًا مع ExpiredPledgesJob، لكن يختلف عنه: الأول يفحص كل ست ساعات ويغير حالات التعهدات مباشرة، والثاني يعمل كل 24 ساعة ويفوض المعالجة إلى IFileStatusService.ProcessExpiredPledgesAsync. لا ينبغي تفعيل الاثنين أو افتراض تكافئهما.

### ملفات أخرى بلا مستهلك

${table(['الملف','الدليل/الإجراء المقترح'],extraBackend.map(([file,line,reason])=>[link(file,line),reason]))}

تكرار DocumentStatus وLeaseStatus هو تكرار تعريفات بقيم واحدة مع اختلاف أسماء الأصناف، ولذلك لم يظهر ضمن تطابق SHA-256. ثوابت FileReviewStatusConstants تكرر enum مستخدمًا في CommonEnums.cs. يمكن توحيد المصدر بعد التحقق من التعاقدات، دون تغيير القيم.

### توابع وحدة المواطن: ليست ملفات مستقلة آمنة للحذف

الواجهات ICitizenAuthService وICitizenDataService وICitizenSupportService وICitizenServicePortal في PensionMS.Core/Interfaces مرتبطة بالخدمات المعطلة ويشير إليها DependencyInjectionTests. حذفها أو حذف PledgeExpiryCheckerService يستلزم تحديث هذه الاختبارات في نفس التغيير كي لا ينكسر التجميع. ملفات DTO في PensionMS.Core/DTOs/Citizen مرتبطة بهذه الوحدة أيضًا؛ لم تُدرج كملفات منفصلة محسومة الحذف. أما SmsService وOtpService فهما مسجلان حاليًا، لكن مستهلك واجهتيهما الإنتاجي الظاهر هو CitizenAuthService المعطل؛ يصنفان كخدمات مسجلة بلا مسار استهلاك فعلي ظاهر، وتحتاجان مراجعة على مستوى الوحدة.

### نتائج ظاهرية استُبعدت

- Controllers: يكتشفها AddControllers تلقائيًا.
- Validators: تسجل عبر AddValidatorsFromAssemblyContaining.
- EF configurations: تُحمّل عبر ApplyConfigurationsFromAssembly في ${link('PensionMS.Infrastructure/Data/PensionDbContext.cs',209)}.
- ControllerExtensions وملفات *ServiceCollectionExtensions: تُستخدم دوال الامتداد دون ذكر اسم الصنف.
- PensionDbContextFactory: مدخل أدوات EF في وقت التصميم.
- RemoteFileModelBinder: يستدعيه RemoteFileModelBinderProvider في نفس الملف، والمزود مسجل في ${link('PensionMS.API/Program.cs',426)}؛ ليس ملفًا مهجورًا.
- perspective-warp.worker.ts: مستدعى عبر new URL داخل ${link('ui/src/lib/crop-worker-client.ts',28)}؛ لا يصنف غير مستخدم لغياب import تقليدي.

## التكرار في الكود المستخدم

${table(['الملفان','تشابه الكتل','الملاحظة'],dup.similar.map(x=>[x.files.map(f=>link(f)).join('<br>'),x.dice+'%',x.files[0].includes('correspondence')?'كلاهما مربوط بمسار في App.tsx. البحث والترقيم والتركيز والحذف ونموذج المراسلة متكررة؛ يمكن استخراج صفحة مشتركة بمعامل direction والعنوان والأيقونة.':'كلاهما مستخدم. تتكرر إدارة Canvas والتدوير والتحسين والتصدير؛ يمكن استخراج دوال ومتحكم معالجة مشترك مع إبقاء غلاف الصفحة/النافذة.']))}

مجموعات دوال متطابقة بعد إزالة التعليقات وتوحيد طباعة AST، وليست مجرد تشابه أسماء:

${table(['الدالة','المواضع'],cloneRows)}

هذه فرص دمج للمنطق المشترك؛ حذف إحدى الواجهتين سيزيل وظيفة مستخدمة.

## الملفات المتطابقة بايتًا

${table(['المجموعة','المسارات','حجم النسخة بالبايت','التصنيف'],duplicateRows)}

بصمات SHA-256 الكاملة في ${link(directory+'/duplicates.json')}. الزيادة النظرية فوق نسخة واحدة لكل مجموعة تساوي ${(dup.exact.reduce((n,g)=>n+(g.files.length-1)*g.bytes,0)/1024/1024).toFixed(2)} MiB، وليست مساحة آمنة للحذف آليًا: ملفات الرفع قد ترتبط بسجلات مختلفة. ملفات t_doc_probe الأربعة حجم كل منها 8 بايت فقط؛ تبدو آثار اختبارات وليست صورًا كاملة، لكن ربطها بسجلات قاعدة البيانات لم يُفحص.

الصور السبع في ui/src/pages/retirees/image/RetireeStatementPrint متطابقة، ولم يُعثر على مرجع لأي منها. مجموع حجمها ${(7*202950/1024/1024).toFixed(2)} MiB؛ وهي أوضح مجموعة تكرار غير مستخدم في مصدر الواجهة. أما notification-sound.mp3 فمستخدم فعلًا، ونسخه بين UI/API تتطلب مراعاة الاستضافة قبل التوحيد.

## أصول وملفات مساعدة مرشحة للمراجعة

${table(['الأصل','الحجم بالبايت','الثقة'],assetCandidates.map(x=>[link(x.file),x.bytes.toLocaleString('en-US'),x.file.startsWith('ui/public/')?'متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL.':'عالية: لا استيراد ولا مرجع نصي.']))}

نسخ PNG لترويسات الطباعة والختم والتوقيع مستخدمة فعليًا، وبعضها يدخل إخراج PDF/Excel. وpension-logo.webp مستخدم في صفحة الدخول والتنقل وindex.html؛ لا يندرج ضمن الصور غير المستخدمة. ملفات الخطوط المحلية لها مراجع ولم تُصنّف مهجورة.

${table(['مجلد الأدلة/المخرجات','عدد الملفات المحصاة','الحجم المحصى'],inventory.artifacts.map(x=>[code(x.prefix),x.files,(x.bytes/1024/1024).toFixed(2)+' MiB']))}

هذه الأعداد للمسارات التي أعادها Git؛ قد تحتوي المجلدات مخرجات إضافية متجاهلة. استُبعدت من أرقام التكرار الأساسية لأنها آثار تشغيل/نشر. ملفات ${link('patch.py')} و${link('patch_perf.py')} سكربتات تعديل يدوية لـFileUploader بمسار محلي ثابت، ولا مرجع لهما ضمن أوامر التشغيل أو النشر؛ هما أدوات تاريخية مرشحة للأرشفة، لا ملفات تشغيل للتطبيق.

## الحزم التي لا يظهر لها استيراد مباشر

${front.unimportedDependencies.map(code).join('، ')}.

هذه قائمة مراجعة تبعيات فقط. غياب الاستيراد المباشر لا يستبعد الاستخدام غير المباشر أو peerDependencies: مثل immer، وqrcode مقابل qrcode.react؛ يلزم npm explain قبل الحذف في تغيير لاحق. كما توجد قواعد CSS قديمة لـreact-image-crop في index.css، لكن لا استيراد للمكتبة الحالية.

## ملاحظة إضافية

${link('ui/src/components/ui/stories/DemoDialog.stories.tsx',7)} يحتوي ثلاثة استيرادات محلية لا تُحل: ../../BaseFormDialog و../../Button و../../Input. الملف قصة Storybook، مستثنى من بناء التطبيق، وpackage.json لا يعرّف أمرًا لتشغيل Storybook؛ لا يُحسب ضمن الملفات الوظيفية الـ33. يصلح للمراجعة كتوثيق/تجربة قديمة بدل اعتباره اعتمادًا مفقودًا من التطبيق الحي.

## التحقق وخطوة التنظيف المقترحة

شُغّل من PensionMS.Tests:

~~~powershell
dotnet test --no-restore --filter FullyQualifiedName~DependencyInjectionTests --nologo
~~~

النتيجة: **6 ناجحة، 0 فاشلة، 0 متخطاة**. ظهرت تحذيرات بناء عن nullable ودعم System.Drawing للمنصات. سجل التشغيل في ${link(directory+'/backend-di-tests.txt')}. هذا يثبت سلوك تسجيل DI الحالي ولا يثبت قابلية حذف الملفات أو سلامة جميع مسارات التطبيق. لم تُشغل حزمة فحوص UI كاملة أو اختبارات Backend كاملة، إذ لم يتغير كود التطبيق.

الترتيب المقترح للعمل اللاحق:

1. مراجعة وإزالة/أرشفة ملفات الواجهة الـ33 والأصول غير المرتبطة بعد مقارنة الوظائف البديلة؛ فصل التغييرات إلى دفعات صغيرة.
2. حسم مصير وحدة المواطن والمهمة الخلفية القديمة، ثم إزالة التنفيذ والعقود والاختبارات التابعة بصورة متناسقة أو تسجيل الوظيفة المطلوبة عمدًا.
3. توحيد الثوابت، وحسم تفعيل/إزالة EncryptedValueConverter وJournalEntryTypeSeeder وDataMasking وفق المتطلبات.
4. استخراج المنطق المشترك للوارد/الصادر ولمعالجة الصور، مع اختبارات تحقق سلوك مناسبة.
5. مراجعة تبعيات npm وآثار النشر والتشخيص؛ عدم حذف uploads بناءً على فحص المصدر.
6. بعد أي تعديل UI: التقاط lint قبل/بعد ومقارنته، ثم build ثم test ثم preview وفق تعليمات المشروع. تحديث اختبارات Backend عند تعديل الأنواع التي تشير إليها.

## أدلة قابلة لإعادة التشغيل

- ${link(directory+'/analyze.mjs')}: تشغيل ${code('node '+directory+'/analyze.mjs')} من جذر المستودع؛ لا يعدّل كود التطبيق.
- ${link(directory+'/report.mjs')}: يولّد هذا التقرير من الأدلة بعد إعادة التحليل؛ وصف المراجعة اليدوية مرتبط بحالة الفحص الحالية ويجب تحديثه عند تغير الكود.
- ${link(directory+'/inventory.json')}: نطاق الجرد وأعداد الملفات والمخرجات المستبعدة.
- ${link(directory+'/frontend.json')}: حواف الاستيراد، المستوردون لكل مرشح، ومراجع الأصول.
- ${link(directory+'/backend.json')}: مراجع الرموز والخدمات؛ المرشحون الخام ليسوا أحكامًا بالحذف.
- ${link(directory+'/duplicates.json')}: بصمات الملفات ودرجات التشابه ومواضع الدوال المتكررة.
`;
fs.writeFileSync('docs/audits/code-duplicates-unused-files-2026-10-18.md',report,'utf8');
console.log(JSON.stringify({report:'docs/audits/code-duplicates-unused-files-2026-10-18.md',frontendCandidates:front.dead.length,backendCandidates:back.inactiveServices.length+extraBackend.length,assetCandidates:assetCandidates.length,exactGroups:dup.exact.length,functionGroups:dup.exactFunctionClones.length},null,2));
