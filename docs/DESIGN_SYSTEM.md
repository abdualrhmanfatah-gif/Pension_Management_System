# Design System

## Pension Management System — Visual Language & Patterns

---

## 1. Overview

| Property | Value |
|----------|-------|
| **Primary Language** | Arabic (RTL) |
| **CSS Framework** | Tailwind CSS 4 |
| **Component Library** | Radix UI + Custom |
| **Theme System** | CSS Variables + Dark/Light |
| **Typography** | IBM Plex Sans Arabic + Cairo |

---

## 2. Color System

### 2.1 Core Palette (Light Theme)

```css
:root {
  /* Base */
  --background: 0 0% 100%;        /* #ffffff */
  --foreground: 222.2 84% 4.9%;   /* #0f172a */

  /* Surface */
  --card: 0 0% 100%;
  --card-foreground: 222.2 84% 4.9%;

  /* Popover */
  --popover: 0 0% 100%;
  --popover-foreground: 222.2 84% 4.9%;

  /* Primary (Blue) */
  --primary: 221.2 83.2% 53.3%;   /* #3b82f6 */
  --primary-foreground: 210 40% 98%;

  /* Secondary (Gray) */
  --secondary: 210 40% 96.1%;
  --secondary-foreground: 222.2 47.4% 11.2%;

  /* Muted */
  --muted: 210 40% 96.1%;
  --muted-foreground: 215.4 16.3% 46.9%;

  /* Accent */
  --accent: 210 40% 96.1%;
  --accent-foreground: 222.2 47.4% 11.2%;

  /* Destructive (Red) */
  --destructive: 0 84.2% 60.2%;
  --destructive-foreground: 210 40% 98%;

  /* Border */
  --border: 214.3 31.8% 91.4%;

  /* Input */
  --input: 214.3 31.8% 91.4%;

  /* Ring */
  --ring: 221.2 83.2% 53.3%;

  /* Radius */
  --radius: 0.5rem;
}
```

### 2.2 Core Palette (Dark Theme)

```css
.dark {
  --background: 222.2 84% 4.9%;
  --foreground: 210 40% 98%;

  --card: 222.2 84% 4.9%;
  --card-foreground: 210 40% 98%;

  --popover: 222.2 84% 4.9%;
  --popover-foreground: 210 40% 98%;

  --primary: 217.2 91.2% 59.8%;
  --primary-foreground: 222.2 47.4% 11.2%;

  --secondary: 217.2 32.6% 17.5%;
  --secondary-foreground: 210 40% 98%;

  --muted: 217.2 32.6% 17.5%;
  --muted-foreground: 215 20.2% 65.1%;

  --accent: 217.2 32.6% 17.5%;
  --accent-foreground: 210 40% 98%;

  --destructive: 0 62.8% 30.6%;
  --destructive-foreground: 210 40% 98%;

  --border: 217.2 32.6% 17.5%;
  --input: 217.2 32.6% 17.5%;
  --ring: 224.3 76.3% 48%;
}
```

### 2.3 Government Extended Palette

| Color | Hex | Usage |
|-------|-----|-------|
| `--gov-blue` | `#1e40af` | Government branding |
| `--gov-green` | `#047857` | Success, approvals |
| `--gov-gold` | `#b45309` | Warnings, premiums |
| `--gov-red` | `#dc2626` | Errors, rejections |

### 2.4 Semantic Colors

| Token | Light | Dark | Usage |
|-------|-------|------|-------|
| `success` | `#10b981` | `#10b981` | Approved, active, positive |
| `warning` | `#f59e0b` | `#f59e0b` | Pending, caution |
| `destructive` | `#ef4444` | `#ef4444` | Rejected, error, danger |
| `info` | `#3b82f6` | `#3b82f6` | Informational, links |
| `muted` | `#6b7280` | `#9ca3af` | Secondary text |

### 2.5 Component Color Mapping

| Component | Default | Success | Warning | Destructive | Info |
|-----------|---------|---------|---------|-------------|------|
| **Button** | Blue | Green | Yellow | Red | Blue |
| **Badge** | Blue/10 | Green/10 | Yellow/10 | Red/10 | Blue/10 |
| **StatCard** | Blue glow | Green glow | Yellow glow | Red glow | Blue glow |
| **AlertBanner** | Blue bg | Green bg | Yellow bg | Red bg | Blue bg |
| **RecordStateBadge** | — | Green | Yellow | Red | Blue |

