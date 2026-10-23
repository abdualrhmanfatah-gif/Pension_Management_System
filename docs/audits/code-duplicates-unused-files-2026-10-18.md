# فحص الملفات المكررة وغير المستخدمة

> تحديث 20 أكتوبر 2026: نُفذت إزالة الملفات غير المستخدمة وتوحيد أجزاء من الكود المكرر. هذا التقرير يحتفظ بنتائج الفحص قبل التنظيف؛ راجع [نتائج التنظيف](code-cleanup-2026-10-20.md) للحالة الجديدة ونتائج التحقق.

تاريخ الفحص: 18 أكتوبر 2026، وفق النسخة المحلية الحالية بما فيها التغييرات غير المحفوظة في Git. أُجري فحص وكتابة أدلة فقط؛ لم يُحذف أو يُعدّل أي ملف من كود التطبيق.

## النتيجة

| البند | النتيجة |
| --- | --- |
| الملفات المشمولة بالجرد | 1486 |
| ملفات الكود C#/TS/TSX/JS ضمن التطبيق | 1294 |
| ملفات C# المفحوصة، بما فيها الاختبارات | 721 |
| ملفات مصدر الواجهة في فحص الوصول، باستثناء الاختبارات والقصص والتعريفات المحيطية | 569 |
| ملفات واجهة لا تصلها الاستيرادات من التطبيق أو الاختبارات أو أدوات الواجهة | 33 = 22 ملفًا وظيفيًا/تعريفيًا و11 ملف تجميع index.ts |
| ملفات خلفية غير مستهلكة وقت التشغيل، عالية الثقة | 11: أربع خدمات مواطن + مهمة خلفية + ستة ملفات مساعدة/ثوابت |
| مجموعات ملفات متطابقة بايتًا، بعد استبعاد مخرجات البناء والمتصفح | 5 |
| ملفات مصدر كود متطابقة بالكامل بايتًا أو بعد حذف المسافات | لم يُعثر على أزواج ضمن نطاق الفحص |
| أبرز أزواج التشابه في الكود | زوجان، إضافة إلى تكرار ثوابت ومجموعات دوال |
| أصول واجهة بلا مرجع نصي في المصدر/الإعدادات | 15 |
| حزم dependencies في ui/package.json بلا استيراد مباشر | 13 مرشحًا للمراجعة، لا للحذف التلقائي |

هذه أعداد نتائج مؤكدة أو مرشحة بالمنهج المذكور، وليست إثباتًا أن كل ما عداها مستخدم. ملفات التجميع قد تجعل ملفًا قابلًا للوصول عبر export رغم أن رموزه لا تُستعمل؛ لم يُجرَ تحليل إزالة الشيفرة الميتة على مستوى كل export.

## طريقة الفحص وحدوده

1. جرد الملفات الحالية عبر `git ls-files --cached --others --exclude-standard`، واستبعاد المسارات المحذوفة فعليًا. استُبعدت مجلدات الأدوات والمهارات وplan وtmp ومخرجات bin/obj/dist/publish وملفات المتصفح. تُحترم .gitignore؛ الملفات المتجاهلة غير المتتبعة خارج هذا الجرد.
2. مقارنة SHA-256 لمحتوى الملفات؛ وفحص مساعد لتطابق ملفات الكود بعد إزالة المسافات. نتيجة التطابق البايتي منفصلة عن التشابه المنطقي.
3. تحليل AST باستخدام TypeScript 5.9 المثبت محليًا، وحل المسارات وفق tsconfig.app.json، وتتبع import وexport وimport() وrequire() وnew URL للـWorker؛ ثم المرور من مدخل ui/index.html إلى src/main.tsx وباقي التطبيق، ومن مداخل الاختبارات وأدوات الواجهة. لم يُعثر على import() بمسار متغير أو import.meta.glob في المصدر. راجعت أيضًا مراجع المرشحين في الاختبارات التي تقرأ الملفات كنصوص.
4. مراجعة رموز C# ومستهلكي الواجهات، وسلسلة تسجيل DI الفعلية، والاختبارات، والاكتشاف التلقائي في ASP.NET وEF. التحليل الآلي الخلفي نصي محافظ، ثم خضعت النتائج المذكورة هنا للمراجعة؛ ملف backend.json يحتوي مرشحين خامًا لا يمثلون قائمة حذف.
5. قياس التشابه بكتل من أربعة أسطر منسقة، ونسبة Dice لتقاطع الكتل الفريدة؛ النسبة مؤشر تشابه وليست نسبة الأسطر القابلة للحذف. كذلك قورنت الدوال العليا بطباعة AST مع إزالة التعليقات.
6. البحث عن مراجع الأصول في المصدر والإعدادات، وليس في قاعدة البيانات أو حركة الشبكة الفعلية. خصوصًا ملفات uploads والروابط الخارجية، لا يثبت غياب مرجع نصي أنها غير مستخدمة.

