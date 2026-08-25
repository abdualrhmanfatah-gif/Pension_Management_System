import type {
    AchievementObstacleDto,
    AchievementObstacleFilter,
    AchievementObstacleStats,
    TrackingCommentDto,
} from "@/types/achievementObstacle";
import type { ApiResponse, PagedResult } from "@/types/api-response";

import api from "./axios";

export const achievementObstacleApi = {
    // ─── CRUD ───────────────────────────────────────────

    list: (filter: AchievementObstacleFilter = {}) =>
        api
            .get<ApiResponse<PagedResult<AchievementObstacleDto>>>("/achievement-obstacles", { params: filter })
            .then((r) => r.data.data),

    stats: (filter: AchievementObstacleFilter = {}) =>
        api
            .get<ApiResponse<AchievementObstacleStats>>("/achievement-obstacles/stats", { params: filter })
            .then((r) => r.data.data),

    get: (id: number) =>
        api
            .get<ApiResponse<AchievementObstacleDto>>(`/achievement-obstacles/${id}`)
            .then((r) => r.data.data),

    create: (data: {
        type: string;
        title: string;
        description?: string;
        status: string;
        ownerUserID: number;
        departmentID?: number;
        employeeID?: number;
    }) =>
        api
            .post<ApiResponse<number>>("/achievement-obstacles", data)
            .then((r) => r.data),

    update: (
        id: number,
        data: {
            type: string;
            title: string;
            description?: string;
            status: string;
            ownerUserID: number;
            departmentID?: number;
            employeeID?: number;
        }
    ) =>
        api
            .put<ApiResponse<boolean>>(`/achievement-obstacles/${id}`, data)
            .then((r) => r.data),

    remove: (id: number) =>
        api
            .delete<ApiResponse<boolean>>(`/achievement-Obstacles/${id}`)
            .then((r) => r.data),

    // ─── Comments ───────────────────────────────────────

    getComments: (id: number) =>
        api
            .get<ApiResponse<TrackingCommentDto[]>>(`/achievement-obstacles/${id}/comments`)
            .then((r) => r.data.data),

    addComment: (id: number, comment: string) =>
        api
            .post<ApiResponse<number>>(`/achievement-obstacles/${id}/comments`, { comment })
            .then((r) => r.data),

    removeComment: (commentId: number) =>
        api
            .delete<ApiResponse<boolean>>(`/achievement-obstacles/comments/${commentId}`)
            .then((r) => r.data),
};
