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

    useConfirm,

} from "@/components/ui";

import { BADGE_VARIANT } from "@/constants";

import { normalizeApiError } from "@/lib/errorUtils";

import type { AgentSummary, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";

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
    docTypeOptions,
    entityOptions,
    onUpdateField,
    onRemove,
    onSetPreview,
    onSetCrop,
}: BulkFileRowProps) {
    const canEdit = !uploading && (item.status === "pending" || item.status === "error");

    const status = (() => {
        if (item.status === "success") {
            return (
                <Badge variant={BADGE_VARIANT.SUCCESS} className="gap-0.5 px-1.5 py-0.5 text-[10px] sm:gap-1 sm:px-2.5 sm:py-0.5 sm:text-xs">
                    <CheckCircle2 className="h-3 w-3 sm:h-3.5 sm:w-3.5" />
                    تم الرفع
                </Badge>
            );
        }

        if (item.status === "error") {
            return (
                <Badge variant={BADGE_VARIANT.DANGER} className="gap-0.5 px-1.5 py-0.5 text-[10px] sm:gap-1 sm:px-2.5 sm:py-0.5 sm:text-xs">
                    <XCircle className="h-3 w-3 sm:h-3.5 sm:w-3.5" />
                    يحتاج مراجعة
                </Badge>
            );
        }

        if (item.status === "uploading") {
            return (
                <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-1.5 py-0.5 text-[10px] font-medium text-primary sm:gap-1.5 sm:px-2 sm:py-1 sm:text-xs">
                    <Loader2 className="h-3 w-3 animate-spin sm:h-3.5 sm:w-3.5" />
                    جارٍ الرفع
                </span>
            );
        }

        return (
            <span className="inline-flex items-center rounded-full bg-muted px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground sm:px-2 sm:py-1 sm:text-xs">
                بانتظار الرفع
            </span>
        );
    })();

    return (
        <div
            dir="rtl"
            className={`w-full max-w-full overflow-hidden rounded-lg border bg-card p-2 text-right shadow-sm transition-colors sm:rounded-xl sm:p-4 ${
                item.status === "error" ? "border-destructive/40" : "border-border/70"
            }`}
        >
            {/* رأس البطاقة: الصورة + معلومات الملف + الحالة */}
            <div className="flex w-full min-w-0 items-start gap-2 sm:gap-3">
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
                        <FileText className="h-5 w-5 text-primary/60 sm:h-6 sm:w-6" />
                    </div>
                )}

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

                    {/* الحالة وأزرار الإجراءات في الجهة المقابلة للصورة */}
                    <div className="flex shrink-0 flex-col items-end gap-1.5">
                        <div>{status}</div>

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
                <div className="mt-2 grid grid-cols-1 gap-2 sm:mt-3 sm:grid-cols-2 sm:gap-3">
                    <div className="min-w-0 space-y-1">
                        <span className="block text-[11px] font-medium leading-4 text-foreground sm:text-xs">
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
                            className="!h-9 !min-h-9 w-full !px-2 !py-0 text-right !text-xs sm:!h-10 sm:!min-h-10 sm:!px-3 sm:!text-sm"
                            disabled={uploading}
                        />
                    </div>

                    <div className="min-w-0 space-y-1">
                        <span className="block text-[11px] font-medium leading-4 text-foreground sm:text-xs">
                            صاحب الوثيقة
                        </span>
                        <NativeSelect
                            options={entityOptions}
                            value={item.ownerEntity}
                            onChange={(val) => onUpdateField(item.id, { ownerEntity: val })}
                            className="!h-9 !min-h-9 w-full !px-2 !py-0 text-right !text-xs sm:!h-10 sm:!min-h-10 sm:!px-3 sm:!text-sm"
                            disabled={uploading}
                        />
                    </div>

                    <div className="min-w-0 space-y-1 sm:col-span-2">
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

            {item.error && (
                <div className="mt-2 flex items-start gap-1.5 rounded-md border border-destructive/20 bg-destructive/5 px-2 py-1.5 text-[11px] font-medium text-destructive sm:mt-3 sm:gap-2 sm:rounded-lg sm:px-3 sm:py-2 sm:text-xs">
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

    const [files, setFiles] = useState<BulkFileItem[]>([]);

    const [uploading, setUploading] = useState(false);

    const [results, setResults] = useState<UploadResults | null>(null);

    const [previewItem, setPreviewItem] = useState<BulkFileItem | null>(null);

    const [cropItem, setCropItem] = useState<BulkFileItem | null>(null);

    const filesRef = useRef<BulkFileItem[]>([]);

    // وثائق نجحت عبر runs: id العنصر → documentID. التحديث النهائي يستخدم الكل، وإعادة المحاولة لا تعيد رفع الناجح.

    const uploadedIdsRef = useRef<Map<string, number>>(new Map());

    const abortRef = useRef<AbortController | null>(null);

    const isMountedRef = useRef(true);

    const isUploadingRef = useRef(false);

    const blobUrlsRef = useRef<Set<string>>(new Set());
    const autoCloseTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    const { confirm, ConfirmDialogElement } = useConfirm();

    /* keep ref in sync with state so callbacks don't stale */

    useEffect(() => {

        filesRef.current = files;

    }, [files]);

    /* fetch entities */

    const { data: fileEntities } = useQuery({

        queryKey: ["file-entities", fileId],

        queryFn: async () => {

            const [agents, guardians, entitled] = await Promise.all([

                retirementApi.getAgents(fileId).catch((): AgentSummary[] => []),

                retirementApi.getGuardians(fileId).catch((): GuardianSummary[] => []),

                retirementApi.getEntitledPersons(fileId).catch((): EntitledPersonSummary[] => []),

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

            setFiles([]);

        }

    }, [open]);

    /* add files (supports single or multiple) */

    const addFileToList = useCallback(

        (input: File | File[] | null) => {

            if (!input) return;

            const arr = Array.isArray(input) ? input : [input];

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

                setFiles((prev) => {

                    const exists = prev.some((f) => f.fileName === file.name && f.fileSize === file.size);

                    if (exists) {

                        toast.warning(`الملف "${file.name}" موجود بالفعل في القائمة`);

                        return prev;

                    }

                    const id = generateId();

                    const preview = isImageFile(file) ? URL.createObjectURL(file) : undefined;

                    if (preview) blobUrlsRef.current.add(preview);

                    const newItem: BulkFileItem = {

                        id,

                        file,

                        fileName: file.name,

                        fileSize: file.size,

                        preview,

                        documentTypeID: 0,

                        ownerEntity: "retiree",

                        notes: "",

                        status: "pending",

                    };

                    return [...prev, newItem];

                });

            });

        },

        []

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

        const currentFiles = filesRef.current;
        const pending = currentFiles.filter((f) => f.status === "pending" || f.status === "error");
        const hadFinalizeError = Boolean(results?.finalizeError);
        const retryFinalizeOnly = pending.length === 0 && hadFinalizeError;

        if (!pending.length && !retryFinalizeOnly) return;

        isUploadingRef.current = true;
        const controller = new AbortController();
        abortRef.current = controller;

        setUploading(true);
        setResults(null);

        const finishUploading = () => {
            if (isMountedRef.current) {
                setUploading(false);
                if (abortRef.current === controller) abortRef.current = null;
            }
            isUploadingRef.current = false;
        };

        const restoreCancelledItems = () => {
            if (isMountedRef.current) {
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
            if (controller.signal.aborted) return;

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

                if (controller.signal.aborted) return;

                if (typeof uploaded?.documentID === "number") {
                    uploadedIdsRef.current.set(item.id, uploaded.documentID);
                    outcomes.push({ success: true, id: item.id, documentId: uploaded.documentID });
                } else {
                    outcomes.push({ success: true, id: item.id });
                }

                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) =>
                            f.id === item.id ? { ...f, status: "success" as const, error: undefined } : f
                        )
                    );
                }
            } catch (err) {
                if (controller.signal.aborted) return;

                const axiosErr = err as {
                    response?: {
                        status?: number;
                        data?: { message?: string; data?: { documentID?: number; }; };
                    };
                };

                if (axiosErr.response?.status === 409) {
                    const matchedId = axiosErr.response.data?.data?.documentID;
                    if (matchedId) {
                        uploadedIdsRef.current.set(item.id, matchedId);
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

                    const dupMessage =
                        axiosErr.response.data?.message ?? "الوثيقة مكررة ولا تخص هذا المالك";
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
            let tracked: Promise<void>;
            tracked = task.finally(() => executing.delete(tracked));
            executing.add(tracked);

            if (executing.size >= UPLOAD_CONCURRENCY) {
                await Promise.race(executing);
            }
        }

        await Promise.all(executing);

        if (controller.signal.aborted) {
            restoreCancelledItems();
            return;
        }

        const runSuccessCount = outcomes.filter((outcome) => outcome.success).length;
        const runFailedCount = outcomes.filter((outcome) => !outcome.success).length;
        const previousSuccessCount = currentFiles.filter((f) => f.status === "success").length;
        const totalSuccessCount = previousSuccessCount + runSuccessCount;
        const totalFailedCount = runFailedCount;

        const shouldFinalize = runSuccessCount > 0 || retryFinalizeOnly || hadFinalizeError;

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
            await retirementApi.refreshAfterUpload(fileId, documentIds);
        } catch (err) {
            if (controller.signal.aborted) {
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

        if (controller.signal.aborted) {
            restoreCancelledItems();
            return;
        }

        if (runFailedCount) {
            toast.error(`فشل رفع ${runFailedCount} وثيقة من أصل ${pending.length}`);
        }

        if (runSuccessCount) {
            toast.success(`تم رفع ${runSuccessCount} وثيقة بنجاح`);
        } else if (hadFinalizeError) {
            toast.success("تم التحديث النهائي بنجاح");
        }

        if (isMountedRef.current) {
            setResults({ success: totalSuccessCount, failed: totalFailedCount });
            if (totalSuccessCount > 0) onSaved?.();

            if (totalSuccessCount > 0 && totalFailedCount === 0) {
                if (autoCloseTimeoutRef.current) clearTimeout(autoCloseTimeoutRef.current);
                autoCloseTimeoutRef.current = setTimeout(() => {
                    autoCloseTimeoutRef.current = null;
                    if (isMountedRef.current) onOpenChange(false);
                }, 1400);
            }
        }

        finishUploading();
    }, [fileId, onSaved, onOpenChange, results?.finalizeError]);

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

        () => files.some((f) => (f.status === "pending" || f.status === "error") && !f.documentTypeID),

        [files]

    );

    const cancelUpload = useCallback(() => {
        abortRef.current?.abort();
    }, []);

    const handleClose = useCallback(async () => {
        if (uploading) return;

        const hasUnuploaded = filesRef.current.some(
            (f) => f.status === "pending" || f.status === "error"
        );
        const hasFinalizeError = Boolean(results?.finalizeError);

        if (hasUnuploaded || hasFinalizeError) {
            const ok = await confirm({
                title: hasFinalizeError
                    ? "لم يكتمل التحديث النهائي بعد. هل تريد الإغلاق رغم ذلك؟"
                    : "لديك ملفات لم تُرفع بعد. هل تريد الإغلاق؟",
                variant: "warning",
            });
            if (!ok) return;
        }

        onOpenChange(false);
    }, [confirm, onOpenChange, results?.finalizeError, uploading]);

    const handleDialogOpenChange = useCallback(
        (nextOpen: boolean) => {
            if (nextOpen) {
                onOpenChange(true);
                return;
            }
            void handleClose();
        },
        [handleClose, onOpenChange]
    );

    return (

        <BaseFormDialog

            open={open}

            onOpenChange={handleDialogOpenChange}

            title="رفع جماعي للوثائق"

            size="full"

            isLoading={uploading}

            isDirty={files.length > 0}

            onSubmit={uploadAll}

            enableKeyboardShortcuts={false}

            footer={
                <div className="flex w-full flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <div className="min-h-5 text-xs">
                        {hasInvalidFiles && !uploading && (
                            <span className="inline-flex items-center gap-1.5 font-medium text-destructive">
                                <CircleAlert className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                اختر نوع الوثيقة لجميع الملفات قبل الرفع.
                            </span>
                        )}
                    </div>

                    <div className="flex w-full flex-col-reverse gap-2 sm:w-auto sm:flex-row">
                        {uploading ? (
                            <Button
                                type="button"
                                variant="outline"
                                onClick={cancelUpload}
                                className="min-h-[44px] w-full min-w-[130px] sm:w-auto"
                            >
                                <XCircle className="h-3.5 w-3.5 sm:h-4 sm:w-4" />
                                إلغاء الرفع
                            </Button>
                        ) : (
                            <Button
                                type="button"
                                variant="outline"
                                onClick={handleClose}
                                className="min-h-[44px] w-full min-w-[110px] sm:w-auto"
                            >
                                إغلاق
                            </Button>
                        )}

                        {files.length > 0 && (
                            <Button
                                type="submit"
                                disabled={uploading || (retryCount === 0 && !results?.finalizeError) || hasInvalidFiles}
                                className="min-h-[44px] w-full min-w-[155px] sm:w-auto"
                            >
                                {uploading ? (
                                    <>
                                        <Loader2 className="h-4 w-4 animate-spin" />
                                        جارٍ الرفع...
                                    </>
                                ) : results?.finalizeError && retryCount === 0 ? (
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

                {!uploading && (

                    <div className="shrink-0 rounded-xl border border-dashed bg-muted/10 p-1">
                        {/* الهاتف: اختيار ملف فقط — بدون الماسح، صورة 4×6 أو عدة أوراق */}
                        <div className="sm:hidden">
                            <FileUploader
                                value={null}
                                onChange={addFileToList}
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
                                accept=".pdf,.jpg,.jpeg,.png"
                                enableCamera
                                size="md"
                                maxSizeMB={10}
                                hint="PDF, JPG, PNG — الحد الأقصى 10MB لكل ملف"
                            />
                        </div>
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
                                ? "border-destructive/30 bg-destructive/5"
                                : "border-emerald-500/25 bg-emerald-500/5"
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

                            <div className="space-y-2 sm:space-y-3">

                                {files.map((item) => (

                                    <BulkFileRow

                                        key={item.id}

                                        item={item}

                                        uploading={uploading}

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

                            <FileText className="w-8 h-8 opacity-40" />

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

                            toast.error("حجم الملف بعد القص يتجاوز 10 ميجابايت");

                            setCropItem(null);

                            return;

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

            {ConfirmDialogElement}

        </BaseFormDialog>

    );

}