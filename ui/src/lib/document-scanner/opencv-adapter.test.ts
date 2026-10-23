import assert from "node:assert/strict";
import { test } from "node:test";

import {
    orderQuadPoints,
    pickBestQuad,
    releaseScanEngine,
} from "./opencv-adapter.ts";

test("orderQuadPoints يرتب مستطيلًا مائلًا ترتيبًا صحيحًا", () => {
    const ordered = orderQuadPoints([
        { x: 90, y: 50 },
        { x: 10, y: 10 },
        { x: 20, y: 60 },
        { x: 100, y: 20 },
    ]);
    assert.deepEqual(ordered, [
        { x: 10, y: 10 },
        { x: 100, y: 20 },
        { x: 90, y: 50 },
        { x: 20, y: 60 },
    ]);
});

test("orderQuadPoints يرفض غير الرباعي", () => {
    assert.equal(orderQuadPoints([{ x: 0, y: 0 }]), null);
});

test("pickBestQuad يختار الأكبر ضمن النطاق ويرفض خارج النطاق", () => {
    const small = [
        { x: 0, y: 0 },
        { x: 10, y: 0 },
        { x: 10, y: 10 },
        { x: 0, y: 10 },
    ];
    const big = [
        { x: 10, y: 10 },
        { x: 190, y: 10 },
        { x: 190, y: 90 },
        { x: 10, y: 90 },
    ];
    const full = [
        { x: 0, y: 0 },
        { x: 200, y: 0 },
        { x: 200, y: 100 },
        { x: 0, y: 100 },
    ];
    const best = pickBestQuad([small, big, full], 200, 100);
    assert.ok(best);
    assert.ok(best.areaRatio > 0.5 && best.areaRatio < 0.95);
    assert.equal(pickBestQuad([small], 200, 100), null);
    assert.equal(pickBestQuad([full], 200, 100), null);
    assert.equal(pickBestQuad([], 200, 100), null);
});

test("releaseScanEngine لجلسة مجهولة لا يرمي", () => {
    releaseScanEngine("no-such-session");
});
