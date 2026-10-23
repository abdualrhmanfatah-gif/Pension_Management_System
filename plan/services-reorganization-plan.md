# خطة إعادة تنظيم خدمات PensionMS

التاريخ: 2026-10-09 بتوقيت اليمن.

الحالة: اكتمل تنفيذ M0–M6 وS1 في نسخة العمل بتاريخ 2026-10-09. تفاصيل المراحل أدناه تحفظ نطاق الخطة الأصلي، وسجل التنفيذ في نهاية الملف يوضح النتائج وحدود التحقق.

## الهدف والنطاق

تنظيم خدمات Infrastructure حسب مجال العمل، وتجميع تسجيلاتها الصريحة، ثم إكمال فصل القراءة والتقارير عن خدمة تعديل ملفات التقاعد. التنفيذ على مراحل مستقلة يمكن مراجعتها والتحقق منها.

النطاق الأساسي هو `PensionMS.Infrastructure`، مع التعديلات اللازمة في `PensionMS.API` و`PensionMS.Core` و`PensionMS.Tests` لتحديث العقود والمراجع. تبقى المشاريع الحالية وASP.NET Core 8 وIUnitOfWork وآلية التسجيل اليدوي.

نقل الملفات يحافظ على قواعد العمل والتواقيع ومسارات HTTP ورسائل الاستجابة ومفاتيح التقارير. إصلاح قواعد الحالة وفصل الخدمات الكبيرة لهما مهام محددة أدناه؛ لا يُخلطان مع دفعات النقل.

## نقطة البداية

- 97 ملف C# داخل `Services`، منها 59 في الجذر، إضافة إلى خمسة ملفات في `Reporting` وملف في `Jobs`: 103 ملفات في خريطة النقل.
- التنظيف السابق أزال وظائف الحالة المكررة من `RetirementFileService` وخفّض اعتماديات منشئها من 16 إلى 12.
- آخر تحقق بعد التنظيف: 128 اختبار Backend ناجحًا، وبناء API ناجحًا بلا أخطاء أو تحذيرات في ذلك التشغيل.
- توجد تعديلات محلية سابقة في خدمات التقاعد والتسويات وغيرها. يعتمد النقل على محتوى نسخة العمل الحالية، مع الحفاظ على تلك التعديلات.
- `FileStatusService` لا تطبق مصفوفتها حاليًا في تغيير الحالة أو قائمة الانتقالات؛ تُتابع المشكلة بالمهمة S1.

خريطة الملفات الأصلية ومساراتها قبل التنفيذ وبعده محفوظة في [services-reorganization-map.json](services-reorganization-map.json). أضيفت إليها قائمة الملفات الجديدة ونتائج التحقق، مع إبقاء أعداد خط الأساس للتتبع.

## الهيكل المستهدف

```text
PensionMS.Infrastructure/
├── Services/
│   ├── Pension/
│   │   ├── Files/
│   │   ├── Beneficiaries/
│   │   └── Entitlements/
│   ├── Payroll/
│   ├── Settlements/
│   ├── Documents/
│   ├── Accounting/
│   ├── HR/
│   ├── Investment/
│   ├── Inventory/
│   ├── Assets/
│   ├── Citizen/
│   ├── Security/
│   │   └── SoD/
│   ├── Audit/
│   ├── Notifications/
│   ├── Administration/
│   └── Reporting/
├── Reporting/
│   ├── Exporters/
│   └── ReportRegistry.cs
├── Jobs/
└── DependencyInjection/
```

تبقى مجلدات Data وRepositories وHelpers وغيرها الموجودة خارج هذا الرسم؛ الرسم يوضح نطاق إعادة التنظيم فقط.

## قرارات توزيع المسؤوليات

