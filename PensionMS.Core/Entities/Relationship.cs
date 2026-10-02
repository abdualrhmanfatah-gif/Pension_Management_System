namespace PensionMS.Core.Entities;

/// <summary>
/// صلات القرابة
/// </summary>
public class Relationship
{
    public int RelationshipID { get; set; }

    public string RelationshipName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<EntitledPerson> EntitledPersons { get; set; } = new List<EntitledPerson>();
}
