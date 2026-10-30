<<<<<<< HEAD
﻿import { useQuery } from "@tanstack/react-query";
import { Building2 } from "lucide-react";
import { useMemo,useState } from "react";

import { fixedAssetApi } from "@/api/fixedAssets";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import { Card, Heading } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
=======
import { useQuery } from "@tanstack/react-query";
import { useState, useMemo } from "react";
import { Building2 } from "lucide-react";

import { fixedAssetApi } from "@/api/fixedAssets";
import { formatCurrency } from "@/lib/utils";
import { Card, Heading } from "@/components/ui";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportFilters } from "@/components/common/ReportFilters";
import { ReportBarChart, ReportPieChart } from "@/components/common/charts";
import { chartColors } from "@/lib/chartColors";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface FixedAssetsReportProps {
  fiscalYearId?: number;
  asOfDate?: string;
  externalFilters?: { assetType?: string };
  hideInternalControls?: boolean;
}

export function FixedAssetsReport(_props: FixedAssetsReportProps) {
  const externalFilters = _props.externalFilters;
  const hideInternalControls = _props.hideInternalControls;
  const [internalFilters, setInternalFilters] = useState<Record<string, any>>({ assetType: 'all' });

  const filters = {
    ...internalFilters,
    ...(externalFilters?.assetType !== undefined ? { assetType: externalFilters.assetType } : {}),
  };

  const { data: assets = [], isLoading: loadingAssets } = useQuery({
    queryKey: ["fixed-assets"],
    queryFn: () => fixedAssetApi.getAll(),
  });

  const { data: assetTypes = [] } = useQuery({
    queryKey: ["asset-types"],
    queryFn: () => fixedAssetApi.getTypes(),
  });

  const filteredAssets = useMemo(() => {
    if (filters.assetType === 'all' || !filters.assetType) return assets;
    return assets.filter(a => a.assetTypeID === Number(filters.assetType));
  }, [assets, filters.assetType]);

  const stats = useMemo(() => {
    const activeAssets = filteredAssets.filter(a => a.status === 0);
    const disposedAssets = filteredAssets.filter(a => a.status === 1);
    return {
      totalCost: activeAssets.reduce((s, a) => s + a.acquiredCost, 0),
      totalAccumulated: activeAssets.reduce((s, a) => s + a.accumulatedDepreciation, 0),
      totalNetBook: activeAssets.reduce((s, a) => s + a.netBookValue, 0),
      activeCount: activeAssets.length,
      disposedCount: disposedAssets.length,
      totalCount: filteredAssets.length,
    };
  }, [filteredAssets]);

  const handleFilterChange = (key: string, value: any) => {
    setInternalFilters(prev => ({ ...prev, [key]: value }));
  };

  const handlePrint = () => {
    const params = new URLSearchParams();
    if (filters.assetType !== 'all') params.set("typeId", filters.assetType!);
    window.open(`/reports/fixed-assets/print?${params.toString()}`, "_blank");
  };

  const customKPIs = [
<<<<<<< HEAD
    { label: 'عدد الأصول النشطة', value: stats.activeCount, color: 'default' },
    { label: 'إجمالي التكلفة', value: formatCurrency(stats.totalCost), color: 'info' },
    { label: 'الإهلاك المتراكم', value: formatCurrency(stats.totalAccumulated), color: 'danger' },
    { label: 'القيمة الدفترية', value: formatCurrency(stats.totalNetBook), color: 'success' },
=======
    { label: 'عدد الأصول النشطة', value: stats.activeCount, color: 'text-foreground' },
    { label: 'إجمالي التكلفة', value: formatCurrency(stats.totalCost), color: 'text-blue-600' },
    { label: 'الإهلاك المتراكم', value: formatCurrency(stats.totalAccumulated), color: 'text-red-600' },
    { label: 'القيمة الدفترية', value: formatCurrency(stats.totalNetBook), color: 'text-green-600' },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  ];

  const chartContent = filteredAssets.length > 0 ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportBarChart
        data={assetTypes.map(t => ({
          name: t.typeName,
          cost: filteredAssets.filter(a => a.assetTypeID === t.assetTypeID).reduce((s, a) => s + a.acquiredCost, 0),
          depreciation: filteredAssets.filter(a => a.assetTypeID === t.assetTypeID).reduce((s, a) => s + a.accumulatedDepreciation, 0),
        }))}
        xKey="name"
        yKeys={[
          { key: 'cost', color: chartColors.primary, label: 'التكلفة' },
          { key: 'depreciation', color: chartColors.danger, label: 'الإهلاك' },
        ]}
        title="التكلفة والإهلاك حسب النوع"
        height={280}
      />
      <ReportPieChart
        data={assetTypes.map(t => ({
          name: t.typeName,
          value: filteredAssets.filter(a => a.assetTypeID === t.assetTypeID && a.status === 0).length,
        })).filter(d => d.value > 0)}
        nameKey="name"
        valueKey="value"
        title="توزيع الأصول حسب النوع"
        height={280}
      />
    </div>
  ) : undefined;

  const filterBar = hideInternalControls ? undefined : (
    <ReportFilters
      filters={['assetType']}
      values={filters}
      onChange={handleFilterChange}
      showApplyButton={false}
    />
  );

  return (
    <AccountingReportLayout
      title="الأصول الثابتة"
      description="عرض سجل الأصول الثابتة والإهلاك"
      isLoading={loadingAssets}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (filteredAssets.length > 0 ? handlePrint : undefined)}
    >
      <Card className="border-none rounded-3xl overflow-hidden">
        <div className="p-4 bg-muted/30 flex items-center gap-2">
          <Building2 className="w-5 h-5 text-primary" />
          <Heading level="h3" className="font-bold text-lg">سجل الأصول الثابتة</Heading>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm" role="table" aria-label="سجل الأصول الثابتة">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-3">رقم الأصل</th>
                <th className="text-end p-3">اسم الأصل</th>
                <th className="text-end p-3">التصنيف</th>
                <th className="text-end p-3">تاريخ الشراء</th>
                <th className="text-end p-3">التكلفة</th>
                <th className="text-end p-3">نسبة الإهلاك</th>
                <th className="text-end p-3">الإهلاك المتراكم</th>
                <th className="text-end p-3">القيمة الدفترية</th>
                <th className="text-end p-3">الحالة</th>
              </tr>
            </thead>
            <tbody>
              {stats.activeCount === 0 ? (
                <tr>
                  <td colSpan={9} className="p-8 text-center text-muted-foreground">
                    لا توجد أصول ثابتة
                  </td>
                </tr>
              ) : (
                filteredAssets
                  .filter(a => a.status === 0)
                  .map((asset) => (
<<<<<<< HEAD
                    <tr key={asset.assetID} className="border-t border-border hover:bg-muted/20">
=======
                    <tr key={asset.assetID} className="border-t border-border/50 hover:bg-muted/20">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                      <td className="p-3 font-mono text-xs">{asset.assetNumber}</td>
                      <td className="p-3 font-medium">{asset.assetName}</td>
                      <td className="p-3 text-muted-foreground">{asset.categoryName || "—"}</td>
                      <td className="p-3 text-xs">
                        {new Date(asset.inServiceDate).toLocaleDateString("ar-SA")}
                      </td>
                      <td className="p-3 font-mono">{formatCurrency(asset.acquiredCost)}</td>
                      <td className="p-3">{asset.depreciationRate}%</td>
                      <td className="p-3 font-mono text-destructive">{formatCurrency(asset.accumulatedDepreciation)}</td>
                      <td className="p-3 font-mono text-emerald-600 dark:text-emerald-400 font-bold">{formatCurrency(asset.netBookValue)}</td>
                      <td className="p-3">
                        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                          نشط
                        </span>
                      </td>
                    </tr>
                  ))
              )}
            </tbody>
            <tfoot className="bg-muted/50 font-bold">
              <tr>
                <td className="p-3" colSpan={4}>الإجمالي</td>
                <td className="p-3 font-mono">{formatCurrency(stats.totalCost)}</td>
                <td className="p-3"></td>
                <td className="p-3 font-mono text-destructive">{formatCurrency(stats.totalAccumulated)}</td>
                <td className="p-3 font-mono text-emerald-600 dark:text-emerald-400">{formatCurrency(stats.totalNetBook)}</td>
                <td className="p-3"></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </Card>
    </AccountingReportLayout>
  );
}