| الوجهة داخل Services | الملفات الحالية التابعة لها |
|---|---|
| `Pension/Files` | `RetirementFileService`, `FileStatusService`, `FileReviewService`, `FileNoteService`, `AchievementObstacleService`, `MinistryApprovalService`, `ComplianceEngine`, `WorkflowEngine` |
| `Pension/Beneficiaries` | `RetireeService`, `EntitledPersonService`, `AgentService`, `GuardianService` |
| `Pension/Entitlements` | `FinancialEntitlementService`, `EntitlementEngine`, `PensionSuspensionService` |
| `Payroll` | `PayrollGenerationService`, `SalaryPaymentBatchService`, `SalaryRecipientService`, `PaymentBookService` |
| `Settlements` | `SettlementService` |
| `Documents` | `DocumentService`, `DocumentTypeService`, `DocumentPledgeService`, `DocumentExemptionService`, `MissingDocumentService`, `LocalFileStorageService` |
| `Accounting` | الخدمات العشر الحالية، مع نقل `BankService` إليها |
| `HR`, `Inventory`, `Assets`, `Citizen` | تبقى الخدمات الحالية في مجلداتها |
| `Investment` | الخدمات السبع الحالية، مع إضافة `InvestmentDocumentValidationService` |
| `Security` | `AuthService`, `UserService`, `PermissionService`, `ScopedPermissionResolver`, `BranchScopeProvider`, `Aes256EncryptionService`, `RiskScoringService`, `ComplianceService` |
| `Security/SoD` | `SoDGuardService`, `SoDRules.cs` |
| `Audit` | `AuditLogService`, `ImmutableAuditService`, `SnapshotService`, `LoginHistoryService` |
| `Notifications` | `NotificationService`, `AdminNotificationService`, `NotificationHub` |
| `Administration` | `AdminSettingsService`, `BranchService`, `RankService`, `LookupService` |
| `Reporting` | ملفات `ReportService` الستة، و`ReportDataService`, `DashboardService`, `DiscrepancyService` |

مبررات الحالات التي قد تلتبس:

- `ComplianceEngine` يحسب اكتمال وثائق ملف التقاعد، لذلك ينتمي إلى Files. أما `ComplianceService` فتدير أحداث المخاطر والتجاوزات، ولذلك تنتمي إلى Security مع `RiskScoringService`.
- `DiscrepancyService` تقدم تقارير مقارنة بيانات الوزارة والمتقاعدين والملفات، لذلك تنتمي إلى Reporting.
- `PaymentBookService` و`SalaryRecipientService` ترتبطان بتسليم المعاش؛ تُجمعان مع Payroll.
- `DocumentService` مشتركة بين عدة مجالات، و`HrDocumentService` تستخدمها بالفعل. تبقى خدمة HR داخل HR، والخدمة المشتركة داخل Documents.
- `WorkflowEngine` يتعامل حاليًا مع ملفات التقاعد ومهام اعتماد الرواتب. يوضع في Files باعتباره موضعه الأساسي الحالي، وتوثق هذه الاعتمادية؛ تقسيمه حسب المجال مهمة لاحقة إذا احتاج ذلك تغيرًا حقيقيًا.
- `NotificationHub` يبقى في مشروع Infrastructure في هذه الخطة، مع انتقاله إلى Notifications واستمرار ربطه من API على `/hubs/notifications`.
- `INotificationClient` عقد يستخدم DTOs من Core ولا يحتاج اعتماد SignalR؛ يُنقل إلى Core وفق جدول العقود.

| الوجهة خارج Services | الملفات |
|---|---|
| `Reporting/Exporters` | `ExcelReportExporter.cs`, `PdfReportExporter.cs`, `AccountingExportService.cs` |
| `Reporting` | يبقى `ReportRegistry.cs` |
| `Jobs` | يبقى `ExpiredPledgesJob.cs`، وينقل `NotificationOutboxProcessor.cs` و`PledgeExpiryCheckerService.cs` إليها |

فصل Reporting مقصود: Services/Reporting تجمع بيانات التقرير وتنسق الطلب؛ Reporting/Exporters تولد الملفات من البيانات. يبقى Registry لتعريفات التقارير ومفاتيحها.

## العقود في Core

