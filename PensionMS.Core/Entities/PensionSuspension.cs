using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// إيقاف المعاش
/// </summary>
public class PensionSuspension : ISoftDeletable
{
    public int SuspensionID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public int? EndedByUserID { get; set; }
    public virtual User? EndedByUser { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public DateTime SuspensionDate { get; set; }
    public string SuspensionReason { get; set; } = string.Empty;
    public string SuspensionSource { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public string? EndReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<DocumentPledge> DocumentPledges { get; set; } = new List<DocumentPledge>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
