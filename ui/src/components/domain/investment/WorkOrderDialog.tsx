import { useQuery } from "@tanstack/react-query";
import { CheckCircle2, Clock, FileText,User, Wrench } from "lucide-react";

import { investmentApi, type MaintenanceRequestDto, type WorkOrderDto } from "@/api/investment";
import { 
    Badge, 
    BaseFormDialog, 
    type BaseOverlayProps,
    FormSection,
    Skeleton,
    Text} from "@/components/ui";

interface WorkOrderDialogProps extends BaseOverlayProps {
    request: MaintenanceRequestDto | null;
}

/**
 * حوار عرض أوامر العمل المرتبطة ببلاغ صيانة
 */
export function WorkOrderDialog({ open, isOpen, onOpenChange, onClose, request }: WorkOrderDialogProps) {
    const { data: workOrders = [], isLoading } = useQuery({
        queryKey: ["work-orders", request?.requestID],
        queryFn: () => request ? investmentApi.getWorkOrders(request.requestID) : Promise.resolve([]),
        enabled: (open || isOpen) && !!request
    });

    const getStatusBadge = (status: string) => {
        switch (status) {
            case 'Completed': return <Badge variant="success" className="gap-1 rounded-lg px-2 py-1"><CheckCircle2 className="w-3 h-3" /> مكتمل</Badge>;
            case 'InProgress': return <Badge variant="info" className="gap-1 rounded-lg px-2 py-1"><Clock className="w-3 h-3" /> قيد التنفيذ</Badge>;
            default: return <Badge variant="outline" className="rounded-lg px-2 py-1">{status}</Badge>;
        }
    };

    return (
        <BaseFormDialog 
            open={open || isOpen}
            onOpenChange={onOpenChange || ((o) => !o && onClose?.())}
            title={`أوامر العمل - بلاغ #${request?.requestID}`}
            description="قائمة المهام والتكليفات الصادرة لمعالجة هذا البلاغ"
            size="3xl"
            onSubmit={() => {}} 
            submitLabel="" 
            footer={null} 
        >
            <div className="space-y-4 text-start">
                <FormSection title="المهام المنفذة والمجدولة" icon={Wrench}>
                    <div className="space-y-4">
                        {isLoading ? (
                            <div className="space-y-3">
                                {[1, 2, 3].map(i => <Skeleton key={i} className="h-24 w-full rounded-2xl" />)}
                            </div>
                        ) : workOrders.length === 0 ? (
                            <div className="text-center py-16 text-muted-foreground bg-muted/10 rounded-2xl border-2 border-dashed border-border">
                                <Wrench className="w-12 h-12 mx-auto mb-4 opacity-100" />
                                <Text variant="label">لا توجد أوامر عمل مسجلة لهذا البلاغ حالياً.</Text>
                            </div>
                        ) : (
                            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                                {workOrders.map((wo: WorkOrderDto) => (
                                    <div 
                                        key={wo.workOrderID} 
                                        className="group bg-background border border-border rounded-2xl p-5  transition-all"
                                    >
                                        <div className="flex justify-between items-start gap-4">
                                            <div className="space-y-3 flex-1">
                                                <div className="flex items-start gap-2">
                                                    <FileText className="w-4 h-4 text-muted-foreground mt-1 shrink-0" />
                                                    <Text variant="body" className="font-bold text-foreground leading-tight">
                                                        {wo.description || "أمر عمل بدون وصف"}
                                                    </Text>
                                                </div>
                                                
                                                <div className="flex items-center gap-4 pt-1">
                                                    <div className="flex items-center gap-1.5 text-xs text-muted-foreground bg-muted/30 px-2.5 py-1 rounded-full">
                                                        <User className="w-3 h-3" />
                                                        <span>المسؤول: <span className="font-bold text-foreground">{wo.assignedContractor || "غير معين"}</span></span>
                                                    </div>
                                                    
                                                    {wo.completionDate && (
                                                        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                                                            <Clock className="w-3 h-3" />
                                                            <span>الإنجاز: <span className="font-mono">{new Date(wo.completionDate).toLocaleDateString("ar-YE")}</span></span>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                            
                                            <div className="shrink-0">
                                                {getStatusBadge(wo.status || "New")}
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