| العقد الحالي | المسار المستهدف | الإجراء |
|---|---|---|
| `IBankTransactionService` داخل ملف التنفيذ | `Core/Interfaces/Accounting/IBankTransactionService.cs` | استخراج العقد وتحديث Controller والتسجيل |
| `IReportDataService` داخل ملف التنفيذ | `Core/Interfaces/Reporting/IReportDataService.cs` | استخراج العقد وتحديث ReportsController |
| `IAccountingExportService` داخل ملف التنفيذ | `Core/Interfaces/Reporting/IAccountingExportService.cs` | استخراج العقد وتحديث AccountingController |
| `Infrastructure/Services/INotificationClient.cs` | `Core/Interfaces/Notifications/INotificationClient.cs` | نقل العقد وتحديث Hub ومرسلي الإشعارات |
| `IImmutableAuditService` و`ISnapshotService` داخل `IAuditAndSnapshotServices.cs` | ملف مستقل لكل عقد داخل `Core/Interfaces` | فصل الملف مع إبقاء namespace والتواقيع الحالية |

الـ namespaces للعقود الأربعة الأولى تطابق مجلداتها الجديدة. تبقى بقية عقود Core في مساراتها الحالية خلال إعادة تنظيم Infrastructure؛ توحيد جميع مجلدات Interfaces القديمة توسع مستقل.

## مراحل التنفيذ

### M0 — تثبيت خط الأساس

- تسجيل حالة نسخة العمل الحالية وقائمة الملفات وتحديث الخريطة إذا تغيرت الملفات منذ إعدادها.
- التحقق من اختبارات Backend وبناء API من مجلدي المشروعين الصحيحين.
- توثيق تسجيلات DI الحالية: العقد والتنفيذ والعمر والترتيب والتسجيلات المتعددة.
- توثيق الخدمات غير المسجلة، ومسارات HTTP وHub ومفاتيح Registry التي يجب الحفاظ عليها.

القبول: خط أساس معروف، وخريطة تغطي جميع الملفات دون وجهات متكررة، وفصل واضح بين التعديلات الموجودة وعمل هذه الخطة. آخر تحقق 128 اختبارًا مرجع سابق؛ يُعاد عند بدء التنفيذ.

### M1 — العقود وتجميع تسجيلات DI

- تنفيذ جدول عقود Core السابق.
- إنشاء `DependencyInjection/InfrastructureServiceCollectionExtensions.cs` بمدخل واحد `AddPensionInfrastructureServices()` يستدعي تسجيلات المجالات.
- إنشاء ملفات تسجيل المجالات داخل DependencyInjection: Pension وPayroll وSettlements وDocuments وAccounting وHR وInvestment وInventory وAssets وCitizen وSecurity وAudit وNotifications وAdministration وReporting، مع تسجيلات Persistence وBackgroundJobs.
- كل دالة تسجل خدمات مجالها صراحة؛ لا assembly scanning ولا تسجيل تلقائي للخدمات غير المفعلة.
- استدعاء المدخل الجديد من Program.cs وإزالة التسجيلات المنقولة منه لمنع الازدواجية.
- تبقى إعدادات DbContext والاتصال وJWT والسياسات وMiddleware وSignalR وValidators في المضيف API.
- المحافظة على `Scoped` للخدمات الحالية و`Singleton` لـ Registry وExporters، وعلى ترتيب قواعد `ISoDRule` وتسجيل المهمتين المضيفتين الموجودتين.
- يظل كل من `IFileQueryService` و`IRetirementFileService` مسجلًا على التنفيذ الحالي حتى M5؛ لا تغيير لطريقة مشاركة النسخ أثناء استخراج التسجيلات.

القبول: تحديث Controllers لاستخدام عقود Core المستخرجة؛ لا عقود أعمال داخل ملفات التنفيذ الثلاثة؛ مقارنة تسجيلات DI قبل/بعد تظهر نفس الخدمات المفعلة وأعمارها وترتيب التسجيلات المتعددة. بناء API واختبارات Backend ناجحان.

### M2 — الخدمات المشتركة والمهام

تُنفذ دفعات صغيرة بالترتيب: Security وSoD، ثم Audit، ثم Notifications وJobs، ثم Documents، ثم Administration.

