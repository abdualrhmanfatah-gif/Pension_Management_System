import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { fiscalYearsApi,reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import {
  altRowFill,
  autoFitColumns,
  buildReportFooter,
  buildReportHeader,
  createWorkbook,
  exportWorkbook,
  styleDataCell,
} from "@/components/domain/print/excel.utils";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import { cn } from "@/lib/utils";
import type { FiscalYearSummaryDto } from "@/types/accounting";

interface IncomeStatementReportProps {
  fiscalYearId?: number;
  hideInternalControls?: boolean;
}

export function IncomeStatementReport({ fiscalYearId, hideInternalControls }: IncomeStatementReportProps) {
  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const [selectedYearId, setSelectedYearId] = useState<number | undefined>(fiscalYearId);

  useEffect(() => {
    if (fiscalYearId) {
      setSelectedYearId(fiscalYearId);
    } else if (fiscalYears && fiscalYears.length > 0 && !selectedYearId) {
      const currentYear = new Date().getFullYear();
      const match = fiscalYears.find((y: FiscalYearSummaryDto) => y.yearName === String(currentYear));
      setSelectedYearId(match?.fiscalYearID ?? fiscalYears[fiscalYears.length - 1].fiscalYearID);
    }
  }, [fiscalYears, fiscalYearId]);

  const { data, isLoading, error } = useQuery({
    queryKey: ["reports", "income-statement", selectedYearId],
    queryFn: () => reportsApi.incomeStatement(selectedYearId!),
    enabled: !!selectedYearId,
  });

  const handleExportExcel = useCallback(async () => {
    if (!data) return;
    const { ws } = createWorkbook("قائمة الدخل");
    autoFitColumns(ws, { 1: 15, 2: 35, 3: 20 });
    const dataStartRow = await buildReportHeader(ws, "قائمة الدخل", 3, [
      { label: "السنة المالية", value: String(selectedYearId) },
    ]);

    let row = dataStartRow;
    const writeSection = (title: string, lines: { accountName: string; amount: number }[], total: number) => {
      ws.getRow(row).getCell(1).value = title;
      styleDataCell(ws.getRow(row).getCell(1), { bold: true });
      row++;
      lines.forEach((line, i) => {
        ws.getRow(row).getCell(2).value = line.accountName;
        ws.getRow(row).getCell(3).value = line.amount;
        styleDataCell(ws.getRow(row).getCell(3), { numFmt: "#,##0" });
        altRowFill(ws.getRow(row).getCell(1), i);
        altRowFill(ws.getRow(row).getCell(2), i);
        altRowFill(ws.getRow(row).getCell(3), i);
        row++;
      });
      ws.getRow(row).getCell(2).value = `إجمالي ${title}`;
      ws.getRow(row).getCell(3).value = total;
      styleDataCell(ws.getRow(row).getCell(2), { bold: true });
      styleDataCell(ws.getRow(row).getCell(3), { numFmt: "#,##0", bold: true });
      row++;
    };

    writeSection("الإيرادات", data.revenues, data.totalRevenue);
    writeSection("المصروفات", data.expenses, data.totalExpenses);
    ws.getRow(row).getCell(1).value = "صافي الدخل";
    ws.getRow(row).getCell(3).value = data.netIncome;
    styleDataCell(ws.getRow(row).getCell(1), { bold: true });
    styleDataCell(ws.getRow(row).getCell(3), { numFmt: "#,##0", bold: true });
    buildReportFooter(ws, 3);
    await exportWorkbook(ws.workbook, `قائمة_الدخل_${selectedYearId}`);
  }, [data, selectedYearId]);

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (selectedYearId) params.set("fiscalYearId", String(selectedYearId));
    window.open(`/reports/income-statement/print?${params.toString()}`, "_blank");
  }, [selectedYearId]);

  const renderLines = (lines: { accountCode: string; accountName: string; amount: number; accountLevel: number }[]) => {
    return lines.map((line, i) => (
      <div key={i} className={cn(
        "flex justify-between px-3 py-1.5 border-t border-border/30 text-sm",
        line.accountLevel <= 1 && "font-medium bg-muted/20",
        line.accountLevel === 2 && "pr-6",
        line.accountLevel >= 3 && "pr-10 text-xs"
      )}>
        <span>{line.accountName}</span>
        <span className="font-mono">{formatCurrency(line.amount)}</span>
      </div>
    ));
  };

  const customKPIs = data ? [
    { label: 'إجمالي الإيرادات', value: formatCurrency(data.totalRevenue), color: 'text-green-600' },
    { label: 'إجمالي المصروفات', value: formatCurrency(data.totalExpenses), color: 'text-red-600' },
    { label: 'صافي الدخل', value: formatCurrency(data.netIncome), color: data.netIncome >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'النسبة', value: data.totalRevenue > 0 ? `${((data.netIncome / data.totalRevenue) * 100).toFixed(1)}%` : '0%', color: 'text-primary' },
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
        title="ملخص قائمة الدخل"
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

  return (
    <AccountingReportLayout
      title="قائمة الدخل"
      description="عرض الإيرادات والمصروفات وصافي الدخل"
      isLoading={isLoading}
      customKPIs={customKPIs}
      chartContent={chartContent}
      onExport={hideInternalControls ? undefined : (data ? handleExportExcel : undefined)}
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !data ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر السنة لعرض قائمة الدخل</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <div className="border rounded-xl overflow-hidden">
            <div className="bg-green-50 p-3 font-medium text-green-800">الإيرادات</div>
            {renderLines(data.revenues)}
            <div className="flex justify-between p-3 bg-green-50/50 font-medium border-t">
              <span>إجمالي الإيرادات</span>
              <span className="font-mono">{formatCurrency(data.totalRevenue)}</span>
            </div>
          </div>

          <div className="border rounded-xl overflow-hidden">
            <div className="bg-red-50 p-3 font-medium text-red-800">المصروفات</div>
            {renderLines(data.expenses)}
            <div className="flex justify-between p-3 bg-red-50/50 font-medium border-t">
              <span>إجمالي المصروفات</span>
              <span className="font-mono">{formatCurrency(data.totalExpenses)}</span>
            </div>
          </div>

          <div className="lg:col-span-2">
            <div className={cn(
              "p-4 rounded-xl text-center",
              data.netIncome >= 0 ? "bg-green-50" : "bg-red-50"
            )}>
              <span className="text-sm text-muted-foreground">صافي الدخل</span>
              <p className={cn("text-2xl font-bold mt-1", data.netIncome >= 0 ? "text-green-700" : "text-red-700")}>
                {formatCurrency(data.netIncome)}
              </p>
              <p className="text-xs text-muted-foreground mt-1">
                من {new Date(data.fromDate).toLocaleDateString("ar-SA")} إلى {new Date(data.toDate).toLocaleDateString("ar-SA")}
              </p>
            </div>
          </div>
        </div>
      )}
    </AccountingReportLayout>
  );
}
