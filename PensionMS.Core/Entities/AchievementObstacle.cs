using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities.HR;

namespace PensionMS.Core.Entities;

/// <summary>
/// سجلات الإنجازات والمعوقات
/// </summary>
public class AchievementObstacle : ISoftDeletable
{
    public int ID { get; set; }

    public string Number { get; set; } = string.Empty;

    public AchievementObstacleType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TrackingStatus Status { get; set; } = TrackingStatus.New;

    public int OwnerUserID { get; set; }
    public virtual User OwnerUser { get; set; } = null!;

    public int? DepartmentID { get; set; }
    public virtual Department? Department { get; set; }

    public int? EmployeeID { get; set; }
    public virtual Employee? Employee { get; set; }

    public int CreatedByUserID { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public virtual ICollection<TrackingComment> Comments { get; set; } = new List<TrackingComment>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
