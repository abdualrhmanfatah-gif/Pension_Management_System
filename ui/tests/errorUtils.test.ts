import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { normalizeApiError } from "../src/lib/errorUtils.ts";

/** ينسخ شكل خطأ Axios: response.status + response.data */
function httpError(status: number, data: unknown): unknown {
    return { isAxiosError: true, request: {}, response: { status, data } };
}

describe("normalizeApiError", () => {
    it("ValidationProblemDetails: يعرض سبب الرفض على مستوى الطلب بلا اسم حقل", () => {
        // bodies réel من POST /api/documents
        const err = httpError(400, {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: { "": ["يجب تحديد مالك الوثيقة"] },
            traceId: "00-170a09f30824c94b-00",
        });

        const result = normalizeApiError(err);

        assert.equal(result.message, "يجب تحديد مالك الوثيقة");
        assert.equal(result.status, 400);
        // no stray colon from the empty field name
        assert.ok(!result.message.includes(":"));
    });

    it("ValidationProblemDetails بمفتاح $ (نمط MVC لأخطاء الجذر)", () => {
        const err = httpError(400, {
            title: "One or more validation errors occurred.",
            status: 400,
            errors: { $: ["'u' is an invalid start of a property name."] },
        });

        const result = normalizeApiError(err);

        assert.equal(result.message, "'u' is an invalid start of a property name.");
    });

    it("أخطاء الحقول تُسمّى بأسماء حقولها", () => {
        const err = httpError(400, {
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
                DocumentTypeID: ["نوع الوثيقة مطلوب"],
                File: ["The File field is required."],
            },
        });

        const result = normalizeApiError(err);

        assert.ok(result.message.includes("DocumentTypeID: نوع الوثيقة مطلوب"));
        assert.ok(result.message.includes("File: The File field is required."));
        assert.deepEqual(result.errors, ["نوع الوثيقة مطلوب", "The File field is required."]);
    });

    it("يرفض العنوان الإنجليزي ويسقط لرسالة عربية", () => {
        const err = httpError(400, {
            title: "One or more validation errors occurred.",
            status: 400,
        });

        const result = normalizeApiError(err);

        // لا رسالة إنجليزية للمستخدم
        assert.equal(result.message, "البيانات المُدخلة غير صالحة");
    });

    it("يقبل العنوان إذا كان بالعربية", () => {
        const err = httpError(400, { title: "طلب غير صالح" });

        assert.equal(normalizeApiError(err).message, "طلب غير صالح");
    });

    it("message من ApiResponse يبقى أولوية على errors", () => {
        const err = httpError(400, {
            isSuccess: false,
            message: "نوع الوثيقة لا يسمح بالرفع لهذا المالك",
            errors: { "": ["سبب آخر"] },
        });

        const result = normalizeApiError(err);

        assert.equal(result.message, "نوع الوثيقة لا يسمح بالرفع لهذا المالك");
    });

    it("أخطاء ApiResponse بصيغة مصفوفة تُعرض كقائمة", () => {
        const err = httpError(400, {
            isSuccess: false,
            message: null,
            errors: ["الخطأ الأول", "الخطأ الثاني"],
        });

        const result = normalizeApiError(err);

        assert.equal(result.message, "الخطأ الأول و الخطأ الثاني");
    });

    it("ينظّف وسوم HTML من الرسالة", () => {
        const err = httpError(400, {
            message: "<b>نوع الوثيقة</b> غير صالح",
        });

        assert.equal(normalizeApiError(err).message, "نوع الوثيقة غير صالح");
    });

    it("يسقط إلى رسالة عربية للحالات بلا سبب", () => {
        assert.equal(normalizeApiError(httpError(400, {})).message, "البيانات المُدخلة غير صالحة");
        assert.equal(normalizeApiError(httpError(409, {})).message, "تعارض في البيانات — يرجى تحديث الصفحة والمحاولة مجدداً");
        assert.equal(normalizeApiError(httpError(403, {})).message, "ليس لديك صلاحية لتنفيذ هذا الإجراء");
    });
});
