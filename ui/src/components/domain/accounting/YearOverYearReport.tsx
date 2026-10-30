<<<<<<< HEAD
﻿import { useQuery } from "@tanstack/react-query";
import { useCallback,useEffect, useState } from "react";

import { fiscalYearsApi,reportsApi } from "@/api/accounting";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { Card, Heading, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type { FiscalYearSummaryDto } from "@/types/accounting";
=======
import { useQuery } from "@tanstack/react-query";
import { useState, useEffect, useCallback } from "react";

import { reportsApi, fiscalYearsApi } from "@/api/accounting";
import type { FiscalYearSummaryDto } from "@/types/accounting";
import { formatCurrency } from "@/lib/utils";
import { Card, Heading, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface YearOverYearReportProps {
  fiscalYearId?: number;
  externalFilters?: { comparisonYear?: number };
  hideInternalControls?: boolean;
}

export function YearOverYearReport({ fiscalYearId, externalFilters, hideInternalControls }: YearOverYearReportProps) {
  const [year1Id, setYear1Id] = useState<number | undefined>(fiscalYearId);
  const [year2Id, setYear2Id] = useState<number | undefined>();

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  useEffect(() => {
    if (externalFilters?.comparisonYear && fiscalYears) {
      const match = fiscalYears.find((y: FiscalYearSummaryDto) => y.yearName === String(externalFilters.comparisonYear));
      if (match) setYear2Id(match.fiscalYearID);
    } else if (fiscalYears && fiscalYears.length >= 2 && !year2Id) {
      const currentYear = new Date().getFullYear().toString();
      const current = fiscalYears.find((y: FiscalYearSummaryDto) => y.yearName === currentYear);
      const previous = fiscalYears.find((y: FiscalYearSummaryDto) => y.yearName === String(Number(currentYear) - 1));
      if (current && previous) {
        setYear1Id(current.fiscalYearID);
        setYear2Id(previous.fiscalYearID);
      } else if (fiscalYears.length >= 2) {
        setYear1Id(fiscalYears[0].fiscalYearID);
        setYear2Id(fiscalYears[1].fiscalYearID);
      }
    }
  }, [fiscalYears]);

  const { data: income1, isLoading: loading1 } = useQuery({
    queryKey: ["reports", "income-statement", year1Id],
    queryFn: () => reportsApi.incomeStatement(year1Id!),
    enabled: !!year1Id,
  });

  const { data: income2, isLoading: loading2 } = useQuery({
    queryKey: ["reports", "income-statement", year2Id],
    queryFn: () => reportsApi.incomeStatement(year2Id!),
    enabled: !!year2Id,
  });

  const isLoading = loading1 || loading2;

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (year1Id) params.set("year1Id", String(year1Id));
    if (year2Id) params.set("year2Id", String(year2Id));
    window.open(`/reports/year-over-year/print?${params.toString()}`, "_blank");
  }, [year1Id, year2Id]);

  const year1 = fiscalYears?.find((y: FiscalYearSummaryDto) => y.fiscalYearID === year1Id);
  const year2 = fiscalYears?.find((y: FiscalYearSummaryDto) => y.fiscalYearID === year2Id);

  const revenueChange = income2 && income2.totalRevenue !== 0
    ? ((income1!.totalRevenue - income2.totalRevenue) / Math.abs(income2.totalRevenue)) * 100
    : 0;
  const expenseChange = income2 && income2.totalExpenses !== 0
    ? ((income1!.totalExpenses - income2.totalExpenses) / Math.abs(income2.totalExpenses)) * 100
    : 0;
  const netIncomeChange = income2 && income2.netIncome !== 0
    ? ((income1!.netIncome - income2.netIncome) / Math.abs(income2.netIncome)) * 100
    : 0;

  const customKPIs = income1 && income2 ? [
    { label: 'الإيرادات', value: `${formatCurrency(income1.totalRevenue)} (${revenueChange >= 0 ? '+' : ''}${revenueChange.toFixed(1)}%)`, color: revenueChange >= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'المصروفات', value: `${formatCurrency(income1.totalExpenses)} (${expenseChange >= 0 ? '+' : ''}${expenseChange.toFixed(1)}%)`, color: expenseChange <= 0 ? 'text-green-600' : 'text-red-600' },
    { label: 'صافي الدخل', value: `${formatCurrency(income1.netIncome)} (${netIncomeChange >= 0 ? '+' : ''}${netIncomeChange.toFixed(1)}%)`, color: netIncomeChange >= 0 ? 'text-green-600' : 'text-red-600' },
<<<<<<< HEAD
    { label: 'السنة المقارنة', value: `${year1?.yearName || ''} vs ${year2?.yearName || ''}`, color: 'primary' },
=======
    { label: 'السنة المقارنة', value: `${year1?.yearName || ''} vs ${year2?.yearName || ''}`, color: 'text-primary' },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  ] : [];

  const chartContent = income1 && income2 ? (
    <ReportBarChart
      data={[
        { name: 'الإيرادات', [year1?.yearName || 'الحالية']: income1.totalRevenue, [year2?.yearName || 'السابقة']: income2.totalRevenue },
        { name: 'المصروفات', [year1?.yearName || 'الحالية']: income1.totalExpenses, [year2?.yearName || 'السابقة']: income2.totalExpenses },
        { name: 'صافي الدخل', [year1?.yearName || 'الحالية']: income1.netIncome, [year2?.yearName || 'السابقة']: income2.netIncome },
      ]}
      xKey="name"
      yKeys={[
        { key: year1?.yearName || 'الحالية', color: chartColors.primary, label: year1?.yearName || 'الحالية' },
        { key: year2?.yearName || 'السابقة', color: chartColors.success, label: year2?.yearName || 'السابقة' },
      ]}
      title="مقارنة الأداء السنوي"
      height={300}
    />
  ) : undefined;

  const filterBar = hideInternalControls ? undefined : (
    <Card className="p-4 flex flex-wrap gap-4 items-end mb-6">
      <div className="w-full sm:w-48">
<<<<<<< HEAD
        <label className="label-filter">السنة الحالية</label>
=======
        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">السنة الحالية</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        <Select
          value={year1Id ? String(year1Id) : ''}
          onValueChange={(value) => setYear1Id(Number(value))}
        >
          <SelectTrigger className="w-full">
            <SelectValue placeholder="اختر السنة" />
          </SelectTrigger>
          <SelectContent>
            {fiscalYears?.map((y: FiscalYearSummaryDto) => (
              <SelectItem key={y.fiscalYearID} value={String(y.fiscalYearID)}>
                {y.yearName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="w-full sm:w-48">
<<<<<<< HEAD
        <label className="label-filter">السنة السابقة</label>
=======
        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">السنة السابقة</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        <Select
          value={year2Id?.toString() ?? ''}
          onValueChange={(val) => setYear2Id(Number(val))}
        >
          <SelectTrigger>
            <SelectValue placeholder="اختر السنة" />
          </SelectTrigger>
          <SelectContent>
            {fiscalYears?.map((y: FiscalYearSummaryDto) => (
              <SelectItem key={y.fiscalYearID} value={y.fiscalYearID.toString()}>{y.yearName}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </Card>
  );

  return (
    <AccountingReportLayout
      title="المقارنة السنوية"
      description="مقارنة الأداء المالي بين سنتين ماليتين"
      isLoading={isLoading}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (income1 && income2 ? handlePrint : undefined)}
    >
      {!income1 || !income2 ? (
        <div className="text-muted-foreground text-sm p-8 text-center">اختر سنتين ماليتين للمقارنة</div>
      ) : (
        <Card className="border-none rounded-3xl overflow-hidden">
          <div className="p-4 bg-muted/30">
            <Heading level="h3" className="font-bold text-lg">مقارنة تفصيلية: {year1?.yearName} vs {year2?.yearName}</Heading>
          </div>
          <table className="w-full text-sm" role="table" aria-label="مقارنة سنوية">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-3">البند</th>
                <th className="text-end p-3">{year1?.yearName}</th>
                <th className="text-end p-3">{year2?.yearName}</th>
                <th className="text-end p-3">الفرق</th>
                <th className="text-end p-3">النسبة</th>
              </tr>
            </thead>
            <tbody>
<<<<<<< HEAD
              <tr className="border-t border-border bg-emerald-50/50 dark:bg-emerald-950/20">
=======
              <tr className="border-t border-border/50 bg-emerald-50/50 dark:bg-emerald-950/20">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <td className="p-3 font-medium">إجمالي الإيرادات</td>
                <td className="p-3">{formatCurrency(income1.totalRevenue)}</td>
                <td className="p-3">{formatCurrency(income2.totalRevenue)}</td>
                <td className={`p-3 font-medium ${revenueChange >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {formatCurrency(income1.totalRevenue - income2.totalRevenue)}
                </td>
                <td className={`p-3 ${revenueChange >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {revenueChange >= 0 ? "+" : ""}{revenueChange.toFixed(1)}%
                </td>
              </tr>
<<<<<<< HEAD
              <tr className="border-t border-border bg-red-50/50 dark:bg-red-950/20">
=======
              <tr className="border-t border-border/50 bg-red-50/50 dark:bg-red-950/20">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <td className="p-3 font-medium">إجمالي المصروفات</td>
                <td className="p-3">{formatCurrency(income1.totalExpenses)}</td>
                <td className="p-3">{formatCurrency(income2.totalExpenses)}</td>
                <td className={`p-3 font-medium ${expenseChange <= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {formatCurrency(income1.totalExpenses - income2.totalExpenses)}
                </td>
                <td className={`p-3 ${expenseChange <= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {expenseChange >= 0 ? "+" : ""}{expenseChange.toFixed(1)}%
                </td>
              </tr>
<<<<<<< HEAD
              <tr className="border-t border-border font-bold">
=======
              <tr className="border-t border-border/50 font-bold">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <td className="p-3">صافي الدخل</td>
                <td className={`p-3 ${income1.netIncome >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {formatCurrency(income1.netIncome)}
                </td>
                <td className={`p-3 ${income2.netIncome >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {formatCurrency(income2.netIncome)}
                </td>
                <td className={`p-3 ${netIncomeChange >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {formatCurrency(income1.netIncome - income2.netIncome)}
                </td>
                <td className={`p-3 ${netIncomeChange >= 0 ? "text-emerald-600 dark:text-emerald-400" : "text-destructive"}`}>
                  {netIncomeChange >= 0 ? "+" : ""}{netIncomeChange.toFixed(1)}%
                </td>
              </tr>
            </tbody>
          </table>
        </Card>
      )}
    </AccountingReportLayout>
  );
}
