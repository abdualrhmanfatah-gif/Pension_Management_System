import { Navigate, Outlet, useLocation } from "react-router-dom";

import { Button, Loader } from "@/components/ui";
import { useAuthStore } from "@/stores/authStore";

interface PermissionRouteProps {
    permission?: string;
    anyPermission?: string[];
}

export function PermissionRoute({ permission, anyPermission }: PermissionRouteProps) {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isHydrated = useAuthStore((s) => s.isHydrated);
    const permissions = useAuthStore((s) => s.permissions);
    const permissionsLoaded = useAuthStore((s) => s.permissionsLoaded);
    const permissionsError = useAuthStore((s) => s.permissionsError);
    const location = useLocation();

    // ✅ CRITICAL: Wait for hydration + permissions before ANY decision
    if (!isHydrated || !permissionsLoaded) {
        return <Loader variant="fullscreen" />;
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (permissionsError) {
        return (
            <div className="flex flex-col items-center justify-center h-[60dvh] gap-4">
                <div className="text-red-500 font-bold text-lg">فشل التحقق من الصلاحيات</div>
                <p className="text-muted-foreground">لم نتمكن من جلب بيانات الصلاحيات الخاصة بك.</p>
                <Button onClick={() => useAuthStore.getState().refreshPermissions()}>
                    إعادة المحاولة
                </Button>
            </div>
        );
    }

    // No permission required = allow
    if (!permission && !anyPermission) {
        return <Outlet />;
    }

    // Check permissions
    let hasAccess = false;
    if (permission) {
        hasAccess = permissions.includes(permission);
    }
    if (anyPermission && anyPermission.length > 0) {
        hasAccess = hasAccess || anyPermission.some((p) => permissions.includes(p));
    }

    if (!hasAccess) {
        return (
            <div className="flex items-center justify-center h-[60dvh]">
                <div className="text-center space-y-4 animate-fade-in">
                    <div className="w-16 h-16 mx-auto rounded-2xl bg-red-100 dark:bg-red-900/20 flex items-center justify-center">
                        <span className="text-3xl">🚫</span>
                    </div>
                    <h2 className="text-xl font-bold text-foreground">غير مصرح بالوصول</h2>
                    <p className="text-sm text-muted-foreground max-w-md">
                        ليس لديك الصلاحيات الكافية للوصول إلى هذه الصفحة.
                    </p>
                    <Button onClick={() => window.history.back()}>
                        العودة للخلف
                    </Button>
                </div>
            </div>
        );
    }

    return <Outlet />;
}