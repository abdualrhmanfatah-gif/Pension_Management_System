import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye } from "lucide-react";
import { useEffect, useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";

import type { ProjectDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { ProjectDialog } from "@/components/domain/investment/ProjectDialog";
import { Badge, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { PROJECT_STATUS } from "@/constants";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

interface PortfolioProjectsTabProps {
    portfolioId: number;
}

/**
 * تبويب مشاريع المحفظة الاستثمارية
 */
export function PortfolioProjectsTab({ portfolioId }: PortfolioProjectsTabProps) {
    const navigate = useNavigate();
    const canManage = useHasPermission("Investment.Manage");
    const crud = useCrudState<ProjectDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: projectsData, isLoading, error } = useQuery({
        queryKey: ["portfolio-projects", portfolioId],
        queryFn: () => investmentApi.getProjectsByPortfolio(portfolioId).then(res => res || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل مشاريع المحفظة", { id: "portfolio-projects" });
    }, [error]);

    const projects = projectsData || [];

    const columns = useMemo<CustomColumnDef<ProjectDto>[]>(() => [
        {
            key: "projectID",
            header: "رقم المشروع",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.projectID}</span>
        },
        {
            key: "name",
            header: "اسم المشروع",
            render: (row) => <span className="font-bold text-primary">{row.name}</span>
        },
        { key: "typeName", header: "النوع", accessor: (row) => row.typeName },
        {
            key: "plannedBudget",
            header: "الميزانية المخططة",
            render: (row) => (
                <span className="font-semibold text-info font-mono">
                    {row.plannedBudget.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.status === PROJECT_STATUS.ACTIVE ? 'info' : 'outline'}>
                    {row.status === PROJECT_STATUS.ACTIVE ? 'نشط' : row.status === PROJECT_STATUS.PLANNED ? 'مخطط' : row.status}
                </Badge>
            )
        },
        {
            key: "startDate",
            header: "تاريخ البدء",
            render: (row) => <span className="text-sm font-mono">{new Date(row.startDate).toLocaleDateString("ar-YE")}</span>
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <Link
                    to={`/investment/projects/${row.projectID}`}
                    className="p-2 hover:bg-muted rounded-md text-primary transition-colors block w-fit"
                    title="عرض تفاصيل المشروع"
                    onClick={(e) => e.stopPropagation()}
                >
                    <Eye className="w-4 h-4" />
                </Link>
            )
        }
    ], []);

    if (error) {
        return <ErrorState message="خطأ في تحميل مشاريع المحفظة" />;
    }

    return (
        <CrudPageLayout
            title="مشاريع المحفظة"
            description="إدارة المشاريع الاستثمارية التابعة لهذه المحفظة"
            createLabel="إضافة مشروع"
            onCreateClick={canManage ? crud.openCreate : undefined}
            canCreate={canManage}
            columns={columns}
            data={projects}
            loading={isLoading}
            emptyMessage="لا توجد مشاريع مسجلة في هذه المحفظة حالياً."
            onRowClick={(row) => navigate(`/investment/projects/${row.projectID}`)}
        >
            <ProjectDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem || undefined}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["portfolio-projects", portfolioId] }); }}
            />
        </CrudPageLayout>
    );
}