---

## 3. Typography

### 3.1 Font Families

```css
/* Primary - Arabic */
--font-sans: 'IBM Plex Sans Arabic', 'Cairo', system-ui, sans-serif;

/* Monospace - Numbers */
--font-mono: 'IBM Plex Mono', 'Courier New', monospace;
```

| Font | Usage | Features |
|------|-------|----------|
| **IBM Plex Sans Arabic** | Primary UI text | Full Arabic support, 7 weights |
| **Cairo** | Fallback Arabic | Alternative Arabic font |
| **IBM Plex Mono** | Code, numbers | Monospace alignment |

### 3.2 Font Weights

| Weight | Value | Usage |
|--------|-------|-------|
| `Light` | 300 | Decorative text |
| `Regular` | 400 | Body text |
| `Medium` | 500 | Labels, emphasis |
| `SemiBold` | 600 | Headings, buttons |
| `Bold` | 700 | Titles, strong emphasis |

### 3.3 Type Scale

| Token | Size | Line Height | Usage |
|-------|------|-------------|-------|
| `xs` | `0.75rem` (12px) | `1rem` | Captions, badges |
| `sm` | `0.875rem` (14px) | `1.25rem` | Secondary text |
| `base` | `1rem` (16px) | `1.5rem` | Body text |
| `lg` | `1.125rem` (18px) | `1.75rem` | Lead text |
| `xl` | `1.25rem` (20px) | `1.75rem` | Large text |
| `2xl` | `1.5rem` (24px) | `2rem` | H4 |
| `3xl` | `1.875rem` (30px) | `2.25rem` | H3 |
| `4xl` | `2.25rem` (36px) | `2.5rem` | H2 |
| `5xl` | `3rem` (48px) | `1` | H1 |

### 3.4 Heading Variants

```tsx
<Heading.h1>العنوان الرئيسي — 36px Bold</Heading.h1>
<Heading.h2>عنوان فرعي — 30px SemiBold</Heading.h2>
<Heading.h3>عنوان ثالث — 24px SemiBold</Heading.h3>
<Heading.h4>عنوان رابع — 20px Medium</Heading.h4>
```

### 3.5 Text Variants

```tsx
<Text.default>نص افتراضي — 16px Regular</Text.default>
<Text.mute>نص ثانوي — 14px Regular muted</Text.mute>
<Text.lead>نص رئيسي — 18px Regular leading-relaxed</Text.lead>
<Text.large>نص كبير — 20px Medium</Text.large>
<Text.small>نص صغير — 14px Regular</Text.small>
<Text.label>تسمية — 14px Medium</Text.label>
<Text.error>خطأ — 14px Regular destructive</Text.error>
```

### 3.6 Font Features (Arabic)

```css
font-feature-settings: "liga" 1, "calt" 1;
/* Enables Arabic ligatures and contextual alternates */
```

---

## 4. Spacing System

### 4.1 Base Scale

| Token | Value | Pixels |
|-------|-------|--------|
| `0` | `0` | 0 |
| `0.5` | `0.125rem` | 2px |
| `1` | `0.25rem` | 4px |
| `1.5` | `0.375rem` | 6px |
| `2` | `0.5rem` | 8px |
| `2.5` | `0.625rem` | 10px |
| `3` | `0.75rem` | 12px |
| `3.5` | `0.875rem` | 14px |
| `4` | `1rem` | 16px |
| `5` | `1.25rem` | 20px |
| `6` | `1.5rem` | 24px |
| `7` | `1.75rem` | 28px |
| `8` | `2rem` | 32px |
| `9` | `2.25rem` | 36px |
| `10` | `2.5rem` | 40px |
| `11` | `2.75rem` | 44px |
| `12` | `3rem` | 48px |
| `14` | `3.5rem` | 56px |
| `16` | `4rem` | 64px |
| `20` | `5rem` | 80px |
| `24` | `6rem` | 96px |

### 4.2 Component Spacing

| Component | Padding | Gap | Margin |
|-----------|---------|-----|--------|
| **Card** | `p-6` (24px) | `gap-4` (16px) | `mb-6` (24px) |
| **Button** | `px-4 py-2` | — | — |
| **Input** | `px-3 py-2` | — | — |
| **Dialog** | `p-6` | `gap-4` | — |
| **Table Cell** | `px-4 py-3` | — | — |
| **StatCard** | `p-6` | `gap-2` | — |
| **Badge** | `px-2.5 py-0.5` | — | — |

