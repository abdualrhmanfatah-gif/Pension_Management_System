import { useQuery } from "@tanstack/react-query";
import { AlertCircle, AlertTriangle, CheckCircle2, ChevronDown, ChevronUp, Eye, FileText, History, Inbox, ShieldCheck, ThumbsDown, ThumbsUp, XCircle } from "lucide-react";
import { useState } from "react";
<<<<<<< HEAD
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { DocumentViewerDialog } from "@/components/domain/retirement/DocumentViewerDialog";
=======

import { getDocumentUrl } from "@/lib/utils";
import { retirementApi } from "@/api/retirement";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    AlertBanner,
    Badge,
    Button,
    Card,
    EmptyState,
    Heading,
    LoadingIndicator,
    Text,
    Textarea,
    useConfirm} from "@/components/ui";
import { REVIEW_ACTION } from "@/constants";
import { useApiMutation } from "@/hooks";
import { FILE_DETAIL_KEY } from "@/hooks/queries";
import { translateFieldName } from "@/lib/fieldTranslations";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { DocumentItem, FieldChange, FileReviewDiffSection, RetirementFileDetail, StatusTransition } from "@/types/retirement";

interface ReviewTabProps {
    fileId: number;
    file: RetirementFileDetail;
}

const PENDING = 0;
const APPROVED = 1;

