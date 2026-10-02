import type { PixelBudget } from "./crop-region.ts";
import type { Point } from "./homography.ts";
import type { PixelBuffer, ProcessResult } from "./pixel-buffer.ts";

export type WarpResult = ProcessResult;

export { type PixelBuffer };

/** مهلة المعالجة داخل الـWorker انتهت — لا إعادة محاولة بعده */
export class WorkerTimeoutError extends Error {
    constructor(message = "انتهت مهلة المعالجة") {
        super(message);
        this.name = "WorkerTimeoutError";
    }
}

/** عطل قابل للاسترداد (crash أو استجابة غير صالحة) — يستحق إعادة محاولة واحدة */
export class WorkerFailureError extends Error {
    constructor(message: string) {
        super(message);
        this.name = "WorkerFailureError";
    }
}

export function createCropWorker(): Worker | null {
    try {
        return new Worker(
            new URL("../workers/perspective-warp.worker.ts", import.meta.url),
            { type: "module" },
        );
    } catch (error) {
        console.warn("تعذر إنشاء Worker المعالجة:", error);
        return null;
    }
}

/**
 * إرسال القص إلى Worker مع مهلة حقيقية وإلغاء فوري.
 * انتهاء المهلة يرفض النتيجة، وعلى المستدعي إنهاء الـ Worker (terminate)
 * لأن حلقة CPU داخله لا تُلغى من الخيط الرئيسي.
 * النسخة تُرسل بـ structured clone ليبقى المخزن الأصلي صالحاً لمسار الرجوع CPU،
 * والنتيجة تُنقل كـTransferable (لا نسخة إضافية في الخيط الرئيسي).
 */
export function warpInWorker(
    worker: Worker,
    srcData: Uint8ClampedArray,
    srcWidth: number,
    srcHeight: number,
    corners: Point[],
    budget: PixelBudget,
    timeoutMs: number,
    signal?: AbortSignal,
): Promise<WarpResult> {
    return new Promise<WarpResult>((resolve, reject) => {
        let settled = false;
        const finish = (action: () => void) => {
            if (settled) return;
            settled = true;
            clearTimeout(timer);
            if (signal) signal.removeEventListener("abort", onAbort);
            worker.onmessage = null;
            worker.onerror = null;
            action();
        };

        const timer = setTimeout(() => {
            finish(() => reject(new WorkerTimeoutError()));
        }, timeoutMs);

        const onAbort = () => {
            finish(() => reject(new WorkerFailureError("تم إلغاء المعالجة")));
        };

        if (signal) {
            if (signal.aborted) {
                finish(() => reject(new WorkerFailureError("تم إلغاء المعالجة")));
                return;
            }
            signal.addEventListener("abort", onAbort, { once: true });
        }

        worker.onmessage = (e: MessageEvent) => {
            const msg = e.data as {
                type: string;
                data?: PixelBuffer;
                width?: number;
                height?: number;
                message?: string;
            };
            if (msg.type === "error") {
                finish(() => reject(new WorkerFailureError(msg.message || "فشلت معالجة الصورة")));
            } else if (
                msg.type === "combined-done" &&
                msg.data &&
                typeof msg.width === "number" &&
                typeof msg.height === "number"
            ) {
                finish(() => resolve({ data: msg.data as PixelBuffer, width: msg.width!, height: msg.height! }));
            } else {
                finish(() => reject(new WorkerFailureError("استجابة غير صالحة من Worker المعالجة")));
            }
        };

        worker.onerror = (e) => {
            finish(() => reject(new WorkerFailureError(e.message || "خطأ في Worker المعالجة")));
        };

        worker.postMessage({
            type: "combined",
            srcData,
            srcWidth,
            srcHeight,
            corners,
            maxDimension: budget.maxDimension,
            maxPixels: budget.maxPixels,
        });
    });
}
