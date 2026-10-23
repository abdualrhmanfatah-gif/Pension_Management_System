using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Workflow;

/// <summary>
/// محرك توجيه سير العمل — يعتمد على حالة الملفات ويعين المهام آلياً
/// </summary>
public class WorkflowTaskService : IWorkflowTaskService
{
    private readonly PensionDbContext _db;

    public WorkflowTaskService(PensionDbContext db)
    {
        _db = db;
    }

    public async Task RouteFileAsync(int fileId, string action, int currentUserId)
    {
        var file = await _db.RetirementFiles.FindAsync(fileId);
        if (file == null) return;

        // إكمال المهام المعلقة السابقة
        var pendingTasks = await _db.PendingTasks
            .Where(t => t.FileId == fileId && t.Status == LookupConstants.Pending)
            .ToListAsync();

        foreach (var task in pendingTasks)
        {
            task.Status = LookupConstants.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedByUserId = currentUserId;
        }

        // تحديد المهمة القادمة استناداً لحالة الملفات الجديدة
        string? nextTaskType = null;
        int? assignToRoleId = null;
        int? assignToUserId = null;

        if (file.StatusID == FileStatusConstants.New)
        {
            nextTaskType = "DataEntry";
            assignToRoleId = RoleConstants.DataEntry;
        }
        else if (file.StatusID == FileStatusConstants.Pending)
        {
            nextTaskType = "FinalApproval";
            assignToRoleId = RoleConstants.FinalApprover;

            // إذا كان المدخل قد عُين مسبقاً، نوجهه لرئيسه المباشر أو لمجموعة المعتمدين في نفس الفرع
            // هنا للتبسيط نوجه للمجموعة
        }

        if (nextTaskType != null)
        {
            const int defaultDurationHours = 48;
            var newTask = new PendingTask
            {
                FileId = fileId,
                TaskType = nextTaskType,
                AssignedToRoleId = assignToRoleId,
                AssignedToUserId = assignToUserId,
                Status = LookupConstants.Pending,
                DueDate = DateTime.UtcNow.AddHours(defaultDurationHours),
                CreatedAt = DateTime.UtcNow
            };

            _db.PendingTasks.Add(newTask);
        }

        await _db.SaveChangesAsync();
    }

    public async Task CreateSalaryBatchApprovalTaskAsync(int batchId, DateTime batchMonth)
    {
        var newTask = new PendingTask
        {
            FileId = null,
            SalaryBatchID = batchId,
            TaskType = "SalaryBatchApproval",
            AssignedToRoleId = RoleConstants.FinalApprover,
            AssignedToUserId = null,
            Status = LookupConstants.Pending,
            DueDate = DateTime.UtcNow.AddHours(72),
            CreatedAt = DateTime.UtcNow
        };
        _db.PendingTasks.Add(newTask);
        await _db.SaveChangesAsync();
    }

    public async Task ResolveSalaryBatchApprovalTasksAsync(int batchId, int completedByUserId)
    {
        var tasks = await _db.PendingTasks
            .Where(t => t.SalaryBatchID == batchId && t.Status == LookupConstants.Pending)
            .ToListAsync();
        foreach (var task in tasks)
        {
            task.Status = LookupConstants.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedByUserId = completedByUserId;
        }
        await _db.SaveChangesAsync();
    }
}
