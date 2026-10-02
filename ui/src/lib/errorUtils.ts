import type { AxiosError } from "axios";

// ─── Types ────────────────────────────────────────────

/** Normalized error returned to components */
export interface NormalizedApiError {
    message: string;
    errors?: string[];
    status?: number;
}

function sanitize(str: string): string {
    return str.replace(/<[^>]*>/g, "").trim();
}

/** Shape of the backend ApiResponse<T> */
interface ApiResponseShape {
    isSuccess?: boolean;
    message?: string;
    errors?: string[] | Record<string, string[] | string>;
}

/** ASP.NET ValidationProblemDetails — يحمل title لا message */
interface ValidationProblemDetailsShape {
    title?: string;
}

/** مفاتيح أخطاء الطلب نفسه (لا حقل محدد) في ASP.NET */
const ROOT_ERROR_KEYS = new Set(["", "$", "_errors"]);

function hasArabic(text: string): boolean {
    return /[\u0600-\u06FF]/.test(text);
}

/**
 * يحوّل أخطاء التحقق إلى رسالة واحدة مقروءة.
 * الخطأ على مستوى الطلب كله (مفتاح "" أو "$") يُعرض بلا اسم حقل.
 */
function formatValidationErrors(
    errors: unknown,
): { message: string; errors: string[] } | null {
    if (errors == null) return null;

    if (Array.isArray(errors)) {
        const list = errors.map((e) => sanitize(String(e))).filter(Boolean);
        if (!list.length) return null;
        return { message: list.join(" و "), errors: list };
    }

    if (typeof errors !== "object") {
        const single = sanitize(String(errors));
        return single ? { message: single, errors: [single] } : null;
    }

    const parts: string[] = [];
    const flat: string[] = [];
    for (const [field, value] of Object.entries(errors as Record<string, unknown>)) {
        const list = (Array.isArray(value) ? value : [value])
            .map((v) => sanitize(String(v)))
            .filter(Boolean);
        if (!list.length) continue;
        flat.push(...list);
        const joined = list.join(" و ");
        parts.push(
            ROOT_ERROR_KEYS.has(field.trim())
                ? joined
                : `${sanitize(field)}: ${joined}`,
        );
    }

    if (!parts.length) return null;
    return { message: parts.join(" • "), errors: flat };
}

// ─── Normalizer ───────────────────────────────────────

/**
 * Extracts a user-friendly error from any Axios error.
 * Handles: ApiResponse<T>, AuthResponse, { message }, network errors.
 *
 * Usage:
 *   catch (err) {
 *     const { message } = normalizeApiError(err);
 *     toast({ variant: "error", title: message });
 *   }
 */
export function normalizeApiError(error: unknown): NormalizedApiError {
    // Axios error with a response from the server
    const axiosErr = error as AxiosError<ApiResponseShape>;

    if (axiosErr?.response) {
        const { status, data } = axiosErr.response;

        // Structured ApiResponse or AuthResponse
        if (data && typeof data === "object") {
            const dataObj = data as Record<string, unknown>;
            const msg =
                (data as ApiResponseShape).message ??
                dataObj.Message ??
                null;

            const errors =
                (data as ApiResponseShape).errors ??
                dataObj.Errors ??
                undefined;

            if (typeof msg === "string" && msg.length > 0) {
                return { 
                    message: sanitize(msg), 
                    errors: Array.isArray(errors) ? (errors as string[]).map(sanitize) : undefined, 
                    status 
                };
            }

            // ValidationProblemDetails: لا message — السبب في errors (وقد يكون على مستوى الطلب)
            const detail = formatValidationErrors(errors);
            if (detail) {
                return { message: detail.message, errors: detail.errors, status };
            }

            // عنوان بديل — يُعرض فقط كان بالعربية (رسائل المستخدم عربية)
            const title =
                (data as ValidationProblemDetailsShape).title ??
                dataObj.Title ??
                null;
            if (typeof title === "string" && title.length > 0 && hasArabic(title)) {
                return { message: sanitize(title), status };
            }
        }

        // Fallback by HTTP status code
        switch (status) {
            case 400:
                return { message: "البيانات المُدخلة غير صالحة", status };
            case 401:
                return { message: "انتهت صلاحية الجلسة — يرجى تسجيل الدخول", status };
            case 403:
                return { message: "ليس لديك صلاحية لتنفيذ هذا الإجراء", status };
            case 404:
                return { message: "العنصر المطلوب غير موجود", status };
            case 409:
                return { message: "تعارض في البيانات — يرجى تحديث الصفحة والمحاولة مجدداً", status };
            case 500:
                return { message: "حدث خطأ في الخادم — يرجى المحاولة لاحقاً", status };
            default:
                return { message: `خطأ غير متوقع (${status})`, status };
        }
    }

    // Network / connection error (no response received)
    if (axiosErr?.request) {
        return { message: "تعذر الاتصال بالخادم — تحقق من اتصالك بالإنترنت" };
    }

    // Unknown / non-Axios error
    if (error instanceof Error) {
        return { message: sanitize(error.message) };
    }

    return { message: "حدث خطأ غير متوقع" };
}

/**
 * Returns a user-facing title for the error toast based on the HTTP status code.
 * - 400 → "خطأ في البيانات المُدخلة"
 * - 401/403 → "خطأ في الصلاحيات"
 * - 404 → "العنصر غير موجود"
 * - 409 → "تعارض في البيانات"
 * - 500+ → "خطأ في الخادم"
 * - Network → "خطأ في الاتصال"
 * - Default → "خطأ"
 */
export function getErrorTitle(error: unknown): string {
    const axiosErr = error as AxiosError<{ message?: string }>;
    if (axiosErr?.response) {
        const { status } = axiosErr.response;
        switch (status) {
            case 400: return "خطأ في البيانات المُدخلة";
            case 401: 
            case 403: return "خطأ في الصلاحيات";
            case 404: return "العنصر غير موجود";
            case 409: return "تعارض في البيانات";
            default:
                if (status >= 500) return "خطأ في الخادم";
                return "خطأ";
        }
    }
    if (axiosErr?.request) {
        return "خطأ في الاتصال";
    }
    return "خطأ";
}
