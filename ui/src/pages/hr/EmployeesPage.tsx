import { useQuery } from "@tanstack/react-query";
import {
  Edit,
  Eye,
  Filter,
  User,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import type { EmployeeListItemDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { CreateEmployeeDialog } from "@/components/domain/hr/CreateEmployeeDialog";
import {
  Badge,
  Button,
  CrudPageLayout,
  type CustomColumnDef,
  ErrorState,
  Loader
} from "@/components/ui";
import { useCrudState, useRoleBasedUI } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { getPhotoUrl } from "@/lib/utils";

/**
 * صفحة إدارة الموظفين - وحدة الموارد البشرية
 *Named Export (Constraint 8)
 */
export function EmployeesPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch();
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [cursor, setCursor] = useState<number | null | undefined>(undefined);
  const cursorHistory = useRef<number[]>([]);
  const [sortConfig, setSortConfig] = useState<{ column: string, direction: "asc" | "desc" | null }>({ column: "jobTitleName", direction: "asc" });
  const pageSize = 10;

  // ── Debounce Search ────────────────────────────────
  useEffect(() => {
      setPage(1);
      setCursor(undefined);
      cursorHistory.current = [];
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
  }, [debouncedSearch]);

  const { hasPermission } = useRoleBasedUI();
  const canCreate = hasPermission("HR.Employee.Create");
  const canEdit = hasPermission("HR.Employee.Edit");

  const {
    isFormOpen,
    selectedItem,
    openCreate,
    openEdit,
    closeForm
  } = useCrudState<EmployeeListItemDto>();

  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "employees", { page, search: debouncedSearch, cursor }],
    queryFn: () => hrApi.getEmployees({ page, pageSize, search: debouncedSearch, cursor })
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل بيانات الموظفين", { id: "employees" });
  }, [error]);

  const onCursorNext = useCallback(() => {
    if (data?.nextCursor) {
      cursorHistory.current.push(data.nextCursor);
      setCursor(data.nextCursor);
      setPage((p) => p + 1);
    }
  }, [data?.nextCursor]);

  const onCursorPrev = useCallback(() => {
    const prev = cursorHistory.current.pop();
    setCursor(prev ?? undefined);
    setPage((p) => Math.max(1, p - 1));
  }, []);

  const onSearchChange = useCallback((val: string) => {
    setSearch(val);
  }, []);

  const sortedData = useMemo(() => {
    if (!data?.items) return [];
    if (!sortConfig.direction || !sortConfig.column) return data.items;

    return [...data.items].sort((a, b) => {
      // Sort by jobTitleID when sorting the jobTitleName column (hierarchy order)
      const sortKey = sortConfig.column === "jobTitleName" ? "jobTitleID" : sortConfig.column;
      const valA = a[sortKey as keyof EmployeeListItemDto] ?? "";
      const valB = b[sortKey as keyof EmployeeListItemDto] ?? "";

      if (typeof valA === "string" && typeof valB === "string") {
        return sortConfig.direction === "asc" ? valA.localeCompare(valB) : valB.localeCompare(valA);
      }

      if (valA < valB) return sortConfig.direction === "asc" ? -1 : 1;
      if (valA > valB) return sortConfig.direction === "asc" ? 1 : -1;
      return 0;
    });
  }, [data, sortConfig]);

  const columns: CustomColumnDef<EmployeeListItemDto>[] = [
    {
      key: "fullName",
      header: "الموظف",
      sortable: true,
      render: (row) => (
        <div className="flex items-center gap-3 text-start">
          {row.profilePhotoPath ? (
            <img 
              src={getPhotoUrl(row.profilePhotoPath)!}  
              alt={row.fullName}
              className="w-10 h-10 rounded-xl object-cover border border-border"
              loading="lazy"
            />
          ) : (
            <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary font-bold text-sm">
              <User className="w-5 h-5 opacity-40" />
            </div>
          )}
          <div className="flex flex-col">
            <span className="font-bold text-foreground">{row.fullName}</span>
            <span className="text-[10px] sm:text-xs text-muted-foreground font-mono">{row.nationalID}</span>
          </div>
        </div>
      )
    },
    {
      key: "employeeNumber",
      header: "الرقم الوظيفي",
      sortable: true,
      render: (row) => <span className="font-mono text-xs bg-muted/50 px-2 py-1 rounded font-bold">{row.employeeNumber}</span>
    },
    {
      key: "departmentName",
      header: "الإدارة / القسم",
      render: (row) => (
        <div className="flex flex-col text-start">
          <span className="text-sm font-medium">{row.departmentName}</span>
          <span className="text-[10px] sm:text-xs text-muted-foreground">{row.sectionName || '-'}</span>
        </div>
      )
    },
    {
      key: "jobTitleName",
      header: "المسمى الوظيفي",
      sortable: true,
      accessor: (row) => row.jobTitleName
    },
    {
      key: "isActive",
      header: "الحالة",
      render: (row) => (
        <Badge variant={row.isActive ? "success" : "outline"}>
          {row.employmentStatusName}
        </Badge>
      )
    },
    {
      key: "actions",
      header: "الإجراءات",
      className: "w-20",
      render: (row) => (
        <div className="flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
          <Button
            variant="ghost"
            size="icon"
            className="min-h-[44px] min-w-[44px] text-primary hover:bg-primary/10 transition-colors"
            onClick={() => navigate(`/hr/employees/${row.employeeID}`)}
            title="عرض التفاصيل"
          >
            <Eye className="h-4 w-4" />
          </Button>
          {canEdit && (
            <Button
              variant="ghost"
              size="icon"
              className="min-h-[44px] min-w-[44px] text-muted-foreground hover:bg-muted transition-colors"
              title="تعديل"
              onClick={() => openEdit(row)}
            >
              <Edit className="h-4 w-4" />
            </Button>
          )}
        </div>
      )
    }
  ];

  // ── Keyboard Navigation ────────────────────────────
  const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
    const items = sortedData;
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
        navigate(`/hr/employees/${items[idx].employeeID}`);
      }
    } else if (e.key === "Escape") {
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }
  }, [sortedData, navigate]);

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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-[60dvh]">
        <Loader text="جاري تحميل قائمة الموظفين..." />
      </div>
    );
  }

  if (error) {
    return <ErrorState message="تعذر تحميل بيانات الموظفين" />;
  }

  return (
    <CrudPageLayout
      title="إدارة الموظفين"
      description="عرض وإدارة السجلات الوظيفية والبيانات الأساسية للمنتسبين"
      createLabel="إضافة موظف جديد"
      onCreateClick={canCreate ? openCreate : undefined}
      canCreate={canCreate}
      columns={columns}
      data={sortedData}
      loading={isLoading}
      sortColumn={sortConfig.column}
      sortDirection={sortConfig.direction}
      onSort={(column, direction) => setSortConfig({ column, direction })}
      searchable
      searchInHeader
      searchPlaceholder="البحث بالاسم، الرقم الوظيفي، أو الرقم الوطني..."
      onSearchChange={onSearchChange}
      searchValue={search}
      onSearchKeyDown={handleSearchKeyDown}
      searchInputRef={searchInputRef}
      focusedRowIndex={focusedIndex}
      pagination={{
        page: page,
        pageSize,
        total: data?.totalCount || 0,
        onPageChange: (p) => { setPage(p); setCursor(undefined); cursorHistory.current = []; },
        cursor,
        hasMore: data?.hasMore,
        onCursorNext: cursor !== undefined ? onCursorNext : undefined,
        onCursorPrev: cursor !== undefined ? onCursorPrev : undefined,
      }}
      onRowClick={(row) => navigate(`/hr/employees/${row.employeeID}`)}
      emptyMessage="لم يتم العثور على موظفين يطابقون معايير البحث"
      headerActions={
        <Button variant="outline" className="gap-2 border-primary/20 bg-primary/5 hover:bg-primary/10">
          <Filter className="h-4 w-4" />
          تصفية متقدمة
        </Button>
      }
    >
      <CreateEmployeeDialog
        open={isFormOpen}
        employeeId={selectedItem?.employeeID}
        onOpenChange={(open) => !open && closeForm()}
        onSaved={() => {
          setPage(1);
        }}
      />
    </CrudPageLayout>
  );
}
