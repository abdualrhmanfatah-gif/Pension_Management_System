using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// الوكلاء
/// </summary>
public class Agent : ISoftDeletable
{
    public int AgentID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    /// <summary>
    /// المستحق المرتبط — NULL = وكيل عام للملف
    /// </summary>
    public int? EntitledID { get; set; }
    public virtual EntitledPerson? EntitledPerson { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? PersonalPhotoPath { get; set; }
    public string? Phone { get; set; }
    public string? AgencyNumber { get; set; }
    public DateTime? AgencyDate { get; set; }
    public DateTime? AgencyEndDate { get; set; }
    public string? Notes { get; set; }
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

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<SalaryRecipient> SalaryRecipients { get; set; } = new List<SalaryRecipient>();
}
