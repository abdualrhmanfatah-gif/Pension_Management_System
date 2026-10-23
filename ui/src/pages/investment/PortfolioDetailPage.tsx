import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Briefcase, Building2, FileText,PieChart, ShieldAlert, TrendingUp } from "lucide-react";
import { useEffect } from "react";
import { Link,useParams } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi } from "@/api/investment";
import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { Breadcrumb, ErrorState, Heading, LoadingIndicator, PageLayout, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";

import { PortfolioProjectsTab } from "./tabs/PortfolioProjectsTab";
import { PortfolioPropertiesTab } from "./tabs/PortfolioPropertiesTab";

/**
 * صفحة تفاصيل المحفظة الاستثمارية
 */
export function PortfolioDetailPage() {
    const { id } = useParams<{ id: string }>();
    const portfolioId = Number(id);

    const { data: portfolio, isLoading: loadingPortfolio, error } = useQuery({
        queryKey: ["portfolio", portfolioId],
        queryFn: () => investmentApi.getPortfolio(portfolioId),
        enabled: !!portfolioId
    });

    const { data: summary, isLoading: loadingSummary } = useQuery({
        queryKey: ["portfolio-summary", portfolioId],
        queryFn: () => investmentApi.getPortfolioSummary(portfolioId),
        enabled: !!portfolioId
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل تفاصيل المحفظة", { id: "portfolio-detail" });
    }, [error]);

    const isLoading = loadingPortfolio || loadingSummary;

    if (isLoading) return <LoadingIndicator message="جاري تحميل بيانات المحفظة..." />;
    if (error) {
        return <ErrorState message="تعذر تحميل تفاصيل المحفظة" />;
    }
    if (!portfolio) return <div className="p-6 sm:p-12 text-center text-destructive font-bold">المحفظة غير موجودة</div>;

    return (
        <PageLayout className="text-start">
            <Breadcrumb items={[
                { label: "الاستثمارات", href: "/investment" },
                { label: "المحافظ", href: "/investment/portfolios" },
                { label: portfolio.name }
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to="/investment/portfolios" className="p-2 hover:bg-muted rounded-full transition-colors border">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <Heading level="h1" className="font-black">{portfolio.name}</Heading>
                        <Text variant="description" className="flex items-center gap-2 font-medium">
                            <Briefcase className="w-4 h-4 text-muted-primary" /> المدير المسؤول: {portfolio.managerName}
                        </Text>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard 
                    title="رأس المال المستهدف" 
                    value={`${portfolio.totalTargetCapital.toLocaleString()} ر.ي`} 
                    icon={<Briefcase className="w-4 h-4 text-primary" />} 
                />
                <StatCard 
                    title="الإنفاق الفعلي" 
                    value={`${summary?.totalActualSpent?.toLocaleString() || 0} ر.ي`} 
                    icon={<TrendingUp className="w-4 h-4 text-success" />} 
                    variant="emerald"
                />
                <StatCard 
                    title="عدد المشاريع" 
                    value={summary?.projectCount || 0} 
                    icon={<Building2 className="w-4 h-4 text-info" />} 
                />
                <StatCard 
                    title="نسبة الإشغال" 
                    value={`${summary?.occupancyRate || 0}%`} 
                    icon={<PieChart className="w-4 h-4 text-warning" />} 
                    variant="amber"
                />
            </div>

            <Tabs defaultValue="projects" className="w-full">
                <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
                    <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
                        <TabsTrigger value="projects" className="gap-2 px-4 py-2">
                            <Building2 className="w-4 h-4" /> المشاريع
                        </TabsTrigger>
                        <TabsTrigger value="properties" className="gap-2 px-4 py-2">
                            <ShieldAlert className="w-4 h-4" /> الأصول
                        </TabsTrigger>
                        <TabsTrigger value="finance" className="gap-2 px-4 py-2">
                            <PieChart className="w-4 h-4" /> المالية
                        </TabsTrigger>
                        <TabsTrigger value="documents" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> الوثائق
                        </TabsTrigger>
                    </TabsList>
                </div>

                <TabsContent value="projects" className="mt-0 outline-none">
                    <PortfolioProjectsTab portfolioId={portfolioId} />
                </TabsContent>
                
                <TabsContent value="properties" className="mt-0 outline-none">
                    <PortfolioPropertiesTab portfolioId={portfolioId} />
                </TabsContent>
                
                <TabsContent value="finance" className="mt-0 outline-none">
                    <div className="bg-card border rounded-2xl p-4 sm:p-8 lg:p-16 text-center text-muted-foreground border-dashed">
                        <TrendingUp className="w-12 h-12 mx-auto mb-4 opacity-100" />
                        <p className="text-lg font-bold">التحليل المالي والتدفقات</p>
                        <p className="text-sm font-medium opacity-100">سيتم تفعيل الرسوم البيانية التفصيلية للمحفظة قريباً.</p>
                    </div>
                </TabsContent>
                
                <TabsContent value="documents" className="mt-0 outline-none">
                    <InvestmentDocumentsManager 
                        entityType="investment-portfolio" 
                        entityId={portfolioId} 
                        entityName={portfolio.name}
                    />
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
