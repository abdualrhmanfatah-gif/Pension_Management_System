<<<<<<< HEAD
import type { ApiResponse } from "@/types/api";
import type { NotificationAttachmentDto, NotificationDto, PagedResult } from "@/types/notifications";

import api from "./axios";
=======
import type { NotificationAttachmentDto, NotificationDto, PagedResult } from "@/types/notifications";

import api from "./axios";
import type { ApiResponse } from "@/types/api";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
  getAll: (page = 1, pageSize = 20, category?: string, signal?: AbortSignal) => 
    api.get<ApiResponse<PagedResult<NotificationDto>>>('/notifications', { params: { page, pageSize, category }, signal }).then(r => r.data),
=======
  getAll: (page = 1, pageSize = 20, category?: string) => 
    api.get<ApiResponse<PagedResult<NotificationDto>>>('/notifications', { params: { page, pageSize, category } }).then(r => r.data),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

  getAttachments: (notificationId: number) =>
    api.get<ApiResponse<NotificationAttachmentDto[]>>(`/notifications/${notificationId}/attachments`)
      .then(r => r.data)
      .catch((err: any) => {
        if (err?.response?.status === 404) {
          return { isSuccess: true, data: [] };
        }
        throw err;
      }),
    
<<<<<<< HEAD
  getUnreadCount: (signal?: AbortSignal) => 
    api.get<ApiResponse<number>>('/notifications/unread-count', { signal }).then(r => r.data),
=======
  getUnreadCount: () => 
    api.get<ApiResponse<number>>('/notifications/unread-count').then(r => r.data),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    
  markAsRead: (id: number) => 
    api.put<ApiResponse<boolean>>(`/notifications/${id}/read`).then(r => r.data),
    
  markAllAsRead: () => 
    api.put<ApiResponse<boolean>>('/notifications/read-all').then(r => r.data),

<<<<<<< HEAD
  /**
   * تنزيل مرفق عبر مسار الملكية (NF1.3).
   * المصادقة عبر Axios — لا رمز في URL، رابط Blob مؤقت يُلغي بعد الاستخدام.
   */
  downloadAttachment: async (notificationId: number, attachmentId: number, fileName: string) => {
    try {
      const response = await api.get<Blob>(
        `/notifications/${notificationId}/attachments/${attachmentId}/download`,
        { responseType: 'blob' }
      );
      const blobUrl = URL.createObjectURL(response.data);
      const link = document.createElement('a');
      link.href = blobUrl;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(blobUrl);
    } catch (err) {
      const axiosErr = err as { response?: { data?: { message?: string }; status?: number } };
      const message = axiosErr?.response?.data?.message
        || (axiosErr?.response?.status === 403 && 'غير مصرح بتنزيل هذا المرفق')
        || (axiosErr?.response?.status === 404 && 'المرفق غير موجود')
        || 'تعذر تنزيل المرفق';
      throw new Error(message);
    }
  },

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
