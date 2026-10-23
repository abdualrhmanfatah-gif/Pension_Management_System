# Feature Context: إصلاح آلية الإشعارات (Notification Mechanism Remediation)

## Document Metadata
- **Generated**: 2026-10-11
- **Input Type**: existing_document
- **Source**: `plan/notification-remediation-plan.md` (نُقل نص الخطة كاملًا في المحادثة)
- **Status**: DISCOVERY_COMPLETE

---

## Original Request

خطة إصلاح الإشعارات (NF0–NF6) المرجعية تقرير التدقيق `docs/audits/notification-mechanism-audit.md` (نتائج N01–N16). تهدف الخطة إلى: حفظ إشعارات الأعمال داخل معاملاتها، إيصال السجل الصحيح عند الدخول والعودة من الانقطاع، تنبيه لحظي واحد لكل حدث، توحيد حالة القراءة والعدّاد، حماية الحذف الجماعي والمرفقات، ومعالجة ازدحام Outbox وتعدد نسخ الخادم — دون فقد بيانات ولا ادعاء بأن محاولة البث تعني الاستلام. المطلوب من هذه الاكتشاف: تقييم اكتمال الخطة مقابل أهدافها المعلنة، والثبات في الكود، والفجوات التي تحتاج قرار مستخدم.

---

## Problem Statement

الإشعارات تُحفظ في قاعدة البيانات لكن عرضها ووصولها اللحظي معطوب: البطاقة العائمة تعلق عند ١٠٠٪ ولا تستهلك إلا أول عنصر من السجل، وجلسة مستعادة بلا `fresh_login` لا تجلب القائمة ولا العدّاد، و`unreadCount` ينتهي إلى حجم الصفحة بدل قيمة الخادم، وطلب حذف فارغ `{}` يحذف جدول الإشعارات كاملًا، وإشعار إنشاء ملف يُحفظ بعد `Commit` فقد يجعل عملية ناججة تبدو فاشلة. النتيجة للمستخدم: عدد خاطئ، بطاقات ناقصة أو مكررة، ورسائل قد لا تصل أبدًا لمستخدم غير متصل.

**Evidence**: تقرير التدقيق `docs/audits/notification-mechanism-audit.md` — ١٤ مقارنة متصفح بـ١٣ انحرافًا (N01–N09, N16) و٨/٨ اختبارات خادم تشخيصية تعيد إنتاج السلوكيات (N10–N15)، ونتائج إضافية من تتبّع الكود (مرفقات بلا تحقق ملكية، إحصاء يوم UTC، مستلمون بأدوار غير موجودة). أُعيد التحقق من كل الادعاءات في الكود الحالي ضمن هذا الاكتشاف — راجع Codebase Research.
**Frequency**: كل دخول/إعادة تحميل/انقطاع اتصال (دورة المزامنة)، وكل عملية مالية أو إنشاء ملف (دورة الحفظ)، وأسبوعيًا لكل إجراء إداري بالحذف.
**Impact**: مستخدم يفوّت إشعارًا ماليًا حاسمًا أو يرى عددًا خاطئًا فيتّخذ قرارًا على معلومات ناقصة؛ حذف إداري فارغ قد يمسح سجل الإشعارات كاملًا؛ إشعار مرفق برابط معروف يصلح كفتح لمستخدم آخر؛ فشل حفظ بعد Commit يجعل ملف تقاعد ثابتًا يبدو فاشلًا.

---

## Core Intent Analysis

### WHO (Target Users)
- **مستخدمو الأعمال**: موظفو الفروع/الإدارة المركزية (بند، مالية، مستخدمون مخولون) يستقبلون إشعارات إنشاء الملفات والتسويات والرواتب والاستحقاقات.
- **المدراء (System.Admin)**: يديرون سجل الإشعارات وحذفها الجماعي وإرسالها.
- **مشغّلو النظام**: من يشغّل نسخ API/IIS والمعامل، ويحتاج سلامة Outbox عند التعدد أو التوقف.

