import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Edit2, Eye, Trash2, UserCheck, X } from "lucide-react";
import { useCallback,useMemo, useState } from "react";

import { retirementApi } from "@/api/retirement";
import { Badge, Button, CrudPageLayout, type CustomColumnDef, EditableImage,useConfirm } from "@/components/ui";
import { AgentDialog } from "@/components/domain/retirement/AgentDialog";
import { ViewAgentDialog } from "@/components/domain/retirement/ViewAgentDialog";
import { PENDING_REVIEW_STATUS } from "@/constants";
import { useApiMutation, useCreateShortcut, useCrudState } from "@/hooks";
import { formatDate, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { AgentDto } from "@/types/retirement";

// ─── Types ─────────────────────────────────────────────

export interface Agent {
    agentID: number;
    fileID: number;
    fileNumber: string;
    fullName: string;
    nationalID: string;
    personalPhotoPath?: string;
    phone?: string;
    agencyNumber: string;
    agencyDate: string;
    agencyEndDate?: string;
    notes?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
    pendingReviewStatus?: number;
}

interface AgentsTabProps { fileId: number; fileNumber?: string; }

/**
 * تبويب إدارة الوكلاء لملف التقاعد
 */
export function AgentsTab({ fileId, fileNumber }: AgentsTabProps) {
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<Agent>();

    // الصلاحيات
    const canView = useHasPermission("Agent.View", "File.View.Sensitive");
    const canCreate = useHasPermission("Agent.Add");
    const canEdit = useHasPermission("Agent.Edit", "Agent.Manage");
    const canDelete = useHasPermission("Agent.Delete", "Agent.Manage");

    const [modalMode, setModalMode] = useState<"create" | "edit">("create");

    // جلب البيانات
    const { data: agents = [], isLoading } = useQuery({
        queryKey: ["agents", fileId],
        queryFn: () => retirementApi.getAgents(fileId).then(data => data as unknown as Agent[])
    });

    const acknowledgeMutation = useApiMutation(
        (params: { entityType: string; entityId: number }) =>
            retirementApi.acknowledgeEntityRejection(params.entityType, params.entityId),
        {
            successMessage: "تم قبول الرفض وإعادة الكيان لحالته الطبيعية",
            invalidateKeys: [["agents", fileId], ["retirement-file"]]
        }
    );

    const getAgentPhotoUrl = (agent: Agent) => getPhotoUrl(resolvePersonalPhotoPath(agent));

    const handleOpenCreate = useCallback(() => {
        setModalMode("create");
        crud.openCreate();
    }, [crud]);

    const handleOpenEdit = useCallback((agent: Agent) => {
        setModalMode("edit");
        crud.openEdit(agent);
    }, [crud]);

    const deactivateMutation = useApiMutation(
        (id: number) => retirementApi.deactivateAgent(id),
        {
            successMessage: "تم إلغاء تفعيل الوكيل بنجاح",
            invalidateKeys: [["agents", fileId], ["retirement-file"]],
            onSuccess: () => crud.closeDelete()
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => retirementApi.deleteAgent(id),
        {
            successMessage: "تم حذف الوكيل بنجاح",
            invalidateKeys: [["agents", fileId], ["retirement-file"]]
        }
    );

    const updatePhotoMutation = useApiMutation(
        ({ id, data }: { id: number; data: FormData }) => retirementApi.updateAgent(id, data),
        {
            successMessage: "تم تحديث الصورة الشخصية للوكيل بنجاح",
            invalidateKeys: [["agents", fileId], ["retirement-file"]],
        }
    );

    const handleDeactivate = useCallback(async (agent: Agent) => {
        const ok = await confirm({
            title: "تأكيد إلغاء التفعيل",
            description: `هل أنت متأكد من إلغاء تفعيل الوكيل "${agent.fullName}"؟`,
            variant: "danger"
        });
        if (ok) deactivateMutation.mutate(agent.agentID);
    }, [confirm, deactivateMutation]);

    const handleDelete = useCallback(async (agent: Agent) => {
        const ok = await confirm({
            title: "تأكيد الحذف",
            description: `هل أنت متأكد من حذف الوكيل "${agent.fullName}"؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(agent.agentID);
    }, [confirm, deleteMutation]);

    const columns = useMemo<CustomColumnDef<Agent>[]>(() => [
        {
            key: "photo",
            header: "الصورة",
            render: (row) => (
                <EditableImage
                    src={getAgentPhotoUrl(row)}
                    alt={row.fullName}
                    fallbackIcon={<UserCheck className="w-5 h-5 text-muted-foreground" />}
                    className="w-10 h-10 rounded-full object-cover border bg-muted/30"
                    canEdit={canEdit}
                    size="sm"
                    onUpload={async (file) => {
                        const formData = new FormData();
                        formData.append("FullName", row.fullName);
                        formData.append("NationalID", row.nationalID);
                        formData.append("Phone", row.phone || "");
                        formData.append("AgencyNumber", row.agencyNumber || "");
                        
                        const agencyDate = row.agencyDate ? row.agencyDate.split('T')[0] : "";
                        formData.append("AgencyDate", agencyDate);
                        
                        if (row.agencyEndDate && row.agencyEndDate !== "0001-01-01T00:00:00") {
                             formData.append("AgencyEndDate", row.agencyEndDate.split('T')[0]);
                        }
                        
                        formData.append("Notes", row.notes || "");
                        formData.append("PersonalPhoto", file);

                        await updatePhotoMutation.mutateAsync({ id: row.agentID, data: formData });
                    }}
                />
            )
        },
        {
            key: "fullName",
            header: "اسم الوكيل",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <span className="font-bold">{row.fullName}</span>
                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                        <Badge variant="warning" className="text-[10px] sm:text-xs h-5">قيد المراجعة</Badge>
                    )}
                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                        <Badge variant="destructive" className="text-[10px] sm:text-xs h-5">مرفوض</Badge>
                    )}
                </div>
            )
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.nationalID}</span>
        },
        { key: "agencyNumber", header: "رقم الوكالة", accessor: (row) => row.agencyNumber },
        {
            key: "agencyDate",
            header: "تاريخ الوكالة",
            render: (row) => <span className="text-xs font-mono">{formatDate(row.agencyDate)}</span>
        },
        {
            key: "agencyEndDate",
            header: "تاريخ الانتهاء",
            render: (row) => row.agencyEndDate ? <span className="text-xs font-mono text-amber-600">{formatDate(row.agencyEndDate)}</span> : "—"
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isActive ? "success" : "ghost"}>
                    {row.isActive ? "نشط" : "معطل"}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex gap-1 justify-center">
                    {canView && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-blue-600" onClick={() => crud.openViewer(row)} title="عرض التفاصيل">
                            <Eye className="w-4 h-4" />
                        </Button>
                    )}
                    {row.isActive && canEdit && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-muted-foreground" onClick={() => handleOpenEdit(row)} title="تعديل">
                            <Edit2 className="w-4 h-4" />
                        </Button>
                    )}
                    {row.isActive && canDelete && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-destructive" onClick={() => handleDeactivate(row)} title="إلغاء تفعيل">
                            <X className="w-4 h-4" />
                        </Button>
                    )}
                    {canDelete && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-red-600" onClick={() => handleDelete(row)} title="حذف">
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-amber-600" onClick={() => acknowledgeMutation.mutate({ entityType: "Agent", entityId: row.agentID })} title="قبول الرفض">
                            <Check className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ], [canView, canEdit, canDelete, crud, handleOpenEdit, handleDeactivate, handleDelete, deleteMutation, updatePhotoMutation, acknowledgeMutation]);

    useCreateShortcut(handleOpenCreate, canCreate && !agents.some(a => a.isActive));

    return (
        <CrudPageLayout
            title="الوكلاء المعتمدون"
            description={`إدارة الوكلاء المخولين بالاستلام أو المراجعة للملف التقاعدي`}
            createLabel="إضافة وكيل"
            onCreateClick={handleOpenCreate}
            canCreate={canCreate && !agents.some(a => a.isActive)}
            columns={columns}
            data={agents}
            loading={isLoading}
            emptyMessage="لا يوجد وكلاء مسجلون لهذا الملف."
            renderMobileCard={(row) => (
                <div className="p-3 space-y-2">
                    <div className="flex items-center gap-2.5">
                        <EditableImage
                            src={getAgentPhotoUrl(row)}
                            alt={row.fullName}
                            fallbackIcon={<UserCheck className="w-7 h-7 text-muted-foreground" />}
                            className="w-10 h-10 rounded-full object-cover border bg-muted/30 shrink-0"
                            canEdit={canEdit}
                            size="sm"
                            onUpload={async (file) => {
                                const formData = new FormData();
                                formData.append("FullName", row.fullName);
                                formData.append("NationalID", row.nationalID);
                                formData.append("Phone", row.phone || "");
                                formData.append("AgencyNumber", row.agencyNumber || "");
                                
                                const agencyDate = row.agencyDate ? row.agencyDate.split('T')[0] : "";
                                formData.append("AgencyDate", agencyDate);
                                
                                if (row.agencyEndDate && row.agencyEndDate !== "0001-01-01T00:00:00") {
                                     formData.append("AgencyEndDate", row.agencyEndDate.split('T')[0]);
                                }
                                
                                formData.append("Notes", row.notes || "");
                                formData.append("PersonalPhoto", file);

                                await updatePhotoMutation.mutateAsync({ id: row.agentID, data: formData });
                            }}
                        />
                        <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-1 flex-wrap">
                                <span className="font-bold text-xs break-words">{row.fullName}</span>
                                {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                    <Badge variant="warning" className="text-[8px] h-3.5 shrink-0">قيد المراجعة</Badge>
                                )}
                                {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                    <Badge variant="destructive" className="text-[8px] h-3.5 shrink-0">مرفوض</Badge>
                                )}
                            </div>
                            <p className="text-[10px] text-muted-foreground font-mono">{row.agencyNumber}</p>
                        </div>
                    </div>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-1.5 text-xs border-b border-border/60 pb-2">
                        <div>
                            <span className="text-muted-foreground">تاريخ الوكالة:</span>
                            <span className="font-bold me-1">{formatDate(row.agencyDate)}</span>
                        </div>
                        <div>
                            <span className="text-muted-foreground">الحالة:</span>
                            <Badge variant={row.isActive ? "success" : "ghost"} className="text-[9px] h-3.5 me-1">
                                {row.isActive ? "نشط" : "معطل"}
                            </Badge>
                        </div>
                    </div>
                    <div className="flex justify-end items-center gap-0.5">
                        {canView && (
                            <Button size="icon" variant="ghost" className="min-h-[36px] min-w-[36px] text-blue-600" onClick={() => crud.openViewer(row)}>
                                <Eye className="w-3.5 h-3.5" />
                            </Button>
                        )}
                        {row.isActive && canEdit && (
                            <Button size="icon" variant="ghost" className="min-h-[36px] min-w-[36px] text-muted-foreground" onClick={() => handleOpenEdit(row)}>
                                <Edit2 className="w-3.5 h-3.5" />
                            </Button>
                        )}
                        {row.isActive && canDelete && (
                            <Button size="icon" variant="ghost" className="min-h-[36px] min-w-[36px] text-destructive" onClick={() => handleDeactivate(row)}>
                                <X className="w-3.5 h-3.5" />
                            </Button>
                        )}
                        {canDelete && (
                            <Button size="icon" variant="ghost" className="min-h-[36px] min-w-[36px] text-red-600" onClick={() => handleDelete(row)}>
                                <Trash2 className="w-3.5 h-3.5" />
                            </Button>
                        )}
                        {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                            <Button size="icon" variant="ghost" className="min-h-[36px] min-w-[36px] text-amber-600" onClick={() => acknowledgeMutation.mutate({ entityType: "Agent", entityId: row.agentID })}>
                                <Check className="w-3.5 h-3.5" />
                            </Button>
                        )}
                    </div>
                </div>
            )}
        >
            <AgentDialog
                open={crud.isFormOpen}
                onClose={crud.closeForm}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["agents", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
                }}
                fileId={fileId}
                fileNumber={fileNumber}
                mode={modalMode}
                initialData={(crud.selectedItem as unknown as AgentDto) || undefined}
            />

            <ViewAgentDialog
                open={crud.isViewerOpen}
                onClose={crud.closeViewer}
                agent={crud.viewerTarget}
                fileId={fileId}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
