/**
 * تشخيص أخطاء الكاميرا برسائل عربية قابلة للفعل — دوال خالصة قابلة للاختبار.
 * السبب الأشيع على الهاتف: صفحة HTTP عبر IP شبكي ليست secure context
 * فيرفض المتصفح getUserMedia حتى مع وجود كاميرا وصلاحية.
 */

export interface CameraPrerequisites {
    isSecureContext: boolean;
    hasMediaDevices: boolean;
    hasGetUserMedia: boolean;
}

/** فحص مسبق قبل طلب البث — يعيد رسالة عربية عند العائق أو null عند الجاهزية */
export function checkCameraPrerequisites(prereqs: CameraPrerequisites): string | null {
    if (!prereqs.isSecureContext) {
        return "الكاميرا محظورة على HTTP غير آمن. افتح الصفحة عبر HTTPS أو من نفس الجهاز عبر localhost.";
    }
    if (!prereqs.hasMediaDevices || !prereqs.hasGetUserMedia) {
        return "المتصفح لا يدعم الوصول للكاميرا. حدّث المتصفح أو جرّب Chrome/Safari حديثًا.";
    }
    return null;
}

/** رسالة عربية لخطأ getUserMedia حسب اسمه */
export function describeCameraError(errorName: string): string {
    switch (errorName) {
        case "NotAllowedError":
            return "تم رفض صلاحية الكاميرا. اسمح بالوصول من إعدادات المتصفح ثم أعد المحاولة.";
        case "NotFoundError":
            return "لا توجد كاميرا متاحة على هذا الجهاز.";
        case "NotReadableError":
            return "الكاميرا مشغولة بتطبيق آخر. أغلقه ثم أعد المحاولة.";
        case "OverconstrainedError":
            return "الكاميرا لا تدعم الإعدادات المطلوبة. جرّب تبديل الكاميرا.";
        case "SecurityError":
            return "الكاميرا محظورة في هذا السياق. افتح الصفحة عبر HTTPS أو localhost.";
        case "AbortError":
            return "أُلغي طلب الكاميرا. أعد فتح الجلسة.";
        default:
            return "تعذر الوصول إلى الكاميرا أو قد تكون غير متصلة.";
    }
}

export function readErrorName(error: unknown): string {
    if (error instanceof DOMException) return error.name;
    if (error instanceof Error && error.name) return error.name;
    return "UnknownError";
}