### WHAT (Desired Outcome)
سجل وعدّاد صحيحين من الخادم عند كل جلسة صالحة، تنبيه عائم واحد لكل حدث جديد بلا تكرار، قراءة موحّدة عبر كل الأسطح والتبويبات، إشعارات تُحفظ مع معاملة أعمالها وتُبثّ عبر Outbox متين بحجز، حذف ومرفقات محميّان بتحقق ملكية، وإحصاءات/وقت متسقة بعقد ADR 0001.

### WHEN (Trigger Conditions)
عند الدخول أو إعادة التحميل أو عودة التركيز/الاتصال؛ عند وصول حدث أعمال جديد عبر SignalR؛ عند فتح/قراءة/حذف إشعار من أي سطح؛ عند إرسال إشعار إداري أو رفع مرفق؛ عند تشغيل عامل Outbox (٣٠ ثانية/دورة).

### WHY (Problem Being Solved)
المصدران المزدوجان (Zustand للجرس مقابل TanStack Query للصفحة) وأبواب غير محمية (حذف فارغ، مرفقات بلا ملكية) وحفظ خارج المعاملة تُنتج نتائج خاطئة وتسجيلًا غير موثوق؛ الخطة تُحوّلها إلى مصدر خادم واحد وحدود صريحة.

---

## Non-Goals (Explicit Exclusions)

| Non-Goal | Rationale |
|----------|-----------|
| بروتوكول تأكيد استقبال (delivery receipt) من العميل | قرار صريح في الخطة: `Sent` = «اكتملت محاولة البث»؛ القراءة حدث منفصل؛ لا يلزم لإصلاح مشكلات N01–N16 |
| إنشاء أدوار جديدة أو تعديل أدوار مستخدمين لتغطية خطأ تسمية `Accountant`/`Manager` | NF3 يرفض ذلك صراحةً؛ التصحيح يقع على قواعد المستلمين لا على البيانات |
| قنوات أخرى (بريد/رسائل نصية) أو تعدد قنوات Outbox | خارج نطاق التدقيق؛ القناة الحالية SignalR وحدها |
| تغيير عقود تواريخ الحقول غير المتعلقة بالإشعارات | NF5 يحدّث ADR 0001 ليعكس المحوّل الحالي ويوحّد طوابع الإشعارات فقط |
| ضمان «مرة واحدة» عبر الشبكة أو التسليم خلال ٣٠ ثانية | الخطة ترفضهما صراحةً؛ الحجز يمنع التزامن المعتاد فقط |

---

## Codebase Research

### Similar Patterns Found

#### Pattern 1: تجهيز إشعار داخل معاملة أعمال (النموذج المرجعي)
- **Location**: `PensionMS.Infrastructure/Services/Pension/Entitlements/FinancialEntitlementService.cs:180,190,208-209`
- **Relevance**: تعليق «Stage notifications inside the business transaction» + `EnqueueAsync` قبل `Commit` — بالضبط ما تطلبه NF3.
- **Reusable**: نمط `UnitOfWorkTransaction`/`BeginTransactionAsync` الحالي؛ `EnqueueAsync`/`EnqueueRoleAsync` (`NotificationService.cs:34-44`) يجهّزان دون حفظ ويتركانه للمالك.

#### Pattern 2: أقفال ذرية على SQL Server (precedent لحجز NF4)
- **Location**: `PensionMS.Infrastructure/Data/UnitOfWork.cs:449-466` (`LockFileReviewAsync`/`LockSalaryBatchAsync` بـ `UPDLOCK, HOLDLOCK`)، `PensionMS.Infrastructure/Services/ImmutableAuditService.cs:42` (`sys.sp_getapplock`)
- **Relevance**: NF4 يطلب حجز دفعة Outbox؛ المشروع يملك سابقة أقفال مُختبرة بدل البدء من صفر.
- **Reusable**: صيغة UPDATE/OUTPUT والأقفال؛ يبقى الاختبار على SQL Server شرطًا (Q4).

