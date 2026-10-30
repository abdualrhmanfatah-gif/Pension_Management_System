# نتائج تنفيذ إصلاحات الوصول إلى قاعدة البيانات

تاريخ التحديث: 2026-10-11 بتوقيت اليمن. المرجع: [الخطة ومعرّفات R01–R22](../../plan/database-access-remediation-plan.md).

نُفذت الإصلاحات الأساسية في الخدمات والعقود والمستودعات والتقارير والمصادقة. احتُفظ بالـGeneric Repository وUnitOfWork والمستودع المتخصص للتسويات. لم يُنشأ Repository جديد، ولم يُطبق تغيير مخطط على قاعدة المشروع، ولم يحدث نشر.

الإصلاحات تعالج عيوبًا محددة في حدود المعاملات وصحة النتائج ونطاق الفرع. واجهة UoW ذات 119 مستودعًا بقيت كما هي؛ لا يوجد دليل يبرر إعادة كتابة النظام أو تحويل كل خدمة إلى مستودع متخصص.

## التحقق وحدوده

| التحقق | النتيجة | ما يثبته |
|---|---|---|
| PensionMS.Tests | 179 ناجحًا؛ صفر فشل | اختبارات الخدمات والعقود وDI القائمة؛ مستقلة عن قاعدة SQL فعلية |
| PensionMS.IntegrationTests | 19 ناجحًا؛ صفر فشل | ترجمة EF، المعاملات والتزامن وHTTP وJWT والفهارس على بيانات مصطنعة |
| بناء API | ناجح | توافق العقود وتسجيلات الخدمات مع المشروع الفعلي |
| بناء التكامل الكامل | ناجح مع تحذيرات المشروع | توجد تحذيرات nullable وواجهات Windows في Infrastructure؛ لا يُوصف البناء الكامل بأنه خالٍ من التحذيرات |
| قائمة الجرد | قراءة SQL واحدة | إزالة استعلام البنود لكل جرد في المسار المختبر |
| Outbox لعدد 50 إشعارًا | ثلاث قراءات SQL | الدفعة والإشعارات وعدد غير المقروء؛ لا قراءة منفصلة لكل إشعار |

خط الأساس الأول كان 165 اختبارًا ناجحًا. العدد الحالي يتضمن اختبارات نسخة العمل السابقة أيضًا؛ الفرق العددي لا يُنسب كله لهذه الإصلاحات.

كل تجهيز اختبار ينشئ قاعدة ذات اسم `PensionMS_Test_<GUID>` ويتحقق من الاسم قبل التنظيف. لا يُقرأ اتصال التطبيق من appsettings. المخطط مُنشأ من نموذج EF الفعلي مع العلاقات والفهارس؛ استُثنيت تهيئة RolePermissions لأنها تشير إلى Permissions غير موجودة في HasData الحالي. الاختبارات تزرع صلاحياتها بنفسها؛ نجاحها لا يثبت صحة بيانات الصلاحيات الأولية للنشر.

اختبار HTTP يشغّل Kestrel على loopback ومنفذ عشوائي، باستخدام Controllers وmiddleware وJWT وPermissionHandler الفعلية وتسجيلات اختبار صريحة. لا يشغّل Program.cs أو Jobs أو قاعدة التطبيق. أُثبت رفض فرع مخالف في مساري PDF وExcel، وقورن مصدر بيانات PDF بصف داخل Excel فعلي لفرع اختاره HQ. لا يُعد ذلك فحصًا بصريًا أو استخراجًا لمحتوى ملف PDF النهائي.

## تتبع الإصلاحات

المواقع التالية مراجع وقت هذا التحديث؛ اسم الدالة ومعرّف R يبقيان مرجع التتبع إذا تحركت الأسطر.

