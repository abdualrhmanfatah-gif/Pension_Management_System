// ─── ترجمة أسماء الحقول التقنية إلى عربية مقروءة ───
// مرجع مركزي لجميع صفحات سجل التدقيق والتغييرات

const fieldTranslations: Record<string, string> = {
    // ─── المعرّفات العامة ───
    Id: "المعرّف",
    Code: "الرمز",
    Type: "النوع",
    Title: "العنوان",
    Description: "الوصف",
    Notes: "ملاحظات",
    Reason: "السبب",
    Status: "الحالة",
    NameAr: "الاسم (عربي)",
    NameEn: "الاسم (إنجليزي)",

    // ─── التواريخ ───
    CreatedAt: "تاريخ الإنشاء",
    UpdatedAt: "تاريخ التحديث",
    ProcessedAt: "تاريخ المعالجة",

    // ─── المستخدمون ───
    CreatedBy: "أنشئ بواسطة",
    UpdatedBy: "عدّل بواسطة",
    DeletedBy: "حُذف بواسطة",
    ProcessedByUserID: "تمت المعالجة بواسطة",

    // ─── الحقول المنطقية ───
    IsActive: "نشط",
    IsDeleted: "محذوف",
    IsArchived: "مؤرشف",

    // ─── الملف التقاعدي ───
    FileID: "رقم الملف",
    FileNumber: "رقم الملف",
    FileStatusID: "حالة الملفات",
    FileOpenDate: "تاريخ فتح الملف",
    RetirementDate: "تاريخ التقاعد",
    EndOfServiceDate: "تاريخ نهاية الخدمة",
    StartOfServiceDate: "تاريخ بداية الخدمة",
    MilitaryNumber: "الرقم العسكري",
    SerialNumber: "الرقم التسلسلي",
    UnitName: "الجهة",
    RankID: "الرتبة",
    PensionBranchID: "فرع المعاش",
    Salary: "المعاش",

    // ─── المتقاعد ───
    RetireeID: "المتقاعد",
    FullName: "الاسم الكامل",
    FirstName: "الاسم الأول",
    LastName: "اللقب",
    NationalID: "الرقم الوطني",
    PhoneNumber: "رقم الهاتف",
    Phone: "الهاتف",
    Email: "البريد الإلكتروني",
    Address: "العنوان",
    DateOfBirth: "تاريخ الميلاد",
    BirthDate: "تاريخ الميلاد",
    Gender: "الجنس",
    IsAlive: "على قيد الحياة",

    // ─── الورثة والمستفيدين ───
    relationshipID: "صلة القرابة",
    socialStatusID: "الحالة الاجتماعية",
    healthStatusID: "الحالة الصحية",
    hasDisabilityException: "استثناء عجز",

    // ─── الوثائق ───
    DocumentTypeID: "نوع الوثيقة",
    MissingDocumentsCount: "الوثائق الناقصة",

    // ─── المالي والتعويضات ───
    Percentage: "النسبة",
    Amount: "المبلغ",

    // ─── الوظائف ───
    EmployeeNo: "الرقم الوظيفي",
    UserID: "معرّف المستخدم",
};

/**
 * ترجمة اسم الحقل التقني إلى عربية مقروءة
 * @example translateFieldName("FileStatusID") // "حالة الملفات"
 * @example translateFieldName("unknownField") // "unknownField"
 */
export function translateFieldName(fieldName: string): string {
    return fieldTranslations[fieldName] ?? fieldName;
}

export default fieldTranslations;
