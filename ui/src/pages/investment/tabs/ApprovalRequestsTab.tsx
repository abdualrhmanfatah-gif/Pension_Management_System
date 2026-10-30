import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle, XCircle } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { ApprovalRequestDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
<<<<<<< HEAD
import { ApprovalRequestDialog } from "@/components/domain/investment/ApprovalRequestDialog";
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
=======
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { ApprovalRequestDialog } from "@/components/domain/investment/ApprovalRequestDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { APPROVAL_STATUS } from "@/constants";
import { useApiMutation,useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

/**
 * تبويب طلبات الاعتماد وتدفقات العمل
 */
export function ApprovalRequestsTab() {
    const canApprove = useHasPermission("Investment.Approve");
    const crud = useCrudState<ApprovalRequestDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: approvalsRes, isLoading, error } = useQuery({
        queryKey: ["investment-approvals"],
        queryFn: () => investmentApi.getApprovals().then(r => r || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل طلبات الاعتماد", { id: "approval-requests" });
    }, [error]);

    const approvals = approvalsRes || [];

    // طفرة تحديث الحالة (approve/reject)
    const updateStatusMutation = useApiMutation(
        ({ id, status, note }: { id: number; status: string; note: string }) =>
            investmentApi.updateApprovalStatus(id, { status, decisionNote: note }),
        {
            invalidateKeys: [["investment-approvals"]],
            successMessage: "تم تحديث حالة الطلب بنجاح",
        }
    );

    const getStatusBadge = (status: string) => {
        switch (status) {
            case APPROVAL_STATUS.APPROVED: return <Badge variant="success">معتمد</Badge>;
            case APPROVAL_STATUS.REJECTED: return <Badge variant="destructive">مرفوض</Badge>;
            case APPROVAL_STATUS.PENDING: return <Badge variant="warning">قيد الانتظار</Badge>;
            default: return <Badge variant="outline">{status}</Badge>;
        }
    };

    const getEntityTypeLabel = (type: string) => {
        switch (type) {
            case "Project": return "مشروع";
            case "Property": return "أصل عقاري";
            case "Lease": return "عقد إيجار";
            case "Transaction": return "معاملة مالية";
            default: return type;
        }
    };

    const columns = useMemo<CustomColumnDef<ApprovalRequestDto>[]>(() => [
        {
            key: "approvalID",
            header: "رقم الطلب",
            render: (row) => <span className="font-mono font-bold text-primary">#{row.approvalID}</span>
        },
        {
            key: "entityType",
            header: "الكيان المرتبط",
            render: (row) => (
                <span className="text-sm font-medium">
                    {getEntityTypeLabel(row.entityType)} <span className="font-mono text-muted-foreground">({row.entityID})</span>
                </span>
            )
        },
        { key: "workflowStage", header: "المرحلة الحالية", accessor: (row) => row.workflowStage || "—" },
        { key: "requestedByUserName", header: "مقدم الطلب", accessor: (row) => row.requestedByUserName },
        {
            key: "status",
            header: "الحالة",
            render: (row) => getStatusBadge(row.status)
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (row.status === APPROVAL_STATUS.PENDING && canApprove) ? (
                <div className="flex items-center gap-2">
                    <Button
                        variant="ghost"
                        size="sm"
<<<<<<< HEAD
                        className="text-success hover:text-success hover:bg-success/10 gap-1"
=======
                        className="text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 gap-1"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        onClick={() => updateStatusMutation.mutate({ id: row.approvalID, status: APPROVAL_STATUS.APPROVED, note: "تم الاعتماد من خلال النظام" })}
                        loading={updateStatusMutation.isPending}
                    >
                        <CheckCircle className="w-4 h-4" /> اعتماد
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
<<<<<<< HEAD
                        className="text-danger hover:text-danger hover:bg-danger/10 gap-1"
=======
                        className="text-red-600 hover:text-red-700 hover:bg-red-50 gap-1"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        onClick={() => updateStatusMutation.mutate({ id: row.approvalID, status: APPROVAL_STATUS.REJECTED, note: "تم الرفض من خلال النظام" })}
                        loading={updateStatusMutation.isPending}
                    >
                        <XCircle className="w-4 h-4" /> رفض
                    </Button>
                </div>
            ) : null
        }
    ], [canApprove, updateStatusMutation]);

    if (error) {
        return <ErrorState message="خطأ في تحميل طلبات الاعتماد" />;
    }

    return (
        <CrudPageLayout
            title="طلبات الاعتماد وتدفقات العمل"
            description="متابعة الطلبات المعلقة التي تتطلب موافقة أصحاب الصلاحية"
            createLabel="طلب اعتماد"
            onCreateClick={crud.openCreate}
            canCreate={canApprove}
            columns={columns}
            data={approvals}
            loading={isLoading}
            emptyMessage="لا توجد طلبات اعتماد معلقة حالياً."
        >
            <ApprovalRequestDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["investment-approvals"] }); }}
            />
        </CrudPageLayout>
    );
}
