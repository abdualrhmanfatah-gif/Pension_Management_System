import { create } from 'zustand';

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