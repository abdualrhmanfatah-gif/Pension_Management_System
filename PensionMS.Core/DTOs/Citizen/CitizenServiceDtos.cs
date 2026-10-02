namespace PensionMS.Core.DTOs.Citizen;

public class CitizenDocumentDto
{
    public int DocumentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

public class CitizenMissingDocumentDto
{
    public int MissingDocumentID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CitizenDocumentPledgeDto
{
    public int PledgeID { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateServiceRequestDto
{
    public string RequestType { get; set; } = string.Empty;
    public string? CitizenNotes { get; set; }
    public string? RequestDataJson { get; set; }
}

public class ServiceRequestDto
{
    public int RequestID { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CitizenNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
