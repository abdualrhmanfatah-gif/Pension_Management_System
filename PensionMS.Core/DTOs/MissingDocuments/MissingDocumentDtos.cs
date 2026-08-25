namespace PensionMS.Core.DTOs.MissingDocuments;

public class CreateMissingDocumentDto
{
    public int FileID { get; set; }
    public int DocumentTypeID { get; set; }
    public string? Notes { get; set; }
    
    // Entity References for context
    public int? AgentID { get; set; }
    public int? GuardianID { get; set; }
    public int? EntitledID { get; set; }
}

public class MissingDocumentDto
{
    public int MissingDocumentID { get; set; }
    public int FileID { get; set; }
    public int DocumentTypeID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;       // Pending | Pledged | Exempted | Fulfilled
    public int? PledgeID { get; set; }
    public int? ExemptionID { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Entity References for clearer UI context
    public int? AgentID { get; set; }
    public string? AgentName { get; set; }
    public int? GuardianID { get; set; }
    public string? GuardianName { get; set; }
    public int? EntitledID { get; set; }
    public string? EntitledName { get; set; }
}

// DTO للوثائق الناقصة المكتشفة تلقائياً
public class AutoDetectedMissingDocumentDto
{
    public int DocumentTypeID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsAutoDetected { get; set; } = true;
    
    /// <summary>
    /// نوع الكيان المرتبط: Agent | Guardian | EntitledPerson | Retiree | Pledge | Exemption
    /// </summary>
    public string? EntityType { get; set; }
    
    /// <summary>
    /// اسم الكيان المرتبط (اسم الوكيل، رقم التعهد، إلخ)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// معرفات الكيان المرتبط لربط الوثيقة بالمالك الصحيح
    /// </summary>
    public int? RetireeID { get; set; }
    public int? AgentID { get; set; }
    public int? GuardianID { get; set; }
    public int? EntitledID { get; set; }
    public int? PledgeID { get; set; }
    public int? ExemptionID { get; set; }
}