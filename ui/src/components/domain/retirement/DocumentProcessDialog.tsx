import { useQueryClient } from "@tanstack/react-query";
import {
    Check,
    Contrast,
    Crop,
    Download,
    Loader2,
    RotateCcw,
    RotateCw,
    Save,
    Sun,
    Undo,
    ZoomIn,
    ZoomOut,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import { documentsApi } from "@/api/documents";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { CornerEditor } from "@/components/ui/CornerEditor";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/Dialog";
import { Slider } from "@/components/ui/Slider";
import { useDocumentBlob } from "@/hooks/useDocumentBlob";
import { DEFAULT_CROP_TIMEOUT_MS, runCropPipeline } from "@/lib/crop-pipeline";
import { getCropBudget } from "@/lib/crop-region";
import { getDeviceCapabilities } from "@/lib/device-capabilities";
import { applySharpen, canvasToFile, type ImageSettings, renderSettingsImage, rotateCorners } from "@/lib/document-image";
import { getDocumentUrl } from "@/lib/utils";

interface DocumentData {
    documentID: number;
    documentTypeID: number;
    documentTypeName: string;
    fileName: string;
    filePath?: string | null;
    notes?: string | null;
    uploadedByUser?: string | null;
    createdAt: string;
}

const DEFAULT_SETTINGS: ImageSettings = {
    brightness: 100,
    contrast: 100,
    sharpness: 0,
    rotation: 0,
};

const IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif"];
/** مهلة رفع الملف — مستقلة عن مهلة المعالجة */
const UPLOAD_TIMEOUT_MS = 60000;

// ─── Canvas Pool ─────────────────────────────────────────
const globalCanvasPool: HTMLCanvasElement[] = [];

function acquireCanvas(w?: number, h?: number): HTMLCanvasElement {
    const c = globalCanvasPool.pop() || window.document.createElement("canvas");
    if (w !== undefined) c.width = w;
    if (h !== undefined) c.height = h;
    return c;
}

function releaseCanvas(c: HTMLCanvasElement) {
    c.width = 0;
    c.height = 0;
    globalCanvasPool.push(c);
}

function isImageDocument(doc: DocumentData | null): boolean {
    if (!doc?.fileName) return false;
    const lower = doc.fileName.toLowerCase();
    return IMAGE_EXTENSIONS.some((ext) => lower.endsWith(ext));
}

// ─── Helpers ─────────────────────────────────────────────

/**
 * تحميل عنصر صورة من blob URL.
 *
 * لا crossOrigin: رابط blob: نفس الأصل، فلا يتلوث الـ canvas عند toBlob —
 * وهو شرط ضروري هنا لأن المعالجة ترسم على canvas ثم تصدّر.
 * الجلب نفسه يمرّ عبر useDocumentBlob (النمط الموحد للمشروع).
 */
function loadImageElement(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = () => reject(new Error("فشل تحميل الصورة"));
        img.src = src;
    });
}

function rotateImage(
    img: HTMLImageElement,
    rotation: 0 | 90 | 180 | 270,
): HTMLCanvasElement {
    const isVertical = rotation === 90 || rotation === 270;
    const w = isVertical ? img.height : img.width;
    const h = isVertical ? img.width : img.height;
    const canvas = acquireCanvas(w, h);
    const ctx = canvas.getContext("2d");
    if (!ctx) throw new Error("فشل إنشاء سياق الرسم للدوران");

    ctx.translate(w / 2, h / 2);
    ctx.rotate((rotation * Math.PI) / 180);
    ctx.drawImage(img, -img.width / 2, -img.height / 2);

    return canvas;
}

// ─── Component ───────────────────────────────────────────

const deviceCapabilities = getDeviceCapabilities();

