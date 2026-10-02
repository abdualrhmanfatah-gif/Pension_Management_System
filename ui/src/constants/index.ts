/**
 * ثوابت النظام المركزية - واجهة المستخدم
 */

export const GAP_STATUS = {
  MATCHED: "مطابق",
  NOT_IN_SYSTEM: "غير موجود في النظام",
} as const;

export const TRANSACTION_TYPE = {
  INCOME: "Income",
  EXPENSE: "Expense",
} as const;

export const BATCH_STATUS = {
  DRAFT: "مسودة",
  APPROVED: "معتمد",
  NEW: "جديد",
  IMPORTED: "تم الاستيراد",
  PROCESSING: "قيد المعالجة",
  COMPLETED: "مكتمل",
} as const;

export const GENDER = {
  MALE: 1,
  FEMALE: 2,
} as const;

export const RETIREE_TAB = {
  APPROVAL: "approval",
  FILES: "files",
  DOCUMENTS: "documents",
  SETTLEMENTS: "settlements",
  ENTITLEMENTS: "entitlements",
} as const;

export const VIEW_MODE = {
  DATA_ENTRY: "dataEntry",
  DOCUMENT_UPLOADER: "documentUploader",
  REVIEWER: "reviewer",
  FINANCIAL_OFFICER: "financialOfficer",
} as const;

export const REVIEW_ACTION = {
  RETURN: "return",
  REJECT: "reject",
  APPROVE: "approve",
} as const;

export const RECIPIENT_TYPE = {
  RETIREE: "Retiree",
  AGENT: "Agent",
  ENTITLED: "Entitled",
  GUARDIAN: "Guardian",
} as const;

export const RECIPIENT_TYPE_AR = {
  RETIREE: "متقاعد",
  AGENT: "وكيل",
  ENTITLED: "مستحق",
  GUARDIAN: "وصي",
} as const;

export const SALARY_STATUS = {
  ACTIVE: "نشط",
  SUSPENDED: "موقوف",
} as const;

export const SUSPENSION_STATUS = {
  ACTIVE: "فعّال",
} as const;

export const COLUMN_TYPE = {
  CURRENCY: "currency",
  PERCENT: "percent",
  DATE: "date",
  NUMBER: "number",
} as const;

export const TENANT_TYPE = {
  COMPANY: "Company",
  INDIVIDUAL: "Individual",
  MERCHANT: "Merchant",
} as const;

export const PRIORITY = {
  URGENT: "Urgent",
  HIGH: "High",
  NORMAL: "Normal",
  LOW: "Low",
} as const;

export const AUDIT_CHANGE_TYPE = {
  CREATE: "Create",
  DELETE: "Delete",
  STATUS_CHANGE: "StatusChange",
} as const;

export const BOOLEAN_STRING = {
  TRUE: "true",
  FALSE: "false",
} as const;

export const BADGE_VARIANT = {
  SUCCESS: "success",
  WARNING: "warning",
  DANGER: "danger",
  INFO: "info",
  GHOST: "ghost",
  OUTLINE: "outline",
} as const;

export const TREASURY_SOURCE = {
  BANK: "Bank",
  CASH: "Cash",
} as const;

export const ATTENDANCE_TYPE = {
  CHECK_IN: "CheckIn",
  CHECK_OUT: "CheckOut",
} as const;

export const GENERAL_STATUS = {
  ACTIVE: "Active",
  INACTIVE: "Inactive",
} as const;

export const PLEDGE_TAB = {
  PLEDGES: "pledges",
  EXEMPTIONS: "exemptions",
} as const;

export const LEAVE_STATUS = {
  PENDING: "Pending",
  APPROVED: "Approved",
  REJECTED: "Rejected",
} as const;

export const APPROVAL_STATUS = {
  PENDING: "Pending",
  APPROVED: "Approved",
  REJECTED: "Rejected",
} as const;

export const EXPENSE_STATUS = {
    NEW: "جديد",
    UNDER_REVIEW: "تحت المراجعة",
    APPROVED: "معتمد",
    DISBURSED: "تم الصرف",
    CANCELLED: "ملغي",
    PENDING_CLASSIFICATION: "بانتظار التصنيف",
} as const;

export const PROJECT_STATUS = {
  ACTIVE: "Active",
  PLANNED: "Planned",
  COMPLETED: "Completed",
  ON_HOLD: "OnHold",
} as const;

export const PENDING_REVIEW_STATUS = {
  NONE: 0,
  PENDING_REVIEW: 1,
  APPROVED: 2,
  REJECTED: 3,
} as const;

export const BOOK_TYPES = [
  { value: "إلكتروني", label: "إلكتروني (بطاقة صرف)" },
  { value: "يدوي", label: "يدوي (دفتر قديم)" },
] as const;

export const LEGACY_BOOK_COLORS = ["أبيض", "أخضر", "أزرق"] as const;

// ═══════════════════════════════════════════
// ACCOUNTING (المحاسبة)
// ═══════════════════════════════════════════

export const ACCOUNT_NATURE = {
  DEBIT: "Debit",
  CREDIT: "Credit",
} as const;

export const JOURNAL_STATUS = {
  DRAFT: "Draft",
  POSTED: "Posted",
  REVERSED: "Reversed",
} as const;

export const SOURCE_MODULE = {
  MANUAL: "Manual",
  SALARY: "Salary",
  SETTLEMENT: "Settlement",
} as const;

export const ACCOUNT_LEVELS = {
  BOOK: 1,
  MAIN: 2,
  SUB: 3,
  DETAIL: 4,
  POSTABLE: 5,
} as const;

// ═══════════════════════════════════════════
// REDESIGN - مراجعة ملف التقاعد
// ═══════════════════════════════════════════

export const REVIEW_SECTIONS = {
  BASIC: "basic",
  PEOPLE: "people",
  DOCUMENTS: "documents",
  FINANCIAL: "financial",
  HISTORY: "history",
} as const;

export const REVIEW_SECTIONS_AR = {
  [REVIEW_SECTIONS.BASIC]: "البيانات الأساسية",
  [REVIEW_SECTIONS.PEOPLE]: "الأشخاص",
  [REVIEW_SECTIONS.DOCUMENTS]: "الوثائق",
  [REVIEW_SECTIONS.FINANCIAL]: "المالية",
  [REVIEW_SECTIONS.HISTORY]: "التاريخ",
} as const;

export const PERSON_TYPE = {
  ENTITLED: "entitled",
  AGENT: "agent",
  GUARDIAN: "guardian",
} as const;

export const PERSON_TYPE_AR = {
  [PERSON_TYPE.ENTITLED]: "مستحق",
  [PERSON_TYPE.AGENT]: "وكيل",
  [PERSON_TYPE.GUARDIAN]: "وصي",
} as const;

export const PERSON_DOC_FILTER = {
  ALL: "all",
  UPLOADED: "uploaded",
  MISSING: "missing",
} as const;

export const PERSON_DOC_FILTER_AR = {
  [PERSON_DOC_FILTER.ALL]: "الكل",
  [PERSON_DOC_FILTER.UPLOADED]: "مرفوعة",
  [PERSON_DOC_FILTER.MISSING]: "ناقصة",
} as const;

export const DOC_STATUS_COLOR = {
  uploaded: "text-success",
  missing: "text-danger",
  pledge: "text-warning",
  exempted: "text-special",
} as const;
