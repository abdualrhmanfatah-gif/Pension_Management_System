/**
 * ثوابت ملفات التقاعد — مصدر واحد بدل التكرار داخل المكوّنات.
 * القيم مطابقة لـ PensionMS.Core/Constants/FileStatusConstants.cs
 */

export const FILE_STATUS = {
    New: 1,
    Active: 2,
    EntitlementEnded: 4,
    Pending: 6,
} as const;

export type FileStatusId = (typeof FILE_STATUS)[keyof typeof FILE_STATUS];

export function retirementFileStatusName(statusId: number | null | undefined): string {
    switch (statusId) {
        case FILE_STATUS.New: return "جديد";
        case FILE_STATUS.Active: return "نشط";
        case FILE_STATUS.Pending: return "معلق";
        case FILE_STATUS.EntitlementEnded: return "انتهى استحقاقه";
        default: return "حالة سابقة — يلزم تحويل الملف للمراجعة";
    }
}

export const YEMEN_GOVERNORATES = [
    "أمانة العاصمة", "عدن", "تعز", "الحديدة", "إب", "ذمار", "حجة",
    "صعدة", "عمران", "المحويت", "ريمة", "صنعاء", "البيضاء", "لحج",
    "أبين", "الضالع", "شبوة", "حضرموت", "المهرة", "سقطرى", "مأرب", "الجوف",
] as const;
