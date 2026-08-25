import { create } from "zustand";
import { jwtDecode } from "jwt-decode";

import api, { broadcastToken, broadcastLogout } from "@/api/axios";
import { useNotificationStore } from "./useNotificationStore";

// ─── Constants ──────────────────────────────────────────
const LS_TOKEN_KEY = "auth_token";
const LS_REFRESH_TOKEN_KEY = "auth_refresh_token";
const LS_USER_KEY = "auth_user";
const LS_PERMISSIONS_KEY = "auth_permissions";

// ─── Types ──────────────────────────────────────────────
export interface AuthUser {
    id: number;
    fullName: string;
    username: string;
    email?: string | null;
    role: string;
    roleId?: number;
    branchId?: number;
    branchName?: string;
    mustChangePassword?: boolean;
    createdAt?: string;
    lastLogin?: string | null;
    isActive?: boolean;
}

export interface LoginResponse {
    token: string;
    refreshToken?: string;
    user: AuthUser;
}

export type HydrationState = 'idle' | 'restoring' | 'loadingPermissions' | 'ready' | 'error';

interface AuthState {
    token: string | null;
    user: AuthUser | null;
    permissions: string[];
    permissionsLoaded: boolean;
    permissionsError: boolean;
    isLoadingPermissions: boolean;
    isAuthenticated: boolean;
    isHydrated: boolean;
    hydrationState: HydrationState;
    sessionExpired: boolean;
    hydrationError: boolean;
    login: (data: LoginResponse) => Promise<void>;
    logout: () => Promise<void>;
    handleSessionExpiry: () => void;
    hydrate: () => Promise<void>;
    refreshPermissions: (silent?: boolean) => Promise<void>;
    refreshTokenNow: () => Promise<void>;
}

// ─── Logger ─────────────────────────────────────────────
function logError(context: string, error: unknown) {
    if (import.meta.env.DEV) {
        // eslint-disable-next-line no-console
        console.error(`[AuthStore] ${context}:`, error);
    }
    // TODO: Send to error tracking service in production
}

// ─── Pre-emptive token refresh ──────────────────────────
let _refreshTimer: ReturnType<typeof setTimeout> | null = null;

function _clearRefreshTimer() {
    if (_refreshTimer) clearTimeout(_refreshTimer);
    _refreshTimer = null;
}

function _scheduleTokenRefresh(token: string | null) {
    _clearRefreshTimer();
    if (!token) return;

    try {
        const decoded = jwtDecode<{ exp?: number }>(token);
        const exp = decoded?.exp;

        if (typeof exp !== 'number' || isNaN(exp)) {
            logError('Token missing or invalid exp claim', { exp });
            return;
        }

        const timeLeft = exp * 1000 - Date.now();
        if (timeLeft <= 10_000) return; // less than 10s left — too late to pre-empt

        // Refresh at 70% of token lifetime, or 5 min before expiry — whichever is sooner
        const refreshAt = Math.min(timeLeft - 5 * 60 * 1000, timeLeft * 0.7);
        if (refreshAt <= 0) return;

        const scheduledToken = token; // Capture for closure identity check

        _refreshTimer = setTimeout(async () => {
            const currentToken = useAuthStore.getState().token;
            if (!currentToken || currentToken !== scheduledToken) return;

            const currentRefreshToken = localStorage.getItem(LS_REFRESH_TOKEN_KEY);
            if (!currentRefreshToken) return;

            try {
                // ✅ Use api instance instead of raw axios to respect interceptors
                const res = await api.post<{ isSuccess: boolean; token?: string; refreshToken?: string }>(
                    "/auth/refresh",
                    { token: currentToken, refreshToken: currentRefreshToken },
                    { timeout: 5000 }
                );
                if (res.data.isSuccess && res.data.token) {
                    localStorage.setItem(LS_TOKEN_KEY, res.data.token);
                    if (res.data.refreshToken) localStorage.setItem(LS_REFRESH_TOKEN_KEY, res.data.refreshToken);
                    useAuthStore.setState({ token: res.data.token });
                    broadcastToken(res.data.token, res.data.refreshToken ?? "");
                    _scheduleTokenRefresh(res.data.token);
                }
            } catch (err) {
                logError('Pre-emptive token refresh failed', err);
                /* Refresh failed — will be handled reactively on next 401 */
            }
        }, refreshAt);
    } catch (err) {
        logError('Invalid token in _scheduleTokenRefresh', err);
    }
}

