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
    </div>
  );
}
