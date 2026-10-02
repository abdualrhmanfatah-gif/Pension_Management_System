# Print System

## Pension Management System — Print Components & Patterns

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Total Print Components** | 6 |
| **Location** | `ui/src/components/ui/print/` |
| **Common Components** | `ui/src/components/common/` |
| **Page Sizes** | A4, A3, A5, Letter |
| **Orientation** | Portrait, Landscape |

---

## 2. Print Components

### 2.1 PrintLayout

**File:** `ui/src/components/ui/print/PrintLayout.tsx`

Main print page layout with header, footer, and watermark.

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `pageSize` | `'A4'\|'A3'\|'A5'\|'letter'` | `'A4'` | Page size |
| `orientation` | `'portrait'\|'landscape'` | `'portrait'` | Page orientation |
| `watermark` | `string` | — | Watermark text |
| `header` | `ReactNode` | — | Page header |
| `footer` | `ReactNode` | — | Page footer |
| `children` | `ReactNode` | — | Page content |
| `className` | `string` | — | Custom classes |

**Page Sizes:**
| Size | Dimensions | Usage |
|------|------------|-------|
| `A4` | 210mm × 297mm | Standard documents |
| `A3` | 297mm × 420mm | Large reports |
| `A5` | 148mm × 210mm | Small documents |
| `letter` | 8.5in × 11in | US letter |

**Features:**
- Double-border frame
- Print-specific CSS
- `@page` rules
- `break-inside: avoid`
- Watermark support

**Usage:**
```tsx
<PrintLayout
  pageSize="A4"
  orientation="portrait"
  watermark="سري"
  header={<PrintHeader />}
  footer={<PrintFooter />}
>
  <h1>تقرير المتقاعدين</h1>
  <PrintableReportTable columns={columns} data={data} />
</PrintLayout>
```

---

### 2.2 PrintableReportTable

**File:** `ui/src/components/ui/print/PrintableReportTable.tsx`

Print-optimized table with alternating rows.

| Prop | Type | Description |
|------|------|-------------|
| `columns` | `Column[]` | Column definitions |
| `data` | `Row[]` | Table data |
| `showTotals` | `boolean` | Show totals row |
| `totals` | `Record<string, number>` | Totals values |

**Features:**
- Alternating row colors
- Total row support
- Page break handling
- RTL support
- Print-safe styling

**Usage:**
```tsx
<PrintableReportTable
  columns={[
    { key: 'name', header: 'الاسم', width: '40%' },
    { key: 'amount', header: 'المبلغ', width: '30%', type: 'currency' },
    { key: 'date', header: 'التاريخ', width: '30%', type: 'date' },
  ]}
  data={rows}
  showTotals
  totals={{ amount: 1500000 }}
/>
```

---

### 2.3 PrintField

**File:** `ui/src/components/ui/print/PrintField.tsx`

Print field display with 3 variants.

| Variant | Layout | Usage |
|---------|--------|-------|
| `inline` | Horizontal | Compact fields |
| `column` | Vertical | Label above value |
| `row` | Label + value row | Standard field |

**Props:**
| Prop | Type | Description |
|------|------|-------------|
| `label` | `string` | Field label |
| `value` | `ReactNode` | Field value |
| `variant` | `'inline'\|'column'\|'row'` | Layout variant |
| `mono` | `boolean` | Monospace value font |

**Usage:**
```tsx
<PrintField variant="row" label="رقم الملف" value="SA-2026-00001" />
<PrintField variant="column" label="الاسم" value="محمد أحمد" />
<PrintField variant="inline" label="المبلغ" value="1,500,000" mono />
```

---

### 2.4 PrintSectionHeader

**File:** `ui/src/components/ui/print/PrintSectionHeader.tsx`

Print section header with 2 variants.

| Variant | Style | Usage |
|---------|-------|-------|
| `box` | Boxed header | Major sections |
| `line` | Underlined header | Minor sections |

**Props:**
| Prop | Type | Description |
|------|------|-------------|
| `title` | `string` | Section title |
| `variant` | `'box'\|'line'` | Header variant |
| `number` | `number` | Section number |
| `icon` | `ReactNode` | Optional icon |

**Usage:**
```tsx
<PrintSectionHeader variant="box" number={1} title="البيانات الأساسية" />
<PrintSectionHeader variant="line" number={2} title="الأشخاص" />
```

---

### 2.5 PrintSignatureBlock

**File:** `ui/src/components/ui/print/PrintSignatureBlock.tsx`

Signature block for print documents.

| Prop | Type | Description |
|------|------|-------------|
| `signatures` | `Signature[]` | Signature definitions |
| `layout` | `'grid'\|'inline'` | Layout variant |

**Signature Interface:**
```typescript
interface Signature {
  label: string;      // "توقيع المدير"
  name?: string;      // "محمد أحمد"
  title?: string;     // "مدير القسم"
  showStamp?: boolean; // Show stamp circle
}
```

**Features:**
- Grid layout for multiple signatures
- Stamp circle for official seals
- Dashed signature line
- RTL support

