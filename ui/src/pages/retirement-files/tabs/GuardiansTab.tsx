import { useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, Check, FileText, Hash, Inbox, ShieldCheck, UserPlus, X } from "lucide-react";
import { useEffect, useMemo,useState } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { AddGuardianMinorDialog } from "@/components/domain/retirement/AddGuardianMinorDialog";
import { GuardianDialog } from "@/components/domain/retirement/GuardianDialog";
import { Badge, Button, Card, EditableImage, EmptyState, Heading, LoadingIndicator, useConfirm } from "@/components/ui";
import { PENDING_REVIEW_STATUS } from "@/constants";
import { useApiMutation, useCreateShortcut } from "@/hooks";
import { useEntitledPersonsQuery } from "@/hooks/queries";
import { cn,formatDate, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { EntitledPersonSummary } from "@/types/retirement";

// ─── Types ─────────────────────────────────────────────

export interface GuardianMinor {
    entitledID: number;
    entitledPersonName: string;
    isActive: boolean;
    startDate: string;
    endDate?: string;
}

export interface Guardian {
    guardianID: number;
    minors: GuardianMinor[];
    fullName: string;
    nationalID: string;
    personalPhotoPath?: string;
    phone?: string;
    guardianshipDocNumber: string;
    startDate: string;
    endDate?: string;
    isActive: boolean;
    createdAt: string;
    pendingReviewStatus?: number;
}

interface GuardiansTabProps { fileId: number; }

/**
 * تبويب إدارة الأوصياء والمستحقين القاصرين لملف التقاعد
 */
export function GuardiansTab({ fileId }: GuardiansTabProps) {
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();

    // الصلاحيات
    const canCreate = useHasPermission("Guardian.Add");
    const canEnd = useHasPermission("Guardian.Delete", "Guardian.Manage");

    // حالة الحوارات
    const [addOpen, setAddOpen] = useState(false);
    const [addMinorOpen, setAddMinorOpen] = useState(false);
    const [selectedGuardianId, setSelectedGuardianId] = useState<number | null>(null);

    const { data: entitledsData = [], isLoading: isLoadingEntitleds } = useEntitledPersonsQuery(fileId);

    const { data: guardiansData = [], isLoading: isLoadingGuardians, error: guardiansError } = useQuery({
        queryKey: ["guardians-tab", fileId],
        queryFn: () => retirementApi.getGuardians(fileId).then(data =>
            (data as Guardian[]).map(g => ({ ...g, minors: g.minors || [] }))
        )
    });

    const acknowledgeMutation = useApiMutation(
        (params: { entityType: string; entityId: number }) =>
            retirementApi.acknowledgeEntityRejection(params.entityType, params.entityId),
        {
            successMessage: "تم قبول الرفض وإعادة الكيان لحالته الطبيعية",
            invalidateKeys: [["guardians-tab", fileId], ["retirement-file"]]
        }
    );

    const isLoading = isLoadingEntitleds || isLoadingGuardians;

    const entitleds = entitledsData as EntitledPersonSummary[];
    const guardians = guardiansData || [];
    const hasActiveGuardian = guardians.some(g => g.isActive);

    const handleOpenCreate = () => setAddOpen(true);

    useCreateShortcut(handleOpenCreate, canCreate && !hasActiveGuardian);

    // تصفية المستحقين القاصرين المتاحين للوصاية
    const availableEntitleds = useMemo(() => entitleds.filter(ep => {
        if (!ep.isEligible || !ep.dateOfBirth) return false;
        // حساب العمر بشكل تقريبي
        const age = (new Date().getTime() - new Date(ep.dateOfBirth).getTime()) / (1000 * 60 * 60 * 24 * 365.25);
        if (age >= 18) return false;

        const hasActiveG = guardians.some(g =>
            g.isActive && g.minors && g.minors.some(m => m.entitledID === ep.entitledID && m.isActive)
        );
        return !hasActiveG;
    }), [entitleds, guardians]);

    // طفرات البيانات
    const endMutation = useApiMutation(
        (id: number) => retirementApi.endGuardianship(id, { reason: "تم إنهاء الوصاية من قبل المستخدم" }),
        {
            successMessage: "تم إنهاء الوصاية بنجاح",
            invalidateKeys: [["guardians-tab", fileId], ["retirement-file"]]
        }
    );

    const updatePhotoMutation = useApiMutation(
        ({ id, file }: { id: number, file: File }) => {
            const formData = new FormData();
            formData.append("personalPhoto", file);
            return retirementApi.updateGuardian(id, formData);
        },
        {
            successMessage: "تم تحديث الصورة الشخصية للوصي بنجاح",
            invalidateKeys: [["guardians-tab", fileId]]
        }
    );

    // المعالجات
    const handleEndGuardianship = async (id: number) => {
        const ok = await confirm({
            title: "هل تريد إنهاء هذه الوصاية؟",
            description: "سيتم إيقاف صلاحية الوصي عن كافة القاصرين التابعين له.",
            variant: "danger"
        });
        if (ok) endMutation.mutate(id);
    };

    const handleAddMinor = (id: number) => {
        setSelectedGuardianId(id);
        setAddMinorOpen(true);
    };

    useEffect(() => {
        if (guardiansError) toast.error("حدث خطأ أثناء تحميل بيانات الأوصياء");
    }, [guardiansError]);

    if (guardiansError) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل بيانات الأوصياء</p>
        </div>
    );

    if (isLoading) return <LoadingIndicator message="جاري تحميل الأوصياء..." />;

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start" dir="rtl">
            {ConfirmDialogElement}
            <Card className="flex flex-col md:flex-row md:items-center justify-between gap-2 md:gap-4 bg-muted/30 p-3 md:p-5">
                <div className="space-y-0.5 md:space-y-1">
                    <Heading level="h3" className="font-bold flex items-center gap-1.5 md:gap-2 text-sm md:text-base">
                        <ShieldCheck className="w-4 h-4 md:w-5 md:h-5 text-primary" /> الأوصياء المعتمدون
                    </Heading>
                    <p className="text-xs md:text-sm text-muted-foreground font-medium hidden sm:block">إدارة الأوصياء القانونيين على القاصرين من الورثة</p>
                </div>
                {canCreate && !hasActiveGuardian && (
                    <Button size="sm" onClick={() => setAddOpen(true)} className="gap-1.5  font-bold h-8 md:h-auto text-xs md:text-sm px-3 md:px-4">
                        تسجيل وصي جديد
                    </Button>
                )}
            </Card>

            {guardians.length === 0 ? (
                <EmptyState icon={Inbox} title="لا يوجد أوصياء مسجلون لهذا الملف" />
            ) : (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                    {guardians.map(g => (
                        <div key={g.guardianID} className={cn("border rounded-2xl p-5 bg-card  transition-all border-border/60", !g.isActive && "opacity-75 grayscale bg-muted/20")}>
                            <div className="flex gap-4 items-start mb-4">
                                <div className="flex-shrink-0">
                                    <EditableImage
                                        src={getPhotoUrl(resolvePersonalPhotoPath(g))}
                                        alt={g.fullName}
                                        fallbackIcon={<ShieldCheck className="w-8 h-8 opacity-40" />}
                                        className="w-16 h-16 rounded-xl object-cover border-2 border-background "
                                        canEdit={canEnd && g.isActive}
                                        onUpload={async (file) => {
                                            await updatePhotoMutation.mutateAsync({ id: g.guardianID, file });
                                        }}
                                    />
                                </div>
                                <div className="flex-1 min-w-0">
                                    <div className="flex justify-between items-start gap-2">
                                        <h4 className="font-black text-lg truncate text-foreground flex items-center gap-2">
                                            {g.fullName}
                                            {g.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                                <Badge variant="warning" className="text-[10px] sm:text-xs h-5">قيد المراجعة</Badge>
                                            )}
                                            {g.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                                <Badge variant="destructive" className="text-[10px] sm:text-xs h-5">مرفوض</Badge>
                                            )}
                                        </h4>
                                        <Badge variant={g.isActive ? "success" : "secondary"} className="font-black h-5">{g.isActive ? "نشط" : "منتهي"}</Badge>
                                    </div>
                                    <p className="text-xs text-muted-foreground flex items-center gap-1.5 mt-1 font-mono font-bold">
                                        <Hash className="w-3 h-3 text-muted-primary" /> {g.nationalID}
                                    </p>
                                    <p className="text-[10px] sm:text-xs text-muted-foreground flex items-center gap-1.5 mt-1.5 font-medium">
                                        <FileText className="w-3 h-3 text-muted-primary" /> صك وصاية رقم: <span className="font-mono font-black text-foreground/80">{g.guardianshipDocNumber}</span>
                                    </p>
                                </div>
                            </div>

                            <div className="bg-muted/40 rounded-xl p-4 space-y-3 mb-4 border border-border/40">
                                <p className="text-[10px] sm:text-xs font-black text-primary/70 uppercase tracking-widest flex items-center gap-2">
                                    <span className="w-1.5 h-1.5 rounded-full bg-primary" />
                                    المستحقين تحت الوصاية
                                </p>
                                <div className="space-y-2">
                                    {g.minors.length === 0 ? (
                                        <p className="text-xs text-muted-foreground italic py-1">لا يوجد قاصرين مرتبطين حالياً</p>
                                    ) : g.minors.map(m => (
                                        <div key={m.entitledID} className="flex items-center justify-between text-sm py-1.5 border-b border-border/30 last:border-0">
                                            <span className={cn("font-bold", !m.isActive && "line-through text-muted-foreground opacity-50")}>{m.entitledPersonName}</span>
                                            <span className="text-[10px] sm:text-xs font-mono text-muted-foreground bg-background px-1.5 py-0.5 rounded border border-border/50">منذ: {formatDate(m.startDate)}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>

                            <div className="flex justify-between items-center pt-4 border-t border-dashed border-border/60">
                                <span className="text-[10px] sm:text-xs text-muted-foreground flex items-center gap-1 font-medium">
                                    التسجيل: <span className="font-mono font-bold">{formatDate(g.createdAt)}</span>
                                </span>
                                <div className="flex gap-2">
                                    {g.isActive && canCreate && (
                                        <Button size="sm" variant="ghost" className="min-h-[44px] px-2.5 text-primary hover:bg-primary/5 text-xs font-black" onClick={() => handleAddMinor(g.guardianID)}>
                                            <UserPlus className="w-3.5 h-3.5 ms-1" /> إضافة قاصر
                                        </Button>
                                    )}
                                    {g.isActive && canEnd && (
                                        <Button size="sm" variant="ghost" className="min-h-[44px] px-2.5 text-destructive hover:bg-destructive/5 text-xs font-black" onClick={() => handleEndGuardianship(g.guardianID)}>
                                            <X className="w-3.5 h-3.5 ms-1" /> إنهاء الوصاية
                                        </Button>
                                    )}
                                    {g.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                        <Button size="sm" variant="ghost" className="min-h-[44px] px-2.5 text-amber-600 hover:bg-amber-50 text-xs font-black" onClick={() => acknowledgeMutation.mutate({ entityType: "Guardian", entityId: g.guardianID })}>
                                            <Check className="w-3.5 h-3.5 ms-1" /> قبول الرفض
                                        </Button>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            <GuardianDialog
                open={addOpen}
                onClose={() => setAddOpen(false)}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["guardians-tab", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
                }}
                fileId={fileId}
                availableEntitleds={availableEntitleds}
            />

            <AddGuardianMinorDialog
                open={addMinorOpen}
                onClose={() => setAddMinorOpen(false)}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["guardians-tab", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
                }}
                guardianId={selectedGuardianId || 0}
                availableEntitleds={availableEntitleds}
            />
        </div>
    );
}
