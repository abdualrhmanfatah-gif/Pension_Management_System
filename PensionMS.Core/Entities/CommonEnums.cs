namespace PensionMS.Core.Entities;

public enum PendingReviewStatus
{
    None = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum FileReviewStatus
{
    None = 0,
    PendingReview = 1,
    Approved = 2,
    Rejected = 3
}

public enum AchievementObstacleType
{
    Achievement = 1,
    Obstacle = 2
}

public enum TrackingStatus
{
    New = 1,
    InProgress = 2,
    Completed = 3,
    Closed = 4
}

public enum PriorityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TrackingEntityType
{
    Department = 1,
    Employee = 2
}
