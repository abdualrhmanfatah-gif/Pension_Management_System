# Feature Context: Agent ↔ Entitled Person Link (وكيل لكل مستحق)

## Document Metadata
- **Generated**: 2026-09-22
- **Input Type**: simple_description
- **Source**: "اذا كان لدينا اكثر من مستحق ولكل مستحق وكيل كيف نحل هاذا السناريوا" — الخيار (ب): `int? EntitledID` مباشرة في `Agent`
- **Status**: DISCOVERY_COMPLETE

---

## Original Request

> "اذا كان لدينا اكثر من مستحق ولكل مستحق وكيل كيف نحل هاذا السناريوا" — ثم اختيار المستخدم **الخيار (ب): حقل `int? EntitledID` مباشرة في `Agent`** — ثم: "ابدا با اعداد خطه تفصيليه تتضمن جميع السناريوهات المرتبطه".

---

## Problem Statement

ملف ورثة واحد قد يحوي عدة مستحقين (ورثة قُصّر/غائبين/ذوي كفالات) وكل مستحق له وكيل خاص به، لكن النظام اليوم يعامل الوكيل **على مستوى الملف فقط** — لا يوجد أي ربط Agent↔EntitledPerson، ويمنع النظام من تسجيل أكثر من وكيل نشط واحد لكل ملف أصلاً.

**Evidence**:
- `Agent.cs:10-12` — لا يوجد أي FK إلى `EntitledPerson`.
- `AgentService.cs:63-66` — **يمنع أي وكيل نشط ثانٍ للملف** → السيناريو كله محجوب.
- `AgentsTab.tsx:232,240` — زر "إضافة وكيل" يُعطّل عند وجود أي وكيل نشط.
- `AgentDialog.tsx` — لا حقل "لحساب مستحق".

**Frequency**: دوري — لكل ملف ورثة جديد بعدد ورثته.
**Impact**: فرع الموظف يُجبر على تسجيل وكيل واحد يجمع للجميع، أو عدم تسجيل وكلاء أصلاً → `SharePercent` غير مُنسب، ودفاتر/تقارير الصرف تُظهر اسماً واحداً لكل الملف.

---

## Core Intent Analysis

| Element | Question | Answer |
|---------|----------|--------|
| **WHO** | من يستخدم هذه الميزة؟ | موظف فرع pensions يُسجّل وكيلاً لكل مستحق في ملف ورثة |
| **WHAT** | ما النتيجة المطلوبة؟ | ربط كل وكيل بمستحق محدد (Option B) + السماح بعدة وكلاء نشطين متوازيين |
| **WHEN** | متى تُستخدم؟ | أثناء إعداد ملف الورثة وبعد إضافة المستحقين |
| **WHY** | لماذا هذا مهم؟ | لتمكين تحصيل/صرف حصة كل مستحق عبر وكيله حصريًا |

---

## Non-Goals (Explicit Exclusions)

| # | Non-Goal | Rationale |
|---|----------|-----------|
| NG1 | جدول وسيط N:M `AgentEntitledPerson` | المستخدم اختار الخيار (ب) صراحةً — ربط مباشر أبسط |
| NG2 | تعديل CHECK `CK_SalaryRecipient_TypeOrGuardianWithEntitled` | الربط يعيش في `Agent.EntitledID`؛ `SalaryRecipient` يستمر بـ `AgentID` فقط |
| NG3 | وكيل واحد يخدم عدة ملفات (Cross-file) | `Agent` يبقى File-scoped |
| NG4 | تاريخ ربط زمني متعدد لنفس الوكيل | Option B = رابط حالي واحد؛ التغيير عبر تعديل/استبدال الوكيل |
| NG5 | إنشاء مستلم الصرف تلقائيًا عند تسجيل الوكيل | Parking Lot — يُحسم بعد إجابة Q1 |

---

## Codebase Research

### Patterns Found

#### Pattern 1: Guardian ↔ Entitled (النمط المرجعي)
- **Location**: `GuardianEntitledPerson.cs` + `GuardianService.cs:106-126`
- **Relevance**: النمط اللازم موجود فعلاً للوصي: جدول وسيط N:M + تحويل معاش كل مستحق لوكيله/وصيه تلقائيًا.
- **Reusable**: فكرة الربط وتحويل المستلم نموذج لinaire — لكننا نختار N:1 أبسط.