- في كل دفعة: نقل الملفات وتعديل namespace وusing والأسماء المؤهلة بالكامل في التسجيلات والمستدعين والاختبارات.
- الحفاظ على ملفات SoDRules مجتمعة في هذه المرحلة؛ فصل كل قاعدة في ملف مستقل تحسين لاحق إن لزم.
- الحفاظ على Hub وعقد العميل والمجموعات والأسماء ومسار الاتصال.
- `PledgeExpiryCheckerService` تُنقل فقط وتبقى غير مسجلة؛ لا تشغيل جديد لمعالج التعهدات البديل.
- الحفاظ على إنشاء scopes داخل BackgroundServices وأزمنة التشغيل الحالية.

القبول لكل دفعة: بناء API واختبارات Backend ناجحان، وعدم بقاء استيرادات namespaces القديمة للأنواع التي انتقلت في الدفعة. لا تغيير في شروط العمل أو جداول قاعدة البيانات.

### M3 — مجالات التقاعد والرواتب والتسويات

الدفعات: Pension/Files، ثم Beneficiaries وEntitlements، ثم Payroll وSettlements، ثم استكمال Accounting وInvestment.

- نقل الملفات حسب الخريطة وتحديث المستدعين والاختبارات والتسجيلات.
- إبقاء تنفيذ RetirementFileService موحدًا مؤقتًا حتى M5؛ نقل ملفه لا يُعد فصلًا لمسؤولياته.
- إبقاء خدمات HR وInventory وAssets وCitizen في أماكنها مع تحديث الاستيرادات التي تأثرت بالدفعات السابقة.
- المحافظة على تعديلات التواريخ والتسويات واعتماديات المنشئات الموجودة في نسخة العمل.

القبول: الجذر Services خالٍ من ملفات C# التابعة للمجالات المنقولة، مع نجاح البناء والاختبارات. أسماء الكلاسات والعقود والتواقيع العامة ثابتة خلال هذه المرحلة.

### M4 — تنظيم التقارير وأدوات التصدير

- نقل ملفات `ReportService*` الستة معًا إلى Services/Reporting؛ تبقى `partial` لنفس الكلاس وبنفس namespace الجديد.
- نقل `ReportDataService`, `DashboardService`, `DiscrepancyService` إلى Services/Reporting.
- نقل أدوات التصدير الثلاث إلى Reporting/Exporters وتحديث Controllers والتسجيلات.
- الحفاظ على مفاتيح Registry وتعريفات الأعمدة والمرشحات وتنسيق العربية وRTL والخطوط وترقيم الصفحات وأنواع الاستجابة.
- توثيق بقاء إنتاج Excel/PDF داخل بعض الخدمات الكبيرة؛ استخراج تلك المسارات إلى Exporters يتم لاحقًا عند فصل مسؤولياتها، وليس بإعادة كتابة كل تصدير أثناء النقل.

القبول: إنشاء عينات تقارير ممثلة ومقارنتها بالقيم والأعمدة والتنسيق السابق عند توفر بيئة وبيانات مناسبة، إضافة إلى بناء API واختبارات Backend. اختبارات الوحدات والبناء وحدهما لا يثبتان مطابقة عرض PDF أو Excel.

### M5 — إكمال فصل RetirementFileService

هذه مرحلة فصل التنفيذ، بعد استقرار المواقع والتسجيلات.

| التنفيذ المستهدف | ما ينتقل إليه |
|---|---|
| `Pension/Files/FileQueryService.cs` | جميع عمليات `IFileQueryService` الحالية: التفاصيل والبحث والسجل وملفات إدخال البيانات والرفع وإحصاءات المستخدمين |
| `Services/Reporting/RetirementFileReportService.cs` | `GetRetirementFileReportAsync`, `GetRetirementFileReportSummaryAsync`, `ExportRetirementFileReportAsync` ودوال بناء استعلام التقرير ومرشحاته |
| `Pension/Files/RetirementFileService.cs` | الإنشاء والتحديث والحذف والإسناد والوفاة وتصحيح الحالة الحياتية |
| `Pension/Files/RetirementFileDtoMapper.cs` | تحويل البيانات إلى DTO المستخدم في القراءة والإنشاء والتحديث؛ مساعد داخلي دون واجهة عامة جديدة |
| `Pension/Files/RetirementFileCompleteness.cs` | حساب البيانات الناقصة وتجميع الوثائق الناقصة المشتركة بين القراءة والتحديث، مع فصل الحساب الخالص عن التحميل من المستودعات؛ مساعد داخلي |

