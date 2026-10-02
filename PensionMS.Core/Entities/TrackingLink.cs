using PensionMS.Core.Entities.HR;

namespace PensionMS.Core.Entities;

/// <summary>
/// ربط سجل الإنجاز/المعوق بكيانات متعددة
/// </summary>
public class TrackingLink
{
    public int ID { get; set; }

    public int TrackingRecordId { get; set; }
    public virtual AchievementObstacle Record { get; set; } = null!;

    public TrackingEntityType EntityType { get; set; }

    public int? DepartmentID { get; set; }
    public virtual Department? Department { get; set; }

    public int? EmployeeID { get; set; }
    public virtual Employee? Employee { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
