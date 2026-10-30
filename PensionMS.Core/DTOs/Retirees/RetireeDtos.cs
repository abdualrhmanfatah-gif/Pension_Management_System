namespace PensionMS.Core.DTOs.Retirees;

// ─── Main read DTO ────────────────────────────────────────────
public class RetireeDto
{
    public int RetireeID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? MilitaryNumber { get; set; }
    public string? LinkNumber { get; set; }
    public byte Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public string? UnitName { get; set; }
    public string? RankName { get; set; }
    public string? CategoryName { get; set; }
<<<<<<< HEAD
    public DateOnly? EnrollmentDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public bool IsAlive { get; set; }
    public DateOnly? DeathDate { get; set; }
=======
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? RetirementDate { get; set; }
    public bool IsAlive { get; set; }
    public DateTime? DeathDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? PersonalPhotoPath { get; set; }
    public string? SocialStatusName { get; set; }
    public string? HealthStatusName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PendingReviewStatus { get; set; }
}

// ─── List DTO (dashboard / paged) ────────────────────────────
public class RetireeListDto
{
    public int RetireeID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? MilitaryNumber { get; set; }
    public string? LinkNumber { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public string? RankName { get; set; }
    public string? UnitName { get; set; }
    public string? SocialStatusName { get; set; }
    public string? HealthStatusName { get; set; }
    public bool IsAlive { get; set; }
    public int? CategoryId { get; set; }
    public int? RankId { get; set; }
    public byte Gender { get; set; }
    public int FileCount { get; set; }
    public int? FileID { get; set; }
}

// ─── Social Research Query DTO ────────────────────────────────
public class SocialResearchQueryDto
{
    public string? Search { get; set; }
    public int? SocialStatusId { get; set; }
    public int? HealthStatusId { get; set; }
    public bool? IsAlive { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public int? CategoryId { get; set; }
    public int? RankId { get; set; }
    public byte? Gender { get; set; }
    public bool? HasFiles { get; set; }
<<<<<<< HEAD
    public bool? IsProcessed { get; set; }
    public bool? HasSettlements { get; set; }
    public string? UnitName { get; set; }
    public DateOnly? RetirementDateFrom { get; set; }
    public DateOnly? RetirementDateTo { get; set; }
=======
    public DateTime? RetirementDateFrom { get; set; }
    public DateTime? RetirementDateTo { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ─── Update DTO ───────────────────────────────────────────────
public class UpdateRetireeDto
{
    public string? FullName { get; set; }
    public string? NationalID { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? LinkNumber { get; set; }
    public byte? Gender { get; set; }
<<<<<<< HEAD
    public DateOnly? BirthDate { get; set; }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }

    // ملفات المتقاعد للتحديث
    public PensionMS.Core.Interfaces.IRemoteFile? PersonalPhoto { get; set; }
    public PensionMS.Core.Interfaces.IRemoteFile? IdDocument { get; set; }
    public PensionMS.Core.Interfaces.IRemoteFile? IdDocumentBack { get; set; }
}

// ─── Full statement DTO ───────────────────────────────────────
public class RetireeStatementDto
{
    public RetireeDto Retiree { get; set; } = null!;
    public List<ApprovalItemSummaryDto> ApprovalItems { get; set; } = new();
    public List<RetirementFileSummaryDto> RetirementFiles { get; set; } = new();
    public List<SettlementSummaryDto> Settlements { get; set; } = new();
    public List<EntitlementSummaryDto> Entitlements { get; set; } = new();
}

// ─── Nested summary DTOs for statement ───────────────────────
public class ApprovalItemSummaryDto
{
    public int ApprovalItemID { get; set; }
    public string? BatchNumber { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? FullName { get; set; }
    public byte? Gender { get; set; }
    public string? RankName { get; set; }
    public string? UnitName { get; set; }
<<<<<<< HEAD
    public DateOnly? RetirementDate { get; set; }
=======
    public DateTime? RetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public decimal? Salary { get; set; }
    public string? CategoryName { get; set; }
    public string? Notes { get; set; }
    public bool IsProcessed { get; set; }
}

public class RetirementFileSummaryDto
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? StatusName { get; set; }
    public string? RankName { get; set; }
    public string? BranchName { get; set; }
    public DateTime? FileOpenDate { get; set; }
}

public class SettlementSummaryDto
{
    public int SettlementItemID { get; set; }
    public string? BatchNumber { get; set; }
    public string? DecisionNumber { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? DecisionType { get; set; }
    public string? CurrentRankName { get; set; }
    public string? ProcessedRankName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EntitlementSummaryDto
{
    public int EntitlementID { get; set; }
    public string? RankName { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ─── Full Data DTO for File Creation Prefill ─────────────────────
public class RetireeFullDataDto
{
    public int RetireeID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? MilitaryNumber { get; set; }
    public string? RankName { get; set; }
    public string? LinkNumber { get; set; }
    public byte Gender { get; set; }
<<<<<<< HEAD
    public DateOnly? BirthDate { get; set; }
=======
    public DateTime? BirthDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }
    public bool IsAlive { get; set; }
<<<<<<< HEAD
    public DateOnly? DeathDate { get; set; }
    public string? PersonalPhotoPath { get; set; }
}
=======
    public DateTime? DeathDate { get; set; }
    public string? PersonalPhotoPath { get; set; }
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
