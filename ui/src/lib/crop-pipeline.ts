import { CPUProcessor } from "./cpu-processor.ts";
import {
    computeCropRegion,
    estimateOutputSize,
    getCropBudget,
    type PixelBudget,
    type Point2D,
} from "./crop-region.ts";
import {
    createCropWorker,
    warpInWorker,
    WorkerFailureError,
    WorkerTimeoutError,
} from "./crop-worker-client.ts";
import { type DeviceCapabilities,getDeviceCapabilities, MIN_MAX_PIXELS, shouldUseWebGL } from "./device-capabilities.ts";
import type { PixelBuffer, ProcessResult } from "./pixel-buffer.ts";
import { WebGLProcessor } from "./webgl-processor.ts";

export type { PixelBuffer };

export type CropEngine = "webgl" | "worker" | "cpu";

export interface CropResult extends ProcessResult {
    engine: CropEngine;
}

export interface CropRequest {
    /** مصدر الصورة: HTMLImageElement أو canvas */
    source: CanvasImageSource;
    sourceWidth: number;
    sourceHeight: number;
    /** الزوايا بإحداثيات المصدر قبل أي تصغير */
    corners: Point2D[];
    capabilities?: DeviceCapabilities;
    signal?: AbortSignal;
    /** مهلة المعالجة الكلية (ms) — تشمل WebGL والـWorker وCPU فقط، لا الحفظ ولا الرفع */
    timeoutMs?: number;
    /** مهلة محاولة الـWorker الواحدة (ms) */
    workerTimeoutMs?: number;
}

/** مهلة المعالجة الكلية — لا تغطي onCrop ولا الضغط ولا الرفع */
export const DEFAULT_CROP_TIMEOUT_MS = 30_000;
/** مهلة محاولة الـWorker — أقل من الكلية ليتسع الوقت لمسار بديل */
export const DEFAULT_WORKER_TIMEOUT_MS = 20_000;
/** سقف أمان للواجهة: فوقه لا نجمّد الخيط الرئيسي بـCPU */
export const CPU_MAIN_THREAD_MAX_PIXELS = 1_000_000;

const MAX_POOLED_CANVASES = 3;

// ─── Canvas Pool ─────────────────────────────────────────

const globalCanvasPool: HTMLCanvasElement[] = [];

function acquireCanvas(w: number, h: number): HTMLCanvasElement {
    const c = globalCanvasPool.pop() || document.createElement("canvas");
    c.width = w;
    c.height = h;
    return c;
}

function releaseCanvas(c: HTMLCanvasElement) {
    c.width = 0;
    c.height = 0;
    if (globalCanvasPool.length < MAX_POOLED_CANVASES) globalCanvasPool.push(c);
}

// ─── Engine Manager ─────────────────────────────────────
//
// موارد واحدة على مستوى التطبيق: context واحد لـWebGL وworker واحد وprocessor واحد للـCPU.
// الـworker يُنهي عند خلو الخيوط (idle) في finally، فلا يبقى resident بعد انتهاء أي عملية قص.
let webglProcessor: WebGLProcessor | null = null;
let webglUnavailable = false;
let cpuProcessor: CPUProcessor | null = null;
let cachedCpuBudgetKey = "";
let worker: Worker | null = null;
let workerChain: Promise<unknown> = Promise.resolve();
let activeRuns = 0;

function budgetKey(budget: PixelBudget): string {
    return `${budget.maxDimension}x${budget.maxPixels}`;
}

function getWebGL(capabilities: DeviceCapabilities, budget: PixelBudget): WebGLProcessor | null {
    if (webglUnavailable) return null;
    if (!webglProcessor) {
        if (!shouldUseWebGL(capabilities)) {
            webglUnavailable = true;
            return null;
        }
        webglProcessor = new WebGLProcessor({
            maxTextureSize: capabilities.maxTextureSize,
            maxPixels: budget.maxPixels,
            enhanceAmount: 0.3,
        });
    }
    return webglProcessor;
}

function getCPU(budget: PixelBudget): CPUProcessor {
    const key = budgetKey(budget);
    if (!cpuProcessor || cachedCpuBudgetKey !== key) {
        cpuProcessor = new CPUProcessor({
            maxDimension: budget.maxDimension,
            maxPixels: budget.maxPixels,
            enhanceAmount: 0.3,
        });
        cachedCpuBudgetKey = key;
    }
    return cpuProcessor;
}

function ensureWorker(): Worker | null {
    if (!worker) worker = createCropWorker();
    return worker;
}

function releaseWorker(): void {
    worker?.terminate();
    worker = null;
}

/** Worker واحد مشترك — تُسلسل الطلبات حتى لا يستبدل مستمع onmessage طلباً آخر */
function runExclusively<T>(task: () => Promise<T>): Promise<T> {
    const run = workerChain.then(task, task);
    workerChain = run.then(() => undefined, () => undefined);
    return run;
}

