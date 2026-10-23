import { useCallback, useEffect, useRef, useState } from "react";

import { runCropPipeline } from "@/lib/crop-pipeline";
import { getDeviceCapabilities, getOptimalProcessingSize } from "@/lib/device-capabilities";
import {
    checkCameraPrerequisites,
    describeCameraError,
    readErrorName,
} from "@/lib/document-scanner/camera-errors";
import { scalePoints } from "@/lib/document-scanner/geometry";
import {
    createScanEngine,
    type DetectedQuad,
    detectQuadFromImageData,
    releaseScanEngine,
} from "@/lib/document-scanner/opencv-adapter";
import {
    assessQuality,
    type QualityAssessment,
} from "@/lib/document-scanner/quality";
import {
    createInitialSnapshot,
    DEFAULT_SCAN_CONFIG,
    hasReachedItemCap,
    type ScanAction,
    type ScanMachineConfig,
    type ScanSnapshot,
    transition,
} from "@/lib/document-scanner/session-machine";
import { validateCorners } from "@/lib/homography";

export interface AcceptedCapture {
    captureId: number;
    file: File;
    /** null عند الالتقاط اليدوي بلا مضلع — الزوايا تحدد يدويًا في CornerEditor */
    quad: DetectedQuad | null;
    manual: boolean;
    /** تنبيهي فقط — null عند تعذر الحساب ويعامل معاملة needsReview في م5 */
    quality: QualityAssessment | null;
}

interface SessionCallbacks {
    onAccept: (capture: AcceptedCapture) => void;
    onError: (message: string) => void;
}

function newSessionId(): string {
    try {
        if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
            return crypto.randomUUID();
        }
    } catch {
        // fallback أدناه
    }
    return `scan_${Date.now()}_${Math.random().toString(36).slice(2, 10)}`;
}

function canvasToJpeg(canvas: HTMLCanvasElement, name: string): Promise<File> {    return new Promise((resolve, reject) => {
        canvas.toBlob((blob) => {
            if (!blob) {
                reject(new Error("فشل إنشاء الصورة"));
                return;
            }
            resolve(new File([blob], name, { type: "image/jpeg" }));
        }, "image/jpeg", 0.9);
    });
}

/**
 * جلسة مسح الكاميرا (م3): تملك البث وحلقة التحليل وتنفيذ الالتقاط.
 * القرار لآلة الحالات. الاكتشاف للمحوّل. القص لخط crop-pipeline. الجودة تنبيهية في م4.
 */
