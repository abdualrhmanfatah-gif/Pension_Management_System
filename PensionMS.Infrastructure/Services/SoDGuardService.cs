using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

/// <summary>
/// محرك فصل المهام (SoD) — يجمع جميع القواعد المسجلة في DI ويطبقها بالتسلسل
/// </summary>
public class SoDGuardService : ISoDGuard
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ISoDRule> _rules;

    public SoDGuardService(IUnitOfWork unitOfWork, IEnumerable<ISoDRule> rules)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
    }

    public async Task<SoDResult> ValidateAsync(int fileId, int actorId, SoDAction action)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        if (file == null)
            return SoDResult.Fail("الملف غير موجود");

        // جمع سجل الأفعال التاريخي لهذا الملف من AuditLog
        var history = await _unitOfWork.AuditLogs.Query()
            .AsNoTracking()
            .Where(a => a.RecordID == fileId
                     && a.TableID == 2) // 2 = RetirementFiles (SystemTables seed)
            .Select(a => new FileActionRecord
            {
                UserId = a.UserID ?? 0,
                Action = a.Action,
                Timestamp = a.CreatedAt
            })
            .ToListAsync();

        // تشغيل كل قاعدة SoD مسجلة في DI
        foreach (var rule in _rules)
        {
            var result = await rule.EvaluateAsync(actorId, action, file, history);
            if (!result.IsAllowed)
                return result;
        }

        return SoDResult.Ok();
    }

    public async Task<SoDResult> ValidateSalaryBatchApprovalAsync(int batchId, int actorId)
    {
        var batch = await _unitOfWork.SalaryPaymentBatches.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BatchID == batchId && !b.IsDeleted);

        if (batch == null)
            return SoDResult.Fail("كشف الصرف غير موجود");

        if (batch.ImportedByUserID == actorId)
            return SoDResult.Fail("لا يجوز لمن أنشأ كشف الصرف اعتماده (فصل المهام)");

        return SoDResult.Ok();
    }
}

