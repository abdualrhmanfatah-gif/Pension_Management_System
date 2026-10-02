import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import {
altRowFill,
autoFitColumns,
  buildReportFooter, buildReportHeader, buildTableHeader,   createWorkbook, exportWorkbook, styleDataCell, } from "@/components/domain/print/excel.utils";
import { Badge } from "@/components/ui";
import { JOURNAL_STATUS } from "@/constants";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { JournalRegisterItemDto } from "@/types/accounting";

interface JournalRegisterReportProps {
  fiscalYearId?: number;
  fromDate?: string;
  toDate?: string;
  hideInternalControls?: boolean;
}

const statusBadge: Record<string, { label: string; variant: "warning" | "success" | "destructive" | "outline" }> = {
  [JOURNAL_STATUS.DRAFT]: { label: "مسودة", variant: "warning" },
  [JOURNAL_STATUS.POSTED]: { label: "مرحّل", variant: "success" },
  [JOURNAL_STATUS.REVERSED]: { label: "ملغي", variant: "destructive" },
};

export function JournalRegisterReport({ fiscalYearId, fromDate: propFromDate, toDate: propToDate, hideInternalControls }: JournalRegisterReportProps) {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
  const [filters, setFilters] = useState<Record<string, any>>({
    fromDate: propFromDate || firstDay.toISOString().split("T")[0],
    toDate: propToDate || today.toISOString().split("T")[0],
  });

  useEffect(() => {
    if (propFromDate) setFilters(prev => ({ ...prev, fromDate: propFromDate }));
    if (propToDate) setFilters(prev => ({ ...prev, toDate: propToDate }));
  }, [propFromDate, propToDate]);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "journal-register", fiscalYearId, filters.fromDate, filters.toDate],
    queryFn: () => reportsApi.journalRegister({ fiscalYearId, fromDate: filters.fromDate, toDate: filters.toDate }),
    enabled: !!filters.fromDate && !!filters.toDate,
  });

  const handleFilterChange = useCallback((key: string, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    if (filters.fromDate) params.set("fromDate", filters.fromDate);
    if (filters.toDate) params.set("toDate", filters.toDate);
    window.open(`/reports/journal-register/print?${params.toString()}`, "_blank");
  }, [fiscalYearId, filters]);

  const handleExportExcel = useCallback(async () => {
    if (!data) return;
    const { ws } = createWorkbook("سجل اليومية");
    autoFitColumns(ws, { 1: 12, 2: 15, 3: 25, 4: 12, 5: 10, 6: 12, 7: 18, 8: 18 });
    const dataStartRow = await buildReportHeader(ws, "سجل اليومية", 8, [
      { label: "من تاريخ", value: filters.fromDate },
      { label: "إلى تاريخ", value: filters.toDate },
    ]);

    buildTableHeader(ws, dataStartRow, ["التاريخ", "رقم القيد", "البيان", "المصدر", "الحالة", "بواسطة", "مدين", "دائن"]);

    data.entries.forEach((entry: JournalRegisterItemDto, i: number) => {
      const r = ws.getRow(dataStartRow + 1 + i);
      r.getCell(1).value = new Date(entry.entryDate).toLocaleDateString("ar-SA");
      r.getCell(2).value = entry.entryNumber;
      r.getCell(3).value = entry.description;
      r.getCell(4).value = entry.sourceModule === "Manual" ? "يدوي" : entry.sourceModule;
      r.getCell(5).value = statusBadge[entry.status]?.label ?? entry.status;
      r.getCell(6).value = entry.createdByUserName;
      r.getCell(7).value = entry.totalDebit;
      r.getCell(8).value = entry.totalCredit;

      [7, 8].forEach(c => styleDataCell(r.getCell(c), { numFmt: "#,##0" }));
      for (let c = 1; c <= 8; c++) altRowFill(r.getCell(c), i);
    });

    const totalRowNum = dataStartRow + 1 + data.entries.length;
    ws.getRow(totalRowNum).getCell(1).value = "الإجمالي";
    ws.getRow(totalRowNum).getCell(7).value = data.grandTotalDebit;
    ws.getRow(totalRowNum).getCell(8).value = data.grandTotalCredit;
    styleDataCell(ws.getRow(totalRowNum).getCell(1), { bold: true });
    styleDataCell(ws.getRow(totalRowNum).getCell(7), { numFmt: "#,##0", bold: true });
    styleDataCell(ws.getRow(totalRowNum).getCell(8), { numFmt: "#,##0", bold: true });

    buildReportFooter(ws, 8);
    await exportWorkbook(ws.workbook, `سجل_اليومية_${filters.fromDate}_${filters.toDate}`);
  }, [data, filters]);

  const chartContent = data ? (
    <ReportBarChart
      data={[
        { name: 'القيود', value: data.totalEntries },
        { name: 'المدين', value: data.grandTotalDebit },
        { name: 'الدائن', value: data.grandTotalCredit },
      ]}
      xKey="name"
      yKeys={[{ key: 'value', color: chartColors.primary, label: 'المبلغ' }]}
      title="ملخص سجل اليومية"
      height={250}
    />
  ) : undefined;

  const customKPIs = data ? [
    { label: 'عدد القيود', value: data.totalEntries, color: 'text-foreground' },
    { label: 'إجمالي المدين', value: formatCurrency(data.grandTotalDebit), color: 'text-blue-600' },
    { label: 'إجمالي الدائن', value: formatCurrency(data.grandTotalCredit), color: 'text-green-600' },
  ] : [];

  const filterBar = hideInternalControls ? undefined : (
    <ReportFilters
      filters={['fromDate', 'toDate']}
      values={filters}
      onChange={handleFilterChange}
      onApply={() => refetch()}
      showApplyButton={false}
    />
  );

  return (
    <AccountingReportLayout
      title="سجل اليومية"
      description="عرض القيود اليومية المحاسبية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
      onExport={hideInternalControls ? undefined : (data ? handleExportExcel : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل سجل اليومية</div>
      ) : !data ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر التاريخ لعرض سجل اليومية</div>
      ) : (
        <div className="border rounded-xl overflow-hidden">
          <table className="w-full text-sm" role="table" aria-label="سجل القيود اليومية">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-2">التاريخ</th>
                <th className="text-end p-2">رقم القيد</th>
                <th className="text-end p-2">البيان</th>
                <th className="text-end p-2">المصدر</th>
                <th className="text-end p-2">الحالة</th>
                <th className="text-end p-2">بواسطة</th>
                <th className="text-start p-2 w-28">مدين</th>
                <th className="text-start p-2 w-28">دائن</th>
              </tr>
            </thead>
            <tbody>
              {data.entries.map((entry: JournalRegisterItemDto) => (
                <tr key={entry.journalEntryID} className="border-t border-border/50 hover:bg-muted/20">
                  <td className="p-2 text-xs">{new Date(entry.entryDate).toLocaleDateString("ar-SA")}</td>
                  <td className="p-2 font-mono text-xs ltr" dir="ltr">{entry.entryNumber}</td>
                  <td className="p-2 text-xs max-w-[200px] truncate">{entry.description}</td>
                  <td className="p-2 text-xs">{entry.sourceModule === "Manual" ? "يدوي" : entry.sourceModule}</td>
                  <td className="p-2">
                    <Badge variant={statusBadge[entry.status]?.variant ?? "outline"}>
                      {statusBadge[entry.status]?.label ?? entry.status}
                    </Badge>
                  </td>
                  <td className="p-2 text-xs">{entry.createdByUserName}</td>
                  <td className="p-2 text-start font-mono text-xs">{formatCurrency(entry.totalDebit)}</td>
                  <td className="p-2 text-start font-mono text-xs">{formatCurrency(entry.totalCredit)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </AccountingReportLayout>
  );
}
