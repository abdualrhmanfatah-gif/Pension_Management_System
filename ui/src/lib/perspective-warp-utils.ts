export const MAX_CANVAS_DIM = 2048;

export function perspectiveWarpBilinear(
    srcData: Uint8ClampedArray,
    srcWidth: number,
    srcHeight: number,
    corners: { x: number; y: number }[],
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

    const scale = Math.min(1, MAX_CANVAS_DIM / Math.max(outW, outH));
    const w = Math.max(1, Math.round(outW * scale));
    const h = Math.max(1, Math.round(outH * scale));

    const outData = new Uint8ClampedArray(w * h * 4);

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
        }
    }

    return { data: outData, width: w, height: h };
}

export function enhanceImage(
    srcData: Uint8ClampedArray,
    width: number,
    height: number,
): { data: Uint8ClampedArray; width: number; height: number } {
    const len = width * height * 4;

    const kernel = [0.25, 0.5, 0.25];
    const kernelHalf = 1;
    const kernelSize = 3;

    // Horizontal blur
    const hBlurred = new Uint8ClampedArray(len);
    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            const base = (y * width + x) * 4;
            for (let c = 0; c < 3; c++) {
                let sum = 0;
                for (let k = 0; k < kernelSize; k++) {
                    const sx = Math.min(width - 1, Math.max(0, x + k - kernelHalf));
                    sum += srcData[(y * width + sx) * 4 + c] * kernel[k];
                }
                hBlurred[base + c] = sum;
            }
            hBlurred[base + 3] = 255;
        }
    }

    // Vertical blur
    const blurred = new Uint8ClampedArray(len);
    for (let x = 0; x < width; x++) {
        for (let y = 0; y < height; y++) {
            const base = (y * width + x) * 4;
            for (let c = 0; c < 3; c++) {
                let sum = 0;
                for (let k = 0; k < kernelSize; k++) {
                    const sy = Math.min(height - 1, Math.max(0, y + k - kernelHalf));
                    sum += hBlurred[(sy * width + x) * 4 + c] * kernel[k];
                }
                blurred[base + c] = sum;
            }
            blurred[base + 3] = 255;
        }
    }

    // Unsharp mask: output = original + (original - blurred) * amount
    const amount = 0.3;
    const out = new Uint8ClampedArray(len);
    for (let i = 0; i < len; i += 4) {
        for (let c = 0; c < 3; c++) {
            const diff = srcData[i + c] - blurred[i + c];
            out[i + c] = Math.min(255, Math.max(0, Math.round(srcData[i + c] + diff * amount)));
        }
        out[i + 3] = 255;
    }

    return { data: out, width, height };
}
