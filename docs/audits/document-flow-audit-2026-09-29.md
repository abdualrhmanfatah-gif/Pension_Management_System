# تقرير تدقيق مسار رفع الوثائق ومعالجتها وقصها

التاريخ: 2026-09-29. النطاق: النسخة الموجودة في مساحة العمل، دون تعديل كود التطبيق أو تشغيل طلبات على بيانات حقيقية.

عدد الملاحظات: **49**. «تشغيل» يعني اختبارًا اصطناعيًا على الكود الفعلي أو بناء المشروع. «كود» يعني مسارًا تتبعت مدخلاته وآثاره دون إعادة إنتاج كاملة على النظام المنشور. لا تدعي هذه المراجعة حصر كل عيب ممكن، ولم تُختبر أجهزة WIA الفعلية أو إعدادات IIS الإنتاجية أو قاعدة البيانات الحقيقية.

## خط السير الفعلي

1. الرفع الفردي والجماعي ونماذج الأشخاص تستخدم FileUploader لاختيار ملف أو التقاط صورة أو طلب المسح المحلي.
2. الكاميرا وبعض إجراءات التحرير تفتح ImageCropperDialog ثم CornerEditor؛ المعالجة تختار WebGL على الحاسوب وCPU على الجوال غالبًا.
3. المسح يستدعي ScannerAgent على localhost:5000؛ ADF يعيد صفحات JPEG في JSON، ثم combineScannedPages يحاول جمعها في PDF.
4. documentsApi.upload يرسل multipart إلى DocumentsController ثم DocumentService. الوجه الأمامي والخلفي للهوية قد يدمجان بخدمة ImageMergeHelper.
5. LocalFileStorageService يكتب داخل wwwroot/uploads/documents؛ بعدها تُحفظ قاعدة البيانات وتُحدّث النواقص والامتثال وطلب المراجعة.
6. العرض المباشر يستخدم /api/files، مع بقاء /uploads متاحًا. التنزيل عبر Axios وuseDocumentBlob يستخدمان بناء رابط إضافيًا.
7. DocumentProcessDialog وDocumentProcessPage يعيدان رفع النتيجة كوثيقة جديدة؛ لا يستبدلان سجل الأصل ولا ينشئان علاقة إصدار رسمية به.

## نتائج الاختبارات

- بناء الواجهة: فشل TS2307 بسبب pdf-lib المفقودة.
- مقارنة CPU/WebGL: القص المطلوب من Y=5 إلى45؛ CPU أعاد 5 إلى45، وWebGL أعاد56 إلى95، وتكررت النتيجة.
- المنظور: Y=50 بدل38 في العينة ذات شبه المنحرف.
- زوايا منطبقة: أعاد CPU الصورة كاملة بدل رفض الإدخال.
- Axios: الرابط الفعلي يحتوي /api/api/files.
- اسم يحتوي #: جزء الاسم بعد # لا يبقى ضمن pathname.
- FilesController: مسار ملف اصطناعي في uploads-backup أعاد PhysicalFileResult.
- التخزين: قُبل PNG تالف من8 بايت.
- SendFileAsync: لا يضع Content-Type وحده؛ بقي null في الاختبار.
- تحكم إيجابي: قص CPU مستطيل سليم أعاد الأبعاد والإحداثيات المتوقعة. تهيئة CornerEditor وسحب نقطة واحدة عملا في العينة؛ لم أدرجهما كعيب عام.

أوامر إعادة الإنتاج ومخرجاتها في [.audit-documents/README.md](F:/Projects/Pension_Management_System/.audit-documents/README.md)، [نتائج JavaScript](F:/Projects/Pension_Management_System/.audit-documents/probe-results.json)، [نتائج المتصفح](F:/Projects/Pension_Management_System/.audit-documents/browser-results.json)، [نتائج .NET](F:/Projects/Pension_Management_System/.audit-documents/backend-results.json).

## الملاحظات التفصيلية

### 1. [حرج] إتاحة محتوى الوثائق دون تسجيل دخول

- **نوع الدليل:** كود + فحص سمات المتحكم.
- **السبب/شرط الحدوث:** FilesController لا يحمل Authorize، ولا توجد FallbackPolicy تلزم المصادقة. والمسار القديم /uploads يعمل قبل المصادقة أيضًا.
- **الأثر:** من يعرف رابط الملف يستطيع طلب محتواه، حتى لو لم يملك صلاحية الوثائق. الحذف المنطقي لا يمنع هذا المسار لأنه لا يقرأ سجل الوثيقة.
- **الإصلاح المقترح:** تقديم الملفات عبر معرف الوثيقة مع التحقق من المستخدم والفرع والحذف المنطقي، وإغلاق المسار العام.
- **الموقع:** [PensionMS.API/Controllers/FilesController.cs:6](F:/Projects/Pension_Management_System/PensionMS.API/Controllers/FilesController.cs:6)، [PensionMS.API/Program.cs:707](F:/Projects/Pension_Management_System/PensionMS.API/Program.cs:707).

### 2. [حرج] غياب عزل الفروع في خدمة الوثائق

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** GetAllAsync يقبل branchId من الطلب، وباقي القراءة والرفع والتعديل والحذف تعمل بالمعرف دون سياق الفرع. فلتر DbContext العام يقتصر على IsDeleted.
- **الأثر:** صاحب صلاحية عامة يستطيع استهداف وثائق أو ملف فرع آخر بتغيير المعرف؛ الواجهة ليست حاجزًا أمنيًا.
- **الإصلاح المقترح:** فرض نطاق الفرع في استعلامات الخدمة والتحقق من صلاحية الوصول إلى المالك قبل كل تغيير.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:29](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:29)، [PensionMS.Infrastructure/Data/PensionDbContext.cs:283](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Data/PensionDbContext.cs:283).

### 3. [عالٍ] تجاوز حدود مجلد الرفع بسبب مقارنة بادئة نصية

- **نوع الدليل:** تشغيل معزول.
- **السبب/شرط الحدوث:** فحص StartsWith(_uploadsPath) يقبل مجلدًا مجاورًا مثل uploads-backup. الاختبار استدعى GetFile على ملف اصطناعي في ذلك المجلد وحصل على PhysicalFileResult.
- **الأثر:** يمكن قراءة ملفات خارج uploads إذا وُجد مجلد مجاور ذو البادئة نفسها وكان المسار يصل إلى المتحكم. لم أختبر مرور ترميز الطلب عبر IIS.
- **الإصلاح المقترح:** تطبيع المسار ومقارنته بحد مجلد كامل يتضمن الفاصل، ورفض المسارات الخارجة قبل فحص الوجود.
- **الموقع:** [PensionMS.API/Controllers/FilesController.cs:30](F:/Projects/Pension_Management_System/PensionMS.API/Controllers/FilesController.cs:30)، [PensionMS.API/Program.cs:718](F:/Projects/Pension_Management_System/PensionMS.API/Program.cs:718).

