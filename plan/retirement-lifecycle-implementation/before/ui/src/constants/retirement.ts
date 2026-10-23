/**
 * ثوابت ملفات التقاعد — مصدر واحد بدل التكرار داخل المكوّنات.
 * القيم مطابقة لـ PensionMS.Core/Constants/FileStatusConstants.cs
 */

export const FILE_STATUS = {
    New: 1,
    Active: 2,
    Suspended: 3,
    Closed: 4,
    Incomplete: 5,
    Pending: 6,
    UnderReview: 7,
    Rejected: 8,
} as const;

export type FileStatusId = (typeof FILE_STATUS)[keyof typeof FILE_STATUS];

/** حالة الملف التي تمنع فتح ملف جديد للمتقاعد نفسه. */
export const CLOSED_STATUS_ID = FILE_STATUS.Closed;

export const YEMEN_GOVERNORATES = [
    "أمانة العاصمة", "عدن", "تعز", "الحديدة", "إب", "ذمار", "حجة",
    "صعدة", "عمران", "المحويت", "ريمة", "صنعاء", "البيضاء", "لحج",
    "أبين", "الضالع", "شبوة", "حضرموت", "المهرة", "سقطرى", "مأرب", "الجوف",
] as const;
