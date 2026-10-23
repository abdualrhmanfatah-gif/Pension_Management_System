import { useQuery } from "@tanstack/react-query";
import { CreditCard, Layers, Play, Printer, User, Users } from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { type ForceGlobalSearchDto, ministryApi } from "@/api/ministry";
import { SearchDialog } from "@/components/common/SearchDialog";
import { 
    Badge, 
    Button,
    CrudPageLayout,
    type CustomColumnDef
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { useHasPermission } from "@/stores/authStore";

export function MinistrySearchPage() {
    const navigate = useNavigate();
    const canManage = useHasPermission("Ministry.Manage", "System.Admin");
    const { searchTerm, setSearchTerm, debouncedValue: debouncedQuery } = useDebouncedSearch();

    const [focusedIndex, setFocusedIndex] = useState(-1);
    const focusedIndexRef = useRef(-1);
    const searchInputRef = useRef<HTMLInputElement>(null);

    // ── Debounce Search & Reset Focus ───────────────────
    useEffect(() => {
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, [debouncedQuery]);

    // ── Data Fetching ──────────────────────────────────
    const { data: resultsRes, isLoading, error, refetch } = useQuery({
        queryKey: ["force", "global-search", debouncedQuery],
        queryFn: () => ministryApi.searchGlobalForce(debouncedQuery),
        enabled: debouncedQuery.length >= 3
    });

    const results = resultsRes?.data || [];

    useEffect(() => {
        if (error) toast.error("تعذر تحميل نتائج البحث", { id: "global-search" });
    }, [error]);

    // ── Mutations ─────────────────────────────────────
    const processItemMutation = useApiMutation(
        (itemId: number) => ministryApi.processItem(itemId),
        {
            successMessage: "تمت معالجة القيد بنجاح",
            onSuccessCallback: () => refetch()
        }
    );

    // ── Keyboard Navigation ────────────────────────────
    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setFocusedIndex(prev => {
                const next = prev + 1;
                const clamped = next < results.length ? next : prev;
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
            if (idx >= 0 && idx < results.length) {
                const row = results[idx];
                if (row.retireeID) navigate(`/retirees/${row.retireeID}`);
            }
        } else if (e.key === "Escape") {
            setFocusedIndex(-1);
            focusedIndexRef.current = -1;
        }
    }, [results, navigate]);

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

    // ── Table Columns ──────────────────────────────────
    const columns: CustomColumnDef<ForceGlobalSearchDto>[] = [
        {
            key: "fullName",
            header: "الاسم الكامل",
            render: (row: ForceGlobalSearchDto) => (
                <div className="flex flex-col">
                    <div className="flex items-center gap-2">
                        <User className="w-3.5 h-3.5 text-primary" />
                        <span className="font-semibold text-sm">{row.fullName}</span>
                    </div>
                    <div className="flex items-center gap-2 font-mono text-[10px] sm:text-xs text-muted-foreground mt-0.5">
                        <CreditCard className="w-2.5 h-2.5" />
                        {row.militaryId}
                    </div>
                </div>
            )
        },
        { 
            key: "gender", 
            header: "الجنس", 
            render: (row) => (
                <span className="text-xs">
                    {row.gender === 1 ? "ذكر" : row.gender === 2 ? "أنثى" : "—"}
                </span>
            )
        },
        { key: "rankName", header: "الرتبة", accessor: (row) => row.rankName, className: "text-sm" },
        {
            key: "batchName",
            header: "الكشف",
            render: (row: ForceGlobalSearchDto) => (
                <div className="flex items-center gap-2">
                    <Layers className="w-3 h-3 text-muted-foreground" />
                    <span className="text-xs text-muted-foreground">{row.batchName || "---"}</span>
                </div>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: ForceGlobalSearchDto) => (
                <Badge variant={row.isProcessed ? "success" : "warning"} className="h-5 text-[10px] sm:text-xs px-2">
                    {row.status}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: ForceGlobalSearchDto) => (
                <div className="flex items-center gap-1">
                    {canManage && !row.isProcessed && (
                        <Button 
                            variant="ghost" 
                            size="sm" 
                            className="h-7 px-2 gap-1 text-xs text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/50"
                            onClick={(e) => { e.stopPropagation(); processItemMutation.mutate(row.approvalItemID); }}
                            loading={processItemMutation.isPending && processItemMutation.variables === row.approvalItemID}
                        >
                            <Play className="w-3 h-3 me-1" /> معالجة
                        </Button>
                    )}
                    {row.isProcessed && row.retireeID && (
                        <Button 
                            variant="ghost" 
                            size="sm" 
                            className="h-7 px-2 gap-1 text-xs"
                            onClick={(e) => { e.stopPropagation(); navigate(`/retirees/${row.retireeID}`); }}
                        >
                            <Users className="w-3 h-3 me-1" /> الملف
                        </Button>
                    )}
                    <a 
                        href={`/force/items/${row.approvalItemID}/print`}
                        target="_blank"
                        rel="noopener noreferrer"
                        onClick={(e) => e.stopPropagation()}
                        className="inline-flex items-center justify-center h-7 w-7 text-primary hover:bg-primary/10 rounded transition-colors"
                    >
                        <Printer className="w-3.5 h-3.5" />
                    </a>
                </div>
            )
        }
    ];

    return (
        <CrudPageLayout
            title="البحث عن محال"
            description="البحث الشامل في كافة كشوفات الإحالة إلى التقاعد"
            headerActions={
                <SearchDialog
                    value={searchTerm}
                    onSelect={(name) => setSearchTerm(name)}
                    queryKey={["force-global-search"]}
                    fetchFn={(term) =>
                        ministryApi
                            .searchGlobalForce(term)
                            .then((r) => ({ items: r.data || [] }))
                    }
                    getName={(item) => item.fullName}
                    getSubtitle={(item) => item.militaryId}
                    title="بحث عن محال"
                    placeholder="بحث بالاسم أو الرقم العسكري..."
                    minLength={3}
                    hideTrigger
                />
            }
            columns={columns}
            data={results}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="ابحث بالاسم أو الرقم العسكري (3 أحرف على الأقل)..."
            searchValue={searchTerm}
            onSearchChange={(val) => setSearchTerm(val)}
            onSearchKeyDown={handleSearchKeyDown}
            searchInputRef={searchInputRef}
            focusedRowIndex={focusedIndex}
            rowKey={(row) => row.approvalItemID}
            onRowClick={(row) => {
                if (row.retireeID) navigate(`/retirees/${row.retireeID}`);
            }}
            emptyMessage={
                debouncedQuery.length < 3 
                    ? "ابدأ الكتابة للبحث الشامل في كافة الكشوفات..." 
                    : "لا توجد نتائج مطابقة لبحثك."
            }
        />
    );
}
