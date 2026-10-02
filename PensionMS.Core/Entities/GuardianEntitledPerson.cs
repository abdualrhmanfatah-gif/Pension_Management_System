using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// جدول وسيط لربط الوصي بأكثر من قاصر (Many-to-Many)
/// </summary>
public class GuardianEntitledPerson : ISoftDeletable
{
    public int GuardianID { get; set; }
    public virtual Guardian Guardian { get; set; } = null!;

    public int EntitledID { get; set; }
    public virtual EntitledPerson EntitledPerson { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
