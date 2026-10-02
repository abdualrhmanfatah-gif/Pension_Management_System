using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.Documents;

public class DocumentTypeDto
{
    public int DocumentTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? TypeNameEn { get; set; }
    public string SystemCode { get; set; } = string.Empty;
    public int? OwnerType { get; set; }
    public bool IsRequired { get; set; }
    public bool HasExpiry { get; set; }
    public int? MaxFileSize { get; set; }
    public string? AllowedExtensions { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDocumentTypeDto
{
    [Required(ErrorMessage = "اسم النوع مطلوب")]
    public string TypeName { get; set; } = string.Empty;

    public string? TypeNameEn { get; set; }
    public string? SystemCode { get; set; }
    public int? OwnerType { get; set; }
    public bool IsRequired { get; set; }
    public bool HasExpiry { get; set; }
    public int? MaxFileSize { get; set; } // in MB
    public string? AllowedExtensions { get; set; } // e.g. ".pdf,.jpg"
}

public class UpdateDocumentTypeDto
{
    public string? TypeName { get; set; }
    public string? TypeNameEn { get; set; }
    public string? SystemCode { get; set; }
    public int? OwnerType { get; set; }
    public bool? IsRequired { get; set; }
    public bool? HasExpiry { get; set; }
    public int? MaxFileSize { get; set; }
    public string? AllowedExtensions { get; set; }
    public bool? IsActive { get; set; }
}
