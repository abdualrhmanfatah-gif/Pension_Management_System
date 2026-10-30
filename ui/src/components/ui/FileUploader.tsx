import { Camera, CheckCircle2, Crop as CropIcon,FileText, Loader2, ScanLine, Upload, User, X } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/Button";
<<<<<<< HEAD
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { Dialog, DialogContent, DialogDescription,DialogHeader, DialogTitle } from "@/components/ui/Dialog";
import { combineScannedPages } from "@/lib/scanned-document";
=======
import { Dialog, DialogContent, DialogDescription,DialogHeader, DialogTitle } from "@/components/ui/Dialog";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { cn } from "@/lib/utils";

import { ImageCropperDialog } from "./ImageCropperDialog";

// ─── Types ─────────────────────────────────────────────

export interface FileUploaderProps {
    /** قيمة الملف الحالي */
    value?: File | string | null;
<<<<<<< HEAD
    /** callback عند تغيير الملف؛ يمكن إرجاع false عند رفض إضافته */
    onChange?: (file: File | null) => void | boolean;
=======
    /** callback عند تغيير الملف */
    onChange?: (file: File | null) => void;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    /** Label للمكون */
    label?: string;
    /** هل الحقل إلزامي */
    required?: boolean;
    /** أنواع الملفات المقبولة (accept attribute) */
    accept?: string;
    /** رسالة خطأ */
    error?: string;
    /** نص مساعد */
    hint?: string;
    /** هل يتم عرض معاينة الصورة */
    showPreview?: boolean;
    /** هل يتم تفعيل الكاميرا */
    enableCamera?: boolean;
<<<<<<< HEAD
    /** سؤال المستخدم عن تصوير مرفق آخر بعد قبول الصورة */
    allowContinuousCapture?: boolean;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    /** هل يتم تفعيل الماسح الضوئي */
    enableScanner?: boolean;
    /** إعدادات الماسح الضوئي */
    scannerConfig?: {
        url?: string;
        dpi?: number;
        width?: number; // inches
        height?: number; // inches
        adf?: boolean;  // automatic document feeder
    };
    /** حجم المكون */
    size?: "sm" | "md" | "lg";
    /** className إضافي */
    className?: string;
    /** هل يتم تعطيل المكون */
    disabled?: boolean;
    /** هل يتم عرض حالة التحميل */
    uploading?: boolean;
    /** أقصى حجم مسموح للملف بالميغابايت */
    maxSizeMB?: number;
}

interface ScannerFile {
    data: string;
    type: string;
    name: string;
}

// ─── Scanner Agent Config ─────────────────────────────

const DEFAULT_SCANNER_URL = "http://localhost:5000";
const DEFAULT_SCANNER_CONFIG = {
    url: DEFAULT_SCANNER_URL,
    dpi: 200,
<<<<<<< HEAD
    // م45: A4 هو المقاس القياسي للمستندات، لا Letter (8.5×11). الوحدة: إنش (inches).
    width: 8.27,
    height: 11.69,
};

// ─── Helper: Probe Scanner Availability ───────────────

type ScannerProblem = "agent-offline" | "no-device" | "wia-error" | "origin-blocked";

const SCANNER_PROBLEM_MESSAGES: Record<ScannerProblem, string> = {
    "agent-offline": "تعذر الوصول لبرنامج Scanner Agent — تأكد من تشغيله.",
    "no-device": "Scanner Agent يعمل لكن لا يوجد جهاز موصول.",
    "wia-error": "حدث خطأ في الوصول لجهاز المسح (WIA).",
    "origin-blocked": "موقع المستخدم غير مصرح به في إعدادات Scanner Agent.",
};

async function checkScannerAvailable(scannerUrl: string): Promise<{ available: boolean; problem?: ScannerProblem }> {
=======
    width: 8.5,
    height: 11,
};

// ─── Helper: Check Scanner Availability ────────────────

async function checkScannerAvailable(scannerUrl: string): Promise<boolean> {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    try {
        const r = await fetch(`${scannerUrl}/scanners`, {
            signal: AbortSignal.timeout(1500)
        });
<<<<<<< HEAD
        // م49: حالة الاستجابة والجسم يحددان السبب — ليست مجرد r.ok.
        if (r.status === 403) return { available: false, problem: "origin-blocked" };
        if (!r.ok) return { available: false, problem: "wia-error" };
        const data: unknown = await r.json();
        if (!Array.isArray(data)) return { available: false, problem: "wia-error" };
        if (data.length === 0) return { available: false, problem: "no-device" };
        return { available: true };
    } catch {
        return { available: false, problem: "agent-offline" };
=======
        return r.ok;
    } catch {
        return false;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }
}

// ─── Helper: Detect mobile device ──────────────────────

function isMobileDevice(): boolean {
    if (typeof navigator === "undefined") return false;
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
        || ("ontouchstart" in window && navigator.maxTouchPoints > 0);
}

// ─── Helper: Validate file against accept string ───────

function isFileAccepted(file: File, accept: string): boolean {
    if (!accept || accept === "*") return true;
    const fileExtension = "." + file.name.split('.').pop()?.toLowerCase();
    const acceptedTypes = accept.split(',').map(t => t.trim().toLowerCase());
    return acceptedTypes.some(type => {
        if (type.startsWith('.')) return type === fileExtension;
        if (type.endsWith('/*')) return file.type.startsWith(type.replace('/*', ''));
        return type === file.type;
    });
}

// ─── Helper: Compress image if it exceeds max size ─────

