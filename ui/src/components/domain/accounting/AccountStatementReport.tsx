import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { accountsApi, reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportLineChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import { Button } from "@/components/ui";
import { Card } from "@/components/ui/Card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { AccountStatementResultDto, ChartOfAccountDto } from "@/types/accounting";

interface AccountStatementReportProps {
  fiscalYearId?: number;
  fromDate?: string;
  toDate?: string;
  externalFilters?: { accountId?: number };
  hideInternalControls?: boolean;
}

export function AccountStatementReport({ fiscalYearId, fromDate: propFromDate, toDate: propToDate, externalFilters, hideInternalControls }: AccountStatementReportProps) {
  const [internalAccountId, setInternalAccountId] = useState<number | null>(null);
  const accountId = externalFilters?.accountId !== undefined ? externalFilters.accountId : internalAccountId;
  const [filters, setFilters] = useState<Record<string, any>>({
    fromDate: propFromDate || new Date(new Date().getFullYear(), 0, 1).toISOString().split("T")[0],
    toDate: propToDate || new Date().toISOString().split("T")[0],
  });

  useEffect(() => {
    if (propFromDate) setFilters(prev => ({ ...prev, fromDate: propFromDate }));
    if (propToDate) setFilters(prev => ({ ...prev, toDate: propToDate }));
  }, [propFromDate, propToDate]);

  const { data: accounts } = useQuery({
    queryKey: ["accounts", "postable"],
    queryFn: () => accountsApi.getPostable(),
  });

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "account-statement", accountId, fiscalYearId, filters.fromDate, filters.toDate],
    queryFn: () => reportsApi.accountStatement({ accountId: accountId!, fiscalYearId, fromDate: filters.fromDate, toDate: filters.toDate }),
    enabled: !!accountId && !!filters.fromDate && !!filters.toDate,
  });

  const handleFilterChange = useCallback((key: string, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const handleSearch = () => {
    if (!accountId || !filters.fromDate || !filters.toDate) return;
    refetch();
  };

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (accountId) params.set("accountId", String(accountId));
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    if (filters.fromDate) params.set("fromDate", filters.fromDate);
    if (filters.toDate) params.set("toDate", filters.toDate);
    window.open(`/reports/account-statement/print?${params.toString()}`, "_blank");
  }, [accountId, fiscalYearId, filters]);

  const result = data as AccountStatementResultDto | undefined;
  const items = result?.items ?? [];

  const chartContent = items.length > 0 ? (
    <ReportLineChart
      data={items.map(item => ({
        name: new Date(item.entryDate).toLocaleDateString('ar-SA', { month: 'short', day: 'numeric' }),
        balance: item.runningBalance,
      }))}
      xKey="name"
      lines={[{ key: 'balance', color: chartColors.primary, label: 'الرصيد' }]}
      title="تطور الرصيد"
      area
      height={250}
    />
  ) : undefined;

  const customKPIs = items.length > 0 ? [
    { label: 'عدد الحركات', value: items.length, color: 'default' },
    { label: 'إجمالي المدين', value: formatCurrency(items.reduce((s, i) => s + i.debit, 0)), color: 'info' },
    { label: 'إجمالي الدائن', value: formatCurrency(items.reduce((s, i) => s + i.credit, 0)), color: 'success' },
    { label: 'الرصيد النهائي', value: formatCurrency(items[items.length - 1]?.runningBalance ?? 0), color: 'primary' },
  ] : [];

  const filterBar = hideInternalControls ? undefined : (
    <Card className="p-4 flex flex-wrap gap-4 items-end mb-6">
      <div className="w-full sm:w-64">
        <label className="label-filter">الحساب</label>
        <Select value={String(accountId ?? "")} onValueChange={(v) => setInternalAccountId(Number(v))}>
          <SelectTrigger className="h-10">
            <SelectValue placeholder="اختر الحساب" />
          </SelectTrigger>
          <SelectContent className="max-h-[250px]">
            {accounts?.map((a: ChartOfAccountDto) => (
              <SelectItem key={a.accountID} value={String(a.accountID)}>
                {a.accountCode} - {a.accountName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <ReportFilters
        filters={['fromDate', 'toDate']}
        values={filters}
        onChange={handleFilterChange}
        showApplyButton={false}
        compact
      />
      <Button
        onClick={handleSearch}
        disabled={!accountId || !filters.fromDate || !filters.toDate}
      >
        عرض
      </Button>
    </Card>
  );

  return (
    <AccountingReportLayout
      title="كشف حساب"
      description="عرض حركات حساب معين في فترة محددة"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (items.length > 0 ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل كشف الحساب</div>
      ) : items.length === 0 ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر الحساب والتاريخ لعرض كشف الحساب</div>
      ) : (
        <div className="border rounded-xl overflow-hidden">
          <table className="w-full text-sm" role="table" aria-label="كشف حساب">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-2">التاريخ</th>
                <th className="text-end p-2">رقم القيد</th>
                <th className="text-end p-2">البيان</th>
                <th className="text-start p-2 w-28">مدين</th>
                <th className="text-start p-2 w-28">دائن</th>
                <th className="text-start p-2 w-28">الرصيد</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item, i) => (
                <tr key={i} className="border-t border-border/50 hover:bg-muted/20">
                  <td className="p-2 text-xs">{new Date(item.entryDate).toLocaleDateString("ar-SA")}</td>
                  <td className="p-2 font-mono text-xs ltr" dir="ltr">{item.entryNumber}</td>
                  <td className="p-2 text-xs">{item.description}</td>
                  <td className="p-2 text-start font-mono text-xs">{item.debit > 0 ? formatCurrency(item.debit) : "—"}</td>
                  <td className="p-2 text-start font-mono text-xs">{item.credit > 0 ? formatCurrency(item.credit) : "—"}</td>
                  <td className="p-2 text-start font-mono text-xs">{formatCurrency(item.runningBalance)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-muted/30 font-medium">
              <tr>
                <td colSpan={5} className="p-2 text-start">الرصيد النهائي</td>
                <td className="p-2 text-start font-mono">{formatCurrency(items[items.length - 1]?.runningBalance ?? 0)}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      )}
    </AccountingReportLayout>
  );
}
