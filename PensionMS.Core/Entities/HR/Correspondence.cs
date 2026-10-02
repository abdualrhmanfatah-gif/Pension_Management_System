using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

/// <summary>
/// سجل الصادر والوارد الموحد
/// </summary>
public class Correspondence : ISoftDeletable
{
    public int CorrespondenceID { get; set; }

    /// <summary>
    /// الاتجاه: 0 = صادر (Outgoing)، 1 = وارد (Incoming)
    /// </summary>
    public byte Direction { get; set; }

    public int CorrespondenceTypeID { get; set; }
    public virtual CorrespondenceType CorrespondenceType { get; set; } = null!;

    /// <summary>
    /// الرقم المتسلسل اليدوي
    /// </summary>
    public int SequentialNumber { get; set; }

    /// <summary>
    /// رقم المرجعي (القيد) مثل "07/03/25"
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    public DateTime CorrespondenceDate { get; set; }
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// رقم الصفحة "ص" (اختياري)
    /// </summary>
    public string? PageNumber { get; set; }

    public int? ExternalPartyID { get; set; }
    public virtual ExternalParty? ExternalParty { get; set; }

    public int? EmployeeID { get; set; }
    public virtual Employee? Employee { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
