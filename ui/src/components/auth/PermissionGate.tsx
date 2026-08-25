import type { ReactNode } from "react";

import { useHasPermission } from "@/stores/authStore";

interface PermissionGateProps {
    /** الصلاحية المطلوبة لعرض المحتوى */
    permission: string;
    /** محتوى بديل يُعرض عند عدم وجود الصلاحية */
    fallback?: ReactNode;
    children: ReactNode;
}

/**
 * مكون يُظهر المحتوى فقط إذا كان المستخدم يملك الصلاحية المحددة
 *
 * @example
 * <PermissionGate permission="File.Create">
 *   <Button>إنشاء ملف جديد</Button>
 * </PermissionGate>
 */
export function PermissionGate({ permission, fallback = null, children }: PermissionGateProps) {
    const hasPermission = useHasPermission(permission);

    if (!hasPermission) return <>{fallback}</>;
    return <>{children}</>;
}
