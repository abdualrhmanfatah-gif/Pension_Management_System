# جرد الشفافية في صفحات ومكوّنات الواجهة

تاريخ الفحص: 2026-10-17 — هذا جرد ساكن للمصدر، ولم يتضمن تعديل ملفات التطبيق أو الحكم على التباين الفعلي في المتصفح.

فُحص 429 ملفًا في `ui/src/components` و`ui/src/pages`، إضافة إلى 3 ملفات مشتركة: `index.css` و`styles/print.css` و`tailwind.config.js`.

رُصد 1169 استخدامًا في 267 ملفًا من الصفحات والمكوّنات، إضافة إلى 12 استخدامًا في 2 من الملفات المشتركة. المقصود بالاستخدام ظهور قاعدة أو قيمة في المصدر؛ قد تكون مشروطة أو ضمن فرع غير ظاهر حاليًا. هذه الأعداد ليست عدد أخطاء أو عدد عناصر تظهر للمستخدم.

توجد 0 إشارة إلى تخفيف لون النص أو العنصر كاملًا في 0 ملفات. تشمل الأخيرة النصوص والأيقونات والزخارف؛ يلزم تمييز العنصر قبل أي إزالة جماعية.

## ملخص الصفحات والمكوّنات

| النوع | مواضع المصدر | ملفات |
|---|---:|---:|
| شفافية الخلفيات والتدرجات | 1016 | 255 |
| شفافية مرتبطة بحالة التعطيل | 15 | 10 |
| ألوان rgba أو hsla أو HEX مع قناة شفافية | 9 | 3 |
| شفافية حلقة التركيز والظلال | 40 | 17 |
| إخفاء تام أو لون شفاف صريح | 64 | 44 |
| شفافية لون تعبئة أو حد الأيقونة | 2 | 2 |
| تلاشي مرتبط بحركة الفتح والإغلاق | 13 | 6 |
| شفافية عبر style أو CSS أو خاصية رسم | 7 | 3 |
| إعدادات شفافية العلامة المائية | 3 | 2 |

أعداد الملفات تتداخل بين الأنواع، لذلك لا تُجمع للحصول على عدد الملفات الفريدة.

## مواضع ذات أولوية عند تحسين وضوح النص

| الملف والسطر | الاستخدام |
|---|---|

## تفسير الأثر قبل التعديل

- `text-…/60` يخفف لون النص وحده؛ `opacity-60` يخفف العنصر وجميع محتوياته، بما فيها النص والأيقونات والخلفية والحدود.
- اجتماع تخفيف لون النص وشفافية العنصر يضاعف الأثر؛ مثل `opacity-30 text-primary/40` في أيقونات الحالة الفارغة في `DataTable`، فتكون العتامة الفعالة نحو 12% قبل احتساب التنعيم.
- `bg-…/10` و`border-…/40` لا يخففان النص مباشرة. إزالة شفافية الخلفيات تغيّر مظهر الشارات والتنبيهات والبطاقات، ولا تمثل تلقائيًا تحسينًا للنص.
- بعض استخدامات `opacity-0` تخفي عناصر ملفات أو تواريخ أصلية مع الإبقاء على التفاعل؛ وبعضها جزء من الحركة. لا تُعامل على أنها نص باهت.
- توجد حالات تعطيل في مكوّنات مشتركة مثل الأزرار وحقول الإدخال وCheckbox وTabs. يلزم الحفاظ على دلالة التعطيل عند استبدال الشفافية بلون صريح.
- لم تُصنّف القيم الكاملة `opacity-100` على أنها شفافية فعلية، ولم يُحتسب `transition-opacity` وحده لأنه يحدد الخاصية المتحركة دون أن يخففها.
- اكتُشفت إشارتان بالصياغة غير الصحيحة `placeholderr:` في صفحة الدخول؛ لا يولّد Tailwind تنسيق النص الإرشادي المقصود منهما، لذلك فُصلتا عن العدد الأساسي.
- يشمل الجرد صيغ Tailwind والقيم المباشرة في CSS وstyle وSVG وإعدادات الطباعة. لا يقيس شفافية صور أو فيديوهات أو ملفات أصول خارج هذا النطاق، ولا قواعد مكتبات المتصفح أو الأنماط المحسوبة ديناميكيًا من بيانات التشغيل.

## الجرد التفصيلي للصفحات والمكوّنات

### شفافية مرتبطة بحالة التعطيل — 15 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/layout/NavDropdown.tsx:100](/F:/Projects/Pension_Management_System/ui/src/components/layout/NavDropdown.tsx:100) | `data-[disabled]:opacity-50` |
| [ui/src/components/ui/AlertDialog.tsx:110](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:110) | `disabled:opacity-50` |
| [ui/src/components/ui/AlertDialog.tsx:125](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:125) | `disabled:opacity-50` |
| [ui/src/components/ui/buttonVariants.ts:4](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:4) | `disabled:opacity-50` |
| [ui/src/components/ui/Checkbox.tsx:24](/F:/Projects/Pension_Management_System/ui/src/components/ui/Checkbox.tsx:24) | `disabled:opacity-50` |
| [ui/src/components/ui/DropdownMenu.tsx:46](/F:/Projects/Pension_Management_System/ui/src/components/ui/DropdownMenu.tsx:46) | `data-[disabled]:opacity-50` |
| [ui/src/components/ui/FileUploader.tsx:733](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:733) | `opacity-50` |
| [ui/src/components/ui/Input.tsx:86](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:86) | `opacity-50` |
| [ui/src/components/ui/Input.tsx:102](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:102) | `disabled:opacity-50` |
| [ui/src/components/ui/Input.tsx:413](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:413) | `disabled:opacity-50` |
| [ui/src/components/ui/Input.tsx:438](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:438) | `opacity-50` |
| [ui/src/components/ui/Input.tsx:476](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:476) | `disabled:opacity-50` |
| [ui/src/components/ui/RadioGroup.tsx:26](/F:/Projects/Pension_Management_System/ui/src/components/ui/RadioGroup.tsx:26) | `disabled:opacity-50` |
| [ui/src/components/ui/Slider.tsx:21](/F:/Projects/Pension_Management_System/ui/src/components/ui/Slider.tsx:21) | `disabled:opacity-50` |
| [ui/src/components/ui/Tabs.tsx:36](/F:/Projects/Pension_Management_System/ui/src/components/ui/Tabs.tsx:36) | `disabled:opacity-50` |

