import assert from "node:assert/strict";
import { test } from "node:test";

import {
    checkCameraPrerequisites,
    describeCameraError,
    readErrorName,
} from "./camera-errors.ts";

test("سياق غير آمن يرشد لـ HTTPS أو localhost", () => {
    const message = checkCameraPrerequisites({ isSecureContext: false, hasMediaDevices: true, hasGetUserMedia: true });
    assert.ok(message?.includes("HTTPS"));
    assert.ok(message?.includes("localhost"));
});

test("غياب getUserMedia يرشد لتحديث المتصفح", () => {
    const message = checkCameraPrerequisites({ isSecureContext: true, hasMediaDevices: true, hasGetUserMedia: false });
    assert.ok(message?.includes("المتصفح"));
});

test("الجاهزية تعيد null", () => {
    assert.equal(checkCameraPrerequisites({ isSecureContext: true, hasMediaDevices: true, hasGetUserMedia: true }), null);
});

test("أسماء الأخطاء تترجم لرسائل مميزة", () => {
    assert.ok(describeCameraError("NotAllowedError").includes("صلاحية"));
    assert.ok(describeCameraError("NotFoundError").includes("لا توجد كاميرا"));
    assert.ok(describeCameraError("NotReadableError").includes("مشغولة"));
    assert.ok(describeCameraError("SecurityError").includes("HTTPS"));
    assert.ok(describeCameraError("Whatever").includes("تعذر الوصول"));
});

test("readErrorName يقرأ DOMException وغيره", () => {
    assert.equal(readErrorName(new DOMException("x", "NotAllowedError")), "NotAllowedError");
    assert.equal(readErrorName(new Error("y")), "Error");
    assert.equal(readErrorName(null), "UnknownError");
});