- إنشاء `IRetirementFileReportService` داخل `Core/Interfaces/Reporting` ونقل دوال التقارير الثلاث إليه، ثم تحديث Controller والتسجيلات والمستدعين في نفس الدفعة.
- إزالة `IFileQueryService` من تعريف RetirementFileService وتسجيلها على FileQueryService الجديدة.
- نقل GetMissingDocsListAsync وقراءات سجلات التغيير ومساعداتها إلى موضع استخدامها؛ عدم نسخ نفس قواعد الاكتمال بين الخدمات.
- لا تعتمد خدمة القراءة على خدمة التعديل؛ تستخدم العمليات المشتركة الداخلية. مراجعة منشئي الخدمتين وإزالة الاعتماديات غير المستخدمة وتحديث الم mocks.
- إبقاء شروط SoD والتدقيق والمراجعة وإيقاف المستلمين ودفاتر الصرف ضمن العمليات التي تحتاجها.
- يبقى العقد الحالي لمعلمات GetAllAsync أثناء الاستخراج؛ استبداله بـ DTO مرشحات تحسين منفصل لتجنب تعديل المستدعين أثناء نقل التنفيذ.
- فصل بيانات تقرير الملفات عن تنسيق Excel/PDF باستخدام Exporters دون تغيير النتائج.

القبول: لكل واجهة تنفيذ ذو مسؤولية واضحة؛ لا دوال قراءة أو تصدير عامة في RetirementFileService؛ لا منطق حالة مكرر؛ اختبارات القراءة تراجع الفرع والبحث والترقيم والتواريخ، واختبارات التعديل تراجع الأثر المالي والمراجعة والتدقيق الموجودين. الحفاظ على النتائج السابقة هو معيار النجاح، وليس عدد الأسطر.

### M6 — المراجعة النهائية والتوثيق

- مراجعة الخريطة الفعلية بعد إضافة ملفات M1 وM5؛ كل ملف داخل مجال واضح والـ namespace متوافق مع موضعه.
- البحث عن namespaces ومسارات قديمة في المصدر والمشاريع والاختبارات والوثائق وتحديث المراجع اللازمة.
- تحديث أقسام الخدمات والتقارير وDI ذات الصلة في docs/Architecture.md بما يعكس التنفيذ الفعلي.
- إضافة تحقق بنيوي محدود عند التنفيذ: عقود الأعمال في Core، عدم وجود ملفات خدمات في الجذر، وتوافق namespaces مع المسارات ضمن المجلدات المنظمة.
- اختبار تسجيلات المجالات من Infrastructure ممكن دون إضافة مرجع API إلى مشروع الاختبارات. تبقى مراجعة Program.cs والسياسات والـ middleware مسؤولية تحقق مستقلة عن تلك الاختبارات.
- تشغيل اختبارات Backend وبناء API، ثم التحقق الوظيفي للتقارير وHub في بيئة مناسبة.

القبول: لا أخطاء بناء ولا اختبارات فاشلة ولا تحذيرات جديدة بسبب النقل، مع توثيق التحذيرات الموجودة مسبقًا وأي تحقق وظيفي تعذر تنفيذه.

## S1 — إصلاح قواعد الحالة، مهمة سلوكية مستقلة

هذه المهمة ليست شرطًا لنقل الملفات، لكنها مطلوبة قبل اعتبار مسار الحالة مطبقًا لقواعد دورة حياة الملف.

