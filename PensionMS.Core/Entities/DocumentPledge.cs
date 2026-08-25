using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// التعهدات بإحضار الوثائق
/// </summary>
public class DocumentPledge : ISoftDeletable
{
    public int PledgeID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int? SuspensionID { get; set; }
    public virtual PensionSuspension? Suspension { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public int? ExtendedByUserID { get; set; }
    public virtual User? ExtendedByUser { get; set; }

    public string? PledgeNumber { get; set; }
    public DateTime PledgeDate { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public string PledgedByType { get; set; } = string.Empty;
    public string? PledgedByName { get; set; }
    public DateTime? OriginalDeadline { get; set; }
    public bool IsExtended { get; set; }
    public DateTime? ExtendedAt { get; set; }
    public string? ExtensionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<MissingDocument> MissingDocuments { get; set; } = new List<MissingDocument>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
