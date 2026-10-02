import type { ApiResponse } from "@/types/api";
import type { NotificationAttachmentDto, NotificationDto, PagedResult } from "@/types/notifications";

import api from "./axios";

export interface CreateNotificationMultiDto {
  title: string;
  message: string;
  userIds: number[];
  category?: string;
  priority?: number;
  actionUrl?: string;
  attachments?: File[];
}

export interface AdminNotificationStatsDto {
  totalNotifications: number;
  unreadNotifications: number;
  sentToday: number;
  failedOutbox: number;
  unreadCount?: number;
  todayCount?: number;
  failedOutboxCount?: number;
  pendingOutboxCount?: number;
  byCategory?: Record<string, number>;
}

export interface AdminNotificationFilterDto {
  category?: string;
  userId?: number;
  status?: string;
  startDate?: string;
  endDate?: string;
}

export interface NotificationOutboxDto {
  outboxId: number;
  notificationId: number;
  channel: string;
  status: string;
  retryCount: number;
  maxRetries?: number;
  errorMessage?: string;
  createdAt: string;
  lastAttemptAt?: string;
  title?: string;
  userName?: string;
}

// Types exported for consumers (previously in adminNotifications.ts)
export interface AdminBroadcastNotificationDto {
  roleName?: string;
  branchID?: number;
  title: string;
  message: string;
  category?: string;
  priority?: number;
  actionUrl?: string;
}

export interface AdminNotificationFilter {
  page?: number;
  pageSize?: number;
  userID?: number;
  category?: string;
  isRead?: boolean;
  from?: string;
  to?: string;
  search?: string;
}

export interface BulkDeleteDto {
  ids?: number[];
  deleteAll?: boolean;
  category?: string;
  onlyRead?: boolean;
  olderThan?: string;
}

export const notificationsApi = {
  getAll: (page = 1, pageSize = 20, category?: string) => 
    api.get<ApiResponse<PagedResult<NotificationDto>>>('/notifications', { params: { page, pageSize, category } }).then(r => r.data),

  getAttachments: (notificationId: number) =>
    api.get<ApiResponse<NotificationAttachmentDto[]>>(`/notifications/${notificationId}/attachments`)
      .then(r => r.data)
      .catch((err: any) => {
        if (err?.response?.status === 404) {
          return { isSuccess: true, data: [] };
        }
        throw err;
      }),
    
  getUnreadCount: () => 
    api.get<ApiResponse<number>>('/notifications/unread-count').then(r => r.data),
    
  markAsRead: (id: number) => 
    api.put<ApiResponse<boolean>>(`/notifications/${id}/read`).then(r => r.data),
    
  markAllAsRead: () => 
    api.put<ApiResponse<boolean>>('/notifications/read-all').then(r => r.data),

  // Admin endpoints
  admin: {
    getAll: (page = 1, pageSize = 20, filters?: AdminNotificationFilterDto) =>
      api.get<ApiResponse<PagedResult<NotificationDto>>>('/admin/notifications', { params: { page, pageSize, ...filters } }).then(r => r.data),
    
    getStats: () =>
      api.get<ApiResponse<AdminNotificationStatsDto>>('/admin/notifications/stats').then(r => r.data),
    
    send: (dto: { userId: number; title: string; message: string; category?: string; priority?: number }) =>
      api.post<ApiResponse<NotificationDto>>('/admin/notifications/send', dto).then(r => r.data),
    
    broadcast: (dto: { title: string; message: string; category?: string; priority?: number }) =>
      api.post<ApiResponse<number>>('/admin/notifications/broadcast', dto).then(r => r.data),
    
    sendMulti: (dto: CreateNotificationMultiDto) => {
      const formData = new FormData();
      formData.append('title', dto.title);
      formData.append('message', dto.message);
      dto.userIds.forEach(id => formData.append('userIds', id.toString()));
      if (dto.category) formData.append('category', dto.category);
      if (dto.priority) formData.append('priority', dto.priority.toString());
      if (dto.actionUrl) formData.append('actionUrl', dto.actionUrl);
      dto.attachments?.forEach(f => formData.append('attachments', f));
      
      return api.post<ApiResponse<NotificationDto[]>>('/admin/notifications/send-multi', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      }).then(r => r.data);
    },
    
    getOutbox: (page = 1, pageSize = 20, status?: string) =>
      api.get<ApiResponse<PagedResult<NotificationOutboxDto>>>('/admin/notifications/outbox', { params: { page, pageSize, status } }).then(r => r.data),
    
    retryOutbox: (id: number) =>
      api.post<ApiResponse<boolean>>(`/admin/notifications/outbox/${id}/retry`).then(r => r.data),
    
    delete: (id: number) =>
      api.delete<ApiResponse<boolean>>(`/admin/notifications/${id}`).then(r => r.data),
    
    bulkDelete: (dto: BulkDeleteDto) =>
      api.delete<ApiResponse<number>>('/admin/notifications/bulk', { data: dto }).then(r => r.data),
  }
};
