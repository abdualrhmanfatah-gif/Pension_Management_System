<<<<<<< HEAD
import { Bell, CheckCheck, Clock, ExternalLink, Inbox } from "lucide-react";
import { useCallback,useMemo } from "react";
import { useNavigate } from "react-router-dom";

=======
import { useMemo, useCallback } from "react";
import { Bell, CheckCheck, Clock, ExternalLink, Inbox } from "lucide-react";
import { useNavigate } from "react-router-dom";

import { notificationsApi } from "@/api/notifications";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Badge,
  Button,
  EmptyState,
  Popover,
  PopoverContent,
  PopoverTrigger,
  ScrollArea,
  Separator,
} from "@/components/ui";
<<<<<<< HEAD
import { useMarkAllAsRead, useNotificationsList, useUnreadCount } from "@/hooks/useNotifications";
import { useOpenNotification } from "@/hooks/useOpenNotification";
import { formatDate } from "@/lib/utils";
import { cn } from "@/lib/utils";
=======
import { formatDate } from "@/lib/utils";
import { cn } from "@/lib/utils";
import { useNotificationStore } from "@/stores/useNotificationStore";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type { NotificationDto } from "@/types/notifications";
import { CATEGORY_COLORS, CATEGORY_LABELS } from "@/types/notifications";

