<<<<<<< HEAD
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
=======
import { useCallback, useEffect, useRef, useState } from "react";
import { toast } from "sonner";

import { CornerEditor } from "./CornerEditor";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "./Dialog";
import { FormDialogFooter } from "./FormDialogFooter";
import { CPUProcessor } from "../../lib/cpu-processor";
import { getDeviceCapabilities, getOptimalProcessingSize, shouldUseWebGL } from "../../lib/device-capabilities";
import { WebGLProcessor } from "../../lib/webgl-processor";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export interface ImageCropperDialogProps {
    open: boolean;
    imageFile: File | string | null;
    onClose: () => void;
    onCrop: (file: File) => Promise<void> | void;
    detectedCorners?: { x: number; y: number }[] | null;
}

<<<<<<< HEAD
// ─── Helpers ─────────────────────────────────────────────

=======
const CROP_TIMEOUT_MS = 30000;

// ─── Canvas Pool ─────────────────────────────────────────
const globalCanvasPool: HTMLCanvasElement[] = [];

function acquireCanvas(w?: number, h?: number): HTMLCanvasElement {
    const c = globalCanvasPool.pop() || document.createElement("canvas");
    if (w !== undefined) c.width = w;
    if (h !== undefined) c.height = h;
    return c;
}

function releaseCanvas(c: HTMLCanvasElement) {
    c.width = 0;
    c.height = 0;
    globalCanvasPool.push(c);
}

// ─── Helpers ─────────────────────────────────────────────

function loadImage(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
        const img = new Image();
        if (src.startsWith("http")) img.crossOrigin = "anonymous";
        img.onload = () => resolve(img);
        img.onerror = () => {
            console.error("loadImage: Failed to load", { src, crossOrigin: img.crossOrigin });
            reject(new Error("فشل تحميل الصورة"));
        };
        img.src = src;
    });
}

function canvasToFile(canvas: HTMLCanvasElement, originalName: string): Promise<File> {
    return new Promise((resolve, reject) => {
        canvas.toBlob((blob) => {
            if (!blob) {
                reject(new Error("فشل تحويل Canvas"));
                return;
            }
            // Strip GUID prefix added by LocalFileStorageService (e.g. "guid_original.jpg")
            const cleaned = originalName.replace(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_/i, "");
            const baseName = cleaned.replace(/\.[^.]+$/, "");
            const name = baseName.endsWith("_scanned") ? `${baseName}.jpg` : `${baseName}_scanned.jpg`;
            resolve(new File([blob], name, { type: "image/jpeg" }));
        }, "image/jpeg", 0.85);
    });
}

// ─── Processor Manager ──────────────────────────────────

let webglProcessor: WebGLProcessor | null = null;
let cpuProcessor: CPUProcessor | null = null;
let deviceCapabilities = getDeviceCapabilities();

function getProcessors() {
    if (!webglProcessor && shouldUseWebGL(deviceCapabilities)) {
        webglProcessor = new WebGLProcessor({
            maxTextureSize: deviceCapabilities.maxTextureSize,
            enhanceAmount: 0.3,
        });
    }
    if (!cpuProcessor) {
        cpuProcessor = new CPUProcessor({
            maxDimension: getOptimalProcessingSize(deviceCapabilities),
            enhanceAmount: 0.3,
        });
    }
    return { webglProcessor, cpuProcessor };
}

function processImage(
    srcData: Uint8ClampedArray,
    srcWidth: number,
    srcHeight: number,
    corners: { x: number; y: number }[],
): { data: Uint8ClampedArray; width: number; height: number } {
    const { webglProcessor, cpuProcessor } = getProcessors();

    // Try WebGL first
    if (webglProcessor) {
        try {
            const result = webglProcessor.process(srcData, srcWidth, srcHeight, corners);
            if (result) return result;
        } catch (error) {
            console.warn("WebGL processing failed, falling back to CPU:", error);
        }
    }

    // Fallback to CPU
    if (cpuProcessor) {
        return cpuProcessor.process(srcData, srcWidth, srcHeight, corners);
    }

    throw new Error("No processor available");
}

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
// ─── Component ───────────────────────────────────────────

