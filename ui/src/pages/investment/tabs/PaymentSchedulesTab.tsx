import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit, FileText } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { PaymentScheduleDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { PaymentScheduleDialog } from "@/components/domain/investment/PaymentScheduleDialog";
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { useCrudState } from "@/hooks";

interface PaymentSchedulesTabProps {
    contractId?: number;
}

/**
 * تبويب جداول الدفع لعقود الإيجار
 */
export function PaymentSchedulesTab({ contractId }: PaymentSchedulesTabProps) {
    const crud = useCrudState<PaymentScheduleDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: schedulesRes, isLoading, error } = useQuery({
        queryKey: contractId ? ["schedules", contractId] : ["schedules-all"],
        queryFn: () => investmentApi.getSchedules(contractId)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل جداول الدفع", { id: "payment-schedules" });
    }, [error]);

    const schedules = Array.isArray(schedulesRes) ? schedulesRes : [];

    const getStatusColor = (status: string): "success" | "warning" | "danger" | "info" | "outline" => {
        switch (status) {
            case "Paid": return "success";
            case "Pending": return "warning";
            case "Overdue": return "danger";
            case "Partial": return "info";
            default: return "outline";
        }
    };

    const getStatusLabel = (status: string) => {
        switch (status) {
            case "Paid": return "مدفوعة";
            case "Pending": return "قيد الانتظار";
            case "Overdue": return "متأخرة";
            case "Partial": return "جزئية";
            default: return status;
        }
    };

    const columns = useMemo<CustomColumnDef<PaymentScheduleDto>[]>(() => [
        { key: "scheduleID", header: "رقم الدفعة", accessor: (row) => row.scheduleID },
        {
            key: "dueDate",
            header: "تاريخ الاستحقاق",
            render: (row) => <span className="font-mono">{new Date(row.dueDate).toLocaleDateString("ar-YE")}</span>
        },
        {
            key: "expectedAmount",
            header: "المبلغ المتوقع",
            render: (row) => <span className="font-mono">{row.expectedAmount.toLocaleString()} ر.ي</span>
        },
        {
            key: "paidAmount",
            header: "المبلغ المدفوع",
            render: (row) => <span className="text-success font-bold font-mono">{row.paidAmount.toLocaleString()} ر.ي</span>
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={getStatusColor(row.status)}>
                    {getStatusLabel(row.status)}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-1">
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق والمستندات" className="text-warning hover:bg-warning/10">
                        <FileText className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" className="h-11 gap-2" onClick={() => crud.openEdit(row)}>
                        <Edit className="w-4 h-4 text-muted-foreground" /> إجراء
                    </Button>
                </div>
            )
        }
    ], [crud]);

    if (error) {
        return <ErrorState message="خطأ في تحميل جداول الدفع" />;
    }

    return (
        <CrudPageLayout
            title="جداول الدفع للعقود"
            description="متابعة تحصيل الدفعات المالية المجدولة لعقود الإيجار"
            columns={columns}
            data={schedules}
            loading={isLoading}
            hideHeader={!!contractId}
        >
            <PaymentScheduleDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                initialData={crud.selectedItem}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["schedules"] }); queryClient.invalidateQueries({ queryKey: ["schedules-all"] }); }}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="lease-payment"
                entityId={crud.viewerTarget?.scheduleID || 0}
                title={`وثائق الدفعة: #${crud.viewerTarget?.scheduleID}`}
            />
        </CrudPageLayout>
    );
}