#### Pattern 3: ثلاثية سكربتات SQL (precheck/apply/rollback)
- **Location**: `scripts/db-access-indexes-precheck.sql`، `scripts/db-access-indexes-apply.sql`، `scripts/db-access-indexes-rollback.sql`
- **Relevance**: NF4 يطلب `scripts/notification-outbox-precheck/apply/rollback.sql` بنفس النمط (idempotent، علامة extended property للتراجع، `XACT_ABORT`).
- **Reusable**: الهيكل والقوالب الحرفية كما هي.

#### Pattern 4: مفاتيح استعلام موحّدة + useApiMutation (النموذج الأمامي لـ NF2)
- **Location**: `ui/src/constants/queryKeys.ts:310-314` (`notifications.all/list/unread`)، `ui/src/hooks/useApiMutation.ts:44-95`
- **Relevance**: الخطة تطلب hook مشترك يستهلك `queryKeys.notifications`؛ حاليًا المفاتيح موجودة لكن **غير مستهلكة** — الصفحة تستخدم مفاتيح داخلية `NotificationsPage.tsx:31` والمزود يبطل مفتاحًا ميتًا `NotificationProvider.tsx:283` (`["notifications","unread-count"]`).
- **Reusable**: `useApiMutation` يوفّر الإبطال والتوست العربي؛ يكفي توجيه استعلامات الإشعارات إليها.

#### Pattern 5: أدوات إعادة إنتاج/تحقق موجودة
- **Location**: `scripts/debug/NotificationAudit/NotificationDiagnostics.cs` (N10–N15 + ضابطان)، `ui/.debug-capture/notification-audit.html`، `PensionMS.IntegrationTests/NotificationOutboxSqlTests.cs:18-57`
- **Relevance**: NF0 يحوّل هذه الحالات التشخيصية إلى اختبارات انحدار؛ الواجهة والحراك والاختبار المعزول موجودة.
- **Reusable**: كلها تُستأنف كما هي؛ الاختبارات الجديدة تُضاف بجانبها لا بدلها.

#### Pattern 6: خدمة مصادقة/اعتماد مسارات عبر DI يدوي
- **Location**: `PensionMS.Infrastructure/DependencyInjection/NotificationsServiceCollectionExtensions.cs:11-12`
- **Relevance**: NF1 يشترط تسجيل أي خدمة وصول مرفق جديدة هنا (المشروع يسجّل يدويًا في `Program.cs`).
- **Reusable**: الملف موجود ويستقبل الاعتماد الجديد.

### Existing Infrastructure
- `YemenTime.TodayStartUtc` و`PeriodUtc` موجودان في `PensionMS.Core/Time/YemenTime.cs:9,16-28` ومستخدمان في خدمات أخرى — NF5 يحتاج فقط تطبيقهما على `GetStatsAsync`.
- `PensionMS.IntegrationTests` مشروع قائم مع `SqlTestDatabase.cs` و`PENSIONMS_TEST_SQLSERVER` و`NotificationOutboxSqlTests.cs` — بوابة NF6 موجودة.
- سياسة `System.Admin` مطبّقة على `AdminNotificationsController.cs:13`؛ مساعد `.ToActionResult()` موجود (`ControllerExtensions.cs:44`) لكن كل مسارات الإشعارات تعيد `Ok()` مباشر.
- سكربتات النشر `deploy-api.ps1`/`deploy-ui.ps1` موثقة في AGENTS.md لخطوة النشر.
- `relatedTableID` يُرسل الخادم فعلًا (`PensionMS.Core/DTOs/Notifications/NotificationDto.cs:32`) وغائب عن نوع الواجهة (`ui/src/types/notifications.ts`) — كما تدّعي الخطة.