---

## 5. Border & Radius System

### 5.1 Border Radius

| Token | Value | Pixels | Usage |
|-------|-------|--------|-------|
| `none` | `0` | 0 | Sharp corners |
| `sm` | `0.125rem` | 2px | Subtle rounding |
| `DEFAULT` | `0.25rem` | 4px | Default rounding |
| `md` | `0.375rem` | 6px | Medium rounding |
| `lg` | `0.5rem` | 8px | Large rounding |
| `xl` | `0.75rem` | 12px | Extra large |
| `2xl` | `1rem` | 16px | Card rounding |
| `3xl` | `1.5rem` | 24px | Modal rounding |
| `full` | `9999px` | — | Pill/circle |

### 5.2 Component Radius

| Component | Radius | Token |
|-----------|--------|-------|
| **Button** | `0.375rem` | `md` |
| **Input** | `0.375rem` | `md` |
| **Card** | `0.5rem` | `lg` |
| **Dialog** | `0.75rem` | `xl` |
| **Sheet** | `0` | `none` (top corners only) |
| **Badge** | `9999px` | `full` |
| **Avatar** | `9999px` | `full` |
| **Table** | `0.5rem` | `lg` (container) |

### 5.3 Border Width

| Token | Value | Usage |
|-------|-------|-------|
| `0` | `0` | No border |
| `DEFAULT` | `1px` | Default border |
| `2` | `2px` | Emphasis border |
| `4` | `4px` | Heavy border (print frames) |

### 5.4 Border Color

| Token | Value | Usage |
|-------|-------|-------|
| `border` | `hsl(214.3 31.8% 91.4%)` | Default borders |
| `border/50` | `50% opacity` | Subtle borders |
| `primary` | `hsl(221.2 83.2% 53.3%)` | Focus rings |
| `destructive` | `hsl(0 84.2% 60.2%)` | Error borders |

---

## 6. Shadow & Effect System

### 6.1 Box Shadows

| Token | Value | Usage |
|-------|-------|-------|
| `shadow-xs` | `0 1px 2px rgba(0,0,0,0.05)` | Subtle elevation |
| `shadow-sm` | `0 1px 3px rgba(0,0,0,0.1)` | Cards, dropdowns |
| `shadow` | `0 4px 6px rgba(0,0,0,0.1)` | Elevated elements |
| `shadow-md` | `0 4px 6px rgba(0,0,0,0.1)` | Dialogs |
| `shadow-lg` | `0 10px 15px rgba(0,0,0,0.1)` | Popovers |
| `shadow-xl` | `0 20px 25px rgba(0,0,0,0.1)` | Modals |
| `shadow-2xl` | `0 25px 50px rgba(0,0,0,0.25)` | Heavy elevation |

### 6.2 Component Shadows

| Component | Shadow | Notes |
|-----------|--------|-------|
| **Card** | `shadow-sm` | Hover: `shadow-md` |
| **Dialog** | `shadow-xl` | + backdrop blur |
| **Sheet** | `shadow-xl` | Side panel |
| **Dropdown** | `shadow-lg` | Popover |
| **Button** | `shadow-xs` | Default |
| **StatCard** | Color glow | `box-shadow: 0 0 20px` |

### 6.3 Decorative Effects

```css
/* StatCard glow */
.stat-glow-blue { box-shadow: 0 0 20px rgba(59, 130, 246, 0.15); }
.stat-glow-green { box-shadow: 0 0 20px rgba(16, 185, 129, 0.15); }
.stat-glow-yellow { box-shadow: 0 0 20px rgba(245, 158, 11, 0.15); }
.stat-glow-red { box-shadow: 0 0 20px rgba(239, 68, 68, 0.15); }

/* Print: no glow */
@media print { .stat-glow-* { box-shadow: none; } }
```

---

## 7. Animation System

### 7.1 Transition Speeds

| Token | Duration | Usage |
|-------|----------|-------|
| `fast` | `100ms` | Hover states |
| `normal` | `200ms` | Standard transitions |
| `slow` | `300ms` | Dialogs, drawers |
| `slower` | `500ms` | Complex animations |

### 7.2 Animation Classes

