import type { StatusConfig } from "@/components/ui/StatusBadge";

export const VOUCHER_STATUS: Record<string, StatusConfig> = {
    "مسودة": { label: "مسودة", variant: "ghost" },
    "مرحّل": { label: "مرحّل", variant: "success" },
    "ملغي": { label: "ملغي", variant: "danger" },
};

export const JOURNAL_ENTRY_STATUS: Record<string, StatusConfig> = {
    "Draft": { label: "مسودة", variant: "ghost" },
    "Posted": { label: "مرحّل", variant: "success" },
    "Reversed": { label: "ملغي", variant: "danger" },
};

export const SALARY_PAYMENT_STATUS: Record<string, StatusConfig> = {
    "ناجحة": { label: "ناجحة", variant: "success" },
    "تم الصرف": { label: "تم الصرف", variant: "success" },
    "فاشلة": { label: "فاشلة", variant: "danger" },
    "مرتجع": { label: "مرتجع", variant: "danger" },
    "قيد التنفيذ": { label: "قيد التنفيذ", variant: "warning" },
    "جاري": { label: "جاري", variant: "warning" },
    "ملغي": { label: "ملغي", variant: "ghost" },
    "تم الإرسال": { label: "تم الإرسال", variant: "info" },
};

export const STOCKTAKE_STATUS: Record<string, StatusConfig> = {
    "InProgress": { label: "قيد التنفيذ", variant: "warning" },
    "Completed": { label: "مكتمل", variant: "success" },
    "Adjusted": { label: "تمت التسوية", variant: "outline" },
    "Verified": { label: "تم التحقق", variant: "outline" },
};

export const PAYMENT_BOOK_STATUS: Record<string, StatusConfig> = {
    "نشط": { label: "نشط", variant: "success" },
    "فعال": { label: "فعال", variant: "success" },
    "منتهي": { label: "منتهي", variant: "warning" },
    "ملغي": { label: "ملغي", variant: "danger" },
    "مفقود": { label: "مفقود", variant: "danger" },
    "تالف": { label: "تالف", variant: "danger" },
    "معلّق": { label: "معلّق", variant: "info" },
};

export const INVESTMENT_STATUS: Record<string, StatusConfig> = {
    "نشط": { label: "نشط", variant: "success" },
    "معلّق": { label: "معلّق", variant: "warning" },
    "مغلق": { label: "مغلق", variant: "ghost" },
    "ملغي": { label: "ملغي", variant: "danger" },
};

export const REVIEW_STATUS: Record<string, StatusConfig> = {
    "قيد المراجعة": { label: "قيد المراجعة", variant: "info" },
    "معتمد": { label: "معتمد", variant: "success" },
    "مرفوض": { label: "مرفوض", variant: "danger" },
};
