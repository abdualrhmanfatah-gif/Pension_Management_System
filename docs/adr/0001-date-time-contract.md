# ADR 0001 — عقد الوقت/التاريخ بين API والواجهة

- **الحالة**: معتمد مؤقتاً (نُفّذ الخيار أ). يحتاج مراجعة مستقلة.
- **التاريخ**: 2026-09-22
- **النطاق**: `PensionMS.API/Converters/YemenDateTimeConverter.cs`، `Program.cs:551`، كل حقول التاريخ في DTOs، وعرض التاريخ في `ui/`.

## السياق

- التخزين في قاعدة البيانات UTC (أعمدة `datetime2` تُكتب من `DateTime.UtcNow` و`GETUTCDATE()`).
- كل استجابات الـAPI تمر عبر `YemenDateTimeConverter`:
  - **Write**: UTC ← توقيت اليمن (`Asia/Aden`)، الصيغة `yyyy-MM-ddTHH:mm:ss` **بلا لاحقة `Z` وبلا إزاحة**.
  - **Read**: `DateTime.Parse` خام بلا تحويل.
- الواجهة تعرض التواريخ بـ`toLocaleDateString("ar-YE")` أي بمنطقة المتصفح، وتفسّر النصوص بلا إزاحة كوقت محلي للمتصفح.

## القرار

الإبقاء على العقد الحالي كما هو في هذه المرحلة، مع توثيقه ومنع الاستخراج التقويمي الخاطئ منه:

1. **DateTime (من API)**: وقت جدار اليمن بلا إزاحة. التاريخ التقويمي هو جزء التاريخ من النص نفسه — لا يُحوَّل عبر منطقة زمنية.
2. **Instant (Date/UTC/±HH:mm)**: يُحوَّل إلى تاريخ اليمن عبر `toDateOnlyYemen`.
3. **DateOnly (نص `YYYY-MM-DD`)**: تاريخ تقويمي يُحفظ كما هو بلا أي تحويل.
4. **الطوابع الزمنية** (`CreatedAt`/`UpdatedAt`/`AuditLog`/`Date.now()` لتوكنات JWT): تبقى UTC وتُسلسل بـ`toISOString()` — خارج نطاق `toDateOnlyYemen`.

الواجهة تستهلك هذا العقد عبر `ui/src/lib/date.ts`:

- `todayYemen()` — تاريخ اليوم التقويمي في اليمن (بديل `new Date().toISOString().split("T")[0]`).
- `toDateOnlyYemen(instant)` — لحظة زمنية ← تاريخ اليمن (عبر `Intl.DateTimeFormat.formatToParts` مع `timeZone: "Asia/Aden"`).
- `normalizeDateInputValue(value)` — تطبيع قيم `DateInput` (DateOnly كما هو، وقت جدار اليمن بجزء تاريخه، Instant بتحويل، غير الصالح `""`).

## العواقب

- الإيجابيات: لا تغيير في شكل البيانات المرسلة، لا هجرة، لا انحدار في الحسابات المحاسبية، ومنع نمط `toISOString().split("T")[0]` بقاعدة ESLint (`error`).
- السلبيات: يبقى تفسير النصوص بلا إزاحة معتمداً على جهاز المستهلك.

## مخاطر معروفة (تحتاج معالجة مستقلة)

1. **عرض يعتمد على منطقة المتصفح**: على جهاز بإزاحة غير إزاحة اليمن، العرض قد يختلف. غير مُعالَج في هذا القرار.
2. **`DateInput` مع `DateTime` كامل**: كان يعرض فراغاً (`Input.tsx` يشترط `YYYY-MM-DD`). أُصلح عبر `formatDateInputDisplay` (تطبيع + عرض `DD/MM/YYYY`)، مع إبقاء `isValidDate` مسؤولاً عن التحقق من الكتابة اليدوية.
3. **معرّف منطقة خاطئ**: `RiskScoringService.cs:31` يستخدم `Arabic Standard Time` (= `Asia/Baghdad` حسب جدول مايكروسوفت) بدل `Arab Standard Time` (= `Asia/Aden`). يُعالَج في P3/P4 بتوحيد المصدر.
4. **قرار لاحق محتمل**: إرسال `+03:00` صريح أو UTC مع تحويل عرض مركزي — مؤجل حتى مراجعة مستقلة لأنه يمس كل عمليات العرض.

## المراجع

- `PensionMS.API/Converters/YemenDateTimeConverter.cs`
- `PensionMS.API/Program.cs:551`
- `ui/src/lib/date.ts`, `ui/src/lib/date.test.ts`
- `ui/eslint.config.js` (قاعدة `no-restricted-syntax`)
