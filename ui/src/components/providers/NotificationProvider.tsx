import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import React, { useEffect, useRef, useCallback, useMemo } from 'react';
import { toast } from 'sonner';

import { notificationsApi } from '@/api/notifications';
import { logger } from '@/lib/logger';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/useNotificationStore';

import { NotificationContext } from './useSignalR';

// ─── Constants ───────────────────────────────────────────

const MAX_RETRIES = 5;
const INITIAL_RETRY_DELAY = 1000;
const MAX_RETRY_DELAY = 30000;
const SOUND_PATH = '/notification-sound.mp3';

// ─── Audio Manager (singleton pattern) ───────────────────

class AudioManager {
  private static instance: AudioManager;
  private audio: HTMLAudioElement | null = null;
  private pendingPlay = false;
  private interactionHandler: (() => void) | null = null;

  private constructor() {}

  static getInstance(): AudioManager {
    if (!AudioManager.instance) {
      AudioManager.instance = new AudioManager();
    }
    return AudioManager.instance;
  }

  private ensureLoaded() {
    if (!this.audio) {
      this.audio = new Audio(SOUND_PATH);
      this.audio.preload = 'auto';
    }
  }

  play() {
    const soundEnabled = localStorage.getItem('notification_sound') !== 'false';
    if (!soundEnabled) return;

    this.ensureLoaded();
    if (!this.audio) return;

    this.pendingPlay = true;
    this.tryPlay();
  }

  private tryPlay() {
    if (!this.pendingPlay || !this.audio) return;

    this.audio.currentTime = 0;
    const playPromise = this.audio.play();

    if (playPromise !== undefined) {
      playPromise
        .then(() => {
          this.pendingPlay = false;
        })
        .catch((err) => {
          if (err.name === 'NotAllowedError' && !this.interactionHandler) {
            this.interactionHandler = () => {
              this.audio!.play().catch(() => {});
              this.cleanup();
            };
            document.addEventListener('click', this.interactionHandler, { once: true });
          }
        });
    }
  }

  private cleanup() {
    if (this.interactionHandler) {
      document.removeEventListener('click', this.interactionHandler);
      this.interactionHandler = null;
    }
    this.pendingPlay = false;
  }

  destroy() {
    this.cleanup();
    this.audio = null;
  }
}

// ─── Connection Manager ───────────────────────────────────

class SignalRConnectionManager {
  private connection: HubConnection | null = null;
  private retryCount = 0;
  private retryTimeout: ReturnType<typeof setTimeout> | null = null;
  private isDisposed = false;

  private readonly onNotification: (notification: any) => void;
  private readonly onUnreadCount: (count: number) => void;
  private readonly onConnected: () => void;
  private readonly onError: (err: any) => void;

  constructor(
    onNotification: (notification: any) => void,
    onUnreadCount: (count: number) => void,
    onConnected: () => void,
    onError: (err: any) => void
  ) {
    this.onNotification = onNotification;
    this.onUnreadCount = onUnreadCount;
    this.onConnected = onConnected;
    this.onError = onError;
  }

  async start() {
    if (this.isDisposed) return;

    const token = useAuthStore.getState().token;
    if (!token) return;

    const baseUrl = import.meta.env.VITE_API_URL || '';
    const hubUrl = baseUrl.replace(/\/api\/?$/, '') + '/hubs/notifications';

    this.connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: async () => useAuthStore.getState().token || ''
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.elapsedMilliseconds < 60000) return 2000;
          return 10000;
        }
      })
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('ReceiveNotification', this.onNotification);
    this.connection.on('UnreadCountUpdated', this.onUnreadCount);

    this.connection.onreconnecting(() => {
      logger.warn('SignalR reconnecting...', { component: 'NotificationProvider' });
    });

    this.connection.onreconnected(() => {
      logger.info('SignalR reconnected', { component: 'NotificationProvider' });
      this.retryCount = 0;
    });

    this.connection.onclose(() => {
      if (!this.isDisposed) {
        this.scheduleRetry();
      }
    });

    await this.tryStart();
  }

  private async tryStart() {
    if (this.isDisposed || !this.connection) return;

    try {
      await this.connection.start();
      if (!this.isDisposed) {
        logger.info('SignalR Connected', { component: 'NotificationProvider' });
        this.retryCount = 0;
        this.onConnected();
      }
    } catch (err) {
      if (!this.isDisposed) {
        this.onError(err);
        this.scheduleRetry();
      }
    }
  }

  private scheduleRetry() {
    if (this.retryCount >= MAX_RETRIES) {
      logger.error('SignalR max retries reached', { component: 'NotificationProvider' });
      return;
    }

    this.retryCount++;
    const delay = Math.min(INITIAL_RETRY_DELAY * Math.pow(2, this.retryCount), MAX_RETRY_DELAY);
    
    logger.warn(`SignalR retrying in ${delay}ms`, { 
      attempt: this.retryCount, 
      component: 'NotificationProvider' 
    });

    this.retryTimeout = setTimeout(() => this.tryStart(), delay);
  }

  stop() {
    this.isDisposed = true;
    
    if (this.retryTimeout) {
      clearTimeout(this.retryTimeout);
      this.retryTimeout = null;
    }

    if (this.connection) {
      this.connection.stop().catch(() => {});
      this.connection = null;
    }
  }
}

