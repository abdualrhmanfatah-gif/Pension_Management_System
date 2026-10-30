using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
using PensionMS.Core.Interfaces;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

namespace PensionMS.Core.DTOs.Documents;

// UploadDocumentDto moved to UploadDto.cs

public class UpdateDocumentDto
{
    [Required]
    public int DocumentTypeID { get; set; }

<<<<<<< HEAD
    // ملف جديد بديل (اختياري) — الحقول القياسية تُعامل كتحديث جزئي: null يعني عدم الإرسال
    public IRemoteFile? File { get; set; }

    public string? Notes { get; set; }
    public string? DocumentNumber { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    // مالك الوثيقة (واحد فقط مطلوب) — لا تُغيَّر إلا عند إرسال أحدها
=======
    public string? Notes { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // مالك الوثيقة (واحد فقط مطلوب)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public int? RetirementFileID { get; set; }
    public int? RetireeID { get; set; }
    public int? EntitledPersonID { get; set; }
    public int? AgentID { get; set; }
    public int? GuardianID { get; set; }

}

public class ReviewDocumentDto
{
    [Required]
    public int PendingReviewStatus { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}

public class DocumentDto
{
    public int DocumentID { get; set; }
    public int DocumentTypeID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileExtension { get; set; }
    public long? FileSize { get; set; }
    public string? Notes { get; set; }
    public string? UploadedByUser { get; set; }
    public string? EntityName { get; set; }
    public string? EntityType { get; set; }
    public DateTime CreatedAt { get; set; }

    // Document lifecycle
<<<<<<< HEAD
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
=======
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? DocumentNumber { get; set; }
    public bool Verified { get; set; }
    public bool HasExpiry { get; set; }
    public bool IsRequired { get; set; }
<<<<<<< HEAD
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < PensionMS.Core.Time.YemenTime.Today;
=======
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    // Owner IDs for filtering
    public int? AgentID { get; set; }
    public int? GuardianID { get; set; }
    public int? RetireeID { get; set; }
    public int? EntitledID { get; set; }

    // Investment Module
    public int? InvestmentProjectID { get; set; }
    public int? InvestmentPropertyID { get; set; }
    public int? PropertyUnitID { get; set; }
    public int? LeaseContractID { get; set; }
    public int? TenantID { get; set; }
    public int? InvestmentPortfolioID { get; set; }
    public int? MaintenanceRequestID { get; set; }
    public int? WorkOrderID { get; set; }
    public string? PropertyUnitNumber { get; set; }
    public int? AssetValuationID { get; set; }
    public int? DepreciationRecordID { get; set; }
    public int? PaymentScheduleID { get; set; }

    // Operational Budget Module
    public int? OperationalExpenseID { get; set; }

    // Project Documents
    public int? InvestmentPlanID { get; set; }
    public int? InvestmentTxnID { get; set; }
    public int? RiskAssessmentID { get; set; }

    // HR Module
    public int? EmployeeID { get; set; }
    public int? CorrespondenceID { get; set; }
    public int PendingReviewStatus { get; set; }
    public int? FileID { get; set; }
    public string? FileNumber { get; set; }
    public string? ReviewNotes { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Archive
    public int OwnerType { get; set; }
    public string? MimeType { get; set; }
}

public class ArchiveStatsDto
{
    public int TotalDocuments { get; set; }
    public int VerifiedDocuments { get; set; }
    public int PendingReview { get; set; }
    public int ExpiredDocuments { get; set; }

    // الملفات ذات الوثائق الناقصة
    public int FilesWithMissingDocuments { get; set; }
    public int TotalMissingDocuments { get; set; }
    public int PendingMissingDocuments { get; set; }
    public int PledgedMissingDocuments { get; set; }
}
