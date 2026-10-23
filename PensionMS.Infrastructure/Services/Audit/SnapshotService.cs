using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Audit;

/// <summary>
/// خدمة لقطات الملفات — تحفظ حالة الملفات كاملة عند كل تغيير رئيسي
/// </summary>
public class SnapshotService : ISnapshotService
{
    private readonly PensionDbContext _db;
    private readonly ILogger<SnapshotService> _logger;

    public SnapshotService(PensionDbContext db, ILogger<SnapshotService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task CaptureAsync(int fileId, string snapshotType, int userId, string? reason = null)
    {
        var file = await _db.RetirementFiles
            .Include(f => f.Retiree)
            .Include(f => f.Status)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        if (file == null) return;

        // آخر رقم نسخة + 1
        var lastVersion = await _db.FileSnapshots
            .Where(s => s.FileId == fileId)
            .MaxAsync(s => (int?)s.Version) ?? 0;

        var fullState = JsonSerializer.Serialize(new
        {
            file.FileID,
            file.FileNumber,
            file.RetireeID,
            RetireeName = file.Retiree?.FullName,
            file.StatusID,
            StatusName = file.Status?.StatusName,
            file.RankID,
            RankName = file.Rank?.RankName,
            file.PensionBranchID,
            BranchName = file.PensionBranch?.BranchName,
            file.AssignedEmployeeID,
            file.FileOpenDate,
            file.ClosedDate,
            file.ServiceStartDate,
            file.ServiceEndDate,
            file.RetirementDate,
            file.StatusReason,
            file.Notes,
            file.CreatedAt,
            file.UpdatedAt
        }, new JsonSerializerOptions { WriteIndented = false });

        // حساب الـ Diff مع آخر لقطة
        string? jsonDiff = null;
        var previousSnapshot = await _db.FileSnapshots
            .Where(s => s.FileId == fileId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();

        if (previousSnapshot != null)
        {
            jsonDiff = ComputeDiff(previousSnapshot.FullState, fullState);
        }

        var snapshot = new FileSnapshot
        {
            FileId = fileId,
            Version = lastVersion + 1,
            SnapshotType = snapshotType,
            FullState = fullState,
            JsonDiff = jsonDiff,
            CreatedByUserId = userId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _db.FileSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// حساب فرق بسيط بين حالتين JSON — يُرجع الحقول المتغيرة فقط
    /// </summary>
    private string? ComputeDiff(string oldJson, string newJson)
    {
        try
        {
            var oldDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson);
            var newDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newJson);

            if (oldDict == null || newDict == null) return null;

            var changes = new Dictionary<string, object?>();
            foreach (var kvp in newDict)
            {
                if (!oldDict.TryGetValue(kvp.Key, out var oldVal) ||
                    oldVal.ToString() != kvp.Value.ToString())
                {
                    changes[kvp.Key] = new
                    {
                        Old = oldDict.TryGetValue(kvp.Key, out var ov) ? ov.ToString() : null,
                        New = kvp.Value.ToString()
                    };
                }
            }

            return changes.Count > 0
                ? JsonSerializer.Serialize(changes, new JsonSerializerOptions { WriteIndented = false })
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل في حساب الفرق بين اللقطات (JSON Diff)");
            return null;
        }
    }
}
