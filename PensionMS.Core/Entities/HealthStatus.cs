namespace PensionMS.Core.Entities;

/// <summary>
/// الحالة الصحية
/// </summary>
public class HealthStatus
{
    public int HealthStatusID { get; set; }

    public string HealthStatusName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Retiree> Retirees { get; set; } = new List<Retiree>();
    public virtual ICollection<EntitledPerson> EntitledPersons { get; set; } = new List<EntitledPerson>();
}