**Usage:**
```tsx
<PrintSignatureBlock
  layout="grid"
  signatures={[
    { label: 'إعداد', name: 'أحمد محمد', title: 'محرر' },
    { label: 'مراجعة', name: 'محمد علي', title: 'مراجع' },
    { label: 'اعتماد', name: 'علي أحمد', title: 'مدير', showStamp: true },
  ]}
/>
```

---

### 2.6 PrintableReport

**File:** `ui/src/components/ui/print/PrintableReport.tsx`

Report wrapper component.

| Prop | Type | Description |
|------|------|-------------|
| `title` | `string` | Report title |
| `subtitle` | `string` | Report subtitle |
| `header` | `ReactNode` | Custom header |
| `footer` | `ReactNode` | Custom footer |
| `children` | `ReactNode` | Report content |

---

## 3. Common Print Components

### 3.1 PrintSummaryBox

**File:** `ui/src/components/common/PrintSummaryBox.tsx`

Summary box with 2 variants.

| Variant | Style | Usage |
|---------|-------|-------|
| `modern` | Clean, minimal | Modern reports |
| `classic` | Decorative borders | Traditional documents |

**Props:**
| Prop | Type | Description |
|------|------|-------------|
| `items` | `SummaryItem[]` | Key-value pairs |
| `variant` | `'modern'\|'classic'` | Box variant |

**Usage:**
```tsx
<PrintSummaryBox
  variant="modern"
  items={[
    { label: 'عدد المتقاعدين', value: '1,234' },
    { label: 'إجمالي المبالغ', value: '15,000,000' },
    { label: 'تاريخ التقرير', value: '05/08/2026' },
  ]}
/>
```

---

### 3.2 PrintSignatureBlock (Common)

**File:** `ui/src/components/common/PrintSignatureBlock.tsx`

Common signature block with 2 variants.

| Variant | Style | Usage |
|---------|-------|-------|
| `default` | Simple signature | Standard documents |
| `boxes` | Boxed signatures | Official documents |

---

### 3.3 ReportKPICards

**File:** `ui/src/components/common/ReportKPICards.tsx`

KPI cards for reports.

**Props:**
| Prop | Type | Description |
|------|------|-------------|
| `kpis` | `KpiItem[]` | KPI definitions |
| `variant` | `'default'\|'compact'` | Card variant |

---

## 4. Print CSS

### 4.1 Page Setup

```css
@media print {
  @page {
    size: A4 portrait;
    margin: 15mm;
  }

  @page :first {
    margin-top: 20mm;
  }

  @page :last {
    margin-bottom: 25mm;
  }
}
```

### 4.2 Hide Non-Print Elements

```css
@media print {
  .no-print { display: none !important; }
  .print-only { display: block !important; }
}
```

### 4.3 Page Breaks

```css
@media print {
  .page-break { page-break-before: always; }
  .break-inside-avoid { break-inside: avoid; }
  .break-after-avoid { break-after: avoid; }
}
```

### 4.4 Background Colors

```css
@media print {
  .print-bg { print-color-adjust: exact; }
  * { print-color-adjust: exact; }
}
```

### 4.5 Double-Border Frame

```css
.print-frame {
  border: 2px solid #000;
  outline: 1px solid #000;
  outline-offset: 4px;
  padding: 20px;
}
```

### 4.6 Watermark

```css
.print-watermark {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%) rotate(-45deg);
  font-size: 72px;
  color: rgba(0, 0, 0, 0.05);
  pointer-events: none;
  z-index: -1;
}
```

---

## 5. Print Typography

### 5.1 Font Sizes

```css
@media print {
  body { font-size: 12pt; }
  h1 { font-size: 18pt; }
  h2 { font-size: 16pt; }
  h3 { font-size: 14pt; }
  h4 { font-size: 12pt; }
  .text-print-xs { font-size: 8pt; }
  .text-print-sm { font-size: 10pt; }
  .text-print-base { font-size: 12pt; }
  .text-print-lg { font-size: 14pt; }
}
```

### 5.2 Line Heights

```css
@media print {
  .leading-print-tight { line-height: 1.2; }
  .leading-print-normal { line-height: 1.5; }
  .leading-print-relaxed { line-height: 1.8; }
}
```

---

## 6. Print Colors

### 6.1 Print-Safe Colors

| Color | Hex | Usage |
|-------|-----|-------|
| Black | #000000 | Primary text |
| Dark Gray | #333333 | Secondary text |
| Medium Gray | #666666 | Muted text |
| Light Gray | #999999 | Borders |
| White | #FFFFFF | Backgrounds |

### 6.2 Avoid in Print

- Bright colors (may not print correctly)
- Transparent backgrounds
- Gradients (may not render)
- Small text in color (may be hard to read)

---

## 7. Print Patterns

### 7.1 Standard Report Structure

