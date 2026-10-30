<<<<<<< HEAD
import { Building2, DollarSign, FileText, Settings, ShieldAlert, Users, X } from 'lucide-react';
import { useCallback, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';

import { Button } from '@/components/ui';
import { useOpenNotification } from '@/hooks/useOpenNotification';
import { formatYemenRelativeTime } from '@/lib/date';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/useNotificationStore';
import type { NotificationDto } from '@/types/notifications';
import { CATEGORY_LABELS } from '@/types/notifications';

const CATEGORY_CONFIG = {
  WORKFLOW: { icon: FileText, accent: 'bg-info', iconColor: 'text-info', bgColor: 'bg-info/20' },
  FINANCIAL: { icon: DollarSign, accent: 'bg-success', iconColor: 'text-success', bgColor: 'bg-success/20' },
  SECURITY: { icon: ShieldAlert, accent: 'bg-danger', iconColor: 'text-danger', bgColor: 'bg-danger/20' },
  SYSTEM: { icon: Settings, accent: 'bg-muted-foreground', iconColor: 'text-muted-foreground', bgColor: 'bg-muted' },
  HR: { icon: Users, accent: 'bg-warning', iconColor: 'text-warning', bgColor: 'bg-warning/20' },
  PENSIONS: { icon: Building2, accent: 'bg-special', iconColor: 'text-special', bgColor: 'bg-special/20' },
};

const DURATION = 7000;
const EXIT_DURATION = 300;
const MAX_VISIBLE = 3;

function NotificationCard({ notification }: { notification: NotificationDto }) {
  const [remaining, setRemaining] = useState(DURATION);
  const [paused, setPaused] = useState(false);
  const pausedRef = useRef(false);
  const dequeueToast = useNotificationStore(s => s.dequeueToast);
  const openNotification = useOpenNotification();
  const dismiss = useCallback(() => dequeueToast(notification.notificationID), [dequeueToast, notification.notificationID]);
  const pause = (value: boolean) => { pausedRef.current = value; setPaused(value); };

  useEffect(() => {
    let timeLeft = DURATION;
    let lastTime = performance.now();
    let rafId: number;
    const exitDuration = window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : EXIT_DURATION;
    const tick = (now: number) => {
      const delta = now - lastTime;
      if (delta >= 50) {
        lastTime = now;
        if (!pausedRef.current || timeLeft <= 0) {
          timeLeft -= delta;
          setRemaining(Math.max(0, timeLeft));
          if (timeLeft <= -exitDuration) { dismiss(); return; }
        }
      }
      rafId = requestAnimationFrame(tick);
    };
    rafId = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(rafId);
  }, [dismiss]);

  const category = notification.category && notification.category in CATEGORY_CONFIG
    ? notification.category as keyof typeof CATEGORY_CONFIG : 'SYSTEM';
  const config = CATEGORY_CONFIG[category];
  return (
    <article
      role="status"
      className={`pointer-events-auto relative overflow-hidden rounded-xl border border-border bg-card transition-all duration-slow motion-reduce:transition-none
        ${remaining === 0 ? 'opacity-0 translate-x-8 scale-95' : 'opacity-100 translate-x-0 scale-100'}`}
      onMouseEnter={() => pause(true)}
      onMouseLeave={e => pause(e.currentTarget.contains(document.activeElement))}
      onFocusCapture={() => pause(true)}
      onBlurCapture={e => { if (!e.currentTarget.contains(e.relatedTarget)) pause(e.currentTarget.matches(':hover')); }}
    >
      <button
        type="button"
        className="flex w-full items-start gap-3 p-4 pe-14 text-start focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring"
        onClick={() => { openNotification(notification); dismiss(); }}
        aria-label={`فتح الإشعار: ${notification.title}`}
      >
        <span className={`shrink-0 flex h-10 w-10 items-center justify-center rounded-full ${config.bgColor}`}>
          <config.icon className={`h-5 w-5 ${config.iconColor}`} aria-hidden="true" />
        </span>
        <span className="min-w-0 flex-1 py-0.5">
          <span className="mb-1 flex flex-wrap items-center gap-2">
            <h4 className="min-w-0 break-words text-sm font-bold text-card-foreground">{notification.title}</h4>
            <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${config.bgColor} ${config.iconColor}`}>
              {CATEGORY_LABELS[category]}
            </span>
          </span>
          <span className="block break-words text-xs leading-relaxed text-card-foreground line-clamp-2">{notification.message}</span>
          <span className="mt-1.5 block text-xs text-muted-foreground">{formatYemenRelativeTime(notification.createdAt)}</span>
        </span>
      </button>
      <Button variant="ghost" size="icon" className="absolute end-1 top-2 h-10 w-10" onClick={dismiss} aria-label="إخفاء الإشعار">
        <X className="h-4 w-4 text-muted-foreground" />
      </Button>
      <div className="absolute inset-x-0 bottom-0 h-1 bg-muted" aria-hidden="true">
        <div className={`h-full transition-[width] duration-fast ease-linear motion-reduce:transition-none ${config.accent} ${paused ? 'opacity-100' : ''}`}
          style={{ width: `${remaining / DURATION * 100}%` }} />
      </div>
    </article>
  );
}

export function FloatingNotification() {
  const ownerId = useNotificationStore(s => s.ownerId);
  const userId = useAuthStore(s => s.user?.id ?? null);
  const toastQueue = useNotificationStore(s => s.toastQueue);
  const overflowCount = useNotificationStore(s => s.overflowCount);
  const clearOverflow = useNotificationStore(s => s.clearOverflow);
  if (ownerId === null || ownerId !== userId) return null;
  return (
    <div className="pointer-events-none fixed end-4 top-20 z-notification flex w-[calc(100%-2rem)] max-w-[380px] flex-col gap-3">
      {toastQueue.slice(0, MAX_VISIBLE).map(notification => (
        <NotificationCard key={`${ownerId}:${notification.notificationID}`} notification={notification} />
      ))}
      {overflowCount > 0 && (
        <Link to="/notifications" onClick={clearOverflow} className="pointer-events-auto rounded-lg border border-border bg-card p-3 text-sm text-foreground">
          هناك {overflowCount} إشعارات إضافية في سجل الإشعارات
        </Link>
      )}
=======
import { useEffect, useRef, useState, type MouseEvent } from 'react';
import { useNavigate } from 'react-router-dom';

import { Building2, DollarSign, FileText, Settings, ShieldAlert, Users, X } from 'lucide-react';

import { Button } from "@/components/ui";
import { useNotificationStore } from '@/stores/useNotificationStore';
import { CATEGORY_LABELS } from '@/types/notifications';
import type { NotificationDto } from '@/types/notifications';

interface FloatingItem {
  notification: NotificationDto;
  remaining: number;
  paused: boolean;
  exiting: boolean;
}

const CATEGORY_CONFIG: Record<string, { icon: React.ElementType; gradient: string; accent: string; iconColor: string; bgColor: string }> = {
  WORKFLOW:  { icon: FileText, gradient: 'from-blue-500/20 to-transparent', accent: 'bg-blue-500', iconColor: 'text-blue-400', bgColor: 'bg-blue-500/20' },
  FINANCIAL: { icon: DollarSign, gradient: 'from-emerald-500/20 to-transparent', accent: 'bg-emerald-500', iconColor: 'text-emerald-400', bgColor: 'bg-emerald-500/20' },
  SECURITY:  { icon: ShieldAlert, gradient: 'from-red-500/20 to-transparent', accent: 'bg-red-500', iconColor: 'text-red-400', bgColor: 'bg-red-500/20' },
  SYSTEM:    { icon: Settings, gradient: 'from-slate-500/20 to-transparent', accent: 'bg-slate-500', iconColor: 'text-slate-400', bgColor: 'bg-slate-500/20' },
  HR:        { icon: Users, gradient: 'from-amber-500/20 to-transparent', accent: 'bg-amber-500', iconColor: 'text-amber-400', bgColor: 'bg-amber-500/20' },
  PENSIONS:  { icon: Building2, gradient: 'from-violet-500/20 to-transparent', accent: 'bg-violet-500', iconColor: 'text-violet-400', bgColor: 'bg-violet-500/20' },
};

const DURATION = 7000;
const MAX_VISIBLE = 3;

function timeAgo(dateStr: string): string {
  const normalizedStr = dateStr.endsWith('Z') ? dateStr : `${dateStr}Z`;
  const diff = Date.now() - new Date(normalizedStr).getTime();
  const seconds = Math.floor(diff / 1000);
  if (seconds < 10) return 'الآن';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 1) return 'منذ لحظات';
  if (minutes === 1) return 'منذ دقيقة';
  if (minutes === 2) return 'منذ دقيقتين';
  if (minutes < 11) return `منذ ${minutes} دقائق`;
  if (minutes < 60) return `منذ ${minutes} دقيقة`;
  const hours = Math.floor(minutes / 60);
  if (hours === 1) return 'منذ ساعة';
  if (hours === 2) return 'منذ ساعتين';
  if (hours < 11) return `منذ ${hours} ساعات`;
  return `منذ ${hours} ساعة`;
}

export function FloatingNotification() {
  const [items, setItems] = useState<FloatingItem[]>([]);
  const lastIdRef = useRef<number | null>(null);
  const notifications = useNotificationStore((s) => s.notifications);
  const navigate = useNavigate();

  useEffect(() => {
    if (notifications.length === 0) return;
    const latest = notifications[0];
    if (!latest?.notificationID || latest.notificationID === lastIdRef.current) return;
    lastIdRef.current = latest.notificationID;

    setItems((prev) => {
      const next: FloatingItem[] = [
        { notification: latest, remaining: DURATION, paused: false, exiting: false },
        ...prev,
      ];
      return next.slice(0, MAX_VISIBLE);
    });
  }, [notifications]);

  useEffect(() => {
    if (items.length === 0) return;

    let rafId: number;
    let lastTime = performance.now();

    const tick = (now: number) => {
      const delta = now - lastTime;
      // Only update every ~50ms worth of time, but via rAF (battery/CPU friendly)
      if (delta >= 50) {
        lastTime = now;
        setItems((prev) => {
          let changed = false;
          const next = prev
            .map((item) => {
              if (item.paused || item.exiting) return item;
              const updated = { ...item, remaining: item.remaining - delta };
              if (updated.remaining <= 0) {
                changed = true;
                return { ...updated, remaining: 0, exiting: true };
              }
              return updated;
            })
            .filter((item) => {
              if (item.exiting && item.remaining <= -300) return false;
              return true;
            });
          if (!changed && next.length === prev.length) return prev;
          return next;
        });
      }
      rafId = requestAnimationFrame(tick);
    };

    rafId = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(rafId);
  }, [items.length]);

  const dismiss = (id: number) => {
    setItems((prev) => prev.filter((i) => i.notification.notificationID !== id));
  };

  return (
    <div className="fixed end-4 top-20 z-notification flex flex-col gap-3 w-full max-w-[380px] pointer-events-none">
      {items.map((item) => {
        const { notification, remaining, paused, exiting } = item;
        const progress = Math.max(0, (remaining / DURATION) * 100);
        const category = notification.category && CATEGORY_CONFIG[notification.category]
          ? notification.category
          : 'SYSTEM';
        const config = CATEGORY_CONFIG[category];

        return (
          <div
            key={notification.notificationID}
            className={`pointer-events-auto relative overflow-hidden rounded-xl border border-white/10 bg-slate-950/95 transition-all duration-500 cursor-pointer group
              ${exiting ? 'opacity-0 translate-x-8 scale-95' : 'opacity-100 translate-x-0 scale-100'}
            `}
            style={{ animation: 'none' }}
            onClick={() => {
              if (notification.actionUrl) navigate(notification.actionUrl);
              dismiss(notification.notificationID);
            }}
            onMouseEnter={() =>
              setItems((prev) =>
                prev.map((i) =>
                  i.notification.notificationID === notification.notificationID
                    ? { ...i, paused: true }
                    : i
                )
              )
            }
            onMouseLeave={() =>
              setItems((prev) =>
                prev.map((i) =>
                  i.notification.notificationID === notification.notificationID
                    ? { ...i, paused: false }
                    : i
                )
              )
            }
          >
            {/* Animated gradient background */}
            <div className={`absolute inset-0 bg-gradient-to-br ${config.gradient} opacity-50`} />

            {/* Accent start border */}
            <div className={`absolute start-0 top-0 bottom-0 w-1 ${config.accent}`} />

            {/* Progress bar at bottom */}
            <div className="absolute bottom-0 inset-x-0 h-1 bg-white/5">
              <div
                className={`h-full transition-all duration-[50ms] ease-linear ${config.accent} ${paused ? 'opacity-50' : 'opacity-100'}`}
                style={{ width: `${progress}%` }}
              />
            </div>

            <div className="relative flex items-start gap-4 p-4 ps-5">
              {/* Icon */}
              <div className={`shrink-0 w-10 h-10 rounded-full flex items-center justify-center ${config.bgColor}`}>
                <config.icon className={`w-5 h-5 ${config.iconColor}`} />
              </div>

              {/* Content */}
              <div className="flex-1 min-w-0 py-0.5">
                <div className="flex items-center gap-2 mb-0.5">
                  <h4 className="text-sm font-bold text-white truncate">
                    {notification.title}
                  </h4>
                  <span className={`shrink-0 text-[10px] sm:text-xs font-semibold px-2 py-0.5 rounded-full ${config.accent} text-white`}>
                    {CATEGORY_LABELS[category] || category}
                  </span>
                </div>
                {notification.message && (
                  <p className="text-xs text-white leading-relaxed line-clamp-2">
                    {notification.message}
                  </p>
                )}
                <p className="text-[10px] sm:text-xs text-white/70 mt-1.5 flex items-center gap-1.5">
                  <span className="inline-block w-1 h-1 rounded-full bg-slate-600" />
                  {timeAgo(notification.createdAt)}
                </p>
              </div>

              {/* Close button */}
              <Button
                variant="ghost"
                size="icon"
                className="shrink-0 opacity-0 group-hover:opacity-100 transition-all duration-200 hover:bg-white/10 active:scale-90 h-10 w-10"
                onClick={(e: MouseEvent) => {
                  e.stopPropagation();
                  dismiss(notification.notificationID);
                }}
                aria-label="إخفاء الإشعار"
              >
                <X className="w-4 h-4 text-slate-400" />
              </Button>
            </div>
          </div>
        );
      })}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    </div>
  );
}