async function compressImage(file: File, maxSizeMB: number): Promise<File> {
    // Only compress images
    if (!file.type.startsWith('image/') || file.type === 'image/gif') return file;

    const maxBytes = maxSizeMB * 1024 * 1024;

    // If already under limit, return as-is
    if (file.size <= maxBytes) return file;

<<<<<<< HEAD
    // Determine target format (keep original if supported by canvas toBlob, else fallback to jpeg)
    const targetType = (file.type === 'image/png' || file.type === 'image/webp') ? file.type : 'image/jpeg';
    const targetExt = targetType === 'image/png' ? '.png' : targetType === 'image/webp' ? '.webp' : '.jpg';

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    return new Promise<File>((resolve) => {
        const img = new Image();
        const url = URL.createObjectURL(file);

        img.onload = () => {
            URL.revokeObjectURL(url);

            // Dimension tiers to try (progressively smaller)
            const dimensionTiers = [2048, 1600, 1280, 1024, 800];
<<<<<<< HEAD
            
            // Reduced quality levels for jpeg/webp. PNG ignores quality so it only gets 1 level.
            const qualityLevels = targetType === 'image/png' ? [1.0] : [0.8, 0.5, 0.2];

            const tryCompress = (dimIdx: number, qualIdx: number, cachedCanvas?: HTMLCanvasElement) => {
                let currentCanvas = cachedCanvas;
                
                // Create and draw canvas only once per dimension
                if (!currentCanvas) {
                    const maxDim = dimensionTiers[dimIdx];
                    let { width, height } = img;

                    // Scale down to fit within maxDim
                    if (width > maxDim || height > maxDim) {
                        const ratio = Math.min(maxDim / width, maxDim / height);
                        width = Math.round(width * ratio);
                        height = Math.round(height * ratio);
                    }

                    currentCanvas = document.createElement('canvas');
                    currentCanvas.width = width;
                    currentCanvas.height = height;
                    const ctx = currentCanvas.getContext('2d')!;
                    ctx.drawImage(img, 0, 0, width, height);
                }

                const quality = qualityLevels[qualIdx];

                currentCanvas.toBlob(
=======
            // Quality levels to try at each dimension tier
            const qualityLevels = [0.8, 0.65, 0.5, 0.35, 0.2];

            const tryCompress = (dimIdx: number, qualIdx: number) => {
                const maxDim = dimensionTiers[dimIdx];
                let { width, height } = img;

                // Scale down to fit within maxDim
                if (width > maxDim || height > maxDim) {
                    const ratio = Math.min(maxDim / width, maxDim / height);
                    width = Math.round(width * ratio);
                    height = Math.round(height * ratio);
                }

                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d')!;
                ctx.drawImage(img, 0, 0, width, height);

                const quality = qualityLevels[qualIdx];

                canvas.toBlob(
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    (blob) => {
                        if (!blob) { resolve(file); return; }

                        if (blob.size <= maxBytes) {
                            // Success — fits under the limit
                            const compressed = new File(
                                [blob],
<<<<<<< HEAD
                                file.name.replace(/\.[^.]+$/, targetExt),
                                { type: targetType }
                            );
                            resolve(compressed);
                        } else if (qualIdx < qualityLevels.length - 1) {
                            // Try lower quality with SAME canvas
                            tryCompress(dimIdx, qualIdx + 1, currentCanvas);
                        } else if (dimIdx < dimensionTiers.length - 1) {
                            // Move to smaller dimension, reset quality and clear canvas
=======
                                file.name.replace(/\.[^.]+$/, '.jpg'),
                                { type: 'image/jpeg' }
                            );
                            resolve(compressed);
                        } else if (qualIdx < qualityLevels.length - 1) {
                            // Try lower quality at same dimension
                            tryCompress(dimIdx, qualIdx + 1);
                        } else if (dimIdx < dimensionTiers.length - 1) {
                            // Move to smaller dimension, reset quality
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            tryCompress(dimIdx + 1, 0);
                        } else {
                            // All options exhausted, return best effort
                            const compressed = new File(
                                [blob],
<<<<<<< HEAD
                                file.name.replace(/\.[^.]+$/, targetExt),
                                { type: targetType }
=======
                                file.name.replace(/\.[^.]+$/, '.jpg'),
                                { type: 'image/jpeg' }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            );
                            resolve(compressed);
                        }
                    },
<<<<<<< HEAD
                    targetType,
=======
                    'image/jpeg',
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    quality
                );
            };

<<<<<<< HEAD
            // Start with first tier that's smaller than or equal to the original
            const startDimIdx = Math.max(0, dimensionTiers.findIndex(d => d <= Math.max(img.width, img.height)));
            tryCompress(startDimIdx, 0);
=======
            // Start with first tier that's smaller than the original
            const startDimIdx = dimensionTiers.findIndex(
                d => d < Math.max(img.width, img.height)
            );
            tryCompress(Math.max(0, startDimIdx), 0);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        };

        img.onerror = () => {
            URL.revokeObjectURL(url);
            console.error("Failed to compress image", { fileName: file.name, fileType: file.type, fileSize: file.size });
            resolve(file); // fallback: return original
        };

        img.src = url;
    });
}

// ─── Main Component ────────────────────────────────────

export function FileUploader({
    value,
    onChange,
    label,
    required = false,
    accept = "image/*",
    error,
    hint,
    showPreview = true,
    enableCamera = true,
<<<<<<< HEAD
    allowContinuousCapture = false,
    enableScanner = true,
    scannerConfig: userScannerConfig,
=======
    enableScanner = true,
    scannerConfig = DEFAULT_SCANNER_CONFIG,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    size = "md",
    className,
    disabled = false,
    uploading = false,
    maxSizeMB = 10,
}: FileUploaderProps) {
    const fileInputRef = useRef<HTMLInputElement>(null);
    const cameraInputRef = useRef<HTMLInputElement>(null);
    const videoRef = useRef<HTMLVideoElement>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    // Detect mobile once
<<<<<<< HEAD
    // Merge scanner config properties with defaults
    const scannerConfig = useMemo(() => {
        return { ...DEFAULT_SCANNER_CONFIG, ...userScannerConfig };
    }, [userScannerConfig]);

    // Detect mobile once
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const isMobile = useMemo(() => isMobileDevice(), []);

    // Camera state
    const [cameraOpen, setCameraOpen] = useState(false);
<<<<<<< HEAD
    const [continueCaptureOpen, setContinueCaptureOpen] = useState(false);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const [previewOpen, setPreviewOpen] = useState(false);
    const [cropOpen, setCropOpen] = useState(false);
    const [cameraStream, setCameraStream] = useState<MediaStream | null>(null);
    const [cameraError, setCameraError] = useState<string>("");
    const [videoReady, setVideoReady] = useState(false);

    // Pending file from camera capture waiting to be cropped
    const [pendingCropFile, setPendingCropFile] = useState<File | null>(null);
    const cropCompletedRef = useRef(false);

    // Scanner state
    const [scannerAvailable, setScannerAvailable] = useState(false);
<<<<<<< HEAD
    const [scannerProblem, setScannerProblem] = useState<ScannerProblem | null>(null);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const [scanning, setScanning] = useState(false);

    // Check scanner availability on mount
    useEffect(() => {
        if (enableScanner) {
            checkScannerAvailable(scannerConfig.url || DEFAULT_SCANNER_URL)
<<<<<<< HEAD
                .then((res) => {
                    setScannerAvailable(res.available);
                    setScannerProblem(res.problem ?? null);
                })
=======
                .then(setScannerAvailable)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                .catch(() => setScannerAvailable(false));
        }
    }, [enableScanner, scannerConfig.url]);

    // ─── File Input Handler ────────────────────────────

    const handleFileSelect = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        let file = e.target.files?.[0];
        if (file) {
            // Validate Extension / MIME Type
            if (accept && !isFileAccepted(file, accept)) {
                toast.error("نوع الملف غير مدعوم أو غير مسموح به");
                if (fileInputRef.current) fileInputRef.current.value = "";
                return;
            }

            // Auto-compress images that exceed the size limit
            if (maxSizeMB && file.type.startsWith('image/') && file.size > maxSizeMB * 1024 * 1024) {
                file = await compressImage(file, maxSizeMB);
            }

            // Final size check (for non-image files or if compression wasn't enough)
            if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                toast.error(`حجم الملف يجب أن لا يتجاوز ${maxSizeMB} ميغابايت`);
                if (fileInputRef.current) fileInputRef.current.value = "";
                return;
            }

            onChange?.(file);
        }
        // Reset input to allow selecting same file again
        if (fileInputRef.current) {
            fileInputRef.current.value = "";
        }
    }, [onChange, maxSizeMB, accept]);

    // ─── Mobile Camera Handler (uses native <input capture>) ──

    const handleMobileCameraCapture = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        let file = e.target.files?.[0];
        if (file) {
            if (accept && !isFileAccepted(file, accept)) {
                toast.error("صيغة الملف غير مدعومة");
                if (cameraInputRef.current) cameraInputRef.current.value = "";
                return;
            }

            // Auto-compress camera images (mobile cameras produce very large files)
            if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                file = await compressImage(file, maxSizeMB);
            }

            // Final size check
            if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                toast.error(`حجم الملف يجب أن لا يتجاوز ${maxSizeMB} ميغابايت حتى بعد الضغط`);
                return;
            }
            // فتح crop يدوي مباشر بدون auto-scan
