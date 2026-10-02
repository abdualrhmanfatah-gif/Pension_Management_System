import { CPUProcessor } from "../lib/cpu-processor";

interface CombinedRequest {
    type: "combined";
    srcData: Uint8ClampedArray;
    srcWidth: number;
    srcHeight: number;
    corners: { x: number; y: number }[];
    maxDimension?: number;
    maxPixels?: number;
}

interface EnhanceRequest {
    type: "enhance";
    data: Uint8ClampedArray;
    width: number;
    height: number;
}

type WorkerRequest = CombinedRequest | EnhanceRequest;

function runCombined(msg: CombinedRequest) {
    const processor = new CPUProcessor({
        maxDimension: msg.maxDimension ?? 1536,
        maxPixels: msg.maxPixels,
        enhanceAmount: 0.3,
    });
    return processor.process(msg.srcData, msg.srcWidth, msg.srcHeight, msg.corners);
}

self.onmessage = (e: MessageEvent<WorkerRequest>) => {
    const msg = e.data;

    try {
        if (msg.type === "combined") {
            const result = runCombined(msg);
            self.postMessage(
                {
                    type: "combined-done",
                    data: result.data,
                    width: result.width,
                    height: result.height,
                },
                { transfer: [result.data.buffer] },
            );
        } else if (msg.type === "enhance") {
            const processor = new CPUProcessor({
                maxDimension: 1536,
                enhanceAmount: 0.3,
            });
            const result = processor.process(
                msg.data,
                msg.width,
                msg.height,
                [{ x: 0, y: 0 }, { x: msg.width, y: 0 }, { x: msg.width, y: msg.height }, { x: 0, y: msg.height }],
            );
            self.postMessage(
                {
                    type: "enhance-done",
                    data: result.data,
                    width: result.width,
                    height: result.height,
                },
                { transfer: [result.data.buffer] },
            );
        }
    } catch (error) {
        self.postMessage({
            type: "error",
            message: error instanceof Error ? error.message : "فشلت معالجة الصورة",
        });
    }
};
