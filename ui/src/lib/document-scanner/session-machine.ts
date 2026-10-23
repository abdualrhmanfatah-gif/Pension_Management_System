/**
 * آلة حالات جلسة المسح (م3) — خالصة بلا DOM ولا React، قابلة لاختبار الوحدات.
 *
 * - شرطا Auto Capture فقط: مضلع كافٍ + ثبات (عدّ إطارات مستقرة متتالية).
 * - waitingRemoval: إزالة فقط + إعادة تسليح. الثبات يحسم فقط في stabilizing.
 * - الالتقاط اليدوي يتجاوز الثبات والمضلع. القفل: captureId لكل محاولة.
 */

import type { DetectedQuad } from "./opencv-adapter.ts";

export type ScanState =
    | "idle"
    | "initializing"
    | "searching"
    | "stabilizing"
    | "processing"
    | "waitingRemoval"
    | "reviewPause"
    | "paused"
    | "ended";

export type ScanAction =
    | { kind: "loadEngine" }
    | { kind: "capture"; captureId: number; manual: boolean }
    | { kind: "releaseEngine" };

export interface ScanMachineConfig {
    /** إطارات مستقرة متتالية لازمة قبل الالتقاط التلقائي */
    stableFramesRequired: number;
    /** إطارات غياب متتالية لتأكيد إزالة الورقة (ليست لمحة) */
    absentFramesRequired: number;
    /** حد عناصر الجلسة — configurable، القيمة مبدئية قبل المعايرة الميدانية */
    maxItems: number;
}

/** حد مبدئي قبل المعايرة على الأجهزة الفعلية — ليس نهائيًا */
export const DEFAULT_MAX_SESSION_ITEMS = 50;

export const DEFAULT_SCAN_CONFIG: ScanMachineConfig = {
    stableFramesRequired: 5,
    absentFramesRequired: 4,
    maxItems: DEFAULT_MAX_SESSION_ITEMS,
};

export type ScanEvent =
    | { type: "OPEN" }
    | { type: "ENGINE_READY" }
    | { type: "ENGINE_FAILED" }
    | { type: "FRAME_QUAD"; quad: DetectedQuad }
    | { type: "FRAME_EMPTY" }
    | { type: "MANUAL_CAPTURE" }
    | { type: "CAPTURE_DONE"; captureId: number }
    | { type: "CAPTURE_FAILED" }
    | { type: "PAUSE" }
    | { type: "RESUME" }
    | { type: "END" };

export interface ScanSnapshot {
    state: ScanState;
    /**-set عند دخول processing؛ تُرفض أي نتيجة captureId مختلف */
    captureId: number | null;
    stableFrames: number;
    absentFrames: number;
    acceptedCount: number;
}

export interface TransitionResult {
    snapshot: ScanSnapshot;
    actions: ScanAction[];
}

const INITIAL: ScanSnapshot = {
    state: "idle",
    captureId: null,
    stableFrames: 0,
    absentFrames: 0,
    acceptedCount: 0,
};

export function createInitialSnapshot(): ScanSnapshot {
    return { ...INITIAL };
}

