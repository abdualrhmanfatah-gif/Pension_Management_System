import assert from "node:assert/strict";
import test from "node:test";

import { canvasToFile, rotateCorners } from "../src/lib/document-image.ts";

const corners = [{ x: 10, y: 20 }, { x: 80, y: 160 }];

for (const { rotation, expected } of [
    { rotation: 0, expected: [{ x: 10, y: 20 }, { x: 80, y: 160 }] },
    { rotation: 90, expected: [{ x: 180, y: 10 }, { x: 40, y: 80 }] },
    { rotation: 180, expected: [{ x: 90, y: 180 }, { x: 20, y: 40 }] },
    { rotation: 270, expected: [{ x: 20, y: 90 }, { x: 160, y: 20 }] },
] as const) {
    test(`document corners retain their position after ${rotation} degree rotation`, () => {
        assert.deepEqual(rotateCorners(corners, rotation, 100, 200), expected);
        assert.deepEqual(corners, [{ x: 10, y: 20 }, { x: 80, y: 160 }]);
    });
}

for (const { original, expected } of [
    { original: "91de3467-a393-4e54-b4de-398b5b395bc9_هوية.png", expected: "هوية_scanned.jpg" },
    { original: "هوية_scanned.jpg", expected: "هوية_scanned.jpg" },
    { original: "statement.v2.png", expected: "statement.v2_scanned.jpg" },
]) {
    test(`document export preserves a usable filename for ${original}`, async () => {
        const blob = new Blob(["exported image"], { type: "image/jpeg" });
        const canvas: Pick<HTMLCanvasElement, "toBlob"> = {
            toBlob(callback, type, quality) {
                assert.equal(type, "image/jpeg");
                assert.equal(quality, 0.85);
                callback(blob);
            },
        };
        const file = await canvasToFile(canvas, original);
        assert.equal(file.name, expected);
        assert.equal(file.type, "image/jpeg");
        assert.equal(await file.text(), "exported image");
    });
}

test("document export reports failed canvas encoding", async () => {
    const canvas: Pick<HTMLCanvasElement, "toBlob"> = {
        toBlob(callback) { callback(null); },
    };
    await assert.rejects(canvasToFile(canvas, "document.png"), /فشل تحويل Canvas/);
});