// ─── Provider Component ─────────────────────────────────

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const user = useAuthStore((s) => s.user);
  const addNotification = useNotificationStore((s) => s.addNotification);
  const setUnreadCount = useNotificationStore((s) => s.setUnreadCount);
  const setNotifications = useNotificationStore((s) => s.setNotifications);
  const queryClient = useQueryClient();
  
  const connectionRef = useRef<SignalRConnectionManager | null>(null);
  const audioManager = useMemo(() => AudioManager.getInstance(), []);

  // ── جلب الإشعارات الأولية (عند تسجيل الدخول فقط) ─────────────────────────
  useEffect(() => {
    if (!user) return;

    // التحقق من علم "تسجيل دخول جديد" في sessionStorage
    const isFreshLogin = sessionStorage.getItem('fresh_login') === 'true';
    if (!isFreshLogin) return; // تجاهل التحميل إذا كان استعادة جلسة (refresh)

    // مسح العلم لتجنب التكرار في نفس الجلسة
    sessionStorage.removeItem('fresh_login');

    let cancelled = false;

    const loadInitialData = async () => {
      try {
        const [countRes, notificationsRes] = await Promise.all([
          notificationsApi.getUnreadCount(),
          notificationsApi.getAll(1, 10)
        ]);

        if (cancelled) return;

        if (countRes.isSuccess) {
          setUnreadCount(countRes.data);
        }

        if (notificationsRes.isSuccess) {
          const items = Array.isArray(notificationsRes.data) 
            ? notificationsRes.data 
            : notificationsRes.data.items ?? [];
          setNotifications(items);
        }
      } catch (err) {
        logger.error('Failed to load initial notifications', err, { component: 'NotificationProvider' });
      }
    };

    loadInitialData();

    return () => {
      cancelled = true;
    };
  }, [user, setUnreadCount, setNotifications]);

  // ── معالجة الإشعارات الواردة ───────────────────────
  const handleNotification = useCallback((notification: any) => {
    addNotification(notification);

    const isError = notification.type === 'ERROR' || notification.priority > 2;
    if (isError) {
      toast.error(notification.title, { description: notification.message });
    } else {
      toast.info(notification.title, { description: notification.message });
    }

    queryClient.invalidateQueries({ queryKey: ['notifications'] });
    audioManager.play();
  }, [addNotification, queryClient, audioManager]);

  const handleUnreadCount = useCallback((count: number) => {
    setUnreadCount(count);
    queryClient.invalidateQueries({ queryKey: ['notifications', 'unread-count'] });
  }, [setUnreadCount, queryClient]);

  // ── إدارة اتصال SignalR ────────────────────────────
  useEffect(() => {
    if (!user) {
      connectionRef.current?.stop();
      connectionRef.current = null;
      return;
    }

    // Defer SignalR connection by 5s after mount
    const startTimer = setTimeout(() => {
      const manager = new SignalRConnectionManager(
        handleNotification,
        handleUnreadCount,
        () => {},
        (err) => logger.error('SignalR error', err, { component: 'NotificationProvider' })
      );

      connectionRef.current = manager;
      manager.start();
    }, 5000);

    return () => {
      clearTimeout(startTimer);
      connectionRef.current?.stop();
      connectionRef.current = null;
    };
  }, [user, handleNotification, handleUnreadCount]);

  return (
    <NotificationContext.Provider value={null}>
      {children}
    </NotificationContext.Provider>
  );
};