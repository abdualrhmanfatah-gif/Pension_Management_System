import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye } from "lucide-react";
import { useEffect, useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";

import type { PropertyDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { PropertyDialog } from "@/components/domain/investment/PropertyDialog";
import { CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

interface PortfolioPropertiesTabProps {
    portfolioId: number;
}

/**
 * تبويب عقارات المحفظة الاستثمارية
 */
export function PortfolioPropertiesTab({ portfolioId }: PortfolioPropertiesTabProps) {
    const navigate = useNavigate();
    const canManage = useHasPermission("Investment.Manage");
    const crud = useCrudState<PropertyDto>();
    const queryClient = useQueryClient();

    // 1. جلب مشاريع هذه المحفظة
    const { data: projectsData } = useQuery({
        queryKey: ["portfolio-projects", portfolioId],
        queryFn: () => investmentApi.getProjectsByPortfolio(portfolioId).then(res => res || [])
    });

    const { data: propertiesData, isLoading, error } = useQuery({
        queryKey: ["properties"],
        queryFn: () => investmentApi.getProperties().then(res => res || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل العقارات", { id: "portfolio-properties" });
    }, [error]);

    const projects = useMemo(() => projectsData || [], [projectsData]);
    const allProperties = useMemo(() => propertiesData || [], [propertiesData]);
    const projectIds = useMemo(() => projects.map(p => p.projectID), [projects]);

    // تصفية العقارات التابعة لمشاريع هذه المحفظة
    const portfolioProperties = useMemo(() =>
        allProperties.filter(prop => prop.projectID && projectIds.includes(prop.projectID)),
        [allProperties, projectIds]
    );

    const columns = useMemo<CustomColumnDef<PropertyDto>[]>(() => [
        {
            key: "propertyID",
            header: "رقم العقار",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.propertyID}</span>
        },
        {
            key: "propertyName",
            header: "اسم العقار",
            render: (row) => <span className="font-bold">{row.propertyName}</span>
        },
        { key: "projectName", header: "المشروع التابع", accessor: (row) => row.projectName || "—" },
        { key: "propertyTypeName", header: "نوع العقار", accessor: (row) => row.propertyTypeName },
        { key: "locationCity", header: "المدينة", accessor: (row) => row.locationCity || "—" },
        {
            key: "currentMarketValue",
            header: "القيمة السوقية",
            render: (row) => (
                <span className="font-semibold text-success font-mono">
                    {row.currentMarketValue.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <Link
                    to={`/investment/properties/${row.propertyID}`}
                    className="p-2 hover:bg-muted rounded-md text-primary transition-colors block w-fit"
                    title="عرض تفاصيل العقار"
                    onClick={(e) => e.stopPropagation()}
                >
                    <Eye className="w-4 h-4" />
                </Link>
            )
        }
    ], []);

    if (error) {
        return <ErrorState message="خطأ في تحميل العقارات" />;
    }

    return (
        <CrudPageLayout
            title="الأصول العقارية للمحفظة"
            description="عرض كافة العقارات والمجمعات المرتبطة بمشاريع هذه المحفظة"
            createLabel="إضافة عقار"
            onCreateClick={canManage ? crud.openCreate : undefined}
            canCreate={canManage}
            columns={columns}
            data={portfolioProperties}
            loading={isLoading}
            emptyMessage="لم يتم العثور على عقارات مرتبطة بهذه المحفظة."
            onRowClick={(row) => navigate(`/investment/properties/${row.propertyID}`)}
        >
            <PropertyDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem || undefined}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["properties"] }); }}
            />
        </CrudPageLayout>
    );
}
