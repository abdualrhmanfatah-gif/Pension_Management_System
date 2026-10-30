<<<<<<< HEAD
﻿import { useQuery } from "@tanstack/react-query";
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
=======
import { useQuery } from "@tanstack/react-query";
import { useState, useEffect, useCallback } from "react";

import { reportsApi, fiscalYearsApi, quartersApi } from "@/api/accounting";
import type { FiscalYearSummaryDto, QuarterDto } from "@/types/accounting";
import { formatCurrency } from "@/lib/utils";
import { cn } from "@/lib/utils";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";
import { Card } from "@/components/ui/Card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import {
  createWorkbook, buildReportHeader, styleDataCell, altRowFill,
  buildReportFooter, exportWorkbook, autoFitColumns,
} from "@/components/domain/print/excel.utils";

interface QuarterlyReportProps {
  fiscalYearId?: number;
  externalFilters?: { quarter?: number };
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
    if (externalFilters?.quarterId && quarters) {
      const match = quarters.find((q: QuarterDto) => q.quarterID === externalFilters.quarterId);
=======
    if (externalFilters?.quarter && quarters) {
      const match = quarters.find((q: QuarterDto) => q.quarterNumber === externalFilters.quarter);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
      if (match) setSelectedQuarterId(match.quarterID);
    } else if (quarters && quarters.length > 0 && !selectedQuarterId) {
      const openQ = quarters.find((q: QuarterDto) => !q.isClosed);
      setSelectedQuarterId(openQ?.quarterID ?? quarters[quarters.length - 1].quarterID);
    }
<<<<<<< HEAD
  }, [externalFilters?.quarterId, quarters, selectedQuarterId]);
=======
  }, [quarters]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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

<<<<<<< HEAD

  const customKPIs = data ? [
    { label: 'إجمالي الإيرادات', value: formatCurrency(data.totalRevenue), color: 'success' },
    { label: 'إجمالي المصروفات', value: formatCurrency(data.totalExpenses), color: 'danger' },
    { label: 'صافي الدخل الفصلي', value: formatCurrency(data.netIncome), color: data.netIncome >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'الفصل', value: selectedQuarter?.quarterName || '', color: 'primary' },
=======
  const handleExportExcel = useCallback(async () => {
    if (!data) return;
    const { ws } = createWorkbook("التقرير الفصلي");
    autoFitColumns(ws, { 1: 30, 2: 20 });
    const dataStartRow = await buildReportHeader(ws, "التقرير الفصلي", 2, [
      { label: "الفصل", value: selectedQuarter?.quarterName || "" },
    ]);

    let row = dataStartRow;
    const writeSection = (title: string, lines: { accountName: string; amount: number }[], total: number) => {
      ws.getRow(row).getCell(1).value = title;
      styleDataCell(ws.getRow(row).getCell(1), { bold: true });
      row++;
      lines.forEach((line, i) => {
        ws.getRow(row).getCell(1).value = line.accountName;
        ws.getRow(row).getCell(2).value = line.amount;
        styleDataCell(ws.getRow(row).getCell(2), { numFmt: "#,##0" });
        altRowFill(ws.getRow(row).getCell(1), i);
        altRowFill(ws.getRow(row).getCell(2), i);
        row++;
      });
      ws.getRow(row).getCell(1).value = `إجمالي ${title}`;
      ws.getRow(row).getCell(2).value = total;
      styleDataCell(ws.getRow(row).getCell(1), { bold: true });
      styleDataCell(ws.getRow(row).getCell(2), { numFmt: "#,##0", bold: true });
      row++;
    };

    writeSection("الإيرادات", data.revenues, data.totalRevenue);
    writeSection("المصروفات", data.expenses, data.totalExpenses);
    ws.getRow(row).getCell(1).value = "صافي الدخل";
    ws.getRow(row).getCell(2).value = data.netIncome;
    styleDataCell(ws.getRow(row).getCell(1), { bold: true });
    styleDataCell(ws.getRow(row).getCell(2), { numFmt: "#,##0", bold: true });
    buildReportFooter(ws, 2);
    await exportWorkbook(ws.workbook, `التقرير_الفصلي_${selectedQuarter?.quarterName || ""}`);
  }, [data, selectedQuarter]);

  const customKPIs = data ? [
    { label: 'إجمالي الإيرادات', value: formatCurrency(data.totalRevenue), color: 'text-green-600' },
    { label: 'إجمالي المصروفات', value: formatCurrency(data.totalExpenses), color: 'text-red-600' },
    { label: 'صافي الدخل الفصلي', value: formatCurrency(data.netIncome), color: data.netIncome >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'الفصل', value: selectedQuarter?.quarterName || '', color: 'text-primary' },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
        <label className="label-filter">السنة المالية</label>
=======
        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">السنة المالية</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
        <label className="label-filter">الفصل المالي</label>
=======
        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">الفصل المالي</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
        "flex justify-between px-3 py-1.5 border-t border-border text-sm",
=======
        "flex justify-between px-3 py-1.5 border-t border-border/30 text-sm",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}    >
=======
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
      onExport={hideInternalControls ? undefined : (data ? handleExportExcel : undefined)}
    >
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