export function transition(
    snapshot: ScanSnapshot,
    event: ScanEvent,
    config: ScanMachineConfig = DEFAULT_SCAN_CONFIG,
): TransitionResult {
    const actions: ScanAction[] = [];
    const next: ScanSnapshot = { ...snapshot };

    // The cap must be enforced at the decision point, not only on the button.
    if (snapshot.acceptedCount >= config.maxItems &&
        (event.type === "MANUAL_CAPTURE" || event.type === "FRAME_QUAD")) {
        return { snapshot: next, actions };
    }

    switch (snapshot.state) {
        case "idle":
            if (event.type === "OPEN") {
                next.state = "initializing";
                actions.push({ kind: "loadEngine" });
            }
            break;

        case "initializing":
            if (event.type === "ENGINE_READY") next.state = "searching";
            else if (event.type === "ENGINE_FAILED") next.state = "searching";
            else if (event.type === "END") {
                next.state = "ended";
                actions.push({ kind: "releaseEngine" });
            }
            break;

        case "searching":
            if (event.type === "FRAME_QUAD") {
                next.state = "stabilizing";
                next.stableFrames = 1;
                next.absentFrames = 0;
            } else if (event.type === "MANUAL_CAPTURE") {
                next.state = "processing";
                next.captureId = (snapshot.captureId ?? 0) + 1;
                actions.push({ kind: "capture", captureId: next.captureId, manual: true });
            } else if (event.type === "PAUSE") next.state = "paused";
            else if (event.type === "END") {
                next.state = "ended";
                actions.push({ kind: "releaseEngine" });
            }
            break;

        case "stabilizing":
            if (event.type === "FRAME_QUAD") {
                next.stableFrames = snapshot.stableFrames + 1;
                if (next.stableFrames >= config.stableFramesRequired) {
                    next.state = "processing";
                    next.captureId = (snapshot.captureId ?? 0) + 1;
                    next.stableFrames = 0;
                    actions.push({ kind: "capture", captureId: next.captureId, manual: false });
                }
            } else if (event.type === "FRAME_EMPTY") {
                next.state = "searching";
                next.stableFrames = 0;
            } else if (event.type === "MANUAL_CAPTURE") {
                next.state = "processing";
                next.captureId = (snapshot.captureId ?? 0) + 1;
                next.stableFrames = 0;
                actions.push({ kind: "capture", captureId: next.captureId, manual: true });
            } else if (event.type === "PAUSE") {
                next.state = "paused";
                next.stableFrames = 0;
            } else if (event.type === "END") {
                next.state = "ended";
                actions.push({ kind: "releaseEngine" });
            }
            break;

        case "processing":
            if (event.type === "CAPTURE_DONE" && event.captureId === snapshot.captureId) {
                next.state = "waitingRemoval";
                next.captureId = null;
                next.absentFrames = 0;
                next.acceptedCount = snapshot.acceptedCount + 1;
            } else if (event.type === "CAPTURE_FAILED") {
                // فشل الجودة/القص: تعليمات ثم إعادة محاولة — لا انتقال للوثيقة التالية
                next.state = "searching";
                next.captureId = null;
            } else if (event.type === "END") {
                // نتيجة متأخرة بعد الإغلاق يرفضها الخطاف بمقارنة captureId؛ هنا ننهي فقط
                next.state = "ended";
                next.captureId = null;
                actions.push({ kind: "releaseEngine" });
            }
            // نتائج captureId قديمة/مختلفة تُتجاهل بصمت (قفل الحدث)
            break;

        case "waitingRemoval":
            if (event.type === "FRAME_EMPTY") {
                next.absentFrames = snapshot.absentFrames + 1;
            } else if (event.type === "FRAME_QUAD") {
                // مضلع جديد بعد إزالة مؤكدة ← المسار المعتاد عبر searching
                if (snapshot.absentFrames >= config.absentFramesRequired) {
                    next.state = "searching";
                    next.absentFrames = 0;
                    next.stableFrames = 0;
                }
                // قبل اكتمال الغياب: فقدان لحظي سابق أو الورقة نفسها — تجاهل
            } else if (event.type === "PAUSE") next.state = "paused";
            else if (event.type === "END") {
                next.state = "ended";
                actions.push({ kind: "releaseEngine" });
            }
            break;

        case "reviewPause":
            if (event.type === "END") {
                next.state = "ended";
                actions.push({ kind: "releaseEngine" });
            }
            break;

        case "paused":
            if (event.type === "RESUME") next.state = "searching";
            else if (event.type === "END") {
                next.state = "ended";
                actions.push({ kind: "releaseEngine" });
            }
            break;

        case "ended":
            break;
    }

    return { snapshot: next, actions };
}

/** هل وصلت الجلسة لحد العناصر القابل للضبط؟ (يفحصه الخطاف قبل قبول عنصر جديد) */
export function hasReachedItemCap(snapshot: ScanSnapshot, config: ScanMachineConfig = DEFAULT_SCAN_CONFIG): boolean {
    return snapshot.acceptedCount >= config.maxItems;
}
