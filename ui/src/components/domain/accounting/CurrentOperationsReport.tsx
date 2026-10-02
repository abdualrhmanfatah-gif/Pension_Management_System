import { useQuery } from "@tanstack/react-query";
import { useCallback, useEffect, useMemo,useState } from "react";

import { periodsApi,reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { Card, Heading, Text } from "@/components/ui";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { FiscalPeriodDto, TrialBalanceItemDto } from "@/types/accounting";

interface CurrentOperationsReportProps {
  fiscalYearId?: number;
  externalFilters?: { periodId?: number };
  hideInternalControls?: boolean;
}

export function CurrentOperationsReport({ fiscalYearId, externalFilters, hideInternalControls }: CurrentOperationsReportProps) {
  const [internalSelectedPeriodId, setInternalSelectedPeriodId] = useState<number | undefined>();
  const selectedPeriodId = externalFilters?.periodId !== undefined ? externalFilters.periodId : internalSelectedPeriodId;

  const { data: periods } = useQuery({
    queryKey: ["periods", fiscalYearId],
    queryFn: () => periodsApi.getAll(fiscalYearId!),
    enabled: !!fiscalYearId,
  });

  useEffect(() => {
    if (periods && periods.length > 0 && !selectedPeriodId) {
      const activePeriod = periods.find((p: FiscalPeriodDto) => !p.isClosed);
      setInternalSelectedPeriodId(activePeriod?.periodID ?? periods[periods.length - 1].periodID);
    }
  }, [periods]);

  const { data: trialBalance, isLoading, error } = useQuery({
    queryKey: ["reports", "trial-balance", fiscalYearId, selectedPeriodId],
    queryFn: () => reportsApi.trialBalance({ fiscalYearId, periodId: selectedPeriodId }),
    enabled: !!fiscalYearId && !!selectedPeriodId,
  });

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    if (selectedPeriodId) params.set("periodId", String(selectedPeriodId));
    window.open(`/reports/current-operations/print?${params.toString()}`, "_blank");
  }, [fiscalYearId, selectedPeriodId]);

  const selectedPeriod = periods?.find((p: FiscalPeriodDto) => p.periodID === selectedPeriodId);

  const classified = useMemo(() => {
    if (!trialBalance) return null;
    const revenueAccounts = trialBalance.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("4"));
    const expenseAccounts = trialBalance.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("3"));
    const assetAccounts = trialBalance.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("1"));
    const liabilityAccounts = trialBalance.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("2") && !a.accountCode.startsWith("21") && !a.accountCode.startsWith("22"));
    const equityAccounts = trialBalance.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("21") || a.accountCode.startsWith("22"));

    return {
      revenueAccounts,
      expenseAccounts,
      assetAccounts,
      liabilityAccounts,
      equityAccounts,
      totalRevenue: revenueAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0),
      totalExpenses: expenseAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0),
      totalAssets: assetAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0),
      totalLiabilities: liabilityAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0),
      totalEquity: equityAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0),
    };
  }, [trialBalance]);

  const customKPIs = classified ? [
    { label: 'الإيرادات', value: formatCurrency(classified.totalRevenue), color: 'success' },
    { label: 'المصروفات', value: formatCurrency(classified.totalExpenses), color: 'danger' },
    { label: 'صافي الدخل', value: formatCurrency(classified.totalRevenue - classified.totalExpenses), color: (classified.totalRevenue - classified.totalExpenses) >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'الفترة', value: selectedPeriod?.periodName || '', color: 'primary' },
  ] : [];

  const chartContent = classified ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportBarChart
        data={[
          { name: 'الأصول', value: classified.totalAssets },
          { name: 'الخصوم', value: classified.totalLiabilities },
          { name: 'حقوق الملكية', value: classified.totalEquity },
          { name: 'صافي الدخل', value: classified.totalRevenue - classified.totalExpenses },
        ]}
        xKey="name"
        yKeys={[{ key: 'value', color: chartColors.primary, label: 'المبلغ' }]}
        title="ملخص العمليات الجارية"
        height={280}
      />
      <ReportPieChart
        data={[
          { name: 'الإيرادات', value: classified.totalRevenue },
          { name: 'المصروفات', value: classified.totalExpenses },
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
      <div className="w-full sm:w-64">
        <label className="label-filter">الفترة المحاسبية</label>
        <Select
          value={selectedPeriodId != null ? String(selectedPeriodId) : undefined}
          onValueChange={(v) => setInternalSelectedPeriodId(Number(v))}
        >
          <SelectTrigger className="h-10">
            <SelectValue placeholder="اختر الفترة" />
          </SelectTrigger>
          <SelectContent>
            {periods?.map((p: FiscalPeriodDto) => (
              <SelectItem key={p.periodID} value={String(p.periodID)}>
                {p.periodName} {p.isClosed ? "(مغلقة)" : ""}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      {selectedPeriod && (
        <div className="text-sm text-muted-foreground">
          من {new Date(selectedPeriod.startDate).toLocaleDateString("ar-SA")} إلى{" "}
          {new Date(selectedPeriod.endDate).toLocaleDateString("ar-SA")}
        </div>
      )}
    </Card>
  );

  return (
    <AccountingReportLayout
      title="العمليات الجارية"
      description="عرض ملخص العمليات المحاسبية للفترة الجارية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (classified ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !classified ? (
        <div className="text-muted-foreground text-sm p-8 text-center">لا توجد بيانات العمليات الجارية</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-green-50">
               <Heading level="h3" className="font-bold text-lg text-green-800">الإيرادات</Heading>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.revenueAccounts.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد إيرادات</Text>
              ) : (
                <div className="space-y-2">
                  {classified.revenueAccounts.map((account: TrialBalanceItemDto) => (
                    <div key={account.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{account.accountCode}</span>
                        <span className="text-sm mr-2">{account.accountName}</span>
                      </div>
                      <span className="text-sm font-medium text-green-600">{formatCurrency(account.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>

          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-red-50">
               <Heading level="h3" className="font-bold text-lg text-red-800">المصروفات</Heading>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.expenseAccounts.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد مصروفات</Text>
              ) : (
                <div className="space-y-2">
                  {classified.expenseAccounts.map((account: TrialBalanceItemDto) => (
                    <div key={account.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{account.accountCode}</span>
                        <span className="text-sm mr-2">{account.accountName}</span>
                      </div>
                      <span className="text-sm font-medium text-red-600">{formatCurrency(account.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>

          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-blue-50">
               <Heading level="h3" className="font-bold text-lg text-blue-800">الأصول</Heading>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.assetAccounts.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد أصول</Text>
              ) : (
                <div className="space-y-2">
                  {classified.assetAccounts.map((account: TrialBalanceItemDto) => (
                    <div key={account.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{account.accountCode}</span>
                        <span className="text-sm mr-2">{account.accountName}</span>
                      </div>
                      <span className="text-sm font-medium text-blue-600">{formatCurrency(account.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>

          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-purple-50">
               <Heading level="h3" className="font-bold text-lg text-purple-800">الخصوم وحقوق الملكية</Heading>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.liabilityAccounts.length === 0 && classified.equityAccounts.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد خصوم أو حقوق ملكية</Text>
              ) : (
                <div className="space-y-2">
                  {classified.equityAccounts.map((account: TrialBalanceItemDto) => (
                    <div key={account.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{account.accountCode}</span>
                        <span className="text-sm mr-2">{account.accountName}</span>
                        <span className="text-xs text-purple-600 mr-1">(حقوق ملكية)</span>
                      </div>
                      <span className="text-sm font-medium text-purple-600">{formatCurrency(account.balance)}</span>
                    </div>
                  ))}
                  {classified.liabilityAccounts.map((account: TrialBalanceItemDto) => (
                    <div key={account.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{account.accountCode}</span>
                        <span className="text-sm mr-2">{account.accountName}</span>
                      </div>
                      <span className="text-sm font-medium text-purple-600">{formatCurrency(account.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>
        </div>
      )}
    </AccountingReportLayout>
  );
}
