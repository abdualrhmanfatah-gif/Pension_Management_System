using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// الوثائق الناقصة
/// </summary>
public class MissingDocument : ISoftDeletable
{
    public int MissingDocumentID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int DocumentTypeID { get; set; }
    public virtual DocumentType DocumentType { get; set; } = null!;

    public int? PledgeID { get; set; }
    public virtual DocumentPledge? Pledge { get; set; }

    public int? ExemptionID { get; set; }
    public virtual DocumentExemption? Exemption { get; set; }

    public int? AgentID { get; set; }
    public virtual Agent? Agent { get; set; }

    public int? GuardianID { get; set; }
    public virtual Guardian? Guardian { get; set; }

    public int? EntitledID { get; set; }
    public virtual EntitledPerson? EntitledPerson { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }
}
