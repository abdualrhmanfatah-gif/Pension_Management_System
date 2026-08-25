import { useQuery } from "@tanstack/react-query";
import { Download, FileText, User } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import { hrApi, type HrGlobalDocumentDto } from "@/api/hr";
import { getDocumentUrl } from "@/lib/utils";
import { 
  Badge,
  Button,
  type CustomColumnDef,
  ErrorState,
  Text} from "@/components/ui";
import { CrudPageLayout } from "@/components/ui/CrudPageLayout";


/**
 * صفحة استعراض وثائق الموظفين الشاملة
 */
export function HrDocumentsPage() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ["global-employee-documents", page, pageSize, debouncedSearch],
    queryFn: () => hrApi.getGlobalDocuments({ page, pageSize, search: debouncedSearch }),
  });

  const handleDownload = (doc: HrGlobalDocumentDto) => {
    if (doc.filePath) {
      window.open(getDocumentUrl(doc.filePath) ?? "", "_blank");
    }
  };

  const columns = useMemo<CustomColumnDef<HrGlobalDocumentDto>[]>(() => [
    {
      key: "employeeInfo",
      header: "الموظف",
      render: (row) => (
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <User className="h-5 w-5" />
          </div>
          <div>
            <p className="text-sm font-semibold text-foreground">{row.employeeFullName}</p>
            <Text variant="caption">{row.employeeNumber}</Text>
          </div>
        </div>
      ),
    },
    {
      key: "fileName",
      header: "اسم الملف",
      render: (row) => (
        <div className="flex items-center gap-2">
            <FileText className="h-4 w-4 text-muted-primary" />
            <span className="font-medium text-sm text-foreground/90">{row.fileName}</span>
        </div>
      ),
    },
    {
      key: "documentType",
      header: "التصنيف",
      render: (row) => (
        <Badge variant="outline" className="bg-primary/5 text-primary border-primary/20">
          {row.documentTypeName}
        </Badge>
      ),
    },
    {
       key: "fileSize",
       header: "الحجم",
       render: (row) => (
          <span className="text-sm text-muted-foreground" dir="ltr">
            {(row.fileSize / 1024 / 1024).toFixed(2)} MB
          </span>
       )
    },
    {
      key: "createdAt",
      header: "تاريخ الرفع",
      render: (row) => (
        <span className="text-sm text-muted-foreground whitespace-normal sm:whitespace-nowrap">
          {new Date(row.createdAt).toLocaleDateString('ar-EG', { year: 'numeric', month: 'long', day: 'numeric' })}
        </span>
      ),
    },
    {
      key: "uploadedBy",
      header: "بواسطة",
      render: (row) => (
        <span className="text-sm text-muted-foreground">
          {row.uploadedByUser || "غير محدد"}
        </span>
      ),
    },
    {
      key: "actions",
      header: "",
      render: (row) => (
        <Button 
            variant="ghost" 
            size="sm" 
            onClick={() => handleDownload(row)}
            className="text-primary hover:text-primary hover:bg-primary/10"
            disabled={!row.filePath}
        >
            <Download className="h-4 w-4 ms-1" />
            تحميل
        </Button>
      ),
    },
  ], []);

  useEffect(() => {
    if (error) toast.error("تعذر تحميل المستندات", { id: "hr-documents" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل المستندات" />;
  }

  const searchTimeoutRef = useRef<ReturnType<typeof setTimeout>>(undefined);

  // Clean up search timeout on unmount
  useEffect(() => {
    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, []);

  const handleSearch = (term: string) => {
    setSearch(term);
    if (searchTimeoutRef.current) clearTimeout(searchTimeoutRef.current);
    searchTimeoutRef.current = setTimeout(() => {
        setDebouncedSearch(term);
        setPage(1);
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, 500);
  };

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
        handleDownload(items[idx]);
      }
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
      title="مستندات الموظفين"
      description="استعراض وإدارة الأرشيف الإلكتروني الشامل لجميع الموظفين"
      icon={<FileText className="h-6 w-6" />}
      columns={columns}
      data={Array.isArray(data?.items) ? data.items : []}
      loading={isLoading}
      searchable
      searchInHeader
      searchPlaceholder="ابحث باسم الموظف، الرقم الوظيفي، أو اسم الملف..."
      onSearchChange={handleSearch}
      searchValue={search}
      onSearchKeyDown={handleSearchKeyDown}
      searchInputRef={searchInputRef}
      focusedRowIndex={focusedIndex}
      pagination={{
          page,
          pageSize,
          total: data?.totalCount || 0,
          onPageChange: setPage,
      }}
      canCreate={false}
    />
  );
}
