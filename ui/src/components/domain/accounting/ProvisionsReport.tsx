<<<<<<< HEAD
﻿import { useQuery } from "@tanstack/react-query";
import { useCallback, useMemo,useState } from "react";

import { reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart,ReportPieChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import { Card, Heading, Text } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { BalanceSheetSectionDto } from "@/types/accounting";
=======
import { useQuery } from "@tanstack/react-query";
import { useState, useCallback, useMemo } from "react";

import { reportsApi } from "@/api/accounting";
import type { BalanceSheetSectionDto } from "@/types/accounting";
import { formatCurrency } from "@/lib/utils";
import { Card, Heading, Text } from "@/components/ui";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportFilters } from "@/components/common/ReportFilters";
import { ReportPieChart, ReportBarChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface ProvisionsReportProps {
  fiscalYearId?: number;
  asOfDate?: string;
  externalFilters?: { toDate?: string };
  hideInternalControls?: boolean;
}

export function ProvisionsReport({ fiscalYearId, asOfDate: propAsOfDate, externalFilters, hideInternalControls }: ProvisionsReportProps) {
  const [internalFilters, setInternalFilters] = useState<Record<string, any>>({
    toDate: propAsOfDate || new Date().toISOString().split("T")[0],
  });

  const filters = {
    ...internalFilters,
    ...(externalFilters?.toDate !== undefined ? { toDate: externalFilters.toDate } : {}),
  };

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["reports", "balance-sheet", filters.toDate, fiscalYearId],
    queryFn: () => reportsApi.balanceSheet(filters.toDate!, fiscalYearId),
    enabled: !!filters.toDate,
  });

  const handleFilterChange = useCallback((key: string, value: any) => {
    setInternalFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (fiscalYearId) params.set("fiscalYearId", String(fiscalYearId));
    if (filters.toDate) params.set("date", filters.toDate);
    window.open(`/reports/provisions/print?${params.toString()}`, "_blank");
  }, [fiscalYearId, filters.toDate]);

  const classified = useMemo(() => {
    if (!data) return null;
    const allLiabilities = data.liabilities || [];
    const provisions = allLiabilities.filter((l: BalanceSheetSectionDto) => {
      const code = l.accountCode || "";
      return code.startsWith("28");
    });
    const otherProvisions = allLiabilities.filter((l: BalanceSheetSectionDto) => {
      const code = l.accountCode || "";
      return code.startsWith("27");
    });
    const totalProvisions = provisions.reduce((s: number, p: BalanceSheetSectionDto) => s + p.balance, 0);
    const totalOtherProvisions = otherProvisions.reduce((s: number, p: BalanceSheetSectionDto) => s + p.balance, 0);
    return {
      provisions,
      otherProvisions,
      totalProvisions,
      totalOtherProvisions,
      totalAllProvisions: totalProvisions + totalOtherProvisions,
    };
  }, [data]);

  const customKPIs = classified ? [
<<<<<<< HEAD
    { label: 'المخصصات والاستحقاقات', value: formatCurrency(classified.totalProvisions), color: 'warning' },
    { label: 'مخصصات أخرى', value: formatCurrency(classified.totalOtherProvisions), color: 'warning' },
    { label: 'إجمالي المخصصات', value: formatCurrency(classified.totalAllProvisions), color: 'danger' },
=======
    { label: 'المخصصات والاستحقاقات', value: formatCurrency(classified.totalProvisions), color: 'text-amber-600' },
    { label: 'مخصصات أخرى', value: formatCurrency(classified.totalOtherProvisions), color: 'text-orange-600' },
    { label: 'إجمالي المخصصات', value: formatCurrency(classified.totalAllProvisions), color: 'text-red-600' },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  ] : [];

  const chartContent = classified ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportPieChart
        data={[
          { name: 'المخصصات والاستحقاقات', value: classified.totalProvisions },
          { name: 'مخصصات أخرى', value: classified.totalOtherProvisions },
        ].filter(d => d.value > 0)}
        nameKey="name"
        valueKey="value"
        title="توزيع المخصصات"
        height={280}
      />
      <ReportBarChart
        data={[
          { name: 'المخصصات', value: classified.totalProvisions },
          { name: 'أخرى', value: classified.totalOtherProvisions },
        ]}
        xKey="name"
        yKeys={[{ key: 'value', color: chartColors.warning, label: 'المبلغ' }]}
        title="مقارنة المخصصات"
        height={280}
      />
    </div>
  ) : undefined;

  const filterBar = hideInternalControls ? undefined : (
    <ReportFilters
      filters={['toDate']}
      values={filters}
      onChange={handleFilterChange}
      onApply={() => refetch()}
      showApplyButton={false}
    />
  );

  return (
    <AccountingReportLayout
      title="المخصصات والاحتياطيات"
      description="عرض المخصصات والاستحقاقات المحاسبية"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (classified ? handlePrint : undefined)}
    >
      {error ? (
        <div className="text-destructive text-sm">فشل في تحميل التقرير</div>
      ) : !classified ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر تاريخاً لعرض المخصصات</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-amber-50">
               <Heading level="h3" className="font-bold text-lg text-amber-800">المخصصات والاستحقاقات</Heading>
              <p className="text-sm text-amber-600 mt-1">الإجمالي: {formatCurrency(classified.totalProvisions)}</p>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.provisions.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد مخصصات</Text>
              ) : (
                <div className="space-y-2">
                  {classified.provisions.map((item: BalanceSheetSectionDto) => (
<<<<<<< HEAD
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border last:border-0">
=======
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{item.accountCode}</span>
                        <span className="text-sm mr-2">{item.accountName}</span>
                      </div>
                      <span className="text-sm font-medium">{formatCurrency(item.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>

          <Card className="border-none rounded-3xl overflow-hidden">
            <div className="p-4 bg-orange-50">
               <Heading level="h3" className="font-bold text-lg text-orange-800">مخصصات أخرى</Heading>
              <p className="text-sm text-orange-600 mt-1">الإجمالي: {formatCurrency(classified.totalOtherProvisions)}</p>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              {classified.otherProvisions.length === 0 ? (
                <Text variant="description" className="text-center">لا توجد مخصصات أخرى</Text>
              ) : (
                <div className="space-y-2">
                  {classified.otherProvisions.map((item: BalanceSheetSectionDto) => (
<<<<<<< HEAD
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border last:border-0">
=======
                    <div key={item.accountID} className="flex items-center justify-between py-2 border-b border-border/30 last:border-0">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                      <div>
                        <span className="text-xs font-mono text-muted-foreground">{item.accountCode}</span>
                        <span className="text-sm mr-2">{item.accountName}</span>
                      </div>
                      <span className="text-sm font-medium">{formatCurrency(item.balance)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </Card>
        </div>
      )}
    </AccountingReportLayout>
  );
}
