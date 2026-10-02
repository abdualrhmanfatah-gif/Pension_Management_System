import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { fiscalYearsApi, quartersApi,reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { Card, Heading, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { FiscalYearSummaryDto } from "@/types/accounting";

interface CumulativePnLReportProps {
  fiscalYearId?: number;
  hideInternalControls?: boolean;
}

export function CumulativePnLReport({ fiscalYearId, hideInternalControls }: CumulativePnLReportProps) {
  const [selectedYearId, setSelectedYearId] = useState<number | undefined>(fiscalYearId);

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: quarters } = useQuery({
    queryKey: ["quarters", selectedYearId],
    queryFn: () => quartersApi.getAll(selectedYearId!),
    enabled: !!selectedYearId,
  });

  useEffect(() => {
    if (fiscalYearId) setSelectedYearId(fiscalYearId);
  }, [fiscalYearId]);

  const { data: q1Data, isLoading: l1 } = useQuery({
    queryKey: ["reports", "income-statement", selectedYearId, quarters?.[0]?.quarterID],
    queryFn: () => reportsApi.incomeStatement(selectedYearId!, quarters?.[0]?.quarterID),
    enabled: !!selectedYearId && !!quarters?.[0]?.quarterID,
  });

  const { data: q2Data, isLoading: l2 } = useQuery({
    queryKey: ["reports", "income-statement", selectedYearId, quarters?.[1]?.quarterID],
    queryFn: () => reportsApi.incomeStatement(selectedYearId!, quarters?.[1]?.quarterID),
    enabled: !!selectedYearId && !!quarters?.[1]?.quarterID,
  });

  const { data: q3Data, isLoading: l3 } = useQuery({
    queryKey: ["reports", "income-statement", selectedYearId, quarters?.[2]?.quarterID],
    queryFn: () => reportsApi.incomeStatement(selectedYearId!, quarters?.[2]?.quarterID),
    enabled: !!selectedYearId && !!quarters?.[2]?.quarterID,
  });

  const { data: q4Data, isLoading: l4 } = useQuery({
    queryKey: ["reports", "income-statement", selectedYearId, quarters?.[3]?.quarterID],
    queryFn: () => reportsApi.incomeStatement(selectedYearId!, quarters?.[3]?.quarterID),
    enabled: !!selectedYearId && !!quarters?.[3]?.quarterID,
  });

  const isLoading = l1 || l2 || l3 || l4;

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (selectedYearId) params.set("fiscalYearId", String(selectedYearId));
    window.open(`/reports/cumulative-pnl/print?${params.toString()}`, "_blank");
  }, [selectedYearId]);

  const quartersData = [q1Data, q2Data, q3Data, q4Data].filter(Boolean);

  let cumulativeRevenue = 0;
  let cumulativeExpenses = 0;
  const cumulativeData = quartersData.map((q, i) => {
    cumulativeRevenue += q?.totalRevenue ?? 0;
    cumulativeExpenses += q?.totalExpenses ?? 0;
    return {
      quarter: `الفصل ${i + 1}`,
      quarterName: quarters?.[i]?.quarterName ?? `Q${i + 1}`,
      revenue: q?.totalRevenue ?? 0,
      expenses: q?.totalExpenses ?? 0,
      netIncome: q?.netIncome ?? 0,
      cumulativeRevenue,
      cumulativeExpenses,
      cumulativeNetIncome: cumulativeRevenue - cumulativeExpenses,
    };
  });

  const selectedYear = fiscalYears?.find((y: FiscalYearSummaryDto) => y.fiscalYearID === selectedYearId);

  const customKPIs = [
    { label: 'الإيرادات التراكمية', value: formatCurrency(cumulativeRevenue), color: 'text-green-600' },
    { label: 'المصروفات التراكمية', value: formatCurrency(cumulativeExpenses), color: 'text-red-600' },
    { label: 'صافي الدخل التراكمي', value: formatCurrency(cumulativeRevenue - cumulativeExpenses), color: (cumulativeRevenue - cumulativeExpenses) >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'السنة المالية', value: selectedYear?.yearName || '', color: 'text-primary' },
  ];

  const chartContent = cumulativeData.length > 0 ? (
    <ReportBarChart
      data={cumulativeData.map(d => ({
        name: d.quarterName,
        revenue: d.revenue,
        expenses: d.expenses,
        netIncome: d.netIncome,
      }))}
      xKey="name"
      yKeys={[
        { key: 'revenue', color: chartColors.success, label: 'الإيرادات' },
        { key: 'expenses', color: chartColors.danger, label: 'المصروفات' },
        { key: 'netIncome', color: chartColors.primary, label: 'صافي الدخل' },
      ]}
      title="مقارنة الفصول"
      height={300}
    />
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
      title="الأرباح والخسائر المتراكم"
      description="عرض الأرباح والخسائر التراكمية عبر الفصول"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (cumulativeData.length > 0 ? handlePrint : undefined)}
    >
      {cumulativeData.length === 0 ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر السنة المالية لعرض التقرير</div>
      ) : (
        <Card className="border-none rounded-3xl overflow-hidden">
          <div className="p-4 bg-muted/30">
            <Heading level="h3" className="font-bold text-lg">قائمة الأرباح والخسائر التراكمية - {selectedYear?.yearName}</Heading>
          </div>
          <table className="w-full text-sm" role="table" aria-label="قائمة الأرباح والخسائر التراكمية">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-3">الفترة</th>
                <th className="text-end p-3">الإيرادات</th>
                <th className="text-end p-3">المصروفات</th>
                <th className="text-end p-3">صافي الدخل</th>
                <th className="text-end p-3">إيرادات تراكمية</th>
                <th className="text-end p-3">مصروفات تراكمية</th>
                <th className="text-end p-3">صافي الدخل التراكمي</th>
              </tr>
            </thead>
            <tbody>
              {cumulativeData.map((row, i) => (
                <tr key={i} className="border-t border-border/50 hover:bg-muted/20">
                  <td className="p-3 font-medium">{row.quarterName}</td>
                  <td className="p-3 text-emerald-600 dark:text-emerald-400">{formatCurrency(row.revenue)}</td>
                  <td className="p-3 text-destructive">{formatCurrency(row.expenses)}</td>
                  <td className={`p-3 font-medium ${row.netIncome >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                    {formatCurrency(row.netIncome)}
                  </td>
                  <td className="p-3 text-emerald-600 dark:text-emerald-400">{formatCurrency(row.cumulativeRevenue)}</td>
                  <td className="p-3 text-destructive">{formatCurrency(row.cumulativeExpenses)}</td>
                  <td className={`p-3 font-bold ${row.cumulativeNetIncome >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                    {formatCurrency(row.cumulativeNetIncome)}
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-muted/50 font-bold">
              <tr>
                <td className="p-3">الإجمالي السنوي</td>
                <td className="p-3 text-emerald-600 dark:text-emerald-400">{formatCurrency(cumulativeRevenue)}</td>
                <td className="p-3 text-destructive">{formatCurrency(cumulativeExpenses)}</td>
                <td className={`p-3 ${cumulativeRevenue - cumulativeExpenses >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {formatCurrency(cumulativeRevenue - cumulativeExpenses)}
                </td>
                <td colSpan={3}></td>
              </tr>
            </tfoot>
          </table>
        </Card>
      )}
    </AccountingReportLayout>
  );
}
