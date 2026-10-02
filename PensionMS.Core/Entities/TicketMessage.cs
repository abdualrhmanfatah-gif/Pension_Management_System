using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// رسائل التذاكر (TicketMessage)
/// </summary>
public class TicketMessage : ISoftDeletable
{
    public int TicketMessageID { get; set; }
    
    public int TicketID { get; set; }
    public virtual SupportTicket Ticket { get; set; } = null!;
    
    public string SenderType { get; set; } = string.Empty;  // CITIZEN | STAFF
    public int? SenderUserID { get; set; }                   // null if citizen
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
    public virtual User? DeletedByUser { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
