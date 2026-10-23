# NF0 — أساس التحقق: لقطة الخط الأساس

التاريخ: ١١ أكتوبر ٢٠٢٦. الحالة: مكتمل (اختبارات حمراء متعمدة + ضوابط خضراء).

## الخط الأساس قبل التغيير

| البوابة | النتيجة | الأثر |
|---|---|---|
| `git status` | تغييرات سابقة واسعة محفوظة (نظير الخطة) | `plan/nf0-git-snapshot.txt` |
| `dotnet build` PensionMS.Tests | 0 أخطاء | `plan/nf0-build-tests.txt` |
| `dotnet build` PensionMS.API | نجاح تجميع؛ 2 أخطاء نسخ MSB3021/MSB3026 لأن `PensionMS.API.exe` (PID 33880) يعمل ويقفل DLL — ليس عائق كود | `plan/nf0-build-api.txt` |
| عائق `OfficeProperties.Description` القديم | غير موجود في `ExcelReportTemplate.cs` الحالي — تأكيد أن التدقيق قديم في هذا السطر | grep: صفر نتائج في *.cs |
| UI lint | 391 مشكلة (305 أخطاء، 86 تحذيرات) — خط الأساس الجديد | `plan/nf0-ui-lint-before.txt` |
| UI build | نجاح | `plan/nf0-ui-build.txt` |
| UI test قبل الإضافة | 106/106 | `plan/nf0-ui-test-before.txt` |

## اختبارات الانحدار المضافة (NF0)

### Backend — `PensionMS.Tests/Services/`

| الملف | التغطية | الحالة |
|---|---|---|
| `AdminNotificationServiceTests.cs` | N13: رفض `{}` و`OnlyRead=false` وقائمة فارغة؛ DeleteAll وحده يحذف الكل؛ IDs وCategory وتقاطع DeleteAll+Category | 3 حمراء (الرفض) + 3 خضراء |
| `NotificationOutboxProcessorTests.cs` | N11: إخفاق `UnreadCountUpdated` لا يعيد الرسالة؛ ضوابط Sent/缺失/انتهاء الحد | 1 حمراء + 3 خضراء |
| `NotificationServiceTests.cs` | ضوابط: Enqueue يجهّز دون حفظ، نطاق الدور النشط، رفض قراءة إشعار آخر، N15: دفع العدد لا يكتب Outbox | 4 خضراء |

`dotnet test --filter FullyQualifiedName~Notification`: **4 حمراء + 11 خضراء من 15**.
الحمراء المتعمدة:
1. `BulkDelete_EmptyDto_IsRejectedAndDeletesNothing` (N13)
2. `BulkDelete_OnlyReadFalseWithoutIdsOrFilters_IsRejected` (N13)
3. `BulkDelete_EmptyIdsList_IsRejected` (N13)
4. `CountPushFailure_DoesNotResendDeliveredMessage` (N11)

المجموعة الكاملة: **200 ناجح / 4 فاشلة من 204** — الفشل فقط في الحمراء أعلاه.
سجل: `plan/nf0-backend-tests-run.txt`، `plan/nf0-backend-tests-full.txt`.

### Frontend — `ui/tests/notifications.test.ts`

| الاختبار | التغطية | الحالة |
|---|---|---|
| العدّاد والحجم لا يستبدلان بأحجام الصفحة | N06 | حمراء |
| markAsRead لا يخصم من عدّاد مدفوع | N16 | حمراء |
| وصول حدث لا يعدّل العدّاد محليًا | N06/N08 | حمراء |
| منع التكرار / سقف 50 / reset / markAllAsRead | ضوابط | 4 خضراء |

`npm test`: **110 ناجح / 3 فاشلة من 113** (106 قديمة كلها ناجحة + 7 جديدة).
سجل: `plan/nf0-ui-test-after.txt`.

## قرارات مسجّلة

- اختبارات «رفض الحذف الفارغ» مكتوبة بالسلوك **المطلوب** (Fail closed) — تُغلق عند NF1.
- اختبار N11 يحدد السلوك المطلوب: الرسالة الناجحة لا تُعاد بسبب فشل العدّاد؛ التغطية الكاملة لحجز NF4 تأتي على SQL مستقل (`PensionMS.IntegrationTests` + `PENSIONMS_TEST_SQLSERVER`) — غير مضبوطة الآن؛ بقاء قبول NF4 معلقًا حتى الإعداد.
- N14 (أسماء الأدوار) لم يُكتب بعد اختباره الحاسم — قرار Q2 (الصلاحية مقابل الاسم) معلق؛ اختبارات EnqueueRoleAsync الحالية تغطي السلوك المضمون (مطابقة الاسم الفعلي).
- أدوات `scripts/debug/NotificationAudit` و`ui/.debug-capture/notification-audit.html` تبقى كما هي لإعادة الإنتاج؛ لم تُحلّ محلها.
- اختبارات المتصفح للمؤقّت/StrictMode (N01/N07) تحتاج تعديل المكوّنات أولًا (NF2)؛ تُضاف مع مرحلة التصحيح لا قبلها.
- بناء API يتطلب إغلاق `PensionMS.API.exe` الجارٍ قبل بوابة النشر النهائية.

## ما لم يُتحقق

- SQL: قفل/معامل/حجز — لا قاعدة اختبار (Q4 معلق).
- HTTP: مسارات الإشعارات — لا بيئة منشورة.
- أدوار/صلاحيات البيئة الفعلية — قراءة فقط، لم تُعدّل بيانات البيئة.