#### Pattern 2: SalaryRecipient النمطي
- **Location**: `SalaryRecipient.cs:69-80` + `PayrollConfiguration.cs:27-35`
- **Relevance**: `SalaryRecipient` يدعم عدة أنواع مستلمين عبر CHECK constraint — الربط يبقى عبر `AgentID` فقط.

#### Pattern 3: EntitledPerson Unique Filtered Index
- **Location**: `EntitledPersonConfiguration.cs:46`
- **Relevance**: نمط الفهرس الفريد المفلتر `(FileID, EntitledID) WHERE IsDeleted=0 AND IsActive=1` — نفس النمط سيُطبق على `(FileID, EntitledID)` في `Agent`.

### Existing Infrastructure
- `MissingDocumentService.cs:238-242,428-431` — وثائق كل وكيل نشط (تتكيف مع تعددهم).
- `FileStatusService.cs:396-400` + `RetirementFileService.cs:1637-1641` — قاعدة ملفية "AgencyDocument إذا وُجد وكيل نشط".
- `FileReviewService.cs:193-194` — المراجعة تُدرج جدول الوكلاء.
- `ComplianceEngine.cs:23` — Smart scanning يتضمن الوكلاء.

### Code References (الإشارات الحرجة)
- `Agent.cs:10-12` — الكيان: AgentID + FileID فقط، لا EntitledID.
- `AgentService.cs:63-66` — قيد "وكيل واحد للملف" يمنع السيناريو.
- `AgentsTab.tsx:232,240` — بوابة الزر تمنع إضافة وكيل ثانٍ.
- `AgentDialog.tsx` — لا حقل لـ Entitled.
- `SalaryRecipientService.cs:222-234` — إلغاء كل المستلمين النشطين عند إنشاء واحد.
- `PaymentBookService.cs:257` — نفس نمط "النشط الأول للملف".
- `PayrollGenerationService.cs:54-59` — تعليق "multiple allowed" + تقسيم بـ `SharePercent` (130-182).
- `Init.cs:7011-7016` — فهرس فريد `(FileID, BatchID)` على `SalaryPaymentItems` — يمنع بنود متعددة لنفس الملف/دفعة.
- `GuardianService.cs:106-126` — تحويل معاش الوصي per-entitled.
- `GuardianService.cs:298-353` — إنشاء مستلم وصي **دون** إلغاء مستلمي مستحقين آخرين.
- `ReportService.Documents.cs:72-82` — AgentPhone = FirstOrDefault.
- `FinancialEntitlementService.cs:288-320` — بحث بالاسم، `agentDict` = First لكل متقاعد.
- `DashboardController.cs:79,198` + `PensionStatisticsDto.cs:12` — ActiveAgents.
- `SalaryPaymentBatchService.cs:456-468` — Excel export recipient = agent.
- `RetirementFileService.cs:1637-1641` — قاعدة الوثائق السياقية.

---

## Use Scenarios (جميع السيناريوهات المرتبطة)

### S1 — التسجيل والصيانة
- **S1.1** وكيل عام للملف (`EntitledID = NULL`) — سلوك وراثي قائم يبقى.
- **S1.2** تسجيل وكيل مرتبط بمستحق X داخل الملف.
- **S1.3** **السيناريو الأساسي:** 3 ورثة → 3 وكلاء نشطين متزامنين في الملف.
- **S1.4** تعديل وكيل: تغيير المستحق المرتبط / الوكيل العام.
- **S1.5** إلغاء/حذف وكيل مستحق واحد دون المساس بالوكلاء الآخرين.
- **S1.6** عرض القائمة يميّز: "وكيل عام" مقابل "وكيل حساب مستحق فلان".
- **S1.7** (قيد) وكيل نشط واحد لكل **نطاق** (نفس `EntitledID` أو كلاهما NULL).

### S2 — الصرف والمستلمين
- **S2.1** وكيل X يحصّل حصة X بنسبة `SharePercent` الخاصة بـ X.
- **S2.2** **تعدد متزامن:** وكلاء مختلفون يحصّلون في نفس الشهر/الدفعة.
- **S2.3** تبديل المحصّل: مستحق X ← وكيل X (إنهاء مستلم + بدء وكيل).
- **S2.4** توليد دفعة الصرف وتقسيم الحصص بين عدة مستلمين (الكود موجود، الفهرس يعارضه).
- **S2.5** دفتر الصرف يولّد مستلم "وكيل حساب X".
- **S2.6** Excel/طباعة تُظهر "الوكيل عن حساب المستحق X".

