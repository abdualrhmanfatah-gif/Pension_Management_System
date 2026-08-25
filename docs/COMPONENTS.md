# UI Components Library

## Pension Management System — Complete Component Reference

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Total Components** | 54 |
| **Location** | `ui/src/components/ui/` |
| **Styling** | Tailwind CSS + CVA (class-variance-authority) |
| **Primitives** | Radix UI |
| **RTL Support** | Full (Arabic primary) |
| **Print Support** | Dedicated print components |

### Component Categories

| Category | Count | Components |
|----------|-------|------------|
| **Core UI** | 12 | Button, Badge, Input, Select, Checkbox, Slider, Popover, ScrollArea, Separator, Tabs, Accordion, NativeSelect |
| **Layout** | 4 | Card, Dialog, Sheet, Stepper |
| **Data Display** | 6 | DataTable, StatCard, SummaryCard, QuickStats, InfoPair, AuditTimeline |
| **Feedback** | 7 | AlertBanner, ErrorAlert, NoteBox, ConfirmDialog, AlertDialog, EmptyState, RecordStateBadge |
| **Form** | 5 | FormField, BaseFormDialog, BaseSheet, FormDialogFooter, FileField |
| **Navigation** | 2 | Breadcrumb, Typography |
| **Media** | 3 | FileUploader, ImageCropperDialog, CornerEditor |
| **Print** | 6 | PrintLayout, PrintableReportTable, PrintField, PrintSectionHeader, PrintSignatureBlock, PrintableReport |
| **Loading** | 2 | Loader, Skeleton |
| **Utility** | 7 | LazyQRCode, EditableImage, TabbedFormSection, FormMessage, Avatar, Tooltip, CommandPalette |

---

## 2. Core UI Components

### 2.1 Button

**File:** `ui/src/components/ui/Button.tsx`

Multi-variant button component using CVA.

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `variant` | `'default'\|'destructive'\|'outline'\|'secondary'\|'ghost'\|'link'\|'success'\|'warning'\|'info'\|'danger'` | `'default'` | Visual variant |
| `size` | `'default'\|'sm'\|'lg'\|'icon'\|'xl'` | `'default'` | Button size |
| `loading` | `boolean` | `false` | Shows spinner, disables button |
| `asChild` | `boolean` | `false` | Render as child element (Radix Slot) |

**Variants:**
```typescript
// buttonVariants.ts
default:     "bg-primary text-primary-foreground hover:bg-primary/90"
destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90"
outline:     "border border-input bg-background hover:bg-accent hover:text-accent-foreground"
secondary:   "bg-secondary text-secondary-foreground hover:bg-secondary/80"
ghost:       "hover:bg-accent hover:text-accent-foreground"
link:        "text-primary underline-offset-4 hover:underline"
success:     "bg-success text-success-foreground hover:bg-success/90"
warning:     "bg-warning text-warning-foreground hover:bg-warning/90"
info:        "bg-info text-info-foreground hover:bg-info/90"
danger:      "bg-danger text-danger-foreground hover:bg-danger/90"
```

**Usage:**
```tsx
<Button variant="default" size="lg" loading={isSaving}>
  حفظ
</Button>

<Button variant="destructive" size="sm">
  حذف
</Button>

<Button variant="outline" size="icon">
  <Settings className="h-4 w-4" />
</Button>
```

---

### 2.2 Badge

**File:** `ui/src/components/ui/Badge.tsx`

Status badge with dot indicator and multiple variants.

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `variant` | `'default'\|'secondary'\|'destructive'\|'outline'\|'success'\|'warning'\|'info'` | `'default'` | Badge variant |
| `dot` | `boolean` | `false` | Show status dot indicator |
| `className` | `string` | — | Custom classes |

**Variants:**
```typescript
default:     "bg-primary/10 text-primary border-primary/20"
secondary:   "bg-secondary/10 text-secondary-foreground border-secondary/20"
destructive: "bg-destructive/10 text-destructive border-destructive/20"
outline:     "text-foreground border-border"
success:     "bg-success/10 text-success border-success/20"
warning:     "bg-warning/10 text-warning border-warning/20"
info:        "bg-info/10 text-info border-info/20"
```

**Usage:**
```tsx
<Badge variant="success" dot>نشط</Badge>
<Badge variant="destructive">مرفوض</Badge>
<Badge variant="secondary">قيد المراجعة</Badge>
```

---

### 2.3 Input