### 4. [عالٍ] صلاحية مشاهدة الوثائق تسمح بتغيير نتيجة فحصها

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** PUT /documents/{id}/review محمي بـFile.View.Sensitive. الخدمة تحفظ القيمة بعد تحويل int إلى enum دون فحص العضوية أو فصل المهام.
- **الأثر:** مستخدم للعرض يستطيع اعتماد/رفض وثيقة أو إرسال حالة غير معرفة؛ لا توجد حماية تمنع فحص وثيقته بنفسه في هذا المسار.
- **الإصلاح المقترح:** صلاحية مراجعة مستقلة، وفحص الحالات المسموحة وسبب الرفض وفصل المهام في الخادم.
- **الموقع:** [PensionMS.API/Controllers/DocumentsController.cs:212](F:/Projects/Pension_Management_System/PensionMS.API/Controllers/DocumentsController.cs:212)، [PensionMS.Infrastructure/Services/DocumentService.cs:654](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:654).

### 5. [عالٍ] رفع وثيقة يغيّر حالة ملف التقاعد دون حراسة الانتقال

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** UploadAsync يستدعي RequestReviewAsync الذي يضع StatusID=UnderReview دون قيد على الحالة السابقة أو استدعاء SoD في هذا المسار.
- **الأثر:** رفع وثيقة يمكن أن يعيد ملفًا مكتملًا أو مؤرشفًا أو موقوفًا إلى المراجعة، بدل تطبيق انتقال مقصود ومصرح به.
- **الإصلاح المقترح:** فصل طلب مراجعة التعديل عن حالة الملف، والتحقق من الانتقالات المسموحة والصلاحيات وفصل المهام.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:492](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:492)، [PensionMS.Infrastructure/Services/FileReviewService.cs:164](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/FileReviewService.cs:164).

### 6. [متوسط] تغييرات هذا المسار لا تُسجل في التدقيق غير القابل للتعديل المطلوب بالمشروع

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** يوجد تدقيق بالفعل عبر AuditSaveChangesInterceptor، لكنه يضيف AuditLog؛ لا يستدعي مسار تغيير الحالات IImmutableAuditService.
- **الأثر:** التدقيق العادي لا يحقق ضمان السجل غير القابل للتعديل الذي تنص عليه تعليمات المشروع.
- **الإصلاح المقترح:** إضافة أحداث تغيير الحالة إلى خدمة التدقيق غير القابل للتعديل دون إلغاء التدقيق الموجود.
- **الموقع:** [PensionMS.API/Interceptors/AuditSaveChangesInterceptor.cs:174](F:/Projects/Pension_Management_System/PensionMS.API/Interceptors/AuditSaveChangesInterceptor.cs:174)، [PensionMS.Infrastructure/Services/DocumentService.cs:654](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:654).

### 7. [عالٍ] تعطل بناء الواجهة بسبب اعتماد مفقود

- **نوع الدليل:** تشغيل.
- **السبب/شرط الحدوث:** npm.cmd run build فشل بخطأ TS2307 عند import('pdf-lib'). الحزمة غير مدرجة في package.json وغير موجودة في node_modules.
- **الأثر:** لا يمكن إنتاج بناء جديد، وتجميع صفحات ADF يعتمد على هذا المسار.
- **الإصلاح المقترح:** إضافة الاعتماد وملف القفل ثم اختبار تجميع عدة صفحات وبناء الواجهة.
- **الموقع:** [ui/src/lib/scanned-document.ts:3](F:/Projects/Pension_Management_System/ui/src/lib/scanned-document.ts:3)، [ui/package.json:1](F:/Projects/Pension_Management_System/ui/package.json:1).

### 8. [عالٍ] WebGL يقص من المنطقة المقابلة رأسيًا بدل المنطقة المحددة

- **نوع الدليل:** تشغيل متصفح معزول ومتكرر.
- **السبب/شرط الحدوث:** نفس المصدر والزوايا أعطيا CPU الصفوف 5–45، بينما أعطى WebGL الصفوف 56–95. الكود يقلب إحداثيات Y ويقلب الناتج مع عدم اتساق اتجاه نسيج المصدر.
- **الأثر:** قد تُحفظ منطقة أخرى من الوثيقة رغم تحديد الحواف الصحيحة. أجهزة الحاسوب تستخدم WebGL وفق shouldUseWebGL.
- **الإصلاح المقترح:** توحيد اتجاه إحداثيات الصورة والنسيج وقراءة البكسلات، وإضافة اختبار مقارنة بمنطقة غير متناظرة.
- **الموقع:** [ui/src/lib/webgl-processor.ts:315](F:/Projects/Pension_Management_System/ui/src/lib/webgl-processor.ts:315)، [ui/src/lib/webgl-processor.ts:438](F:/Projects/Pension_Management_System/ui/src/lib/webgl-processor.ts:438)، [ui/src/lib/device-capabilities.ts:105](F:/Projects/Pension_Management_System/ui/src/lib/device-capabilities.ts:105).

### 9. [عالٍ] القص الرباعي لا يطبق تصحيح منظور إسقاطيًا

- **نوع الدليل:** تشغيل + كود.
- **السبب/شرط الحدوث:** CPU وWebGL يستخدمان مزجًا ثنائيًا بين الزوايا، لا homography. في شبه منحرف اختباري كان موضع Y الناتج 50 مقابل 38 للتحويل الإسقاطي المتوقع.
- **الأثر:** صورة الورقة الملتقطة بزاوية قد تتشوه داخليًا حتى إذا طابقت الزوايا حدود الورقة.
- **الإصلاح المقترح:** استخدام تحويل منظور إسقاطي موحد في المعالجين مع اختبارات شبكة وخطوط مستقيمة.
- **الموقع:** [ui/src/lib/cpu-processor.ts:63](F:/Projects/Pension_Management_System/ui/src/lib/cpu-processor.ts:63)، [ui/src/lib/webgl-processor.ts:18](F:/Projects/Pension_Management_System/ui/src/lib/webgl-processor.ts:18).

### 10. [متوسط] قبول زوايا قص منعدمة المساحة أو متقاطعة

- **نوع الدليل:** تشغيل + كود.
- **السبب/شرط الحدوث:** CornerEditor يقيد كل نقطة بحدود الصورة فقط. CPU يعيد الصورة الأصلية عند انعدام أبعاد الناتج، ويقبل الزوايا المتقاطعة.
- **الأثر:** يمكن إعلان نجاح قص لم يحدث أصلًا، أو إخراج صورة مطوية/مشوهة بدل رسالة خطأ.
- **الإصلاح المقترح:** التحقق من التحدب والترتيب والمساحة الدنيا وعدم تقاطع الأضلاع قبل المعالجة.
- **الموقع:** [ui/src/components/ui/CornerEditor.tsx:358](F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:358)، [ui/src/lib/cpu-processor.ts:23](F:/Projects/Pension_Management_System/ui/src/lib/cpu-processor.ts:23).

