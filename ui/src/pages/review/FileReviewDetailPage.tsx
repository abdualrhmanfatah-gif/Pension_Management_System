import { useQuery } from "@tanstack/react-query";
import {
<<<<<<< HEAD
    ArrowLeft,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    CheckCircle2,
    ExternalLink,
    Info,
    ListChecks,
<<<<<<< HEAD
    Loader2,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    Plus,
    ThumbsDown,
    ThumbsUp,
    User,
    XCircle,
<<<<<<< HEAD
} from "lucide-react";
import { useCallback, useEffect,useId, useMemo, useRef, useState } from "react";
=======
    Loader2,
    ArrowLeft,
} from "lucide-react";
import { useMemo, useState, useCallback, useRef, useId, useEffect } from "react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { ReviewFilePreview } from "@/components/common";
import {
    AlertBanner,
    Badge,
    Button,
    Card,
    Dialog,
    DialogContent,
<<<<<<< HEAD
    DialogFooter,
    DialogHeader,
    DialogTitle,
=======
    DialogHeader,
    DialogTitle,
    DialogFooter,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    EmptyState,
    Heading,
    Loader,
    PageLayout,
    RecordStateBadge,
    ScrollArea,
    Textarea,
    useConfirm,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { translateFieldName } from "@/lib/fieldTranslations";
import { cn, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { FieldChange } from "@/types/retirement";

const ReviewStatus = {
    PENDING: 0,
    APPROVED: 1,
    REJECTED: 2,
} as const;

type FieldValue = string | number | boolean | Date | null | undefined;

function formatFieldValue(val: FieldValue): string {
    if (val === null || val === undefined || val === "") return "—";
    if (typeof val === "string" && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/.test(val)) {
        return formatDate(val);
    }
    return String(val);
}

interface ReviewDiffSection {
    sectionName: string;
    entityDisplayName?: string | null;
    fieldChanges: FieldChange[];
}

interface ReviewDiff {
    sections: ReviewDiffSection[];
}

function useReviewStats(diff: ReviewDiff | undefined) {
    return useMemo(() => {
        if (!diff) return { total: 0, pending: 0, approved: 0, rejected: 0, progress: 0 };
        let total = 0, pending = 0, approved = 0, rejected = 0;
        diff.sections.forEach(s =>
            s.fieldChanges.forEach(c => {
                total++;
                if (c.itemStatus === ReviewStatus.PENDING) pending++;
                else if (c.itemStatus === ReviewStatus.APPROVED) approved++;
                else if (c.itemStatus === ReviewStatus.REJECTED) rejected++;
            })
        );
        const reviewed = total - pending;
        return {
            total, pending, approved, rejected,
            progress: total > 0 ? Math.round((reviewed / total) * 100) : 0,
        };
    }, [diff]);
}

function StatusIcon({ status, isNew }: { status: number; isNew: boolean }) {
    if (status === ReviewStatus.APPROVED) return <CheckCircle2 className="w-5 h-5" aria-hidden="true" />;
    if (status === ReviewStatus.REJECTED) return <XCircle className="w-5 h-5" aria-hidden="true" />;
    if (isNew) return <Plus className="w-5 h-5" aria-hidden="true" />;
    return <Info className="w-5 h-5" aria-hidden="true" />;
}

function statusIconBg(status: number) {
    if (status === ReviewStatus.APPROVED) return "bg-[hsl(var(--gov-success)/.1)] text-[hsl(var(--gov-success))]";
    if (status === ReviewStatus.REJECTED) return "bg-[hsl(var(--gov-danger)/.1)] text-[hsl(var(--gov-danger))]";
    return "bg-muted text-muted-foreground";
}

function statusBorder(status: number) {
    if (status === ReviewStatus.APPROVED) return "border-l-[3px] border-l-[hsl(var(--gov-success))] bg-[hsl(var(--gov-success)/.02)]";
    if (status === ReviewStatus.REJECTED) return "border-l-[3px] border-l-[hsl(var(--gov-danger))] bg-[hsl(var(--gov-danger)/.02)]";
    if (status === ReviewStatus.PENDING) return "border-l-[3px] border-l-primary bg-primary/[0.02]";
    return "border-muted";
}

function RejectItemDialog({ isOpen, onClose, onConfirm, fieldName, isLoading }: {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: (reason: string) => void;
    fieldName: string;
    isLoading: boolean;
}) {
    const [reason, setReason] = useState("");
    const textareaId = useId();

    const handleSubmit = useCallback(() => {
        const trimmed = reason.trim();
        if (!trimmed) return;
        onConfirm(trimmed);
        setReason("");
    }, [reason, onConfirm]);

    const handleClose = useCallback(() => { setReason(""); onClose(); }, [onClose]);

    return (
        <Dialog open={isOpen} onOpenChange={(open: boolean) => { if (!open) handleClose(); }}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="text-right text-sm font-black flex items-center gap-2">
                        <XCircle className="w-4 h-4 text-destructive" aria-hidden="true" />
                        رفض: {fieldName}
                    </DialogTitle>
                </DialogHeader>
                <div className="space-y-4 py-2">
                    <div>
                        <Textarea
                            id={textareaId}
                            label="سبب الرفض"
                            className="min-h-[80px] text-xs"
                            placeholder="اكتب سبب الرفض بالتفصيل..."
                            value={reason}
                            onChange={(e) => setReason(e.target.value)}
                            aria-required="true"
                            disabled={isLoading}
                            error={!reason.trim() ? "السبب مطلوب" : undefined}
                        />
                    </div>
                </div>
                <DialogFooter className="gap-2 sm:gap-0">
                    <Button variant="outline" size="sm" onClick={handleClose} disabled={isLoading}>إلغاء</Button>
                    <Button
                        variant="destructive"
                        size="sm"
                        onClick={handleSubmit}
                        disabled={!reason.trim() || isLoading}
                        loading={isLoading}
                    >
                        <XCircle className="w-3.5 h-3.5 mr-1" aria-hidden="true" />
                        تأكيد الرفض
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

function ReviewChangeCard({
    change,
    onApprove,
    onReject,
    canApprove,
    canReject,
    isMutating,
    mutationTargetId,
}: {
    change: FieldChange;
    onApprove: () => void;
    onReject: () => void;
    canApprove: boolean;
    canReject: boolean;
    isMutating: boolean;
    mutationTargetId?: number | null;
}) {
    const status = change.itemStatus ?? ReviewStatus.PENDING;
    const isPending = status === ReviewStatus.PENDING;
    const isApproved = status === ReviewStatus.APPROVED;
    const isNew = change.isNew === true;
    const hasDetails = Array.isArray(change.fieldDetails) && change.fieldDetails.length > 0;
    const isCurrentItemMutating = mutationTargetId === change.itemId && isMutating;

    return (
        <Card className={cn("border rounded-2xl transition-all hover:shadow-md", statusBorder(status))}>
            <div className="p-4 md:p-5">
                <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
                    <div className="flex items-start gap-3 flex-1 min-w-0">
                        <div className={cn("w-10 h-10 rounded-xl flex items-center justify-center shrink-0 border", statusIconBg(status))}>
                            <StatusIcon status={status} isNew={isNew} />
                        </div>
                        <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2 flex-wrap justify-end">
                                <h4 className="font-black text-sm truncate">{translateFieldName(change.fieldName)}</h4>
<<<<<<< HEAD
                                <Badge variant="ghost" className="h-4 p-0 text-[10px] font-bold opacity-100">
                                    {isNew ? "إضافة" : "تعديل"}
                                </Badge>
                                {change.modifiedByUserName && (
                                    <Badge variant="secondary" className="h-5 text-[10px] px-1.5 opacity-100 gap-1 rounded-md">
=======
                                <Badge variant="ghost" className="h-4 p-0 text-[10px] font-bold opacity-60">
                                    {isNew ? "إضافة" : "تعديل"}
                                </Badge>
                                {change.modifiedByUserName && (
                                    <Badge variant="secondary" className="h-5 text-[10px] px-1.5 opacity-80 gap-1 rounded-md">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        <User className="w-2.5 h-2.5" aria-hidden="true" />
                                        {change.modifiedByUserName}
                                    </Badge>
                                )}
                            </div>

                            {hasDetails && change.fieldDetails && (
<<<<<<< HEAD
                                <div className="mt-3 rounded-xl border border-border bg-muted/5 overflow-x-auto">
                                     <table className="w-full text-end border-collapse table-fixed" aria-label="مقارنة القيم">
                                        <thead>
                                            <tr className="bg-muted/30 border-b border-border">
=======
                                <div className="mt-3 rounded-xl border border-border/60 bg-muted/5 overflow-x-auto">
                                     <table className="w-full text-end border-collapse table-fixed" aria-label="مقارنة القيم">
                                        <thead>
                                            <tr className="bg-muted/30 border-b border-border/60">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                <th scope="col" className="px-3 py-2 text-[10px] font-black text-muted-foreground text-end">الحقل</th>
                                                <th scope="col" className="px-3 py-2 text-[10px] font-black text-muted-foreground text-end">السابق</th>
                                                <th scope="col" className="px-3 py-2 text-[10px] font-black text-muted-foreground text-end">الجديد</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {change.fieldDetails.map((detail, i) => (
                                                <tr
                                                    key={`${change.itemId ?? change.fieldName}-${detail.fieldName}-${i}`}
<<<<<<< HEAD
                                                    className="border-b border-border last:border-0 hover:bg-background/50 transition-colors"
=======
                                                    className="border-b border-border/40 last:border-0 hover:bg-background/50 transition-colors"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                >
<td className="px-3 py-2 text-xs font-bold text-foreground bg-muted/10 text-end truncate overflow-hidden">{translateFieldName(detail.fieldName)}</td>
                                                     <td className="px-3 py-2 text-end overflow-hidden">
                                                         {detail.isMedia ? (
                                                             <ReviewFilePreview path={detail.oldValue ?? ""} label="سابقاً" />
                                                         ) : (
<<<<<<< HEAD
                                                             <span className="text-xs font-medium text-[hsl(var(--gov-danger))] line-through opacity-100 break-all truncate block max-w-full">
=======
                                                             <span className="text-xs font-medium text-[hsl(var(--gov-danger))] line-through opacity-60 break-all truncate block max-w-full">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                                 <bdi>{formatFieldValue(detail.oldValue)}</bdi>
                                                             </span>
                                                         )}
                                                     </td>
                                                     <td className="px-3 py-2 text-end overflow-hidden">
                                                         {detail.isMedia ? (
                                                             <ReviewFilePreview path={detail.newValue ?? ""} label="حالياً" />
                                                         ) : (
                                                             <span className="text-xs font-black text-[hsl(var(--gov-success))] break-all truncate block max-w-full">
                                                                 <bdi>{formatFieldValue(detail.newValue)}</bdi>
                                                             </span>
                                                         )}
                                                     </td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            )}

                            {isNew && !hasDetails && (
<<<<<<< HEAD
                                <div className="mt-3 p-3 bg-[hsl(var(--gov-info)/.08)] border border-[hsl(var(--gov-info))] rounded-xl flex items-center gap-3">
                                    <Info className="w-4 h-4 text-[hsl(var(--gov-info))] opacity-100 shrink-0" aria-hidden="true" />
=======
                                <div className="mt-3 p-3 bg-[hsl(var(--gov-info)/.08)] border border-[hsl(var(--gov-info)/.2)] rounded-xl flex items-center gap-3">
                                    <Info className="w-4 h-4 text-[hsl(var(--gov-info))] opacity-50 shrink-0" aria-hidden="true" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    <p className="text-xs font-bold text-[hsl(var(--gov-info))]">
                                        بيانات الكيان الجديد: <span className="font-black">{change.newValue}</span>
                                    </p>
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="shrink-0">
                        {isPending ? (
                            <div className="flex items-center gap-2">
                                {isCurrentItemMutating ? (
                                    <div className="flex items-center gap-2 px-3 py-2 text-xs font-bold text-muted-foreground">
                                        <Loader2 className="w-4 h-4 animate-spin" />
                                        جاري الحفظ...
                                    </div>
                                ) : (
                                    <>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            className="min-h-[44px] px-3 text-xs font-black text-[hsl(var(--gov-success))] hover:bg-[hsl(var(--gov-success)/.08)] gap-1.5"
                                            onClick={onApprove}
                                            disabled={!canApprove || isMutating}
                                            aria-label={`اعتماد ${change.fieldName}`}
                                        >
                                            <ThumbsUp className="w-3.5 h-3.5" aria-hidden="true" />
                                            اعتماد
                                        </Button>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            className="min-h-[44px] px-3 text-xs font-black text-[hsl(var(--gov-danger))] hover:bg-[hsl(var(--gov-danger)/.08)] gap-1.5"
                                            onClick={onReject}
                                            disabled={!canReject || isMutating}
                                            aria-label={`رفض ${change.fieldName}`}
                                        >
                                            <ThumbsDown className="w-3.5 h-3.5" aria-hidden="true" />
                                            رفض
                                        </Button>
                                    </>
                                )}
                            </div>
                        ) : (
                            <RecordStateBadge state={isApproved ? "approved" : "rejected"} />
                        )}
                    </div>
                </div>
            </div>
        </Card>
    );
}

export function FileReviewDetailPage() {
    const { fileId: fileIdParam } = useParams<{ fileId: string }>();
    const fileId = Number(fileIdParam);
    const navigate = useNavigate();
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canReviewApprove = useHasPermission("File.Review.Approve");
    const canReviewReject = useHasPermission("File.Review.Reject");

    const [rejectNotes, setRejectNotes] = useState("");
    const sectionKey = `review_section_${fileId}`;
    const [_activeSection] = useState<string | null>(() => sessionStorage.getItem(sectionKey));
    const [rejectDialog, setRejectDialog] = useState<{ isOpen: boolean; itemId: number | null; fieldName: string }>({ isOpen: false, itemId: null, fieldName: "" });

    const { data: diff, isLoading, error } = useQuery({
        queryKey: ["review-diff", fileId],
        queryFn: () => retirementApi.getReviewDiff(fileId),
        enabled: !isNaN(fileId),
    });

    // استعادة موضع التمرير داخل ScrollArea
    useEffect(() => {
        if (isLoading) return;

        const scrollKey = `scroll_pos_review_${fileId}`;
        const savedSection = sessionStorage.getItem(`review_section_${fileId}`);
        const saved = sessionStorage.getItem(scrollKey);
        const findViewport = () => document.querySelector("[data-radix-scroll-area-viewport]") as HTMLElement | null;

        const tryRestore = (attempt = 0) => {
            const vp = findViewport();
            if (vp) {
                // استعادة التمرير أولاً
                if (saved) vp.scrollTop = parseInt(saved, 10);
                // ثم التنقل للقسم المحفوظ
                if (savedSection) {
                    const el = document.getElementById(`section-${savedSection}`);
                    if (el) el.scrollIntoView({ block: "start" });
                }
            } else if (attempt < 30) {
                requestAnimationFrame(() => tryRestore(attempt + 1));
            }
        };
        requestAnimationFrame(() => tryRestore());

        const handleBeforeUnload = () => {
            const vp = findViewport();
            if (vp) sessionStorage.setItem(scrollKey, String(vp.scrollTop));
        };
        window.addEventListener("beforeunload", handleBeforeUnload);

        return () => {
            window.removeEventListener("beforeunload", handleBeforeUnload);
            const vp = findViewport();
            if (vp) sessionStorage.setItem(scrollKey, String(vp.scrollTop));
        };
    }, [fileId, isLoading]);

    const invalidateReviewKeys = useMemo(() => [
        ["review-diff", fileId],
        ["retirement-file", fileId],
        ["entitled-persons", fileId],
        ["agents", fileId],
        ["guardians", fileId],
        ["salary-recipients", fileId],
        ["documents", fileId],
    ], [fileId]);

    const reviewMutation = useApiMutation(
        async ({ action, notes }: { action: "approve" | "reject"; notes?: string }) =>
            action === "approve"
                ? retirementApi.approveReview(fileId, notes)
                : retirementApi.rejectReview(fileId, notes ?? ""),
        {
            successMessage: (_, v) => v.action === "approve" ? "تم اعتماد الملف بالكامل" : "تم رفض المراجعة",
            invalidateKeys: [["pending-reviews"], ["review-diff", fileId]],
            onSuccess: () => { setTimeout(() => navigate("/review"), 1200); },
            onError: (error) => {
                toast.error("حدث خطأ", { description: error instanceof Error ? error.message : "فشل تنفيذ القرار" });
            }
        }
    );

    const approveItemMutation = useApiMutation(
        (itemId: number) => retirementApi.approveReviewItem(itemId),
        {
            successMessage: "تم الاعتماد",
            invalidateKeys: invalidateReviewKeys,
            onError: (error) => {
                toast.error("فشل الاعتماد", { description: error instanceof Error ? error.message : "حدث خطأ غير متوقع" });
            }
        }
    );

    const rejectItemMutation = useApiMutation(
        ({ itemId, reason }: { itemId: number; reason: string }) =>
            retirementApi.rejectReviewItem(itemId, reason),
        {
            successMessage: "تم الرفض",
            invalidateKeys: invalidateReviewKeys,
            onError: (error) => {
                toast.error("فشل الرفض", { description: error instanceof Error ? error.message : "حدث خطأ غير متوقع" });
            }
        }
    );

    const stats = useReviewStats(diff);

    const handleDecision = useCallback(async (action: "approve" | "reject") => {
        if (action === "reject" && !rejectNotes.trim()) {
            toast.error("يرجى كتابة سبب الرفض");
            return;
        }
        const ok = await confirm({
            title: action === "approve" ? "اعتماد كلي" : "رفض كلي",
            description: `تطبيق القرار على كافة البنود الـ (${stats.pending}) المتبقية؟`,
            variant: action === "reject" ? "danger" : "default",
        });
        if (ok) reviewMutation.mutate({ action, notes: rejectNotes });
    }, [rejectNotes, stats.pending, confirm, reviewMutation]);

    const sectionRefs = useRef<Record<string, HTMLDivElement | null>>({});

    const handleApproveItem = useCallback((itemId: number | undefined | null) => {
        if (itemId === undefined || itemId === null) return;
        approveItemMutation.mutate(itemId);
    }, [approveItemMutation]);

    const handleOpenRejectDialog = useCallback((itemId: number | undefined | null, fieldName: string) => {
        if (itemId === undefined || itemId === null) return;
        setRejectDialog({ isOpen: true, itemId, fieldName });
    }, []);

    const handleConfirmRejectItem = useCallback((reason: string) => {
        if (rejectDialog.itemId === null) return;
        rejectItemMutation.mutate(
            { itemId: rejectDialog.itemId, reason },
            { onSuccess: () => setRejectDialog({ isOpen: false, itemId: null, fieldName: "" }) },
        );
    }, [rejectDialog.itemId, rejectItemMutation]);

    const handleCloseRejectDialog = useCallback(() => {
        setRejectDialog({ isOpen: false, itemId: null, fieldName: "" });
    }, []);

    const showApproveAll = canReviewApprove && stats.pending > 0;
    const showRejectAll = canReviewReject && stats.pending > 0;
    const textareaId = useId();

    if (isNaN(fileId)) {
        return <div className="p-8"><AlertBanner variant="danger" message="معرّف غير صالح" /></div>;
    }
    if (isLoading) return <Loader variant="page" text="جاري تجهيز بيانات التدقيق..." />;
    if (error) {
        return <div className="p-6"><AlertBanner variant="danger" message="فشل تحميل البيانات" /></div>;
    }

    return (
        <PageLayout className="flex flex-col max-w-none space-y-0 h-[calc(100dvh-4rem)] overflow-hidden">
            {/* ── Top Info Bar ── */}
            <header className="bg-background border-b px-4 md:px-6 py-3 shrink-0 z-30">
                <div className="flex items-center justify-between gap-3 flex-wrap">
                    <div className="flex items-center gap-3">
                        <Button variant="ghost" size="icon" onClick={() => navigate("/review")} aria-label="العودة" className="rounded-xl">
                            <ArrowLeft className="w-5 h-5" aria-hidden="true" />
                        </Button>
                        <div className="flex items-center gap-2">
                            <div className="w-8 h-8 md:w-10 md:h-10 rounded-xl bg-primary/10 flex items-center justify-center">
                                <ListChecks className="w-4 h-4 md:w-5 md:h-5 text-primary" aria-hidden="true" />
                            </div>
                            <div>
                                <Heading level="h1" className="text-sm md:text-base font-black">مراجعة ملف: {diff?.fileNumber}</Heading>
                                <p className="text-[10px] md:text-xs text-muted-foreground font-bold">{diff?.retireeFullName}</p>
                            </div>
                        </div>
                    </div>

                    <div className="flex items-center gap-2">
                        {diff?.reviewRequestedByUserName && (
                            <Badge variant="outline" className="h-6 text-[9px] px-2 gap-1 rounded-lg hidden sm:flex">
                                <User className="w-2.5 h-2.5" aria-hidden="true" />
                                {diff.reviewRequestedByUserName}
                            </Badge>
                        )}
                        <Button variant="outline" size="sm" className="rounded-xl text-xs font-bold gap-1.5" onClick={() => navigate(`/files/${fileId}`)}>
                            <ExternalLink className="w-3.5 h-3.5" aria-hidden="true" />
                            عرض الملف
                        </Button>
                    </div>
                </div>
            </header>

            {/* ── Main Content ── */}
            <div className="flex-1 overflow-hidden">
                <ScrollArea className="h-full min-h-0">
                    <div className="p-4 md:p-6 space-y-5 max-w-5xl mx-auto pb-40">
                        {diff?.sections.length === 0 && (
                            <EmptyState title="لا يوجد تعديلات للمراجعة في هذا الملف" icon={Info} />
                        )}

                        {diff?.sections.map((section: ReviewDiffSection) => (
                            <div
                                key={section.sectionName}
                                id={`section-${section.sectionName}`}
                                ref={(el) => { sectionRefs.current[section.sectionName] = el; }}
                            >
                                <div className="flex items-center gap-2 mb-4">
                                    <div className="h-6 w-1 bg-primary rounded-full" />
                                    <Heading level="h2" className="text-sm font-black">
                                        {section.entityDisplayName
                                            ? `${section.sectionName} — ${section.entityDisplayName}`
                                            : section.sectionName}
                                    </Heading>
                                    <Badge variant="outline" className="h-5 px-1.5 text-[9px] font-black">
                                        {section.fieldChanges.length}
                                    </Badge>
                                </div>
                                <div className="space-y-3">
                                    {section.fieldChanges.map((change: FieldChange, index: number) => (
                                        <ReviewChangeCard
                                            key={`${section.sectionName}-${change.fieldName}-${change.itemId ?? index}`}
                                            change={change}
                                            onApprove={() => handleApproveItem(change.itemId)}
                                            onReject={() => handleOpenRejectDialog(change.itemId, change.fieldName)}
                                            canApprove={canReviewApprove}
                                            canReject={canReviewReject}
                                            isMutating={approveItemMutation.isPending || rejectItemMutation.isPending}
                                            mutationTargetId={approveItemMutation.isPending ? (approveItemMutation.variables as number) : rejectItemMutation.isPending ? (rejectItemMutation.variables as { itemId: number })?.itemId : null}
                                        />
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                </ScrollArea>

                {/* ── Bottom Action Bar ── */}
                <div className="absolute bottom-0 inset-x-0 p-4 md:p-6 bg-gradient-to-t from-background via-background/95 to-transparent pointer-events-none">
<<<<<<< HEAD
                    <Card className="max-w-5xl mx-auto rounded-2xl p-3 md:p-4 border border-primary bg-background/95 backdrop-blur-sm pointer-events-auto flex flex-col gap-2">
=======
                    <Card className="max-w-5xl mx-auto rounded-2xl p-3 md:p-4 border border-primary/10 bg-background/95 backdrop-blur-sm pointer-events-auto flex flex-col gap-2">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <div className="flex items-center justify-between gap-3 flex-wrap">
                            <div className="hidden sm:block">
                                <p className="text-xs font-black">القرار النهائي للملف</p>
                                <p className="text-[10px] text-muted-foreground font-bold">تطبيق القرار على ({stats.pending}) بند معلق</p>
                            </div>
                            <div className="flex items-center gap-2 flex-1 sm:flex-initial">
                                {showRejectAll && (
                                    <Button
                                        variant="destructive"
                                        className="flex-1 md:flex-none rounded-xl gap-2 font-black text-xs"
                                        onClick={() => handleDecision("reject")}
                                        loading={reviewMutation.isPending}
                                        disabled={!rejectNotes.trim()}
                                    >
                                        <XCircle className="w-4 h-4" aria-hidden="true" /> رفض الكل
                                    </Button>
                                )}
                                {showApproveAll && (
                                    <Button
                                        variant="success"
                                        className="flex-1 sm:flex-none rounded-xl gap-2 font-black text-xs"
                                        onClick={() => handleDecision("approve")}
                                        loading={reviewMutation.isPending}
                                    >
                                        <CheckCircle2 className="w-4 h-4" aria-hidden="true" /> اعتماد الكل
                                    </Button>
                                )}
                            </div>
                        </div>
                        {showRejectAll && (
                            <div>
                                <Textarea
                                    id={textareaId}
                                    className="min-h-[50px] text-xs"
                                    placeholder="اكتب مبررات الرفض (إلزامي في حال الرفض الكلي)..."
                                    value={rejectNotes}
                                    onChange={(e) => setRejectNotes(e.target.value)}
                                    aria-label="مبررات الرفض الكلي"
                                />
                            </div>
                        )}
                    </Card>
                </div>
            </div>

            {ConfirmDialogElement}
            <RejectItemDialog
                isOpen={rejectDialog.isOpen}
                onClose={handleCloseRejectDialog}
                onConfirm={handleConfirmRejectItem}
                fieldName={rejectDialog.fieldName}
                isLoading={rejectItemMutation.isPending}
            />
        </PageLayout>
    );
}