export function ReviewTab({ fileId, file: fileSummary }: ReviewTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canReview = useHasPermission("File.Review.View");
    const canReviewApprove = useHasPermission("File.Review.Approve");
    const canReviewReject = useHasPermission("File.Review.Reject");
    const canViewHistory = useHasPermission("File.View.History");

    const [reviewAction, setReviewAction] = useState<typeof REVIEW_ACTION[keyof typeof REVIEW_ACTION] | null>(null);
    const [reviewNotes, setReviewNotes] = useState("");
    const [expandedChange, setExpandedChange] = useState<number | null>(null);
<<<<<<< HEAD
    const [viewingDoc, setViewingDoc] = useState<DocumentItem | null>(null);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const needsReview = fileSummary.reviewStatus === 1;
    const isReviewed = fileSummary.reviewStatus === 2 || fileSummary.reviewStatus === 3;
    const wasRejected = fileSummary.reviewStatus === 3;
    const showDiff = (needsReview || isReviewed) && (canReview || canViewHistory);

    const { data: reviewDiff, isLoading: loadingDiff } = useQuery({
        queryKey: ["review-diff", fileId],
        queryFn: () => retirementApi.getReviewDiff(fileId),
        enabled: showDiff,
    });

    const { data: documents = [], isLoading: loadingDocs, isError: docsError } = useQuery({
        queryKey: ["documents", fileId],
        queryFn: () => retirementApi.getDocuments(fileId)
    });

    const { data: transitions = [], isLoading: loadingTransitions, isError: transitionsError } = useQuery({
        queryKey: ["status-transitions", fileId],
        queryFn: () => retirementApi.getStatusTransitions(fileId).then(data => data as unknown as StatusTransition[])
    });

    const statusMutation = useApiMutation(
        async ({ action, notes }: { action: typeof REVIEW_ACTION[keyof typeof REVIEW_ACTION], notes: string }) => {
<<<<<<< HEAD
            if (action === REVIEW_ACTION.APPROVE) return retirementApi.approveReview(fileId, notes);
            return retirementApi.rejectReview(fileId, notes);
=======
            let newStatusId = 2;
            if (action === REVIEW_ACTION.RETURN) newStatusId = 6;
            if (action === REVIEW_ACTION.REJECT) newStatusId = 4;
            return retirementApi.updateFileStatus(fileId, newStatusId, notes);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        },
        {
            successMessage: (_, variables) =>
                variables.action === REVIEW_ACTION.APPROVE ? "تم اعتماد الملف بنجاح" :
                variables.action === REVIEW_ACTION.RETURN ? "تم إرجاع الملف للاستكمال" : "تم رفض الملف",
            invalidateKeys: [[FILE_DETAIL_KEY, fileId], ["status-transitions", fileId]],
            onSuccess: () => {
                setReviewAction(null);
                setReviewNotes("");
            }
        }
    );

    const reviewMutation = useApiMutation(
        async ({ action, notes }: { action: "approve" | "reject", notes?: string }) => {
            if (action === "approve") return retirementApi.approveReview(fileId, notes);
            return retirementApi.rejectReview(fileId, notes || "");
        },
        {
            successMessage: (_, variables) =>
                variables.action === "approve" ? "تم اعتماد المراجعة بنجاح" : "تم رفض المراجعة",
            invalidateKeys: [[FILE_DETAIL_KEY, fileId], ["review-diff", fileId]],
        }
    );

    const invalidateReviewKeys = [
        [FILE_DETAIL_KEY, fileId],
        ["review-diff", fileId],
        ["entitled-persons", fileId],
        ["agents", fileId],
        ["guardians", fileId],
        ["salary-recipients", fileId],
        ["documents", fileId]
    ];

    const approveItemMutation = useApiMutation(
        async (itemId: number) => retirementApi.approveReviewItem(itemId),
        { successMessage: "تم اعتماد التغيير", invalidateKeys: invalidateReviewKeys }
    );

    const rejectItemMutation = useApiMutation(
        async ({ itemId, reason }: { itemId: number, reason: string }) => retirementApi.rejectReviewItem(itemId, reason),
        { successMessage: "تم رفض التغيير", invalidateKeys: invalidateReviewKeys }
    );

    const handleStatusAction = async (action: typeof REVIEW_ACTION[keyof typeof REVIEW_ACTION]) => {
        if ((action === REVIEW_ACTION.RETURN || action === REVIEW_ACTION.REJECT) && !reviewNotes.trim()) return;

        const titles = { [REVIEW_ACTION.APPROVE]: "تأكيد اعتماد الملف", [REVIEW_ACTION.RETURN]: "تأكيد إرجاع الملف", [REVIEW_ACTION.REJECT]: "تأكيد رفض الملف" };
        const descriptions = {
            [REVIEW_ACTION.APPROVE]: "هل أنت متأكد من اعتماد هذا الملف؟",
            [REVIEW_ACTION.RETURN]: "سيتم إرجاع الملف لمدخل البيانات لإكمال النواقص",
            [REVIEW_ACTION.REJECT]: "سيتم رفض الملف نهائياً"
        };

        const ok = await confirm({ title: titles[action], description: descriptions[action], variant: action === REVIEW_ACTION.REJECT ? "danger" : "default" });
        if (ok) statusMutation.mutate({ action, notes: reviewNotes });
    };

    const handleReviewDecision = async (action: "approve" | "reject") => {
        if (action === "reject" && !reviewNotes.trim()) return;
        const title = action === "approve" ? "تأكيد اعتماد المراجعة" : "تأكيد رفض المراجعة";
        const description = action === "approve"
            ? "بعد الاعتماد سيعود الملف لحالته الطبيعية ويمكن تعديله لاحقاً."
            : "سيتم تسجيل رفض المراجعة مع ملاحظاتك.";
        const ok = await confirm({ title, description, variant: action === "reject" ? "danger" : "default" });
        if (ok) reviewMutation.mutate({ action, notes: reviewNotes });
    };

    const handleApproveItem = async (change: FieldChange) => {
        if (!change.itemId) return;
        const ok = await confirm({ title: "اعتماد التغيير", description: `اعتماد "${translateFieldName(change.fieldName)}"؟`, variant: "default" });
        if (ok) approveItemMutation.mutate(change.itemId);
    };

    const handleRejectItem = async (change: FieldChange) => {
        if (!change.itemId) return;
<<<<<<< HEAD
        if (!reviewNotes.trim()) {
            toast.error("اكتب سبب الرفض في ملاحظات المراجعة أولًا");
            return;
        }
        const ok = await confirm({ title: "رفض التغيير", description: `رفض "${translateFieldName(change.fieldName)}"؟`, variant: "danger" });
        if (ok && reviewNotes.trim()) rejectItemMutation.mutate({ itemId: change.itemId, reason: reviewNotes.trim() });
=======
        const ok = await confirm({ title: "رفض التغيير", description: `رفض "${translateFieldName(change.fieldName)}"؟`, variant: "danger" });
        if (ok) rejectItemMutation.mutate({ itemId: change.itemId, reason: "" });
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    };

    if (loadingDocs || loadingTransitions) return <LoadingIndicator message="جاري تحميل بيانات المراجعة..." />;
    if (docsError || transitionsError) return <div className="py-12 text-center text-destructive">تعذر تحميل بيانات المراجعة</div>;

    return (
        <div className="space-y-8 animate-in fade-in duration-500 text-start">
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                <div className="lg:col-span-2 space-y-6">
                    {showDiff && (
<<<<<<< HEAD
                        <Card className={`rounded-3xl p-8 relative overflow-hidden ${needsReview ? 'border-warning' : 'border-muted'}`}>
                            <div className={`absolute top-0 end-0 p-4 opacity-100`}>
                                {needsReview ? <AlertTriangle className="w-24 h-24 text-warning" /> : <CheckCircle2 className="w-24 h-24 text-success" />}
=======
                        <Card className={`rounded-3xl p-8 relative overflow-hidden ${needsReview ? 'border-amber-400/30' : 'border-muted'}`}>
                            <div className={`absolute top-0 end-0 p-4 opacity-5`}>
                                {needsReview ? <AlertTriangle className="w-24 h-24 text-amber-500" /> : <CheckCircle2 className="w-24 h-24 text-emerald-500" />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            </div>

                            {needsReview ? (
                                <div className="space-y-4 mb-6">
<<<<<<< HEAD
                                    <Heading level="h3" className="text-base md:text-lg lg:text-xl font-black flex items-center gap-2 text-warning">
=======
                                    <Heading level="h3" className="text-base md:text-lg lg:text-xl font-black flex items-center gap-2 text-amber-600">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        <AlertTriangle className="w-6 h-6" />
                                        يحتاج مراجعة — تم تعديل بيانات الملف
                                    </Heading>
                                    <Text variant="description" className="leading-relaxed">
                                        تم إجراء تعديلات على هذا الملف. يرجى مراجعة التغييرات أدناه واعتمادها أو رفضها.
                                    </Text>
                                    {fileSummary.reviewRequestedAt && (
                                        <Text variant="caption">تاريخ الطلب: {formatDate(fileSummary.reviewRequestedAt)}</Text>
                                    )}
                                </div>
                            ) : (
                                <div className="space-y-4 mb-6">
<<<<<<< HEAD
                                    <Heading level="h3" className="text-base md:text-lg lg:text-xl font-black flex items-center gap-2 text-success">
=======
                                    <Heading level="h3" className="text-base md:text-lg lg:text-xl font-black flex items-center gap-2 text-emerald-600">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        <CheckCircle2 className="w-6 h-6" />
                                        {wasRejected ? "تم رفض المراجعة" : "تم اعتماد المراجعة"}
                                    </Heading>
                                    {fileSummary.reviewCompletedAt && (
                                        <Text variant="caption">تاريخ المراجعة: {formatDate(fileSummary.reviewCompletedAt)}</Text>
                                    )}
                                    {fileSummary.reviewNotes && (
                                        <AlertBanner variant={wasRejected ? "danger" : "info"} className="mb-0" message={fileSummary.reviewNotes} />
                                    )}
                                </div>
                            )}

                            {loadingDiff ? (
                                <div className="py-6 text-center text-muted-foreground animate-pulse">جاري تحميل الفروقات...</div>
                            ) : reviewDiff && reviewDiff.sections.length > 0 ? (
<<<<<<< HEAD
                                <div className={`space-y-4 mb-6 max-h-[40vh] sm:max-h-[500px] overflow-y-auto ${needsReview ? '' : 'opacity-100'}`}>
=======
                                <div className={`space-y-4 mb-6 max-h-[40vh] sm:max-h-[500px] overflow-y-auto ${needsReview ? '' : 'opacity-75'}`}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    {reviewDiff.sections.map((section: FileReviewDiffSection) => (
                                        <div key={section.sectionName} className="border border-border rounded-2xl overflow-hidden">
                                            <div className="bg-muted/30 px-5 py-3 border-b">
                                                <h4 className="font-bold text-sm">{section.sectionName}</h4>
                                            </div>
<<<<<<< HEAD
                                            <div className="divide-y divide-border">
=======
                                            <div className="divide-y divide-border/50">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                {section.fieldChanges.length === 0 ? (
                                                    <div className="px-5 py-4 text-xs text-muted-foreground">لا توجد تغييرات في هذا القسم</div>
                                                ) : (
                                                    section.fieldChanges.map((change: FieldChange) => (
                                                        <EntityChangeCard
                                                            key={change.itemId || change.fieldName}
                                                            change={change}
                                                            needsReview={needsReview}
                                                            expandedChange={expandedChange}
                                                            onToggleExpand={(id) => setExpandedChange(expandedChange === id ? null : id)}
                                                            onApprove={() => handleApproveItem(change)}
                                                            onReject={() => handleRejectItem(change)}
                                                            canApprove={canReviewApprove}
                                                            canReject={canReviewReject}
                                                            isMutating={approveItemMutation.isPending || rejectItemMutation.isPending}
                                                        />
                                                    ))
                                                )}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <AlertBanner variant="info" className="mb-6" message="لا توجد فروقات متاحة للمقارنة." />
                            )}

                            {needsReview && (
<<<<<<< HEAD
                                <Textarea value={reviewNotes} onChange={(event) => setReviewNotes(event.target.value)}
                                    placeholder="ملاحظات المراجعة وسبب الرفض" aria-label="ملاحظات المراجعة وسبب الرفض" />
                            )}
                            {needsReview && (
                                <div className="flex flex-wrap gap-3 pt-4 border-t border-dashed">
                                    <Button
                                        size="lg"
                                        className="h-12 px-8 rounded-2xl gap-2 font-black bg-success hover:bg-success border-none text-white"
=======
                                <div className="flex flex-wrap gap-3 pt-4 border-t border-dashed">
                                    <Button
                                        size="lg"
                                        className="h-12 px-8 rounded-2xl gap-2 font-black bg-emerald-600 hover:bg-emerald-700 border-none text-white"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        onClick={() => handleReviewDecision("approve")}
                                        loading={reviewMutation.isPending}
                                        disabled={!canReviewApprove}
                                    >
                                        <CheckCircle2 className="w-5 h-5" /> اعتماد المراجعة
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="lg"
<<<<<<< HEAD
                                        className="h-12 px-8 rounded-2xl gap-2 font-black border-danger bg-danger/5 text-danger hover:bg-danger/10 transition-all"
=======
                                        className="h-12 px-8 rounded-2xl gap-2 font-black border-red-500/20 bg-red-500/5 text-red-700 hover:bg-red-100 transition-all"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        onClick={() => handleReviewDecision("reject")}
                                        loading={reviewMutation.isPending}
                                        disabled={!canReviewReject}
                                    >
                                        <XCircle className="w-5 h-5" /> رفض المراجعة
                                    </Button>
                                </div>
                            )}
                        </Card>
                    )}

                    <Card className="rounded-3xl p-8 relative overflow-hidden">
<<<<<<< HEAD
                        <div className="absolute top-0 end-0 p-4 opacity-100">
=======
                        <div className="absolute top-0 end-0 p-4 opacity-5">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <ShieldCheck className="w-24 h-24" />
                        </div>

                        <div className="space-y-4 mb-8">
                            <Heading level="h3" className="text-base md:text-lg lg:text-xl font-black flex items-center gap-2">
                                <ShieldCheck className="w-6 h-6 text-primary" />
                                معالجة طلب الاعتماد
                            </Heading>
                            <Text variant="description" className="leading-relaxed">
                                بصفتك مراجعاً للملفات، يرجى التحقق من اكتمال كافة الوثائق المرفقة وصحة البيانات المالية المدخلة قبل اتخاذ قرار الاعتماد.
                            </Text>
                        </div>

                        {fileSummary && ((fileSummary.missingDocumentsCount || 0) > 0 || (fileSummary.missingDataCount || 0) > 0) && (
                            <AlertBanner
                                variant="warning"
<<<<<<< HEAD
                                className="mb-8 items-start border-warning"
=======
                                className="mb-8 items-start border-amber-500/30"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                message={
                                    <div className="flex flex-col gap-2">
                                        <div>
                                            <strong className="text-sm">تنبيه قبل الاعتماد: مستندات وبيانات ناقصة</strong>
                                            <Text variant="body" className="mt-0.5">يوجد {fileSummary.missingDocumentsCount || 0} وثيقة ناقصة و {fileSummary.missingDataCount || 0} حقول بيانات غير مكتملة في الملف.</Text>
                                        </div>
                                    </div>
                                }
<<<<<<< HEAD
                                icon={<AlertTriangle className="w-5 h-5 mt-0.5 text-warning" />}
=======
                                icon={<AlertTriangle className="w-5 h-5 mt-0.5 text-amber-600" />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            />
                        )}

                        <div className="flex flex-wrap gap-3">
<<<<<<< HEAD
                            <Button size="lg" className="h-14 px-8 rounded-2xl gap-2 font-black bg-success hover:bg-success border-none text-white"
=======
                            <Button size="lg" className="h-14 px-8 rounded-2xl gap-2 font-black bg-emerald-600 hover:bg-emerald-700 border-none text-white"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                onClick={() => handleStatusAction(REVIEW_ACTION.APPROVE)}
                                loading={statusMutation.isPending && reviewAction === REVIEW_ACTION.APPROVE}>
                                <ShieldCheck className="w-5 h-5" /> اعتماد الملف نهائياً
                            </Button>
<<<<<<< HEAD
                            <Button variant="outline" size="lg" className="h-14 px-8 rounded-2xl gap-2 font-black border-warning bg-warning/5 text-warning hover:bg-warning/10 transition-all"
=======
                            <Button variant="outline" size="lg" className="h-14 px-8 rounded-2xl gap-2 font-black border-amber-500/20 bg-amber-500/5 text-amber-700 hover:bg-amber-100 transition-all"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                onClick={() => setReviewAction(REVIEW_ACTION.RETURN)}>
                                <History className="w-5 h-5" /> إرجاع للاستكمال
                            </Button>
                            <Button variant="ghost" size="lg" className="h-14 px-8 rounded-2xl gap-2 font-black text-destructive hover:bg-destructive/5 hover:text-destructive"
                                onClick={() => setReviewAction(REVIEW_ACTION.REJECT)}>
                                <AlertCircle className="w-5 h-5" /> رفض الملف
                            </Button>
                        </div>

                        {reviewAction && (
                            <div className="mt-8 pt-8 border-t border-dashed border-border animate-in slide-in-from-top-4 duration-300">
                                <div className="space-y-4">
                                    <div className="flex justify-between items-center flex-wrap">
                                        <h4 className="font-bold text-sm">ملاحظات {reviewAction === REVIEW_ACTION.RETURN ? "الإرجاع" : "الرفض"}</h4>
<<<<<<< HEAD
                                        <Badge variant="outline" className="text-[10px] sm:text-xs uppercase font-black tracking-widest px-2 py-0.5 rounded-lg border-primary text-primary">إلزامي</Badge>
=======
                                        <Badge variant="outline" className="text-[10px] sm:text-xs uppercase font-black tracking-widest px-2 py-0.5 rounded-lg border-primary/20 text-primary">إلزامي</Badge>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    </div>
                                    <Textarea
                                        className="min-h-[120px] rounded-2xl border-2 border-muted bg-muted/30 p-4"
                                        placeholder="يرجى كتابة أسباب الإرجاع أو الرفض بالتفصيل…"
                                        value={reviewNotes}
                                        onChange={(e) => setReviewNotes(e.target.value)}
                                        aria-label="ملاحظات الإرجاع أو الرفض"
                                    />
                                    <div className="flex justify-end gap-2 flex-wrap">
                                        <Button variant="ghost" onClick={() => {setReviewAction(null); setReviewNotes("");}} className="rounded-xl font-bold">إلغاء</Button>
<<<<<<< HEAD
                                        <Button className={`rounded-xl px-8 font-black text-white ${reviewAction === REVIEW_ACTION.REJECT ? 'bg-destructive hover:bg-destructive/90' : 'bg-warning hover:bg-warning'}`}
=======
                                        <Button className={`rounded-xl px-8 font-black text-white ${reviewAction === REVIEW_ACTION.REJECT ? 'bg-destructive hover:bg-destructive/90' : 'bg-amber-600 hover:bg-amber-700'}`}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            disabled={!reviewNotes.trim()}
                                            onClick={() => handleStatusAction(reviewAction)}
                                            loading={statusMutation.isPending}>
                                            تأكيد {reviewAction === REVIEW_ACTION.RETURN ? "الإرجاع" : "الرفض"}
                                        </Button>
                                    </div>
                                </div>
                            </div>
                        )}
                    </Card>

                    <Card className="rounded-3xl overflow-hidden">
                        <div className="p-5 border-b bg-muted/30">
                            <Heading level="h3" className="font-black text-sm flex items-center gap-2">
                                <History className="w-4 h-4 text-primary" />
                                سجل تاريخ الحالات
                            </Heading>
                        </div>
                        <div className="p-6 space-y-6">
                            {!transitions || !Array.isArray(transitions) || transitions.length === 0 ? (
                                <EmptyState icon={Inbox} title="لا يوجد سجل حركات لهذا الملف." />
                            ) : (
                                transitions.map((t, idx) => (
                                    <div key={t.transitionID || idx} className="relative flex items-start gap-4">
                                        <div className="z-10 w-2 h-2 rounded-full mt-2 bg-primary shrink-0" />
                                        {idx < transitions.length - 1 && <div className="absolute top-4 start-[3px] bottom-[-24px] w-0.5 bg-border/40" />}
                                        <div className="flex-1 text-start">
                                            <div className="flex items-center justify-between flex-wrap">
                                                <div className="flex items-center gap-2">
                                                    <Badge variant="outline" className="text-[10px] sm:text-xs">{t.oldStatusName || "بداية"}</Badge>
                                                    <span className="text-muted-foreground">→</span>
                                                    <Badge className="text-[10px] sm:text-xs">{t.newStatusName}</Badge>
                                                </div>
                                                <span className="text-[10px] sm:text-xs font-mono text-muted-foreground">{formatDate(t.changedAt)}</span>
                                            </div>
                                            {t.reason && <Text variant="caption" className="mt-1.5 bg-muted/30 p-2 rounded-lg">{t.reason}</Text>}
                                            <p className="text-[9px] mt-1 text-muted-foreground font-bold">بواسطة: {t.changedByUser || "النظام"}</p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </Card>
                </div>

                <div className="lg:col-span-1">
                    <Card className="rounded-3xl overflow-hidden h-full">
                        <div className="p-5 border-b bg-muted/30">
                            <Heading level="h3" className="font-black text-sm">قائمة التحقق من الوثائق</Heading>
                        </div>
<<<<<<< HEAD
                        <div className="p-2 divide-y divide-border max-h-[40vh] sm:max-h-[500px] overflow-y-auto">
=======
                        <div className="p-2 divide-y divide-border/50 max-h-[40vh] sm:max-h-[500px] overflow-y-auto">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            {!documents || !Array.isArray(documents) || documents.length === 0 ? (
                                <EmptyState icon={Inbox} title="لا توجد وثائق مرفوعة حالياً." />
                            ) : (
                                documents.map((doc: DocumentItem) => (
                                    <div key={doc.documentID} className="p-4 flex items-center justify-between hover:bg-muted/20 transition-colors group">
                                        <div className="flex items-center gap-3 text-start">
                                            <div className="w-8 h-8 rounded-lg bg-primary/5 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-white transition-all">
                                                <FileText className="w-4 h-4" />
                                            </div>
                                            <div className="text-start">
                                                <Text variant="caption" className="font-bold truncate max-w-[140px] sm:max-w-[200px]">{doc.documentTypeName}</Text>
                                                <p className="text-[9px] text-muted-foreground">{formatDate(doc.createdAt)}</p>
                                            </div>
                                        </div>
<<<<<<< HEAD
                                        <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0 rounded-lg" onClick={() => setViewingDoc(doc)}>
=======
                                        <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0 rounded-lg" onClick={() => window.open(getDocumentUrl("/uploads/" + doc.filePath) ?? "", '_blank')}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            <Eye className="w-3.5 h-3.5" />
                                        </Button>
                                    </div>
                                ))
                            )}
                        </div>
                    </Card>
                </div>
            </div>

            {ConfirmDialogElement}
<<<<<<< HEAD
            <DocumentViewerDialog
                open={viewingDoc !== null}
                onOpenChange={(open) => { if (!open) setViewingDoc(null); }}
                document={viewingDoc}
                fileId={fileId}
            />
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        </div>
    );
}

interface EntityChangeCardProps {
    change: FieldChange;
    needsReview: boolean;
    expandedChange: number | null;
    onToggleExpand: (id: number | null) => void;
    onApprove: () => void;
    onReject: () => void;
    canApprove: boolean;
    canReject: boolean;
    isMutating: boolean;
}

function EntityChangeCard({ change, needsReview, expandedChange, onToggleExpand, onApprove, onReject, canApprove, canReject, isMutating }: EntityChangeCardProps) {
    const itemId = change.itemId ?? 0;
    const isExpanded = expandedChange === itemId;
    const isPending = change.itemStatus === PENDING;
    const isApproved = change.itemStatus === APPROVED;
    const hasDetails = change.fieldDetails && change.fieldDetails.length > 0;

<<<<<<< HEAD
    const statusIcon = isApproved ? <CheckCircle2 className="w-4 h-4 text-success" /> : isPending ? <AlertTriangle className="w-4 h-4 text-warning" /> : <XCircle className="w-4 h-4 text-danger" />;
    const statusBadge = isApproved ? "معتمد" : isPending ? "بانتظار المراجعة" : "مرفوض";
    const statusBadgeClass = isApproved ? "bg-success/5 text-success border-success" : isPending ? "bg-warning/5 text-warning border-warning" : "bg-danger/5 text-danger border-danger";
=======
    const statusIcon = isApproved ? <CheckCircle2 className="w-4 h-4 text-emerald-500" /> : isPending ? <AlertTriangle className="w-4 h-4 text-amber-500" /> : <XCircle className="w-4 h-4 text-red-500" />;
    const statusBadge = isApproved ? "معتمد" : isPending ? "بانتظار المراجعة" : "مرفوض";
    const statusBadgeClass = isApproved ? "bg-emerald-500/5 text-emerald-700 border-emerald-500/20" : isPending ? "bg-amber-500/5 text-amber-700 border-amber-500/20" : "bg-red-500/5 text-red-700 border-red-500/20";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    return (
        <div className="relative">
            <div className="px-5 py-3 flex items-center justify-between gap-4 hover:bg-muted/5 transition-colors">
                <div className="flex items-center gap-3 min-w-0 flex-1" onClick={() => hasDetails && onToggleExpand(isExpanded ? null : itemId)}>
<<<<<<< HEAD
                    <div className={`w-8 h-8 rounded-xl flex items-center justify-center shrink-0 ${isApproved ? 'bg-success/10' : isPending ? 'bg-warning/10' : 'bg-danger/10'}`}>
=======
                    <div className={`w-8 h-8 rounded-xl flex items-center justify-center shrink-0 ${isApproved ? 'bg-emerald-100' : isPending ? 'bg-amber-100' : 'bg-red-100'}`}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        {statusIcon}
                    </div>
                    <div className="min-w-0">
                        <Text variant="body" className="font-bold truncate">{translateFieldName(change.fieldName)}</Text>
                        {change.newValue && <p className="text-[10px] sm:text-xs text-muted-foreground truncate">{change.oldValue !== "جديد" ? `${change.oldValue} ← ${change.newValue}` : change.newValue}</p>}
                    </div>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                    <Badge variant="outline" className={`text-[10px] sm:text-xs border ${statusBadgeClass}`}>{statusBadge}</Badge>
                    {hasDetails && (
                        <Button variant="ghost" size="sm" className="p-1.5 min-h-[44px] min-w-[44px]" onClick={(e) => { e.stopPropagation(); onToggleExpand(isExpanded ? null : itemId); }}>
                            {isExpanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
                        </Button>
                    )}
                </div>
            </div>

            {isExpanded && hasDetails && (
                <div className="px-5 pb-3 animate-in slide-in-from-top-1 duration-150">
<<<<<<< HEAD
                    <div className="bg-muted/20 rounded-xl p-3 space-y-2 border border-border">
                        {(change.fieldDetails || []).map((detail, i) => (
                            <div key={i} className="grid grid-cols-1 sm:grid-cols-[1fr_1.5fr_1.5fr] gap-3 text-xs">
                                <span className="font-bold text-muted-foreground">{translateFieldName(detail.fieldName)}</span>
                                <span className="bg-danger/5  text-danger  p-1.5 rounded-lg line-through">{detail.oldValue || "—"}</span>
                                <span className="bg-success/5  text-success  p-1.5 rounded-lg">{detail.newValue || "—"}</span>
=======
                    <div className="bg-muted/20 rounded-xl p-3 space-y-2 border border-border/50">
                        {(change.fieldDetails || []).map((detail, i) => (
                            <div key={i} className="grid grid-cols-1 sm:grid-cols-[1fr_1.5fr_1.5fr] gap-3 text-xs">
                                <span className="font-bold text-muted-foreground">{translateFieldName(detail.fieldName)}</span>
                                <span className="bg-red-500/5 dark:bg-red-950/30 text-red-700 dark:text-red-400 p-1.5 rounded-lg line-through">{detail.oldValue || "—"}</span>
                                <span className="bg-green-500/5 dark:bg-green-950/30 text-green-700 dark:text-green-400 p-1.5 rounded-lg">{detail.newValue || "—"}</span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {needsReview && isPending && (
                <div className="px-5 pb-3 flex gap-2">
                    <Button
                        variant="ghost"
                        size="sm"
<<<<<<< HEAD
                        className="gap-1 text-success hover:bg-success/10 min-h-[44px]"
=======
                        className="gap-1 text-emerald-700 hover:bg-emerald-100 min-h-[44px]"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        disabled={!canApprove || isMutating}
                        onClick={onApprove}
                    >
                        <ThumbsUp className="w-3.5 h-3.5" /> اعتماد
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
<<<<<<< HEAD
                        className="gap-1 text-danger hover:bg-danger/10 min-h-[44px]"
=======
                        className="gap-1 text-red-700 hover:bg-red-100 min-h-[44px]"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        disabled={!canReject || isMutating}
                        onClick={onReject}
                    >
                        <ThumbsDown className="w-3.5 h-3.5" /> رفض
                    </Button>
                </div>
            )}
        </div>
    );
}


