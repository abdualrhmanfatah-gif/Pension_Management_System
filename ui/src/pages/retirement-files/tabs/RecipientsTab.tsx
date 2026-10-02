import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Check, Wallet, X } from "lucide-react";
import { useEffect, useMemo,useState } from "react";
import { toast } from "sonner";

import { type EndRecipientDto,retirementApi } from "@/api/retirement";
import { type CreateSalaryRecipientDto,salaryRecipientApi } from "@/api/salary-recipients";
import { EndRecipientDialog, type EndRecipientFormValues } from "@/components/domain/retirement/EndRecipientDialog";
import { type RecipientFormValues,RecipientsDialog } from "@/components/domain/retirement/RecipientsDialog";
import { Badge, Button, Card, type CustomColumnDef, DataTable, Heading } from "@/components/ui";
import { PENDING_REVIEW_STATUS } from "@/constants";
import { useApiMutation, useCreateShortcut, useCrudState } from "@/hooks";
import { useAgentsQuery, useEntitledPersonsQuery, useGuardiansQuery } from "@/hooks/queries";
import { cn, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { AgentSummary, EntitledPersonSummary, GuardianSummary, RecipientItem } from "@/types/retirement";

interface RecipientsTabProps {
    fileId: number;
    retireeId?: number;
    retireeName?: string;
}

/**
 * تبويب مستلمي المعاش لملف التقاعد
 */
export function RecipientsTab({ fileId, retireeId, retireeName }: RecipientsTabProps) {
    const canCreate = useHasPermission("SalaryRecipient.Add");
    const canEnd = useHasPermission("SalaryRecipient.Edit");

    const crud = useCrudState<RecipientItem>();
    const [endOpen, setEndOpen] = useState(false);
    const [endingRecipient, setEndingRecipient] = useState<RecipientItem | null>(null);

    // جلب البيانات
    const { data: recipients = [], isLoading: loadingRecipients, error } = useQuery({
        queryKey: ["salary-recipients", fileId],
        queryFn: () => retirementApi.getRecipients(fileId).then(data => data as RecipientItem[])
    });

    const acknowledgeMutation = useApiMutation(
        (params: { entityType: string; entityId: number }) =>
            retirementApi.acknowledgeEntityRejection(params.entityType, params.entityId),
        {
            successMessage: "تم قبول الرفض وإعادة الكيان لحالته الطبيعية",
            invalidateKeys: [["salary-recipients", fileId], ["retirement-file"]]
        }
    );

    const { data: agentsData = [] } = useAgentsQuery(fileId);
    const { data: entitledsData = [] } = useEntitledPersonsQuery(fileId);
    const { data: guardiansData = [] } = useGuardiansQuery(fileId);

    const agents = useMemo(() => 
        (agentsData as AgentSummary[])
            .filter(a => (a.isActive === true) && !a.isDeleted)
            .map(a => ({ id: a.agentID, fullName: a.fullName })), 
    [agentsData]);

    const entitleds = useMemo(() => 
        (entitledsData as EntitledPersonSummary[])
            .filter(e => !e.isDeceased && !e.isDeleted)
            .map(e => ({ 
                id: e.entitledID, 
                fullName: e.fullName,
                isEligible: e.isEligible,
                hasPersonalPhoto: e.hasPersonalPhoto,
                hasNationalIdDocument: e.hasNationalIdDocument
            })), 
    [entitledsData]);

    const guardians = useMemo(() => 
        (guardiansData as GuardianSummary[])
            .filter(g => (g.isActive === true) && !g.isDeleted)
            .map(g => ({ id: g.guardianID, fullName: g.fullName })), 
    [guardiansData]);

    // طفرات البيانات
    const createMutation = useApiMutation(
        (payload: CreateSalaryRecipientDto) => salaryRecipientApi.create(payload),
        {
            successMessage: "تم إضافة مستلم المعاش بنجاح وتفعيل المستحق",
            invalidateKeys: [["salary-recipients", fileId], ["entitled-persons", fileId]],
            onSuccess: () => crud.closeForm()
        }
    );

    const endMutation = useApiMutation(
        (payload: EndRecipientDto) =>
            retirementApi.endRecipient(endingRecipient?.recipientID || 0, payload),
        {
            successMessage: "تم إنهاء مستلم المعاش بنجاح",
            invalidateKeys: [["salary-recipients", fileId]],
            onSuccess: () => {
                setEndOpen(false);
                setEndingRecipient(null);
            }
        }
    );

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل بيانات مستلمي المعاش");
    }, [error]);

    // المعالجات
    const handleCreate = (values: RecipientFormValues) => {
        const payload: CreateSalaryRecipientDto = {
            fileID: fileId,
            startDate: values.startDate,
            notes: values.notes || undefined,
            // بيانات تكميلية
            nationalID: values.nationalID,
            birthDate: values.birthDate,
            socialStatusID: values.socialStatusID,
            healthStatusID: values.healthStatusID,
            personalPhoto: values.personalPhoto,
            nationalIdImage: values.nationalIdImage,
        };

        if (values.recipientType === "retiree") payload.retireeID = Number(values.targetID);
        else if (values.recipientType === "agent") payload.agentID = Number(values.targetID);
        else if (values.recipientType === "entitled") payload.entitledID = Number(values.targetID);
        else if (values.recipientType === "guardian") payload.guardianID = Number(values.targetID);

        createMutation.mutate(payload);
    };

    const handleEnd = (values: EndRecipientFormValues) => {
        const payload: EndRecipientDto = {
            endDate: values.endDate,
            notes: values.notes || undefined
        };
        endMutation.mutate(payload);
    };

    const columns: CustomColumnDef<RecipientItem>[] = [
        {
            key: "recipientName",
            header: "اسم المستلم",
            render: (row) => (
                <div className="flex flex-col text-start">
                    <span className="font-bold flex items-center gap-2">
                        {row.retireeFullName || row.agentFullName || row.entitledFullName || row.guardianFullName || "—"}
                        {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                            <Badge variant="warning" className="text-[10px] sm:text-xs h-5">قيد المراجعة</Badge>
                        )}
                        {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                            <Badge variant="destructive" className="text-[10px] sm:text-xs h-5">مرفوض</Badge>
                        )}
                    </span>
                    <span className="text-[10px] sm:text-xs text-muted-foreground">{row.isActive ? "نشط" : "غير نشط"}</span>
                </div>
            )
        },
        {
            key: "startDate",
            header: "تاريخ البدء",
            render: (row) => <span className="font-mono text-xs">{formatDate(row.startDate)}</span>
        },
        {
            key: "endDate",
            header: "تاريخ الانتهاء",
            render: (row) => row.endDate ? <span className="font-mono text-xs text-rose-600">{formatDate(row.endDate)}</span> : "—"
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isActive ? "success" : "outline"}>
                    {row.isActive ? "نشط حالياً" : "سابق / منتهي"}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex items-center justify-center gap-1">
                    {row.isActive && canEnd ? (
                        <Button
                            size="sm"
                            variant="ghost"
                            className="text-destructive hover:bg-destructive/5 hover:text-destructive gap-1.5 font-bold"
                            onClick={() => {
                                setEndingRecipient(row);
                                setEndOpen(true);
                            }}
                        >
                            <X className="w-3.5 h-3.5" /> إنهاء الاستلام
                        </Button>
                    ) : null}
                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-amber-600" onClick={() => acknowledgeMutation.mutate({ entityType: "SalaryRecipient", entityId: row.recipientID })} title="قبول الرفض">
                            <Check className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    if (error) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل بيانات مستلمي المعاش</p>
        </div>
    );

    useCreateShortcut(crud.openCreate, canCreate);

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start">
            <Card className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 sm:gap-4 p-3 sm:p-5">
                <div className="space-y-0.5 sm:space-y-1">
                    <Heading level="h3" className="font-bold text-sm sm:text-base">مستلمي المعاش</Heading>
                    <p className="text-[10px] sm:text-xs text-muted-foreground font-medium hidden sm:block">سجل الأشخاص المخولين باستلام المعاش (متقاعد، وكلاء، أوصياء)</p>
                </div>
                {canCreate && (
                    <Button size="sm" className="gap-1.5 sm:gap-2 rounded-xl h-8 sm:h-10 font-bold text-xs sm:text-sm px-3 sm:px-4" onClick={crud.openCreate}>
                        <Wallet className="w-3.5 h-3.5 sm:w-4 sm:h-4" /> تعيين مستلم جديد
                    </Button>
                )}
            </Card>

            <DataTable
                columns={columns}
                data={recipients}
                loading={loadingRecipients}
                emptyMessage="لا يوجد مستلمين مسجلين حالياً"
                renderMobileCard={(item) => (
                    <div className="p-3 space-y-2">
                        <div className="flex items-center justify-between">
                            <span className="font-bold text-xs truncate max-w-[200px]">
                                {item.retireeFullName || item.agentFullName || item.entitledFullName || item.guardianFullName || "—"}
                            </span>
                            <span className={cn("w-2 h-2 rounded-full shrink-0", item.isActive ? "bg-emerald-500" : "bg-muted-foreground")} title={item.isActive ? "نشط" : "منتهي"} />
                        </div>
                        <div className="flex items-center gap-1.5 flex-wrap">
                            {item.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                <Badge variant="warning" className="text-[8px] h-3.5 shrink-0">قيد المراجعة</Badge>
                            )}
                            {item.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                <Badge variant="destructive" className="text-[8px] h-3.5 shrink-0">مرفوض</Badge>
                            )}
                        </div>
                        <div className="text-[10px] text-muted-foreground flex items-center gap-3 flex-wrap">
                            <span>البدء: <span className="font-mono">{formatDate(item.startDate)}</span></span>
                            {item.endDate && <span>الانتهاء: <span className="font-mono text-rose-600">{formatDate(item.endDate)}</span></span>}
                        </div>
                        <div className="flex justify-end gap-1">
                            {item.isActive && canEnd && (
                                <Button
                                    size="sm"
                                    variant="ghost"
                                    className="text-destructive hover:bg-destructive/5 gap-1 font-bold min-h-[32px] h-8 text-xs px-2"
                                    onClick={() => {
                                        setEndingRecipient(item);
                                        setEndOpen(true);
                                    }}
                                >
                                    <X className="w-3.5 h-3.5" /> إنهاء
                                </Button>
                            )}
                            {item.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                <Button
                                    size="sm"
                                    variant="ghost"
                                    className="min-h-[32px] h-8 text-amber-600 gap-1 text-xs px-2"
                                    onClick={() => acknowledgeMutation.mutate({ entityType: "SalaryRecipient", entityId: item.recipientID })}
                                >
                                    <Check className="w-3.5 h-3.5" /> قبول
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            />

            <RecipientsDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                onSubmit={handleCreate}
                isLoading={createMutation.isPending}
                retireeId={retireeId}
                retireeName={retireeName}
                lookups={{
                    agents,
                    entitleds,
                    guardians
                }}
            />

            {endOpen && endingRecipient && (
                <EndRecipientDialog
                    open={endOpen}
                    onOpenChange={(o) => !o && setEndOpen(false)}
                    onSubmit={handleEnd}
                    isLoading={endMutation.isPending}
                    recipientName={endingRecipient.retireeFullName || endingRecipient.agentFullName || endingRecipient.entitledFullName || endingRecipient.guardianFullName || ""}
                />
            )}
        </div>
    );
}
