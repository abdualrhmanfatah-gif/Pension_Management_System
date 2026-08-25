export interface CPUProcessorOptions {
    maxDimension?: number;
    enhanceAmount?: number;
}

export class CPUProcessor {
    private options: CPUProcessorOptions;

    constructor(options: CPUProcessorOptions = {}) {
        this.options = {
            maxDimension: 1536,
            enhanceAmount: 0.3,
            ...options,
        };
    }

    process(
        srcData: Uint8ClampedArray,
        srcWidth: number,
        srcHeight: number,
        corners: { x: number; y: number }[],
    ): { data: Uint8ClampedArray; width: number; height: number } {
        const [tl, tr, br, bl] = corners;

        // Calculate output dimensions
        const wTop = Math.hypot(tr.x - tl.x, tr.y - tl.y);
        const wBot = Math.hypot(br.x - bl.x, br.y - bl.y);
        const outW = Math.round((wTop + wBot) / 2);
        const hL = Math.hypot(bl.x - tl.x, bl.y - tl.y);
        const hR = Math.hypot(br.x - tr.x, br.y - tr.y);
        const outH = Math.round((hL + hR) / 2);

        if (outW < 1 || outH < 1) {
            return { data: new Uint8ClampedArray(srcData), width: srcWidth, height: srcHeight };
        }

        // Scale to fit max dimension
        const scale = Math.min(1, this.options.maxDimension! / Math.max(outW, outH));
        const w = Math.max(1, Math.round(outW * scale));
        const h = Math.max(1, Math.round(outH * scale));

        // Combined warp + enhance in single pass
        return this.warpAndEnhance(srcData, srcWidth, srcHeight, tl, tr, br, bl, w, h);
    }

    private warpAndEnhance(
        srcData: Uint8ClampedArray,
        srcWidth: number,
        srcHeight: number,
        tl: { x: number; y: number },
        tr: { x: number; y: number },
        br: { x: number; y: number },
        bl: { x: number; y: number },
        outW: number,
        outH: number,
    ): { data: Uint8ClampedArray; width: number; height: number } {
        const outData = new Uint8ClampedArray(outW * outH * 4);
        const blurredData = new Uint8ClampedArray(outW * outH * 4);

        const kernel = [0.25, 0.5, 0.25];
        const amount = this.options.enhanceAmount!;

        // Single pass: warp + horizontal blur
        for (let dy = 0; dy < outH; dy++) {
            for (let dx = 0; dx < outW; dx++) {
                const u = dx / (outW - 1 || 1);
                const v = dy / (outH - 1 || 1);

                const sx =
                    (1 - u) * (1 - v) * tl.x +
                    u * (1 - v) * tr.x +
                    u * v * br.x +
                    (1 - u) * v * bl.x;
                const sy =
                    (1 - u) * (1 - v) * tl.y +
                    u * (1 - v) * tr.y +
                    u * v * br.y +
                    (1 - u) * v * bl.y;

                const csx = Math.max(0, Math.min(srcWidth - 1, sx));
                const csy = Math.max(0, Math.min(srcHeight - 1, sy));

                const sx0 = Math.floor(csx);
                const sy0 = Math.floor(csy);
                const sx1 = Math.min(sx0 + 1, srcWidth - 1);
                const sy1 = Math.min(sy0 + 1, srcHeight - 1);
                const fx = csx - sx0;
                const fy = csy - sy0;
                const fx1 = 1 - fx;
                const fy1 = 1 - fy;

                const i00 = (sy0 * srcWidth + sx0) * 4;
                const i10 = (sy0 * srcWidth + sx1) * 4;
                const i01 = (sy1 * srcWidth + sx0) * 4;
                const i11 = (sy1 * srcWidth + sx1) * 4;
                const di = (dy * outW + dx) * 4;

                // Warp with bilinear interpolation
                for (let c = 0; c < 3; c++) {
                    outData[di + c] = Math.round(
                        fx1 * fy1 * srcData[i00 + c] +
                        fx * fy1 * srcData[i10 + c] +
                        fx1 * fy * srcData[i01 + c] +
                        fx * fy * srcData[i11 + c],
                    );
                }
                outData[di + 3] = 255;

                // Horizontal blur (combined)
                let hSum = 0;
                for (let k = 0; k < 3; k++) {
                    const sx = Math.min(outW - 1, Math.max(0, dx + k - 1));
                    hSum += outData[(dy * outW + sx) * 4] * kernel[k];
                }
                blurredData[di] = hSum;
                blurredData[di + 1] = hSum;
                blurredData[di + 2] = hSum;
                blurredData[di + 3] = 255;
            }
        }

        // Vertical blur + unsharp mask (combined)
        const finalData = new Uint8ClampedArray(outW * outH * 4);
        for (let y = 0; y < outH; y++) {
            for (let x = 0; x < outW; x++) {
                const base = (y * outW + x) * 4;

                // Vertical blur
                let vSum = 0;
                for (let k = 0; k < 3; k++) {
                    const sy = Math.min(outH - 1, Math.max(0, y + k - 1));
                    vSum += blurredData[(sy * outW + x) * 4] * kernel[k];
                }

                // Unsharp mask
                for (let c = 0; c < 3; c++) {
                    const diff = outData[base + c] - vSum;
                    finalData[base + c] = Math.min(
                        255,
                        Math.max(0, Math.round(outData[base + c] + diff * amount)),
                    );
                }
                finalData[base + 3] = 255;
            }
        }

        return { data: finalData, width: outW, height: outH };
    }