export function useCameraScanSession(
    videoRef: React.RefObject<HTMLVideoElement | null>,
    active: boolean,
    callbacks: SessionCallbacks,
    config: ScanMachineConfig = DEFAULT_SCAN_CONFIG,
) {
    const [snapshot, setSnapshot] = useState<ScanSnapshot>(createInitialSnapshot);
    const [overlay, setOverlay] = useState<{ quad: DetectedQuad; width: number; height: number } | null>(null);
    const [streamError, setStreamError] = useState("");

    const sessionIdRef = useRef("");
    const snapshotRef = useRef<ScanSnapshot>(createInitialSnapshot());
    const captureInFlightRef = useRef(false);
    const captureControllerRef = useRef<AbortController | null>(null);
    const streamRequestRef = useRef(0);
    const streamRef = useRef<MediaStream | null>(null);
    const analysisCanvasRef = useRef<HTMLCanvasElement | null>(null);
    const latestQuadRef = useRef<{ quad: DetectedQuad; width: number; height: number } | null>(null);
    const facingModeRef = useRef<"environment" | "user">("environment");
    const callbacksRef = useRef(callbacks);
    useEffect(() => {
        callbacksRef.current = callbacks;
    }, [callbacks]);

    const stopStream = useCallback(() => {
        streamRequestRef.current += 1;
        streamRef.current?.getTracks().forEach((track) => track.stop());
        streamRef.current = null;
        const video = videoRef.current;
        if (video) video.srcObject = null;
    }, [videoRef]);

    const dispatch = useCallback((event: Parameters<typeof transition>[1]) => {
        const current = snapshotRef.current;
        const { snapshot: next, actions } = transition(current, event, config);
        snapshotRef.current = next;
        setSnapshot(next);
        void handleActions(actions);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [config]);

    const handleActions = useCallback(async (actions: ScanAction[]) => {
        for (const action of actions) {
            if (action.kind === "loadEngine") {
                const sessionId = sessionIdRef.current;
                const engine = await createScanEngine(sessionId);
                if (sessionIdRef.current !== sessionId || !enginesAlive()) return;
                dispatch({ type: engine ? "ENGINE_READY" : "ENGINE_FAILED" });
            } else if (action.kind === "capture") {
                await executeCapture(action.captureId, action.manual);
            } else if (action.kind === "releaseEngine") {
                stopStream();
                releaseScanEngine(sessionIdRef.current);
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [dispatch, stopStream]);

    const enginesAlive = useCallback(() => snapshotRef.current.state !== "ended", []);

    const executeCapture = useCallback(async (captureId: number, manual: boolean) => {
        if (captureInFlightRef.current) return;
        const video = videoRef.current;
        if (!video || video.videoWidth === 0) {
            dispatch({ type: "CAPTURE_FAILED" });
            return;
        }
        captureInFlightRef.current = true;
        const controller = new AbortController();
        captureControllerRef.current = controller;
        const captureSessionId = sessionIdRef.current;
        const isCurrentCapture = () =>
            !controller.signal.aborted &&
            sessionIdRef.current === captureSessionId &&
            snapshotRef.current.state === "processing" &&
            snapshotRef.current.captureId === captureId;
        try {
            const fullCanvas = document.createElement("canvas");
            fullCanvas.width = video.videoWidth;
            fullCanvas.height = video.videoHeight;
            const ctx = fullCanvas.getContext("2d", { willReadFrequently: false });
            if (!ctx) throw new Error("فشل إنشاء سياق التصوير");
            ctx.drawImage(video, 0, 0, fullCanvas.width, fullCanvas.height);

            const detected = latestQuadRef.current;
            const stamp = `scan_${Date.now()}.jpg`;
            if (detected && snapshotRef.current.state !== "ended") {
                const corners = scalePoints(
                    [...detected.quad.points],
                    detected.width,
                    detected.height,
                    fullCanvas.width,
                    fullCanvas.height,
                );
                const img = await createImageFromCanvas(fullCanvas);
                const result = await runCropPipeline({
                    source: img,
                    sourceWidth: img.naturalWidth,
                    sourceHeight: img.naturalHeight,
                    corners,
                    capabilities: getDeviceCapabilities(),
                    signal: controller.signal,
                });
                const outCanvas = document.createElement("canvas");
                outCanvas.width = result.width;
                outCanvas.height = result.height;
                const outCtx = outCanvas.getContext("2d");
                if (!outCtx) throw new Error("فشل إنشاء سياق الإخراج");
                outCtx.putImageData(new ImageData(result.data, result.width, result.height), 0, 0);
                const file = await canvasToJpeg(outCanvas, stamp);
                const quality = sampleQuality(outCanvas);
                outCanvas.width = 0;
                outCanvas.height = 0;
                fullCanvas.width = 0;
                fullCanvas.height = 0;
                if (!isCurrentCapture()) return;
                callbacksRef.current.onAccept({ captureId, file, quad: detected.quad, manual, quality });
            } else {
                // بلا مضلع (يدوي عند الحاجة): الصورة الخام — الزوايا يدويًا في CornerEditor
                const file = await canvasToJpeg(fullCanvas, stamp);
                const quality = sampleQuality(fullCanvas);
                fullCanvas.width = 0;
                fullCanvas.height = 0;
                if (!isCurrentCapture()) return;
                callbacksRef.current.onAccept({ captureId, file, quad: null, manual, quality });
            }
            if (isCurrentCapture()) {
                dispatch({ type: "CAPTURE_DONE", captureId });
            }
        } catch {
            if (isCurrentCapture()) {
                dispatch({ type: "CAPTURE_FAILED" });
            }
        } finally {
            controller.abort();
            if (captureControllerRef.current === controller) {
                captureControllerRef.current = null;
                captureInFlightRef.current = false;
            }
        }
         
    }, [dispatch, videoRef]);

    /**
     * عينة جودة مصغرة (ضلع ≤400) من canvas الناتج — تنبيه فقط، لا تمنع القبول.
     * null عند التعذر ويعامل معاملة needsReview في م5.
     */
    function sampleQuality(source: HTMLCanvasElement): QualityAssessment | null {
        try {
            const scale = Math.min(1, 400 / Math.max(source.width, source.height));
            const sample = document.createElement("canvas");
            sample.width = Math.max(2, Math.round(source.width * scale));
            sample.height = Math.max(2, Math.round(source.height * scale));
            const ctx = sample.getContext("2d", { willReadFrequently: true });
            if (!ctx) return null;
            ctx.drawImage(source, 0, 0, sample.width, sample.height);
            const imageData = ctx.getImageData(0, 0, sample.width, sample.height);
            const assessment = assessQuality(imageData);
            sample.width = 0;
            sample.height = 0;
            return assessment;
        } catch {
            return null;
        }
    }

    function createImageFromCanvas(canvas: HTMLCanvasElement): Promise<HTMLImageElement> {        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(img);
            img.onerror = () => reject(new Error("فشل تحميل إطار التصوير"));
            img.src = canvas.toDataURL("image/jpeg", 0.92);
        });
    }

    const startStream = useCallback(async () => {
        stopStream();
        const requestId = streamRequestRef.current;
        setStreamError("");
        const blocked = checkCameraPrerequisites({
            isSecureContext: typeof window === "undefined" ? true : window.isSecureContext,
            hasMediaDevices: typeof navigator !== "undefined" && !!navigator.mediaDevices,
            hasGetUserMedia: typeof navigator !== "undefined" && !!navigator.mediaDevices?.getUserMedia,
        });
        if (blocked) {
            setStreamError(blocked);
            callbacksRef.current.onError(blocked);
            return;
        }
        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: facingModeRef.current },
            });
            if (requestId !== streamRequestRef.current || snapshotRef.current.state === "ended") {
                stream.getTracks().forEach((track) => track.stop());
                return;
            }
            streamRef.current = stream;
            const video = videoRef.current;
            if (video) {
                video.srcObject = stream;
                await video.play().catch(() => undefined);
            }
        } catch (error: unknown) {
            if (requestId !== streamRequestRef.current || snapshotRef.current.state === "ended") return;
            const message = describeCameraError(readErrorName(error));
            setStreamError(message);
            callbacksRef.current.onError(message);
        }
    }, [stopStream, videoRef]);

    // فتح/إغلاق الجلسة
    useEffect(() => {
        if (!active) return;
        sessionIdRef.current = newSessionId();
        const initial = createInitialSnapshot();
        snapshotRef.current = initial;
        setSnapshot(initial);
        setOverlay(null);
        latestQuadRef.current = null;
        captureInFlightRef.current = false;
        dispatch({ type: "OPEN" });
        void startStream();
        return () => {
            snapshotRef.current = transition(snapshotRef.current, { type: "END" }, config).snapshot;
            captureControllerRef.current?.abort();
            stopStream();
            releaseScanEngine(sessionIdRef.current);
            sessionIdRef.current = "";
            setOverlay(null);
            latestQuadRef.current = null;
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [active]);

    // إيقاف عند انتقال الصفحة للخلفية
    useEffect(() => {
        if (!active) return;
        const onVisibility = () => {
            if (document.hidden) dispatch({ type: "PAUSE" });
        };
        document.addEventListener("visibilitychange", onVisibility);
        return () => document.removeEventListener("visibilitychange", onVisibility);
    }, [active, dispatch]);

    // حلقة التحليل: إطار صغير → كشف → حدث للآلة (بحث/ثبات/انتظار فقط)
    useEffect(() => {
        if (!active) return;
        const capabilities = getDeviceCapabilities();
        const target = getOptimalProcessingSize(capabilities);
        const id = setInterval(() => {
            const state = snapshotRef.current.state;
            if (state !== "searching" && state !== "stabilizing" && state !== "waitingRemoval") return;
            const video = videoRef.current;
            if (!video || video.videoWidth === 0 || video.readyState < 2) return;
            let canvas = analysisCanvasRef.current;
            if (!canvas) {
                canvas = document.createElement("canvas");
                analysisCanvasRef.current = canvas;
            }
            const scale = Math.min(1, target / Math.max(video.videoWidth, video.videoHeight));
            canvas.width = Math.max(2, Math.round(video.videoWidth * scale));
            canvas.height = Math.max(2, Math.round(video.videoHeight * scale));
            const ctx = canvas.getContext("2d", { willReadFrequently: true });
            if (!ctx) return;
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
            let imageData: ImageData;
            try {
                imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
            } catch {
                return;
            }
            const engine = { sessionId: sessionIdRef.current, alive: true };
            const quad = detectQuadFromImageData(engine, imageData, validateCorners);
            if (quad) {
                latestQuadRef.current = { quad, width: canvas.width, height: canvas.height };
                setOverlay({ quad, width: canvas.width, height: canvas.height });
                dispatch({ type: "FRAME_QUAD", quad });
            } else {
                latestQuadRef.current = null;
                setOverlay(null);
                dispatch({ type: "FRAME_EMPTY" });
            }
        }, 250);
        return () => clearInterval(id);
    }, [active, dispatch, videoRef]);

    const manualCapture = useCallback(() => dispatch({ type: "MANUAL_CAPTURE" }), [dispatch]);
    const pause = useCallback(() => {
        stopStream();
        latestQuadRef.current = null;
        setOverlay(null);
        dispatch({ type: "PAUSE" });
    }, [dispatch, stopStream]);
    const resume = useCallback(() => {
        void startStream();
        dispatch({ type: "RESUME" });
    }, [dispatch, startStream]);
    const end = useCallback(() => {
        captureControllerRef.current?.abort();
        stopStream();
        dispatch({ type: "END" });
    }, [dispatch, stopStream]);
    const switchCamera = useCallback(() => {
        facingModeRef.current = facingModeRef.current === "environment" ? "user" : "environment";
        latestQuadRef.current = null;
        setOverlay(null);
        void startStream();
    }, [startStream]);

    const capped = hasReachedItemCap(snapshot, config);

    return { snapshot, overlay, streamError, manualCapture, pause, resume, end, switchCamera, capped };
}
