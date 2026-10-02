export const actionLabels: Record<string, string> = {
    Read: "عرض", View: "عرض", Create: "إضافة", Add: "إضافة", Update: "تعديل", Edit: "تعديل", Delete: "حذف", Manage: "إدارة كاملة",
    Export: "تصدير", Import: "استيراد", Approve: "اعتماد", Reject: "رفض", Print: "طباعة", Admin: "مدير النظام",
    Lifecycle: "إيقاف/إعادة تفعيل", Return: "إعادة لوجود نواقص", Soft: "حذف منطقي", Upload: "رفع ملفات",
    BasicData: "بيانات أساسية", History: "سجل العمليات", Sensitive: "بيانات حساسة", FinancialData: "بيانات مالية",
    BankData: "بيانات الحساب البنكي", Finance: "المالية", ExcludeBeneficiary: "استبعاد مستفيد", ManageDeductions: "إدارة الاستقطاعات",
    ImportResults: "استيراد نتائج", ViewAll: "رؤية الكل", Override: "تجاوز الصلاحيات", ManageUsers: "إدارة المستخدمين",
    Settings: "الإعدادات", Banks: "البنوك", DocumentTypes: "أنواع الوثائق", InvestmentTypes: "أنواع الاستثمارات", JobTitles: "المسميات الوظيفية"
};

export const getPermissionName = (code: string) => {
    const parts = code.split(".");
    if (parts.length > 1) {
        const action = parts[parts.length - 1];
        return actionLabels[action] || action;
    }
    return code;
};

export const categoryLabels: Record<string, string> = {
    File: "إدارة الملفات", Financial: "العمليات المالية", Export: "التصدير والتقارير", System: "إدارة النظام",
    EntitledPerson: "المستحقون", Guardian: "الأوصياء", Retiree: "المتقاعدون", Agent: "الوكلاء",
    FileNote: "ملاحظات الملفات", DocumentExemption: "إعفاءات الوثائق", DocumentPledge: "التعهدات",
    MissingDocument: "الوثائق الناقصة", PaymentBook: "دفاتر الصرف", Ministry: "الإحالة إلى التقاعد",
    Suspension: "إيقاف المعاش", Dashboard: "لوحة التحكم", Settlement: "التسويات المالية",
    Correspondence: "المراسلات والأرشفة", User: "المستخدمون", Role: "الأدوار والصلاحيات", SystemLog: "سجل النظام",
    Investment: "الاستثمارات", HR: "الشؤون الإدارية", 
    SalaryBatch: "كشوفات المعاشات", SalaryRecipient: "مستلمو المعاشات",
    Payroll: "المعاشات", Audit: "سجل التدقيق"
};

export const categoryColors: Record<string, string> = {
    File: "bg-info/10 text-info",
    Financial: "bg-success/10 text-success",
    Export: "bg-warning/10 text-warning",
    System: "bg-special/10 text-special",
    EntitledPerson: "bg-info/10 text-info",
    Guardian: "bg-special/10 text-special",
    Retiree: "bg-success/10 text-success",
    Agent: "bg-warning/10 text-warning",
    FileNote: "bg-info/10 text-info",
    DocumentExemption: "bg-success/10 text-success",
    DocumentPledge: "bg-danger/10 text-danger",
    MissingDocument: "bg-warning/10 text-warning",
    PaymentBook: "bg-special/10 text-special",
    Ministry: "bg-special/10 text-special",
    Suspension: "bg-danger/10 text-danger",
    Dashboard: "bg-muted text-muted-foreground",
    Settlement: "bg-success/10 text-success",
    Correspondence: "bg-muted text-muted-foreground",
    User: "bg-special/10 text-special",
    Role: "bg-special/10 text-special",
    SystemLog: "bg-muted text-muted-foreground",
    Investment: "bg-warning/10 text-warning",
    HR: "bg-info/10 text-info",
    SalaryBatch: "bg-info/10 text-info",
    SalaryRecipient: "bg-info/10 text-info",
    Payroll: "bg-success/10 text-success",
    Audit: "bg-muted text-muted-foreground"
};
