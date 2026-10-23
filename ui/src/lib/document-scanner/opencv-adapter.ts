/**
 * محوّل OpenCV المحلي (م2) — ملكية الكشف فقط، لا قرار قبول ولا تخزين دائم.
 *
 * - التحميل كسول عبر dynamic import عند فتح الجلسة فقط (chunk منفصل، نفس الأصل).
 * - نتيجة التحميل المتأخرة بعد إغلاق الجلسة تُتجاهل: لا تهيئة ولا موارد لجلسة منتهية.
 * - كل Mat يُحذف في finally داخل نفس الاستدعاء — لا مخازن مقيمة.
 * - الدوال الخالصة (ترتيب/اختيار الرباعي) قابلة لاختبار الوحدات بلا DOM ولا WASM.
 */

import type { validateCorners } from "../homography.ts";

export interface QuadPoint {
    x: number;
    y: number;
}

export interface DetectedQuad {
    /** الزوايا مرتبة: أعلى-يسار، أعلى-يمين، أسفل-يمين، أسفل-يسار (بإحداثيات التحليل) */
    points: [QuadPoint, QuadPoint, QuadPoint, QuadPoint];
    /** نسبة مساحة الرباعي لمساحة الإطار */
    areaRatio: number;
}

type OpenCvModule = typeof import("@techstark/opencv-js");

export interface ScanEngine {
    readonly sessionId: string;
    readonly alive: boolean;
}

interface EngineState {
    cv: OpenCvModule | null;
    sessionId: string;
    alive: boolean;
    loadEpoch: number;
}

let cachedModule: OpenCvModule | null = null;
let cachedModulePromise: Promise<OpenCvModule | null> | null = null;
const engines = new Map<string, EngineState>();

async function waitRuntime(cvModule: OpenCvModule, timeoutMs: number): Promise<OpenCvModule> {
    const candidate = cvModule as unknown as Record<string, unknown>;
    if (typeof candidate["Mat"] === "function") return cvModule;
    const ready = candidate["ready"];
    if (ready instanceof Promise) {
        const timeout = new Promise<never>((_, reject) => {
            setTimeout(() => reject(new Error("انتهت مهلة تهيئة OpenCV")), timeoutMs);
        });
        await Promise.race([ready, timeout]);
        return cvModule;
    }
    await new Promise<void>((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error("انتهت مهلة تهيئة OpenCV")), timeoutMs);
        candidate["onRuntimeInitialized"] = () => {
            clearTimeout(timer);
            resolve();
        };
    });
    return cvModule;
}

function loadModuleOnce(timeoutMs: number): Promise<OpenCvModule | null> {
    if (cachedModule) return Promise.resolve(cachedModule);
    if (!cachedModulePromise) {
        cachedModulePromise = import("@techstark/opencv-js")
            .then(async (mod) => {
                // The package is CommonJS: Vite/Node may put its thenable runtime
                // under `default` instead of exporting Mat on the namespace.
                const namespace = mod as unknown as Record<string, unknown>;
                const runtime = namespace["default"] ?? mod;
                const resolved = await Promise.race([
                    Promise.resolve(runtime),
                    new Promise<never>((_, reject) =>
                        setTimeout(() => reject(new Error("انتهت مهلة تهيئة OpenCV")), timeoutMs)
                    ),
                ]);
                return waitRuntime(resolved as OpenCvModule, timeoutMs);
            })
            .then((mod) => {
                cachedModule = mod;
                return mod;
            })
            .catch(() => {
                cachedModulePromise = null;
                return null;
            });
    }
    return cachedModulePromise;
}

/**
 * إنشاء محرك مرتبط بجلسة. يعيد null عند فشل التحميل/التهيئة (سقوط للمسار اليدوي).
 * إن انتهت الجلسة أثناء التحميل تُتجاهل النتيجة ولا تُنشأ موارد لها.
 */
export async function createScanEngine(sessionId: string, timeoutMs = 20000): Promise<ScanEngine | null> {
    const state: EngineState = { cv: null, sessionId, alive: true, loadEpoch: Date.now() };
    engines.set(sessionId, state);
    const mod = await loadModuleOnce(timeoutMs);
    const current = engines.get(sessionId);
    if (!current || !current.alive || !mod) {
        engines.delete(sessionId);
        return null;
    }
    current.cv = mod;
    return { sessionId, alive: true };
}

/** تحرير موارد الجلسة. أي تحميل متأخر لاحق يُتجاهل تلقائيًا عبر فحص alive. */
export function releaseScanEngine(sessionId: string): void {
    engines.delete(sessionId);
}

