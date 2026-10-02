namespace PensionMS.Core.Entities.Assets;

public enum AssetType
{
    IT = 0,        // أجهزة وتقنية
    Vehicle = 1,   // مركبات
    Building = 2,  // مباني
    Land = 3,      // أراضي
    Furniture = 4  // أثاث ومفروشات
}

public enum AssetStatus
{
    Active = 0,        // نشط
    Inactive = 1,      // متوقف
    Disposed = 2,      // متخلص
    Maintenance = 3    // تحت الصيانة
}

public enum AssetValuationMethod
{
    PurchasePrice = 0,       // سعر الشراء (جديد)
    FairValue = 1,           // القيمة العادلة (تقييم خبير)
    EstimatedValue = 2,      // القيمة المقدرة
    ReplacementCost = 3      // تكلفة الإعادة (سعر السوق)
}
