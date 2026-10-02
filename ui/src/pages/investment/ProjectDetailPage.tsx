import { useQuery } from "@tanstack/react-query";
import { 
    ArrowRight, 
    Briefcase,
    Building2, 
    DollarSign, 
    FileText,
    LayoutDashboard, 
    ShieldAlert,
    TrendingUp} from "lucide-react";
import { useEffect } from "react";
import { Link,useParams } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi } from "@/api/investment";
import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { Badge, Breadcrumb, ErrorState, Heading, LoadingIndicator, PageLayout, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";
import { PROJECT_STATUS } from "@/constants";

import { ProjectPlansTab } from "./tabs/ProjectPlansTab";
import { ProjectPropertiesTab } from "./tabs/ProjectPropertiesTab";
import { RiskAssessmentsTab } from "./tabs/RiskAssessmentsTab";

/**
 * صفحة تفاصيل المشروع الاستثماري
 */
export function ProjectDetailPage() {
    const { id } = useParams<{ id: string }>();
    const projectId = Number(id);

    const { data: project, isLoading: loadingProject, error } = useQuery({
        queryKey: ["project", projectId],
        queryFn: () => investmentApi.getProject(projectId),
        enabled: !!projectId
    });

    const { data: summary, isLoading: loadingSummary } = useQuery({
        queryKey: ["project-summary", projectId],
        queryFn: () => investmentApi.getProjectSummary(projectId),
        enabled: !!projectId
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل تفاصيل المشروع", { id: "project-detail" });
    }, [error]);

    const isLoading = loadingProject || loadingSummary;

    if (isLoading) return <LoadingIndicator message="جاري تحميل بيانات المشروع..." />;
    if (error) {
        return <ErrorState message="تعذر تحميل بيانات المشروع" />;
    }
    if (!project) return <div className="p-6 sm:p-12 text-center text-destructive font-bold">المشروع غير موجود</div>;

    return (
        <PageLayout className="text-start">
            <Breadcrumb items={[
                { label: "الاستثمارات", href: "/investment" },
                { label: "المشاريع", href: "/investment/projects" },
                { label: project.name }
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to={`/investment/portfolios/${project.portfolioID}`} className="p-2 hover:bg-muted rounded-full transition-colors border " title="العودة للمحفظة">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <div className="flex items-center gap-3">
                            <Heading level="h1" className="font-black">{project.name}</Heading>
                            <Badge variant={project.status === PROJECT_STATUS.ACTIVE ? 'info' : 'outline'} className="rounded-full font-bold">
                                {project.status === PROJECT_STATUS.ACTIVE ? 'نشط' : 'مخطط'}
                            </Badge>
                        </div>
                        <Text variant="description" className="flex items-center gap-2 mt-1 font-medium">
                            <Briefcase className="w-3.5 h-3.5 text-muted-primary" />
                            <span>المحفظة: {project.portfolioName}</span>
                            <span className="opacity-30">|</span>
                            <Building2 className="w-3.5 h-3.5 text-muted-primary" />
                            <span>النوع: {project.typeName}</span>
                        </Text>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard 
                    title="الميزانية المخططة" 
                    value={`${project.plannedBudget.toLocaleString()} ر.ي`} 
                    icon={<DollarSign className="w-4 h-4 text-primary" />} 
                />
                <StatCard 
                    title="الإنفاق الفعلي" 
                    value={`${summary?.totalActualSpent?.toLocaleString() || 0} ر.ي`} 
                    icon={<TrendingUp className="w-4 h-4 text-emerald-500" />} 
                    variant="emerald"
                />
                <StatCard 
                    title="عدد الأصول" 
                    value={summary?.propertyCount || 0} 
                    icon={<Building2 className="w-4 h-4 text-blue-500" />} 
                />
                <StatCard 
                    title="نسبة الإنجاز" 
                    value={`${summary?.progressPercentage || 0}%`} 
                    icon={<LayoutDashboard className="w-4 h-4 text-amber-500" />} 
                    variant="amber"
                />
            </div>

            <Tabs defaultValue="properties" className="w-full">
                <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
                    <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
                        <TabsTrigger value="properties" className="gap-2 px-4 py-2">
                            <Building2 className="w-4 h-4" /> الأصول
                        </TabsTrigger>
                        <TabsTrigger value="plans" className="gap-2 px-4 py-2">
                            <ShieldAlert className="w-4 h-4" /> الموازنة
                        </TabsTrigger>

                        <TabsTrigger value="risks" className="gap-2 px-4 py-2">
                            <ShieldAlert className="w-4 h-4" /> المخاطر
                        </TabsTrigger>
                        <TabsTrigger value="documents" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> الوثائق
                        </TabsTrigger>
                    </TabsList>
                </div>
                
                <TabsContent value="properties" className="mt-0 outline-none">
                    <ProjectPropertiesTab projectId={projectId} />
                </TabsContent>
                
                <TabsContent value="plans" className="mt-0 outline-none">
                    <ProjectPlansTab projectId={projectId} />
                </TabsContent>
                

                
                <TabsContent value="risks" className="mt-0 outline-none">
                    <RiskAssessmentsTab projectId={projectId} />
                </TabsContent>
                
                <TabsContent value="documents" className="mt-0 outline-none">
                    <InvestmentDocumentsManager 
                        entityType="investment-project" 
                        entityId={projectId} 
                    />
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
