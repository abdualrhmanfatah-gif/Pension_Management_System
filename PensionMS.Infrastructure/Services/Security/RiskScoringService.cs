using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Security;

/// <summary>
/// خدمة تقييم المخاطر الأساسية (نموذج مبدئي لـ Fraud Detection)
/// تصدر تنبيهات لمسؤول الامتثال عند اكتشاف أنماط مشبوهة
/// </summary>
public class RiskScoringService : IRiskScoringService
{
    private readonly PensionDbContext _db;
    private readonly ILogger<RiskScoringService> _logger;

    public RiskScoringService(PensionDbContext db, ILogger<RiskScoringService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EvaluateRiskAsync(int fileId, string action, int userId, string? oldValues, string? newValues)
    {
        var risks = new List<RiskEvent>();

        // 1. فحص التعديل خارج أوقات العمل (After-Hours Action)
        var localTime = PensionMS.Core.Time.YemenTime.Now;
        if (localTime.Hour < 7 || localTime.Hour > 16)
        {
            risks.Add(new RiskEvent
            {
                FileId = fileId,
                RiskType = RiskType.AfterHoursAction,
                Description = $"إجراء ({action}) خارج أوقات العمل الرسمية ({localTime:HH:mm}).",
                Severity = RiskSeverity.Medium,
                RiskScore = 50
            });
        }

        // 2. فحص التعديلات المالية الحساسة
        if (action == AuditActionConstants.EditFinancial && !string.IsNullOrEmpty(newValues))
        {
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(newValues);
                // تبسيط: إذا احتوى التحديث المالي على قيمة كبيرة كمستحق
                if (dict != null && dict.ContainsKey("EntitlementAmount"))
                {
                    double amount = Convert.ToDouble(dict["EntitlementAmount"]?.ToString());
                    if (amount > 1500000) // أعلى من 1.5 مليون ريال يمني
                    {
                        risks.Add(new RiskEvent
                        {
                            FileId = fileId,
                            RiskType = RiskType.HighValueRetirement,
                            Description = "تم تعيين استحقاق مالي مرتفع القيمة.",
                            Severity = RiskSeverity.High,
                            RiskScore = 80
                        });
                    }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "فشل في تحليل البيانات المالية للملف {FileId}", fileId); }
        }

        if (risks.Any())
        {
            _db.RiskEvents.AddRange(risks);
            await _db.SaveChangesAsync();
        }
    }
}
