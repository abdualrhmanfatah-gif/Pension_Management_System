using PensionMS.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.Ministry;

public class MinistryApprovalBatchDto
{
    public int BatchID { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime BatchDate { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ImportedByUser { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBatchDto
{
    [Required(ErrorMessage = "رقم الكشف مطلوب")]
    public string BatchNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الكشف مطلوب")]
    public DateTime BatchDate { get; set; }

    [Required(ErrorMessage = "تاريخ الورود مطلوب")]
    public DateTime ReceivedDate { get; set; }

    public string? Notes { get; set; }
}

public class MinistryApprovalItemDto
{
    public int ApprovalItemID { get; set; }
    public int SerialNumber { get; set; }
    public string MilitaryNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public byte? Gender { get; set; }
<<<<<<< HEAD
    public int? RankID { get; set; }
    public string? RankName { get; set; }
    public int? CategoryID { get; set; }
    public string? UnitName { get; set; }
    public DateOnly? EnrollmentDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
=======
    public string? RankName { get; set; }
    public string? UnitName { get; set; }
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? RetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public decimal? Salary { get; set; }
    public string? CategoryName { get; set; }
    public string? Notes { get; set; }
    public string? LinkNumber { get; set; }
    public bool IsProcessed { get; set; }
    public int? RetireeID { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? BatchDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedByUser { get; set; }
}

public class ImportBatchFromExcelDto
{
    [Required]
    public IRemoteFile File { get; set; } = null!;
}

public class AddManualItemDto
{
    [Required(ErrorMessage = "الرقم العسكري مطلوب")]
    public string MilitaryNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    public int? RankID { get; set; }
    public string? UnitName { get; set; }
<<<<<<< HEAD
    public DateOnly? EnrollmentDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public decimal? Salary { get; set; }
    public int? CategoryID { get; set; }
    public byte? Gender { get; set; }
    public string? Notes { get; set; }
    public string? LinkNumber { get; set; }
}

public class UpdateManualItemDto
{
    [Required(ErrorMessage = "الرقم العسكري مطلوب")]
    public string MilitaryNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    public int? RankID { get; set; }
    public string? UnitName { get; set; }
    public DateOnly? EnrollmentDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
=======
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? RetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public decimal? Salary { get; set; }
    public int? CategoryID { get; set; }
    public byte? Gender { get; set; }
    public string? Notes { get; set; }
    public string? LinkNumber { get; set; }
}