export function ImageCropperDialog({
    open,
    imageFile,
    onClose,
    onCrop,
    detectedCorners,
}: ImageCropperDialogProps) {
<<<<<<< HEAD
    const titleId = useId();
    const [isSaving, setIsSaving] = useState(false);
    const [isStoring, setIsStoring] = useState(false);
=======
    const [isSaving, setIsSaving] = useState(false);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const [manualCorners, setManualCorners] = useState<{ x: number; y: number }[] | null>(null);
    const [displayUrl, setDisplayUrl] = useState<string>("");
    const dialogRef = useRef<HTMLDivElement>(null);
    const loadedImgRef = useRef<HTMLImageElement | null>(null);
<<<<<<< HEAD
    const operationRef = useRef(0);
    const displayUrlRef = useRef<string>("");
    const isClosingRef = useRef(false);
    const originalFileRef = useRef<File | null>(null);
    const deviceCapabilities = getDeviceCapabilities();

    const cleanupAll = useCallback(() => {
=======
    const workerRef = useRef<Worker | null>(null);
    const displayUrlRef = useRef<string>("");
    const isClosingRef = useRef(false);
    const originalFileRef = useRef<File | null>(null);

    // Initialize worker only when dialog opens
    useEffect(() => {
        if (!open) return;

        const w = new Worker(
            new URL("../../workers/perspective-warp.worker.ts", import.meta.url),
            { type: "module" },
        );
        workerRef.current = w;
        return () => {
            w.terminate();
            workerRef.current = null;
        };
    }, [open]);

    // Cleanup function for URLs and image
    const cleanupDisplayUrl = useCallback((url: string) => {
        if (!url || !url.startsWith("blob:")) return;
        // Don't revoke the original image if it's the current base image
        if (loadedImgRef.current && url === loadedImgRef.current.src) return;
        URL.revokeObjectURL(url);
    }, []);

    const cleanupAll = useCallback(() => {
        cleanupDisplayUrl(displayUrlRef.current);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        displayUrlRef.current = "";
        setDisplayUrl("");
        if (loadedImgRef.current) {
            loadedImgRef.current.src = "";
            loadedImgRef.current = null;
        }
        setManualCorners(null);
        originalFileRef.current = null;
<<<<<<< HEAD
    }, []);
=======
    }, [cleanupDisplayUrl]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
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
=======
        const img = new Image();
        if (src.startsWith("http")) img.crossOrigin = "anonymous";
        img.onload = () => {
            if (cancelled || isClosingRef.current) {
                if (typeof imageFile !== "string") URL.revokeObjectURL(src);
                return;
            }
            loadedImgRef.current = img;
            if (displayUrlRef.current !== src) {
                cleanupDisplayUrl(displayUrlRef.current);
            }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            displayUrlRef.current = src;
            setDisplayUrl(src);
            setManualCorners(detectedCorners || null);
        };
        img.onerror = () => {
<<<<<<< HEAD
            releaseSource();
=======
            if (!cancelled && typeof imageFile !== "string") URL.revokeObjectURL(src);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (!cancelled) {
                console.error("Failed to load image", {
                    src,
                    type: typeof imageFile === "string" ? "url" : imageFile?.type,
                    size: typeof imageFile === "string" ? null : imageFile?.size,
<<<<<<< HEAD
=======
                    crossOrigin: img.crossOrigin,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                });
            }
        };
        img.src = src;

        return () => {
            cancelled = true;
<<<<<<< HEAD
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
=======
            // Only revoke if NOT still used as displayUrl (CornerEditor may still reference it)
            if (typeof imageFile !== "string" && src !== displayUrlRef.current) {
                URL.revokeObjectURL(src);
            }
        };
    }, [imageFile, detectedCorners, cleanupAll, open, cleanupDisplayUrl]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    // Reset on open / cleanup on close
    useEffect(() => {
        if (open) {
            isClosingRef.current = false;
            setIsSaving(false);
<<<<<<< HEAD
            setIsStoring(false);
        } else {
            isClosingRef.current = true;
            operationRef.current++;
=======
        } else {
            isClosingRef.current = true;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            cleanupAll();
        }
    }, [open, cleanupAll]);

    const handleApply = useCallback(async () => {
        if (!manualCorners || manualCorners.length !== 4 || !imageFile || isSaving) return;
        setIsSaving(true);

<<<<<<< HEAD
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
=======
        let srcCtxCanvas: HTMLCanvasElement | null = null;
        let outCanvas: HTMLCanvasElement | null = null;

        // Add timeout for the entire operation
        const timeoutId = setTimeout(() => {
            toast.error("انتهت مهلة القص، يرجى المحاولة مرة أخرى");
            setIsSaving(false);
        }, CROP_TIMEOUT_MS);

        try {
            const isFile = imageFile instanceof File;
            const src = isFile ? URL.createObjectURL(imageFile) : imageFile;
            let img: HTMLImageElement;
            try {
                img = await loadImage(src);
            } finally {
                if (isFile) URL.revokeObjectURL(src);
            }

            // Draw image on canvas
            const workCanvas = acquireCanvas(img.naturalWidth, img.naturalHeight);
            const tmpCtx = workCanvas.getContext("2d")!;
            tmpCtx.drawImage(img, 0, 0);

            // Downscale to prevent OOM - use device-aware size
            const MAX_PROC_DIM = getOptimalProcessingSize(deviceCapabilities);
            const procRatio = Math.min(MAX_PROC_DIM / workCanvas.width, MAX_PROC_DIM / workCanvas.height, 1);
            const sourceW = Math.max(1, Math.round(workCanvas.width * procRatio));
            const sourceH = Math.max(1, Math.round(workCanvas.height * procRatio));

            const scaledCorners = manualCorners.map(c => ({
                x: c.x * procRatio,
                y: c.y * procRatio,
            }));

            // Extract pixel data from rotated+downscaled canvas
            srcCtxCanvas = acquireCanvas(sourceW, sourceH);
            const srcCtx = srcCtxCanvas.getContext("2d", { willReadFrequently: true })!;
            srcCtx.drawImage(workCanvas, 0, 0, sourceW, sourceH);
            const srcData = srcCtx.getImageData(0, 0, sourceW, sourceH);
            releaseCanvas(srcCtxCanvas);
            srcCtxCanvas = null;
            releaseCanvas(workCanvas);

            let finalData: Uint8ClampedArray;
            let finalW: number;
            let finalH: number;

            // Use optimized processor (WebGL with CPU fallback)
            try {
                const result = processImage(srcData.data, sourceW, sourceH, scaledCorners);
                finalData = result.data;
                finalW = result.width;
                finalH = result.height;
            } catch (error) {
                console.error("Processor failed:", error);
                throw new Error("فشل معالجة الصورة");
            }

            outCanvas = acquireCanvas(finalW, finalH);
            const outCtx = outCanvas.getContext("2d")!;
            outCtx.putImageData(new ImageData(finalData as any, finalW, finalH), 0, 0);

            const file = await canvasToFile(outCanvas, isFile ? imageFile.name : "cropped.jpg");
            releaseCanvas(outCanvas);
            outCanvas = null;

            await onCrop(file);
            onClose();
        } catch (error) {
            console.error("Perspective crop error:", error);
            toast.error("فشل قص الصورة");
        } finally {
            clearTimeout(timeoutId);
            // Emergency cleanup
            if (srcCtxCanvas) releaseCanvas(srcCtxCanvas);
            if (outCanvas) releaseCanvas(outCanvas);
            setIsSaving(false);
        }
    }, [manualCorners, imageFile, onCrop, onClose, isSaving]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const handleCancel = useCallback(() => {
        if (!isSaving) onClose();
    }, [isSaving, onClose]);

    return (
        <Dialog open={open} onOpenChange={(v) => !v && handleCancel()}>
<<<<<<< HEAD
            <DialogContent ref={dialogRef} aria-labelledby={titleId} className="p-0 sm:max-w-2xl" dir="rtl" size="lg" clipContent={false}>
                <DialogHeader className="shrink-0 px-3 sm:px-6 py-2 sm:py-4 border-b border-border">
                    <DialogTitle id={titleId} className="text-base sm:text-xl">تعديل حواف المستند</DialogTitle>
=======
            <DialogContent ref={dialogRef} className="p-0 sm:max-w-2xl" dir="rtl" size="lg" clipContent={false}>
                <DialogHeader className="shrink-0 px-3 sm:px-6 py-2 sm:py-4 border-b border-border/50">
                    <DialogTitle className="text-base sm:text-xl">تعديل حواف المستند</DialogTitle>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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

<<<<<<< HEAD
                <div className="shrink-0 px-3 sm:px-6 py-2 sm:py-4 border-t border-border bg-muted/5">
                    <FormDialogFooter
                        saveLabel={isStoring ? "جارٍ الحفظ..." : isSaving ? "جاري القص..." : "تأكيد التعديل"}
=======
                <div className="shrink-0 px-3 sm:px-6 py-2 sm:py-4 border-t border-border/50 bg-muted/5">
                    <FormDialogFooter
                        saveLabel={isSaving ? "جاري..." : "تأكيد التعديل"}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
