import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

import type { NotificationDto } from '@/types/notifications';

const MAX_NOTIFICATIONS = 50;

interface NotificationState {
  notifications: NotificationDto[];
  unreadCount: number;
  totalCount: number;
  currentPage: number;
  pageSize: number;
  isLoading: boolean;
  
  // Actions
  setNotifications: (notifications: NotificationDto[]) => void;
  addNotification: (notification: NotificationDto) => void;
  removeNotification: (id: number) => void;
  setUnreadCount: (count: number) => void;
  markAsRead: (id: number) => void;
  markAllAsRead: () => void;
  setTotalCount: (total: number) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  setLoading: (loading: boolean) => void;
  reset: () => void;
}

const initialState = {
  notifications: [] as NotificationDto[],
  unreadCount: 0,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20,
  isLoading: false,
};

export const useNotificationStore = create<NotificationState>()(
  immer((set) => ({
    ...initialState,

    // ── Set all notifications (recalculates counts) ──
    setNotifications: (notifications) =>
      set((state) => {
        state.notifications = notifications;
        state.unreadCount = notifications.filter((n) => !n.isRead).length;
        state.totalCount = notifications.length;
      }),

    // ── Add single notification (only increment if unread, cap at MAX) ──
    addNotification: (notification) =>
      set((state) => {
        // Prevent duplicates
        const exists = state.notifications.some(
          (n) => n.notificationID === notification.notificationID
        );
        if (exists) return;

        state.notifications.unshift(notification);
        // Cap to prevent unbounded memory growth on mobile
        if (state.notifications.length > MAX_NOTIFICATIONS) {
          state.notifications = state.notifications.slice(0, MAX_NOTIFICATIONS);
        }
        state.totalCount += 1;
        
        if (!notification.isRead) {
          state.unreadCount += 1;
        }
      }),

    // ── Remove notification (adjusts counts) ──
    removeNotification: (id) =>
      set((state) => {
        const notification = state.notifications.find((n) => n.notificationID === id);
        if (!notification) return;

        state.notifications = state.notifications.filter((n) => n.notificationID !== id);
        state.totalCount -= 1;
        
        if (!notification.isRead) {
          state.unreadCount = Math.max(0, state.unreadCount - 1);
        }
      }),

    // ── Set unread count directly ──
    setUnreadCount: (count) =>
      set((state) => {
        state.unreadCount = Math.max(0, count);
      }),

    // ── Mark single as read (only if currently unread) ──
    markAsRead: (id) =>
      set((state) => {
        const notification = state.notifications.find((n) => n.notificationID === id);
        if (!notification || notification.isRead) return;

        notification.isRead = true;
        state.unreadCount = Math.max(0, state.unreadCount - 1);
      }),

    // ── Mark all as read ──
    markAllAsRead: () =>
      set((state) => {
        state.notifications.forEach((n) => {
          n.isRead = true;
        });
        state.unreadCount = 0;
      }),

    // ── Set total count ──
    setTotalCount: (total) =>
      set((state) => {
        state.totalCount = Math.max(0, total);
      }),

    // ── Pagination ──
    setPage: (page) =>
      set((state) => {
        state.currentPage = Math.max(1, page);
      }),

    setPageSize: (size) =>
      set((state) => {
        state.pageSize = Math.max(1, size);
        state.currentPage = 1; // Reset to first page
      }),

    // ── Loading state ──
    setLoading: (loading) =>
      set((state) => {
        state.isLoading = loading;
      }),

    // ── Reset to initial ──
    reset: () => set(initialState),
  }))
);