**File:** `ui/src/components/ui/Input.tsx`

Text input with optional icon, password toggle, and wrapper.

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `icon` | `ReactNode` | — | Left icon |
| `error` | `boolean` | `false` | Error state styling |
| `className` | `string` | — | Custom classes |

**Password Features:**
- Auto-detects `type="password"`
- Shows show/hide toggle button
- RTL-aware icon positioning

**DateInput Variant:**
```tsx
<Input type="date" />
// Forces LTR for date picker, RTL for labels
```

---

### 2.4 Select

**File:** `ui/src/components/ui/Select.tsx`

Radix-based select with RTL support.

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `label` | `string` | — | Label text |
| `error` | `string` | — | Error message |
| `placeholder` | `string` | `'اختر...'` | Placeholder text |
| `options` | `{value: string, label: string}[]` | — | Select options |

**RTL Behavior:**
- Default direction is RTL
- Scroll button arrows flip for RTL
- Dropdown aligns to start (right in RTL)

**Usage:**
```tsx
<Select
  label="ال status"
  options={[
    { value: "1", label: "نشط" },
    { value: "2", label: "غير نشط" }
  ]}
  error={errors.status?.message}
/>
```

---

### 2.5 Checkbox

**File:** `ui/src/components/ui/Checkbox.tsx`

Simple checkbox input.

---

### 2.6 Slider

**File:** `ui/src/components/ui/Slider.tsx`

Radix Slider for numeric ranges.

---

### 2.7 Popover

**File:** `ui/src/components/ui/Popover.tsx`

Radix Popover for dropdown content.

---

### 2.8 ScrollArea

**File:** `ui/src/components/ui/ScrollArea.tsx`

Radix ScrollArea with custom scrollbar styling.

---

### 2.9 Separator

**File:** `ui/src/components/ui/Separator.tsx`

Radix Separator (horizontal/vertical).

---

### 2.10 Tabs

**File:** `ui/src/components/ui/Tabs.tsx`

Radix Tabs with RTL default direction.

| Prop | Type | Description |
|------|------|-------------|
| `tabs` | `{id: string, label: string, icon?: ReactNode}[]` | Tab definitions |
| `activeTab` | `string` | Currently active tab |
| `onTabChange` | `(id: string) => void` | Tab change handler |

**Features:**
- RTL direction by default
- Keyboard navigation (ArrowLeft/Right)
- Scrollable on mobile
- Animated indicator

---

### 2.11 Accordion

**File:** `ui/src/components/ui/Accordion.tsx`

Radix Accordion for collapsible content.

---

### 2.12 NativeSelect

**File:** `ui/src/components/ui/NativeSelect.tsx`

Fallback native HTML select for simple cases.

---

## 3. Layout Components

### 3.1 Card

**File:** `ui/src/components/ui/Card.tsx`

Compound card component.

| Component | Purpose |
|-----------|---------|
| `Card` | Main container |
| `CardHeader` | Header section |
| `CardTitle` | Title text |
| `CardDescription` | Description text |
| `CardContent` | Main content |
| `CardFooter` | Footer actions |

**Usage:**
```tsx
<Card>
  <CardHeader>
    <CardTitle>بيانات المتقاعد</CardTitle>
    <CardDescription>المعلومات الأساسية</CardDescription>
  </CardHeader>
  <CardContent>
    {/* Content */}
  </CardContent>
  <CardFooter>
    <Button>حفظ</Button>
  </CardFooter>
</Card>
```

---

### 3.2 Dialog

**File:** `ui/src/components/ui/Dialog.tsx`

Custom modal dialog with focus trap and scroll lock.

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `size` | `'sm'\|'md'\|'lg'\|'xl'\|'2xl'\|'3xl'\|'4xl'\|'5xl'\|'full'` | `'md'` | Dialog size |
| `open` | `boolean` | — | Controlled open state |
| `onOpenChange` | `(open: boolean) => void` | — | Open change handler |

**Sizes:**
```
sm:  max-w-sm (24rem)
md:  max-w-md (28rem)
lg:  max-w-lg (32rem)
xl:  max-w-xl (36rem)
2xl: max-w-2xl (42rem)
3xl: max-w-3xl (48rem)
4xl: max-w-4xl (56rem)
5xl: max-w-5xl (64rem)
full: max-w-[95vw]
```

