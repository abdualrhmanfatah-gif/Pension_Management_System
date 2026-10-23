import assert from "node:assert/strict";
import { test } from "node:test";

import { assessQuality } from "./quality.ts";

function makeImage(width: number, height: number, pixel: (x: number, y: number) => [number, number, number]): ImageData {
    const data = new Uint8ClampedArray(width * height * 4);
    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            const [r, g, b] = pixel(x, y);
            const i = (y * width + x) * 4;
            data[i] = r;
            data[i + 1] = g;
            data[i + 2] = b;
            data[i + 3] = 255;
        }
    }
    return { data, width, height } as ImageData;
}

test("رقعة شطرنج حادة لا تحتاج مراجعة", () => {
    const img = makeImage(40, 40, (x, y) => ((x + y) % 2 === 0 ? [30, 30, 30] : [220, 220, 220]));
    const q = assessQuality(img);
    assert.equal(q.needsReview, false);
    assert.deepEqual(q.flags, []);
});

test("مسطح رمادي: ضبابية وتباين منخفض", () => {
    const img = makeImage(40, 40, () => [128, 128, 128]);
    const q = assessQuality(img);
    assert.equal(q.needsReview, true);
    assert.ok(q.flags.includes("blur"));
    assert.ok(q.flags.includes("lowContrast"));
});

test("صورة داكنة ترفع تنبيه إضاءة خافتة", () => {
    const img = makeImage(40, 40, () => [20, 20, 20]);
    const q = assessQuality(img);
    assert.ok(q.flags.includes("dark"));
});

test("بقعة بيضاء كبيرة ترفع تنبيه لمعان", () => {
    const img = makeImage(40, 40, (x) => (x < 30 ? [250, 250, 250] : [100, 100, 100]));
    const q = assessQuality(img);
    assert.ok(q.flags.includes("glare") || q.flags.includes("bright"));
});
