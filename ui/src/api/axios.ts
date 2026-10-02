import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from "axios";

import { useAuthStore } from "@/stores/authStore";

// ─── Environment Validation ─────────────────────────

const API_BASE_URL = import.meta.env.VITE_API_URL;

if (!API_BASE_URL) {
    throw new Error(
        "[API] VITE_API_URL is not defined. Please set it in your .env file."
    );
}

// ─── Axios Instance ───────────────────────────────────

const api: AxiosInstance = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
    },
    timeout: 60000,
    withCredentials: true,
    xsrfCookieName: "XSRF-TOKEN",      // ── FIX #10: صراحة لضمان CSRF ──
    xsrfHeaderName: "X-XSRF-TOKEN",
});

/**
 * Returns the base URL used to construct static file (photo/document) URLs.
 * Priority: VITE_FILE_BASE_URL override → API server origin (without trailing /api).
 */
export function getFileBaseUrl(): string {
    const override = import.meta.env.VITE_FILE_BASE_URL;
    if (override) return override.replace(/\/?$/, "/");
    try {
        const url = new URL(API_BASE_URL);
        return `${url.origin}/`;
    } catch {
        const base = API_BASE_URL.replace(/\/api\/?$/, "");
        if (!base) {
            return typeof window !== "undefined" ? `${window.location.origin}/` : "";
        }
        return base.replace(/\/?$/, "/");
    }
}

/** تطبيع مسار ملف مع منع الخروج من المجلد (نقاط وشرائح فارغة). */
function normalizeFilePath(filePath: string): string | null {
    const normalized = filePath
        .replace(/\\/g, "/")
        .replace(/^\/+/, "")
        .split("/")
        .filter((seg) => seg.length > 0 && seg !== "." && seg !== "..")
        .map((seg) => {
            try {
                return encodeURIComponent(decodeURIComponent(seg));
            } catch {
                return encodeURIComponent(seg);
            }
        })
        .join("/");
    return normalized || null;
}

/**
 * بناء رابط ملف موحّد — يمر عبر FilesController (JWT authenticated).
 * يحل محل 51 استخدام مكرر لـ getFileBaseUrl() + بناء الـ URL يدوياً.
 *
 * يرجع رابطاً كاملاً حتى لا يتكرر /api مع baseURL في axios ولا يبتلع # في أسماء الملفات.
 *
 * @param filePath - المسار النسبي (مثل "uploads/documents/abc.pdf")
 * @returns الرابط الكامل عبر api/files/
 */
export function getFileUrl(filePath: string | null | undefined): string | null {
    if (!filePath) return null;
    if (/^(https?:|blob:|data:)/i.test(filePath)) return filePath;
    const normalized = normalizeFilePath(filePath);
    if (!normalized) return null;
    const base = API_BASE_URL.replace(/\/+$/, "");
    return `${base}/files/${normalized}`;
}

/**
 * نفس getFileUrl لكنه **نسبي على أصل الصفحة** (/api/files/...).
 *
 * في التطوير VITE_API_URL مطلق (localhost:5001) فيصير الطلب عابر للنطاقات:
 * يحتاج preflight بسبب ترويسة Authorization، وطلب blob عبر XHR يُحجب.
 * المسار النسبي يمرّ عبر Vite proxy فيبقى same-origin — بلا CORS وبلا preflight.
 * وفي الإنتاج VITE_API_URL نسبي أصلاً، فالنتيجة مطابقة لـ getFileUrl.
 *
 * يُستخدم مع axios عبر baseURL: "" حتى لا يُضمّ baseURL المطلق.
 */
export function getFileRelativeUrl(filePath: string | null | undefined): string | null {
    if (!filePath) return null;
    if (/^(https?:|blob:|data:)/i.test(filePath)) return filePath;
    const normalized = normalizeFilePath(filePath);
    if (!normalized) return null;
    // "/api" من VITE_API_URL بعد تجريد الأصل إن كان مطلقاً
    const base = API_BASE_URL.replace(/\/+$/, "").replace(/^https?:\/\/[^/]+/i, "");
    return `${base}/files/${normalized}`;
}

