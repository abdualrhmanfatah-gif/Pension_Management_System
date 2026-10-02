import { useQuery } from "@tanstack/react-query";
import { Download, FileText, Printer, RefreshCw } from "lucide-react";
import { useCallback,useEffect, useState } from "react";
import { toast } from "sonner";

import { accountsApi, fiscalYearsApi, periodsApi, quartersApi, reportsApi } from "@/api/accounting";
import {
  AccountStatementReport,
  BalanceSheetReport,
  BudgetReportView,
  CashFlowReport,
  CostCenterReport,
  CumulativePnLReport,
  CurrentOperationsReport,
  DepreciationReport,
  FixedAssetsReport,
  GeneralLedgerReport,
  IncomeStatementReport,
  JournalRegisterReport,
  LiabilitiesReport,
  ProvisionsReport,
  QuarterlyReport,
  SubledgerReport,
  TrialBalanceReport,
  YearOverYearReport,
} from "@/components/domain/accounting";
import { Button, Card, DateInput, Heading, PageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, StickyHeader, Text } from "@/components/ui";
import type { ChartOfAccountDto, FiscalPeriodDto, FiscalYearSummaryDto, QuarterDto } from "@/types/accounting";

// ═══════════════════════════════════════
// Types
// ═══════════════════════════════════════
type ReportType =
  | "trial-balance"
  | "balance-sheet"
  | "income-statement"
  | "quarterly"
  | "general-ledger"
  | "account-statement"
  | "journal-register"
  | "cash-flow"
  | "budget"
  | "current-operations"
  | "liabilities"
  | "year-over-year"
  | "fixed-assets"
  | "cumulative-pnl"
  | "cost-centers"
  | "provisions"
  | "depreciation"
  | "subledger";

type FilterKey = "accountLevel" | "toDate" | "fromDate" | "quarter" | "assetType" | "comparisonYear" | "accountId" | "periodId";

interface ReportConfig {
  type: ReportType;
  label: string;
  description: string;
  category: string;
  printPath: string;
  filters: FilterKey[];
}

// ═══════════════════════════════════════
// Report Configurations
// ═══════════════════════════════════════
const REPORT_CONFIGS: ReportConfig[] = [
  { type: "trial-balance", label: "ميزان المراجعة", description: "قائمة بأرصدة جميع الحسابات", category: "التقارير الأساسية", printPath: "/reports/trial-balance/print", filters: ["accountLevel"] },
  { type: "balance-sheet", label: "الميزانية العمومية", description: "المركز المالي في تاريخ محدد", category: "التقارير الأساسية", printPath: "/reports/balance-sheet/print", filters: ["toDate"] },
  { type: "income-statement", label: "قائمة الدخل", description: "الإيرادات والمصروفات وصافي الدخل", category: "التقارير الأساسية", printPath: "/reports/income-statement/print", filters: [] },
  { type: "quarterly", label: "التقرير الفصلي", description: "قائمة الدخل حسب الفصل", category: "التقارير الأساسية", printPath: "/reports/quarterly/print", filters: ["quarter"] },
  { type: "general-ledger", label: "دفتر الأستاذ العام", description: "حركات حساب معين", category: "دفاتر الأستاذ", printPath: "/reports/general-ledger/print", filters: ["accountId", "fromDate", "toDate"] },
  { type: "account-statement", label: "كشف حساب", description: "كشف حساب مفصل لحساب معين", category: "دفاتر الأستاذ", printPath: "/reports/account-statement/print", filters: ["accountId", "fromDate", "toDate"] },
  { type: "subledger", label: "كشف حساب تفصيلي", description: "عرض حركات حساب معين بتفاصيل القيود", category: "دفاتر الأستاذ", printPath: "/reports/subledger/print", filters: ["accountId", "fromDate", "toDate"] },
  { type: "journal-register", label: "سجل اليومية", description: "سجل بجميع القيود في فترة محددة", category: "دفاتر الأستاذ", printPath: "/reports/journal-register/print", filters: ["fromDate", "toDate"] },
  { type: "cash-flow", label: "التدفقات النقدية", description: "تحليل التدفقات النقدية", category: "التدفقات والميزانية", printPath: "/reports/cash-flow/print", filters: [] },
  { type: "budget", label: "تقرير الموازنة", description: "مقارنة المخطط مع الفعلي", category: "التدفقات والميزانية", printPath: "/reports/budget/print", filters: [] },
  { type: "liabilities", label: "الديون والالتزامات", description: "الالتزامات القصيرة والطويلة الأجل", category: "المركز المالي", printPath: "/reports/liabilities/print", filters: ["toDate"] },
  { type: "fixed-assets", label: "الأصول الثابتة", description: "سجل الأصول الثابتة", category: "المركز المالي", printPath: "/reports/fixed-assets/print", filters: ["assetType"] },
  { type: "depreciation", label: "تقرير الإهلاك", description: "جدول إهلاك جميع الأصول النشطة", category: "المركز المالي", printPath: "/reports/depreciation/print", filters: ["assetType"] },
  { type: "provisions", label: "المخصصات والاحتياطيات", description: "المخصصات المالية والاستحقاقات", category: "المركز المالي", printPath: "/reports/provisions/print", filters: ["toDate"] },
  { type: "current-operations", label: "العمليات الجارية", description: "ملخص العمليات المالية الحالية", category: "تحليل الأداء", printPath: "/reports/current-operations/print", filters: ["periodId"] },
  { type: "year-over-year", label: "المقارنة السنوية", description: "مقارنة الأداء بين سنتين", category: "تحليل الأداء", printPath: "/reports/year-over-year/print", filters: ["comparisonYear"] },
  { type: "cumulative-pnl", label: "الأرباح والخسائر المتراكم", description: "قائمة الدخل التراكمية", category: "تحليل الأداء", printPath: "/reports/cumulative-pnl/print", filters: [] },
  { type: "cost-centers", label: "مراكز التكلفة", description: "أداء مراكز التكلفة", category: "تحليل الأداء", printPath: "/reports/cost-centers/print", filters: [] },
];

