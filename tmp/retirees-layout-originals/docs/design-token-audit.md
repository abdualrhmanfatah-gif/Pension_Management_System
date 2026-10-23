# تقرير تدقيق رموز التصميم — مُصحَّح (Design Token Audit, Corrected)

**المشروع**: نظام إدارة المعاشات التقاعدية — الواجهة الأمامية (React 19 + Vite + Tailwind 3.4)
**تاريخ التصحيح**: 2026-10-02
**ثبات سطر البيانات — 2026-10-17**: رقم الملف والمدخل في صف `flex` واحد دون التفاف؛ يحتفظ الرقم بعرضه الكامل، ويُختصر اسم المدخل الطويل بعلامة الحذف مع إبقاء الاسم الكامل في النص و`title`.
**إزالة الشفافية من الفئات المحددة — 2026-10-17**: أزيلت معاملات الشفافية من 130 استخدامًا لألوان النصوص والأيقونات و389 استخدامًا للحدود والفواصل، وحُوّل 116 استخدامًا لتخفيف العنصر كاملًا إلى `opacity-100`. شمل التعديل 181 ملفًا وفق الجرد السابق، مع الحفاظ على شروط الظهور ومتغيرات المرور والتركيز. سجل كل استبدال محفوظ في `plan/transparency-removal-changes.json` والجرد السابق في `plan/transparency-removal-before.json`، والنتائج في `docs/audits/ui-transparency-remediation.md`. لا يغيّر هذا العمل إحصاءات F-ID التاريخية.
**ترتيب البطاقة النهائي — 2026-10-17**: اسم المتقاعد وشارة حالة الملف في الصف الأول، ورقم الملف وبجانبه اسم المدخل عند توفره في الصف الثاني. أزيل وصف حالة الوثائق من العرض المرئي، وأصبح لون الخلفية يعبّر عنها؛ بقي وصف `sr-only` لقارئات الشاشة. أدلة إصلاح لون الحالة: `plan/documents-status-red.txt` و`plan/documents-status-green.txt` و`plan/documents-status-browser-results.json`.
**إزالة شفافية القوائم — 2026-10-17**: أزيلت شفافية الأسهم والأيقونات والحالات المعطّلة من `Select` و`NativeSelect`. تُميّز الحالات المعطّلة بخلفية `muted` صريحة مع الإبقاء على منع التفاعل؛ لا تتغير حركة فتح القائمة وإغلاقها. أدلة التحقق: `plan/select-opacity-lint-before.json` و`plan/select-opacity-lint-after.json` و`plan/select-opacity-build.txt` و`plan/select-opacity-tests.txt`. لا يغيّر هذا التعديل إحصاءات F-ID التاريخية.
**نص القوائم المنسدلة — 2026-10-17**: أضيف الرمز `--select-foreground` بقيمة أسود صريح في الوضع الفاتح، وباللون الحالي في الوضع الداكن. يستخدمه `SelectTrigger` و`SelectContent` وعناصر القائمة عند التركيز و`NativeSelect`؛ تبقى حالات التعطيل الدلالية كما هي. هذا تخصيص مطلوب للنص ولا يغيّر إحصاءات F-ID التاريخية. أدلة التحقق: `plan/select-text-lint-before.json` و`plan/select-text-lint-after.json` و`plan/select-text-build.txt` و`plan/select-text-tests.txt`.
**بطاقات الوثائق على الهاتف — 2026-10-17**: نُقل اسم المتقاعد فوق رقم الملف، وحُصر صف الإجراءات في الرفع والطباعة مع إزالة الفاصل الداخلي. أصبحت خلفية البطاقة تعبّر عن حالة الوثائق بمزج 10% من رمز الخطر أو التحذير أو النجاح مع `--card` باستخدام `color-mix`؛ الخلفية معتمة لمنع ظهور العلامة المائية خلال المحتوى. مُرّرت `rowClassName` إلى بطاقات الهاتف في `DataTable` الأساسي حتى يُطبّق التنسيق على حاوية البطاقة. أدلة التحقق: `plan/documents-card-lint-comparison.json` و`plan/documents-card-browser-results.json` ولقطات `plan/documents-card-{360,1280}-{light,dark}.png`. لا ألوان ثابتة أو رموز جديدة، ولا تعديل لإحصاءات F-ID التاريخية.
**إصلاح شارة حالة الملف — 2026-10-17**: `RetirementFileDto` يرسل `statusID` و`fileStatusName` دون `fileStatusColor`. أُزيل الاعتماد على الحقل غير الموجود، ووُحّد الربط بين `FILE_STATUS` وألوان الشارات في `ui/src/components/domain/retirement/FileStatusBadge.tsx`، للبطاقات والجدول. أُعيد إنتاج الشارة الشفافة للحالة 7 باستخدام الاستجابة الفعلية من حيث الحقول (`plan/documents-status-red.txt`)؛ الاختبار المصحح لا يزوّد الواجهة بحقل لون مصطنع. الجديد محايد، النشط أخضر، الموقوف والمرفوض أحمر، المغلق رمادي، الناقص والمعلق كهرماني، وتحت المراجعة أزرق.
**وضوح النصوص — 2026-10-17**: استُبدلت جميع استخدامات `text-foreground/80` في `ui/src` (9 مواضع) بـ `text-foreground` لإظهار لون النص الدلالي كاملًا في الوضعين الفاتح والداكن. شمل ذلك عناوين الفلاتر وتفاصيل الوصاية والمراسلات والإشعارات وبعض المكونات المشتركة. لم تُضف ألوان ثابتة أو رموز جديدة، ولا تتغير إحصاءات F-ID التاريخية بهذا التعديل. أدلة التحقق: `plan/foreground-opacity-lint-before.json` و`plan/foreground-opacity-lint-after.json` و`plan/foreground-opacity-build.txt` و`plan/foreground-opacity-tests.txt`.
**ترتيب الفلاتر — 2026-10-17**: «حالة الملف» و«حالة الوثائق» قائمتان متجاورتان، و«المدخل» أسفلهما بعرض كامل. تُطوى المجموعة كاملة على الهاتف باستخدام `Accordion`، وتبقى ظاهرة ابتداءً من `sm`. أضيف خيار `density="compact"` إلى `AccordionTrigger` الأساسي لتقليل الحشو مع الحفاظ على ارتفاع لمس 44px، وخُفّضت المسافات بين أقسام الصفحة والبطاقات.
**متابعة واجهة رفع الوثائق — 2026-10-17**: أزيل تلوين معرّف الملف واسمه بالأحمر لمجرد غياب الوثائق في `ui/src/pages/documents/MissingDocsCompletionPage.tsx`؛ أصبحت التحذيرات في شارات الحالة، مع استخدام خصائص `Input` و`NativeSelect` ومقاسات `Button` و`Badge` الأساسية بدل تصغيرها محليًا. عُزلت العلامة المائية في `ui/src/index.css` داخل سياق طبقات مستقل، خلف المحتوى (`z-index: -1`) وبشفافية أخف في الوضعين. لم تُضف ألوان ثابتة أو رموز تصميم جديدة، ولا تغيّر هذه المتابعة إحصاءات تدقيق F-ID التاريخية أدناه. أدلة التحقق: `plan/documents-ui-lint-comparison.json` و`plan/documents-ui-browser-results.json` ولقطات `plan/documents-ui-{360,1280}-{light,dark}.png`.

**المنهج**: تحليل ساكن فقط (static analysis) عبر `rg` ومتابعة السياق في الملفات. لم تتوفر معاينة حية (Light/Dark/RTL/viewports) أثناء التدقيق؛ لذلك كل النتائج تُفرّق بين ثقة الرصد في المصدر وثقة الأثر البصري (تُحدَّد لاحقًا بالمعاينة). لا تُعتبر مطابقة `rg` وحدها دليلاً على المراجعة.

---

## 1. نطاق التدقيق وتغطيته (Scope & Coverage)

### أوامر الجرد المستخدمة
- `rg --files ui/src | Measure-Object` → 583 ملفًا.
- `rg --files ui/src -g '!**/*.test.*' -g '!**/*.stories.*'` → 579 ملفًا فعليًا (استبعاد اختبارات وقصص).
- `rg --files ui/src/components/ui | Measure-Object` → **66 ملفًا** (بما فيها `print/` و`stories/`).
- `rg --files ui/src -g '*.test.*'` → **3 ملفات اختبار داخل `ui/src`** (`lib/date.test.ts`، `lib/homography.test.ts`، `lib/cpu-processor.test.ts`) + 3 ملفات داخل `ui/tests`.
- `rg --files ui/src/pages | Measure-Object` → 169 (ضمنها 15 ملفًا تحت `pages/print`).
- `rg --files ui/src/components | Measure-Object` → 265 (إجمالي `components`).

> **تصحيح P-9**: التقرير السابق عدّ `+4` ملفات اختبار داخل `ui/src` و`components/ui` غير دقيق؛ القيم المصححة أعلاه. كذلك `+29`"مستبعدًا" شملت `tests` و`stories` فقط، والتفاصيل في الملحق.

### الملفات المُستبعدة
- `ui/tests/*` (3 ملفات) — خارج `ui/src`.
- `*.test.*` داخل `ui/src` (3 ملفات) — اختبارات `node:test` (`node:test`).
- `*.stories.*` (1 ملف — `stories/DemoDialog.stories.tsx`) — بيئة Storybook غير المُثبتة.
- `node_modules/`، `dist/`، `.debug-harness/` — مولّدة/غير منشورة.
- `ui/src/api/*`، `ui/src/types/*` — لا طبقة عرض (مستبعد مع السبب). `ui/src/hooks/*` و`ui/src/main.tsx` ليست مستبعدة: حصرت أنماطًا فقط (حصر-بحث فقط).
- شعار `react.svg`/أصول العلامة — استثناء.
- تصدير Excel (`AccountingReportPrintPage`، `QuarterDetailsPage` ARGB) — لا يوجد pipeline رموز.
- `currentColor/inherit/transparent` — استثناءات مقبولة.

### حالات الفحص الأربع (موحّدة في كل التقرير)
- **مفحوص سياقيًا** — قُرئ الملف/المقطع مباشرة، والدليل مذكور بـ File:Line في جدول النتائج.
- **حصر-بحث فقط** — رصد أنماط عبر `rg`/agent دون قراءة سياقية كاملة.
- **لم يُفحص** — خارج نطاق فحص أنماط العرض (ملفات إعداد `ui/*`).
- **مستبعد مع السبب** — الاختبارات والقصص والأصول الثنائية و`api`+`types`.
الأعداد محسوبة من الملحق C: **606 = 67 مفحوص سياقيًا + 435 حصر-بحث فقط + 15 لم يُفحص + 89 مستبعد**.

### الملحق (Appendix): تغطية الملفات
ملخّص المجلدات في §الملحق، والجرد الكامل لكل ملف بحالته في الملحق C.

---

## 2. فجوات في ربط الرموز مع Tailwind (Corrected)

### 2.1 ألوان
| الرمز | مرجع Tailwind | الحالة | ملاحظة مُصححة |
|---|---|---|---|
| `--background/--foreground/--card/--popover/--primary/--secondary/--muted/--accent/--destructive/--border/--input/--ring` | أسماء Tailwind الافتراضية | ✅ | — |
| `--muted-foreground`، `--label-foreground`، `--table-foreground`، `--sidebar*` | أسماء Tailwind | ✅ | — |
| `--gov-success/--gov-warning/--gov-danger/--gov-info` (+bg) | Tailwind: `success`/`warning`/`brand`/... قيم ثابتة | ⚠️ | تستخدم أسماء دلالية (`bg-success`، `text-warning`…) لكن قيمها ثابتة لا تتبع `--gov-*`. لا يوجد alias لـ `--gov-success`/`--gov-danger`/`--gov-info`؛ الرموز معرّفة في `:root` فقط، **دون** نسخة `.dark` (انظر 2.3) |
| `--gov-primary-50/100/500/600/700/800/900` | **❌ لا alias** | فجوة | لا توجد تدرّجات `--gov-primary-200/300/400/950` — F-03 السابق لا يضمن تغطية كاملة |
| `success/warning/brand` | قيم ثابتة | ⚠️ قيم ثابتة | `success=#10b981`, `warning=#f59e0b`, `brand=#1b3a5c`؛ لا تتبّع `--gov-*` |
| `primary.50..950` | قيم ثابتة | ⚠️ قيم ثابتة | لا تتبّع `--primary` |
| `tremor.*` | قيم ثابتة | ⚠️ قيم ثابتة | — |
| `surface-print/table-header/border-print` | قيم ثابتة | ⚠️ | انظر 2.4 |
| `--dlg-*` | arbitrary value | ✅ | `p-[var(--dlg-section-p)]` مستخدم في BaseFormDialog/Dialog |

