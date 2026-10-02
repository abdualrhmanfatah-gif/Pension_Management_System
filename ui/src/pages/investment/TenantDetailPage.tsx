import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, Building2, Edit, FileText, Phone, Star, User } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi } from "@/api/investment";
import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { TenantDialog } from "@/components/domain/investment/TenantDialog";
import { Badge, Breadcrumb, Button, ErrorState, Heading, LoadingIndicator, PageLayout, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";
import { BADGE_VARIANT } from "@/constants";

import { ContractsTab } from "./tabs/ContractsTab";
import { TenantPropertiesTab } from "./tabs/TenantPropertiesTab";

/**
 * صفحة تفاصيل المستأجر
 */
export function TenantDetailPage() {
    const { id } = useParams<{ id: string }>();
    const tenantId = Number(id);
    const queryClient = useQueryClient();
    const [editOpen, setEditOpen] = useState(false);

    const { data: tenant, isLoading, error } = useQuery({
        queryKey: ["tenant", tenantId],
        queryFn: () => investmentApi.getTenant(tenantId),
        enabled: !!tenantId
    });

    const { data: properties } = useQuery({
        queryKey: ["tenant-properties", tenantId],
        queryFn: () => investmentApi.getTenantProperties(tenantId),
        enabled: !!tenantId
    });



    useEffect(() => {
        if (error) toast.error("تعذر تحميل تفاصيل المستأجر", { id: "tenant-detail" });
    }, [error]);

    const getTypeLabel = (type?: string) => {
        switch (type) {
            case "Individual": return "فرد";
            case "Corporate": return "شركة";
            case "Merchant": return "تاجر";
            case "Government": return "حكومي";
            default: return type || "غير محدد";
        }
    };

    if (isLoading) return <LoadingIndicator message="جاري تحميل بيانات المستأجر..." />;
    if (error) {
        return <ErrorState message="تعذر تحميل بيانات المستأجر" />;
    }
    if (!tenant) return <div className="p-6 sm:p-12 text-center text-destructive font-bold">المستأجر غير موجود</div>;

    return (
        <PageLayout className="text-start">
            <Breadcrumb items={[
                { label: "الاستثمارات", href: "/investment" },
                { label: "إدارة التأجير", href: "/investment/leasing" },
                { label: tenant.fullName }
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to="/investment/leasing" className="p-2 hover:bg-muted rounded-full transition-colors border " title="رجوع">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <Heading level="h1" className="font-black">{tenant.fullName}</Heading>
                        <Text variant="description" className="flex items-center gap-2 font-medium">
                            <User className="w-4 h-4 text-muted-primary" /> <Badge variant={BADGE_VARIANT.OUTLINE}>{getTypeLabel(tenant.tenantType)}</Badge>
                        </Text>
                    </div>
                </div>
                <Button variant="outline" onClick={() => setEditOpen(true)}>
                    <Edit className="w-4 h-4 ml-2" /> تعديل البيانات
                </Button>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard
                    title="التقييم"
                    value={`${tenant.ratingScore}`}
                    icon={<Star className="w-4 h-4 text-warning fill-warning" />}
                    variant="amber"
                />
                <StatCard
                    title="العقود النشطة"
                    value={tenant.activeContractsCount || 0}
                    icon={<FileText className="w-4 h-4 text-info" />}
                />
                <StatCard
                    title="رقم التواصل"
                    value={tenant.phone || "—"}
                    icon={<Phone className="w-4 h-4 text-success" />}
                    variant="emerald"
                />
                <StatCard
                    title="العقارات المستأجرة"
                    value={properties?.length || 0}
                    icon={<Building2 className="w-4 h-4 text-special" />}
                />
            </div>

            <Tabs defaultValue="properties" className="w-full">
                <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
                    <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
                        <TabsTrigger value="properties" className="gap-2 px-4 py-2">
                            <Building2 className="w-4 h-4" /> العقارات
                        </TabsTrigger>
                        <TabsTrigger value="contracts" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> العقود
                        </TabsTrigger>
                        <TabsTrigger value="documents" className="gap-2 px-4 py-2">
                            <FileText className="w-4 h-4" /> الوثائق
                        </TabsTrigger>
                    </TabsList>
                </div>

                <TabsContent value="properties" className="mt-0 outline-none">
                    <TenantPropertiesTab tenantId={tenantId} />
                </TabsContent>

                <TabsContent value="contracts" className="mt-0 outline-none">
                    <ContractsTab tenantId={tenantId} />
                </TabsContent>

                <TabsContent value="documents" className="mt-0 outline-none">
                    <InvestmentDocumentsManager
                        entityType="tenant"
                        entityId={tenantId}
                        entityName={tenant.fullName}
                    />
                </TabsContent>
            </Tabs>

            <TenantDialog
                open={editOpen}
                onOpenChange={setEditOpen}
                mode="edit"
                initialData={tenant}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["tenant", tenantId] }); }}
            />
        </PageLayout>
    );
}