### 11. [عالٍ] القص يخفض دقة الأصل قبل تحديد مساحة الوثيقة النهائية

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** تُصغّر الصورة كاملة إلى 768 أو 1024 أو 1536 بكسل حسب الجهاز ثم تُقص وتُحفظ JPEG بجودة 0.85.
- **الأثر:** وثيقة صغيرة داخل صورة كبيرة تفقد معظم تفاصيل النص والختم؛ الملف المحفوظ قد لا يصلح للتكبير أو الطباعة رغم وضوح الأصل.
- **الإصلاح المقترح:** فصل دقة المعاينة عن دقة التصدير ومعالجة المنطقة المطلوبة بالدقة الأصلية ضمن حدود ذاكرة محسوبة.
- **الموقع:** [ui/src/lib/device-capabilities.ts:92](F:/Projects/Pension_Management_System/ui/src/lib/device-capabilities.ts:92)، [ui/src/components/ui/ImageCropperDialog.tsx:257](F:/Projects/Pension_Management_System/ui/src/components/ui/ImageCropperDialog.tsx:257)، [ui/src/components/ui/ImageCropperDialog.tsx:52](F:/Projects/Pension_Management_System/ui/src/components/ui/ImageCropperDialog.tsx:52).

### 12. [متوسط] المعالجة الثقيلة تعمل على خيط الواجهة وتخصص الصورة كاملة قبل تصغيرها

- **نوع الدليل:** كود؛ لم تُقَس على هاتف فعلي.
- **السبب/شرط الحدوث:** يُنشأ Worker في ImageCropperDialog لكن لا يُرسل إليه عمل؛ processImage وعمليات الحدة تعمل تزامنيًا. كما يُنشأ Canvas بالحجم الطبيعي قبل التصغير.
- **الأثر:** صور الهاتف الكبيرة قد تجمد التفاعل أو تستهلك ذاكرة كبيرة قبل أن تعمل حماية التصغير. مؤقت المهلة لا يستطيع قطع حلقة CPU متزامنة.
- **الإصلاح المقترح:** نقل المعالجة إلى Worker فعلي وتصغير المصدر دون إنشاء نسخ كاملة غير لازمة، مع ميزانية للبكسلات.
- **الموقع:** [ui/src/components/ui/ImageCropperDialog.tsx:136](F:/Projects/Pension_Management_System/ui/src/components/ui/ImageCropperDialog.tsx:136)، [ui/src/components/ui/ImageCropperDialog.tsx:257](F:/Projects/Pension_Management_System/ui/src/components/ui/ImageCropperDialog.tsx:257)، [ui/src/pages/retirement-files/DocumentProcessPage.tsx:292](F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:292).

### 13. [عالٍ] مهلة القص تعلن الفشل لكنها لا تلغي العملية

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** المؤقت بعد 30 ثانية يعرض خطأ ويعيد isProcessing/isSaving إلى false؛ لا يلغي تحميل الصورة أو طلب الرفع. مهلة upload نفسها خمس دقائق.
- **الأثر:** يمكن للمستخدم إعادة المحاولة بينما العملية الأولى تستمر، فتُرفع نسختان أو تظهر رسالة نجاح بعد رسالة انتهاء المهلة.
- **الإصلاح المقترح:** ربط المهلة بإلغاء فعلي ورقم عملية، ومنع نتائج العمليات الملغاة من تغيير الواجهة أو الحفظ.
- **الموقع:** [ui/src/pages/retirement-files/DocumentProcessPage.tsx:396](F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:396)، [ui/src/components/ui/ImageCropperDialog.tsx:241](F:/Projects/Pension_Management_System/ui/src/components/ui/ImageCropperDialog.tsx:241)، [ui/src/api/documents.ts:28](F:/Projects/Pension_Management_System/ui/src/api/documents.ts:28).

### 14. [عالٍ] حفظ الصورة المعالجة يفقد مالكها وبياناتها الوصفية

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** الصفحة والنافذة ترسلان File وRetirementFileID وDocumentTypeID وNotes فقط، ولا ترسلان مالك الوثيقة الفعلي أو رقمها وتواريخها.
- **الأثر:** قص هوية مستحق/وكيل/وصي ينشئ وثيقة للملف العام، ويترك الهوية الأصلية كما هي؛ وقد يؤثر في احتساب النواقص لأشخاص آخرين.
- **الإصلاح المقترح:** حفظ نسخة مرتبطة بمعرف المصدر مع نقل المالك والبيانات والتحقق منها على الخادم؛ جعل إنشاء نسخة أو استبدالها قرارًا واضحًا.
- **الموقع:** [ui/src/pages/retirement-files/DocumentProcessPage.tsx:472](F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:472)، [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:474](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:474).

### 15. [متوسط] حالة جلسة المعالجة لا تُعاد تهيئتها عند تغيير الوثيقة

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** settings/history/zoom/manualCorners وoriginalImageRef لا تُصفّر عند تغيير docData، ولا توجد حالة جاهزية مستقلة للصورة تمنع الحفظ حتى onload.
- **الأثر:** عند إعادة استخدام النافذة لوثيقة أخرى قد تنتقل إعدادات السابقة إليها؛ وبقاء الأصل السابق أثناء التحميل يسمح بتعامل غير صحيح مع التعديلات.
- **الإصلاح المقترح:** ربط جلسة التحرير بمعرف الوثيقة، وتصفير الصورة والسجل والزوايا ومنع الحفظ قبل اكتمال تحميل المصدر الحالي.
- **الموقع:** [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:207](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:207)، [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:229](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:229)، [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:637](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:637).

### 16. [عالٍ] إضافة ملاحظة تمسح مالك الوثيقة ورقمها وتواريخها

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** noteMutation يرسل نوع الوثيقة والملاحظة فقط؛ UpdateAsync ينسخ حقول DTO الاختيارية الفارغة ويصفّر RetireeID/EntitledID/AgentID/GuardianID.
- **الأثر:** تعديل ملاحظة بسيط يمكن أن يحول وثيقة شخص إلى وثيقة عامة ويزيل رقمها وتاريخ إصدارها وانتهائها.
- **الإصلاح المقترح:** تخصيص تحديث للملاحظة فقط أو اعتماد تحديث جزئي يميز الحقل غير المرسل عن الحقل المراد مسحه.
- **الموقع:** [ui/src/components/domain/retirement/DocumentActions.tsx:55](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentActions.tsx:55)، [PensionMS.Infrastructure/Services/DocumentService.cs:568](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:568).

