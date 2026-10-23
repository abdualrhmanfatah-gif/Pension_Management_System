import { useCallback, useEffect, useId, useRef, useState } from "react";
import { toast } from "sonner";

import { canvasToFile } from "@/lib/document-image";

import {
    DEFAULT_CROP_TIMEOUT_MS,
    runCropPipeline,
} from "../../lib/crop-pipeline";
import { getDeviceCapabilities } from "../../lib/device-capabilities";
import { CornerEditor } from "./CornerEditor";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "./Dialog";
import { FormDialogFooter } from "./FormDialogFooter";

export interface ImageCropperDialogProps {
    open: boolean;
    imageFile: File | string | null;
    onClose: () => void;
    onCrop: (file: File) => Promise<void> | void;
    detectedCorners?: { x: number; y: number }[] | null;
}

// ─── Helpers ─────────────────────────────────────────────

// ─── Component ───────────────────────────────────────────

export function ImageCropperDialog({
    open,
    imageFile,
    onClose,
    onCrop,
    detectedCorners,
}: ImageCropperDialogProps) {
    const titleId = useId();
    const [isSaving, setIsSaving] = useState(false);
    const [isStoring, setIsStoring] = useState(false);
    const [manualCorners, setManualCorners] = useState<{ x: number; y: number }[] | null>(null);
    const [displayUrl, setDisplayUrl] = useState<string>("");
    const dialogRef = useRef<HTMLDivElement>(null);
    const loadedImgRef = useRef<HTMLImageElement | null>(null);
    const operationRef = useRef(0);
    const displayUrlRef = useRef<string>("");
    const isClosingRef = useRef(false);
    const originalFileRef = useRef<File | null>(null);
    const deviceCapabilities = getDeviceCapabilities();

    const cleanupAll = useCallback(() => {
        displayUrlRef.current = "";
        setDisplayUrl("");
        if (loadedImgRef.current) {
            loadedImgRef.current.src = "";
            loadedImgRef.current = null;
        }
        setManualCorners(null);
        originalFileRef.current = null;
    }, []);

    // Load original image
    useEffect(() => {
        if (!imageFile) {
            cleanupAll();
            return;
        }
        if (!open) return;

        if (imageFile instanceof File) {
            originalFileRef.current = imageFile;
        }

        let cancelled = false;
        const src = typeof imageFile === "string" ? imageFile : URL.createObjectURL(imageFile);
        let released = false;
        const releaseSource = () => {
            if (typeof imageFile !== "string" && !released) {
                URL.revokeObjectURL(src);
                released = true;
            }
        };
        const img = new Image();
        // لا crossOrigin — انظر loadImage أعلاه
        img.onload = () => {
            if (cancelled || isClosingRef.current) {
                releaseSource();
                return;
            }
            loadedImgRef.current = img;
            displayUrlRef.current = src;
            setDisplayUrl(src);
            setManualCorners(detectedCorners || null);
        };
        img.onerror = () => {
            releaseSource();
            if (!cancelled) {
                console.error("Failed to load image", {
                    src,
                    type: typeof imageFile === "string" ? "url" : imageFile?.type,
                    size: typeof imageFile === "string" ? null : imageFile?.size,
                });
            }
        };
        img.src = src;

        return () => {
            cancelled = true;
            img.onload = null;
            img.onerror = null;
            img.src = "";
            if (loadedImgRef.current === img) loadedImgRef.current = null;
            if (displayUrlRef.current === src) {
                displayUrlRef.current = "";
                setDisplayUrl("");
                setManualCorners(null);
            }
            releaseSource();
        };
    }, [imageFile, detectedCorners, cleanupAll, open]);

    // Reset on open / cleanup on close
    useEffect(() => {
        if (open) {
            isClosingRef.current = false;
            setIsSaving(false);
            setIsStoring(false);
        } else {
            isClosingRef.current = true;
            operationRef.current++;
            cleanupAll();
        }
    }, [open, cleanupAll]);

    const handleApply = useCallback(async () => {
        if (!manualCorners || manualCorners.length !== 4 || !imageFile || isSaving) return;
        setIsSaving(true);

        const opId = ++operationRef.current;

        let outCanvas: HTMLCanvasElement | null = null;

        // مهلة المعالجة فقط (WebGL/Worker/CPU) — الحفظ والرفع خارجها
        const abortController = new AbortController();
        const timeoutId = setTimeout(() => {
            if (opId !== operationRef.current) return;
            operationRef.current++;
            abortController.abort();
            toast.error("انتهت مهلة القص، يرجى المحاولة مرة أخرى");
            setIsSaving(false);
        }, DEFAULT_CROP_TIMEOUT_MS);

        try {
            const isFile = imageFile instanceof File;
            // الصورة محمّلة مسبقاً في loadedImgRef — لا داعي لإعادة التحميل من الملف
            const img = loadedImgRef.current;
            if (!img) throw new Error("الصورة لم تُحمَّل بعد");
            if (opId !== operationRef.current) return;

            const result = await runCropPipeline({
                source: img,
                sourceWidth: img.naturalWidth,
                sourceHeight: img.naturalHeight,
                corners: manualCorners,
                capabilities: deviceCapabilities,
                signal: abortController.signal,
            });
            if (opId !== operationRef.current) return;

            // انتهت المعالجة: تُلغى المهلة فوراً حتى لا تُطلق خطأً أثناء الحفظ أو الرفع
            clearTimeout(timeoutId);

            outCanvas = document.createElement("canvas");
            outCanvas.width = result.width;
            outCanvas.height = result.height;
            const outCtx = outCanvas.getContext("2d");
            if (!outCtx) throw new Error("فشل إنشاء سياق الإخراج");
            outCtx.putImageData(new ImageData(result.data, result.width, result.height), 0, 0);

            const file = await canvasToFile(outCanvas, isFile ? imageFile.name : "cropped.jpg");
            outCanvas.width = 0;
            outCanvas.height = 0;
            outCanvas = null;

            if (opId !== operationRef.current) return;

            // الحفظ والرفع خارج مهلة المعالجة
            setIsStoring(true);
            await onCrop(file);
            if (opId !== operationRef.current) return;
            onClose();
        } catch (error) {
            if (opId === operationRef.current) {
                console.error("Perspective crop error:", error);
                const message = error instanceof Error ? error.message : "";
                toast.error(/[\u0600-\u06FF]/.test(message) ? message : "فشل قص الصورة");
            }
        } finally {
            clearTimeout(timeoutId);
            if (outCanvas) {
                outCanvas.width = 0;
                outCanvas.height = 0;
            }
            if (opId === operationRef.current) {
                setIsSaving(false);
                setIsStoring(false);
            }
        }
    }, [manualCorners, imageFile, onCrop, onClose, isSaving, deviceCapabilities]);

    const handleCancel = useCallback(() => {
        if (!isSaving) onClose();
    }, [isSaving, onClose]);

    return (
        <Dialog open={open} onOpenChange={(v) => !v && handleCancel()}>
            <DialogContent ref={dialogRef} aria-labelledby={titleId} className="p-0 sm:max-w-2xl" dir="rtl" size="lg" clipContent={false}>
                <DialogHeader className="shrink-0 px-3 sm:px-6 py-2 sm:py-4 border-b border-border">
                    <DialogTitle id={titleId} className="text-base sm:text-xl">تعديل حواف المستند</DialogTitle>
                </DialogHeader>

                {displayUrl && (
                    <div className="px-1 sm:px-4 py-2 sm:py-3">
                        <CornerEditor
                            imageUrl={displayUrl}
                            initialCorners={detectedCorners}
                            corners={manualCorners ?? undefined}
                            onCornersChange={(c) => setManualCorners(c)}
                        />
                    </div>
                )}

                <div className="shrink-0 px-3 sm:px-6 py-2 sm:py-4 border-t border-border bg-muted/5">
                    <FormDialogFooter
                        saveLabel={isStoring ? "جارٍ الحفظ..." : isSaving ? "جاري القص..." : "تأكيد التعديل"}
                        cancelLabel="إلغاء"
                        onSave={handleApply}
                        onCancel={handleCancel}
                        disabled={isSaving}
                    />
                </div>
            </DialogContent>
        </Dialog>
    );
}