function renderExportCanvas(img: HTMLImageElement, settings: ImageSettings): HTMLCanvasElement {
    const isVertical = settings.rotation === 90 || settings.rotation === 270;
    const canvas = acquireCanvas(
        isVertical ? img.height : img.width,
        isVertical ? img.width : img.height,
    );
    const ctx = canvas.getContext("2d");
    if (!ctx) {
        releaseCanvas(canvas);
        throw new Error("فشل إنشاء سياق الرسم");
    }
    // التصدير من الأصل بالدقة الكاملة — دقة المعاينة لا تدخل هنا
    renderSettingsImage(ctx, img, settings, 1);
    return canvas;
}

interface DocumentProcessDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    document: DocumentData | null;
    fileId: number;
    onSaved?: () => void;
}

export function DocumentProcessDialog({
    open,
    onOpenChange,
    document: docData,
    fileId,
    onSaved,
}: DocumentProcessDialogProps) {
    const queryClient = useQueryClient();

    const canvasRef = useRef<HTMLCanvasElement>(null);
    const originalImageRef = useRef<HTMLImageElement | null>(null);
    const settingsRef = useRef<ImageSettings>(DEFAULT_SETTINGS);
    // رقم العملية الجارية: انتهاء المهلة يجعل الفحص اللاحق يرفض نتائج العملية القديمة
    const operationRef = useRef(0);
    const abortRef = useRef<AbortController | null>(null);

    const [settings, setSettings] = useState<ImageSettings>(DEFAULT_SETTINGS);
    const [history, setHistory] = useState<ImageSettings[]>([DEFAULT_SETTINGS]);
    const [historyIndex, setHistoryIndex] = useState(0);
    const [zoom, setZoom] = useState(1);
    const [isProcessing, setIsProcessing] = useState(false);

    const [isCropMode, setIsCropMode] = useState(false);
    const [manualCorners, setManualCorners] = useState<{ x: number; y: number }[] | null>(null);

    useEffect(() => {
        settingsRef.current = settings;
    }, [settings]);

    // إعادة تهيئة جلسة التحرير عند تغيير الوثيقة أو فتح النافذة لوثيقة أخرى
    useEffect(() => {
        setSettings(DEFAULT_SETTINGS);
        setHistory([DEFAULT_SETTINGS]);
        setHistoryIndex(0);
        setZoom(1);
        setIsProcessing(false);
        setIsCropMode(false);
        setManualCorners(null);
        settingsRef.current = DEFAULT_SETTINGS;
        originalImageRef.current = null;
    }, [docData?.documentID, open]);

    // الجلب يمرّ عبر useDocumentBlob — النمط الموحد للمشروع لملفات محمية (JWT).
    // blob: نفس الأصل، فلا حاجة لـ crossOrigin ولا تتلوث الـ canvas.
    const { blobUrl: imageBlobUrl, loading: blobLoading } = useDocumentBlob(docData?.filePath, {
        enabled: open,
    });

    const imageUrl = useMemo(() => {
        if (!docData?.filePath) return null;
        return getDocumentUrl(docData.filePath);
    }, [docData?.filePath]);

    useEffect(() => {
        if (!imageBlobUrl || !open) return;

        let cancelled = false;

        void (async () => {
            try {
                const img = await loadImageElement(imageBlobUrl);
                if (cancelled) return;
                originalImageRef.current = img;
                applySettings(settingsRef.current);
            } catch {
                if (cancelled) return;
                toast.error("تعذّر تحميل الصورة، تحقّق من الاتصال أو من صلاحية الجلسة");
            }
        })();

        return () => {
            cancelled = true;
        };
    }, [imageBlobUrl, open]);

    // فشل الجلب: useDocumentBlob يُبقي blobUrl=null و loading=false
    useEffect(() => {
        if (!open || !docData?.filePath || blobLoading) return;
        if (imageBlobUrl) return;
        if (originalImageRef.current) return;
        toast.error("تعذّر تحميل الصورة، تحقّق من الاتصال أو من صلاحية الجلسة");
    }, [open, docData?.filePath, blobLoading, imageBlobUrl]);

    const applySettings = useCallback(
        (newSettings: ImageSettings) => {
            const canvas = canvasRef.current;
            const img = originalImageRef.current;
            if (!canvas || !img) return;

            setIsProcessing(true);

            const ctx = canvas.getContext("2d");
            if (!ctx) {
                setIsProcessing(false);
                return;
            }

            const { previewDim } = getCropBudget(deviceCapabilities);
            const isVertical = newSettings.rotation === 90 || newSettings.rotation === 270;
            const baseW = isVertical ? img.height : img.width;
            const baseH = isVertical ? img.width : img.height;
            // معاينة بدقة منفصلة عن التصدير: حجم شاشة المعاينة لا يمس دقة الحفظ
            const scale = Math.min(1, previewDim / baseW, previewDim / baseH);
            canvas.width = Math.max(1, Math.round(baseW * scale));
            canvas.height = Math.max(1, Math.round(baseH * scale));

            renderSettingsImage(ctx, img, newSettings, scale);

            setIsProcessing(false);
        },
        []
    );

    useEffect(() => {
        if (!isCropMode) {
            applySettings(settings);
        }
    }, [settings, applySettings, isCropMode]);

    const undo = useCallback(() => {
        if (historyIndex > 0) {
            const newIndex = historyIndex - 1;
            setHistoryIndex(newIndex);
            setSettings(history[newIndex]);
        }
    }, [historyIndex, history]);

    const redo = useCallback(() => {
        if (historyIndex < history.length - 1) {
            const newIndex = historyIndex + 1;
            setHistoryIndex(newIndex);
            setSettings(history[newIndex]);
        }
    }, [historyIndex, history]);

    const saveToHistory = useCallback(
        (newSettings: ImageSettings) => {
            const newHistory = history.slice(0, historyIndex + 1);
            newHistory.push(newSettings);
            setHistory(newHistory);
            setHistoryIndex(newHistory.length - 1);
        },
        [history, historyIndex]
    );

    const updateSetting = useCallback(
        <K extends keyof ImageSettings>(key: K, value: ImageSettings[K]) => {
            const newSettings = { ...settings, [key]: value };
            setSettings(newSettings);
            saveToHistory(newSettings);
        },
        [settings, saveToHistory]
    );

    const rotate = useCallback(
        (delta: 90 | -90) => {
            const newRotation = ((settings.rotation + delta + 360) % 360) as 0 | 90 | 180 | 270;
            updateSetting("rotation", newRotation);
        },
        [settings.rotation, updateSetting]
    );

    const enableCropMode = useCallback(() => {
        setIsCropMode(true);
        setManualCorners(null);
    }, []);

    const cancelCrop = useCallback(() => {
        setIsCropMode(false);
        setManualCorners(null);
        applySettings(settings);
    }, [applySettings, settings]);

    const applyCrop = useCallback(async () => {
        if (!manualCorners || manualCorners.length !== 4 || !imageUrl || !docData) return;

        setIsProcessing(true);

        const opId = ++operationRef.current;
        const controller = new AbortController();
        abortRef.current = controller;

        let workCanvas: HTMLCanvasElement | null = null;
        let outCanvas: HTMLCanvasElement | null = null;

        // مهلة المعالجة فقط (WebGL/Worker/CPU) — الرفع خارجها
        const timeoutId = setTimeout(() => {
            // إنهاء فعلي: توقف رقم العملية وإلغاء المعالجة الجارية
            operationRef.current++;
            controller.abort();
            toast.error("انتهت مهلة القص، يرجى المحاولة مرة أخرى");
            setIsProcessing(false);
        }, DEFAULT_CROP_TIMEOUT_MS);

        try {
            // الصورة محمّلة مسبقاً في originalImageRef من blob: — لا جلب ثانٍ
            const img = originalImageRef.current;
            if (!img) throw new Error("الصورة غير محمّلة");
            if (opId !== operationRef.current) return;

            const hasRotation = settings.rotation !== 0;
            const rotatedCorners = hasRotation
                ? rotateCorners(manualCorners, settings.rotation, img.naturalWidth, img.naturalHeight)
                : manualCorners;

            let regionSource: CanvasImageSource;
            let regionSourceW: number;
            let regionSourceH: number;

            if (hasRotation) {
                workCanvas = rotateImage(img, settings.rotation);
                regionSource = workCanvas;
                regionSourceW = workCanvas.width;
                regionSourceH = workCanvas.height;
            } else {
                // بدون دوران: لا تُنشأ نسخة بالحجم الطبيعي — تُستخرج منطقة القص مباشرة من الأصل
                regionSource = img;
                regionSourceW = img.naturalWidth;
                regionSourceH = img.naturalHeight;
            }

            let finalData: Uint8ClampedArray<ArrayBuffer>;
            let finalW: number;
            let finalH: number;

            try {
                const result = await runCropPipeline({
                    source: regionSource,
                    sourceWidth: regionSourceW,
                    sourceHeight: regionSourceH,
                    corners: rotatedCorners,
                    capabilities: deviceCapabilities,
                    signal: controller.signal,
                });
                finalData = result.data;
                finalW = result.width;
                finalH = result.height;
            } catch (error) {
                console.error("Processor failed:", error);
                const message = error instanceof Error ? error.message : "";
                throw new Error(/[\u0600-\u06FF]/.test(message) ? message : "فشل معالجة الصورة");
            }

            if (opId !== operationRef.current) return;

            // انتهت المعالجة: تُلغى المهلة قبل الحفظ والرفع
            clearTimeout(timeoutId);

            outCanvas = acquireCanvas(finalW, finalH);
            const outCtx = outCanvas.getContext("2d");
            if (!outCtx) throw new Error("فشل إنشاء سياق الإخراج");
            outCtx.putImageData(new ImageData(finalData, finalW, finalH), 0, 0);

            const brightness = settings.brightness / 100;
            const contrast = settings.contrast / 100;
            if (brightness !== 1 || contrast !== 1) {
                outCtx.filter = `brightness(${brightness}) contrast(${contrast})`;
                outCtx.drawImage(outCanvas, 0, 0);
                outCtx.filter = "none";
            }

            if (settings.sharpness > 0) {
                applySharpen(outCtx, finalW, finalH, settings.sharpness / 100);
            }

            const file = await canvasToFile(outCanvas, docData.fileName);
            releaseCanvas(outCanvas);
            outCanvas = null;

            if (opId !== operationRef.current) return;

            // استبدال الأصل: تحديث الوثيقة الحالية — المالك والبيانات تبقى كما هي
            const formData = new FormData();
            formData.append("File", file);
            formData.append("DocumentTypeID", String(docData.documentTypeID));
            formData.append("Notes", `صورة مقصوصة من: ${docData.fileName}`);

            // مهلة الرفع منفصلة: الطلب يُلغى فعلياً عبر signal
            const uploadController = new AbortController();
            abortRef.current = uploadController;
            const uploadTimeoutId = setTimeout(() => uploadController.abort(), UPLOAD_TIMEOUT_MS);
            try {
                await documentsApi.update(docData.documentID, formData, uploadController.signal);
            } catch (error) {
                if (uploadController.signal.aborted) {
                    throw new Error("انتهت مهلة رفع الصورة، يرجى المحاولة مرة أخرى");
                }
                throw error;
            } finally {
                clearTimeout(uploadTimeoutId);
            }
            if (opId !== operationRef.current) return;

            toast.success("تم القص والحفظ بنجاح");
            setIsCropMode(false);
            setManualCorners(null);
            await queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
            await queryClient.invalidateQueries({ queryKey: ["document", docData.documentID] });
            onSaved?.();
            onOpenChange(false);
        } catch (error) {
            if (opId !== operationRef.current) return; // نتيجة ملغاة — لا رسالة ولا واجهة
            console.error("Crop error:", error);
            toast.error(error instanceof Error ? error.message : "فشل قص الصورة");
        } finally {
            clearTimeout(timeoutId);
            if (workCanvas) releaseCanvas(workCanvas);
            if (outCanvas) releaseCanvas(outCanvas);
            setIsProcessing(false);
        }
    }, [manualCorners, imageUrl, docData, settings, fileId, queryClient, onSaved, onOpenChange]);

    const handleSave = useCallback(async () => {
        const img = originalImageRef.current;
        if (!img || !docData) return;

        setIsProcessing(true);

        const opId = ++operationRef.current;
        const controller = new AbortController();
        abortRef.current = controller;

        try {
            // إعادة الرسم من الأصل بالدقة الكاملة — دقة المعاينة المصغرة لا تُحفظ
            const exportCanvas = renderExportCanvas(img, settings);
            let file: File;
            try {
                file = await canvasToFile(exportCanvas, docData.fileName);
            } finally {
                releaseCanvas(exportCanvas);
            }
            if (opId !== operationRef.current) return;

            // استبدال الأصل بدل رفع نسخة جديدة — يمنع فقدان المالك والبيانات الوصفية
            const formData = new FormData();
            formData.append("File", file);
            formData.append("DocumentTypeID", String(docData.documentTypeID));
            formData.append("Notes", `صورة معالجة من: ${docData.fileName}`);

            await documentsApi.update(docData.documentID, formData, controller.signal);
            if (opId !== operationRef.current) return;

            toast.success("تم حفظ الصورة المعالجة بنجاح");
            await queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
            await queryClient.invalidateQueries({ queryKey: ["document", docData.documentID] });
            onSaved?.();
            onOpenChange(false);
        } catch (error) {
            if (opId !== operationRef.current) return;
            console.error("Save error:", error);
            toast.error("فشل حفظ الصورة");
        } finally {
            setIsProcessing(false);
        }
    }, [docData, settings, fileId, queryClient, onSaved, onOpenChange]);

    const handleDownload = useCallback(() => {
        const img = originalImageRef.current;
        if (!img) return;

        const link = window.document.createElement("a");
        // الامتداد من صيغة التصدير الفعلية (JPEG) لا من اسم الأصل
        const baseName = (docData?.fileName || "image").replace(/\.[^.]+$/, "");
        link.download = `processed_${baseName}.jpg`;
        const exportCanvas = renderExportCanvas(img, settings);
        try {
            link.href = exportCanvas.toDataURL("image/jpeg", 0.92);
        } finally {
            releaseCanvas(exportCanvas);
        }
        link.click();
    }, [docData?.fileName, settings]);

    const resetSettings = useCallback(() => {
        setSettings(DEFAULT_SETTINGS);
        saveToHistory(DEFAULT_SETTINGS);
    }, [saveToHistory]);

    if (!docData || !isImageDocument(docData)) {
        return (
            <Dialog open={open} onOpenChange={onOpenChange}>
                <DialogContent size="sm">
                    <DialogHeader>
                        <DialogTitle>معالجة الصورة</DialogTitle>
                    </DialogHeader>
                    <div className="py-12 text-center text-muted-foreground">
                        الوثيقة غير موجودة أو ليست صورة
                    </div>
                </DialogContent>
            </Dialog>
        );
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent size="full" className="p-0 overflow-hidden" clipContent={false}>
                <DialogHeader className="shrink-0 px-4 py-3 border-b">
                    <DialogTitle className="flex items-center gap-2 text-base">
                        <span className="font-bold">{docData.documentTypeName}</span>
                        <span className="text-xs text-muted-foreground font-normal">— {docData.fileName}</span>
                    </DialogTitle>
                </DialogHeader>

                <div className="flex-1 flex overflow-hidden" style={{ height: "calc(100dvh - 200px)" }}>
                    {/* Image Area */}
                    <div className="flex-1 overflow-auto bg-muted/20 flex items-center justify-center p-4">
                        {isCropMode ? (
                            <div className="w-full h-full max-w-5xl">
                                <CornerEditor
                                    imageUrl={imageBlobUrl || imageUrl || ""}
                                    rotation={settings.rotation}
                                    corners={manualCorners ?? undefined}
                                    onCornersChange={setManualCorners}
                                />
                            </div>
                        ) : (
                            <div
                                className="relative bg-white rounded-lg overflow-hidden"
                                style={{ transform: `scale(${zoom})`, transformOrigin: "center" }}
                            >
                                <canvas
                                    ref={canvasRef}
                                    className="max-w-full max-h-[70vh]"
                                />
                                {isProcessing && (
                                    <div className="absolute inset-0 bg-background/50 flex items-center justify-center">
                                        <Loader2 className="w-8 h-8 animate-spin text-primary" />
                                    </div>
                                )}
                            </div>
                        )}
                    </div>

                    {/* Sidebar Controls */}
                    <aside className="w-72 border-r bg-card overflow-y-auto shrink-0">
                        <div className="p-3 space-y-3">
                            {/* Action Buttons */}
                            {isCropMode ? (
                                <div className="flex gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={cancelCrop}
                                        disabled={isProcessing}
                                        className="flex-1"
                                    >
                                        إلغاء
                                    </Button>
                                    <Button
                                        size="sm"
                                        onClick={applyCrop}
                                        disabled={!manualCorners || manualCorners.length !== 4 || isProcessing}
                                        className="flex-1"
                                    >
                                        {isProcessing ? (
                                            <Loader2 className="w-4 h-4 animate-spin ml-1" />
                                        ) : (
                                            <Check className="w-4 h-4 ml-1" />
                                        )}
                                        تطبيق القص
                                    </Button>
                                </div>
                            ) : (
                                <div className="flex gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={handleDownload}
                                        className="flex-1 gap-1"
                                    >
                                        <Download className="w-4 h-4" />
                                        تحميل
                                    </Button>
                                    <Button
                                        size="sm"
                                        onClick={handleSave}
                                        disabled={isProcessing}
                                        className="flex-1 gap-1"
                                    >
                                        {isProcessing ? (
                                            <Loader2 className="w-4 h-4 animate-spin" />
                                        ) : (
                                            <Save className="w-4 h-4" />
                                        )}
                                        حفظ
                                    </Button>
                                </div>
                            )}

                            {/* Zoom */}
                            <Card className="p-3">
                                <h3 className="font-bold text-xs mb-2 flex items-center gap-1.5">
                                    <ZoomIn className="w-3.5 h-3.5" />
                                    التكبير
                                </h3>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="outline"
                                        size="icon"
                                        className="h-7 w-7"
                                        onClick={() => setZoom(Math.max(0.25, zoom - 0.25))}
                                        disabled={zoom <= 0.25 || isCropMode}
                                    >
                                        <ZoomOut className="w-3.5 h-3.5" />
                                    </Button>
                                    <span className="flex-1 text-center text-xs font-mono">
                                        {Math.round(zoom * 100)}%
                                    </span>
                                    <Button
                                        variant="outline"
                                        size="icon"
                                        className="h-7 w-7"
                                        onClick={() => setZoom(Math.min(3, zoom + 0.25))}
                                        disabled={zoom >= 3 || isCropMode}
                                    >
                                        <ZoomIn className="w-3.5 h-3.5" />
                                    </Button>
                                </div>
                            </Card>

                            {/* Rotation */}
                            <Card className="p-3">
                                <h3 className="font-bold text-xs mb-2 flex items-center gap-1.5">
                                    <RotateCw className="w-3.5 h-3.5" />
                                    التدوير
                                </h3>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="outline"
                                        className="flex-1 gap-1 text-xs h-8"
                                        onClick={() => rotate(-90)}
                                        disabled={isCropMode}
                                    >
                                        <RotateCcw className="w-3.5 h-3.5" />
                                        يسار
                                    </Button>
                                    <Button
                                        variant="outline"
                                        className="flex-1 gap-1 text-xs h-8"
                                        onClick={() => rotate(90)}
                                        disabled={isCropMode}
                                    >
                                        <RotateCw className="w-3.5 h-3.5" />
                                        يمين
                                    </Button>
                                </div>
                                {settings.rotation !== 0 && (
                                    <p className="text-[10px] text-muted-foreground text-center mt-1">
                                        {settings.rotation}°
                                    </p>
                                )}
                            </Card>

                            {/* Crop */}
                            <Card className={`p-3 ${isCropMode ? "border-primary" : ""}`}>
                                <h3 className="font-bold text-xs mb-2 flex items-center gap-1.5">
                                    <Crop className="w-3.5 h-3.5" />
                                    القص
                                </h3>
                                {isCropMode ? (
                                    <p className="text-[10px] text-muted-foreground">
                                        اسحب النقاط لتعديل حواف المستند
                                    </p>
                                ) : (
                                    <Button
                                        variant="outline"
                                        className="w-full gap-1 text-xs h-8"
                                        onClick={enableCropMode}
                                    >
                                        <Crop className="w-3.5 h-3.5" />
                                        بدء القص
                                    </Button>
                                )}
                            </Card>

                            {/* Image Adjustments */}
                            <Card className="p-3">
                                <h3 className="font-bold text-xs mb-2 flex items-center gap-1.5">
                                    <Sun className="w-3.5 h-3.5" />
                                    تحسين
                                </h3>
                                <div className="space-y-3">
                                    <div>
                                        <div className="flex items-center justify-between mb-1">
                                            <label className="text-[10px] font-medium flex items-center gap-1">
                                                <Sun className="w-3 h-3" />
                                                السطوع
                                            </label>
                                            <span className="text-[10px] font-mono text-muted-foreground">
                                                {settings.brightness}%
                                            </span>
                                        </div>
                                        <Slider
                                            value={[settings.brightness]}
                                            onValueChange={([v]) => updateSetting("brightness", v)}
                                            min={50}
                                            max={150}
                                            step={1}
                                            disabled={isCropMode}
                                        />
                                    </div>

                                    <div>
                                        <div className="flex items-center justify-between mb-1">
                                            <label className="text-[10px] font-medium flex items-center gap-1">
                                                <Contrast className="w-3 h-3" />
                                                التباين
                                            </label>
                                            <span className="text-[10px] font-mono text-muted-foreground">
                                                {settings.contrast}%
                                            </span>
                                        </div>
                                        <Slider
                                            value={[settings.contrast]}
                                            onValueChange={([v]) => updateSetting("contrast", v)}
                                            min={50}
                                            max={150}
                                            step={1}
                                            disabled={isCropMode}
                                        />
                                    </div>

                                    <div>
                                        <div className="flex items-center justify-between mb-1">
                                            <label className="text-[10px] font-medium">الحدة</label>
                                            <span className="text-[10px] font-mono text-muted-foreground">
                                                {settings.sharpness}%
                                            </span>
                                        </div>
                                        <Slider
                                            value={[settings.sharpness]}
                                            onValueChange={([v]) => updateSetting("sharpness", v)}
                                            min={0}
                                            max={100}
                                            step={1}
                                            disabled={isCropMode}
                                        />
                                    </div>
                                </div>
                            </Card>

                            {/* History */}
                            <Card className="p-3">
                                <h3 className="font-bold text-xs mb-2">سجل التعديلات</h3>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={undo}
                                        disabled={historyIndex === 0 || isCropMode}
                                        className="flex-1 gap-1 text-xs h-7"
                                    >
                                        <Undo className="w-3.5 h-3.5" />
                                        تراجع
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={redo}
                                        disabled={historyIndex === history.length - 1 || isCropMode}
                                        className="flex-1 gap-1 text-xs h-7"
                                    >
                                        <RotateCcw className="w-3.5 h-3.5" />
                                        إعادة
                                    </Button>
                                </div>
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={resetSettings}
                                    disabled={isCropMode}
                                    className="w-full mt-2 text-destructive hover:text-destructive text-xs h-7"
                                >
                                    إعادة ضبط
                                </Button>
                            </Card>
                        </div>
                    </aside>
                </div>
            </DialogContent>
        </Dialog>
    );
}