### 2.2 الخطوط والأحجام
| الرمز | مرجع Tailwind | الحالة | ملاحظة مُصححة |
|---|---|---|---|
| `--font-sans/--font-mono` | `font-sans/font-mono` | ✅ | — |
| `--font-xs..--font-4xl` | `text-xs/…` | ✅ | — |
| `--label-*` | `font-label`, `text-label` | ✅ | — |
| `fontSize.micro/xsmall/caption/caption-md/body-sm/body/body-md/body-lg` | `text-micro…body-lg` | ⚠️ قيم ثابتة | `(8px,12px)…(15px,21px)`، لا تتبع `--font-*` |
| أحجام اعتباطية | — | ❌ | `text-[Npx]`/`text-[Nrem]` منتشرة (105 ملفًا تحت `ui/src` عدا الاختبارات/الأصول) |

### 2.3 الحركة والطبقات (P-1, P-4, P-22 مُصحّحة)
| الرمز | الاستخدام الحالي | ملاحظة مُصححة |
|---|---|---|
| `--transition-fast/base/slow/bounce` | **غير مربوطة** | قيمها **تتضمّن المدة والتخفيف معًا** (`150ms cubic-bezier(...)`)؛ لا يمكن زرعها مباشرة في `transitionDuration`. الخيارات الصحيحة: (أ) تقسيمها إلى `--transition-duration-*` زمنيًا و`--transition-easing-*` منفصلًا ثم ربط `transitionDuration`/`transitionTimingFunction`، أو (ب) استخدامها عبر `transition: <prop> var(--transition-base)`. التوصية السابقة بزرعها في `transitionDuration` **خاطئة** → **P-1 مُصحّح**. |
| `--z-base/dropdown/sticky/fixed/modal-backdrop/modal/popover/tooltip/toast` | لا ربط صريح | See P-2. |
| `zIndex.dialog=9999`, `dropdown=10000` في config | ثوابت | Select يستخدم `z-dropdown` عبر Portal داخل Dialogs → dropdown أعلى من dialog في الإعداد الحالي. ربط أعمى `dropdown→--z-dropdown(100)` و`dialog→--z-modal(500)` قد يضع Select خلف Dialog. يجب إبقاء المستويين ضمن **سياق الـ portal** أعلى من الحوار → **P-2 مُصحّح**. |

### 2.4 الطباعة (P-2 مُصحّح)
`print.css` يستخدم `var(--brand)` `var(--border-print)` `var(--table-header)` **غير المعرّفة** في `:root`؛ هذه الأسماء موجودة فقط كقيم ثابتة في `tailwind.config.js`. النتيجة: `border: 1px solid var(--border-print)` تُحل إلى قيمة غير صالحة (invalid at computed-value time) فتُسقط القاعدة. بالإضافة، `DocumentsTab.tsx:252-260` ينشئ مستندًا منفصلًا بـ `window.open` + `document.write` ويكتب `background: var(--bg-card)` (مرجع غير معرّف؛ التعريف الصحيح هو `--card` من ضمن triple HSL، فيجب استخدامه كـ `background: hsl(var(--card))`)، ولا يرث ذلك المستند متغيرات `:root` → **P-42 مُصحّح**: أي إصلاح يجب أن يحقن الـ variables داخل المستند الجديد أو يكتب قيمًا ثابتة.

---

## 3. جدول النتائج (Findings) — مُصحَّح

ترميز: P1 حرج، P2 مرتفع، P3 متوسط، P4 بسيط.
كل سطر يحمل: **الحالة** (`confirmed`/`corrected`/`withdrawn`/`summary`)، وثقة الرصد المصدري × ثقة الأثر البصري. `summary` = ملخّص مرتبط بنتيجة أساسية (لا يُحسب كمخالفة مستقلة)، `withdrawn` = سُحب لعدم كفاية الدليل.