```tsx
function RetirementReport() {
  return (
    <PrintLayout pageSize="A4" watermark="سري">
      <PrintHeader />

      <PrintSectionHeader variant="box" number={1} title="ملخص التقرير" />
      <PrintSummaryBox items={summaryItems} variant="modern" />

      <PrintSectionHeader variant="box" number={2} title="قائمة المتقاعدين" />
      <PrintableReportTable columns={columns} data={data} showTotals />

      <PrintSectionHeader variant="box" number={3} title="التوقيعات" />
      <PrintSignatureBlock signatures={signatures} />

      <PrintFooter />
    </PrintLayout>
  );
}
```

### 7.2 File Details Report

```tsx
function FileDetailsReport({ file }) {
  return (
    <PrintLayout pageSize="A4">
      <PrintSectionHeader variant="box" title="بيانات الملف" />

      <div className="grid grid-cols-2 gap-4">
        <PrintField variant="row" label="رقم الملف" value={file.fileNumber} />
        <PrintField variant="row" label="الحالة" value={file.status} />
        <PrintField variant="row" label="التاريخ" value={file.createdAt} />
        <PrintField variant="row" label="الفرع" value={file.branch} />
      </div>

      <PrintSectionHeader variant="box" title="المتقاعد" />
      <div className="grid grid-cols-2 gap-4">
        <PrintField variant="row" label="الاسم" value={file.retiree.fullName} />
        <PrintField variant="row" label="رقم الهوية" value={file.retiree.nationalId} />
      </div>

      <PrintSectionHeader variant="box" title="الوثائق" />
      <PrintableReportTable columns={docColumns} data={file.documents} />
    </PrintLayout>
  );
}
```

### 7.3 Salary Batch Report

```tsx
function SalaryBatchReport({ batch }) {
  return (
    <PrintLayout pageSize="A4">
      <PrintSectionHeader variant="box" title="دفعة الرواتب" />

      <PrintSummaryBox
        variant="classic"
        items={[
          { label: 'رقم الدفعة', value: batch.batchNumber },
          { label: 'الشهر', value: batch.month },
          { label: 'السنة', value: batch.year },
          { label: 'الإجمالي', value: batch.totalAmount },
        ]}
      />

      <PrintSectionHeader variant="line" title="تفاصيل الصرف" />
      <PrintableReportTable
        columns={salaryColumns}
        data={batch.items}
        showTotals
        totals={{ amount: batch.totalAmount }}
      />

      <PrintSignatureBlock
        signatures={[
          { label: 'إعداد', showStamp: false },
          { label: 'اعتماد', showStamp: true },
        ]}
      />
    </PrintLayout>
  );
}
```

---

## 8. Print Utilities

### 8.1 Print Trigger

```tsx
function PrintButton({ onPrint }) {
  return (
    <Button onClick={() => window.print()} className="no-print">
      <Printer className="h-4 w-4 me-2" />
      طباعة
    </Button>
  );
}
```

### 8.2 Print Stylesheet

```tsx
// In component
<link rel="stylesheet" href="/print.css" media="print" />

// Or inline
<style>{`
  @media print {
    /* Print-specific styles */
  }
`}</style>
```

### 8.3 Print Hook

```typescript
function usePrint() {
  const print = useCallback(() => {
    window.print();
  }, []);

  const printElement = useCallback((elementId: string) => {
    const element = document.getElementById(elementId);
    if (element) {
      const printWindow = window.open('', '_blank');
      printWindow.document.write(`
        <html>
          <head>
            <title>طباعة</title>
            <style>/* Print styles */</style>
          </head>
          <body>
            ${element.innerHTML}
          </body>
        </html>
      `);
      printWindow.print();
      printWindow.close();
    }
  }, []);

  return { print, printElement };
}
```

---

## 9. File Structure

```
ui/src/components/
├── ui/print/
│   ├── PrintLayout.tsx              # Page layout
│   ├── PrintableReportTable.tsx     # Print table
│   ├── PrintField.tsx               # Print field
│   ├── PrintSectionHeader.tsx       # Section header
│   ├── PrintSignatureBlock.tsx      # Signature block
│   └── PrintableReport.tsx          # Report wrapper
├── common/
│   ├── PrintSummaryBox.tsx          # Summary box
│   ├── PrintSignatureBlock.tsx      # Common signature
│   └── ReportKPICards.tsx           # KPI cards
└── print.css                        # Print styles (optional)
```

---

## 10. Best Practices

### 10.1 Do's

- ✅ Use `break-inside-avoid` for important sections
- ✅ Use `no-print` class for non-print elements
- ✅ Use print-safe colors (black, gray, white)
- ✅ Use logical properties for RTL (`ps-`, `pe-`, `ms-`, `me-`)
- ✅ Test print preview before finalizing
- ✅ Use `@page` rules for page setup
- ✅ Use `print-color-adjust: exact` for backgrounds

### 10.2 Don'ts

- ❌ Don't use bright colors (may not print)
- ❌ Don't use transparent backgrounds
- ❌ Don't use gradients
- ❌ Don't use small text in color
- ❌ Don't use complex layouts that may break
- ❌ Don't forget to hide non-print elements
- ❌ Don't use fixed positioning in print
