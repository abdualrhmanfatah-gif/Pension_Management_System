import assert from "node:assert/strict";
import { test } from "node:test";

import {
    applyHomography,
    computeHomography,
    validateCorners,
} from "./homography.ts";

const rect = [
    { x: 10, y: 5 },
    { x: 80, y: 5 },
    { x: 80, y: 45 },
    { x: 10, y: 45 },
];

test("validateCorners يقبل مستطيلاً صحيحاً", () => {
    assert.equal(validateCorners(rect), null);
});

test("validateCorners يرفض الزوايا المتقطعة (bow-tie)", () => {
    const crossed = [
        { x: 0, y: 0 },
        { x: 100, y: 100 },
        { x: 100, y: 0 },
        { x: 0, y: 100 },
    ];
    assert.notEqual(validateCorners(crossed), null);
});

test("validateCorners يرفض الزوايا المتطابقة والمنطقية", () => {
    assert.notEqual(validateCorners(Array(4).fill({ x: 20, y: 20 })), null);
    assert.notEqual(
        validateCorners([
            { x: 0, y: 0 },
            { x: 50, y: 0 },
            { x: 9, y: 0 },
            { x: 0, y: 9 },
        ]),
        null,
    );
    assert.notEqual(validateCorners([]), null);
});

test("computeHomography ينقل مربع الوحدة إلى الزوايا بدقة (topLeftV=0)", () => {
    const h = computeHomography(rect, 0);
    assert.ok(h, "homography must not be null");
    const cases: Array<[number, number, { x: number; y: number }]> = [
        [0, 0, { x: 10, y: 5 }], // tl
        [1, 0, { x: 80, y: 5 }], // tr
        [1, 1, { x: 80, y: 45 }], // br
        [0, 1, { x: 10, y: 45 }], // bl
    ];
    for (const [u, v, expected] of cases) {
        const p = applyHomography(h, u, v);
        assert.ok(p);
        assert.ok(Math.abs(p.x - expected.x) < 1e-6, `x at (${u},${v})`);
        assert.ok(Math.abs(p.y - expected.y) < 1e-6, `y at (${u},${v})`);
    }
});

test("computeHomography topLeftV=1 يقلب اتجاه v (مخرج WebGL)", () => {
    const h = computeHomography(rect, 1);
    assert.ok(h);
    const top = applyHomography(h, 0, 1); // أعلى المخرج v=1
    const bottom = applyHomography(h, 0, 0);
    assert.ok(top && bottom);
    assert.ok(Math.abs(top.y - 5) < 1e-6, "v=1 يجب أن يقرأ أعلى المنطقة");
    assert.ok(Math.abs(bottom.y - 45) < 1e-6, "v=0 يجب أن يقرأ أسفل المنطقة");
});

test("computeHomography يتعامل مع الشكل المنظوري (trapezoid)", () => {
    const trapezoid = [
        { x: 20, y: 0 },
        { x: 80, y: 0 },
        { x: 100, y: 100 },
        { x: 0, y: 100 },
    ];
    const h = computeHomography(trapezoid, 0);
    assert.ok(h);
    // منتصف الناتج (0.5,0.5): ارتفاع منظوري متوقع 38 (مطابق للـ probe)
    const mid = applyHomography(h, 0.5, 0.5);
    assert.ok(mid);
    assert.ok(Math.abs(mid.y - 38) < 1.5, `mid.y=${mid.y}`);
    assert.ok(Math.abs(mid.x - 50) < 1.5, `mid.x=${mid.x}`);
});

test("computeHomography يرجع null عند الزوايا المعيبة", () => {
    assert.equal(computeHomography(Array(4).fill({ x: 20, y: 20 }), 0), null);
    assert.equal(computeHomography(rect.slice(0, 3), 0), null);
});
