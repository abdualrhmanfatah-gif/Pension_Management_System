using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// ملفات التقاعد
/// </summary>
public class RetirementFile : ISoftDeletable
{
    public int FileID { get; set; }

    public int RetireeID { get; set; }
    public virtual Retiree Retiree { get; set; } = null!;

    public int PensionBranchID { get; set; }
    public virtual PensionFundBranch PensionBranch { get; set; } = null!;

    public int? StatusID { get; set; }
    public virtual FileStatus? Status { get; set; }

    public int? PreviousStatusID { get; set; }

    public int? RankID { get; set; }
    public virtual Rank? Rank { get; set; }

    public int? AssignedEmployeeID { get; set; }
    public virtual User? AssignedEmployee { get; set; }

    public string FileNumber { get; set; } = string.Empty;
    public int MissingDocumentsCount { get; set; }
    public string? StatusReason { get; set; }
    public DateTime FileOpenDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    
<<<<<<< HEAD
    public DateOnly? ServiceStartDate { get; set; }
    public DateOnly? ServiceEndDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
=======
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    public DateTime? RetirementDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    public string? Notes { get; set; }

    // File review after modification
    public FileReviewStatus ReviewStatus { get; set; } = FileReviewStatus.None;
    public DateTime? ReviewRequestedAt { get; set; }
    public DateTime? ReviewCompletedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public string? LastReviewedFileState { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<EntitledPerson> EntitledPersons { get; set; } = new List<EntitledPerson>();
    public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();
    public virtual ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<FileNote> FileNotes { get; set; } = new List<FileNote>();
    public virtual ICollection<PensionSuspension> PensionSuspensions { get; set; } = new List<PensionSuspension>();
    public virtual ICollection<SalaryPaymentItem> SalaryPaymentItems { get; set; } = new List<SalaryPaymentItem>();
    public virtual ICollection<SalaryRecipient> SalaryRecipients { get; set; } = new List<SalaryRecipient>();
    public virtual ICollection<MissingDocument> MissingDocuments { get; set; } = new List<MissingDocument>();
    public virtual ICollection<DocumentExemption> DocumentExemptions { get; set; } = new List<DocumentExemption>();
    public virtual ICollection<DocumentPledge> DocumentPledges { get; set; } = new List<DocumentPledge>();
    public virtual ICollection<PaymentBook> PaymentBooks { get; set; } = new List<PaymentBook>();
}
