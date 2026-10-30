<<<<<<< HEAD
import { CorrespondenceDirection } from "@/types/correspondence";

import { CorrespondenceRegisterPage } from "./CorrespondenceRegisterPage";

export function IncomingRegisterPage() {
  return <CorrespondenceRegisterPage direction={CorrespondenceDirection.Incoming} />;
=======
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Edit,
  Eye,
  FileDown,
  Trash2,
} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { correspondenceApi } from "@/api/correspondence";
import { queryKeys } from "@/constants/queryKeys";
import {
  Button,
  CrudPageLayout,
  Tabs,
  TabsList,
  TabsTrigger,
  useConfirm,
} from "@/components/ui";
import { correspondenceColumns } from "@/components/domain/correspondence/CorrespondenceColumns";
import { CorrespondenceFormDialog } from "@/components/domain/correspondence/CorrespondenceFormDialog";
import { useApiMutation,useCrudState } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import {
  type CorrespondenceDetailsDto,
  CorrespondenceDirection,
  type CorrespondenceListItemDto,
  type CorrespondenceTypeDto
} from "@/types/correspondence";

/**
 * صفحة سجل الوارد
 */
export function IncomingRegisterPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch();
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [typeId, setTypeId] = useState<number | undefined>(undefined);
  const pageSize = 15;
  const direction = CorrespondenceDirection.Incoming;

  // ── Debounce Search ────────────────────────────────
  useEffect(() => {
      setPage(1);
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
  }, [debouncedSearch]);

  const queryClient = useQueryClient();
  const { confirm, ConfirmDialogElement } = useConfirm();

  const {
    isFormOpen,
    selectedItem,
    openCreate,
    openEdit,
    closeForm
  } = useCrudState<CorrespondenceDetailsDto>();

  const { data: res, isLoading, error: queryError } = useQuery({
    queryKey: queryKeys.correspondence.list({ direction, page, search: debouncedSearch, typeId }),
    queryFn: () => correspondenceApi.getAll({
      direction,
      page,
      pageSize,
      search: debouncedSearch,
      typeId: typeId === 0 ? undefined : typeId
    })
  });

  useEffect(() => {
    if (queryError) {
      toast.error("فشل تحميل السجلات", {
        description: queryError instanceof Error ? queryError.message : "حدث خطأ غير متوقع"
      });
    }
  }, [queryError]);

  const { data: typesRes } = useQuery({
    queryKey: queryKeys.correspondence.types,
    queryFn: () => correspondenceApi.getTypes()
  });

  const data = res?.data;
  const types = typesRes?.data || [];

  // Auto-navigate back if the current page becomes empty (e.g., after delete)
  useEffect(() => {
    if (data && data.items.length === 0 && page > 1) {
      setPage(p => p - 1);
    }
  }, [data, page]);

  const deleteMutation = useApiMutation(
    (id: number) => correspondenceApi.delete(id),
    {
      successMessage: "تم حذف القيد بنجاح",
      invalidateKeys: [queryKeys.correspondence.all],
    }
  );

  const handleDelete = useCallback(async (row: CorrespondenceListItemDto) => {
    const ok = await confirm({
      title: "حذف قيد وارد",
      description: `هل أنت متأكد من حذف القيد رقم (${row.referenceNumber})؟ لا يمكن التراجع عن هذا الإجراء.`,
      variant: "danger"
    });
    if (ok) deleteMutation.mutate(row.correspondenceID);
  }, [confirm, deleteMutation]);

  const columns = useMemo(() => [
    ...correspondenceColumns,
    {
      key: "actions",
      header: "الإجراءات",
      className: "w-24",
      render: (row: CorrespondenceListItemDto) => (
        <div className="flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
          <Button
            variant="ghost"
            size="icon"
            className="min-h-[44px] min-w-[44px] text-primary hover:bg-primary/10 transition-colors"
            title="عرض"
            onClick={() => navigate(`/correspondence/${row.correspondenceID}`)}
          >
            <Eye className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:bg-muted transition-colors"
            title="تعديل"
            onClick={async () => {
              const details = await correspondenceApi.getById(row.correspondenceID);
              openEdit(details.data);
            }}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="min-h-[44px] min-w-[44px] text-destructive hover:bg-destructive/10 transition-colors"
            title="حذف"
            onClick={() => handleDelete(row)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      )
    }
  ], [openEdit, handleDelete, navigate]);

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
        navigate(`/correspondence/${items[idx].correspondenceID}`);
      }
    } else if (e.key === "Escape") {
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }
  }, [data?.items, navigate]);

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
      title="سجل الوارد"
      description="توثيق وأرشفة جميع المراسلات والمعاملات الواردة للمؤسسة"
      createLabel="تسجيل وارد"
      onCreateClick={openCreate}
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
      icon={<FileDown className="h-6 w-6 text-primary" />}
    >
      <div className="mb-6" dir="rtl">
        <Tabs
          value={String(typeId || 0)}
          onValueChange={(v) => {
            setTypeId(v === "0" ? undefined : Number(v));
            setPage(1);
          }}
        >
          <div className="overflow-x-auto pb-1 scrollbar-hide">
            <TabsList className="bg-muted/50 p-1">
              <TabsTrigger value="0" className="px-6">الكل</TabsTrigger>
              {types.map((t: CorrespondenceTypeDto) => (
                <TabsTrigger key={t.correspondenceTypeID} value={String(t.correspondenceTypeID)} className="px-6">
                  {t.name}
                </TabsTrigger>
              ))}
            </TabsList>
          </div>
        </Tabs>
      </div>

      <CorrespondenceFormDialog
        open={isFormOpen}
        direction={direction}
        correspondence={selectedItem}
        onOpenChange={(open) => !open && closeForm()}
        onSaved={() => {
          setPage(1);
          queryClient.invalidateQueries({ queryKey: queryKeys.correspondence.all });
        }}
      />

      {ConfirmDialogElement}
    </CrudPageLayout>
  );
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}
