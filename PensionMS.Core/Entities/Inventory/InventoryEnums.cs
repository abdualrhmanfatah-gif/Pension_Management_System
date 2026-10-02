namespace PensionMS.Core.Entities.Inventory;

public enum ItemCondition
{
    New = 0,
    Used = 1
}

public enum ItemStatus
{
    Active = 0,
    Inactive = 1,
    UnderMaintenance = 2,
    Disposed = 3
}

public enum CustodyStatus
{
    Active = 0,
    Returned = 1,
    Damaged = 2,
    Lost = 3
}

public enum TransactionType
{
    StockIn = 0,
    StockOut = 1,
    Transfer = 2,
    Disposal = 3,
    Adjustment = 4
}