<<<<<<< HEAD
            cropCompletedRef.current = false;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            setPendingCropFile(file);
            setCropOpen(true);
        }
        // Reset input
        if (cameraInputRef.current) {
            cameraInputRef.current.value = "";
        }
    }, [maxSizeMB, accept]);

    const handleOpenMobileCamera = useCallback(() => {
        cameraInputRef.current?.click();
    }, []);

<<<<<<< HEAD
    // ─── م40: فحص موحد على كل نتيجة نهائية (رفع/كاميرا/ماسح/قص) ──
    // يرمي خطأ برسالة عربية يعرضها ImageCropperDialog ويُبقي النافذة مفتوحة
    const validateFinalFile = useCallback(async (file: File): Promise<File> => {
        if (accept && !isFileAccepted(file, accept)) {
            throw new Error("نوع الملف الناتج غير مدعوم أو غير مسموح به");
        }
        let out = file;
        if (maxSizeMB && out.type.startsWith("image/") && out.size > maxSizeMB * 1024 * 1024) {
            out = await compressImage(out, maxSizeMB);
        }
        if (maxSizeMB && out.size > maxSizeMB * 1024 * 1024) {
            throw new Error(`حجم الملف يجب أن لا يتجاوز ${maxSizeMB} ميغابايت`);
        }
        return out;
    }, [accept, maxSizeMB]);

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    // ─── Camera Handlers ────────────────────────────────

    const handleOpenCamera = useCallback(() => {
        if (isMobile) {
            cameraInputRef.current?.click();
        } else {
            setCameraOpen(true);
        }
    }, [isMobile]);

    const handleCloseCamera = useCallback(() => {
        if (cameraStream) {
            cameraStream.getTracks().forEach(track => track.stop());
            setCameraStream(null);
        }
        setCameraOpen(false);
        setCameraError("");
        setVideoReady(false);
    }, [cameraStream]);

    const capturePhoto = useCallback(() => {
        if (videoRef.current && canvasRef.current && videoReady) {
            const video = videoRef.current;
            const canvas = canvasRef.current;
            const context = canvas.getContext("2d");

            if (context && video.videoWidth > 0 && video.videoHeight > 0) {
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                context.drawImage(video, 0, 0, canvas.width, canvas.height);

                canvas.toBlob((blob) => {
                    if (blob) {
                        const file = new File(
                            [blob],
                            `capture_${Date.now()}.jpg`,
                            { type: "image/jpeg" }
                        );
                        handleCloseCamera();
                        // فتح crop يدوي مباشر بدون auto-scan
<<<<<<< HEAD
                        cropCompletedRef.current = false;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        setPendingCropFile(file);
                        setCropOpen(true);
                    }
                }, "image/jpeg", 0.9);
            }
        }
<<<<<<< HEAD
    }, [handleCloseCamera, videoReady, validateFinalFile]);
