import { useQuery } from "@tanstack/react-query";
import { useState, useCallback, useMemo } from "react";

import { reportsApi } from "@/api/accounting";
import type { BalanceSheetSectionDto } from "@/types/accounting";
import { formatCurrency } from "@/lib/utils";
import { Card, Heading, Text } from "@/components/ui";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportFilters } from "@/components/common/ReportFilters";
import { ReportPieChart, ReportBarChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";

interface LiabilitiesReportProps {
  fiscalYearId?: number;
  asOfDate?: string;
  externalFilters?: { toDate?: string };
  hideInternalControls?: boolean;
}

export function LiabilitiesReport({ fiscalYearId, asOfDate: propAsOfDate, externalFilters, hideInternalControls }: LiabilitiesReportProps) {
  const [internalFilters, setInternalFilters] = useState<Record<string, any>>({
    toDate: propAsOfDate || new Date().toISOString().split("T")[0],
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
    window.open(`/reports/liabilities/print?${params.toString()}`, "_blank");
  }, [fiscalYearId, filters.toDate]);

  const classified = useMemo(() => {
    if (!data) return null;
    const liabilities = data.liabilities || [];
    const currentLiabilities = liabilities.filter((l: BalanceSheetSectionDto) => {
      const code = l.accountCode || "";
      return code.startsWith("21") || code.startsWith("22");
    });
    const nonCurrentLiabilities = liabilities.filter((l: BalanceSheetSectionDto) => {
      const code = l.accountCode || "";
      return code.startsWith("23") || code.startsWith("24") || code.startsWith("25");
    });
    const otherLiabilities = liabilities.filter((l: BalanceSheetSectionDto) => {
      const code = l.accountCode || "";
      return !code.startsWith("21") && !code.startsWith("22") && !code.startsWith("23") && !code.startsWith("24") && !code.startsWith("25");
    });
    return {
      currentLiabilities,
      nonCurrentLiabilities,
      otherLiabilities,
      totalCurrent: currentLiabilities.reduce((s: number, l: BalanceSheetSectionDto) => s + l.balance, 0),
      totalNonCurrent: nonCurrentLiabilities.reduce((s: number, l: BalanceSheetSectionDto) => s + l.balance, 0),
      totalOther: otherLiabilities.reduce((s: number, l: BalanceSheetSectionDto) => s + l.balance, 0),
      totalLiabilities: data.totalLiabilities,
    };
  }, [data]);

  const customKPIs = classified ? [
    { label: 'الالتزامات قصيرة الأجل', value: formatCurrency(classified.totalCurrent), color: 'text-amber-600' },
    { label: 'الالتزامات طويلة الأجل', value: formatCurrency(classified.totalNonCurrent), color: 'text-orange-600' },
    { label: 'التزامات أخرى', value: formatCurrency(classified.totalOther), color: 'text-foreground' },
    { label: 'إجمالي الديون', value: formatCurrency(classified.totalLiabilities), color: 'text-red-600' },
  ] : [];

  const chartContent = classified ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportPieChart
        data={[
          { name: 'قصيرة الأجل', value: classified.totalCurrent },
          { name: 'طويلة الأجل', value: classified.totalNonCurrent },
          { name: 'أخرى', value: classified.totalOther },
        ].filter(d => d.value > 0)}
        nameKey="name"
        valueKey="value"
        title="توزيع الديون حسب الأجل"
        height={280}
      />
      <ReportBarChart
        data={[
          { name: 'قصيرة الأجل', value: classified.totalCurrent },
          { name: 'طويلة الأجل', value: classified.totalNonCurrent },
          { name: 'أخرى', value: classified.totalOther },
        ]}
        xKey="name"
        yKeys={[{ key: 'value', color: chartColors.warning, label: 'المبلغ' }]}
        title="مقارنة الديون"
        height={280}
      />
    </div>
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
      title="الديون والالتزامات"
      description="عرض الديون والالتزامات حسب الأجل"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (classified ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !classified ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر تاريخاً لعرض الديون والالتزامات</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-amber-50">
               <Heading level="h3" className="font-bold text-lg text-amber-800">الالتزامات قصيرة الأجل (أقل من سنة)</Heading>
              <p className="text-sm text-amber-600 mt-1">الإجمالي: {formatCurrency(classified.totalCurrent)}</p>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.currentLiabilities.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد التزامات قصيرة الأجل</Text>
              ) : (
                <div className="space-y-2">
                  {classified.currentLiabilities.map((item: BalanceSheetSectionDto) => (
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{item.accountCode}</span>
                        <span className="text-sm mr-2">{item.accountName}</span>
                      </div>
                      <span className="text-sm font-medium">{formatCurrency(item.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>

          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-orange-50">
               <Heading level="h3" className="font-bold text-lg text-orange-800">الالتزامات طويلة الأجل (أكثر من سنة)</Heading>
              <p className="text-sm text-orange-600 mt-1">الإجمالي: {formatCurrency(classified.totalNonCurrent)}</p>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.nonCurrentLiabilities.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد التزامات طويلة الأجل</Text>
              ) : (
                <div className="space-y-2">
                  {classified.nonCurrentLiabilities.map((item: BalanceSheetSectionDto) => (
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{item.accountCode}</span>
                        <span className="text-sm mr-2">{item.accountName}</span>
                      </div>
                      <span className="text-sm font-medium">{formatCurrency(item.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>

          {classified.otherLiabilities.length > 0 && (
            <Card className="lg:col-span-2 border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التزامات أخرى</Heading>
                <p className="text-sm text-muted-foreground mt-1">الإجمالي: {formatCurrency(classified.totalOther)}</p>
              </div>
              <div className="p-4">
                <div className="space-y-2">
                  {classified.otherLiabilities.map((item: BalanceSheetSectionDto) => (
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{item.accountCode}</span>
                        <span className="text-sm mr-2">{item.accountName}</span>
                      </div>
                      <span className="text-sm font-medium">{formatCurrency(item.balance)}</span>
                    </div>
                  ))}
                </div>
              </div>
            </Card>
          )}
        </div>
      )}
    </AccountingReportLayout>
  );
}