| ID | Prio | الحالة | ثقة (رصد×أثر) | File:Line | Page/Component | Current Code | Classification | Corrected Recommendation | Expected Impact | Affected Pages |
|---|---|---|---|---|---|---|---|---|---|---|
| F-01 | P2 | corrected |High×Med| `ui/src/index.css:1057-1067` | نظام عام | `.text-slate-300,400,500,600, .text-gray-300,400,500,600, .dark .text-slate-300,400,500,600, .dark .text-gray-300,400,500,600 { color: hsl(var(--foreground)) !important; } .text-slate-700,800 .text-gray-700,800 { color: hsl(var(--foreground)) !important; }` | قاعدة تفرض `--foreground` على **درجات محددة فقط** (300-600، +700/800 بدون dark) من `text-slate-*`/`text-gray-*` — **السابق بالغ في التعميم**. `bg-slate-*`/`border-slate-*`/`text-gray-50/100/200/900` لا يطالها القاعدة | إما الإبقاء والاعتراف بأنها طبقة override مُتعمّدة، أو إزالةً تدريجية مع قياس contrast بعد كل استبدال. لا يضمن حذفها contrast كافٍ دون قياس → **needs verification** بعد الانتقال | قد يُغيّر لون كثير من النصوص من «أسود forced» إلى intent؛ contrast غير مؤكّد | كل الصفحات |
| F-02 | P1 | confirmed |High×High| `ui/src/styles/print.css:14-61` | طباعة | `border: 1px solid var(--border-print); background: var(--table-header); color: var(--brand);` | **فجوة تعريف**: المتغيرات غير معرّفة في `:root` (فقط قيم ثابتة في config) → الحلقات والخلفيات تُسقط → أثر على تنسيق الجداول | تعريف الثلاثية صراحة في `:root` (أو حقن قيم ثابتة في CSS المولّدة) → ثم قياس ظهورها في معاينة الطباعة | كسر حدود/خلفيات/ألوان جداول الطباعة | كل صفحات الطباعة |
| F-03 | P2 | corrected |High×Med| `ui/tailwind.config.js` | نظام عام | `success/warning/brand`، `primary.50..950`، `tremor.*`، `surface-print/table-header/border-print` قيم ثابتة | **فجوة ربط**. التصحيح: لا يتم وعد الوضع المظلم لأن `--gov-*` **بلا `.dark` overrides** ولأن `--gov-primary-*` ناقص (لا 200/300/400/950) | (1) إضافة `.dark` overrides لـ `--gov-*`، (2) استكمال تدرّج `--gov-primary-*`، (3) ثم ربط `tailwind.config.js` بـ `hsl(var(--gov-success))`… | تناسق الهوية، dark mode لاحقًا — **لا تضمن dark قبل خطوة (1)** | عام |
| F-04 | P2 | corrected |High×Med| `ui/tailwind.config.js` (zIndex) | نظام عام | `dialog=9999`, `dropdown=10000`, `notification=110`, `backdrop=-1`, `print-*` | ثوابت؛ **Select/DropdownMenu يستخدمان Portal** ويحتاج dropdown أعلى من الحوار داخله | بدلًا من الربط الأعمى بـ `--z-*` الحالية، عرّف `--z-dropdown` بحيث `--z-dropdown > --z-modal`، أو استخدام `z-modal+1` للـ portal. لا يُربط `dropdown=100` و`dialog=500` دون مراجعة سياقات portal | منع اختفاء Dialog/Select خلف بعضهما | Dialog/Popover/Sheet/Loader/Table |
| F-05 | P2 | corrected |High×Med| `ui/tailwind.config.js` | نظام عام | لا `transitionDuration` مرتبط | المدة+التخفيف مدموجتان في `--transition-*` (`index.css:318-321`) — لا تصلح مباشرة في `transitionDuration` | قسّم: `--transition-duration-*` (150/200/300ms) و`--transition-easing-*` ثم اربط Tailwind، **مع تحديث `@media (prefers-reduced-motion)` في `index.css:407-413` ليضبط المدد الجديدة أيضًا** (وإلا يضيع تفضيل الحركة) | توحيد الحركة مع الحفاظ على reduced-motion | عام |
| F-06 | P2 | confirmed |High×Med| `ui/src/lib/chartColors.ts:6-21` | رسوم | `primary: "hsl(215, 75%, 36%)"`… | لوحة ثابتة تُسقَط في `chart.tsx:8` (`--color-${key}`). لا تتبع `--gov-*` | اشتقاق من `hsl(var(--primary))`، `--gov-success`، `--gov-warning`، `--gov-danger` (مع تحديد 2-3 ألوان سلسلة جديدة كـ tokens إن لزم) | توحيد الرسوم | صفحات التقارير |
| F-07 | P2 | confirmed |High×Med| `ui/src/constants/inventory.ts:13-27` | جرد | `TRANSACTION_TYPE_GRADIENTS`/`ACCENT_COLORS` بألوان ثابتة | خريطة حالة→ألوان ثابتة؛ no dark | استبدال بمراجع `--gov-*-bg`/`--gov-*` أو ألوان domain جديدة | تناسق dark | الجرد |
| F-08 | P2 | confirmed |High×Med| `ui/src/constants/index.ts:248-253`، `ui/src/lib/utils.ts:75-86` | مستندات | `DOC_STATUS_COLOR = { uploaded:"text-green-600", … }` و`getStatusColorClass` تُعيد `text-blue-600 dark:text-blue-400` / `text-emerald-600…` / `text-slate-500…` | خريطتان متوازيتان (حالة→لون ثابت) بنفس التصنيف؛ `purple`/`sky` بلا token | `--gov-success/--gov-danger/--gov-warning` + رمز جديد للدرجات الناقصة، ثم استبدال الخريطتين معًا | توحيد شارات الحالة | مستندات/سجلات |
| F-09 | P2 | confirmed |High×Med| `ui/src/components/domain/admin/permission-utils.ts:33-58` | صلاحيات | خريطة ~26 شارة ثابتة | fixed chips | خريطة دلالية محدودة بـ `--gov-*` أو role tokens جديدة | تناسق | الصلاحيات |
| F-10 | P2 | confirmed |High×Med| `ui/src/components/notifications/FloatingNotification.tsx:18-23,123,195` | إشعارات | gradient/accent ثابتة + `duration-[50ms]`/`duration-500` | ثابت + مدد غير token | `--gov-*` + `--transition-*` (بعد F-05) | تناسق | الإشعارات |
| F-11 | P2 | confirmed |High×Med| `ui/src/components/ui/AlertBanner.tsx:36-52` | تنبيهات | `bg-amber-50 border-amber-200 text-amber-800`… | soft-status غير موجود | إضافة `--gov-*-bg` aliases واستخدامها | توحيد التنبيهات | كل الصفحات |
| F-12 | P2 | confirmed |High×Med| `ui/src/components/ui/badgeVariants.ts` | شارات | `bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 …` | soft-status غير موجود | نفس F-11 | توحيد الشارات | عام |
| F-13 | P2 | corrected |High×Med| `ui/src/components/ui/StatCard.tsx:24-40,58,83,88` | KPI | خرائط `blue/orange/emerald/…`، `dark:` variants موجودة، `muted` variant يستخدم `bg-muted` | **التصحيح**: لا يُسمح بالادعاء بأنها «لا تدعم dark» — فهي تحمل `dark:text-blue-400` إلخ + variant مُسمّاة `muted` يستخدم `--muted`. المشكلة: تنشر لوحة ثابتة بدل `--gov-*` | ربط variant الحاسمة (`success`→`--gov-success`، `warning`→`--gov-warning`، `destructive`→`--gov-danger`) بـ tokens، وإبقاء `blue/orange/…` كهيئة امتداديّة إن وُجدت حاجة منتجيّة | off-token ثم on-token تدريجيًا | عام |
| F-14 | P2 | confirmed |High×Med| `ui/src/components/ui/NoteBox.tsx:36-77` | ملاحظات | `bg-blue-50/40 … bg-slate-50/60 border-slate-200/60` | soft-status فجوة | tokens | توحيد | عام |
| F-15 | P2 | confirmed |High×Med| `ui/src/components/ui/ErrorAlert.tsx:26-36` | أخطاء | `bg-red-50/60 border-red-200/60 text-red-900` | فجوة soft-destructive | `--gov-danger-bg` alias | توحيد | نماذج |
| F-16 | P2 | confirmed |High×Med| `ui/src/components/ui/Stepper.tsx:67-177` | خطوة | `bg-emerald-500 … duration-500` | ثابت + مدة غير token | success token + `--transition-*` | توحيد | معالجات |
| F-17 | P2 | confirmed |High×Med| `ui/src/components/ui/AuditTimeline.tsx:27-170` | تدقيق | `bg-emerald-500/10 …` | فجوة soft-status | tokens | توحيد | التدقيق |
| F-18 | P2 | confirmed |High×Med| `ui/src/components/ui/RecordStateBadge.config.ts:28-58` | حالة سجل | `bg-amber-50 text-amber-700 border-amber-200 …` | فجوة soft-status | tokens | توحيد | السجلات |
| F-19 | P2 | confirmed |High×Med| `ui/src/components/ui/Badge.tsx:26-29` | شارة | `bg-emerald-500/bg-amber-500/bg-red-500/bg-blue-500` | لا مرادَفات دلالية | `bg-success/bg-warning/bg-destructive/bg-primary` | توحيد | عام |
| F-20 | P2 | confirmed |High×Med| `ui/src/components/ui/DropdownMenu.tsx:44-45` | قوائم | `text-black font-bold focus:text-black` | نص ثابت | `text-popover-foreground` | dark | القوائم |
| F-21 | P2 | confirmed |High×Med| `ui/src/components/ui/Popover.tsx:19`، `ui/src/components/ui/Sheet.tsx:21,35`، `ui/src/components/ui/Loader.tsx:142`، `ui/src/components/ui/DropdownMenu.tsx:21`، `ui/src/components/ui/DataTable.tsx:394` | طبقات | `z-50` في مكوّنات Portal/fixed، و`z-10` لرأس جدول لاصق | انظر F-04 (تباين `--z-*` مع `zIndex` في config) | ربط بعد مراجعة سياقات portal | اتساق | الواجهات ذات الطوابق |
| F-22 | P3 | corrected |High×Med| `ui/src/components/ui/Loader.tsx:73` | مؤشر تحميل | `animate-[pulse_2s_ease-in-out_infinite]` | **التصحيح**: وجود `prefers-reduced-motion` عامًا في `index.css:407-422` يجبر كل الأنيميشنات على 0.01ms → لا تدّعي أنها تفشل احترام تفضيلات المستخدم. المشكلة: **تجاوز** مقاييس الحركة المعمّمة ويصعّب تطبيق policy مستقبلًا | استخدام `--transition-*`/`--animate-*` tokens | قابلية صيانة | عام |
| F-23 | P3 | confirmed |High×Med| `ui/src/components/ui/FormMessage.tsx:16` | رسائل | `text-[0.8rem]` | لا يطابق `--font-*` | `text-sm` أو alias | اتساق | نماذج |
| F-24 | P3 | confirmed |High×Med| `ui/src/components/ui/DataTable.tsx:419,167,599` | جداول | `text-[13px]`, `text-[10px]` | أحجام اعتباطية | `text-sm`/`text-caption` (بعد تثبيت `--font-*`) | اتساق | DataTable |
| F-25 | P2 | confirmed |High×Med| `ui/src/components/ui/Dialog.tsx:358`، `ui/src/components/ui/Sheet.tsx:21`، `ui/src/components/ui/CornerEditor.tsx` (انظر F-55) | حوارات | `bg-black/50`، `bg-black/80` | لا token للـ overlay | `--overlay` token (new) | توحيد | حوارات |
| F-26 | P3 | corrected |High×Unknown| `ui/src/components/ui/chart.tsx:31` | رسوم | `[&_.recharts-grid_line[stroke='#ccc']]:stroke-border/50` | **التصحيح**: `#ccc` هنا قيمة مطابقة لعنصريات Recharts الافتراضية (selector matching)، بينما اللون المطبَّق فعليًا هو `stroke-border/50` (token). لو تم الـ override بنجاح فاللون النهائي token-backed. لا يُصنَّف كخرق بصري مباشر؛ يُصنّف كـ coupling هشّ لإصدار Recharts | لا تعديل إلزامي؛ يمكن الانتقال إلى selector attribute `stroke` ثابت أو قيم default مباشرة مع `stroke-border/50`. **verification-required** إن تغيّر سلوك Recharts | fragility, not visual bug | الرسوم |
| F-27 | P3 | confirmed |High×Med| `ui/src/components/ui/CornerEditor.tsx:241,246,258` | محرر | `ctx.fillStyle = "rgba(0,0,0,0.5)"`, `ctx.strokeStyle = "#3b82f6"`, `colors = ["#ef4444", ...]` | ألوان ثابتة في Canvas | قراءة `--primary` عبر `getComputedStyle(...).getPropertyValue("--primary")` تعيد HSL triple (`215 85% 22%`) ولا تصلح مباشرة لـ `ctx.strokeStyle`. الصيغة الصحيحة: تغليفها كـ `hsl(var(--primary))` (JavaScript: `hsl(${value})`)، وإعادة رسم Canvas عند تغيّر الثيم إذا طُلب دعم التبديل الحي | dark | الزوايا |
| F-28 | P2 | confirmed |High×Med| `ui/src/components/ErrorBoundary.tsx:51` | خطأ | `bg-red-100 dark:bg-red-950/50` | soft-destructive فجوة | tokens | تناسق | عام |
| F-29 | P2 | confirmed |High×Med| `ui/src/components/auth/PermissionRoute.tsx:31,58` | صلاحيات | `text-red-500`, `bg-red-100` | فجوة | tokens | توحيد | Routes |
| F-30 | P2 | confirmed |High×Med| `ui/src/pages/auth/LoginPage.tsx:31,38,168-233`، `ui/src/pages/auth/ChangePasswordPage.tsx` | مصادقة | hero بلون ثابت، `bg-white/5 backdrop-blur-xl border-white/10 text-white … from-primary-950 via-primary-900 to-primary-800` | ثوابت | `--gov-hero` (new) | هوية | Auth |
| F-31 | P2 | confirmed |High×Med| `ui/src/pages/accounting/CashFlowPage.tsx:105-258` | محاسبة | `text-green-600/red-600` متكررة | فجوة | `--gov-success/--gov-danger` | توحيد | المحاسبة |
| F-32 | P2 | confirmed |High×Med| `ui/src/pages/accounting/QuarterDetailsPage.tsx:305-376` | محاسبة | `bg-green-100 text-green-600`، `bg-blue-100 text-blue-700`، `text-red-700` | فجوة | tokens | توحيد | التقارير |
| F-33 | P2 | confirmed |High×Med| `ui/src/components/domain/accounting/TrialBalanceReport.tsx:246,256,275-276,303-304,331`، `ui/src/components/domain/accounting/AccountStatementReport.tsx:84-85`، `ui/src/components/domain/accounting/BalanceSheetReport.tsx:128-131`، `ui/src/components/domain/accounting/CostCenterReport.tsx:57-58` | تقارير | `text-blue-700`/`text-green-700`، `bg-blue-100 text-blue-700`/`bg-green-100 text-green-700`، `bg-green-50 border-green-200`، وألوان `color` لبطاقات KPI | فجوة (لوحة ثابتة في نصوص وKPI — نمط KPI انظر F-56) | `--gov-success`/`--gov-danger`/`--gov-warning` أو مفاتيح دلالية (F-56) | توحيد | التقارير المحاسبية |
| F-34 | P2 | confirmed |High×Med| `ui/src/pages/hr/HrDashboardPage.tsx:92-140` | HR | `bg-orange-500/10 text-orange-500`، `bg-emerald-500/10 text-emerald-500` | فجوة | tokens | توحيد | HR |
| F-35 | P2 | confirmed |High×Med| `ui/src/pages/finance/TreasuryPage.tsx:112-197` | Finance | `bg-emerald-100 … text-red-600` | فجوة | tokens | توحيد | Finance |
| F-36 | P2 | confirmed |High×Med| `ui/src/pages/settlements/SettlementBatchDetailsPage.tsx:195-415` | تسويات | `text-green-600/amber-600` | فجوة | tokens | توحيد | تسويات |
| F-37 | P2 | confirmed |High×Med| `ui/src/pages/pension/FileReviewListPage.tsx:155-451` | Pension | `text-emerald-600/blue-600/red-600` | فجوة | tokens | توحيد | Pension |
| F-38 | P2 | confirmed |High×Med| `ui/src/pages/ministry-approvals/ReconciliationTab.tsx:84-103` | موافقات | `text-rose-600/emerald-600`، `amber-50/200/900/400` | فجوة | tokens | توحيد | موافقات |
| F-39 | P2 | confirmed |High×Med| `ui/src/pages/inventory/StocktakePage.tsx:134-483` | الجرد | `bg-green-600`، `text-yellow-600`، `text-red-600` | فجوة | tokens | توحيد | الجرد |
| F-40 | P2 | confirmed |High×Med| `ui/src/pages/investment/PropertiesPage.tsx:88,101,122,127`، `ui/src/pages/investment/PortfoliosPage.tsx:76,100,103` (+28 ملفًا آخر تحت `ui/src/pages/investment/`، الجرد في الملحق C) | استثمارات | `text-emerald-600`، `text-blue-600`، `text-red-600` في خلايا الحالة/الإجراءات | نمط منتشر؛ مُثبت بالسطور أعلاه وبحصر الملحق C | ربط بـ `--gov-success`/`--gov-danger`/`--gov-warning` | توحيد | الاستثمار |
| F-41 | P2 | confirmed |High×Med| `ui/src/pages/retirement-files/tabs/ReviewTab.tsx:161-474` | مراجعة | `text-amber-600/emerald-600/red-600`، `bg-amber-600` بجانب `bg-destructive` | فجوة | tokens | توحيد | المراجعة |
| F-42 | P2 | corrected |High×Med| `ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:263-805` | مستندات | `<style>#e5e7eb,#0066cc,#6b7280,#111827</style>` + `window.open` + `document.write` + `var(--bg-card)` | **التصحيح**: المستند الجديد لا يرث متغيرات `:root`؛ `--bg-card` غير مُعرّف (التعريف الصحيح `--card` بما أن قيمته HSL ثلاثية، فالصيغة `background: hsl(var(--card))`). استبدال الـ hex بـ `var(--…)` داخل `document.write` يتطلب حقن التعريفات داخل المستند الجديد | حقن `<style>:root{--brand…;--border…;--card…}</style>` في `document.write`، أو كتابة قيم ثابتة. استبدال `--bg-card` بـ `hsl(var(--card))` داخل ذلك المستند | تناسق الطباعة | الملفات |
| F-43 | P3 | confirmed |High×Med| `ui/src/pages/DashboardPage.tsx:68-76,390-391,506-525` | لوحة | خريطة actionStyle ثابتة + `color: "hsl(346, 84%, 60%)"` | خرائط ثابتة + ألوان chart مكررة | tokens + `chartColors` | توحيد | Dashboard |
| F-44 | P3 | confirmed |High×Med| `ui/src/components/domain/retirement/DocumentUploadView.tsx:214-603` | رفع مستندات | `bg-rose-100 text-rose-600` + صفوف بيضاء | فجوة + `bg-white` بدل `--card` | tokens + `--card` | توحيد | الرفع |
| F-45 | P3 | confirmed |High×Med| `ui/src/pages/print/` (15 ملفًا — قائمة الأسماء في الملحق C)، مثال `ui/src/pages/print/PaymentBookPrint.tsx:260,438,736` | طباعة | `bg-slate-100/200`، `text-slate-500`، `print:bg-gray-100`، `border-black`، تدرجات hex (`#e3effc…#b0d0f5`، `#1a3a6b`/`#2563eb`) | جزئيًّا استثناء طباعة، وجزء فجوة `--border-print`/`--brand` (F-02) | توحيد عبر تعريف CSS vars | تناسق | الطباعة |
| F-46 | P3 | **withdrawn** |—×—| لا يوجد (انظر التبرير) | — | — | **مسحوب في هذه الجولة**: لم يُثبت بملف/سطر — لا يوجد مُقتطف قابل للتحقق، والنمط المُفترض مغطّى أصلًا بـ F-09/F-33/F-44 | لا توصية (يُعاد التسجيل بمعرّف جديد فقط إن ظهر دليل File:Line) | لا أثر محسوب | — |
| F-47 | P3 | confirmed |High×Med| `ui/src/components/ui/print/PrintableReportTable.tsx:76` | طباعة | `bg-brand text-white` | `brand` ثابت | تحويل `brand` إلى رمز (بعد F-03) | تناسق | الطباعة |
| F-48 | P4 | **summary** |linked| `ui/src/components/ui/Dialog.tsx:357,380,407`، `ui/src/components/ui/DataTable.tsx:315,403`، `ui/src/components/ui/Loader.tsx:142` | عام | `duration-200/300/500`، `ease-out` | **ملخّص مرتبط بـ F-05** — ليس مخالفة مستقلة؛ لا يُحسب مع F-05/F-10/F-16/F-54/F-57 كتكرار | انظر F-05 (قسّم المدة عن التخفيف ثم اربط Tailwind) | توحيد | عام |
| F-49 | P4 | **summary** |linked| `ui/src/components/ui/FormMessage.tsx:16`، `ui/src/components/ui/DataTable.tsx:167,419,599` | عام | `text-[0.8rem]`، `text-[13px]`، `text-[10px]` | **ملخّص مرتبط بـ F-23 وF-24** — ليس مخالفة مستقلة | انظر F-23/F-24 | اتساق | النماذج/الجداول |
| F-50 | P4 | **summary** |linked| `ui/src/components/ui/Popover.tsx:19`، `ui/src/components/ui/Sheet.tsx:21,35`، `ui/src/components/ui/DropdownMenu.tsx:21`، `ui/src/components/ui/Loader.tsx:142` | عام | `z-50` | **ملخّص مرتبط بـ F-04 وF-21** — ليس مخالفة مستقلة | انظر F-04/F-21 | اتساق | الواجهات ذات الطوابق |
| F-51 | P2 | confirmed |High×Med| `ui/src/components/ui/print/PrintField.tsx:18-40` | طباعة | `border-gray-200 … even:bg-gray-50/50 print:border-black`، `text-[10px]/[11px]/[12px]`، `print:text-gray-700` | `border-gray-*`/`bg-gray-*` لا تُغطّى بقاعدة `!important` (F-01)؛ `print:` يعيد إعلان ثوابت → تطبيق غير محكم | `--border`/`--muted`/`--foreground` + `--border-print` في طباعة | تناسق | الطباعة |
| F-52 | P2 | confirmed |High×Med| `ui/src/components/ui/print/PrintLayout.tsx:220,268-269,292` | طباعة | `border: 4px double #000`, `background: #fff`, `box-shadow: 0 5px 15px rgba(0,0,0,0.2)`, `color: #000` | ثوابت داخل `<style>`؛ لا تتبّع tokens | تعريف `--print-fg`/`--print-bg`/etc. داخل print أو استبدال بـ ثابت موحّد | معاينة طباعة متسقة | الطباعة |
| F-53 | P2 | confirmed |High×Med| `ui/src/components/ui/print/PrintSectionHeader.tsx:14,23`، `ui/src/components/ui/print/PrintSignatureBlock.tsx:52`، `ui/src/components/ui/print/PrintableReportTable.tsx:115` | طباعة | `bg-gray-100 print:bg-gray-100`، `border-primary-600 print:border-black`، `bg-black text-white`، `bg-slate-50/60`، `text-gray-700` | فجوة تدرّج + `primary-600` ثابت (لا alias)؛ `text-gray-*` هنا يُغطّى جزئيًا بقاعدة F-01 لكن `bg-gray-*`/`border-primary-600` لا | tokens لـ `--border`/`--muted`/`--primary` + `--border-print` | تناسق | الطباعة |
| F-54 | P3 | confirmed |High×Med| `ui/src/components/notifications/FloatingNotification.tsx:123-181` | إشعارات | `border-white/10`, `hover:bg-white/10`, `bg-slate-950/95`, `text-slate-400`, `bg-slate-600`, `duration-500`/`duration-[50ms]` | ثوابت سطح/حدود لا تتبع `--card/--border`؛ مدة غير token | `--terminal`/`--card` + `--transition-*` | توحيد | الإشعارات |
| F-55 | P3 | confirmed |High×Med| `ui/src/components/ui/CornerEditor.tsx:439-450` | محرر | `bg-black/5`, `bg-black/60 text-white` | overlay ثابت | `--overlay` token | توحيد | الزوايا |
| F-56 | P3 | confirmed |High×Med| `ui/src/components/common/ReportKPICards.tsx:28` و`ui/src/components/common/AccountingReportLayout.tsx:86` (تعبير `kpi.color` مع افتراضي `text-primary`)؛ مُستهلكون بقيم ثابتة: `ui/src/components/domain/accounting/AccountStatementReport.tsx:84-85`، `ui/src/components/domain/accounting/TrialBalanceReport.tsx:303-304`، `ui/src/components/domain/accounting/BalanceSheetReport.tsx:128-131`، `ui/src/components/domain/accounting/CostCenterReport.tsx:57-58`؛ و`ui/src/pages/system-admin/UsersDirectoryPage.tsx:283` (نفس النمط محليًّا في سطر 287) | KPI | حقل `color` من نوع string يُحقن مباشرة في `className`؛ القيمة الافتراضية `text-primary` ✅ لكن المستهلكين يمرّران `text-blue-600`/`text-green-600`/`text-red-600`/`text-amber-600`/`text-purple-600` | نمط تمرير ألوان ثابت عبر حقل `color` (**صُحّح**: `pages/accounting/ChartOfAccountsPage.tsx` ليس مستهلكًا لـ ReportKPICards) | تحويل الحقل إلى semantic keys (`success`/`danger`/`warning`/`primary`) وتُترجم داخل ReportKPICards/AccountingReportLayout إلى tokens | توحيد التقارير (18 مكوّنًا تستخدم AccountingReportLayout) | التقارير المحاسبية |
| F-57 | P4 | corrected |High×Med| `ui/src/components/ui/Dialog.tsx:357,380,407`، `ui/src/components/ui/Accordion.tsx:36`، `ui/src/components/ui/Tabs.tsx:36` | حوارات/ألسنة | `duration-200` + `transition-all`؛ `rounded-md sm:rounded-lg` | `rounded-*` تتبع `--radius` ✅؛ المدة لا تتبع `--transition-*`. **التصحيح**: `Select.tsx` لا يحوي `duration-*` إطلاقًا (لم يُثبت) — حُذف من القائمة | انظر F-05 | حركة متسقة | عام |

