import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { fiscalYearsApi, quartersApi,reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { Card } from "@/components/ui/Card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import { cn } from "@/lib/utils";
import type { FiscalYearSummaryDto, QuarterDto } from "@/types/accounting";

interface QuarterlyReportProps {
  fiscalYearId?: number;
  externalFilters?: { quarterId?: number };
  hideInternalControls?: boolean;
}

export function QuarterlyReport({ fiscalYearId, externalFilters, hideInternalControls }: QuarterlyReportProps) {
  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const [selectedYearId, setSelectedYearId] = useState<number | undefined>(fiscalYearId);
  const [selectedQuarterId, setSelectedQuarterId] = useState<number | undefined>();

  const { data: quarters } = useQuery({
    queryKey: ["quarters", selectedYearId],
    queryFn: () => quartersApi.getAll(selectedYearId!),
    enabled: !!selectedYearId,
  });

  useEffect(() => {
    if (fiscalYearId) {
      setSelectedYearId(fiscalYearId);
    } else if (fiscalYears && fiscalYears.length > 0 && !selectedYearId) {
      const currentYear = new Date().getFullYear();
      const match = fiscalYears.find((y: FiscalYearSummaryDto) => y.yearName === String(currentYear));
      setSelectedYearId(match?.fiscalYearID ?? fiscalYears[fiscalYears.length - 1].fiscalYearID);
    }
  }, [fiscalYears, fiscalYearId]);

  useEffect(() => {
    if (externalFilters?.quarterId && quarters) {
      const match = quarters.find((q: QuarterDto) => q.quarterID === externalFilters.quarterId);
      if (match) setSelectedQuarterId(match.quarterID);
    } else if (quarters && quarters.length > 0 && !selectedQuarterId) {
      const openQ = quarters.find((q: QuarterDto) => !q.isClosed);
      setSelectedQuarterId(openQ?.quarterID ?? quarters[quarters.length - 1].quarterID);
    }
  }, [externalFilters?.quarterId, quarters, selectedQuarterId]);

  const selectedQuarter = quarters?.find((q: QuarterDto) => q.quarterID === selectedQuarterId);

  const { data, isLoading, error } = useQuery({
    queryKey: ["reports", "income-statement", selectedYearId, selectedQuarterId],
    queryFn: () => reportsApi.incomeStatement(selectedYearId!, selectedQuarterId),
    enabled: !!selectedYearId && !!selectedQuarterId,
  });

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (selectedYearId) params.set("fiscalYearId", String(selectedYearId));
    if (selectedQuarterId) params.set("quarterId", String(selectedQuarterId));
    window.open(`/reports/quarterly/print?${params.toString()}`, "_blank");
  }, [selectedYearId, selectedQuarterId]);


  const customKPIs = data ? [
    { label: 'إجمالي الإيرادات', value: formatCurrency(data.totalRevenue), color: 'success' },
    { label: 'إجمالي المصروفات', value: formatCurrency(data.totalExpenses), color: 'danger' },
    { label: 'صافي الدخل الفصلي', value: formatCurrency(data.netIncome), color: data.netIncome >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'الفصل', value: selectedQuarter?.quarterName || '', color: 'primary' },
  ] : [];

  const chartContent = data ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportBarChart
        data={[
          { name: 'الإيرادات', value: data.totalRevenue },
          { name: 'المصروفات', value: data.totalExpenses },
          { name: 'صافي الدخل', value: data.netIncome },
        ]}
        xKey="name"
        yKeys={[{ key: 'value', color: chartColors.primary, label: 'المبلغ' }]}
        title="ملخص الفصل المالي"
        height={280}
      />
      <ReportPieChart
        data={[
          { name: 'الإيرادات', value: data.totalRevenue },
          { name: 'المصروفات', value: data.totalExpenses },
        ]}
        nameKey="name"
        valueKey="value"
        title="نسبة الإيرادات والمصروفات"
        height={280}
      />
    </div>
  ) : undefined;

  const filterBar = hideInternalControls ? undefined : (
    <Card className="p-4 flex flex-wrap gap-4 items-end mb-6">
      <div className="w-full sm:w-48">
        <label className="label-filter">السنة المالية</label>
        <Select
          value={selectedYearId != null ? String(selectedYearId) : undefined}
          onValueChange={(v) => {
            setSelectedYearId(Number(v));
            setSelectedQuarterId(undefined);
          }}
        >
          <SelectTrigger className="h-10">
            <SelectValue placeholder="اختر السنة" />
          </SelectTrigger>
          <SelectContent>
            {fiscalYears?.map((y: FiscalYearSummaryDto) => (
              <SelectItem key={y.fiscalYearID} value={String(y.fiscalYearID)}>
                {y.yearName} {y.isClosed ? "(مغلقة)" : ""}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="w-full sm:w-48">
        <label className="label-filter">الفصل المالي</label>
        <Select
          value={selectedQuarterId != null ? String(selectedQuarterId) : undefined}
          onValueChange={(v) => setSelectedQuarterId(Number(v))}
        >
          <SelectTrigger className="h-10">
            <SelectValue placeholder="اختر الفصل" />
          </SelectTrigger>
          <SelectContent>
            {quarters?.map((q: QuarterDto) => (
              <SelectItem key={q.quarterID} value={String(q.quarterID)}>
                {q.quarterName} {q.isClosed ? "(مغلق)" : ""}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </Card>
  );

  const renderLines = (lines: { accountCode: string; accountName: string; amount: number; accountLevel: number }[]) => {
    return lines.map((line, i) => (
      <div key={i} className={cn(
        "flex justify-between px-3 py-1.5 border-t border-border text-sm",
        line.accountLevel <= 1 && "font-medium bg-muted/20",
        line.accountLevel === 2 && "pr-6",
        line.accountLevel >= 3 && "pr-10 text-xs"
      )}>
        <span>{line.accountName}</span>
        <span className="font-mono">{formatCurrency(line.amount)}</span>
      </div>
    ));
  };

  return (
    <AccountingReportLayout
      title="التقرير الفصلي"
      description="عرض الإيرادات والمصروفات الفصلية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}    >
      {selectedQuarter && (
        <div className="bg-blue-50 border border-blue-200 text-blue-800 px-4 py-2 rounded-xl text-sm flex justify-between mb-4">
          <span>
            <strong>{selectedQuarter.quarterName}</strong> — {selectedQuarter.periods.length} فترات
          </span>
          <span>
            من {new Date(selectedQuarter.startDate).toLocaleDateString("ar-SA")} إلى{" "}
            {new Date(selectedQuarter.endDate).toLocaleDateString("ar-SA")}
          </span>
        </div>
      )}

      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !data || !selectedQuarter ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر الفصل لعرض التقرير الفصلي</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <div className="border rounded-xl overflow-hidden">
            <div className="bg-green-50 p-3 font-medium text-green-800">الإيرادات — {selectedQuarter.quarterName}</div>
            {renderLines(data.revenues)}
            <div className="flex justify-between p-3 bg-green-50/50 font-medium border-t">
              <span>إجمالي الإيرادات</span>
              <span className="font-mono">{formatCurrency(data.totalRevenue)}</span>
            </div>
          </div>

          <div className="border rounded-xl overflow-hidden">
            <div className="bg-red-50 p-3 font-medium text-red-800">المصروفات — {selectedQuarter.quarterName}</div>
            {renderLines(data.expenses)}
            <div className="flex justify-between p-3 bg-red-50/50 font-medium border-t">
              <span>إجمالي المصروفات</span>
              <span className="font-mono">{formatCurrency(data.totalExpenses)}</span>
            </div>
          </div>

          <div className="lg:col-span-2">
            <div className={cn("p-4 rounded-xl text-center", data.netIncome >= 0 ? "bg-green-50" : "bg-red-50")}>
              <span className="text-sm text-muted-foreground">صافي الدخل الفصلي</span>
              <p className={cn("text-2xl font-bold mt-1", data.netIncome >= 0 ? "text-green-700" : "text-red-700")}>
                {formatCurrency(data.netIncome)}
              </p>
              <p className="text-xs text-muted-foreground mt-1">
                من {new Date(selectedQuarter.startDate).toLocaleDateString("ar-SA")} إلى{" "}
                {new Date(selectedQuarter.endDate).toLocaleDateString("ar-SA")}
              </p>
            </div>
          </div>
        </div>
      )}
    </AccountingReportLayout>
  );
}
