import assert from "node:assert/strict";
import { test } from "node:test";

import { scalePoints } from "./geometry.ts";

test("scalePoints يحجّم المحورين independently", () => {
    const out = scalePoints([{ x: 10, y: 20 }], 100, 200, 200, 100);
    assert.deepEqual(out, [{ x: 20, y: 10 }]);
});

test("scalePoints بأبعاد صفرية يعيد نسخة دون تغيير", () => {
    const out = scalePoints([{ x: 5, y: 5 }], 0, 0, 200, 200);
    assert.deepEqual(out, [{ x: 5, y: 5 }]);
});
