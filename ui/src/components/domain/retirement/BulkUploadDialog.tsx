import { useQuery } from "@tanstack/react-query";
import { CheckCircle2, CircleAlert, Crop as CropIcon, FileText, ListChecks, Loader2, RefreshCw, Trash2, Upload, XCircle } from "lucide-react";
import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import {
    Badge,
    BaseFormDialog,
    Button,
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    FileUploader,
    FormSection,
    ImageCropperDialog,
    NativeSelect,
    ScrollArea,
    Text,
    Textarea,
} from "@/components/ui";
import { BADGE_VARIANT } from "@/constants";
import type { AcceptedCapture } from "@/hooks/useCameraScanSession";
import { bulkUploadDraftKey, loadBulkUploadDraft, prepareBulkUploadDraft, saveBulkUploadDraft } from "@/lib/bulkUploadDraft";
import { QUALITY_FLAG_LABELS, type QualityFlag } from "@/lib/document-scanner/quality";
import { normalizeApiError } from "@/lib/errorUtils";
import { useAuthStore } from "@/stores/authStore";
import type { AgentSummary, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";

import { duplicateMismatchMessage, isDuplicateMatch, parseOwnerEntity, readDuplicatePayload } from "./bulkUploadDuplicate";
import { CameraScanDialog } from "./CameraScanDialog";

interface BulkFileItem {

    id: string;

    file: File;

    fileName: string;

    fileSize: number;

    preview?: string;

    documentTypeID: number;

    ownerEntity: string;

    notes: string;

    status: "pending" | "uploading" | "success" | "error";

    error?: string;

    /** شارة جودة من جلسة المسح (م4/م5) — تنبيه فقط */
    needsReview: boolean;

    /** تأكيد بشري إجباري قبل الرفع عند needsReview */
    reviewConfirmed: boolean;

    qualityFlags: QualityFlag[];

    documentId?: number;

}

interface BulkUploadDialogProps {

    open: boolean;

    onOpenChange: (open: boolean) => void;

    onSaved?: () => void;

    fileId: number;

    docTypes: { id: number; name: string }[];

}

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_EXTENSIONS = new Set([".pdf", ".jpg", ".jpeg", ".png"]);
const UPLOAD_CONCURRENCY = 3;

type UploadResults = {
    success: number;
    failed: number;
    finalizeError?: string;
};

function getFileExtension(fileName: string): string {
    const index = fileName.lastIndexOf(".");
    return index >= 0 ? fileName.slice(index).toLowerCase() : "";
}

function isImageFile(file: File): boolean {
    const extension = getFileExtension(file.name);
    return file.type.startsWith("image/") || [".jpg", ".jpeg", ".png"].includes(extension);
}

function generateId(): string {

    if (typeof crypto !== "undefined" && crypto.randomUUID) {

        try {

            return crypto.randomUUID();

        } catch {

            // fallback

        }

    }

    return `${Date.now()}_${Math.random().toString(36).slice(2, 9)}_${Math.random().toString(36).slice(2, 5)}`;

}

function formatFileSize(bytes: number): string {

    if (bytes < 1024) return bytes.toLocaleString() + " B";

    if (bytes < 1048576) return (bytes / 1024).toFixed(1) + " KB";

    return (bytes / 1048576).toFixed(1) + " MB";

}

/* ─────────────── Memoized Row ─────────────── */

interface BulkFileRowProps {

    item: BulkFileItem;

    uploading: boolean;
    ownersUnavailable: boolean;

    docTypeOptions: { value: string; label: string }[];

    entityOptions: { value: string; label: string }[];

    onUpdateField: (id: string, field: Partial<BulkFileItem>) => void;

    onRemove: (id: string) => void;

    onSetPreview: (item: BulkFileItem) => void;

    onSetCrop: (item: BulkFileItem) => void;

}

const BulkFileRow = memo(function BulkFileRow({
    item,
    uploading,
    ownersUnavailable,
    docTypeOptions,
    entityOptions,
    onUpdateField,
    onRemove,
    onSetPreview,
    onSetCrop,
}: BulkFileRowProps) {
    const canEdit = !uploading && (item.status === "pending" || item.status === "error");

    const status = {
        pending: { label: "بانتظار الرفع", className: "bg-amber-500" },
        uploading: { label: "جارٍ الرفع", className: "bg-blue-500 motion-safe:animate-pulse" },
        success: { label: "تم الرفع", className: "bg-emerald-500" },
        error: { label: "يحتاج مراجعة", className: "bg-red-500" },
    }[item.status];

    return (
        <div
            dir="rtl"
            data-file-id={item.id}
            className={`w-full max-w-full overflow-hidden rounded-lg border bg-card p-2 text-right shadow-sm transition-colors sm:rounded-xl sm:p-4 ${
                item.status === "error" ? "border-destructive" : "border-border"
            }`}
        >
            {/* رأس البطاقة: الصورة + معلومات الملف + الحالة */}
            <div className="flex w-full min-w-0 items-start gap-2 sm:gap-3">
                <div className="relative shrink-0">
                    {item.preview ? (
                        <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            className="h-11 w-11 shrink-0 overflow-hidden rounded-lg border bg-muted/20 p-0 hover:bg-muted/50 sm:h-16 sm:w-16 sm:rounded-xl"
                            onClick={() => onSetPreview(item)}
                            aria-label={`معاينة ${item.fileName}`}
                        >
                            <img src={item.preview} alt="" className="h-full w-full object-cover" />
                        </Button>
                    ) : (
                        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg border bg-primary/5 sm:h-16 sm:w-16 sm:rounded-xl">
                            <FileText className="h-5 w-5 text-primary sm:h-6 sm:w-6" />
                        </div>
                    )}
                    <span
                        role="status"
                        aria-atomic="true"
                        title={status.label}
                        className={`absolute -right-1 -top-1 h-3 w-3 rounded-full ring-2 ring-card sm:h-3.5 sm:w-3.5 ${status.className}`}
                    >
                        <span className="sr-only">{status.label}</span>
                    </span>
                </div>

                <div className="flex w-0 min-w-0 flex-1 items-start justify-between gap-2">
                    <div className="w-0 min-w-0 flex-1 overflow-hidden">
                        <p
                            dir="auto"
                            className="block max-w-full overflow-hidden text-ellipsis whitespace-nowrap text-right text-xs font-semibold leading-5 sm:text-base"
                            title={item.fileName}
                        >
                            {item.fileName}
                        </p>
                        <p
                            dir="ltr"
                            className="mt-0.5 text-right text-[10px] leading-4 text-muted-foreground sm:text-xs"
                        >
                            {formatFileSize(item.fileSize)}
                        </p>
                    </div>

                    {/* أزرار الإجراءات في الجهة المقابلة للصورة */}
                    <div className="flex shrink-0 flex-col items-end gap-1.5">
                        {canEdit && (
                            <div className="flex items-center gap-1">
                                {item.preview && (
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="icon"
                                        aria-label={`قص ${item.fileName}`}
                                        title="قص الصورة"
                                        className="!h-8 !w-8 sm:!h-9 sm:!w-9"
                                        onClick={() => onSetCrop(item)}
                                        disabled={uploading}
                                    >
                                        <CropIcon className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                    </Button>
                                )}

                                <Button
                                    type="button"
                                    variant="outline"
                                    size="icon"
                                    aria-label={`حذف ${item.fileName}`}
                                    title="حذف الملف"
                                    className="!h-8 !w-8 text-destructive hover:bg-destructive/10 hover:text-destructive sm:!h-9 sm:!w-9"
                                    onClick={() => onRemove(item.id)}
                                    disabled={uploading}
                                >
                                    <Trash2 className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                </Button>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            {/* الحقول تأخذ عرض البطاقة كاملاً على الهاتف بدل حشرها بجانب الصورة */}
            {canEdit && (
                <div className="mt-1.5 grid grid-cols-1 gap-1.5 sm:mt-2 sm:grid-cols-[minmax(0,0.85fr)_minmax(0,1.35fr)] sm:gap-2">
                    <div className="grid min-w-0 grid-cols-[4.75rem_minmax(0,1fr)] items-center gap-2 sm:grid-cols-[5rem_minmax(0,1fr)]">
                        <span className="block whitespace-nowrap text-[11px] font-medium leading-4 text-foreground sm:text-xs">
                            نوع الوثيقة <span className="text-destructive">*</span>
                        </span>
                        <NativeSelect
                            options={docTypeOptions}
                            value={String(item.documentTypeID || "")}
                            onChange={(val) =>
                                onUpdateField(item.id, {
                                    documentTypeID: Number(val),
                                    error: undefined,
                                })
                            }
                            placeholder="اختر النوع"
                            className="!h-8 !min-h-8 w-full !px-2 !py-0 text-right !text-[11px] sm:!h-10 sm:!min-h-10 sm:!px-3 sm:!text-sm"
                            disabled={uploading}
                        />
                    </div>

                    <div className="grid min-w-0 grid-cols-[4.75rem_minmax(0,1fr)] items-center gap-2 sm:grid-cols-[5rem_minmax(0,1fr)]">
                        <span className="block whitespace-nowrap text-[11px] font-medium leading-4 text-foreground sm:text-xs">
                            صاحب الوثيقة <span className="text-destructive">*</span>
                        </span>
                        <NativeSelect
                            options={entityOptions}
                            value={item.ownerEntity}
                            onChange={(val) => onUpdateField(item.id, { ownerEntity: val, error: undefined })}
                            placeholder="اختر الصاحب"
                            className="!h-8 !min-h-8 w-full !px-2 !py-0 text-right !text-[11px] sm:!h-10 sm:!min-h-10 sm:!px-3 sm:!text-sm"
                            disabled={uploading || ownersUnavailable}
                        />
                    </div>

                    <div className="hidden min-w-0 space-y-1 sm:col-span-2 sm:block">
                        <span className="block text-[11px] font-medium text-foreground sm:text-xs">ملاحظات</span>

                        <Textarea
                            value={item.notes}
                            onChange={(e) => onUpdateField(item.id, { notes: e.target.value })}
                            placeholder="أضف ملاحظة اختيارية..."
                            dir="rtl"
                            className="!min-h-9 w-full resize-none !px-2 !py-1.5 text-right !text-xs sm:!min-h-10 sm:!px-3 sm:!py-2 sm:!text-sm"
                            rows={1}
                            disabled={uploading}
                        />
                    </div>
                </div>
            )}

            {item.needsReview && !item.reviewConfirmed && (
                <div className="mt-2 rounded-md border border-amber-500 bg-amber-500/5 px-2 py-1.5 text-[11px] font-medium sm:mt-3 sm:rounded-lg sm:px-3 sm:py-2 sm:text-xs">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                        <span className="text-amber-700 dark:text-amber-400">
                            تحتاج مراجعة: {item.qualityFlags.length ? item.qualityFlags.map((flag) => QUALITY_FLAG_LABELS[flag]).join("؛ ") : "فحص الجودة تعذر — راجع الصورة"}
                        </span>
                        {canEdit && (
                            <Button
                                type="button"
                                variant="outline"
                                className="!h-8 !min-h-8 !px-2 !text-[11px] sm:!h-9 sm:!text-xs"
                                onClick={() => onUpdateField(item.id, { reviewConfirmed: true, error: undefined })}
                                disabled={uploading}
                            >
                                تأكيد بعد المراجعة
                            </Button>
                        )}
                    </div>
                </div>
            )}

            {item.needsReview && item.reviewConfirmed && (
                <div className="mt-2 px-1 text-[11px] font-medium text-emerald-700 dark:text-emerald-400 sm:text-xs">
                    تم تأكيد مراجعة الجودة
                </div>
            )}

            {item.error && (
                <div className="mt-2 flex items-start gap-1.5 rounded-md border border-destructive bg-destructive/5 px-2 py-1.5 text-[11px] font-medium text-destructive sm:mt-3 sm:gap-2 sm:rounded-lg sm:px-3 sm:py-2 sm:text-xs">
                    <CircleAlert className="mt-0.5 h-3.5 w-3.5 shrink-0 sm:h-4 sm:w-4" />
                    <span>{item.error}</span>
                </div>
            )}
        </div>
    );
});

/* ─────────────── Main Dialog ─────────────── */

export function BulkUploadDialog({

    open,

    onOpenChange,

    onSaved,

    fileId,

    docTypes,

}: BulkUploadDialogProps) {

    const userId = useAuthStore((state) => state.user?.id);
    const draftKey = userId ? bulkUploadDraftKey(userId, fileId) : null;
    const [draftReady, setDraftReady] = useState(false);
    const [draftMessage, setDraftMessage] = useState("");
    const [draftError, setDraftError] = useState(false);
    const [draftExpiresAt, setDraftExpiresAt] = useState<number | null>(null);
    const [online, setOnline] = useState(() => typeof navigator === "undefined" || navigator.onLine);
    const draftSessionRef = useRef<object | null>(null);
    const draftHydratedRef = useRef(false);
    const lastDraftStateRef = useRef<{ files: BulkFileItem[]; needsFinalization: boolean } | null>(null);

    const [files, setFiles] = useState<BulkFileItem[]>([]);

    const [uploading, setUploading] = useState(false);

    const [results, setResults] = useState<UploadResults | null>(null);
    const [needsFinalization, setNeedsFinalization] = useState(false);
    const finalizationPendingRef = useRef(false);

    const [previewItem, setPreviewItem] = useState<BulkFileItem | null>(null);

    const [cropItem, setCropItem] = useState<BulkFileItem | null>(null);

    const [scanOpen, setScanOpen] = useState(false);

    const filesRef = useRef<BulkFileItem[]>([]);
    const fileListRef = useRef<HTMLDivElement>(null);
    const lastAddedFileIdRef = useRef<string | null>(null);

    // وثائق نجحت عبر runs: id العنصر → documentID. التحديث النهائي يستخدم الكل، وإعادة المحاولة لا تعيد رفع الناجح.

    const uploadedIdsRef = useRef<Map<string, number>>(new Map());

    const abortRef = useRef<AbortController | null>(null);

    const isMountedRef = useRef(true);

    const isUploadingRef = useRef(false);

    const blobUrlsRef = useRef<Set<string>>(new Set());
    const autoCloseTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    const cancelAutoClose = useCallback(() => {
        if (autoCloseTimeoutRef.current) clearTimeout(autoCloseTimeoutRef.current);
        autoCloseTimeoutRef.current = null;
    }, []);

    const markFinalizationPending = useCallback((pending: boolean) => {
        finalizationPendingRef.current = pending;
        if (isMountedRef.current) setNeedsFinalization(pending);
    }, []);

    /* keep ref in sync with state so callbacks don't stale */

    useEffect(() => {

        filesRef.current = files;

    }, [files]);

    // أظهر المرفق الجديد حتى لا يبقى أسفل الجزء المرئي بعد إنهاء التصوير.
    useEffect(() => {
        const fileId = lastAddedFileIdRef.current;
        if (!fileId) return;

        const frame = requestAnimationFrame(() => {
            const row = fileListRef.current?.querySelector<HTMLElement>(`[data-file-id="${fileId}"]`);
            if (row) {
                row.scrollIntoView({ block: "start", inline: "nearest" });
                lastAddedFileIdRef.current = null;
            }
        });

        return () => cancelAnimationFrame(frame);
    }, [files]);

    /* fetch entities */

    const { data: fileEntities, isError: entitiesError, isPending: entitiesLoading, refetch: refetchEntities } = useQuery({

        queryKey: ["file-entities", fileId],

        queryFn: async () => {

            const [agents, guardians, entitled] = await Promise.all([

                retirementApi.getAgents(fileId),

                retirementApi.getGuardians(fileId),

                retirementApi.getEntitledPersons(fileId),

            ]);

            const entities: { value: string; label: string; }[] = [

                { value: "retiree", label: "المتقاعد (صاحب الملف)" },

            ];

            agents.forEach((a: AgentSummary) =>

                entities.push({ value: "agent_" + a.agentID, label: "الوكيل: " + a.fullName })

            );

            guardians.forEach((g: GuardianSummary) =>

                entities.push({ value: "guardian_" + g.guardianID, label: "الوصي: " + g.fullName })

            );

            entitled.forEach((e: EntitledPersonSummary) =>

                entities.push({ value: "entitled_" + e.entitledID, label: "المستفيد: " + e.fullName })

            );

            return entities;

        },

        enabled: open && !!fileId,

        staleTime: 5 * 60 * 1000,

    });

    /* blob cleanup */

    const revokeBlob = useCallback((url: string | undefined) => {

        if (url?.startsWith("blob:")) {

            URL.revokeObjectURL(url);

            blobUrlsRef.current.delete(url);

        }

    }, []);

    useEffect(() => {

        isMountedRef.current = true;

        return () => {

            isMountedRef.current = false;

            abortRef.current?.abort();
            if (autoCloseTimeoutRef.current) {
                clearTimeout(autoCloseTimeoutRef.current);
                autoCloseTimeoutRef.current = null;
            }

            Array.from(blobUrlsRef.current).forEach((url) => URL.revokeObjectURL(url));

            blobUrlsRef.current.clear();

        };

    }, []);

    useEffect(() => {

        if (!open) {

            if (autoCloseTimeoutRef.current) {
                clearTimeout(autoCloseTimeoutRef.current);
                autoCloseTimeoutRef.current = null;
            }
            abortRef.current?.abort();

            abortRef.current = null;

            isUploadingRef.current = false;

            if (isMountedRef.current) {

                setUploading(false);

                setResults(null);

                setPreviewItem(null);

                setCropItem(null);

            }

            Array.from(blobUrlsRef.current).forEach((url) => URL.revokeObjectURL(url));

            blobUrlsRef.current.clear();

            uploadedIdsRef.current.clear();
            finalizationPendingRef.current = false;
            setNeedsFinalization(false);
            filesRef.current = [];
            lastAddedFileIdRef.current = null;

            setFiles([]);
            setScanOpen(false);

        }

    }, [open]);

    useEffect(() => {
        const handleOnline = () => { setOnline(true); };
        const handleOffline = () => {
            setOnline(false);
            abortRef.current?.abort();
        };
        window.addEventListener("online", handleOnline);
        window.addEventListener("offline", handleOffline);
        return () => {
            window.removeEventListener("online", handleOnline);
            window.removeEventListener("offline", handleOffline);
        };
    }, []);

    useEffect(() => {
        const session = {};
        draftSessionRef.current = session;
        draftHydratedRef.current = false;
        lastDraftStateRef.current = null;
        setDraftReady(false);
        setDraftMessage("");
        setDraftError(false);
        setDraftExpiresAt(null);
        if (!open) return;
        abortRef.current?.abort();
        abortRef.current = null;
        isUploadingRef.current = false;
        setUploading(false);
        setResults(null);
        setPreviewItem(null);
        setCropItem(null);
        uploadedIdsRef.current.clear();
        markFinalizationPending(false);
        Array.from(blobUrlsRef.current).forEach((url) => URL.revokeObjectURL(url));
        blobUrlsRef.current.clear();
        filesRef.current = [];
        setFiles([]);
        const restore = async () => {
            try {
                if (!draftKey) throw new Error("تعذر تحديد المستخدم لحفظ الملفات محليًا");
                const draft = await loadBulkUploadDraft(draftKey);
                if (draftSessionRef.current !== session) return;
                if (draft) {
                    const restored: BulkFileItem[] = draft.files.map((item) => {
                        const file = new File([item.file], item.fileName, {
                            type: item.file.type, lastModified: item.file.lastModified,
                        });
                        const preview = isImageFile(file) ? URL.createObjectURL(file) : undefined;
                        if (preview) blobUrlsRef.current.add(preview);
                        return { ...item, file, preview, status: item.status === "uploading" ? "pending" : item.status };
                    });
                    uploadedIdsRef.current = new Map(restored.filter((item) => item.documentId)
                        .map((item) => [item.id, item.documentId!]));
                    const finalize = draft.needsFinalization || uploadedIdsRef.current.size > 0;
                    filesRef.current = restored;
                    setFiles(restored);
                    markFinalizationPending(finalize);
                    lastDraftStateRef.current = { files: restored, needsFinalization: finalize };
                    setDraftMessage("استُعيدت الملفات المحفوظة. يمكنك متابعة الرفع عند عودة الاتصال.");
                    setDraftExpiresAt(draft.expiresAt);
                }
            } catch {
                if (draftSessionRef.current !== session) return;
                setDraftError(true);
                setDraftMessage("تعذر الحفظ المحلي. أبقِ هذه النافذة مفتوحة حتى يكتمل الرفع؛ تحديث الصفحة قد يفقد الملفات.");
            } finally {
                if (draftSessionRef.current === session) {
                    draftHydratedRef.current = true;
                    setDraftReady(true);
                }
            }
        };
        void restore();
        return () => { if (draftSessionRef.current === session) draftSessionRef.current = null; };
    }, [open, draftKey, markFinalizationPending]);

    useEffect(() => {
        if (!open || !draftReady || !draftHydratedRef.current || !draftKey) return;
        const previous = lastDraftStateRef.current;
        if (previous?.files === files && previous.needsFinalization === needsFinalization) return;
        const snapshot = { files, needsFinalization };
        lastDraftStateRef.current = snapshot;
        const session = draftSessionRef.current;
        const draft = prepareBulkUploadDraft(draftKey, files.map((item) => ({
            ...item, documentId: uploadedIdsRef.current.get(item.id),
        })), needsFinalization);
        if (draft) setDraftMessage("جارٍ حفظ الملفات على هذا الجهاز...");
        void saveBulkUploadDraft(draft, draftKey).then(() => {
            if (draftSessionRef.current !== session || lastDraftStateRef.current !== snapshot) return;
            setDraftError(false);
            setDraftExpiresAt(draft?.expiresAt ?? null);
            setDraftMessage(draft ? "حُفظت الملفات على هذا الجهاز لمدة ساعة من آخر تعديل، حتى عند إغلاق النافذة أو تحديث الصفحة." : "");
        }).catch(() => {
            if (draftSessionRef.current !== session || lastDraftStateRef.current !== snapshot) return;
            setDraftError(true);
            setDraftExpiresAt(null);
            setDraftMessage("تعذر الحفظ المحلي. أبقِ هذه النافذة مفتوحة حتى يكتمل الرفع؛ تحديث الصفحة قد يفقد الملفات.");
        });
    }, [open, draftReady, draftKey, files, needsFinalization]);

    useEffect(() => {
        if (!open || !draftExpiresAt) return;
        const timeout = setTimeout(() => {
            setDraftExpiresAt(null);
            setDraftMessage("انتهت ساعة الاحتفاظ بالنسخة المحلية. الملفات ما زالت في هذه النافذة؛ أبقها مفتوحة أو عدّل البيانات لحفظها مجددًا.");
        }, Math.max(0, draftExpiresAt - Date.now()));
        return () => clearTimeout(timeout);
    }, [open, draftExpiresAt]);

    /* add files (supports single or multiple) */

    const addFileToList = useCallback(

        (input: File | File[] | null) => {

            if (!input) return false;

            const arr = Array.isArray(input) ? input : [input];
            const addedFiles: BulkFileItem[] = [];

            arr.forEach((file) => {

                if (!file) return;

                if (file.size > MAX_FILE_SIZE_BYTES) {

                    toast.error(`حجم الملف "${file.name}" يتجاوز الحد الأقصى المسموح به (10 ميجابايت)`);

                    return;

                }

                const ext = getFileExtension(file.name);

                if (!ALLOWED_EXTENSIONS.has(ext)) {

                    toast.error(`صيغة الملف "${file.name}" غير مدعومة. يرجى رفع ملفات PDF, JPG, JPEG أو PNG فقط`);

                    return;

                }

                // الاسم والحجم لا يثبتان التكرار؛ الخادم يطابق بصمة المحتوى ونطاق المالك.
                const exists = [...filesRef.current, ...addedFiles].some((f) => f.file === file);

                if (exists) {
                    toast.warning(`الملف "${file.name}" موجود بالفعل في القائمة`);
                    return;
                }

                const preview = isImageFile(file) ? URL.createObjectURL(file) : undefined;
                if (preview) blobUrlsRef.current.add(preview);

                addedFiles.push({
                    id: generateId(),
                    file,
                    fileName: file.name,
                    fileSize: file.size,
                    preview,
                    documentTypeID: 0,
                    // المتقاعد صاحب الملف هو الاختيار المبدئي، ويمكن تغييره قبل الرفع.
                    ownerEntity: "retiree",
                    notes: "",
                    status: "pending",
                    // ملفات غير جلسة المسح: بلا شارة جودة
                    needsReview: false,
                    reviewConfirmed: true,
                    qualityFlags: [],
                });

            });

            if (!addedFiles.length) return false;

            cancelAutoClose();
            filesRef.current = [...filesRef.current, ...addedFiles];
            lastAddedFileIdRef.current = addedFiles[addedFiles.length - 1].id;
            setFiles((prev) => [...prev, ...addedFiles]);
            return true;

        },

        [cancelAutoClose]

    );

    /* إضافة لقطة من جلسة المسح (م5): شارة جودة تنبيهية + قص يدوي عند غياب المضلع */
    const addScanCapture = useCallback(
        (capture: AcceptedCapture) => {
            const file = capture.file;
            if (file.size > MAX_FILE_SIZE_BYTES) {
                toast.error(`حجم الصورة "${file.name}" يتجاوز الحد الأقصى المسموح به (10 ميجابايت)`);
                return;
            }
            const needsReview = !capture.quality || capture.quality.needsReview;
            cancelAutoClose();
            const preview = URL.createObjectURL(file);
            blobUrlsRef.current.add(preview);
            const item: BulkFileItem = {
                id: generateId(),
                file,
                fileName: file.name,
                fileSize: file.size,
                preview,
                documentTypeID: 0,
                ownerEntity: "retiree",
                notes: "",
                status: "pending",
                needsReview,
                reviewConfirmed: !needsReview,
                qualityFlags: capture.quality?.flags ?? [],
            };
            filesRef.current = [...filesRef.current, item];
            lastAddedFileIdRef.current = item.id;
            setFiles((prev) => [...prev, item]);
            // بلا مضلع: الزوايا تحدد يدويًا في CornerEditor فورًا
            if (!capture.quad) {
                setCropItem(item);
            }
        },
        [cancelAutoClose]
    );

    const removeFile = useCallback(

        (id: string) => {

            setFiles((prev) => {

                const removed = prev.find((f) => f.id === id);

                if (removed?.preview) revokeBlob(removed.preview);

                return prev.filter((f) => f.id !== id);

            });

        },

        [revokeBlob]

    );

    const updateFileField = useCallback(

        (id: string, field: Partial<BulkFileItem>) => {

            setFiles((prev) =>

                prev.map((f) => {

                    if (f.id === id) {

                        if (field.preview && f.preview !== field.preview) revokeBlob(f.preview);

                        return { ...f, ...field };

                    }

                    return f;

                })

            );

        },

        [revokeBlob]

    );

    /* upload with concurrency limit */
    const uploadAll = useCallback(async () => {
        if (isUploadingRef.current) return;
        if (!draftReady || !online) return;

        const currentFiles = filesRef.current;
        const pending = currentFiles.filter((f) => f.status === "pending" || f.status === "error");
        const hadFinalizeError = Boolean(results?.finalizeError);
        const retryFinalizeOnly = pending.length === 0 && (finalizationPendingRef.current || hadFinalizeError);

        if (!pending.length && !retryFinalizeOnly) return;
        if (pending.length && (entitiesLoading || entitiesError)) {
            toast.error(entitiesError ? "تعذر تحميل أصحاب الوثائق. أعد المحاولة قبل الرفع" : "انتظر اكتمال تحميل أصحاب الوثائق قبل الرفع");
            return;
        }

        cancelAutoClose();
        isUploadingRef.current = true;
        const controller = new AbortController();
        abortRef.current = controller;

        setUploading(true);
        setResults(null);

        const isCurrentRun = () => isMountedRef.current && abortRef.current === controller;
        const finishUploading = () => {
            if (!isCurrentRun()) return;
            setUploading(false);
            abortRef.current = null;
            isUploadingRef.current = false;
        };

        const restoreCancelledItems = () => {
            if (isCurrentRun()) {
                setFiles((prev) =>
                    prev.map((f) =>
                        f.status === "uploading"
                            ? { ...f, status: "pending" as const, error: undefined }
                            : f
                    )
                );
            }
            finishUploading();
        };

        if (pending.length) {
            setFiles((prev) =>
                prev.map((f) =>
                    f.status === "pending" || f.status === "error"
                        ? { ...f, status: "uploading" as const, error: undefined }
                        : f
                )
            );
        }

        const outcomes: { success: boolean; id: string; documentId?: number; }[] = [];
        const executing = new Set<Promise<void>>();

        async function process(item: BulkFileItem) {
            if (controller.signal.aborted || !isCurrentRun()) return;

            if (!item.documentTypeID) {
                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) =>
                            f.id === item.id
                                ? { ...f, status: "error" as const, error: "نوع الوثيقة غير محدد" }
                                : f
                        )
                    );
                }
                outcomes.push({ success: false, id: item.id });
                return;
            }

            // لا رفع قبل تحديد صاحب لكل وثيقة.
            if (!item.ownerEntity) {
                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) =>
                            f.id === item.id
                                ? { ...f, status: "error" as const, error: "صاحب الوثيقة غير محدد" }
                                : f
                        )
                    );
                }
                outcomes.push({ success: false, id: item.id });
                return;
            }

            // Q2/R2: الوثيقة ذات الشارة لا ترفع قبل تأكيد بشري
            if (item.needsReview && !item.reviewConfirmed) {
                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) =>
                            f.id === item.id
                                ? { ...f, status: "error" as const, error: "الوثيقة تحتاج مراجعة جودة وتأكيدًا قبل الرفع" }
                                : f
                        )
                    );
                }
                outcomes.push({ success: false, id: item.id });
                return;
            }

            try {
                const fd = new FormData();
                fd.append("File", item.file);
                fd.append("RetirementFileID", String(fileId));
                fd.append("DocumentTypeID", String(item.documentTypeID));
                if (item.notes.trim()) fd.append("Notes", item.notes.trim());

                if (item.ownerEntity && item.ownerEntity !== "retiree") {
                    const [type, id] = item.ownerEntity.split("_");
                    if (type === "agent" && id) fd.append("AgentID", id);
                    if (type === "guardian" && id) fd.append("GuardianID", id);
                    if (type === "entitled" && id) fd.append("EntitledPersonID", id);
                }

                const uploaded = await documentsApi.upload(fd, controller.signal, {
                    deferPostProcessing: true,
                });

                if (controller.signal.aborted || !isCurrentRun()) return;

                if (!Number.isInteger(uploaded?.documentID) || uploaded.documentID <= 0) {
                    throw new Error("لم يرجع الخادم معرفًا صالحًا للوثيقة. أعد محاولة الرفع");
                }
                uploadedIdsRef.current.set(item.id, uploaded.documentID);
                markFinalizationPending(true);
                outcomes.push({ success: true, id: item.id, documentId: uploaded.documentID });

                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) =>
                            f.id === item.id ? { ...f, status: "success" as const, error: undefined } : f
                        )
                    );
                }
            } catch (err) {
                if (controller.signal.aborted || !isCurrentRun()) return;

                const axiosErr = err as {
                    response?: {
                        status?: number;
                        data?: { message?: string; data?: unknown; };
                    };
                };

                if (axiosErr.response?.status === 409) {
                    // R3: لا قبول 409 كنجاح إلا بمطابقة النطاق (ملف + نوع + مالك) مع الطلب
                    const dup = readDuplicatePayload(axiosErr.response.data?.data);
                    if (dup && isDuplicateMatch(
                        { fileId, documentTypeId: item.documentTypeID, owner: parseOwnerEntity(item.ownerEntity) },
                        dup
                    )) {
                        const matchedId = dup.documentId as number;
                        uploadedIdsRef.current.set(item.id, matchedId);
                        markFinalizationPending(true);
                        outcomes.push({ success: true, id: item.id, documentId: matchedId });

                        if (isMountedRef.current) {
                            setFiles((prev) =>
                                prev.map((f) =>
                                    f.id === item.id
                                        ? { ...f, status: "success" as const, error: undefined }
                                        : f
                                )
                            );
                        }
                        return;
                    }

                    const dupMessage = dup
                        ? duplicateMismatchMessage(dup)
                        : (axiosErr.response.data?.message ?? "الوثيقة مكررة ولا تخص هذا المالك");
                    if (isMountedRef.current) {
                        setFiles((prev) =>
                            prev.map((f) =>
                                f.id === item.id
                                    ? { ...f, status: "error" as const, error: dupMessage }
                                    : f
                            )
                        );
                    }
                    outcomes.push({ success: false, id: item.id });
                    return;
                }

                const { message } = normalizeApiError(err);
                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) =>
                            f.id === item.id
                                ? { ...f, status: "error" as const, error: message }
                                : f
                        )
                    );
                }
                outcomes.push({ success: false, id: item.id });
            }
        }

        for (const item of pending) {
            if (controller.signal.aborted) break;

            const task = process(item);
            const tracked = task.finally(() => executing.delete(tracked));
            executing.add(tracked);

            if (executing.size >= UPLOAD_CONCURRENCY) {
                await Promise.race(executing);
            }
        }

        await Promise.all(executing);

        if (controller.signal.aborted || !isCurrentRun()) {
            restoreCancelledItems();
            return;
        }

        const runSuccessCount = outcomes.filter((outcome) => outcome.success).length;
        const runFailedCount = outcomes.filter((outcome) => !outcome.success).length;
        const previousSuccessCount = currentFiles.filter((f) => f.status === "success").length;
        const totalSuccessCount = previousSuccessCount + runSuccessCount;
        const totalFailedCount = runFailedCount;

        const shouldFinalize = finalizationPendingRef.current || retryFinalizeOnly || hadFinalizeError;

        if (!shouldFinalize) {
            if (runFailedCount) {
                toast.error(`فشل رفع ${runFailedCount} وثيقة من أصل ${pending.length}`);
            }
            if (isMountedRef.current) {
                setResults({ success: previousSuccessCount, failed: totalFailedCount });
            }
            finishUploading();
            return;
        }

        try {
            const documentIds = Array.from(new Set(uploadedIdsRef.current.values()));
            const finalized = await retirementApi.refreshAfterUpload(fileId, documentIds);
            if (!isCurrentRun()) return;
            if (!finalized.isSuccess || !finalized.data) {
                throw new Error(finalized.message || "تعذر إكمال التحديث النهائي للوثائق");
            }
            const skipped = new Set(finalized.data.skippedDocumentIds);
            if (skipped.size) {
                const skippedItems = new Set<string>();
                for (const [itemId, documentId] of uploadedIdsRef.current) {
                    if (skipped.has(documentId)) {
                        skippedItems.add(itemId);
                        uploadedIdsRef.current.delete(itemId);
                    }
                }
                const message = "لم يعثر الخادم على بعض الوثائق أثناء التحديث النهائي. أعد رفع الوثائق الموضحة في القائمة";
                setFiles((prev) => prev.map((item) => skippedItems.has(item.id)
                    ? { ...item, status: "error" as const, error: message }
                    : item));
                setResults({ success: totalSuccessCount - skippedItems.size, failed: totalFailedCount + skippedItems.size, finalizeError: message });
                toast.error(message);
                finishUploading();
                return;
            }
        } catch (err) {
            if (controller.signal.aborted || !isCurrentRun()) {
                restoreCancelledItems();
                return;
            }

            const { message } = normalizeApiError(err);
            toast.error(`تم رفع ${totalSuccessCount} وثيقة، لكن فشل التحديث النهائي: ${message}`);
            if (isMountedRef.current) {
                setResults({
                    success: totalSuccessCount,
                    failed: totalFailedCount,
                    finalizeError: message,
                });
            }
            finishUploading();
            return;
        }

        if (controller.signal.aborted || !isCurrentRun()) {
            restoreCancelledItems();
            return;
        }

        markFinalizationPending(false);
        if (runFailedCount) {
            toast.error(`فشل رفع ${runFailedCount} وثيقة من أصل ${pending.length}`);
        }

        if (runSuccessCount) {
            toast.success(`تم رفع ${runSuccessCount} وثيقة بنجاح`);
        } else if (hadFinalizeError || retryFinalizeOnly) {
            toast.success("تم التحديث النهائي بنجاح");
        }

        if (isMountedRef.current) {
            setResults({ success: totalSuccessCount, failed: totalFailedCount });
            if (totalSuccessCount > 0) onSaved?.();

            if (totalSuccessCount > 0 && totalFailedCount === 0) {
                if (autoCloseTimeoutRef.current) clearTimeout(autoCloseTimeoutRef.current);
                autoCloseTimeoutRef.current = setTimeout(() => {
                    autoCloseTimeoutRef.current = null;
                    if (isMountedRef.current && !isUploadingRef.current && !finalizationPendingRef.current &&
                        filesRef.current.length > 0 && filesRef.current.every((item) => item.status === "success")) {
                        onOpenChange(false);
                    }
                }, 1400);
            }
        }

        finishUploading();
    }, [cancelAutoClose, draftReady, entitiesError, entitiesLoading, fileId, markFinalizationPending, online, onSaved, onOpenChange, results?.finalizeError]);

    /* derived stats */
    const {
        completedCount,
        retryCount,
        progress,
        successfulFilesCount,
        errorFilesCount,
    } = useMemo(() => {
        const successful = files.filter((f) => f.status === "success").length;
        const failed = files.filter((f) => f.status === "error").length;
        const pending = files.filter((f) => f.status === "pending").length;
        const completed = successful + failed;

        return {
            completedCount: completed,
            retryCount: pending + failed,
            progress: files.length ? (completed / files.length) * 100 : 0,
            successfulFilesCount: successful,
            errorFilesCount: failed,
        };
    }, [files]);

    const docTypeOptions = useMemo(

        () => docTypes.map((t) => ({ value: String(t.id), label: t.name })),

        [docTypes]

    );

    const entityOptions = useMemo(() => {

        if (fileEntities?.length) return fileEntities;

        return [{ value: "retiree", label: "المتقاعد (صاحب الملف)" }];

    }, [fileEntities]);

    const hasInvalidFiles = useMemo(

        () => files.some((f) =>
            (f.status === "pending" || f.status === "error") &&
            (!f.documentTypeID || !f.ownerEntity || (f.needsReview && !f.reviewConfirmed))
        ),

        [files]

    );

    const cancelUpload = useCallback(() => {
        abortRef.current?.abort();
    }, []);

    const handleDialogOpenChange = useCallback(
        (nextOpen: boolean) => {
            if (nextOpen || !isUploadingRef.current) onOpenChange(nextOpen);
        },
        [onOpenChange]
    );

    return (

        <BaseFormDialog

            open={open}

            onOpenChange={handleDialogOpenChange}

            title="رفع جماعي للوثائق"

            size="full"

            isLoading={uploading}

            isDirty={retryCount > 0 || needsFinalization || Boolean(results?.finalizeError)}
            dirtyWarningMessage={needsFinalization || results?.finalizeError
                ? "لم يكتمل التحديث النهائي بعد. هل تريد الإغلاق رغم ذلك؟"
                : "لديك ملفات لم تُرفع بعد. هل تريد الإغلاق؟"}

            onSubmit={uploadAll}

            enableKeyboardShortcuts={false}

            footer={
                <div className="flex w-full flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <div className="min-h-5 text-xs">
                        {hasInvalidFiles && !uploading && (
                            <span className="inline-flex items-center gap-1.5 font-medium text-destructive">
                                <CircleAlert className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                اختر نوع الوثيقة وصاحبها لجميع الملفات قبل الرفع. الوثائق ذات شارة الجودة تحتاج تأكيدًا.
                            </span>
                        )}
                    </div>

                    <div className="flex w-full flex-col-reverse gap-2 sm:w-auto sm:flex-row">
                        {uploading && (
                            <Button
                                type="button"
                                variant="outline"
                                onClick={cancelUpload}
                                className="min-h-[44px] w-full min-w-[130px] sm:w-auto"
                            >
                                <XCircle className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                إلغاء الرفع
                            </Button>
                        )}

                        {files.length > 0 && (
                            <Button
                                type="submit"
                                disabled={!draftReady || !online || uploading || (retryCount === 0 && !needsFinalization && !results?.finalizeError) || hasInvalidFiles ||
                                    (retryCount > 0 && (entitiesLoading || entitiesError))}
                                className="min-h-[44px] w-full min-w-[155px] sm:w-auto"
                            >
                                {uploading ? (
                                    <>
                                        <Loader2 className="h-4 w-4 animate-spin" />
                                        جارٍ الرفع...
                                    </>
                                ) : (needsFinalization || results?.finalizeError) && retryCount === 0 ? (
                                    <>
                                        <RefreshCw className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                        إعادة التحديث النهائي
                                    </>
                                ) : (
                                    <>
                                        <Upload className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                        رفع الملفات ({retryCount})
                                    </>
                                )}
                            </Button>
                        )}
                    </div>
                </div>
            }
        >

            <div dir="rtl" className="flex min-h-0 flex-1 flex-col gap-2.5 sm:gap-4">
                {(!draftReady || draftMessage || !online) && (
                    <div role={draftError ? "alert" : "status"} aria-live="polite"
                        className={`shrink-0 rounded-lg border p-3 text-sm ${draftError ? "border-destructive bg-destructive/5 text-destructive" : "border-border bg-muted/20 text-foreground"}`}>
                        {!draftReady ? "جارٍ استعادة الملفات المحفوظة..." : draftMessage}
                        {!online && <p className="mt-1">الاتصال منقطع. عند عودته اضغط «رفع الملفات» أو «إعادة التحديث النهائي» للمتابعة.</p>}
                    </div>
                )}
                {entitiesError && (
                    <div role="alert" className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-destructive bg-destructive/5 p-3 text-sm text-destructive">
                        <span>تعذر تحميل أصحاب الوثائق. أعد المحاولة قبل اختيار صاحب الوثيقة ورفع الملفات.</span>
                        <Button type="button" variant="outline" onClick={() => void refetchEntities()} disabled={uploading}>
                            إعادة تحميل أصحاب الوثائق
                        </Button>
                    </div>
                )}

                {files.length > 0 && (
                    <div className="grid shrink-0 grid-cols-3 gap-1.5 sm:gap-2">
                        <div className="rounded-lg border bg-muted/20 px-1.5 py-1.5 text-center sm:rounded-xl sm:px-3 sm:py-2.5">
                            <div className="text-sm font-bold leading-none sm:text-lg">{files.length}</div>
                            <div className="mt-0.5 text-[9px] leading-tight text-muted-foreground sm:mt-1 sm:text-xs">إجمالي الملفات</div>
                        </div>
                        <div className="rounded-lg border bg-muted/20 px-1.5 py-1.5 text-center sm:rounded-xl sm:px-3 sm:py-2.5">
                            <div className="text-sm font-bold leading-none text-emerald-600 dark:text-emerald-400 sm:text-lg">
                                {successfulFilesCount}
                            </div>
                            <div className="mt-0.5 text-[9px] leading-tight text-muted-foreground sm:mt-1 sm:text-xs">تم رفعها</div>
                        </div>
                        <div className="rounded-lg border bg-muted/20 px-1.5 py-1.5 text-center sm:rounded-xl sm:px-3 sm:py-2.5">
                            <div className={`text-sm font-bold leading-none sm:text-lg ${errorFilesCount ? "text-destructive" : ""}`}>
                                {retryCount}
                            </div>
                            <div className="mt-0.5 text-[9px] leading-tight text-muted-foreground sm:mt-1 sm:text-xs">تحتاج إجراء</div>
                        </div>
                    </div>
                )}

                {/* Upload Zone */}

                {!uploading && draftReady && (

                    <div className="shrink-0 rounded-xl border border-dashed bg-muted/10 p-1">
                        {/* الهاتف: اختيار ملف فقط — بدون الماسح، صورة 4×6 أو عدة أوراق */}
                        <div className="sm:hidden">
                            <FileUploader
                                value={null}
                                onChange={addFileToList}
                                allowContinuousCapture
                                accept=".pdf,.jpg,.jpeg,.png"
                                size="md"
                                maxSizeMB={10}
                                hint="PDF, JPG, PNG — الحد الأقصى 10MB"
                            />
                        </div>

                        {/* الشاشات الأكبر: إبقاء خيارات الكاميرا/المسح كما كانت */}
                        <div className="hidden sm:block">
                            <FileUploader
                                value={null}
                                onChange={addFileToList}
                                allowContinuousCapture
                                accept=".pdf,.jpg,.jpeg,.png"
                                enableCamera
                                size="md"
                                maxSizeMB={10}
                                hint="PDF, JPG, PNG — الحد الأقصى 10MB لكل ملف"
                            />
                        </div>

                        {/* جلسة المسح الجماعي بالكاميرا (م5) — هاتف أولًا */}
                        {/* <div className="mt-1 px-1 pb-1">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => setScanOpen(true)}
                                disabled={uploading}
                                className="min-h-[44px] w-full"
                            >
                                مسح الوثائق بالكاميرا (دفعة متتالية)
                            </Button>
                        </div> */}
                    </div>

                )}

                {/* Progress */}
                {uploading && (
                    <div className="shrink-0 rounded-xl border bg-primary/5 p-3 sm:p-4">
                        <div className="mb-2 flex items-center justify-between gap-3 text-sm">
                            <span className="inline-flex items-center gap-2 font-semibold">
                                <Loader2 className="h-4 w-4 animate-spin text-primary" />
                                جارٍ معالجة الملفات ({completedCount}/{files.length})
                            </span>
                            <span className="font-medium text-muted-foreground">{Math.round(progress)}%</span>
                        </div>
                        <div
                            className="h-2.5 w-full overflow-hidden rounded-full bg-muted"
                            role="progressbar"
                            aria-label="تقدم رفع الملفات"
                            aria-valuenow={Math.round(progress)}
                            aria-valuemin={0}
                            aria-valuemax={100}
                        >
                            <div
                                className="h-full rounded-full bg-primary transition-[width] duration-300 ease-out"
                                style={{ width: `${progress}%` }}
                            />
                        </div>
                    </div>
                )}

                {/* Results */}
                {results && (
                    <div
                        className={`shrink-0 rounded-xl border p-3 sm:p-4 ${results.finalizeError || results.failed > 0
                                ? "border-destructive bg-destructive/5"
                                : "border-emerald-500 bg-emerald-500/5"
                            }`}
                        aria-live="polite"
                    >
                        <div className="flex flex-wrap items-center gap-2">
                            {results.success > 0 && (
                                <Badge variant={BADGE_VARIANT.SUCCESS} dot>
                                    {results.success} مرفوعة بنجاح
                                </Badge>
                            )}
                            {results.failed > 0 && (
                                <Badge variant={BADGE_VARIANT.DANGER} dot>
                                    {results.failed} تحتاج إعادة محاولة
                                </Badge>
                            )}
                            {!results.finalizeError && results.failed === 0 && results.success === files.length && (
                                <span className="inline-flex items-center gap-1.5 text-sm font-semibold text-emerald-700 dark:text-emerald-400">
                                    <CheckCircle2 className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                    اكتملت العملية بنجاح
                                </span>
                            )}
                        </div>

                        {results.finalizeError && (
                            <div className="mt-2 flex items-start gap-2 text-sm text-destructive">
                                <CircleAlert className="mt-0.5 h-3.5 w-3.5 shrink-0 sm:h-4 sm:w-4" />
                                <span>
                                    فشل التحديث النهائي: {results.finalizeError}. يمكنك إعادة التحديث دون رفع الملفات الناجحة مرة أخرى.
                                </span>
                            </div>
                        )}
                    </div>
                )}

                {/* File List */}

                {files.length > 0 && (

                    <FormSection

                        title="الملفات المضافة"

                        icon={ListChecks}

                        badge={files.length}

                        className="flex min-h-0 flex-1 flex-col"

                    >

                        <ScrollArea className="min-h-0 flex-1 pr-1 sm:pr-2">

                            <div ref={fileListRef} className="space-y-2 sm:space-y-3">

                                {files.map((item) => (

                                    <BulkFileRow

                                        key={item.id}

                                        item={item}

                                        uploading={uploading}
                                        ownersUnavailable={Boolean(entitiesLoading || entitiesError)}

                                        docTypeOptions={docTypeOptions}

                                        entityOptions={entityOptions}

                                        onUpdateField={updateFileField}

                                        onRemove={removeFile}

                                        onSetPreview={setPreviewItem}

                                        onSetCrop={setCropItem}

                                    />

                                ))}

                            </div>

                        </ScrollArea>

                    </FormSection>

                )}

                {/* Empty state */}

                {files.length === 0 && !results && (

                    <div className="flex shrink-0 flex-col items-center justify-center rounded-xl border border-dashed bg-muted/10 px-4 py-12 text-muted-foreground">

                        <div className="w-16 h-16 rounded-full bg-muted flex items-center justify-center mb-4">

                            <FileText className="w-8 h-8 opacity-100" />

                        </div>

                        <Text variant="body" className="text-base font-medium">لم تُضف ملفات بعد</Text>

                        <p className="text-sm mt-2 max-w-xs text-center">استخدم مرفق الملفات أو الكاميرا لإضافة وثائق متعددة دفعة واحدة</p>

                    </div>

                )}

            </div>

            {/* Preview Dialog */}

            <Dialog open={!!previewItem} onOpenChange={(v) => !v && setPreviewItem(null)}>

                <DialogContent size="full" className="flex flex-col overflow-hidden max-h-[90dvh] p-0">

                    <DialogHeader className="shrink-0 px-4 py-3 border-b">

                        <DialogTitle className="text-base sm:text-lg flex items-center gap-2">

                            <FileText className="w-5 h-5" />

                            معاينة الصورة

                        </DialogTitle>

                    </DialogHeader>

                    <div className="flex-1 overflow-auto flex justify-center items-center bg-muted/10 p-2 sm:p-4">

                        {previewItem?.preview && (

                            <img

                                src={previewItem.preview}

                                alt={previewItem.fileName}

                                className="max-w-full max-h-[70dvh] sm:max-h-[75dvh] object-contain rounded-md shadow-lg"

                            />

                        )}

                    </div>

                </DialogContent>

            </Dialog>

            {/* Crop Dialog */}

            <ImageCropperDialog

                open={!!cropItem}

                imageFile={cropItem?.file || null}

                onClose={() => setCropItem(null)}

                onCrop={(croppedFile) => {

                    if (cropItem) {

                        if (croppedFile.size > MAX_FILE_SIZE_BYTES) {

                            // رمي الخطأ يُبقي محرر القص مفتوحًا ليعيد المستخدم القص بدل فقدان الصورة
                            throw new Error("حجم الملف بعد القص يتجاوز 10 ميجابايت");

                        }

                        const preview = URL.createObjectURL(croppedFile);

                        blobUrlsRef.current.add(preview);

                        updateFileField(cropItem.id, {

                            file: croppedFile,

                            preview,

                            fileName: croppedFile.name,

                            fileSize: croppedFile.size,

                        });

                    }

                    setCropItem(null);

                }}

            />

            {/* جلسة المسح الجماعي (م5) */}
            <CameraScanDialog
                open={scanOpen}
                onOpenChange={setScanOpen}
                onAccept={addScanCapture}
            />

        </BaseFormDialog>

    );

}
