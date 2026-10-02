import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

import { getFileUrl } from "@/api/axios";

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

export function formatCurrency(amount: number): string {
    return new Intl.NumberFormat("ar-YE", {
        style: "decimal",
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
    }).format(amount) + " ر.ي";
}

export function formatNumber(n: number): string {
    return new Intl.NumberFormat("ar-YE").format(n);
}

export function formatDate(date: string | Date | null | undefined, includeTime: boolean = false): string {
    if (!date) return "";
    const d = new Date(date);
    if (isNaN(d.getTime())) return "";
    
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    
    let result = `${day}/${month}/${year}`;
    
    if (includeTime) {
        const hours = String(d.getHours()).padStart(2, '0');
        const minutes = String(d.getMinutes()).padStart(2, '0');
        result += ` ${hours}:${minutes}`;
    }

    return result;
}

/**
 * بناء رابط صورة/وثيقة من مسار نسبي.
 * يمر عبر FilesController (/api/files/*).
 * مثال: getPhotoUrl("uploads/photos/abc.jpg") → "/api/files/uploads/photos/abc.jpg"
 */
export function getPhotoUrl(path?: string | null): string | null {
    return getFileUrl(path);
}

/**
 * مختصر مُنسق لـ getPhotoUrl — للاستخدام في سياق الوثائق.
 * يستبدل الأنماط المكررة: getFileBaseUrl() + "/uploads/" + path
 */
export function getDocumentUrl(path?: string | null): string | null {
    return getPhotoUrl(path);
}

export function resolvePersonalPhotoPath(entity?: Record<string, any>): string | null {
    if (!entity) return null;
    return entity.personalPhotoPath ?? entity.PersonalPhotoPath ?? entity.personalPhoto ?? entity.PersonalPhoto ?? entity.photoPath ?? entity.PhotoPath ?? null;
}

/**
 * تحويل الأرقام العربية/الهندية (٠١٢...) إلى أرقام إنجليزية (012...)
 */
export function normalizeArabicDigits(str: string): string {
    return str.replace(/[٠-٩]/g, (d) => "٠١٢٣٤٥٦٧٨٩".indexOf(d).toString());
}

/**
 * جلب كلاس اللون لاسم المتقاعد بناءً على حالة الملفات
 */
export function getStatusColorClass(statusID: number | null | undefined): string {
    switch (statusID) {
        case 1: return "text-info";               // جديد
        case 2: return "text-success";             // نشط
        case 3: return "text-destructive";         // موقوف
        case 4: return "text-muted-foreground";    // مغلق
        case 5: return "text-warning";             // ناقص
        case 6: return "text-warning";             // معلق
        case 7: return "text-info";                // تحت المراجعة
        case 8: return "text-destructive";         // مرفوض
        default: return "text-foreground";
    }
}

/**
 * إرجاع الصورة الافتراضية للمتقاعد المتوفى حسب الجنس
 * @param gender - 1 = ذكر، 2 = أنثى
 */
export function getDefaultDeceasedAvatar(gender?: number | null): string {
    return gender === 2 ? "/avatars/deceased-female.svg" : "/avatars/deceased-male.svg";
}

/** مفاتيح لونية دلالية لبطاقات KPI — تُترجم إلى tokens عبر KPI_TONE_CLASS */
export type KpiTone = 'default' | 'primary' | 'success' | 'danger' | 'warning' | 'info' | 'special';

export const KPI_TONE_CLASS: Record<KpiTone, string> = {
    default: 'text-foreground',
    primary: 'text-primary',
    success: 'text-success',
    danger: 'text-danger',
    warning: 'text-warning',
    info: 'text-info',
    special: 'text-special',
};

/** يحوّل مفتاح النبرة إلى كلاس Tailwind؛ القيم غير المعروفة تُمرَّر كما هي (سلوك آمن) */
export const kpiToneClass = (tone?: string): string =>
    KPI_TONE_CLASS[tone as KpiTone] ?? tone ?? KPI_TONE_CLASS.primary;

