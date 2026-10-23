using System.Linq.Expressions;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Constants;

/// <summary>تواريخ الاستحقاق تقويمية؛ يوم البداية والنهاية مشمولان في السريان.</summary>
public static class PensionEntitlementRules
{
    public static Expression<Func<FinancialEntitlement, bool>> EffectiveOn(DateOnly onDate) =>
        EffectiveDuring(onDate, onDate);

    public static Expression<Func<FinancialEntitlement, bool>> EffectiveDuring(DateOnly fromDate, DateOnly throughDate)
    {
        var from = fromDate.ToDateTime(TimeOnly.MinValue);
        var through = throughDate.ToDateTime(TimeOnly.MinValue);
        return e => !e.IsDeleted && e.EffectiveDate.Date <= through &&
            (!e.EndDate.HasValue || e.EndDate.Value.Date >= from);
    }

    public static Expression<Func<FinancialEntitlement, bool>> NotEndedOn(DateOnly onDate)
    {
        var day = onDate.ToDateTime(TimeOnly.MinValue);
        return e => !e.IsDeleted && (!e.EndDate.HasValue || e.EndDate.Value.Date >= day);
    }

    public static bool AllEnded(IEnumerable<FinancialEntitlement> entitlements, DateOnly onDate)
    {
        var current = entitlements.Where(e => !e.IsDeleted).ToList();
        return current.Count > 0 && !current.Any(NotEndedOn(onDate).Compile());
    }
}
