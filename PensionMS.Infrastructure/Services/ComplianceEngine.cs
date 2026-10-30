using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class ComplianceEngine : IComplianceEngine
{
    private readonly IUnitOfWork _unitOfWork;

    public ComplianceEngine(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        var requiredTypes = await GetRequiredDocumentTypesAsync(fileId);
        var requiredTypeIds = requiredTypes.Select(t => t.DocumentTypeID).ToList();

        var uploadedTypeIds = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.FileID == fileId && !d.IsDeleted)
            .Select(d => d.DocumentTypeID)
            .Distinct()
            .ToListAsync();

        var missingTypeIds = requiredTypeIds.Except(uploadedTypeIds).ToList();

        return new ComplianceSummary
        {
            TotalRequiredCount = requiredTypeIds.Count,
            UploadedCount = uploadedTypeIds.Count(id => requiredTypeIds.Contains(id)),
            MissingCount = missingTypeIds.Count,
            MissingDocumentTypeIds = missingTypeIds
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
