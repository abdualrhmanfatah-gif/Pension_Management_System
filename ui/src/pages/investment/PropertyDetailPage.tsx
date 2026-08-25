import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Building2, Calendar, DollarSign, FileText, History, LayoutDashboard, MapPin, TrendingDown,TrendingUp, Wrench } from "lucide-react";
import { useEffect } from "react";
import { Link,useParams } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi } from "@/api/investment";
import { Breadcrumb, ErrorState, Heading, LoadingIndicator, PageLayout, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";

import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { ContractsTab } from "./tabs/ContractsTab";
import { PropertyActivityLogTab } from "./tabs/PropertyActivityLogTab";
import { PropertyDepreciationTab } from "./tabs/PropertyDepreciationTab";
import { PropertyMaintenanceTab } from "./tabs/PropertyMaintenanceTab";
import { PropertyUnitsTab } from "./tabs/PropertyUnitsTab";
import { PropertyValuationsTab } from "./tabs/PropertyValuationsTab";

/**
 * صفحة تفاصيل الأصل العقاري
 */
export function PropertyDetailPage() {
    const { id } = useParams<{ id: string }>();
    const propertyId = Number(id);

    const { data: property, isLoading, error } = useQuery({
        queryKey: ["property", propertyId],
        queryFn: () => investmentApi.getProperty(propertyId),
        enabled: !!propertyId
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل تفاصيل العقار", { id: "property-detail" });
    }, [error]);

    if (isLoading) return <LoadingIndicator message="جاري تحميل بيانات العقار..." />;
    if (error) {
        return <ErrorState message="تعذر تحميل بيانات العقار" />;
    }
    if (!property) return <div className="p-6 sm:p-12 text-center text-destructive font-bold">العقار غير موجود</div>;

    return (
        <PageLayout className="text-start">
            <Breadcrumb items={[
                { label: "الاستثمارات", href: "/investment" },
                { label: "العقارات", href: "/investment/properties" },
                { label: property.propertyName }
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to="/investment/properties" className="p-2 hover:bg-muted rounded-full transition-colors border " title="رجوع">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <Heading level="h1" className="font-black">{property.propertyName}</Heading>
                        <Text variant="description" className="flex items-center gap-2 font-medium">
                            <Building2 className="w-4 h-4 text-muted-primary" /> {property.propertyTypeName} 
                            <span className="opacity-30 mx-1">|</span>
                            <MapPin className="w-4 h-4 text-muted-primary" /> {property.locationCity}
                        </Text>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard 
                    title="المساحة الإجمالية" 
                    value={`${property.totalArea.toLocaleString('ar-YE')} م²`} 
                    icon={<LayoutDashboard className="w-4 h-4 text-primary" />} 
                />
                <StatCard 
                    title="القيمة السوقية" 
                    value={`${property.currentMarketValue.toLocaleString('ar-YE')} ر.ي`} 
                    icon={<DollarSign className="w-4 h-4 text-emerald-500" />} 
                    variant="emerald"
                />
                <StatCard 
                    title="عدد الوحدات" 
                    value={property.units?.length || 0} 
                    icon={<Building2 className="w-4 h-4 text-blue-500" />} 
                />
                <StatCard 
                    title="تاريخ الاستحواذ" 
                    value={property.acquisitionDate || "—"} 
                    icon={<Calendar className="w-4 h-4 text-amber-500" />} 
                    variant="amber"
                />
            </div>

            <Tabs defaultValue="units" className="w-full">
                <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
                    <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
                        <TabsTrigger value="units" className="gap-2 px-4 py-2">
                            <LayoutDashboard className="w-4 h-4" /> الوحدات
                        </TabsTrigger>
                        <TabsTrigger value="leases" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> العقود
                        </TabsTrigger>
                        <TabsTrigger value="valuations" className="gap-2 px-4 py-2">
                            <TrendingUp className="w-4 h-4" /> التقييمات
                        </TabsTrigger>
                        <TabsTrigger value="depreciation" className="gap-2 px-4 py-2">
                            <TrendingDown className="w-4 h-4" /> الإهلاك
                        </TabsTrigger>
                        <TabsTrigger value="maintenance" className="gap-2 px-4 py-2">
                            <Wrench className="w-4 h-4" /> الصيانة
                        </TabsTrigger>
                        <TabsTrigger value="documents" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> الوثائق
                        </TabsTrigger>
                        <TabsTrigger value="history" className="gap-2 px-4 py-2">
                            <History className="w-4 h-4" /> السجل
                        </TabsTrigger>
                    </TabsList>
                </div>

                <TabsContent value="units" className="mt-0 outline-none">
                    <PropertyUnitsTab propertyId={propertyId} />
                </TabsContent>

                <TabsContent value="leases" className="mt-0 outline-none">
                    <ContractsTab propertyId={propertyId} />
                </TabsContent>

                <TabsContent value="valuations" className="mt-0 outline-none">
                    <PropertyValuationsTab propertyId={propertyId} />
                </TabsContent>

                <TabsContent value="depreciation" className="mt-0 outline-none">
                    <PropertyDepreciationTab propertyId={propertyId} />
                </TabsContent>

                <TabsContent value="maintenance" className="mt-0 outline-none">
                    <PropertyMaintenanceTab propertyId={propertyId} />
                </TabsContent>

                <TabsContent value="documents" className="mt-0 outline-none">
                    <InvestmentDocumentsManager 
                        entityType="investment-property" 
                        entityId={propertyId}
                        entityName={property.propertyName}
                    />
                </TabsContent>

                <TabsContent value="history" className="mt-0 outline-none">
                    <PropertyActivityLogTab propertyId={propertyId} />
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