/** تحرير كل موارد المعالجة — للاختبارات و tear-down */
export function disposeCropEngine(): void {
    releaseWorker();
    webglProcessor?.destroy();
    webglProcessor = null;
    webglUnavailable = false;
    cpuProcessor = null;
    cachedCpuBudgetKey = "";
    while (globalCanvasPool.length) {
        const c = globalCanvasPool.pop()!;
        c.width = 0;
        c.height = 0;
    }
}

// ─── Pipeline ───────────────────────────────────────────

/**
 * WebGL ← Worker ← CPU(main thread بحد pixels).
 * كل الموارد (canvas + worker) تُنظَّف في finally، والنتيجة مخزن بكسلات واحد.
 */
export async function runCropPipeline(request: CropRequest): Promise<CropResult> {
    activeRuns++;
    try {
        return await executePipeline(request);
    } finally {
        activeRuns--;
        // لا يبقى الـworker معلّقاً بين عمليات القص؛ المتوازي منها ينتظر دوره على workerChain
        if (activeRuns === 0) releaseWorker();
    }
}

async function executePipeline(request: CropRequest): Promise<CropResult> {
    const capabilities = request.capabilities ?? getDeviceCapabilities();
    const budget = getCropBudget(capabilities);
    const cropBudget: PixelBudget = {
        maxDimension: budget.maxDim,
        maxPixels: budget.maxPixels,
    };
    const deadline = Date.now() + (request.timeoutMs ?? DEFAULT_CROP_TIMEOUT_MS);
    const workerTimeoutMs = request.workerTimeoutMs ?? DEFAULT_WORKER_TIMEOUT_MS;

    const region = computeCropRegion(
        request.corners,
        request.sourceWidth,
        request.sourceHeight,
        capabilities,
    );
    const scaledCorners = request.corners.map((c) => ({
        x: (c.x - region.sx) * region.scale,
        y: (c.y - region.sy) * region.scale,
    }));

    const canvas = acquireCanvas(region.workWidth, region.workHeight);
    try {
        const ctx = canvas.getContext("2d", { willReadFrequently: true });
        if (!ctx) throw new Error("فشل إنشاء سياق رسم الصورة");
        ctx.drawImage(
            request.source,
            region.sx,
            region.sy,
            region.sw,
            region.sh,
            0,
            0,
            region.workWidth,
            region.workHeight,
        );
        const srcData = ctx.getImageData(0, 0, region.workWidth, region.workHeight).data;
        const { workWidth, workHeight } = region;

        if (request.signal?.aborted) throw new WorkerFailureError("تم إلغاء المعالجة");

        // 1) WebGL
        const gl = getWebGL(capabilities, cropBudget);
        if (gl) {
            const result = gl.process(srcData, workWidth, workHeight, scaledCorners);
            if (result) return { ...result, engine: "webgl" };
            if (gl.isPermanentlyDisabled()) webglUnavailable = true;
        }

        // 2) Worker — إعادة محاولة واحدة فقط عند عطل سريع قابل للاسترداد، ولا شيء بعد timeout
        const workerResult = await runWorkerStage(
            srcData,
            workWidth,
            workHeight,
            scaledCorners,
            cropBudget,
            deadline,
            workerTimeoutMs,
            request.signal,
        );
        if (workerResult) return { ...workerResult, engine: "worker" };

        // 3) CPU على الخيط الرئيسي — مسموح فقط ضمن سقف أمان الواجهة
        const predicted = estimateOutputSize(scaledCorners, cropBudget);
        if (predicted.width * predicted.height > CPU_MAIN_THREAD_MAX_PIXELS) {
            throw new Error(
                "تعذّرت معالجة الصورة على هذا الجهاز. صغّر الصورة أو أعد المحاولة من جهاز آخر.",
            );
        }

        const result = getCPU(cropBudget).process(srcData, workWidth, workHeight, scaledCorners);
        return { ...result, engine: "cpu" };
    } finally {
        releaseCanvas(canvas);
    }
}

async function runWorkerStage(
    srcData: Uint8ClampedArray,
    width: number,
    height: number,
    corners: Point2D[],
    budget: PixelBudget,
    deadline: number,
    workerTimeoutMs: number,
    signal?: AbortSignal,
): Promise<ProcessResult | null> {
    for (let attempt = 0; attempt < 2; attempt++) {
        const remaining = deadline - Date.now();
        if (remaining <= 0) return null;
        if (signal?.aborted) throw new WorkerFailureError("تم إلغاء المعالجة");

        const active = ensureWorker();
        if (!active) return null;

        const timeout = attempt === 0 ? Math.min(workerTimeoutMs, remaining) : remaining;
        try {
            return await runExclusively(() =>
                warpInWorker(active, srcData, width, height, corners, budget, timeout, signal),
            );
        } catch (error) {
            releaseWorker();
            if (error instanceof WorkerTimeoutError) return null;
            if (!(error instanceof WorkerFailureError)) throw error;
            console.warn("WebGL/Worker crop attempt failed:", error);
        }
    }
    return null;
}

export { MIN_MAX_PIXELS };
