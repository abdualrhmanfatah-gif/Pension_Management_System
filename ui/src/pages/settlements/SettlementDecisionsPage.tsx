import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Calendar, FileText, Hash,Trash2 } from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import type { SettlementDecisionDto } from "@/api/settlements";
import { settlementApi } from "@/api/settlements";
<<<<<<< HEAD
import { SearchDialog } from "@/components/common/SearchDialog";
import { CreateSettlementDecisionDialog } from "@/components/domain/settlements/CreateSettlementDecisionDialog";
import { Badge, Button, CrudPageLayout, useConfirm } from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
=======
import { Badge, Button, CrudPageLayout, useConfirm } from "@/components/ui";
import { CreateSettlementDecisionDialog } from "@/components/domain/settlements/CreateSettlementDecisionDialog";
import { SearchDialog } from "@/components/common/SearchDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation,useCrudState } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
<<<<<<< HEAD
=======
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Page ──────────────────────────────────────────────

export function SettlementDecisionsPage() {
    const navigate = useNavigate();
    const canManage = useHasPermission("Settlement.Manage");
    const { confirm, ConfirmDialogElement } = useConfirm();
    const queryClient = useQueryClient();
    const crudState = useCrudState<SettlementDecisionDto>();

    const { searchTerm, setSearchTerm, debouncedValue: debouncedSearch } = useDebouncedSearch();
    const [focusedIndex, setFocusedIndex] = useState(-1);
    const focusedIndexRef = useRef(-1);
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);
    const pageSize = 20;

    // ── Debounce Search ────────────────────────────────
    useEffect(() => {
        setPage(1);
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, [debouncedSearch]);

    const { data: pagedResponse, isLoading: loading, error: queryError } = useQuery({
        queryKey: queryKeys.settlements.decisions.list({ page, pageSize, searchTerm: debouncedSearch }),
        queryFn: () => settlementApi.getAllDecisions({ page, pageSize, searchTerm: debouncedSearch || undefined })
    });

    useEffect(() => {
        if (queryError) {
            toast.error("فشل تحميل القرارات", {
                description: queryError instanceof Error ? queryError.message : "حدث خطأ غير متوقع"
            });
        }
    }, [queryError]);

    const decisions = pagedResponse?.items || [];
    const totalCount = pagedResponse?.totalCount || 0;

    // Auto-navigate back if the current page becomes empty (e.g., after delete)
    useEffect(() => {
        if (pagedResponse && decisions.length === 0 && page > 1) {
            setPage(p => p - 1);
        }
    }, [pagedResponse, decisions.length, page]);

    const deleteMutation = useApiMutation(
        async (id: number) => { await settlementApi.deleteDecision(id); },
        {
            successMessage: "تم حذف القرار بنجاح",
            onSuccessCallback: () => {
                queryClient.invalidateQueries({ queryKey: queryKeys.settlements.decisions.all });
            }
        }
    );

    const handleDelete = async (id: number) => {
        const confirmed = await confirm({
            title: "تأكيد الحذف",
            description: "هل أنت متأكد من حذفه هذا القرار وجميع بنوده؟",
            variant: "danger",
        });
        if (!confirmed) return;
        deleteMutation.mutate(id);
    };

    const columns = [
        {
            key: "decisionNumber",
            header: "رقم القرار",
            render: (row: SettlementDecisionDto) => (
                <span className="font-mono font-bold text-primary">{row.decisionNumber}</span>
            )
        },
        {
            key: "decisionDate",
            header: "تاريخ القرار",
            render: (row: SettlementDecisionDto) => (
                <div className="flex items-center gap-1.5 text-sm">
                    <Calendar className="w-3.5 h-3.5 text-muted-foreground" />
                    {formatDate(row.decisionDate)}
                </div>
            )
        },
        {
            key: "decisionType",
            header: "النوع",
            render: (row: SettlementDecisionDto) => (
                <Badge variant="outline">{row.decisionType || "تسوية"}</Badge>
            )
        },
        {
            key: "itemCount",
            header: "عدد القيود",
            render: (row: SettlementDecisionDto) => (
                <div className="flex items-center gap-1.5">
                    <Hash className="w-3.5 h-3.5 text-muted-foreground" />
                    <span className="font-medium">{row.itemCount}</span>
                </div>
            )
        },
        {
            key: "totalAmount",
            header: "إجمالي المبلغ",
            render: (row: SettlementDecisionDto) => (
                <span className="font-medium">{row.totalAmount?.toLocaleString("ar-YE") || "0"} ر.ي</span>
            )
        },
        {
            key: "createdBy",
            header: "بواسطة",
            render: (row: SettlementDecisionDto) => (
                <span className="text-sm text-muted-foreground">{row.createdByUserName || "-"}</span>
            )
        },
        {
            key: "actions",
            header: "",
            render: (row: SettlementDecisionDto) => (
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="icon"
                        className="min-h-[44px] min-w-[44px]"
                        onClick={(e) => { e.stopPropagation(); navigate(`/settlements/${row.decisionID}`); }}
                    >
                        <FileText className="w-4 h-4" />
                    </Button>
                    {canManage && (
                        <Button
                            variant="ghost"
                            size="icon"
                            className="min-h-[44px] min-w-[44px] text-destructive hover:text-destructive"
                            onClick={(e) => { e.stopPropagation(); handleDelete(row.decisionID); }}
                            loading={deleteMutation.isPending && deleteMutation.variables === row.decisionID}
                        >
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    // ── Keyboard Navigation ────────────────────────────
    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setFocusedIndex(prev => {
                const next = prev + 1;
                const clamped = next < decisions.length ? next : prev;
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
            if (idx >= 0 && idx < decisions.length) {
                navigate(`/settlements/${decisions[idx].decisionID}`);
            }
        } else if (e.key === "Escape") {
            setFocusedIndex(-1);
            focusedIndexRef.current = -1;
        }
    }, [decisions, navigate]);

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
            title="قرارات التسوية"
            description="إدارة قرارات تسوية الرتب الصادرة من الوزارة"
            createLabel="إضافة قرار جديد"
            onCreateClick={canManage ? () => crudState.openCreate() : undefined}
            headerActions={
                <SearchDialog
                    value={searchTerm}
                    onSelect={(name) => setSearchTerm(name)}
                    queryKey={["settlement-decisions-search"]}
                    fetchFn={(term) =>
                        settlementApi.getAllDecisions({ page: 1, pageSize: 100, searchTerm: term })
                    }
                    getName={(item) => item.decisionNumber}
                    getSubtitle={(item) => item.decisionType || "تسوية"}
                    title="بحث عن قرار"
                    placeholder="بحث برقم القرار..."
                    hideTrigger
                />
            }
            columns={columns}
            data={decisions}
            loading={loading}
            searchable
            searchInHeader
            searchPlaceholder="بحث برقم أو نوع القرار..."
            searchValue={searchTerm}
            onSearchChange={(val) => setSearchTerm(val)}
            onSearchKeyDown={handleSearchKeyDown}
            searchInputRef={searchInputRef}
            focusedRowIndex={focusedIndex}
            rowKey={(row) => row.decisionID}
            onRowClick={(row) => navigate(`/settlements/${row.decisionID}`)}
            pagination={{
                page,
                pageSize,
                total: totalCount,
                onPageChange: (p) => setPage(p)
            }}
        >
            <CreateSettlementDecisionDialog
                open={crudState.isFormOpen}
                onOpenChange={(open) => !open && crudState.closeForm()}
                onSaved={() => { setPage(1); queryClient.invalidateQueries({ queryKey: ["settlement-decisions"] }); }}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
