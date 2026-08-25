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
    </div>
  );
}