---

## 4. تصحيحات واستبعادات (Summary of Changes)

- **P-1 / F-05**: غير صالح كما كان؛ `--transition-*` تتضمن مدة+تخفيف. التوصية المُصححة في F-05.
- **P-2 / F-04**: غير صالح كما كان؛ ربط مباشر `dropdown=100` خلف `dialog=500` يضع Select خلف الحوارات (Select عبر Portal). التوصية المُصححة في F-04.
- **P-3 / F-03**: لا وعد dark mode تلقائي؛ `--gov-*` بلا `.dark` overrides ولا يوجد تدرّج `--gov-primary-*` كامل. التوصية المُصححة في F-03.
- **P-4 / F-42**: `DocumentsTab` يكتب مستندًا منفصلًا؛ يجب حقن المتغيرات داخله؛ كما أن `var(--bg-card)` غير معرّف (الصحيح `hsl(var(--card))`).
- **P-5 / F-26**: `#ccc` في `chart.tsx` مطابقة selector لإصدار Recharts الافتراضي، واللون المطبَّق `stroke-border/50` — يُصنَّف coupling هشّ لا خرقًا بصريًا.
- **P-6 / F-13**: StatCard تحمل `dark:` variants و`muted` variant token-backed — التعميم «لا دعم dark» غير صحيح. المشكلة الآن محصورة في نشر لوحة ثابتة.
- **P-7 / F-22**: يوجد `* { transition-duration: 0.01ms !important; animation-duration: 0.01ms !important; }` في `index.css:407-422` → الفشل ليس في تجاهل تفضيلات المستخدم، بل في تجاوز مقاييس الحركة المعمّمة.
- **P-8 / F-01**: القاعدة تستهدف `text-slate-300/400/500/600` (+dark) و`text-gray-300/400/500/600` (+dark)، و`text-slate-700/800` و`text-gray-700/800` بدون dark. لا تمسّ `bg-slate-*`/`border-slate-*`/`text-gray-50/100/200/900`/`text-slate-900`. حذفها لا يضمن contrast مناسبًا دون قياس.
- **P-9**: 66 ملفًا تحت `components/ui`، 3 اختبارات فقط داخل `ui/src` (انظر الملحق).
- **P-10**: المراجع السابقة «components/domain/...» و«accounting/TrialBalanceReport» أصبحت explicit؛ التنبيه لـ «multiple files» استُبدل بقوائم في الملحق.
- **P-11 (جولة 3) / F-46**: **مسحوب** — لا يوجد ملف/سطر يُثبته، والنمط المُفترض مغطّى أصلًا بـ F-09/F-33/F-44؛ لا يُعدّ مخالفة مستقلة.
- **P-12 (جولة 3) / F-48, F-49, F-50**: أُعيد تصنيفها **ملخّصات** مرتبطة بـ F-05 / F-23+F-24 / F-04+F-21 — تُعرض كمواقع تكرار مُتحقَّقة، ولا تُحسب مع نتائجها كمخالفات منفصلة.
- **P-13 (جولة 3) / F-56**: سلسلة الدليل صُحّحت — `ChartOfAccountsPage.tsx` **ليس** مستهلكًا لـ ReportKPICards؛ السلسلة الحقيقية `AccountingReportLayout.tsx:86` ← `ReportKPICards.tsx:28` ← مُستهلكون بألوان ثابتة (قائمة F-56).
- **P-14 (جولة 3) / F-57**: `Select.tsx` لا يحوي `duration-*` (بحث مباشر) — حُذف؛ `Tabs.tsx:36` فيه `transition-all` بلا مدة صريحة.
- **P-15 (جولة 3) / F-08**: أُضيف `lib/utils.ts:75-86` (`getStatusColorClass`) كمصدر ثانٍ لنفس النمط (خرائط حالة→ألوان ثابتة).
- **P-16 (جولة 3) / §5**: نُقلت توصية soft-status (F-11..F-19) من «جاهزة» إلى §5b لأن `--gov-*-bg` بلا `.dark` overrides (F-03)؛ كل بند في §5 صار يحمل المتطلبات/النطاق/طريقة التحقق.

---

## 5. توصيات جاهزة للتنفيذ (مثبتة ساكنًا — بلا قرارات تصميم معلّقة)

كل توصية هنا: **المتطلبات السابقة** / **نطاق التغيير** / **طريقة التحقق**.

1. **إصلاح `print.css` (F-02)**
   - المتطلبات السابقة: لا شيء.
   - نطاق التغيير: `ui/src/styles/print.css:14-61` — تعريف `--brand`, `--border-print`, `--table-header` في `:root`، أو استبدال `var(--…)` بقيم ثابتة داخل نفس الملف.
   - التحقق: معاينة طباعة صفحة جدول (print preview) — هل الحدود والرؤوس تظهر.
2. **تقسيم وقت الحركة عن التخفيف (F-05)**
   - المتطلبات السابقة: لا شيء، **لكن يجب** تحديث `@media (prefers-reduced-motion)` في `index.css:407-413` في نفس التغيير وإلا يضيع تفضيل الحركة.
   - نطاق التغيير: `ui/src/index.css:318-321,407-413` + `ui/tailwind.config.js` (`transitionDuration`/`transitionTimingFunction`).
   - التحقق: `npm run build` يمرّ؛ حركة `Dialog`/`DataTable` 200ms؛ مع تفعيل «تقليل الحركة» تصبح 0.01ms.
3. **استبدال `--bg-card` غير المعرّف (F-42)**
   - المتطلبات السابقة: لا شيء (الفعل: `hsl(var(--card))` لأن القيمة HSL ثلاثية).
   - نطاق التغيير: `ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:227,252,260`؛ مع حقن `<style>:root{…}</style>` داخل `document.write` لو أُريدت هوية متطابقة.
   - التحقق: فتح `window.open` للطباعة والتأكد من خلفية الجدول.
4. **ربط `chartColors` بمصادر حيّة (F-06)**
   - المتطلبات السابقة: لا شيء.
   - نطاق التغيير: `ui/src/lib/chartColors.ts:6-21` فقط (لا يمسّ `chart.tsx` — انظر F-26).
   - التحقق: رسم بياني واحد في صفحة تقارير: اللون يتبع `--primary` بعد تغيير الثيم.