| المهمة | الموقع الأساسي | التغيير والتحقق | الحالة |
|---|---|---|---|
| R01 | [ReportingQueries.ResolveBranch/Query](../../PensionMS.Infrastructure/Services/Reporting/ReportingQueries.cs:13)، [DashboardService.GetStatsAsync](../../PensionMS.Infrastructure/Services/Reporting/DashboardService.cs:28) | نطاق الفرع قبل العد والتجميع والصفحات؛ HQ يختار فرعًا؛ غياب النطاق/طلب فرع آخر يعطي 403؛ cache مفصول. نُقلت استعلامات Dashboard وStocktake من Controllers إلى الخدمات | منفذ؛ سياسة الجداول المركزية مفتوحة |
| R02 | [SalaryPaymentBatchService.GenerateItemsAsync](../../PensionMS.Infrastructure/Services/Payroll/SalaryPaymentBatchService.cs:236) | معاملة وقفل UPDLOCK/HOLDLOCK ثم إعادة تحميل الكشف؛ استبدال البنود مرة واحدة. اختبار توليد مرتين وطلبين متزامنين وفشل بعد الحفظ ومعاملة خارجية | منفذ |
| R03 | [PayrollGenerationService.GenerateForBatchAsync](../../PensionMS.Infrastructure/Services/Payroll/PayrollGenerationService.cs:24)، [NotificationService](../../PensionMS.Infrastructure/Services/Notifications/NotificationService.cs:25) | المولد يعيد كيانات دون إضافة/حفظ/إرسال؛ enqueue يضيف الإشعار وOutbox دون حفظ. Save واحد لطلب الإشعار المستقل، وكل تسليم SignalR عبر Outbox | منفذ للمسارات المحولة؛ تبنّي أوسع مفتوح |
| R04 | [UnitOfWorkTransaction](../../PensionMS.Infrastructure/Data/UnitOfWorkTransaction.cs:9)، [VoucherService.Post/Cancel](../../PensionMS.Infrastructure/Services/Accounting/VoucherService.cs:223)، [SettlementService](../../PensionMS.Infrastructure/Services/Settlements/SettlementService.cs:557) | مالك المعاملة وحده يلتزم/يتراجع؛ معاملات للسند والجرد والأصل والعقد وقرار التسوية وحذفه واعتماد الكشف. فشل قيد الأصل يُرجع العملية؛ اختبارات إعادة القراءة تثبت التراجع | منفذ للمسارات المحددة |
| R05 | [ImmutableAuditService.LogAsync/VerifyChainIntegrityAsync](../../PensionMS.Infrastructure/Services/Audit/ImmutableAuditService.cs:23) | sp_getapplock لمدة المعاملة؛ ترتيب Id ومراجعة على دفعات keyset وحد علوي ثابت؛ استعادة UTC في صيغة التجزئة بعد datetime2. اختبار 12 كاتبًا وRollback ومهلة 15 ثانية وتعديل محتوى | منفذ للكتابة الجديدة |
| R06 | [Repository.Delete/DeleteRange](../../PensionMS.Infrastructure/Repositories/Repository.cs:57)، [UserService.DeleteUserAsync](../../PensionMS.Infrastructure/Services/Security/UserService.cs:249) | الحذف المنطقي في Documents/Inventory/Assets؛ المستودع يرفض hard DELETE على ISoftDeletable قبل تغيير التتبع. حذف المستخدم يحافظ على الأدوار والتاريخ ويعطل الجلسات والأجهزة | منفذ للمسارات المفحوصة |
| R07 | [إعداد فهارس AuditLogs](../../PensionMS.Infrastructure/Data/Configurations/System/SystemCoreConfiguration.cs:15)، [GetRecentActivitiesAsync](../../PensionMS.Infrastructure/Services/Reporting/DashboardService.cs:358) | فهرس سجل محدد وفهرس آخر أنشطة، مع ترتيب LogID حاسم؛ سكربتات تطبيق وتراجع قابلة للتكرار ومختبرة | معدّ ومختبر؛ لم يُطبق على قاعدة المشروع |
| R08 | [FixedAssetService.RunYearlyDepreciationAsync](../../PensionMS.Infrastructure/Services/Assets/FixedAssetService.cs:456) | لا إهلاك قبل سنة/شهر بدء الخدمة؛ بقاء احتساب الشهر الجزئي وحد القيمة الدفترية. اختبار أصل بدأ في يوليو ينتج ستة أشهر فقط | منفذ |
| R09 | [PayrollGenerationService](../../PensionMS.Infrastructure/Services/Payroll/PayrollGenerationService.cs:24) | اختيار أحدث استحقاق صالح بتاريخ السريان والنهاية، مع شمول يوم النهاية وترتيب EntitlementID حاسم. اختبار استحقاق أحدث منتهٍ مقابل استحقاق صالح | منفذ |
| R10 | [EntitlementEngine.ReEvaluateEntitlementStatusAsync](../../PensionMS.Infrastructure/Services/Pension/Entitlements/EntitlementEngine.cs:66) | تعطيل الوكلاء والمستلمين قبل الحفظ؛ يشمل مستحقًا غير فعال أصلًا عند بقاء المتقاعد حيًا. اختبار سياق جديد يثبت حفظ التعطيل؛ قواعد الإيقاف اليدوي محفوظة | منفذ |
| R11 | [PagedResultGuards.RequireComplete](../../PensionMS.Core/DTOs/Common/PagedResult.cs:24)، [ReportDataService.GetPagedDataAsync](../../PensionMS.Infrastructure/Services/Reporting/ReportDataService.cs:150) | لا ملف ناجح ناقص: تجاوز حد المصدر يعطي خطأ عربيًا بعدد النتائج والحد؛ تطبيقه على التصديرات المقيدة. تثبيت ترتيب صفحات الكيانات واختبار تاريخ متساوٍ؛ حد 10,001 مختبر بعقد التصدير. الحارس امتداد Core على `ApiResponse<PagedResult<T>>` بعد أن كان `ReportExportGuard` داخليًا في Infrastructure | منفذ لمنع الاقتطاع؛ حمل التصدير الكبير مفتوح |
| R12 | [HrAttendanceService.RecordAttendanceAsync](../../PensionMS.Infrastructure/Services/HR/HrAttendanceService.cs:75) | حفظ Identity الحضور قبل نسخه إلى SourceEntityId ضمن المعاملة نفسها؛ فشل حفظ التاريخ يتراجع عن السجلين | منفذ |
| R13 | [FixedAssetService.RunYearlyDepreciationAsync](../../PensionMS.Infrastructure/Services/Assets/FixedAssetService.cs:456) | قراءة أزواج أصل/شهر للسنة مرة واحدة واستعمال HashSet؛ إعادة التنفيذ لا تكرر الإهلاك | منفذ |
| R14 | [SimpleStocktakeService.GetAllStocktakesAsync](../../PensionMS.Infrastructure/Services/Inventory/SimpleStocktakeService.cs:318) | تحميل Lines والبيانات اللازمة للقائمة مرة واحدة؛ لا fallback داخل الحلقة بعد تحميلها. SQL interceptor أثبت قراءة واحدة | منفذ |
| R15 | [NotificationOutboxProcessor.ProcessPendingItemsAsync](../../PensionMS.Infrastructure/Jobs/NotificationOutboxProcessor.cs:46) | قراءة جماعية للإشعارات والمرفقات وتجميع غير المقروء؛ interceptor أثبت ثلاث قراءات لعدد 50 | منفذ |
| R16 | [DiscrepancyService.GetKashfReportSummaryAsync](../../PensionMS.Infrastructure/Services/Reporting/DiscrepancyService.cs:108)، ReportService.Admin.GetKashfReportSummaryAsync | Count/GroupBy في SQL بدل تحميل الصفوف؛ اختبار ترجمة التجميع ونتيجة المرشح الفارغ وعزل الفرع | منفذ |
| R17 | [ScopedPermissionResolver.HasPermissionInBranchAsync](../../PensionMS.Infrastructure/Services/Security/ScopedPermissionResolver.cs:55) | إسقاط نطاقات الأدوار التي تحمل الصلاحية المطلوبة بدل Include لجميع الصلاحيات. اختبار السحب الفردي والنطاق المباشر والإقليمي وعدم fallback عند وجود تعيين نشط | منفذ؛ تحسن الزمن لم يُقَس |
| R18 | [IRepository.Update/Query](../../PensionMS.Core/Interfaces/IRepository.cs:21) ومسارات Voucher/Journal/Payroll/Assets/Settlement | إزالة Update الزائد على كيانات متتبعة في المسارات المستهدفة، لتجنب تعليم الرسم المرتبط كله كمعدل. تعديل الكيان ثم Save يكفي | منفذ تدريجيًا؛ ليس تدقيقًا شاملًا لكل Update |
| R19 | [SettlementRepository.GetByRetireeIdAsync](../../PensionMS.Infrastructure/Repositories/SettlementRepository.cs:285) | مطابقة RetireeID/ApprovalItemID والرقم العسكري للعنصر أو عنصر الإحالة في SQL، ثم إسقاط DTO. تطبيع عربي/فارسي وفراغات مع بقاء الأصفار؛ لا تحميل جدول المرشحين للذاكرة | منفذ؛ كلفة REPLACE ما زالت تحتاج قياسًا |
| R20 | [SettlementRepository.GetItemsByDecisionIdAsync](../../PensionMS.Infrastructure/Repositories/SettlementRepository.cs:316) | تحميل CurrentRank/ProcessedRank/ApprovalItem المطلوب للتصدير؛ اختبار SQL يؤكد وجود الرتب | منفذ |
| R21 | [SettlementRepository.ProjectDecisions/GetBatchDecisions/GetDecisionsPaged](../../PensionMS.Infrastructure/Repositories/SettlementRepository.cs:78) | إسقاط واحد؛ أول عنصر بترتيب ID معلن، والكشف المطلوب في تقرير الكشف؛ ترتيب صفحات حاسم وحدود صالحة؛ Dictionary لتحديث الرتب. اختبارات قرار متعدد الكشوف وصفحات وبحث | منفذ |
| R22 | [IRepository](../../PensionMS.Core/Interfaces/IRepository.cs:7)، [Repository.GetCursorPagedAsync](../../PensionMS.Infrastructure/Repositories/Repository.cs:80) | حذف overloads عامة غير مستخدمة للصفحات دون ترتيب؛ توثيق مفتاح cursor الصحيح والفريد والترتيب التنازلي والتحقق من الحجم | منفذ |

