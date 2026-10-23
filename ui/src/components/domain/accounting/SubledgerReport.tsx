import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { accountsApi, reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportLineChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import { Button, Card, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { ChartOfAccountDto,SubledgerReportLineDto } from "@/types/accounting";

interface SubledgerReportProps {
  fiscalYearId?: number;
  fromDate?: string;
  toDate?: string;
  externalFilters?: { accountId?: number };
  hideInternalControls?: boolean;
}

export function SubledgerReport({ fromDate: propFromDate, toDate: propToDate, externalFilters, hideInternalControls }: SubledgerReportProps) {
  const [internalAccountId, setInternalAccountId] = useState<number | null>(null);
  const accountId = externalFilters?.accountId !== undefined ? externalFilters.accountId : internalAccountId;
  const [filters, setFilters] = useState<Record<string, any>>({
    fromDate: propFromDate || '',
    toDate: propToDate || '',
  });

  useEffect(() => {
    if (propFromDate) setFilters(prev => ({ ...prev, fromDate: propFromDate }));
    if (propToDate) setFilters(prev => ({ ...prev, toDate: propToDate }));
  }, [propFromDate, propToDate]);

  const { data: postableAccounts = [] } = useQuery({
    queryKey: ["accounts", "postable"],
    queryFn: () => accountsApi.getPostable(),
  });

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "subledger", accountId, filters.fromDate, filters.toDate],
    queryFn: () => reportsApi.subledger({
      accountId: accountId!,
      fromDate: filters.fromDate || undefined,
      toDate: filters.toDate || undefined,
    }),
    enabled: !!accountId,
  });

  const handleFilterChange = useCallback((key: string, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const handleSearch = () => {
    if (!accountId) return;
    refetch();
  };

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (accountId) params.set("accountId", String(accountId));
    if (filters.fromDate) params.set("fromDate", filters.fromDate);
    if (filters.toDate) params.set("toDate", filters.toDate);
    window.open(`/reports/subledger/print?${params.toString()}`, "_blank");
  }, [accountId, filters]);

  const chartContent = data ? (
    <ReportLineChart
      data={data.entries.map((e: SubledgerReportLineDto) => ({
        name: new Date(e.entryDate).toLocaleDateString('ar-SA', { month: 'short', day: 'numeric' }),
        balance: e.runningBalance,
      }))}
      xKey="name"
      lines={[{ key: 'balance', color: chartColors.primary, label: 'الرصيد' }]}
      title="تطور الرصيد"
      area
      height={250}
    />
  ) : undefined;

  const customKPIs = data ? [
    { label: 'الحساب', value: `${data.accountCode} - ${data.accountName}`, color: 'default' },
    { label: 'الرصيد الافتتاحي', value: formatCurrency(data.openingBalance), color: 'default' },
    { label: 'إجمالي المدين', value: formatCurrency(data.totalDebit), color: 'info' },
    { label: 'إجمالي الدائن', value: formatCurrency(data.totalCredit), color: 'success' },
  ] : [];

  const filterBar = hideInternalControls ? undefined : (
    <Card className="p-4 flex flex-wrap gap-4 items-end mb-6">
      <div className="w-full sm:w-64">
        <label className="label-filter">الحساب</label>
        <Select
          value={accountId?.toString() ?? ""}
          onValueChange={(val) => setInternalAccountId(val ? Number(val) : null)}
        >
          <SelectTrigger>
            <SelectValue placeholder="اختر الحساب" />
          </SelectTrigger>
          <SelectContent>
            {postableAccounts.map((a: ChartOfAccountDto) => (
              <SelectItem key={a.accountID} value={a.accountID.toString()}>
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
        disabled={!accountId}
      >
        عرض
      </Button>
    </Card>
  );

  return (
    <AccountingReportLayout
      title="كشف حساب تفصيلي"
      description="عرض حركات حساب معين بتفاصيل القيود"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !data ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر الحساب ثم انقر عرض</div>
      ) : (
        <div className="border rounded-xl overflow-hidden">
          <table className="w-full text-sm" role="table" aria-label="دفتر الأستاذ الفرعي">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-2">التاريخ</th>
                <th className="text-end p-2">رقم القيد</th>
                <th className="text-end p-2">الحساب</th>
                <th className="text-end p-2">البيان</th>
                <th className="text-start p-2 w-28">مدين</th>
                <th className="text-start p-2 w-28">دائن</th>
                <th className="text-start p-2 w-28">الرصيد</th>
              </tr>
            </thead>
            <tbody>
              {data.entries.length === 0 ? (
                <tr>
                  <td colSpan={7} className="p-8 text-center text-muted-foreground">
                    لا توجد حركات في الفترة المحددة
                  </td>
                </tr>
              ) : (
                data.entries.map((entry: SubledgerReportLineDto, i) => (
                  <tr key={i} className="border-t border-border hover:bg-muted/20">
                    <td className="p-2 text-xs">{new Date(entry.entryDate).toLocaleDateString("ar-SA")}</td>
                    <td className="p-2 font-mono text-xs ltr" dir="ltr">{entry.entryNumber}</td>
                    <td className="p-2 text-xs">{entry.accountCode} - {entry.accountName}</td>
                    <td className="p-2 text-xs">{entry.description}</td>
                    <td className="p-2 text-start font-mono text-xs">{entry.debit > 0 ? formatCurrency(entry.debit) : "—"}</td>
                    <td className="p-2 text-start font-mono text-xs">{entry.credit > 0 ? formatCurrency(entry.credit) : "—"}</td>
                    <td className="p-2 text-start font-mono text-xs">{formatCurrency(entry.runningBalance)}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </AccountingReportLayout>
  );
}
