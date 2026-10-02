import { useQuery } from "@tanstack/react-query";
import {
  CalendarDays,
  Clock,
  Filter,
  LogIn,
  LogOut,
  MapPin} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import type { AttendanceEventDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { AttendanceDialog } from "@/components/domain/hr/AttendanceDialog";
import {
  Badge,
  Button,
  CrudPageLayout,
  type CustomColumnDef,
  DateInput,
  ErrorState,
  StatCard} from "@/components/ui";
import { ATTENDANCE_TYPE } from "@/constants";
import { formatDate } from "@/lib/utils";

/**
 * صفحة متابعة سجلات الحضور والانصراف - وحدة الموارد البشرية
 */
export function HrAttendancePage() {
  const [page, setPage] = useState(1);
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const pageSize = 20;

  // جلب السجلات
  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "attendance", page, selectedDate],
    queryFn: () => hrApi.getAttendance({
      page,
      pageSize,
      fromDate: selectedDate,
      toDate: selectedDate
    })
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل بيانات الحضور", { id: "hr-attendance" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل بيانات الحضور" />;
  }

  // جلب الإحصائيات اليومية
  const { data: stats } = useQuery({
    queryKey: ["hr", "attendance-stats", selectedDate],
    queryFn: () => hrApi.getDailyStats(selectedDate)
  });

  const columns = useMemo<CustomColumnDef<AttendanceEventDto>[]>(() => [
    {
      key: "employeeFullName",
      header: "الموظف",
      render: (r) => (
        <div className="flex items-center gap-2 text-start">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-[10px] sm:text-xs font-bold text-primary shrink-0">
            {r.employeeFullName.split(' ').map(n => n[0]).join('').substring(0, 2)}
          </div>
          <div className="flex flex-col">
            <span className="text-sm font-bold">{r.employeeFullName}</span>
            <span className="text-[10px] sm:text-xs text-muted-foreground font-mono">{r.employeeNumber}</span>
          </div>
        </div>
      )
    },
    {
      key: "eventDate",
      header: "التاريخ",
      render: (r) => <span className="font-mono text-xs">{formatDate(r.eventDate)}</span>
    },
    {
      key: "eventTime",
      header: "الوقت",
      render: (r) => <span className="font-mono font-bold text-primary">{r.eventTime ? r.eventTime.substring(0, 5) : "--:--"}</span>
    },
    {
      key: "eventType",
      header: "نوع الحركة",
      render: (r) => {
        const isCheckIn = r.eventType === ATTENDANCE_TYPE.CHECK_IN;
        return (
          <Badge variant={isCheckIn ? "success" : "warning"} className="gap-1.5 px-2 h-6 font-black">
            {isCheckIn ? <LogIn className="h-3.5 w-3.5" /> : <LogOut className="h-3.5 w-3.5" />}
            {isCheckIn ? "حضور" : "انصراف"}
          </Badge>
        );
      }
    },
    {
      key: "location",
      header: "الموقع",
      render: (r) => (
        <div className="flex items-center gap-1 text-xs text-muted-foreground justify-center">
          <MapPin className="h-3 w-3" />
          {r.location || "غير محدد"}
        </div>
      )
    },
    {
      key: "isManualEntry",
      header: "المصدر",
      render: (r) => (
        <Badge variant="outline" className="text-[10px] sm:text-xs font-medium h-5">
          {r.isManualEntry ? "إدخال يدوي" : "جهاز البصمة"}
        </Badge>
      )
    }
  ], []);

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
    } else if (e.key === "Escape") {
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }
  }, [data?.items]);

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
      title="سجلات الحضور والانصراف"
      description="متابعة سجلات حضور وانصراف الموظفين وإحصائيات الالتزام بالدوام اليومي"
      createLabel="تسجيل يدوي"
      onCreateClick={() => setIsDialogOpen(true)}
      columns={columns}
      data={data?.items || []}
      loading={isLoading}
      searchable
      searchInHeader
      searchInputRef={searchInputRef}
      onSearchKeyDown={handleSearchKeyDown}
      focusedRowIndex={focusedIndex}
      pagination={{
        page: page,
        pageSize,
        total: data?.totalCount || 0,
        onPageChange: (p) => setPage(p)
      }}
      headerActions={
        <div className="flex items-center gap-2 me-2 flex-wrap">
          <DateInput
            className="h-9 text-xs w-full sm:w-40 font-mono bg-background"
            value={selectedDate}
            onChange={(val) => val && setSelectedDate(val)}
          />
          <Button variant="outline" size="sm" className="gap-2 text-xs h-11 ">
            <Filter className="h-3.5 w-3.5" /> تصفية
          </Button>
        </div>
      }
    >
      {/* Daily Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
        <StatCard
          title="الحضور اليوم"
          value={stats?.presentCount || 0}
          description={`${stats?.presencePercentage.toFixed(1) || 0}% من إجمالي القوة`}
          icon={<LogIn className="h-4 w-4" />}
          variant="emerald"
        />
        <StatCard
          title="حالات التأخير"
          value={stats?.lateCount || 0}
          description="بعد الساعة 08:15 ص"
          icon={<Clock className="h-4 w-4" />}
          variant="rose"
        />
        <StatCard
          title="الغياب اليوم"
          value={stats?.absentCount || 0}
          description={`من أصل ${stats?.totalEmployees || 0} موظف`}
          icon={<CalendarDays className="h-4 w-4" />}
          variant="blue"
        />
        <StatCard
          title="مهام خارجية"
          value={stats?.externalTaskCount || 0}
          description="خارج المقر الرسمي"
          icon={<MapPin className="h-4 w-4" />}
          variant="orange"
        />
      </div>

      <AttendanceDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        onSaved={() => setPage(1)}
      />
    </CrudPageLayout>
  );
}