- تطبيق مصفوفة FileStatusService الموجودة على تغيير الحالة وعلى قائمة الانتقالات، مع منع الخروج من Closed.
- الحفاظ على شروط المراجعة ودفتر الصرف والمستلم والتعهدات وSoD؛ الاتفاق على نتائج الحالات التي تؤثر عليها تلك الشروط قبل تعديل السلوك.
- اختبارات تحقق انتقالات الحالات الثماني، بما فيها Incomplete وRejected، ورفض الخروج من Closed، وتغطية الوثائق بالتعهدات، وموانع التفعيل، وأثر الرفض دون حفظ.
- تنفيذها بدفعة مستقلة عن تغيير namespaces، ووصفها كإصلاح سلوكي في المراجعة.

## الخدمات غير المفعلة

تبقى `CitizenAuthService`, `CitizenDataService`, `CitizenServicePortal`, `CitizenSupportService` داخل Citizen وغير مسجلة. تبقى SMS وOTP مسجلتين كما هما.

يبقى `PledgeExpiryCheckerService` غير مسجل. المعالج المسجل حاليًا هو `ExpiredPledgesJob` الذي يستدعي `IFileStatusService.ProcessExpiredPledgesAsync`. لا يُحذف المعالج البديل أو يفعّل ضمن النقل؛ يحسم مصيره بمهمة مستقلة بناء على المقارنة الوظيفية.

## قواعد التحقق وإدارة الدفعات

1. كل دفعة تشمل النقل وnamespace والاستيرادات والتسجيلات واختبارات منشئات الخدمات التي تأثرت.
2. تُراجع فروق الدفعة مقابل حالة نسخة العمل عند بدايتها، للحفاظ على التعديلات السابقة. لا رجوع عام أو حذف للتعديلات المحلية عند الفشل.
3. تُشغّل الأوامر بالتتابع لتجنب بناء المشاريع المشتركة في الوقت نفسه:

```powershell
Set-Location F:\Projects\Pension_Management_System\PensionMS.Tests
dotnet test --nologo
Set-Location F:\Projects\Pension_Management_System\PensionMS.API
dotnet build --nologo
```

4. لا حاجة لتغيير EF أو SQL أو تشغيل قاعدة بيانات حية للتحقق من البناء واختبارات الوحدات الحالية.
5. النقل الميكانيكي لا يغير Core Entities أو DTOs أو UI. إذا تغير عقد API لاحقًا ضمن فصل مسؤولية، تُراجع آثاره في دفعة ذلك التغيير.
6. التواريخ تظل وفق ADR 0001، والحذف المنطقي والتدقيق ونطاق الفرع وفصل المهام تُراجع عند فصل التنفيذ.

## قائمة متابعة التنفيذ

- [x] تنظيف وظائف الحالة المكررة من RetirementFileService، قبل إعداد هذه الخطة.
- [x] إعداد خريطة الملفات والمراحل وقرارات التصنيف.
- [x] M0: تثبيت خط أساس التنفيذ.
- [x] M1: عقود Core وتسجيلات المجالات.
- [x] M2: الخدمات المشتركة والمهام.
- [x] M3: مجالات الأعمال.
- [x] M4: التقارير والتصدير.
- [x] M5: فصل القراءة وتقارير ملفات التقاعد.
- [x] M6: المراجعة النهائية والتوثيق.
- [x] S1: إصلاح تطبيق قواعد الحالة في دفعة مستقلة.

المخرج النهائي: الهيكل المطلوب، وتسجيل صريح قابل للتتبع، وخدمة تعديل ملفات تقاعد ذات مسؤولية محددة، مع الحفاظ على سلوك النظام خلال دفعات النقل وإثبات أي تغيير سلوكي باختبارات مستقلة.

## سجل التنفيذ — 2026-10-09

نُقلت الملفات الـ64 المحددة في الخريطة، وبقيت الملفات الـ39 الأخرى في مواضعها. فُحصت الوجهات الـ103 والـ namespaces، ولم تبق ملفات خدمات C# في جذر Services أو مراجع namespaces القديمة في المصدر. تمت مقارنة محتوى دفعات النقل بخط أساس نسخة العمل؛ تغييرات قواعد الحالة وفصل الخدمة موثقة بصورة مستقلة عن النقل.

