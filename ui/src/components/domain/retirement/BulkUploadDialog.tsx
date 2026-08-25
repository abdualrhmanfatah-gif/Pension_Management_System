import { useQuery } from "@tanstack/react-query";
import { Crop as CropIcon, FileText, ListChecks, Loader2, Trash2, Upload, XCircle } from "lucide-react";
import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";


import { retirementApi } from "@/api/retirement";
import { documentsApi } from "@/api/documents";
import { normalizeApiError } from "@/lib/errorUtils";
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
    return (
        <div className="py-3 first:pt-0 last:pb-0">
            <div className="flex items-start gap-3">
                {/* Thumbnail */}
                {item.preview ? (
                    <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="w-10 h-10 sm:w-12 sm:h-12 rounded-lg border shrink-0 overflow-hidden p-0 hover:bg-muted/50"
                        onClick={() => onSetPreview(item)}
                        aria-label="معاينة الصورة"
                        disabled={uploading}
                    >
                        <img
                            src={item.preview}
                            alt=""
                            className="w-full h-full object-cover"
                        />
                    </Button>
                ) : (
                    <div className="w-10 h-10 sm:w-12 sm:h-12 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                        <FileText className="w-4 h-4 sm:w-5 sm:h-5 text-primary/60" />
                    </div>
                )}

                {/* Info + Controls */}
                <div className="flex-1 min-w-0 space-y-2">
                    {/* File name row */}
                    <div className="flex items-center gap-2 flex-wrap">
                        <span
                            title={item.fileName}
                            className="text-sm font-semibold truncate max-w-[120px] sm:max-w-[200px] md:max-w-[300px]"
                        >
                            {item.fileName}
                        </span>
                        <span className="text-xs text-muted-foreground shrink-0">
                            {formatFileSize(item.fileSize)}
                        </span>
                        {item.status === "success" && (
                            <Badge variant={BADGE_VARIANT.SUCCESS} className="text-xs px-1.5 py-0">
                                مرفوع
                            </Badge>
                        )}
                        {item.status === "error" && (
                            <Badge variant={BADGE_VARIANT.DANGER} className="text-xs px-1.5 py-0">
                                فشل
                            </Badge>
                        )}
                        {item.status === "uploading" && (
                            <Loader2 className="w-4 h-4 text-primary animate-spin shrink-0" />
                        )}
                    </div>

                    {/* Classification controls */}
                    {!uploading && (item.status === "pending" || item.status === "error") && (
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                            <NativeSelect
                                options={docTypeOptions}
                                value={String(item.documentTypeID || "")}
                                onChange={(val) => onUpdateField(item.id, { documentTypeID: Number(val) })}
                                placeholder="اختر النوع..."
                                className="text-xs min-h-[36px]"
                                disabled={uploading}
                            />
                            {entityOptions.length > 1 && (
                                <NativeSelect
                                    options={entityOptions}
                                    value={item.ownerEntity}
                                    onChange={(val) => onUpdateField(item.id, { ownerEntity: val })}
                                    className="text-xs min-h-[36px]"
                                    disabled={uploading}
                                />
                            )}
                            <div className="sm:col-span-2 flex items-center gap-2">
                                <Textarea
                                    value={item.notes}
                                    onChange={(e) => onUpdateField(item.id, { notes: e.target.value })}
                                    placeholder="ملاحظات..."
                                    className="text-xs min-h-[36px] py-2 resize-none flex-1"
                                    rows={1}
                                    disabled={uploading}
                                />
                                {item.preview && (
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="icon"
                                        aria-label="قص الصورة"
                                        className="h-9 w-9 shrink-0 text-blue-600 hover:text-blue-700 hover:bg-blue-50"
                                        onClick={() => onSetCrop(item)}
                                        disabled={uploading}
                                    >
                                        <CropIcon className="w-4 h-4" />
                                    </Button>
                                )}
                                <Button
                                    type="button"
                                    variant="ghost"
                                    size="icon"
                                    aria-label="حذف الملف"
                                    className="h-9 w-9 shrink-0 text-destructive hover:text-destructive hover:bg-destructive/10"
                                    onClick={() => onRemove(item.id)}
                                    disabled={uploading}
                                >
                                    <Trash2 className="w-4 h-4" />
                                </Button>
                            </div>
                        </div>
                    )}

                    {/* Error message */}
                    {item.error && (
                        <p className="text-xs text-destructive font-medium flex items-center gap-1">
                            <XCircle className="w-3.5 h-3.5 shrink-0" />
                            {item.error}
                        </p>
                    )}
                </div>
            </div>
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
    const [results, setResults] = useState<{ success: number; failed: number } | null>(null);
    const [previewItem, setPreviewItem] = useState<BulkFileItem | null>(null);
    const [cropItem, setCropItem] = useState<BulkFileItem | null>(null);

    const filesRef = useRef<BulkFileItem[]>([]);
    const abortRef = useRef<AbortController | null>(null);
    const isMountedRef = useRef(true);
    const isUploadingRef = useRef(false);
    const blobUrlsRef = useRef<Set<string>>(new Set());

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

            const entities: { value: string; label: string }[] = [
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
            Array.from(blobUrlsRef.current).forEach((url) => URL.revokeObjectURL(url));
            blobUrlsRef.current.clear();
        };
    }, []);

    useEffect(() => {
        if (!open) {
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

                if (file.size > 10 * 1024 * 1024) {
                    toast.error(`حجم الملف "${file.name}" يتجاوز الحد الأقصى المسموح به (10 ميجابايت)`);
                    return;
                }

                const allowed = [".pdf", ".jpg", ".jpeg", ".png"];
                const ext = "." + file.name.split(".").pop()?.toLowerCase();
                if (!allowed.includes(ext)) {
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
                    const preview = file.type.startsWith("image/") ? URL.createObjectURL(file) : undefined;
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

    /* upload with concurrency limit = 3 */
    const uploadAll = useCallback(async () => {
        if (isUploadingRef.current) return;
        isUploadingRef.current = true;

        const currentFiles = filesRef.current;
        const pending = currentFiles.filter((f) => f.status === "pending" || f.status === "error");
        if (!pending.length) {
            isUploadingRef.current = false;
            return;
        }

        const controller = new AbortController();
        abortRef.current = controller;

        setUploading(true);
        setResults(null);

        setFiles((prev) =>
            prev.map((f) =>
                f.status === "pending" || f.status === "error"
                    ? { ...f, status: "uploading" as const, error: undefined }
                    : f
            )
        );

        const CONCURRENCY = 3;
        const outcomes: { success: boolean; id: string }[] = [];
        const executing = new Set<Promise<void>>();

        async function process(item: BulkFileItem) {
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
                if (item.notes) fd.append("Notes", item.notes);
                if (item.ownerEntity && item.ownerEntity !== "retiree") {
                    const [type, id] = item.ownerEntity.split("_");
                    if (type === "agent" && id) fd.append("AgentID", id);
                    if (type === "guardian" && id) fd.append("GuardianID", id);
                    if (type === "entitled" && id) fd.append("EntitledPersonID", id);
                }

                await documentsApi.upload(fd);

                if (controller.signal.aborted) {
                    outcomes.push({ success: false, id: item.id });
                    return;
                }

                if (isMountedRef.current) {
                    setFiles((prev) =>
                        prev.map((f) => (f.id === item.id ? { ...f, status: "success" as const } : f))
                    );
                }
                outcomes.push({ success: true, id: item.id });
            } catch (err) {
                if (controller.signal.aborted) {
                    outcomes.push({ success: false, id: item.id });
                    return;
                }

                // ─── 409 Conflict = وثيقة مكررة → اعتبرها نجاح ───
                const axiosErr = err as { response?: { status?: number } };
                if (axiosErr.response?.status === 409) {
                    if (isMountedRef.current) {
                        setFiles((prev) =>
                            prev.map((f) =>
                                f.id === item.id ? { ...f, status: "success" as const } : f
                            )
                        );
                    }
                    outcomes.push({ success: true, id: item.id });
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
            const p = process(item);
            const tracked = p.then(() => { executing.delete(tracked); });
            executing.add(tracked);
            if (executing.size >= CONCURRENCY) await Promise.race(executing);
        }
        await Promise.all(executing);

        if (controller.signal.aborted) {
            if (isMountedRef.current) {
                setUploading(false);
                abortRef.current = null;
            }
            isUploadingRef.current = false;
            return;
        }

        const successCount = outcomes.filter((o) => o.success).length;
        const failedCount = outcomes.filter((o) => !o.success).length;

        if (failedCount) toast.error(`فشل رفع ${failedCount} وثيقة من أصل ${pending.length}`);
        if (successCount) toast.success(`تم رفع ${successCount} وثيقة بنجاح`);

        if (isMountedRef.current) {
            setResults({ success: successCount, failed: failedCount });
            setUploading(false);
            abortRef.current = null;
            isUploadingRef.current = false;
            if (successCount) onSaved?.();
            
            // Auto close dialog if all uploads were successful
            if (successCount > 0 && failedCount === 0) {
                setTimeout(() => {
                    if (isMountedRef.current) onOpenChange(false);
                }, 1500);
            }
        }
    }, [fileId, onSaved, onOpenChange]);

    /* derived stats */
    const { completedCount, retryCount, progress } = useMemo(() => {
        const pending = files.filter((f) => f.status === "pending").length;
        const completed = files.filter((f) => f.status === "success" || f.status === "error").length;
        const retry = files.filter((f) => f.status === "pending" || f.status === "error").length;
        return {
            pendingCount: pending,
            completedCount: completed,
            retryCount: retry,
            progress: files.length ? (completed / files.length) * 100 : 0,
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

    const handleClose = useCallback(async () => {
        if (uploading) return;
        const hasUnuploaded = filesRef.current.some((f) => f.status === "pending" || f.status === "error");
        if (hasUnuploaded) {
            const ok = await confirm({
                title: "لديك ملفات غير مرفوعة، هل أنت متأكد من الإغلاق؟",
                variant: "warning",
            });
            if (!ok) return;
        }
        onOpenChange(false);
    }, [uploading, onOpenChange, confirm]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="رفع جماعي للوثائق"
            size="full"
            isLoading={uploading}
            isDirty={files.length > 0}
            onSubmit={uploadAll}
            enableKeyboardShortcuts={false}
            footer={
                <div className="flex flex-col-reverse sm:flex-row items-center justify-end gap-2 sm:gap-3 w-full">
                    {hasInvalidFiles && (
                        <span className="text-xs text-destructive font-semibold sm:ml-auto text-center sm:text-start">
                            * يرجى اختيار نوع الوثيقة لجميع الملفات قبل الرفع
                        </span>
                    )}
                    <Button
                        type="button"
                        variant="outline"
                        onClick={handleClose}
                        disabled={uploading}
                        className="w-full sm:w-auto min-w-[110px] min-h-[44px] order-3 sm:order-1"
                    >
                        إغلاق
                    </Button>

                    {files.length > 0 && (
                        <Button
                            type="submit"
                            disabled={uploading || retryCount === 0 || hasInvalidFiles}
                            className="w-full sm:w-auto min-w-[140px] min-h-[44px] order-1 sm:order-3"
                        >
                            <span className="flex items-center justify-center gap-2">
                                {uploading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Upload className="w-4 h-4" />}
                                {uploading ? "جاري الرفع..." : `رفع الكل (${retryCount})`}
                            </span>
                        </Button>
                    )}
                </div>
            }
        >
            <div className="flex flex-col flex-1 min-h-0 gap-4">
                {/* Upload Zone */}
                {!uploading && (
                    <div className="shrink-0">
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
                )}

                {/* Progress */}
                {uploading && (
                    <div className="shrink-0 p-3 bg-muted/30 rounded-lg border border-border/30">
                        <div className="flex items-center justify-between text-xs mb-2">
                            <span className="font-semibold">جاري الرفع... ({completedCount}/{files.length})</span>
                            <span className="text-muted-foreground">{Math.round(progress)}%</span>
                        </div>
                        <div
                            className="w-full bg-muted rounded-full h-2 overflow-hidden"
                            role="progressbar"
                            aria-valuenow={Math.round(progress)}
                            aria-valuemin={0}
                            aria-valuemax={100}
                        >
                            <div
                                className="bg-primary h-2 rounded-full transition-all duration-300 ease-out"
                                style={{ width: `${progress}%` }}
                            />
                        </div>
                    </div>
                )}

                {/* Results */}
                {results && (
                    <div className="shrink-0 p-3 bg-muted/30 rounded-lg border border-border/30 flex flex-wrap items-center gap-3 text-sm" aria-live="polite">
                        {results.success > 0 && <Badge variant={BADGE_VARIANT.SUCCESS} dot>{results.success} مرفوعة بنجاح</Badge>}
                        {results.failed > 0 && <Badge variant={BADGE_VARIANT.DANGER} dot>{results.failed} فشلت</Badge>}
                        {results.success === files.length && <Badge variant={BADGE_VARIANT.SUCCESS}>اكتمل الرفع بنجاح</Badge>}
                    </div>
                )}

                {/* File List */}
                {files.length > 0 && (
                    <FormSection
                        title="الملفات المرفوعة"
                        icon={ListChecks}
                        badge={files.length}
                        className="flex flex-col flex-1 min-h-0"
                    >
                        <ScrollArea className="flex-1 min-h-0 pr-2">
                            <div className="divide-y divide-border/30">
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
                    <div className="shrink-0 flex flex-col items-center justify-center py-12 text-muted-foreground">
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
                        if (croppedFile.size > 10 * 1024 * 1024) {
                            toast.error("حجم الملف بعد القص يتجاوز 10 ميجابايت");
                            setCropItem(null);
                            return;
                        }
                        const preview = URL.createObjectURL(croppedFile);
                        blobUrlsRef.current.add(preview);
                        if (cropItem.preview) revokeBlob(cropItem.preview);
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