### شفافية الخلفيات والتدرجات — 1016 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/auth/PermissionRoute.tsx:58](/F:/Projects/Pension_Management_System/ui/src/components/auth/PermissionRoute.tsx:58) | `dark:bg-red-900/20` |
| [ui/src/components/common/charts/ChartToggle.tsx:13](/F:/Projects/Pension_Management_System/ui/src/components/common/charts/ChartToggle.tsx:13) | `bg-muted/50` |
| [ui/src/components/common/charts/ReportBarChart.tsx:149](/F:/Projects/Pension_Management_System/ui/src/components/common/charts/ReportBarChart.tsx:149) | `hover:bg-muted/30` |
| [ui/src/components/common/charts/ReportBarChart.tsx:167](/F:/Projects/Pension_Management_System/ui/src/components/common/charts/ReportBarChart.tsx:167) | `bg-secondary/50` |
| [ui/src/components/common/charts/ReportPieChart.tsx:256](/F:/Projects/Pension_Management_System/ui/src/components/common/charts/ReportPieChart.tsx:256) | `hover:bg-muted/40` |
| [ui/src/components/common/dashboards/DashboardHeader.tsx:20](/F:/Projects/Pension_Management_System/ui/src/components/common/dashboards/DashboardHeader.tsx:20) | `from-primary/5` |
| [ui/src/components/common/dashboards/DashboardHeader.tsx:20](/F:/Projects/Pension_Management_System/ui/src/components/common/dashboards/DashboardHeader.tsx:20) | `to-primary/[0.02]` |
| [ui/src/components/common/dashboards/DashboardHeader.tsx:29](/F:/Projects/Pension_Management_System/ui/src/components/common/dashboards/DashboardHeader.tsx:29) | `bg-primary/10` |
| [ui/src/components/common/FilterBar.tsx:33](/F:/Projects/Pension_Management_System/ui/src/components/common/FilterBar.tsx:33) | `bg-card/40` |
| [ui/src/components/common/FilterBar.tsx:70](/F:/Projects/Pension_Management_System/ui/src/components/common/FilterBar.tsx:70) | `bg-primary/10` |
| [ui/src/components/common/PrintSignatureBlock.tsx:38](/F:/Projects/Pension_Management_System/ui/src/components/common/PrintSignatureBlock.tsx:38) | `bg-muted/50` |
| [ui/src/components/common/PrintSummaryBox.tsx:68](/F:/Projects/Pension_Management_System/ui/src/components/common/PrintSummaryBox.tsx:68) | `bg-green-50/50` |
| [ui/src/components/common/PrintSummaryBox.tsx:69](/F:/Projects/Pension_Management_System/ui/src/components/common/PrintSummaryBox.tsx:69) | `bg-amber-50/50` |
| [ui/src/components/common/PrintSummaryBox.tsx:70](/F:/Projects/Pension_Management_System/ui/src/components/common/PrintSummaryBox.tsx:70) | `bg-red-50/50` |
| [ui/src/components/common/PrintSummaryBox.tsx:71](/F:/Projects/Pension_Management_System/ui/src/components/common/PrintSummaryBox.tsx:71) | `bg-muted/50` |
| [ui/src/components/common/PrintSummaryBox.tsx:72](/F:/Projects/Pension_Management_System/ui/src/components/common/PrintSummaryBox.tsx:72) | `bg-muted/50` |
| [ui/src/components/common/ReportCard.tsx:50](/F:/Projects/Pension_Management_System/ui/src/components/common/ReportCard.tsx:50) | `bg-primary/10` |
| [ui/src/components/common/ReportFilters.tsx:259](/F:/Projects/Pension_Management_System/ui/src/components/common/ReportFilters.tsx:259) | `bg-primary/10` |
| [ui/src/components/common/RetirementFileReportViewer.tsx:166](/F:/Projects/Pension_Management_System/ui/src/components/common/RetirementFileReportViewer.tsx:166) | `bg-primary/10` |
| [ui/src/components/common/RetirementFileReportViewer.tsx:385](/F:/Projects/Pension_Management_System/ui/src/components/common/RetirementFileReportViewer.tsx:385) | `bg-primary/5` |
| [ui/src/components/common/RetirementFileReportViewer.tsx:386](/F:/Projects/Pension_Management_System/ui/src/components/common/RetirementFileReportViewer.tsx:386) | `bg-emerald-500/5` |
| [ui/src/components/common/RetirementFileReportViewer.tsx:387](/F:/Projects/Pension_Management_System/ui/src/components/common/RetirementFileReportViewer.tsx:387) | `bg-amber-500/5` |
| [ui/src/components/common/RetirementFileReportViewer.tsx:388](/F:/Projects/Pension_Management_System/ui/src/components/common/RetirementFileReportViewer.tsx:388) | `bg-rose-500/5` |
| [ui/src/components/common/RetirementFileReportViewer.tsx:425](/F:/Projects/Pension_Management_System/ui/src/components/common/RetirementFileReportViewer.tsx:425) | `hover:bg-muted/50` |
| [ui/src/components/common/ReviewFilePreview.tsx:42](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:42) | `bg-muted/30` |
| [ui/src/components/common/ReviewFilePreview.tsx:62](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:62) | `bg-black/40` |
| [ui/src/components/common/ReviewFilePreview.tsx:68](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:68) | `bg-primary/5` |
| [ui/src/components/common/ReviewFilePreview.tsx:93](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:93) | `bg-muted/20` |
| [ui/src/components/common/ReviewFilePreview.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:103) | `bg-zinc-900/5` |
| [ui/src/components/common/ReviewFilePreview.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:103) | `dark:bg-black/20` |
| [ui/src/components/common/ReviewFilePreview.tsx:121](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:121) | `bg-primary/10` |
| [ui/src/components/common/ReviewFilePreview.tsx:135](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:135) | `bg-muted/20` |
| [ui/src/components/common/ReviewFilePreview.tsx:138](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:138) | `bg-background/50` |
| [ui/src/components/common/SearchDialog.tsx:136](/F:/Projects/Pension_Management_System/ui/src/components/common/SearchDialog.tsx:136) | `bg-muted/30` |
| [ui/src/components/common/SearchDialog.tsx:154](/F:/Projects/Pension_Management_System/ui/src/components/common/SearchDialog.tsx:154) | `bg-primary/5` |
| [ui/src/components/common/SearchDialog.tsx:155](/F:/Projects/Pension_Management_System/ui/src/components/common/SearchDialog.tsx:155) | `hover:bg-muted/50` |
| [ui/src/components/common/SearchDialog.tsx:161](/F:/Projects/Pension_Management_System/ui/src/components/common/SearchDialog.tsx:161) | `bg-primary/10` |
| [ui/src/components/common/SearchDialog.tsx:167](/F:/Projects/Pension_Management_System/ui/src/components/common/SearchDialog.tsx:167) | `bg-muted/50` |
| [ui/src/components/common/UnifiedRetireeStatement.tsx:76](/F:/Projects/Pension_Management_System/ui/src/components/common/UnifiedRetireeStatement.tsx:76) | `bg-muted/50` |
| [ui/src/components/common/UnifiedRetireeStatement.tsx:84](/F:/Projects/Pension_Management_System/ui/src/components/common/UnifiedRetireeStatement.tsx:84) | `bg-muted/50` |
| [ui/src/components/common/UnifiedRetireeStatement.tsx:194](/F:/Projects/Pension_Management_System/ui/src/components/common/UnifiedRetireeStatement.tsx:194) | `bg-muted/50` |
| [ui/src/components/common/UnifiedRetireeStatement.tsx:251](/F:/Projects/Pension_Management_System/ui/src/components/common/UnifiedRetireeStatement.tsx:251) | `bg-muted/50` |
| [ui/src/components/common/UnifiedRetireeStatement.tsx:287](/F:/Projects/Pension_Management_System/ui/src/components/common/UnifiedRetireeStatement.tsx:287) | `bg-muted/50` |
| [ui/src/components/common/UploaderStatsView.tsx:506](/F:/Projects/Pension_Management_System/ui/src/components/common/UploaderStatsView.tsx:506) | `bg-primary/10` |
| [ui/src/components/common/UploaderStatsView.tsx:745](/F:/Projects/Pension_Management_System/ui/src/components/common/UploaderStatsView.tsx:745) | `bg-black/50` |
| [ui/src/components/common/UploaderStatsView.tsx:783](/F:/Projects/Pension_Management_System/ui/src/components/common/UploaderStatsView.tsx:783) | `bg-black/50` |
| [ui/src/components/domain/accounting/AccountStatementReport.tsx:139](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/AccountStatementReport.tsx:139) | `bg-muted/50` |
| [ui/src/components/domain/accounting/AccountStatementReport.tsx:151](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/AccountStatementReport.tsx:151) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/AccountStatementReport.tsx:161](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/AccountStatementReport.tsx:161) | `bg-muted/30` |
| [ui/src/components/domain/accounting/AccountTreeView.tsx:34](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/AccountTreeView.tsx:34) | `hover:bg-accent/50` |
| [ui/src/components/domain/accounting/AccountTreeView.tsx:36](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/AccountTreeView.tsx:36) | `bg-primary/10` |
| [ui/src/components/domain/accounting/AccountTreeView.tsx:67](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/AccountTreeView.tsx:67) | `bg-destructive/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:29](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:29) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:30](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:30) | `bg-muted/20` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:127](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:127) | `bg-info/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:129](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:129) | `bg-info/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:137](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:137) | `bg-warning/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:139](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:139) | `bg-warning/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:146](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:146) | `bg-success/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:148](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:148) | `bg-success/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:158](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:158) | `bg-destructive/10` |
| [ui/src/components/domain/accounting/BalanceSheetReport.tsx:158](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BalanceSheetReport.tsx:158) | `bg-success/10` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:49](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:49) | `bg-muted/30` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:57](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:57) | `bg-muted/50` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:71](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:71) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:130](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:130) | `bg-muted/30` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:136](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:136) | `bg-muted/50` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:152](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:152) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:211](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:211) | `bg-muted/50` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:241](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:241) | `bg-muted/30` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:249](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:249) | `bg-muted/50` |
| [ui/src/components/domain/accounting/BudgetReport.tsx:263](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReport.tsx:263) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/BudgetReportView.tsx:69](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReportView.tsx:69) | `bg-muted/30` |
| [ui/src/components/domain/accounting/BudgetReportView.tsx:73](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReportView.tsx:73) | `bg-muted/50` |
| [ui/src/components/domain/accounting/BudgetReportView.tsx:86](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReportView.tsx:86) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/BudgetReportView.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/BudgetReportView.tsx:103) | `bg-muted/50` |
| [ui/src/components/domain/accounting/CashFlowReport.tsx:97](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CashFlowReport.tsx:97) | `bg-muted/30` |
| [ui/src/components/domain/accounting/CashFlowReport.tsx:130](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CashFlowReport.tsx:130) | `bg-muted/30` |
| [ui/src/components/domain/accounting/CashFlowReport.tsx:163](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CashFlowReport.tsx:163) | `bg-muted/30` |
| [ui/src/components/domain/accounting/CostCenterReport.tsx:140](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CostCenterReport.tsx:140) | `bg-success/10` |
| [ui/src/components/domain/accounting/CostCenterReport.tsx:163](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CostCenterReport.tsx:163) | `bg-danger/10` |
| [ui/src/components/domain/accounting/CumulativePnLReport.tsx:149](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CumulativePnLReport.tsx:149) | `bg-muted/30` |
| [ui/src/components/domain/accounting/CumulativePnLReport.tsx:153](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CumulativePnLReport.tsx:153) | `bg-muted/50` |
| [ui/src/components/domain/accounting/CumulativePnLReport.tsx:166](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CumulativePnLReport.tsx:166) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/CumulativePnLReport.tsx:181](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/CumulativePnLReport.tsx:181) | `bg-muted/50` |
| [ui/src/components/domain/accounting/DepreciationReport.tsx:132](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/DepreciationReport.tsx:132) | `bg-muted/30` |
| [ui/src/components/domain/accounting/DepreciationReport.tsx:138](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/DepreciationReport.tsx:138) | `bg-muted/50` |
| [ui/src/components/domain/accounting/DepreciationReport.tsx:161](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/DepreciationReport.tsx:161) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/DepreciationReport.tsx:178](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/DepreciationReport.tsx:178) | `bg-muted/50` |
| [ui/src/components/domain/accounting/FiscalYearCloseDialog.tsx:159](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FiscalYearCloseDialog.tsx:159) | `bg-muted/30` |
| [ui/src/components/domain/accounting/FiscalYearCloseDialog.tsx:224](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FiscalYearCloseDialog.tsx:224) | `bg-muted/30` |
| [ui/src/components/domain/accounting/FixedAssetsReport.tsx:124](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FixedAssetsReport.tsx:124) | `bg-muted/30` |
| [ui/src/components/domain/accounting/FixedAssetsReport.tsx:130](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FixedAssetsReport.tsx:130) | `bg-muted/50` |
| [ui/src/components/domain/accounting/FixedAssetsReport.tsx:154](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FixedAssetsReport.tsx:154) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/FixedAssetsReport.tsx:166](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FixedAssetsReport.tsx:166) | `dark:bg-emerald-900/30` |
| [ui/src/components/domain/accounting/FixedAssetsReport.tsx:174](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/FixedAssetsReport.tsx:174) | `bg-muted/50` |
| [ui/src/components/domain/accounting/GeneralLedgerReport.tsx:153](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/GeneralLedgerReport.tsx:153) | `bg-muted/50` |
| [ui/src/components/domain/accounting/GeneralLedgerReport.tsx:167](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/GeneralLedgerReport.tsx:167) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/IncomeStatementReport.tsx:52](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/IncomeStatementReport.tsx:52) | `bg-muted/20` |
| [ui/src/components/domain/accounting/IncomeStatementReport.tsx:112](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/IncomeStatementReport.tsx:112) | `bg-green-50/50` |
| [ui/src/components/domain/accounting/IncomeStatementReport.tsx:121](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/IncomeStatementReport.tsx:121) | `bg-red-50/50` |
| [ui/src/components/domain/accounting/InitialOpeningBalanceDialog.tsx:175](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/InitialOpeningBalanceDialog.tsx:175) | `bg-muted/30` |
| [ui/src/components/domain/accounting/InitialOpeningBalanceDialog.tsx:243](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/InitialOpeningBalanceDialog.tsx:243) | `bg-muted/30` |
| [ui/src/components/domain/accounting/JournalEntryFormDialog.tsx:292](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryFormDialog.tsx:292) | `bg-muted/50` |
| [ui/src/components/domain/accounting/JournalEntryFormDialog.tsx:349](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryFormDialog.tsx:349) | `hover:bg-destructive/10` |
| [ui/src/components/domain/accounting/JournalEntryFormDialog.tsx:359](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryFormDialog.tsx:359) | `bg-muted/30` |
| [ui/src/components/domain/accounting/JournalEntryReverseDialog.tsx:64](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryReverseDialog.tsx:64) | `bg-destructive/10` |
| [ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:101](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:101) | `bg-muted/50` |
| [ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:112](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:112) | `bg-muted/5` |
| [ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:112](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:112) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:134](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalEntryViewDialog.tsx:134) | `bg-muted/30` |
| [ui/src/components/domain/accounting/JournalRegisterReport.tsx:105](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalRegisterReport.tsx:105) | `bg-muted/50` |
| [ui/src/components/domain/accounting/JournalRegisterReport.tsx:119](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/JournalRegisterReport.tsx:119) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/LiabilitiesReport.tsx:183](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/LiabilitiesReport.tsx:183) | `bg-muted/30` |
| [ui/src/components/domain/accounting/QuarterlyReport.tsx:152](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/QuarterlyReport.tsx:152) | `bg-muted/20` |
| [ui/src/components/domain/accounting/QuarterlyReport.tsx:192](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/QuarterlyReport.tsx:192) | `bg-green-50/50` |
| [ui/src/components/domain/accounting/QuarterlyReport.tsx:201](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/QuarterlyReport.tsx:201) | `bg-red-50/50` |
| [ui/src/components/domain/accounting/SubledgerReport.tsx:140](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/SubledgerReport.tsx:140) | `bg-muted/50` |
| [ui/src/components/domain/accounting/SubledgerReport.tsx:160](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/SubledgerReport.tsx:160) | `hover:bg-muted/20` |
| [ui/src/components/domain/accounting/TrialBalanceReport.tsx:221](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/TrialBalanceReport.tsx:221) | `bg-info/10` |
| [ui/src/components/domain/accounting/TrialBalanceReport.tsx:222](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/TrialBalanceReport.tsx:222) | `bg-success/10` |
| [ui/src/components/domain/accounting/TrialBalanceReport.tsx:277](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/TrialBalanceReport.tsx:277) | `bg-danger/10` |
| [ui/src/components/domain/accounting/TrialBalanceReport.tsx:277](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/TrialBalanceReport.tsx:277) | `bg-success/10` |
| [ui/src/components/domain/accounting/VoucherImportDialog.tsx:162](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/VoucherImportDialog.tsx:162) | `bg-muted/50` |
| [ui/src/components/domain/accounting/VoucherViewDialog.tsx:116](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/VoucherViewDialog.tsx:116) | `bg-primary/5` |
| [ui/src/components/domain/accounting/YearOverYearReport.tsx:156](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/YearOverYearReport.tsx:156) | `bg-muted/30` |
| [ui/src/components/domain/accounting/YearOverYearReport.tsx:160](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/YearOverYearReport.tsx:160) | `bg-muted/50` |
| [ui/src/components/domain/accounting/YearOverYearReport.tsx:170](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/YearOverYearReport.tsx:170) | `bg-emerald-50/50` |
| [ui/src/components/domain/accounting/YearOverYearReport.tsx:170](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/YearOverYearReport.tsx:170) | `dark:bg-emerald-950/20` |
| [ui/src/components/domain/accounting/YearOverYearReport.tsx:181](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/YearOverYearReport.tsx:181) | `bg-red-50/50` |
| [ui/src/components/domain/accounting/YearOverYearReport.tsx:181](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/YearOverYearReport.tsx:181) | `dark:bg-red-950/20` |
| [ui/src/components/domain/admin/BankFormDialog.tsx:150](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/BankFormDialog.tsx:150) | `bg-muted/30` |
| [ui/src/components/domain/admin/DocumentTypeDialog.tsx:213](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/DocumentTypeDialog.tsx:213) | `bg-muted/30` |
| [ui/src/components/domain/admin/DocumentTypeDialog.tsx:213](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/DocumentTypeDialog.tsx:213) | `hover:bg-muted/50` |
| [ui/src/components/domain/admin/DocumentTypeDialog.tsx:224](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/DocumentTypeDialog.tsx:224) | `bg-muted/30` |
| [ui/src/components/domain/admin/DocumentTypeDialog.tsx:224](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/DocumentTypeDialog.tsx:224) | `hover:bg-muted/50` |
| [ui/src/components/domain/admin/DocumentTypeDialog.tsx:236](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/DocumentTypeDialog.tsx:236) | `bg-primary/5` |
| [ui/src/components/domain/admin/ExcelImportDialog.tsx:100](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/ExcelImportDialog.tsx:100) | `bg-primary/5` |
| [ui/src/components/domain/admin/ExcelImportDialog.tsx:100](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/ExcelImportDialog.tsx:100) | `hover:bg-muted/30` |
| [ui/src/components/domain/admin/ExcelImportDialog.tsx:157](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/ExcelImportDialog.tsx:157) | `group-hover:bg-primary/10` |
| [ui/src/components/domain/admin/ExcelImportDialog.tsx:172](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/ExcelImportDialog.tsx:172) | `bg-destructive/10` |
| [ui/src/components/domain/admin/ExcelImportDialog.tsx:173](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/ExcelImportDialog.tsx:173) | `bg-destructive/10` |
| [ui/src/components/domain/admin/permission-utils.ts:33](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:33) | `bg-info/10` |
| [ui/src/components/domain/admin/permission-utils.ts:34](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:34) | `bg-success/10` |
| [ui/src/components/domain/admin/permission-utils.ts:35](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:35) | `bg-warning/10` |
| [ui/src/components/domain/admin/permission-utils.ts:36](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:36) | `bg-special/10` |
| [ui/src/components/domain/admin/permission-utils.ts:37](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:37) | `bg-info/10` |
| [ui/src/components/domain/admin/permission-utils.ts:38](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:38) | `bg-special/10` |
| [ui/src/components/domain/admin/permission-utils.ts:39](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:39) | `bg-success/10` |
| [ui/src/components/domain/admin/permission-utils.ts:40](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:40) | `bg-warning/10` |
| [ui/src/components/domain/admin/permission-utils.ts:41](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:41) | `bg-info/10` |
| [ui/src/components/domain/admin/permission-utils.ts:42](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:42) | `bg-success/10` |
| [ui/src/components/domain/admin/permission-utils.ts:43](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:43) | `bg-danger/10` |
| [ui/src/components/domain/admin/permission-utils.ts:44](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:44) | `bg-warning/10` |
| [ui/src/components/domain/admin/permission-utils.ts:45](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:45) | `bg-special/10` |
| [ui/src/components/domain/admin/permission-utils.ts:46](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:46) | `bg-special/10` |
| [ui/src/components/domain/admin/permission-utils.ts:47](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:47) | `bg-danger/10` |
| [ui/src/components/domain/admin/permission-utils.ts:49](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:49) | `bg-success/10` |
| [ui/src/components/domain/admin/permission-utils.ts:51](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:51) | `bg-special/10` |
| [ui/src/components/domain/admin/permission-utils.ts:52](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:52) | `bg-special/10` |
| [ui/src/components/domain/admin/permission-utils.ts:54](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:54) | `bg-warning/10` |
| [ui/src/components/domain/admin/permission-utils.ts:55](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:55) | `bg-info/10` |
| [ui/src/components/domain/admin/permission-utils.ts:56](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:56) | `bg-info/10` |
| [ui/src/components/domain/admin/permission-utils.ts:57](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:57) | `bg-info/10` |
| [ui/src/components/domain/admin/permission-utils.ts:58](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/permission-utils.ts:58) | `bg-success/10` |
| [ui/src/components/domain/admin/RolePermissionsDialog.tsx:139](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/RolePermissionsDialog.tsx:139) | `bg-muted/20` |
| [ui/src/components/domain/admin/SendNotificationDialog.tsx:369](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/SendNotificationDialog.tsx:369) | `hover:bg-muted/50` |
| [ui/src/components/domain/admin/SendNotificationDialog.tsx:403](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/SendNotificationDialog.tsx:403) | `hover:bg-primary/5` |
| [ui/src/components/domain/admin/SendNotificationDialog.tsx:430](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/SendNotificationDialog.tsx:430) | `bg-muted/50` |
| [ui/src/components/domain/admin/SendNotificationDialog.tsx:443](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/SendNotificationDialog.tsx:443) | `hover:bg-destructive/10` |
| [ui/src/components/domain/admin/UserPermissionsDialog.tsx:149](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/UserPermissionsDialog.tsx:149) | `bg-muted/30` |
| [ui/src/components/domain/admin/UserPermissionsDialog.tsx:191](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/UserPermissionsDialog.tsx:191) | `bg-muted/20` |
| [ui/src/components/domain/assets/AssetCategoryFormDialog.tsx:221](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/AssetCategoryFormDialog.tsx:221) | `bg-muted/30` |
| [ui/src/components/domain/assets/DepreciationRunDialog.tsx:184](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/DepreciationRunDialog.tsx:184) | `dark:bg-green-950/20` |
| [ui/src/components/domain/assets/DepreciationRunDialog.tsx:185](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/DepreciationRunDialog.tsx:185) | `dark:bg-red-950/20` |
| [ui/src/components/domain/assets/DepreciationRunDialog.tsx:212](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/DepreciationRunDialog.tsx:212) | `dark:bg-green-950/20` |
| [ui/src/components/domain/assets/FixedAssetFormDialog.tsx:508](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/FixedAssetFormDialog.tsx:508) | `bg-primary/5` |
| [ui/src/components/domain/assets/FixedAssetFormDialog.tsx:521](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/FixedAssetFormDialog.tsx:521) | `bg-muted/50` |
| [ui/src/components/domain/assets/FixedAssetFormDialog.tsx:546](/F:/Projects/Pension_Management_System/ui/src/components/domain/assets/FixedAssetFormDialog.tsx:546) | `dark:bg-blue-950/30` |
| [ui/src/components/domain/hr/DepartmentDialog.tsx:134](/F:/Projects/Pension_Management_System/ui/src/components/domain/hr/DepartmentDialog.tsx:134) | `bg-muted/30` |
| [ui/src/components/domain/hr/JobTitleDialog.tsx:133](/F:/Projects/Pension_Management_System/ui/src/components/domain/hr/JobTitleDialog.tsx:133) | `bg-muted/30` |
| [ui/src/components/domain/hr/SectionDialog.tsx:169](/F:/Projects/Pension_Management_System/ui/src/components/domain/hr/SectionDialog.tsx:169) | `bg-muted/30` |
| [ui/src/components/domain/investment/ContractDialog.tsx:273](/F:/Projects/Pension_Management_System/ui/src/components/domain/investment/ContractDialog.tsx:273) | `bg-muted/30` |
| [ui/src/components/domain/investment/InvestmentDocumentsManager.tsx:220](/F:/Projects/Pension_Management_System/ui/src/components/domain/investment/InvestmentDocumentsManager.tsx:220) | `bg-primary/10` |
| [ui/src/components/domain/investment/InvestmentDocumentsManager.tsx:367](/F:/Projects/Pension_Management_System/ui/src/components/domain/investment/InvestmentDocumentsManager.tsx:367) | `hover:bg-primary/90` |
| [ui/src/components/domain/investment/InvestmentDocumentsManager.tsx:376](/F:/Projects/Pension_Management_System/ui/src/components/domain/investment/InvestmentDocumentsManager.tsx:376) | `bg-black/5` |
| [ui/src/components/domain/investment/WorkOrderDialog.tsx:54](/F:/Projects/Pension_Management_System/ui/src/components/domain/investment/WorkOrderDialog.tsx:54) | `bg-muted/10` |
| [ui/src/components/domain/investment/WorkOrderDialog.tsx:75](/F:/Projects/Pension_Management_System/ui/src/components/domain/investment/WorkOrderDialog.tsx:75) | `bg-muted/30` |
| [ui/src/components/domain/retirement/AddFinancialEntitlementDialog.tsx:99](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddFinancialEntitlementDialog.tsx:99) | `dark:bg-amber-950/20` |
| [ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:125](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:125) | `bg-muted/20` |
| [ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:142](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:142) | `bg-primary/5` |
| [ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:143](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:143) | `bg-muted/30` |
| [ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:143](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:143) | `hover:bg-muted/60` |
| [ui/src/components/domain/retirement/AgentDialog.tsx:515](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AgentDialog.tsx:515) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/AgentDialog.tsx:627](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AgentDialog.tsx:627) | `bg-primary/5` |
| [ui/src/components/domain/retirement/AgentDialog.tsx:673](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AgentDialog.tsx:673) | `bg-primary/10` |
| [ui/src/components/domain/retirement/AgentDialog.tsx:673](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AgentDialog.tsx:673) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/AgentDialog.tsx:714](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AgentDialog.tsx:714) | `bg-muted/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:192](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:192) | `bg-muted/20` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:192](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:192) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:199](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:199) | `bg-primary/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:255](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:255) | `hover:bg-destructive/10` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:320](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:320) | `bg-amber-500/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:347](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:347) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1310](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1310) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1310](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1310) | `bg-muted/20` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1316](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1316) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1326](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1326) | `bg-muted/20` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1330](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1330) | `bg-muted/20` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1336](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1336) | `bg-muted/20` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1349](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1349) | `bg-muted/10` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1395](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1395) | `bg-primary/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1423](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1423) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1424](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1424) | `bg-emerald-500/5` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1517](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1517) | `bg-muted/10` |
| [ui/src/components/domain/retirement/BulkUploadDialog.tsx:1553](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/BulkUploadDialog.tsx:1553) | `bg-muted/10` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1132](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1132) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1220](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1220) | `bg-primary/10` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1228](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1228) | `bg-primary/10` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1256](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1256) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1270](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1270) | `bg-emerald-50/50` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1318](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1318) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1407](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1407) | `bg-muted/5` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1411](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1411) | `bg-primary/10` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1411](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1411) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1421](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1421) | `bg-destructive/10` |
| [ui/src/components/domain/retirement/CreateFileWizard.tsx:1421](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/CreateFileWizard.tsx:1421) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/DataEntryView.tsx:110](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DataEntryView.tsx:110) | `bg-primary/10` |
| [ui/src/components/domain/retirement/DataEntryView.tsx:209](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DataEntryView.tsx:209) | `bg-primary/10` |
| [ui/src/components/domain/retirement/DocumentChecklistField.tsx:44](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentChecklistField.tsx:44) | `bg-background/30` |
| [ui/src/components/domain/retirement/DocumentChecklistField.tsx:59](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentChecklistField.tsx:59) | `bg-primary/5` |
| [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:644](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:644) | `bg-muted/20` |
| [ui/src/components/domain/retirement/DocumentProcessDialog.tsx:664](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentProcessDialog.tsx:664) | `bg-background/50` |
| [ui/src/components/domain/retirement/DocumentReviewPanel.tsx:131](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentReviewPanel.tsx:131) | `bg-muted/30` |
| [ui/src/components/domain/retirement/DocumentReviewPanel.tsx:180](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentReviewPanel.tsx:180) | `bg-muted/10` |
| [ui/src/components/domain/retirement/DocumentReviewPanel.tsx:288](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentReviewPanel.tsx:288) | `bg-muted/50` |
| [ui/src/components/domain/retirement/DocumentReviewPanel.tsx:298](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentReviewPanel.tsx:298) | `bg-muted/50` |
| [ui/src/components/domain/retirement/DocumentReviewPanel.tsx:314](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentReviewPanel.tsx:314) | `bg-destructive/5` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:107](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:107) | `bg-background/95` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:107](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:107) | `supports-[backdrop-filter]:bg-background/60` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:123](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:123) | `bg-muted/40` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:135](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:135) | `bg-muted/20` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:179](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:179) | `hover:bg-primary/90` |
| [ui/src/components/domain/retirement/EntitledPersonDetails.tsx:39](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitledPersonDetails.tsx:39) | `from-primary/5` |
| [ui/src/components/domain/retirement/EntitledPersonDetails.tsx:39](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitledPersonDetails.tsx:39) | `to-primary/10` |
| [ui/src/components/domain/retirement/EntitledPersonDetails.tsx:134](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitledPersonDetails.tsx:134) | `bg-card/80` |
| [ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx:488](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx:488) | `bg-primary/5` |
| [ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx:488](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx:488) | `hover:bg-primary/10` |
| [ui/src/components/domain/retirement/EntitlementAmountField.tsx:40](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitlementAmountField.tsx:40) | `bg-muted/20` |
| [ui/src/components/domain/retirement/EntitlementAmountField.tsx:49](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitlementAmountField.tsx:49) | `bg-destructive/10` |
| [ui/src/components/domain/retirement/EntitlementAmountField.tsx:49](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/EntitlementAmountField.tsx:49) | `bg-primary/5` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:44](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:44) | `from-primary/15` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:44](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:44) | `via-primary/5` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:46](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:46) | `via-primary/20` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:54](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:54) | `from-primary/30` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:54](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:54) | `to-primary/10` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:85](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:85) | `bg-primary/8` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:140](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:140) | `dark:bg-red-950/20` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:155](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:155) | `bg-muted/40` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:155](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:155) | `hover:bg-muted/60` |
| [ui/src/components/domain/retirement/FulfillPledgeDialog.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FulfillPledgeDialog.tsx:103) | `bg-muted/20` |
| [ui/src/components/domain/retirement/FulfillPledgeDialog.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FulfillPledgeDialog.tsx:103) | `bg-success/5` |
| [ui/src/components/domain/retirement/FulfillPledgeDialog.tsx:128](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FulfillPledgeDialog.tsx:128) | `bg-success/10` |
| [ui/src/components/domain/retirement/GuardianDialog.tsx:233](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/GuardianDialog.tsx:233) | `bg-background/30` |
| [ui/src/components/domain/retirement/GuardianDialog.tsx:252](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/GuardianDialog.tsx:252) | `bg-primary/5` |
| [ui/src/components/domain/retirement/GuardianDialog.tsx:253](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/GuardianDialog.tsx:253) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/PaymentBookDialog.tsx:101](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PaymentBookDialog.tsx:101) | `bg-muted/20` |
| [ui/src/components/domain/retirement/PaymentBookDialog.tsx:292](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PaymentBookDialog.tsx:292) | `bg-primary/5` |
| [ui/src/components/domain/retirement/PaymentBookDialog.tsx:306](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PaymentBookDialog.tsx:306) | `bg-primary/5` |
| [ui/src/components/domain/retirement/PaymentBookDialog.tsx:560](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PaymentBookDialog.tsx:560) | `bg-muted/20` |
| [ui/src/components/domain/retirement/PersonDocCard.tsx:60](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PersonDocCard.tsx:60) | `dark:bg-red-950/20` |
| [ui/src/components/domain/retirement/PersonDocCard.tsx:82](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PersonDocCard.tsx:82) | `dark:bg-amber-950/20` |
| [ui/src/components/domain/retirement/PledgeDialog.tsx:284](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PledgeDialog.tsx:284) | `bg-primary/5` |
| [ui/src/components/domain/retirement/PledgeDialog.tsx:284](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PledgeDialog.tsx:284) | `hover:bg-primary/10` |
| [ui/src/components/domain/retirement/RecipientsDialog.tsx:221](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RecipientsDialog.tsx:221) | `bg-rose-50/30` |
| [ui/src/components/domain/retirement/RecipientsDialog.tsx:221](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RecipientsDialog.tsx:221) | `dark:bg-rose-950/10` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:69](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:69) | `from-primary/15` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:69](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:69) | `via-primary/5` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:72](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:72) | `via-primary/20` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:80](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:80) | `from-primary/30` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:80](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:80) | `to-primary/10` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:113](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:113) | `bg-primary/8` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:214](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:214) | `bg-muted/40` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:214](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:214) | `hover:bg-muted/60` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:228](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:228) | `hover:bg-muted/30` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:102](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:102) | `bg-emerald-50/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:102](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:102) | `dark:bg-emerald-950/20` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:103) | `bg-rose-50/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:103) | `dark:bg-rose-950/20` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:148](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:148) | `dark:bg-emerald-950/20` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:165](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:165) | `bg-emerald-100/60` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:165](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:165) | `dark:bg-emerald-900/40` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:210](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:210) | `bg-muted/40` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:217](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:217) | `bg-rose-100/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:217](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:217) | `dark:bg-rose-900/30` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:217](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:217) | `dark:hover:bg-rose-900/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:217](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:217) | `hover:bg-rose-200/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:232](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:232) | `bg-border/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:239](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:239) | `bg-emerald-100/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:239](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:239) | `dark:bg-emerald-900/30` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:239](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:239) | `dark:hover:bg-emerald-900/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:239](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:239) | `hover:bg-emerald-200/50` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:262](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:262) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:285](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:285) | `bg-primary/10` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:285](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:285) | `hover:bg-primary/20` |
| [ui/src/components/domain/retirement/ReviewItemCard.tsx:327](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewItemCard.tsx:327) | `dark:bg-rose-950/20` |
| [ui/src/components/domain/retirement/ReviewPanel.tsx:74](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewPanel.tsx:74) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ReviewPanel.tsx:78](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewPanel.tsx:78) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ReviewPanel.tsx:82](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewPanel.tsx:82) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ReviewPanel.tsx:86](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ReviewPanel.tsx:86) | `bg-muted/30` |
| [ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:152](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:152) | `hover:bg-primary/10` |
| [ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:165](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:165) | `from-primary/30` |
| [ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:165](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:165) | `to-primary/10` |
| [ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:214](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/BasicInfoSection.tsx:214) | `hover:bg-primary/10` |
| [ui/src/components/domain/retirement/sections/DocumentsSection.tsx:145](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/DocumentsSection.tsx:145) | `dark:bg-red-950/20` |
| [ui/src/components/domain/retirement/sections/DocumentsSection.tsx:188](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/DocumentsSection.tsx:188) | `bg-primary/5` |
| [ui/src/components/domain/retirement/sections/DocumentsSection.tsx:225](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/DocumentsSection.tsx:225) | `bg-muted/30` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:91](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:91) | `bg-muted/20` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:99](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:99) | `bg-muted/20` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:108](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:108) | `bg-muted/10` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:117](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:117) | `bg-background/80` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:128](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:128) | `bg-background/80` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:397](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:397) | `hover:bg-primary/10` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:410](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:410) | `hover:bg-primary/10` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:464](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:464) | `bg-primary/5` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:489](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:489) | `dark:hover:bg-blue-950/30` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:504](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:504) | `bg-muted/30` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:604](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:604) | `dark:hover:bg-red-950/30` |
| [ui/src/components/domain/retirement/sections/HistorySection.tsx:25](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/HistorySection.tsx:25) | `bg-primary/10` |
| [ui/src/components/domain/retirement/sections/HistorySection.tsx:50](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/HistorySection.tsx:50) | `bg-primary/10` |
| [ui/src/components/domain/retirement/sections/HistorySection.tsx:93](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/HistorySection.tsx:93) | `bg-muted/50` |
| [ui/src/components/domain/retirement/sections/HistorySection.tsx:101](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/HistorySection.tsx:101) | `bg-muted/30` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:371](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:371) | `bg-primary/5` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:436](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:436) | `dark:hover:bg-blue-950/30` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:486](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:486) | `from-primary/5` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:486](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:486) | `to-primary/10` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:578](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:578) | `bg-muted/50` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:747](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:747) | `bg-black/50` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:753](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:753) | `bg-destructive/10` |
| [ui/src/components/domain/retirement/StatusUpdateDialog.tsx:122](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/StatusUpdateDialog.tsx:122) | `bg-muted/30` |
| [ui/src/components/domain/retirement/StatusUpdateDialog.tsx:180](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/StatusUpdateDialog.tsx:180) | `bg-amber-50/30` |
| [ui/src/components/domain/retirement/StatusUpdateDialog.tsx:180](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/StatusUpdateDialog.tsx:180) | `bg-muted/10` |
| [ui/src/components/domain/retirement/StatusUpdateDialog.tsx:191](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/StatusUpdateDialog.tsx:191) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ViewAgentDialog.tsx:75](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewAgentDialog.tsx:75) | `from-primary/5` |
| [ui/src/components/domain/retirement/ViewAgentDialog.tsx:75](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewAgentDialog.tsx:75) | `to-primary/10` |
| [ui/src/components/domain/retirement/ViewAgentDialog.tsx:174](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewAgentDialog.tsx:174) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ViewAgentDialog.tsx:182](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewAgentDialog.tsx:182) | `bg-muted/50` |
| [ui/src/components/domain/retirement/ViewAgentDialog.tsx:184](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewAgentDialog.tsx:184) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:178](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:178) | `hover:bg-muted/50` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:237](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:237) | `bg-muted/20` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:247](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:247) | `bg-muted/30` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:250](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:250) | `bg-primary/10` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:275](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:275) | `bg-black/20` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:276](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:276) | `bg-black/40` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:294](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:294) | `bg-primary/[0.02]` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:319](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:319) | `bg-muted/20` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:324](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:324) | `bg-destructive/10` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:346](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:346) | `bg-primary/[0.03]` |
| [ui/src/components/domain/salaries/EntitlementsTab.tsx:102](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/EntitlementsTab.tsx:102) | `hover:bg-destructive/10` |
| [ui/src/components/domain/salaries/RecipientsTab.tsx:149](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/RecipientsTab.tsx:149) | `hover:bg-destructive/10` |
| [ui/src/components/domain/salaries/RecipientsTab.tsx:164](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/RecipientsTab.tsx:164) | `bg-muted/30` |
| [ui/src/components/domain/salaries/SalaryRecipientFormDialog.tsx:287](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/SalaryRecipientFormDialog.tsx:287) | `bg-rose-50/30` |
| [ui/src/components/domain/salaries/SalaryRecipientFormDialog.tsx:287](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/SalaryRecipientFormDialog.tsx:287) | `dark:bg-rose-950/10` |
| [ui/src/components/domain/salaries/SalaryRecipientViewerDialog.tsx:41](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/SalaryRecipientViewerDialog.tsx:41) | `from-primary/90` |
| [ui/src/components/domain/salaries/SalaryRecipientViewerDialog.tsx:89](/F:/Projects/Pension_Management_System/ui/src/components/domain/salaries/SalaryRecipientViewerDialog.tsx:89) | `bg-primary/20` |
| [ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:147](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:147) | `bg-destructive/10` |
| [ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:158](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:158) | `hover:bg-destructive/10` |
| [ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:202](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:202) | `hover:bg-muted/50` |
| [ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:232](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:232) | `hover:bg-muted/50` |
| [ui/src/components/domain/settlements/LinkRetireeDialog.tsx:107](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/LinkRetireeDialog.tsx:107) | `bg-muted/30` |
| [ui/src/components/domain/settlements/LinkRetireeDialog.tsx:162](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/LinkRetireeDialog.tsx:162) | `hover:bg-muted/40` |
| [ui/src/components/domain/settlements/LinkRetireeDialog.tsx:163](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/LinkRetireeDialog.tsx:163) | `bg-primary/5` |
| [ui/src/components/domain/suspensions/CreateSuspensionDialog.tsx:133](/F:/Projects/Pension_Management_System/ui/src/components/domain/suspensions/CreateSuspensionDialog.tsx:133) | `bg-destructive/5` |
| [ui/src/components/domain/suspensions/CreateSuspensionDialog.tsx:135](/F:/Projects/Pension_Management_System/ui/src/components/domain/suspensions/CreateSuspensionDialog.tsx:135) | `bg-destructive/10` |
| [ui/src/components/ErrorBoundary.tsx:51](/F:/Projects/Pension_Management_System/ui/src/components/ErrorBoundary.tsx:51) | `dark:bg-red-950/50` |
| [ui/src/components/layout/MainLayout.tsx:88](/F:/Projects/Pension_Management_System/ui/src/components/layout/MainLayout.tsx:88) | `bg-black/40` |
| [ui/src/components/layout/NavDropdown.tsx:51](/F:/Projects/Pension_Management_System/ui/src/components/layout/NavDropdown.tsx:51) | `bg-primary/10` |
| [ui/src/components/layout/NavDropdown.tsx:102](/F:/Projects/Pension_Management_System/ui/src/components/layout/NavDropdown.tsx:102) | `bg-primary/5` |
| [ui/src/components/layout/NavDropdown.tsx:174](/F:/Projects/Pension_Management_System/ui/src/components/layout/NavDropdown.tsx:174) | `bg-primary/30` |
| [ui/src/components/layout/NotificationBell.tsx:61](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:61) | `hover:bg-accent/50` |
| [ui/src/components/layout/NotificationBell.tsx:62](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:62) | `bg-primary/[0.03]` |
| [ui/src/components/layout/NotificationBell.tsx:62](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:62) | `hover:bg-primary/[0.06]` |
| [ui/src/components/layout/NotificationBell.tsx:145](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:145) | `bg-muted/40` |
| [ui/src/components/layout/NotificationBell.tsx:163](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:163) | `hover:bg-primary/10` |
| [ui/src/components/layout/NotificationBell.tsx:182](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:182) | `bg-muted/30` |
| [ui/src/components/layout/NotificationBell.tsx:195](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:195) | `bg-muted/30` |
| [ui/src/components/layout/PageLayout.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/layout/PageLayout.tsx:37) | `bg-background/95` |
| [ui/src/components/layout/SecondaryNav.tsx:60](/F:/Projects/Pension_Management_System/ui/src/components/layout/SecondaryNav.tsx:60) | `bg-primary/10` |
| [ui/src/components/layout/SecondaryNav.tsx:79](/F:/Projects/Pension_Management_System/ui/src/components/layout/SecondaryNav.tsx:79) | `bg-primary/10` |
| [ui/src/components/layout/SecondaryNav.tsx:148](/F:/Projects/Pension_Management_System/ui/src/components/layout/SecondaryNav.tsx:148) | `bg-primary/10` |
| [ui/src/components/layout/Sidebar.tsx:42](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:42) | `bg-white/[0.12]` |
| [ui/src/components/layout/Sidebar.tsx:43](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:43) | `hover:bg-white/[0.08]` |
| [ui/src/components/layout/Sidebar.tsx:94](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:94) | `hover:bg-white/[0.04]` |
| [ui/src/components/layout/Sidebar.tsx:99](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:99) | `bg-white/[0.12]` |
| [ui/src/components/layout/Sidebar.tsx:296](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:296) | `hover:bg-white/[0.08]` |
| [ui/src/components/layout/Sidebar.tsx:345](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:345) | `hover:bg-white/[0.04]` |
| [ui/src/components/layout/Sidebar.tsx:395](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:395) | `bg-white/[0.08]` |
| [ui/src/components/layout/Sidebar.tsx:410](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:410) | `hover:bg-white/[0.08]` |
| [ui/src/components/layout/TopNav.tsx:61](/F:/Projects/Pension_Management_System/ui/src/components/layout/TopNav.tsx:61) | `hover:bg-muted/50` |
| [ui/src/components/layout/TopNav.tsx:69](/F:/Projects/Pension_Management_System/ui/src/components/layout/TopNav.tsx:69) | `bg-primary/10` |
| [ui/src/components/layout/TopNav.tsx:116](/F:/Projects/Pension_Management_System/ui/src/components/layout/TopNav.tsx:116) | `focus:bg-destructive/10` |
| [ui/src/components/layout/TopNav.tsx:222](/F:/Projects/Pension_Management_System/ui/src/components/layout/TopNav.tsx:222) | `bg-border/60` |
| [ui/src/components/notifications/FloatingNotification.tsx:14](/F:/Projects/Pension_Management_System/ui/src/components/notifications/FloatingNotification.tsx:14) | `bg-info/20` |
| [ui/src/components/notifications/FloatingNotification.tsx:15](/F:/Projects/Pension_Management_System/ui/src/components/notifications/FloatingNotification.tsx:15) | `bg-success/20` |
| [ui/src/components/notifications/FloatingNotification.tsx:16](/F:/Projects/Pension_Management_System/ui/src/components/notifications/FloatingNotification.tsx:16) | `bg-danger/20` |
| [ui/src/components/notifications/FloatingNotification.tsx:18](/F:/Projects/Pension_Management_System/ui/src/components/notifications/FloatingNotification.tsx:18) | `bg-warning/20` |
| [ui/src/components/notifications/FloatingNotification.tsx:19](/F:/Projects/Pension_Management_System/ui/src/components/notifications/FloatingNotification.tsx:19) | `bg-special/20` |
| [ui/src/components/ui/AlertBanner.tsx:36](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:36) | `dark:bg-amber-950/30` |
| [ui/src/components/ui/AlertBanner.tsx:38](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:38) | `dark:bg-blue-950/30` |
| [ui/src/components/ui/AlertBanner.tsx:40](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:40) | `dark:bg-red-950/30` |
| [ui/src/components/ui/AlertBanner.tsx:42](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:42) | `dark:bg-emerald-950/30` |
| [ui/src/components/ui/AlertBanner.tsx:44](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:44) | `dark:bg-red-950/30` |
| [ui/src/components/ui/AlertBanner.tsx:99](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:99) | `dark:hover:bg-white/10` |
| [ui/src/components/ui/AlertBanner.tsx:99](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:99) | `hover:bg-black/5` |
| [ui/src/components/ui/AlertBanner.tsx:122](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:122) | `dark:hover:bg-white/10` |
| [ui/src/components/ui/AlertBanner.tsx:122](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertBanner.tsx:122) | `hover:bg-black/5` |
| [ui/src/components/ui/AlertDialog.tsx:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:19) | `bg-black/80` |
| [ui/src/components/ui/AlertDialog.tsx:110](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:110) | `hover:bg-primary/90` |
| [ui/src/components/ui/AuditTimeline.tsx:27](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:27) | `bg-emerald-500/10` |
| [ui/src/components/ui/AuditTimeline.tsx:35](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:35) | `bg-rose-500/10` |
| [ui/src/components/ui/AuditTimeline.tsx:43](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:43) | `bg-blue-500/10` |
| [ui/src/components/ui/AuditTimeline.tsx:51](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:51) | `bg-amber-500/10` |
| [ui/src/components/ui/AuditTimeline.tsx:142](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:142) | `from-primary/20` |
| [ui/src/components/ui/AuditTimeline.tsx:142](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:142) | `via-border/50` |
| [ui/src/components/ui/AuditTimeline.tsx:151](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:151) | `bg-primary/5` |
| [ui/src/components/ui/AuditTimeline.tsx:207](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:207) | `bg-primary/[0.03]` |
| [ui/src/components/ui/AuditTimeline.tsx:212](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:212) | `bg-primary/10` |
| [ui/src/components/ui/AuditTimeline.tsx:224](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:224) | `bg-muted/20` |
| [ui/src/components/ui/AuditTimeline.tsx:227](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:227) | `bg-muted/40` |
| [ui/src/components/ui/AuditTimeline.tsx:235](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:235) | `hover:bg-background/50` |
| [ui/src/components/ui/badgeVariants.ts:9](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:9) | `dark:bg-emerald-900/30` |
| [ui/src/components/ui/badgeVariants.ts:11](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:11) | `dark:bg-amber-900/30` |
| [ui/src/components/ui/badgeVariants.ts:13](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:13) | `dark:bg-red-900/30` |
| [ui/src/components/ui/badgeVariants.ts:15](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:15) | `dark:bg-red-900/30` |
| [ui/src/components/ui/badgeVariants.ts:17](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:17) | `dark:bg-blue-900/30` |
| [ui/src/components/ui/badgeVariants.ts:25](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:25) | `bg-primary/10` |
| [ui/src/components/ui/BaseFormDialog.tsx:265](/F:/Projects/Pension_Management_System/ui/src/components/ui/BaseFormDialog.tsx:265) | `bg-muted/10` |
| [ui/src/components/ui/BaseSheet.tsx:109](/F:/Projects/Pension_Management_System/ui/src/components/ui/BaseSheet.tsx:109) | `bg-background/50` |
| [ui/src/components/ui/BaseSheet.tsx:114](/F:/Projects/Pension_Management_System/ui/src/components/ui/BaseSheet.tsx:114) | `bg-muted/20` |
| [ui/src/components/ui/buttonVariants.ts:9](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:9) | `hover:bg-primary/90` |
| [ui/src/components/ui/buttonVariants.ts:11](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:11) | `hover:bg-secondary/80` |
| [ui/src/components/ui/buttonVariants.ts:13](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:13) | `hover:bg-primary/90` |
| [ui/src/components/ui/buttonVariants.ts:15](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:15) | `hover:bg-destructive/90` |
| [ui/src/components/ui/buttonVariants.ts:27](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:27) | `hover:bg-primary/5` |
| [ui/src/components/ui/Card.tsx:16](/F:/Projects/Pension_Management_System/ui/src/components/ui/Card.tsx:16) | `bg-muted/30` |
| [ui/src/components/ui/CornerEditor.tsx:1085](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:1085) | `bg-overlay/5` |
| [ui/src/components/ui/CornerEditor.tsx:1107](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:1107) | `bg-overlay/60` |
| [ui/src/components/ui/DataTable.tsx:200](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:200) | `bg-muted/20` |
| [ui/src/components/ui/DataTable.tsx:360](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:360) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:362](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:362) | `bg-primary/10` |
| [ui/src/components/ui/DataTable.tsx:432](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:432) | `hover:bg-muted/40` |
| [ui/src/components/ui/DataTable.tsx:500](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:500) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:503](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:503) | `bg-primary/10` |
| [ui/src/components/ui/DataTable.tsx:629](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:629) | `bg-muted/10` |
| [ui/src/components/ui/DataTable.tsx:688](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:688) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:707](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:707) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:726](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:726) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:739](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:739) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:758](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:758) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:774](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:774) | `hover:bg-primary/5` |
| [ui/src/components/ui/DataTable.tsx:787](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:787) | `hover:bg-primary/5` |
| [ui/src/components/ui/Dialog.tsx:362](/F:/Projects/Pension_Management_System/ui/src/components/ui/Dialog.tsx:362) | `bg-black/0` |
| [ui/src/components/ui/Dialog.tsx:362](/F:/Projects/Pension_Management_System/ui/src/components/ui/Dialog.tsx:362) | `bg-black/50` |
| [ui/src/components/ui/Dialog.tsx:463](/F:/Projects/Pension_Management_System/ui/src/components/ui/Dialog.tsx:463) | `bg-muted/5` |
| [ui/src/components/ui/EditableImage.tsx:162](/F:/Projects/Pension_Management_System/ui/src/components/ui/EditableImage.tsx:162) | `bg-muted/50` |
| [ui/src/components/ui/EditableImage.tsx:175](/F:/Projects/Pension_Management_System/ui/src/components/ui/EditableImage.tsx:175) | `hover:bg-primary/90` |
| [ui/src/components/ui/EmptyState.tsx:34](/F:/Projects/Pension_Management_System/ui/src/components/ui/EmptyState.tsx:34) | `bg-muted/5` |
| [ui/src/components/ui/EmptyState.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/ui/EmptyState.tsx:37) | `bg-primary/5` |
| [ui/src/components/ui/EmptyState.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/ui/EmptyState.tsx:37) | `hover:bg-primary/10` |
| [ui/src/components/ui/ErrorAlert.tsx:26](/F:/Projects/Pension_Management_System/ui/src/components/ui/ErrorAlert.tsx:26) | `bg-red-50/60` |
| [ui/src/components/ui/ErrorAlert.tsx:31](/F:/Projects/Pension_Management_System/ui/src/components/ui/ErrorAlert.tsx:31) | `bg-red-100/80` |
| [ui/src/components/ui/ErrorState.tsx:28](/F:/Projects/Pension_Management_System/ui/src/components/ui/ErrorState.tsx:28) | `bg-destructive/5` |
| [ui/src/components/ui/ErrorState.tsx:33](/F:/Projects/Pension_Management_System/ui/src/components/ui/ErrorState.tsx:33) | `bg-destructive/10` |
| [ui/src/components/ui/FileUploader.tsx:730](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:730) | `hover:bg-primary/5` |
| [ui/src/components/ui/FileUploader.tsx:731](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:731) | `bg-primary/5` |
| [ui/src/components/ui/FileUploader.tsx:803](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:803) | `hover:bg-destructive/10` |
| [ui/src/components/ui/FileUploader.tsx:878](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:878) | `hover:bg-primary/10` |
| [ui/src/components/ui/FileUploader.tsx:898](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:898) | `hover:bg-blue-500/10` |
| [ui/src/components/ui/FileUploader.tsx:1019](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:1019) | `bg-muted/10` |
| [ui/src/components/ui/FormDialogFooter.tsx:60](/F:/Projects/Pension_Management_System/ui/src/components/ui/FormDialogFooter.tsx:60) | `bg-black/20` |
| [ui/src/components/ui/FormDialogFooter.tsx:77](/F:/Projects/Pension_Management_System/ui/src/components/ui/FormDialogFooter.tsx:77) | `hover:bg-muted/50` |
| [ui/src/components/ui/FormDialogFooter.tsx:97](/F:/Projects/Pension_Management_System/ui/src/components/ui/FormDialogFooter.tsx:97) | `hover:bg-muted/50` |
| [ui/src/components/ui/FormSection.tsx:73](/F:/Projects/Pension_Management_System/ui/src/components/ui/FormSection.tsx:73) | `bg-primary/10` |
| [ui/src/components/ui/ImageCropperDialog.tsx:245](/F:/Projects/Pension_Management_System/ui/src/components/ui/ImageCropperDialog.tsx:245) | `bg-muted/5` |
| [ui/src/components/ui/Input.tsx:87](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:87) | `bg-muted/30` |
| [ui/src/components/ui/Input.tsx:87](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:87) | `group-hover:bg-muted/50` |
| [ui/src/components/ui/Input.tsx:106](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:106) | `hover:bg-muted/5` |
| [ui/src/components/ui/Loader.tsx:47](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:47) | `bg-primary/[0.07]` |
| [ui/src/components/ui/Loader.tsx:62](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:62) | `bg-primary/10` |
| [ui/src/components/ui/Loader.tsx:142](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:142) | `bg-background/90` |
| [ui/src/components/ui/NoteBox.tsx:36](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:36) | `bg-blue-50/40` |
| [ui/src/components/ui/NoteBox.tsx:39](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:39) | `bg-blue-100/80` |
| [ui/src/components/ui/NoteBox.tsx:43](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:43) | `bg-amber-50/50` |
| [ui/src/components/ui/NoteBox.tsx:46](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:46) | `bg-amber-100/80` |
| [ui/src/components/ui/NoteBox.tsx:50](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:50) | `bg-emerald-50/40` |
| [ui/src/components/ui/NoteBox.tsx:53](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:53) | `bg-emerald-100/80` |
| [ui/src/components/ui/NoteBox.tsx:57](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:57) | `bg-slate-50/60` |
| [ui/src/components/ui/NoteBox.tsx:60](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:60) | `bg-slate-100/80` |
| [ui/src/components/ui/NoteBox.tsx:70](/F:/Projects/Pension_Management_System/ui/src/components/ui/NoteBox.tsx:70) | `hover:bg-opacity-80` |
| [ui/src/components/ui/PageHeader.tsx:29](/F:/Projects/Pension_Management_System/ui/src/components/ui/PageHeader.tsx:29) | `bg-primary/10` |
| [ui/src/components/ui/print/PrintableReportTable.tsx:115](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintableReportTable.tsx:115) | `bg-slate-50/60` |
| [ui/src/components/ui/print/PrintableReportTable.tsx:116](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintableReportTable.tsx:116) | `hover:bg-slate-100/80` |
| [ui/src/components/ui/print/PrintField.tsx:18](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintField.tsx:18) | `even:bg-gray-50/50` |
| [ui/src/components/ui/print/PrintSignatureBlock.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintSignatureBlock.tsx:103) | `bg-gray-50/80` |
| [ui/src/components/ui/RecordStateBadge.config.ts:28](/F:/Projects/Pension_Management_System/ui/src/components/ui/RecordStateBadge.config.ts:28) | `dark:bg-amber-950/30` |
| [ui/src/components/ui/RecordStateBadge.config.ts:33](/F:/Projects/Pension_Management_System/ui/src/components/ui/RecordStateBadge.config.ts:33) | `dark:bg-blue-950/30` |
| [ui/src/components/ui/RecordStateBadge.config.ts:38](/F:/Projects/Pension_Management_System/ui/src/components/ui/RecordStateBadge.config.ts:38) | `dark:bg-emerald-950/30` |
| [ui/src/components/ui/RecordStateBadge.config.ts:43](/F:/Projects/Pension_Management_System/ui/src/components/ui/RecordStateBadge.config.ts:43) | `dark:bg-slate-950/30` |
| [ui/src/components/ui/RecordStateBadge.config.ts:53](/F:/Projects/Pension_Management_System/ui/src/components/ui/RecordStateBadge.config.ts:53) | `dark:bg-red-950/30` |
| [ui/src/components/ui/RecordStateBadge.config.ts:58](/F:/Projects/Pension_Management_System/ui/src/components/ui/RecordStateBadge.config.ts:58) | `dark:bg-red-950/30` |
| [ui/src/components/ui/Sheet.tsx:21](/F:/Projects/Pension_Management_System/ui/src/components/ui/Sheet.tsx:21) | `bg-black/80` |
| [ui/src/components/ui/Skeleton.tsx:25](/F:/Projects/Pension_Management_System/ui/src/components/ui/Skeleton.tsx:25) | `bg-muted/50` |
| [ui/src/components/ui/Skeleton.tsx:54](/F:/Projects/Pension_Management_System/ui/src/components/ui/Skeleton.tsx:54) | `bg-muted/40` |
| [ui/src/components/ui/Slider.tsx:18](/F:/Projects/Pension_Management_System/ui/src/components/ui/Slider.tsx:18) | `bg-primary/20` |
| [ui/src/components/ui/StatCard.tsx:24](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:24) | `bg-blue-500/10` |
| [ui/src/components/ui/StatCard.tsx:25](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:25) | `bg-orange-500/10` |
| [ui/src/components/ui/StatCard.tsx:26](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:26) | `bg-emerald-500/10` |
| [ui/src/components/ui/StatCard.tsx:27](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:27) | `bg-purple-500/10` |
| [ui/src/components/ui/StatCard.tsx:28](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:28) | `bg-rose-500/10` |
| [ui/src/components/ui/StatCard.tsx:29](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:29) | `bg-amber-500/10` |
| [ui/src/components/ui/StatCard.tsx:30](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:30) | `bg-cyan-500/10` |
| [ui/src/components/ui/StatCard.tsx:31](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:31) | `bg-indigo-500/10` |
| [ui/src/components/ui/StatCard.tsx:32](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:32) | `bg-teal-500/10` |
| [ui/src/components/ui/StatCard.tsx:33](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:33) | `bg-lime-500/10` |
| [ui/src/components/ui/StatCard.tsx:34](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:34) | `bg-emerald-500/10` |
| [ui/src/components/ui/StatCard.tsx:35](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:35) | `bg-amber-500/10` |
| [ui/src/components/ui/StatCard.tsx:36](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:36) | `bg-rose-500/10` |
| [ui/src/components/ui/StatCard.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:37) | `from-muted-foreground/40` |
| [ui/src/components/ui/StatCard.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:37) | `to-muted-foreground/60` |
| [ui/src/components/ui/StatCard.tsx:38](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:38) | `bg-blue-500/10` |
| [ui/src/components/ui/StatCard.tsx:39](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:39) | `bg-rose-500/10` |
| [ui/src/components/ui/StatCard.tsx:40](/F:/Projects/Pension_Management_System/ui/src/components/ui/StatCard.tsx:40) | `bg-blue-500/10` |
| [ui/src/components/ui/Stepper.tsx:81](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:81) | `bg-primary/20` |
| [ui/src/components/ui/TabbedFormSection.tsx:85](/F:/Projects/Pension_Management_System/ui/src/components/ui/TabbedFormSection.tsx:85) | `bg-muted/30` |
| [ui/src/components/ui/Table.tsx:46](/F:/Projects/Pension_Management_System/ui/src/components/ui/Table.tsx:46) | `hover:bg-muted/50` |
| [ui/src/pages/accounting/AccountingDashboardPage.tsx:60](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/AccountingDashboardPage.tsx:60) | `bg-primary/10` |
| [ui/src/pages/accounting/AccountingDashboardPage.tsx:180](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/AccountingDashboardPage.tsx:180) | `hover:bg-accent/50` |
| [ui/src/pages/accounting/BudgetPage.tsx:137](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/BudgetPage.tsx:137) | `bg-primary/10` |
| [ui/src/pages/accounting/CashFlowPage.tsx:65](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CashFlowPage.tsx:65) | `bg-primary/10` |
| [ui/src/pages/accounting/CashFlowPage.tsx:168](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CashFlowPage.tsx:168) | `bg-muted/30` |
| [ui/src/pages/accounting/CashFlowPage.tsx:202](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CashFlowPage.tsx:202) | `bg-muted/30` |
| [ui/src/pages/accounting/CashFlowPage.tsx:236](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CashFlowPage.tsx:236) | `bg-muted/30` |
| [ui/src/pages/accounting/ChartOfAccountsPage.tsx:182](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ChartOfAccountsPage.tsx:182) | `bg-primary/10` |
| [ui/src/pages/accounting/ChartOfAccountsPage.tsx:269](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ChartOfAccountsPage.tsx:269) | `from-primary/5` |
| [ui/src/pages/accounting/ChartOfAccountsPage.tsx:269](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ChartOfAccountsPage.tsx:269) | `to-primary/10` |
| [ui/src/pages/accounting/ChartOfAccountsPage.tsx:272](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ChartOfAccountsPage.tsx:272) | `bg-primary/10` |
| [ui/src/pages/accounting/ChartOfAccountsPage.tsx:340](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ChartOfAccountsPage.tsx:340) | `bg-muted/10` |
| [ui/src/pages/accounting/CostCentersPage.tsx:73](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CostCentersPage.tsx:73) | `hover:bg-accent/50` |
| [ui/src/pages/accounting/CostCentersPage.tsx:81](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CostCentersPage.tsx:81) | `bg-destructive/10` |
| [ui/src/pages/accounting/CostCentersPage.tsx:107](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/CostCentersPage.tsx:107) | `bg-primary/10` |
| [ui/src/pages/accounting/FiscalPeriodsPage.tsx:65](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/FiscalPeriodsPage.tsx:65) | `bg-primary/10` |
| [ui/src/pages/accounting/FiscalPeriodsPage.tsx:105](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/FiscalPeriodsPage.tsx:105) | `bg-emerald-500/5` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:463](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:463) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:500](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:500) | `bg-amber-500/5` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:509](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:509) | `bg-destructive/5` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:522](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:522) | `bg-primary/5` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:572](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:572) | `bg-muted/30` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:574](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:574) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:580](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:580) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:607](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:607) | `bg-muted/30` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:640](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:640) | `bg-muted/30` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:642](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:642) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:648](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:648) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:675](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:675) | `bg-muted/30` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:708](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:708) | `bg-muted/30` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:710](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:710) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:715](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:715) | `bg-primary/10` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:765](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:765) | `bg-muted/20` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:795](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:795) | `bg-muted/20` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:826](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:826) | `bg-muted/20` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:847](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:847) | `bg-primary/5` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:860](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:860) | `bg-green-500/5` |
| [ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:860](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/InitialOpeningBalancesPage.tsx:860) | `bg-red-500/5` |
| [ui/src/pages/accounting/JournalEntriesPage.tsx:159](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/JournalEntriesPage.tsx:159) | `hover:bg-destructive/10` |
| [ui/src/pages/accounting/JournalEntriesPage.tsx:186](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/JournalEntriesPage.tsx:186) | `bg-primary/10` |
| [ui/src/pages/accounting/JournalEntryDetailsPage.tsx:61](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/JournalEntryDetailsPage.tsx:61) | `bg-primary/10` |
| [ui/src/pages/accounting/JournalEntryDetailsPage.tsx:137](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/JournalEntryDetailsPage.tsx:137) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:128](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:128) | `bg-primary/10` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:157](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:157) | `bg-info/10` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:161](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:161) | `bg-danger/10` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:165](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:165) | `bg-success/10` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:171](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:171) | `bg-muted/50` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:189](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:189) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:192](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:192) | `bg-muted/50` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:194](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:194) | `hover:bg-muted/20` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:195](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:195) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:205](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:205) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:209](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:209) | `bg-muted/50` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:210](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:210) | `hover:bg-muted/20` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:222](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:222) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:228](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:228) | `bg-muted/50` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:241](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:241) | `hover:bg-muted/20` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:256](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:256) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:277](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:277) | `bg-muted/30` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:389](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:389) | `bg-info/10` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:389](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:389) | `bg-success/10` |
| [ui/src/pages/accounting/QuartersPage.tsx:84](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuartersPage.tsx:84) | `bg-primary/10` |
| [ui/src/pages/accounting/QuartersPage.tsx:181](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuartersPage.tsx:181) | `bg-muted/30` |
| [ui/src/pages/accounting/QuartersPage.tsx:203](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuartersPage.tsx:203) | `bg-muted/50` |
| [ui/src/pages/accounting/QuartersPage.tsx:214](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuartersPage.tsx:214) | `hover:bg-muted/20` |
| [ui/src/pages/accounting/QuartersPage.tsx:240](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuartersPage.tsx:240) | `bg-emerald-500/5` |
| [ui/src/pages/accounting/ReportsPage.tsx:240](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ReportsPage.tsx:240) | `bg-primary/10` |
| [ui/src/pages/accounting/ReportsPage.tsx:263](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/ReportsPage.tsx:263) | `bg-muted/50` |
| [ui/src/pages/accounting/VoucherListPage.tsx:74](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/VoucherListPage.tsx:74) | `bg-primary/10` |
| [ui/src/pages/achievements/AchievementObstacleDetailDialog.tsx:334](/F:/Projects/Pension_Management_System/ui/src/pages/achievements/AchievementObstacleDetailDialog.tsx:334) | `bg-muted/30` |
| [ui/src/pages/achievements/AchievementObstacleDetailPage.tsx:245](/F:/Projects/Pension_Management_System/ui/src/pages/achievements/AchievementObstacleDetailPage.tsx:245) | `bg-muted/30` |
| [ui/src/pages/achievements/AchievementObstaclesPage.tsx:181](/F:/Projects/Pension_Management_System/ui/src/pages/achievements/AchievementObstaclesPage.tsx:181) | `bg-muted/10` |
| [ui/src/pages/admin/AdminLandingPage.tsx:68](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AdminLandingPage.tsx:68) | `bg-primary/10` |
| [ui/src/pages/admin/AdminLandingPage.tsx:95](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AdminLandingPage.tsx:95) | `hover:bg-muted/50` |
| [ui/src/pages/admin/AdminNotificationsPage.tsx:127](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AdminNotificationsPage.tsx:127) | `bg-primary/10` |
| [ui/src/pages/admin/AuditLogsPage.tsx:89](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:89) | `dark:bg-red-950/20` |
| [ui/src/pages/admin/AuditLogsPage.tsx:90](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:90) | `dark:bg-emerald-950/20` |
| [ui/src/pages/admin/AuditLogsPage.tsx:165](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:165) | `from-violet-500/20` |
| [ui/src/pages/admin/AuditLogsPage.tsx:165](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:165) | `to-purple-500/20` |
| [ui/src/pages/admin/AuditLogsPage.tsx:241](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:241) | `bg-muted/50` |
| [ui/src/pages/admin/AuditLogsPage.tsx:262](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:262) | `hover:bg-muted/30` |
| [ui/src/pages/admin/AuditLogsPage.tsx:288](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:288) | `bg-muted/20` |
| [ui/src/pages/admin/AuditLogsPage.tsx:321](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:321) | `bg-primary/5` |
| [ui/src/pages/admin/AuditLogsPage.tsx:338](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:338) | `bg-muted/50` |
| [ui/src/pages/admin/AuditLogsPage.tsx:356](/F:/Projects/Pension_Management_System/ui/src/pages/admin/AuditLogsPage.tsx:356) | `bg-muted/20` |
| [ui/src/pages/admin/LoginHistoryPage.tsx:69](/F:/Projects/Pension_Management_System/ui/src/pages/admin/LoginHistoryPage.tsx:69) | `from-blue-500/20` |
| [ui/src/pages/admin/LoginHistoryPage.tsx:69](/F:/Projects/Pension_Management_System/ui/src/pages/admin/LoginHistoryPage.tsx:69) | `to-cyan-500/20` |
| [ui/src/pages/admin/SystemSettingsPage.tsx:117](/F:/Projects/Pension_Management_System/ui/src/pages/admin/SystemSettingsPage.tsx:117) | `from-emerald-500/20` |
| [ui/src/pages/admin/SystemSettingsPage.tsx:117](/F:/Projects/Pension_Management_System/ui/src/pages/admin/SystemSettingsPage.tsx:117) | `to-teal-500/20` |
| [ui/src/pages/admin/SystemSettingsPage.tsx:177](/F:/Projects/Pension_Management_System/ui/src/pages/admin/SystemSettingsPage.tsx:177) | `bg-muted/50` |
| [ui/src/pages/admin/SystemSettingsPage.tsx:182](/F:/Projects/Pension_Management_System/ui/src/pages/admin/SystemSettingsPage.tsx:182) | `hover:bg-muted/20` |
| [ui/src/pages/archive/DocumentReviewArchivePage.tsx:569](/F:/Projects/Pension_Management_System/ui/src/pages/archive/DocumentReviewArchivePage.tsx:569) | `bg-muted/30` |
| [ui/src/pages/archive/DocumentReviewArchivePage.tsx:597](/F:/Projects/Pension_Management_System/ui/src/pages/archive/DocumentReviewArchivePage.tsx:597) | `hover:bg-muted/50` |
| [ui/src/pages/archive/DocumentReviewArchivePage.tsx:598](/F:/Projects/Pension_Management_System/ui/src/pages/archive/DocumentReviewArchivePage.tsx:598) | `bg-primary/5` |
| [ui/src/pages/archive/DocumentReviewArchivePage.tsx:661](/F:/Projects/Pension_Management_System/ui/src/pages/archive/DocumentReviewArchivePage.tsx:661) | `bg-muted/50` |
| [ui/src/pages/archive/DocumentReviewArchivePage.tsx:726](/F:/Projects/Pension_Management_System/ui/src/pages/archive/DocumentReviewArchivePage.tsx:726) | `bg-muted/50` |
| [ui/src/pages/assets/AssetCategoriesPage.tsx:77](/F:/Projects/Pension_Management_System/ui/src/pages/assets/AssetCategoriesPage.tsx:77) | `bg-primary/10` |
| [ui/src/pages/assets/AssetDetailPage.tsx:61](/F:/Projects/Pension_Management_System/ui/src/pages/assets/AssetDetailPage.tsx:61) | `bg-primary/10` |
| [ui/src/pages/assets/AssetDetailPage.tsx:103](/F:/Projects/Pension_Management_System/ui/src/pages/assets/AssetDetailPage.tsx:103) | `bg-muted/50` |
| [ui/src/pages/assets/FixedAssetsPage.tsx:73](/F:/Projects/Pension_Management_System/ui/src/pages/assets/FixedAssetsPage.tsx:73) | `bg-primary/10` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:86](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:86) | `bg-hero-end/30` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:87](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:87) | `bg-hero-accent/20` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:92](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:92) | `bg-hero-fg/10` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:102](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:102) | `bg-hero-fg/10` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:104](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:104) | `bg-hero-accent/20` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:118](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:118) | `bg-red-500/10` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:126](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:126) | `bg-hero-end/10` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:139](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:139) | `bg-hero-fg/5` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:162](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:162) | `bg-hero-fg/5` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:185](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:185) | `bg-hero-fg/5` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:202](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:202) | `hover:bg-hero-end/90` |
| [ui/src/pages/auth/LoginPage.tsx:31](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:31) | `bg-hero-fg/5` |
| [ui/src/pages/auth/LoginPage.tsx:38](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:38) | `bg-hero-fg/5` |
| [ui/src/pages/auth/LoginPage.tsx:160](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:160) | `bg-hero-end/30` |
| [ui/src/pages/auth/LoginPage.tsx:161](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:161) | `bg-hero-accent/20` |
| [ui/src/pages/auth/LoginPage.tsx:173](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:173) | `bg-hero-fg/10` |
| [ui/src/pages/auth/LoginPage.tsx:181](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:181) | `bg-red-500/10` |
| [ui/src/pages/auth/LoginPage.tsx:193](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:193) | `bg-hero-fg/5` |
| [ui/src/pages/auth/LoginPage.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:206) | `bg-hero-fg/5` |
| [ui/src/pages/auth/LoginPage.tsx:225](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:225) | `hover:bg-hero-end/90` |
| [ui/src/pages/banks/BanksListPage.tsx:81](/F:/Projects/Pension_Management_System/ui/src/pages/banks/BanksListPage.tsx:81) | `bg-primary/10` |
| [ui/src/pages/branches/BranchesListPage.tsx:74](/F:/Projects/Pension_Management_System/ui/src/pages/branches/BranchesListPage.tsx:74) | `bg-primary/10` |
| [ui/src/pages/compliance/ComplianceDashboardPage.tsx:131](/F:/Projects/Pension_Management_System/ui/src/pages/compliance/ComplianceDashboardPage.tsx:131) | `bg-emerald-500/5` |
| [ui/src/pages/compliance/ComplianceDashboardPage.tsx:132](/F:/Projects/Pension_Management_System/ui/src/pages/compliance/ComplianceDashboardPage.tsx:132) | `bg-red-500/5` |
| [ui/src/pages/compliance/ComplianceDashboardPage.tsx:194](/F:/Projects/Pension_Management_System/ui/src/pages/compliance/ComplianceDashboardPage.tsx:194) | `bg-muted/30` |
| [ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx:233](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx:233) | `bg-muted/30` |
| [ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx:261](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx:261) | `bg-muted/20` |
| [ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx:271](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx:271) | `bg-muted/30` |
| [ui/src/pages/correspondence/IncomingRegisterPage.tsx:127](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/IncomingRegisterPage.tsx:127) | `hover:bg-primary/10` |
| [ui/src/pages/correspondence/IncomingRegisterPage.tsx:148](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/IncomingRegisterPage.tsx:148) | `hover:bg-destructive/10` |
| [ui/src/pages/correspondence/IncomingRegisterPage.tsx:233](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/IncomingRegisterPage.tsx:233) | `bg-muted/50` |
| [ui/src/pages/correspondence/OutgoingRegisterPage.tsx:127](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/OutgoingRegisterPage.tsx:127) | `hover:bg-primary/10` |
| [ui/src/pages/correspondence/OutgoingRegisterPage.tsx:148](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/OutgoingRegisterPage.tsx:148) | `hover:bg-destructive/10` |
| [ui/src/pages/correspondence/OutgoingRegisterPage.tsx:233](/F:/Projects/Pension_Management_System/ui/src/pages/correspondence/OutgoingRegisterPage.tsx:233) | `bg-muted/50` |
| [ui/src/pages/DashboardPage.tsx:68](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:68) | `bg-success/10` |
| [ui/src/pages/DashboardPage.tsx:70](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:70) | `bg-info/10` |
| [ui/src/pages/DashboardPage.tsx:72](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:72) | `bg-danger/10` |
| [ui/src/pages/DashboardPage.tsx:74](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:74) | `bg-warning/10` |
| [ui/src/pages/DashboardPage.tsx:76](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:76) | `bg-special/10` |
| [ui/src/pages/DashboardPage.tsx:96](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:96) | `bg-muted/30` |
| [ui/src/pages/DashboardPage.tsx:117](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:117) | `bg-muted/30` |
| [ui/src/pages/DashboardPage.tsx:117](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:117) | `hover:bg-muted/50` |
| [ui/src/pages/DashboardPage.tsx:619](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:619) | `bg-muted/30` |
| [ui/src/pages/DashboardPage.tsx:625](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:625) | `bg-muted/30` |
| [ui/src/pages/DashboardPage.tsx:666](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:666) | `bg-muted/30` |
| [ui/src/pages/DashboardPage.tsx:672](/F:/Projects/Pension_Management_System/ui/src/pages/DashboardPage.tsx:672) | `bg-muted/30` |
| [ui/src/pages/director/DirectorOfficePage.tsx:48](/F:/Projects/Pension_Management_System/ui/src/pages/director/DirectorOfficePage.tsx:48) | `hover:bg-muted/50` |
| [ui/src/pages/documents/DocumentTypesPage.tsx:75](/F:/Projects/Pension_Management_System/ui/src/pages/documents/DocumentTypesPage.tsx:75) | `bg-primary/10` |
| [ui/src/pages/documents/MissingDocsCompletionPage.tsx:459](/F:/Projects/Pension_Management_System/ui/src/pages/documents/MissingDocsCompletionPage.tsx:459) | `bg-info/10` |
| [ui/src/pages/documents/MissingDocsCompletionPage.tsx:468](/F:/Projects/Pension_Management_System/ui/src/pages/documents/MissingDocsCompletionPage.tsx:468) | `bg-success/10` |
| [ui/src/pages/documents/MissingDocsCompletionPage.tsx:486](/F:/Projects/Pension_Management_System/ui/src/pages/documents/MissingDocsCompletionPage.tsx:486) | `bg-warning/10` |
| [ui/src/pages/finance/TreasuryPage.tsx:112](/F:/Projects/Pension_Management_System/ui/src/pages/finance/TreasuryPage.tsx:112) | `bg-success/10` |
| [ui/src/pages/finance/TreasuryPage.tsx:222](/F:/Projects/Pension_Management_System/ui/src/pages/finance/TreasuryPage.tsx:222) | `bg-muted/50` |
| [ui/src/pages/finance/TreasuryPage.tsx:233](/F:/Projects/Pension_Management_System/ui/src/pages/finance/TreasuryPage.tsx:233) | `hover:bg-muted/30` |
| [ui/src/pages/finance/TreasuryPage.tsx:250](/F:/Projects/Pension_Management_System/ui/src/pages/finance/TreasuryPage.tsx:250) | `bg-muted/50` |
| [ui/src/pages/force/CodeBatchDetailsPage.tsx:326](/F:/Projects/Pension_Management_System/ui/src/pages/force/CodeBatchDetailsPage.tsx:326) | `hover:bg-destructive/10` |
| [ui/src/pages/force/CodeBatchDetailsPage.tsx:363](/F:/Projects/Pension_Management_System/ui/src/pages/force/CodeBatchDetailsPage.tsx:363) | `bg-destructive/5` |
| [ui/src/pages/force/CodeBatchDetailsPage.tsx:364](/F:/Projects/Pension_Management_System/ui/src/pages/force/CodeBatchDetailsPage.tsx:364) | `bg-destructive/5` |
| [ui/src/pages/force/CodeBatchDetailsPage.tsx:472](/F:/Projects/Pension_Management_System/ui/src/pages/force/CodeBatchDetailsPage.tsx:472) | `bg-muted/50` |
| [ui/src/pages/force/CodeBatchDetailsPage.tsx:504](/F:/Projects/Pension_Management_System/ui/src/pages/force/CodeBatchDetailsPage.tsx:504) | `hover:bg-primary/5` |
| [ui/src/pages/hr/EmployeeDetailsPage.tsx:174](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeeDetailsPage.tsx:174) | `bg-muted/50` |
| [ui/src/pages/hr/EmployeeDetailsPage.tsx:405](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeeDetailsPage.tsx:405) | `hover:bg-muted/30` |
| [ui/src/pages/hr/EmployeesPage.tsx:126](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeesPage.tsx:126) | `bg-primary/10` |
| [ui/src/pages/hr/EmployeesPage.tsx:141](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeesPage.tsx:141) | `bg-muted/50` |
| [ui/src/pages/hr/EmployeesPage.tsx:177](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeesPage.tsx:177) | `hover:bg-primary/10` |
| [ui/src/pages/hr/EmployeesPage.tsx:286](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeesPage.tsx:286) | `bg-primary/5` |
| [ui/src/pages/hr/EmployeesPage.tsx:286](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeesPage.tsx:286) | `hover:bg-primary/10` |
| [ui/src/pages/hr/HrAttendancePage.tsx:69](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrAttendancePage.tsx:69) | `bg-primary/10` |
| [ui/src/pages/hr/HrDashboardPage.tsx:49](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:49) | `bg-primary/5` |
| [ui/src/pages/hr/HrDashboardPage.tsx:49](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:49) | `hover:bg-primary/10` |
| [ui/src/pages/hr/HrDashboardPage.tsx:92](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:92) | `bg-warning/10` |
| [ui/src/pages/hr/HrDashboardPage.tsx:107](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:107) | `bg-muted/5` |
| [ui/src/pages/hr/HrDashboardPage.tsx:115](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:115) | `bg-primary/10` |
| [ui/src/pages/hr/HrDashboardPage.tsx:139](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:139) | `bg-success/10` |
| [ui/src/pages/hr/HrDashboardPage.tsx:160](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:160) | `bg-primary/5` |
| [ui/src/pages/hr/HrDashboardPage.tsx:169](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:169) | `bg-primary/5` |
| [ui/src/pages/hr/HrDashboardPage.tsx:186](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:186) | `hover:bg-primary/[0.02]` |
| [ui/src/pages/hr/HrDashboardPage.tsx:188](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:188) | `bg-muted/50` |
| [ui/src/pages/hr/HrDashboardPage.tsx:188](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:188) | `group-hover:bg-primary/10` |
| [ui/src/pages/hr/HrDocumentsPage.tsx:46](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDocumentsPage.tsx:46) | `bg-primary/10` |
| [ui/src/pages/hr/HrDocumentsPage.tsx:70](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDocumentsPage.tsx:70) | `bg-primary/5` |
| [ui/src/pages/hr/HrDocumentsPage.tsx:110](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDocumentsPage.tsx:110) | `hover:bg-primary/10` |
| [ui/src/pages/hr/HrLeavesPage.tsx:87](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrLeavesPage.tsx:87) | `bg-primary/10` |
| [ui/src/pages/hr/HrReferenceDataPage.tsx:56](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrReferenceDataPage.tsx:56) | `bg-muted/50` |
| [ui/src/pages/hr/HrReferenceDataPage.tsx:157](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrReferenceDataPage.tsx:157) | `bg-primary/5` |
| [ui/src/pages/hr/HrReferenceDataPage.tsx:205](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrReferenceDataPage.tsx:205) | `bg-primary/5` |
| [ui/src/pages/hr/HrReferenceDataPage.tsx:253](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrReferenceDataPage.tsx:253) | `bg-primary/5` |
| [ui/src/pages/inventory/InventoryCustodyPage.tsx:73](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryCustodyPage.tsx:73) | `bg-primary/10` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:394](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:394) | `bg-muted/60` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:462](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:462) | `bg-muted/60` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:652](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:652) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:659](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:659) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:666](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:666) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:673](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:673) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:708](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:708) | `bg-primary/10` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:717](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:717) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:721](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:721) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:734](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:734) | `bg-primary/10` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:743](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:743) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:747](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:747) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryDashboardPage.tsx:751](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryDashboardPage.tsx:751) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/InventoryItemsPage.tsx:60](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryItemsPage.tsx:60) | `bg-primary/10` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:80](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:80) | `bg-muted/40` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:80](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:80) | `hover:bg-muted/70` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:123](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:123) | `bg-muted/20` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:132](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:132) | `bg-muted/40` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:144](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:144) | `bg-emerald-500/5` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:146](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:146) | `bg-red-500/5` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:147](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:147) | `bg-blue-500/5` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:164](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:164) | `bg-primary/5` |
| [ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:226](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionDetailsPage.tsx:226) | `bg-muted/20` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:106](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:106) | `hover:bg-muted/30` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:142](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:142) | `bg-muted/10` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:142](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:142) | `hover:bg-muted/30` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:153](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:153) | `bg-muted/5` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:192](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:192) | `bg-muted/20` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:217](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:217) | `bg-destructive/10` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:219](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:219) | `bg-muted/10` |
| [ui/src/pages/inventory/InventoryTransactionsPage.tsx:226](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/InventoryTransactionsPage.tsx:226) | `bg-muted/40` |
| [ui/src/pages/inventory/StocktakePage.tsx:462](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/StocktakePage.tsx:462) | `bg-muted/50` |
| [ui/src/pages/inventory/StocktakePage.tsx:462](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/StocktakePage.tsx:462) | `hover:bg-muted/50` |
| [ui/src/pages/inventory/StocktakePage.tsx:482](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/StocktakePage.tsx:482) | `bg-warning/10` |
| [ui/src/pages/inventory/StocktakePage.tsx:482](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/StocktakePage.tsx:482) | `hover:bg-muted/30` |
| [ui/src/pages/inventory/StocktakePage.tsx:483](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/StocktakePage.tsx:483) | `bg-danger/10` |
| [ui/src/pages/inventory/StocktakePage.tsx:487](/F:/Projects/Pension_Management_System/ui/src/pages/inventory/StocktakePage.tsx:487) | `bg-muted/60` |
| [ui/src/pages/investment/InvestmentHub.tsx:125](/F:/Projects/Pension_Management_System/ui/src/pages/investment/InvestmentHub.tsx:125) | `hover:bg-primary/[0.02]` |
| [ui/src/pages/investment/InvestmentHub.tsx:128](/F:/Projects/Pension_Management_System/ui/src/pages/investment/InvestmentHub.tsx:128) | `bg-primary/10` |
| [ui/src/pages/investment/InvestmentHub.tsx:159](/F:/Projects/Pension_Management_System/ui/src/pages/investment/InvestmentHub.tsx:159) | `bg-muted/20` |
| [ui/src/pages/investment/LeasingPage.tsx:20](/F:/Projects/Pension_Management_System/ui/src/pages/investment/LeasingPage.tsx:20) | `bg-muted/50` |
| [ui/src/pages/investment/MaintenanceDetailsPage.tsx:37](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenanceDetailsPage.tsx:37) | `bg-danger/5` |
| [ui/src/pages/investment/MaintenanceDetailsPage.tsx:49](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenanceDetailsPage.tsx:49) | `bg-danger/5` |
| [ui/src/pages/investment/MaintenanceDetailsPage.tsx:143](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenanceDetailsPage.tsx:143) | `bg-muted/50` |
| [ui/src/pages/investment/MaintenancePage.tsx:36](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenancePage.tsx:36) | `bg-success/10` |
| [ui/src/pages/investment/MaintenancePage.tsx:40](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenancePage.tsx:40) | `bg-info/10` |
| [ui/src/pages/investment/MaintenancePage.tsx:44](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenancePage.tsx:44) | `bg-danger/10` |
| [ui/src/pages/investment/MaintenancePage.tsx:110](/F:/Projects/Pension_Management_System/ui/src/pages/investment/MaintenancePage.tsx:110) | `hover:bg-success/10` |
| [ui/src/pages/investment/PortfolioDetailPage.tsx:94](/F:/Projects/Pension_Management_System/ui/src/pages/investment/PortfolioDetailPage.tsx:94) | `bg-muted/50` |
| [ui/src/pages/investment/ProjectDetailPage.tsx:113](/F:/Projects/Pension_Management_System/ui/src/pages/investment/ProjectDetailPage.tsx:113) | `bg-muted/50` |
| [ui/src/pages/investment/ProjectsPage.tsx:86](/F:/Projects/Pension_Management_System/ui/src/pages/investment/ProjectsPage.tsx:86) | `hover:bg-primary/10` |
| [ui/src/pages/investment/PropertyDetailPage.tsx:92](/F:/Projects/Pension_Management_System/ui/src/pages/investment/PropertyDetailPage.tsx:92) | `bg-muted/50` |
| [ui/src/pages/investment/tabs/ApprovalRequestsTab.tsx:93](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/ApprovalRequestsTab.tsx:93) | `hover:bg-success/10` |
| [ui/src/pages/investment/tabs/ApprovalRequestsTab.tsx:102](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/ApprovalRequestsTab.tsx:102) | `hover:bg-danger/10` |
| [ui/src/pages/investment/tabs/PaymentSchedulesTab.tsx:87](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PaymentSchedulesTab.tsx:87) | `hover:bg-warning/10` |
| [ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:73](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:73) | `bg-primary/10` |
| [ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:93](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:93) | `bg-muted/50` |
| [ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:112](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:112) | `bg-muted/30` |
| [ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:122](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:122) | `bg-muted/10` |
| [ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:136](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyActivityLogTab.tsx:136) | `bg-info/10` |
| [ui/src/pages/investment/tabs/PropertyDepreciationTab.tsx:101](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyDepreciationTab.tsx:101) | `hover:bg-warning/10` |
| [ui/src/pages/investment/tabs/PropertyDepreciationTab.tsx:110](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyDepreciationTab.tsx:110) | `hover:bg-danger/10` |
| [ui/src/pages/investment/tabs/PropertyMaintenanceTab.tsx:125](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyMaintenanceTab.tsx:125) | `hover:bg-warning/10` |
| [ui/src/pages/investment/tabs/PropertyMaintenanceTab.tsx:138](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/PropertyMaintenanceTab.tsx:138) | `hover:bg-success/10` |
| [ui/src/pages/investment/tabs/TransactionsTab.tsx:40](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/TransactionsTab.tsx:40) | `bg-danger/10` |
| [ui/src/pages/investment/tabs/TransactionsTab.tsx:40](/F:/Projects/Pension_Management_System/ui/src/pages/investment/tabs/TransactionsTab.tsx:40) | `bg-success/10` |
| [ui/src/pages/investment/TenantDetailPage.tsx:111](/F:/Projects/Pension_Management_System/ui/src/pages/investment/TenantDetailPage.tsx:111) | `bg-muted/50` |
| [ui/src/pages/ministry-approvals/ForceAnalyticsDashboard.tsx:33](/F:/Projects/Pension_Management_System/ui/src/pages/ministry-approvals/ForceAnalyticsDashboard.tsx:33) | `bg-destructive/10` |
| [ui/src/pages/ministry-approvals/ForceAuditTab.tsx:82](/F:/Projects/Pension_Management_System/ui/src/pages/ministry-approvals/ForceAuditTab.tsx:82) | `bg-primary/10` |
| [ui/src/pages/ministry-approvals/ForceAuditTab.tsx:114](/F:/Projects/Pension_Management_System/ui/src/pages/ministry-approvals/ForceAuditTab.tsx:114) | `bg-blue-50/50` |
| [ui/src/pages/ministry-approvals/ReconciliationTab.tsx:99](/F:/Projects/Pension_Management_System/ui/src/pages/ministry-approvals/ReconciliationTab.tsx:99) | `bg-warning/10` |
| [ui/src/pages/notifications/NotificationsPage.tsx:125](/F:/Projects/Pension_Management_System/ui/src/pages/notifications/NotificationsPage.tsx:125) | `hover:bg-muted/40` |
| [ui/src/pages/notifications/NotificationsPage.tsx:126](/F:/Projects/Pension_Management_System/ui/src/pages/notifications/NotificationsPage.tsx:126) | `bg-primary/[0.04]` |
| [ui/src/pages/notifications/NotificationsPage.tsx:167](/F:/Projects/Pension_Management_System/ui/src/pages/notifications/NotificationsPage.tsx:167) | `bg-muted-foreground/30` |
| [ui/src/pages/notifications/NotificationsPage.tsx:182](/F:/Projects/Pension_Management_System/ui/src/pages/notifications/NotificationsPage.tsx:182) | `bg-muted/50` |
| [ui/src/pages/pension/AgentsPage.tsx:41](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AgentsPage.tsx:41) | `bg-primary/10` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:314](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:314) | `bg-muted/40` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:316](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:316) | `bg-primary/10` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:367](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:367) | `bg-amber-500/10` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:383](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:383) | `bg-muted/20` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:395](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:395) | `bg-black/40` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:399](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:399) | `bg-background/90` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:432](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:432) | `bg-muted/20` |
| [ui/src/pages/pension/EntitledPersonsPage.tsx:235](/F:/Projects/Pension_Management_System/ui/src/pages/pension/EntitledPersonsPage.tsx:235) | `bg-primary/10` |
| [ui/src/pages/pension/EntitledPersonsPage.tsx:462](/F:/Projects/Pension_Management_System/ui/src/pages/pension/EntitledPersonsPage.tsx:462) | `bg-muted/30` |
| [ui/src/pages/pension/FileReviewListPage.tsx:430](/F:/Projects/Pension_Management_System/ui/src/pages/pension/FileReviewListPage.tsx:430) | `hover:bg-info/15` |
| [ui/src/pages/pension/FileReviewListPage.tsx:441](/F:/Projects/Pension_Management_System/ui/src/pages/pension/FileReviewListPage.tsx:441) | `hover:bg-success/15` |
| [ui/src/pages/pension/FileReviewListPage.tsx:451](/F:/Projects/Pension_Management_System/ui/src/pages/pension/FileReviewListPage.tsx:451) | `hover:bg-danger/15` |
| [ui/src/pages/pension/FileReviewListPage.tsx:565](/F:/Projects/Pension_Management_System/ui/src/pages/pension/FileReviewListPage.tsx:565) | `bg-muted/30` |
| [ui/src/pages/pension/FinancialEntitlementsPage.tsx:207](/F:/Projects/Pension_Management_System/ui/src/pages/pension/FinancialEntitlementsPage.tsx:207) | `bg-primary/10` |
| [ui/src/pages/pension/GuardiansPage.tsx:70](/F:/Projects/Pension_Management_System/ui/src/pages/pension/GuardiansPage.tsx:70) | `bg-primary/10` |
| [ui/src/pages/pension/GuardiansPage.tsx:225](/F:/Projects/Pension_Management_System/ui/src/pages/pension/GuardiansPage.tsx:225) | `bg-primary/10` |
| [ui/src/pages/pension/KashfReportsPage.tsx:197](/F:/Projects/Pension_Management_System/ui/src/pages/pension/KashfReportsPage.tsx:197) | `bg-primary/10` |
| [ui/src/pages/pension/KashfReportsPage.tsx:417](/F:/Projects/Pension_Management_System/ui/src/pages/pension/KashfReportsPage.tsx:417) | `bg-primary/5` |
| [ui/src/pages/pension/KashfReportsPage.tsx:418](/F:/Projects/Pension_Management_System/ui/src/pages/pension/KashfReportsPage.tsx:418) | `bg-emerald-500/5` |
| [ui/src/pages/pension/KashfReportsPage.tsx:419](/F:/Projects/Pension_Management_System/ui/src/pages/pension/KashfReportsPage.tsx:419) | `bg-amber-500/5` |
| [ui/src/pages/pension/KashfReportsPage.tsx:420](/F:/Projects/Pension_Management_System/ui/src/pages/pension/KashfReportsPage.tsx:420) | `bg-blue-500/5` |
| [ui/src/pages/pension/KashfReportsPage.tsx:421](/F:/Projects/Pension_Management_System/ui/src/pages/pension/KashfReportsPage.tsx:421) | `bg-rose-500/5` |
| [ui/src/pages/pension/MinistryBatchesPage.tsx:80](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryBatchesPage.tsx:80) | `bg-primary/10` |
| [ui/src/pages/pension/MinistryBatchesPage.tsx:125](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryBatchesPage.tsx:125) | `hover:bg-destructive/10` |
| [ui/src/pages/pension/MinistryReferralsPage.tsx:556](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryReferralsPage.tsx:556) | `dark:hover:bg-destructive/20` |
| [ui/src/pages/pension/MinistryReferralsPage.tsx:556](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryReferralsPage.tsx:556) | `hover:bg-destructive/10` |
| [ui/src/pages/pension/MinistryReferralsPage.tsx:581](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryReferralsPage.tsx:581) | `dark:hover:bg-emerald-950/50` |
| [ui/src/pages/pension/MinistryReferralsPage.tsx:641](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryReferralsPage.tsx:641) | `dark:hover:bg-emerald-950/50` |
| [ui/src/pages/pension/MinistryReferralsPage.tsx:653](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryReferralsPage.tsx:653) | `bg-card/50` |
| [ui/src/pages/pension/MinistryReferralsPage.tsx:665](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistryReferralsPage.tsx:665) | `bg-primary/15` |
| [ui/src/pages/pension/MinistrySearchPage.tsx:153](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistrySearchPage.tsx:153) | `dark:hover:bg-emerald-950/50` |
| [ui/src/pages/pension/MinistrySearchPage.tsx:175](/F:/Projects/Pension_Management_System/ui/src/pages/pension/MinistrySearchPage.tsx:175) | `hover:bg-primary/10` |
| [ui/src/pages/pension/PensionSuspensionsPage.tsx:315](/F:/Projects/Pension_Management_System/ui/src/pages/pension/PensionSuspensionsPage.tsx:315) | `bg-primary/5` |
| [ui/src/pages/pension/PensionSuspensionsPage.tsx:316](/F:/Projects/Pension_Management_System/ui/src/pages/pension/PensionSuspensionsPage.tsx:316) | `bg-primary/10` |
| [ui/src/pages/pension/PensionSuspensionsPage.tsx:348](/F:/Projects/Pension_Management_System/ui/src/pages/pension/PensionSuspensionsPage.tsx:348) | `hover:bg-muted/40` |
| [ui/src/pages/pension/SalaryRecipientsPage.tsx:64](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryRecipientsPage.tsx:64) | `bg-primary/10` |
| [ui/src/pages/pension/SalaryRecipientsPage.tsx:227](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryRecipientsPage.tsx:227) | `bg-primary/10` |
| [ui/src/pages/pension/SalaryReconciliationPage.tsx:149](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryReconciliationPage.tsx:149) | `bg-muted/30` |
| [ui/src/pages/pension/SalaryReconciliationPage.tsx:153](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryReconciliationPage.tsx:153) | `bg-muted/30` |
| [ui/src/pages/pension/SalaryReconciliationPage.tsx:157](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryReconciliationPage.tsx:157) | `bg-muted/30` |
| [ui/src/pages/pension/SalaryReconciliationPage.tsx:161](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryReconciliationPage.tsx:161) | `bg-muted/30` |
| [ui/src/pages/pension/SalaryReconciliationPage.tsx:181](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SalaryReconciliationPage.tsx:181) | `hover:bg-muted/30` |
| [ui/src/pages/pension/SocialResearchPage.tsx:231](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SocialResearchPage.tsx:231) | `hover:bg-blue-500/15` |
| [ui/src/pages/pension/SocialResearchPage.tsx:241](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SocialResearchPage.tsx:241) | `hover:bg-emerald-500/15` |
| [ui/src/pages/pension/SocialResearchPage.tsx:251](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SocialResearchPage.tsx:251) | `hover:bg-amber-500/15` |
| [ui/src/pages/pension/SocialResearchPage.tsx:262](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SocialResearchPage.tsx:262) | `hover:bg-indigo-500/15` |
| [ui/src/pages/pension/SocialResearchPage.tsx:361](/F:/Projects/Pension_Management_System/ui/src/pages/pension/SocialResearchPage.tsx:361) | `bg-muted/30` |
| [ui/src/pages/print/AccountingReportPrintPage.tsx:772](/F:/Projects/Pension_Management_System/ui/src/pages/print/AccountingReportPrintPage.tsx:772) | `bg-white/95` |
| [ui/src/pages/print/EmployeePrintPage.tsx:33](/F:/Projects/Pension_Management_System/ui/src/pages/print/EmployeePrintPage.tsx:33) | `bg-slate-200/60` |
| [ui/src/pages/print/PaymentBookPrint.tsx:357](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:357) | `bg-slate-200/50` |
| [ui/src/pages/print/PaymentBookPrint.tsx:368](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:368) | `bg-primary/10` |
| [ui/src/pages/print/PaymentBookPrint.tsx:514](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:514) | `bg-white/60` |
| [ui/src/pages/print/PrintableEmployeeReport.tsx:50](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableEmployeeReport.tsx:50) | `bg-muted/50` |
| [ui/src/pages/print/PrintableRetirementFileReport.tsx:98](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableRetirementFileReport.tsx:98) | `bg-muted/50` |
| [ui/src/pages/print/PrintableRetirementFileReport.tsx:106](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableRetirementFileReport.tsx:106) | `bg-muted/50` |
| [ui/src/pages/print/PrintableRetirementFileReport.tsx:164](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableRetirementFileReport.tsx:164) | `bg-muted/50` |
| [ui/src/pages/print/PrintableRetirementFileReport.tsx:199](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableRetirementFileReport.tsx:199) | `bg-muted/50` |
| [ui/src/pages/print/PrintableRetirementFileReport.tsx:230](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableRetirementFileReport.tsx:230) | `bg-muted/50` |
| [ui/src/pages/print/PrintableRetirementFileReport.tsx:307](/F:/Projects/Pension_Management_System/ui/src/pages/print/PrintableRetirementFileReport.tsx:307) | `bg-muted/50` |
| [ui/src/pages/print/SettlementDecisionPrint.tsx:106](/F:/Projects/Pension_Management_System/ui/src/pages/print/SettlementDecisionPrint.tsx:106) | `bg-muted/50` |
| [ui/src/pages/print/SettlementDecisionPrint.tsx:123](/F:/Projects/Pension_Management_System/ui/src/pages/print/SettlementDecisionPrint.tsx:123) | `bg-muted/50` |
| [ui/src/pages/ranks/RanksListPage.tsx:65](/F:/Projects/Pension_Management_System/ui/src/pages/ranks/RanksListPage.tsx:65) | `bg-primary/10` |
| [ui/src/pages/reports/ReportsPage.tsx:27](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportsPage.tsx:27) | `bg-primary/10` |
| [ui/src/pages/reports/ReportsPage.tsx:45](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportsPage.tsx:45) | `bg-border/60` |
| [ui/src/pages/reports/ReportViewerPage.tsx:235](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportViewerPage.tsx:235) | `bg-primary/10` |
| [ui/src/pages/reports/ReportViewerPage.tsx:260](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportViewerPage.tsx:260) | `bg-primary/10` |
| [ui/src/pages/reports/ReportViewerPage.tsx:384](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportViewerPage.tsx:384) | `bg-primary/5` |
| [ui/src/pages/reports/ReportViewerPage.tsx:410](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportViewerPage.tsx:410) | `bg-muted/30` |
| [ui/src/pages/reports/ReportViewerPage.tsx:410](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportViewerPage.tsx:410) | `hover:bg-muted/50` |
| [ui/src/pages/reports/ReportViewerPage.tsx:413](/F:/Projects/Pension_Management_System/ui/src/pages/reports/ReportViewerPage.tsx:413) | `bg-primary/10` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:153](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:153) | `bg-primary/5` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:154](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:154) | `bg-primary/5` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:223](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:223) | `hover:bg-primary/5` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:234](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:234) | `hover:bg-primary/90` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:283](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:283) | `hover:bg-primary/90` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:300](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:300) | `bg-muted/30` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:315](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:315) | `hover:bg-muted/20` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:342](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:342) | `bg-muted/30` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:353](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:353) | `hover:bg-muted/20` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:407](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:407) | `bg-muted/30` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:420](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:420) | `hover:bg-muted/20` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:441](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:441) | `bg-muted/30` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:452](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:452) | `hover:bg-muted/20` |
| [ui/src/pages/retirees/RetireeDetailsPage.tsx:489](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireeDetailsPage.tsx:489) | `bg-muted/5` |
| [ui/src/pages/retirees/RetireesPage.tsx:116](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:116) | `bg-primary/10` |
| [ui/src/pages/retirees/RetireesPage.tsx:176](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:176) | `dark:hover:bg-destructive/20` |
| [ui/src/pages/retirees/RetireesPage.tsx:176](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:176) | `hover:bg-destructive/10` |
| [ui/src/pages/retirees/RetireesPage.tsx:199](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:199) | `dark:hover:bg-blue-950/50` |
| [ui/src/pages/retirees/RetireesPage.tsx:215](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:215) | `dark:hover:bg-emerald-950/50` |
| [ui/src/pages/retirees/RetireesPage.tsx:228](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:228) | `dark:hover:bg-violet-950/50` |
| [ui/src/pages/retirees/RetireesPage.tsx:242](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:242) | `dark:hover:bg-amber-950/50` |
| [ui/src/pages/retirees/RetireesPage.tsx:254](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:254) | `dark:hover:bg-violet-950/50` |
| [ui/src/pages/retirees/RetireesPage.tsx:315](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:315) | `bg-primary/10` |
| [ui/src/pages/retirees/RetireesPage.tsx:355](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:355) | `bg-card/50` |
| [ui/src/pages/retirees/RetireesPage.tsx:367](/F:/Projects/Pension_Management_System/ui/src/pages/retirees/RetireesPage.tsx:367) | `bg-primary/15` |
| [ui/src/pages/retirement-files/DocumentProcessPage.tsx:704](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:704) | `bg-muted/20` |
| [ui/src/pages/retirement-files/DocumentProcessPage.tsx:724](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/DocumentProcessPage.tsx:724) | `bg-background/50` |
| [ui/src/pages/retirement-files/FileWorkflowManager.tsx:277](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/FileWorkflowManager.tsx:277) | `hover:bg-muted/40` |
| [ui/src/pages/retirement-files/FileWorkflowManager.tsx:282](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/FileWorkflowManager.tsx:282) | `bg-primary/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:248](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:248) | `bg-emerald-500/5` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:286](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:286) | `bg-amber-50/50` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:286](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:286) | `bg-destructive/5` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:286](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:286) | `dark:bg-amber-950/20` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:292](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:292) | `hover:bg-amber-500/5` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:292](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:292) | `hover:bg-destructive/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:312](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:312) | `bg-destructive/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:353](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:353) | `bg-amber-900/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:353](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:353) | `dark:bg-amber-100/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:358](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:358) | `bg-amber-900/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:358](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:358) | `dark:bg-amber-100/10` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:489](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:489) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:515](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:515) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/RetirementFileReportsPage.tsx:288](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileReportsPage.tsx:288) | `hover:bg-muted/40` |
| [ui/src/pages/retirement-files/RetirementFileReportsPage.tsx:291](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileReportsPage.tsx:291) | `bg-primary/10` |
| [ui/src/pages/retirement-files/RetirementFileReportsPage.tsx:308](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileReportsPage.tsx:308) | `bg-muted/10` |
| [ui/src/pages/retirement-files/RetirementFilesPage.tsx:299](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFilesPage.tsx:299) | `bg-primary/10` |
| [ui/src/pages/retirement-files/RetirementFilesPage.tsx:370](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFilesPage.tsx:370) | `bg-card/50` |
| [ui/src/pages/retirement-files/RetirementFilesPage.tsx:382](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFilesPage.tsx:382) | `bg-primary/15` |
| [ui/src/pages/retirement-files/tabs/AgentsTab.tsx:140](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/AgentsTab.tsx:140) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/AgentsTab.tsx:272](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/AgentsTab.tsx:272) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:354](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:354) | `bg-primary/10` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:477](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:477) | `hover:bg-destructive/10` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:577](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:577) | `bg-destructive/[0.03]` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:579](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:579) | `bg-destructive/10` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:594](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:594) | `hover:bg-destructive/[0.02]` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:597](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:597) | `bg-destructive/10` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:611](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:611) | `bg-destructive/[0.05]` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:629](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:629) | `hover:bg-primary/5` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:651](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:651) | `hover:bg-emerald-500/10` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:663](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:663) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:689](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:689) | `bg-primary/10` |
| [ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:725](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/DocumentsTab.tsx:725) | `bg-muted/50` |
| [ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx:150](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx:150) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx:261](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx:261) | `hover:bg-destructive/10` |
| [ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx:346](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx:346) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/FinancialEntitlementsTab.tsx:94](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/FinancialEntitlementsTab.tsx:94) | `hover:bg-destructive/10` |
| [ui/src/pages/retirement-files/tabs/FinancialEntitlementsTab.tsx:158](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/FinancialEntitlementsTab.tsx:158) | `hover:bg-destructive/10` |
| [ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:170](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:170) | `bg-muted/20` |
| [ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:206) | `bg-muted/40` |
| [ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:229](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:229) | `hover:bg-primary/5` |
| [ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:234](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/GuardiansTab.tsx:234) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/NotesTab.tsx:120](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/NotesTab.tsx:120) | `bg-muted-foreground/30` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:201](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:201) | `bg-green-500/5` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:201](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:201) | `dark:bg-green-900/30` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:201](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:201) | `dark:bg-green-950/20` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:204](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:204) | `dark:bg-muted/20` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:205](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:205) | `bg-green-500/5` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:205](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:205) | `dark:bg-green-900/30` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:205](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:205) | `dark:bg-green-950/20` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:206) | `bg-blue-500/5` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:206) | `dark:bg-blue-900/30` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:206) | `dark:bg-blue-950/20` |
| [ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:207](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PaymentBooksTab.tsx:207) | `dark:bg-muted/20` |
| [ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:317](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:317) | `hover:bg-primary/5` |
| [ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:331](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:331) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:356](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:356) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:384](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:384) | `bg-background/80` |
| [ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:395](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:395) | `bg-primary/20` |
| [ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:407](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx:407) | `bg-primary/20` |
| [ui/src/pages/retirement-files/tabs/RecipientsTab.tsx:192](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/RecipientsTab.tsx:192) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/RecipientsTab.tsx:264](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/RecipientsTab.tsx:264) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:201](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:201) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:245](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:245) | `bg-danger/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:245](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:245) | `hover:bg-danger/10` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:294](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:294) | `bg-warning/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:294](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:294) | `hover:bg-warning/10` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:298](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:298) | `hover:bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:312](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:312) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:320](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:320) | `hover:bg-destructive/90` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:333](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:333) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:346](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:346) | `bg-border/40` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:356](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:356) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:368](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:368) | `bg-muted/30` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:376](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:376) | `hover:bg-muted/20` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:378](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:378) | `bg-primary/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:429](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:429) | `bg-danger/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:429](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:429) | `bg-success/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:429](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:429) | `bg-warning/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:433](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:433) | `hover:bg-muted/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:435](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:435) | `bg-danger/10` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:435](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:435) | `bg-success/10` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:435](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:435) | `bg-warning/10` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:455](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:455) | `bg-muted/20` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:459](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:459) | `bg-danger/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:460](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:460) | `bg-success/5` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:472](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:472) | `hover:bg-success/10` |
| [ui/src/pages/retirement-files/tabs/ReviewTab.tsx:481](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/ReviewTab.tsx:481) | `hover:bg-danger/10` |
| [ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:162](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:162) | `bg-destructive/5` |
| [ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:175](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:175) | `bg-muted/10` |
| [ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:216](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:216) | `bg-muted/50` |
| [ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:235](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:235) | `bg-primary/10` |
| [ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:246](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/StatusTransitionsTab.tsx:246) | `bg-muted/50` |
| [ui/src/pages/retirement-files/tabs/SuspensionsTab.tsx:154](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/SuspensionsTab.tsx:154) | `bg-rose-500/5` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:100](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:100) | `bg-[hsl(var(--gov-success)/.1)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:101](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:101) | `bg-[hsl(var(--gov-danger)/.1)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:106](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:106) | `bg-[hsl(var(--gov-success)/.02)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:107](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:107) | `bg-[hsl(var(--gov-danger)/.02)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:108](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:108) | `bg-primary/[0.02]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:220](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:220) | `bg-muted/5` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:223](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:223) | `bg-muted/30` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:233](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:233) | `hover:bg-background/50` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:235](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:235) | `bg-muted/10` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:262](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:262) | `bg-[hsl(var(--gov-info)/.08)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:285](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:285) | `hover:bg-[hsl(var(--gov-success)/.08)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:296](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:296) | `hover:bg-[hsl(var(--gov-danger)/.08)]` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:483](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:483) | `bg-primary/10` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:553](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:553) | `via-background/95` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:554](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:554) | `bg-background/95` |
| [ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:256](/F:/Projects/Pension_Management_System/ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:256) | `bg-emerald-500/5` |
| [ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:256](/F:/Projects/Pension_Management_System/ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:256) | `dark:bg-emerald-950/20` |
| [ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:274](/F:/Projects/Pension_Management_System/ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:274) | `bg-blue-500/5` |
| [ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:274](/F:/Projects/Pension_Management_System/ui/src/pages/salaries/SalaryBatchDetailsPage.tsx:274) | `dark:bg-blue-950/20` |
| [ui/src/pages/settlements/SettlementBatchDetailsPage.tsx:411](/F:/Projects/Pension_Management_System/ui/src/pages/settlements/SettlementBatchDetailsPage.tsx:411) | `hover:bg-warning/10` |
| [ui/src/pages/suspensions/SuspensionsPage.tsx:147](/F:/Projects/Pension_Management_System/ui/src/pages/suspensions/SuspensionsPage.tsx:147) | `bg-destructive/10` |
| [ui/src/pages/system-admin/UsersDirectoryPage.tsx:187](/F:/Projects/Pension_Management_System/ui/src/pages/system-admin/UsersDirectoryPage.tsx:187) | `from-primary/20` |
| [ui/src/pages/system-admin/UsersDirectoryPage.tsx:187](/F:/Projects/Pension_Management_System/ui/src/pages/system-admin/UsersDirectoryPage.tsx:187) | `to-primary/5` |
| [ui/src/pages/system-admin/UsersDirectoryPage.tsx:317](/F:/Projects/Pension_Management_System/ui/src/pages/system-admin/UsersDirectoryPage.tsx:317) | `bg-muted/30` |

### شفافية حلقة التركيز والظلال — 40 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/common/SearchDialog.tsx:136](/F:/Projects/Pension_Management_System/ui/src/components/common/SearchDialog.tsx:136) | `focus-visible:ring-primary/20` |
| [ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:142](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:142) | `ring-primary/20` |
| [ui/src/components/domain/retirement/DataEntryView.tsx:198](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DataEntryView.tsx:198) | `focus:ring-primary/20` |
| [ui/src/components/domain/retirement/PaymentBookDialog.tsx:360](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PaymentBookDialog.tsx:360) | `focus:ring-destructive/20` |
| [ui/src/components/domain/retirement/PledgeDialog.tsx:289](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PledgeDialog.tsx:289) | `focus:ring-primary/20` |
| [ui/src/components/domain/retirement/sections/HistorySection.tsx:83](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/HistorySection.tsx:83) | `group-hover:ring-primary/20` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:371](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:371) | `ring-primary/10` |
| [ui/src/components/ui/buttonVariants.ts:9](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:9) | `hover:shadow-primary/30` |
| [ui/src/components/ui/buttonVariants.ts:9](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:9) | `shadow-primary/20` |
| [ui/src/components/ui/buttonVariants.ts:17](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:17) | `hover:shadow-emerald-500/30` |
| [ui/src/components/ui/buttonVariants.ts:17](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:17) | `shadow-emerald-500/20` |
| [ui/src/components/ui/buttonVariants.ts:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:19) | `hover:shadow-amber-500/30` |
| [ui/src/components/ui/buttonVariants.ts:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:19) | `shadow-amber-500/20` |
| [ui/src/components/ui/buttonVariants.ts:21](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:21) | `hover:shadow-blue-500/30` |
| [ui/src/components/ui/buttonVariants.ts:21](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:21) | `shadow-blue-500/20` |
| [ui/src/components/ui/Checkbox.tsx:23](/F:/Projects/Pension_Management_System/ui/src/components/ui/Checkbox.tsx:23) | `focus:ring-primary/30` |
| [ui/src/components/ui/DataTable.tsx:362](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:362) | `ring-primary/30` |
| [ui/src/components/ui/DataTable.tsx:503](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:503) | `ring-primary/30` |
| [ui/src/components/ui/DataTable.tsx:663](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:663) | `focus:ring-primary/30` |
| [ui/src/components/ui/FileField.tsx:41](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileField.tsx:41) | `focus-within:ring-primary/50` |
| [ui/src/components/ui/Input.tsx:101](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:101) | `focus:ring-ring/20` |
| [ui/src/components/ui/Input.tsx:107](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:107) | `focus:ring-destructive/20` |
| [ui/src/components/ui/Input.tsx:412](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:412) | `focus:ring-ring/20` |
| [ui/src/components/ui/Input.tsx:415](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:415) | `focus:ring-destructive/20` |
| [ui/src/components/ui/Input.tsx:475](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:475) | `focus:ring-ring/20` |
| [ui/src/components/ui/Input.tsx:477](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:477) | `focus:ring-destructive/20` |
| [ui/src/components/ui/NativeSelect.tsx:77](/F:/Projects/Pension_Management_System/ui/src/components/ui/NativeSelect.tsx:77) | `focus:ring-primary/20` |
| [ui/src/components/ui/NativeSelect.tsx:79](/F:/Projects/Pension_Management_System/ui/src/components/ui/NativeSelect.tsx:79) | `focus:ring-destructive/20` |
| [ui/src/components/ui/Stepper.tsx:67](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:67) | `shadow-emerald-500/20` |
| [ui/src/components/ui/Stepper.tsx:68](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:68) | `ring-primary/10` |
| [ui/src/components/ui/Stepper.tsx:68](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:68) | `shadow-primary/25` |
| [ui/src/components/ui/Stepper.tsx:161](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:161) | `shadow-emerald-500/20` |
| [ui/src/components/ui/Stepper.tsx:162](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:162) | `ring-primary/10` |
| [ui/src/components/ui/Stepper.tsx:162](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:162) | `shadow-primary/20` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:139](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:139) | `focus:ring-hero-accent/50` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:162](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:162) | `focus:ring-hero-accent/50` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:185](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:185) | `focus:ring-hero-accent/50` |
| [ui/src/pages/auth/LoginPage.tsx:193](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:193) | `focus:ring-hero-accent/50` |
| [ui/src/pages/auth/LoginPage.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:206) | `focus:ring-hero-accent/50` |
| [ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:348](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx:348) | `focus-visible:ring-primary/20` |

### إخفاء تام أو لون شفاف صريح — 64 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/common/ReviewFilePreview.tsx:62](/F:/Projects/Pension_Management_System/ui/src/components/common/ReviewFilePreview.tsx:62) | `opacity-0` |
| [ui/src/components/domain/accounting/VoucherImportDialog.tsx:135](/F:/Projects/Pension_Management_System/ui/src/components/domain/accounting/VoucherImportDialog.tsx:135) | `border-t-transparent` |
| [ui/src/components/domain/admin/ExcelImportDialog.tsx:126](/F:/Projects/Pension_Management_System/ui/src/components/domain/admin/ExcelImportDialog.tsx:126) | `opacity-0` |
| [ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:143](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx:143) | `border-transparent` |
| [ui/src/components/domain/retirement/DocumentChecklistField.tsx:60](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentChecklistField.tsx:60) | `border-transparent` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:151](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:151) | `opacity-0` |
| [ui/src/components/domain/retirement/DocumentViewerDialog.tsx:162](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/DocumentViewerDialog.tsx:162) | `opacity-0` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:46](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:46) | `from-transparent` |
| [ui/src/components/domain/retirement/FileReviewSidebar.tsx:46](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/FileReviewSidebar.tsx:46) | `to-transparent` |
| [ui/src/components/domain/retirement/GuardianDialog.tsx:253](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/GuardianDialog.tsx:253) | `border-transparent` |
| [ui/src/components/domain/retirement/PaymentBookDialog.tsx:567](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/PaymentBookDialog.tsx:567) | `opacity-0` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:72](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:72) | `from-transparent` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:72](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:72) | `to-transparent` |
| [ui/src/components/domain/retirement/RetirementFileSidebar.tsx:180](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/RetirementFileSidebar.tsx:180) | `opacity-0` |
| [ui/src/components/domain/retirement/sections/FinancialSection.tsx:112](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/FinancialSection.tsx:112) | `opacity-0` |
| [ui/src/components/domain/retirement/sections/PeopleSection.tsx:382](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/sections/PeopleSection.tsx:382) | `transparent` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:247](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:247) | `border-transparent` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:275](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:275) | `opacity-0` |
| [ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:319](/F:/Projects/Pension_Management_System/ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx:319) | `border-transparent` |
| [ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:207](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:207) | `opacity-0` |
| [ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:237](/F:/Projects/Pension_Management_System/ui/src/components/domain/settlements/CreateSettlementDecisionDialog.tsx:237) | `opacity-0` |
| [ui/src/components/layout/MainLayout.tsx:90](/F:/Projects/Pension_Management_System/ui/src/components/layout/MainLayout.tsx:90) | `opacity-0` |
| [ui/src/components/layout/NotificationBell.tsx:102](/F:/Projects/Pension_Management_System/ui/src/components/layout/NotificationBell.tsx:102) | `opacity-0` |
| [ui/src/components/layout/Sidebar.tsx:310](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:310) | `transparent` |
| [ui/src/components/notifications/FloatingNotification.tsx:63](/F:/Projects/Pension_Management_System/ui/src/components/notifications/FloatingNotification.tsx:63) | `opacity-0` |
| [ui/src/components/ui/AuditTimeline.tsx:142](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:142) | `to-transparent` |
| [ui/src/components/ui/badgeVariants.ts:23](/F:/Projects/Pension_Management_System/ui/src/components/ui/badgeVariants.ts:23) | `bg-transparent` |
| [ui/src/components/ui/buttonVariants.ts:27](/F:/Projects/Pension_Management_System/ui/src/components/ui/buttonVariants.ts:27) | `border-transparent` |
| [ui/src/components/ui/chart.tsx:31](/F:/Projects/Pension_Management_System/ui/src/components/ui/chart.tsx:31) | `stroke-transparent` |
| [ui/src/components/ui/chart.tsx:31](/F:/Projects/Pension_Management_System/ui/src/components/ui/chart.tsx:31) | `stroke-transparent` |
| [ui/src/components/ui/CornerEditor.tsx:715](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:715) | `transparent` |
| [ui/src/components/ui/DataTable.tsx:558](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:558) | `bg-transparent` |
| [ui/src/components/ui/DataTable.tsx:610](/F:/Projects/Pension_Management_System/ui/src/components/ui/DataTable.tsx:610) | `bg-transparent` |
| [ui/src/components/ui/Dialog.tsx:388](/F:/Projects/Pension_Management_System/ui/src/components/ui/Dialog.tsx:388) | `opacity-0` |
| [ui/src/components/ui/FileUploader.tsx:921](/F:/Projects/Pension_Management_System/ui/src/components/ui/FileUploader.tsx:921) | `opacity-0` |
| [ui/src/components/ui/FormSection.tsx:118](/F:/Projects/Pension_Management_System/ui/src/components/ui/FormSection.tsx:118) | `opacity-0` |
| [ui/src/components/ui/Input.tsx:103](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:103) | `file:bg-transparent` |
| [ui/src/components/ui/Input.tsx:374](/F:/Projects/Pension_Management_System/ui/src/components/ui/Input.tsx:374) | `opacity: 0` |
| [ui/src/components/ui/Loader.tsx:35](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:35) | `border-transparent` |
| [ui/src/components/ui/Loader.tsx:39](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:39) | `border-transparent` |
| [ui/src/components/ui/Loader.tsx:114](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:114) | `opacity-0` |
| [ui/src/components/ui/Loader.tsx:143](/F:/Projects/Pension_Management_System/ui/src/components/ui/Loader.tsx:143) | `opacity-0` |
| [ui/src/components/ui/print/PrintLayout.tsx:196](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintLayout.tsx:196) | `transparent` |
| [ui/src/components/ui/ScrollArea.tsx:34](/F:/Projects/Pension_Management_System/ui/src/components/ui/ScrollArea.tsx:34) | `border-l-transparent` |
| [ui/src/components/ui/ScrollArea.tsx:36](/F:/Projects/Pension_Management_System/ui/src/components/ui/ScrollArea.tsx:36) | `border-t-transparent` |
| [ui/src/components/ui/Stepper.tsx:58](/F:/Projects/Pension_Management_System/ui/src/components/ui/Stepper.tsx:58) | `bg-transparent` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:139](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:139) | `focus:border-transparent` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:162](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:162) | `focus:border-transparent` |
| [ui/src/pages/auth/ChangePasswordPage.tsx:185](/F:/Projects/Pension_Management_System/ui/src/pages/auth/ChangePasswordPage.tsx:185) | `focus:border-transparent` |
| [ui/src/pages/auth/LoginPage.tsx:193](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:193) | `focus:border-transparent` |
| [ui/src/pages/auth/LoginPage.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:206) | `focus:border-transparent` |
| [ui/src/pages/documents/MissingDocsCompletionPage.tsx:453](/F:/Projects/Pension_Management_System/ui/src/pages/documents/MissingDocsCompletionPage.tsx:453) | `opacity-0` |
| [ui/src/pages/hr/EmployeeDetailsPage.tsx:415](/F:/Projects/Pension_Management_System/ui/src/pages/hr/EmployeeDetailsPage.tsx:415) | `opacity-0` |
| [ui/src/pages/hr/HrDashboardPage.tsx:188](/F:/Projects/Pension_Management_System/ui/src/pages/hr/HrDashboardPage.tsx:188) | `border-transparent` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:395](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:395) | `opacity-0` |
| [ui/src/pages/pension/AnnualBookReviewPage.tsx:399](/F:/Projects/Pension_Management_System/ui/src/pages/pension/AnnualBookReviewPage.tsx:399) | `group-hover:opacity-0` |
| [ui/src/pages/print/AccountingReportPrintPage.tsx:716](/F:/Projects/Pension_Management_System/ui/src/pages/print/AccountingReportPrintPage.tsx:716) | `border-t-transparent` |
| [ui/src/pages/print/PaymentBookPrint.tsx:487](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:487) | `transparent` |
| [ui/src/pages/retirement-files/FileReviewPage.tsx:151](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/FileReviewPage.tsx:151) | `border-transparent` |
| [ui/src/pages/retirement-files/FileWorkflowManager.tsx:269](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/FileWorkflowManager.tsx:269) | `bg-transparent` |
| [ui/src/pages/retirement-files/FileWorkflowManager.tsx:277](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/FileWorkflowManager.tsx:277) | `bg-transparent` |
| [ui/src/pages/retirement-files/FileWorkflowManager.tsx:277](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/FileWorkflowManager.tsx:277) | `data-[state=active]:bg-transparent` |
| [ui/src/pages/retirement-files/tabs/NotesTab.tsx:110](/F:/Projects/Pension_Management_System/ui/src/pages/retirement-files/tabs/NotesTab.tsx:110) | `opacity-0` |
| [ui/src/pages/review/FileReviewDetailPage.tsx:553](/F:/Projects/Pension_Management_System/ui/src/pages/review/FileReviewDetailPage.tsx:553) | `to-transparent` |

### شفافية لون تعبئة أو حد الأيقونة — 2 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/ui/AuditTimeline.tsx:237](/F:/Projects/Pension_Management_System/ui/src/components/ui/AuditTimeline.tsx:237) | `decoration-muted-foreground/30` |
| [ui/src/components/ui/chart.tsx:31](/F:/Projects/Pension_Management_System/ui/src/components/ui/chart.tsx:31) | `stroke-border/50` |

### تلاشي مرتبط بحركة الفتح والإغلاق — 13 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/ui/AlertDialog.tsx:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:19) | `data-[state=closed]:fade-out-0` |
| [ui/src/components/ui/AlertDialog.tsx:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:19) | `data-[state=open]:fade-in-0` |
| [ui/src/components/ui/AlertDialog.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:37) | `data-[state=closed]:fade-out-0` |
| [ui/src/components/ui/AlertDialog.tsx:37](/F:/Projects/Pension_Management_System/ui/src/components/ui/AlertDialog.tsx:37) | `data-[state=open]:fade-in-0` |
| [ui/src/components/ui/DropdownMenu.tsx:23](/F:/Projects/Pension_Management_System/ui/src/components/ui/DropdownMenu.tsx:23) | `data-[state=closed]:fade-out-0` |
| [ui/src/components/ui/DropdownMenu.tsx:23](/F:/Projects/Pension_Management_System/ui/src/components/ui/DropdownMenu.tsx:23) | `data-[state=open]:fade-in-0` |
| [ui/src/components/ui/Popover.tsx:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/Popover.tsx:19) | `data-[state=closed]:fade-out-0` |
| [ui/src/components/ui/Popover.tsx:19](/F:/Projects/Pension_Management_System/ui/src/components/ui/Popover.tsx:19) | `data-[state=open]:fade-in-0` |
| [ui/src/components/ui/Select.tsx:100](/F:/Projects/Pension_Management_System/ui/src/components/ui/Select.tsx:100) | `data-[state=open]:fade-in-0` |
| [ui/src/components/ui/Select.tsx:101](/F:/Projects/Pension_Management_System/ui/src/components/ui/Select.tsx:101) | `data-[state=closed]:fade-out-0` |
| [ui/src/components/ui/Sheet.tsx:22](/F:/Projects/Pension_Management_System/ui/src/components/ui/Sheet.tsx:22) | `data-[state=open]:fade-in-0` |
| [ui/src/components/ui/Sheet.tsx:23](/F:/Projects/Pension_Management_System/ui/src/components/ui/Sheet.tsx:23) | `data-[state=closed]:fade-out-0` |
| [ui/src/components/ui/TabbedFormSection.tsx:114](/F:/Projects/Pension_Management_System/ui/src/components/ui/TabbedFormSection.tsx:114) | `fade-in-0` |

### شفافية عبر style أو CSS أو خاصية رسم — 7 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/common/charts/ReportBarChart.tsx:229](/F:/Projects/Pension_Management_System/ui/src/components/common/charts/ReportBarChart.tsx:229) | `opacity={0.6` |
| [ui/src/components/common/charts/ReportBarChart.tsx:252](/F:/Projects/Pension_Management_System/ui/src/components/common/charts/ReportBarChart.tsx:252) | `opacity: 0.4 ` |
| [ui/src/components/ui/print/PrintLayout.tsx:280](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintLayout.tsx:280) | `opacity: ${wmOpacity` |
| [ui/src/pages/print/PaymentBookPrint.tsx:272](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:272) | `opacity: 0.10` |
| [ui/src/pages/print/PaymentBookPrint.tsx:384](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:384) | `opacity: 0.8` |
| [ui/src/pages/print/PaymentBookPrint.tsx:409](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:409) | `opacity: 0.95` |
| [ui/src/pages/print/PaymentBookPrint.tsx:729](/F:/Projects/Pension_Management_System/ui/src/pages/print/PaymentBookPrint.tsx:729) | `opacity: 0.20` |

### إعدادات شفافية العلامة المائية — 3 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/ui/print/PrintLayout.tsx:99](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintLayout.tsx:99) | `watermarkOpacity: 0.07` |
| [ui/src/components/ui/print/PrintLayout.tsx:503](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintLayout.tsx:503) | `watermarkOpacity={cfg.watermarkOpacity ?? 0.07` |
| [ui/src/pages/accounting/QuarterDetailsPage.tsx:298](/F:/Projects/Pension_Management_System/ui/src/pages/accounting/QuarterDetailsPage.tsx:298) | `watermarkOpacity: 0.06` |

### ألوان rgba أو hsla أو HEX مع قناة شفافية — 9 موضعًا

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/components/layout/Sidebar.tsx:310](/F:/Projects/Pension_Management_System/ui/src/components/layout/Sidebar.tsx:310) | `rgba(255,255,255,0.08)` |
| [ui/src/components/ui/CornerEditor.tsx:505](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:505) | `rgba(0, 0, 0, 0.5)` |
| [ui/src/components/ui/CornerEditor.tsx:651](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:651) | `rgba(0, 0, 0, 0.78)` |
| [ui/src/components/ui/CornerEditor.tsx:689](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:689) | `rgba(0, 0, 0, 0.9)` |
| [ui/src/components/ui/CornerEditor.tsx:691](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:691) | `rgba(255, 255, 255, 0.98)` |
| [ui/src/components/ui/CornerEditor.tsx:703](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:703) | `rgba(0, 0, 0, 0.45)` |
| [ui/src/components/ui/CornerEditor.tsx:709](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:709) | `rgba(0, 0, 0, 0.72)` |
| [ui/src/components/ui/CornerEditor.tsx:717](/F:/Projects/Pension_Management_System/ui/src/components/ui/CornerEditor.tsx:717) | `rgba(255, 255, 255, 0.98)` |
| [ui/src/components/ui/print/PrintLayout.tsx:269](/F:/Projects/Pension_Management_System/ui/src/components/ui/print/PrintLayout.tsx:269) | `rgba(0,0,0,0.2)` |

## القواعد المشتركة المؤثرة

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/index.css:502](/F:/Projects/Pension_Management_System/ui/src/index.css:502) | `transparent` |
| [ui/src/index.css:725](/F:/Projects/Pension_Management_System/ui/src/index.css:725) | `transparent` |
| [ui/src/index.css:772](/F:/Projects/Pension_Management_System/ui/src/index.css:772) | `opacity: 0` |
| [ui/src/index.css:801](/F:/Projects/Pension_Management_System/ui/src/index.css:801) | `opacity: 0.7` |
| [ui/src/index.css:817](/F:/Projects/Pension_Management_System/ui/src/index.css:817) | `transparent` |
| [ui/src/index.css:867](/F:/Projects/Pension_Management_System/ui/src/index.css:867) | `opacity: 0.04` |
| [ui/src/index.css:874](/F:/Projects/Pension_Management_System/ui/src/index.css:874) | `opacity: 0.05` |
| [ui/src/index.css:881](/F:/Projects/Pension_Management_System/ui/src/index.css:881) | `opacity: 0.05` |
| [ui/src/index.css:885](/F:/Projects/Pension_Management_System/ui/src/index.css:885) | `opacity: 0.06` |
| [ui/src/index.css:889](/F:/Projects/Pension_Management_System/ui/src/index.css:889) | `opacity: 0.07` |
| [ui/src/index.css:1007](/F:/Projects/Pension_Management_System/ui/src/index.css:1007) | `opacity: 0` |
| [ui/tailwind.config.js:157](/F:/Projects/Pension_Management_System/ui/tailwind.config.js:157) | `opacity: "0"` |

## إشارات مستبعدة من العدد الأساسي

قيم العتامة الكاملة: 139. إشارات Tailwind غير الصحيحة: 2.

| الملف والسطر | الاستخدام |
|---|---|
| [ui/src/pages/auth/LoginPage.tsx:193](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:193) | `placeholderr:text-hero-accent/50` |
| [ui/src/pages/auth/LoginPage.tsx:206](/F:/Projects/Pension_Management_System/ui/src/pages/auth/LoginPage.tsx:206) | `placeholderr:text-hero-accent/50` |

## إعادة إنتاج الجرد

شغّل `node plan/transparency-scan.mjs` من جذر المشروع. البيانات الكاملة مع سياق كل سطر محفوظة في `plan/transparency-inventory.json`. لم تُشغّل اختبارات التطبيق لأن الفحص لا يعدّل تنفيذه.
