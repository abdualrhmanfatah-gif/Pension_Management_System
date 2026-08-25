import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye, FileText } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";
import { Link, useNavigate } from "react-router-dom";

import type { PropertyDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { Button, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { PropertyDialog } from "@/components/domain/investment/PropertyDialog";
import { useCrudState } from "@/hooks";

interface ProjectPropertiesTabProps {
    projectId: number;
}

/**
 * تبويب عقارات المشروع الاستثماري
 */
export function ProjectPropertiesTab({ projectId }: ProjectPropertiesTabProps) {
    const navigate = useNavigate();
    const crud = useCrudState<PropertyDto>();
    const queryClient = useQueryClient();

    // جلب العقارات
    const { data: propertiesData, isLoading, error } = useQuery({
        queryKey: ["project-properties", projectId],
        queryFn: () => investmentApi.getProperties(projectId).then(res => res || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل عقارات المشروع", { id: "project-properties" });
    }, [error]);

    const properties = propertiesData || [];

    const columns = useMemo<CustomColumnDef<PropertyDto>[]>(() => [
        {
            key: "propertyID",
            header: "رقم العقار",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.propertyID}</span>
        },
        {
            key: "propertyName",
            header: "اسم العقار",
            render: (row) => <span className="font-bold text-primary">{row.propertyName}</span>
        },
        { key: "propertyTypeName", header: "النوع", accessor: (row) => row.propertyTypeName },
        { key: "locationCity", header: "المدينة", accessor: (row) => row.locationCity || "—" },
        {
            key: "totalArea",
            header: "المساحة",
            render: (row) => (
                <span className="font-medium font-mono">{row.totalArea.toLocaleString()} م²</span>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق">
                        <FileText className="w-4 h-4 text-primary" />
                    </Button>
                    <Link
                        to={`/investment/properties/${row.propertyID}`}
                        className="p-2 hover:bg-muted rounded-md text-primary transition-colors block w-fit"
                        title="عرض التفاصيل"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <Eye className="w-4 h-4" />
                    </Link>
                </div>
            )
        }
    ], [crud]);

    if (error) {
        return <ErrorState message="خطأ في تحميل عقارات المشروع" />;
    }

    return (
        <CrudPageLayout
            title="الأصول العقارية للمشروع"
            description="إدارة الوحدات والمباني التابعة لهذا المشروع"
            createLabel="إضافة عقار"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={properties}
            loading={isLoading}
            emptyMessage="لم يتم ربط أي أصول عقارية بهذا المشروع حتى الآن."
            onRowClick={(row) => navigate(`/investment/properties/${row.propertyID}`)}
        >
            <PropertyDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem || undefined}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["project-properties", projectId] }); }}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="investment-property"
                entityId={crud.viewerTarget?.propertyID || 0}
                title={`وثائق العقار: ${crud.viewerTarget?.propertyName}`}
            />
        </CrudPageLayout>
    );
}
