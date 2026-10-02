import { useQuery } from "@tanstack/react-query";
import { Play, Printer, Search, Users } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import { discrepancyApi, type DiscrepancyFilter,type RetireeDiscrepancy } from "@/api/discrepancy";
import { ministryApi } from "@/api/ministry";
import {
  Badge,
  Button,
  Card,
  CrudPageLayout,
  Input,
  NativeSelect,
  Text,
  useConfirm
} from "@/components/ui";
import type { CustomColumnDef } from "@/components/ui/DataTable";
import { useApiMutation, useRoleBasedUI } from "@/hooks";

export function DiscrepancyReportPage() {
  const navigate = useNavigate();
  const { hasPermission } = useRoleBasedUI();
  const { confirm, ConfirmDialogElement } = useConfirm();
  const [filter, setFilter] = useState<DiscrepancyFilter>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: undefined,
    stage: undefined
  });
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);

  // ── Debounce Search ────────────────────────────────
  useEffect(() => {
    const timer = setTimeout(() => {
      setFilter(prev => ({ ...prev, pageNumber: 1 }));
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }, 500);
    return () => clearTimeout(timer);
  }, [filter.searchTerm]);

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

  const formatDate = useCallback((dateString?: string) => {
    if (!dateString) return "---";
    try {
      const d = new Date(dateString);
      if (isNaN(d.getTime())) return "---";
      const day = String(d.getDate()).padStart(2, '0');
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const year = d.getFullYear();
      return `${day}/${month}/${year}`;
    } catch {
      return "---";
    }
  }, []);

  const { data, isLoading, refetch } = useQuery({
    queryKey: ["discrepancy-report", filter],
    queryFn: () => discrepancyApi.getReport(filter),
  });

  const processMutation = useApiMutation(
    (id: number) => ministryApi.processItem(id),
    {
      successMessage: "تمت معالجة القيد بنجاح",
      onSuccess: () => refetch()
    }
  );

  const handleProcess = useCallback(async (row: RetireeDiscrepancy) => {
    if (!row.approvalItemID) return;

    const ok = await confirm({
      title: "تأكيد معالجة القيد",
      description: `هل أنت متأكد من معالجة المتقاعد ${row.approvalFullName || row.militaryNumber}؟ سيتم إنشاء سجل متقاعد معتمد في النظام.`
    });

    if (ok) {
      processMutation.mutate(row.approvalItemID);
    }
  }, [confirm, processMutation]);

  const items = useMemo(() => (data?.items || []) as unknown as RetireeDiscrepancy[], [data]);

  // ── Keyboard Navigation ────────────────────────────
  const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
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
        if (!row.isProcessed && row.approvalItemID && hasPermission("Ministry.Manage")) {
          handleProcess(row);
        }
      }
    } else if (e.key === "Escape") {
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }
  }, [items, hasPermission, handleProcess]);

  const columns = useMemo<CustomColumnDef<RetireeDiscrepancy>[]>(() => [
    {
      key: "militaryNumber",
      header: "الرقم العسكري",
      accessor: (row) => row.militaryNumber,
      className: "text-center font-bold",
    },
    {
      key: "fullName",
      header: "الاسم الرباعي",
      render: (row) => row.retireeFullName || row.approvalFullName || "---",
    },
    {
      key: "rank",
      header: "الرتبة",
      render: (row) => row.fileRankName || row.approvalRankName || "---",
    },
    {
      key: "unitName",
      header: "الوحدة",
      accessor: (row) => row.approvalUnitName || "---",
      className: "text-sm",
    },
    {
      key: "retirementDate",
      header: "تاريخ التقاعد",
      render: (row) => formatDate(row.fileRetirementDate || row.approvalRetirementDate),
      className: "text-center",
    },
    {
      key: "currentStage",
      header: "المرحلة الحالية",
      className: "text-center",
      render: (row) => {
        let variant: "default" | "success" | "warning" | "secondary" = "secondary";
        if (row.fileID) variant = "success";
        else if (row.retireeID) variant = "warning";

        return (
          <Badge variant={variant}>
            {row.currentStage === "Stage 3: File Created" ? "ملف مكتمل" :
              row.currentStage === "Stage 2: Processed" ? "تمت المعالجة" : "كشف إحالة فقط"}
          </Badge>
        );
      },
    },
    {
      key: "actions",
      header: "إجراءات",
      className: "text-center",
      render: (row) => (
        <div className="flex items-center justify-center gap-2">
          {!row.isProcessed && row.approvalItemID && hasPermission("Ministry.Manage") && (
            <Button
              variant="ghost"
              size="sm"
              className="gap-1 text-emerald-700 hover:text-emerald-800 hover:bg-emerald-50"
              onClick={() => handleProcess(row)}
              loading={processMutation.isPending && processMutation.variables === row.approvalItemID}
            >
              <Play className="w-3.5 h-3.5" />
              معالجة
            </Button>
          )}
          {row.retireeID && (
            <Button
              variant="ghost"
              size="sm"
              className="gap-1 text-slate-700 hover:text-slate-800 hover:bg-slate-50"
              onClick={() => navigate(`/retirees/${row.retireeID}`)}
            >
              <Users className="w-3.5 h-3.5" />
              الملف
            </Button>
          )}
          {row.approvalItemID ? (
            <Button
              variant="ghost"
              size="sm"
              className="gap-1 text-blue-700 hover:text-blue-800 hover:bg-blue-50"
              onClick={() => window.open(`/force/items/${row.approvalItemID}/print`, '_blank')}
            >
              <Printer className="w-3.5 h-3.5" />
              طباعة
            </Button>
          ) : row.retireeID ? (
            <Button
              variant="ghost"
              size="sm"
              className="gap-1 text-blue-700 hover:text-blue-800 hover:bg-blue-50"
              onClick={() => window.open(`/retirees/${row.retireeID}/statement`, '_blank')}
            >
              <Printer className="w-3.5 h-3.5" />
              طباعة
            </Button>
          ) : null}
        </div>
      ),
    },
  ], [hasPermission, processMutation, confirm, handleProcess, formatDate]);

  return (
    <>
      <CrudPageLayout
        title="مباينة بيانات المتقاعدين"
        description="عرض بيانات المتقاعدين المستخرجة من كشوفات الإحالة"
        columns={columns}
        data={items}
        loading={isLoading}
        focusedRowIndex={focusedIndex}
        pagination={{
          page: filter.pageNumber,
          pageSize: filter.pageSize,
          total: data?.totalCount || 0,
          onPageChange: (page) => setFilter(prev => ({ ...prev, pageNumber: page })),
        }}
        headerActions={
          <Button
            variant="outline"
            onClick={() => {
              const params = new URLSearchParams();
              if (filter.searchTerm) params.set("searchTerm", filter.searchTerm);
              if (filter.stage) params.set("stage", filter.stage);
              navigate(`/admin/discrepancy-report/print?${params.toString()}`);
            }}
            className="gap-2"
          >
            <Printer className="h-4 w-4" />
            طباعة التقرير
          </Button>
        }
      >
        <Card className="p-4 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
            <div className="space-y-2">
              <Text variant="label">البحث</Text>
              <div className="relative">
                <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  ref={searchInputRef}
                  placeholder="اسم، رقم عسكري، رقم ملف..."
                  className="ps-10"
                  value={filter.searchTerm}
                  onChange={(e) => setFilter(prev => ({ ...prev, searchTerm: e.target.value }))}
                  onKeyDown={handleSearchKeyDown}
                />
              </div>

            </div>

            <div className="space-y-2">
              <Text variant="label">المرحلة</Text>
              <NativeSelect
                placeholder="كل المراحل"
                options={[
                  { label: "كشف إحالة فقط", value: "Stage 1" },
                  { label: "تمت المعالجة", value: "Stage 2" },
                  { label: "ملف مكتمل", value: "Stage 3" },
                ]}
                value={filter.stage || ""}
                onChange={(val) => setFilter(prev => ({ ...prev, stage: val || undefined, pageNumber: 1 }))}
              />
            </div>

            <Badge variant="outline" className="h-10 justify-center text-xs">
              إجمالي السجلات: {data?.totalCount || 0}
            </Badge>
          </div>
        </Card>
      </CrudPageLayout>
      {ConfirmDialogElement}
    </>
  );
}
