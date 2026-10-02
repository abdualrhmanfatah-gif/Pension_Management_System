using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// إعفاءات الوثائق
/// </summary>
public class DocumentExemption : ISoftDeletable
{
    public int ExemptionID { get; set; }

    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public string DirectiveSource { get; set; } = string.Empty;
    public DateTime ExemptionDate { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }

    public virtual ICollection<MissingDocument> MissingDocuments { get; set; } = new List<MissingDocument>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
