import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Building, CheckCircle2, Clock, FileText, Wrench } from "lucide-react";
import { useState } from "react";

import type { MaintenanceRequestDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { MaintenanceRequestDialog } from "@/components/domain/investment/MaintenanceRequestDialog";
import { WorkOrderDialog } from "@/components/domain/investment/WorkOrderDialog";
import { Badge, BaseFormDialog, Button, CrudPageLayout } from "@/components/ui";
import { type CustomColumnDef } from "@/components/ui/DataTable";
import { BADGE_VARIANT, BATCH_STATUS, PRIORITY } from "@/constants";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

export function MaintenancePage() {
    const canManage = useHasPermission("Investment.Manage");

    const crud = useCrudState<MaintenanceRequestDto>();

    // Work order state
    const [workOrderOpen, setWorkOrderOpen] = useState(false);
    const [workOrderRequest, setWorkOrderRequest] = useState<MaintenanceRequestDto | null>(null);

    // جلب البلاغات - استخدام getAllRequests للشاشة المركزية
    const { data: requestsRes, isLoading } = useQuery({
        queryKey: ["maintenance-requests-all"],
        queryFn: () => investmentApi.getAllRequests()
    });

    const requests = requestsRes || [];

    const getStatusBadge = (status: string) => {
        switch (status) {
            case BATCH_STATUS.COMPLETED:
                return <Badge variant={BADGE_VARIANT.SUCCESS} className="gap-1 bg-success/10 text-success border-success">
                    <CheckCircle2 className="w-3 h-3" /> مكتملة
                </Badge>;
            case 'InProgress':
                return <Badge variant={BADGE_VARIANT.INFO} className="gap-1 bg-info/10 text-info border-info">
                    <Clock className="w-3 h-3" /> قيد التنفيذ
                </Badge>;
            case BATCH_STATUS.NEW:
                return <Badge variant={BADGE_VARIANT.DANGER} className="gap-1 bg-danger/10 text-danger border-danger">
                    <AlertCircle className="w-3 h-3" /> جديدة
                </Badge>;
            default:
                return <Badge variant={BADGE_VARIANT.OUTLINE}>{status}</Badge>;
        }
    };

    const columns: CustomColumnDef<MaintenanceRequestDto>[] = [
        {
            key: "property",
            header: "العقار / الوحدة",
            render: (row) => (
                <div className="flex flex-col text-start">
                    <span className="font-bold text-sm">{row.propertyName}</span>
                    <span className="text-[10px] sm:text-xs text-muted-foreground flex items-center gap-1">
                        <Building className="w-3 h-3" /> وحدة رقم: {row.unitNumber}
                    </span>
                </div>
            )
        },
        {
            key: "requestType",
            header: "نوع الصيانة",
            render: () => (
                <Badge variant="outline" className="text-[10px] sm:text-xs font-bold">صيانة</Badge>
            )
        },
        {
            key: "priority",
            header: "الأولوية",
            render: (row) => (
                <div className="flex items-center gap-1.5 text-start">
                    <div className={`w-2 h-2 rounded-full ${row.priority === PRIORITY.URGENT ? 'bg-danger animate-pulse' :
                        row.priority === PRIORITY.HIGH ? 'bg-warning' : 'bg-muted'
                    }`} />
                    <span className="text-xs">{row.priority === PRIORITY.URGENT ? 'عاجل جداً' : row.priority === PRIORITY.HIGH ? 'مرتفعة' : 'عادي'}</span>
                </div>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => getStatusBadge(row.status)
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex items-center gap-1 justify-center">
                    <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0" onClick={() => crud.openViewer(row)}>
                        <FileText className="w-4 h-4" />
                    </Button>
                    {canManage && (
                        <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0" onClick={() => crud.openEdit(row)}>
                            <Wrench className="w-4 h-4" />
                        </Button>
                    )}
                    {canManage && (
                        <Button
                            size="sm"
                            variant="outline"
                            onClick={() => {
                                setWorkOrderRequest(row);
                                setWorkOrderOpen(true);
                            }}
                            className="gap-1 border-success text-success hover:bg-success/10 font-bold"
                        >
                            <Wrench className="w-3.5 h-3.5" /> أوامر العمل
                        </Button>
                    )}
                </div>
            )
        }
    ];

    return (
        <CrudPageLayout
            title="بلاغات الصيانة"
            description="إدارة ومتابعة طلبات صيانة العقارات الاستثمارية"
            createLabel="تسجيل بلاغ جديد"
            onCreateClick={canManage ? crud.openCreate : undefined}
            columns={columns}
            data={requests}
            loading={isLoading}
        >
            <MaintenanceRequestDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                initialData={crud.selectedItem || undefined}
                mode={crud.isEditing ? "edit" : "create"}
                onSuccess={crud.closeForm}
            />

            {workOrderRequest && (
                <WorkOrderDialog
                    open={workOrderOpen}
                    onOpenChange={setWorkOrderOpen}
                    request={workOrderRequest}
                />
            )}

            <BaseFormDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.onOpenChange}
                title="تفاصيل بلاغ الصيانة"
                size="3xl"
            >
                {crud.viewerTarget && (
                    <InvestmentDocumentsManager
                        entityType="maintenance-ticket"
                        entityId={crud.viewerTarget.requestID}
                        entityName={`بلاغ صيانة رقم ${crud.viewerTarget.requestID} - ${crud.viewerTarget.propertyName}`}
                    />
                )}
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
