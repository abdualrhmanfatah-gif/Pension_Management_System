import { useQuery } from "@tanstack/react-query";
import { TrendingDown } from "lucide-react";
import { useCallback,useMemo, useState } from "react";

import { fixedAssetApi } from "@/api/fixedAssets";
import { AccountingReportLayout } from "@/components/common/AccountingReportLayout";
import { ReportBarChart, ReportLineChart } from "@/components/common/charts";
import { ReportFilters } from "@/components/common/ReportFilters";
import { Card, Heading } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";

interface DepreciationReportProps {
  fiscalYearId?: number;
  externalFilters?: { assetType?: string };
  hideInternalControls?: boolean;
}

export function DepreciationReport(_props: DepreciationReportProps) {
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
    return {
      totalCost: activeAssets.reduce((s, a) => s + a.acquiredCost, 0),
      totalAccumulated: activeAssets.reduce((s, a) => s + a.accumulatedDepreciation, 0),
      totalNetBook: activeAssets.reduce((s, a) => s + a.netBookValue, 0),
      count: activeAssets.length,
    };
  }, [filteredAssets]);

  const handleFilterChange = useCallback((key: string, value: any) => {
    setInternalFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const handlePrint = useCallback(() => {
    const params = new URLSearchParams();
    if (filters.assetType !== 'all') params.set("typeId", filters.assetType!);
    window.open(`/reports/depreciation/print?${params.toString()}`, "_blank");
  }, [filters.assetType]);

  const customKPIs = [
    { label: 'عدد الأصول النشطة', value: stats.count, color: 'text-foreground' },
    { label: 'إجمالي التكلفة', value: formatCurrency(stats.totalCost), color: 'text-blue-600' },
    { label: 'الإهلاك المتراكم', value: formatCurrency(stats.totalAccumulated), color: 'text-red-600' },
    { label: 'القيمة الدفترية', value: formatCurrency(stats.totalNetBook), color: 'text-green-600' },
  ];

  const chartContent = filteredAssets.filter(a => a.status === 0).length > 0 ? (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <ReportBarChart
        data={assetTypes.map(t => ({
          name: t.typeName,
          cost: filteredAssets.filter(a => a.assetTypeID === t.assetTypeID && a.status === 0).reduce((s, a) => s + a.acquiredCost, 0),
          depreciation: filteredAssets.filter(a => a.assetTypeID === t.assetTypeID && a.status === 0).reduce((s, a) => s + a.accumulatedDepreciation, 0),
          netBook: filteredAssets.filter(a => a.assetTypeID === t.assetTypeID && a.status === 0).reduce((s, a) => s + a.netBookValue, 0),
        }))}
        xKey="name"
        yKeys={[
          { key: 'cost', color: chartColors.primary, label: 'التكلفة' },
          { key: 'depreciation', color: chartColors.danger, label: 'الإهلاك' },
          { key: 'netBook', color: chartColors.success, label: 'القيمة الدفترية' },
        ]}
        title="الإهلاك حسب نوع الأصل"
        height={300}
      />
      <ReportLineChart
        data={filteredAssets
          .filter(a => a.status === 0)
          .sort((a, b) => new Date(a.inServiceDate).getTime() - new Date(b.inServiceDate).getTime())
          .slice(0, 10)
          .map(a => ({
            name: a.assetName.length > 15 ? a.assetName.substring(0, 15) + '...' : a.assetName,
            cost: a.acquiredCost,
            depreciation: a.accumulatedDepreciation,
            netBook: a.netBookValue,
          }))}
        xKey="name"
        lines={[
          { key: 'cost', color: chartColors.primary, label: 'التكلفة' },
          { key: 'depreciation', color: chartColors.danger, label: 'الإهلاك' },
          { key: 'netBook', color: chartColors.success, label: 'القيمة الدفترية' },
        ]}
        title="أعلى 10 أصول حسب التكلفة"
        height={300}
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
      title="تقرير الإهلاك"
      description="عرض إهلاك الأصول الثابتة والقيمة الدفترية"
      isLoading={loadingAssets}
      customKPIs={customKPIs}
      filterBar={filterBar}
      chartContent={chartContent}
      onPrint={hideInternalControls ? undefined : (stats.count > 0 ? handlePrint : undefined)}
    >
      <Card className="border-none rounded-3xl overflow-hidden">
        <div className="p-4 bg-muted/30 flex items-center gap-2">
          <TrendingDown className="w-5 h-5 text-primary" />
          <Heading level="h3" className="font-bold text-lg">جدول إهلاك الأصول الثابتة</Heading>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm" role="table" aria-label="جدول الإهلاك">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-3">رقم الأصل</th>
                <th className="text-end p-3">اسم الأصل</th>
                <th className="text-end p-3">التصنيف</th>
                <th className="text-end p-3">التكلفة</th>
                <th className="text-end p-3">نسبة الإهلاك</th>
                <th className="text-end p-3">الإهلاك المتراكم</th>
                <th className="text-end p-3">القيمة الدفترية</th>
                <th className="text-end p-3">آخر إهلاك</th>
              </tr>
            </thead>
            <tbody>
              {filteredAssets.filter(a => a.status === 0).length === 0 ? (
                <tr>
                  <td colSpan={8} className="p-8 text-center text-muted-foreground">
                    لا توجد أصول نشطة
                  </td>
                </tr>
              ) : (
                filteredAssets
                  .filter(a => a.status === 0)
                  .map((asset) => (
                    <tr key={asset.assetID} className="border-t border-border/50 hover:bg-muted/20">
                      <td className="p-3 font-mono text-xs">{asset.assetNumber}</td>
                      <td className="p-3 font-medium">{asset.assetName}</td>
                      <td className="p-3 text-muted-foreground">{asset.categoryName || "—"}</td>
                      <td className="p-3 font-mono">{formatCurrency(asset.acquiredCost)}</td>
                      <td className="p-3">{asset.depreciationRate}%</td>
                      <td className="p-3 font-mono text-destructive">{formatCurrency(asset.accumulatedDepreciation)}</td>
                      <td className="p-3 font-mono text-emerald-600 dark:text-emerald-400 font-bold">{formatCurrency(asset.netBookValue)}</td>
                      <td className="p-3 text-xs text-muted-foreground">
                        {asset.lastDepreciationDate
                          ? new Date(asset.lastDepreciationDate).toLocaleDateString("ar-SA")
                          : "لم يتم بعد"}
                      </td>
                    </tr>
                  ))
              )}
            </tbody>
            <tfoot className="bg-muted/50 font-bold">
              <tr>
                <td className="p-3" colSpan={3}>الإجمالي</td>
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