### 17. [عالٍ] عارض الوثيقة يسقط بيانات يحتاجها نموذج التعديل

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** mapToActionsItem يبني كائنًا محدودًا لا يحمل معرفات الأشخاص ولا DocumentNumber/IssueDate/ExpiryDate. نموذج التعديل يستخدم هذه البيانات لتعبئة الحقول.
- **الأثر:** فتح تعديل من العارض ثم الحفظ قد يرسل مالكًا افتراضيًا وحقولًا فارغة، حتى لو كانت الوثيقة الأصلية تحتويها.
- **الإصلاح المقترح:** تمرير DTO كامل أو جلب تفاصيل الوثيقة بمعرفها قبل فتح نموذج التعديل.
- **الموقع:** [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:51](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:51)، [ui/src/components/domain/retirement/EditDocumentDialog.tsx:153](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EditDocumentDialog.tsx:153).

### 18. [عالٍ] عدم التحقق من توافق المالك مع ملف التقاعد

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** UploadAsync يختار OwnerType بالأولوية لكنه يحتفظ بمعرفات الملاك المرسلة؛ إذا أُرسل RetirementFileID فإنه لا يتحقق من أن AgentID أو EntitledPersonID تابع لذلك الملف. التعديل يحتفظ غالبًا بـFileID السابق.
- **الأثر:** يمكن ربط وثيقة شخص من ملف بملف آخر أو حفظ عدة روابط متعارضة، مع تأثيرها في البحث والنواقص والمراجعة.
- **الإصلاح المقترح:** فرض مالك منطقي واحد، والسماح بمعرف الملف بوصفه سياقًا متحققًا منه لا مالكًا متعارضًا.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:241](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:241)، [PensionMS.Infrastructure/Services/DocumentService.cs:271](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:271)، [PensionMS.Infrastructure/Services/DocumentService.cs:575](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:575).

### 19. [متوسط] الخادم لا يفرض نشاط نوع الوثيقة ولا توافق نوعها مع المالك

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** التحقق من DocumentType يقتصر على وجوده وحدود الملف؛ لا يفحص IsActive أو OwnerType رغم وجودهما في الكيان.
- **الأثر:** يمكن رفع وثيقة من نوع معطل أو تابع لنطاق مختلف عبر الطلب المباشر.
- **الإصلاح المقترح:** التحقق من نشاط النوع وتوافقه مع المالك في الرفع والتعديل، وإرجاع سبب عربي محدد.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:212](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:212)، [PensionMS.Infrastructure/Services/DocumentService.cs:561](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:561)، [PensionMS.Core/Entities/DocumentType.cs:12](F:/Projects/Pension_Management_System/PensionMS.Core/Entities/DocumentType.cs:12).

### 20. [عالٍ] رفع وثيقة للملف العام يغلق نواقص عدة أشخاص

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** استعلام MissingDocuments يُقيد بالمالك فقط عند إرسال AgentID أو GuardianID أو EntitledPersonID. المسار العام لا يشترط أن تكون معرفات الأشخاص فارغة.
- **الأثر:** رفع هوية للمتقاعد قد يغلق هويات ناقصة لمستحقين ووكلاء في الملف نفسه لم تُرفع وثائقهم.
- **الإصلاح المقترح:** مطابقة نوع الوثيقة وهوية المالك كاملة؛ وفي رفع الملف العام اشتراط خلو معرفات ملاك الأشخاص.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:425](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:425).

### 21. [عالٍ] تعديل تصنيف الوثيقة يغير النواقص لجميع الملاك

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** عند تغيير النوع يعيد UpdateAsync جميع النواقص المستوفاة من النوع القديم إلى Pending ويغلق جميع Pending من النوع الجديد حسب FileID فقط؛ تغيير المالك وحده لا يعيد حسابها.
- **الأثر:** قد تُفتح نواقص صحيحة لأشخاص آخرين أو تُغلق نواقص غير مستوفاة؛ وقد يبقى المالك السابق مستوفيًا بعد نقل وثيقته.
- **الإصلاح المقترح:** إعادة حساب النواقص للمالك القديم والجديد وبالاعتماد على الوثائق الفعلية المتبقية.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:621](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:621).

### 22. [عالٍ] عداد الاكتمال يكتفي بوجود نوع الوثيقة دون تكراره لكل مالك

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** ComplianceEngine يستخدم Distinct(DocumentTypeID) على الملف كله، بينما كشف النواقص التفصيلي يميز المستحق والوكيل والوصي.
- **الأثر:** هوية واحدة قد تجعل النوع مكتملًا في العداد مع بقاء هويات أشخاص آخرين ناقصة. كما لا يميز هذا العد الوثائق المرفوضة أو المنتهية.
- **الإصلاح المقترح:** توحيد نموذج المتطلبات على زوج النوع والمالك، وتحديد قواعد صلاحية الوثيقة المستخدمة للاكتمال صراحة.
- **الموقع:** [PensionMS.Infrastructure/Services/ComplianceEngine.cs:72](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/ComplianceEngine.cs:72)، [PensionMS.Infrastructure/Services/MissingDocumentService.cs:291](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/MissingDocumentService.cs:291).

### 23. [متوسط] الحذف لا يعيد فتح النواقص اليدوية التي أغلقها الرفع

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** DeleteAsync يحذف منطقيًا ويحدث ملخص الامتثال ويطلب المراجعة، دون إعادة حالة MissingDocuments المرتبطة إلى النقص.
- **الأثر:** بعد حذف آخر وثيقة مستوفية قد يبقى سجل النقص اليدوي Fulfilled، مع اختلافه عن الكشف التلقائي والعداد.
- **الإصلاح المقترح:** إعادة تقييم الاستيفاء بعد الحذف وفق المالك والنوع والوثائق المتبقية.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:508](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:508)، [PensionMS.Infrastructure/Services/ComplianceEngine.cs:95](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/ComplianceEngine.cs:95).

### 24. [عالٍ] منع التكرار يخلط بين ملفات مختلفة وبين أصحاب مختلفين

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** المقارنة تعتمد FileID/DocumentTypeID/FileName/FileSize فقط؛ لا تفحص المحتوى أو معرف المالك. BulkUploadDialog يعتبر أي 409 نجاحًا.
- **الأثر:** صورتان مختلفتان بالاسم والحجم نفسيهما قد تُرفض الثانية، أو يُعتبر رفع الشخص الثاني ناجحًا رغم أن الموجود يخص شخصًا آخر.
- **الإصلاح المقترح:** استخدام بصمة محتوى ونطاق مالك صحيح، وإرجاع الوثيقة المطابقة والتحقق منها قبل اعتبار التعارض نجاحًا.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:345](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:345)، [ui/src/components/domain/retirement/BulkUploadDialog.tsx:459](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:459).