// ─── Activity-based token refresh ───────────────────────
let _lastActivityRefresh = 0;
const ACTIVITY_REFRESH_INTERVAL = 15 * 60 * 1000; // 15 دقيقة

async function _refreshTokenNow() {
    const now = Date.now();
    // منع التجديد المتكرر — مرة كل 15 دقيقة كحد أقصى
    if (now - _lastActivityRefresh < ACTIVITY_REFRESH_INTERVAL) return;

    const state = useAuthStore.getState();
    const currentToken = state.token;
    if (!currentToken || !state.isAuthenticated) return;

    // فحص: هل التوكن سينتهي خلال 15 دقيقة؟
    try {
        const decoded = jwtDecode<{ exp?: number }>(currentToken);
        const exp = decoded?.exp;
        if (typeof exp !== 'number' || isNaN(exp)) return;

        const timeLeft = exp * 1000 - now;
        // إذا بقي أكثر من 15 دقيقة، لا حاجة للتجديد
        if (timeLeft > ACTIVITY_REFRESH_INTERVAL) return;
        // إذا انتهى منذ أكثر من دقيقتين، نترك الـ 401 interceptor يتعامل
        if (timeLeft < -120_000) return;
    } catch {
        return;
    }

    _lastActivityRefresh = now;

    const currentRefreshToken = localStorage.getItem(LS_REFRESH_TOKEN_KEY);
    if (!currentRefreshToken) return;

    try {
        const res = await api.post<{ isSuccess: boolean; token?: string; refreshToken?: string }>(
            "/auth/refresh",
            { token: currentToken, refreshToken: currentRefreshToken },
            { timeout: 5000 }
        );
        if (res.data.isSuccess && res.data.token) {
            localStorage.setItem(LS_TOKEN_KEY, res.data.token);
            if (res.data.refreshToken) localStorage.setItem(LS_REFRESH_TOKEN_KEY, res.data.refreshToken);
            useAuthStore.setState({ token: res.data.token });
            broadcastToken(res.data.token, res.data.refreshToken ?? "");
            _scheduleTokenRefresh(res.data.token);
        }
    } catch (err) {
        logError('Activity-based token refresh failed', err);
    }
}

// ─── AbortController & concurrency guards ─────────────────
let _hydrateAbortController: AbortController | null = null;
let _permissionsAbortController: AbortController | null = null;

function _clearAbortControllers() {
    if (_hydrateAbortController) {
        try { _hydrateAbortController.abort(); } catch { /* ignore */ }
        _hydrateAbortController = null;
    }
    if (_permissionsAbortController) {
        try { _permissionsAbortController.abort(); } catch { /* ignore */ }
        _permissionsAbortController = null;
    }
}

// ─── Synchronous cache read for instant first render ─────
function _readCacheSync(): { token: string | null; user: AuthUser | null; permissions: string[] } {
    try {
        const token = localStorage.getItem(LS_TOKEN_KEY);
        const userRaw = localStorage.getItem(LS_USER_KEY);
        const permsRaw = localStorage.getItem(LS_PERMISSIONS_KEY);
        if (token && userRaw && permsRaw) {
            return {
                token,
                user: JSON.parse(userRaw) as AuthUser,
                permissions: JSON.parse(permsRaw) as string[],
            };
        }
    } catch { /* corrupt cache — ignore */ }
    return { token: null, user: null, permissions: [] };
}

const _initialCache = _readCacheSync();

