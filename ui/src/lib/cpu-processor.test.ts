import assert from "node:assert/strict";
import { test } from "node:test";

import { CPUProcessor } from "./cpu-processor.ts";

function gradientImage(w: number, h: number): Uint8ClampedArray {
    const pixels = new Uint8ClampedArray(w * h * 4);
    for (let y = 0; y < h; y++) {
        for (let x = 0; x < w; x++) {
            pixels.set([x, y, 80, 255], (y * w + x) * 4);
        }
    }
    return pixels;
}

const noopProcessor = () => new CPUProcessor({ enhanceAmount: 0, maxDimension: 4096 });

test("rect crop يلتقط المنطقة غير المتناظرة بالاتجاه الصحيح", () => {
    const w = 101, h = 101;
    const r = noopProcessor().process(
        gradientImage(w, h),
        w,
        h,
        [
            { x: 10, y: 5 },
            { x: 80, y: 5 },
            { x: 80, y: 45 },
            { x: 10, y: 45 },
        ],
    );
    assert.equal(r.width, 70);
    assert.equal(r.height, 40);
    const top = [r.data[0], r.data[1]];
    const last = (r.width * r.height - 1) * 4;
    const bottom = [r.data[last], r.data[last + 1]];
    assert.deepEqual(top, [10, 5], "أعلى يسار = صورة (10,5)");
    assert.deepEqual(bottom, [80, 45], "أسفل يمين = صورة (80,45)");
});

test("rect crop مع تكبير maxDimension يحافظ على الاتجاه", () => {
    const w = 101, h = 101;
    const r = new CPUProcessor({ enhanceAmount: 0, maxDimension: 768 }).process(
        gradientImage(w, h),
        w,
        h,
        [
            { x: 0, y: 0 },
            { x: 100, y: 0 },
            { x: 100, y: 100 },
            { x: 0, y: 100 },
        ],
    );
    assert.equal(r.width, 100);
    assert.equal(r.height, 100);
    assert.deepEqual([r.data[0], r.data[1]], [0, 0]);
});

test("القائسة المركزية للشكل المنظوري = 38 (projective لا bilinear)", () => {
    const w = 101, h = 101;
    const r = noopProcessor().process(
        gradientImage(w, h),
        w,
        h,
        [
            { x: 20, y: 0 },
            { x: 80, y: 0 },
            { x: 100, y: 100 },
            { x: 0, y: 100 },
        ],
    );
    const mid = (Math.floor(r.height / 2) * r.width + Math.floor(r.width / 2)) * 4;
    assert.equal(r.data[mid + 1], 38);
});

test("يرمي خطأً بالعربية عند الزوايا المعيبة بدل إرجاع الأصل", () => {
    const w = 101, h = 101;
    const px = gradientImage(w, h);
    assert.throws(() => noopProcessor().process(px, w, h, Array(4).fill({ x: 20, y: 20 })));
    assert.throws(() =>
        noopProcessor().process(
            px,
            w,
            h,
            [
                { x: 0, y: 0 },
                { x: 100, y: 100 },
                { x: 100, y: 0 },
                { x: 0, y: 100 },
            ],
        ),
    );
    assert.throws(() => noopProcessor().process(px, w, h, []));
});

test("processWithProgress يرفض الزوايا المعيبة أيضاً", () => {
    const w = 101, h = 101;
    assert.throws(() =>
        noopProcessor().processWithProgress(gradientImage(w, h), w, h, Array(4).fill({ x: 20, y: 20 })),
    );
});
