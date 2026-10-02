using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// الأوصياء
/// </summary>
public class Guardian : ISoftDeletable
{
    public int GuardianID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? PersonalPhotoPath { get; set; }
    public string? Phone { get; set; }
    public string GuardianshipDocNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public PendingReviewStatus PendingReviewStatus { get; set; } = PendingReviewStatus.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<GuardianEntitledPerson> GuardianEntitledPersons { get; set; } = new List<GuardianEntitledPerson>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<SalaryRecipient> SalaryRecipients { get; set; } = new List<SalaryRecipient>();
}
