using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

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
        if (ep.PendingReviewStatus == PendingReviewStatus.Pending || ep.PendingReviewStatus == PendingReviewStatus.Rejected)
            return ApiResponse<bool>.Fail("المستحق قيد المراجعة أو مرفوض");

        var retiree = ep.RetirementFile?.Retiree;
        if (retiree == null) return ApiResponse<bool>.Fail("بيانات المتقاعد غير موجودة");

        // 1. فحص حالة المتقاعد
        if (retiree.IsAlive)
        {
            return ApiResponse<bool>.Fail("لا يمكن التفعيل: المتقاعد لا يزال على قيد الحياة");
        }

        // 2. فحص الأهلية القانونية (العمر والزواج)
        if (ep.BirthDate == default)
            return ApiResponse<bool>.Fail("تاريخ الميلاد غير محدد — لا يمكن تقييم الأهلية");

        var ageYears = PensionMS.Core.Time.YemenTime.Age(ep.BirthDate);
        var rel = ep.Relationship?.RelationshipName;

        if (rel == RelationshipConstants.Son)
        {
            if (ageYears >= 24 && !ep.HasDisabilityException)
            {
                return ApiResponse<bool>.Fail("المستحق غير مؤهل: تجاوز السن القانوني (24 سنة)");
            }
        }
        else if (rel == RelationshipConstants.Daughter)
        {
            if (ep.SocialStatusID == SocialStatusConstants.Married)
            {
                return ApiResponse<bool>.Fail("المستحق غير مؤهل: البنت متزوجة");
            }
        }

        return ApiResponse<bool>.Success(true, "المستحق مستوفٍ للشروط القانونية الأساسية");
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
        if (ep.IsDeleted || ep.IsDeceased) return ApiResponse<bool>.Success(true, "المستحق محذوف أو متوفى");
        if (ep.PendingReviewStatus == PendingReviewStatus.Pending || ep.PendingReviewStatus == PendingReviewStatus.Rejected)
            return ApiResponse<bool>.Success(true, "المستحق قيد المراجعة أو مرفوض — لا يمكن إعادة التقييم");

        var retiree = ep.RetirementFile?.Retiree;
        if (retiree == null) return ApiResponse<bool>.Fail("بيانات المتقاعد غير موجودة");

        // 1. إذا كان المتقاعد على قيد الحياة → يبقى معلقاً (موقوفاً)
        if (retiree.IsAlive)
        {
            if (ep.IsActive || ep.StopCode != EntitledStopCodes.RetireeAlive)
            {
                ep.IsActive = false;
                ep.StopCode = EntitledStopCodes.RetireeAlive;
                ep.StopReason = "المتقاعد على قيد الحياة — الاستحقاق معلق";
            }
            await DeactivateLinkedAgentsAndRecipientsAsync(entitledId);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "تم تحديث الحالة: المتقاعد على قيد الحياة");
        }

        // 2. فحص الأهلية القانونية (العمر والزواج) عبر الدالة المركزية
        var coreResult = await CheckCoreEligibilityAsync(entitledId);
        if (!coreResult.IsSuccess)
        {
            var ageYears = ep.BirthDate == default ? 0 : PensionMS.Core.Time.YemenTime.Age(ep.BirthDate);
            var rel = ep.Relationship?.RelationshipName;

            if (rel == RelationshipConstants.Son && ageYears >= 24 && !ep.HasDisabilityException)
            {
                ep.IsActive = false;
                ep.StopCode = EntitledStopCodes.AgeLimit;
                ep.StopReason = coreResult.Message;
            }
            else if (rel == RelationshipConstants.Daughter && ep.SocialStatusID == SocialStatusConstants.Married)
            {
                ep.IsActive = false;
                ep.StopCode = EntitledStopCodes.DaughterMarried;
                ep.StopReason = coreResult.Message;
            }
            else
            {
                ep.IsActive = false;
                ep.StopReason = coreResult.Message;
            }

            ep.StopDate = DateTime.UtcNow;
            await DeactivateLinkedAgentsAndRecipientsAsync(entitledId);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, coreResult.Message);
        }

        // 3. المتقاعد متوفى والأهلية صحيحة → فحص اكتمال البيانات والوثائق
        if (ep.StopCode != null && ep.StopCode != EntitledStopCodes.RetireeAlive && ep.StopCode != EntitledStopCodes.MissingData)
        {
            return ApiResponse<bool>.Success(true, "المستحق لديه إيقاف يدوي أو سبب آخر فلا يتم تفعيله تلقائياً");
        }

        var issues = new List<string>();

        // ── فحص البيانات الأساسية ──
        if (string.IsNullOrWhiteSpace(ep.FullName)) issues.Add("الاسم الكامل");
        if (ep.BirthDate == default) issues.Add("تاريخ الميلاد");
        if (ep.SocialStatusID == null || ep.SocialStatusID == 0) issues.Add("الحالة الاجتماعية");
        if (ep.HealthStatusID == null || ep.HealthStatusID == 0) issues.Add("الحالة الصحية");

        var ageYearsVal = ep.BirthDate == default ? 0 : PensionMS.Core.Time.YemenTime.Age(ep.BirthDate);
        bool isAdult = ep.BirthDate != default && ageYearsVal >= 18;

        if (isAdult && string.IsNullOrWhiteSpace(ep.NationalID)) issues.Add("الرقم الوطني");

        // ── فحص الوثائق الإلزامية ──
        if (isAdult)
        {
            bool hasId = ep.Documents.Any(d => d.DocumentTypeID == DocumentTypeConstants.NationalId);
            if (!hasId) issues.Add("وثيقة الهوية الوطنية");
        }

        // وثائق مشروطة حسب الحالة الاجتماعية (للإناث)
        var relName = ep.Relationship?.RelationshipName;
        var femaleRels = new[] { RelationshipConstants.Daughter, RelationshipConstants.Wife, RelationshipConstants.Mother, RelationshipConstants.Sister };
        if (femaleRels.Contains(relName ?? ""))
        {
            var ssId = ep.SocialStatusID;
            if (ssId == SocialStatusConstants.Single && ageYearsVal >= 18)
            {
                bool hasSingleCert = ep.Documents.Any(d => d.DocumentTypeID == DocumentTypeConstants.SingleStatusCert);
                if (!hasSingleCert) issues.Add("إثبات العزوبية");
            }
            else if (ssId == SocialStatusConstants.Married)
            {
                bool hasMarriageContract = ep.Documents.Any(d => d.DocumentTypeID == DocumentTypeConstants.MarriageContract);
                if (!hasMarriageContract) issues.Add("عقد الزواج");
            }
        }

        if (ep.HasDisabilityException)
        {
            bool hasMedicalReport = ep.Documents.Any(d => d.DocumentTypeID == DocumentTypeConstants.MedicalReport);
            if (!hasMedicalReport) issues.Add("التقرير الطبي");
        }

        // ── تحديث الحالة النهائية ──
        if (issues.Count > 0)
        {
            ep.IsActive = false;
            ep.StopCode = EntitledStopCodes.MissingData;
            ep.StopReason = $"بيانات/وثائق ناقصة: {string.Join("، ", issues)}";
        }
        else
        {
            ep.IsActive = true;
            ep.StopCode = null;
            ep.StopReason = null;
            ep.StopDate = null;
        }

        ep.UpdatedAt = DateTime.UtcNow;
        if (!ep.IsActive) await DeactivateLinkedAgentsAndRecipientsAsync(entitledId);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, ep.IsActive ? "تم تفعيل المستحق بنجاح" : "المستحق لا يزال لديه نواقص");
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
}
