using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// خدمة خلفية تفحص التعهدات المنتهية المدة كل 6 ساعات
/// Item 10 — PledgeExpiryChecker
/// </summary>
public class PledgeExpiryCheckerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PledgeExpiryCheckerService> _logger;
    private static readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public PledgeExpiryCheckerService(
        IServiceScopeFactory scopeFactory,
        ILogger<PledgeExpiryCheckerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PledgeExpiryCheckerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiredPledgesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking pledge expiry.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckExpiredPledgesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensionDbContext>();

        var now = DateTime.UtcNow;

        // Find active pledges whose deadline has passed
        var expiredPledges = await db.DocumentPledges
            .Where(p => p.Status == PledgeStatus.Pending
                     && p.Deadline < now
                     && !p.IsDeleted)
            .Include(p => p.RetirementFile)
            .ToListAsync(ct);

        if (expiredPledges.Count == 0)
        {
            _logger.LogDebug("No expired pledges found at {Time}.", now);
            return;
        }

        foreach (var pledge in expiredPledges)
        {
            pledge.Status = PledgeStatus.Expired;
            pledge.UpdatedAt = now;

            _logger.LogWarning(
                "Pledge {PledgeID} for File {FileNumber} expired on {Deadline}.",
                pledge.PledgeID,
                pledge.RetirementFile?.FileNumber,
                pledge.Deadline);
        }

        int updated = await db.SaveChangesAsync(ct);
        _logger.LogInformation("Marked {Count} pledges as expired.", updated);
    }
}