أُضيفت إصلاحات مؤكدة أثناء اختبارات R04:

- **عجز الجرد:** `SimpleStocktakeLine.VarianceValue` يعيد قيمة مطلقة، لكن Adjust كان يفترض أن مجموع العجز سالب. أصبح الجمع سالبًا للعجز، واختبار انخفاض 10 إلى 7 وحدات بتكلفة 20 يثبت قيدًا متوازنًا بقيمة 60 وتراجعًا كاملًا إذا فشل التدقيق.
- **الحركة البنكية:** أُزيل التسجيل المكرر من VoucherService لأن JournalEntryService يسجل الحركة بسطر القيد الفعلي. أُضيفت الحركة المعاكسة عند ReverseJournalEntryAsync؛ اختبار السند يثبت حركة واحدة عند الترحيل وحركة عكسية عند الإلغاء، مع LineID صحيح.
- **المستخدم المحذوف:** الحفاظ على روابط الأدوار يتطلب رفض access token قديم؛ أُضيف OnTokenValidated للتحقق من نشاط المستخدم من الخدمة. اختبار HTTP يثبت 401 للرمز نفسه بعد الحذف، ولا يعتمد على انتظار انتهاء JWT.
- **ResetCategories:** يُرفض مع وجود أصول، بما فيها المحذوفة منطقيًا، بدل مسح تفاصيلها أو تاريخها لإعادة التصنيف.