// ─── BroadcastChannel: cross-tab token sync ───────────

const AUTH_CHANNEL = "pension-auth-channel";

type ChannelMessage =
    | { type: "token-refreshed"; token: string; refreshToken: string }
    | { type: "logout" }
    | { type: "session-expired" };

function getAuthChannel(): BroadcastChannel | null {
    if (typeof window === "undefined" || !("BroadcastChannel" in window)) {
        return null;
    }
    try {
        return new BroadcastChannel(AUTH_CHANNEL);
    } catch {
        return null;
    }
}

const authChannel = getAuthChannel();

let isRefreshing = false;
let failedQueue: Array<{
    resolve: (token: string) => void;
    reject: (error: unknown) => void;
}> = [];

let refreshController: AbortController | null = null; // ── FIX #7: للإلغاء عند logout ──

function processQueue(error: unknown, token: string | null) {
    failedQueue.forEach((prom) => {
        if (token) prom.resolve(token);
        else prom.reject(error);
    });
    failedQueue = [];
}

function handleChannelMessage(event: MessageEvent<ChannelMessage>) {
    if (event.data.type === "token-refreshed") {
        const { token } = event.data;
        const store = useAuthStore.getState();
        if (token && token !== store.token) {
            useAuthStore.setState({ token });

 
          
            if (isRefreshing) {
                isRefreshing = false;
                if (refreshController) {
                    refreshController.abort();
                    refreshController = null;
                }
            }

            processQueue(null, token);
        }
    } else if (event.data.type === "logout" || event.data.type === "session-expired") {
        const store = useAuthStore.getState();
        if (store.isAuthenticated) {
            store.handleSessionExpiry();
        }
    }
}

if (authChannel) {
    authChannel.addEventListener("message", handleChannelMessage);
    window.addEventListener("beforeunload", () => {
        authChannel.removeEventListener("message", handleChannelMessage);
        authChannel.close();
    });
   
}

export function broadcastToken(token: string, refreshToken: string) {
    if (authChannel) {
        authChannel.postMessage({ type: "token-refreshed", token, refreshToken });
    }
}

export function broadcastLogout() {

    if (refreshController) {
        refreshController.abort();
        refreshController = null;
    }
    if (isRefreshing) {
        isRefreshing = false;
        processQueue(new Error("Logout initiated"), null);
    }
    if (authChannel) {
        authChannel.postMessage({ type: "logout" });
    }
}

// ─── Logger (sanitized) ───────────────────────────────

function logError(context: string, error: unknown) {
    if (import.meta.env.DEV) {

        let safeError: unknown = error;
        if (error instanceof AxiosError) {
            safeError = {
                message: error.message,
                status: error.response?.status,
                url: error.config?.url?.replace(/token=[^&]*/gi, "token=REDACTED"),
                method: error.config?.method,
            };
        }
         
        console.error(`[API Error] ${context}:`, safeError);
    }
    // TODO: Send to error tracking service in production (ensure PII stripping)
}

// ─── Request Interceptor ──────────────────────────────

