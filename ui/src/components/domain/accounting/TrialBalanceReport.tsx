import { useQuery } from "@tanstack/react-query";
import { AlertCircle,CheckCircle } from "lucide-react";
import { useCallback,useMemo, useState } from "react";

import { reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import {
  altRowFill,
  autoFitColumns,
  buildReportFooter,
  buildReportHeader,
  buildTableHeader,
  buildTotalsRow,
  createWorkbook,
  exportWorkbook,
  styleDataCell,
} from "@/components/domain/print/excel.utils";
import { type CustomColumnDef,DataTable } from "@/components/ui/DataTable";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { TrialBalanceItemDto } from "@/types/accounting";

interface TrialBalanceReportProps {
  fiscalYearId?: number;
  periodId?: number;
  fromDate?: string;
  toDate?: string;
  onExport?: (data: TrialBalanceItemDto[]) => void | Promise<void>;
  exportLabel?: string;
  externalFilters?: { accountLevel?: string };
  hideInternalControls?: boolean;
}

const COL_COUNT = 9;

export function TrialBalanceReport({
  fiscalYearId,
  periodId,
  fromDate,
  toDate,
  externalFilters,
  hideInternalControls,
}: TrialBalanceReportProps) {
  const [internalFilters, setInternalFilters] = useState<Record<string, any>>({ accountLevel: 'all' });

  const filters = {
    ...internalFilters,
    ...(externalFilters?.accountLevel !== undefined ? { accountLevel: externalFilters.accountLevel } : {}),
  };

  const { data = [], isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "trial-balance", fiscalYearId, periodId, fromDate, toDate],
    queryFn: () => reportsApi.trialBalance({ fiscalYearId, periodId, fromDate, toDate }),
    enabled: true,
  });

  const filteredData = useMemo(() => {
    if (filters.accountLevel === 'all' || !filters.accountLevel) return data;
    const level = parseInt(filters.accountLevel);
    return data.filter((a: TrialBalanceItemDto) => a.accountLevel <= level);
  }, [data, filters.accountLevel]);

  const postableData = filteredData.filter((a: TrialBalanceItemDto) => a.isPostable);
  const grandOpeningDebit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.openingDebit, 0);
  const grandOpeningCredit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.openingCredit, 0);
  const grandMovementDebit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.movementDebit, 0);
  const grandMovementCredit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.movementCredit, 0);
  const grandClosingDebit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.closingDebit, 0);
  const grandClosingCredit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.closingCredit, 0);
  const grandTotalDebit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.totalDebit, 0);
  const grandTotalCredit = postableData.reduce((s: number, i: TrialBalanceItemDto) => s + i.totalCredit, 0);
  const isBalanced = Math.abs(grandTotalDebit - grandTotalCredit) < 0.01;

  const totalAccounts = filteredData.length;
  const postableAccounts = postableData.length;
  const debitAccounts = filteredData.filter((a: TrialBalanceItemDto) => a.balanceType === "Debit" && a.balance > 0).length;
  const creditAccounts = filteredData.filter((a: TrialBalanceItemDto) => a.balanceType === "Credit" && a.balance > 0).length;

  // Chart data: top 10 accounts by balance
  const chartData = useMemo(() => {
    return postableData
      .filter((a: TrialBalanceItemDto) => a.balance > 0)
      .sort((a: TrialBalanceItemDto, b: TrialBalanceItemDto) => b.balance - a.balance)
      .slice(0, 10)
      .map((a: TrialBalanceItemDto) => ({
        name: a.accountName.length > 20 ? a.accountName.substring(0, 20) + '...' : a.accountName,
        fullName: a.accountName,
        debit: a.totalDebit,
        credit: a.totalCredit,
        balance: a.balance,
        type: a.balanceType,
      }));
  }, [postableData]);

  // Pie chart data: debit vs credit totals
  const pieData = useMemo(() => [
    { name: 'المدين', value: grandTotalDebit, fill: chartColors.primary },
    { name: 'الدائن', value: grandTotalCredit, fill: chartColors.success },
  ], [grandTotalDebit, grandTotalCredit]);

  const handleFilterChange = useCallback((key: string, value: any) => {
    setInternalFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const handleExportExcel = useCallback(async () => {
    const { ws } = createWorkbook("ميزان المراجعه");
    autoFitColumns(ws, { 1: 15, 2: 28, 3: 18, 4: 18, 5: 18, 6: 18, 7: 18, 8: 18, 9: 18 });

    const levelLabel = filters.accountLevel === 'all' || !filters.accountLevel ? "جميع المستويات" : `المستوى ${filters.accountLevel}`;
    const dataStartRow = await buildReportHeader(ws, "ميزان المراجعه", COL_COUNT, [
      { label: "الفلتر", value: levelLabel },
    ]);

    buildTableHeader(ws, dataStartRow, [
      "الكود", "الحساب", "افتتاحي مدين", "افتتاحي دائن",
      "حركه مدين", "حركه دائن", "ختامي مدين", "ختامي دائن", "الرصيد",
    ]);

    filteredData.forEach((item: TrialBalanceItemDto, i) => {
      const r = ws.getRow(dataStartRow + 1 + i);
      r.getCell(1).value = item.accountCode;
      r.getCell(2).value = item.accountName;
      r.getCell(3).value = item.openingDebit;
      r.getCell(4).value = item.openingCredit;
      r.getCell(5).value = item.movementDebit;
      r.getCell(6).value = item.movementCredit;
      r.getCell(7).value = item.closingDebit;
      r.getCell(8).value = item.closingCredit;
      r.getCell(9).value = item.balance;

      styleDataCell(r.getCell(1), { bold: true });
      styleDataCell(r.getCell(2), { bold: true });
      [3, 4, 5, 6, 7, 8].forEach(c => styleDataCell(r.getCell(c), { numFmt: "#,##0" }));
      styleDataCell(r.getCell(9), { numFmt: "#,##0", bold: true });

      for (let c = 1; c <= 9; c++) altRowFill(r.getCell(c), i);
    });

    const totalRowNum = dataStartRow + 1 + filteredData.length;
    buildTotalsRow(ws, totalRowNum, [
      { value: "الإجمالي" },
      undefined,
      { value: filteredData.reduce((s: number, i: TrialBalanceItemDto) => s + i.openingDebit, 0), numFmt: "#,##0" },
      { value: filteredData.reduce((s: number, i: TrialBalanceItemDto) => s + i.openingCredit, 0), numFmt: "#,##0" },
      { value: filteredData.reduce((s: number, i: TrialBalanceItemDto) => s + i.movementDebit, 0), numFmt: "#,##0" },
      { value: filteredData.reduce((s: number, i: TrialBalanceItemDto) => s + i.movementCredit, 0), numFmt: "#,##0" },
      { value: filteredData.reduce((s: number, i: TrialBalanceItemDto) => s + i.closingDebit, 0), numFmt: "#,##0" },
      { value: filteredData.reduce((s: number, i: TrialBalanceItemDto) => s + i.closingCredit, 0), numFmt: "#,##0" },
      { value: grandTotalDebit, numFmt: "#,##0" },
    ]);

    buildReportFooter(ws, COL_COUNT);
    await exportWorkbook(ws.workbook, `ميزان_المراجعه_${new Date().toISOString().slice(0, 10)}`);
  }, [filteredData, grandTotalDebit, filters.accountLevel]);

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    if (periodId) params.set("periodId", String(periodId));
    if (fromDate) params.set("fromDate", fromDate);
    if (toDate) params.set("toDate", toDate);
    window.open(`/reports/trial-balance/print?${params.toString()}`, "_blank");
  }, [fiscalYearId, periodId, fromDate, toDate]);

  const columns: CustomColumnDef<TrialBalanceItemDto>[] = useMemo(() => [
    {
      key: "accountCode",
      header: "الكود",
      className: "text-center",
      render: (row) => (
        <span
          className="font-mono font-bold text-foreground"
          dir="ltr"
          style={{
            paddingRight: `${(row.accountLevel - 1) * 16}px`,
            fontSize: row.isPostable ? '14px' : '16px'
          }}
        >
          {row.accountCode}
        </span>
      ),
    },
    {
      key: "accountName",
      header: "الحساب",
      render: (row) => (
        <span
          className="text-foreground"
          style={{
            fontWeight: row.isPostable ? 'normal' : 'bold',
            paddingRight: `${(row.accountLevel - 1) * 16}px`
          }}
        >
          {!row.isPostable && <span className="text-muted-foreground ml-1">📂</span>}
          {row.accountName}
        </span>
      ),
    },
    {
      key: "openingDebit",
      header: "افتتاحي مدين",
      className: "text-center",
      render: (row) => (
        <span className="font-mono font-bold text-sm text-info">
          {row.openingDebit > 0 ? formatCurrency(row.openingDebit) : "-"}
        </span>
      ),
    },
    {
      key: "openingCredit",
      header: "افتتاحي دائن",
      className: "text-center",
      render: (row) => (
        <span className="font-mono font-bold text-sm text-success">
          {row.openingCredit > 0 ? formatCurrency(row.openingCredit) : "-"}
        </span>
      ),
    },
    {
      key: "movementDebit",
      header: "حركه مدين",
      className: "text-center",
      render: (row) => (
        <span className="font-mono font-bold text-sm text-info">
          {row.movementDebit > 0 ? formatCurrency(row.movementDebit) : "-"}
        </span>
      ),
    },
    {
      key: "movementCredit",
      header: "حركه دائن",
      className: "text-center",
      render: (row) => (
        <span className="font-mono font-bold text-sm text-success">
          {row.movementCredit > 0 ? formatCurrency(row.movementCredit) : "-"}
        </span>
      ),
    },
    {
      key: "closingDebit",
      header: "ختامي مدين",
      className: "text-center",
      render: (row) => (
        <span className="font-mono font-bold text-sm text-info">
          {row.closingDebit > 0 ? formatCurrency(row.closingDebit) : "-"}
        </span>
      ),
    },
    {
      key: "closingCredit",
      header: "ختامي دائن",
      className: "text-center",
      render: (row) => (
        <span className="font-mono font-bold text-sm text-success">
          {row.closingCredit > 0 ? formatCurrency(row.closingCredit) : "-"}
        </span>
      ),
    },
    {
      key: "balance",
      header: "الرصيد",
      className: "text-center",
      render: (row) => (
        <div className="flex items-center justify-center gap-2">
          <span className={`font-mono font-bold text-lg ${
            row.balanceType === "Debit" ? "text-info" : "text-success"
          }`}>
            {row.balance > 0 ? formatCurrency(row.balance) : "-"}
          </span>
          {row.balance > 0 && (
            <span className={`text-xs px-1.5 py-0.5 rounded ${
              row.balanceType === "Debit"
                ? "bg-info/10 text-info"
                : "bg-success/10 text-success"
            }`}>
              {row.balanceType === "Debit" ? "مدين" : "دائن"}
            </span>
          )}
        </div>
      ),
    },
  ], []);

  if (error)
    return (
      <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
    );

  const filterBar = hideInternalControls ? undefined : (
    <ReportFilters
      filters={['accountLevel']}
      values={filters}
      onChange={handleFilterChange}
      onApply={() => refetch()}
      showApplyButton={false}
    />
  );

  const customKPIs = [
    { label: 'إجمالي الحسابات', value: totalAccounts, color: 'default' },
    { label: 'حسابات مدينة', value: debitAccounts, color: 'info' },
    { label: 'حسابات دائنة', value: creditAccounts, color: 'success' },
    { label: 'حسابات فرعية', value: postableAccounts, color: 'default' },
  ];

  const chartContent = (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportBarChart
        data={chartData}
        xKey="name"
        yKeys={[
          { key: 'debit', color: chartColors.primary, label: 'المدين' },
          { key: 'credit', color: chartColors.success, label: 'الدائن' },
        ]}
        title="أعلى 10 حسابات حسب الرصيد"
        height={300}
      />
      <ReportPieChart
        data={pieData}
        nameKey="name"
        valueKey="value"
        title="توزيع المدين والدائن"
        height={300}
      />
    </div>
  );

  const footer = filteredData.length > 0 ? (
    <div className={`rounded-lg p-4 font-bold ${isBalanced ? 'bg-success/10 border border-success/30' : 'bg-danger/10 border border-danger/30'}`}>
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div className="flex items-center gap-3">
          {isBalanced ? (
            <CheckCircle className="w-6 h-6 text-success" />
          ) : (
            <AlertCircle className="w-6 h-6 text-danger" />
          )}
          <div>
            <div className="text-lg">
              {isBalanced ? "الميزان متوازن ✓" : "الميزان غير متوازن ✗"}
            </div>
            {!isBalanced && (
              <div className="text-sm font-normal text-danger">
                الفرق: {formatCurrency(Math.abs(grandTotalDebit - grandTotalCredit))}
              </div>
            )}
          </div>
        </div>
        <div className="flex gap-6 flex-wrap">
          <div className="text-center">
            <div className="text-xs text-muted-foreground">الافتتاحي</div>
            <div className="flex gap-3">
              <div><span className="text-xs text-muted-foreground">م:</span> <span className="font-mono text-info">{formatCurrency(grandOpeningDebit)}</span></div>
              <div><span className="text-xs text-muted-foreground">د:</span> <span className="font-mono text-success">{formatCurrency(grandOpeningCredit)}</span></div>
            </div>
          </div>
          <div className="text-center">
            <div className="text-xs text-muted-foreground">الحركه</div>
            <div className="flex gap-3">
              <div><span className="text-xs text-muted-foreground">م:</span> <span className="font-mono text-info">{formatCurrency(grandMovementDebit)}</span></div>
              <div><span className="text-xs text-muted-foreground">د:</span> <span className="font-mono text-success">{formatCurrency(grandMovementCredit)}</span></div>
            </div>
          </div>
          <div className="text-center">
            <div className="text-xs text-muted-foreground">الختامي</div>
            <div className="flex gap-3">
              <div><span className="text-xs text-muted-foreground">م:</span> <span className="font-mono text-info">{formatCurrency(grandClosingDebit)}</span></div>
              <div><span className="text-xs text-muted-foreground">د:</span> <span className="font-mono text-success">{formatCurrency(grandClosingCredit)}</span></div>
            </div>
          </div>
          <div className="text-center">
            <div className="text-xs text-muted-foreground">الإجمالي</div>
            <div className="flex gap-3">
              <div><span className="text-xs text-muted-foreground">م:</span> <span className="font-mono text-xl text-info">{formatCurrency(grandTotalDebit)}</span></div>
              <div><span className="text-xs text-muted-foreground">د:</span> <span className="font-mono text-xl text-success">{formatCurrency(grandTotalCredit)}</span></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  ) : null;

  return (
    <AccountingReportLayout
      title="ميزان المراجعة"
      description="عرض أرصدة الحسابات وميزان التوازن"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onExport={hideInternalControls ? undefined : (filteredData.length > 0 ? handleExportExcel : undefined)}
      onPrint={hideInternalControls ? undefined : (filteredData.length > 0 ? handlePrint : undefined)}
      footer={footer}
    >
      <DataTable
        columns={columns}
        data={filteredData}
        loading={isLoading}
        emptyMessage="لا توجد بيانات"
        rowKey={(row) => row.accountID}
      />
    </AccountingReportLayout>
  );
}
