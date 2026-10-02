import { useQuery, useQueryClient } from "@tanstack/react-query";
import { formatDistanceToNowStrict } from "date-fns";
import { format } from "date-fns";
import { ar } from "date-fns/locale";
import { Bell, CheckCheck, ChevronLeft, ChevronRight, Clock, Download, Inbox, Paperclip, Volume2, VolumeX } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { notificationsApi } from "@/api/notifications";
import { Badge, Button, Card, EmptyState, ErrorState, Heading, PageLayout, Text } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { cn } from "@/lib/utils";
import { useNotificationStore } from "@/stores/useNotificationStore";
import type { NotificationAttachmentDto,NotificationDto } from "@/types/notifications";
import { CATEGORY_COLORS, CATEGORY_LABELS } from "@/types/notifications";

const CATEGORIES = ["", "WORKFLOW", "FINANCIAL", "SECURITY", "SYSTEM", "HR", "PENSIONS"] as const;

export function NotificationsPage() {
    const queryClient = useQueryClient();
    const markStoreRead = useNotificationStore((s) => s.markAsRead);
    const setUnreadCount = useNotificationStore((s) => s.setUnreadCount);
    const storeUnreadCount = useNotificationStore((s) => s.unreadCount);
    
    const [category, setCategory] = useState("");
    const [page, setPage] = useState(1);
    const [soundEnabled, setSoundEnabled] = useState(() => { try { return localStorage.getItem('notification_sound') !== 'false'; } catch { return true; } });
    const pageSize = 20;

    const { data, isLoading, error } = useQuery({
        queryKey: ["notifications", "all", page, category],
        queryFn: () => notificationsApi.getAll(page, pageSize, category || undefined),
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل الإشعارات", { id: "notifications" });
    }, [error]);

    const markAllAsReadMutation = useApiMutation(
        () => notificationsApi.markAllAsRead(),
        {
            successMessage: "تم تحديد جميع الإشعارات كمقروءة",
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: ["notifications"] });
                setUnreadCount(0);
            }
        }
    );

    const markAsReadMutation = useApiMutation(
        (id: number) => notificationsApi.markAsRead(id),
        {
            onSuccess: (_, id) => {
                queryClient.invalidateQueries({ queryKey: ["notifications"] });
                markStoreRead(id);
            }
        }
    );

    const result = data?.data;
    const notifications = result?.items ?? [];
    const totalPages = result?.totalPages ?? 0;
    const totalCount = result?.totalCount ?? 0;

    const toggleSound = () => {
        const next = !soundEnabled;
        setSoundEnabled(next);
        try { localStorage.setItem('notification_sound', String(next)); } catch { };
    };

    const handleCategoryChange = (cat: string) => {
        setCategory(cat);
        setPage(1);
    };

    return (
        <PageLayout maxWidth="max-w-5xl" className="flex-1">
            <div className="flex flex-col gap-6 w-full mx-auto">
                <div className="flex items-center justify-between">
                    <div>
                        <Heading level="h1">الإشعارات</Heading>
                        <Text variant="description" className="mt-1">
                            {storeUnreadCount > 0 ? (
                                <span className="text-primary font-medium">لديك {storeUnreadCount} إشعارات جديدة</span>
                            ) : (
                                "عرض جميع إشعارات وتنبيهات النظام الخاصة بك."
                            )}
                        </Text>
                    </div>
                    <div className="flex items-center gap-2">
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={toggleSound}
                            className="text-muted-foreground"
                            title={soundEnabled ? "كتم الصوت" : "تفعيل الصوت"}
                        >
                            {soundEnabled ? <Volume2 className="w-4 h-4" /> : <VolumeX className="w-4 h-4" />}
                        </Button>
                        <Button 
                            variant="outline" 
                            onClick={() => markAllAsReadMutation.mutate(undefined)}
                            disabled={!notifications.some(n => !n.isRead) || markAllAsReadMutation.isPending}
                            className="gap-2 "
                        >
                            <CheckCheck className="w-4 h-4" />
                            تحديد الكل كمقروء
                        </Button>
                    </div>
                </div>

                <div className="flex gap-2 flex-wrap">
                    {CATEGORIES.map((cat) => (
                        <Button
                            key={cat}
                            variant={category === cat ? "primary" : "outline"}
                            size="sm"
                            onClick={() => handleCategoryChange(cat)}
                            className="gap-1.5"
                        >
                            {cat ? CATEGORY_LABELS[cat] ?? cat : "الكل"}
                        </Button>
                    ))}
                </div>

                <Card className="border-border overflow-hidden">
                    <div className="flex flex-col divide-y divide-border/60">
                        {isLoading ? (
                            <div className="p-6 sm:p-16 text-center text-muted-foreground flex flex-col items-center justify-center">
                                <Bell className="w-12 h-12 mb-4 animate-pulse opacity-20" />
                                <p className="font-medium">جاري تحميل الإشعارات...</p>
                            </div>
                        ) : error ? (
                            <div className="p-6 sm:p-20">
                                <ErrorState message="تعذر تحميل الإشعارات" />
                            </div>
                        ) : notifications.length === 0 ? (
                            <div className="p-6 sm:p-20">
                                <EmptyState icon={Inbox} title="لا توجد إشعارات" description="لم تتلقَ أي إشعارات جديدة حتى الآن." />
                            </div>
                        ) : (
                            notifications.map((n: NotificationDto) => (
                                <div 
                                    key={n.notificationID}
                                    onClick={() => !n.isRead && markAsReadMutation.mutate(n.notificationID)}
                                    onKeyDown={(e: React.KeyboardEvent) => { if (!n.isRead && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); markAsReadMutation.mutate(n.notificationID); } }}
                                    role={!n.isRead ? "button" : undefined}
                                    tabIndex={!n.isRead ? 0 : undefined}
                                    className={cn(
                                        "p-5 sm:p-6 flex flex-col sm:flex-row sm:items-start gap-4 transition-all duration-200 hover:bg-muted/40 group",
                                        !n.isRead ? "bg-primary/[0.04] cursor-pointer focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset" : ""
                                    )}
                                >
                                    <div className={cn(
                                        "w-12 h-12 rounded-full flex items-center justify-center shrink-0 mt-1  border",
                                        !n.isRead ? "bg-background border-primary/20 text-primary" : "bg-muted border-border text-muted-foreground"
                                    )}>
                                        <Bell className="w-5 h-5" />
                                    </div>
                                    <div className="flex-1 space-y-2">
                                        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-2 sm:gap-4">
                                            <div className="flex items-center gap-2.5 flex-wrap">
                                                <h4 className={cn(
                                                    "text-base font-semibold leading-tight",
                                                    !n.isRead ? "text-foreground" : "text-foreground/80"
                                                )}>
                                                    {n.title}
                                                </h4>
                                                {!n.isRead && (
                                                    <Badge variant="info" className="h-5 px-2 text-[10px] sm:text-xs font-bold tracking-wider rounded-md ">جديد</Badge>
                                                )}
                                                {n.category && CATEGORY_LABELS[n.category] && (
                                                    <Badge variant={CATEGORY_COLORS[n.category] ?? "ghost"} className="h-5 text-[10px] sm:text-xs leading-none px-1.5">
                                                        {CATEGORY_LABELS[n.category]}
                                                    </Badge>
                                                )}
                                            </div>
                                             <span className="text-xs text-muted-foreground whitespace-nowrap flex items-center gap-1.5 font-medium shrink-0">
                                                 <Clock className="w-3.5 h-3.5 opacity-70" />
                                                 {(() => {
                                                     const dateString = n.createdAt.endsWith('Z') ? n.createdAt : `${n.createdAt}Z`;
                                                     const dateObj = new Date(dateString);
                                                     const absTime = format(dateObj, "d MMMM yyyy, h:mm a", { locale: ar });
                                                     return (
                                                         <span title={absTime} className="cursor-help">
                                                             {formatDistanceToNowStrict(dateObj, { addSuffix: true, locale: ar })}
                                                         </span>
                                                     );
                                                 })()}
                                             </span>
                                        </div>
                                        <Text variant="description" className="leading-relaxed max-w-4xl">
                                            {n.message}
                                        </Text>
                                        {n.actorName && (
                                            <Text variant="caption" className="mt-3 flex items-center gap-1.5">
                                                <span className="w-1.5 h-1.5 rounded-full bg-muted-foreground/30"></span>
                                                بواسطة: <span className="font-medium text-muted-foreground">{n.actorName}</span>
                                            </Text>
                                        )}
                                        {n.attachments && n.attachments.length > 0 && (
                                            <div className="mt-3 pt-3 border-t border-border/50">
                                                <div className="flex items-center gap-1 text-xs text-muted-foreground mb-2">
                                                    <Paperclip className="w-3 h-3" />
                                                    <span>مرفقات ({n.attachments.length})</span>
                                                </div>
                                                <div className="flex flex-wrap gap-2">
                                                    {n.attachments.map((att: NotificationAttachmentDto) => (
                                                        <a 
                                                            key={att.attachmentID}
                                                            href={att.fileUrl}
                                                            target="_blank"
                                                            rel="noopener noreferrer"
                                                            className="flex items-center gap-2 px-3 py-2 bg-muted/50 border border-border/50 rounded-lg text-sm hover:bg-muted transition-colors group"
                                                            onClick={(e) => e.stopPropagation()}
                                                        >
                                                            <Paperclip className="w-4 h-4 text-muted-foreground group-hover:text-primary" />
                                                            <span className="truncate max-w-[120px] sm:max-w-[200px] font-medium">{att.fileName}</span>
                                                            <span className="text-xs text-muted-foreground">({(att.fileSize / 1024).toFixed(1)} KB)</span>
                                                            <Download className="w-3.5 h-3.5 text-muted-foreground group-hover:text-primary" />
                                                        </a>
                                                    ))}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </Card>

                {totalPages > 1 && (
                    <div className="flex items-center justify-between flex-wrap gap-2">
                        <Text variant="description" className="text-sm">
                            إجمالي {totalCount} إشعار
                        </Text>
                        <div className="flex items-center gap-2">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => setPage(p => Math.max(1, p - 1))}
                                disabled={page === 1}
                            >
                                <ChevronLeft className="w-4 h-4" />
                            </Button>
                            <span className="text-sm text-muted-foreground px-2">
                                {page} من {totalPages}
                            </span>
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                                disabled={page === totalPages}
                            >
                                <ChevronRight className="w-4 h-4" />
                            </Button>
                        </div>
                    </div>
                )}
            </div>
        </PageLayout>
    );
}