## قياس الفهارس

هذه مقارنة على 100,000 سجل مصطنع في قاعدة اختبار، وليست قياسًا جديدًا على قاعدة التطوير أو الإنتاج:

| شكل الاستعلام | قبل | بعد | التحقق |
|---|---:|---:|---|
| آخر عشرة أنشطة: ORDER BY CreatedAt DESC, LogID DESC | 25,119 قراءة منطقية | 38 | معرفات النتائج متطابقة |
| WHERE TableID/RecordID مع ترتيب CreatedAt | 25,119 قراءة منطقية | 318 | معرفات النتائج متطابقة |

الفهارس المقترحة ثلاثة فقط: `IX_AuditLogs_Record_CreatedAt`، `IX_AuditLogs_Recent` مع INCLUDE المحدد، و`IX_EntitledPersons_FileID`. الأولان مدعومان أيضًا بأدلة Query Store التاريخية في الخطة. الثالث يخدم FileID لجميع المستحقين؛ الفهرس المركب المفلتر الموجود لا يغطي جميع حالات الرقم الوطني. لم يُدَّع انخفاض زمنه بقياس لم يحدث.

السكربتات: [precheck](../../scripts/db-access-indexes-precheck.sql)، [apply](../../scripts/db-access-indexes-apply.sql)، [rollback](../../scripts/db-access-indexes-rollback.sql). يفحص apply الفهارس المكافئة قبل الإنشاء، ويعلّم ما أنشأه فقط؛ rollback يزيل الفهارس المعلّمة فقط. اختُبر التطبيق والتراجع مرتين، مع الحفاظ على النتائج.