**Features:**
- Custom React Portal rendering
- Focus trap with `@radix-ui/react-focus-scope`
- Scroll lock on body
- Backdrop click to close
- Escape key to close
- Animated entrance/exit

---

### 3.3 Sheet

**File:** `ui/src/components/ui/Sheet.tsx`

Side panel component (4 sides).

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `side` | `'top'\|'right'\|'bottom'\|'left'` | `'right'` | Panel position |
| `size` | `'sm'\|'md'\|'lg'\|'xl'\|'full'` | `'md'` | Panel width |

**CVA Variants:**
```typescript
sheetVariants: {
  top:    "inset-x-0 top-0 border-b"
  bottom: "inset-x-0 bottom-0 border-t"
  left:   "inset-y-0 left-0 h-full w-3/4 border-r sm:max-w-sm"
  right:  "inset-y-0 right-0 h-full w-3/4 border-l sm:max-w-sm"
}
```

---

### 3.4 Stepper

**File:** `ui/src/components/ui/Stepper.tsx`

Step-by-step wizard indicator.

| Prop | Type | Description |
|------|------|-------------|
| `steps` | `{id: string, label: string, description?: string}[]` | Step definitions |
| `currentStep` | `number` | Current step index (0-based) |
| `onStepClick` | `(index: number) => void` | Step click handler |

**Variants:**
- **Desktop:** Horizontal line with circles
- **Mobile:** Compact vertical list

**Features:**
- Pulse animation on current step
- Completed steps show checkmark
- Clickable completed steps
- Error state support

---

## 4. Data Display Components

### 4.1 DataTable

**File:** `ui/src/components/ui/DataTable.tsx`

Feature-rich data table with virtualization.

| Prop | Type | Description |
|------|------|-------------|
| `columns` | `ColumnDef<T>[]` | Column definitions (TanStack Table) |
| `data` | `T[]` | Table data |
| `loading` | `boolean` | Loading state |
| `pagination` | `{page, pageSize, total, onChange}` | Server-side pagination |
| `virtualization` | `boolean` | Enable row virtualization |
| `searchable` | `boolean` | Show search input |
| `exportable` | `boolean` | Show export button |
| `selectable` | `boolean` | Enable row selection |

**Features:**
- **Virtualization:** Handles 10k+ rows via `@tanstack/react-virtual`
- **Dual Pagination:** Server-side (top) + Client-side (bottom)
- **Mobile:** Card-based layout on small screens
- **Drawer:** Mobile detail view in sheet
- **RTL:** Full right-to-left support
- **Export:** Excel/CSV/PDF export
- **Bulk Actions:** Multi-row selection

**Usage:**
```tsx
<DataTable
  columns={columns}
  data={data}
  loading={isLoading}
  pagination={{ page, pageSize, total, onChange: setPage }}
  searchable
  exportable
/>
```

---

### 4.2 StatCard

**File:** `ui/src/components/ui/StatCard.tsx`

Dashboard statistic card with trends.

| Prop | Type | Description |
|------|------|-------------|
| `title` | `string` | Stat label |
| `value` | `string\|number` | Stat value |
| `icon` | `ReactNode` | Icon component |
| `trend` | `{value: number, isPositive: boolean}` | Trend indicator |
| `color` | `'primary'\|'success'\|'warning'\|'destructive'\|'info'\|'muted'` | Color theme |

**15 Color Themes:**
```typescript
primary, success, warning, destructive, info, muted,
blue, purple, emerald, amber, rose, cyan, orange, indigo, teal
```

**Features:**
- Decorative glow effect
- Trend arrow with percentage
- Responsive sizing
- Print-safe (no glow in print)

---

### 4.3 SummaryCard

**File:** `ui/src/components/ui/SummaryCard.tsx`

Grid layout for key-value pairs.

---

### 4.4 QuickStats

**File:** `ui/src/components/ui/QuickStats.tsx`

Inline metrics display.

| Variant | Description |
|---------|-------------|
| `default` | Standard stat boxes |
| `compact` | Smaller, tighter layout |
| `cards` | Card-based layout |

---

### 4.5 InfoPair

**File:** `ui/src/components/ui/InfoPair.tsx`

Label-value pair display.

| Prop | Type | Description |
|------|------|-------------|
| `label` | `string` | Label text |
| `value` | `ReactNode` | Value content |
| `layout` | `'row'\|'column'` | Layout direction |
| `mono` | `boolean` | Monospace value font |

---

### 4.6 AuditTimeline