=======
    }, [handleCloseCamera, videoReady]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    useEffect(() => {
        if (!cameraOpen) {
            return;
        }

        let mounted = true;
        let stream: MediaStream | null = null;
        const currentVideoRef = videoRef.current;

        const initCamera = async () => {
            try {
                setCameraError("");
                setVideoReady(false);
                const mediaStream = await navigator.mediaDevices.getUserMedia({
                    video: { facingMode: "environment" }
                });

                if (!mounted) {
                    mediaStream.getTracks().forEach(track => track.stop());
                    return;
                }

                stream = mediaStream;
                setCameraStream(mediaStream);
                if (videoRef.current) {
                    videoRef.current.srcObject = mediaStream;

                    // Wait for video to be ready
                    videoRef.current.onloadedmetadata = () => {
                        if (mounted && videoRef.current) {
                            setVideoReady(true);
                        }
                    };
                }
            } catch (err: unknown) {
                console.error("Camera Init Error:", err);
                if (mounted) {
                    const name = err instanceof Error ? err.name : '';
                    const errorMsg = name === 'NotAllowedError' || name === 'NotFoundError'
                        ? "تم رفض صلاحية استخدام الكاميرا." 
                        : "تعذر الوصول إلى الكاميرا أو قد تكون غير متصلة.";
                    setCameraError(errorMsg);
                }
            }
        };

        initCamera();

        return () => {
            mounted = false;
            if (stream) {
                stream.getTracks().forEach(track => track.stop());
            }
            setCameraStream(null);
            setVideoReady(false);
            if (currentVideoRef) {
                currentVideoRef.srcObject = null;
            }
        };
    }, [cameraOpen]);

    // ─── Scanner Handler ────────────────────────────────

    const handleScan = useCallback(async (options: { useAdf?: boolean, isPassport?: boolean } = {}) => {
<<<<<<< HEAD
        const { isPassport = false } = options;
        setScanning(true);
        try {
            // Check scanner availability first
            const probe = await checkScannerAvailable(scannerConfig.url || DEFAULT_SCANNER_URL);
            if (!probe.available) {
                toast.error("الماسح الضوئي غير متاح", {
                    description: probe.problem
                        ? SCANNER_PROBLEM_MESSAGES[probe.problem]
                        : "تأكد من تشغيل برنامج 'Scanner Agent' وتوصيل الماسح الضوئي."
                });
                setScannerAvailable(false);
                setScannerProblem(probe.problem ?? null);
=======
        const { useAdf = false, isPassport = false } = options;
        setScanning(true);
        try {
            // Check scanner availability first
            const available = await checkScannerAvailable(scannerConfig.url || DEFAULT_SCANNER_URL);
            if (!available) {
                toast.error("الماسح الضوئي غير متاح", {
                    description: "تأكد من تشغيل برنامج 'Scanner Agent' وتوصيل الماسح الضوئي."
                });
                setScannerAvailable(false);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                setScanning(false);
                return;
            }

            setScannerAvailable(true);
<<<<<<< HEAD
            setScannerProblem(null);

            const isAdf = options.useAdf ?? scannerConfig.adf ?? false;
            const adfParam = isAdf ? "&adf=true" : "";

            // عقد الوحدات: كل الأبعاد والإزاحات هنا بالإنش (inches)، والـ backend يحوّلها لوحدات WIA.
            // م45: مقاس المستند الافتراضي A4 (من DEFAULT_SCANNER_CONFIG).
            // م46: مقاس الصورة الشخصية 4×6 سم = 1.575×2.362 إنش، والإزاحة 0.1 إنش تتجاهل حواف الزجاج الفارغة.
            const scanWidth = isPassport ? 1.575 : scannerConfig.width;
            const scanHeight = isPassport ? 2.362 : scannerConfig.height;
=======

            const adfParam = useAdf || scannerConfig.adf ? "&adf=true" : "";

            // 4cm = 1.575 inches, 6cm = 2.362 inches.
            // We use offsets to ignore the empty glass borders (margins) from all sides.
            const scanWidth = isPassport ? 1.35 : scannerConfig.width;
            const scanHeight = isPassport ? 1.75 : scannerConfig.height;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            const offsetX = isPassport ? 0.1 : 0;
            const offsetY = isPassport ? 0.1 : 0;

            let url = `${scannerConfig.url || DEFAULT_SCANNER_URL}/scan?dpi=${scannerConfig.dpi || 200}${adfParam}`;
            if (scanWidth) url += `&width=${scanWidth}`;
            if (scanHeight) url += `&height=${scanHeight}`;
            if (offsetX) url += `&offsetX=${offsetX}`;
            if (offsetY) url += `&offsetY=${offsetY}`;

<<<<<<< HEAD
            const r = await fetch(url, { method: "POST", signal: AbortSignal.timeout(60000) });

            // م49: كل حالة له رسالة حقيقية — إلغاء المستخدم (204) منفصل عن الأعطال.
            if (r.status === 204) {
=======
            const r = await fetch(url, { method: "POST" });

            if (r.status === 204 || !r.ok) {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                toast.warning("تم إلغاء المسح الضوئي", {
                    description: "لم يتم استلام أي صورة من الماسح الضوئي."
                });
                return;
            }
<<<<<<< HEAD
            if (r.status === 429) {
                toast.error("جهاز المسح مشغول بطلب آخر", {
                    description: "انتظر انتهاء المسح الجاري ثم أعد المحاولة."
                });
                return;
            }
            if (r.status === 504) {
                toast.error("انتهت مهلة المسح", {
                    description: "لم يستجب الجهاز في الوقت المحدد؛ تحقق من حالة الماسح الضوئي."
                });
                return;
            }
            if (r.status === 403) {
                toast.error("الموقع غير مصرح به", {
                    description: "أضف عنوان الموقع إلى SCANNER_ALLOWED_ORIGINS في إعدادات Scanner Agent."
                });
                return;
            }
            if (!r.ok) {
                toast.error("فشل المسح الضوئي", {
                    description: `أعاد برنامج المسح خطأ (${r.status}).`
                });
                return;
            }

            const contentType = r.headers.get("content-type");
            let files: File[];

            if (contentType?.includes("application/json")) {
                const json = await r.json();
                let pages: ScannerFile[] | null = null;

                if (Array.isArray(json)) {
                    pages = json; // توافق مع إصدارات Scanner Agent القديمة
                } else if (json && typeof json === "object" && Array.isArray((json as { pages?: unknown }).pages)) {
                    const batch = json as { pages: ScannerFile[]; complete?: boolean; stopReason?: string; pageCount?: number };
                    // م43: دفعة جزئية لا تُعتمد أبدًا إلا بقرار صريح من المستخدم.
                    if (batch.complete === false) {
                        const reason = typeof batch.stopReason === "string" ? batch.stopReason : "";
                        const detail = reason.startsWith("error")
                            ? `خطأ أثناء النقل: ${reason.slice(7)}`
                            : reason === "max-pages"
                                ? `وصلت للحد الأقصى من الصفحات (${batch.pageCount ?? "?"})`
                                : reason === "max-bytes"
                                    ? "تجاوز الحد الأقصى لحجم المسح"
                                    : reason || "غير معروف";
                        const accepted = window.confirm(
                            `انتهى المسح قبل اكتمال الدفعة: ${detail}.\nتم مسح ${batch.pageCount ?? batch.pages.length} صفحة فقط. هل تريد اعتماد هذه الصفحات؟`
                        );
                        if (!accepted) {
                            toast.warning("تم إلغاء المسح الضوئي", {
                                description: "لم يتم اعتماد أي صورة من الدفعة الجزئية."
                            });
                            return;
                        }
                    }
                    pages = batch.pages;
                }

                if (!pages || pages.length === 0) {
                    toast.warning("تم إلغاء المسح الضوئي", {
                        description: "لم يتم استلام أي صورة من الماسح الضوئي."
                    });
                    return;
                }

                files = pages.map((j: ScannerFile) => {
                    if (!j || typeof j.data !== 'string' || typeof j.type !== 'string' || typeof j.name !== 'string') {
                        throw new Error('استجابة الماسح غير صالحة');
                    }
=======

            const contentType = r.headers.get("content-type");

            if (contentType?.includes("application/json")) {
                const json = await r.json();
                if (!Array.isArray(json) || json.length === 0) {
                    toast.warning("تم إلغاء المسح الضوئي", {
                    description: "لم يتم استلام أي صورة من الماسح الضوئي."
                });
                    return;
                }

                const files: File[] = json.map((j: ScannerFile) => {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    const byteString = atob(j.data);
                    const ab = new ArrayBuffer(byteString.length);
                    const ia = new Uint8Array(ab);
                    for (let i = 0; i < byteString.length; i++) {
                        ia[i] = byteString.charCodeAt(i);
                    }
                    const blob = new Blob([ab], { type: j.type });
                    return new File([blob], j.name, { type: j.type });
                });

<<<<<<< HEAD
            } else {
                const blob = await r.blob();
                const extension = blob.type === 'application/pdf' ? 'pdf' : blob.type === 'image/png' ? 'png' : 'jpg';
                // ADF does not imply PDF: preserve the response's actual media type.
                files = [new File([blob], `scan_${Date.now()}.${extension}`, { type: blob.type })];
            }

            // Validate the entire batch before changing the parent's single-file state.
            for (const file of files) {
                if (!file.size || !['image/jpeg', 'image/png', 'application/pdf'].includes(file.type)) {
                    toast.error(`صيغة الملف "${file.name}" غير مدعومة من الماسح أو الملف فارغ`);
                    return;
                }
                if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                    toast.error(`حجم الملف يجب أن لا يتجاوز ${maxSizeMB} ميغابايت`);
                    return;
                }
            }
            if (files.length > 1 && accept && !isFileAccepted(new File([], 'scan.pdf', { type: 'application/pdf' }), accept)) {
                toast.error('المسح متعدد الصفحات ينتج ملف PDF، وهذا الحقل لا يقبل PDF. يرجى مسح صفحة واحدة.');
                return;
            }
            let document: File;
            try {
                document = files.length === 1 ? files[0] : await combineScannedPages(files, scannerConfig.dpi);
            } catch {
                toast.error('تعذر تجميع صفحات الماسح؛ لم يتم اعتماد أي صفحة. يرجى إعادة المسح.');
                return;
            }
            if (maxSizeMB && document.size > maxSizeMB * 1024 * 1024) {
                toast.error(`حجم المستند المجمع يجب أن لا يتجاوز ${maxSizeMB} ميغابايت`);
                return;
            }
            if (accept && !isFileAccepted(document, accept)) {
                toast.error(`صيغة الملف الممسوح "${document.name}" غير مدعومة`);
                return;
            }
            onChange?.(document);
        } catch (err: unknown) {
            console.error(err);
            // م49: إلغاء المستخدم (timeout/abort) ليس عطل اتصال.
            if (err instanceof DOMException && (err.name === "AbortError" || err.name === "TimeoutError")) {
                toast.warning("تم إلغاء المسح الضوئي", {
                    description: "لم يتم استلام أي صورة من الماسح الضوئي."
                });
                return;
            }
            setScannerAvailable(false);
            setScannerProblem("agent-offline");
=======
                // Emit files sequentially to allow parent components (e.g. uploadDocument) to process them
                for (let i = 0; i < files.length; i++) {
                    if (accept && !isFileAccepted(files[i], accept)) {
                        toast.error(`صيغة الملف "${files[i].name}" غير مدعومة من الماسح`);
                        continue;
                    }
                    onChange?.(files[i]);
                    if (i < files.length - 1) {
                        await new Promise(res => setTimeout(res, 600));
                    }
                }
            } else {
                const blob = await r.blob();

                if (blob) {
                    const isPdf = blob.type === "application/pdf" || useAdf || scannerConfig.adf;
                    const extension = isPdf ? "pdf" : "jpg";
                    const mimeType = isPdf ? "application/pdf" : "image/jpeg";

                    const file = new File(
                        [blob],
                        `scan_${Date.now()}.${extension}`,
                        { type: mimeType }
                    );
                    if (accept && !isFileAccepted(file, accept)) {
                        toast.error(`صيغة الملف الممسوح "${file.name}" غير مدعومة`);
                    } else {
                        onChange?.(file);
                    }
                } else {
                    toast.warning("تم إلغاء المسح الضوئي", {
                    description: "لم يتم استلام أي صورة من الماسح الضوئي."
                });
                }
            }
        } catch (err: unknown) {
            console.error(err);
            setScannerAvailable(false);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            toast.error("فشل الاتصال بالماسح الضوئي", {
                description: "تأكد من تشغيل برنامج 'Scanner Agent' وتوصيل الجهاز."
            });
        } finally {
            setScanning(false);
        }
<<<<<<< HEAD
    }, [scannerConfig, onChange, accept, maxSizeMB]);
=======
    }, [scannerConfig, onChange, accept]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    // ─── Remove File ────────────────────────────────────

    const handleRemove = useCallback(() => {
        onChange?.(null);
        if (fileInputRef.current) {
            fileInputRef.current.value = "";
        }
    }, [onChange]);

    // ─── Size Variants ──────────────────────────────────

    const sizeClasses = {
        sm: "min-h-[140px] sm:min-h-[160px]",
        md: "min-h-[120px] sm:min-h-[160px]",
        lg: "min-h-[100px] sm:min-h-[140px] md:min-h-[160px]",
    };

    const buttonSize = size === "sm" ? "sm" : "md";

    // ─── Render ─────────────────────────────────────────

    const hasFile = !!value;
    const isString = typeof value === "string";
    const isImage = isString
        ? !value.toLowerCase().endsWith(".pdf")
        : (value as File)?.type?.startsWith("image/");
    const isPdf = isString
        ? value.toLowerCase().endsWith(".pdf")
        : (value as File)?.type === "application/pdf" || (value as File)?.name?.toLowerCase().endsWith(".pdf");

<<<<<<< HEAD
    const [previewUrl, setPreviewUrl] = useState("");

    // Setup preview URL and manage Blob URL lifecycle
    useEffect(() => {
        if (!hasFile) {
            setPreviewUrl("");
            return;
        }
        if (isString) {
            setPreviewUrl(value as string);
            return;
        }
        const url = URL.createObjectURL(value as File);
        setPreviewUrl(url);
        
        return () => URL.revokeObjectURL(url);
    }, [hasFile, isString, value]);

    return (
        <div className={cn("space-y-1.5 flex flex-col h-full", className)}>
            {label && (
                <label className="label-field">
=======
    const previewUrlRef = useRef("");

    const previewUrl = useMemo(() => {
        if (!hasFile) return "";
        if (isString) return value as string;
        return URL.createObjectURL(value as File);
    }, [hasFile, isString, value]);

    // Revoke old blob URL when a new one is created; revoke current on unmount
    useEffect(() => {
        if (!isString && previewUrl && previewUrl !== previewUrlRef.current) {
            if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current);
            previewUrlRef.current = previewUrl;
        }
        return () => {
            if (previewUrlRef.current) {
                URL.revokeObjectURL(previewUrlRef.current);
                previewUrlRef.current = "";
            }
        };
    }, [previewUrl, isString]);

    return (
        <div className={cn("space-y-1.5 flex flex-col h-full", className)}>
            {label && (
                <label className="block text-sm font-medium text-foreground">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    {label}
                    {required && <span className="text-destructive me-1">*</span>}
                </label>
            )}

            {/* Upload Area */}
            <div
                className={cn(
                    "relative border-2 border-dashed rounded-lg transition-all flex-1 flex flex-col items-center justify-center",
<<<<<<< HEAD
                    "hover:border-primary hover:bg-primary/5",
=======
                    "hover:border-primary/50 hover:bg-primary/5",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    hasFile && "border-primary bg-primary/5",
                    error && "border-destructive",
                    disabled && "opacity-50 cursor-not-allowed",
                    sizeClasses[size]
                )}
            >
                {hasFile ? (
                    // File Selected State
                    <div className="flex flex-col items-center justify-center h-full p-2 relative z-10 w-full overflow-hidden">
                        {isImage && showPreview ? (
                            <img
                                src={previewUrl}
                                alt="Preview"
                                onError={() => {
                                    console.error("Failed to load preview image", {
                                        src: previewUrl,
                                        isString,
                                        fileName: isString ? null : (value as File)?.name,
                                        fileType: isString ? null : (value as File)?.type,
                                        fileSize: isString ? null : (value as File)?.size,
                                    });
                                }}
                                onClick={(e) => {
                                    e.preventDefault();
                                    e.stopPropagation();
                                    setPreviewOpen(true);
                                }}
<<<<<<< HEAD
                                className="max-h-16 sm:max-h-20 max-w-[90%] rounded object-contain mb-1 cursor-pointer hover:opacity-100 transition-opacity"
=======
                                className="max-h-16 sm:max-h-20 max-w-[90%] rounded object-contain mb-1 cursor-pointer hover:opacity-80 transition-opacity"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            />
                        ) : isPdf ? (
                            <div className="p-2 bg-red-100 rounded-lg mb-1">
                                <FileText className="w-8 h-8 text-red-600" />
                            </div>
                        ) : (
                            <FileText className="w-8 h-8 text-primary mb-1" />
                        )}
                        <div className="flex items-center gap-1">
                            <CheckCircle2 className="w-3.5 h-3.5 text-green-600 flex-shrink-0" />
                            <p className="text-caption sm:text-caption-md font-medium text-foreground truncate max-w-[120px] sm:max-w-[150px] md:max-w-[200px]" dir="ltr">
                                {isString ? (value.split('/').pop() || "ملف مرفق") : (value as File).name}
                            </p>
                        </div>
                        <p className="text-caption sm:text-xs text-muted-foreground">
                            {isString ? "مخزن مسبقاً" : (((value as File).size) / 1024).toFixed(1) + " KB"}
                        </p>
                        {!disabled && (
                            <div className="flex items-center gap-2 mt-0.5 z-20">
                                {isImage && (
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="sm"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            e.stopPropagation();
                                            setCropOpen(true);
                                        }}
                                        className="min-h-[44px] min-w-[44px] px-1.5 sm:px-2 text-caption sm:text-xs text-blue-600 hover:bg-blue-50 hover:text-blue-700"
                                    >
                                        <CropIcon className="w-3 h-3 ms-1" />
                                        قص
                                    </Button>
                                )}
                                <Button
                                    type="button"
                                    variant="ghost"
                                    size="sm"
                                    onClick={(e) => {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        handleRemove();
                                    }}
                                    className="min-h-[44px] min-w-[44px] px-1.5 sm:px-2 text-caption sm:text-xs text-destructive hover:bg-destructive/10 hover:text-destructive"
                                >
                                    <X className="w-3 h-3 ms-1" />
                                    إزالة
                                </Button>
                            </div>
                        )}
                    </div>
                ) : (
                    // Empty State
                    <div className="flex flex-col items-center justify-center h-full p-4 gap-3">
                        {uploading ? (
                            <>
                                <Loader2 className="w-8 h-8 text-primary animate-spin" />
                                <p className="text-sm text-muted-foreground">جاري الرفع...</p>
                            </>
                        ) : (
                            <>
                                <Upload className="w-8 h-8 text-muted-foreground" />
                                <p className="text-xs sm:text-sm text-muted-foreground text-center">
                                    اسحب الملف هنا أو انقر للاختيار
                                </p>
                                <div className="flex flex-wrap items-center justify-center gap-2 relative z-10 w-full">
                                    {enableCamera && (
                                        <Button
                                            type="button"
                                            variant="outline"
                                            size={buttonSize}
                                            onClick={(e) => {
                                                e.preventDefault();
                                                e.stopPropagation();
                                                handleOpenCamera();
                                            }}
                                            disabled={disabled}
                                            className="min-h-[40px] px-3 text-xs sm:text-sm relative z-10"
                                            title="التقاط من الكاميرا"
                                        >
                                            <Camera className="w-4 h-4 ms-1.5" />
                                            كاميرا
                                        </Button>
                                    )}
                                    {enableScanner && !isMobile && (
                                        <>
                                            <Button
                                                type="button"
                                                variant="outline"
                                                size={buttonSize}
                                                onClick={(e) => {
                                                    e.preventDefault();
                                                    e.stopPropagation();
                                                    handleScan({ useAdf: false });
                                                }}
                                                disabled={disabled || scanning}
                                                loading={scanning}
                                                className={cn(
                                                    "min-h-[40px] px-3 text-xs sm:text-sm relative z-10",
<<<<<<< HEAD
                                                    !scannerAvailable && "opacity-100"
                                                )}
                                                title={scannerAvailable ? "مسح ضوئي لورقة واحدة" : scannerProblem ? `الماسح غير متاح: ${SCANNER_PROBLEM_MESSAGES[scannerProblem]}` : "الماسح غير متاح - تأكد من تشغيل Scanner Agent"}
=======
                                                    !scannerAvailable && "opacity-60"
                                                )}
                                                title={scannerAvailable ? "مسح ضوئي لورقة واحدة" : "الماسح غير متاح - تأكد من تشغيل Scanner Agent"}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            >
                                                <ScanLine className="w-4 h-4 ms-1.5" />
                                                ماسح
                                            </Button>
                                            <Button
                                                type="button"
                                                variant="outline"
                                                size={buttonSize}
                                                onClick={(e) => {
                                                    e.preventDefault();
                                                    e.stopPropagation();
                                                    handleScan({ useAdf: true });
                                                }}
                                                disabled={disabled || scanning}
                                                loading={scanning}
                                                className={cn(
                                                    "min-h-[40px] px-3 text-xs sm:text-sm relative z-10 border-primary text-primary hover:bg-primary/10",
<<<<<<< HEAD
                                                    !scannerAvailable && "opacity-100"
                                                )}
                                                title={scannerAvailable ? "سحب أكثر من ورقة (ADF)" : scannerProblem ? `الماسح غير متاح: ${SCANNER_PROBLEM_MESSAGES[scannerProblem]}` : "الماسح غير متاح - تأكد من تشغيل Scanner Agent"}
=======
                                                    !scannerAvailable && "opacity-60"
                                                )}
                                                title={scannerAvailable ? "سحب أكثر من ورقة (ADF)" : "الماسح غير متاح - تأكد من تشغيل Scanner Agent"}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            >
                                                <FileText className="w-4 h-4 ms-1.5" />
                                                عدة أوراق
                                            </Button>
                                            <Button
                                                type="button"
                                                variant="outline"
                                                size={buttonSize}
                                                onClick={(e) => {
                                                    e.preventDefault();
                                                    e.stopPropagation();
                                                    handleScan({ isPassport: true });
                                                }}
                                                disabled={disabled || scanning}
                                                loading={scanning}
                                                className={cn(
<<<<<<< HEAD
                                                    "min-h-[40px] px-3 text-xs sm:text-sm relative z-10 border-blue-500 text-blue-600 hover:bg-blue-500/10",
                                                    !scannerAvailable && "opacity-100"
                                                )}
                                                title={scannerAvailable ? "مسح صورة شخصية (4×6 سم)" : scannerProblem ? `الماسح غير متاح: ${SCANNER_PROBLEM_MESSAGES[scannerProblem]}` : "الماسح غير متاح"}
                                            >
                                                <User className="w-4 h-4 ms-1.5" />
                                                صورة 4×6
=======
                                                    "min-h-[40px] px-3 text-xs sm:text-sm relative z-10 border-blue-500/50 text-blue-600 hover:bg-blue-500/10",
                                                    !scannerAvailable && "opacity-60"
                                                )}
                                                title={scannerAvailable ? "مسح صورة شخصية (6×4)" : "الماسح غير متاح"}
                                            >
                                                <User className="w-4 h-4 ms-1.5" />
                                                صورة 6×4
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            </Button>
                                        </>
                                    )}
                                </div>
                            </>
                        )}
                    </div>
                )}

                {/* Hidden File Input */}
                <input
                    ref={fileInputRef}
                    type="file"
                    accept={accept}
                    onChange={handleFileSelect}
                    disabled={disabled || uploading}
                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer disabled:cursor-not-allowed z-0"
                    onClick={(e) => {
                        // Only allow clicking on the input area itself, not on buttons
                        const target = e.target as HTMLElement;
                        if (target.closest('button')) {
                            e.preventDefault();
                            e.stopPropagation();
                        }
                    }}
                />

                {/* Hidden Camera Input for Mobile (uses native camera via capture attribute) */}
                <input
                    ref={cameraInputRef}
                    type="file"
                    accept="image/*"
                    capture="environment"
                    onChange={handleMobileCameraCapture}
                    className="hidden"
                />
            </div>

            {/* Error/Hint Messages */}
            {error && (
                <p className="text-xs text-destructive font-medium">{error}</p>
            )}
            {!error && hint && (
                <p className="text-xs text-muted-foreground">{hint}</p>
            )}

            {/* Camera Dialog */}
            <Dialog open={cameraOpen} onOpenChange={handleCloseCamera}>
                <DialogContent size="xl">
                    <DialogHeader>
                        <DialogTitle>التقاط صورة</DialogTitle>
                        <DialogDescription>
                            قم بتوجيه الكاميرا والتقط الصورة المطلوبة
                        </DialogDescription>
                    </DialogHeader>

                    <div className="flex flex-col items-center space-y-4">
                        {cameraError ? (
                            <div className="text-destructive p-4 text-center text-sm space-y-3">
                                <p>{cameraError}</p>
                                {isMobile && (
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => {
                                            handleCloseCamera();
                                            handleOpenMobileCamera();
                                        }}
                                    >
                                        استخدام كاميرا الجهاز
                                    </Button>
                                )}
                            </div>
                        ) : (
                            <div className="relative w-full aspect-video bg-black rounded-lg overflow-hidden">
                                <video
                                    ref={videoRef}
                                    autoPlay
                                    playsInline
                                    className="w-full h-full object-cover"
                                />
                                <canvas ref={canvasRef} className="hidden" />
                            </div>
                        )}

                        <div className="flex gap-2 w-full">
                            <Button
                                variant="outline"
                                className="flex-1"
                                onClick={handleCloseCamera}
                            >
                                إلغاء
                            </Button>
                            {!cameraError && (
                                <Button
                                    className="flex-1"
                                    onClick={capturePhoto}
                                    disabled={!videoReady}
                                >
                                    <Camera className="w-4 h-4 ms-2" />
                                    {videoReady ? "التقاط" : "جاري التحميل..."}
                                </Button>
                            )}
                        </div>
                    </div>
                </DialogContent>
            </Dialog>

            {/* Image Preview Dialog */}
            <Dialog open={previewOpen} onOpenChange={setPreviewOpen}>
                <DialogContent size="xl" className="flex flex-col overflow-hidden max-h-[90dvh]">
                    <DialogHeader>
                        <DialogTitle>معاينة الصورة المرفقة</DialogTitle>
                    </DialogHeader>
                    <div className="flex-1 overflow-auto flex justify-center items-center bg-muted/10 rounded-lg p-2">
                        {value && (
                            <img
                                src={previewUrl}
                                alt="Full Preview"
                                onError={() => {
                                    console.error("Failed to load full preview image", {
                                        src: previewUrl,
                                        isString,
                                        fileName: isString ? null : (value as File)?.name,
                                        fileType: isString ? null : (value as File)?.type,
                                        fileSize: isString ? null : (value as File)?.size,
                                    });
                                }}
                                className="max-w-full max-h-[75dvh] object-contain rounded-md"
                            />
                        )}
                    </div>
                </DialogContent>
            </Dialog>

            {/* Image Cropper Dialog */}
            <ImageCropperDialog
                open={cropOpen}
                imageFile={pendingCropFile || value!}