إنشاء الفهرس يزيد كلفة كتابة AuditLogs ومساحتها وقد يحجز الجدول أثناء الإنشاء. لا يستخدم السكربت ONLINE مفترضًا. يلزم قياس مساحة ووقت تنفيذ في بيئة مماثلة قبل التطبيق على قاعدة المشروع، ثم مقارنة Query Store للشكل نفسه وحجم النتائج نفسه. لا تتساوى أرقام Query Store التاريخية مع هذا القياس المصطنع.

## المعاملات والتدقيق

SaveChanges يحفظ داخل المعاملة، وCommit لا يستدعي الحفظ تلقائيًا. الخدمة الداخلية لا تلتزم بمعاملة المستدعي؛ يجب على المستدعي نشر نتيجة الفشل والتراجع عنها. Rollback لا يعيد ChangeTracker لحالته السابقة؛ الاختبارات تتخلص من السياق الفاشل وتعيد القراءة بسياق جديد. لا يوجد Clear عام قد يفسد مراجع المستدعي.

Outbox يعيد المحاولة وقد يعيد تسليم إشعار إذا توقف العامل بعد الإرسال وقبل تحديث حالته؛ لا يضمن exactly-once. NotificationID هو معرف الإشعار المستخدم للتعامل مع التكرار. التسليم الدوري قد يتأخر إلى دورة المعالج التالية، نحو 30 ثانية.

قفل التدقيق يحمي الكتابة الجديدة التي تمر عبر الخدمة المعدلة. تبقى الملاحظة التاريخية: 45 رابطًا مخالفًا و42 مجموعة PreviousHash مكررة عند فحص قاعدة التطوير. لا يحدد هذا سبب الخلل ولا يثبت تلاعبًا؛ لم تُعد كتابة Hash أو PreviousHash، ولم تُعلن سلامة السلسلة التاريخية. التحقق الكامل وحفظ دليل النتائج ومعالجة التاريخ عمل مستقل.

## العمل المتبقي مرتّبًا

