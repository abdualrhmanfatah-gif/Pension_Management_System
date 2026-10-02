using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.SoDRules;

/// <summary>
/// قاعدة SoD: رافع الوثائق لا يمكنه اعتماد الملف
/// </summary>
public class DocumentUploaderCannotApproveRule : ISoDRule
{
    public Task<SoDResult> EvaluateAsync(
        int actorId, SoDAction action, RetirementFile file, List<FileActionRecord> history)
    {
        if (action != SoDAction.Approve)
            return Task.FromResult(SoDResult.Ok());

        var uploadedByActor = history.Any(h =>
            h.UserId == actorId &&
            (h.Action == AuditActionConstants.UploadDocument || h.Action == AuditActionConstants.CreateDocument));
        if (uploadedByActor)
            return Task.FromResult(SoDResult.Fail("تعارض صلاحيات: لا يمكن لمن رفع وثائق الملف اعتماده."));

        return Task.FromResult(SoDResult.Ok());
    }
}

/// <summary>
/// قاعدة SoD: محرر البيانات المالية لا يمكنه اعتماد الملف
/// </summary>
public class FinancialEditorCannotApproveRule : ISoDRule
{
    public Task<SoDResult> EvaluateAsync(
        int actorId, SoDAction action, RetirementFile file, List<FileActionRecord> history)
    {
        if (action != SoDAction.Approve)
            return Task.FromResult(SoDResult.Ok());

        var editedFinancialByActor = history.Any(h =>
            h.UserId == actorId &&
            (h.Action == AuditActionConstants.EditFinancial || h.Action == AuditActionConstants.UpdateFinancialEntitlement));
        if (editedFinancialByActor)
            return Task.FromResult(SoDResult.Fail("تعارض صلاحيات: لا يمكن لمن حرر البيانات المالية اعتماد الملف."));

        return Task.FromResult(SoDResult.Ok());
    }
}

/// <summary>
/// قاعدة SoD: المعتمِد لا يمكنه تفعيل نفس الملف
/// </summary>
public class ApproverCannotActivateRule : ISoDRule
{
    public Task<SoDResult> EvaluateAsync(
        int actorId, SoDAction action, RetirementFile file, List<FileActionRecord> history)
    {
        if (action != SoDAction.Activate)
            return Task.FromResult(SoDResult.Ok());

        var approvedByActor = history.Any(h =>
            h.UserId == actorId &&
            (h.Action == AuditActionConstants.Approve || h.Action == AuditActionConstants.ChangeStatusApprove));
        if (approvedByActor)
            return Task.FromResult(SoDResult.Fail("تعارض صلاحيات: لا يمكن لمن اعتمد الملف تفعيله."));

        return Task.FromResult(SoDResult.Ok());
    }
}

