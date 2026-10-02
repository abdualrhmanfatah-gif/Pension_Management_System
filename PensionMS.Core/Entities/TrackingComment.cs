using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities;

/// <summary>
/// تعليقات على سجل الإنجاز/المعوق
/// </summary>
public class TrackingComment : ISoftDeletable
{
    public int ID { get; set; }

    public int TrackingRecordId { get; set; }
    public virtual AchievementObstacle Record { get; set; } = null!;

    public string Comment { get; set; } = string.Empty;

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
