import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Check, Edit2,Eye, Trash2, UserCircle2, UserMinus, UserPlus, Users } from "lucide-react";
import { useEffect, useMemo,useState } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
<<<<<<< HEAD
=======
import { Badge, Button, Card, type CustomColumnDef, DataTable, EditableImage, Heading, useConfirm } from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
// ─── Standalone Dialogs ───────────────────────────────
import { EntitledPersonFormDialog } from "@/components/domain/retirement/EntitledPersonFormDialog";
import { ReactivateEntitledPersonDialog } from "@/components/domain/retirement/ReactivateEntitledPersonDialog";
import { StopEntitledPersonDialog } from "@/components/domain/retirement/StopEntitledPersonDialog";
import { ViewEntitledPersonDialog } from "@/components/domain/retirement/ViewEntitledPersonDialog";
<<<<<<< HEAD
import { Badge, Button, type CustomColumnDef, DataTable, EditableImage, TabHeader, useConfirm } from "@/components/ui";
import { PENDING_REVIEW_STATUS } from "@/constants";
import { useApiMutation, useCreateShortcut } from "@/hooks";
import { normalizeDateInputValue } from "@/lib/date";
=======
import { PENDING_REVIEW_STATUS } from "@/constants";
import { useApiMutation, useCreateShortcut } from "@/hooks";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { cn, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Types ─────────────────────────────────────────────

export interface EntitledPerson {
    entitledID: number;
    fileID: number;
    fullName: string;
    nationalID: string;
    relationshipName?: string;
    dateOfBirth?: string;
    ageYears?: number;
    phone?: string;
    socialStatusID?: number;
    socialStatusName?: string;
    healthStatusID?: number;
    healthStatusName?: string;
    hasDisabilityException: boolean;
    isEligible: boolean;
    isDeceased: boolean;
    stopReason?: string;
    createdAt: string;
    updatedAt?: string;
    pendingReviewStatus?: number;
    relationshipID?: number;
    personalPhotoPath?: string | null;
<<<<<<< HEAD
    sharePercent?: number | null;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}

interface EntitledPersonsTabProps {
    fileId: number;
}

/**
 * تبويب الورثة والمستحقين لملف التقاعد
 */
export function EntitledPersonsTab({ fileId }: EntitledPersonsTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canView = useHasPermission("EntitledPerson.View");
    const canCreate = useHasPermission("EntitledPerson.Create");
    const canEdit = useHasPermission("EntitledPerson.Edit");
    const canDelete = useHasPermission("EntitledPerson.Delete");
    const canManageLifecycle = useHasPermission("EntitledPerson.Lifecycle");

    // ─── Dialog State ──────────────────────────────────
    const [formOpen, setFormOpen] = useState(false);
    const [formMode, setFormMode] = useState<"create" | "edit">("create");
    const [selectedPerson, setSelectedPerson] = useState<EntitledPerson | null>(null);

    const [stopOpen, setStopOpen] = useState(false);
    const [reactivateOpen, setReactivateOpen] = useState(false);
    const [viewOpen, setViewOpen] = useState(false);

    // ─── Data Query ─────────────────────────────────────
    const { data: persons = [], isLoading, error } = useQuery({
        queryKey: ["entitled-persons", fileId],
        queryFn: () => retirementApi.getEntitledPersons(fileId).then(data => data as unknown as EntitledPerson[])
    });

    const acknowledgeMutation = useApiMutation(
        (params: { entityType: string; entityId: number }) =>
            retirementApi.acknowledgeEntityRejection(params.entityType, params.entityId),
        {
<<<<<<< HEAD
            successMessage: "تم إعادة المراجعة وإعادة الكيان لحالته الطبيعية",
=======
            successMessage: "تم قبول الرفض وإعادة الكيان لحالته الطبيعية",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            invalidateKeys: [["entitled-persons", fileId], ["retirement-file"]]
        }
    );

    const getPersonPhotoUrl = (person: EntitledPerson) => getPhotoUrl(resolvePersonalPhotoPath(person));

    const updatePhotoMutation = useApiMutation(
        ({ id, data }: { id: number; data: FormData }) => retirementApi.updateEntitledPerson(id, data),
        {
            successMessage: "تم تحديث الصورة الشخصية للمستحق بنجاح",
            invalidateKeys: [["entitled-persons", fileId], ["retirement-file"]],
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => retirementApi.deleteEntitledPerson(id),
        {
            successMessage: "تم حذف المستحق بنجاح",
            invalidateKeys: [["entitled-persons", fileId], ["retirement-file"]]
        }
    );

    const handleDelete = async (person: EntitledPerson) => {
        const ok = await confirm({
            title: "تأكيد الحذف",
            description: `هل أنت متأكد من حذف "${person.fullName}"؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(person.entitledID);
    };

    // ─── Handlers ────────────────────────────────────────

    const handleOpenCreate = () => {
        setFormMode("create");
        setSelectedPerson(null);
        setFormOpen(true);
    };

    const handleOpenEdit = (p: EntitledPerson) => {
        setFormMode("edit");
        setSelectedPerson(p);
        setFormOpen(true);
    };

    const handleOpenStop = (p: EntitledPerson) => {
        setSelectedPerson(p);
        setStopOpen(true);
    };

    const handleOpenReactivate = (p: EntitledPerson) => {
        setSelectedPerson(p);
        setReactivateOpen(true);
    };

    const handleOpenView = (p: EntitledPerson) => {
        setSelectedPerson(p);
        setViewOpen(true);
    };

    const columns = useMemo<CustomColumnDef<EntitledPerson>[]>(() => [
        {
            key: "photo",
            header: "الصورة",
            render: (row) => (
                <EditableImage
                    src={getPersonPhotoUrl(row)}
                    alt={row.fullName}
                    fallbackIcon={<UserCircle2 className="w-5 h-5 text-muted-foreground" />}
                    className="w-10 h-10 rounded-full object-cover border bg-muted/30"
                    canEdit={canEdit}
                    size="sm"
                    onUpload={async (file) => {
                        const formData = new FormData();
                        formData.append("fullName", row.fullName);
                        formData.append("nationalID", row.nationalID || "");
                        formData.append("relationshipID", (row.relationshipID || 0).toString());
                        if (row.dateOfBirth) {
<<<<<<< HEAD
                            // DateOnly في الـAPI: "YYYY-MM-DD" فقط، بلا طابع زمني.
                            const dateOfBirth = normalizeDateInputValue(row.dateOfBirth);
                            if (dateOfBirth) formData.append("dateOfBirth", dateOfBirth);
=======
                            const d = new Date(row.dateOfBirth);
                            if (!isNaN(d.getTime())) {
                                formData.append("dateOfBirth", d.toISOString());
                            } else {
                                formData.append("dateOfBirth", row.dateOfBirth);
                            }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        }
                        formData.append("phone", row.phone || "");
                        formData.append("socialStatusID", (row.socialStatusID || 0).toString());
                        formData.append("healthStatusID", (row.healthStatusID || 0).toString());
                        formData.append("hasDisabilityException", (row.hasDisabilityException ?? false).toString());
                        formData.append("PersonalPhoto", file);

                        await updatePhotoMutation.mutateAsync({ id: row.entitledID, data: formData });
                    }}
                />
            )
        },
        {
            key: "fullName",
            header: "الاسم الكامل",
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
            key: "relationshipName",
            header: "القرابة",
            accessor: (row) => row.relationshipName || "—"
        },
        {
<<<<<<< HEAD
            key: "sharePercent",
            header: "حصة المعاش",
            render: (row) => row.sharePercent == null ? "—" : `${row.sharePercent}%`
        },
        {
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.nationalID || "—"}</span>
        },
        {
            key: "ageYears",
            header: "العمر",
            render: (row) => (
                <div className="text-center font-mono">
                    {row.ageYears} <span className="text-[10px] sm:text-xs text-muted-foreground">سنة</span>
                </div>
            )
        },
        {
            key: "socialStatusName",
            header: "الحالة الاجتماعية",
            accessor: (row) => row.socialStatusName || "—"
        },
        {
            key: "isEligible",
            header: "حالة الاستحقاق",
            render: (row) => (
                <div className="text-center">
                    <Badge variant={row.isEligible ? "success" : "ghost"}>
                        {row.isEligible ? "مستحق" : "موقوف"}
                    </Badge>
                </div>
            )
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex justify-center gap-1">
                    {canView && (
                        <Button 
                            size="icon" 
                            variant="ghost-action" 
                            className="min-h-[44px] min-w-[44px] rounded-xl text-blue-600 hover:bg-blue-50" 
                            onClick={() => handleOpenView(row)} 
                            title="عرض"
                        >
                            <Eye className="w-4 h-4" />
                        </Button>
                    )}
                    {canEdit && (
                        <Button 
                            size="icon" 
                            variant="ghost-action" 
                            className="min-h-[44px] min-w-[44px] rounded-xl text-muted-foreground hover:bg-muted" 
                            onClick={() => handleOpenEdit(row)} 
                            title="تعديل"
                        >
                            <Edit2 className="w-4 h-4" />
                        </Button>
                    )}
                    {canManageLifecycle && (
                        row.isEligible ? (
                            <Button 
                                size="icon" 
                                variant="ghost-action" 
                                className="min-h-[44px] min-w-[44px] rounded-xl text-destructive hover:bg-destructive/10" 
                                onClick={() => handleOpenStop(row)} 
                                title="إيقاف"
                            >
                                <UserMinus className="w-4 h-4" />
                            </Button>
                        ) : !row.isDeceased && (
                            <Button 
                                size="icon" 
                                variant="ghost-action" 
                                className="min-h-[44px] min-w-[44px] rounded-xl text-emerald-600 hover:bg-emerald-50" 
                                onClick={() => handleOpenReactivate(row)} 
                                title="تفعيل"
                            >
                                <UserPlus className="w-4 h-4" />
                            </Button>
                        )
                    )}
                    {canDelete && (
                        <Button 
                            size="icon" 
                            variant="ghost-action" 
                            className="min-h-[44px] min-w-[44px] rounded-xl text-red-600 hover:bg-red-50" 
                            onClick={() => handleDelete(row)} 
                            title="حذف"
                        >
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                        <Button
                            size="icon"
                            variant="ghost-action"
                            className="min-h-[44px] min-w-[44px] rounded-xl text-amber-600 hover:bg-amber-50"
                            onClick={() => acknowledgeMutation.mutate({ entityType: "EntitledPerson", entityId: row.entitledID })}
<<<<<<< HEAD
                            title="إعادة المراجعة"
=======
                            title="قبول الرفض"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        >
                            <Check className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ], [canView, canEdit, canManageLifecycle, canDelete, getPersonPhotoUrl, updatePhotoMutation]);

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل بيانات الورثة والمستحقين");
    }, [error]);

    useCreateShortcut(handleOpenCreate, canCreate && !formOpen);

    if (error) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل بيانات الورثة والمستحقين</p>
        </div>
    );

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500">
<<<<<<< HEAD
            <TabHeader
                title="قائمة الورثة والمستحقين"
                description="إدارة بيانات أفراد عائلة المتقاعد المستحقين للمعاش"
                icon={Users}
                action={{
                    label: "إضافة مستحق",
                    icon: UserPlus,
                    onClick: handleOpenCreate,
                    show: canCreate,
                    title: "إضافة مستحق (Alt+A)"
                }}
            />
=======
            <Card className="flex flex-col md:flex-row md:items-center justify-between gap-2 md:gap-4 bg-muted/30 p-3 md:p-5">
                <div className="space-y-0.5 md:space-y-1 text-start">
                    <Heading level="h3" className="font-bold flex items-center gap-1.5 md:gap-2 text-sm md:text-base">
                        <Users className="w-4 h-4 md:w-5 md:h-5 text-primary" /> قائمة الورثة والمستحقين
                    </Heading>
                    <p className="text-xs md:text-sm text-muted-foreground hidden sm:block">
                        إدارة بيانات أفراد عائلة المتقاعد المستحقين للمعاش
                    </p>
                </div>
                {canCreate && (
                    <Button 
                        size="sm" 
                        onClick={handleOpenCreate} 
                        title="إضافة مستحق (Ctrl+Alt+N)"
                        className="gap-1 rounded-xl font-bold h-8 md:h-10 px-4 md:px-6 transition-all text-xs md:text-sm"
                    >
                        <UserPlus className="w-3.5 h-3.5 md:w-4 md:h-4 ms-1" /> إضافة مستحق
                    </Button>
                )}
            </Card>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            <DataTable
                columns={columns}
                data={persons}
                loading={isLoading}
                emptyMessage="لا يوجد ورثة مسجلين لهذا الملف التقاعدي"
                renderMobileCard={(row) => (
                    <div className="p-3 space-y-2">
                        <div className="flex items-start gap-2.5">
                            <EditableImage
                                src={getPersonPhotoUrl(row)}
                                alt={row.fullName}
                                fallbackIcon={<UserCircle2 className="w-7 h-7 text-muted-foreground" />}
                                className="w-11 h-11 rounded-full object-cover border bg-muted/30 shrink-0"
                                canEdit={canEdit}
                                size="sm"
                                onUpload={async (file) => {
                                    const formData = new FormData();
                                    formData.append("fullName", row.fullName);
                                    formData.append("nationalID", row.nationalID || "");
                                    formData.append("relationshipID", (row.relationshipID || 0).toString());
                                    if (row.dateOfBirth) {
<<<<<<< HEAD
                                        // DateOnly في الـAPI: "YYYY-MM-DD" فقط، بلا طابع زمني.
                                        const dateOfBirth = normalizeDateInputValue(row.dateOfBirth);
                                        if (dateOfBirth) formData.append("dateOfBirth", dateOfBirth);
=======
                                        const d = new Date(row.dateOfBirth);
                                        if (!isNaN(d.getTime())) {
                                            formData.append("dateOfBirth", d.toISOString());
                                        } else {
                                            formData.append("dateOfBirth", row.dateOfBirth);
                                        }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    }
                                    formData.append("phone", row.phone || "");
                                    formData.append("socialStatusID", (row.socialStatusID || 0).toString());
                                    formData.append("healthStatusID", (row.healthStatusID || 0).toString());
                                    formData.append("hasDisabilityException", (row.hasDisabilityException ?? false).toString());
                                    formData.append("PersonalPhoto", file);

                                    await updatePhotoMutation.mutateAsync({ id: row.entitledID, data: formData });
                                }}
                            />
                            <div className="flex-1 min-w-0">
                                <p className="font-bold text-xs truncate">{row.fullName}</p>
                                <div className="flex items-center gap-1.5 mt-0.5 flex-wrap">
                                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                        <Badge variant="warning" className="text-[8px] h-3.5 shrink-0">قيد المراجعة</Badge>
                                    )}
                                    {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                        <Badge variant="destructive" className="text-[8px] h-3.5 shrink-0">مرفوض</Badge>
                                    )}
                                    <span className="text-[10px] text-muted-foreground">{row.relationshipName || "—"}</span>
                                    <span className={cn("w-2 h-2 rounded-full shrink-0", row.isEligible ? "bg-emerald-500" : "bg-muted-foreground")} title={row.isEligible ? "مستحق" : "موقوف"} />
                                </div>
                            </div>
                        </div>
                        <div className="flex items-center gap-1 text-xs">
                            <span className="text-muted-foreground">العمر:</span>
                            <span className="font-bold">{row.ageYears || "—"}</span>
                        </div>
                        <div className="flex justify-end items-center gap-0">
                            {canView && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-blue-600" onClick={() => handleOpenView(row)}>
                                    <Eye className="w-3.5 h-3.5" />
                                </Button>
                            )}
                            {canEdit && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-muted-foreground" onClick={() => handleOpenEdit(row)}>
                                    <Edit2 className="w-3.5 h-3.5" />
                                </Button>
                            )}
                            {canDelete && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-destructive" onClick={() => handleDelete(row)}>
                                    <Trash2 className="w-3.5 h-3.5" />
                                </Button>
                            )}
                            {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-amber-600" onClick={() => acknowledgeMutation.mutate({ entityType: "EntitledPerson", entityId: row.entitledID })}>
                                    <Check className="w-3.5 h-3.5" />
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            />

            {/* ─── Dialogs ──────────────── */}

            <EntitledPersonFormDialog
                open={formOpen}
                onOpenChange={setFormOpen}
                mode={formMode}
                initialData={selectedPerson || undefined}
                fileId={fileId}
            />

            <StopEntitledPersonDialog
                open={stopOpen}
                onOpenChange={setStopOpen}
                personId={selectedPerson?.entitledID || 0}
                targetName={selectedPerson?.fullName || ""}
                fileId={fileId}
            />

            <ReactivateEntitledPersonDialog
                open={reactivateOpen}
                onOpenChange={setReactivateOpen}
                personId={selectedPerson?.entitledID || 0}
                targetName={selectedPerson?.fullName || ""}
                fileId={fileId}
            />

            <ViewEntitledPersonDialog
                open={viewOpen}
                onOpenChange={setViewOpen}
                person={selectedPerson}
            />

            {ConfirmDialogElement}
        </div>
    );
<<<<<<< HEAD
}
=======
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
