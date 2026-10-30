import { useQuery } from "@tanstack/react-query";
import {
  Calendar,
  CalendarDays,
  CheckCircle2,
  Clock,
  Filter,
  type LucideIcon,
  XCircle} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import type { LeaveRequestListItemDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
<<<<<<< HEAD
import { LeaveRequestDialog } from "@/components/domain/hr/LeaveRequestDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Badge,
  Button,
  CrudPageLayout,
  type CustomColumnDef,
  ErrorState,
  NativeSelect,
  StatCard} from "@/components/ui";
<<<<<<< HEAD
=======
import { LeaveRequestDialog } from "@/components/domain/hr/LeaveRequestDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { LEAVE_STATUS } from "@/constants";
import { useApiMutation } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { formatDate } from "@/lib/utils";

/**
 * صفحة إدارة طلبات الإجازات - وحدة الموارد البشرية
 */
export function HrLeavesPage() {
  const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch();
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [status, setStatus] = useState<string>("");
  const [page, setPage] = useState(1);
  const pageSize = 15;
  const [isModalOpen, setIsModalOpen] = useState(false);

  // ── Debounce Search ────────────────────────────────
  useEffect(() => {
      setPage(1);
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
  }, [debouncedSearch]);

  // جلب الطلبات مع البحث والفلترة
  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "leaves", page, debouncedSearch, status],
    queryFn: () => hrApi.getLeaveRequests({
      page,
      pageSize,
      search: debouncedSearch || undefined,
      status: status || undefined
    })
  });

  // جلب الإحصائيات
  const { data: stats } = useQuery({
    queryKey: ["hr", "leaves-stats"],
    queryFn: () => hrApi.getLeaveStats()
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل بيانات الإجازات", { id: "hr-leaves" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل بيانات الإجازات" />;
  }

  // معالجة الطلب (قبول/رفض)
  const processMutation = useApiMutation(({ id, status }: { id: number, status: "Approved" | "Rejected" }) =>
    hrApi.processLeave(id, { status, notes: "تمت المعالجة عبر صفحة الإدارة" }), {
    successMessage: "تم تحديث حالة الطلب بنجاح",
    invalidateKeys: [["hr", "leaves"], ["hr", "leaves-stats"]]
  });

  const columns = useMemo<CustomColumnDef<LeaveRequestListItemDto>[]>(() => [
    {
      key: "employeeFullName",
      header: "الموظف",
      render: (r) => (
        <div className="flex items-center gap-2 text-start">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-[10px] sm:text-xs font-bold text-primary">
            {r.employeeFullName.split(' ').map((n: string) => n[0]).join('').substring(0, 2)}
          </div>
          <div className="flex flex-col">
            <span className="text-sm font-medium">{r.employeeFullName}</span>
            <span className="text-[10px] sm:text-xs text-muted-foreground font-mono">{r.employeeNumber}</span>
          </div>
        </div>
      )
    },
    {
      key: "leaveTypeName",
      header: "نوع الإجازة",
      render: (r) => <Badge variant="outline" className="font-bold">{r.leaveTypeName}</Badge>
    },
    {
      key: "startDate",
      header: "الفترة الزمنية",
      render: (r) => (
        <div className="text-[10px] sm:text-xs space-y-0.5 text-start font-mono">
          <div className="flex items-center gap-1 text-muted-foreground">
            <Calendar className="h-3 w-3" />
            <span>من: {formatDate(r.startDate)}</span>
          </div>
          <div className="flex items-center gap-1 text-muted-foreground">
            <Calendar className="h-3 w-3" />
            <span>إلى: {formatDate(r.endDate)}</span>
          </div>
        </div>
      )
    },
    { key: "requestedDays", header: "الأيام", render: (r) => <span className="font-bold font-mono">{r.requestedDays}</span> },
    {
      key: "status",
      header: "الحالة",
      render: (r) => {
        const statusMap: Record<string, { label: string, variant: "warning" | "success" | "danger" | "outline", icon: LucideIcon }> = {
          "Pending": { label: "قيد الانتظار", variant: "warning", icon: Clock },
          "Approved": { label: "معتمدة", variant: "success", icon: CheckCircle2 },
          "Rejected": { label: "مرفوضة", variant: "danger", icon: XCircle },
        };
        const config = statusMap[r.status] || { label: r.status, variant: "outline", icon: Clock };
        const Icon = config.icon;
        return (
          <div className="flex flex-col items-center gap-1">
            <Badge variant={config.variant} className="gap-1 px-2 h-6 font-black">
              <Icon className="h-3 w-3" />
              {config.label}
            </Badge>
          </div>
        );
      }
    },
    {
      key: "actions",
      header: "إجراءات الإدارة",
      render: (r) => (
        <div className="flex gap-1 justify-center">
          {r.status === LEAVE_STATUS.PENDING && (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="min-h-[44px] min-w-[44px] text-emerald-600 hover:bg-emerald-50"
                onClick={() => processMutation.mutate({ id: r.leaveRequestID, status: "Approved" })}
                loading={processMutation.isPending && processMutation.variables?.id === r.leaveRequestID}
                title="اعتماد الإجازة"
              >
                <CheckCircle2 className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="min-h-[44px] min-w-[44px] text-red-600 hover:bg-red-50"
                onClick={() => processMutation.mutate({ id: r.leaveRequestID, status: "Rejected" })}
                loading={processMutation.isPending && processMutation.variables?.id === r.leaveRequestID}
                title="رفض الإجازة"
              >
                <XCircle className="h-4 w-4" />
              </Button>
            </>
          )}
          <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-muted-foreground hover:bg-muted" title="تصفية الموظف">
            <Filter className="h-4 w-4" />
          </Button>
        </div>
      )
    }
  ], [processMutation]);

  // ── Keyboard Navigation ────────────────────────────
  const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
    const items = data?.items || [];
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setFocusedIndex(prev => {
        const next = prev + 1;
        const clamped = next < items.length ? next : prev;
        focusedIndexRef.current = clamped;
        return clamped;
      });
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setFocusedIndex(prev => {
        const clamped = prev > 0 ? prev - 1 : prev;
        focusedIndexRef.current = clamped;
        return clamped;
      });
    } else if (e.key === "Enter") {
      e.preventDefault();
      const idx = focusedIndexRef.current;
      if (idx >= 0 && idx < items.length) {
        const row = items[idx];
        if (row.status === LEAVE_STATUS.PENDING) {
          processMutation.mutate({ id: row.leaveRequestID, status: "Approved" });
        }
      }
    } else if (e.key === "Escape") {
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }
  }, [data?.items, processMutation]);

  // Scroll to focused row
  useEffect(() => {
    if (focusedIndex >= 0) {
      const row = document.querySelector(`[data-row-index="${focusedIndex}"]`)
                || document.querySelector(`[data-index="${focusedIndex}"]`);
      if (row) {
        row.scrollIntoView({ behavior: "smooth", block: "nearest" });
      }
    }
  }, [focusedIndex]);

  return (
    <CrudPageLayout
      title="طلبات الإجازات"
      description="إدارة ومتابعة طلبات الإجازات السنوية والمرضية والاضطرارية للمنتسبين"
      createLabel="تقديم طلب إجازة"
      onCreateClick={() => setIsModalOpen(true)}
      columns={columns}
      data={data?.items || []}
      loading={isLoading}
      searchable
      searchInHeader
      onSearchChange={(val) => setSearch(val)}
      searchValue={search}
      onSearchKeyDown={handleSearchKeyDown}
      searchInputRef={searchInputRef}
      focusedRowIndex={focusedIndex}
      pagination={{
        page: page,
        pageSize,
        total: data?.totalCount || 0,
        onPageChange: (p) => setPage(p)
      }} headerActions={
        <div className="flex items-center gap-2 me-2 flex-wrap">
          <NativeSelect
            value={status}
            onChange={(val) => { setStatus(val); setPage(1); }}
            options={[
              { label: "كل الحالات", value: "" },
              { label: "قيد الانتظار", value: LEAVE_STATUS.PENDING },
              { label: "معتمدة", value: LEAVE_STATUS.APPROVED },
              { label: "مرفوضة", value: LEAVE_STATUS.REJECTED },
            ]}
            placeholder="فلترة بالحالة"
            className="w-full sm:w-40"
          />
        </div>
      }
    >
      {/* Quick Stats */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-2 sm:gap-4 mb-6">
        <StatCard title="بانتظار الموافقة" value={stats?.pendingCount || 0} icon={<Clock className="h-4 w-4" />} variant="orange" />
        <StatCard title="المعتمدة هذا الشهر" value={stats?.approvedThisMonthCount || 0} icon={<CheckCircle2 className="h-4 w-4" />} variant="emerald" />
        <StatCard title="إجمالي الطلبات (العام)" value={stats?.totalThisYearCount || 0} icon={<CalendarDays className="h-4 w-4" />} variant="blue" />
      </div>

      <LeaveRequestDialog
        open={isModalOpen}
        onOpenChange={(open) => !open && setIsModalOpen(false)}
      />
    </CrudPageLayout>
  );
}

