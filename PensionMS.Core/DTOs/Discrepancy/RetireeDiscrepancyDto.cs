namespace PensionMS.Core.DTOs.Discrepancy;

public class RetireeDiscrepancyDto
{
    // Identity Information
    public int? ApprovalItemID { get; set; }
    public int? RetireeID { get; set; }
    public int? FileID { get; set; }

    public string? MilitaryNumber { get; set; }
    public string? NationalID { get; set; }

    // Phase 1: Approval Data (The Source)
    public string? ApprovalFullName { get; set; }
    public string? ApprovalRankName { get; set; }
<<<<<<< HEAD
    public DateOnly? ApprovalRetirementDate { get; set; }
=======
    public DateTime? ApprovalRetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? ApprovalUnitName { get; set; }
    public byte? Gender { get; set; }
    public decimal? Salary { get; set; }
    public string? CategoryName { get; set; }
    public string? LinkNumber { get; set; }

    // Phase 2: Retiree Data (The System Entry)
    public string? RetireeFullName { get; set; }
    public bool IsProcessed { get; set; }

    // Phase 3: File Data (The Final Output)
    public string? FileNumber { get; set; }
    public string? FileRankName { get; set; }
<<<<<<< HEAD
    public DateOnly? FileRetirementDate { get; set; }
=======
    public DateTime? FileRetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? FileStatusName { get; set; }

    // Discrepancy Flags (Calculated)
    public bool IsNameMismatch => !string.IsNullOrEmpty(ApprovalFullName) &&
                                  !string.IsNullOrEmpty(RetireeFullName) &&
                                  ApprovalFullName != RetireeFullName;

    public bool IsRankMismatch => !string.IsNullOrEmpty(ApprovalRankName) &&
                                  !string.IsNullOrEmpty(FileRankName) &&
                                  ApprovalRankName != FileRankName;

    public bool IsDateMismatch => ApprovalRetirementDate.HasValue &&
                                  FileRetirementDate.HasValue &&
<<<<<<< HEAD
                                  ApprovalRetirementDate.Value != FileRetirementDate.Value;
=======
                                  ApprovalRetirementDate.Value.Date != FileRetirementDate.Value.Date;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    public bool HasAnyDiscrepancy => IsNameMismatch || IsRankMismatch || IsDateMismatch;

    public string CurrentStage => FileID.HasValue ? "Stage 3: File Created" :
                                 RetireeID.HasValue ? "Stage 2: Processed" :
                                 "Stage 1: Referral Only";
}

public class DiscrepancyFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? OnlyMismatches { get; set; }
    public string? Stage { get; set; }
    public string? UnitName { get; set; }
    public string? RankName { get; set; }
    public string? CategoryName { get; set; }
    public byte? Gender { get; set; }
    public string? ProcessingStatus { get; set; }
    public string? FileStatus { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class KashfReportSummaryDto
{
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public int PendingCount { get; set; }
    public int FileCreatedCount { get; set; }
    public List<NameValuePairDto> ByUnit { get; set; } = new();
    public List<NameValuePairDto> ByRank { get; set; } = new();
    public List<NameValuePairDto> ByCategory { get; set; } = new();
    public List<NameValuePairDto> ByGender { get; set; } = new();
}

public class NameValuePairDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
