using PensionMS.Core.Entities;

namespace PensionMS.Core.DTOs.AchievementObstacles;

// ─── Commands ──────────────────────────────────────────

public class CreateAchievementObstacleDto
{
    public AchievementObstacleType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TrackingStatus Status { get; set; } = TrackingStatus.New;
    public int OwnerUserID { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
}

public class UpdateAchievementObstacleDto
{
    public AchievementObstacleType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TrackingStatus Status { get; set; }
    public int OwnerUserID { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
    public List<CreateTrackingLinkDto>? Links { get; set; }
}

public class CreateTrackingLinkDto
{
    public TrackingEntityType EntityType { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
}

// ─── Read Models ───────────────────────────────────────

public class AchievementObstacleDto
{
    public int ID { get; set; }
    public string Number { get; set; } = string.Empty;
    public AchievementObstacleType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TrackingStatus Status { get; set; }
    public int OwnerUserID { get; set; }
    public string? OwnerUsername { get; set; }
    public int? DepartmentID { get; set; }
    public string? DepartmentName { get; set; }
    public int? EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public int CreatedByUserID { get; set; }
    public string? CreatedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedByUsername { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TrackingLinkDto> Links { get; set; } = new();
    public int CommentsCount { get; set; }
    public int DocumentsCount { get; set; }
}

public class TrackingLinkDto
{
    public int ID { get; set; }
    public TrackingEntityType EntityType { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TrackingCommentDto
{
    public int ID { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int CreatedByUserID { get; set; }
    public string? CreatedByUsername { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ─── Filter ────────────────────────────────────────────

public class AchievementObstacleFilterDto
{
    public AchievementObstacleType? Type { get; set; }
    public TrackingStatus? Status { get; set; }
    public int? OwnerUserID { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
    public TrackingEntityType? EntityType { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ─── Stats ─────────────────────────────────────────────

public class AchievementObstacleStatsDto
{
    public int TotalAchievements { get; set; }
    public int TotalObstacles { get; set; }
    public int TotalOpen { get; set; }
    public int TotalInProgress { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalClosed { get; set; }
}
