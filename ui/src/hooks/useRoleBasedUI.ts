import { useMemo } from "react";

import { useAuthStore } from "@/stores/authStore";

/**
 * Hook لتحديد الدور الوظيفي بناءً على الصلاحيات
 * يستخدم لتخصيص الواجهات حسب دور المستخدم
 */
export function useRoleBasedUI() {
    const permissions = useAuthStore((s) => s.permissions);

    return useMemo(() => {
        const hasPermission = (code: string) => permissions.includes(code);

        // مدخل البيانات: يملك صلاحية إنشاء أو تعديل البيانات الأساسية
        const isDataEntry =
            hasPermission("File.Create") || hasPermission("File.Edit.BasicData");

        // رافع الوثائق: يملك صلاحية رفع الوثائق
        const isDocumentUploader = hasPermission("File.Documents.Upload");

        // المراجع/المصادق: يملك صلاحية اعتماد أو إرجاع الملف
        const isReviewer =
            hasPermission("File.ChangeStatus.Approve") ||
            hasPermission("File.ChangeStatus.Return");

        // الموظف المالي: يملك صلاحية تعديل البيانات المالية
        const isFinancialOfficer = hasPermission("File.Edit.FinancialData");

        // قارئ فقط: يملك فقط صلاحيات القراءة
        const isViewerOnly =
            (hasPermission("File.View.Sensitive") ||
                hasPermission("Audit.ViewAll")) &&
            !isDataEntry &&
            !isDocumentUploader &&
            !isReviewer &&
            !isFinancialOfficer;

        // مدير: يملك صلاحيات إدارية
        const isAdmin = hasPermission("System.Admin");

        // تحديد View Mode الأساسي
        let primaryViewMode: "dataEntry" | "documentUploader" | "reviewer" | "financialOfficer" | "viewer" | "admin" | null = null;

        if (isAdmin) {
            primaryViewMode = "admin";
        } else if (isDataEntry && !isReviewer && !isFinancialOfficer) {
            primaryViewMode = "dataEntry";
        } else if (isDocumentUploader && !isReviewer && !isFinancialOfficer) {
            primaryViewMode = "documentUploader";
        } else if (isReviewer) {
            primaryViewMode = "reviewer";
        } else if (isFinancialOfficer) {
            primaryViewMode = "financialOfficer";
        } else if (isViewerOnly) {
            primaryViewMode = "viewer";
        }

        return {
            isDataEntry,
            isDocumentUploader,
            isReviewer,
            isFinancialOfficer,
            isViewerOnly,
            isAdmin,
            primaryViewMode,
            hasPermission,
        };
    }, [permissions]);
}