5. **توحيد أحجام النصوص (F-23, F-24 — F-49 ملخّص لهما)**
   - المتطلبات السابقة: تثبيت أي مقاس يُعتمد من سلّم `--font-*` الموجود فعلًا (`index.css:296-304`).
   - نطاق التغيير: `ui/src/components/ui/FormMessage.tsx:16`، `ui/src/components/ui/DataTable.tsx:167,419,599`.
   - التحقق: مقارنة بصرية للنماذج/الجدول قبل/بعد (لا اختلاف بصري ملحوظ).
6. **تحويل ألوان KPI إلى مفاتيح دلالية (F-56)**
   - المتطلبات السابقة: لا شيء — السلسلة مُثبتة: `ReportKPICards.tsx:28` و`AccountingReportLayout.tsx:86` يحقنان `kpi.color` في `className` كما هو.
   - نطاق التغيير: `ui/src/components/common/ReportKPICards.tsx` + `ui/src/components/common/AccountingReportLayout.tsx:22,86` (ترجمة المفتاح → token) ثم 5 مُستهلكين بألوان ثابتة (قائمة F-56) + `UsersDirectoryPage.tsx:283` محليًّا.
   - التحقق: `TrialBalanceReport`/`BalanceSheetReport` تعرض نفس الاتجاه اللوني بعد التحويل؛ 18 مكوّنًا تستخدم `AccountingReportLayout` لا تتأثر سلوكًا.
7. **توحيد خرائط حالة الألوان (F-08)**
   - المتطلبات السابقة: لا شيء.
   - نطاق التغيير: `ui/src/constants/index.ts:248-253` + `ui/src/lib/utils.ts:75-86` (يُستبدل النمطان معًا).
   - التحقق: شارات/صفوف الحالة تحمل نفس درجات `--gov-*` بعد التغيير.
8. **نصّ قائمة منسدلة (F-20)**
   - المتطلبات السابقة: لا شيء — `--popover-foreground` معرّف ويحوي `.dark` override.
   - نطاق التغيير: `ui/src/components/ui/DropdownMenu.tsx:44-45` (`text-black` → `text-popover-foreground`).
   - التحقق: القائمة المنسدلة في الوضعين الفاتح والمظلم.

## 5b. قرارات تصميم/تغيّر سلوكي تتطلب مراجعة واعتمادًا

- **F-01**: إزالة أو إبقاء قاعدة `!important` الرمادية — قرار يعتمد على قياس contrast بعد الاستبدال. *نطاق*: `index.css:1057-1067`. *تحقق*: قياس APCA على صفحات حقيقية.
- **F-03**: إضافة `.dark` overrides لـ `--gov-*` واستكمال تدرّج `--gov-primary-*` ثم ربط `tailwind.config.js` — قرار تصميم. *نطاق*: `index.css` (`:root` 254-275، `.dark` 325-351) + `tailwind.config.js`. *تحقق*: تبديل الثيم على صفحة KPI.
- **F-04**: ترتيب `--z-*` الفعلي مع إبقاء portal-over-dialog — يعتمد على اختبار Dialogs مفتوحة. *نطاق*: `tailwind.config.js:120` + `Popover/Sheet/Loader/DataTable/DropdownMenu`. *تحقق*: فتح Select داخل Dialog.
- **F-22**: سياسة `prefers-reduced-motion` مقابل النية الحركية (القاعدة العالمية تكسر أي حركة مقصودة داخل نفس الـ `@media`) — قرار accessibility. *نطاق*: `index.css:407-422`. *تحقق*: تشغيل «تقليل الحركة» ومعاينة Loader/Stepper.
- **F-26**: تعديل selector المخططات — قرار صيانة تبعية Recharts (لا خرق بصري مباشر). *نطاق*: `ui/src/components/ui/chart.tsx:31`. *تحقق*: وجود `stroke='#ccc'` في DOM مع Recharts الحالي.
- **F-11..F-19, F-28, F-29 (soft-status)**: تحويل الخرائط إلى `--gov-*-bg` — **مشروط بـ F-03**: القيم الحالية `--gov-success-bg` إلخ. تُعرَّف في `:root` فقط (`index.css:269-273`) بلا override في `.dark` (325-351) → استخدامها الآن يعطي خلفية فاتحة في الوضع المظلم. *نطاق*: `AlertBanner/badgeVariants/ErrorAlert/NoteBox/Stepper/AuditTimeline/RecordStateBadge/Badge/StatCard/FloatingNotification/permission-utils/ErrorBoundary/PermissionRoute`. *تحقق*: تبديل الثيم على صفحة تحمل شارات.
- **F-45/F-47/F-51/F-52/F-53 (طباعة)**: توحيد ألوان الطباعة عبر tokens — قرار اعتماد tokens للطباعة (مرتبط بـ F-02، وF-47 ينتظر رمز `brand` من F-03). *نطاق*: `pages/print/*` (15) + `components/ui/print/*` (7 ملفات: 6 مكوّنات + index). *تحقق*: معاينة طباعة Light/Dark.

## 5c. توصيات جماعية (نمط واحد يغطّي عدة نتائج)

هذه النتائج كلها **نفس النمط** (استبدال لون/مدة/طبقة ثابتة بـ token) وتُنفَّذ كموجة واحدة؛ لكل مجموعة متطلبات/نطاق/تحقق:

- **خرائط وعناصر ثابتة (F-07, F-09, F-10, F-12, F-13, F-14, F-15, F-16, F-17, F-18, F-43, F-54)**: استبدال قيم ثابتة بـ `--gov-*`/`--transition-*`.
  - المتطلبات السابقة: لا شيء للألوان؛ **F-05** قبل المدد (F-10, F-16, F-54).
  - النطاق: `constants/inventory.ts:13-27`، `permission-utils.ts:33-58`، `FloatingNotification.tsx:18-23,123`، `badgeVariants.ts`، `AlertBanner.tsx:36-52`، `StatCard.tsx:24-40`، `NoteBox.tsx:36-77`، `ErrorAlert.tsx:26-36`، `Stepper.tsx:67-177`، `AuditTimeline.tsx:27-170`، `RecordStateBadge.config.ts:28-58`، `Badge.tsx:26-29`، `DashboardPage.tsx:68-76,506-525`، `FloatingNotification.tsx:123-181`.
  - التحقق: مسح `rg` لعدم بقاء الأنماط + تبديل الثيم.
- **صفحات/نطاقات موحّدة (F-30, F-31, F-32, F-33, F-34, F-35, F-36, F-37, F-38, F-39, F-40, F-41, F-44, F-55)**: استبدال `text/bg` ثابتة بـ tokens في صفحات Auth/Accounting/HR/Finance/Settlements/Pension/Inventory/Investment/Retirement/Documents.
  - المتطلبات السابقة: لا شيء (توصية لونية بحتة).
  - النطاق: الملفات المذكورة بـ File:Line في جدول F (كلها مفحوصة سياقيًا عدا ما هو موسوم «حصر» في الملحق C).
  - التحقق: `rg` للأنماط + معاينة صفحة لكل مجموعة.
- **طبقات/حواف/حركة متفرقة (F-21, F-25, F-27, F-48, F-50, F-57)**: ربط `--z-*`/`--overlay`/ألوان Canvas/`--transition-*`.
  - المتطلبات السابقة: **F-04** (طبقات) و**F-05** (مدد) قبل التنفيذ.
  - النطاق: `Popover/Sheet/Loader/DropdownMenu/DataTable`، `Dialog.tsx:358`، `CornerEditor.tsx:241-258,439-450`.
  - التحقق: فتح Dialog محمّل بـ Select/Popover؛ إعادة رسم CornerEditor بعد تبديل الثيم.
- **F-46**: مسحوب — لا ينفَّذ شيئًا (انظر P-11).
- **F-48/F-49/F-50**: ملخّصات — تُنفَّذ عبر F-05/F-23/F-24/F-04/F-21 ولا تُعدّ أعمالًا منفصلة.

## 6. حالات تحتاج تحققًا قبل التنفيذ

- F-01: contrast بعد إزالة القاعدة (قياس APCA) على صفحات حقيقية.
- F-04: سلوك Portal للـ Select/DropdownMenu داخل Dialog مفتوحة (preview واختبار فتحة قائمة داخل حوار).
- F-26: هل `stroke='#ccc'` موجود في DOM مع إصدار Recharts الحالي.
- F-22: تعارض القاعدة العالمية `prefers-reduced-motion` مع نية حركة تتطلب مدة أكبر من 0.01ms داخل نفس الـ `@media`.
- F-45/F-51/F-53: السلوك الفعلي لألوان الطباعة (`PrintLayout`, `PrintField`, `PrintSectionHeader`, `PaymentBookPrint`) في معاينة الطباعة Light/Dark.
- F-42: اختبار طباعة ملف بحقن `<style>:root{…}</style>` داخل `document.write`.
- ~~F-56: هل `color` يُمرّر كما هو~~ — **مُثبت في هذه الجولة**: يُمرَّر خامًا (`ReportKPICards.tsx:28`، `AccountingReportLayout.tsx:86`)؛ نُقل إلى §5 بند 6.

---

## الملحق — تغطية الملفات (ملخّص المجلدات)

> حالات الفحص الأربع (نفسها في الملحق C أدناه): **مفحوص سياقيًا** = قُرئ مباشرة والدليل في F-xx؛ **حصر-بحث فقط** = `rg`/agent دون قراءة سياقية؛ **لم يُفحص** = خارج نطاق فحص أنماط العرض؛ **مستبعد مع السبب** = اختبارات/قصص/أصول/`api`+`types`.
> ملاحظات الأعمدة مرتبطة بنتائج F-xx فقط؛ ملاحظة `—` تعني «لا نتيجة F مُسجّلة لهذه المجموعة» (لا تُدّعي نظافة).

### `ui/src` (أعلى المستوى)
| الملف | حالة الفحص | ملاحظة (F-xx) |
|---|---|---|
| `index.css` (1067 سطرًا) | مفحوص سياقيًا | F-01, F-05, F-22 |
| `main.tsx` | حصر-بحث فقط | — |
| `App.tsx` (43KB) | حصر-بحث فقط | — |

### `ui/src/components` (265)
| المسار | العدد | حالة الفحص | ملاحظة (F-xx) |
|---|---:|---|---|
| `components/ui/**` | 66 | مفحوص سياقيًا (الموصوفة بـ F-xx) + حصر-بحث فقط للبقية | F-11..F-26, F-47, F-51..F-57 |
| `components/ui/print/**` | ضمن 66 | مفحوص سياقيًا | F-47, F-51, F-52, F-53 |
| `components/ui/stories/**` | 1 | مستبعد مع السبب | Storybook غير منشور |
| `components/domain/**` | كثير | حصر-بحث فقط + مفحوص سياقيًا (F المُ citeée) | F-09, F-33, F-44 |
| `components/common/**` | متعدد | مفحوص سياقيًا | F-56 |
| `components/notifications/**` | متعدد | مفحوص سياقيًا | F-10, F-54 |
| `components/providers/**` | 4 | مفحوص سياقيًا (ThemeProvider) | — |
| `components/auth/**` | متعدد | مفحوص سياقيًا (PermissionRoute) | F-29 |
| `components/layout/**` | متعدد | حصر-بحث فقط | — |
| `ErrorBoundary.tsx` | 1 | مفحوص سياقيًا | F-28 |

### `ui/src/pages` (169)
| المسار | العدد | حالة الفحص | ملاحظة (F-xx) |
|---|---:|---|---|
| `pages/auth/**` | 2 | مفحوص سياقيًا | F-30 |
| `pages/account/**` | 1 | حصر-بحث فقط | — |
| `pages/admin/**` | 5 | حصر-بحث فقط | — |
| `pages/accounting/**` | 13 | مفحوص سياقيًا (جزئي) + حصر | F-31, F-32, F-56 (مستهلكون) |
| `pages/finance/**` | متعدد | مفحوص سياقيًا (Treasury) | F-35 |
| `pages/hr/**` | متعدد | مفحوص سياقيًا (HrDashboard) | F-34 |
| `pages/force/**` | متعدد | حصر-بحث فقط | — |
| `pages/settlements/**` | متعدد | مفحوص سياقيًا (SettlementBatchDetails) | F-36 |
| `pages/salaries/**` | متعدد | حصر-بحث فقط | — |
| `pages/suspensions/**` | متعدد | حصر-بحث فقط | — |
| `pages/pension/**` | متعدد | مفحوص سياقيًا (FileReviewList) | F-37 |
| `pages/ministry-approvals/**` | متعدد | مفحوص سياقيًا (ReconciliationTab) | F-38 |
| `pages/inventory/**` | متعدد | مفحوص سياقيًا (Stocktake) | F-39 |
| `pages/investment/**` | 30 | حصر-بحث فقط + مفحوص سياقيًا (Properties/Portfolios) | F-40 |
| `pages/retirement-files/**` (incl. tabs) | 22 | مفحوص سياقيًا (ReviewTab/DocumentsTab) | F-41, F-42 |
| `pages/retirees/**` | متعدد | حصر-بحث فقط | — |
| `pages/documents/**` | متعدد | حصر-بحث فقط | — |
| `pages/discrepancy/**` | متعدد | حصر-بحث فقط | — |
| `pages/print/**` | 15 | مفحوص سياقيًا (PaymentBookPrint) + حصر | F-45 |
| `pages/system-admin/**` | متعدد | مفحوص سياقيًا (UsersDirectoryPage) | F-56 |
| بقية `pages/*` (13 مجلدًا من 33) | متعدد | حصر-بحث فقط | — |

