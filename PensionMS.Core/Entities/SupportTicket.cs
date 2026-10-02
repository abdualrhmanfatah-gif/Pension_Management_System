using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// تذاكر الدعم (SupportTicket)
/// </summary>
public class SupportTicket : ISoftDeletable
{
    public int SupportTicketID { get; set; }
    
    public int CitizenAccountID { get; set; }
    public virtual CitizenAccount CitizenAccount { get; set; } = null!;
    
    public int? AssignedToUserID { get; set; }
    public virtual User? AssignedToUser { get; set; }
    
    public string TicketNumber { get; set; } = string.Empty;  // T-2026-00001
    public string Category { get; set; } = string.Empty;       // شكوى | استفسار | اقتراح
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "مفتوح";             // مفتوح | قيد المعالجة | مغلق
    public string Priority { get; set; } = "عادي";            // عاجل | عادي | منخفض
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }

    public virtual ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}
