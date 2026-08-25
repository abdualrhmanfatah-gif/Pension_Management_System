import { useQuery } from "@tanstack/react-query";
import { useCallback } from "react";
import { ArrowDown, ArrowUp } from "lucide-react";

import { reportsApi } from "@/api/accounting";
import type { CashFlowItemDto } from "@/types/accounting";
import { formatCurrency } from "@/lib/utils";
import { Card, Heading, Text } from "@/components/ui";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";
import {
  createWorkbook, buildReportHeader, styleDataCell, altRowFill,
  buildReportFooter, exportWorkbook, autoFitColumns,
} from "@/components/domain/print/excel.utils";

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

  const handleExportExcel = useCallback(async () => {
    if (!report) return;
    const { ws } = createWorkbook("التدفقات النقدية");
    autoFitColumns(ws, { 1: 25, 2: 20, 3: 20, 4: 20 });
    const dataStartRow = await buildReportHeader(ws, "التدفقات النقدية", 4, [
      { label: "السنة المالية", value: String(fiscalYearId) },
    ]);

    let row = dataStartRow;
    const writeSection = (title: string, items: { accountName: string; amount: number; type: string }[], inflow: number, outflow: number) => {
      ws.getRow(row).getCell(1).value = title;
      ws.getRow(row).getCell(2).value = "وارد";
      ws.getRow(row).getCell(3).value = "صادر";
      ws.getRow(row).getCell(4).value = "الصافي";
      styleDataCell(ws.getRow(row).getCell(1), { bold: true });
      styleDataCell(ws.getRow(row).getCell(2), { bold: true });
      styleDataCell(ws.getRow(row).getCell(3), { bold: true });
      styleDataCell(ws.getRow(row).getCell(4), { bold: true });
      row++;

      items.forEach((item, i) => {
        ws.getRow(row).getCell(1).value = item.accountName;
        ws.getRow(row).getCell(item.type === "Inflow" ? 2 : 3).value = item.amount;
        styleDataCell(ws.getRow(row).getCell(2), { numFmt: "#,##0" });
        styleDataCell(ws.getRow(row).getCell(3), { numFmt: "#,##0" });
        for (let c = 1; c <= 4; c++) altRowFill(ws.getRow(row).getCell(c), i);
        row++;
      });

      ws.getRow(row).getCell(1).value = `إجمالي ${title}`;
      ws.getRow(row).getCell(2).value = inflow;
      ws.getRow(row).getCell(3).value = outflow;
      ws.getRow(row).getCell(4).value = inflow - outflow;
      styleDataCell(ws.getRow(row).getCell(1), { bold: true });
      [2, 3, 4].forEach(c => styleDataCell(ws.getRow(row).getCell(c), { numFmt: "#,##0", bold: true }));
      row++;
    };

    writeSection("التدفقات التشغيلية", report.operatingItems, report.operatingCashInflow, report.operatingCashOutflow);
    writeSection("التدفقات الاستثمارية", report.investingItems, report.investingCashInflow, report.investingCashOutflow);
    writeSection("التدفقات التمويلية", report.financingItems, report.financingCashInflow, report.financingCashOutflow);

    ws.getRow(row).getCell(1).value = "صافي التدفق النقدي";
    ws.getRow(row).getCell(4).value = report.netCashFlow;
    styleDataCell(ws.getRow(row).getCell(1), { bold: true });
    styleDataCell(ws.getRow(row).getCell(4), { numFmt: "#,##0", bold: true });

    buildReportFooter(ws, 4);
    await exportWorkbook(ws.workbook, `التدفقات_النقدية_${fiscalYearId}`);
  }, [report, fiscalYearId]);

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
      onPrint={hideInternalControls ? undefined : (report ? handlePrint : undefined)}
      onExport={hideInternalControls ? undefined : (report ? handleExportExcel : undefined)}
    >
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
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
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
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
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
                      <div key={i} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
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
