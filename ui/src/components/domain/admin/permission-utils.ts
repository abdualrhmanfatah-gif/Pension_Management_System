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
    File: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    Financial: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    Export: "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
    System: "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
    EntitledPerson: "bg-cyan-100 text-cyan-700 dark:bg-cyan-900/30 dark:text-cyan-400",
    Guardian: "bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400",
    Retiree: "bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400",
    Agent: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
    FileNote: "bg-sky-100 text-sky-700 dark:bg-sky-900/30 dark:text-sky-400",
    DocumentExemption: "bg-lime-100 text-lime-700 dark:bg-lime-900/30 dark:text-lime-400",
    DocumentPledge: "bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-400",
    MissingDocument: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    PaymentBook: "bg-fuchsia-100 text-fuchsia-700 dark:bg-fuchsia-900/30 dark:text-fuchsia-400",
    Ministry: "bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400",
    Suspension: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
    Dashboard: "bg-slate-100 text-slate-700 dark:bg-slate-900/30 dark:text-slate-400",
    Settlement: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    Correspondence: "bg-stone-100 text-stone-700 dark:bg-stone-900/30 dark:text-stone-400",
    User: "bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400",
    Role: "bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-400",
    SystemLog: "bg-muted text-foreground dark:bg-muted dark:text-muted-foreground",
    Investment: "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400",
    HR: "bg-cyan-100 text-cyan-800 dark:bg-cyan-900/30 dark:text-cyan-400",
    SalaryBatch: "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400",
    SalaryRecipient: "bg-indigo-100 text-indigo-800 dark:bg-indigo-900/30 dark:text-indigo-400",
    Payroll: "bg-indigo-100 text-indigo-800 dark:bg-indigo-900/30 dark:text-indigo-400",
    Audit: "bg-muted text-foreground dark:bg-muted dark:text-muted-foreground"
};