### Code References
- `ui/src/components/notifications/FloatingNotification.tsx:83-95` — عيب المؤقّت N01 (`return prev` يتجاهل التناقص)؛ `:55` قراءة `notifications[0]` فقط (N04)
- `ui/src/components/providers/NotificationProvider.tsx:227-231` — بوابة `fresh_login` (N05/N07)؛ `:245-252` استبدال العدّاد (N06)؛ `:268-278` آثار بلا منع تكرار (N08)؛ `:146-149,299` لا إعادة جلب عند الاتصال (N09)؛ `:295-305` تأخير ٥ ثوانٍ؛ `:283` إبطال مفتاح ميت
- `ui/src/stores/useNotificationStore.ts:44-48` — العدّاد من حجم الصفحة (N06)؛ `:93-99` خصم محلي (N16)؛ `:136` `reset`
- `ui/src/components/layout/NotificationBell.tsx:23` — الجرس لا يجلب شيئًا (N05)؛ `:40-44` خصم بعد HTTP
- `ui/src/pages/notifications/NotificationsPage.tsx:39-48` — قراءة الكل لا تُحدّث المتجر؛ `:211` `<a href>` خام للمرفق
- `PensionMS.Infrastructure/Services/Notifications/AdminNotificationService.cs:266-294` — الحذف الفارغ يمسح الجدول (N13)؛ `:78-79` إحصاء «اليوم» من منتصف ليل UTC
- `PensionMS.Infrastructure/Services/Notifications/NotificationService.cs:315-318` — دفع العدّاد قبل إرجاع نجاح القراءة (N16)؛ `:362-373` ابتلاع فشل العدّاد بتعليق خاطئ (N15)؛ `:400` `FileUrl` خام
- `PensionMS.Infrastructure/Jobs/NotificationOutboxProcessor.cs:18,43,57` — ٣٠ ثانية/دفعة ٥٠؛ `:89-97` `Sent` بلا استقبال؛ `:53-58` قراءة بلا حجز (N12)
- `PensionMS.Infrastructure/Services/Pension/Files/RetirementFileService.cs:370,390,405-408` — Commit قبل الإشعار ثم catch/rollback على ملف ثابت
- `PensionMS.Infrastructure/Services/Payroll/SalaryPaymentBatchService.cs:203,269` — `EnqueueRoleAsync("Accountant"/"Manager")` بأسماء غير موجودة في Seeds (N14)؛ `:130` `NotifyRoleAsync` يحفظ خارج معاملة
- `PensionMS.Infrastructure/Data/Configurations/Security/RoleConfiguration.cs:17-23` — Seeds: Admin/PensionManager/PensionClerk/FinanceOfficer/Auditor/Archivist/Technician فقط
- `PensionMS.API/Program.cs:385,652` — `AddSignalR()` بلا `AddJsonProtocol` (لا يرث محوّل التاريخ)؛ `:579-633` فرع `/uploads` مصادقة بلا تحقق ملكية
- `ui/src/components/domain/admin/SendNotificationDialog.tsx:414` — يقبل Word/Excel مقابل سياسة تخزين PDF/JPG/PNG فقط (`LocalFileStorageService.cs:26-30`)

---

## Use Scenarios

### Scenario 1: دخول/إعادة تحميل الصفحة
**Actor**: موظف يستقبل إشعارات أعمال
**Trigger**: تسجيل دخول أو refresh للصفحة
**Goal**: رؤية سجله وعدّاده الصحيحين فورًا
**Expected Outcome**: الجرس والعدد صحيحان من الخادم؛ السجل القديم لا يفتح بطاقات عائمة؛ لا يعتمد التحميل على `fresh_login`.

### Scenario 2: انقطاع ثم عودة الاتصال
**Actor**: مستخدم بقى مفصولًا некотор الوقت ثم عاد
**Trigger**: عودة SignalR أو عودة تركيز الصفحة
**Goal**: استرجاع ما فات دون تكرار
**Expected Outcome**: القائمة والعدّاد يُعادان؛ بطاقات لا تُعاد من السجل القديم؛ صوت واحد لكل ID؛ أثناء الانقطاع إعادة جلب كل ٣٠ ثانية فقط والصفحة مرئية.

