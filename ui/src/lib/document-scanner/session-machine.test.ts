import assert from "node:assert/strict";
import { test } from "node:test";

import type { DetectedQuad } from "./opencv-adapter.ts";
import {
    createInitialSnapshot,
    DEFAULT_SCAN_CONFIG,
    hasReachedItemCap,
    transition,
} from "./session-machine.ts";

const CONFIG = { ...DEFAULT_SCAN_CONFIG, stableFramesRequired: 3, absentFramesRequired: 2, maxItems: 2 };

function quadEvent() {
    const quad: DetectedQuad = {
        points: [
            { x: 0, y: 0 },
            { x: 10, y: 0 },
            { x: 10, y: 10 },
            { x: 0, y: 10 },
        ],
        areaRatio: 0.5,
    };
    return { type: "FRAME_QUAD", quad } as const;
}

test("OPEN من idle يحمّل المحرك وينتقل للتهيئة", () => {
    const { snapshot, actions } = transition(createInitialSnapshot(), { type: "OPEN" });
    assert.equal(snapshot.state, "initializing");
    assert.deepEqual(actions, [{ kind: "loadEngine" }]);
});

test("الثبات لثلاث إطارات يطلق التقاطًا واحدًا بـ captureId", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    s = transition(s, { type: "ENGINE_READY" }).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    assert.equal(s.state, "stabilizing");
    s = transition(s, quadEvent(), CONFIG).snapshot;
    assert.equal(s.state, "stabilizing");
    const r = transition(s, quadEvent(), CONFIG);
    assert.equal(r.snapshot.state, "processing");
    assert.equal(r.snapshot.captureId, 1);
    assert.deepEqual(r.actions, [{ kind: "capture", captureId: 1, manual: false }]);
});

test("انقطاع الإطار أثناء الثبات يعيد للبحث", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    s = transition(s, { type: "ENGINE_READY" }).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, { type: "FRAME_EMPTY" }, CONFIG).snapshot;
    assert.equal(s.state, "searching");
    assert.equal(s.stableFrames, 0);
});

test("نتيجة captureId قديمة تُتجاهل ولا تقبل عنصرًا", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    s = transition(s, { type: "ENGINE_READY" }).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    assert.equal(s.state, "processing");
    const stale = transition(s, { type: "CAPTURE_DONE", captureId: 999 }, CONFIG);
    assert.equal(stale.snapshot.state, "processing");
    assert.equal(stale.snapshot.acceptedCount, 0);
    const fresh = transition(s, { type: "CAPTURE_DONE", captureId: 1 }, CONFIG);
    assert.equal(fresh.snapshot.state, "waitingRemoval");
    assert.equal(fresh.snapshot.acceptedCount, 1);
});

test("فشل الالتقاط يعيد للبحث ولا ينتقل للوثيقة التالية", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    s = transition(s, { type: "ENGINE_READY" }).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, { type: "CAPTURE_FAILED" }, CONFIG).snapshot;
    assert.equal(s.state, "searching");
    assert.equal(s.acceptedCount, 0);
});

test("بوابة الإزالة: إطار واحد فارغ لا يسلّح، وغياب مؤكد ثم مضلع يعيد للبحث", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    s = transition(s, { type: "ENGINE_READY" }).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    s = transition(s, { type: "CAPTURE_DONE", captureId: 1 }, CONFIG).snapshot;
    assert.equal(s.state, "waitingRemoval");
    s = transition(s, { type: "FRAME_EMPTY" }, CONFIG).snapshot;
    assert.equal(s.state, "waitingRemoval");
    s = transition(s, quadEvent(), CONFIG).snapshot;
    assert.equal(s.state, "waitingRemoval");
    s = transition(s, { type: "FRAME_EMPTY" }, CONFIG).snapshot;
    s = transition(s, quadEvent(), CONFIG).snapshot;
    assert.equal(s.state, "searching");
});

test("الالتقاط اليدوي يتجاوز الثبات من البحث", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    s = transition(s, { type: "ENGINE_READY" }).snapshot;
    const r = transition(s, { type: "MANUAL_CAPTURE" }, CONFIG);
    assert.equal(r.snapshot.state, "processing");
    assert.deepEqual(r.actions, [{ kind: "capture", captureId: 1, manual: true }]);
});

test("حد العناصر القابل للضبط يعمل", () => {
    let s = createInitialSnapshot();
    assert.equal(hasReachedItemCap(s, CONFIG), false);
    s = { ...s, acceptedCount: 2 };
    assert.equal(hasReachedItemCap(s, CONFIG), true);
    s = { ...s, state: "searching" };
    assert.deepEqual(transition(s, { type: "MANUAL_CAPTURE" }, CONFIG).actions, []);
    assert.equal(transition(s, quadEvent(), CONFIG).snapshot.state, "searching");
});

test("END من أي حالة يحرر المحرك وينهي", () => {
    let s = createInitialSnapshot();
    s = transition(s, { type: "OPEN" }).snapshot;
    const r = transition(s, { type: "END" }, CONFIG);
    assert.equal(r.snapshot.state, "ended");
    assert.deepEqual(r.actions, [{ kind: "releaseEngine" }]);
    const after = transition(r.snapshot, quadEvent(), CONFIG);
    assert.equal(after.snapshot.state, "ended");
    assert.deepEqual(after.actions, []);
});
