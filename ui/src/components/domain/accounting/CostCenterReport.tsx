import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { costCentersApi, fiscalYearsApi,reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { Card, Heading, Select, SelectContent, SelectItem, SelectTrigger, SelectValue,Text } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { CostCenterDto,FiscalYearSummaryDto, TrialBalanceItemDto } from "@/types/accounting";

interface CostCenterReportProps {
  fiscalYearId?: number;
  hideInternalControls?: boolean;
}

export function CostCenterReport({ fiscalYearId, hideInternalControls }: CostCenterReportProps) {
  const [selectedYearId, setSelectedYearId] = useState<number | undefined>(fiscalYearId);

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: costCenters } = useQuery({
    queryKey: ["cost-centers"],
    queryFn: () => costCentersApi.getAll(),
  });

  const { data: trialBalance, isLoading, error } = useQuery({
    queryKey: ["reports", "trial-balance", selectedYearId],
    queryFn: () => reportsApi.trialBalance({ fiscalYearId: selectedYearId }),
    enabled: !!selectedYearId,
  });

  useEffect(() => {
    if (fiscalYearId) setSelectedYearId(fiscalYearId);
  }, [fiscalYearId]);

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (selectedYearId) params.set("fiscalYearId", String(selectedYearId));
    window.open(`/reports/cost-centers/print?${params.toString()}`, "_blank");
  }, [selectedYearId]);

  const selectedYear = fiscalYears?.find((y: FiscalYearSummaryDto) => y.fiscalYearID === selectedYearId);

  const accounts = trialBalance || [];
  const revenueAccounts = accounts.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("4") || a.accountCode.startsWith("5"));
  const expenseAccounts = accounts.filter((a: TrialBalanceItemDto) => a.accountCode.startsWith("6") || a.accountCode.startsWith("7"));

  const totalRevenue = revenueAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0);
  const totalExpenses = expenseAccounts.reduce((s: number, a: TrialBalanceItemDto) => s + a.balance, 0);

  const customKPIs = [
    { label: 'عدد مراكز التكلفة', value: costCenters?.length || 0, color: 'text-foreground' },
    { label: 'إجمالي الإيرادات', value: formatCurrency(totalRevenue), color: 'text-green-600' },
    { label: 'إجمالي المصروفات', value: formatCurrency(totalExpenses), color: 'text-red-600' },
    { label: 'السنة المالية', value: selectedYear?.yearName || '', color: 'text-primary' },
  ];

  const chartContent = costCenters && costCenters.length > 0 ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportPieChart
        data={costCenters.map((cc: CostCenterDto) => ({
          name: cc.name,
          value: 1,
        }))}
        nameKey="name"
        valueKey="value"
        title="توزيع مراكز التكلفة"
        height={280}
      />
      <ReportBarChart
        data={[
          { name: 'الإيرادات', value: totalRevenue },
          { name: 'المصروفات', value: totalExpenses },
        ]}
        xKey="name"
        yKeys={[{ key: 'value', color: chartColors.primary, label: 'المبلغ' }]}
        title="مقارنة الإيرادات والمصروفات"
        height={280}
      />
    </div>
  ) : undefined;

  const filterBar = hideInternalControls ? undefined : (
    <Card className="p-4 flex flex-wrap gap-4 items-end mb-6">
      <div className="w-full sm:w-64">
        <label className="label-filter">السنة المالية</label>
        <Select
          value={selectedYearId?.toString() ?? ''}
          onValueChange={(val) => setSelectedYearId(Number(val))}
        >
          <SelectTrigger>
            <SelectValue placeholder="اختر السنة المالية" />
          </SelectTrigger>
          <SelectContent>
            {fiscalYears?.map((y: FiscalYearSummaryDto) => (
              <SelectItem key={y.fiscalYearID} value={y.fiscalYearID.toString()}>{y.yearName}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </Card>
  );

  return (
    <AccountingReportLayout
      title="مراكز التكلفة"
      description="عرض مراكز التكلفة والإيرادات والمصروفات"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (trialBalance && trialBalance.length > 0 ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : (
        <>
          {costCenters && costCenters.length > 0 && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
              {costCenters.map((cc: CostCenterDto) => (
                <Card key={cc.costCenterID} className="p-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <Text variant="description">{cc.name}</Text>
                      <Text variant="body" className="font-bold">{cc.code}</Text>
                    </div>
                    <div className={`w-3 h-3 rounded-full ${cc.isActive ? "bg-green-500" : "bg-red-500"}`} />
                  </div>
                </Card>
              ))}
            </div>
          )}

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-green-50">
                 <Heading level="h3" className="font-bold text-lg text-green-800">الإيرادات - {selectedYear?.yearName}</Heading>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {revenueAccounts.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد إيرادات</Text>
                ) : (
                  <div className="space-y-2">
                    {revenueAccounts.map((account: TrialBalanceItemDto) => (
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
                 <Heading level="h3" className="font-bold text-lg text-red-800">المصروفات - {selectedYear?.yearName}</Heading>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {expenseAccounts.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد مصروفات</Text>
                ) : (
                  <div className="space-y-2">
                    {expenseAccounts.map((account: TrialBalanceItemDto) => (
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
          </div>
        </>
      )}
    </AccountingReportLayout>
  );
}
