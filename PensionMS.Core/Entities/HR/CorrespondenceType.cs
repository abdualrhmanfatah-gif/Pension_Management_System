using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

/// <summary>
/// جدول المرجعية لأنواع المراسلات (صادر، وارد، أمر إداري، إلخ)
/// </summary>
public class CorrespondenceType : ISoftDeletable
{
    public int CorrespondenceTypeID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
}