| المرحلة | النتيجة |
|---|---|
| M0 | حفظ محتوى نسخة العمل وتسجيلات DI؛ 128 اختبارًا ناجحًا وبناء API ناجح |
| M1 | نقل واستخراج العقود إلى Core وفصل عقدي التدقيق واللقطات؛ 18 ملف تسجيل DI يدوي؛ 132 اختبارًا ناجحًا |
| M2–M4 | نقل المجالات والمهام والتقارير وأدوات التصدير؛ نجاح بناء API والاختبارات بعد كل مرحلة؛ مفاتيح ReportRegistry ومحتواه محفوظة |
| M5 | فصل الأوامر والقراءة والتقارير والتصدير والمساعدات المشتركة؛ 140 اختبارًا ناجحًا |
| S1 | تطبيق مصفوفة الحالات في التغيير وقائمة الانتقالات؛ 163 اختبارًا ناجحًا |
| M6 | تحقق اعتماديات DI وأعمار التسجيلات وترتيب SoD والعقود والهيكل؛ 165 اختبارًا ناجحًا وبناء API بلا أخطاء |

يتضمن Services الآن 99 ملف C#، وReporting خمسة ملفات، وJobs ثلاثة، وDependencyInjection 18. تحتوي Core/Interfaces على 97 ملفًا. انخفض RetirementFileService من 2751 سطرًا عند بدء التنفيذ إلى نحو 1090 سطرًا، وتنفذ IRetirementFileService فقط. خدمة القراءة مستقلة بأربعة اعتماديات، بينما بقيت اعتماديات خدمة الأوامر الـ12 اللازمة للعمليات الحالية.

تسجيلات DI الأصلية الـ97 محفوظة من حيث العقود والتنفيذ والأعمار، مع تغيير تنفيذ IFileQueryService وإضافة تسجيلين لتقارير الملفات وأداة تصديرها، ليصبح العدد 99. تم الحفاظ على ترتيب قواعد SoD، وعلى تشغيل ExpiredPledgesJob وNotificationOutboxProcessor فقط. تبقى خدمات المواطن الأربع ومعالج التعهدات البديل غير مفعلة.

تختبر الإضافات نطاق الفرع والبحث والترقيم وحدود اليوم اليمني ورسالة الملف المفقود. اختبارات الحالة تغطي الحالات الثماني، ورفض الانتقالات غير المسموحة دون حفظ، والإغلاق النهائي، وموانع التفعيل ورفض SoD والتعهد الصالح والمنتهي. يحتفظ التفعيل بقيود الاستحقاق ودفتر الصرف والمستلم والوثائق والمراجعة والتدقيق.

نجح البناء الكامل باستخدام `dotnet build --nologo --no-restore -t:Rebuild -v quiet`: صفر أخطاء و110 تحذيرات موجودة سابقًا. تمت مقارنة رسائل التحذير وتكرارها بخط الأساس، دون تحذيرات جديدة. ما زال تحذير CS8603 الموجود في مساعد الاستعلامات للاختبارات ظاهرًا. نجح `git diff --check` للمشاريع الخلفية والوثائق المعدلة.

حدود التحقق: فُحص ملف Excel بإعادة قراءته للتحقق من الأعمدة والقيم والعربية وRTL، وفُحص إنشاء PDF وترويسة الملف؛ لم تُجر مقارنة بصرية للتقارير. تم التحقق من رسم اعتماديات DI دون اتصال بقاعدة بيانات، ولم يُختبر اتصال SignalR فعلي أو تشغيل IIS. تبقى صياغة تاريخ Excel المعتمدة على ثقافة المضيف كما كانت؛ استخدم اختبارها ثقافة محددة لتثبيت التوقع. لا تشمل هذه النتائج اختبارات سياسات API أو Middleware، لأن مشروع الاختبارات لا يشير إلى مشروع API.