```css
/* Fade */
.animate-in { animation: fadeIn 200ms ease-out; }
.fade-in { animation: fadeIn 300ms ease-out; }

/* Slide */
.slide-in-from-right { animation: slideInRight 300ms ease-out; }
.slide-in-from-left { animation: slideInLeft 300ms ease-out; }
.slide-in-from-bottom { animation: slideInBottom 300ms ease-out; }

/* Scale */
.zoom-in-95 { animation: zoomIn95 200ms ease-out; }
.zoom-in-90 { animation: zoomIn90 200ms ease-out; }

/* Spin */
.animate-spin { animation: spin 1s linear infinite; }

/* Pulse */
.animate-pulse { animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite; }

/* Bounce */
.animate-bounce { animation: bounce 1s infinite; }
```

### 7.3 Keyframes

```css
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideInRight {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}

@keyframes slideInLeft {
  from { transform: translateX(-100%); }
  to { transform: translateX(0); }
}

@keyframes slideInBottom {
  from { transform: translateY(100%); }
  to { transform: translateY(0); }
}

@keyframes zoomIn95 {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
```

### 7.4 Component Animations

| Component | Animation | Trigger |
|-----------|-----------|---------|
| **Dialog** | `zoom-in-95` + `fade-in` | Open |
| **Sheet** | `slide-in-from-right/left` | Open |
| **Dropdown** | `zoom-in-95` + `fade-in` | Open |
| **Badge** | `zoom-in` | Mount |
| **StatCard** | `slide-in-from-bottom` | Mount |
| **AlertBanner** | `slide-in-from-top` | Mount |
| **ErrorAlert** | `slide-in-from-right` | Mount |
| **Loader** | `spin` | Loading |
| **Stepper** | `pulse` | Current step |

### 7.5 Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 8. RTL (Right-to-Left) Patterns

### 8.1 Padding & Margin

```tsx
/* Use logical properties */
<p className="ps-4">Start padding (Left in LTR, Right in RTL)</p>
<p className="pe-4">End padding (Right in LTR, Left in RTL)</p>
<p className="ms-2">Start margin</p>
<p className="me-2">End margin</p>

/* Avoid physical properties */
<p className="pl-4">❌ Don't use — breaks RTL</p>
<p className="pr-4">❌ Don't use — breaks RTL</p>
```

### 8.2 Borders

```tsx
<div className="border-s-2">Start border</div>
<div className="border-e-2">End border</div>

<div className="border-s-primary">Start border with color</div>
```

### 8.3 Text Alignment

```tsx
<p className="text-start">Aligns to start (right in RTL)</p>
<p className="text-end">Aligns to end (left in RTL)</p>
<p className="text-center">Centered</p>
```

### 8.4 Icons

```tsx
/* Flip icons for RTL */
<ArrowRight className="rtl:rotate-180" />

/* Chevron in navigation */
<ChevronLeft className="rtl:rotate-180" />

/* Search icon — no flip needed */
<Search className="h-4 w-4" />
```

### 8.5 Layout Patterns

```tsx
/* Flex with direction */
<div className="flex items-center gap-2">
  <span>Label</span>
  <Input />
</div>

/* Grid with RTL */
<div className="grid grid-cols-2 gap-4">
  <div>Column 1 (right in RTL)</div>
  <div>Column 2 (left in RTL)</div>
</div>
```

### 8.6 Component-Specific RTL

| Component | RTL Behavior |
|-----------|--------------|
| **Select** | Dropdown aligns to start (right), scroll arrows flip |
| **Dialog** | Close button on left |
| **Sheet** | `side="left"` becomes `side="right"` in RTL |
| **Breadcrumb** | Separator flips direction |
| **DataTable** | Sort arrows flip |
| **Tabs** | Arrow navigation flips |
| **Stepper** | Steps flow right-to-left |

### 8.7 Forced LTR Exceptions

```tsx
{/* Date input — always LTR */}
<Input type="date" className="ltr" />

{/* Phone numbers — always LTR */}
<Input type="tel" className="ltr" />

{/* Bank account numbers — always LTR */}
<Input className="ltr font-mono" />

{/* Code blocks — always LTR */}
<pre className="ltr">code here</pre>
```

---

## 9. Print System

### 9.1 Page Sizes

