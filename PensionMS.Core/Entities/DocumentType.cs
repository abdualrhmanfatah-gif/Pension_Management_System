namespace PensionMS.Core.Entities;

/// <summary>
/// أنواع الوثائق
/// </summary>
public class DocumentType
{
    public int DocumentTypeID { get; set; }

    public int? OwnerTableID { get; set; }
    public virtual SystemTable? OwnerTable { get; set; }
    public DocumentOwnerType? OwnerType { get; set; }

    public string TypeName { get; set; } = string.Empty;
    public string SystemCode { get; set; } = string.Empty;
    public string? TypeNameEn { get; set; }
    public bool IsRequired { get; set; }
    public bool HasExpiry { get; set; }
    public int? MaxFileSize { get; set; }
    public string? AllowedExtensions { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<MissingDocument> MissingDocuments { get; set; } = new List<MissingDocument>();
}
