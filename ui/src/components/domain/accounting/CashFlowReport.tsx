import { useQuery } from "@tanstack/react-query";
import { ArrowDown, ArrowUp } from "lucide-react";

import { reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { Card, Heading, Text } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { CashFlowItemDto } from "@/types/accounting";

interface CashFlowReportProps {
  fiscalYearId?: number;
  hideInternalControls?: boolean;
}

export function CashFlowReport({ fiscalYearId, hideInternalControls }: CashFlowReportProps) {
  const { data: report, isLoading, error } = useQuery({
    queryKey: ["reports", "cash-flow", fiscalYearId],
    queryFn: () => reportsApi.cashFlow(fiscalYearId!),
    enabled: !!fiscalYearId,
  });

  const handlePrint = () => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    window.open(`/reports/cash-flow/print?${params.toString()}`, "_blank");
  };


  const customKPIs = report ? [
    { label: 'التدفقات التشغيلية', value: formatCurrency(report.netOperatingCashFlow), color: report.netOperatingCashFlow >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'التدفقات الاستثمارية', value: formatCurrency(report.netInvestingCashFlow), color: report.netInvestingCashFlow >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'التدفقات التمويلية', value: formatCurrency(report.netFinancingCashFlow), color: report.netFinancingCashFlow >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'صافي التدفق النقدي', value: formatCurrency(report.netCashFlow), color: report.netCashFlow >= 0 ? 'text-green-600' : 'text-red-600' },
  ] : [];

  const chartContent = report ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportBarChart
        data={[
          { name: 'تشغيلي', value: report.netOperatingCashFlow },
          { name: 'استثماري', value: report.netInvestingCashFlow },
          { name: 'تمويلي', value: report.netFinancingCashFlow },
        ]}
        xKey="name"
        yKeys={[{ key: 'value', color: chartColors.primary, label: 'صافي التدفق' }]}
        title="التدفقات النقدية حسب النوع"
        height={280}
      />
      <ReportPieChart
        data={[
          { name: 'وارد تشغيلي', value: report.operatingCashInflow },
          { name: 'صادر تشغيلي', value: report.operatingCashOutflow },
          { name: 'وارد استثماري', value: report.investingCashInflow },
          { name: 'صادر استثماري', value: report.investingCashOutflow },
        ]}
        nameKey="name"
        valueKey="value"
        title="توزيع التدفقات"
        height={280}
      />
    </div>
  ) : undefined;

  return (
    <AccountingReportLayout
      title="التدفقات النقدية"
      description="عرض التدفقات النقدية التشغيلية والاستثمارية والتمويلية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (report ? handlePrint : undefined)}    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !report ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر السنة المالية لعرض التقرير</div>
      ) : (
        <>
          <Card className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <Text variant="description">صافي التدفق النقدي</Text>
                <Text variant="body" className={`md:text-3xl font-bold ${report.netCashFlow >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                  {formatCurrency(report.netCashFlow)}
                </Text>
              </div>
              <div className="text-left">
                <Text variant="description">الرصيد النهائي</Text>
                <Text variant="body" className="font-bold">{formatCurrency(report.endingCashBalance)}</Text>
              </div>
            </div>
          </Card>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التدفقات التشغيلية</Heading>
                <div className="flex gap-4 text-sm mt-2">
                  <span className="text-green-600">وارد: {formatCurrency(report.operatingCashInflow)}</span>
                  <span className="text-red-600">صادر: {formatCurrency(report.operatingCashOutflow)}</span>
                </div>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {report.operatingItems.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد حركات</Text>
                ) : (
                  <div className="space-y-2">
                    {report.operatingItems.map((item: CashFlowItemDto, i) => (
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border last:border-0">
                        <div className="flex items-center gap-2">
                          {item.type === "Inflow" ? (
                            <ArrowDown className="w-4 h-4 text-green-600" />
                          ) : (
                            <ArrowUp className="w-4 h-4 text-red-600" />
                          )}
                          <span className="text-sm">{item.accountName}</span>
                        </div>
                        <span className={`text-sm font-medium ${item.type === "Inflow" ? "text-green-600" : "text-red-600"}`}>
                          {formatCurrency(item.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>

            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التدفقات الاستثمارية</Heading>
                <div className="flex gap-4 text-sm mt-2">
                  <span className="text-green-600">وارد: {formatCurrency(report.investingCashInflow)}</span>
                  <span className="text-red-600">صادر: {formatCurrency(report.investingCashOutflow)}</span>
                </div>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {report.investingItems.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد حركات</Text>
                ) : (
                  <div className="space-y-2">
                    {report.investingItems.map((item: CashFlowItemDto, i) => (
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border last:border-0">
                        <div className="flex items-center gap-2">
                          {item.type === "Inflow" ? (
                            <ArrowDown className="w-4 h-4 text-green-600" />
                          ) : (
                            <ArrowUp className="w-4 h-4 text-red-600" />
                          )}
                          <span className="text-sm">{item.accountName}</span>
                        </div>
                        <span className={`text-sm font-medium ${item.type === "Inflow" ? "text-green-600" : "text-red-600"}`}>
                          {formatCurrency(item.amount)}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </Card>

            <Card className="border-none rounded-3xl overflow-hidden">
              <div className="p-4 bg-muted/30">
                <Heading level="h3" className="font-bold text-lg">التدفقات التمويلية</Heading>
                <div className="flex gap-4 text-sm mt-2">
                  <span className="text-green-600">وارد: {formatCurrency(report.financingCashInflow)}</span>
                  <span className="text-red-600">صادر: {formatCurrency(report.financingCashOutflow)}</span>
                </div>
              </div>
              <div className="p-4 max-h-64 overflow-y-auto">
                {report.financingItems.length === 0 ? (
                  <Text variant="description" className="text-center">لا توجد حركات</Text>
                ) : (
                  <div className="space-y-2">
                    {report.financingItems.map((item: CashFlowItemDto, i) => (
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border last:border-0">
                        <div className="flex items-center gap-2">
                          {item.type === "Inflow" ? (
                            <ArrowDown className="w-4 h-4 text-green-600" />
                          ) : (
                            <ArrowUp className="w-4 h-4 text-red-600" />
                          )}
                          <span className="text-sm">{item.accountName}</span>
                        </div>
                        <span className={`text-sm font-medium ${item.type === "Inflow" ? "text-green-600" : "text-red-600"}`}>
                          {formatCurrency(item.amount)}
                        </span>
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
