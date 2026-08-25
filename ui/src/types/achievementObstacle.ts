export type AchievementObstacleType = "Achievement" | "Obstacle";
export type TrackingStatus = "New" | "InProgress" | "Completed" | "Closed";

export interface AchievementObstacleDto {
    id: number;
    number: string;
    type: AchievementObstacleType;
    title: string;
    description?: string;
    status: TrackingStatus;
    ownerUserID: number;
    ownerUsername?: string;
    departmentID?: number;
    departmentName?: string;
    employeeID?: number;
    employeeName?: string;
    createdByUserID: number;
    createdByUsername?: string;
    createdAt: string;
    updatedByUsername?: string;
    updatedAt?: string;
    commentsCount: number;
    documentsCount: number;
}

export interface TrackingCommentDto {
    id: number;
    comment: string;
    createdByUserID: number;
    createdByUsername?: string;
    createdAt: string;
}

export interface AchievementObstacleFilter {
    type?: AchievementObstacleType;
    status?: TrackingStatus;
    ownerUserID?: number;
    departmentID?: number;
    employeeID?: number;
    search?: string;
    page?: number;
    pageSize?: number;
}

export interface AchievementObstacleStats {
    totalAchievements: number;
    totalObstacles: number;
    totalOpen: number;
    totalInProgress: number;
    totalCompleted: number;
    totalClosed: number;
}