**File:** `ui/src/components/ui/AuditTimeline.tsx`

Change history timeline with field diffs.

---

## 5. Feedback Components

### 5.1 AlertBanner

**File:** `ui/src/components/ui/AlertBanner.tsx`

Dismissible alert banner.

| Variant | Color | Use Case |
|---------|-------|----------|
| `info` | Blue | Informational messages |
| `success` | Green | Success confirmations |
| `warning` | Yellow | Warnings |
| `destructive` | Red | Errors |

**Props:**
| Prop | Type | Description |
|------|------|-------------|
| `variant` | `'info'\|'success'\|'warning'\|'destructive'` | Alert type |
| `title` | `string` | Alert title |
| `dismissible` | `boolean` | Show close button |
| `action` | `{label: string, onClick: () => void}` | Action button |

---

### 5.2 ErrorAlert

**File:** `ui/src/components/ui/ErrorAlert.tsx`

Error display with slide-in animation.

**Features:**
- Red palette styling
- Slide-in animation
- Retry button support
- Error details expansion

---

### 5.3 NoteBox

**File:** `ui/src/components/ui/NoteBox.tsx`

Styled note/callout box.

| Variant | Icon | Color |
|---------|------|-------|
| `info` | ℹ️ | Blue |
| `warning` | ⚠️ | Yellow |
| `success` | ✓ | Green |
| `help` | ? | Purple |

---

### 5.4 ConfirmDialog

**File:** `ui/src/components/ui/ConfirmDialog.tsx`

Async confirmation dialog.

| Variant | Color | Use Case |
|---------|-------|----------|
| `danger` | Red | Destructive actions |
| `warning` | Yellow | Caution actions |
| `info` | Blue | Informational |
| `success` | Green | Positive actions |

**Props:**
| Prop | Type | Description |
|------|------|-------------|
| `open` | `boolean` | Controlled open state |
| `onConfirm` | `() => Promise<void>` | Async confirm handler |
| `variant` | `'danger'\|'warning'\|'info'\|'success'` | Dialog variant |
| `title` | `string` | Dialog title |
| `description` | `string` | Dialog description |
| `confirmText` | `string` | Confirm button text |
| `cancelText` | `string` | Cancel button text |

**Features:**
- Loading state during async operations
- Keyboard shortcuts (Ctrl+S save, Esc close)
- Focus trap
- Scroll lock

---

### 5.5 AlertDialog

**File:** `ui/src/components/ui/AlertDialog.tsx`

Radix AlertDialog wrapper.

---

### 5.6 EmptyState

**File:** `ui/src/components/ui/EmptyState.tsx`

Empty state placeholder.

| Prop | Type | Description |
|------|------|-------------|
| `icon` | `ReactNode` | Center icon |
| `title` | `string` | Empty state title |
| `description` | `string` | Description text |
| `action` | `{label: string, onClick: () => void}` | Action button |

---

### 5.7 RecordStateBadge

**File:** `ui/src/components/ui/RecordStateBadge.tsx`

Status badge with icons.

| State | Icon | Color | Arabic |
|-------|------|-------|--------|
| `active` | ✓ | Green | نشط |
| `inactive` | ✗ | Gray | غير نشط |
| `pending` | ◷ | Yellow | قيد الانتظار |
| `suspended` | ⏸ | Red | معلق |
| `completed` | ✓✓ | Blue | مكتمل |
| `archived` | 📁 | Muted | أرشيف |
| `rejected` | ✗ | Red | مرفوض |

---

## 6. Form Components

### 6.1 FormField

**File:** `ui/src/components/ui/FormField.tsx`

Form field wrapper with label and error.

| Prop | Type | Description |
|------|------|-------------|
| `label` | `string` | Field label |
| `required` | `boolean` | Show required indicator |
| `error` | `string` | Error message |
| `hint` | `string` | Help text |
| `htmlFor` | `string` | Label association |

**Features:**
- Auto-generated `id` for accessibility
- `aria-required` for screen readers
- `aria-invalid` on error
- `aria-describedby` linking error/hint

---

### 6.2 BaseFormDialog

**File:** `ui/src/components/ui/BaseFormDialog.tsx`

Dialog-based form with dual API.

| Prop | Type | Description |
|------|------|-------------|
| `open` | `boolean` | Controlled open state |
| `onOpenChange` | `(open: boolean) => void` | Open change handler |
| `onSubmit` | `(data: T) => Promise<void>` | Submit handler |
| `onSaveAndNew` | `(data: T) => Promise<void>` | Save & new handler |
| `title` | `string` | Dialog title |
| `size` | Dialog sizes | Dialog size |

