<<<<<<< HEAD
﻿import { useQuery } from "@tanstack/react-query";
import { useCallback } from "react";

import { budgetApi } from "@/api/budget";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { Badge, Heading } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { BudgetReportDto } from "@/types/budget";
=======
import { useQuery } from "@tanstack/react-query";
import { useCallback } from "react";

import { budgetApi } from "@/api/budget";
import type { BudgetReportDto } from "@/types/budget";
import { formatCurrency } from "@/lib/utils";
import { Badge, Heading } from "@/components/ui";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";
import {
  createWorkbook, buildReportHeader, buildTableHeader, styleDataCell, altRowFill,
  buildTotalsRow, buildReportFooter, exportWorkbook, autoFitColumns,
} from "@/components/domain/print/excel.utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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

<<<<<<< HEAD

  const customKPIs = data ? [
    { label: 'المخطط', value: formatCurrency(data.totalPlanned), color: 'primary' },
    { label: 'الفعلي', value: formatCurrency(data.totalActual), color: 'default' },
    { label: 'الفرق', value: formatCurrency(Math.abs(data.totalVariance)), color: data.totalVariance >= 0 ? 'text-red-600' : 'text-green-600' },
    { label: 'نسبة الانحراف', value: `${Math.abs(data.totalVariancePercentage)}%`, color: 'primary' },
=======
  const handleExportExcel = useCallback(async () => {
    if (!data) return;
    const { ws } = createWorkbook("تقرير الموازنة");
    autoFitColumns(ws, { 1: 15, 2: 25, 3: 18, 4: 18, 5: 18, 6: 12, 7: 12 });
    const dataStartRow = await buildReportHeader(ws, "تقرير الموازنة السنوي", 7, [
      { label: "السنة المالية", value: data.yearName },
    ]);

    buildTableHeader(ws, dataStartRow, ["رقم الحساب", "الحساب", "المخطط", "الفعلي", "الفرق", "النسبة", "الحالة"]);

    data.items?.forEach((item: any, i: number) => {
      const r = ws.getRow(dataStartRow + 1 + i);
      r.getCell(1).value = item.accountCode;
      r.getCell(2).value = item.accountName;
      r.getCell(3).value = item.plannedAmount;
      r.getCell(4).value = item.actualAmount;
      r.getCell(5).value = item.variance;
      r.getCell(6).value = `${Math.abs(item.variancePercentage)}%`;
      r.getCell(7).value = item.status;

      [3, 4, 5].forEach(c => styleDataCell(r.getCell(c), { numFmt: "#,##0" }));
      for (let c = 1; c <= 7; c++) altRowFill(r.getCell(c), i);
    });

    const totalRowNum = dataStartRow + 1 + (data.items?.length || 0);
    buildTotalsRow(ws, totalRowNum, [
      { value: "" },
      { value: "الإجمالي" },
      { value: data.totalPlanned, numFmt: "#,##0" },
      { value: data.totalActual, numFmt: "#,##0" },
      { value: data.totalVariance, numFmt: "#,##0" },
      { value: `${Math.abs(data.totalVariancePercentage)}%` },
      { value: "" },
    ]);

    buildReportFooter(ws, 7);
    await exportWorkbook(ws.workbook, `تقرير_الموازنة_${data.yearName}`);
  }, [data]);

  const customKPIs = data ? [
    { label: 'المخطط', value: formatCurrency(data.totalPlanned), color: 'text-primary' },
    { label: 'الفعلي', value: formatCurrency(data.totalActual), color: 'text-foreground' },
    { label: 'الفرق', value: formatCurrency(Math.abs(data.totalVariance)), color: data.totalVariance >= 0 ? 'text-red-600' : 'text-green-600' },
    { label: 'نسبة الانحراف', value: `${Math.abs(data.totalVariancePercentage)}%`, color: 'text-primary' },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}    >
=======
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
      onExport={hideInternalControls ? undefined : (data ? handleExportExcel : undefined)}
    >
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                <tr key={item.budgetItemID} className="border-t border-border hover:bg-muted/20">
=======
                <tr key={item.budgetItemID} className="border-t border-border/50 hover:bg-muted/20">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