ملاحظة على الحالة الحالية: تسجيل الخدمات انتقل من الأسطر المباشرة في Program.cs إلى [PensionMS.API/Program.cs:73](../../PensionMS.API/Program.cs#L73) ثم [PensionMS.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs:7](../../PensionMS.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs#L7). لذلك لم يُعتمد غياب اسم الخدمة من Program.cs وحده. ويوجد مشروع PensionMS.IntegrationTests حاليًا وقد شمله البحث، رغم أن بعض وصف المشروع أقدم من هذه البنية.

## ملفات الواجهة غير المرتبطة بالتطبيق

درجة الثقة عالية في غياب استخدامها عبر سلسلة الاستيراد الحالية. بعض الملفات تستورد بعضها داخل مجموعة معزولة؛ لذلك لا يكفي البحث عن الملفات ذات صفر مستوردين فقط. هذه الملفات ما زالت ضمن مصدر TypeScript وقد يفحصها البناء؛ «غير مستخدم» هنا يعني غير مرتبط بمداخل التشغيل والاختبار.

| الملف | عدد الأسطر التقريبي | الدليل/الملاحظة |
| --- | --- | --- |
| [ui/src/components/common/ActionButtons.tsx](../../ui/src/components/common/ActionButtons.tsx) | 84 | لا مستورد؛ مكوّن إجراءات عام غير مرتبط بالتطبيق. |
| [ui/src/components/common/FilterBar.tsx](../../ui/src/components/common/FilterBar.tsx) | 123 | لا مستورد؛ لا يظهر في شجرة الصفحات الحالية. |
| [ui/src/components/domain/hr/CreateOrderDialog.tsx](../../ui/src/components/domain/hr/CreateOrderDialog.tsx) | 166 | مصدّر فقط من ملف تجميع غير مستخدم؛ لا مستهلك فعلي. |
| [ui/src/components/domain/investment/AccountDialog.tsx](../../ui/src/components/domain/investment/AccountDialog.tsx) | 129 | مصدّر فقط من ملف تجميع غير مستخدم. |
| [ui/src/components/domain/investment/TransactionDialog.tsx](../../ui/src/components/domain/investment/TransactionDialog.tsx) | 187 | مراجعُه محصورة في ملف التجميع وTransactionsTab غير المرتبطين بالتطبيق. |
| [ui/src/components/domain/retirement/EndRecipientDialog.tsx](../../ui/src/components/domain/retirement/EndRecipientDialog.tsx) | 110 | مراجعُه محصورة في ملف التجميع وتبويب المستلمين القديم غير المرتبط بالتطبيق. |
| [ui/src/components/domain/salaries/EntitlementFormDialog.tsx](../../ui/src/components/domain/salaries/EntitlementFormDialog.tsx) | 130 | يستهلكه EntitlementsTab غير المرتبط بالتطبيق وملف تجميع غير مستخدم. |
| [ui/src/components/domain/salaries/EntitlementsTab.tsx](../../ui/src/components/domain/salaries/EntitlementsTab.tsx) | 148 | مصدّر من ملف تجميع غير مستخدم؛ لا صفحة حالية تستورده. |
| [ui/src/components/ui/print/PrintField.tsx](../../ui/src/components/ui/print/PrintField.tsx) | 46 | مصدّر من print/index.ts غير المستخدم؛ بقية مكونات الطباعة لها استيرادات مباشرة مستقلة. |
| [ui/src/components/ui/print/PrintSectionHeader.tsx](../../ui/src/components/ui/print/PrintSectionHeader.tsx) | 31 | مصدّر من print/index.ts غير المستخدم؛ لا مستهلك فعلي. |
| [ui/src/hooks/useDialogNavigation.ts](../../ui/src/hooks/useDialogNavigation.ts) | 87 | لا مستورد؛ يعتمد وحده على dialogLayers.ts. |
| [ui/src/hooks/useKeyboardNavigation.ts](../../ui/src/hooks/useKeyboardNavigation.ts) | 62 | لا مستورد. |
| [ui/src/hooks/usePensionStatistics.ts](../../ui/src/hooks/usePensionStatistics.ts) | 18 | لا مستورد. |
| [ui/src/lib/dialogLayers.ts](../../ui/src/lib/dialogLayers.ts) | 12 | المستهلك الوحيد useDialogNavigation.ts غير المستخدم. |
| [ui/src/pages/achievements/AchievementObstacleDetailPage.tsx](../../ui/src/pages/achievements/AchievementObstacleDetailPage.tsx) | 289 | لا استيراد ولا مسار في App.tsx؛ الصفحة الفعلية تستخدم AchievementObstacleDetailDialog. |
| [ui/src/pages/investment/tabs/TransactionsTab.tsx](../../ui/src/pages/investment/tabs/TransactionsTab.tsx) | 126 | غير مستورد من ProjectDetailPage أو أي صفحة أخرى. |
| [ui/src/pages/retirement-files/tabs/RecipientsTab.tsx](../../ui/src/pages/retirement-files/tabs/RecipientsTab.tsx) | 314 | نسخة تبويب غير مستخدمة؛ FileWorkflowManager.tsx:34 يستورد components/domain/salaries/RecipientsTab مباشرة. |
| [ui/src/types/api/fiscal-period.ts](../../ui/src/types/api/fiscal-period.ts) | 31 | لا مستورد؛ تعريفات محاسبية موازية للأنواع المستعملة في types/accounting.ts. |
| [ui/src/types/api/journal-entry.ts](../../ui/src/types/api/journal-entry.ts) | 63 | لا مستورد؛ تعريفات محاسبية موازية للأنواع المستعملة في types/accounting.ts. |
| [ui/src/types/api/retirement-file.ts](../../ui/src/types/api/retirement-file.ts) | 130 | لا مستورد؛ يوجد مسار أنواع مستخدم في types/retirement.ts؛ الحقول ليست متطابقة بالكامل. |
| [ui/src/types/api/salary-batch.ts](../../ui/src/types/api/salary-batch.ts) | 64 | لا مستورد؛ مسار الأنواع المستخدم types/salaries.ts؛ العقود ليست قابلة للاستبدال آليًا. |
| [ui/src/types/financial-entitlements.ts](../../ui/src/types/financial-entitlements.ts) | 16 | يستهلكه فقط EntitlementFormDialog.tsx غير المستخدم. |

### ملفات التجميع غير المستوردة

| الملف | الدليل |
| --- | --- |
| [ui/src/components/domain/admin/index.ts](../../ui/src/components/domain/admin/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/assets/index.ts](../../ui/src/components/domain/assets/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/correspondence/index.ts](../../ui/src/components/domain/correspondence/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/hr/index.ts](../../ui/src/components/domain/hr/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/inventory/index.ts](../../ui/src/components/domain/inventory/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/investment/index.ts](../../ui/src/components/domain/investment/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/print/index.ts](../../ui/src/components/domain/print/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/retirement/index.ts](../../ui/src/components/domain/retirement/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/salaries/index.ts](../../ui/src/components/domain/salaries/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/domain/settlements/index.ts](../../ui/src/components/domain/settlements/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |
| [ui/src/components/ui/print/index.ts](../../ui/src/components/ui/print/index.ts) | لا ملف يستورده؛ تصديره لمكونات أخرى لا يعني أن تلك المكونات نفسها غير مستخدمة. |

لا يُحذف مجلد كامل اعتمادًا على عدم استخدام index.ts: مثل BankFormDialog وPrintLayout وغيرها تُستورد مباشرة من ملفاتها.

### تكرار وظيفي يستحق الانتباه

[ui/src/pages/retirement-files/tabs/RecipientsTab.tsx:27](../../ui/src/pages/retirement-files/tabs/RecipientsTab.tsx#L27) و[ui/src/components/domain/salaries/RecipientsTab.tsx:31](../../ui/src/components/domain/salaries/RecipientsTab.tsx#L31) يؤديان دور تبويب مستلمي المعاش، لكنهما مختلفان في السلوك والعقود وليسَا نسختين متطابقتين. المستورد في [ui/src/pages/retirement-files/FileWorkflowManager.tsx:34](../../ui/src/pages/retirement-files/FileWorkflowManager.tsx#L34) هو الثاني. الأول غير مرتبط بالتطبيق، ومعه EndRecipientDialog. كذلك AchievementObstacleDetailPage غير مستوردة بينما AchievementObstaclesPage تستخدم AchievementObstacleDetailDialog. يجب مقارنة الوظائف قبل إزالة أي نسخة كي لا تُفقد معالجة موجودة فقط في النسخة القديمة.

## ملفات الخلفية غير المستخدمة وقت التشغيل

### خدمات غير مسجلة

| الملف | الدليل |
| --- | --- |
| [PensionMS.Infrastructure/Jobs/PledgeExpiryCheckerService.cs:14](../../PensionMS.Infrastructure/Jobs/PledgeExpiryCheckerService.cs#L14) | غير مسجل كـHostedService؛ المرجع الخارجي الوحيد اختبار يثبت عدم تسجيله. البديل العامل ExpiredPledgesJob. |
| [PensionMS.Infrastructure/Services/Citizen/CitizenAuthService.cs:14](../../PensionMS.Infrastructure/Services/Citizen/CitizenAuthService.cs#L14) | لا مرجع إلى التنفيذ خارج الملف، ولا تسجيل DI أو استهلاك إنتاجي لواجهته. الاختبار يثبت غياب تسجيل الواجهة. |
| [PensionMS.Infrastructure/Services/Citizen/CitizenDataService.cs:7](../../PensionMS.Infrastructure/Services/Citizen/CitizenDataService.cs#L7) | لا مرجع إلى التنفيذ خارج الملف، ولا تسجيل DI أو استهلاك إنتاجي لواجهته. الاختبار يثبت غياب تسجيل الواجهة. |
| [PensionMS.Infrastructure/Services/Citizen/CitizenServicePortal.cs:8](../../PensionMS.Infrastructure/Services/Citizen/CitizenServicePortal.cs#L8) | لا مرجع إلى التنفيذ خارج الملف، ولا تسجيل DI أو استهلاك إنتاجي لواجهته. الاختبار يثبت غياب تسجيل الواجهة. |
| [PensionMS.Infrastructure/Services/Citizen/CitizenSupportService.cs:8](../../PensionMS.Infrastructure/Services/Citizen/CitizenSupportService.cs#L8) | لا مرجع إلى التنفيذ خارج الملف، ولا تسجيل DI أو استهلاك إنتاجي لواجهته. الاختبار يثبت غياب تسجيل الواجهة. |

الدليل المشترك: [PensionMS.Infrastructure/DependencyInjection/CitizenServiceCollectionExtensions.cs:9](../../PensionMS.Infrastructure/DependencyInjection/CitizenServiceCollectionExtensions.cs#L9) يسجل SmsService وOtpService فقط، و[PensionMS.Infrastructure/DependencyInjection/BackgroundJobsServiceCollectionExtensions.cs:8](../../PensionMS.Infrastructure/DependencyInjection/BackgroundJobsServiceCollectionExtensions.cs#L8) يسجل ExpiredPledgesJob وNotificationOutboxProcessor فقط. اختبار [PensionMS.Tests/DependencyInjectionTests.cs:38](../../PensionMS.Tests/DependencyInjectionTests.cs#L38) يتحقق صراحةً من هذه الحدود.

PledgeExpiryCheckerService يتقاطع وظيفيًا مع ExpiredPledgesJob، لكن يختلف عنه: الأول يفحص كل ست ساعات ويغير حالات التعهدات مباشرة، والثاني يعمل كل 24 ساعة ويفوض المعالجة إلى IFileStatusService.ProcessExpiredPledgesAsync. لا ينبغي تفعيل الاثنين أو افتراض تكافئهما.

### ملفات أخرى بلا مستهلك

| الملف | الدليل/الإجراء المقترح |
| --- | --- |
| [PensionMS.Infrastructure/Data/EncryptedValueConverter.cs:10](../../PensionMS.Infrastructure/Data/EncryptedValueConverter.cs#L10) | لا استخدام للصنف خارج ملفه، ولا ربط له عبر HasConversion أو خاصية ValueConverter. وجوده لا يثبت تفعيل تشفير الحقول. |
| [PensionMS.Infrastructure/Data/Seeders/JournalEntryTypeSeeder.cs:5](../../PensionMS.Infrastructure/Data/Seeders/JournalEntryTypeSeeder.cs#L5) | لا استدعاء للصنف أو SeedAsync الخاصة به؛ غير موجود ضمن PrimaryDataSeeder.SeedAllAsync. يلزم تقرير هل المقصود تفعيله أم إزالته. |
| [PensionMS.Core/Helpers/DataMasking.cs:6](../../PensionMS.Core/Helpers/DataMasking.cs#L6) | لا مراجع للصنف أو الدوال MaskNationalId وMaskBankAccount وMaskPhone؛ ليست دوال امتداد. |
| [PensionMS.Core/Constants/DocumentStatus.cs:6](../../PensionMS.Core/Constants/DocumentStatus.cs#L6) | غير مستخدم؛ نفس الثوابت Provided/Missing/Expired في DocumentStatusConstants.cs المستخدم فعليًا. |
| [PensionMS.Core/Constants/LeaseStatus.cs:6](../../PensionMS.Core/Constants/LeaseStatus.cs#L6) | غير مستخدم؛ نفس الثوابت Active/Expired/Terminated في LeaseStatusConstants.cs المستخدم فعليًا. |
| [PensionMS.Core/Constants/FileReviewStatusConstants.cs:3](../../PensionMS.Core/Constants/FileReviewStatusConstants.cs#L3) | غير مستخدم؛ يكرر القيم 0–3 الموجودة في enum FileReviewStatus داخل Entities/CommonEnums.cs. |

تكرار DocumentStatus وLeaseStatus هو تكرار تعريفات بقيم واحدة مع اختلاف أسماء الأصناف، ولذلك لم يظهر ضمن تطابق SHA-256. ثوابت FileReviewStatusConstants تكرر enum مستخدمًا في CommonEnums.cs. يمكن توحيد المصدر بعد التحقق من التعاقدات، دون تغيير القيم.

### توابع وحدة المواطن: ليست ملفات مستقلة آمنة للحذف

الواجهات ICitizenAuthService وICitizenDataService وICitizenSupportService وICitizenServicePortal في PensionMS.Core/Interfaces مرتبطة بالخدمات المعطلة ويشير إليها DependencyInjectionTests. حذفها أو حذف PledgeExpiryCheckerService يستلزم تحديث هذه الاختبارات في نفس التغيير كي لا ينكسر التجميع. ملفات DTO في PensionMS.Core/DTOs/Citizen مرتبطة بهذه الوحدة أيضًا؛ لم تُدرج كملفات منفصلة محسومة الحذف. أما SmsService وOtpService فهما مسجلان حاليًا، لكن مستهلك واجهتيهما الإنتاجي الظاهر هو CitizenAuthService المعطل؛ يصنفان كخدمات مسجلة بلا مسار استهلاك فعلي ظاهر، وتحتاجان مراجعة على مستوى الوحدة.

### نتائج ظاهرية استُبعدت

- Controllers: يكتشفها AddControllers تلقائيًا.
- Validators: تسجل عبر AddValidatorsFromAssemblyContaining.
- EF configurations: تُحمّل عبر ApplyConfigurationsFromAssembly في [PensionMS.Infrastructure/Data/PensionDbContext.cs:209](../../PensionMS.Infrastructure/Data/PensionDbContext.cs#L209).
- ControllerExtensions وملفات *ServiceCollectionExtensions: تُستخدم دوال الامتداد دون ذكر اسم الصنف.
- PensionDbContextFactory: مدخل أدوات EF في وقت التصميم.
- RemoteFileModelBinder: يستدعيه RemoteFileModelBinderProvider في نفس الملف، والمزود مسجل في [PensionMS.API/Program.cs:426](../../PensionMS.API/Program.cs#L426)؛ ليس ملفًا مهجورًا.
- perspective-warp.worker.ts: مستدعى عبر new URL داخل [ui/src/lib/crop-worker-client.ts:28](../../ui/src/lib/crop-worker-client.ts#L28)؛ لا يصنف غير مستخدم لغياب import تقليدي.

## التكرار في الكود المستخدم

| الملفان | تشابه الكتل | الملاحظة |
| --- | --- | --- |
| [ui/src/pages/correspondence/IncomingRegisterPage.tsx](../../ui/src/pages/correspondence/IncomingRegisterPage.tsx)<br>[ui/src/pages/correspondence/OutgoingRegisterPage.tsx](../../ui/src/pages/correspondence/OutgoingRegisterPage.tsx) | 84.85% | كلاهما مربوط بمسار في App.tsx. البحث والترقيم والتركيز والحذف ونموذج المراسلة متكررة؛ يمكن استخراج صفحة مشتركة بمعامل direction والعنوان والأيقونة. |
| [ui/src/components/domain/retirement/DocumentProcessDialog.tsx](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx) | 67.61% | كلاهما مستخدم. تتكرر إدارة Canvas والتدوير والتحسين والتصدير؛ يمكن استخراج دوال ومتحكم معالجة مشترك مع إبقاء غلاف الصفحة/النافذة. |

مجموعات دوال متطابقة بعد إزالة التعليقات وتوحيد طباعة AST، وليست مجرد تشابه أسماء:

| الدالة | المواضع |
| --- | --- |
| `rotateImage` | [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:101](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx#L101)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx:103](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx#L103) |
| `rotateCorners` | [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:119](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx#L119)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx:121](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx#L121) |
| `canvasToFile` | [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:133](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx#L133)<br>[ui/src/components/ui/ImageCropperDialog.tsx:23](../../ui/src/components/ui/ImageCropperDialog.tsx#L23)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx:135](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx#L135) |
| `applySharpen` | [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:153](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx#L153)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx:151](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx#L151) |
| `renderSettingsImage` | [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:188](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx#L188)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx:186](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx#L186) |
| `renderExportCanvas` | [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:215](../../ui/src/components/domain/retirement/DocumentProcessDialog.tsx#L215)<br>[ui/src/pages/retirement-files/DocumentProcessPage.tsx:213](../../ui/src/pages/retirement-files/DocumentProcessPage.tsx#L213) |

هذه فرص دمج للمنطق المشترك؛ حذف إحدى الواجهتين سيزيل وظيفة مستخدمة.

## الملفات المتطابقة بايتًا

| المجموعة | المسارات | حجم النسخة بالبايت | التصنيف |
| --- | --- | --- | --- |
| 1 | [PensionMS.API/wwwroot/notification-sound.mp3](../../PensionMS.API/wwwroot/notification-sound.mp3)<br>[ui/public/notification-sound.mp3](../../ui/public/notification-sound.mp3) | 76,904 | أصل مشترك بين الواجهة والخادم؛ استخدامه مؤكد في NotificationProvider.tsx |
| 2 | [PensionMS.API/wwwroot/uploads/documents/0cb20eaa-bf09-4c0b-a6dd-df6af398a96c_t_doc_probe.png](../../PensionMS.API/wwwroot/uploads/documents/0cb20eaa-bf09-4c0b-a6dd-df6af398a96c_t_doc_probe.png)<br>[PensionMS.API/wwwroot/uploads/documents/6ebbf9fb-c00e-4e8f-af68-371f4fba0af8_t_doc_probe.png](../../PensionMS.API/wwwroot/uploads/documents/6ebbf9fb-c00e-4e8f-af68-371f4fba0af8_t_doc_probe.png)<br>[PensionMS.API/wwwroot/uploads/documents/b351b7d0-0f40-45bd-b6b3-05c8147e5a8a_t_doc_probe.png](../../PensionMS.API/wwwroot/uploads/documents/b351b7d0-0f40-45bd-b6b3-05c8147e5a8a_t_doc_probe.png)<br>[PensionMS.API/wwwroot/uploads/documents/b85aa5a0-a405-4f42-bef4-8d818cf56d10_t_doc_probe.png](../../PensionMS.API/wwwroot/uploads/documents/b85aa5a0-a405-4f42-bef4-8d818cf56d10_t_doc_probe.png) | 8 | بيانات رفع؛ ارتباطاتها قد تكون في قاعدة البيانات |
| 3 | [PensionMS.API/wwwroot/uploads/documents/a41a8f15-fe88-4844-a441-d8df6f407edf_BalanceSheet-2026-09-18.pdf](../../PensionMS.API/wwwroot/uploads/documents/a41a8f15-fe88-4844-a441-d8df6f407edf_BalanceSheet-2026-09-18.pdf)<br>[PensionMS.API/wwwroot/uploads/documents/ffb85fbd-c590-4483-b5ef-cdaefea4cc3a_BalanceSheet-2026-09-18.pdf](../../PensionMS.API/wwwroot/uploads/documents/ffb85fbd-c590-4483-b5ef-cdaefea4cc3a_BalanceSheet-2026-09-18.pdf) | 340,954 | بيانات رفع؛ ارتباطاتها قد تكون في قاعدة البيانات |
| 4 | [PensionMS.API/wwwroot/uploads/photos/488397ba280240fc9951418ce2addee2.jpg](../../PensionMS.API/wwwroot/uploads/photos/488397ba280240fc9951418ce2addee2.jpg)<br>[PensionMS.API/wwwroot/uploads/photos/90d95eed9f8f44df97897bb4a328d91a.jpg](../../PensionMS.API/wwwroot/uploads/photos/90d95eed9f8f44df97897bb4a328d91a.jpg) | 1,966,535 | بيانات رفع؛ ارتباطاتها قد تكون في قاعدة البيانات |
| 5 | [ui/src/pages/retirees/image/RetireeStatementPrint/1777575773099.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575773099.png)<br>[ui/src/pages/retirees/image/RetireeStatementPrint/1777575776437.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575776437.png)<br>[ui/src/pages/retirees/image/RetireeStatementPrint/1777575781404.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575781404.png)<br>[ui/src/pages/retirees/image/RetireeStatementPrint/1777575784350.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575784350.png)<br>[ui/src/pages/retirees/image/RetireeStatementPrint/1777575788871.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575788871.png)<br>[ui/src/pages/retirees/image/RetireeStatementPrint/1777575791419.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575791419.png)<br>[ui/src/pages/retirees/image/RetireeStatementPrint/1777575807944.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575807944.png) | 202,950 | صور داخل مصدر الواجهة بلا مراجع |

بصمات SHA-256 الكاملة في [plan/file-audit-2026-10-18/duplicates.json](../../plan/file-audit-2026-10-18/duplicates.json). الزيادة النظرية فوق نسخة واحدة لكل مجموعة تساوي 3.44 MiB، وليست مساحة آمنة للحذف آليًا: ملفات الرفع قد ترتبط بسجلات مختلفة. ملفات t_doc_probe الأربعة حجم كل منها 8 بايت فقط؛ تبدو آثار اختبارات وليست صورًا كاملة، لكن ربطها بسجلات قاعدة البيانات لم يُفحص.

الصور السبع في ui/src/pages/retirees/image/RetireeStatementPrint متطابقة، ولم يُعثر على مرجع لأي منها. مجموع حجمها 1.35 MiB؛ وهي أوضح مجموعة تكرار غير مستخدم في مصدر الواجهة. أما notification-sound.mp3 فمستخدم فعلًا، ونسخه بين UI/API تتطلب مراعاة الاستضافة قبل التوحيد.

## أصول وملفات مساعدة مرشحة للمراجعة

| الأصل | الحجم بالبايت | الثقة |
| --- | --- | --- |
| [ui/public/1.pdf](../../ui/public/1.pdf) | 403,000 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/public/Director's Signature.webp](../../ui/public/Director's Signature.webp) | 5,444 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/public/Stamp.webp](../../ui/public/Stamp.webp) | 31,952 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/public/basmalah.webp](../../ui/public/basmalah.webp) | 10,402 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/public/ministry_header.webp](../../ui/public/ministry_header.webp) | 3,230 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/public/republic_header.webp](../../ui/public/republic_header.webp) | 4,270 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/public/yemen-republic-logo.webp](../../ui/public/yemen-republic-logo.webp) | 18,886 | متوسطة: لا مرجع نصي داخلي؛ قد يُطلب مباشرة من URL. |
| [ui/src/assets/react.svg](../../ui/src/assets/react.svg) | 4,126 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575773099.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575773099.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575776437.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575776437.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575781404.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575781404.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575784350.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575784350.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575788871.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575788871.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575791419.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575791419.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |
| [ui/src/pages/retirees/image/RetireeStatementPrint/1777575807944.png](../../ui/src/pages/retirees/image/RetireeStatementPrint/1777575807944.png) | 202,950 | عالية: لا استيراد ولا مرجع نصي. |

نسخ PNG لترويسات الطباعة والختم والتوقيع مستخدمة فعليًا، وبعضها يدخل إخراج PDF/Excel. وpension-logo.webp مستخدم في صفحة الدخول والتنقل وindex.html؛ لا يندرج ضمن الصور غير المستخدمة. ملفات الخطوط المحلية لها مراجع ولم تُصنّف مهجورة.

| مجلد الأدلة/المخرجات | عدد الملفات المحصاة | الحجم المحصى |
| --- | --- | --- |
| `ui/.debug-capture/` | 892 | 59.23 MiB |
| `PensionMS.API/publish/` | 125 | 77.88 MiB |

هذه الأعداد للمسارات التي أعادها Git؛ قد تحتوي المجلدات مخرجات إضافية متجاهلة. استُبعدت من أرقام التكرار الأساسية لأنها آثار تشغيل/نشر. ملفات [patch.py](../../patch.py) و[patch_perf.py](../../patch_perf.py) سكربتات تعديل يدوية لـFileUploader بمسار محلي ثابت، ولا مرجع لهما ضمن أوامر التشغيل أو النشر؛ هما أدوات تاريخية مرشحة للأرشفة، لا ملفات تشغيل للتطبيق.

## الحزم التي لا يظهر لها استيراد مباشر

`@radix-ui/react-label`، `@radix-ui/react-slot`، `@radix-ui/react-toast`، `apexcharts`، `cmdk`، `date-fns`، `framer-motion`، `immer`، `qrcode`، `react-apexcharts`، `react-easy-crop`، `react-image-crop`، `react-perspective-crop`.

هذه قائمة مراجعة تبعيات فقط. غياب الاستيراد المباشر لا يستبعد الاستخدام غير المباشر أو peerDependencies: مثل immer، وqrcode مقابل qrcode.react؛ يلزم npm explain قبل الحذف في تغيير لاحق. كما توجد قواعد CSS قديمة لـreact-image-crop في index.css، لكن لا استيراد للمكتبة الحالية.

## ملاحظة إضافية

[ui/src/components/ui/stories/DemoDialog.stories.tsx:7](../../ui/src/components/ui/stories/DemoDialog.stories.tsx#L7) يحتوي ثلاثة استيرادات محلية لا تُحل: ../../BaseFormDialog و../../Button و../../Input. الملف قصة Storybook، مستثنى من بناء التطبيق، وpackage.json لا يعرّف أمرًا لتشغيل Storybook؛ لا يُحسب ضمن الملفات الوظيفية الـ33. يصلح للمراجعة كتوثيق/تجربة قديمة بدل اعتباره اعتمادًا مفقودًا من التطبيق الحي.

## التحقق وخطوة التنظيف المقترحة

شُغّل من PensionMS.Tests:

~~~powershell
dotnet test --no-restore --filter FullyQualifiedName~DependencyInjectionTests --nologo
~~~

النتيجة: **6 ناجحة، 0 فاشلة، 0 متخطاة**. ظهرت تحذيرات بناء عن nullable ودعم System.Drawing للمنصات. سجل التشغيل في [plan/file-audit-2026-10-18/backend-di-tests.txt](../../plan/file-audit-2026-10-18/backend-di-tests.txt). هذا يثبت سلوك تسجيل DI الحالي ولا يثبت قابلية حذف الملفات أو سلامة جميع مسارات التطبيق. لم تُشغل حزمة فحوص UI كاملة أو اختبارات Backend كاملة، إذ لم يتغير كود التطبيق.

الترتيب المقترح للعمل اللاحق:

1. مراجعة وإزالة/أرشفة ملفات الواجهة الـ33 والأصول غير المرتبطة بعد مقارنة الوظائف البديلة؛ فصل التغييرات إلى دفعات صغيرة.
2. حسم مصير وحدة المواطن والمهمة الخلفية القديمة، ثم إزالة التنفيذ والعقود والاختبارات التابعة بصورة متناسقة أو تسجيل الوظيفة المطلوبة عمدًا.
3. توحيد الثوابت، وحسم تفعيل/إزالة EncryptedValueConverter وJournalEntryTypeSeeder وDataMasking وفق المتطلبات.
4. استخراج المنطق المشترك للوارد/الصادر ولمعالجة الصور، مع اختبارات تحقق سلوك مناسبة.
5. مراجعة تبعيات npm وآثار النشر والتشخيص؛ عدم حذف uploads بناءً على فحص المصدر.
6. بعد أي تعديل UI: التقاط lint قبل/بعد ومقارنته، ثم build ثم test ثم preview وفق تعليمات المشروع. تحديث اختبارات Backend عند تعديل الأنواع التي تشير إليها.

## أدلة قابلة لإعادة التشغيل

- [plan/file-audit-2026-10-18/analyze.mjs](../../plan/file-audit-2026-10-18/analyze.mjs): تشغيل `node plan/file-audit-2026-10-18/analyze.mjs` من جذر المستودع؛ لا يعدّل كود التطبيق.
- [plan/file-audit-2026-10-18/report.mjs](../../plan/file-audit-2026-10-18/report.mjs): يولّد هذا التقرير من الأدلة بعد إعادة التحليل؛ وصف المراجعة اليدوية مرتبط بحالة الفحص الحالية ويجب تحديثه عند تغير الكود.
- [plan/file-audit-2026-10-18/inventory.json](../../plan/file-audit-2026-10-18/inventory.json): نطاق الجرد وأعداد الملفات والمخرجات المستبعدة.
- [plan/file-audit-2026-10-18/frontend.json](../../plan/file-audit-2026-10-18/frontend.json): حواف الاستيراد، المستوردون لكل مرشح، ومراجع الأصول.
- [plan/file-audit-2026-10-18/backend.json](../../plan/file-audit-2026-10-18/backend.json): مراجع الرموز والخدمات؛ المرشحون الخام ليسوا أحكامًا بالحذف.
- [plan/file-audit-2026-10-18/duplicates.json](../../plan/file-audit-2026-10-18/duplicates.json): بصمات الملفات ودرجات التشابه ومواضع الدوال المتكررة.