**Features:**
- **Ctrl+S:** Save (with Radix popper awareness)
- **Dirty Warning:** Confirm before close if form dirty
- **Dual API:** Controlled (`open`/`onOpenChange`) or Legacy (`isOpen`/`onClose`)
- **Loading State:** Spinner during submit
- **Error Handling:** API error display

---

### 6.3 BaseSheet

**File:** `ui/src/components/ui/BaseSheet.tsx`

Side panel form (same pattern as BaseFormDialog).

---

### 6.4 FormDialogFooter

**File:** `ui/src/components/ui/FormDialogFooter.tsx`

Form dialog footer with keyboard hints.

| Prop | Type | Description |
|------|------|-------------|
| `onSave` | `() => void` | Save handler |
| `onSaveAndNew` | `() => void` | Save & new handler |
| `onCancel` | `() => void` | Cancel handler |
| `isSaving` | `boolean` | Loading state |

**Keyboard Hints:**
```
Ctrl+S → حفظ
Ctrl+N → حفظ و جديد
Esc → إلغاء
```

---

### 6.5 FileField

**File:** `ui/src/components/ui/FileField.tsx`

File input field wrapper.

---

## 7. Navigation Components

### 7.1 Breadcrumb

**File:** `ui/src/components/ui/Breadcrumb.tsx`

Navigation breadcrumb with RTL support.

**Features:**
- ChevronLeft separator (RTL-aware)
- Truncation on long items
- Active item styling

---

### 7.2 Typography

**File:** `ui/src/components/ui/Typography.tsx`

Text component with variants.

| Component | Variants |
|-----------|----------|
| `Heading` | `h1`, `h2`, `h3`, `h4` |
| `Text` | `default`, `muted`, `lead`, `large`, `small`, `label`, `error` |

**Usage:**
```tsx
<Heading.h1>العنوان الرئيسي</Heading.h1>
<Heading.h2>عنوان فرعي</Heading.h2>
<Text.muted>نص ثانوي</Text.muted>
<Text.lead>نص رئيسي طويل</Text.lead>
```

---

## 8. Media Components

### 8.1 FileUploader

**File:** `ui/src/components/ui/FileUploader.tsx`

File upload with 3 modes.

| Mode | Description |
|------|-------------|
| `upload` | Drag-and-drop or click |
| `camera` | Device camera capture |
| `scanner` | TWAIN/WIA scanner integration |

**Features:**
- Image preview
- File type validation
- Size limit validation
- Image cropper integration
- Progress indicator

---

### 8.2 ImageCropperDialog

**File:** `ui/src/components/ui/ImageCropperDialog.tsx`

Image cropping with WebGL/CPU processors.

| Feature | Description |
|---------|-------------|
| **WebGL Processor** | GPU-accelerated for large images |
| **CPU Processor** | Fallback for older browsers |
| **Canvas Pool** | Memory-efficient processing |
| **Auto Corners** | Automatic document corner detection |
| **Corner Editor** | Manual corner adjustment |

---

### 8.3 CornerEditor

**File:** `ui/src/components/ui/CornerEditor.tsx`

Canvas-based quad point editor.

| Feature | Description |
|---------|-------------|
| **Touch Drag** | Mobile-friendly dragging |
| **60fps RAF** | Smooth animation |
| **Quad Points** | 4-corner document perspective |

---

## 9. Loading Components

### 9.1 Loader

**File:** `ui/src/components/ui/Loader.tsx`

Loading spinner with 3 variants.

| Variant | Description |
|---------|-------------|
| `default` | Single spinner |
| `compact` | Smaller spinner |
| `security` | Multi-ring spinner |

**Features:**
- Multi-ring animation
- Customizable size
- RTL-aware

---

### 9.2 Skeleton

**File:** `ui/src/components/ui/Skeleton.tsx`

Loading placeholder with presets.

| Preset | Description |
|--------|-------------|
| `Table` | Table row skeletons |
| `Profile` | Profile card skeleton |
| `TabContent` | Tab content skeleton |
| `default` | Basic rectangle |

---

## 10. Print Components

### 10.1 PrintLayout

**File:** `ui/src/components/ui/print/PrintLayout.tsx`

Print page layout with header/footer.

