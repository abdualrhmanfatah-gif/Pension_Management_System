# اختبارات الإشعارات

نُقل التحقق إلى مشاريع الاختبار الفعلية. أُزيل مشروع التشخيص المعزول القديم لأنه كان يعيد تجميع خدمات الإنتاج ويحتوي assertions تثبت السلوك المعيب بعد إصلاحه.

- الخادم: `PensionMS.Tests/Services/NotificationOutboxProcessorTests.cs` و`NotificationServiceTests.cs` و`AdminNotificationServiceTests.cs`.
- المسارات والمصادقة: `PensionMS.IntegrationTests/NotificationFileHttpTests.cs`؛ لا تتطلب قاعدة بيانات.
- المتجر والتنقل: `ui/tests/notifications.test.ts`.
- الوقت: `ui/src/lib/notificationDate.test.ts`.
- المكوّنات: شغّل Vite ثم افتح `/.debug-capture/notification-review.html` ونفّذ `await window.runNotificationReview()`؛ حقول passes كلها يجب أن تكون true. HTTP وSignalR والصوت محاكية، والمكوّنات والاستعلامات والمتجر حقيقية.
- النتائج: `docs/audits/notification-remediation-results.md` و`plan/notification-fix-*.txt`.

آثار notification-audit القديمة توثق نتيجة التدقيق السابقة، وليست دليل قبول للإصلاح.