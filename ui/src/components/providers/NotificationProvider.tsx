import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { type ReactNode, useEffect } from 'react';

import { queryKeys } from '@/constants/queryKeys';
import { logger } from '@/lib/logger';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/useNotificationStore';
import type { NotificationDto } from '@/types/notifications';

/** Owned by one connection, including deferred browser interaction. */
class NotificationAudio {
  private audio: HTMLAudioElement | null = null;
  private disposed = false;
  private interaction: (() => void) | null = null;

  play() {
    if (this.disposed || localStorage.getItem('notification_sound') === 'false') return;
    const audio = this.audio ??= new Audio('/notification-sound.mp3');
    audio.currentTime = 0;
    void audio.play().catch((error: unknown) => {
      if (this.disposed || !(error instanceof DOMException) || error.name !== 'NotAllowedError' || this.interaction) return;
      this.interaction = () => {
        this.removeInteraction();
        if (!this.disposed && localStorage.getItem('notification_sound') !== 'false') void audio.play().catch(() => {});
      };
      document.addEventListener('click', this.interaction, { once: true });
    });
  }

  private removeInteraction() {
    if (this.interaction) document.removeEventListener('click', this.interaction);
    this.interaction = null;
  }

  destroy() {
    this.disposed = true;
    this.removeInteraction();
    this.audio?.pause();
    this.audio = null;
  }
}

export function NotificationProvider({ children }: { children: ReactNode }) {
  const userId = useAuthStore(s => s.user?.id ?? null);
  const hasToken = useAuthStore(s => Boolean(s.token));
  const queryClient = useQueryClient();

  useEffect(() => {
    if (userId === null || !hasToken) {
      void queryClient.cancelQueries({ queryKey: queryKeys.notifications.all });
      queryClient.removeQueries({ queryKey: queryKeys.notifications.all });
      useNotificationStore.getState().reset();
      return;
    }

    const store = useNotificationStore.getState();
    store.bindUser(userId);
    store.setConnectionStatus('connecting');
    const audio = new NotificationAudio();
    let disposed = false;
    let retryTimer: ReturnType<typeof setTimeout> | undefined;
    let invalidateTimer: ReturnType<typeof setTimeout> | undefined;
    let retries = 0;
    const queryKey = queryKeys.notifications.byUser(userId);
    // Identity belongs to this closure, never a ref shared with another connection.
    const isCurrent = () => !disposed && useAuthStore.getState().user?.id === userId && Boolean(useAuthStore.getState().token);
    const sync = () => {
      if (!isCurrent()) return;
      if (invalidateTimer) clearTimeout(invalidateTimer);
      invalidateTimer = undefined;
      void queryClient.invalidateQueries({ queryKey });
    };
    const scheduleSync = () => {
      if (!isCurrent() || invalidateTimer) return;
      invalidateTimer = setTimeout(sync, 250);
    };
    const connected = () => {
      if (!isCurrent()) return;
      retries = 0;
      useNotificationStore.getState().setConnectionStatus('connected');
      sync();
    };
    const hubUrl = (import.meta.env.VITE_API_URL || '').replace(/\/api\/?$/, '') + '/hubs/notifications';
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => isCurrent() ? useAuthStore.getState().token ?? '' : '' })
      .withAutomaticReconnect({ nextRetryDelayInMilliseconds: context => context.elapsedMilliseconds < 60000 ? 2000 : 10000 })
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('ReceiveNotification', (notification: NotificationDto) => {
      if (!isCurrent() || notification.userID !== userId) return;
      scheduleSync();
      // Floating cards are the sole surface for server notification events.
      if (useNotificationStore.getState().enqueueToast(notification)) audio.play();
    });
    connection.on('UnreadCountUpdated', scheduleSync);
    connection.onreconnecting(() => {
      if (isCurrent()) useNotificationStore.getState().setConnectionStatus('disconnected');
    });
    connection.onreconnected(connected);
    const start = async () => {
      if (!isCurrent()) return;
      try {
        await connection.start();
        connected();
      } catch (error) {
        if (!isCurrent()) return;
        useNotificationStore.getState().setConnectionStatus('disconnected');
        logger.error('Notification connection unavailable', error, { component: 'NotificationProvider' });
        retryTimer = setTimeout(() => { void start(); }, Math.min(1000 * 2 ** Math.min(retries++, 5), 30000));
      }
    };
    connection.onclose(() => {
      if (!isCurrent()) return;
      useNotificationStore.getState().setConnectionStatus('disconnected');
      retryTimer = setTimeout(() => { void start(); }, 2000);
    });
    void start();

    return () => {
      disposed = true;
      if (retryTimer) clearTimeout(retryTimer);
      if (invalidateTimer) clearTimeout(invalidateTimer);
      audio.destroy();
      void connection.stop().catch(() => {});
      if (useAuthStore.getState().user?.id !== userId || !useAuthStore.getState().token) {
        void queryClient.cancelQueries({ queryKey });
        queryClient.removeQueries({ queryKey });
        if (useNotificationStore.getState().ownerId === userId) useNotificationStore.getState().reset();
      }
    };
  }, [userId, hasToken, queryClient]);

  return children;
}
