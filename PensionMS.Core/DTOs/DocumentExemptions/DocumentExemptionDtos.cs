namespace PensionMS.Core.DTOs.DocumentExemptions;

public class CreateDocumentExemptionDto
{
    public int FileID { get; set; }
    public List<int> MissingDocumentIDs { get; set; } = new();
    public string DirectiveSource { get; set; } = string.Empty;   // مصدر التوجيه
    public DateTime ExemptionDate { get; set; }
    public string? Notes { get; set; }
}

public class MissingDocumentInfo
{
    public int MissingDocumentID { get; set; }
    public int DocumentTypeID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
}

public class DocumentExemptionDto
{
    public int ExemptionID { get; set; }
    public int FileID { get; set; }
    public List<MissingDocumentInfo> MissingDocuments { get; set; } = new();
    public string DirectiveSource { get; set; } = string.Empty;
    public DateTime ExemptionDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateDocumentExemptionDto
{
    public string DirectiveSource { get; set; } = string.Empty;
    public DateTime ExemptionDate { get; set; }
    public string? Notes { get; set; }
}