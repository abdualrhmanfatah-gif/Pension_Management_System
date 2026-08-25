import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Edit, FileText, Wrench } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";
import { Link } from "react-router-dom";

import type { MaintenanceRequestDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { MaintenanceRequestDialog } from "@/components/domain/investment/MaintenanceRequestDialog";
import { WorkOrderDialog } from "@/components/domain/investment/WorkOrderDialog";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

interface PropertyMaintenanceTabProps {
    propertyId: number;
}

/**
 * تبويب بلاغات الصيانة للعقار
 */
export function PropertyMaintenanceTab({ propertyId }: PropertyMaintenanceTabProps) {
    const canManage = useHasPermission("Investment.Manage");
    const crud = useCrudState<MaintenanceRequestDto>();

    const [workOrderOpen, setWorkOrderOpen] = useState(false);
    const [workOrderRequest, setWorkOrderRequest] = useState<MaintenanceRequestDto | null>(null);

    // جلب البيانات
    const { data: requestsData, isLoading, error } = useQuery({
        queryKey: ["property-maintenance", propertyId],
        queryFn: () => investmentApi.getRequests(propertyId).then(r => r || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل بلاغات الصيانة", { id: "property-maintenance" });
    }, [error]);

    const requests = requestsData || [];

    const handleOpenWorkOrder = (request: MaintenanceRequestDto) => {
        setWorkOrderRequest(request);
        setWorkOrderOpen(true);
    };

    const getStatusColor = (status: string): "warning" | "info" | "success" | "danger" | "outline" => {
        switch (status) {
            case "New": return "warning";
            case "InProgress": return "info";
            case "Completed": return "success";
            case "Cancelled": return "danger";
            default: return "outline";
        }
    };

    const getStatusLabel = (status: string) => {
        switch (status) {
            case "New": return "جديد";
            case "InProgress": return "قيد المعالجة";
            case "Completed": return "مكتمل";
            case "Cancelled": return "ملغي";
            default: return status;
        }
    };

    const getPriorityLabel = (priority?: string) => {
        switch (priority) {
            case "High": return "عالي";
            case "Medium": return "متوسط";
            case "Low": return "منخفض";
            case "Urgent": return "عاجل جداً";
            default: return priority || "غير محدد";
        }
    };

    const columns = useMemo<CustomColumnDef<MaintenanceRequestDto>[]>(() => [
        {
            key: "requestID",
            header: "رقم الطلب",
            render: (row) => <span className="font-mono font-bold text-primary">#{row.requestID}</span>
        },
        {
            key: "description",
            header: "الوصف",
            render: (row) => (
                <div className="max-w-[40vw] sm:max-w-[250px] truncate font-medium text-sm text-start" title={row.description}>
                    {row.description}
                </div>
            )
        },
        {
            key: "reportedDate",
            header: "تاريخ الطلب",
            render: (row) => (
                <span className="text-sm font-mono">{new Date(row.reportedDate).toLocaleDateString("ar-YE")}</span>
            )
        },
        {
            key: "priority",
            header: "الأولوية",
            render: (row) => (
                <Badge variant={row.priority === 'Urgent' || row.priority === 'High' ? 'destructive' : 'outline'}>
                    {getPriorityLabel(row.priority)}
                </Badge>
            )
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
                    <Link to={`/investment/maintenance/${row.requestID}`} className="inline-flex items-center justify-center rounded-md min-h-[44px] min-w-[44px] hover:bg-muted text-blue-600 transition-colors" title="عرض التفاصيل الكاملة">
                        <AlertCircle className="w-4 h-4" />
                    </Link>
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق" className="min-h-[44px] min-w-[44px] text-amber-600 hover:bg-amber-50">
                        <FileText className="w-4 h-4" />
                    </Button>
                    {canManage && (
                        <Button variant="ghost" size="sm" onClick={() => crud.openEdit(row)} className="min-h-[44px] min-w-[44px] p-0" title="تعديل">
                            <Edit className="w-4 h-4 text-muted-foreground" />
                        </Button>
                    )}
                    {canManage && (
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenWorkOrder(row)}
                            className="gap-1.5 text-xs border-emerald-200 text-emerald-700 hover:bg-emerald-50"
                        >
                            <Wrench className="w-3.5 h-3.5" /> الأوامر
                        </Button>
                    )}
                </div>
            )
        }
    ], [crud, canManage]);

    if (error) {
        return <ErrorState message="خطأ في تحميل صيانة العقار" />;
    }

    return (
        <CrudPageLayout
            title="بلاغات الصيانة"
            description="تسجيل ومتابعة طلبات الإصلاح للمباني والوحدات"
            createLabel="طلب صيانة جديد"
            onCreateClick={canManage ? crud.openCreate : undefined}
            columns={columns}
            data={requests}
            loading={isLoading}
            emptyMessage="لا توجد بلاغات صيانة مسجلة حالياً."
        >
            <MaintenanceRequestDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                initialData={crud.selectedItem || undefined}
                mode={crud.isEditing ? "edit" : "create"}
                onSuccess={crud.closeForm}
                propertyId={propertyId}
            />

            <WorkOrderDialog
                isOpen={workOrderOpen}
                onOpenChange={(o) => !o && setWorkOrderOpen(false)}
                request={workOrderRequest}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="maintenance-ticket"
                entityId={crud.viewerTarget?.requestID || 0}
                title={`وثائق بلاغ الصيانة: #${crud.viewerTarget?.requestID}`}
            />
        </CrudPageLayout>
    );
}