<<<<<<< HEAD
                onClose={async () => {
                    // إذا كان هناك التقاط جديد، يتم اعتماد الصورة الأصلية بدلاً من تجاهلها عند إلغاء القص
                    const originalFile = pendingCropFile && !cropCompletedRef.current ? pendingCropFile : null;
                    cropCompletedRef.current = true;
                    setPendingCropFile(null);
                    setCropOpen(false);

                    if (originalFile) {
                        try {
                            const validated = await validateFinalFile(originalFile);
                            const accepted = onChange?.(validated);
                            if (allowContinuousCapture && onChange && accepted !== false) {
                                setContinueCaptureOpen(true);
                            }
                        } catch (err) {
                            const message = err instanceof Error ? err.message : "فشل اعتماد الصورة";
                            toast.error(message);
                        }
                    }

                }}
                onCrop={async (croppedFile) => {
                    // م40: إعادة فحص accept/size على ناتج القص قبل التسليم.
                    // الرمي يُبقي محرر القص مفتوحًا ويعرض ImageCropperDialog الرسالة.
                    const validated = await validateFinalFile(croppedFile);
                    cropCompletedRef.current = true;
                    const accepted = onChange?.(validated);
                    if (pendingCropFile && allowContinuousCapture && onChange && accepted !== false) {
                        setContinueCaptureOpen(true);
                    }
                    setPendingCropFile(null);
                }}
            />
            <ConfirmDialog
                open={continueCaptureOpen}
                onOpenChange={setContinueCaptureOpen}
                title="هل تريد مواصلة التصوير؟"
                description="تمت إضافة الصورة إلى قائمة المرفقات. هل تريد تصوير مرفق آخر؟"
                confirmLabel="تصوير مرفق آخر"
                cancelLabel="إنهاء التصوير"
                variant="info"
                onConfirm={() => {
                    setContinueCaptureOpen(false);
                    handleOpenCamera();
=======
                onClose={() => {
                    // Only use the original file if crop wasn't already confirmed
                    if (pendingCropFile && !cropCompletedRef.current) {
                        onChange?.(pendingCropFile);
                    }
                    setPendingCropFile(null);
                    cropCompletedRef.current = false;
                    setCropOpen(false);
                }}
                onCrop={(croppedFile) => {
                    onChange?.(croppedFile);
                    setPendingCropFile(null);
                    cropCompletedRef.current = true;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                }}
            />
        </div>
    );
}
