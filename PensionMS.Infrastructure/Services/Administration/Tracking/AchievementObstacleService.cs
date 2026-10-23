using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.AchievementObstacles;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Administration.Tracking;

public class AchievementObstacleService : IAchievementObstacleService
{
    private readonly IUnitOfWork _unitOfWork;

    public AchievementObstacleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ─── List / Paged ───────────────────────────────────

    public async Task<ApiResponse<PagedResult<AchievementObstacleDto>>> GetPagedAsync(AchievementObstacleFilterDto filter)
    {
        var query = _unitOfWork.AchievementObstacles.Query()
            .AsNoTracking()
            .Include(a => a.OwnerUser)
            .Include(a => a.Department)
            .Include(a => a.Employee)
            .Where(a => !a.IsDeleted);

        if (filter.Type.HasValue)
            query = query.Where(a => a.Type == filter.Type.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (filter.OwnerUserID.HasValue)
            query = query.Where(a => a.OwnerUserID == filter.OwnerUserID.Value);

        if (filter.DepartmentID.HasValue)
            query = query.Where(a => a.DepartmentID == filter.DepartmentID.Value);

        if (filter.EmployeeID.HasValue)
            query = query.Where(a => a.EmployeeID == filter.EmployeeID.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(a => a.Title.Contains(filter.Search) || a.Number.Contains(filter.Search));

        var total = await query.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Load comments/documents counts
        var ids = items.Select(a => a.ID).ToList();
        var commentCounts = await _unitOfWork.TrackingComments.Query()
            .AsNoTracking()
            .Where(c => ids.Contains(c.TrackingRecordId) && !c.IsDeleted)
            .GroupBy(c => c.TrackingRecordId)
            .Select(g => new { RecordId = g.Key, Count = g.Count() })
            .ToListAsync();
        var docCounts = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => ids.Contains(d.TrackingRecordID!.Value) && !d.IsDeleted)
            .GroupBy(d => d.TrackingRecordID)
            .Select(g => new { RecordId = g.Key!.Value, Count = g.Count() })
            .ToListAsync();

        var dictComments = commentCounts.ToDictionary(x => x.RecordId, x => x.Count);
        var dictDocs = docCounts.ToDictionary(x => x.RecordId, x => x.Count);

        var dtoItems = items.Select(a =>
        {
            var dto = MapToDto(a);
            dto.CommentsCount = dictComments.GetValueOrDefault(a.ID, 0);
            dto.DocumentsCount = dictDocs.GetValueOrDefault(a.ID, 0);
            return dto;
        }).ToList();

        return ApiResponse<PagedResult<AchievementObstacleDto>>.Success(new PagedResult<AchievementObstacleDto>
        {
            Items = dtoItems,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    // ─── By ID ──────────────────────────────────────────

    public async Task<ApiResponse<AchievementObstacleDto>> GetByIdAsync(int id)
    {
        var record = await _unitOfWork.AchievementObstacles.Query()
            .AsNoTracking()
            .Include(a => a.OwnerUser)
            .Include(a => a.Department)
            .Include(a => a.Employee)
            .Include(a => a.CreatedByUser)
            .Include(a => a.UpdatedByUser)
            .Include(a => a.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.CreatedByUser)
            .FirstOrDefaultAsync(a => a.ID == id && !a.IsDeleted);

        if (record == null)
            return ApiResponse<AchievementObstacleDto>.Fail("السجل غير موجود");

        var dto = MapToDto(record);
        dto.CommentsCount = record.Comments.Count;

        var docCount = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .CountAsync(d => d.TrackingRecordID == id && !d.IsDeleted);
        dto.DocumentsCount = docCount;

        return ApiResponse<AchievementObstacleDto>.Success(dto);
    }

    // ─── Create ─────────────────────────────────────────

    public async Task<ApiResponse<int>> CreateAsync(CreateAchievementObstacleDto dto, int userId)
    {
        var record = new AchievementObstacle
        {
            Type = dto.Type,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            OwnerUserID = dto.OwnerUserID,
            DepartmentID = dto.DepartmentID,
            EmployeeID = dto.EmployeeID,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AchievementObstacles.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        // Generate number: TRK-{year}-{id:00000}
        record.Number = $"TRK-{record.CreatedAt.Year}-{record.ID:00000}";
        _unitOfWork.AchievementObstacles.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(record.ID, "تم إنشاء السجل بنجاح");
    }

    // ─── Update ─────────────────────────────────────────

    public async Task<ApiResponse<bool>> UpdateAsync(int id, UpdateAchievementObstacleDto dto, int userId)
    {
        var record = await _unitOfWork.AchievementObstacles.GetByIdAsync(id);
        if (record == null || record.IsDeleted)
            return ApiResponse<bool>.Fail("السجل غير موجود");

        record.Type = dto.Type;
        record.Title = dto.Title;
        record.Description = dto.Description;
        record.Status = dto.Status;
        record.OwnerUserID = dto.OwnerUserID;
        record.DepartmentID = dto.DepartmentID;
        record.EmployeeID = dto.EmployeeID;
        record.UpdatedByUserID = userId;
        record.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AchievementObstacles.Update(record);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "تم تحديث السجل بنجاح");
    }

    // ─── Delete ─────────────────────────────────────────

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var record = await _unitOfWork.AchievementObstacles.GetByIdAsync(id);
        if (record == null)
            return ApiResponse<bool>.Fail("السجل غير موجود");

        record.IsDeleted = true;
        record.DeletedAt = DateTime.UtcNow;
        record.DeletedByUserID = userId;

        _unitOfWork.AchievementObstacles.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف السجل بنجاح");
    }

    // ─── Stats ──────────────────────────────────────────

    public async Task<ApiResponse<AchievementObstacleStatsDto>> GetStatsAsync(AchievementObstacleFilterDto filter)
    {
        var query = _unitOfWork.AchievementObstacles.Query()
            .AsNoTracking()
            .Where(a => !a.IsDeleted);

        if (filter.DepartmentID.HasValue)
            query = query.Where(a => a.DepartmentID == filter.DepartmentID.Value);

        if (filter.EmployeeID.HasValue)
            query = query.Where(a => a.EmployeeID == filter.EmployeeID.Value);

        var stats = new AchievementObstacleStatsDto();

        stats.TotalAchievements = await query.CountAsync(a => a.Type == AchievementObstacleType.Achievement);
        stats.TotalObstacles = await query.CountAsync(a => a.Type == AchievementObstacleType.Obstacle);

        stats.TotalOpen = await query.CountAsync(a => a.Status == TrackingStatus.New);
        stats.TotalInProgress = await query.CountAsync(a => a.Status == TrackingStatus.InProgress);
        stats.TotalCompleted = await query.CountAsync(a => a.Status == TrackingStatus.Completed);
        stats.TotalClosed = await query.CountAsync(a => a.Status == TrackingStatus.Closed);

        return ApiResponse<AchievementObstacleStatsDto>.Success(stats);
    }

    // ─── Comments ───────────────────────────────────────

    public async Task<ApiResponse<List<TrackingCommentDto>>> GetCommentsAsync(int recordId)
    {
        var comments = await _unitOfWork.TrackingComments.Query()
            .AsNoTracking()
            .Include(c => c.CreatedByUser)
            .Where(c => c.TrackingRecordId == recordId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<TrackingCommentDto>>.Success(comments.Select(c => new TrackingCommentDto
        {
            ID = c.ID,
            Comment = c.Comment,
            CreatedByUserID = c.CreatedByUserID,
            CreatedByUsername = c.CreatedByUser?.FullName,
            CreatedAt = c.CreatedAt
        }).ToList());
    }

    public async Task<ApiResponse<int>> AddCommentAsync(int recordId, string comment, int userId)
    {
        var record = await _unitOfWork.AchievementObstacles.GetByIdAsync(recordId);
        if (record == null || record.IsDeleted)
            return ApiResponse<int>.Fail("السجل غير موجود");

        var trackingComment = new TrackingComment
        {
            TrackingRecordId = recordId,
            Comment = comment,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TrackingComments.AddAsync(trackingComment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(trackingComment.ID, "تمت إضافة التعليق بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(int commentId, int userId)
    {
        var comment = await _unitOfWork.TrackingComments.GetByIdAsync(commentId);
        if (comment == null)
            return ApiResponse<bool>.Fail("التعليق غير موجود");

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedByUserID = userId;

        _unitOfWork.TrackingComments.Update(comment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف التعليق بنجاح");
    }

    // ─── Mapper ─────────────────────────────────────────

    private static AchievementObstacleDto MapToDto(AchievementObstacle a) => new()
    {
        ID = a.ID,
        Number = a.Number,
        Type = a.Type,
        Title = a.Title,
        Description = a.Description,
        Status = a.Status,
        OwnerUserID = a.OwnerUserID,
        OwnerUsername = a.OwnerUser?.FullName,
        DepartmentID = a.DepartmentID,
        DepartmentName = a.Department?.Name,
        EmployeeID = a.EmployeeID,
        EmployeeName = a.Employee?.FullName,
        CreatedByUserID = a.CreatedByUserID,
        CreatedByUsername = a.CreatedByUser?.FullName,
        CreatedAt = a.CreatedAt,
        UpdatedByUsername = a.UpdatedByUser?.FullName,
        UpdatedAt = a.UpdatedAt
    };
}