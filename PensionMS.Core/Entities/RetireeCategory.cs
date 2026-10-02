namespace PensionMS.Core.Entities;

/// <summary>
/// فئات المتقاعدين
/// </summary>
public class RetireeCategory
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MinistryApprovalItem> MinistryApprovalItems { get; set; } = new List<MinistryApprovalItem>();
}