### Scenario 3: إنشاء ملف تقاعد
**Actor**: موظف ينشئ ملفًا
**Trigger**: نجاح عملية الإنشاء
**Goal**: ثبات الملف وإشعار المستلم في نفس الذرة
**Expected Outcome**: الملف + Notification + Outbox تُحفظ معًا وتُسترد معًا عند rollback؛ النقر يفتح تفاصيل الملف الصحيح ويحدّث القراءة بعد نجاح الخادم.

### Scenario 4: حذف جماعي إداري
**Actor**: مدير بسياسة `System.Admin`
**Trigger**: إرسال طلب حذف من نافذة الإدارة
**Goal**: حذف ما اختير فقط
**Expected Outcome**: `{}` أو قائمة فارغة أو `OnlyRead=false` وحدها → رفض برسالة عربية وصفر حذف وسجل تدقيق؛ الاختيارات الفعّالة تعمل ضمن نطاقها.

### Scenario 5: مرفق برابط معروف لمستخدم آخر
**Actor**: مستخدم مصادق آخر يعرف رابط الملف
**Trigger**: فتح الرابط مباشرة أو عبر `/uploads`
**Goal**: حماية ملكية المرفق
**Expected Outcome**: رفض عبر المسار الجديد والمسارين القديمين؛ المستلم وحده ينزّل عبر Blob مصادق عليه.

---

## Gap Analysis

### Identified Gaps

| # | Category | Gap Description | Impact |
|---|----------|-----------------|--------|
| 1 | Scope | NF3 يقول «إشعار ما بعد Commit وإنشاء الملف والمنادين المشابهم»، لكن الفحص يظهر صنفين غير ملتزمين فقط: `RetirementFileService.cs:390` (بعد Commit) وصنف `NotifyRoleAsync` الذي يحفظ خارج أي معاملة (`SalaryPaymentBatchService.cs:130`، `SettlementService.cs:109,533`، `MinistryApprovalService.cs:78,539,698`)؛ أغلب نداءات `Enqueue*` ملتزمون بالفعل. حدود «المنادين المشابهم» غير محددة | عمل زائد أو إغلاق ناقص لصنف خارج المعاملة |
| 2 | Behavior | قاعدة اختيار المستلمين المالية: أسماء أدوار مصحَّحة (`FinanceOfficer`/`PensionManager`) أم اختيار بالصلاحية؟ ونطاق الفرع: هل إشعار الدور يُحصر بفرع المعاملة أم يبقى عالميًا؟ الخطة تؤجل إلى «بعد مقارنة Seeds والصلاحيات» دون قاعدة نهائية | N14 يبقى مفتوحًا؛ إما صفر مستلمين أو تسريب بين الفروع |
| 3 | Scope | سياسة رفع المرفقات: NF5 يقول «البداية PDF/JPG/PNG فقط، وWord/Excel تحتاج دعمًا صريحًا فلا تُعرض قبل ذلك» — هل v1 يُسقط Word/Excel من النافذة أم يضيف دعمها للخادم؟ | إن قُرّر لاحقًا إضافتها، تتعارض قرار وتنفيذ NF5 |
| 4 | Integration | شكل الاستضافة (نسخة واحدة أم عدة عمال/IIS) مُعلَّق داخل NF0 بينما هو الذي يحكم أولوية حجز NF4 | تأخير قرار الحجز أو بناء حجز لا يحتاجه |
| 5 | Integration | توفّر قاعدة SQL اختبارية عبر `PENSIONMS_TEST_SQLSERVER` غير محسوم (غير مضبوطة في البيئة الحالية) | قبول NF4 (أقفال/معاملات) وNF6 (اختبارات HTTP/SQL) يبقى معلقًا |
| 6 | Behavior | عيب غير موثّق في التدقيق: `NotificationProvider.tsx:283` يبطل مفتاحًا ميتًا `["notifications","unread-count"]` بينما المصنع يعرّف `unread` ولا يستهلكه أحد | إصلاح مفاتيح NF2 يجب أن يغطيه صراحةً وإلا بقي فشل إبطال خفي |
| 7 | Scope | الخطة بلا قسم Non-Goals صريح ولا بيان توفّر بيئة اختبار ولا سجل مخاطر | تمدد نطاق غير مقصود (Word/Excel، بروتوكولات) وتأخر اكتشاف شرط القبول |
| 8 | User | ترتيب التنفيذ: هل يُنفَّذ NF0–NF6 كلها كمبادرة واحدة أم تُسلَّم NF1+NF2 أولًا (بندان بلا تغيير مخطط يغطيان أعلى الأولويات N01–N09, N13, N16)؟ | تأخر وصول الإصلاحات عالية الأثر خلف أعمال NF4/NF5 |