const REPORT_GROUPS = REPORT_CONFIGS.reduce((acc, config) => {
  if (!acc[config.category]) acc[config.category] = [];
  acc[config.category].push(config);
  return acc;
}, {} as Record<string, ReportConfig[]>);

const ACCOUNT_LEVEL_OPTIONS = [
  { value: "all", label: "جميع المستويات" },
  { value: "1", label: "المستوى 1 — الأبواب" },
  { value: "2", label: "المستوى 2 — الحسابات العامة" },
  { value: "3", label: "المستوى 3 — الحسابات المساعدة" },
  { value: "4", label: "المستوى 4 — الحسابات الفرعية" },
  { value: "5", label: "المستوى 5 — التفاصيل" },
];

const ASSET_TYPE_OPTIONS = [
  { value: "all", label: "الكل" },
  { value: "1", label: "الأراضي" },
  { value: "2", label: "المباني" },
  { value: "3", label: "الأثاث والمعدات" },
  { value: "4", label: "المركبات" },
  { value: "5", label: "حقوق الإيجار" },
];

// ═══════════════════════════════════════
// Component
// ═══════════════════════════════════════
export function ReportsPage() {
  const [activeReport, setActiveReport] = useState<ReportType>("trial-balance");
  const [selectedYearId, setSelectedYearId] = useState<number | undefined>();

  // الفلاتر الفرعية
  const [accountLevel, setAccountLevel] = useState("all");
  const [toDate, setToDate] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [quarter, setQuarter] = useState("");
  const [assetType, setAssetType] = useState("all");
  const [comparisonYear, setComparisonYear] = useState("");
  const [accountId, setAccountId] = useState<number | null>(null);
  const [periodId, setPeriodId] = useState<number | undefined>();

  // جلب البيانات
  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({ queryKey: ["fiscal-years"], queryFn: () => fiscalYearsApi.getAll() });
  const { data: _accounts } = useQuery<ChartOfAccountDto[]>({ queryKey: ["accounts", "all"], queryFn: () => accountsApi.getAll() });
  const { data: postableAccounts } = useQuery<ChartOfAccountDto[]>({ queryKey: ["accounts", "postable"], queryFn: () => accountsApi.getPostable() });
  const { data: periods } = useQuery<FiscalPeriodDto[]>({ queryKey: ["periods", selectedYearId], queryFn: () => periodsApi.getAll(selectedYearId!), enabled: !!selectedYearId });
  const { data: quarters } = useQuery<QuarterDto[]>({ queryKey: ["quarters", selectedYearId], queryFn: () => quartersApi.getAll(selectedYearId!), enabled: !!selectedYearId });

  // تصفير الفلاتر عند تغيير نوع التقرير
  useEffect(() => {
    setAccountLevel("all");
    setToDate("");
    setFromDate("");
    setQuarter("");
    setAssetType("all");
    setComparisonYear("");
    setAccountId(null);
    setPeriodId(undefined);
  }, [activeReport]);

  // اختيار السنة المالية تلقائياً
  useEffect(() => {
    if (fiscalYears && fiscalYears.length > 0 && !selectedYearId) {
      const currentYear = new Date().getFullYear();
      const match = fiscalYears.find((y) => y.yearName === String(currentYear));
      setSelectedYearId(match?.fiscalYearID ?? fiscalYears[fiscalYears.length - 1].fiscalYearID);
    }
  }, [fiscalYears]);

  // اختيار الفترة تلقائياً
  useEffect(() => {
    if (periods && periods.length > 0 && !periodId) {
      const active = periods.find(p => !p.isClosed);
      setPeriodId(active?.periodID ?? periods[periods.length - 1].periodID);
    }
  }, [periods]);

  // اختيار الفصل تلقائياً
  useEffect(() => {
    if (quarters && quarters.length > 0 && !quarter) {
      const openQ = quarters.find(q => !q.isClosed);
      setQuarter(String(openQ?.quarterID ?? quarters[quarters.length - 1].quarterID));
    }
  }, [quarters]);

  const selectedYear = fiscalYears?.find((y) => y.fiscalYearID === selectedYearId);
  const activeReportConfig = REPORT_CONFIGS.find(r => r.type === activeReport);

  // تصدير
  const [isExporting, setIsExporting] = useState(false);
  const [exportFormat, setExportFormat] = useState<"excel" | "pdf">("excel");

  const handleExport = useCallback(async (format: "excel" | "pdf") => {
    if (!activeReportConfig) return;
    setIsExporting(true);
    setExportFormat(format);
    try {
      let response;
      const params: any = {};
      if (selectedYearId) params.fiscalYearId = selectedYearId;
      if (fromDate || selectedYear?.startDate) params.fromDate = fromDate || selectedYear?.startDate;
      if (toDate || selectedYear?.endDate) params.toDate = toDate || selectedYear?.endDate;
      if (accountId) params.accountId = accountId;
      if (periodId) params.periodId = periodId;
      if (quarter) params.quarterId = Number(quarter);
      params.format = format;

      switch (activeReport) {
        case "trial-balance": response = await reportsApi.exportTrialBalance(params); break;
        case "balance-sheet": response = await reportsApi.exportBalanceSheet({ asOfDate: params.toDate, format }); break;
        case "income-statement": response = await reportsApi.exportIncomeStatement(params); break;
        case "general-ledger": response = await reportsApi.exportGeneralLedger(params); break;
        case "account-statement": response = await reportsApi.exportAccountStatement(params); break;
        case "subledger": response = await reportsApi.exportSubledger(params); break;
        case "journal-register": response = await reportsApi.exportJournalRegister(params); break;
        case "cash-flow": response = await reportsApi.exportCashFlow(params); break;
        default: toast.error("هذا التقرير لا يدعم التصدير حالياً"); return;
      }

      if (response?.data) {
        const blob = new Blob([response.data], { type: format === "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
        const link = document.createElement("a");
        link.href = window.URL.createObjectURL(blob);
        link.download = `${activeReportConfig?.label || "report"}_${new Date().toISOString().slice(0, 10)}.${format === "pdf" ? "pdf" : "xlsx"}`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(link.href);
        toast.success("تم التصدير بنجاح");
      }
    } catch {
      toast.error("حدث خطأ أثناء التصدير");
    } finally {
      setIsExporting(false);
    }
  }, [activeReportConfig, activeReport, selectedYearId, selectedYear, fromDate, toDate, accountId, periodId, quarter]);

  // طباعة
  const handlePrint = useCallback(() => {
    if (!activeReportConfig) return;
    const params = new URLSearchParams();
    if (selectedYearId) params.set("fiscalYearId", String(selectedYearId));
    if (selectedYear?.endDate) params.set("date", selectedYear.endDate);
    if (selectedYear?.startDate) params.set("fromDate", fromDate || selectedYear.startDate);
    if (toDate || selectedYear?.endDate) params.set("toDate", (toDate || selectedYear?.endDate)!);
    if (accountId) params.set("accountId", String(accountId));
    if (periodId) params.set("periodId", String(periodId));
    if (quarter) params.set("quarterId", quarter);
    window.open(`${activeReportConfig.printPath}?${params.toString()}`, "_blank");
  }, [activeReportConfig, selectedYearId, selectedYear, fromDate, toDate, accountId, periodId, quarter]);

  const filters = activeReportConfig?.filters || [];

  return (
    <PageLayout>
      {/* ═══ العنوان ═══ */}
      <StickyHeader>
        <div className="flex items-center gap-4">
          <div className="p-3 bg-primary/10 rounded-2xl">
            <FileText className="w-7 h-7 md:w-9 md:h-9 text-primary" />
          </div>
          <div>
            <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">التقارير المالية</Heading>
            <Text variant="caption" className="md:text-sm mt-1">التقارير المحاسبية الموحدة للصندوق</Text>
          </div>
        </div>
      </StickyHeader>

      {/* ═══ شريط الفلاتر الموحد ═══ */}
      <Card className="p-4 mb-6">
        <div className="flex flex-col xl:flex-row items-end gap-3">
          {/* نوع التقرير */}
          <div className="w-full xl:w-64 space-y-1">
            <Text variant="caption" className="font-bold uppercase tracking-wider">نوع التقرير</Text>
            <Select value={activeReport} onValueChange={(v) => setActiveReport(v as ReportType)}>
              <SelectTrigger className="h-10 text-sm">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="max-h-[400px]">
                {Object.entries(REPORT_GROUPS).map(([group, reports]) => (
                  <div key={group}>
                    <div className="px-2 py-1.5 text-xs font-bold text-muted-foreground bg-muted/50 sticky top-0">{group}</div>
                    {reports.map((r) => (
                      <SelectItem key={r.type} value={r.type}>
                        <span>{r.label}</span>
                      </SelectItem>
                    ))}
                  </div>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* السنة المالية */}
          <div className="w-full xl:w-40 space-y-1">
            <Text variant="caption" className="font-bold uppercase tracking-wider">السنة المالية</Text>
            <Select value={selectedYearId != null ? String(selectedYearId) : undefined} onValueChange={(v) => setSelectedYearId(Number(v))}>
              <SelectTrigger className="h-10 text-sm"><SelectValue placeholder="السنة" /></SelectTrigger>
              <SelectContent>
                {fiscalYears?.map((y) => (
                  <SelectItem key={y.fiscalYearID} value={String(y.fiscalYearID)}>{y.yearName} {y.isClosed ? "(مغلقة)" : ""}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* ═══ الفلاتر الفرعية حسب نوع التقرير ═══ */}

          {/* مستوى الحساب - ميزان المراجعة */}
          {filters.includes("accountLevel") && (
            <div className="w-full xl:w-48 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">المستوى</Text>
              <Select value={accountLevel} onValueChange={setAccountLevel}>
                <SelectTrigger className="h-10 text-sm"><SelectValue /></SelectTrigger>
                <SelectContent>
                  {ACCOUNT_LEVEL_OPTIONS.map(o => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* حتى تاريخ - الميزانية/الديون/المخصصات */}
          {filters.includes("toDate") && !filters.includes("fromDate") && (
            <div className="w-full xl:w-40 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">حتى تاريخ</Text>
              <DateInput className="h-10" value={toDate || selectedYear?.endDate || ""} onChange={(v) => setToDate(v || "")} />
            </div>
          )}

          {/* من تاريخ / إلى تاريخ - الأستاذ/الكشف/اليومية */}
          {filters.includes("fromDate") && (
            <>
              <div className="w-full xl:w-36 space-y-1">
                <Text variant="caption" className="font-bold uppercase tracking-wider">من تاريخ</Text>
                <DateInput className="h-10" value={fromDate || selectedYear?.startDate || ""} onChange={(v) => setFromDate(v || "")} />
              </div>
              <div className="w-full xl:w-36 space-y-1">
                <Text variant="caption" className="font-bold uppercase tracking-wider">إلى تاريخ</Text>
                <DateInput className="h-10" value={toDate || selectedYear?.endDate || ""} onChange={(v) => setToDate(v || "")} />
              </div>
            </>
          )}

          {/* الفصل - التقرير الفصلي */}
          {filters.includes("quarter") && (
            <div className="w-full xl:w-40 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">الفصل</Text>
              <Select value={quarter} onValueChange={setQuarter}>
                <SelectTrigger className="h-10 text-sm"><SelectValue placeholder="اختر الفصل" /></SelectTrigger>
                <SelectContent>
                  {quarters?.map(q => <SelectItem key={q.quarterID} value={String(q.quarterID)}>{q.quarterName} {q.isClosed ? "(مغلق)" : ""}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* الحساب - دفتر الأستاذ/كشف حساب */}
          {filters.includes("accountId") && (
            <div className="w-full xl:w-64 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">الحساب</Text>
              <Select value={accountId != null ? String(accountId) : ""} onValueChange={(v) => setAccountId(Number(v))}>
                <SelectTrigger className="h-10 text-sm"><SelectValue placeholder="اختر الحساب" /></SelectTrigger>
                <SelectContent className="max-h-[250px]">
                  {postableAccounts?.map((a: any) => (
                    <SelectItem key={a.accountID} value={String(a.accountID)}>{a.accountCode} - {a.accountName}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* نوع الأصل - الأصول الثابتة/الإهلاك */}
          {filters.includes("assetType") && (
            <div className="w-full xl:w-44 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">نوع الأصل</Text>
              <Select value={assetType} onValueChange={setAssetType}>
                <SelectTrigger className="h-10 text-sm"><SelectValue /></SelectTrigger>
                <SelectContent>
                  {ASSET_TYPE_OPTIONS.map(o => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* السنة للمقارنة */}
          {filters.includes("comparisonYear") && (
            <div className="w-full xl:w-40 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">السنة السابقة</Text>
              <Select value={comparisonYear} onValueChange={setComparisonYear}>
                <SelectTrigger className="h-10 text-sm"><SelectValue placeholder="اختر السنة" /></SelectTrigger>
                <SelectContent>
                  {fiscalYears?.map(y => <SelectItem key={y.fiscalYearID} value={String(y.fiscalYearID)}>{y.yearName}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* الفترة المحاسبية */}
          {filters.includes("periodId") && (
            <div className="w-full xl:w-52 space-y-1">
              <Text variant="caption" className="font-bold uppercase tracking-wider">الفترة المحاسبية</Text>
              <Select value={periodId != null ? String(periodId) : ""} onValueChange={(v) => setPeriodId(Number(v))}>
                <SelectTrigger className="h-10 text-sm"><SelectValue placeholder="اختر الفترة" /></SelectTrigger>
                <SelectContent>
                  {periods?.map(p => <SelectItem key={p.periodID} value={String(p.periodID)}>{p.periodName} {p.isClosed ? "(مغلقة)" : ""}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* أزرار */}
          <div className="flex items-center gap-2 w-full xl:w-auto">
            <Button variant="outline" size="sm" onClick={handlePrint} className="gap-2 h-10">
              <Printer className="w-4 h-4" /> طباعة
            </Button>
            <Button variant={exportFormat === "excel" ? "success" : "outline"} size="sm" onClick={() => handleExport("excel")} disabled={isExporting} className="gap-2 h-10">
              {isExporting && exportFormat === "excel" ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
              Excel
            </Button>
            <Button variant={exportFormat === "pdf" ? "success" : "outline"} size="sm" onClick={() => handleExport("pdf")} disabled={isExporting} className="gap-2 h-10">
              {isExporting && exportFormat === "pdf" ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
              PDF
            </Button>
          </div>
        </div>
      </Card>

      {/* ═══ محتوى التقرير ═══ */}
      <Card className="border-none rounded-3xl p-6 shadow-sm">
        {activeReport === "trial-balance" && (
          <TrialBalanceReport fiscalYearId={selectedYearId} fromDate={selectedYear?.startDate} toDate={selectedYear?.endDate} externalFilters={{ accountLevel }} hideInternalControls />
        )}
        {activeReport === "balance-sheet" && (
          <BalanceSheetReport fiscalYearId={selectedYearId} asOfDate={toDate || selectedYear?.endDate} externalFilters={{ toDate }} hideInternalControls />
        )}
        {activeReport === "income-statement" && (
          <IncomeStatementReport fiscalYearId={selectedYearId} />
        )}
        {activeReport === "quarterly" && (
          <QuarterlyReport fiscalYearId={selectedYearId} externalFilters={{ quarter: Number(quarter) }} hideInternalControls />
        )}
        {activeReport === "general-ledger" && (
          <GeneralLedgerReport fiscalYearId={selectedYearId} fromDate={fromDate || selectedYear?.startDate} toDate={toDate || selectedYear?.endDate} externalFilters={{ accountId: accountId ?? undefined }} hideInternalControls />
        )}
        {activeReport === "account-statement" && (
          <AccountStatementReport fiscalYearId={selectedYearId} fromDate={fromDate || selectedYear?.startDate} toDate={toDate || selectedYear?.endDate} externalFilters={{ accountId: accountId ?? undefined }} hideInternalControls />
        )}
        {activeReport === "subledger" && (
          <SubledgerReport fiscalYearId={selectedYearId} fromDate={fromDate || selectedYear?.startDate} toDate={toDate || selectedYear?.endDate} externalFilters={{ accountId: accountId ?? undefined }} hideInternalControls />
        )}
        {activeReport === "journal-register" && (
          <JournalRegisterReport fiscalYearId={selectedYearId} fromDate={fromDate || selectedYear?.startDate} toDate={toDate || selectedYear?.endDate} hideInternalControls />
        )}
        {activeReport === "cash-flow" && (
          <CashFlowReport fiscalYearId={selectedYearId} hideInternalControls />
        )}
        {activeReport === "budget" && (
          <BudgetReportView fiscalYearId={selectedYearId} hideInternalControls />
        )}
        {activeReport === "current-operations" && (
          <CurrentOperationsReport fiscalYearId={selectedYearId} externalFilters={{ periodId }} hideInternalControls />
        )}
        {activeReport === "liabilities" && (
          <LiabilitiesReport fiscalYearId={selectedYearId} asOfDate={toDate || selectedYear?.endDate} externalFilters={{ toDate }} hideInternalControls />
        )}
        {activeReport === "year-over-year" && (
          <YearOverYearReport fiscalYearId={selectedYearId} externalFilters={{ comparisonYear: Number(comparisonYear) }} hideInternalControls />
        )}
        {activeReport === "fixed-assets" && (
          <FixedAssetsReport fiscalYearId={selectedYearId} asOfDate={selectedYear?.endDate} externalFilters={{ assetType }} hideInternalControls />
        )}
        {activeReport === "cumulative-pnl" && (
          <CumulativePnLReport fiscalYearId={selectedYearId} hideInternalControls />
        )}
        {activeReport === "cost-centers" && (
          <CostCenterReport fiscalYearId={selectedYearId} hideInternalControls />
        )}
        {activeReport === "provisions" && (
          <ProvisionsReport fiscalYearId={selectedYearId} asOfDate={toDate || selectedYear?.endDate} externalFilters={{ toDate }} hideInternalControls />
        )}
        {activeReport === "depreciation" && (
          <DepreciationReport fiscalYearId={selectedYearId} externalFilters={{ assetType }} hideInternalControls />
        )}
      </Card>
    </PageLayout>
  );
}