### `ui/src/lib` / `constants` / `styles` / `hooks` / `stores` / …
| المسار | حالة الفحص | ملاحظة (F-xx) |
|---|---|---|
| `lib/chartColors.ts` | مفحوص سياقيًا | F-06 |
| `lib/utils.ts` (`getStatusColorClass`) | مفحوص سياقيًا | F-08 |
| `constants/index.ts`, `constants/inventory.ts` | مفحوص سياقيًا | F-07, F-08 |
| `styles/print.css` | مفحوص سياقيًا | F-02 |
| `hooks/useConfirm.tsx`, `components/ui/ConfirmDialog.tsx` | مفحوص سياقيًا (فقط لتصنيف السياق — لا نتيجة F مستقلة) | — |
| `hooks/*`، `stores/*`، `utils/*`، `workers/*`، `lib` الباقية | حصر-بحث فقط | — |
| `api/*`, `types/*` | مستبعد مع السبب | لا طبقة عرض |
| `assets/**` (خطوط/react.svg)، صور `pages/retirees/image/*.png` | مستبعد مع السبب | أصول ثنائية |

### الاختبارات والقصص والمستبعدات
| المسار | العدد | الحالة |
|---|---:|---|
| `ui/tests/*.test.ts` | 3 | مستبعد مع السبب — اختبارات خارج `ui/src` |
| `ui/src/**/*.test.ts` (`date/homography/cpu-processor`) | 3 | مستبعد مع السبب — ليست عرضًا |
| `ui/src/components/ui/stories/DemoDialog.stories.tsx` | 1 | مستبعد مع السبب — Storybook |
| `node_modules/`, `dist/`, `.debug-harness/` | — | مستبعد مع السبب — مولّدة |

> **تنبيه منهجي**: لم تُقرأ كل صفحة من 169 صفحة ملفًا بملف؛ الاعتماد عبر `rg` patterns واسعة (حالة «حصر-بحث فقط»). الملفات المُدرجة في جدول F بمقتطفات (F-01, F-02, F-05, F-06, F-08, F-26, F-42, F-45, F-47, F-51..F-57) قُرئت مباشرة («مفحوص سياقيًا»)؛ مواقع صفوف F-48/F-49/F-50 (ملخّصات) تحققت عبر `rg` دون قراءة سياقية كاملة. الملحق C يعكس الحالة لكل ملف؛ لا يُدّعى أن كل ملف قُرئ نصًا.

<br>


## الملحق C — جرد الملفات (لكل ملف حالة الفحص)

أربعة حالات فحص مُميّزة (تُحسب الأعداد من القائمة نفسها أدناه):

- **مفحوص سياقيًا** — قُرئ الملف (أو مقطع منه) مباشرة والدليل مذكور في جدول F-xx.
- **حصر-بحث فقط** — رُصدت الأنماط عبر `rg`/agent دون قراءة سياقية كاملة؛ قد تحتوي أنماطًا خارج جدول النتائج.
- **لم يُفحص** — خارج نطاق فحص أنماط العرض (ملفات إعداد المشروع).
- **مستبعد مع السبب** — اختبارات/قصص/أصول ثنائية/`api`+`types` بلا طبقة عرض.

**الأعداد (محسوبة من القائمة أدناه):** الإجمالي 606 ملفًا — مفحوص سياقيًا 67، حصر-بحث فقط 435، لم يُفحص 15، مستبعد مع السبب 89.

