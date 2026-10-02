using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.Financial;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Assets;

public class AssetStocktake : ISoftDeletable
{
    public int AssetStocktakeID { get; set; }
    public int FiscalYearID { get; set; }
    public DateTime StocktakeDate { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Verified
    public int CreatedByUserID { get; set; }
    public int? VerifiedByUserID { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual FiscalYear FiscalYear { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? VerifiedByUser { get; set; }
    public virtual ICollection<AssetStocktakeLine> Lines { get; set; } = new List<AssetStocktakeLine>();

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }
}
