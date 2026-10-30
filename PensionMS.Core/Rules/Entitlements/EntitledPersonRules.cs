using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Rules.Documents;

namespace PensionMS.Core.Rules.Entitlements;

/// <summary>
/// قواعد وثائق المستحقين المشتركة — مصدر واحد يستهلكه محرك التفعيل (EntitlementEngine)
/// وبوابة الامتثال (MissingDocumentService.BuildPairsCoreAsync) حتى لا تختلف شروط الاستيفاء.
/// معرفات الصلات الثابتة وفق البذرة: 1=زوجة، 2=ابن، 3=بنت، 4=أم، 5=أب، 6=أخت، 7=أخ، 8=زوج.
/// </summary>
public static class EntitledPersonRules
{
    /// <summary>صلات القرابة الأنثوية وفق معرفات البذرة (زوجة، ابنة، أم، أخت).</summary>
    public static bool IsFemaleRelationship(int? relationshipId) =>
        relationshipId is 1 or 3 or 4 or 6;

    public readonly record struct EntitledPersonProfile(
        int RelationshipID, int Age, int? SocialStatusID, int? HealthStatusID, bool HasDisabilityException);

    public readonly record struct DocumentRequirement(int DocumentTypeID, string Label);

    public readonly record struct EligibilityDecision(bool IsEligible, string? StopCode, string Message);

    public readonly record struct StopState(
        bool IsActive, string? Relationship, DateOnly BirthDate, bool HasDisabilityException);

    /// <param name="SocialStatusID">قيمة مقترحة عند الزواج؛ null يعني إبقاء الحالة الحالية.</param>
    public readonly record struct StopDecision(bool IsAllowed, string? ErrorMessage, int? SocialStatusID);

    /// <summary>قرار الإيقاف وقيمة الحالة الاجتماعية المقترحة، دون تعديل بيانات المستحق.</summary>
    public static StopDecision EvaluateStop(in StopState state, string? stopCode, DateOnly evaluationDate)
    {
        if (!state.IsActive)
            return new(false, "المستحق موقوف مسبقاً", null);

        var error = stopCode switch
        {
            EntitledStopCodes.AgeLimit when state.Relationship != RelationshipConstants.Son =>
                "إيقاف بسبب العمر مخصص للأبناء فقط",
            EntitledStopCodes.AgeLimit when state.HasDisabilityException =>
                "لا يمكن إيقاف هذا الابن بسبب العمر — لديه استثناء إعاقة/مرض نفسي",
            EntitledStopCodes.AgeLimit when PensionMS.Core.Time.YemenTime.Age(state.BirthDate, evaluationDate) < 24 =>
                $"الابن لم يبلغ 24 سنة بعد (العمر الحالي: {PensionMS.Core.Time.YemenTime.Age(state.BirthDate, evaluationDate)} سنة)",
            EntitledStopCodes.DaughterMarried when state.Relationship != RelationshipConstants.Daughter =>
                "إيقاف بسبب الزواج مخصص للبنات فقط",
            EntitledStopCodes.WidowRemarried when state.Relationship != RelationshipConstants.Wife =>
                "إيقاف بسبب إعادة الزواج مخصص للزوجات فقط",
            _ => null
        };
        if (error != null) return new(false, error, null);

        return new(true, null, stopCode is EntitledStopCodes.DaughterMarried or EntitledStopCodes.WidowRemarried
            ? SocialStatusConstants.Married : null);
    }

    /// <param name="RetireeIsAlive">null عند غياب بيانات المتقاعد.</param>
    public readonly record struct ReactivationState(
        bool IsActive, bool IsDeceased, string? StopCode, string? Relationship, DateOnly BirthDate,
        int? SocialStatusID, bool HasDisabilityException, bool? RetireeIsAlive);

    public readonly record struct ReactivationDecision(
        bool IsAllowed, string? ErrorMessage, int? SocialStatusID, bool HasDisabilityException);

