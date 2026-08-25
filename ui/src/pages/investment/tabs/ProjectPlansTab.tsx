import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit, FileText, Trash2 } from "lucide-react";
import { useCallback,useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { InvestmentPlanDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { Button, CrudPageLayout, ErrorState, useConfirm } from "@/components/ui";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { InvestmentPlanDialog } from "@/components/domain/investment/InvestmentPlanDialog";
import { useApiMutation,useCrudState } from "@/hooks";

interface ProjectPlansTabProps {
    projectId: number;
}

/**
 * تبويب خطط الموازنة السنوية للمشروع
 */
export function ProjectPlansTab({ projectId }: ProjectPlansTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<InvestmentPlanDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: plansData, isLoading, error } = useQuery({
        queryKey: ["project-plans", projectId],
        queryFn: () => investmentApi.getInvestmentPlans(projectId)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل خطط الموازنة", { id: "project-plans" });
    }, [error]);

    const plans = plansData || [];

    // طفرة الحذف
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteInvestmentPlan(id),
        {
            invalidateKeys: [["project-plans", projectId], ["project-summary", projectId]],
            successMessage: "تم حذف خطة الموازنة بنجاح",
        }
    );

    const handleDelete = useCallback(async (plan: InvestmentPlanDto) => {
        const ok = await confirm({
            title: "حذف خطة الموازنة",
            description: "هل أنت متأكد من رغبتك في حذف هذه الخطة السنوية؟ لا يمكن التراجع عن هذا الإجراء.",
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(plan.planID);
    }, [confirm, deleteMutation]);

    const columns = useMemo(() => [
        {
            key: "fiscalYearID",
            header: "السنة المالية",
            render: (row: InvestmentPlanDto) => <span className="font-bold text-start">سنة {row.yearName || row.fiscalYearID}</span>
        },
        {
            key: "plannedAmount",
            header: "المبلغ المخطط",
            render: (row: InvestmentPlanDto) => (
                <span className="font-semibold text-blue-600 font-mono">
                    {row.plannedAmount.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "actualSpent",
            header: "الإنفاق الفعلي",
            render: (row: InvestmentPlanDto) => (
                <span className="font-semibold text-emerald-600 font-mono">
                    {row.actualSpent.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: InvestmentPlanDto) => (
                <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق">
                        <FileText className="w-4 h-4 text-primary" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => crud.openEdit(row)} title="تعديل">
                        <Edit className="w-4 h-4 text-muted-foreground" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => handleDelete(row)} title="حذف">
                        <Trash2 className="w-4 h-4 text-red-500" />
                    </Button>
                </div>
            )
        }
    ], [crud, handleDelete]);

    if (error) {
        return <ErrorState message="خطأ في تحميل خطط المشروع" />;
    }

    return (
        <CrudPageLayout
            title="خطط الموازنة السنوية"
            description="تتبع الميزانيات المخططة والمنفذة حسب السنوات المالية"
            createLabel="إضافة خطة سنوية"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={plans}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي خطط موازنة سنوية لهذا المشروع حتى الآن."
        >
            <InvestmentPlanDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem}
                projectId={projectId}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["project-plans", projectId] }); }}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="investment-plan"
                entityId={crud.viewerTarget?.planID || 0}
                title={`وثائق خطة موازنة: سنة ${crud.viewerTarget?.yearName || crud.viewerTarget?.fiscalYearID}`}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