| Prop | Type | Description |
|------|------|-------------|
| `pageSize` | `'A4'\|'A3'\|'A5'\|'letter'` | Page size |
| `orientation` | `'portrait'\|'landscape'` | Page orientation |
| `watermark` | `string` | Watermark text |
| `header` | `ReactNode` | Page header |
| `footer` | `ReactNode` | Page footer |

**Features:**
- Double-border frame
- Print-specific CSS
- `@page` rules
- `break-inside: avoid`

---

### 10.2 PrintableReportTable

**File:** `ui/src/components/ui/print/PrintableReportTable.tsx`

Print-optimized table.

**Features:**
- Alternating row colors
- Total row support
- Page break handling
- RTL support

---

### 10.3 PrintField

**File:** `ui/src/components/ui/print/PrintField.tsx`

Print field display.

| Variant | Description |
|---------|-------------|
| `inline` | Horizontal layout |
| `column` | Vertical layout |
| `row` | Label + value row |

---

### 10.4 PrintSectionHeader

**File:** `ui/src/components/ui/print/PrintSectionHeader.tsx`

Print section header.

| Variant | Description |
|---------|-------------|
| `box` | Boxed header |
| `line` | Underlined header |

---

### 10.5 PrintSignatureBlock

**File:** `ui/src/components/ui/print/PrintSignatureBlock.tsx`

Signature block for print documents.

**Features:**
- Grid layout
- Stamp circle
- Dashed signature line

---

### 10.6 PrintableReport

**File:** `ui/src/components/ui/print/PrintableReport.tsx`

Report wrapper component.

---

## 11. Utility Components

### 11.1 CommandPalette

**File:** `ui/src/components/layout/CommandPalette.tsx`

Quick search command palette.

**Features:**
- `Ctrl+K` to toggle
- Search across navigation
- Keyboard navigation
- RTL support

---

### 11.2 NotificationBell

**File:** `ui/src/components/layout/NotificationBell.tsx`

Notification bell with dropdown.

**Features:**
- Unread count badge
- Category-based navigation
- Mark as read
- Mark all as read
- Real-time updates (SignalR)

---

### 11.3 LazyQRCode

**File:** `ui/src/components/ui/LazyQRCode.tsx`

Lazy-loaded QR code generator.

---

### 11.4 EditableImage

**File:** `ui/src/components/ui/EditableImage.tsx`

Inline image edit with upload.

---

### 11.5 TabbedFormSection

**File:** `ui/src/components/ui/TabbedFormSection.tsx`

Tabbed form sections.

---

### 11.6 FormMessage

**File:** `ui/src/components/ui/FormMessage.tsx`

Form message display.

---

## 12. Component Patterns

### 12.1 Compound Components

```tsx
// Card pattern
<Card>
  <CardHeader>
    <CardTitle>Title</CardTitle>
  </CardHeader>
  <CardContent>Content</CardContent>
</Card>

// Form dialog pattern
<BaseFormDialog open={open} onOpenChange={setOpen}>
  <FormField label="Name">
    <Input />
  </FormField>
  <FormDialogFooter onSave={handleSave} />
</BaseFormDialog>
```

### 12.2 Controlled/Uncontrolled Dual API

```tsx
// Controlled
<BaseFormDialog open={open} onOpenChange={setOpen}>

// Legacy (uncontrolled)
<BaseFormDialog isOpen={isOpen} onClose={handleClose}>
```

### 12.3 CVA Variants

```typescript
// buttonVariants.ts
const buttonVariants = cva(
  "inline-flex items-center justify-center rounded-md text-sm font-medium",
  {
    variants: {
      variant: {
        default: "bg-primary text-primary-foreground",
        destructive: "bg-destructive text-destructive-foreground",
        // ...
      },
      size: {
        default: "h-10 px-4 py-2",
        sm: "h-9 rounded-md px-3",
        lg: "h-11 rounded-md px-8",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
);
```

### 12.4 RTL Patterns

```tsx
// Padding/margin
<p className="ps-4">Start padding (RTL-aware)</p>
<p className="pe-4">End padding (RTL-aware)</p>
<p className="ms-2">Start margin (RTL-aware)</p>

// Text alignment
<p className="text-start">Left in LTR, Right in RTL</p>
<p className="text-end">Right in LTR, Left in RTL</p>

// Borders
<div className="border-s-2">Start border</div>
<div className="border-e-2">End border</div>

// Icons
<Icon className="rtl:rotate-180" /> // Flip for RTL
```