    /// <summary>قرار إعادة التفعيل؛ تُفحص الأهلية بالقيم المقترحة قبل تطبيقها في الخدمة.</summary>
    public static ReactivationDecision EvaluateReactivation(in ReactivationState state,
        string? reactivationCode, DateOnly evaluationDate)
    {
        if (state.IsActive)
            return new(false, "المستحق نشط بالفعل", null, false);
        if (state.IsDeceased)
            return new(false, "لا يمكن إعادة تفعيل مستحق متوفى", null, false);

        var compatible = state.StopCode switch
        {
            EntitledStopCodes.DaughterMarried or EntitledStopCodes.WidowRemarried =>
                reactivationCode is ReactivationCodes.Divorced or ReactivationCodes.HusbandDeceased,
            EntitledStopCodes.AgeLimit => reactivationCode == ReactivationCodes.DisabilityReport,
            EntitledStopCodes.AdminDecision => reactivationCode == ReactivationCodes.AdminDecision,
            _ => false
        };
        if (!compatible)
            return new(false, $"رمز إعادة التفعيل ({reactivationCode}) غير متوافق مع سبب الإيقاف ({state.StopCode})", null, false);

        var socialStatus = reactivationCode switch
        {
            ReactivationCodes.Divorced => SocialStatusConstants.Divorced,
            ReactivationCodes.HusbandDeceased => SocialStatusConstants.Widowed,
            _ => state.SocialStatusID
        };
        var disabilityException = state.HasDisabilityException || reactivationCode == ReactivationCodes.DisabilityReport;
        if (!state.RetireeIsAlive.HasValue)
            return new(false, "بيانات المتقاعد غير موجودة", null, false);

        var eligibility = EvaluateEligibility(state.RetireeIsAlive.Value, state.Relationship, state.BirthDate,
            socialStatus, disabilityException, evaluationDate);
        if (!eligibility.IsEligible) return new(false, eligibility.Message, null, false);

        return new(true, null, socialStatus, disabilityException);
    }

    public readonly record struct AutomaticEntitlementState(
        int EntitledID, int FileID, bool IsActive, bool IsDeleted, bool IsDeceased, bool? RetireeIsAlive,
        string? StopCode, int RelationshipID, string? Relationship, DateOnly BirthDate, string? FullName,
        int? SocialStatusID, int? HealthStatusID, bool HasDisabilityException, string? NationalID);

    public enum AutomaticStopDateChange { Preserve, SetUtcNow, Clear }

    /// <param name="ReplaceStopFields">false يحفظ رمز وسبب الإيقاف؛ true يستبدلهما، بما في ذلك مسحهما بقيم null.</param>
    public readonly record struct AutomaticStateChange(
        bool IsActive, bool ReplaceStopFields, string? StopCode, string? StopReason,
        AutomaticStopDateChange StopDateChange, bool RefreshUpdatedAt);

    /// <param name="Change">null يعني عدم تعديل الحالة أو حفظها.</param>
    public sealed record AutomaticDecision(bool IsSuccess, string Message, AutomaticStateChange? Change);

    /// <summary>الأولوية قبل تحميل الأدلة؛ null يعني متابعة فحص البيانات والوثائق.</summary>
    public static AutomaticDecision? EvaluateAutomaticPrerequisites(in AutomaticEntitlementState state, DateOnly evaluationDate)
    {
        if (state.IsDeleted || state.IsDeceased)
            return new(true, "المستحق محذوف أو متوفى", null);
        if (!state.RetireeIsAlive.HasValue)
            return new(false, "بيانات المتقاعد غير موجودة", null);

        if (state.StopCode != null && state.StopCode != EntitledStopCodes.RetireeAlive && state.StopCode != EntitledStopCodes.MissingData)
            return new(true, "المستحق لديه إيقاف يدوي أو سبب آخر فلا يتم تفعيله تلقائياً",
                new(false, false, null, null, AutomaticStopDateChange.Preserve, false));

        if (state.RetireeIsAlive.Value)
        {
            var replace = state.IsActive || state.StopCode != EntitledStopCodes.RetireeAlive;
            return new(true, "تم تحديث الحالة: المتقاعد على قيد الحياة",
                new(false, replace, EntitledStopCodes.RetireeAlive, "المتقاعد على قيد الحياة — الاستحقاق معلق",
                    replace ? AutomaticStopDateChange.SetUtcNow : AutomaticStopDateChange.Preserve, replace));
        }

        var eligibility = EvaluateEligibility(state.RetireeIsAlive.Value, state.Relationship, state.BirthDate,
            state.SocialStatusID, state.HasDisabilityException, evaluationDate);
        if (!eligibility.IsEligible)
            return new(true, eligibility.Message,
                new(false, true, eligibility.StopCode, eligibility.Message, AutomaticStopDateChange.SetUtcNow, false));
        return null;
    }