```text
ui/.env : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/.env.production : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/.gitignore : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/components.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/eslint.config.js : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/eslint.dates.config.js : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/eslint.restricted-date-patterns.js : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/index.html : حصر-بحث فقط
ui/issues.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/package.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/package-lock.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/postcss.config.js : حصر-بحث فقط
ui/public.zip : مستبعد مع السبب - أرشيف مضغوط
ui/src/api/accounting.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/achievementObstacles.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/admin.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/archive.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/axios.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/banks.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/branches.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/budget.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/compliance.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/correspondence.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/director.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/discrepancy.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/documents.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/document-types.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/financial-entitlements.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/fixedAssets.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/hr.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/inventory/custody.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/inventory/items.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/inventory/stocktake.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/inventory/transactions.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/investment.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/ministry.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/notifications.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/pension.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/ranks.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/reports.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/retirees.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/retirement.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/salaries.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/salary-recipients.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/settlements.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/users.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/api/vouchers.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/App.tsx : حصر-بحث فقط
ui/src/assets/fonts/inter-v20-latin-500.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/inter-v20-latin-600.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/inter-v20-latin-700.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/inter-v20-latin-800.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/inter-v20-latin-900.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/inter-v20-latin-regular.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-Black.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-Bold.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-ExtraBold.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-ExtraLight.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-Light.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-Medium.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-Regular.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-SemiBold.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-kufi-arabic/NotoKufiArabic-Thin.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-sans-arabic-v33-arabic-500.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-sans-arabic-v33-arabic-600.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-sans-arabic-v33-arabic-700.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-sans-arabic-v33-arabic-800.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-sans-arabic-v33-arabic-900.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/fonts/noto-sans-arabic-v33-arabic-regular.woff2 : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/assets/react.svg : مستبعد مع السبب - أصول ثابتة (خطوط/شعار)
ui/src/components/auth/PermissionGate.tsx : حصر-بحث فقط
ui/src/components/auth/PermissionRoute.tsx : مفحوص سياقيًا
ui/src/components/auth/ProtectedRoute.tsx : حصر-بحث فقط
ui/src/components/common/AccountingReportLayout.tsx : مفحوص سياقيًا
ui/src/components/common/ActionButtons.tsx : حصر-بحث فقط
ui/src/components/common/charts/ChartToggle.tsx : حصر-بحث فقط
ui/src/components/common/charts/index.ts : حصر-بحث فقط
ui/src/components/common/charts/ReportBarChart.tsx : حصر-بحث فقط
ui/src/components/common/charts/ReportChartSection.tsx : حصر-بحث فقط
ui/src/components/common/charts/ReportLineChart.tsx : حصر-بحث فقط
ui/src/components/common/charts/ReportPieChart.tsx : حصر-بحث فقط
ui/src/components/common/charts/ReportSummaryCards.tsx : حصر-بحث فقط
ui/src/components/common/dashboards/DashboardHeader.tsx : حصر-بحث فقط
ui/src/components/common/dashboards/index.ts : حصر-بحث فقط
ui/src/components/common/FilterBar.tsx : حصر-بحث فقط
ui/src/components/common/index.ts : حصر-بحث فقط
ui/src/components/common/PrintSignatureBlock.tsx : حصر-بحث فقط
ui/src/components/common/PrintSummaryBox.tsx : حصر-بحث فقط
ui/src/components/common/ReportCard.tsx : حصر-بحث فقط
ui/src/components/common/ReportFilters.tsx : حصر-بحث فقط
ui/src/components/common/ReportKPICards.tsx : مفحوص سياقيًا
ui/src/components/common/RetirementFileReportViewer.tsx : حصر-بحث فقط
ui/src/components/common/ReviewFilePreview.tsx : حصر-بحث فقط
ui/src/components/common/SearchDialog.tsx : حصر-بحث فقط
ui/src/components/common/UnifiedRetireeStatement.tsx : حصر-بحث فقط
ui/src/components/common/UploaderStatsView.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/AccountFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/AccountStatementReport.tsx : مفحوص سياقيًا
ui/src/components/domain/accounting/AccountTreeView.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/BalanceSheetReport.tsx : مفحوص سياقيًا
ui/src/components/domain/accounting/BudgetFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/BudgetReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/BudgetReportView.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/CashFlowReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/CostCenterFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/CostCenterReport.tsx : مفحوص سياقيًا
ui/src/components/domain/accounting/CumulativePnLReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/CurrentOperationsReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/DepreciationReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/FiscalYearCloseDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/FiscalYearFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/FixedAssetsReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/GeneralLedgerReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/IncomeStatementReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/index.ts : حصر-بحث فقط
ui/src/components/domain/accounting/InitialOpeningBalanceDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/JournalEntryFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/JournalEntryPostDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/JournalEntryReverseDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/JournalEntryViewDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/JournalRegisterReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/LiabilitiesReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/ProvisionsReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/QuarterlyReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/SubledgerReport.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/TrialBalanceReport.tsx : مفحوص سياقيًا
ui/src/components/domain/accounting/VoucherFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/VoucherImportDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/VoucherPrintDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/VoucherSetAccountDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/VoucherViewDialog.tsx : حصر-بحث فقط
ui/src/components/domain/accounting/YearOverYearReport.tsx : حصر-بحث فقط
ui/src/components/domain/admin/BankFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/BranchDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/DocumentTypeDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/ExcelImportDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/index.ts : حصر-بحث فقط
ui/src/components/domain/admin/permission-utils.ts : مفحوص سياقيًا
ui/src/components/domain/admin/RankFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/RolePermissionsDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/SendNotificationDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/UserDialog.tsx : حصر-بحث فقط
ui/src/components/domain/admin/UserPermissionsDialog.tsx : حصر-بحث فقط
ui/src/components/domain/assets/AssetCategoryFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/assets/DepreciationRunDialog.tsx : حصر-بحث فقط
ui/src/components/domain/assets/FixedAssetFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/assets/index.ts : حصر-بحث فقط
ui/src/components/domain/correspondence/CorrespondenceColumns.tsx : حصر-بحث فقط
ui/src/components/domain/correspondence/CorrespondenceFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/correspondence/index.ts : حصر-بحث فقط
ui/src/components/domain/force/AddReferralItemDialog.tsx : حصر-بحث فقط
ui/src/components/domain/force/CreateForceBatchDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/AttendanceDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/CreateEmployeeDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/CreateOrderDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/DepartmentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/HrUploadDocumentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/index.ts : حصر-بحث فقط
ui/src/components/domain/hr/JobTitleDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/LeaveRequestDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/SectionDialog.tsx : حصر-بحث فقط
ui/src/components/domain/hr/ServiceHistoryDialog.tsx : حصر-بحث فقط
ui/src/components/domain/inventory/index.ts : حصر-بحث فقط
ui/src/components/domain/inventory/InventoryBulkTransactionFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/inventory/InventoryCustodyFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/inventory/InventoryCustodyReturnDialog.tsx : حصر-بحث فقط
ui/src/components/domain/inventory/InventoryItemFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/inventory/InventoryTransactionFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/AccountDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/AddCostDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/ApprovalRequestDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/ContractDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/DepreciationDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/index.ts : حصر-بحث فقط
ui/src/components/domain/investment/InvestmentDocsViewerDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/InvestmentDocumentsManager.tsx : حصر-بحث فقط
ui/src/components/domain/investment/InvestmentPlanDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/LookupDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/MaintenanceCostsTab.tsx : حصر-بحث فقط
ui/src/components/domain/investment/MaintenanceRequestDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/PaymentScheduleDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/PortfolioDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/ProjectDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/PropertyDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/RiskAssessmentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/TenantDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/TransactionDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/UnitDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/ValuationDialog.tsx : حصر-بحث فقط
ui/src/components/domain/investment/WorkOrderDialog.tsx : حصر-بحث فقط
ui/src/components/domain/print/excel.utils.ts : حصر-بحث فقط
ui/src/components/domain/print/index.ts : حصر-بحث فقط
ui/src/components/domain/print/print.types.ts : حصر-بحث فقط
ui/src/components/domain/print/print.utils.ts : حصر-بحث فقط
ui/src/components/domain/print/reportHeader.shared.ts : حصر-بحث فقط
ui/src/components/domain/retirement/AddFinancialEntitlementDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/AddGuardianMinorDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/AgentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/BulkUploadDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/CorrectLifeStatusDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/CreateFileWizard.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/DataEntryView.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/DocumentActions.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/DocumentChecklistField.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/DocumentProcessDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/DocumentReviewPanel.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/DocumentUploadView.tsx : مفحوص سياقيًا
ui/src/components/domain/retirement/DocumentViewerDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/EditDocumentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/EndFinancialEntitlementDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/EndRecipientDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/EndSuspensionDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/EntitledPersonDetails.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/ExemptionDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/FileReviewHeader.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/FileReviewSidebar.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/FulfillPledgeDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/GuardianDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/index.ts : حصر-بحث فقط
ui/src/components/domain/retirement/MissingDocActionDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/NoteDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/PaymentBookDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/PersonDocCard.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/PledgeDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/ReactivateEntitledPersonDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/RecipientsDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/RecordDeathDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/RetirementFileDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/RetirementFileSidebar.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/ReviewItemCard.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/ReviewPanel.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/sections/BasicInfoSection.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/sections/DocumentsSection.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/sections/FinancialSection.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/sections/HistorySection.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/sections/PeopleSection.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/StatusUpdateDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/StopEntitledPersonDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/UploadDocumentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/ViewAgentDialog.tsx : حصر-بحث فقط
ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx : حصر-بحث فقط
ui/src/components/domain/salaries/CreateSalaryBatchDialog.tsx : حصر-بحث فقط
ui/src/components/domain/salaries/EntitlementFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/salaries/EntitlementsTab.tsx : حصر-بحث فقط
ui/src/components/domain/salaries/index.ts : حصر-بحث فقط
ui/src/components/domain/salaries/RecipientsTab.tsx : حصر-بحث فقط
ui/src/components/domain/salaries/SalaryRecipientFormDialog.tsx : حصر-بحث فقط
ui/src/components/domain/salaries/SalaryRecipientViewerDialog.tsx : حصر-بحث فقط
ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx : حصر-بحث فقط
ui/src/components/domain/settlements/index.ts : حصر-بحث فقط
ui/src/components/domain/settlements/LinkRetireeDialog.tsx : حصر-بحث فقط
ui/src/components/domain/suspensions/CreateSuspensionDialog.tsx : حصر-بحث فقط
ui/src/components/ErrorBoundary.tsx : مفحوص سياقيًا
ui/src/components/layout/DetailLayout.tsx : حصر-بحث فقط
ui/src/components/layout/MainLayout.tsx : حصر-بحث فقط
ui/src/components/layout/NavDropdown.tsx : حصر-بحث فقط
ui/src/components/layout/navigationData.tsx : حصر-بحث فقط
ui/src/components/layout/NotificationBell.tsx : حصر-بحث فقط
ui/src/components/layout/PageLayout.tsx : حصر-بحث فقط
ui/src/components/layout/SecondaryNav.tsx : حصر-بحث فقط
ui/src/components/layout/Sidebar.tsx : مفحوص سياقيًا
ui/src/components/layout/TopNav.tsx : حصر-بحث فقط
ui/src/components/notifications/FloatingNotification.tsx : مفحوص سياقيًا
ui/src/components/providers/NotificationProvider.tsx : حصر-بحث فقط
ui/src/components/providers/OfflineDetector.tsx : حصر-بحث فقط
ui/src/components/providers/ThemeProvider.tsx : مفحوص سياقيًا
ui/src/components/providers/useSignalR.ts : حصر-بحث فقط
ui/src/components/ui/Accordion.tsx : مفحوص سياقيًا
ui/src/components/ui/AlertBanner.tsx : مفحوص سياقيًا
ui/src/components/ui/AlertDialog.tsx : حصر-بحث فقط
ui/src/components/ui/AuditTimeline.tsx : مفحوص سياقيًا
ui/src/components/ui/Badge.tsx : مفحوص سياقيًا
ui/src/components/ui/badgeVariants.ts : مفحوص سياقيًا
ui/src/components/ui/BaseFormDialog.tsx : حصر-بحث فقط
ui/src/components/ui/BaseSheet.tsx : حصر-بحث فقط
ui/src/components/ui/Breadcrumb.tsx : حصر-بحث فقط
ui/src/components/ui/Button.tsx : حصر-بحث فقط
ui/src/components/ui/buttonVariants.ts : حصر-بحث فقط
ui/src/components/ui/Card.tsx : حصر-بحث فقط
ui/src/components/ui/chart.tsx : مفحوص سياقيًا
ui/src/components/ui/Checkbox.tsx : حصر-بحث فقط
ui/src/components/ui/ConfirmDialog.tsx : مفحوص سياقيًا
ui/src/components/ui/CornerEditor.tsx : مفحوص سياقيًا
ui/src/components/ui/CrudPageLayout.tsx : حصر-بحث فقط
ui/src/components/ui/DataTable.tsx : مفحوص سياقيًا
ui/src/components/ui/Dialog.tsx : مفحوص سياقيًا
ui/src/components/ui/DropdownMenu.tsx : مفحوص سياقيًا
ui/src/components/ui/EditableImage.tsx : حصر-بحث فقط
ui/src/components/ui/EmptyState.tsx : حصر-بحث فقط
ui/src/components/ui/ErrorAlert.tsx : مفحوص سياقيًا
ui/src/components/ui/ErrorState.tsx : حصر-بحث فقط
ui/src/components/ui/FileField.tsx : حصر-بحث فقط
ui/src/components/ui/FileUploader.tsx : حصر-بحث فقط
ui/src/components/ui/FormDialogFooter.tsx : حصر-بحث فقط
ui/src/components/ui/FormField.tsx : حصر-بحث فقط
ui/src/components/ui/FormMessage.tsx : مفحوص سياقيًا
ui/src/components/ui/FormSection.tsx : حصر-بحث فقط
ui/src/components/ui/ImageCropperDialog.tsx : حصر-بحث فقط
ui/src/components/ui/index.ts : حصر-بحث فقط
ui/src/components/ui/InfoPair.tsx : حصر-بحث فقط
ui/src/components/ui/Input.tsx : حصر-بحث فقط
ui/src/components/ui/LazyQRCode.tsx : حصر-بحث فقط
ui/src/components/ui/Loader.tsx : مفحوص سياقيًا
ui/src/components/ui/LoadingIndicator.tsx : حصر-بحث فقط
ui/src/components/ui/NativeSelect.tsx : حصر-بحث فقط
ui/src/components/ui/NoteBox.tsx : مفحوص سياقيًا
ui/src/components/ui/PageHeader.tsx : حصر-بحث فقط
ui/src/components/ui/Popover.tsx : مفحوص سياقيًا
ui/src/components/ui/print/index.ts : حصر-بحث فقط
ui/src/components/ui/print/PrintableReport.tsx : حصر-بحث فقط
ui/src/components/ui/print/PrintableReportTable.tsx : مفحوص سياقيًا
ui/src/components/ui/print/PrintField.tsx : مفحوص سياقيًا
ui/src/components/ui/print/PrintLayout.tsx : مفحوص سياقيًا
ui/src/components/ui/print/PrintSectionHeader.tsx : مفحوص سياقيًا
ui/src/components/ui/print/PrintSignatureBlock.tsx : مفحوص سياقيًا
ui/src/components/ui/RadioGroup.tsx : حصر-بحث فقط
ui/src/components/ui/RecordStateBadge.config.ts : مفحوص سياقيًا
ui/src/components/ui/RecordStateBadge.tsx : حصر-بحث فقط
ui/src/components/ui/ScrollArea.tsx : حصر-بحث فقط
ui/src/components/ui/Select.tsx : مفحوص سياقيًا
ui/src/components/ui/Separator.tsx : حصر-بحث فقط
ui/src/components/ui/Sheet.tsx : مفحوص سياقيًا
ui/src/components/ui/Skeleton.tsx : حصر-بحث فقط
ui/src/components/ui/Slider.tsx : حصر-بحث فقط
ui/src/components/ui/StatCard.tsx : مفحوص سياقيًا
ui/src/components/ui/StatusBadge.tsx : حصر-بحث فقط
ui/src/components/ui/Stepper.tsx : مفحوص سياقيًا
ui/src/components/ui/stories/DemoDialog.stories.tsx : مستبعد مع السبب - قصص Storybook
ui/src/components/ui/SummaryCard.tsx : حصر-بحث فقط
ui/src/components/ui/TabbedFormSection.tsx : حصر-بحث فقط
ui/src/components/ui/Table.tsx : حصر-بحث فقط
ui/src/components/ui/Tabs.tsx : مفحوص سياقيًا
ui/src/components/ui/Typography.tsx : حصر-بحث فقط
ui/src/constants/assets.ts : حصر-بحث فقط
ui/src/constants/hr-event-types.ts : حصر-بحث فقط
ui/src/constants/index.ts : مفحوص سياقيًا
ui/src/constants/inventory.ts : مفحوص سياقيًا
ui/src/constants/queryKeys.ts : حصر-بحث فقط
ui/src/constants/retirement.ts : حصر-بحث فقط
ui/src/constants/shortcuts.ts : حصر-بحث فقط
ui/src/constants/statusConfigs.ts : حصر-بحث فقط
ui/src/hooks/index.ts : حصر-بحث فقط
ui/src/hooks/queries/index.ts : حصر-بحث فقط
ui/src/hooks/queries/useAccountsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useAgentQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useAgentsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useDocumentsByFileQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useEntitledPersonQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useEntitledPersonsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useFileDetailsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useFiscalYearsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useGuardianQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useGuardiansQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useMissingDocumentsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/usePeriodsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/usePersonDocumentsQuery.ts : حصر-بحث فقط
ui/src/hooks/queries/useReviewItemsQuery.ts : حصر-بحث فقط
ui/src/hooks/useApiMutation.ts : حصر-بحث فقط
ui/src/hooks/useConfirm.tsx : مفحوص سياقيًا
ui/src/hooks/useCrudMutations.ts : حصر-بحث فقط
ui/src/hooks/useCrudState.ts : حصر-بحث فقط
ui/src/hooks/useDebounce.ts : حصر-بحث فقط
ui/src/hooks/useDebouncedSearch.ts : حصر-بحث فقط
ui/src/hooks/useDialogNavigation.ts : حصر-بحث فقط
ui/src/hooks/useDialogReset.ts : حصر-بحث فقط
ui/src/hooks/useDocumentBlob.ts : حصر-بحث فقط
ui/src/hooks/useDocumentMutations.ts : حصر-بحث فقط
ui/src/hooks/useDocumentsQuery.ts : حصر-بحث فقط
ui/src/hooks/useFileEntitiesQuery.ts : حصر-بحث فقط
ui/src/hooks/useKeyboardNavigation.ts : حصر-بحث فقط
ui/src/hooks/usePensionStatistics.ts : حصر-بحث فقط
ui/src/hooks/useReportExport.ts : حصر-بحث فقط
ui/src/hooks/useRoleBasedUI.ts : حصر-بحث فقط
ui/src/index.css : مفحوص سياقيًا
ui/src/lib/chartColors.ts : مفحوص سياقيًا
ui/src/lib/cpu-processor.test.ts : مستبعد مع السبب - اختبارات
ui/src/lib/cpu-processor.ts : حصر-بحث فقط
ui/src/lib/crop-region.ts : حصر-بحث فقط
ui/src/lib/crop-worker-client.ts : حصر-بحث فقط
ui/src/lib/date.test.ts : مستبعد مع السبب - اختبارات
ui/src/lib/date.ts : حصر-بحث فقط
ui/src/lib/device-capabilities.ts : حصر-بحث فقط
ui/src/lib/dialogLayers.ts : حصر-بحث فقط
ui/src/lib/dialog-stack.ts : حصر-بحث فقط
ui/src/lib/enterFocus.ts : حصر-بحث فقط
ui/src/lib/errorUtils.ts : حصر-بحث فقط
ui/src/lib/fieldTranslations.ts : حصر-بحث فقط
ui/src/lib/homography.test.ts : مستبعد مع السبب - اختبارات
ui/src/lib/homography.ts : حصر-بحث فقط
ui/src/lib/logger.ts : حصر-بحث فقط
ui/src/lib/perspective-warp-utils.ts : حصر-بحث فقط
ui/src/lib/retirementFileForm.ts : حصر-بحث فقط
ui/src/lib/scanned-document.ts : حصر-بحث فقط
ui/src/lib/utils.ts : مفحوص سياقيًا
ui/src/lib/validation/common.ts : حصر-بحث فقط
ui/src/lib/validation/index.ts : حصر-بحث فقط
ui/src/lib/validation/retirementFileSchema.ts : حصر-بحث فقط
ui/src/lib/webgl-processor.ts : حصر-بحث فقط
ui/src/main.tsx : مفحوص سياقيًا
ui/src/pages/account/AccountPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/AccountingDashboardPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/BudgetPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/CashFlowPage.tsx : مفحوص سياقيًا
ui/src/pages/accounting/ChartOfAccountsPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/CostCentersPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/FiscalPeriodsPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/InitialOpeningBalancesPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/JournalEntriesPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/JournalEntryDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/QuarterDetailsPage.tsx : مفحوص سياقيًا
ui/src/pages/accounting/QuartersPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/ReportsPage.tsx : حصر-بحث فقط
ui/src/pages/accounting/VoucherListPage.tsx : حصر-بحث فقط
ui/src/pages/achievements/AchievementObstacleDetailDialog.tsx : حصر-بحث فقط
ui/src/pages/achievements/AchievementObstacleDetailPage.tsx : حصر-بحث فقط
ui/src/pages/achievements/AchievementObstacleDialog.tsx : حصر-بحث فقط
ui/src/pages/achievements/AchievementObstaclesPage.tsx : حصر-بحث فقط
ui/src/pages/admin/AdminLandingPage.tsx : حصر-بحث فقط
ui/src/pages/admin/AdminNotificationsPage.tsx : حصر-بحث فقط
ui/src/pages/admin/AuditLogsPage.tsx : حصر-بحث فقط
ui/src/pages/admin/LoginHistoryPage.tsx : حصر-بحث فقط
ui/src/pages/admin/SystemSettingsPage.tsx : حصر-بحث فقط
ui/src/pages/archive/ArchivePage.tsx : حصر-بحث فقط
ui/src/pages/archive/DocumentReviewArchivePage.tsx : حصر-بحث فقط
ui/src/pages/assets/AssetCategoriesPage.tsx : حصر-بحث فقط
ui/src/pages/assets/AssetDetailPage.tsx : حصر-بحث فقط
ui/src/pages/assets/DepreciationRunPage.tsx : حصر-بحث فقط
ui/src/pages/assets/FixedAssetsPage.tsx : حصر-بحث فقط
ui/src/pages/assets/tabs/AssetDepreciationTab.tsx : حصر-بحث فقط
ui/src/pages/assets/tabs/AssetInfoTab.tsx : حصر-بحث فقط
ui/src/pages/auth/ChangePasswordPage.tsx : مفحوص سياقيًا
ui/src/pages/auth/LoginPage.tsx : مفحوص سياقيًا
ui/src/pages/banks/BanksListPage.tsx : حصر-بحث فقط
ui/src/pages/branches/BranchesListPage.tsx : حصر-بحث فقط
ui/src/pages/compliance/ComplianceDashboardPage.tsx : حصر-بحث فقط
ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/correspondence/ExternalPartiesPage.tsx : حصر-بحث فقط
ui/src/pages/correspondence/IncomingRegisterPage.tsx : حصر-بحث فقط
ui/src/pages/correspondence/OutgoingRegisterPage.tsx : حصر-بحث فقط
ui/src/pages/DashboardPage.tsx : مفحوص سياقيًا
ui/src/pages/director/DirectorOfficePage.tsx : حصر-بحث فقط
ui/src/pages/discrepancy/DiscrepancyReportPage.tsx : حصر-بحث فقط
ui/src/pages/documents/DocumentTypesPage.tsx : حصر-بحث فقط
ui/src/pages/documents/MissingDocsCompletionPage.tsx : حصر-بحث فقط
ui/src/pages/finance/TreasuryPage.tsx : مفحوص سياقيًا
ui/src/pages/force/CodeBatchDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/hr/EmployeeDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/hr/EmployeesPage.tsx : حصر-بحث فقط
ui/src/pages/hr/HrAttendancePage.tsx : حصر-بحث فقط
ui/src/pages/hr/HrDashboardPage.tsx : مفحوص سياقيًا
ui/src/pages/hr/HrDocumentsPage.tsx : حصر-بحث فقط
ui/src/pages/hr/HrLeavesPage.tsx : حصر-بحث فقط
ui/src/pages/hr/HrReferenceDataPage.tsx : حصر-بحث فقط
ui/src/pages/hr/HrServiceHistoryPage.tsx : حصر-بحث فقط
ui/src/pages/inventory/InventoryCustodyPage.tsx : حصر-بحث فقط
ui/src/pages/inventory/InventoryDashboardPage.tsx : حصر-بحث فقط
ui/src/pages/inventory/InventoryItemsPage.tsx : حصر-بحث فقط
ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/inventory/InventoryTransactionsPage.tsx : حصر-بحث فقط
ui/src/pages/inventory/StocktakePage.tsx : مفحوص سياقيًا
ui/src/pages/investment/InvestmentGovernancePage.tsx : حصر-بحث فقط
ui/src/pages/investment/InvestmentHub.tsx : حصر-بحث فقط
ui/src/pages/investment/InvestmentSettingsPage.tsx : حصر-بحث فقط
ui/src/pages/investment/LeasingPage.tsx : حصر-بحث فقط
ui/src/pages/investment/MaintenanceDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/investment/MaintenancePage.tsx : حصر-بحث فقط
ui/src/pages/investment/PortfolioDetailPage.tsx : حصر-بحث فقط
ui/src/pages/investment/PortfoliosPage.tsx : مفحوص سياقيًا
ui/src/pages/investment/ProjectDetailPage.tsx : حصر-بحث فقط
ui/src/pages/investment/ProjectsPage.tsx : حصر-بحث فقط
ui/src/pages/investment/PropertiesPage.tsx : مفحوص سياقيًا
ui/src/pages/investment/PropertyDetailPage.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/ApprovalRequestsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/ContractsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/LookupsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PaymentSchedulesTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PortfolioProjectsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PortfolioPropertiesTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/ProjectPlansTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/ProjectPropertiesTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PropertyDepreciationTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PropertyMaintenanceTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PropertyUnitsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/PropertyValuationsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/RiskAssessmentsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/TenantPropertiesTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/TenantsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/tabs/TransactionsTab.tsx : حصر-بحث فقط
ui/src/pages/investment/TenantDetailPage.tsx : حصر-بحث فقط
ui/src/pages/ministry-approvals/ForceAnalyticsDashboard.tsx : حصر-بحث فقط
ui/src/pages/ministry-approvals/ForceAuditTab.tsx : حصر-بحث فقط
ui/src/pages/ministry-approvals/ReconciliationTab.tsx : مفحوص سياقيًا
ui/src/pages/notifications/NotificationsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/AgentsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/AnnualBookReviewPage.tsx : حصر-بحث فقط
ui/src/pages/pension/DocumentReviewPage.tsx : حصر-بحث فقط
ui/src/pages/pension/EntitledPersonsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/FileReviewListPage.tsx : مفحوص سياقيًا
ui/src/pages/pension/FinancialEntitlementsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/GuardiansPage.tsx : حصر-بحث فقط
ui/src/pages/pension/KashfReportsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/MinistryBatchesPage.tsx : حصر-بحث فقط
ui/src/pages/pension/MinistryReferralsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/MinistrySearchPage.tsx : حصر-بحث فقط
ui/src/pages/pension/MonthlySummariesPage.tsx : حصر-بحث فقط
ui/src/pages/pension/PaymentCardsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/PensionStatisticsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/PensionSuspensionsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/SalaryRecipientsPage.tsx : حصر-بحث فقط
ui/src/pages/pension/SalaryReconciliationPage.tsx : حصر-بحث فقط
ui/src/pages/pension/SocialResearchPage.tsx : حصر-بحث فقط
ui/src/pages/print/AccountingReportPrintPage.tsx : حصر-بحث فقط
ui/src/pages/print/DiscrepancyPrintPage.tsx : حصر-بحث فقط
ui/src/pages/print/DocumentPledgePrint.tsx : حصر-بحث فقط
ui/src/pages/print/EmployeePrintPage.tsx : حصر-بحث فقط
ui/src/pages/print/ForceItemPrintPage.tsx : حصر-بحث فقط
ui/src/pages/print/InventoryWithdrawalOrderPrint.tsx : حصر-بحث فقط
ui/src/pages/print/MissingDocumentsPrintPage.tsx : حصر-بحث فقط
ui/src/pages/print/PaymentBookPrint.tsx : مفحوص سياقيًا
ui/src/pages/print/PrintableEmployeeReport.tsx : حصر-بحث فقط
ui/src/pages/print/PrintablePaymentOrder.tsx : حصر-بحث فقط
ui/src/pages/print/PrintableRetirementFileReport.tsx : حصر-بحث فقط
ui/src/pages/print/PrintableVoucher.tsx : حصر-بحث فقط
ui/src/pages/print/RetireeStatementPrint.tsx : حصر-بحث فقط
ui/src/pages/print/RetirementFilePrintPage.tsx : حصر-بحث فقط
ui/src/pages/print/SettlementDecisionPrint.tsx : حصر-بحث فقط
ui/src/pages/ranks/RanksListPage.tsx : حصر-بحث فقط
ui/src/pages/reports/ReportsPage.tsx : حصر-بحث فقط
ui/src/pages/reports/ReportViewerPage.tsx : حصر-بحث فقط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575773099.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575776437.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575781404.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575784350.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575788871.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575791419.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/image/RetireeStatementPrint/1777575807944.png : مستبعد مع السبب - ملف ثنائي غير قابل لفحص أنماط
ui/src/pages/retirees/RetireeDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/retirees/RetireesPage.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/DocumentProcessPage.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/FileReviewPage.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/FileWorkflowManager.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/RetirementFileReportsPage.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/RetirementFilesPage.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/AgentsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/BookFormSchema.ts : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/DocumentsTab.tsx : مفحوص سياقيًا
ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/FinancialEntitlementsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/GuardiansTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/NotesTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/RecipientsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/ReviewTab.tsx : مفحوص سياقيًا
ui/src/pages/retirement-files/tabs/SalaryPaymentsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/SettlementsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/tabs/SuspensionsTab.tsx : حصر-بحث فقط
ui/src/pages/retirement-files/UploaderStatsPage.tsx : حصر-بحث فقط
ui/src/pages/review/FileReviewDetailPage.tsx : حصر-بحث فقط
ui/src/pages/salaries/SalaryBatchDetailsPage.tsx : حصر-بحث فقط
ui/src/pages/salaries/SalaryBatchesPage.tsx : حصر-بحث فقط
ui/src/pages/settlements/SettlementBatchDetailsPage.tsx : مفحوص سياقيًا
ui/src/pages/settlements/SettlementDecisionsPage.tsx : حصر-بحث فقط
ui/src/pages/suspensions/SuspensionsPage.tsx : حصر-بحث فقط
ui/src/pages/system-admin/UsersDirectoryPage.tsx : مفحوص سياقيًا
ui/src/stores/authStore.ts : حصر-بحث فقط
ui/src/stores/navStore.ts : حصر-بحث فقط
ui/src/stores/sidebarStore.ts : حصر-بحث فقط
ui/src/stores/themeStore.ts : مفحوص سياقيًا
ui/src/stores/useNotificationStore.ts : حصر-بحث فقط
ui/src/styles/print.css : مفحوص سياقيًا
ui/src/types/accounting.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/achievementObstacle.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/api.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/api/fiscal-period.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/api/journal-entry.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/api/retirement-file.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/api/salary-batch.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/api-response.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/archive.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/budget.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/correspondence.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/dashboard.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/financial-entitlements.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/notifications.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/pension.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/retirement.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/salaries.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/types/salary-recipients.ts : مستبعد مع السبب - لا طبقة عرض (api/types)
ui/src/utils/missing-documents.ts : حصر-بحث فقط
ui/src/workers/perspective-warp.worker.ts : حصر-بحث فقط
ui/tailwind.config.js : مفحوص سياقيًا
ui/tests/documentImageLoading.test.ts : مستبعد مع السبب - اختبار خارج ui/src
ui/tests/errorUtils.test.ts : مستبعد مع السبب - اختبار خارج ui/src
ui/tests/retirementFileForm.test.ts : مستبعد مع السبب - اختبار خارج ui/src
ui/tsconfig.app.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/tsconfig.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/tsconfig.node.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/tsconfig.test.json : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
ui/vite.config.ts : حصر-بحث فقط
ui/web.config : لم يُفحص - ملفات إعداد مشروع خارج نطاق فحص أنماط العرض
```
