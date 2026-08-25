import { CPUProcessor } from "../lib/cpu-processor";

interface WarpRequest {
    type: "warp";
    srcData: Uint8ClampedArray;
    srcWidth: number;
    srcHeight: number;
    corners: { x: number; y: number }[];
}

interface EnhanceRequest {
    type: "enhance";
    data: Uint8ClampedArray;
    width: number;
    height: number;
}

interface CombinedRequest {
    type: "combined";
    srcData: Uint8ClampedArray;
    srcWidth: number;
    srcHeight: number;
    corners: { x: number; y: number }[];
}

type WorkerRequest = WarpRequest | EnhanceRequest | CombinedRequest;

const processor = new CPUProcessor({
    maxDimension: 1536,
    enhanceAmount: 0.3,
});

self.onmessage = (e: MessageEvent<WorkerRequest>) => {
    const msg = e.data;

    if (msg.type === "combined") {
        // Combined warp + enhance in single operation (optimized)
        const result = processor.process(
            msg.srcData,
            msg.srcWidth,
            msg.srcHeight,
            msg.corners,
        );
        self.postMessage(
            {
                type: "combined-done",
                data: result.data,
                width: result.width,
                height: result.height,
            },
            { transfer: [result.data.buffer] },
        );
    } else if (msg.type === "warp") {
        // Legacy warp-only (for backward compatibility)
        const result = processor.process(
            msg.srcData,
            msg.srcWidth,
            msg.srcHeight,
            msg.corners,
        );
        self.postMessage(
            {
                type: "warp-done",
                data: result.data,
                width: result.width,
                height: result.height,
            },
            { transfer: [result.data.buffer] },
        );
    } else if (msg.type === "enhance") {
        // Legacy enhance-only (for backward compatibility)
        // Note: This is now less efficient since we combined operations
        // Consider migrating all callers to use "combined" type
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
};
