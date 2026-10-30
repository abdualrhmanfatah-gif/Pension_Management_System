/* ═══════════════════════════════════════════════════════════
   Shared Report Header Spec
   مصدر واحد للترويسة الرسمية — يستخدمه PrintLayout (PDF)
<<<<<<< HEAD
=======
   و excel.utils (Excel) لضمان التطابق التام.
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
   ═══════════════════════════════════════════════════════════ */

/** النصوص الرسمية */
export const REPORT_HEADER_TEXT = {
  republic: "الجمهورية اليمنية",
  ministry: "وزارة الداخلية",
  department: "الإدارة العامة للصندوق التقاعد الأمني",
  section: "الالإدارة المالية",
  basmalah: "بسم الله الرحمن الرحيم",
  footer:
    "صادر عن الإدارة العامة للصندوق التقاعد الأمني - وزارة الداخلية - جميع الحقوق محفوظة",
} as const;
<<<<<<< HEAD
=======

/** مواصفات الإطار المزدوج الأسود (PrintLayout: 4px double #000) */
export const HEADER_FRAME = {
  /** لون الحدود بصيغة ARGB (ExcelJS) */
  argb: "FF000000",
  /** نمط الحدود (ExcelJS) */
  style: "double" as const,
  /** نص CSS للحدود (React) */
  css: "4px double #000",
} as const;

/** أوزان الخطوط الموحدة */
export const HEADER_FONT = {
  /** خط الترويسة الجانبي (يمين/يسار) */
  sideWeight: 800,
  /** خط العنوان الرئيسي */
  titleWeight: 900,
  /** حجم خط الترويسة الجانبي */
  sideSize: 13,
  /** حجم خط العنوان الرئيسي */
  titleSize: 18,
  /** حجم خط البسملة */
  basmalahSize: 14,
} as const;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
