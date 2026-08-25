import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
    ArrowRight,
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
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";


import { documentsApi } from "@/api/documents";
import { getDocumentUrl } from "@/lib/utils";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { CornerEditor } from "@/components/ui/CornerEditor";
import { Slider } from "@/components/ui/Slider";
import { Heading, Text } from "@/components/ui";
import { CPUProcessor } from "@/lib/cpu-processor";
import { getDeviceCapabilities, getOptimalProcessingSize, shouldUseWebGL } from "@/lib/device-capabilities";
import { WebGLProcessor } from "@/lib/webgl-processor";


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

interface ImageSettings {
    brightness: number;
    contrast: number;
    sharpness: number;
    rotation: 0 | 90 | 180 | 270;
}

const DEFAULT_SETTINGS: ImageSettings = {
    brightness: 100,
    contrast: 100,
    sharpness: 0,
    rotation: 0,
};

const IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif"];
const CROP_TIMEOUT_MS = 30000;

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

function loadImage(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
        const img = new Image();
        if (src.startsWith("http")) img.crossOrigin = "anonymous";
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

function rotateCorners(
    corners: { x: number; y: number }[],
    rotation: 0 | 90 | 180 | 270,
    naturalW: number,
    naturalH: number,
): { x: number; y: number }[] {
    switch (rotation) {
        case 90:  return corners.map(c => ({ x: naturalH - c.y, y: c.x }));
        case 180: return corners.map(c => ({ x: naturalW - c.x, y: naturalH - c.y }));
        case 270: return corners.map(c => ({ x: c.y, y: naturalW - c.x }));
        default:  return corners;
    }
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

    if (webglProcessor) {
        try {
            const result = webglProcessor.process(srcData, srcWidth, srcHeight, corners);
            if (result) return result;
        } catch (error) {
            console.warn("WebGL processing failed, falling back to CPU:", error);
        }
    }

    if (cpuProcessor) {
        return cpuProcessor.process(srcData, srcWidth, srcHeight, corners);
    }

    throw new Error("No processor available");
}

// ─── Component ───────────────────────────────────────────

export function DocumentProcessPage() {
    const { id, docId } = useParams<{ id: string; docId: string }>();
    const fileId = parseInt(id ?? "", 10);
    const documentId = parseInt(docId ?? "", 10);
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const canvasRef = useRef<HTMLCanvasElement>(null);
    const originalImageRef = useRef<HTMLImageElement | null>(null);
    const settingsRef = useRef<ImageSettings>(DEFAULT_SETTINGS);

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

    const { data: docData, isLoading } = useQuery({
        queryKey: ["document", documentId],
        queryFn: () => documentsApi.getById(documentId),
        enabled: documentId > 0 && !isNaN(documentId),
    });

    const imageUrl = useMemo(() => {
        if (!docData?.filePath) return null;
        return getDocumentUrl(docData.filePath);
    }, [docData?.filePath]);

    useEffect(() => {
        if (!imageUrl) return;

        let cancelled = false;
        const img = new Image();
        img.crossOrigin = "anonymous";
        img.onload = () => {
            if (cancelled) return;
            originalImageRef.current = img;
            applySettings(settingsRef.current);
        };
        img.onerror = () => {
            if (cancelled) return;
            toast.error("فشل تحميل الصورة");
        };
        img.src = imageUrl;

        return () => {
            cancelled = true;
            img.onload = null;
            img.onerror = null;
        };
    }, [imageUrl]);

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

            const isVertical = newSettings.rotation === 90 || newSettings.rotation === 270;
            canvas.width = isVertical ? img.height : img.width;
            canvas.height = isVertical ? img.width : img.height;

            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.save();

            ctx.translate(canvas.width / 2, canvas.height / 2);
            ctx.rotate((newSettings.rotation * Math.PI) / 180);
            ctx.drawImage(img, -img.width / 2, -img.height / 2);
            ctx.restore();

            const brightness = newSettings.brightness / 100;
            const contrast = newSettings.contrast / 100;
            ctx.filter = `brightness(${brightness}) contrast(${contrast})`;
            ctx.drawImage(canvas, 0, 0);
            ctx.filter = "none";

            if (newSettings.sharpness > 0) {
                applySharpen(ctx, canvas.width, canvas.height, newSettings.sharpness / 100);
            }

            setIsProcessing(false);
        },
        []
    );

    const applySharpen = (
        ctx: CanvasRenderingContext2D,
        width: number,
        height: number,
        amount: number
    ) => {
        const imageData = ctx.getImageData(0, 0, width, height);
        const data = imageData.data;
        const original = new Uint8ClampedArray(data);

        const kernel = [
            0, -1, 0,
            -1, 5, -1,
            0, -1, 0,
        ];

        for (let y = 1; y < height - 1; y++) {
            for (let x = 1; x < width - 1; x++) {
                for (let c = 0; c < 3; c++) {
                    let sum = 0;
                    for (let ky = -1; ky <= 1; ky++) {
                        for (let kx = -1; kx <= 1; kx++) {
                            const idx = ((y + ky) * width + (x + kx)) * 4 + c;
                            sum += original[idx] * kernel[(ky + 1) * 3 + (kx + 1)];
                        }
                    }
                    const idx = (y * width + x) * 4 + c;
                    data[idx] = Math.min(255, Math.max(0, original[idx] + (sum - original[idx]) * amount));
                }
            }
        }

        ctx.putImageData(imageData, 0, 0);
    };

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

        let workCanvas: HTMLCanvasElement | null = null;
        let srcCtxCanvas: HTMLCanvasElement | null = null;
        let outCanvas: HTMLCanvasElement | null = null;

        const timeoutId = setTimeout(() => {
            toast.error("انتهت مهلة القص، يرجى المحاولة مرة أخرى");
            setIsProcessing(false);
        }, CROP_TIMEOUT_MS);

        try {
            const img = await loadImage(imageUrl);

            const hasRotation = settings.rotation !== 0;
            if (hasRotation) {
                workCanvas = rotateImage(img, settings.rotation);
            } else {
                workCanvas = acquireCanvas(img.naturalWidth, img.naturalHeight);
                const tmpCtx = workCanvas.getContext("2d");
                if (!tmpCtx) throw new Error("فشل إنشاء سياق الرسم");
                tmpCtx.drawImage(img, 0, 0);
            }

            const MAX_PROC_DIM = getOptimalProcessingSize(deviceCapabilities);
            const procRatio = Math.min(MAX_PROC_DIM / workCanvas.width, MAX_PROC_DIM / workCanvas.height, 1);
            const sourceW = Math.max(1, Math.round(workCanvas.width * procRatio));
            const sourceH = Math.max(1, Math.round(workCanvas.height * procRatio));

            const rotatedCorners = hasRotation
                ? rotateCorners(manualCorners, settings.rotation, img.naturalWidth, img.naturalHeight)
                : manualCorners;
            const scaledCorners = rotatedCorners.map(c => ({
                x: c.x * procRatio,
                y: c.y * procRatio,
            }));

            srcCtxCanvas = acquireCanvas(sourceW, sourceH);
            const srcCtx = srcCtxCanvas.getContext("2d", { willReadFrequently: true });
            if (!srcCtx) throw new Error("فشل إنشاء سياق القراءة");
            srcCtx.drawImage(workCanvas, 0, 0, sourceW, sourceH);
            const srcData = srcCtx.getImageData(0, 0, sourceW, sourceH);
            releaseCanvas(srcCtxCanvas);
            srcCtxCanvas = null;
            releaseCanvas(workCanvas);
            workCanvas = null;

            let finalData: Uint8ClampedArray;
            let finalW: number;
            let finalH: number;

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
            const outCtx = outCanvas.getContext("2d");
            if (!outCtx) throw new Error("فشل إنشاء سياق الإخراج");
            outCtx.putImageData(new ImageData(new Uint8ClampedArray(finalData), finalW, finalH), 0, 0);

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

            const formData = new FormData();
            formData.append("File", file);
            formData.append("RetirementFileID", String(fileId));
            formData.append("DocumentTypeID", String(docData.documentTypeID));
            formData.append("Notes", `صورة مقصوصة من: ${docData.fileName}`);

            await documentsApi.upload(formData);

            toast.success("تم القص والحفظ بنجاح");
            setIsCropMode(false);
            setManualCorners(null);
            await queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
            navigate(`/files/${id}?tab=documents`);
        } catch (error) {
            console.error("Crop error:", error);
            toast.error(error instanceof Error ? error.message : "فشل قص الصورة");
        } finally {
            clearTimeout(timeoutId);
            if (workCanvas) releaseCanvas(workCanvas);
            if (srcCtxCanvas) releaseCanvas(srcCtxCanvas);
            if (outCanvas) releaseCanvas(outCanvas);
            setIsProcessing(false);
        }
    }, [manualCorners, imageUrl, docData, settings, fileId, id, navigate, queryClient]);

    const handleSave = useCallback(async () => {
        const canvas = canvasRef.current;
        if (!canvas || !docData) return;

        setIsProcessing(true);

        try {
            const file = await canvasToFile(canvas, docData.fileName);
            const formData = new FormData();
            formData.append("File", file);
            formData.append("RetirementFileID", String(fileId));
            formData.append("DocumentTypeID", String(docData.documentTypeID));
            formData.append("Notes", `صورة معالجة من: ${docData.fileName}`);

            await documentsApi.upload(formData);

            toast.success("تم حفظ الصورة المعالجة بنجاح");
            navigate(`/files/${id}?tab=documents`);
        } catch (error) {
            console.error("Save error:", error);
            toast.error("فشل حفظ الصورة");
        } finally {
            setIsProcessing(false);
        }
    }, [docData, fileId, id, navigate]);

    const handleDownload = useCallback(() => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const link = window.document.createElement("a");
        link.download = `processed_${docData?.fileName || "image.jpg"}`;
        link.href = canvas.toDataURL("image/jpeg", 0.92);
        link.click();
    }, [docData?.fileName]);

    const resetSettings = useCallback(() => {
        setSettings(DEFAULT_SETTINGS);
        saveToHistory(DEFAULT_SETTINGS);
    }, [saveToHistory]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <Loader2 className="w-8 h-8 animate-spin text-primary" />
            </div>
        );
    }

    if (!docData || !isImageDocument(docData)) {
        return (
            <div className="flex flex-col items-center justify-center min-h-screen gap-4">
                <p className="text-muted-foreground">الوثيقة غير موجودة أو ليست صورة</p>
                <Button onClick={() => navigate(`/files/${id}?tab=documents`)}>
                    العودة للوثائق
                </Button>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-background flex flex-col">
            <header className="border-b bg-card sticky top-0 z-10">
                <div className="container mx-auto px-4 py-3 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => navigate(`/files/${id}?tab=documents`)}
                            className="rounded-full"
                        >
                            <ArrowRight className="w-5 h-5" />
                        </Button>
                        <div>
                            <Heading level="h1" className="font-bold text-lg">{docData.documentTypeName}</Heading>
                            <Text variant="caption">{docData.fileName}</Text>
                        </div>
                    </div>
                    <div className="flex items-center gap-2">
                        {isCropMode ? (
                            <>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={cancelCrop}
                                    disabled={isProcessing}
                                    className="gap-2"
                                >
                                    إلغاء
                                </Button>
                                <Button
                                    size="sm"
                                    onClick={applyCrop}
                                    disabled={!manualCorners || manualCorners.length !== 4 || isProcessing}
                                    className="gap-2"
                                >
                                    {isProcessing ? (
                                        <Loader2 className="w-4 h-4 animate-spin" />
                                    ) : (
                                        <Check className="w-4 h-4" />
                                    )}
                                    تطبيق القص
                                </Button>
                            </>
                        ) : (
                            <>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={handleDownload}
                                    className="gap-2"
                                >
                                    <Download className="w-4 h-4" />
                                    تحميل
                                </Button>
                                <Button
                                    size="sm"
                                    onClick={handleSave}
                                    disabled={isProcessing}
                                    className="gap-2"
                                >
                                    {isProcessing ? (
                                        <Loader2 className="w-4 h-4 animate-spin" />
                                    ) : (
                                        <Save className="w-4 h-4" />
                                    )}
                                    حفظ كنسخة جديدة
                                </Button>
                            </>
                        )}
                    </div>
                </div>
            </header>

            <div className="flex-1 flex overflow-hidden">
                <div className="flex-1 overflow-auto bg-muted/20 flex items-center justify-center p-4">
                    {isCropMode ? (
                        <div className="w-full h-full max-w-5xl">
                            <CornerEditor
                                imageUrl={imageUrl || ""}
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

                <aside className="hidden md:block md:w-80 border-r bg-card overflow-y-auto flex-shrink-0">
                    <div className="p-4 space-y-6">
                        <Card className="p-4">
                            <Heading level="h3" className="font-bold text-sm mb-3 flex items-center gap-2">
                                <ZoomIn className="w-4 h-4" />
                                التكبير والتصغير
                            </Heading>
                            <div className="flex items-center gap-2">
                                <Button
                                    variant="outline"
                                    size="icon"
                                    onClick={() => setZoom(Math.max(0.25, zoom - 0.25))}
                                    disabled={zoom <= 0.25 || isCropMode}
                                >
                                    <ZoomOut className="w-4 h-4" />
                                </Button>
                                <span className="flex-1 text-center text-sm font-mono">
                                    {Math.round(zoom * 100)}%
                                </span>
                                <Button
                                    variant="outline"
                                    size="icon"
                                    onClick={() => setZoom(Math.min(3, zoom + 0.25))}
                                    disabled={zoom >= 3 || isCropMode}
                                >
                                    <ZoomIn className="w-4 h-4" />
                                </Button>
                            </div>
                        </Card>

                        <Card className="p-4">
                            <Heading level="h3" className="font-bold text-sm mb-3 flex items-center gap-2">
                                <RotateCw className="w-4 h-4" />
                                التدوير
                            </Heading>
                            <div className="flex items-center gap-2">
                                <Button
                                    variant="outline"
                                    className="flex-1 gap-2"
                                    onClick={() => rotate(-90)}
                                    disabled={isCropMode}
                                >
                                    <RotateCcw className="w-4 h-4" />
                                    يسار 90°
                                </Button>
                                <Button
                                    variant="outline"
                                    className="flex-1 gap-2"
                                    onClick={() => rotate(90)}
                                    disabled={isCropMode}
                                >
                                    <RotateCw className="w-4 h-4" />
                                    يمين 90°
                                </Button>
                            </div>
                            {settings.rotation !== 0 && (
                                <Text variant="caption" className="text-center mt-2">
                                    الزاوية الحالية: {settings.rotation}°
                                </Text>
                            )}
                        </Card>

                        <Card className={`p-4 ${isCropMode ? "border-primary" : ""}`}>
                            <Heading level="h3" className="font-bold text-sm mb-3 flex items-center gap-2">
                                <Crop className="w-4 h-4" />
                                القص المتقدم
                            </Heading>
                            {isCropMode ? (
                                <div className="space-y-2">
                                    <Text variant="caption">
                                        اسحب النقاط الملونة لتعديل حواف المستند
                                    </Text>
                                    <Text variant="caption">
                                        يمكنك تحديد أي شكل رباعي غير منتظم
                                    </Text>
                                </div>
                            ) : (
                                <>
                                    <Text variant="caption" className="mb-3">
                                        قص متقدم مع تعديل الحواف والمنظور
                                    </Text>
                                    <Button
                                        variant="outline"
                                        className="w-full gap-2"
                                        onClick={enableCropMode}
                                    >
                                        <Crop className="w-4 h-4" />
                                        بدء القص
                                    </Button>
                                </>
                            )}
                        </Card>

                        <Card className="p-4">
                            <Heading level="h3" className="font-bold text-sm mb-3 flex items-center gap-2">
                                <Sun className="w-4 h-4" />
                                تحسين الصورة
                            </Heading>
                            <div className="space-y-4">
                                <div>
                                    <div className="flex items-center justify-between mb-2">
                                        <Text variant="caption" className="flex items-center gap-1">
                                            <Sun className="w-3 h-3" />
                                            السطوع
                                        </Text>
                                        <span className="text-xs font-mono text-muted-foreground">
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
                                    <div className="flex items-center justify-between mb-2">
                                        <Text variant="caption" className="flex items-center gap-1">
                                            <Contrast className="w-3 h-3" />
                                            التباين
                                        </Text>
                                        <span className="text-xs font-mono text-muted-foreground">
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
                                    <div className="flex items-center justify-between mb-2">
                                        <Text variant="caption">الحدة</Text>
                                        <span className="text-xs font-mono text-muted-foreground">
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

                        <Card className="p-4">
                            <Heading level="h3" className="font-bold text-sm mb-3">سجل التعديلات</Heading>
                            <div className="flex items-center gap-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={undo}
                                    disabled={historyIndex === 0 || isCropMode}
                                    className="flex-1 gap-1"
                                >
                                    <Undo className="w-4 h-4" />
                                    تراجع
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={redo}
                                    disabled={historyIndex === history.length - 1 || isCropMode}
                                    className="flex-1 gap-1"
                                >
                                    <RotateCcw className="w-4 h-4" />
                                    إعادة
                                </Button>
                            </div>
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={resetSettings}
                                disabled={isCropMode}
                                className="w-full mt-2 text-destructive hover:text-destructive"
                            >
                                إعادة ضبط الكل
                            </Button>
                        </Card>
                    </div>
                </aside>
            </div>
        </div>
    );
}