---

## 13. Accessibility

### 13.1 Focus Management

```tsx
// Focus ring
<Button className="focus-visible:ring-2 focus-visible:ring-primary">

// Focus trap (Dialog)
<Dialog>
  {/* Focus trapped inside */}
</Dialog>
```

### 13.2 ARIA Attributes

```tsx
// Required field
<FormField label="Name" required>
  <Input aria-required="true" />
</FormField>

// Error state
<FormField error="Required">
  <Input aria-invalid="true" aria-describedby="error-name" />
</FormField>

// Loading state
<Button loading aria-busy="true">Save</Button>
```

### 13.3 Keyboard Navigation

```tsx
// Tabs
<Tabs>
  {/* ArrowLeft/Right to navigate */}
</Tabs>

// DataTable
<DataTable>
  {/* ArrowUp/Down to navigate rows */}
  {/* Enter to select */}
  {/* Space to toggle selection */}
</DataTable>

// CommandPalette
<CommandPalette>
  {/* Ctrl+K to open */}
  {/* ArrowUp/Down to navigate */}
  {/* Enter to select */}
  {/* Esc to close */}
</CommandPalette>
```

---

## 14. File Structure

```
ui/src/components/ui/
├── index.ts                    # Barrel exports
├── Button.tsx                  # Button component
├── buttonVariants.ts           # CVA button variants
├── Badge.tsx                   # Badge component
├── badgeVariants.ts            # CVA badge variants
├── Input.tsx                   # Input with icon/password
├── Select.tsx                  # Radix Select
├── Checkbox.tsx                # Checkbox
├── Slider.tsx                  # Radix Slider
├── Popover.tsx                 # Radix Popover
├── ScrollArea.tsx              # Radix ScrollArea
├── Separator.tsx               # Radix Separator
├── Tabs.tsx                    # Radix Tabs
├── Accordion.tsx               # Radix Accordion
├── NativeSelect.tsx            # Native select fallback
├── Card.tsx                    # Card compound component
├── Dialog.tsx                  # Custom modal dialog
├── Sheet.tsx                   # Side panel
├── Stepper.tsx                 # Step wizard
├── DataTable.tsx               # Data table with virtualization
├── StatCard.tsx                # Dashboard stat card
├── SummaryCard.tsx             # Key-value grid
├── QuickStats.tsx              # Inline metrics
├── InfoPair.tsx                # Label-value pair
├── AuditTimeline.tsx           # Change history
├── AlertBanner.tsx             # Alert banner
├── ErrorAlert.tsx              # Error display
├── NoteBox.tsx                 # Callout box
├── ConfirmDialog.tsx           # Confirmation dialog
├── AlertDialog.tsx             # Radix AlertDialog
├── EmptyState.tsx              # Empty state
├── RecordStateBadge.tsx        # Status badge
├── RecordStateBadge.config.ts  # State config
├── FormField.tsx               # Form field wrapper
├── BaseFormDialog.tsx          # Dialog form
├── BaseSheet.tsx               # Sheet form
├── FormDialogFooter.tsx        # Form footer
├── FileField.tsx               # File input
├── Breadcrumb.tsx              # Breadcrumb
├── Typography.tsx              # Text components
├── FileUploader.tsx            # File upload
├── ImageCropperDialog.tsx      # Image cropper
├── CornerEditor.tsx            # Canvas editor
├── Loader.tsx                  # Loading spinner
├── Skeleton.tsx                # Loading skeleton
├── LazyQRCode.tsx              # QR code
├── EditableImage.tsx           # Inline image edit
├── TabbedFormSection.tsx       # Tabbed sections
├── FormMessage.tsx             # Form message
├── Tooltip.tsx                 # Tooltip
└── print/
    ├── PrintLayout.tsx         # Print page layout
    ├── PrintableReportTable.tsx # Print table
    ├── PrintField.tsx          # Print field
    ├── PrintSectionHeader.tsx  # Print header
    ├── PrintSignatureBlock.tsx # Signature block
    └── PrintableReport.tsx     # Report wrapper
```

---

## 15. Import Patterns

```tsx
// Individual import
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";

// Barrel import
import { Button, Badge, Card, Dialog } from "@/components/ui";

// Print components
import { PrintLayout, PrintableReportTable } from "@/components/ui/print";

// Domain components
import { RecordStateBadge } from "@/components/ui/RecordStateBadge";
```
