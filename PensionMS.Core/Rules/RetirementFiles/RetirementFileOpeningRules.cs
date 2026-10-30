namespace PensionMS.Core.Rules.RetirementFiles;

/// <summary>شروط فتح الملف؛ الحقائق والتاريخ التقويمي يقدمهما المستدعي دون قراءة أو تعديل بيانات.</summary>
public static class RetirementFileOpeningRules
{
    public enum RejectionReason
    {
        None, PreviousFile, MissingRetiree, MissingProcessedReferral,
        MissingEnrollmentDate, MissingRetirementDate, InvalidServicePeriod,
        MissingOpeningDate, OpeningBeforeRetirement, InactiveUser,
        MissingAssignedBranch, DifferentRequestedBranch, InactiveBranch, BranchNotAllowed
    }

    public readonly record struct Decision(RejectionReason Reason)
    {
        public bool IsAllowed => Reason == RejectionReason.None;
        public string? Message => Reason switch
        {
            RejectionReason.None => null,
            RejectionReason.PreviousFile => "يوجد ملف سابق لهذا المتقاعد؛ لا يمكن فتح ملف آخر",
            RejectionReason.MissingRetiree => "المتقاعد غير موجود أو محذوف",
            RejectionReason.MissingProcessedReferral => "يجب أن يرتبط المتقاعد بقيد إحالة غير محذوف ومكتمل المعالجة",
            RejectionReason.MissingEnrollmentDate => "تاريخ بداية الخدمة مفقود؛ استكمل تاريخ الالتحاق في كشف الإحالة أولاً",
            RejectionReason.MissingRetirementDate => "تاريخ التقاعد مفقود؛ استكمله في كشف الإحالة أولاً",
            RejectionReason.InvalidServicePeriod => "يجب أن يكون تاريخ التقاعد بعد تاريخ بداية الخدمة؛ صحح كشف الإحالة أولاً",
            RejectionReason.MissingOpeningDate => "تاريخ فتح الملف مطلوب",
            RejectionReason.OpeningBeforeRetirement => "تاريخ فتح الملف لا يمكن أن يسبق تاريخ التقاعد في كشف الإحالة",
            RejectionReason.InactiveUser => "المستخدم غير موجود أو غير فعّال",
            RejectionReason.MissingAssignedBranch => "يجب إسناد المستخدم إلى فرع قبل فتح الملف",
            RejectionReason.DifferentRequestedBranch => "لا يمكن فتح الملف لفرع مختلف عن فرع المستخدم",
            RejectionReason.InactiveBranch => "فرع المستخدم غير موجود أو غير فعّال",
            RejectionReason.BranchNotAllowed => "لا تملك صلاحية إنشاء ملف في فرع المستخدم",
            _ => throw new ArgumentOutOfRangeException(nameof(Reason))
        };
    }

    public readonly record struct SourceFacts(bool RetireeExists, bool RetireeIsDeleted,
        bool ReferralExists, bool ReferralIsDeleted, bool ReferralIsProcessed,
        DateOnly? EnrollmentDate, DateOnly? RetirementDate);

    public static Decision EvaluatePreviousFile(bool hasPreviousFile) =>
        new(hasPreviousFile ? RejectionReason.PreviousFile : RejectionReason.None);

    public static Decision EvaluateSource(in SourceFacts facts, DateOnly? openingDate)
    {
        if (!facts.RetireeExists || facts.RetireeIsDeleted) return new(RejectionReason.MissingRetiree);
        if (!facts.ReferralExists || facts.ReferralIsDeleted || !facts.ReferralIsProcessed)
            return new(RejectionReason.MissingProcessedReferral);
        if (!facts.EnrollmentDate.HasValue || facts.EnrollmentDate.Value == default)
            return new(RejectionReason.MissingEnrollmentDate);
        if (!facts.RetirementDate.HasValue || facts.RetirementDate.Value == default)
            return new(RejectionReason.MissingRetirementDate);
        if (facts.RetirementDate <= facts.EnrollmentDate) return new(RejectionReason.InvalidServicePeriod);
        if (!openingDate.HasValue || openingDate.Value == default) return new(RejectionReason.MissingOpeningDate);
        return new(openingDate < facts.RetirementDate ? RejectionReason.OpeningBeforeRetirement : RejectionReason.None);
    }

    public static Decision EvaluateBranch(bool activeUser, int? assignedBranchId, int? requestedBranchId,
        bool activeBranch, bool branchAllowed)
    {
        if (!activeUser) return new(RejectionReason.InactiveUser);
        if (assignedBranchId is null or <= 0) return new(RejectionReason.MissingAssignedBranch);
        if (requestedBranchId.HasValue && requestedBranchId != assignedBranchId)
            return new(RejectionReason.DifferentRequestedBranch);
        if (!activeBranch) return new(RejectionReason.InactiveBranch);
        return new(branchAllowed ? RejectionReason.None : RejectionReason.BranchNotAllowed);
    }
}
