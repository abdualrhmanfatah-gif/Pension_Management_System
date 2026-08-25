namespace PensionMS.Core.Entities;

/// <summary>
/// الحالة الاجتماعية
/// </summary>
public class SocialStatus
{
    public int SocialStatusID { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Retiree> Retirees { get; set; } = new List<Retiree>();
    public virtual ICollection<EntitledPerson> EntitledPersons { get; set; } = new List<EntitledPerson>();
}
