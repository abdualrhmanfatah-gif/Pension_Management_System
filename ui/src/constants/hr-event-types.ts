/**
 * ثوابت أنواع أحداث سجل الخدمة للموظفين - واجهة المستخدم
 * تطابق الثوابت الموجودة في Backend: HrEventTypeConstants
 */
export const HR_EVENT_TYPE = {
    ADD_EMPLOYEE: "إضافة موظف",
    EDIT_DATA: "تعديل بيانات",
    DELETE_EMPLOYEE: "حذف موظف",
    LEAVE: "Leave",
    ATTENDANCE: "Attendance",
    PROMOTION: "ترقية",
    JOB_CHANGE: "تغيير مسمى وظيفي",
    JOB_HISTORY: "سجل وظيفي",
    OTHER: "أخرى",
} as const;

export const HrEventTypeConstants = HR_EVENT_TYPE;
