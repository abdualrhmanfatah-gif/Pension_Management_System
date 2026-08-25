using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Entities.Inventory;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Assets;

public class FixedAsset : ISoftDeletable
{
    public int AssetID { get; set; }
    public string AssetNumber { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryID { get; set; }
    public virtual FixedAssetCategory Category { get; set; } = null!;
    public int AssetTypeID { get; set; }
    public virtual FixedAssetType AssetType { get; set; } = null!;

    // ═══ معلومات الشراء والقيمة ═══
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime InServiceDate { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AcquiredCost { get; set; }
    public decimal SalvageValue { get; set; }
    public string CurrencyCode { get; set; } = "YER";
    public int Quantity { get; set; } = 1;
    public AssetValuationMethod ValuationMethod { get; set; } = AssetValuationMethod.PurchasePrice;
    public string? ValuationNotes { get; set; }

    // ═══ معلومات الهلاك ═══
    public decimal DepreciationRate { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    public DateTime? LastDepreciationDate { get; set; }

    // ═══ الموقع والعميل ═══
    public string? Location { get; set; }
    public int? CustodianEmployeeID { get; set; }
    public virtual Employee? CustodianEmployee { get; set; }

    // ═══ الحسابات المحاسبية ═══
    public int AssetAccountID { get; set; }
    public virtual ChartOfAccount AssetAccount { get; set; } = null!;
    public int AccumDeprAccountID { get; set; }
    public virtual ChartOfAccount AccumDeprAccount { get; set; } = null!;
    public int DeprExpenseAccountID { get; set; }
    public virtual ChartOfAccount DeprExpenseAccount { get; set; } = null!;

    // ═══ الحالة ═══
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public DateTime? DisposalDate { get; set; }
    public string? DisposalReason { get; set; }

    // ═══ المراجعات ═══
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserID { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public byte[]? RowVersion { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    // ═══ التفاصيل (One-to-One) ═══
    public virtual AssetITDetail? ITDetail { get; set; }
    public virtual AssetVehicleDetail? VehicleDetail { get; set; }
    public virtual AssetBuildingDetail? BuildingDetail { get; set; }
    public virtual AssetLandDetail? LandDetail { get; set; }
    public virtual AssetFurnitureDetail? FurnitureDetail { get; set; }

    // ═══ الهلاك ═══
    public virtual ICollection<DepreciationSchedule> DepreciationSchedules { get; set; } = new List<DepreciationSchedule>();

    // ═══ العهدة ═══
    public virtual ICollection<InventoryCustody> Custodies { get; set; } = new List<InventoryCustody>();
}