### S3 — الوثائق والامتثال
- **S3.1** وثيقة وكالة لكل وكيل نشط (يقاس بعدة وكلاء) — موجود يتكيف.
- **S3.2** نواقص هوية/صورة لكل وكيل على حدة — موجود.
- **S3.3** قاعدة اعتماد الملف تتطلب وثيقة وكالة إذا وُجد **أي** وكيل — تتكيف تلقائيًا.

### S4 — المراجعة والتدقيق
- **S4.1** إنشاء/تعديل وكيل ← PendingReview + مراجعة تُدرج كل الوكلاء.
- **S4.2** إقرار الرفض لكل وكيل.
- **S4.3** سجل تدقيق لكل وكيل (`IImmutableAuditService`).

### S5 — التقارير والإحصاءات
- **S5.1** نواقص المستندات بصف "وكيل" — يُضاف "عن حساب: X".
- **S5.2** نواقص الملف (AgentPhone FirstOrDefault) — غموض مع تعدّد الوكلاء.
- **S5.3** بحث الالتزام المالي باسم الوكيل (`agentDict` = First) — غموض.
- **S5.4** إحصاء `activeAgents` (Dashboard + PensionStatistics) — يصبح = عدد الوكلاء.
- **S5.5** صفحة الوكلاء العامة — عمود "لحساب مستحق".
- **S5.6** تقرير الطباعة يربط اسم مستلم الوكيل بـ `agentId`.

### S6 — الحالات الحدية
- **S6.1** توقف/وفاة/زواج المستحق X وبقاء وكيله نشطاً → إلغاء تلقائي (Q4).
- **S6.2** بيانات وكياء موجودين: `EntitledID = NULL` يبقى وكيلًا عامًا (لا تكسير بيانات).
- **S6.3** نفس الإنسان وكيل في ملفين → صفّان مستقلان، `EntitledID` ضمني للملف — سليم.
- **S6.4** مستحق له وصي ووكيل معًا → يُمنع الجمع (Q5).
- **S6.5** وكيل قاصر متزامن مع وصاية — تداخل الأنواع.

---

## Gap Analysis

| # | Gap | Scope/Behavior/User/Integration | Evidence |
|---|-----|--------------------------------|----------|
| G1 | لا يوجد ربط Agent↔Entitled | Scope | `Agent.cs` |
| G2 | قيد "وكيل نشط واحد للملف" يمنع السيناريو | Behavior | `AgentService.cs:63-66` |
| G3 | بوابة الزر تمنع إضافة وكيل ثانٍ | Behavior | `AgentsTab.tsx:232,240` |
| G4 | نموذج مستلم واحد نشط للملف مقابل تصميم تعدد المستلمين + فهرس فريد | Behavior+Integration | `SalaryRecipientService.cs:222` vs `PayrollGenerationService.cs:54` vs `Init.cs:7011` |
| G5 | أثر فقد أهلية المستحق على وكيله غير محدد | Behavior | لا كود |
| G6 | الموظف يحتاج تسمية "لحساب مستحق" في الإضافة/العرض/الصرف | User | `AgentDialog`، `RecipientsTab`، Excel/طباعة |
| G7 | `SharePercent` للمستلم الوكيل لا يُشتق تلقائيًا من حصة المستحق (`EntitledID` null في `SalaryRecipient`) | Behavior | `SalaryRecipientService.cs:217` |
| G8 | تقارير First/FirstOrDefault تتدهور مع تعدّد الوكلاء | Integration | `ReportService.Documents.cs:72`، `FinancialEntitlementService.cs:315` |
| G9 | أولوية الوصي مقابل الوكيل لنفس المستحق غير محددة | Behavior | `GuardianService.cs` |
| G10 | هجرة بيانات: تفسير `NULL` كوكيل عام | Scope | Migration `AddEntitledIDToAgents` |

---

## Questions Requiring Resolution

### Q1: مستلمون نشطون متوازون (S2.2)
- **Category**: Scope/Behavior
- **Owner**: User
- **Blocking**: Yes
- **Gap**: هل يجب أن يحصّل وكلاء مختلفون (لكل مستحق وكيله) في نفس الشهر/دفعة الصرف — مستلمون نشطون متوازون؟
- **Options**:
  - A) نعم، متوازون (موصى به) — نعدل إلغاء التفعيل + الفهرس الفريد
  - B) لا، مستلم واحد يتناوب — يُبدَّل بين وكلاء المستحقين شهرياً
  - C) غير متأكد — نصمم يدعم الحالتين