| الأولوية | العمل | سبب إبقائه مفتوحًا |
|---|---|---|
| مرتفعة | تثبيت سياسة قراءة الجداول المركزية، خاصة OperationalExpenses/التقارير المحاسبية | لا تحمل ملكية فرع؛ لا يجوز استنتاج الفرع من منشئ السجل أو اختراع توزيع مالي. تُعامل الآن كمصادر مشتركة بعد التحقق من صلاحية التقرير ونطاق الطلب؛ لم تُعلن حماية جميع المجالات بفلتر واحد |
| مرتفعة | فحص سلامة التدقيق التاريخي وتوثيق معالجة معتمدة | القفل يصلح السباق الجديد ولا يصلح الروابط القديمة؛ تعديلها تلقائيًا يطمس الدليل |
| متوسطة | توسيع enqueue إلى بقية إشعارات العمليات التي تُرسل بعد Commit بصورة مستقلة | الرواتب واعتماد التسوية وإنشاء الاستحقاق وحذف المستخدم تحولت؛ ما زالت خدمات أخرى تحتفظ بإشعارات best-effort مستقلة. جميعها تستخدم Outbox، لكن ربط كل إشعار بمعاملة العمل لم يكتمل |
| متوسطة | قياس ملفات التصدير الكبيرة والذاكرة وتطابق ملفات PDF/Excel النهائية | منع الاقتطاع مُثبت؛ لا streaming جديد، وبعض المصادر غير المقيدة ما زالت تحمل البيانات كاملة. اختبار حد 10,001 تم على العقد وليس اختبار حمل ملف كامل |
| متوسطة | تطبيق الفهارس في بيئة مماثلة ثم قياس Query Store | السكربتات جاهزة ومختبرة؛ لا تعديل على قاعدة المشروع حتى الآن، ولا ادعاء تحسن الإنتاج |
| متوسطة | قياس خطة مطابقة الرقم العسكري الجديدة | SQL REPLACE يمنع تحميل المرشحين للذاكرة لكنه قد يبقي مسحًا وكلفة CPU؛ عمود تطبيع وفهرسه يتطلبان دليلًا قبل اقتراحهما |
| منخفضة | متابعة إزالة Update الزائد وفق المسارات الأكثر استعمالًا | جرى إصلاح المسارات المالية المستهدفة؛ لم تُغيّر دلالة Update للكيانات المنفصلة |
| منخفضة | إعادة تصميم FileQueryService أو تقسيم واجهة UoW | لا قياس يبرر توسعة تغيير المعمارية الحالية؛ تُراجع حاجة كل مسار على حدة |

المحافظ الاستثمارية المختلطة بين فروع لا تظهر للمستخدم المقيد بفرع، لأن رأس المال قيمة مشتركة لا يمكن تجزئتها من النموذج الحالي. تدقيق الكشوف المختلطة أيضًا يبقى لـHQ لأن JSON يحمل snapshot كاملًا؛ تعرض تقارير الكشوف للمستخدم إجماليات البنود المرئية فقط. الوثائق ذات ملكية لا يحددها ملف تقاعد أو استثمار معروف تُستبعد من تقرير الفرع بدل توسيع النطاق بلا دليل.

## إعادة التشغيل

```powershell
dotnet test .\PensionMS.Tests\PensionMS.Tests.csproj --artifacts-path .artifacts/db-access --nologo
dotnet build .\PensionMS.API\PensionMS.API.csproj --artifacts-path .artifacts/db-access --nologo
# اضبط PENSIONMS_TEST_SQLSERVER باتصال خادم الاختبار المخصص قبل الأمر التالي.
dotnet test .\PensionMS.IntegrationTests\PensionMS.IntegrationTests.csproj --artifacts-path .artifacts/db-access --nologo
```

سجلات التحقق: [الخدمات](../../plan/database-remediation-test-output.txt)، [SQL وHTTP](../../plan/database-remediation-sql-test-output.txt)، [بناء API](../../plan/database-remediation-api-build.txt)، [بناء التكامل](../../plan/database-remediation-integration-build.txt).

القرار المعماري هو **الإبقاء على المعمارية الحالية وتحسينها تدريجيًا**: العيوب المثبتة أمكن إصلاحها بإحكام العملية وتقييد IQueryable والإسقاط والتجميع وتحسين الفهارس. لا تدعم الأدلة إضافة مستودع لكل كيان أو إعادة كتابة شاملة؛ التعديلات المعمارية المحددة هي حدود معاملات واضحة، ومسار enqueue صريح، ومصدر نطاق موحد للتقارير.
