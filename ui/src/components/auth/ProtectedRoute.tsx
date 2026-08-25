import { Navigate, Outlet, useLocation } from "react-router-dom";

import { Button, Loader } from "@/components/ui";
import { useAuthStore } from "@/stores/authStore";

export function ProtectedRoute() {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isHydrated = useAuthStore((s) => s.isHydrated);
    const hydrationState = useAuthStore((s) => s.hydrationState);
    const permissionsError = useAuthStore((s) => s.permissionsError);
    const user = useAuthStore((s) => s.user);
    const sessionExpired = useAuthStore((s) => s.sessionExpired);
    const location = useLocation();

    // ✅ CRITICAL: Show loader until hydration completes — NO timeout redirect
    if (!isHydrated || hydrationState === 'idle' || hydrationState === 'restoring') {
        return <Loader variant="fullscreen" />;
    }

    if (hydrationState === 'loadingPermissions') {
        return <Loader variant="fullscreen" text="جاري التحقق من الصلاحيات..." isSecurity />;
    }

    if (sessionExpired) {
        sessionStorage.setItem("postLoginRedirect", location.pathname);
        return <Navigate to="/login" state={{ expired: true, from: location }} replace />;
    }

    if (!isAuthenticated) {
        sessionStorage.setItem("postLoginRedirect", location.pathname);
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (user?.mustChangePassword && location.pathname !== "/change-password") {
        return <Navigate to="/change-password" replace />;
    }

    // ✅ Block navigation on permission error — show retry UI
    if (permissionsError) {
        return (
            <div className="flex flex-col items-center justify-center h-[60dvh] gap-4">
                <div className="text-destructive font-bold text-lg">فشل تحميل الصلاحيات</div>
                <p className="text-muted-foreground">لم نتمكن من جلب بيانات الصلاحيات الخاصة بك.</p>
                <Button onClick={() => useAuthStore.getState().refreshPermissions()}>
                    إعادة المحاولة
                </Button>
            </div>
        );
    }

    return <Outlet />;
}