- **Why It Matters**: يحدد ما إذا كان `SalaryRecipientService` و `SalaryPaymentItems` يحتاجان تعديل.
- **Resolution**: _يمكن اختيار A (متوازون) — مُجاب_

### Q2: إلزام ربط المستحق (S1.2)
- **Category**: Scope
- **Owner**: User
- **Blocking**: Yes
- **Gap**: هل ربط الوكيل بـ "لحساب المستحق" إلزامي أم اختياري؟
- **Options**:
  - A) اختياري (موصى به) — `EntitledID` يقبل NULL = وكيل عام
  - B) إلزامي إن وُجد مستحقون — يمنع خطأ التسجيل
  - C) إلزامي عند التخصيص فقط
- **Why It Matters**: يحدد ما إذا كان `AgentService.Create` يحتاج تحقق "لا يمكن عام إن وُجد مستحقون".
- **Resolution**: _يمكن اختيار A (اختياري) — مُجاب_

### Q3: قيد الوكيل النشط (S1.7)
- **Category**: Scope/Business
- **Owner**: User
- **Blocking**: Yes
- **Gap**: ماذا عن قيد "وكيل نشط واحد" بعد الإضافة؟
- **Options**:
  - A) واحد لكل نطاق (موصى به) — وكيل واحد لكل `EntitledID` (أو وكيلان عامان إن NULL)
  - B) متعدد بلا حد — أي عدد
  - C) واحد للملف كالآن — يُبطل السيناريو
- **Why It Matters**: يحدد التحقق في `AgentService` والفهرس الفريد المفلتر.
- **Resolution**: _يمكن اختيار A (واحد لكل نطاق) — مُجاب_

### Q4: أثر فقد أهلية المستحق (S6.1)
- **Category**: Behavior
- **Owner**: User
- **Blocking**: No
- **Gap**: عند توقف/وفاة/زواج مستحق X وبقاء وكيله نشطاً — ماذا يحدث لوكيله ومستلم الصرف؟
- **Options**:
  - A) إلغاء تلقائي (موصى به) — إيقاف وكيل X ومستلمه آلياً
  - B) إبقاء نشط + تحذير — يبقى الوكيل ويُعلَّم لل reviser
  - C) إيقاف المستلم فقط
- **Why It Matters**: يمنع تحصيلاً غير مشروع بعد فقد الأهلية.
- **Resolution**: _يمكن اختيار A (إلغاء تلقائي) — مُجاب_

### Q5: أولوية الوصي/الوكيل (S6.4)
- **Category**: Behavior
- **Owner**: User
- **Blocking**: No
- **Gap**: مستحق لديه وصي ووكيل معاً — من يحصّل حصة ذلك المستحق؟
- **Options**:
  - A) يُمنع الجمع (موصى به) — لا يمكن ربط وصي ووكيل لنفس المستحق
  - B) الأحدث تعييناً يفوز — التعيين الأخير يحل محل السابق
  - C) الوصي أولاً — الوصية تسبق الوكيلية
- **Why It Matters**: يحدد منطق `GuardianService` عند محاولة تحويل معاش مستحق يملك وكيل.
- **Resolution**: _يمكن اختيار A (يُمنع الجمع) — مُجاب_

---

## Parking Lot (Deferred Ideas)

| # | Idea | Priority | Deferred Because |
|---|------|----------|------------------|
| PL1 | إنشاء/تحويل مستلم الصرف تلقائيًا عند تسجيل وكيل المستحق | Medium | يحتاج إجابة Q1 (مُجاب — يُحسم عند التنفيذ) |
| PL2 | تسمية Excel/طباعة "الوكيل عن حساب X" | Low | تابع لـ S2.6، يُنفَّذ كجزء من M7 |
| PL3 | الترقية لجدول وسيط N:M إن احتاجت المشاركة الحقيقية | Low | المستخدم رفض Option A |

---

## Goals (Resolved)

1. ربط `Agent` بـ `EntitledPerson` عبر `int? EntitledID` (Option B) — **مُحسم**.
2. وكيل نشط واحد لكل نطاق (`EntitledID` أو NULL) — **مُحسم**.
3. مستلمون نشطون متوازون لكل مستحق وكيله في نفس الدفعة — **مُحسم**.
4. إلغاء تلقائي لوكيل ومستلم عند توقف/وفاة/زواج المستحق — **مُحسم**.
5. يُمنع الجمع وصي + وكيل لنفس المستحق — **مُحسم**.
6. ربط الوكيل بالمستحق اختياري (NULL = وكيل عام) — **مُحسم**.

