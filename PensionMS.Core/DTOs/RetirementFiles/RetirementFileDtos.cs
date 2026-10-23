using PensionMS.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.RetirementFiles;

public class CreateRetirementFileDto
{
    public int? BranchID { get; set; }
    public int RetireeID { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public DateOnly? ServiceStartDate { get; set; }
    public DateOnly? ServiceEndDate { get; set; }
    public string? Notes { get; set; }

    // Retiree Data Update
    public string? NationalID { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public byte Gender { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }
    public DateOnly? BirthDate { get; set; }

    public IRemoteFile? PersonalPhoto { get; set; }
    public IRemoteFile? IdDocument { get; set; }
    public IRemoteFile? IdDocumentBack { get; set; }
    public IRemoteFile? PrimaryDocument { get; set; } // أوليات الملف

    // Life Status
    public bool IsAlive { get; set; } = true;
    public DateOnly? DeathDate { get; set; }
    public IRemoteFile? DeathCertificate { get; set; }
}

public class CorrectRetireeLifeStatusDto
{
    [Required(ErrorMessage = "سبب التصحيح مطلوب")]
    [MinLength(10, ErrorMessage = "سبب التصحيح يجب ألا يقل عن 10 أحرف")]
    public string Reason { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "الحالة الصحية يجب أن تكون سليماً أو عاجزاً جزئياً أو عاجزاً كلياً")]
    public int HealthStatusID { get; set; }
}

public class UpdateRetirementFileDto
{
    public int? RankID { get; set; }
    public int? BranchID { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public DateOnly? ServiceStartDate { get; set; }
    public DateOnly? ServiceEndDate { get; set; }
    // FileStatusID removed — use PUT /{id}/status endpoint (ChangeStatusAsync) instead
    public string? Notes { get; set; }

    // Retiree fields
    public string? FullName { get; set; }
    public string? NationalID { get; set; }
    public byte? Gender { get; set; }
    public int? SocialStatusID { get; set; }
    public int? HealthStatusID { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? MilitaryNumber { get; set; }

    // Life Status
    public bool? IsAlive { get; set; }
    public DateOnly? DeathDate { get; set; }

    // ملفات المتقاعد للتحديث
    public IRemoteFile? PersonalPhoto { get; set; }
    public IRemoteFile? IdDocument { get; set; }
    public IRemoteFile? IdDocumentBack { get; set; }
    public IRemoteFile? PrimaryDocument { get; set; }
    public IRemoteFile? DeathCertificate { get; set; }

    // Phase 2.3: Optimistic Concurrency
    public byte[]? RowVersion { get; set; }
}

public class RetirementFileDto
{
    public string SalaryStatus { get; set; } = string.Empty;
    public int FileID { get; set; }
    public int RetireeID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string RetireeFullName { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public string? RankName { get; set; }
    public string? BranchName { get; set; }
    public string? MilitaryNumber { get; set; }
    public int? StatusID { get; set; }
    public int? PreviousStatusID { get; set; }
    public string? FileStatusName { get; set; }
    public string? PersonalPhotoPath { get; set; }
    public string? IdDocumentPath { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public DateOnly? ServiceStartDate { get; set; }
    public DateOnly? ServiceEndDate { get; set; }
    public DateTime FileOpenDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? StatusReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Assigned Employee
    public int? AssignedEmployeeID { get; set; }
    public string? AssignedEmployeeName { get; set; }

    // Active Payment Book
    public string? ActivePaymentBookNumber { get; set; }
    public DateOnly? ActivePaymentBookExpiry { get; set; }
    public string? ActivePaymentBookStatus { get; set; }

    // Retiree Additional Info
    public DateOnly? BirthDate { get; set; }
    public byte Gender { get; set; }
    public string? Address { get; set; }
    public DateOnly? DeathDate { get; set; }
    public bool IsAlive { get; set; } = true;

    // Statistics
    public int EntitledPersonsCount { get; set; }
    public int AgentsCount { get; set; }
    public int GuardiansCount { get; set; }
    public int MissingDocumentsCount { get; set; }
    public List<string> MissingDocumentsList { get; set; } = new();
    public int MissingDataCount { get; set; }  // عدد البيانات الناقصة (عنوان، هاتف، إلخ)
    public List<string> MissingDataList { get; set; } = new();

    // Contextual Flags (computed server-side to avoid extra API calls)
    public bool HasMinors { get; set; }
    public bool HasPledgesOrExemptions { get; set; }
    public bool HasSettlements { get; set; }

    // Review after modification
    public int ReviewStatus { get; set; }
    public DateTime? ReviewRequestedAt { get; set; }
    public DateTime? ReviewCompletedAt { get; set; }
    public string? ReviewNotes { get; set; }
}

public class RetirementFileListDto
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string RetireeFullName { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? RankName { get; set; }
    public int? StatusID { get; set; }
    public int? PreviousStatusID { get; set; }
    public string? FileStatusName { get; set; }
    public DateTime? FileOpenDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActivePaymentBookNumber { get; set; }
    public int MissingDocumentsCount { get; set; }
    public bool HasDocuments { get; set; }
    public int? LastUploadedByUserID { get; set; }
    public string? LastUploadedByUserName { get; set; }
    public DateTime? LastUploadDate { get; set; }
    public int ReviewStatus { get; set; }
    public DateTime? ReviewRequestedAt { get; set; }
}

public class RetirementFileReviewDto
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string RetireeFullName { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public int? StatusID { get; set; }
    public int? PreviousStatusID { get; set; }
    public string? FileStatusName { get; set; }
    public DateTime FileOpenDate { get; set; }
    public int DocumentsCount { get; set; }
    public int MissingDocumentsCount { get; set; }
    public int MissingDataCount { get; set; }
    public bool HasRetireeInfo { get; set; }
    public bool HasAllRequiredData { get; set; }
    public int ReviewStatus { get; set; }
    public DateTime? ReviewRequestedAt { get; set; }
    public string? ReviewRequestedByUserName { get; set; }
}

public class FileReviewDiffDto
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string RetireeFullName { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public DateTime ReviewRequestedAt { get; set; }
    public string? ReviewRequestedByUserName { get; set; }
    public List<FileReviewSectionDto> Sections { get; set; } = new();
}

public class FileReviewSectionDto
{
    public string SectionName { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty; // File, Agent, Guardian, EntitledPerson, PaymentBook, etc.
    public string? EntityDisplayName { get; set; }
    public List<FieldChangeDto> FieldChanges { get; set; } = new();
}

public class DocumentUploadStatsDto
{
    public int TotalFiles { get; set; }
    public int FilesWithDocuments { get; set; }
    public int FilesWithoutDocuments { get; set; }
    public int FilesWithMissing { get; set; }
    public int TotalMissingDocuments { get; set; }
}

public class UploaderStatsDto
{
    public int UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int TotalUploads { get; set; }
    public int UniqueFiles { get; set; }
    public DateTime? LastUploadDate { get; set; }
}

public class UploaderStatsFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Period { get; set; } // "today", "week", "month", "year", "custom"
}

public class DataEntryStatsDto
{
    public int UserID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int TotalFilesCreated { get; set; }
    public int CompletedFiles { get; set; }
    public int PendingFiles { get; set; }
    public int RejectedFiles { get; set; }
    public DateTime? LastCreatedDate { get; set; }
}

public class RetirementFileDocumentUploadDto
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string RetireeFullName { get; set; } = string.Empty;
    public string? NationalID { get; set; }
    public int? StatusID { get; set; }
    public string? FileStatusName { get; set; }
    public int DocumentsCount { get; set; }
    public int MissingDocumentsCount { get; set; }
    public bool IsIncomplete { get; set; }
}