---

## Questions Requiring Resolution

### Q1: سياسة رفع المرفقات في الإصدار الأول
- **Category**: Scope
- **Owner**: User/Stakeholder
- **Blocking**: Yes
- **Gap**: الخطة تحسم أن البداية PDF/JPG/PNG فقط، لكنها لا تقول ما مصير خياري Word/Excel في النافذة.
- **Question**: ما المطلوب في v1 لمرفقات الإشعارات؟
- **Options**:
  - A) إسقاط Word/Excel من نافذة الإرسال (مطابقة لسياسة التخزين الحالية)
  - B) إضافة دعم Word/Excel للخادم (سياسة + تحقق محتوى) ضمن نفس المرحلة
  - C) تأجيل أي تغيير للنافذة والاكتفاء بمنع الرفع الفاشل برسالة عربية
- **Why It Matters**: يحدد مضمون NF5 ويمنع تقديم وعود في الواجهة لا يتحققها التخزين.
- **Resolution**: _{pending}_

### Q2: قاعدة المستلمين ونطاق الفرع للإشعارات المالية
- **Category**: Behavior
- **Owner**: User/Stakeholder
- **Blocking**: Yes
- **Gap**: `EnqueueRoleAsync("Accountant"/"Manager")` يستهدف أسماء غير موجودة في Seeds (N14)، والاختيار الحالي يتجاوز الفروع.
- **Question**: ما القاعدة المطلوبة؟
- **Options**:
  - A) أسماء أدوار مصحَّحة فقط (FinanceOfficer/PensionManager) مع الحفاظ على النطاق الحالي عبر الفروع
  - B) اختيار بالصلاحية (permission) بدل اسم الدور، مع استبعاد غير النشطين
  - C) تصحيح الأسماء + حصر نطاق الفرع بفرع المعاملة، مع إبقاء البث الإداري عالميًا منفصلًا
- **Why It Matters**: يحدد جدول المستلمين في NF3 ومعيار قبول «لا يصل لفرع ب».
- **Resolution**: _{pending}_

### Q3: شكل استضافة الإنتاج (عدد نسخ API/المعالج)
- **Category**: Integration
- **Owner**: User/Stakeholder
- **Blocking**: Yes
- **Gap**: الخطة تؤجل «تحديد شكل الاستضافة» إلى NF0، لكنه يحكم هل حجز NF4 أولوية عاجلة.
- **Question**: كم نسخة تعمل على الإنتاج اليوم وستعمل بعد النشر؟
- **Options**:
  - A) نسخة واحدة (عامل واحد) — الحجز وقائي للمستقبل فقط
  - B) أكثر من نسخة/إعادة تشغيل IIS متداخلة — الحجز شرط أساسي
  - C) غير معروف — يُفحص في NF0 قبل أي عمل في NF4
- **Why It Matters**: يحدد إن كان N12 (إرسال مزدوج) واقعًا حاليًا وأولوية NF4.
- **Resolution**: _{pending}_