### 25. [عالٍ] لا توجد حماية فعلية من تكرار إعادة الطلب أو الرفع المتزامن

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** X-Request-Id يستخدم في نص تسجيل خطأ فقط. فحص التكرار منفصل عن INSERT ولا يوجد قيد تفرد مناسب؛ واسم ملف الهوية المدمجة يتغير بالتوقيت.
- **الأثر:** إعادة الطلب بعد انقطاع الشبكة أو إرسال طلبين متزامنين يمكن أن ينشئ نسختين؛ الدمج قد يتجاوز فحص الاسم والحجم بعد تغير الاسم.
- **الإصلاح المقترح:** مفتاح idempotency محفوظ ومعاملة/قيد مناسب للبصمة والمالك، وإرجاع النتيجة السابقة لإعادة الطلب.
- **الموقع:** [PensionMS.API/Controllers/DocumentsController.cs:173](F:/Projects/Pension_Management_System/PensionMS.API/Controllers/DocumentsController.cs:173)، [PensionMS.Infrastructure/Services/DocumentService.cs:319](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:319)، [PensionMS.Infrastructure/Services/DocumentService.cs:348](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:348).

### 26. [عالٍ] نجاح حفظ الوثيقة لا يضمن اكتمال معالجة النواقص والمراجعة

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** تحفظ الوثيقة أولًا؛ ثم سلسلة تحديثات داخل try/catch تبتلع الفشل وتسجله بـDebug.WriteLine. ويوجد SaveChangesAsync إضافي خارج ذلك catch.
- **الأثر:** قد تعلن العملية نجاحًا والنواقص أو المراجعة غير متسقة، أو تعلن فشلًا بعد حفظ الوثيقة فعليًا؛ إعادة المحاولة تزيد الالتباس.
- **الإصلاح المقترح:** معاملة للآثار المتلازمة أو معالجة لاحقة موثوقة قابلة للإعادة، مع حالة واضحة وسجل أخطاء تشغيلي دائم.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:403](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:403)، [PensionMS.Infrastructure/Services/DocumentService.cs:495](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:495).

### 27. [عالٍ] بعض نماذج الأشخاص تُخفي فشل رفع الهوية

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** AgentService وEntitledPersonService يسجلان فشل رفع الهوية ويواصلان إنشاء الشخص؛ بعض مسارات تحديث الملف تتجاهل ApiResponse الناتج عن UploadAsync.
- **الأثر:** حفظ نموذج الشخص قد يبدو ناجحًا بينما الوثيقة المختارة لم تُحفظ، خصوصًا مع ملف مرفوض أو فشل الدمج.
- **الإصلاح المقترح:** إرجاع نتيجة منفصلة وواضحة للمرفقات، والاحتفاظ بالملف وإتاحة إعادة المحاولة دون إعادة إنشاء الشخص.
- **الموقع:** [PensionMS.Infrastructure/Services/AgentService.cs:159](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/AgentService.cs:159)، [PensionMS.Infrastructure/Services/EntitledPersonService.cs:176](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/EntitledPersonService.cs:176)، [PensionMS.Infrastructure/Services/RetirementFileService.cs:1042](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/RetirementFileService.cs:1042).

### 28. [عالٍ] تحديث هوية المتقاعد يمرر معرف مستخدم ثابتًا بقيمة صفر

- **نوع الدليل:** كود؛ أثر FK مشروط ببيانات المستخدمين.
- **السبب/شرط الحدوث:** RetireeService يستدعي UploadAsync(uploadDto, 0) بدل المستخدم الحالي؛ Document.UploadedByUserID مرتبط بجدول المستخدمين.
- **الأثر:** تُنسب الوثيقة إلى جهة غير صحيحة، أو يفشل الحفظ بقيد FK إذا لم يوجد مستخدم 0. الملف على القرص قد يبقى بعد الفشل.
- **الإصلاح المقترح:** تمرير المستخدم الحالي عبر توقيع الخدمة وتجنب القيم الوهمية في سجل التدقيق.
- **الموقع:** [PensionMS.Infrastructure/Services/RetireeService.cs:240](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/RetireeService.cs:240)، [PensionMS.Infrastructure/Data/Configurations/DocumentEntitiesConfiguration.cs:100](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Data/Configurations/DocumentEntitiesConfiguration.cs:100).

### 29. [عالٍ] الوجه الخلفي لا يخضع لفحص الحجم والصيغة نفسه قبل الدمج

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** DocumentService يتحقق من File فقط، ثم يرسل FileBack مباشرة إلى ImageMergeHelper. يسمح File بامتداد PDF، لكن الدمج يتعامل مع المدخلين كصورتين.
- **الأثر:** مدخل خلفي كبير يُفك في الذاكرة قبل التحقق النهائي؛ وإرسال PDF مع وجه خلفي يفشل برسالة حفظ عامة بدل رفض واضح.
- **الإصلاح المقترح:** فحص المدخلين وحجم البكسلات والحجم الإجمالي وتحديد هل الدمج يدعم الصور فقط أم صفحات PDF أيضًا.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:208](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:208)، [PensionMS.Infrastructure/Services/DocumentService.cs:314](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:314)، [PensionMS.Infrastructure/Helpers/ImageMergeHelper.cs:32](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Helpers/ImageMergeHelper.cs:32).

