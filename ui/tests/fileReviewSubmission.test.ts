import assert from "node:assert/strict";
import { test } from "node:test";

import { submitAndConfirmFileReview } from "../src/lib/fileReviewSubmission.ts";

test("a successful HTTP response with unchanged draft state is not a successful submission", async () => {
    await assert.rejects(submitAndConfirmFileReview(
        async () => ({ isSuccess: true, data: true, message: "الملف جديد؛ أرسله بعد التجهيز" }),
        async () => ({ statusID: 1, reviewStatus: 0 }),
    ), /لم يؤكد الخادم/);
});

test("a business failure never triggers a success confirmation or a state read", async () => {
    let reads = 0;
    await assert.rejects(submitAndConfirmFileReview(
        async () => ({ isSuccess: false, data: false, message: "خارج نطاق صلاحياتك" }),
        async () => { reads++; return { statusID: 6, reviewStatus: 1 }; },
    ), /خارج نطاق صلاحياتك/);
    assert.equal(reads, 0);
});

test("a pending file with rejected review is not confirmed as resubmitted", async () => {
    await assert.rejects(submitAndConfirmFileReview(
        async () => ({ isSuccess: true, data: true }),
        async () => ({ statusID: 6, reviewStatus: 3 }),
    ), /لم يؤكد الخادم/);
});

test("submission is confirmed only after reading the persisted pending review", async () => {
    const calls: string[] = [];
    const response = await submitAndConfirmFileReview(
        async () => { calls.push("submit"); return { isSuccess: true, data: true, message: "تم الإرسال" }; },
        async () => { calls.push("read"); return { statusID: 6, reviewStatus: 1 }; },
    );
    assert.deepEqual(calls, ["submit", "read"]);
    assert.equal(response.message, "تم الإرسال");
});

test("a failed confirmation read never claims submission success", async () => {
    await assert.rejects(submitAndConfirmFileReview(
        async () => ({ isSuccess: true, data: true }),
        async () => { throw new Error("تعذرت قراءة حالة الملف"); },
    ), /تعذرت قراءة/);
});
