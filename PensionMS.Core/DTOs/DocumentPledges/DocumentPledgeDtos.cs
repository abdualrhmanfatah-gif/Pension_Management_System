namespace PensionMS.Core.DTOs.DocumentPledges;

public class CreateDocumentPledgeDto
{
    public int FileID { get; set; }
    public List<int> MissingDocumentIDs { get; set; } = new();
    public DateTime Deadline { get; set; }
    public string? PledgedByName { get; set; }
    public string? PledgedByType { get; set; }   // ذاتي | وكيل | ولي أمر
    public string? Reason { get; set; }
<<<<<<< HEAD

    /// <summary>معرّف الإيقاف عند الإنشاء من مسار إيقاف المعاش؛ يُترك فارغًا في المسار المتخصص.</summary>
    public int? SuspensionID { get; set; }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}

public class MissingDocumentInfo
{
    public int MissingDocumentID { get; set; }
    public int DocumentTypeID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public int? AgentID { get; set; }
    public int? GuardianID { get; set; }
    public int? EntitledID { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DocumentPledgeDto
{
    public int PledgeID { get; set; }
    public int FileID { get; set; }
    public string? RetireeName { get; set; }
    public List<MissingDocumentInfo> MissingDocuments { get; set; } = new();
    public string? PledgeNumber { get; set; }
    public DateTime PledgeDate { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = string.Empty;     // Active | Fulfilled | Expired
    public string? PledgedByName { get; set; }
    public string? PledgedByType { get; set; }
    public string? Reason { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateDocumentPledgeDto
{
    public DateTime Deadline { get; set; }
    public string? PledgedByName { get; set; }
    public string? PledgedByType { get; set; }
    public string? Reason { get; set; }
}