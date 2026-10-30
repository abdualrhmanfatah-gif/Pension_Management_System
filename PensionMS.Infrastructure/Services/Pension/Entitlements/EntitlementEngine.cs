using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Rules.Entitlements;

namespace PensionMS.Infrastructure.Services.Pension.Entitlements;

public class EntitlementEngine : IEntitlementEngine
{
    private readonly IUnitOfWork _unitOfWork;

    public EntitlementEngine(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> CheckCoreEligibilityAsync(int entitledId)
    {
        var ep = await _unitOfWork.EntitledPersons.Query()
            .Include(e => e.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Include(e => e.Relationship)
            .FirstOrDefaultAsync(e => e.EntitledID == entitledId);

        if (ep == null) return ApiResponse<bool>.Fail("المستحق غير موجود");
        if (ep.IsDeleted || ep.IsDeceased) return ApiResponse<bool>.Fail("المستحق محذوف أو متوفى");

        var retiree = ep.RetirementFile?.Retiree;
        if (retiree == null) return ApiResponse<bool>.Fail("بيانات المتقاعد غير موجودة");

        var decision = EntitledPersonRules.EvaluateEligibility(retiree.IsAlive, ep.Relationship?.RelationshipName,
            ep.BirthDate, ep.SocialStatusID, ep.HasDisabilityException, PensionMS.Core.Time.YemenTime.Today);
        return decision.IsEligible ? ApiResponse<bool>.Success(true, decision.Message) : ApiResponse<bool>.Fail(decision.Message);
    }

    public async Task<ApiResponse<bool>> ReEvaluateEntitlementStatusAsync(int entitledId)
    {
        var ep = await _unitOfWork.EntitledPersons.Query()
            .Include(e => e.RetirementFile)
                .ThenInclude(f => f.Retiree)
            .Include(e => e.Relationship)
            .Include(e => e.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(e => e.EntitledID == entitledId);

        if (ep == null) return ApiResponse<bool>.Fail("المستحق غير موجود");
        var state = new EntitledPersonRules.AutomaticEntitlementState(ep.EntitledID, ep.FileID,
            ep.IsActive, ep.IsDeleted, ep.IsDeceased, ep.RetirementFile?.Retiree?.IsAlive, ep.StopCode,
            ep.RelationshipID, ep.Relationship?.RelationshipName, ep.BirthDate, ep.FullName,
            ep.SocialStatusID, ep.HealthStatusID, ep.HasDisabilityException, ep.NationalID);
        var today = PensionMS.Core.Time.YemenTime.Today;
        var decision = EntitledPersonRules.EvaluateAutomaticPrerequisites(state, today);
        if (decision == null)
        {
            var requirements = EntitledPersonRules.AutomaticDocumentRequirements(state, today);
            var activeTypeIds = new HashSet<int>();
            if (requirements.Count > 0)
            {
                var typeIds = requirements.Select(r => r.DocumentTypeID).ToArray();
                activeTypeIds = (await _unitOfWork.DocumentTypes.Query()
                    .Where(t => t.IsActive && typeIds.Contains(t.DocumentTypeID))
                    .Select(t => t.DocumentTypeID).ToListAsync()).ToHashSet();
            }
            var deficiencies = EntitledPersonRules.EvaluateAutomaticDeficiencies(state, requirements,
                activeTypeIds, ep.Documents.ToArray(), today);
            var coverage = deficiencies.NeedsCoverage ? await LoadCoverageAsync(ep) : (EntitledPersonRules.AutomaticCoverage?)null;
            decision = EntitledPersonRules.CompleteAutomaticEvaluation(deficiencies, coverage);
        }

        if (decision.Change is { } change)
        {
            ep.IsActive = change.IsActive;
            if (change.ReplaceStopFields)
            {
                ep.StopCode = change.StopCode;
                ep.StopReason = change.StopReason;
            }
            switch (change.StopDateChange)
            {
                case EntitledPersonRules.AutomaticStopDateChange.SetUtcNow:
                    ep.StopDate = DateTime.UtcNow;
                    break;
                case EntitledPersonRules.AutomaticStopDateChange.Clear:
                    ep.StopDate = null;
                    break;
            }
            if (change.RefreshUpdatedAt) ep.UpdatedAt = DateTime.UtcNow;
            if (!change.IsActive) await DeactivateLinkedAgentsAndRecipientsAsync(entitledId);
            await _unitOfWork.SaveChangesAsync();
        }
        return decision.IsSuccess ? ApiResponse<bool>.Success(true, decision.Message) : ApiResponse<bool>.Fail(decision.Message);
    }

    private async Task DeactivateLinkedAgentsAndRecipientsAsync(int entitledId)
    {
        var agents = await _unitOfWork.Agents.Query()
            .Where(a => a.EntitledID == entitledId && a.IsActive && !a.IsDeleted)
            .ToListAsync();
        foreach (var a in agents)
        {
            a.IsActive = false;
            a.UpdatedAt = DateTime.UtcNow;
        }

        await PensionMS.Infrastructure.Helpers.RecipientScope.EndForEntitledAsync(_unitOfWork, entitledId);
    }

    private async Task<EntitledPersonRules.AutomaticCoverage> LoadCoverageAsync(EntitledPerson person)
    {
        var rows = await _unitOfWork.MissingDocuments.Query().AsNoTracking()
            .Where(m => m.FileID == person.FileID && m.EntitledID == person.EntitledID && !m.IsDeleted).ToListAsync();
        var now = DateTime.UtcNow;
        var pledges = (await _unitOfWork.DocumentPledges.Query().AsNoTracking()
            .Where(p => p.FileID == person.FileID && !p.IsDeleted && p.Deadline > now &&
                p.Status != PledgeStatusConstants.Expired && p.Status != PledgeStatusConstants.Fulfilled)
            .Select(p => p.PledgeID).ToListAsync()).ToHashSet();
        var exemptions = (await _unitOfWork.DocumentExemptions.Query().AsNoTracking()
            .Where(e => e.FileID == person.FileID && !e.IsDeleted).Select(e => e.ExemptionID).ToListAsync()).ToHashSet();
        return new(rows, pledges, exemptions);
    }
}
