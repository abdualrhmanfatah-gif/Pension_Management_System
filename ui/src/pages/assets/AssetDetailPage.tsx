import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Building2, Calendar, DollarSign, FileText, Laptop, LandPlot, Monitor, Sofa, TrendingDown, Truck } from "lucide-react";
import { useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";

import { fixedAssetApi } from "@/api/fixedAssets";
import { Breadcrumb, ErrorState, Heading, LoadingIndicator, PageLayout, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";
import { formatCurrency } from "@/lib/utils";
import { ASSET_TYPE_ICONS } from "@/constants/assets";

import { AssetDepreciationTab } from "./tabs/AssetDepreciationTab";
import { AssetInfoTab } from "./tabs/AssetInfoTab";

// Icon mapping from string name to component
const ICON_MAP: Record<string, React.ReactNode> = {
    Laptop: <Laptop className="w-4 h-4" />,
    Truck: <Truck className="w-4 h-4" />,
    Building2: <Building2 className="w-4 h-4" />,
    LandPlot: <LandPlot className="w-4 h-4" />,
    Sofa: <Sofa className="w-4 h-4" />,
};


export function AssetDetailPage() {
    const { id } = useParams<{ id: string }>();
    const assetId = Number(id);

    const { data: asset, isLoading, error } = useQuery({
        queryKey: ["fixed-asset", assetId],
        queryFn: () => fixedAssetApi.getById(assetId),
        enabled: !!assetId
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل تفاصيل الأصل", { id: "asset-detail" });
    }, [error]);

    if (isLoading) return <LoadingIndicator message="جاري تحميل بيانات الأصل..." />;
    if (error) {
        return (
            <div dir="rtl">
                <ErrorState message="تعذر تحميل تفاصيل الأصل" />
            </div>
        );
    }
    if (!asset) return <div                     className="p-6 sm:p-12 text-center text-destructive font-bold">الأصل غير موجود</div>;

    return (
        <PageLayout className="text-start">
            <Breadcrumb items={[
                { label: "الأصول الثابتة", href: "/assets/fixed" },
                { label: asset.assetName }
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to="/assets/fixed" className="p-2 hover:bg-muted rounded-full transition-colors border " aria-label="رجوع">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div className="w-12 h-12 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0 text-primary">
                        {ICON_MAP[ASSET_TYPE_ICONS[asset.assetTypeID]] || <Monitor className="w-6 h-6" />}
                    </div>
                    <div>
                        <Heading level="h1" className="font-black">{asset.assetName}</Heading>
                        <Text variant="description" className="flex items-center gap-2 font-medium">
                            <span className="font-mono text-xs">{asset.assetNumber}</span>
                            <span className="opacity-30 mx-1">|</span>
                            {asset.categoryName || "بدون تصنيف"}
                        </Text>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard
                    title="التكلفة المكتسبة"
                    value={formatCurrency(asset.acquiredCost)}
                    icon={<DollarSign className="w-4 h-4 text-primary" />}
                />
                <StatCard
                    title="الإهلاك المتراكم"
                    value={formatCurrency(asset.accumulatedDepreciation)}
                    icon={<TrendingDown className="w-4 h-4 text-red-500" />}
                    variant="destructive"
                />
                <StatCard
                    title="القيمة الدفترية"
                    value={formatCurrency(asset.netBookValue)}
                    icon={<DollarSign className="w-4 h-4 text-emerald-500" />}
                    variant="emerald"
                />
                <StatCard
                    title="نسبة الإهلاك السنوية"
                    value={`${asset.depreciationRate}%`}
                    icon={<Calendar className="w-4 h-4 text-amber-500" />}
                    variant="amber"
                />
            </div>

            <Tabs defaultValue="info" className="w-full">
                <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
                    <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
                        <TabsTrigger value="info" className="gap-2 px-4 py-2">
                            <Building2 className="w-4 h-4" /> البيانات الأساسية
                        </TabsTrigger>
                        <TabsTrigger value="depreciation" className="gap-2 px-4 py-2">
                            <TrendingDown className="w-4 h-4" /> الإهلاك
                        </TabsTrigger>
                        <TabsTrigger value="documents" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> الوثائق
                        </TabsTrigger>
                    </TabsList>
                </div>

                <TabsContent value="info" className="mt-0 outline-none">
                    <AssetInfoTab asset={asset} />
                </TabsContent>

                <TabsContent value="depreciation" className="mt-0 outline-none">
                    <AssetDepreciationTab assetId={assetId} />
                </TabsContent>

                <TabsContent value="documents" className="mt-0 outline-none">
                    <div className="text-center py-12 text-muted-foreground">
                        <FileText className="w-12 h-12 mx-auto mb-4 opacity-50" />
                        <Text variant="body">إدارة الوثائق قادمة قريباً</Text>
                    </div>
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
