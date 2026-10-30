using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Jobs;

/// <summary>
/// Background Job يعمل يومياً لفحص التعهدات المنتهية المدة
/// وإيقاف الملفات النشطة المرتبطة بها تلقائياً
/// </summary>
public class ExpiredPledgesJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredPledgesJob> _logger;

    // يعمل مرة واحدة كل 24 ساعة
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public ExpiredPledgesJob(IServiceScopeFactory scopeFactory, ILogger<ExpiredPledgesJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredPledgesJob: بدء تشغيل خدمة مراقبة انتهاء صلاحية التعهدات");

        // انتظر دقيقة واحدة عند بدء التشغيل قبل أول تشغيل
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("ExpiredPledgesJob: بدء فحص التعهدات المنتهية - {Time}", DateTime.UtcNow);

                using var scope = _scopeFactory.CreateScope();
<<<<<<< HEAD
                var fileStatusService = scope.ServiceProvider.GetRequiredService<IExpiredPledgeProcessor>();
=======
                var fileStatusService = scope.ServiceProvider.GetRequiredService<IFileStatusService>();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

                await fileStatusService.ProcessExpiredPledgesAsync();

                _logger.LogInformation("ExpiredPledgesJob: اكتمل الفحص - {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExpiredPledgesJob: خطأ أثناء معالجة التعهدات المنتهية");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