api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = useAuthStore.getState().token;
        if (token) {
            config.headers.set("Authorization", `Bearer ${token}`);
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// ─── Web Locks Helper ─────────────────────────────────

/** يضمن أن لساناً واحداً فقط يُنفذ الـ refresh (مع fallback) */
async function withRefreshLock<T>(fn: () => Promise<T>): Promise<T> {
    if (typeof navigator !== "undefined" && "locks" in navigator) {
        return navigator.locks.request("auth-refresh", { ifAvailable: false }, fn);
    }
    // Fallback: استخدام localStorage كـ mutex بدائي للمتصفحات القديمة
    const LOCK_KEY = "auth-refresh-lock";
    const acquire = (): boolean => {
        const now = Date.now();
        const existing = localStorage.getItem(LOCK_KEY);
        if (existing) {
            const existingTime = parseInt(existing, 10);
            if (!isNaN(existingTime) && now - existingTime < 10000) {
                return false; // Lock مأخوذ مؤخراً
            }
        }
        localStorage.setItem(LOCK_KEY, String(now));
        return true;
    };
    const release = () => localStorage.removeItem(LOCK_KEY);

    while (!acquire()) {
        await new Promise((r) => setTimeout(r, 200));
    }
    try {
        return await fn();
    } finally {
        release();
    }
}

// ─── Response Interceptor: 401 & 403 handling ─────────

/** Endpoints that should NEVER trigger token refresh */
const SKIP_REFRESH_ENDPOINTS = ["/auth/refresh", "/auth/login", "/auth/logout"];

function shouldSkipRefresh(url?: string): boolean {
    if (!url) return false;
    return SKIP_REFRESH_ENDPOINTS.some((endpoint) => url.includes(endpoint));
}

api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const status = error.response?.status;
        const originalRequest = error.config as InternalAxiosRequestConfig & {
            _retry?: boolean;
            _retryCount?: number;
        };

        if (!originalRequest) {
            return Promise.reject(error);
        }

        // ─── 401 Unauthorized ───────────────────────────
        if (
            status === 401 &&
            !originalRequest._retry &&
            !shouldSkipRefresh(originalRequest.url)
        ) {
            const store = useAuthStore.getState();
            const currentToken = store.token;

            if (!currentToken) {
                if (store.isAuthenticated) {
                    store.handleSessionExpiry();
                    broadcastLogout();
                }
                return Promise.reject(error);
            }

            if (isRefreshing) {
                return new Promise<string>((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then((newToken) => {
                        originalRequest.headers.set("Authorization", `Bearer ${newToken}`);
                        return api(originalRequest);
                    })
                    .catch((err) => Promise.reject(err));
            }

            originalRequest._retry = true;
            originalRequest._retryCount = (originalRequest._retryCount ?? 0) + 1;

            if (originalRequest._retryCount > 1) {
                logError("Refresh retry limit exceeded", error);
                processQueue(error, null);
                const latestToken = useAuthStore.getState().token;
                if (latestToken && latestToken !== currentToken) {
                    originalRequest.headers.set("Authorization", `Bearer ${latestToken}`);
                    return api(originalRequest);
                }
                store.handleSessionExpiry();
                broadcastLogout();
                return Promise.reject(error);
            }


            return withRefreshLock(async () => {
                isRefreshing = true;
                refreshController = new AbortController(); 

                try {
                    // ✅ إرسال token + refreshToken في body كـ fallback للـ cookies
                    const currentRefreshToken = localStorage.getItem("auth_refresh_token") ?? "";
                    const res = await axios.post<{
                        isSuccess: boolean;
                        token?: string;
                        refreshToken?: string;
                    }>(
                        `${API_BASE_URL}/auth/refresh`,
                        { token: currentToken, refreshToken: currentRefreshToken },
                        {
                            timeout: 10000,
                            withCredentials: true,
                            signal: refreshController.signal,
                        }
                    );

                    if (res.data.isSuccess && res.data.token) {
                        const newToken = res.data.token;
                        const newRefreshToken = res.data.refreshToken ?? "";

                        useAuthStore.setState({ token: newToken });
                        broadcastToken(newToken, newRefreshToken);

                        originalRequest.headers.set("Authorization", `Bearer ${newToken}`);
                        processQueue(null, newToken);
                        return api(originalRequest);
                    }

                    processQueue(error, null);
                    const latestAfterRefresh = useAuthStore.getState().token;
                    if (latestAfterRefresh && latestAfterRefresh !== currentToken) {
                        originalRequest.headers.set("Authorization", `Bearer ${latestAfterRefresh}`);
                        return api(originalRequest);
                    }
                    store.handleSessionExpiry();
                    broadcastLogout();
                    return Promise.reject(error);
                } catch (refreshError) {
        
                    if (axios.isCancel(refreshError)) {
                        return Promise.reject(new Error("Refresh cancelled due to logout"));
                    }

                    logError("Token refresh failed", refreshError);

                    // ── Broadcast recovery ──
                    if (authChannel) {
                        try {
                            const sharedToken = await new Promise<string>(
                                (resolve, reject) => {
                                    let cleanedUp = false;

                                    const timeout = setTimeout(() => {
                                        if (!cleanedUp) {
                                            cleanedUp = true;
                                            authChannel.removeEventListener("message", handler);
                                            reject(new Error("Broadcast timeout"));
                                        }
                                    }, 3000);

                                    const handler = (ev: MessageEvent<ChannelMessage>) => {
                                        if (ev.data.type === "token-refreshed" && !cleanedUp) {
                                            cleanedUp = true;
                                            clearTimeout(timeout);
                                            authChannel.removeEventListener("message", handler);
                                            resolve(ev.data.token);
                                        }
                                    };

                                    authChannel.addEventListener("message", handler);

                                    // ── FIX #2: Early exit مع cleanup صحيح ──
                                    const latestToken = useAuthStore.getState().token;
                                    if (latestToken && latestToken !== currentToken) {
                                        if (!cleanedUp) {
                                            cleanedUp = true;
                                            clearTimeout(timeout);
                                            authChannel.removeEventListener("message", handler);
                                            resolve(latestToken);
                                        }
                                        return; // منع تسجيل listener مكرر
                                    }
                                }
                            );

                            useAuthStore.setState({ token: sharedToken });
                            originalRequest.headers.set("Authorization", `Bearer ${sharedToken}`);
                            processQueue(null, sharedToken);
                            return api(originalRequest);
                        } catch (broadcastErr) {
                            logError("Broadcast token recovery failed", broadcastErr);
                        }
                    }

                    processQueue(refreshError, null);
                    const latestAfterBroadcast = useAuthStore.getState().token;
                    if (latestAfterBroadcast && latestAfterBroadcast !== currentToken) {
                        originalRequest.headers.set("Authorization", `Bearer ${latestAfterBroadcast}`);
                        return api(originalRequest);
                    }
                    store.handleSessionExpiry();
                    broadcastLogout();
                    return Promise.reject(refreshError);
                } finally {
                    refreshController = null;
                    isRefreshing = false;
                }
            });
        }

        // ─── 403 Forbidden ──────────────────────────────
        if (status === 403) {
            console.error("🚫 [403 Forbidden]", {
                url: originalRequest.url,
                method: originalRequest.method,
                status: error.response?.status,
                data: error.response?.data,
                headers: error.response?.headers,
            });
            logError("Forbidden request", {
                url: originalRequest.url,
                method: originalRequest.method,
            });
            window.dispatchEvent(
                new CustomEvent("api:forbidden", {
                    detail: {
                        message: "ليس لديك صلاحية لتنفيذ هذا الإجراء",
                        url: originalRequest.url,
                        method: originalRequest.method,
                    },
                })
            );
            return Promise.reject(error); // صريح لمنع أي سلوك غير متوقع
        }

        // ─── Network Errors ─────────────────────────────
        if (!error.response && error.request) {
            logError("Network error (no response)", { url: originalRequest.url });
        }

        return Promise.reject(error);
    }
);


// ─── AbortController Helper ───────────────────────────

export function createCancelToken() {
    const controller = new AbortController();
    return {
        signal: controller.signal,
        cancel: (message?: string) => controller.abort(message),
    };
}



export async function apiWithRetry<T>(
    requestFn: () => Promise<T>,
    options: { maxRetries?: number; retryDelay?: number } = {}
): Promise<T> {
    const { maxRetries = 2, retryDelay = 1000 } = options; // تقليل maxRetries لتجنب التكرار مع interceptor

    for (let attempt = 0; attempt <= maxRetries; attempt++) {
        try {
            return await requestFn();
        } catch (error) {
            const axiosErr = error as AxiosError;
            const isRetryable = !axiosErr.response || axiosErr.response.status >= 500;
            if (!isRetryable || attempt === maxRetries) throw error;

            const delay = retryDelay * Math.pow(2, attempt) + Math.random() * 500;
            await new Promise((res) => setTimeout(res, delay));
        }
    }
    throw new Error("Unreachable");
}

export default api;
