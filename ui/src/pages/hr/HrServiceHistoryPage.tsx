import { useQuery } from "@tanstack/react-query";
import {
  Activity,
  Calendar,
  History
} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import type { ServiceHistoryEventDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { 
  Badge,
  CrudPageLayout,
  type CustomColumnDef,
  ErrorState,
  StatCard} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { formatDate } from "@/lib/utils";

/**
 * صفحة السجل التاريخي الشامل للمتغيرات الوظيفية - وحدة الموارد البشرية
 */
export function HrServiceHistoryPage() {
  const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch();
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [page, setPage] = useState(1);
  const pageSize = 50;

  // ── Debounce Search ────────────────────────────────
  useEffect(() => {
      setPage(1);
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
  }, [debouncedSearch]);

  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "service-history-global", page, debouncedSearch],
    queryFn: () => hrApi.getGlobalServiceHistory({ 
        search: debouncedSearch || undefined, 
        page,
        pageSize 
    }),
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل سجل الخدمة", { id: "hr-service-history" });
  }, [error]);

  const columns = useMemo<CustomColumnDef<ServiceHistoryEventDto>[]>(() => [
    { 
      key: "eventDate", 
      header: "التاريخ", 
      render: (r) => <span className="font-mono text-xs">{formatDate(r.eventDate)}</span>
    },
    { 
      key: "employeeFullName", 
      header: "الموظف المعني",
      render: (r) => (
        <div className="flex flex-col text-start">
          <span className="font-bold text-sm text-foreground">{r.employeeFullName}</span>
          <span className="text-[10px] sm:text-xs text-muted-foreground font-mono">{r.employeeNumber}</span>
        </div>
      )
    },
    { 
      key: "eventType", 
      header: "نوع الحدث",
      render: (r) => <Badge variant="outline" className="font-black text-[10px] sm:text-xs">{r.eventType}</Badge>
    },
    { 
      key: "summary", 
      header: "تفاصيل البيان",
      render: (r) => <span className="font-medium text-sm text-start leading-relaxed">{r.summary}</span>
    },
    { 
        key: "sourceEntityType", 
        header: "مصدر الحركة",
        render: (r) => <span className="text-[10px] sm:text-xs text-muted-foreground font-bold uppercase tracking-wider">{r.sourceEntityType}</span>
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

  if (error) {
    return (
      <div dir="rtl">
        <ErrorState message="تعذر تحميل سجل الخدمة" />
      </div>
    );
  }

  return (
    <CrudPageLayout
        title="سجل المتغيرات الوظيفية"
        description="التوثيق التاريخي لكافة القرارات الإدارية والتحركات الوظيفية على مستوى الصندوق"
        icon={<History className="w-5 h-5" />}
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
        }}
    >
        <div className="grid grid-cols-2 sm:grid-cols-2 gap-2 sm:gap-4 mb-6">
            <StatCard 
                title="إجمالي الحركات" 
                value={data?.totalCount || 0} 
                icon={<Activity className="h-4 w-4" />} 
                variant="blue" 
            />
            <StatCard 
                title="الحركات المعروضة" 
                value={data?.items.length || 0} 
                icon={<Calendar className="h-4 w-4" />} 
                variant="emerald" 
            />
        </div>
    </CrudPageLayout>
  );
}
