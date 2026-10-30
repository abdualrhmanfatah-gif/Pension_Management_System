import { create } from 'zustand';
<<<<<<< HEAD

import type { NotificationDto } from '@/types/notifications';

export const MAX_PENDING_TOASTS = 50;
type ConnectionStatus = 'disconnected' | 'connecting' | 'connected';

interface NotificationState {
  ownerId: number | null;
  connectionStatus: ConnectionStatus;
  /** Includes visible cards; entries leave only after dismissal or expiry. */
  toastQueue: NotificationDto[];
  seenIds: ReadonlySet<number>;
  overflowCount: number;
  bindUser: (userId: number) => void;
  setConnectionStatus: (status: ConnectionStatus) => void;
  enqueueToast: (notification: NotificationDto) => boolean;
  dequeueToast: (id: number) => void;
  clearOverflow: () => void;
  reset: () => void;
}

const initialState = () => ({
  ownerId: null as number | null,
  connectionStatus: 'disconnected' as ConnectionStatus,
  toastQueue: [] as NotificationDto[],
  seenIds: new Set<number>(),
  overflowCount: 0,
});

// Server history and unread counts belong exclusively to TanStack Query.
export const useNotificationStore = create<NotificationState>((set, get) => ({
  ...initialState(),
  bindUser: (userId) => {
    if (get().ownerId !== userId) set({ ...initialState(), ownerId: userId });
  },
  setConnectionStatus: (connectionStatus) => set({ connectionStatus }),
  enqueueToast: (notification) => {
    const state = get();
    if (notification.userID !== state.ownerId || state.seenIds.has(notification.notificationID)) return false;
    const seenIds = new Set(state.seenIds).add(notification.notificationID);
    if (state.toastQueue.length >= MAX_PENDING_TOASTS) {
      // Keep queued cards and offer the authoritative history for overflow.
      set({ seenIds, overflowCount: state.overflowCount + 1 });
      return false;
    }
    set({ seenIds, toastQueue: [...state.toastQueue, notification] });
    return true;
  },
  dequeueToast: (id) => set((state) => ({ toastQueue: state.toastQueue.filter(n => n.notificationID !== id) })),
  clearOverflow: () => set({ overflowCount: 0 }),
  reset: () => set(initialState()),
}));
=======
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
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
