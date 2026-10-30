using System;
using PensionMS.Core.DTOs.Discrepancy;
namespace PensionMS.Core.DTOs.Reports;

/// <summary>
/// فلتر تقارير ملفات التقاعد
/// </summary>
public class RetirementFileReportFilter
{
    public string? SearchTerm { get; set; }
    public int? StatusId { get; set; }
    public int? RankId { get; set; }
    public byte? Gender { get; set; }
    public int? BranchId { get; set; }
    public string? Governorate { get; set; }
    public int? ReviewStatus { get; set; }
    public int? SocialStatusId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// بيانات السجل التفصيلية لملف التقاعد
/// </summary>
public class RetirementFileReportRow
{
    public int FileID { get; set; }
    public string? FileNumber { get; set; }
    public string? RetireeName { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? NationalID { get; set; }
    public int? RankID { get; set; }
    public string? RankName { get; set; }
    public int? PensionBranchID { get; set; }
    public string? BranchName { get; set; }
    public string? GovernorateName { get; set; }
    public int? StatusID { get; set; }
    public string? StatusName { get; set; }
    public byte? Gender { get; set; }
    public int? SocialStatusID { get; set; }
    public string? SocialStatusName { get; set; }
    public string? HealthStatusName { get; set; }
<<<<<<< HEAD
    public DateOnly? RetirementDate { get; set; }
=======
    public DateTime? RetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public DateTime? FileOpenDate { get; set; }
    public int MissingDocumentsCount { get; set; }
    public string? ReviewStatusName { get; set; }
    public bool IsAlive { get; set; }
}

/// <summary>
/// ملخص تقارير ملفات التقاعد
/// </summary>
public class RetirementFileReportSummaryDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int SuspendedCount { get; set; }
    public int ClosedCount { get; set; }
    public int IncompleteCount { get; set; }
    public int PendingReviewCount { get; set; }
    public int AliveCount { get; set; }
    public int DeceasedCount { get; set; }
    public List<NameValuePairDto> ByRank { get; set; } = new();
    public List<NameValuePairDto> ByBranch { get; set; } = new();
    public List<NameValuePairDto> ByGovernorate { get; set; } = new();
    public List<NameValuePairDto> ByGender { get; set; } = new();
    public List<NameValuePairDto> ByStatus { get; set; } = new();
    public List<NameValuePairDto> BySocialStatus { get; set; } = new();
}
