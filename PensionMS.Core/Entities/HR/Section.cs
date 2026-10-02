using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

public class Section : ISoftDeletable
{
    public int SectionID { get; set; }
    public int DepartmentID { get; set; }
    public virtual Department Department { get; set; } = null!;

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

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