// ─── Store ──────────────────────────────────────────────
export const useAuthStore = create<AuthState>((set, get) => ({
    token: _initialCache.token,
    user: _initialCache.user,
    permissions: _initialCache.permissions,
    permissionsLoaded: _initialCache.token !== null,
    permissionsError: false,
    isLoadingPermissions: false,
    isAuthenticated: _initialCache.token !== null,
    isHydrated: _initialCache.token !== null,
    hydrationState: _initialCache.token !== null ? 'ready' : 'idle',
    sessionExpired: false,
    hydrationError: false,

    login: async (data: LoginResponse) => {
        _clearRefreshTimer();
        _clearAbortControllers();

        localStorage.setItem(LS_TOKEN_KEY, data.token);
        if (data.refreshToken) localStorage.setItem(LS_REFRESH_TOKEN_KEY, data.refreshToken);
        localStorage.setItem(LS_USER_KEY, JSON.stringify(data.user));

        // علم لتحميل الإشعارات عند تسجيل الدخول فقط (وليس عند refresh)
        sessionStorage.setItem('fresh_login', 'true');

        set({
            token: data.token,
            user: data.user,
            permissions: [],
            permissionsLoaded: false,
            permissionsError: false,
            isLoadingPermissions: true,
            isAuthenticated: true,
            isHydrated: true,
            hydrationState: 'loadingPermissions',
            sessionExpired: false,
            hydrationError: false,
        });

        _scheduleTokenRefresh(data.token);

        try {
            const res = await api.get<{ data: string[] }>("/auth/me/permissions");
            const perms = Array.isArray(res.data.data) ? res.data.data : [];
            localStorage.setItem(LS_PERMISSIONS_KEY, JSON.stringify(perms));
            set({
                permissions: perms,
                permissionsLoaded: true,
                permissionsError: false,
                isLoadingPermissions: false,
                hydrationState: 'ready',
            });
        } catch (err) {
            logError('Failed to load permissions after login', err);
            set({
                permissions: [],
                permissionsLoaded: true,
                permissionsError: true,
                isLoadingPermissions: false,
                hydrationState: 'error',
            });
        }
    },

    logout: async () => {
        _clearRefreshTimer();
        _clearAbortControllers();

        const tokenToRevoke = localStorage.getItem(LS_TOKEN_KEY);

        localStorage.removeItem(LS_TOKEN_KEY);
        localStorage.removeItem(LS_REFRESH_TOKEN_KEY);
        localStorage.removeItem(LS_USER_KEY);
        localStorage.removeItem(LS_PERMISSIONS_KEY);

        if (tokenToRevoke) {
            try {
                await api.post(
                    "/auth/logout",
                    {},
                    { headers: { Authorization: `Bearer ${tokenToRevoke}` }, timeout: 5000 }
                );
            } catch (err) {
                logError('Logout request failed', err);
            }
        }

        try {
            useNotificationStore.getState().reset();
        } catch { /* ignore if store not ready */ }

        set({
            token: null,
            user: null,
            permissions: [],
            permissionsLoaded: false,
            permissionsError: false,
            isLoadingPermissions: false,
            isAuthenticated: false,
            isHydrated: true,
            hydrationState: 'idle',
            sessionExpired: false,
            hydrationError: false,
        });

        broadcastLogout();

        // ✅ Dispatch event instead of direct navigation to avoid NS_BINDING_ABORTED
        // Let the router handle navigation gracefully
        window.dispatchEvent(new CustomEvent("auth:logout"));
    },

    handleSessionExpiry: () => {
        // Prevent duplicate calls — already expired, skip
        if (get().sessionExpired) return;

        // تسجيل السبب للتشخيص
        if (import.meta.env.DEV) {
            // eslint-disable-next-line no-console
            console.error('[AuthStore] Session expired:', {
                hadToken: !!get().token,
                hadRefreshToken: !!localStorage.getItem(LS_REFRESH_TOKEN_KEY),
                timestamp: new Date().toISOString(),
            });
        }

        _clearRefreshTimer();
        _clearAbortControllers();

        localStorage.removeItem(LS_TOKEN_KEY);
        localStorage.removeItem(LS_REFRESH_TOKEN_KEY);
        localStorage.removeItem(LS_USER_KEY);
        localStorage.removeItem(LS_PERMISSIONS_KEY);

        try {
            useNotificationStore.getState().reset();
        } catch { /* ignore if store not ready */ }

        set({
            token: null,
            user: null,
            permissions: [],
            permissionsLoaded: false,
            permissionsError: false,
            isLoadingPermissions: false,
            isAuthenticated: false,
            isHydrated: true,
            hydrationState: 'idle',
            sessionExpired: true,
            hydrationError: false,
        });

        // ✅ Dispatch event for router to handle navigation
        window.dispatchEvent(new CustomEvent("auth:session-expired"));
    },

    hydrate: async () => {
        // Prevent concurrent hydration
        if (_hydrateAbortController) {
            try { _hydrateAbortController.abort(); } catch { }
        }
        _hydrateAbortController = new AbortController();
        const abortSignal = _hydrateAbortController.signal;

        const storedToken = localStorage.getItem(LS_TOKEN_KEY);

        // Try localStorage cache for instant render — NO loader if cache exists
        const cachedUserRaw = localStorage.getItem(LS_USER_KEY);
        const cachedPermsRaw = localStorage.getItem(LS_PERMISSIONS_KEY);
        let restoredFromCache = false;

        if (storedToken && cachedUserRaw && cachedPermsRaw) {
            try {
                const user = JSON.parse(cachedUserRaw) as AuthUser;
                const perms = JSON.parse(cachedPermsRaw) as string[];
                set({
                    token: storedToken,
                    user,
                    permissions: perms,
                    permissionsLoaded: true,
                    isAuthenticated: true,
                    isHydrated: true,
                    hydrationState: 'ready',
                    isLoadingPermissions: false,
                });
                restoredFromCache = true;
            } catch (err) {
                logError('Corrupt cache during hydrate', err);
            }
        }

        // Only show restoring state if NO cache (first load)
        if (!restoredFromCache) {
            set({ hydrationState: 'restoring' });
        }

        let tokenToUse: string | null = null;

        if (storedToken) {
            try {
                const decoded = jwtDecode<{ exp?: number }>(storedToken);
                const exp = decoded?.exp;

                if (typeof exp === 'number' && !isNaN(exp)) {
                    const isExpired = exp * 1000 <= Date.now() + 120_000;

                    if (isExpired) {
                        const currentRefreshToken = localStorage.getItem(LS_REFRESH_TOKEN_KEY);
                        if (currentRefreshToken) {
                            try {
                                const res = await api.post<{ isSuccess: boolean; token?: string; refreshToken?: string }>(
                                    "/auth/refresh",
                                    { token: storedToken, refreshToken: currentRefreshToken },
                                    { timeout: 3000, signal: abortSignal }
                                );
                                if (res.data.isSuccess && res.data.token) {
                                    localStorage.setItem(LS_TOKEN_KEY, res.data.token);
                                    if (res.data.refreshToken) localStorage.setItem(LS_REFRESH_TOKEN_KEY, res.data.refreshToken);
                                    tokenToUse = res.data.token;
                                    set({ token: res.data.token });
                                    broadcastToken(res.data.token, res.data.refreshToken ?? "");
                                }
                            } catch (err) {
                                logError('Token refresh during hydrate failed', err);
                            }
                        }
                    } else {
                        tokenToUse = storedToken;
                        if (!restoredFromCache) set({ token: storedToken });
                    }
                }
            } catch (err) {
                logError('Invalid stored token during hydrate', err);
            }
        }

        if (!tokenToUse && storedToken) {
            localStorage.removeItem(LS_TOKEN_KEY);
            localStorage.removeItem(LS_REFRESH_TOKEN_KEY);
            localStorage.removeItem(LS_USER_KEY);
            localStorage.removeItem(LS_PERMISSIONS_KEY);
            set({ token: null });
        }

        if (tokenToUse) {
            if (!abortSignal.aborted) {
                try {
                    const [meRes, permsRes] = await Promise.all([
                        api.get<{ data: AuthUser }>("/auth/me", {
                            timeout: 3000,
                            signal: abortSignal,
                        }),
                        api.get<{ data: string[] }>("/auth/me/permissions", {
                            timeout: 5000,
                            signal: abortSignal,
                        }),
                    ]);

                    if (abortSignal.aborted) return;

                    const user = meRes.data.data;
                    const perms = Array.isArray(permsRes.data.data) ? permsRes.data.data : [];

                    localStorage.setItem(LS_USER_KEY, JSON.stringify(user));
                    localStorage.setItem(LS_PERMISSIONS_KEY, JSON.stringify(perms));

                    set({
                        token: tokenToUse,
                        user,
                        permissions: perms,
                        permissionsLoaded: true,
                        permissionsError: false,
                        isLoadingPermissions: false,
                        isAuthenticated: true,
                        isHydrated: true,
                        hydrationState: 'ready',
                        sessionExpired: false,
                        hydrationError: false,
                    });

                    _scheduleTokenRefresh(tokenToUse);
                    _hydrateAbortController = null;
                    return;
                } catch (err: any) {
                    if (abortSignal.aborted) return;
                    logError('API failed during hydrate', err);
                    // API failed — keep cached data if we have it
                    if (restoredFromCache) {
                        set({
                            isAuthenticated: true,
                            isHydrated: true,
                            hydrationState: 'ready',
                            sessionExpired: false,
                            hydrationError: false,
                        });
                        _scheduleTokenRefresh(tokenToUse);
                        _hydrateAbortController = null;
                        return;
                    }
                }
            }
        }

        // All attempts failed
        if (abortSignal.aborted) return;

        localStorage.removeItem(LS_TOKEN_KEY);
        localStorage.removeItem(LS_REFRESH_TOKEN_KEY);
        localStorage.removeItem(LS_USER_KEY);
        localStorage.removeItem(LS_PERMISSIONS_KEY);
        set({
            token: null,
            user: null,
            isAuthenticated: false,
            isHydrated: true,
            hydrationState: 'ready',
            hydrationError: false,
            sessionExpired: false,
        });

        _hydrateAbortController = null;
    },

    refreshPermissions: async (silent = false) => {
        if (_permissionsAbortController) {
            try { _permissionsAbortController.abort(); } catch { /* ignore */ }
        }
        _permissionsAbortController = new AbortController();
        const signal = _permissionsAbortController.signal;

        if (!silent || !get().permissionsLoaded) {
            set({ permissionsError: false, hydrationState: 'loadingPermissions', isLoadingPermissions: true });
        }

        try {
            const res = await api.get<{ data: string[] }>("/auth/me/permissions", { signal });
            const perms = Array.isArray(res.data.data) ? res.data.data : [];

            set({
                permissions: perms,
                permissionsLoaded: true,
                permissionsError: false,
                hydrationState: 'ready',
                isLoadingPermissions: false,
            });
        } catch (err: any) {
            if (err.name === 'AbortError' || err?.code === 'ERR_CANCELED') {
                return;
            }

            logError('Permission refresh failed', err);

            if (!silent || !get().permissionsLoaded) {
                set({
                    permissions: [],
                    permissionsLoaded: true,
                    permissionsError: true,
                    hydrationState: 'error',
                    isLoadingPermissions: false,
                });
            } else {
                // Silent refresh failed but we have old permissions — keep them, just flag the error
                set({ permissionsError: true, isLoadingPermissions: false });
            }
        } finally {
            _permissionsAbortController = null;
        }
    },

    refreshTokenNow: async () => {
        await _refreshTokenNow();
    },
}));

// ─── Selectors ──────────────────────────────────────────
export const useHasRole = (...roles: string[]) => {
    const user = useAuthStore((s) => s.user);
    if (!user) return false;
    return roles.some((r) => r.toLowerCase() === user.role?.toLowerCase());
};

export const useHasPermission = (...codes: string[]) => {
    const permissions = useAuthStore((s) => s.permissions);
    const permissionsLoaded = useAuthStore((s) => s.permissionsLoaded);
    const isHydrated = useAuthStore((s) => s.isHydrated);

    // During hydration, deny access — PermissionRoute handles its own loading gate
    if (!isHydrated || !permissionsLoaded) return false;

    return codes.some((code) => permissions.includes(code));
};

export const useHasAllPermissions = (...codes: string[]) => {
    const permissions = useAuthStore((s) => s.permissions);
    const permissionsLoaded = useAuthStore((s) => s.permissionsLoaded);
    const isHydrated = useAuthStore((s) => s.isHydrated);

    // During hydration, deny access
    if (!isHydrated || !permissionsLoaded) return false;

    return codes.every((code) => permissions.includes(code));
};
