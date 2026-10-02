using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class ComplianceEngine : IComplianceEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMissingDocumentService _missingDocumentService;

    public ComplianceEngine(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService)
    {
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
    }

    public async Task<List<DocumentType>> GetRequiredDocumentTypesAsync(int fileId)
    {
        // 1. Fetch the full file graph needed for rule evaluation
        var file = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Agents.Where(a => a.IsActive && !a.IsDeleted))
            .Include(f => f.EntitledPersons.Where(e => e.IsActive && !e.IsDeleted))
            .Include(f => f.Guardians.Where(g => g.IsActive && !g.IsDeleted))
            .Include(f => f.DocumentPledges.Where(p => !p.IsDeleted && p.Status != MissingDocumentStatusConstants.Fulfilled))
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        if (file == null) return new List<DocumentType>();

        // 2. Fetch all active rules and all document types
        var rules = await _unitOfWork.DocumentRequirementRules.Query()
            .AsNoTracking()
            .Include(r => r.DocumentType)
            .Where(r => r.IsActive)
            .ToListAsync();

        var requiredTypes = new HashSet<int>();

        // 3. Evaluate Rules
        foreach (var rule in rules)
        {
            if (rule.IsGlobal)
            {
                requiredTypes.Add(rule.DocumentTypeID);
                continue;
            }

            // Evaluate Conditional Rules
            bool isMet = rule.TargetEntity switch
            {
                "File" => EvaluateCondition(file, rule.ConditionField, rule.ConditionValue),
                "Retiree" => EvaluateCondition(file.Retiree, rule.ConditionField, rule.ConditionValue),
                "Agent" => file.Agents.Any(a => EvaluateCondition(a, rule.ConditionField, rule.ConditionValue)),
                "EntitledPerson" => file.EntitledPersons.Any(e => EvaluateCondition(e, rule.ConditionField, rule.ConditionValue)),
                "Guardian" => file.Guardians.Any(g => EvaluateCondition(g, rule.ConditionField, rule.ConditionValue)),
                _ => false
            };

            if (isMet)
            {
                requiredTypes.Add(rule.DocumentTypeID);
            }
        }

        return await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => requiredTypes.Contains(dt.DocumentTypeID))
            .ToListAsync();
    }

    public async Task<ComplianceSummary> GetComplianceSummaryAsync(int fileId)
    {
        // م22: الأساس = أزواج (نوع + مالك) كاملة من كشف المتطلبات + النواقص اليدوية المعلّقة
        var requiredPairs = await _missingDocumentService.GetRequiredPairsAsync(fileId);

        var seen = new HashSet<(int, int?, int?, int?, int?, int?)>();
        var pairs = new List<(int DocumentTypeID, int? AgentID, int? GuardianID, int? EntitledID, int? PledgeID, int? ExemptionID)>();

        foreach (var p in requiredPairs)
        {
            var key = (p.DocumentTypeID, p.AgentID, p.GuardianID, p.EntitledID, p.PledgeID, p.ExemptionID);
            if (seen.Add(key))
                pairs.Add((p.DocumentTypeID, p.AgentID, p.GuardianID, p.EntitledID, p.PledgeID, p.ExemptionID));
        }

        var manualPending = await _unitOfWork.MissingDocuments.Query()
            .AsNoTracking()
            .Where(m => m.FileID == fileId
                     && (m.Status == MissingDocumentStatusConstants.Pending || m.Status == MissingDocumentStatusConstants.Pledged))
            .Select(m => new { m.DocumentTypeID, m.AgentID, m.GuardianID, m.EntitledID, m.PledgeID, m.ExemptionID })
            .ToListAsync();

        foreach (var m in manualPending)
        {
            var key = (m.DocumentTypeID, m.AgentID, m.GuardianID, m.EntitledID, m.PledgeID, m.ExemptionID);
            if (seen.Add(key))
                pairs.Add((m.DocumentTypeID, m.AgentID, m.GuardianID, m.EntitledID, m.PledgeID, m.ExemptionID));
        }

        // صلاحية الوثيقة للاكتمال: ليست مرفوضة الفحص وليست منتهية الصلاحية
        var today = PensionMS.Core.Time.YemenTime.Today;
        var validDocs = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.FileID == fileId && !d.IsDeleted
                     && d.PendingReviewStatus != PendingReviewStatus.Rejected
                     && (!d.ExpiryDate.HasValue || d.ExpiryDate.Value >= today))
            .Select(d => new { d.DocumentTypeID, d.AgentID, d.GuardianID, d.EntitledID, d.PledgeID, d.ExemptionID })
            .ToListAsync();

        bool IsSatisfied((int DocumentTypeID, int? AgentID, int? GuardianID, int? EntitledID, int? PledgeID, int? ExemptionID) pair) =>
            validDocs.Any(d => d.DocumentTypeID == pair.DocumentTypeID
                            && d.AgentID == pair.AgentID
                            && d.GuardianID == pair.GuardianID
                            && d.EntitledID == pair.EntitledID
                            && d.PledgeID == pair.PledgeID
                            && d.ExemptionID == pair.ExemptionID);

        var missing = pairs.Where(p => !IsSatisfied(p)).ToList();

        return new ComplianceSummary
        {
            TotalRequiredCount = pairs.Count,
            UploadedCount = pairs.Count - missing.Count,
            MissingCount = missing.Count,
            MissingDocumentTypeIds = missing.Select(p => p.DocumentTypeID).Distinct().ToList()
        };
    }

    public async Task RefreshComplianceCacheAsync(int fileId)
    {
        var summary = await GetComplianceSummaryAsync(fileId);
        
        // Use a raw SQL update or a direct entity update to avoid loading the whole entity if possible, 
        // but here we keep it simple for the repository pattern
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file != null)
        {
            file.MissingDocumentsCount = summary.MissingCount;
            file.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.RetirementFiles.Update(file);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private static bool EvaluateCondition(object? target, string? field, string? value)
    {
        if (target == null || string.IsNullOrEmpty(field)) return false;

        // Special case: If field is "Exists", we just check if the target list or entity is not null
        // (Handled by the switch in GetRequiredDocumentTypesAsync mostly, but added for safety)
        if (field == RuleConstants.ConditionFieldExists) return true;

        var prop = target.GetType().GetProperty(field ?? string.Empty);
        if (prop == null) return false;

        var actualValue = prop.GetValue(target)?.ToString();
        return string.Equals(actualValue, value, StringComparison.OrdinalIgnoreCase);
    }
}
