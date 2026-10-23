import { useQuery } from "@tanstack/react-query";
import { useCallback } from "react";

import { budgetApi } from "@/api/budget";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { Badge, Heading } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { BudgetReportDto } from "@/types/budget";

interface BudgetReportViewProps {
  fiscalYearId?: number;
  hideInternalControls?: boolean;
}

export function BudgetReportView({ fiscalYearId, hideInternalControls }: BudgetReportViewProps) {
  const { data, isLoading, error } = useQuery<BudgetReportDto>({
    queryKey: ["reports", "budget-annual", fiscalYearId],
    queryFn: () => budgetApi.getAnnualReport(fiscalYearId!),
    enabled: !!fiscalYearId,
  });

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    window.open(`/reports/budget/print?${params.toString()}`, "_blank");
  }, [fiscalYearId]);


  const customKPIs = data ? [
    { label: 'المخطط', value: formatCurrency(data.totalPlanned), color: 'primary' },
    { label: 'الفعلي', value: formatCurrency(data.totalActual), color: 'default' },
    { label: 'الفرق', value: formatCurrency(Math.abs(data.totalVariance)), color: data.totalVariance >= 0 ? 'text-red-600' : 'text-green-600' },
    { label: 'نسبة الانحراف', value: `${Math.abs(data.totalVariancePercentage)}%`, color: 'primary' },
  ] : [];

  const chartContent = data ? (
    <ReportBarChart
      data={data.items?.slice(0, 8).map((item: any) => ({
        name: item.accountName.length > 15 ? item.accountName.substring(0, 15) + '...' : item.accountName,
        planned: item.plannedAmount,
        actual: item.actualAmount,
      })) || []}
      xKey="name"
      yKeys={[
        { key: 'planned', color: chartColors.primary, label: 'المخطط' },
        { key: 'actual', color: chartColors.success, label: 'الفعلي' },
      ]}
      title="المخطط vs الفعلي"
      height={300}
    />
  ) : undefined;

  return (
    <AccountingReportLayout
      title="تقرير الموازنة"
      description="مقارنة المخطط بالفعلي للسنة المالية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل تقرير الموازنة</div>
      ) : !data ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر السنة المالية لعرض تقرير الموازنة</div>
      ) : (
        <div className="border rounded-xl overflow-hidden">
          <div className="p-4 bg-muted/30">
            <Heading level="h3" className="font-bold text-lg">تقرير الموازنة السنوي - {data.yearName}</Heading>
          </div>
          <table className="w-full text-sm" role="table" aria-label="عرض الموازنة">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-3">رقم الحساب</th>
                <th className="text-end p-3">الحساب</th>
                <th className="text-end p-3">المخطط</th>
                <th className="text-end p-3">الفعلي</th>
                <th className="text-end p-3">الفرق</th>
                <th className="text-end p-3">النسبة</th>
                <th className="text-end p-3">الحالة</th>
              </tr>
            </thead>
            <tbody>
              {data.items?.map((item: any) => (
                <tr key={item.budgetItemID} className="border-t border-border hover:bg-muted/20">
                  <td className="p-3 font-mono text-xs">{item.accountCode}</td>
                  <td className="p-3 font-medium">{item.accountName}</td>
                  <td className="p-3">{formatCurrency(item.plannedAmount)}</td>
                  <td className="p-3">{formatCurrency(item.actualAmount)}</td>
                  <td className={`p-3 font-medium ${item.variance > 0 ? "text-destructive" : item.variance < 0 ? "text-emerald-600 dark:text-emerald-400" : ""}`}>
                    {formatCurrency(Math.abs(item.variance))}
                  </td>
                  <td className="p-3">{Math.abs(item.variancePercentage)}%</td>
                  <td className="p-3">
                    <Badge variant={item.status === "مطابق" ? "success" : item.status === "تجاوز" ? "destructive" : "secondary"}>
                      {item.status}
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-muted/50 font-bold">
              <tr>
                <td className="p-3"></td>
                <td className="p-3">الإجمالي</td>
                <td className="p-3">{formatCurrency(data.totalPlanned)}</td>
                <td className="p-3">{formatCurrency(data.totalActual)}</td>
                <td className={`p-3 ${data.totalVariance > 0 ? "text-destructive" : "text-emerald-600 dark:text-emerald-400"}`}>
                  {formatCurrency(Math.abs(data.totalVariance))}
                </td>
                <td className="p-3">{Math.abs(data.totalVariancePercentage)}%</td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>
      )}
    </AccountingReportLayout>
  );
}
