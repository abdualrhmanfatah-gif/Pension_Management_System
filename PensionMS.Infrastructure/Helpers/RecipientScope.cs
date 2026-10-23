using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;

namespace PensionMS.Infrastructure.Helpers;

internal static class RecipientScope
{
    public static int? EntitledId(SalaryRecipient recipient) =>
        recipient.AgentID.HasValue ? recipient.Agent?.EntitledID : recipient.EntitledID;

    public static IQueryable<SalaryRecipient> Query(IUnitOfWork unit, int fileId, int? entitledId) =>
        unit.SalaryRecipients.Query().Where(r => r.FileID == fileId &&
            (r.AgentID.HasValue ? r.Agent!.EntitledID : r.EntitledID) == entitledId);

    public static void End(SalaryRecipient recipient, DateTime at, int? userId = null)
    {
        recipient.IsActive = false;
        recipient.EndDate = at;
        recipient.UpdatedAt = DateTime.UtcNow;
        recipient.UpdatedByUserID = userId;
    }

    public static async Task EndForEntitledAsync(IUnitOfWork unit, int entitledId, int? userId = null)
    {
        // Explicit agent IDs also cover stale recipients whose agency was already deleted.
        var agentIds = unit.Agents.Query().IgnoreQueryFilters()
            .Where(a => a.EntitledID == entitledId).Select(a => a.AgentID);
        var recipients = await unit.SalaryRecipients.Query()
            .Where(r => r.IsActive && (r.EntitledID == entitledId ||
                (r.AgentID.HasValue && agentIds.Contains(r.AgentID.Value))))
            .ToListAsync();
        foreach (var recipient in recipients) End(recipient, DateTime.UtcNow, userId);
    }

    public static bool Reviewed(PendingReviewStatus status) =>
        status != PendingReviewStatus.Pending && status != PendingReviewStatus.Rejected;

    public static bool AgentPayable(Agent agent, DateOnly onDate) =>
        agent.IsActive && !agent.IsDeleted && Reviewed(agent.PendingReviewStatus) &&
        !string.IsNullOrWhiteSpace(agent.AgencyNumber) && agent.AgencyDate.HasValue &&
        CalendarDate(agent.AgencyDate.Value) <= onDate &&
        (!agent.AgencyEndDate.HasValue || CalendarDate(agent.AgencyEndDate.Value) >= onDate);

    public static DateOnly CalendarDate(DateTime value) => value.Kind == DateTimeKind.Unspecified
        ? DateOnly.FromDateTime(value)
        : YemenTime.LocalDate(new DateTimeOffset(value));
}