    public static IReadOnlyList<DocumentRequirement> AutomaticDocumentRequirements(in AutomaticEntitlementState state,
        DateOnly evaluationDate) => DocumentRequirements(new EntitledPersonProfile(state.RelationshipID,
            state.BirthDate == default ? 0 : PensionMS.Core.Time.YemenTime.Age(state.BirthDate, evaluationDate),
            state.SocialStatusID, state.HealthStatusID, state.HasDisabilityException)).AsReadOnly();

    public sealed record AutomaticDeficiencies(IReadOnlyList<string> Issues,
        IReadOnlyList<RetirementDocumentRequirement> MissingProofs, bool NeedsCoverage);

    /// <summary>البيانات أولاً ثم الأدلة بترتيب التعريف، مع استخدام قواعد الصلاحية والملكية المشتركة.</summary>
    public static AutomaticDeficiencies EvaluateAutomaticDeficiencies(in AutomaticEntitlementState state,
        IReadOnlyList<DocumentRequirement> requirements, IReadOnlySet<int> activeDocumentTypeIds,
        IReadOnlyCollection<Document> documents, DateOnly evaluationDate)
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(state.FullName)) issues.Add("الاسم الكامل");
        if (state.BirthDate == default) issues.Add("تاريخ الميلاد");
        if (state.SocialStatusID == null || state.SocialStatusID == 0) issues.Add("الحالة الاجتماعية");
        if (state.HealthStatusID == null || state.HealthStatusID == 0) issues.Add("الحالة الصحية");
        if (state.BirthDate != default && PensionMS.Core.Time.YemenTime.Age(state.BirthDate, evaluationDate) >= 18 &&
            string.IsNullOrWhiteSpace(state.NationalID)) issues.Add("الرقم الوطني");

        var missingProofs = new List<RetirementDocumentRequirement>();
        var fileId = state.FileID;
        var uploaded = documents.Where(d => d.FileID == fileId && RetirementFileDocumentRules.IsValid(d, evaluationDate))
            .Select(RetirementFileDocumentRules.Owner).ToHashSet();
        foreach (var requirement in requirements.Where(r => activeDocumentTypeIds.Contains(r.DocumentTypeID)))
        {
            var owner = new RetirementDocumentRequirement(requirement.DocumentTypeID, null, null, null, state.EntitledID, null, null);
            if (!uploaded.Contains(owner))
            {
                missingProofs.Add(owner);
                issues.Add(requirement.Label);
            }
        }
        return new(issues.AsReadOnly(), missingProofs.AsReadOnly(),
            state.IsActive && missingProofs.Count > 0 && issues.Count == missingProofs.Count);
    }

    /// <summary>سجلات النقص المقيدة بالملف والمستحق ومعرفات التغطية السارية، محملة مسبقاً من الخدمة.</summary>
    public readonly record struct AutomaticCoverage(IReadOnlyCollection<MissingDocument> MissingDocuments,
        ISet<int> ValidPledgeIds, ISet<int> ValidExemptionIds);

    public static AutomaticDecision CompleteAutomaticEvaluation(AutomaticDeficiencies deficiencies, AutomaticCoverage? coverage = null)
    {
        var covered = deficiencies.NeedsCoverage && coverage is { } facts && deficiencies.MissingProofs.All(r =>
            RetirementFileDocumentRules.IsCovered(r, deficiencies.MissingProofs, facts.MissingDocuments,
                facts.ValidPledgeIds, facts.ValidExemptionIds));
        if (deficiencies.Issues.Count > 0 && !covered)
            return new(true, "المستحق لا يزال لديه نواقص",
                new(false, true, EntitledStopCodes.MissingData, $"بيانات/وثائق ناقصة: {string.Join("، ", deficiencies.Issues)}",
                    AutomaticStopDateChange.Preserve, true));

        return new(true, "تم تفعيل المستحق بنجاح",
            new(true, true, null, null, AutomaticStopDateChange.Clear, true));
    }

    /// <summary>أهلية قانونية للبيانات الحالية؛ اعتماد المراجعة والسماح بالصرف قراران منفصلان.</summary>
    public static EligibilityDecision EvaluateEligibility(bool retireeIsAlive, string? relationship,
        DateOnly birthDate, int? socialStatusId, bool disabilityException, DateOnly today)
    {
        if (retireeIsAlive)
            return new(false, EntitledStopCodes.RetireeAlive, "لا يمكن التفعيل: المتقاعد لا يزال على قيد الحياة");
        if (birthDate == default || birthDate > today)
            return new(false, EntitledStopCodes.MissingData, "تاريخ الميلاد غير محدد أو غير صحيح — لا يمكن تقييم الأهلية");
        if (relationship == RelationshipConstants.Son && PensionMS.Core.Time.YemenTime.Age(birthDate, today) >= 24 && !disabilityException)
            return new(false, EntitledStopCodes.AgeLimit, "المستحق غير مؤهل: تجاوز السن القانوني (24 سنة)");
        if (relationship == RelationshipConstants.Daughter && socialStatusId == SocialStatusConstants.Married)
            return new(false, EntitledStopCodes.DaughterMarried, "المستحق غير مؤهل: البنت متزوجة");
        return new(true, null, "المستحق مستوفٍ للشروط القانونية الأساسية");
    }

    /// <summary>نوع دليل الإيقاف؛ نفس النوع يُرفع أو يُسجل نقصاً عند غياب المرفق.</summary>
    public static int? StopDocumentType(string stopCode) => stopCode switch
    {
        EntitledStopCodes.DaughterMarried or EntitledStopCodes.WidowRemarried => DocumentTypeConstants.MarriageContract,
        EntitledStopCodes.Deceased => DocumentTypeConstants.DeathCertificate,
        _ => null
    };

    public readonly record struct ReactivationDocuments(int? DocumentTypeID, int? CivilServiceProofTypeID);

    /// <summary>الأدلة الأساسية والمساندة لإعادة التفعيل؛ لا تغيّر مصفوفة الانتقالات المسموحة.</summary>
    public static ReactivationDocuments ReactivationDocumentRequirements(string reactivationCode) => reactivationCode switch
    {
        ReactivationCodes.Divorced => new(DocumentTypeConstants.DivorceCertificate, null),
        ReactivationCodes.HusbandDeceased => new(DocumentTypeConstants.DeathCertificate, DocumentTypeConstants.CivilServiceProof),
        ReactivationCodes.DisabilityReport => new(DocumentTypeConstants.MedicalReport, null),
        _ => new(null, null)
    };

    /// <summary>وثائق الحالة الاجتماعية والصحية الإلزامية لمستحق وفق بياناته — بلا أثر لقاعدة البيانات.</summary>
    public static List<DocumentRequirement> DocumentRequirements(in EntitledPersonProfile profile)
    {
        var requirements = new List<DocumentRequirement>();

        if (profile.Age >= 18)
            requirements.Add(new(DocumentTypeConstants.NationalId, "وثيقة الهوية الوطنية"));

        if (profile.HealthStatusID is HealthStatusConstants.Deceased or HealthStatusConstants.Martyr)
        {
            // نفس متطلبات خدمة النواقص: شهادة الوفاة للأنثى بدلاً من وثائق الحالة الاجتماعية.
            if (IsFemaleRelationship(profile.RelationshipID))
                requirements.Add(new(DocumentTypeConstants.DeathCertificate, "شهادة الوفاة"));
        }
        else if (IsFemaleRelationship(profile.RelationshipID))
        {
            // القاصر الأنثى معفاة من إثبات العزوبية — فقط البالغة تُطالب بها
            if (profile.SocialStatusID == SocialStatusConstants.Single && profile.Age >= 18)
                requirements.Add(new(DocumentTypeConstants.SingleStatusCert, "إثبات العزوبية"));

            if (profile.SocialStatusID == SocialStatusConstants.Married)
                requirements.Add(new(DocumentTypeConstants.MarriageContract, "عقد الزواج"));

            if (profile.SocialStatusID == SocialStatusConstants.Divorced)
                requirements.Add(new(DocumentTypeConstants.DivorceCertificate, "وثيقة الطلاق"));
        }

        if (profile.HasDisabilityException ||
            profile.HealthStatusID == HealthStatusConstants.PartiallyDisabled ||
            profile.HealthStatusID == HealthStatusConstants.FullyDisabled)
        {
            requirements.Add(new(DocumentTypeConstants.MedicalReport, "التقرير الطبي"));
            requirements.Add(new(DocumentTypeConstants.MaintenanceJudgment, "حكم الإعالة"));
        }

        return requirements;
    }
}
