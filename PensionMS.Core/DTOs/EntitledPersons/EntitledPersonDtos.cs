using PensionMS.Core.Interfaces;

namespace PensionMS.Core.DTOs.EntitledPersons;

// ─── Stop Codes (سبب الإيقاف) ─────────────────────────────────
public static class EntitledStopCodes
{
    public const string AgeLimit        = "AGE_LIMIT";          // الابن بلغ 24 سنة
    public const string DaughterMarried = "DAUGHTER_MARRIED";   // البنت تزوجت
    public const string WidowRemarried  = "WIDOW_REMARRIED";    // الزوجة/الأرملة تزوجت مجدداً
    public const string Deceased        = "DECEASED";           // وفاة المستحق
    public const string AdminDecision   = "ADMIN_DECISION";     // قرار إداري
    public const string Employed        = "EMPLOYED";           // التحق بوظيفة
    public const string RetireeAlive    = "RETIREE_ALIVE";     // المتقاعد على قيد الحياة — الاستحقاق معلق
    public const string MissingData     = "MISSING_DATA";      // بيانات/وثائق ناقصة بعد وفاة المتقاعد
}

// ─── Reactivation Codes (سبب إعادة التفعيل) ──────────────────
public static class ReactivationCodes
{
    public const string Divorced        = "DIVORCED";           // طلاق (للبنت أو الأرملة)
    public const string HusbandDeceased = "HUSBAND_DECEASED";   // وفاة الزوج الجديد (للبنت أو الأرملة)
    public const string DisabilityReport = "DISABILITY_REPORT"; // تقرير طبي إعاقة/مرض نفسي (للابن)
    public const string AdminDecision   = "ADMIN_DECISION";     // انتهاء القرار الإداري
}

// ─── Create DTO ────────────────────────────────────────────────
public class CreateEntitledPersonDto
{
    public int FileID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public int? RelationshipID { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }
    public bool IsEligible { get; set; } = true;
    /// <summary>
    /// معاق أو مريض نفسي → يُعفى الابن من حد عمر 24 سنة
    /// </summary>
    public bool HasDisabilityException { get; set; }

    /// <summary>
    /// صورة الهوية الوطنية للبالغين
    /// </summary>
    public IRemoteFile? NationalIdImage { get; set; }
    public IRemoteFile? NationalIdBackImage { get; set; }
}

// ─── Update DTO ────────────────────────────────────────────────
public class UpdateEntitledPersonDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }
    public bool? IsEligible { get; set; }
    public bool? HasDisabilityException { get; set; }
    public int? RelationshipID { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalID { get; set; }
    public IRemoteFile? NationalIdImage { get; set; }
    public IRemoteFile? NationalIdBackImage { get; set; }
    public IRemoteFile? PersonalPhoto { get; set; }

    /// <summary>
    /// آخر تاريخ تعديل معروف للعميل — للتحقق من عدم التعارض
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}


// ─── Read DTO ──────────────────────────────────────────────────
public class EntitledPersonDto
{
    public int EntitledID { get; set; }
    public int FileID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? RelationshipName { get; set; }
    public int RelationshipID { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? AgeYears { get; set; }
    public string? Phone { get; set; }
    public int? SocialStatusID { get; set; }
    public string? SocialStatusName { get; set; }
    public int? HealthStatusID { get; set; }
    public string? HealthStatusName { get; set; }
    public bool IsEligible { get; set; }
    public bool IsDeceased { get; set; }
    public DateTime? StopDate { get; set; }
    public string? StopReason { get; set; }
    public string? StopCode { get; set; }
    public bool HasDisabilityException { get; set; }
    public bool HasActiveGuardian { get; set; }
    public bool HasNationalIdDocument { get; set; }
    public bool HasPersonalPhoto { get; set; }
    public string? PersonalPhotoPath { get; set; }
    public string? NationalIdImageUrl { get; set; }
    public int PendingReviewStatus { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ─── Stop / Reactivate DTOs ────────────────────────────────────
public class StopEntitledPersonDto
{
    /// <summary>
    /// AGE_LIMIT | DAUGHTER_MARRIED | WIDOW_REMARRIED | DECEASED | ADMIN_DECISION | EMPLOYED
    /// </summary>
    public string StopCode { get; set; } = string.Empty;
    public string? Notes { get; set; }

    /// <summary>
    /// وثيقة مطلوبة:
    /// DAUGHTER_MARRIED / WIDOW_REMARRIED → عقد زواج (DocumentTypeID=6)
    /// DECEASED → شهادة وفاة (DocumentTypeID=2)
    /// </summary>
    public IRemoteFile? Document { get; set; }
}

public class ReactivateEntitledPersonDto
{
    /// <summary>
    /// DIVORCED | HUSBAND_DECEASED | DISABILITY_REPORT | ADMIN_DECISION
    /// </summary>
    public string ReactivationCode { get; set; } = string.Empty;

    /// <summary>
    /// الوثيقة الأساسية المطلوبة:
    /// DIVORCED → وثيقة طلاق (DocumentTypeID=14)
    /// HUSBAND_DECEASED → شهادة وفاة الزوج (DocumentTypeID=2)
    /// DISABILITY_REPORT → تقرير طبي (DocumentTypeID=16)
    /// </summary>
    public IRemoteFile? Document { get; set; }

    /// <summary>
    /// وثيقة ثانية — مطلوبة فقط عند HUSBAND_DECEASED:
    /// إثبات الخدمة المدنية (DocumentTypeID=15)
    /// </summary>
    public IRemoteFile? CivilServiceProof { get; set; }
}

public class RecordDeathDto
{
    public DateTime DeathDate { get; set; }

    /// <summary>
    /// شهادة الوفاة — مطلوبة (DocumentTypeID=2)
    /// </summary>
    public IRemoteFile? DeathCertificate { get; set; }
}

public class ManualActivateEntitledPersonDto
{
    public string? Reason { get; set; }
    
    /// <summary>
    /// تحويل النواقص الحالية إلى "تعهدات" تلقائياً عند التفعيل اليدوي
    /// </summary>
    public bool CreatePledgesForMissingDocs { get; set; } = true;
    
    public int? PledgeDeadlineMonths { get; set; } = 3;
}

// ─── Search DTO ─────────────────────────────────────────────
public class EntitledPersonSearchResultDto
{
    public int EntitledID { get; set; }
    public int FileID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? RelationshipName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeceased { get; set; }
    public string? PersonalPhotoPath { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? RetireeMilitaryNumber { get; set; }
}