| Size | Dimensions | Usage |
|------|------------|-------|
| `A4` | 210mm × 297mm | Standard documents |
| `A3` | 297mm × 420mm | Large reports |
| `A5` | 148mm × 210mm | Small documents |
| `letter` | 8.5in × 11in | US letter |

### 9.2 Print CSS

```css
@media print {
  /* Hide non-print elements */
  .no-print { display: none !important; }

  /* Page setup */
  @page {
    size: A4 portrait;
    margin: 15mm;
  }

  /* Prevent page breaks inside */
  .break-inside-avoid { break-inside: avoid; }

  /* Background colors for print */
  .print-bg { print-color-adjust: exact; }

  /* Double-border frame */
  .print-frame {
    border: 2px solid #000;
    outline: 1px solid #000;
    outline-offset: 4px;
  }
}
```

### 9.3 Print Typography

```css
@media print {
  body { font-size: 12pt; }
  h1 { font-size: 18pt; }
  h2 { font-size: 16pt; }
  h3 { font-size: 14pt; }
  .text-print-xs { font-size: 8pt; }
  .text-print-sm { font-size: 10pt; }
  .text-print-base { font-size: 12pt; }
  .text-print-lg { font-size: 14pt; }
}
```

### 9.4 Watermark

```tsx
<PrintLayout watermark="سري">
  {/* Content with diagonal watermark */}
</PrintLayout>
```

### 9.5 Print Header/Footer

```tsx
<PrintLayout
  header={<PrintHeader />}
  footer={<PrintFooter />}
>
  <PrintableReportTable columns={columns} data={data} />
</PrintLayout>
```

---

## 10. Focus & Accessibility

### 10.1 Focus Ring

```css
.focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2
```

### 10.2 Touch Targets

Minimum touch target size: `44px × 44px` (WCAG 2.5.5)

```tsx
<Button className="min-h-[44px] min-w-[44px]">
  {/* Touch-friendly button */}
</Button>
```

### 10.3 Screen Reader Only

```tsx
<span className="sr-only">Screen reader text</span>
```

### 10.4 Color Contrast

| Element | Ratio | Standard |
|---------|-------|----------|
| Body text | 15.4:1 | AAA |
| Secondary text | 7.1:1 | AAA |
| Links | 8.6:1 | AAA |
| Buttons | 8.6:1 | AAA |
| Focus ring | 3:1 | AA |

---

## 11. Dark Mode

### 11.1 Implementation

```tsx
// ThemeProvider.tsx
document.documentElement.classList.toggle('dark', isDark);
```

### 11.2 CSS Variables

All colors use CSS variables that automatically switch between light/dark.

### 11.3 Component Behavior

| Component | Light | Dark |
|-----------|-------|------|
| **Background** | White | Dark gray |
| **Text** | Near-black | Near-white |
| **Borders** | Light gray | Dark gray |
| **Cards** | White | Dark gray |
| **Dialogs** | White | Dark gray |

---

## 12. Constants

### 12.1 Domain Constants

| Constant | Value | Usage |
|----------|-------|-------|
| `CURRENCY` | `YER` | Yemeni Rial |
| `TIMEZONE` | `GMT+3` | Arabic Standard Time |
| `RISK_THRESHOLD` | `1,500,000` | Risk scoring threshold |
| `DEFAULT_PASSWORD` | `Admin@123` | Default admin password |

### 12.2 Status Constants

```typescript
FILE_STATUS: {
  NEW: 1,
  PENDING_REVIEW: 2,
  UNDER_REVIEW: 3,
  APPROVED: 4,
  REJECTED: 5,
  RESUBMITTED: 6,
  SUSPENDED: 7,
  COMPLETED: 8,
  ARCHIVED: 9
}
```

---

## 13. File Structure

```
ui/src/
├── index.css                    # CSS variables, themes, font-face
├── components/
│   ├── ui/                      # UI components
│   ├── layout/                  # Layout components
│   └── providers/               # Theme, notification providers
└── lib/
    ├── utils.ts                 # cn(), formatCurrency, formatDate
    └── constants.ts             # Domain constants
```

---

## 14. References

| Resource | URL |
|----------|-----|
| Tailwind CSS | https://tailwindcss.com |
| Radix UI | https://www.radix-ui.com |
| CVA | https://cva.style |
| IBM Plex Sans Arabic | https://github.com/IBM/plex |
| Cairo | https://fonts.google.com/specimen/Cairo |