    processWithProgress(
        srcData: Uint8ClampedArray,
        srcWidth: number,
        srcHeight: number,
        corners: { x: number; y: number }[],
        onProgress?: (progress: number) => void,
    ): { data: Uint8ClampedArray; width: number; height: number } {
        const [tl, tr, br, bl] = corners;

        const wTop = Math.hypot(tr.x - tl.x, tr.y - tl.y);
        const wBot = Math.hypot(br.x - bl.x, br.y - bl.y);
        const outW = Math.round((wTop + wBot) / 2);
        const hL = Math.hypot(bl.x - tl.x, bl.y - tl.y);
        const hR = Math.hypot(br.x - tr.x, br.y - tr.y);
        const outH = Math.round((hL + hR) / 2);

        if (outW < 1 || outH < 1) {
            return { data: new Uint8ClampedArray(srcData), width: srcWidth, height: srcHeight };
        }

        const scale = Math.min(1, this.options.maxDimension! / Math.max(outW, outH));
        const w = Math.max(1, Math.round(outW * scale));
        const h = Math.max(1, Math.round(outH * scale));

        const outData = new Uint8ClampedArray(w * h * 4);
        const blurredData = new Uint8ClampedArray(w * h * 4);

        const kernel = [0.25, 0.5, 0.25];
        const amount = this.options.enhanceAmount!;
        const totalPixels = w * h;
        let processedPixels = 0;

        // Warp + horizontal blur
        for (let dy = 0; dy < h; dy++) {
            for (let dx = 0; dx < w; dx++) {
                const u = dx / (w - 1 || 1);
                const v = dy / (h - 1 || 1);

                const sx =
                    (1 - u) * (1 - v) * tl.x +
                    u * (1 - v) * tr.x +
                    u * v * br.x +
                    (1 - u) * v * bl.x;
                const sy =
                    (1 - u) * (1 - v) * tl.y +
                    u * (1 - v) * tr.y +
                    u * v * br.y +
                    (1 - u) * v * bl.y;

                const csx = Math.max(0, Math.min(srcWidth - 1, sx));
                const csy = Math.max(0, Math.min(srcHeight - 1, sy));

                const sx0 = Math.floor(csx);
                const sy0 = Math.floor(csy);
                const sx1 = Math.min(sx0 + 1, srcWidth - 1);
                const sy1 = Math.min(sy0 + 1, srcHeight - 1);
                const fx = csx - sx0;
                const fy = csy - sy0;
                const fx1 = 1 - fx;
                const fy1 = 1 - fy;

                const i00 = (sy0 * srcWidth + sx0) * 4;
                const i10 = (sy0 * srcWidth + sx1) * 4;
                const i01 = (sy1 * srcWidth + sx0) * 4;
                const i11 = (sy1 * srcWidth + sx1) * 4;
                const di = (dy * w + dx) * 4;

                for (let c = 0; c < 3; c++) {
                    outData[di + c] = Math.round(
                        fx1 * fy1 * srcData[i00 + c] +
                        fx * fy1 * srcData[i10 + c] +
                        fx1 * fy * srcData[i01 + c] +
                        fx * fy * srcData[i11 + c],
                    );
                }
                outData[di + 3] = 255;

                let hSum = 0;
                for (let k = 0; k < 3; k++) {
                    const sx = Math.min(w - 1, Math.max(0, dx + k - 1));
                    hSum += outData[(dy * w + sx) * 4] * kernel[k];
                }
                blurredData[di] = hSum;
                blurredData[di + 1] = hSum;
                blurredData[di + 2] = hSum;
                blurredData[di + 3] = 255;

                processedPixels++;
                if (onProgress && processedPixels % 10000 === 0) {
                    onProgress(processedPixels / totalPixels * 0.5);
                }
            }
        }

        // Vertical blur + unsharp mask
        const finalData = new Uint8ClampedArray(w * h * 4);
        for (let y = 0; y < h; y++) {
            for (let x = 0; x < w; x++) {
                const base = (y * w + x) * 4;

                let vSum = 0;
                for (let k = 0; k < 3; k++) {
                    const sy = Math.min(h - 1, Math.max(0, y + k - 1));
                    vSum += blurredData[(sy * w + x) * 4] * kernel[k];
                }

                for (let c = 0; c < 3; c++) {
                    const diff = outData[base + c] - vSum;
                    finalData[base + c] = Math.min(
                        255,
                        Math.max(0, Math.round(outData[base + c] + diff * amount)),
                    );
                }
                finalData[base + 3] = 255;

                processedPixels++;
                if (onProgress && processedPixels % 10000 === 0) {
                    onProgress(0.5 + (processedPixels / totalPixels) * 0.5);
                }
            }
        }

        if (onProgress) onProgress(1);

        return { data: finalData, width: w, height: h };
    }
}
