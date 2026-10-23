namespace PensionMS.Core.Interfaces;

/// <summary>عملية نظام محددة للتعهدات المنتهية، متاحة للمهمة المجدولة فقط.</summary>
public interface IExpiredPledgeProcessor
{
    Task ProcessExpiredPledgesAsync();
}