### Q4: توفّر قاعدة SQL اختبارية
- **Category**: Integration
- **Owner**: Engineering
- **Blocking**: Yes
- **Gap**: `PENSIONMS_TEST_SQLSERVER` غير مضبوطة؛ اختبارات المعاملات والحجز والفهرس تحتاج SQL Server حقيقيًا.
- **Question**: هل توجد قاعدة اختبار مستقلة يمكن تخصيصها (LocalDB أو خادم) لاختبارات `PensionMS.IntegrationTests`؟
- **Options**:
  - A) نعم، يمكن ضبط `PENSIONMS_TEST_SQLSERVER` الآن
  - B) لا — تُكتب الاختبارات وتُعلّق بحالة «لم يتحقق على SQL»
- **Why It Matters**: قبول NF3/NF4/NF6 يعتمد عليها؛ بدونها تبقى الأقفال والمعاملات غير مُثبتة.
- **Resolution**: _{pending}_

### Q5: ترتيب تسليم المراحل
- **Category**: Scope
- **Owner**: User/Stakeholder
- **Blocking**: No
- **Gap**: الخطة تفرض تسلسل NF0→NF6 دون خيار تسليم مبكر.
- **Question**: هل يُنفَّذ التسلسل كاملًا كمبادرة واحدة، أم يُسلَّم NF1+NF2 (بلا تغيير مخطط) أولًا ثم تكمل الباقي؟
- **Options**:
  - A) التسلسل الكامل كما هو
  - B) NF0→NF1→NF2 أولًا تسليمًا مستقلًا، ثم NF3–NF6
- **Why It Matters**: يؤثر على زمن وصول أعلى الإصلاحات للمستخدم؛ لا يغيّر المضمون النهائي.
- **Resolution**: _{pending}_

---

## Parking Lot (Deferred Ideas)

| Idea | Source | Priority for Future |
|------|--------|---------------------|
| بروتوكول تأكيد استقبال من العميل (ack صريح) | التدقيق N10 (مرفوض صراحةً في الخطة) | Low |
| قنوات إضافية: بريد/رسائل نصية | Discovery | Low |
| لوحة مراقبة لمؤشرات Outbox (أقدم معلّق/معدل فشل) تتجاوز السجلات | NF4 (تُقاس أولًا بالسجلات) | Medium |
| إعادة تصميم صفحة إدارة الإشعارات (Beyond الحذف المحمي) | Discovery | Low |
| رفع حد الذاكرة/التجميع العربي للطابور فوق المقترح (٥٠) | الخطة (مقترح مبدئي) | Low |

---

## Goals (Pending Resolution)

_ستُعتمد هذه الأهداف بعد إجابة الأسئلة._ (مستمدة من أهداف الخطة المعلنة)

1. حفظ Notification وOutbox وبيانات الأعمال داخل معاملة واحدة قابلة للاسترداد والتحقق على SQL.
2. مصدر خادم واحد للسجل والعدّاد يقرأه الجرس والصفحة والتبويبات، مع استعادة عند كل جلسة/اتصال/تركيز.
3. تنبيه عائم واحد لكل ID لكل جلسة، بمؤقّت صحيح ونظيفة كاملة عند الخروج أو تبديل الحساب.
4. حذف جماعي ومرفقات لا يعملان إلا باختيار صريح وتحقق ملكية على كل المسار.
5. Outbox بحجز ذري وإرسال قابل للاستعادة يفصل نجاح الرسالة عن فشل العدّاد، مع `Sent` مُعاد تعريفها كمحاولة بث.

---

## Source Document Analysis

### Document: `plan/notification-remediation-plan.md`
- **Current Status**: Draft (مكتوب «جاهزة للتنفيذ» لكن سجل التنفيذ يظهر كل المراحل «لم تبدأ»، والأسئلة المفتوحة أدناه لم تُحسم)
- **Completeness**: ~85%

