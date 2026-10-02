using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

public class Department : ISoftDeletable
{
    public int DepartmentID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<AchievementObstacle> AchievementObstacles { get; set; } = new List<AchievementObstacle>();
}
