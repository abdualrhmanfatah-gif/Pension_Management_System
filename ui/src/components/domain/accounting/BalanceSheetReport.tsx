import { useQuery } from "@tanstack/react-query";
import { ChevronDown, ChevronLeft } from "lucide-react";
import { useCallback,useState } from "react";

import { reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import { Button } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import { cn } from "@/lib/utils";
import type { BalanceSheetSectionDto } from "@/types/accounting";

interface BalanceSheetReportProps {
  fiscalYearId?: number;
  asOfDate?: string;
  externalFilters?: { toDate?: string };
  hideInternalControls?: boolean;
}

function SectionRow({ section, depth }: { section: BalanceSheetSectionDto; depth: number }) {
  const [expanded, setExpanded] = useState(depth < 1);
  const hasChildren = section.children && section.children.length > 0;

  return (
    <div>
      <div className={cn(
        "flex items-center justify-between px-3 py-1.5 border-t border-border text-sm hover:bg-muted/20",
        depth === 0 && "font-medium bg-muted/20",
        depth === 1 && "pr-8",
        depth >= 2 && "pr-12 text-xs"
      )}>
        <div className="flex items-center gap-2">
          {hasChildren ? (
            <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => setExpanded(!expanded)}>
              {expanded ? <ChevronDown className="w-3 h-3" /> : <ChevronLeft className="w-3 h-3" />}
            </Button>
          ) : <span className="w-4" />}
          <span className="font-mono text-xs text-muted-foreground">{section.accountCode}</span>
          <span>{section.accountName}</span>
        </div>
        <span className="font-mono text-sm text-foreground font-bold">{formatCurrency(section.balance)}</span>
      </div>
      {hasChildren && expanded && section.children.map((child) => (
        <SectionRow key={child.accountID} section={child} depth={depth + 1} />
      ))}
    </div>
  );
}

export function BalanceSheetReport({ fiscalYearId, asOfDate, externalFilters, hideInternalControls }: BalanceSheetReportProps) {
  const [internalFilters, setInternalFilters] = useState<Record<string, any>>({
    toDate: asOfDate || new Date().toISOString().split("T")[0],
  });

  const filters = {
    ...internalFilters,
    ...(externalFilters?.toDate !== undefined ? { toDate: externalFilters.toDate } : {}),
  };

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "balance-sheet", filters.toDate, fiscalYearId],
    queryFn: () => reportsApi.balanceSheet(filters.toDate!, fiscalYearId),
    enabled: !!filters.toDate,
  });

  const handleFilterChange = useCallback((key: string, value: any) => {
    setInternalFilters(prev => ({ ...prev, [key]: value }));
  }, []);


  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    if (filters.toDate) params.set("date", filters.toDate);
    window.open(`/reports/balance-sheet/print?${params.toString()}`, "_blank");
  }, [fiscalYearId, filters.toDate]);

  const customKPIs = data ? [
    { label: 'إجمالي الأصول', value: formatCurrency(data.totalAssets), color: 'info' },
    { label: 'إجمالي الخصوم', value: formatCurrency(data.totalLiabilities), color: 'warning' },
    { label: 'حقوق الملكية', value: formatCurrency(data.totalEquity), color: 'success' },
    { label: 'الحالة', value: data.isBalanced ? 'متوازنة' : 'غير متوازنة', color: data.isBalanced ? 'text-success' : 'text-danger' },
  ] : [];

  const chartContent = data ? (
    <ReportBarChart
      data={[
        { name: 'الأصول', value: data.totalAssets, fill: chartColors.primary },
        { name: 'الخصوم', value: data.totalLiabilities, fill: chartColors.warning },
        { name: 'حقوق الملكية', value: data.totalEquity, fill: chartColors.success },
      ]}
      xKey="name"
      yKeys={[{ key: 'value', color: chartColors.primary, label: 'المبلغ' }]}
      title="الميزانية العمومية - ملخص"
      height={280}
    />
  ) : undefined;

  const filterBar = hideInternalControls ? undefined : (
    <ReportFilters
      filters={['toDate']}
      values={filters}
      onChange={handleFilterChange}
      onApply={() => refetch()}
      showApplyButton={false}
    />
  );

  return (
    <AccountingReportLayout
      title="الميزانية العمومية"
      description="عرض الأصول والخصوم وحقوق الملكية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !data ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر تاريخاً لعرض الميزانية</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <div className="border rounded-xl overflow-hidden">
            <div className="bg-info/10 p-3 font-medium text-info">الأصول</div>
            {data.assets.map((a: BalanceSheetSectionDto) => <SectionRow key={a.accountID} section={a} depth={0} />)}
            <div className="flex justify-between p-3 bg-info/10 font-medium border-t">
              <span>إجمالي الأصول</span>
              <span className="font-mono text-foreground">{formatCurrency(data.totalAssets)}</span>
            </div>
          </div>

          <div className="space-y-4">
            <div className="border rounded-xl overflow-hidden">
              <div className="bg-warning/10 p-3 font-medium text-warning">الخصوم</div>
              {data.liabilities.map((l: BalanceSheetSectionDto) => <SectionRow key={l.accountID} section={l} depth={0} />)}
              <div className="flex justify-between p-3 bg-warning/10 font-medium border-t">
                <span>إجمالي الخصوم</span>
                <span className="font-mono text-foreground">{formatCurrency(data.totalLiabilities)}</span>
              </div>
            </div>

            <div className="border rounded-xl overflow-hidden">
              <div className="bg-success/10 p-3 font-medium text-success">حقوق الملكية</div>
              {data.equity.map((e: BalanceSheetSectionDto) => <SectionRow key={e.accountID} section={e} depth={0} />)}
              <div className="flex justify-between p-3 bg-success/10 font-medium border-t">
                <span>إجمالي حقوق الملكية</span>
                <span className="font-mono">{formatCurrency(data.totalEquity)}</span>
              </div>
            </div>
          </div>

          <div className="lg:col-span-2">
            <div className={cn(
              "p-3 rounded-xl text-center font-medium",
              data.isBalanced ? "bg-success/10 text-success" : "bg-destructive/10 text-destructive"
            )}>
              {data.isBalanced
                ? "الميزانية متوازنة ✓ — الأصول = الخصوم + حقوق الملكية"
                : "الميزانية غير متوازنة ✗ — يوجد فرق في التسجيل"
              }
            </div>
          </div>
        </div>
      )}
    </AccountingReportLayout>
  );
}