---

## Implementation Plan (9 مراحل)

### M1 — النموذج الأساسي (Core)
- `Agent.cs`: إضافة `int? EntitledID` + `EntitledPerson? EntitledPerson`.
- `EntitledPerson.cs`: `ICollection<Agent> Agents` معاد.
- `AgentConfiguration.cs`: FK Restrict + فهرس فريد مفلتر `[IsDeleted]=0 AND [IsActive]=1` على `(FileID, EntitledID)`.
- Migration `AddEntitledIDToAgents`: العمود + FK + الفهرس؛ الوكلاء الحاليون → `NULL`.

### M2 — `AgentService` (التحقق)
- استبدال قيد `:63-66` (وكيل واحد للملف) بقيد النطاق: رفض تكرار **نفس** `EntitledID` نشط (وشامل NULL).
- تحقق: `EntitledID` ينتمي **لنفس الملف** وغير محذوف.
- **Q5**: رفض ربط وكيل بمستحق له وصٍّ نشط (`GuardianEntitledPerson`) برسالة عربية.

### M3 — DTOs و API
- `AgentDtos.cs`: `EntitledID?` في Create/Update + `EntitledPersonName?` في Read/Search.
- المسارات (`AgentsController`) بلا تغيير؛ رسائل التحقق من M2.

### M4 — الصرف المتعدد (Q1) — المرحلة الأثقل
1. `SalaryRecipientService.cs:222-234`: الإلغاء الجماعي ← إلغاء مستلمي نفس النطاق فقط.
2. `PaymentBookService.cs:257`: نفس منطق النطاق بدل "النشط الأول للملف".
3. فهرس `SalaryPaymentItems`: حذف `IX_FileID_BatchID` الفريد ← استبداله بفريد على `(FileID, BatchID, RecipientID)`.
4. **Q5 في الصرف**: إنشاء مستلم وكيل عن X ← رفض إن وُجد مستلم وصي عن X؛ و`GuardianService.TransferSalaryToGuardianAsync` ← رفض إن وُجد وكيل نشط لـ X.
5. **Q4 cascade**: hook into entitled stop/death/marriage operations → deactivate linked active agents + their SalaryRecipients.

### M5 — الوثائق والمراجعة
- لا تغيير — تعمل لكل وكيل نشط تلقائيًا.

### M6 — التقارير والإحصاءات
- `ReportService.Documents.cs:72`: عرض هواتف **كل** الوكلاء النشطين بدل `FirstOrDefault`.
- `FinancialEntitlementService.cs:288-320`: إعطاء أولوية وكيل **المستلم النشط** بدل `First`.
- Excel (`SalaryPaymentBatchService.cs:456`): إضافة "عن حساب: X" (S2.6).

### M7 — الواجهة (`ui`)
- `retirement.ts`: `entitledID?` + `entitledPersonName?`.
- `AgentDialog.tsx`: حقل اختياري "لحساب مستحق" + خيار "وكيل عام للملف".
- `AgentsTab.tsx:232,240`: تغيير بوابة الزر من `agents.some(isActive)` إلى فحص النطاق + عمود "لحساب مستحق".
- `ViewAgentDialog` + `RecipientsTab` + `AgentsPage`: عرض/تسمية "وكيل — حساب X".

### M8 — الاختبارات (xUnit)
- إنشاء وكيل: عام/مرتبط/تكرار نفس النطاق/ربط بمستحق ملف آخر.
- تعارض Q5 (وصي+وكيل) في AgentService + GuardianService.
- Cascade Q4: توقف مستحق ← إيقاف وكيله ومستلمه.
- مستلمون متوازون: `CreateAsync` لا يُلغي نطاقًا آخر؛ حفظ دفعة ببنود متعددة.

### M9 — البناء والتحقق
- `dotnet build` → `dotnet test` → `cd ui && npm run lint && npm run build`.

---

## Next Steps

1. المستخدم يُ نهائي الخطة ويعتمدها.
2. البدء التنفيذ من M1 → M9.
3. كتابة الملف بعد اعتماد الخطة (الآن في وضع التخطيط).
4. ثم `/add-new-feature` للتصميم المعماري التفصيلي (إن لزم).