### Sections Present
- [x] الهدف — واضح وقابل للتحقق
- [x] القرارات الأساسية — ١٠ قرارات صريحة تُغلب معظم أسئلة النطاق
- [x] التسلسل وحجم الدفعات — جدول NF0–NF6 بربط كل مرحلة بنتائج N
- [x] مراحل NF0–NF6 — لكل مرحلة معايير قبول اختبارية
- [x] بوابات التحقق (NF6) — أوامر محددة لـ lint/build/test وقواعد عدم افتراض الأرقام القديمة
- [x] سيناريوهات القبول النهائية — ١٤ سيناريو بالنتيجة المطلوبة
- [x] ترتيب النشر والتراجع — ٦ خطوات مع تنويه استقلال إصلاحات الحماية
- [x] سجل التنفيذ — جدول حالات + قاعدة عدم الإغلاق دون دليل
- [ ] Non-Goals صريحة — القرارات تضمنها ضمنيًا لكنها غير مجمعة؛ خطر تمدد النطاق (Word/Excel، بروتوكولات)
- [ ] سجل الأسئلة المفتوحة — «تحديد شكل الاستضافة» و«مقارنة Seeds» مؤجلتان داخل النص دون مالك أو موعد
- [ ] بيان توفّر بيئة الاختبار — شرط `PENSIONMS_TEST_SQLSERVER` مذكور لكن حالته غير محسومة

### Issues Found

| # | Section | Issue | Severity |
|---|---------|-------|----------|
| 1 | NF3 | «المنادين المشابهم» غير محدد — الصنف غير الملتزم الحقيقي هو `NotifyRoleAsync` خارج المعاملة، لا كل نداءات `Enqueue*` (أغلبها ملتزم) | High |
| 2 | NF5 | صياغة رفع المرفقات تخلط بين «ما لن يُعرض» و«ما لن يُدعم» — لا تحدّد فعل v1 (Q1) | Medium |
| 3 | NF0/NF4 | شكل الاستضافة مؤجَّل مع أنّه مدخل حجز NF4 (Q3) | Medium |
| 4 | العام | لا قسم Non-Goals ولا سجل مخاطر ولا بيان بيئة اختبار | Medium |
| 5 | NF2 | لا يذكر صراحةً الإبطال الميت `["notifications","unread-count"]` (`NotificationProvider.tsx:283`) ضمن توحيد المفاتيح | Low |
| 6 | التدقيق المُشار إليه | اقتباس `main.tsx:59` لـStrictMode — الفعلي `main.tsx:39-42`؛ و`OfficeProperties.Description` في التدقيق سطر 82 قديم (الخطة تصححه صراحةً في NF0) | Low |

### Contradictions
- لا تناقض داخلي في الخطة؛ كل ادعاءات N01–N16 وN15/N16 أُعيد التحقق منها وتطابق الكود الحالي بدقّة (باستشهاد الأسطر في Codebase Research).
- Nuance واحد: الخطة توحي بأن إنشاء الملف والمنادين «كثيرون» خارج المعاملة، والحقيقة أن الصنف غير الملتزم محدود (ملف + `NotifyRoleAsync` خارج المعاملة) — تضييق النطاق يقلّل عمل NF3.
- التدقيق المُشار إليه يتعارض مع شجرة العمل الحالية في سطر بناء `OfficeProperties.Description` — الخطة تلتقط ذلك وتصنّفه «عائق قديم لا يُعامل كحقيقة»، والتحقق هنا يؤكد: الرمز غير موجود في `ExcelReportTemplate.cs` الحالي.

---

## Next Steps

After questions are resolved:
1. Update "Resolution" fields in Questions section (Q1–Q5)
2. Finalize Goals section and fold Non-Goals/بيئة الاختبار into the plan document (تصحيح NF3/NF5 صراحةً)
3. Proceed to RT-ICA assessment on this file
4. Then proceed to architecture design (بدءًا بـ NF0 لتسديد Q3/Q4 عمليًا)
