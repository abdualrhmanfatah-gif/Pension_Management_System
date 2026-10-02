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
import type { ChartOfAccountDto,GeneralLedgerDto, GeneralLedgerLineDto } from "@/types/accounting";

interface GeneralLedgerReportProps {
  fiscalYearId?: number;
  fromDate?: string;
  toDate?: string;
  externalFilters?: { accountId?: number };
  hideInternalControls?: boolean;
}

export function GeneralLedgerReport({ fromDate: propFromDate, toDate: propToDate, externalFilters, hideInternalControls }: GeneralLedgerReportProps) {
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

  const { data: accounts } = useQuery({
    queryKey: ["accounts", "all"],
    queryFn: () => accountsApi.getAll(),
  });

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "general-ledger", accountId, filters.fromDate, filters.toDate],
    queryFn: () => reportsApi.generalLedger({
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
    window.open(`/reports/general-ledger/print?${params.toString()}`, "_blank");
  }, [accountId, filters]);

  const chartContent = data ? (
    <ReportLineChart
      data={data.entries.map((e: GeneralLedgerLineDto) => ({
        name: new Date(e.entryDate).toLocaleDateString('ar-SA', { month: 'short', day: 'numeric' }),
        balance: e.runningBalance,
        debit: e.debit,
        credit: e.credit,
      }))}
      xKey="name"
      lines={[
        { key: 'balance', color: chartColors.primary, label: 'الرصيد' },
      ]}
      title="تطور الرصيد عبر الزمن"
      area
      height={250}
    />
  ) : undefined;

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
        disabled={!accountId}
      >
        عرض
      </Button>
    </Card>
  );

  const customKPIs = data ? [
    { label: 'الرصيد الافتتاحي', value: formatCurrency(data.openingBalance), color: 'text-foreground' },
    { label: 'إجمالي المدين', value: formatCurrency(data.totalDebit), color: 'text-blue-600' },
    { label: 'إجمالي الدائن', value: formatCurrency(data.totalCredit), color: 'text-green-600' },
    { label: 'الرصيد الختامي', value: formatCurrency(data.closingBalance), color: 'text-primary' },
  ] : [];

  return (
    <AccountingReportLayout
      title="دفتر الأستاذ العام"
      description="عرض حركات الحسابات التفصيلية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (data ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !data ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر حساباً وانقر عرض</div>
      ) : (
        <GeneralLedgerTable data={data} />
      )}
    </AccountingReportLayout>
  );
}

function GeneralLedgerTable({ data }: { data: GeneralLedgerDto }) {
  const isParentAccount = data.entries.some(e => e.accountCode && e.accountCode !== data.accountCode);

  return (
    <div className="border rounded-xl overflow-hidden">
      <table className="w-full text-sm" role="table" aria-label="دفتر الأستاذ العام">
        <thead className="bg-muted/50 sticky top-0 z-10">
          <tr>
            <th className="text-end p-2">التاريخ</th>
            <th className="text-end p-2">رقم القيد</th>
            {isParentAccount && <th className="text-end p-2">الحساب</th>}
            <th className="text-end p-2">البيان</th>
            <th className="text-end p-2">مركز التكلفة</th>
            <th className="text-start p-2 w-28">مدين</th>
            <th className="text-start p-2 w-28">دائن</th>
            <th className="text-start p-2 w-28">الرصيد</th>
          </tr>
        </thead>
        <tbody>
          {data.entries.map((entry, i) => (
            <tr key={i} className="border-t border-border/50 hover:bg-muted/20">
              <td className="p-2 text-xs">{new Date(entry.entryDate).toLocaleDateString("ar-SA")}</td>
              <td className="p-2 font-mono text-xs ltr" dir="ltr">{entry.entryNumber}</td>
              {isParentAccount && (
                <td className="p-2 text-xs">
                  <span className="text-muted-foreground">{entry.accountCode}</span>
                  <span className="ms-1">{entry.accountName}</span>
                </td>
              )}
              <td className="p-2 text-xs">{entry.description}</td>
              <td className="p-2 text-xs text-muted-foreground">{entry.costCenterName || "—"}</td>
              <td className="p-2 text-start font-mono text-xs">{entry.debit > 0 ? formatCurrency(entry.debit) : "—"}</td>
              <td className="p-2 text-start font-mono text-xs">{entry.credit > 0 ? formatCurrency(entry.credit) : "—"}</td>
              <td className="p-2 text-start font-mono text-xs">{formatCurrency(entry.runningBalance)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
