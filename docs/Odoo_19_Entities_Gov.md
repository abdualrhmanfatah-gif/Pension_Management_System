# متطلبات النظام المحاسبي والمخزون والأصول المعدلة
# وفق منهجية Odoo 19.0 — للمؤسسات الحكومية

> **تاريخ الإنشاء:** 2026-08-07
> **الإصدار:** 1.0
> **المنهجية:** Odoo 19.0 Community
> **نوع المؤسسة:** حكومية (صندوق pensions)

---

## جدول المحتويات

1. [نظرة عامة](#1-نظرة-عامة)
2. [مبادئ التصميم](#2-مبادئ-التصميم)
3. [وحدة المحاسبة](#3-وحدة-المحاسبة)
4. [وحدة المخزون](#4-وحدة-المخزون)
5. [وحدة الأصول الثابتة](#5-وحدة-الأصول-الثابتة)
6. [ Relationships](#6-علاقات-الجداول)
7. [ملاحظات المؤسسات الحكومية](#7-ملاحظات-المؤسسات-الحكومية)

---

## 1. نظرة عامة

### إحصائيات النظام

| الوحدة | عدد الجداول | ملاحظات |
|--------|------------|---------|
| المحاسبة | 22 | شاملة الدليل والأurnalات والمدفوعات |
| المخزون | 12 | شاملة المنتجات والLocations والQuantities |
| الأصول الثابتة | 17 | شاملة الهلاك والتفصيلات |
| **المجموع** | **51** | |

### المبادئ المستخدمة من Odoo 19.0

| المبدأ | الوصف | تطبيق |
|--------|-------|-------|
| **Computed Fields** | الحسابات التلقائية | `net_book_value = cost - accumulated_depreciation` |
| **Company Dependent** | حقول تعتمد على الشركة | `property_account_receivable` |
| **Selection Fields** | قيم محددة مسبقاً | `state = draft/posted/cancelled` |
| **M2M Relations** | علاقات متعدد-متعدد | `tax_ids` في `account.move.line` |
| **No Fiscal Period Table** | لا جدول فترات منفصل | التواريخ تعتمد على `date` مباشرة |
| **Real-time Inventory** | جرد لحظي | `stock.quant` يتحدث تلقائياً |

---

## 2. مبادئ التصميم

### 2.1 تسمية الجداول (Odoo Conventions)

```
account.account          → جدول الحسابات
account.move             → القيود اليومية (Journal Entries)
account.move.line        → بنود القيود (Journal Items)
stock.quant              → أرصدة المخزون
stock.move               → حركات المخزون
```

### 2.2 الأعمدة الأساسية المشتركة

| العمود | النوع | الوصف |
|--------|-------|-------|
| `id` | `int` | المفتاح الرئيسي (Identity) |
| `create_date` | `datetime` | تاريخ الإنشاء |
| `write_date` | `datetime` | آخر تعديل |
| `create_uid` | `int` | المستخدم المنشئ |
| `write_uid` | `int` | آخر مستخدم عدّل |
| `company_id` | `int` | معرف الشركة (Company-dependent) |
| `active` | `boolean` | هل السجل نشط |

### 2.3 التعامل مع الحذف

**Odoo لا يحذف أي سجل** — يتم استخدام `active = false` بدلاً من الحذف الفعلي.

```csharp
// بدلاً من:
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }

// نستخدم:
public bool Active { get; set; } = true;
```

---

## 3. وحدة المحاسبة

### 3.1 res.company — الشركة

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم الشركة |
| `currency_id` | `int` | No | FK → res.currency |
| `phone` | `string(20)` | Yes | الهاتف |
| `email` | `string(100)` | Yes | البريد الإلكتروني |
| `website` | `string(200)` | Yes | الموقع الإلكتروني |
| `vat` | `string(50)` | Yes | الرقم الضريبي |
| `company_type` | `string(20)` | No | company / person |
| `street` | `string(200)` | Yes | الشارع |
| `city` | `string(100)` | Yes | المدينة |
| `state_id` | `int` | Yes | المحافظة |
| `country_id` | `int` | Yes | الدولة |
| `zip` | `string(10)` | Yes | الرمز البريدي |
| `active` | `bool` | No | نشط |

**ملاحظة حكومية:** استخدام واحد فقط — المؤسسة الحكومية.

---

### 3.2 res.currency — العملة

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(10)` | No | كود العملة (YER, USD) |
| `symbol` | `string(10)` | No | الرمز (﷼, $) |
| `decimal_places` | `int` | No | عدد الخانات العشرية |
| `active` | `bool` | No | نشط |

**البيانات الأولية:**

| كود | رمز | الخانات |
|-----|-----|---------|
| YER | ﷼ | 0 |
| USD | $ | 2 |

---

### 3.3 account.account — الحسابات (الدليل المحاسبي)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `code` | `string(20)` | No | كود الحساب |
| `name` | `string(200)` | No | اسم الحساب |
| `account_type` | `string(50)` | No | نوع الحساب (Odoo 19: asset, liability, equity, revenue, expense) |
| `account_group_id` | `int` | Yes | FK → account.group |
| `parent_id` | `int` | Yes | FK → account.account (الأب) |
| `parent_path` | `string(200)` | Yes | المسار الهرمي (Materialized Path) |
| `currency_id` | `int` | Yes | FK → res.currency |
| `company_id` | `int` | No | FK → res.company |
| `is_off_balance` | `bool` | No | هل خارج الميزانية |
| `deprecated` | `bool` | No | هل الحساب مهمل |
| `active` | `bool` | No | نشط |

**أنواع الحسابات في Odoo 19.0:**

```
asset_receivable     → أصول — مدينة (المدينون)
asset_cash           → أصول — نقدية
asset_current        → أصول — حالية
asset_non_current    → أصول — غير حالية
asset_fixed          → أصول — ثابتة
liability_payable    → التزامات — دائنة (الدائنون)
liability_current    → التزامات — حالية
liability_non_current → التزامات — غير حالية
equity               → حقوق الملكية
equity_unaffected    → حقوق متأثرة
income               → إيرادات
income_other         → إيرادات أخرى
expense              → مصروفات
expense_depreciation → هلاك
expense_direct_cost  → تكلفة مباشرة
```

---

### 3.4 account.account.type — أنواع الحسابات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم النوع |
| `type` | `string(50)` | No | loại (asset, liability, equity, revenue, expense) |
| `internal_group` | `string(50)` | No | المجموعة الداخلية (asset, liability, equity, income, expense) |
| `active` | `bool` | No | نشط |

**المجموعات الداخلية (Internal Groups):**

| المجموعة | الوصف |
|----------|-------|
| `asset` | أصول |
| `liability` | التزامات |
| `equity` | حقوق ملكية |
| `income` | إيرادات |
| `expense` | مصروفات |

---

### 3.5 account.group — مجموعات الحسابات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم المجموعة |
| `code_prefix_start` | `string(20)` | Yes | بداية نطاق الكود |
| `code_prefix_end` | `string(20)` | Yes | نهاية نطاق الكود |
| `parent_id` | `int` | Yes | FK → account.group |
| `company_id` | `int` | No | FK → res.company |

**ملاحظة:** في Odoo 19.0، `account.group` يحل محل مستويات الحسابات الهرمية (Account Level).

---

### 3.6 account.journal — الديوان (الأurnalات)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم الديوان |
| `code` | `string(20)` | No | الكود المختصر |
| `type` | `string(20)` | No | bank / cash / general / sale / purchase |
| `currency_id` | `int` | Yes | FK → res.currency |
| `company_id` | `int` | No | FK → res.company |
| `default_account_id` | `int` | Yes | FK → account.account |
| `active` | `bool` | No | نشط |

**أنواع الديونات:**

| النوع | الوصف | الاستخدام الحكومي |
|-------|-------|------------------|
| `bank` | بنكي | حسابات بنوك المؤسسة |
| `cash` | نقدي | الصندوق |
| `general` | عام | القيود اليومية العامة |
| `sale` | مبيعات | **غير مستخدم** |
| `purchase` | مشتريات | **غير مستخدم** |

---

### 3.7 account.move — القيود اليومية (Journal Entries)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(50)` | Yes | رقم القيد (يولّد تلقائياً) |
| `ref` | `string(500)` | Yes | المرجع / البيان |
| `date` | `date` | No | تاريخ القيد |
| `journal_id` | `int` | No | FK → account.journal |
| `company_id` | `int` | No | FK → res.company |
| `currency_id` | `int` | No | FK → res.currency |
| `partner_id` | `int` | Yes | FK → res.partner |
| `move_type` | `string(20)` | No | entry / out_invoice / in_invoice / out_refund / in_refund |
| `state` | `string(20)` | No | draft / posted / cancelled |
| `invoice_date` | `date` | Yes | تاريخ الفاتورة |
| `invoice_date_due` | `date` | Yes | تاريخ الاستحقاق |
| `invoice_origin` | `string(200)` | Yes | المرجع الأصلي |
| `payment_reference` | `string(200)` | Yes | مرجع الدفع |
| `narration` | `text` | Yes | وصفي / ملاحظات |
| `amount_total` | `decimal` | No | الإجمالي |
| `amount_residual` | `decimal` | No | المتبقي |
| `amount_untaxed` | `decimal` | No | قبل الضريبة |
| `amount_tax` | `decimal` | No | الضريبة |
| `reversed_entry_id` | `int` | Yes | FK → account.move (القيد المعكوس) |
| `auto_post` | `string(20)` | Yes | هل يُنشر تلقائياً |
| `active` | `bool` | No | نشط |

**أنواع القيود (move_type):**

| النوع | الوصف | الاستخدام الحكومي |
|-------|-------|------------------|
| `entry` | قيد عام | نعم — الأكثر استخداماً |
| `out_invoice` | فاتورة بيع | لا |
| `in_invoice` | فاتورة شراء | نعم — للفواتير الواردة |
| `out_refund` | إشعار دائن (بيع) | لا |
| `in_refund` | إشعار دائن (شراء) | نعم |

---

### 3.8 account.move.line — بنود القيود (Journal Items)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `move_id` | `int` | No | FK → account.move |
| `name` | `string(500)` | Yes | وصف البند |
| `account_id` | `int` | No | FK → account.account |
| `partner_id` | `int` | Yes | FK → res.partner |
| `debit` | `decimal(18,2)` | No | المبلغ المدين |
| `credit` | `decimal(18,2)` | No | المبلغ الدائن |
| `currency_id` | `int` | Yes | FK → res.currency |
| `amount_currency` | `decimal(18,2)` | Yes | المبلغ بعملة الحساب |
| `amount_residual` | `decimal(18,2)` | Yes | المتبقي |
| `date` | `date` | No | التاريخ |
| `journal_id` | `int` | No | FK → account.journal |
| `company_id` | `int` | No | FK → res.company |
| `tax_ids` | `M2M` | Yes | account.move.line → account.tax |
| `tax_repartition_line_id` | `int` | Yes | FK → tax repartition |
| `analytic_account_id` | `int` | Yes | FK → account.analytic.account |
| `analytic_distribution` | `json` | Yes | التوزيع التحليلي |
| `reconciled` | `bool` | No | هل تم التسوية |
| `reconcile_model_id` | `int` | Yes | FK → account.reconcile.model |
| `statement_id` | `int` | Yes | FK → account.bank.statement |
| `statement_line_id` | `int` | Yes | FK → account.bank.statement.line |
| `blocked` | `bool` | No | هل محظور من التسوية |
| `date_maturity` | `date` | Yes | تاريخ الاستحقاق |
| `tax_base_amount` | `decimal` | Yes | الأساس الضريبي |
| `tax_exigible` | `bool` | No | هل يجب تقاضي الضريبة |
| `is_refund` | `bool` | No | هل هو إشعار دائن |
| `display_type` | `string(20)` | Yes | product_section / payment_term / tax_line |

---

### 3.9 account.tax — الضرائب

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم الضريبة |
| `amount_type` | `string(20)` | No | fixed / percent / group / division |
| `amount` | `decimal` | No | النسبة أو المبلغ |
| `type_tax_use` | `string(20)` | No | sale / purchase / none |
| `tax_group_id` | `int` | Yes | FK → account.tax.group |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |
| `description` | `string(50)` | Yes | الوصف |
| `tax_include_display` | `bool` | No | هل السعر يشمل الضريبة |

**ملاحظة حكومية:** في المؤسسات الحكومية، عادة لا توجد ضريبة مشتريات (no VAT). يمكن استخدام `type_tax_use = none`.

---

### 3.10 account.fiscal.position — المواقع المالية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | الاسم |
| `auto_apply` | `string(20)` | Yes | dtm / vat / country / country_group |
| `vat_required` | `bool` | No | هل يجب إدخال الرقم الضريبي |
| `country_id` | `int` | Yes | FK → res.country |
| `country_group_id` | `int` | Yes | FK → res.country.group |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

---

### 3.11 account.fiscal.position.tax — تعيين الضرائب للموقع المالي

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `tax_src_id` | `int` | No | FK → account.tax (المصدر) |
| `tax_dest_id` | `int` | Yes | FK → account.tax (البديل) |
| `position_id` | `int` | No | FK → account.fiscal.position |

---

### 3.12 account.payment — الدفعات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | Yes | رقم الدفعة |
| `amount` | `decimal(18,2)` | No | المبلغ |
| `currency_id` | `int` | No | FK → res.currency |
| `payment_type` | `string(20)` | No | outbound / inbound / internal |
| `partner_type` | `string(20)` | Yes | customer / supplier |
| `partner_id` | `int` | Yes | FK → res.partner |
| `journal_id` | `int` | No | FK → account.journal |
| `company_id` | `int` | No | FK → res.company |
| `payment_method_id` | `int` | Yes | FK → account.payment.method |
| `payment_method_line_id` | `int` | Yes | FK → account.payment.method.line |
| `state` | `string(20)` | No | draft / in_process / paid / cancelled |
| `date` | `date` | No | تاريخ الدفع |
| `ref` | `string(500)` | Yes | المرجع / البيان |
| `move_id` | `int` | Yes | FK → account.move |
| `destination_account_id` | `int` | Yes | FK → account.account |
| `matched_move_ids` | `M2M` | Yes | القيود المطابقة |

---

### 3.13 account.bank.statement — كشوفات البنك

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | Yes | اسم الكشف |
| `journal_id` | `int` | No | FK → account.journal |
| `date` | `date` | No | تاريخ الكشف |
| `state` | `string(20)` | No | draft / posted / reconciled |
| `balance_start` | `decimal` | Yes | الرصيد الافتتاحي |
| `balance_end_real` | `decimal` | Yes | الرصيد الختامي الفعلي |
| `balance_end` | `decimal` | Yes | الرصيد الختامي المحسوب |
| `company_id` | `int` | No | FK → res.company |
| `currency_id` | `int` | No | FK → res.currency |

---

### 3.14 account.bank.statement.line — بنود كشوفات البنك

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `statement_id` | `int` | No | FK → account.bank.statement |
| `date` | `date` | No | التاريخ |
| `name` | `string(500)` | No | البيان |
| `amount` | `decimal(18,2)` | No | المبلغ |
| `amount_currency` | `decimal(18,2)` | Yes | المبلغ بعملة أخرى |
| `currency_id` | `int` | Yes | FK → res.currency |
| `partner_id` | `int` | Yes | FK → res.partner |
| `account_id` | `int` | Yes | FK → account.account |
| `move_id` | `int` | Yes | FK → account.move |
| `reconciled` | `bool` | No | هل تم التسوية |
| `company_id` | `int` | No | FK → res.company |

---

### 3.15 account.cash.rounding — التقريب النقدي

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم طريقة التقريب |
| `rounding` | `decimal` | No | مبلغ التقريب |
| `rounding_method` | `string(20)` | No | UP / DOWN / HALF_UP |
| `account_id` | `int` | Yes | FK → account.account (حساب التقريب) |
| `company_id` | `int` | No | FK → res.company |

---

### 3.16 account.reconcile.model — نماذج التسوية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم النموذج |
| `rule_type` | `string(20)` | No | writeoff_button / writeoff_suggestion / matching_button |
| `auto_reconcile` | `bool` | No | هل يسوّي تلقائياً |
| `match_journal_ids` | `M2M` | Yes | الديونات المطابقة |
| `match_nature` | `string(20)` | Yes | amount_paid / amount_received / amount_currency |
| `company_id` | `int` | No | FK → res.company |

---

### 3.17 account.reconcile.model.line — بنود نموذج التسوية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `model_id` | `int` | No | FK → account.reconcile.model |
| `account_id` | `int` | Yes | FK → account.account |
| `label` | `string(200)` | Yes | الوصف |
| `amount_type` | `string(20)` | No | percentage / fixed |
| `amount` | `decimal` | No | المبلغ أو النسبة |
| `tax_ids` | `M2M` | Yes | → account.tax |
| `company_id` | `int` | No | FK → res.company |

---

### 3.18 account.analytic.plan — الخطط التحليلية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم الخطة |
| `description` | `text` | Yes | الوصف |
| `parent_id` | `int` | Yes | FK → account.analytic.plan |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

**ملاحظة حكومية:** يمكن استخدامها لتتبع التكاليف حسب: الأقسام، المشاريع، البرامج الحكومية.

---

### 3.19 account.analytic.account — الحسابات التحليلية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم الحساب التحليلي |
| `plan_id` | `int` | Yes | FK → account.analytic.plan |
| `code` | `string(20)` | Yes | الكود |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

---

### 3.20 account.analytic.line — الخطوط التحليلية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | Yes | الوصف |
| `date` | `date` | No | التاريخ |
| `account_id` | `int` | No | FK → account.analytic.account |
| `company_id` | `int` | No | FK → res.company |
| `currency_id` | `int` | No | FK → res.currency |
| `amount` | `decimal(18,2)` | No | المبلغ |
| `unit_amount` | `decimal` | Yes | الكمية |
| `product_id` | `int` | Yes | FK → product.product |
| `general_account_id` | `int` | Yes | FK → account.account |
| `move_id` | `int` | Yes | FK → account.move.line |
| `partner_id` | `int` | Yes | FK → res.partner |
| `ref` | `string(200)` | Yes | المرجع |
| `user_id` | `int` | Yes | المستخدم |

---

### 3.21 res.partner — الشركاء (الموظفين، الموردون، البنوك)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | الاسم |
| `company_name` | `string(200)` | Yes | اسم الشركة |
| `is_company` | `bool` | No | هل شركة |
| `email` | `string(100)` | Yes | البريد |
| `phone` | `string(20)` | Yes | الهاتف |
| `mobile` | `string(20)` | Yes | الجوال |
| `vat` | `string(50)` | Yes | الرقم الضريبي |
| `street` | `string(200)` | Yes | الشارع |
| `city` | `string(100)` | Yes | المدينة |
| `state_id` | `int` | Yes | المحافظة |
| `country_id` | `int` | Yes | الدولة |
| `zip` | `string(10)` | Yes | الرمز البريدي |
| `category_id` | `M2M` | Yes | → res.partner.category |
| `company_id` | `int` | Yes | FK → res.company |
| `active` | `bool` | No | نشط |
| `customer_rank` | `int` | No | ترتيب العميل |
| `supplier_rank` | `int` | No | ترتيب المورد |

---

### 3.22 res.bank — البنوك

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم البنك |
| `bic` | `string(20)` | Yes | كود SWIFT |
| `code` | `string(20)` | Yes | الكود |
| `street` | `string(200)` | Yes | العنوان |
| `city` | `string(100)` | Yes | المدينة |
| `state` | `string(100)` | Yes | المحافظة |
| `country` | `string(100)` | Yes | الدولة |
| `zip` | `string(10)` | Yes | الرمز البريدي |
| `phone` | `string(20)` | Yes | الهاتف |
| `active` | `bool` | No | نشط |

---

## 4. وحدة المخزون

### 4.1 product.category — فئات المنتجات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم الفئة |
| `parent_id` | `int` | Yes | FK → product.category |
| `parent_path` | `string(200)` | Yes | المسار الهرمي |
| `property_cost_method` | `string(20)` | Yes | standard / average / fifo |
| `property_valuation` | `string(20)` | Yes | manual / automated |
| `property_stock_account_input_categ_id` | `int` | Yes | حساب المخزون الوارد |
| `property_stock_account_output_categ_id` | `int` | Yes | حساب المخزون الصادر |
| `property_stock_valuation_account_id` | `int` | Yes | حساب تقييم المخزون |
| `company_id` | `int` | Yes | FK → res.company |
| `active` | `bool` | No | نشط |

---

### 4.2 product.template — نماذج المنتجات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم المنتج |
| `type` | `string(20)` | No | consu (serv) / product / service |
| `categ_id` | `int` | No | FK → product.category |
| `list_price` | `decimal` | No | سعر البيع |
| `standard_price` | `decimal` | No | التكلفة / سعر التكلفة |
| `currency_id` | `int` | No | FK → res.currency |
| `taxes_id` | `M2M` | Yes | → account.tax (ضرائب البيع) |
| `supplier_taxes_id` | `M2M` | Yes | → account.tax (ضرائب الشراء) |
| `company_id` | `int` | Yes | FK → res.company |
| `active` | `bool` | No | نشط |
| `description_sale` | `text` | Yes | وصف البيع |
| `description_purchase` | `text` | Yes | وصف الشراء |
| `weight` | `decimal` | Yes | الوزن |
| `volume` | `decimal` | Yes | الحجم |

**أنواع المنتجات:**

| النوع | الوصف | الاستخدام الحكومي |
|-------|-------|------------------|
| `consu` | منتج مادي (لا مخزون) | مفروشات بسيطة |
| `product` | منتج مادي (مع مخزون) | أجهزة، قطع غيار |
| `service` | خدمة | صيانة، استشارات |

---

### 4.3 product.product — نسخ المنتجات (Product Variants)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `product_tmpl_id` | `int` | No | FK → product.template |
| `default_code` | `string(50)` | Yes | الكود الافتراضي (SKU) |
| `name` | `string(200)` | No | الاسم |
| `barcode` | `string(100)` | Yes | الباركود |
| `active` | `bool` | No | نشط |
| `company_id` | `int` | Yes | FK → res.company |

---

### 4.4 stock.warehouse — المستودعات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم المستودع |
| `code` | `string(20)` | No | الكود |
| `company_id` | `int` | No | FK → res.company |
| `partner_id` | `int` | Yes | FK → res.partner |
| `active` | `bool` | No | نشط |
| `wh_input_stock_loc_id` | `int` | Yes | FK → stock.location (المستقبل) |
| `wh_output_stock_loc_id` | `int` | Yes | FK → stock.location (المغادر) |
| `wh_stock_loc_id` | `int` | Yes | FK → stock.location (التخزين) |
| `wh_quarantine_loc_id` | `int` | Yes | FK → stock.location (الحجر) |

---

### 4.5 stock.location — مواقع التخزين

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم الموقع |
| `complete_name` | `string(500)` | Yes | المسار الكامل |
| `location_id` | `int` | Yes | FK → stock.location (الأب) |
| `usage` | `string(20)` | No | supplier / customer / inventory / transit / internal |
| `scrap_location` | `bool` | No | هل موقع التخلص |
| `return_location` | `bool` | No | هل موقع الإرجاع |
| `removal_strategy_id` | `int` | Yes | FK → stock.removal (策略) |
| `company_id` | `int` | Yes | FK → res.company |
| `active` | `bool` | No | نشط |

**أنواع المواقع:**

| النوع | الوصف | الاستخدام الحكومي |
|-------|-------|------------------|
| `supplier` | المورد | لا |
| `customer` | العميل | لا |
| `inventory` | الجرد | نعم — للتعديلات |
| `transit` | عابر | نعم — للنقل بين الفروع |
| `internal` | داخلي | نعم — التخزين الأساسي |

---

### 4.6 stock.quant — أرصدة المخزون (Real-time)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `product_id` | `int` | No | FK → product.product |
| `location_id` | `int` | No | FK → stock.location |
| `lot_id` | `int` | Yes | FK → stock.lot (الدفعة) |
| `package_id` | `int` | Yes | FK → stock.quant.package |
| `owner_id` | `int` | Yes | FK → res.partner (المالك) |
| `quantity` | `decimal` | No | الكمية الفعلية |
| `reserved_quantity` | `decimal` | No | الكمية المحجوزة |
| `available_quantity` | `decimal` | No | الكمية المتاحة (= quantity - reserved) |
| `inventory_quantity` | `decimal` | Yes | كمية الجرد (للتعديل) |
| `inventory_diff_quantity` | `decimal` | Yes | فرق الجرد |
| `inventory_date` | `date` | Yes | تاريخ آخر جرد |
| `cost` | `decimal` | Yes | التكلفة |
| `company_id` | `int` | No | FK → res.company |
| `in_date` | `date` | Yes | تاريخ الإدخال |
| `tracking` | `string(20)` | Yes | none / lot / serial |

**ملاحظة مهمة:** `stock.quant` هو الجدول الرئيسي للمخزون في Odoo. يتحدث تلقائياً عند كل حركة مخزون.

---

### 4.7 stock.quant.package — الطرود

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | رقم الطرد |
| `packaging_id` | `int` | Yes | FK → product.packaging |
| `location_id` | `int` | Yes | FK → stock.location |
| `company_id` | `int` | Yes | FK → res.company |

---

### 4.8 stock.move — حركات المخزون

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(500)` | No | الوصف |
| `product_id` | `int` | No | FK → product.product |
| `product_uom_qty` | `decimal` | No | الكمية المطلوبة |
| `product_uom` | `int` | No | FK → uom.uom (وحدة القياس) |
| `location_id` | `int` | No | FK → stock.location (المصدر) |
| `location_dest_id` | `int` | No | FK → stock.location (الوجهة) |
| `picking_id` | `int` | Yes | FK → stock.picking |
| `state` | `string(20)` | No | draft / waiting / assigned / done / cancelled |
| `date` | `date` | No | التاريخ المخطط |
| `date_expected` | `date` | Yes | التاريخ المتوقع |
| `date_done` | `date` | Yes | تاريخ التنفيذ الفعلي |
| `company_id` | `int` | No | FK → res.company |
| `origin` | `string(200)` | Yes | المرجع الأصلي |
| `group_id` | `int` | Yes | FK → procurement.group |
| `rule_id` | `int` | Yes | FK → stock.rule |
| `warehouse_id` | `int` | Yes | FK → stock.warehouse |
| `quantity_done` | `decimal` | Yes | الكمية المنفذة |
| `price_unit` | `decimal` | Yes | سعر الوحدة |

---

### 4.9 stock.move.line — بنود حركات المخزون

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `move_id` | `int` | No | FK → stock.move |
| `product_id` | `int` | No | FK → product.product |
| `product_uom_id` | `int` | No | FK → uom.uom |
| `location_id` | `int` | No | FK → stock.location |
| `location_dest_id` | `int` | No | FK → stock.location |
| `lot_id` | `int` | Yes | FK → stock.lot |
| `package_id` | `int` | Yes | FK → stock.quant.package |
| `result_package_id` | `int` | Yes | FK → stock.quant.package (النتيجة) |
| `qty_done` | `decimal` | No | الكمية المنفذة |
| `qty_done_customer` | `decimal` | Yes | الكمية للعميل |
| `qty_done_production` | `decimal` | Yes | الكمية للإنتاج |
| `state` | `string(20)` | No | draft / assigned / done / cancelled |
| `date` | `date` | No | التاريخ |
| `company_id` | `int` | No | FK → res.company |
| `owner_id` | `int` | Yes | FK → res.partner |

---

### 4.10 stock.picking — التحويلات (Pickings)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | رقم التحويل |
| `picking_type_id` | `int` | No | FK → stock.picking.type |
| `location_id` | `int` | No | FK → stock.location (المصدر) |
| `location_dest_id` | `int` | No | FK → stock.location (الوجهة) |
| `state` | `string(20)` | No | draft / waiting / confirmed / assigned / done / cancelled |
| `date` | `date` | No | التاريخ المخطط |
| `date_done` | `date` | Yes | تاريخ الإنجاز |
| `scheduled_date` | `date` | Yes | التاريخ المجدول |
| `origin` | `string(200)` | Yes | المرجع الأصلي |
| `partner_id` | `int` | Yes | FK → res.partner |
| `company_id` | `int` | No | FK → res.company |
| `user_id` | `int` | Yes | المستخدم المسؤول |
| `priority` | `string(20)` | Yes | 0 / 1 / 2 |

---

### 4.11 stock.picking.type — أنواع التحويلات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم النوع |
| `code` | `string(20)` | No | incoming / outgoing / internal |
| `warehouse_id` | `int` | Yes | FK → stock.warehouse |
| `sequence_id` | `int` | Yes | FK → ir.sequence |
| `default_location_src_id` | `int` | Yes | FK → stock.location |
| `default_location_dest_id` | `int` | Yes | FK → stock.location |
| `active` | `bool` | No | نشط |

**أنواع التحويلات الحكومية:**

| النوع | الكود | الاستخدام |
|-------|-------|----------|
| استلام | `incoming` | استلام المشتريات |
| صرف | `outgoing` | صرف للموظفين |
| داخلي | `internal` | نقل بين الفروع |

---

### 4.12 stock.inventory — جرد المخزون (Inventory Adjustments)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | وصف الجرد |
| `date` | `date` | No | تاريخ الجرد |
| `location_id` | `int` | Yes | FK → stock.location |
| `product_id` | `int` | Yes | FK → product.product |
| `state` | `string(20)` | No | draft / done / cancel |
| `company_id` | `int` | No | FK → res.company |
| `user_id` | `int` | Yes | المستخدم المسؤول |
| `line_ids` | `O2M` | — | → stock.inventory.line |

**ملاحظة:** `stock.inventory` في Odoo 19.0 يُستخدم للجرد اليدوي فقط. الجرد اللحظي يتم عبر `stock.quant`.

---

## 5. وحدة الأصول الثابتة

### 5.1 account.asset.category — فئات الأصول

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم الفئة |
| `code` | `string(20)` | Yes | الكود |
| `account_asset_id` | `int` | No | FK → account.account (حساب الأصل) |
| `account_depreciation_id` | `int` | No | FK → account.account (حساب الهلاك المتراكم) |
| `account_expense_depreciation_id` | `int` | No | FK → account.account (حساب مصروف الهلاك) |
| `method` | `string(20)` | No | linear / declining / declining_then_else |
| `method_number` | `int` | No | عدد الفترات (السنوات) |
| `method_period` | `string(20)` | No | year / month / quarter |
| `method_progress_factor` | `decimal` | Yes | عامل التقدم (declining) |
| `prorata` | `bool` | No | هل يُحسب النسبة |
| `open_asset` | `bool` | No | هل يُفتح الأصل تلقائياً |
| `group_ids` | `M2M` | Yes | → account.asset.group |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

---

### 5.2 account.asset.group — مجموعات الأصول

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم المجموعة |
| `code` | `string(20)` | Yes | الكود |
| `parent_id` | `int` | Yes | FK → account.asset.group |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

---

### 5.3 account.asset.asset — الأصول الثابتة

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم الأصل |
| `code` | `string(50)` | Yes | رقم الأصل (يولّد تلقائياً) |
| `category_id` | `int` | No | FK → account.asset.category |
| `group_ids` | `M2M` | Yes | → account.asset.group |
| `company_id` | `int` | No | FK → res.company |
| `currency_id` | `int` | No | FK → res.currency |
| `date` | `date` | No | تاريخ الشراء |
| `date_first_depreciation` | `string(20)` | No | manual / first_day / halfway |
| `value` | `decimal(18,2)` | No | القيمة الأصلية (التكلفة) |
| `salvage_value` | `decimal(18,2)` | No | القيمة التخلي (ال Junk Value) |
| `account_asset_id` | `int` | No | FK → account.account |
| `account_depreciation_id` | `int` | No | FK → account.account |
| `account_expense_depreciation_id` | `int` | No | FK → account.account |
| `method` | `string(20)` | No | linear / declining / declining_then_else |
| `method_number` | `int` | No | عدد الفترات |
| `method_period` | `string(20)` | No | year / month / quarter |
| `method_progress_factor` | `decimal` | Yes | عامل التقدم |
| `prorata` | `bool` | No | هل يُحسب النسبة |
| `depreciation_line_ids` | `O2M` | — | → account.asset.depreciation.line |
| `state` | `string(20)` | No | draft / open / close |
| `partner_id` | `int` | Yes | FK → res.partner (المورد) |
| `invoice_id` | `int` | Yes | FK → account.move (الفاتورة) |
| `active` | `bool` | No | نشط |
| `date_close` | `date` | Yes | تاريخ الإغلاق |
| `purchased_value` | `decimal` | Yes | القيمة المكتسبة |
| `hours_per_day` | `decimal` | Yes | الساعات اليومية (للأصول المنوية) |
| `prorata_date` | `date` | Yes | تاريخ النسبة |

---

### 5.4 account.asset.depreciation.line — خطوط الهلاك

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `asset_id` | `int` | No | FK → account.asset.asset |
| `depreciation_date` | `date` | No | تاريخ الهلاك |
| `amount` | `decimal(18,2)` | No | مبلغ الهلاك |
| `amount_total` | `decimal(18,2)` | No | الإجمالي المتراكم |
| `depreciated_value` | `decimal(18,2)` | No | القيمة المُهلاكة |
| `remaining_value` | `decimal(18,2)` | No | القيمة المتبقية |
| `move_id` | `int` | Yes | FK → account.move (القيد المحاسبي) |
| `parent_id` | `int` | Yes | FK → account.asset.depreciation.line |
| `move_check` | `bool` | No | هل تم إنشاء القيد |
| `move_posted_check` | `bool` | No | هل تم نشر القيد |
| `name` | `string(200)` | Yes | الوصف |
| `sequence` | `int` | No | الترتيب |
| `company_id` | `int` | No | FK → res.company |

---

### 5.5 asset.asset.type — أنواع الأصول (مخصص)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم النوع |
| `code` | `string(20)` | No | الكود |
| `description` | `string(500)` | Yes | الوصف |
| `is_active` | `bool` | No | نشط |
| `created_at` | `datetime` | No | تاريخ الإنشاء |

**البيانات الأولية (Government):**

| الكود | الاسم |
|-------|-------|
| IT | أجهزة وتقنية |
| VEH | مركبات |
| BLD | مباني |
| LND | أراضي |
| FUR | أثاث ومفروشات |

---

### 5.6 asset.it.detail — تفاصيل الأصول التقنية

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `asset_id` | `int` | No | PK, FK → account.asset.asset |
| `model` | `string(100)` | Yes | الطراز |
| `manufacturer` | `string(100)` | Yes | الشركة المصنعة |
| `processor_type` | `string(100)` | Yes | نوع المعالج |
| `processor_speed` | `string(50)` | Yes | سرعة المعالج |
| `ram_size` | `string(50)` | Yes | حجم الرام |
| `hard_disk_type` | `string(50)` | Yes | نوع القرص |
| `hard_disk_size` | `string(50)` | Yes | حجم القرص |
| `gpu_model` | `string(100)` | Yes | كرت الشاشة |
| `screen_size` | `string(50)` | Yes | حجم الشاشة |
| `screen_resolution` | `string(50)` | Yes | دقة الشاشة |
| `operating_system` | `string(100)` | Yes | نظام التشغيل |
| `has_webcam` | `bool` | No | كاميرا |
| `has_wifi` | `bool` | No | واي فاي |
| `has_bluetooth` | `bool` | No | بلوتوث |
| `ip_address` | `string(50)` | Yes | عنوان IP |
| `mac_address` | `string(50)` | Yes | عنوان MAC |
| `warranty_expiry` | `date` | Yes | انتهاء الضمان |
| `notes` | `string(500)` | Yes | ملاحظات |

---

### 5.7 asset.vehicle.detail — تفاصيل المركبات

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `asset_id` | `int` | No | PK, FK → account.asset.asset |
| `plate_number` | `string(20)` | Yes | رقم اللوحة |
| `plate_letter1` | `string(5)` | Yes | حرف اللوحة 1 |
| `plate_letter2` | `string(5)` | Yes | حرف اللوحة 2 |
| `plate_letter3` | `string(5)` | Yes | حرف اللوحة 3 |
| `engine_number` | `string(100)` | Yes | رقم المحرك |
| `chassis_number` | `string(100)` | Yes | رقم الشاسيه |
| `transmission_type` | `string(50)` | Yes | نوع ناقل الحركة |
| `fuel_type` | `string(50)` | Yes | نوع الوقود |
| `cylinder_count` | `int` | Yes | عدد الأسطوانات |
| `engine_capacity` | `string(50)` | Yes | سعة المحرك |
| `horse_power` | `int` | Yes | القوة الحصانية |
| `wheel_count` | `int` | No | عدد العجلات |
| `tire_size` | `string(50)` | Yes | مقاس الإطارات |
| `color` | `string(50)` | Yes | اللون |
| `model_year` | `int` | Yes | سنة الصنع |
| `registration_expiry` | `date` | Yes | انتهاء التسجيل |
| `insurance_expiry` | `date` | Yes | انتهاء التأمين |
| `mileage` | `int` | No | المسافة المقطوعة |
| `has_gps` | `bool` | No | GPS |
| `notes` | `string(500)` | Yes | ملاحظات |

---

### 5.8 asset.building.detail — تفاصيل المباني

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `asset_id` | `int` | No | PK, FK → account.asset.asset |
| `area_sqm` | `decimal` | Yes | المساحة (م²) |
| `built_area_sqm` | `decimal` | Yes | المساحة المبنية |
| `floor_count` | `int` | No | عدد الطوابق |
| `ownership_doc_number` | `string(100)` | Yes | رقم وثيقة الملكية |
| `license_number` | `string(100)` | Yes | رقم الترخيص |
| `notes` | `string(500)` | Yes | ملاحظات |

---

### 5.9 asset.land.detail — تفاصيل الأراضي

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `asset_id` | `int` | No | PK, FK → account.asset.asset |
| `plot_number` | `string(50)` | Yes | رقم القطعة |
| `land_area_sqm` | `decimal` | Yes | المساحة (م²) |
| `ownership_doc_number` | `string(100)` | Yes | رقم وثيقة الملكية |
| `ownership_doc_date` | `date` | Yes | تاريخ الوثيقة |
| `notes` | `string(500)` | Yes | ملاحظات |

---

### 5.10 asset.furniture.detail — تفاصيل الأثاث

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `asset_id` | `int` | No | PK, FK → account.asset.asset |
| `material` | `string(100)` | Yes | المادة |
| `color` | `string(50)` | Yes | اللون |
| `notes` | `string(500)` | Yes | ملاحظات |

---

### 5.11 asset.life.cycle — دورة حياة الأصل (Government Addition)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `asset_id` | `int` | No | FK → account.asset.asset |
| `event_type` | `string(50)` | No | purchase / receipt / commission / assignment / transfer / maintenance / disposal |
| `event_date` | `date` | No | تاريخ الحدث |
| `from_location` | `string(200)` | Yes | الموقع السابق |
| `to_location` | `string(200)` | Yes | الموقع الجديد |
| `from_employee_id` | `int` | Yes | الموظف السابق |
| `to_employee_id` | `int` | Yes | الموظف الجديد |
| `description` | `string(500)` | Yes | وصف الحدث |
| `document_number` | `string(100)` | Yes | رقم المستند |
| `approved_by` | `string(100)` | Yes | المعتمد |
| `company_id` | `int` | No | FK → res.company |
| `created_at` | `datetime` | No | تاريخ الإنشاء |

**أنواع الأحداث:**

| النوع | الوصف |
|-------|-------|
| `purchase` | الشراء |
| `receipt` | الاستلام |
| `commission` | التشغيل |
| `assignment` | التعيين على موظف |
| `transfer` | النقل بين الأقسام |
| `maintenance` | الصيانة |
| `disposal` | التخلص |

---

### 5.12 asset.assignment — تعيين الأصول للموظفين (Government Addition)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `asset_id` | `int` | No | FK → account.asset.asset |
| `employee_id` | `int` | No | FK → res.partner (الموظف) |
| `assignment_date` | `date` | No | تاريخ التعيين |
| `return_date` | `date` | Yes | تاريخ الإرجاع |
| `status` | `string(20)` | No | active / returned / transferred |
| `assigned_by` | `string(100)` | Yes | المعين |
| `notes` | `string(500)` | Yes | ملاحظات |
| `company_id` | `int` | No | FK → res.company |
| `created_at` | `datetime` | No | تاريخ الإنشاء |

---

### 5.13 maintenance.equipment — معدات الصيانة (Government Addition)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | اسم المعدة |
| `asset_id` | `int` | Yes | FK → account.asset.asset |
| `category_id` | `int` | Yes | FK → maintenance.equipment.category |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |
| `assign_date` | `date` | Yes | تاريخ التعيين |
| `owner_user_id` | `int` | Yes | المستخدم المسؤول |
| `technician_user_id` | `int` | Yes | الفني المسؤول |
| `maintenance_team_id` | `int` | Yes | FK → maintenance.team |
| `next_action_date` | `date` | Yes | تاريخ الصيانة القادمة |
| `cycle_days` | `int` | Yes | دورة الصيانة (أيام) |
| `location` | `string(200)` | Yes | الموقع |

---

### 5.14 maintenance.request — طلبات الصيانة (Government Addition)

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(200)` | No | عنوان الطلب |
| `equipment_id` | `int` | Yes | FK → maintenance.equipment |
| `request_date` | `date` | No | تاريخ الطلب |
| `schedule_date` | `date` | Yes | التاريخ المجدول |
| `completion_date` | `date` | Yes | تاريخ الإنجاز |
| `stage_id` | `int` | No | FK → maintenance.stage |
| `priority` | `string(20)` | Yes | 0 / 1 / 2 |
| `company_id` | `int` | No | FK → res.company |
| `user_id` | `int` | Yes | المسؤول |
| `technician_id` | `int` | Yes | الفني |
| `description` | `text` | Yes | الوصف |
| `maintenance_type` | `string(20)` | No | corrective / preventive |
| `cost` | `decimal` | Yes | التكلفة |

---

### 5.15 maintenance.stage — مراحل الصيانة

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم المرحلة |
| `sequence` | `int` | No | الترتيب |
| `done` | `bool` | No | هل هي مرحلة إنهاء |
| `fold` | `bool` | No | هل تُطوى في الـ Kanban |

**البيانات الأولية:**

| الترتيب | الاسم |
|---------|-------|
| 1 | جديد |
| 2 | قيد المراجعة |
| 3 | قيد التنفيذ |
| 4 | مكتمل |

---

### 5.16 maintenance.team — فرق الصيانة

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم الفريق |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

---

### 5.17 maintenance.equipment.category — فئات معدات الصيانة

| العمود | النوع | Nullable | الوصف |
|--------|-------|----------|-------|
| `id` | `int` | No | PK |
| `name` | `string(100)` | No | اسم الفئة |
| `company_id` | `int` | No | FK → res.company |
| `active` | `bool` | No | نشط |

---

## 6. علاقات الجداول

### 6.1 المحاسبة

```
res.company ──────────────────────────┐
    │                                  │
    ├── res.currency                   │
    │       │                          │
    │       └── account.account        │
    │               │                  │
    │               ├── account.group  │
    │               └── account.account (parent)
    │                                  │
    ├── account.journal ───────────────┤
    │       │                          │
    │       └── account.move           │
    │               │                  │
    │               ├── account.move.line
    │               │       │
    │               │       ├── account.account
    │               │       ├── res.partner
    │               │       └── account.tax (M2M)
    │               │
    │               └── account.bank.statement
    │                       │
    │                       └── account.bank.statement.line
    │
    ├── account.tax
    │       └── account.fiscal.position.tax
    │               └── account.fiscal.position
    │
    ├── account.payment
    │       ├── account.journal
    │       └── account.move
    │
    ├── account.analytic.plan
    │       └── account.analytic.account
    │               └── account.analytic.line
    │
    └── res.partner
            └── res.bank
```

### 6.2 المخزون

```
product.category
    └── product.template
            └── product.product

stock.warehouse
    ├── stock.location (wh_input_stock_loc_id)
    ├── stock.location (wh_output_stock_loc_id)
    └── stock.location (wh_stock_loc_id)

stock.location
    ├── stock.quant
    │       ├── product.product
    │       ├── stock.location
    │       └── stock.quant.package
    │
    ├── stock.move
    │       ├── product.product
    │       ├── stock.location (source)
    │       ├── stock.location (dest)
    │       └── stock.picking
    │
    └── stock.inventory
            └── stock.inventory.line

stock.picking
    ├── stock.picking.type
    └── stock.move (via picking_id)
```

### 6.3 الأصول الثابتة

```
account.asset.category
    ├── account.account (asset)
    ├── account.account (depreciation)
    └── account.account (expense)

account.asset.asset
    ├── account.asset.category
    ├── account.asset.group (M2M)
    ├── res.partner
    ├── account.move (invoice)
    │
    ├── account.asset.depreciation.line (O2M)
    │       └── account.move
    │
    ├── asset.it.detail (1:1)
    ├── asset.vehicle.detail (1:1)
    ├── asset.building.detail (1:1)
    ├── asset.land.detail (1:1)
    ├── asset.furniture.detail (1:1)
    │
    ├── asset.life.cycle (O2M)
    └── asset.assignment (O2M)

maintenance.equipment
    ├── account.asset.asset
    ├── maintenance.equipment.category
    ├── maintenance.team
    └── maintenance.request (O2M)
```

---

## 7. ملاحظات المؤسسات الحكومية

### 7.1 جداول مُستبعدة (غير مطلوبة)

| الجدول | السبب |
|--------|-------|
| `account.payment.term` | لا توجد شروط دفع متنوعة في المؤسسات الحكومية |
| `account.payment.term.line` | مرتبط بالسابق |
| `res.partner.bank` | لا يُستخدم في الدفعات الحكومية |
| `account.accounting.map` | لا حاجة لمطابقة الأكواد |
| `account.incoterms` | لا توجد معاملات تجارية دولية |
| `stock.package` | لا يُستخدم في المستودعات الحكومية |

### 7.2 تعديلات حكومية

| التعديل | الوصف |
|---------|-------|
| **عملة واحدة (YER)** | لا حاجة لدعم عملات متعددة |
| **company_id موحد** | جميع الجداول تنتمي لشركة واحدة |
| **بدون ضرائب** | `type_tax_use = none` |
| **بدون مبيعات** | `account.journal.type != sale` |
| **جرد لحظي** | `stock.quant` يتحدث تلقائياً |
| **أرصدة الأصول** | تتبع تلقائي عبر `account.asset.depreciation.line` |

### 7.3 جداول مُضافة (Government-specific)

| الجدول | الغرض |
|--------|-------|
| `asset.life.cycle` | تتبع دورة حياة الأصل من الشراء للتخلص |
| `asset.assignment` | تعيين الأصول للموظفين |
| `maintenance.equipment` | إدارة معدات الصيانة |
| `maintenance.request` | طلبات الصيانة |
| `maintenance.stage` | مراحل الصيانة |
| `maintenance.team` | فرق الصيانة |
| `maintenance.equipment.category` | فئات المعدات |

### 7.4 الحقول المُعدلة

| الجدول | الحقل | التعديل |
|--------|-------|---------|
| `account.move` | `move_type` | تقييد على `entry` و `in_invoice` فقط |
| `stock.quant` | `tracking` | تعطيل التتبع بالدفعة/الرقم التسلسلي |
| `account.asset.asset` | `state` | تقييد على `draft` و `open` و `close` |

---

## فهرس الجداول

| # | الجدول | الوحدة | Odoo |
|---|--------|--------|------|
| 1 | `res.company` | مشتركة | نعم |
| 2 | `res.currency` | مشتركة | نعم |
| 3 | `res.partner` | مشتركة | نعم |
| 4 | `res.bank` | مشتركة | نعم |
| 5 | `account.account` | المحاسبة | نعم |
| 6 | `account.account.type` | المحاسبة | نعم |
| 7 | `account.group` | المحاسبة | نعم |
| 8 | `account.journal` | المحاسبة | نعم |
| 9 | `account.move` | المحاسبة | نعم |
| 10 | `account.move.line` | المحاسبة | نعم |
| 11 | `account.tax` | المحاسبة | نعم |
| 12 | `account.fiscal.position` | المحاسبة | نعم |
| 13 | `account.fiscal.position.tax` | المحاسبة | نعم |
| 14 | `account.payment` | المحاسبة | نعم |
| 15 | `account.bank.statement` | المحاسبة | نعم |
| 16 | `account.bank.statement.line` | المحاسبة | نعم |
| 17 | `account.cash.rounding` | المحاسبة | نعم |
| 18 | `account.reconcile.model` | المحاسبة | نعم |
| 19 | `account.reconcile.model.line` | المحاسبة | نعم |
| 20 | `account.analytic.plan` | المحاسبة | نعم |
| 21 | `account.analytic.account` | المحاسبة | نعم |
| 22 | `account.analytic.line` | المحاسبة | نعم |
| 23 | `product.category` | المخزون | نعم |
| 24 | `product.template` | المخزون | نعم |
| 25 | `product.product` | المخزون | نعم |
| 26 | `stock.warehouse` | المخزون | نعم |
| 27 | `stock.location` | المخزون | نعم |
| 28 | `stock.quant` | المخزون | نعم |
| 29 | `stock.quant.package` | المخزون | نعم |
| 30 | `stock.move` | المخزون | نعم |
| 31 | `stock.move.line` | المخزون | نعم |
| 32 | `stock.picking` | المخزون | نعم |
| 33 | `stock.picking.type` | المخزون | نعم |
| 34 | `stock.inventory` | المخزون | نعم |
| 35 | `account.asset.category` | الأصول | نعم |
| 36 | `account.asset.group` | الأصول | نعم |
| 37 | `account.asset.asset` | الأصول | نعم |
| 38 | `account.asset.depreciation.line` | الأصول | نعم |
| 39 | `asset.asset.type` | الأصول | مخصص |
| 40 | `asset.it.detail` | الأصول | مخصص |
| 41 | `asset.vehicle.detail` | الأصول | مخصص |
| 42 | `asset.building.detail` | الأصول | مخصص |
| 43 | `asset.land.detail` | الأصول | مخصص |
| 44 | `asset.furniture.detail` | الأصول | مخصص |
| 45 | `asset.life.cycle` | الأصول | حكومي |
| 46 | `asset.assignment` | الأصول | حكومي |
| 47 | `maintenance.equipment` | الصيانة | حكومي |
| 48 | `maintenance.request` | الصيانة | حكومي |
| 49 | `maintenance.stage` | الصيانة | حكومي |
| 50 | `maintenance.team` | الصيانة | حكومي |
| 51 | `maintenance.equipment.category` | الصيانة | حكومي |

---

> **ملاحظة ختامية:** هذا التصميم مبني على Odoo 19.0 Community مع تعديلات خاصة بالمؤسسات الحكومية اليمنية. جميع الأعمدة والأنواع مأخوذة من الكود الفعلي لـ Odoo على GitHub.
