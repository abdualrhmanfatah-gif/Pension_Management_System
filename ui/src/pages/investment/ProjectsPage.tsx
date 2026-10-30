import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Briefcase,Eye } from "lucide-react";
import { useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";

import { investmentApi, type ProjectDto } from "@/api/investment";
<<<<<<< HEAD
import { ProjectDialog } from "@/components/domain/investment/ProjectDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    CrudPageLayout,
    type CustomColumnDef
} from "@/components/ui";
<<<<<<< HEAD
=======
import { ProjectDialog } from "@/components/domain/investment/ProjectDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { PROJECT_STATUS } from "@/constants";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

/**
 * صفحة المشاريع الاستثمارية
 * تعرض قائمة بكافة المشاريع التابعة لمحفظة معينة أو كافة المحافظ
 */
export function ProjectsPage() {
    const navigate = useNavigate();
    const canManage = useHasPermission("Investment.Manage");
    const crud = useCrudState<ProjectDto>();
    const queryClient = useQueryClient();

    // جلب المشاريع
    const { data: projectsData, isLoading } = useQuery({
        queryKey: ["projects-all"],
        queryFn: () => investmentApi.getProjects()
    });

    const projects = projectsData || [];

    // تعريف الأعمدة
    const columns = useMemo<CustomColumnDef<ProjectDto>[]>(() => [
        {
            key: "projectID",
            header: "رقم المشروع",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.projectID}</span>
        },
        {
            key: "name",
            header: "اسم المشروع",
            render: (row) => (
                <span className="font-bold text-primary">
                    {row.name}
                </span>
            )
        },
        {
            key: "portfolioName",
            header: "المحفظة",
            render: (row) => (
                <div className="flex items-center gap-2 text-muted-foreground">
                    <Briefcase className="w-3.5 h-3.5" />
                    <span className="text-sm">{row.portfolioName}</span>
                </div>
            )
        },
        {
            key: "plannedBudget",
            header: "الميزانية المخططة",
            render: (row) => (
<<<<<<< HEAD
                <span className="font-semibold text-info">
=======
                <span className="font-semibold text-blue-600">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    {row.plannedBudget.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.status === PROJECT_STATUS.ACTIVE ? 'info' : 'outline'} className="rounded-full">
                    {row.status === PROJECT_STATUS.ACTIVE ? 'نشط' : 'مخطط'}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <Link
                        to={`/investment/projects/${row.projectID}`}
                        className="p-2 hover:bg-primary/10 rounded-full text-primary transition-colors"
                        title="عرض التفاصيل"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <Eye className="w-4 h-4" />
                    </Link>
                </div>
            )
        }
    ], []);

    return (
        <CrudPageLayout
            title="المشاريع الاستثمارية"
            description="عرض وإدارة كافة المشاريع التابعة لمحافظ الصندوق"
            createLabel="إضافة مشروع جديد"
            onCreateClick={canManage ? crud.openCreate : undefined}
            canCreate={canManage}
            columns={columns}
            data={projects}
            loading={isLoading}
            emptyMessage="لم يتم العثور على أي مشاريع مسجلة في النظام حالياً."
            onRowClick={(row) => navigate(`/investment/projects/${row.projectID}`)}
        >
            <ProjectDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem || undefined}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["projects"] }); }}
            />
        </CrudPageLayout>
    );
}
