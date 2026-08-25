using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.HR;

/// <summary>
/// الجهات الخارجية (بنوك، وزارات، إدارات أمنية، إلخ)
/// </summary>
public class ExternalParty : ISoftDeletable
{
    public int ExternalPartyID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
}
