using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// طلبات الخدمة (ServiceRequest)
/// </summary>
public class ServiceRequest : ISoftDeletable
{
    [System.ComponentModel.DataAnnotations.Key]
    public int RequestID { get; set; }
    
    public int CitizenAccountID { get; set; }
    public virtual CitizenAccount CitizenAccount { get; set; } = null!;
    
    public int FileID { get; set; }
    public virtual RetirementFile RetirementFile { get; set; } = null!;
    
    public int? ProcessedByUserID { get; set; }        // الموظف الذي عالج الطلب
    public virtual User? ProcessedByUser { get; set; }
    
    // أنواع الطلبات:
    // BOOK_RENEWAL | AGENT_CHANGE | REPORT_DEATH | REPORT_MARRIAGE
    // DATA_UPDATE | DOCUMENT_SUBMIT | GENERAL_INQUIRY
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = "جديد";   // جديد | قيد المراجعة | مقبول | مرفوض
    public string? Description { get; set; }
    public string? AdminNotes { get; set; }          // ملاحظات الموظف (لا تظهر للمتقاعد)
    public string? CitizenNotes { get; set; }        // ملاحظات المتقاعد
    public string? RequestDataJson { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