function getLiveCv(sessionId: string): OpenCvModule | null {
    const state = engines.get(sessionId);
    if (!state || !state.alive || !state.cv) return null;
    return state.cv;
}

/** ترتيب 4 نقاط: أعلى-يسار، أعلى-يمين، أسفل-يمين، أسفل-يسار */
export function orderQuadPoints(points: QuadPoint[]): [QuadPoint, QuadPoint, QuadPoint, QuadPoint] | null {
    if (points.length !== 4) return null;
    const sorted = [...points].sort((a, b) => a.x + a.y - (b.x + b.y));
    const topLeft = sorted[0];
    const bottomRight = sorted[3];
    const [p, q] = [sorted[1], sorted[2]];
    const topRight = p.y - p.x < q.y - q.x ? p : q;
    const bottomLeft = topRight === p ? q : p;
    return [topLeft, topRight, bottomRight, bottomLeft];
}

function polygonArea(points: QuadPoint[]): number {
    let sum = 0;
    for (let i = 0; i < points.length; i++) {
        const p = points[i];
        const q = points[(i + 1) % points.length];
        sum += p.x * q.y - q.x * p.y;
    }
    return Math.abs(sum) / 2;
}

/**
 * اختيار أفضل رباعي من مرشحين (إحداثيات تحليل): الأكبر مساحة ضمن [minRatio, maxRatio].
 * الرفض الهندسي التفصيلي (تقاطع/أضلاع) يتم عبر validateCorners المشتركة في م3.
 */
export function pickBestQuad(
    candidates: QuadPoint[][],
    frameWidth: number,
    frameHeight: number,
    minRatio = 0.05,
    maxRatio = 0.95,
): DetectedQuad | null {
    const frameArea = frameWidth * frameHeight;
    if (frameArea <= 0) return null;
    let best: DetectedQuad | null = null;
    for (const cand of candidates) {
        if (cand.length !== 4) continue;
        const area = polygonArea(cand);
        const ratio = area / frameArea;
        if (ratio < minRatio || ratio > maxRatio) continue;
        const ordered = orderQuadPoints(cand);
        if (!ordered) continue;
        if (!best || ratio > best.areaRatio) {
            best = { points: ordered, areaRatio: ratio };
        }
    }
    return best;
}

export type ValidateCornersFn = typeof validateCorners;

/**
 * كشف رباعي الوثيقة من ImageData بإطار التحليل. يعيد null عند غياب وثيقة صالحة.
 * كل موارد OpenCV تُحرر قبل العودة مهما كانت النتيجة.
 */
export function detectQuadFromImageData(
    engine: ScanEngine,
    imageData: ImageData,
    isValid: ValidateCornersFn,
): DetectedQuad | null {
    const cv = getLiveCv(engine.sessionId);
    if (!cv) return null;
    const w = imageData.width;
    const h = imageData.height;
    if (w <= 0 || h <= 0) return null;

    const src = cv.matFromImageData(imageData);
    const gray = new cv.Mat();
    const blurred = new cv.Mat();
    const edges = new cv.Mat();
    const contours = new cv.MatVector();
    const hierarchy = new cv.Mat();
    const approx = new cv.Mat();
    try {
        cv.cvtColor(src, gray, cv.COLOR_RGBA2GRAY, 0);
        cv.GaussianBlur(gray, blurred, new cv.Size(5, 5), 0, 0, cv.BORDER_DEFAULT);
        cv.Canny(blurred, edges, 75, 200, 3, false);
        cv.findContours(edges, contours, hierarchy, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE);
        const candidates: QuadPoint[][] = [];
        const frameArea = w * h;
        for (let i = 0; i < contours.size(); i++) {
            const contour = contours.get(i);
            try {
                const area = cv.contourArea(contour, false);
                if (area < frameArea * 0.02) continue;
                const peri = cv.arcLength(contour, true);
                cv.approxPolyDP(contour, approx, 0.02 * peri, true);
                if (approx.rows !== 4) continue;
                if (!cv.isContourConvex(approx)) continue;
                const data = approx.data32S;
                const pts: QuadPoint[] = [
                    { x: data[0], y: data[1] },
                    { x: data[2], y: data[3] },
                    { x: data[4], y: data[5] },
                    { x: data[6], y: data[7] },
                ];
                if (isValid(pts) !== null) continue;
                candidates.push(pts);
            } finally {
                contour.delete();
            }
        }
        return pickBestQuad(candidates, w, h);
    } finally {
        src.delete();
        gray.delete();
        blurred.delete();
        edges.delete();
        contours.delete();
        hierarchy.delete();
        approx.delete();
    }
}