export function NotificationBell() {
<<<<<<< HEAD
  const navigate = useNavigate();
  // NF2: القائمة والعدّاد من الاستعلامات المشتركة — لا ملكية موازية في Zustand
  const { data } = useNotificationsList(1, 20);
  const { data: unreadData } = useUnreadCount();

  const markAllAsReadMutation = useMarkAllAsRead();
  const handleNotificationClick = useOpenNotification();

  const notifications = useMemo(() => {
    if (!data?.isSuccess) return [];
    const d = data.data;
    return Array.isArray(d) ? d : (d.items ?? []);
  }, [data]);

  const unreadCount = unreadData?.isSuccess ? unreadData.data : 0;
=======
  const notifications = useNotificationStore((s) => s.notifications);
  const unreadCount = useNotificationStore((s) => s.unreadCount);
  const markAsRead = useNotificationStore((s) => s.markAsRead);
  const setUnreadCount = useNotificationStore((s) => s.setUnreadCount);
  const setNotifications = useNotificationStore((s) => s.setNotifications);
  const navigate = useNavigate();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

  // تحسين الأداء: تجميع الإشعارات المقروءة وغير المقروءة
  const { unreadNotifications, readNotifications } = useMemo(() => {
    return {
      unreadNotifications: notifications.filter((n) => !n.isRead),
      readNotifications: notifications.filter((n) => n.isRead),
    };
  }, [notifications]);

<<<<<<< HEAD
  const handleMarkAllAsRead = useCallback(() => {
    markAllAsReadMutation.mutate(undefined);
  }, [markAllAsReadMutation]);
=======
  const handleMarkAsRead = useCallback(
    async (id: number) => {
      const res = await notificationsApi.markAsRead(id);
      if (res.isSuccess) {
        markAsRead(id);
      }
    },
    [markAsRead]
  );

  const handleMarkAllAsRead = useCallback(async () => {
    const res = await notificationsApi.markAllAsRead();
    if (res.isSuccess) {
      setUnreadCount(0);
      setNotifications(notifications.map((n) => ({ ...n, isRead: true })));
    }
  }, [notifications, setNotifications, setUnreadCount]);

  const handleNotificationClick = useCallback(
    (n: NotificationDto) => {
      if (!n.isRead) {
        handleMarkAsRead(n.notificationID);
      }

      switch (n.category) {
        case "WORKFLOW":
          if (n.relatedRecordID) navigate(`/files/${n.relatedRecordID}`);
          break;
        case "FINANCIAL":
          if (n.relatedRecordID) {
            if (n.title.includes("تسوية")) {
              navigate(`/settlements/${n.relatedRecordID}`);
            } else if (n.title.includes("رواتب")) {
              navigate(`/salaries/${n.relatedRecordID}`);
            } else {
              navigate("/salaries");
            }
          }
          break;
        case "HR":
          navigate(`/hr/leaves`);
          break;
        case "SECURITY":
          navigate(`/admin/audit-logs`);
          break;
      }
    },
    [navigate, handleMarkAsRead]
  );
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

  const NotificationItem = useCallback(
    ({ n }: { n: NotificationDto }) => (
      <div
        key={n.notificationID}
        onClick={() => handleNotificationClick(n)}
        onKeyDown={(e: React.KeyboardEvent) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleNotificationClick(n); } }}
        role="button"
        tabIndex={0}
        className={cn(
<<<<<<< HEAD
          "group relative flex flex-col gap-2 border-b border-border p-4 transition-all duration-200 focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset",
=======
          "group relative flex flex-col gap-2 border-b border-border/50 p-4 transition-all duration-200 focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
          "hover:bg-accent/50 cursor-pointer text-start",
          !n.isRead && "bg-primary/[0.03] hover:bg-primary/[0.06]"
        )}
      >
        {/* مؤشر الإشعار غير المقروء */}
        {!n.isRead && (
          <span className="absolute top-4 right-3 h-2 w-2 rounded-full bg-primary animate-pulse" />
        )}

        <div className="flex items-start justify-between gap-2 pe-4">
          <p
            className={cn(
              "text-sm font-medium leading-tight line-clamp-1",
              !n.isRead ? "text-foreground" : "text-muted-foreground"
            )}
          >
            {n.title}
          </p>
          {n.category && CATEGORY_LABELS[n.category] && (
            <Badge
              variant={CATEGORY_COLORS[n.category] ?? "secondary"}
              className="shrink-0 h-5 text-[10px] sm:text-xs leading-none px-2 font-medium"
            >
              {CATEGORY_LABELS[n.category]}
            </Badge>
          )}
        </div>

        <p
          className="text-xs text-muted-foreground line-clamp-2 leading-relaxed"
          dir="rtl"
        >
          {n.message}
        </p>

        <div className="flex items-center justify-between mt-1">
<<<<<<< HEAD
          <div className="flex items-center gap-1.5 text-[11px] text-muted-foreground">
=======
          <div className="flex items-center gap-1.5 text-[11px] text-muted-foreground/80">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            <Clock className="h-3 w-3" />
            <span>{formatDate(n.createdAt.endsWith('Z') ? n.createdAt : `${n.createdAt}Z`, true)}</span>
          </div>
          {n.relatedRecordID && (
            <div className="flex items-center gap-1 text-[11px] text-primary opacity-0 group-hover:opacity-100 transition-opacity duration-200">
              <span>عرض التفاصيل</span>
              <ExternalLink className="h-3 w-3" />
            </div>
          )}
        </div>
      </div>
    ),
    [handleNotificationClick]
  );

  const isEmpty = notifications.length === 0;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="relative text-muted-foreground hover:text-foreground transition-colors"
        >
          <Bell className="h-5 w-5" />
          {unreadCount > 0 && (
            <span
              className={cn(
                "absolute -top-1 -end-1 flex min-w-[20px] h-5 items-center justify-center",
                "rounded-full bg-destructive text-[10px] sm:text-xs font-bold text-white",
                "px-1.5 border-2 border-background",
                "animate-in zoom-in duration-200"
              )}
            >
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Button>
      </PopoverTrigger>

      <PopoverContent
        align="end"
<<<<<<< HEAD
        className="w-full sm:w-96 max-w-sm p-0 overflow-hidden border-border"
        dir="rtl"
      >
        {/* الهيدر */}
        <div className="flex items-center justify-between border-b border-border p-4 bg-muted/40 backdrop-blur-sm">
=======
        className="w-full sm:w-96 max-w-sm p-0 overflow-hidden border-border/50"
        dir="rtl"
      >
        {/* الهيدر */}
        <div className="flex items-center justify-between border-b border-border/50 p-4 bg-muted/40 backdrop-blur-sm">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
          <div className="flex items-center gap-2">
            <h3 className="text-sm font-semibold">الإشعارات</h3>
            {unreadCount > 0 && (
              <Badge
                variant="destructive"
                className="h-5 text-[10px] sm:text-xs px-2 font-medium"
              >
                {unreadCount} جديد
              </Badge>
            )}
          </div>

          {unreadCount > 0 && (
            <Button
              variant="ghost"
              size="sm"
              onClick={handleMarkAllAsRead}
              className="text-xs text-primary hover:bg-primary/10 hover:text-primary gap-1.5"
            >
              <CheckCheck className="h-3.5 w-3.5" />
              تحديد الكل كمقروء
            </Button>
          )}
        </div>

        {/* محتوى الإشعارات */}
        <ScrollArea className="max-h-[60dvh]" dir="rtl">
          {isEmpty ? (
            <div className="p-4">
              <EmptyState icon={Inbox} title="لا توجد إشعارات" description="ستظهر هنا الإشعارات الجديدة" />
            </div>
          ) : (
            <div className="flex flex-col">
              {/* الإشعارات غير المقروءة أولاً */}
              {unreadNotifications.length > 0 && (
                <>
<<<<<<< HEAD
                  <div className="px-4 py-2 text-[10px] sm:text-xs font-semibold text-muted-foreground uppercase tracking-wider bg-muted/30">
=======
                  <div className="px-4 py-2 text-[10px] sm:text-xs font-semibold text-muted-foreground/70 uppercase tracking-wider bg-muted/30">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    جديد
                  </div>
                  {unreadNotifications.map((n) => (
                    <NotificationItem key={n.notificationID} n={n} />
                  ))}
                </>
              )}

              {/* الإشعارات المقروءة */}
              {readNotifications.length > 0 && (
                <>
                  {unreadNotifications.length > 0 && (
<<<<<<< HEAD
                    <div className="px-4 py-2 text-[10px] sm:text-xs font-semibold text-muted-foreground uppercase tracking-wider bg-muted/30">
=======
                    <div className="px-4 py-2 text-[10px] sm:text-xs font-semibold text-muted-foreground/70 uppercase tracking-wider bg-muted/30">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                      سابق
                    </div>
                  )}
                  {readNotifications.map((n) => (
                    <NotificationItem key={n.notificationID} n={n} />
                  ))}
                </>
              )}
            </div>
          )}
        </ScrollArea>

        {/* الفوتر */}
        {!isEmpty && (
          <>
            <Separator />
            <div className="p-2">
              <Button
                variant="ghost"
                size="sm"
                className="w-full text-xs text-muted-foreground hover:text-foreground h-9"
                onClick={() => navigate("/notifications")}
              >
                عرض جميع الإشعارات
              </Button>
            </div>
          </>
        )}
      </PopoverContent>
    </Popover>
  );
<<<<<<< HEAD
}
=======
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