### 30. [متوسط] تعارض قيود الملف بين الواجهة ونوع الوثيقة والتخزين

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** الواجهة تقبل .pdf,image/*، والخدمة تقرأ AllowedExtensions/MaxFileSize، لكن LocalFileStorageService يفرض دائمًا PDF/JPG/PNG و10MB.
- **الأثر:** قد يقبل المستخدم والخدمة ملف WebP أو حجمًا مسموحًا في إعداد النوع ثم يُرفض عند التخزين برسالة عامة؛ إعداد النوع لا يعمل كما يظهر.
- **الإصلاح المقترح:** تعريف سياسة رفع واحدة وتمريرها للواجهة وتطبيقها على الملف النهائي بعد التحويل.
- **الموقع:** [ui/src/components/domain/retirement/UploadDocumentDialog.tsx:242](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/UploadDocumentDialog.tsx:242)، [PensionMS.Infrastructure/Services/LocalFileStorageService.cs:25](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/LocalFileStorageService.cs:25).

### 31. [متوسط] فحص ترويسة الملف يقبل ملفًا تالفًا غير قابل للعرض

- **نوع الدليل:** تشغيل معزول.
- **السبب/شرط الحدوث:** أُرسل إلى LocalFileStorageService ملف PNG اصطناعي من 8 بايت يبدأ بالأربعة بايت المتوقعة، فحُفظ بنجاح.
- **الأثر:** يمكن إعلان رفع ملف لا يمثل صورة سليمة ثم إغلاق نقصه؛ فحص magic bytes وحده لا يثبت قابلية القراءة.
- **الإصلاح المقترح:** فحص بنية الصورة/عدد بكسلاتها، وفحص PDF ضمن حدود موارد، قبل قبول الملف كوثيقة مستوفية.
- **الموقع:** [PensionMS.Infrastructure/Services/LocalFileStorageService.cs:35](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/LocalFileStorageService.cs:35).

### 32. [متوسط] فشل حفظ قاعدة البيانات يترك ملفات يتيمة

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** SaveFileAsync يكتب الملف قبل AddAsync/SaveChangesAsync؛ تنظيف الملف موجود فقط عند اكتشاف نسخة مكررة، ولا يغطي فشل قاعدة البيانات.
- **الأثر:** فشل FK أو طول حقل أو قاعدة البيانات يترك محتوى على القرص دون سجل، وبقاء مسارات التنزيل العامة يزيد الأثر.
- **الإصلاح المقترح:** تخزين مؤقت ثم تثبيت منظم، أو تعويض بحذف الملف عند فشل إنشاء السجل، مع مهمة تنظيف آمنة.
- **الموقع:** [PensionMS.Infrastructure/Services/DocumentService.cs:312](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:312)، [PensionMS.Infrastructure/Services/DocumentService.cs:403](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/DocumentService.cs:403).

### 33. [متوسط] قيود حقول بيانات الوثيقة لا تُتحقق قبل الحفظ

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** DTO لا يحمل حدود طول Notes وDocumentNumber رغم حدود 2000 و50 في EF، ولا توجد مقارنة بين IssueDate وExpiryDate.
- **الأثر:** المدخلات الطويلة تصل إلى فشل قاعدة البيانات بدل تحقق عربي مبكر، ويمكن حفظ ترتيب تواريخ غير منطقي.
- **الإصلاح المقترح:** نقل قيود المخطط وقواعد التواريخ إلى تحقق DTO والخدمة والواجهة.
- **الموقع:** [PensionMS.Core/DTOs/Documents/UploadDto.cs:16](F:/Projects/Pension_Management_System/PensionMS.Core/DTOs/Documents/UploadDto.cs:16)، [PensionMS.Core/DTOs/Documents/DocumentDtos.cs:7](F:/Projects/Pension_Management_System/PensionMS.Core/DTOs/Documents/DocumentDtos.cs:7)، [PensionMS.Infrastructure/Data/Configurations/DocumentEntitiesConfiguration.cs:13](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Data/Configurations/DocumentEntitiesConfiguration.cs:13).

### 34. [عالٍ] تنزيل الوثيقة عبر Axios يكرر /api

- **نوع الدليل:** تشغيل.
- **السبب/شرط الحدوث:** getFileUrl يعيد /api/files/... بينما Axios baseURL=http://localhost:5001/api؛ getUri الفعلي أعاد /api/api/files/....
- **الأثر:** التنزيل وuseDocumentBlob يفشلان رغم إمكان نجاح العرض المباشر في img/iframe، ما يجعل المشكلة متقطعة حسب طريقة العرض.
- **الإصلاح المقترح:** توحيد بناء رابط الملفات: رابط مطلق صحيح أو مسار /files بالنسبة إلى Axios، مع فصل روابط المتصفح عن روابط عميل API.
- **الموقع:** [ui/src/api/axios.ts:55](F:/Projects/Pension_Management_System/ui/src/api/axios.ts:55)، [ui/src/components/domain/retirement/DocumentActions.tsx:69](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentActions.tsx:69)، [ui/src/hooks/useDocumentBlob.ts:32](F:/Projects/Pension_Management_System/ui/src/hooks/useDocumentBlob.ts:32).

### 35. [متوسط] أسماء الملفات لا تُرمز عند تحويلها إلى روابط

- **نوع الدليل:** تشغيل.
- **السبب/شرط الحدوث:** التخزين يحتفظ بالاسم الأصلي، وgetFileUrl لا يرمز مقاطع المسار. اسم report#1.pdf يجعل #1.pdf جزء fragment لا يُرسل إلى الخادم.
- **الأثر:** ملف ينجح رفعه قد يتعذر عرضه أو قصه أو تنزيله بسبب اسمه؛ والرمز % يحتاج معالجة دقيقة أيضًا.
- **الإصلاح المقترح:** استخدام اسم تخزين مستقل عن الاسم الأصلي وترميز مقاطع الرابط مرة واحدة.
- **الموقع:** [PensionMS.Infrastructure/Services/LocalFileStorageService.cs:61](F:/Projects/Pension_Management_System/PensionMS.Infrastructure/Services/LocalFileStorageService.cs:61)، [ui/src/api/axios.ts:55](F:/Projects/Pension_Management_System/ui/src/api/axios.ts:55).

### 36. [متوسط] مسار /uploads القديم لا يضع Content-Type المحسوب

- **نوع الدليل:** تشغيل لآلية الإرسال + كود المسار.
- **السبب/شرط الحدوث:** Program.cs يستنتج contentType ثم لا يسنده إلى Response.ContentType. اختبار SendFileAsync وحده أبقى ContentType=null.
- **الأثر:** روابط /uploads القديمة لا تحمل نوع الاستجابة المقصود، وقد تتعامل معها المعاينات والتنزيلات بصورة خاطئة. FilesController الجديد يضع النوع فعلًا؛ العيب يخص المسار القديم.
- **الإصلاح المقترح:** إزالة المسار القديم أو توحيده مع خدمة الملفات المحمية وإسناد Content-Type صراحة.
- **الموقع:** [PensionMS.API/Program.cs:732](F:/Projects/Pension_Management_System/PensionMS.API/Program.cs:732).

### 37. [عالٍ] مفتاح TanStack Query مشترك لنتيجتين مختلفتي البنية

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** DocumentsTab يستخدم [documents,fileId] لكائن يحوي documents/missingDocs/docTypes؛ useDocumentsByFileQuery يستخدم المفتاح نفسه لمصفوفة وثائق.
- **الأثر:** الانتقال بين الشاشات قد يعرض قائمة فارغة أو يمرر كائنًا إلى documents.map فتفشل الشاشة؛ كما تتداخل إعادة الجلب.
- **الإصلاح المقترح:** تخصيص مفتاح لكل بنية أو توحيد نتيجة الاستعلام مع select للمستهلكين.
- **الموقع:** [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:121](F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:121)، [ui/src/hooks/queries/useDocumentsByFileQuery.ts:11](F:/Projects/Pension_Management_System/ui/src/hooks/queries/useDocumentsByFileQuery.ts:11)، [ui/src/components/domain/retirement/sections/DocumentsSection.tsx:91](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/DocumentsSection.tsx:91).

### 38. [متوسط] الحفظ العادي من صفحة المعالجة لا يحدث ذاكرة الوثائق المؤقتة

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** handleSave يرفع الصورة ثم ينتقل إلى الملف دون invalidateQueries، بينما applyCrop في الصفحة نفسها يحدّث الاستعلام.
- **الأثر:** قد لا تظهر النسخة الجديدة فورًا في الشاشات التي تحتفظ بالبيانات صالحة مؤقتًا؛ ويتباين سلوك حفظ المعالجة وحفظ القص.
- **الإصلاح المقترح:** توحيد mutation وتحديث مفاتيح الوثائق والنواقص وملف التقاعد بعد النجاح.
- **الموقع:** [ui/src/pages/retirement-files/DocumentProcessPage.tsx:497](F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:497)، [ui/src/hooks/queries/useDocumentsByFileQuery.ts:16](F:/Projects/Pension_Management_System/ui/src/hooks/queries/useDocumentsByFileQuery.ts:16).

### 39. [عالٍ] إلغاء الرفع الجماعي لا يوقف الطلبات ولا بقية الطابور

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** AbortController لا يُمرر إلى documentsApi.upload؛ وحلقة pending لا تفحص signal قبل إطلاق التالي. الفحص يأتي بعد اكتمال الطلب.
- **الأثر:** بعد إغلاق/تفكيك النافذة يمكن أن تستمر الوثائق في الرفع، بينما لا تُعرض نتائجها ولا يُحدّث المستخدم بما حُفظ.
- **الإصلاح المقترح:** تمرير signal لعميل HTTP وفحصه قبل كل طلب وإيقاف الطابور، ثم مصالحة نتائج الطلبات التي اكتملت.
- **الموقع:** [ui/src/components/domain/retirement/BulkUploadDialog.tsx:427](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:427)، [ui/src/components/domain/retirement/BulkUploadDialog.tsx:487](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:487)، [ui/src/api/documents.ts:28](F:/Projects/Pension_Management_System/ui/src/api/documents.ts:28).

### 40. [متوسط] ناتج القص والكاميرا لا يمر دائمًا بفحص الملف النهائي

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** onCrop يرسل JPEG مباشرة إلى onChange دون إعادة فحص accept/maxSizeMB. التقاط كاميرا سطح المكتب يسير في المسار نفسه.
- **الأثر:** حقل يقبل PNG فقط قد يستقبل JPEG بعد القص، أو يتجاوز الناتج سقف الحجم الخاص بالحقل ثم يُرفض لاحقًا.
- **الإصلاح المقترح:** إعادة استخدام تحقق موحد على كل نتيجة نهائية من الرفع والكاميرا والماسح والقص.
- **الموقع:** [ui/src/components/ui/FileUploader.tsx:338](F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:338)، [ui/src/components/ui/FileUploader.tsx:920](F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:920).

### 41. [متوسط] صلاحية زر المعالجة لا تطابق صلاحية حفظ نتيجته

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** DocumentActions يظهر المعالجة وفق File.Documents.Update، لكن الحفظ يستخدم POST /documents الذي يتطلب File.Documents.Upload.
- **الأثر:** مستخدم يستطيع فتح الأدوات وإجراء العمل ثم يتلقى 403 عند الحفظ إذا كان يملك التعديل دون الرفع.
- **الإصلاح المقترح:** مواءمة صلاحية العملية الفعلية أو إنشاء endpoint تعديل نسخة وثيقة بصلاحية محددة.
- **الموقع:** [ui/src/components/domain/retirement/DocumentActions.tsx:39](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentActions.tsx:39)، [PensionMS.API/Controllers/DocumentsController.cs:170](F:/Projects/Pension_Management_System/PensionMS.API/Controllers/DocumentsController.cs:170).

### 42. [متوسط] تنزيل الصورة المعالجة قد يحمل امتداد الأصل رغم أنها JPEG

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** handleDownload يضبط البيانات على image/jpeg ويشتق اسم التنزيل من docData.fileName دون تغيير امتداده.
- **الأثر:** مع أصل PNG قد ينزل ملف اسمه processed_*.png ومحتواه JPEG، فيربك البرامج والتحقق اللاحق من الامتداد.
- **الإصلاح المقترح:** اشتقاق امتداد الاسم من صيغة التصدير الفعلية، والحفاظ على امتداد مناسب في تنزيل الوثائق أيضًا.
- **الموقع:** [ui/src/pages/retirement-files/DocumentProcessPage.tsx:523](F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:523)، [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:527](F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:527).

### 43. [عالٍ] ADF يعيد دفعة ناقصة كأنها مسح ناجح

- **نوع الدليل:** كود؛ لم يُختبر بجهاز فعلي.
- **السبب/شرط الحدوث:** إذا حدث خطأ نقل بعد صفحة واحدة فإن العامل يسجل الخطأ ويخرج ثم يعيد الصفحات المتاحة باستجابة نجاح؛ وكذلك يتوقف عند MaxAdfPages=150 دون وصف truncation.
- **الأثر:** قد يُرفع مستند ناقص الصفحات دون علم الموظف، خصوصًا عند انحشار الورق أو مشكلة برنامج التشغيل.
- **الإصلاح المقترح:** إرجاع حالة اكتمال وعدد صفحات وسبب توقف؛ وعدم اعتماد دفعة جزئية إلا بعد قرار صريح من المستخدم.
- **الموقع:** [PensionMS.ScannerAgent/Program.cs:315](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:315)، [PensionMS.ScannerAgent/Program.cs:337](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:337)، [PensionMS.ScannerAgent/Program.cs:363](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:363).

### 44. [عالٍ] طلبات الماسح غير متسلسلة والمهلة لا توقف خيط WIA

- **نوع الدليل:** كود؛ أثر الجهاز غير مختبر.
- **السبب/شرط الحدوث:** كل طلب يحصل على معالجة ThreadPool وخيط STA؛ لا يوجد قفل للجهاز. Join لأربع دقائق يعيد 504 لكنه لا يوقف الخيط أو عملية المسح.
- **الأثر:** عدة تبويبات أو إعادة المحاولة بعد المهلة قد تتنافس على الجهاز وتراكم خيوطًا عالقة؛ لا يوجد سقف إجمالي لذاكرة دفعات Base64.
- **الإصلاح المقترح:** طابور مسح واحد للجهاز وحالة busy وإلغاء مدعوم أو عزل العملية العالقة، وحدود للصفحات والبايتات.
- **الموقع:** [PensionMS.ScannerAgent/Program.cs:53](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:53)، [PensionMS.ScannerAgent/Program.cs:201](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:201)، [PensionMS.ScannerAgent/Program.cs:218](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:218).

### 45. [عالٍ] إعداد المسح الافتراضي Letter يمكن أن يبتر أسفل ورقة A4

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** FileUploader يطلب 8.5×11 بوصة؛ وProcessImage يقص من أعلى اليسار إلى الأبعاد المطلوبة إذا كانت الصورة أكبر.
- **الأثر:** عند مسح A4 بطول نحو 11.69 بوصة يمكن فقدان قرابة 17.7 مم من الأسفل، بما قد يشمل توقيعًا أو ختمًا.
- **الإصلاح المقترح:** اختيار حجم الورقة صراحة، واعتماد A4 المناسب للسياق أو قراءة حجم الجهاز، وإظهار معاينة كاملة قبل الاقتصاص.
- **الموقع:** [ui/src/components/ui/FileUploader.tsx:65](F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:65)، [PensionMS.ScannerAgent/Program.cs:476](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:476).

### 46. [متوسط] إزاحة مسح الصورة الشخصية تُرسل ثم تُهمل

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** الواجهة ترسل offsetX/offsetY=0.1، لكن العامل لا يقرأهما ويضبط بداية المسح على صفر. وضع الصورة يطلب 1.35×1.75 بوصة رغم تعليق يتحدث عن 4×6 سم.
- **الأثر:** تبدأ منطقة الالتقاط من موضع غير المقصود، فتدخل حواف أو يُقص جزء من الصورة الشخصية؛ المقاس الناتج أيضًا لا يطابق الوصف.
- **الإصلاح المقترح:** إقرار عقد وحدات ومقاسات واضح وتمرير الإزاحة إلى إعدادات WIA والقص البديل.
- **الموقع:** [ui/src/components/ui/FileUploader.tsx:448](F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:448)، [PensionMS.ScannerAgent/Program.cs:182](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:182)، [PensionMS.ScannerAgent/Program.cs:424](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:424).

### 47. [متوسط] قائمة CORS الثابتة تمنع الماسح في عناوين نشر صحيحة

- **نوع الدليل:** كود؛ يعتمد على عنوان النشر.
- **السبب/شرط الحدوث:** العامل يسمح بخمسة origins محددة فقط؛ لا يسمح مثلًا بـhttp://localhost على منفذ 80 أو باسم خادم جديد أو HTTPS.
- **الأثر:** قد يعمل الماسح أثناء التطوير ويفشل بعد النشر رغم عمل الخدمة والجهاز.
- **الإصلاح المقترح:** تحميل قائمة origins من إعداد موثوق للنشر مع رسالة تشخيص تفرق بين حظر CORS وعدم وجود الجهاز.
- **الموقع:** [PensionMS.ScannerAgent/Program.cs:72](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:72).

### 48. [عالٍ] العامل لا يرفض طلب المسح من Origin غير مسموح

- **نوع الدليل:** كود؛ قراءة الرد تبقى خاضعة للمتصفح.
- **السبب/شرط الحدوث:** عندما لا يطابق Origin القائمة لا يضع العامل ترويسة السماح، لكنه يواصل HandleScan ولا يرفض الطلب.
- **الأثر:** CORS وحده لا يمنع تنفيذ عملية المسح لطلب يصل إلى الخدمة؛ قد تُشغل صفحة غير موثوقة الجهاز حيث تسمح قيود المتصفح بوصول الطلب. لا يعني هذا أنها تستطيع قراءة الصور.
- **الإصلاح المقترح:** رفض Origin غير الموثوق على الخادم، وربط العميل بالعامل برمز اقتران وتحقق مناسب للطلبات المحلية.
- **الموقع:** [PensionMS.ScannerAgent/Program.cs:75](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:75)، [PensionMS.ScannerAgent/Program.cs:92](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:92).

### 49. [متوسط] فحص جاهزية الماسح ورسائل الفشل مضللة

- **نوع الدليل:** كود.
- **السبب/شرط الحدوث:** checkScannerAvailable يكتفي بـr.ok، بينما /scanners يعيد [] مع 200 عند عدم وجود جهاز أو بعض الأخطاء. وhandleScan يعرض أي !ok، بما فيه 500/504، كإلغاء مسح.
- **الأثر:** يظهر الماسح متاحًا دون جهاز، أو يُنسب خطأ الجهاز/انتهاء المهلة إلى إلغاء المستخدم، ما يصعّب التشخيص.
- **الإصلاح المقترح:** فحص قائمة الأجهزة وحالة العامل، وتفسير أكواد الخطأ بصورة منفصلة عن إلغاء المستخدم.
- **الموقع:** [ui/src/components/ui/FileUploader.tsx:74](F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:74)، [ui/src/components/ui/FileUploader.tsx:463](F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:463)، [PensionMS.ScannerAgent/Program.cs:163](F:/Projects/Pension_Management_System/PensionMS.ScannerAgent/Program.cs:163).

## قيود تصميم وتجربة تحتاج قرارًا

- ناتج ADF المجمع ودمج وجهي الهوية هو PDF، لكن أدوات المعالجة الحالية تقبل الصور فقط؛ لا توجد دورة لقص صفحات PDF وإعادة تجميعها. هذا نقص قدرة، وليس دليلًا على فقدان صفحات في دالة التجميع نفسها.
- المعالجة تحفظ نسخة جديدة بلا ParentDocumentID أو سجل إصدارات أو رابط قابل للتتبع إلى الأصل. يجب تحديد معنى «حفظ المعالجة» للمستخدم قبل تصميم الإصلاح.
- CornerEditor يبدأ بإطار داخلي يستبعد10% من كل جانب؛ لا توجد عملية كشف حواف تلقائي مستخدمة في المسار المفحوص. تأكيد الإطار الافتراضي يقتطع الهوامش.
- اختيار الماسح يتم آليًا لأول جهاز WIA، دون اختيار جهاز من الواجهة؛ يحد ذلك من العمل عند توصيل عدة أجهزة.
- مقابض القص تعتمد أحداث المؤشر فقط؛ لا يوجد مسار لوحة مفاتيح لتعديل الإحداثيات.
- في FileUploader إلغاء قص صورة الكاميرا يعتمد الصورة الأصلية بدل التخلص من الالتقاط. يلزم جعل هذه الدلالة واضحة للمستخدم.
- لم أجد اختبارات مخصصة لهذا المسار ضمن اختبارات المشروع القائمة؛ أدوات التدقيق المعزولة لا تعوض اختبارات التكامل والماسح الحقيقي.

## ترتيب الإصلاح المقترح

1. إغلاق تنزيل الوثائق العام، وعزل الفروع، وتصحيح حدود المسارات وصلاحيات المراجعة.
2. إصلاح المنطقة المقصوصة في WebGL وتصحيح المنظور، ثم منع فقدان المالك والحقول عند الحفظ والتعديل.
3. إصلاح استيفاء النواقص والعدادات ومنع التكرار والمعالجة الجزئية، مع إضافة اختبارات تكامل لها.
4. إصلاح البناء وروابط Axios والتخزين المؤقت والإلغاء والتحقق النهائي للملفات.
5. معالجة اكتمال ADF وحجم الورقة وتسلسل طلبات الجهاز، ثم تنفيذ اختبار فعلي بأوراق A4 متعددة وانحشار ورق.

لا توجد إصلاحات لكود التطبيق ضمن هذا التقرير. الاختبارات استخدمت ملفات اصطناعية داخل مساحة العمل؛ لم تُرفع وثائق حقيقية ولم تُغير بيانات قاعدة البيانات.

