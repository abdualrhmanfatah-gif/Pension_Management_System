import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Bell,
  BellOff,
  BellRing,
  CheckCheck,
  Clock,
  Mail,
  MailX,
  Plus,
  RefreshCw,
  Trash2,
} from "lucide-react";
import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import { toast } from "sonner";

<<<<<<< HEAD
import { type AdminNotificationFilter,notificationsApi } from "@/api/notifications";
import { SendNotificationDialog } from "@/components/domain/admin/SendNotificationDialog";
=======
import { notificationsApi, type AdminNotificationFilter } from "@/api/notifications";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Badge,
  Button,
  Card,
<<<<<<< HEAD
  type CustomColumnDef,
  DataTable,
=======
  DataTable,
  type CustomColumnDef,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  Heading,
  Input,
  PageLayout,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  StatCard,
<<<<<<< HEAD
  StickyHeader,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
  Text,
<<<<<<< HEAD
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
=======
  StickyHeader,
} from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useConfirm } from "@/hooks/useConfirm";
import { formatDate } from "@/lib/utils";
import type { NotificationDto } from "@/types/notifications";
import { CATEGORY_COLORS, CATEGORY_LABELS } from "@/types/notifications";
<<<<<<< HEAD
=======
import { SendNotificationDialog } from "@/components/domain/admin/SendNotificationDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function AdminNotificationsPage() {
  const queryClient = useQueryClient();
  const { confirm, ConfirmDialogElement } = useConfirm();

  const [searchParams, setSearchParams] = useSearchParams();
  const [sendDialogOpen, setSendDialogOpen] = useState(false);
  const [tab, setTab] = useState(searchParams.get("tab") || "all");
  const [page, setPage] = useState(1);
  const [outboxPage, setOutboxPage] = useState(1);
  const [outboxStatus, setOutboxStatus] = useState<string>("");
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  const filter: AdminNotificationFilter = {
    page,
    pageSize: 20,
    search: search || undefined,
    category: categoryFilter || undefined,
    isRead: statusFilter === "unread" ? false : statusFilter === "read" ? true : undefined,
  };

  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ["admin-notifications", "stats"],
    queryFn: notificationsApi.admin.getStats,
  });

  const { data: allData, isLoading: allLoading, isError: allError } = useQuery({
    queryKey: ["admin-notifications", "all", filter],
    queryFn: () => notificationsApi.admin.getAll(filter.page, filter.pageSize, filter),
  });

  const { data: outboxData, isLoading: outboxLoading, isError: outboxError } = useQuery({
    queryKey: ["admin-notifications", "outbox", outboxPage, outboxStatus],
    queryFn: () => notificationsApi.admin.getOutbox(outboxPage, 20, outboxStatus || undefined),
    enabled: tab === "outbox",
  });

  const deleteMutation = useMutation({
    mutationFn: notificationsApi.admin.delete,
    onSuccess: (res) => {
      if (res.isSuccess) {
        toast.success("تم حذف الإشعار");
        queryClient.invalidateQueries({ queryKey: queryKeys.admin.notifications.all });
      } else {
        toast.error(res.message || "فشل الحذف");
      }
    },
  });

  const retryMutation = useMutation({
    mutationFn: notificationsApi.admin.retryOutbox,
    onSuccess: (res) => {
      if (res.isSuccess) {
        toast.success("تم إعادة محاولة الإرسال");
        queryClient.invalidateQueries({ queryKey: ["admin-notifications", "outbox"] });
        queryClient.invalidateQueries({ queryKey: ["admin-notifications", "stats"] });
      } else {
        toast.error(res.message || "فشلت إعادة المحاولة");
      }
    },
  });

  const handleDelete = async (id: number) => {
    const ok = await confirm({ title: "حذف الإشعار", description: "هل أنت متأكد من حذف هذا الإشعار؟" });
    if (ok) deleteMutation.mutate(id);
  };

  const statsData = stats?.data;
  const allResult = allData?.data ?? { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 1, hasPrevious: false, hasNext: false };
  const outboxResult = outboxData?.data ?? { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 1, hasPrevious: false, hasNext: false };

  return (
    <PageLayout maxWidth="max-w-7xl">
      <div className="flex flex-col gap-6">
        {/* Header */}
        <StickyHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center w-10 h-10 rounded-xl bg-primary/10">
                <BellRing className="w-5 h-5 text-primary" />
              </div>
              <div>
                <Heading level="h1">إدارة الإشعارات</Heading>
                <Text variant="description">عرض وإدارة جميع إشعارات النظام وإرسال إشعارات جديدة</Text>
              </div>
            </div>
            <Button variant="primary" size="sm" onClick={() => setSendDialogOpen(true)}>
              <Plus className="w-4 h-4" />
              إرسال إشعار جديد
            </Button>
          </div>
        </StickyHeader>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2 sm:gap-4">
          <StatCard title="إجمالي الإشعارات" value={statsData?.totalNotifications ?? 0} icon={<Bell className="h-5 w-5" />} variant="blue" loading={statsLoading} />
          <StatCard title="غير مقروء" value={statsData?.unreadCount ?? 0} icon={<BellOff className="h-5 w-5" />} variant="rose" loading={statsLoading} />
          <StatCard title="اليوم" value={statsData?.todayCount ?? 0} icon={<Clock className="h-5 w-5" />} variant="amber" loading={statsLoading} />
          <StatCard title="فشل الإرسال" value={statsData?.failedOutboxCount ?? 0} icon={<MailX className="h-5 w-5" />} variant="destructive" loading={statsLoading} />
          <StatCard title="معلق" value={statsData?.pendingOutboxCount ?? 0} icon={<Mail className="h-5 w-5" />} variant="warning" loading={statsLoading} />
        </div>

        {/* Category Breakdown */}
        {statsData?.byCategory && Object.keys(statsData.byCategory).length > 0 && (
          <div className="flex gap-2 flex-wrap">
            {Object.entries(statsData.byCategory).map(([cat, count]) => (
              <Badge key={cat} variant={CATEGORY_COLORS[cat] ?? "ghost"} className="gap-1.5 px-3 py-1">
                {CATEGORY_LABELS[cat] ?? cat}: {count}
              </Badge>
            ))}
          </div>
        )}

        {/* Tabs */}
        <Tabs value={tab} onValueChange={(v) => { setTab(v); setPage(1); setSearchParams((p) => { p.set("tab", v); return p; }, { replace: true }); }}>
          <div className="overflow-x-auto pb-1 scrollbar-hide">
            <TabsList>
              <TabsTrigger value="all">جميع الإشعارات</TabsTrigger>
              <TabsTrigger value="outbox">سجل الإرسال (Outbox)</TabsTrigger>
            </TabsList>
          </div>

          {/* ─── All Notifications Tab ─── */}
          <TabsContent value="all" className="space-y-4">
            {/* Filters */}
            <div className="flex gap-2 flex-wrap">
              <Input
                placeholder="بحث..."
                value={search}
                onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                className="w-full sm:w-48"
              />
              <Select value={categoryFilter} onValueChange={(v) => { setCategoryFilter(v); setPage(1); }}>
                <SelectTrigger className="w-36">
                  <SelectValue placeholder="التصنيف" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value=" ">الكل</SelectItem>
                  {Object.entries(CATEGORY_LABELS).map(([key, label]) => (
                    <SelectItem key={key} value={key}>{label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Select value={statusFilter} onValueChange={(v) => { setStatusFilter(v); setPage(1); }}>
                <SelectTrigger className="w-36">
                  <SelectValue placeholder="الحالة" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value=" ">الكل</SelectItem>
                  <SelectItem value="unread">غير مقروء</SelectItem>
                  <SelectItem value="read">مقروء</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Table */}
            <Card className="overflow-hidden">
              {allLoading ? (
                <div className="p-6 sm:p-12 text-center text-muted-foreground">جاري التحميل...</div>
              ) : allError ? (
                <div className="p-6 sm:p-12 text-center text-destructive">تعذر تحميل الإشعارات</div>
              ) : allResult.items.length === 0 ? (
                <div className="p-6 sm:p-12 text-center text-muted-foreground">لا توجد إشعارات</div>
              ) : (
                <DataTable
                  columns={[
                    { key: "actorName", header: "المستخدم", render: (row) => <span className="text-muted-foreground">{row.actorName || `#${row.userID}`}</span> },
                    { key: "title", header: "العنوان", render: (row) => <span className="font-medium">{row.title}</span> },
                    { key: "category", header: "التصنيف", render: (row) => row.category && CATEGORY_LABELS[row.category] ? <Badge variant={CATEGORY_COLORS[row.category] ?? "ghost"} className="text-[10px] sm:text-xs">{CATEGORY_LABELS[row.category]}</Badge> : <span className="text-muted-foreground">-</span> },
                    { key: "priority", header: "الأولوية", render: (row) => <Badge variant={row.priority >= 3 ? "destructive" : row.priority >= 2 ? "warning" : "ghost"}>{row.priority >= 3 ? "عالية" : row.priority >= 2 ? "متوسطة" : "عادية"}</Badge> },
                    { key: "isRead", header: "الحالة", render: (row) => row.isRead ? <span className="text-muted-foreground flex items-center gap-1"><CheckCheck className="w-3.5 h-3.5" /> مقروء</span> : <Badge variant="info">جديد</Badge> },
                    { key: "createdAt", header: "التاريخ", render: (row) => <span className="text-muted-foreground text-xs">{formatDate(row.createdAt, true)}</span> },
                    { key: "actions", header: "إجراءات", render: (row) => <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px]" onClick={() => handleDelete(row.notificationID)} title="حذف"><Trash2 className="w-4 h-4 text-destructive" /></Button> },
                  ] as CustomColumnDef<NotificationDto>[]}
                  data={allResult.items}
                  rowKey={(row) => row.notificationID}
                  pagination={{ page, pageSize: 20, total: allResult.totalCount, onPageChange: setPage }}
                />
              )}
            </Card>
          </TabsContent>

          {/* ─── Outbox Tab ─── */}
          <TabsContent value="outbox" className="space-y-4">
            <div className="flex gap-2">
              <Select value={outboxStatus} onValueChange={(v) => { setOutboxStatus(v); setOutboxPage(1); }}>
                <SelectTrigger className="w-36">
                  <SelectValue placeholder="الحالة" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value=" ">الكل</SelectItem>
                  <SelectItem value="Pending">معلق</SelectItem>
                  <SelectItem value="Failed">فاشل</SelectItem>
                  <SelectItem value="Sent">تم الإرسال</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <Card className="overflow-hidden">
              {outboxLoading ? (
                <div className="p-6 sm:p-12 text-center text-muted-foreground">جاري التحميل...</div>
              ) : outboxError ? (
                <div className="p-6 sm:p-12 text-center text-destructive">تعذر تحميل سجل الإرسال</div>
              ) : outboxResult.items.length === 0 ? (
                <div className="p-6 sm:p-12 text-center text-muted-foreground">لا توجد عناصر في سجل الإرسال</div>
              ) : (
                <DataTable
                  columns={[
                    { key: "title", header: "الإشعار", render: (row) => <span className="font-medium">{row.title}</span> },
                    { key: "userName", header: "المستخدم", render: (row) => <span className="text-muted-foreground">{row.userName}</span> },
                    { key: "status", header: "الحالة", render: (row) => <Badge variant={row.status === "Sent" ? "success" : row.status === "Failed" ? "destructive" : "warning"}>{row.status === "Sent" ? "تم الإرسال" : row.status === "Failed" ? "فاشل" : "معلق"}</Badge> },
                    { key: "retryCount", header: "المحاولات", render: (row) => <span className="text-muted-foreground">{row.retryCount}/{row.maxRetries}</span> },
                    { key: "createdAt", header: "التاريخ", render: (row) => <span className="text-muted-foreground text-xs">{formatDate(row.createdAt, true)}</span> },
                    { key: "errorMessage", header: "الخطأ", render: (row) => <span className="text-xs text-destructive max-w-[200px] truncate block">{row.errorMessage || "-"}</span> },
                    { key: "actions", header: "إجراءات", render: (row) => row.status !== "Sent" ? <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px]" onClick={() => retryMutation.mutate(row.outboxId)} title="إعادة محاولة"><RefreshCw className="w-4 h-4 text-primary" /></Button> : null },
                  ] as CustomColumnDef<typeof outboxResult.items[number]>[]}
                  data={outboxResult.items}
                  rowKey={(row) => row.outboxId}
                  pagination={{ page: outboxPage, pageSize: 20, total: outboxResult.totalCount, onPageChange: setOutboxPage }}
                />
              )}
            </Card>
          </TabsContent>
        </Tabs>
      </div>
      <SendNotificationDialog open={sendDialogOpen} onOpenChange={setSendDialogOpen} />
      {ConfirmDialogElement}
    </PageLayout>
  );
}
