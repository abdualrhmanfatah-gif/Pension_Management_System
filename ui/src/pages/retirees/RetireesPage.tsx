import { useQuery, useQueryClient } from "@tanstack/react-query";
<<<<<<< HEAD
import { Eye, FileText, FolderPlus, MoreHorizontal, Printer, Trash2, Users, X } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import { hrApi } from "@/api/hr";
import { lookupApi } from "@/api/pension";
import { retireeApi, type RetireeFilters, type RetireeListItem } from "@/api/retirees";
import { SearchDialog } from "@/components/common/SearchDialog";
import { CreateFileWizard } from "@/components/domain/retirement/CreateFileWizard";
import { Badge, BaseFormDialog, Button, CrudPageLayout, Skeleton } from "@/components/ui";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/Accordion";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/DropdownMenu";
import { NativeSelect } from "@/components/ui/NativeSelect";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation,useCrudState } from "@/hooks";
import { useConfirm } from "@/hooks/useConfirm";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { useHasPermission } from "@/stores/authStore";
=======
import { ChevronDown, Eye, FileText, Filter, FolderPlus, Printer, Trash2, Users, X } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import { retireeApi, type RetireeFilters, type RetireeListItem } from "@/api/retirees";
import { lookupApi } from "@/api/pension";
import { hrApi } from "@/api/hr";
import { SearchDialog } from "@/components/common/SearchDialog";
import { CreateFileWizard } from "@/components/domain/retirement/CreateFileWizard";
import { Badge, BaseFormDialog, Button, CrudPageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { useCrudState, useApiMutation } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { useConfirm } from "@/hooks/useConfirm";
import { queryKeys } from "@/constants/queryKeys";
import { useHasPermission } from "@/stores/authStore";
import { cn } from "@/lib/utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Page ──────────────────────────────────────────────

const GENDER_OPTIONS = [
    { id: 1, name: "ذكر" },
    { id: 2, name: "أنثى" },
];

export function RetireesPage() {
    const navigate = useNavigate();
    const crud = useCrudState<RetireeListItem>();
    const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch();
    const [focusedIndex, setFocusedIndex] = useState(-1);
    const focusedIndexRef = useRef(-1);
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);
    const pageSize = 50;

    // ── Filters ──────────────────────────────────────────
    const [categoryId, setCategoryId] = useState<string>("");
    const [rankId, setRankId] = useState<string>("");
    const [gender, setGender] = useState<string>("");
    const [governorate, setGovernorate] = useState<string>("");
<<<<<<< HEAD
=======
    const [filtersOpen, setFiltersOpen] = useState<boolean>(false);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const filters: RetireeFilters = useMemo(() => ({
        categoryId: categoryId ? Number(categoryId) : undefined,
        rankId: rankId ? Number(rankId) : undefined,
        gender: gender ? Number(gender) : undefined,
        governorate: governorate || undefined,
    }), [categoryId, rankId, gender, governorate]);

    const hasActiveFilters = !!(categoryId || rankId || gender || governorate);

    const clearFilters = () => {
        setCategoryId("");
        setRankId("");
        setGender("");
        setGovernorate("");
    };

    // ── Debounce Search ────────────────────────────────
    useEffect(() => {
        setPage(1);
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, [debouncedSearch, categoryId, rankId, gender, governorate]);

    // ── Lookup data for filters ─────────────────────────
    const { data: categories } = useQuery({
        queryKey: ["lookups", "retiree-categories"],
        queryFn: () => lookupApi.getCategories(),
        staleTime: 5 * 60_000,
    });
    const { data: governorates } = useQuery({
        queryKey: ["lookups", "governorates"],
        queryFn: () => lookupApi.getGovernorates(),
        staleTime: 5 * 60_000,
    });
    const { data: ranks } = useQuery({
        queryKey: ["hr", "ranks"],
        queryFn: () => hrApi.getRanks(),
        staleTime: 5 * 60_000,
    });

    const queryClient = useQueryClient();
    const canCreateFile = useHasPermission("File.Create");
    const canViewDetails = useHasPermission("Retiree.View");
    const canViewFiles = useHasPermission("Retiree.View", "File.View.Sensitive", "File.Create", "File.Edit.BasicData");
    const canDelete = useHasPermission("Retiree.Delete");

    const { confirm, ConfirmDialogElement } = useConfirm();

    const deleteMutation = useApiMutation(
        (id: number) => retireeApi.delete(id),
        {
            successMessage: "تم حذف المتقاعد بنجاح",
            onSuccess: () => {
                refetch();
                queryClient.invalidateQueries({ queryKey: queryKeys.retirees.all });
            }
        }
    );

    const { data: retireesData, isLoading, refetch } = useQuery({
        queryKey: queryKeys.retirees.list({ page, pageSize, search: debouncedSearch, ...filters }),
        queryFn: () => retireeApi.getAll({ page, pageSize, search: debouncedSearch || undefined, ...filters })
    });

    const retirees = retireesData?.items || [];
    const totalCount = retireesData?.totalCount || 0;

<<<<<<< HEAD
    const handleDelete = async (row: RetireeListItem) => {
        const confirmed = await confirm({
            title: "حذف متقاعد",
            description: `هل أنت متأكد من حذف المتقاعد ${row.fullName}؟`,
            confirmLabel: "حذف",
            cancelLabel: "إلغاء",
            variant: "danger",
        });
        if (confirmed) deleteMutation.mutate(row.retireeID);
    };

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const columns = [
        {
            key: "fullName",
            header: "الاسم",
            render: (row: RetireeListItem) => (
                <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                        <Users className="w-4 h-4" />
                    </div>
                    <div>
                        <p className="font-medium text-foreground">{row.fullName}</p>
                    </div>
                </div>
            ),
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row: RetireeListItem) => (
                <span className="text-sm font-mono">
                    {row.nationalID || <span className="text-muted-foreground italic">غير معبأ</span>}
                </span>
            ),
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: RetireeListItem) => (
                <span className="text-sm font-mono text-primary font-bold">
                    {row.militaryNumber || <span className="text-muted-foreground italic">—</span>}
                </span>
            ),
        },
        {
<<<<<<< HEAD
            key: "rankName",
            header: "الرتبة",
            render: (row: RetireeListItem) => (
                <span className="text-sm">{row.rankName || "—"}</span>
            ),
        },
        {
            key: "category",
            header: "الفئة",
            render: (row: RetireeListItem) => {
                const name = categories?.find((c) => c.id === row.categoryId)?.name;
                return <span className="text-sm">{name || "—"}</span>;
            },
        },
        {
=======
            key: "phone",
            header: "الهاتف",
            render: (row: RetireeListItem) => (
                <span className="text-sm text-muted-foreground">{row.phone || "—"}</span>
            ),
        },
        {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            key: "status",
            header: "الحالة",
            render: (row: RetireeListItem) => (
                <Badge variant={row.isAlive ? "success" : "outline"}>
                    {row.isAlive ? "على قيد الحياة" : "متوفى"}
                </Badge>
            ),
        },
        {
<<<<<<< HEAD
=======
            key: "fileCount",
            header: "الملفات",
            render: (row: RetireeListItem) => (
                <span className="text-sm font-medium">{row.fileCount}</span>
            ),
        },
        {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            key: "actions",
            header: "",
            render: (row: RetireeListItem) => (
                <div className="flex items-center gap-1">
                    {canDelete && (
                        <Button
                            variant="ghost-action"
                            size="sm"
<<<<<<< HEAD
                            className="text-destructive hover:text-destructive hover:bg-destructive/10 dark:hover:bg-destructive/20"
                            title="حذف المتقاعد"
                            aria-label="حذف المتقاعد"
                            onClick={(e) => {
                                e.stopPropagation();
                                void handleDelete(row);
=======
                            title="حذف المتقاعد"
                            onClick={async (e) => {
                                e.stopPropagation();
                                const confirmed = await confirm({
                                    title: "حذف متقاعد",
                                    description: `هل أنت متأكد من حذف المتقاعد ${row.fullName}؟`,
                                    confirmLabel: "حذف",
                                    cancelLabel: "إلغاء",
                                    variant: "danger"
                                });
                                if (confirmed) {
                                    deleteMutation.mutate(row.retireeID);
                                }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            }}
                        >
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                    {canViewDetails && (
                        <Button
                            variant="ghost-action"
                            size="sm"
                            title="طباعة المباينة"
<<<<<<< HEAD
                            aria-label="طباعة المباينة"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            onClick={(e) => {
                                e.stopPropagation();
                                navigate(`/retirees/${row.retireeID}/statement`);
                            }}
                        >
                            <Printer className="w-4 h-4" />
                        </Button>
                    )}
                    {row.fileCount === 0 ? (
                        <>
                            {canCreateFile && (
                                <Button
                                    variant="ghost-action"
                                    size="sm"
                                    onClick={() => {
                                        crud.openEdit(row);
                                    }}
                                >
                                    <FolderPlus className="w-4 h-4 ms-1" />
                                    إنشاء ملف
                                </Button>
                            )}
                            {canViewDetails && (
<<<<<<< HEAD
                                <Button
                                    variant="ghost-action"
                                    size="sm"
                                    onClick={() => navigate(`/retirees/${row.retireeID}`)}
                                >
=======
                                <Button variant="ghost-action" size="sm" onClick={() => navigate(`/retirees/${row.retireeID}`)}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    <Eye className="w-4 h-4 ms-1" />
                                    التفاصيل
                                </Button>
                            )}
                        </>
                    ) : (
                        <div className="flex items-center gap-1">
                            {canViewFiles && (
                                <Button
                                    variant="ghost-action"
                                    size="sm"
                                    onClick={() => navigate(`/files/${row.fileID}`)}
                                    title="عرض ملف التقاعد"
                                >
                                    <FileText className="w-4 h-4 ms-1" />
                                    عرض الملف
                                </Button>
                            )}
                            {canViewDetails && (
                                <Button
                                    variant="ghost-action"
                                    size="sm"
                                    onClick={() => navigate(`/retirees/${row.retireeID}`)}
                                    title="عرض تفاصيل المتقاعد"
<<<<<<< HEAD
                                    aria-label="عرض تفاصيل المتقاعد"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                >
                                    <Eye className="w-4 h-4" />
                                </Button>
                            )}
                        </div>
                    )}
                </div>
            ),
        },
    ];

<<<<<<< HEAD
    const renderMobileCard = (row: RetireeListItem) => {
        const categoryName = categories?.find((category) => category.id === row.categoryId)?.name;
        return (
            <div className="space-y-2 p-2.5">
                <div className="flex items-start gap-2">
                    <h2 className="min-w-0 flex-1 break-words text-base font-bold leading-relaxed text-foreground">
                        {row.fullName}
                    </h2>
                    <Badge variant={row.isAlive ? "success" : "secondary"} className="shrink-0">
                        {row.isAlive ? "على قيد الحياة" : "متوفى"}
                    </Badge>
                </div>
                <div className="flex min-w-0 items-center gap-2 text-xs text-muted-foreground">
                    <p className="shrink-0 whitespace-nowrap">
                        العسكري: <bdi dir="ltr" className="font-mono font-semibold text-foreground">{row.militaryNumber || "—"}</bdi>
                    </p>
                    <p className="min-w-0 flex-1 truncate" title={`الرقم الوطني: ${row.nationalID || "غير معبأ"}`}>
                        الوطني: <bdi dir="ltr" className="font-mono text-foreground">{row.nationalID || "غير معبأ"}</bdi>
                    </p>
                </div>
                {(row.rankName || categoryName) && (
                    <p className="truncate text-xs text-muted-foreground" title={[row.rankName, categoryName].filter(Boolean).join(" · ")}>
                        {[row.rankName, categoryName].filter(Boolean).join(" · ")}
                    </p>
                )}
                {(canViewDetails || canCreateFile || canViewFiles || canDelete) && (
                    <div className="flex items-center gap-2">
                        {row.fileCount === 0 && canCreateFile && (
                            <Button variant="default" size="sm" className="min-w-0 flex-1" onClick={() => crud.openEdit(row)}>
                                <FolderPlus className="h-4 w-4 shrink-0" aria-hidden="true" />
                                إنشاء ملف
                            </Button>
                        )}
                        {canViewDetails && (
                            <Button
                                variant="outline"
                                size="sm"
                                className="min-w-0 flex-1"
                                onClick={() => navigate(`/retirees/${row.retireeID}/statement`)}
                            >
                                <Printer className="h-4 w-4 shrink-0" aria-hidden="true" />
                                طباعة المباينة
                            </Button>
                        )}
                        {(canViewDetails || (row.fileID && canViewFiles) || canDelete) && (
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="outline" size="icon" className="shrink-0" aria-label="إجراءات المتقاعد" title="المزيد">
                                        <MoreHorizontal className="h-4 w-4" aria-hidden="true" />
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end">
                                    {canViewDetails && (
                                        <DropdownMenuItem onSelect={() => navigate(`/retirees/${row.retireeID}`)}>
                                            <Eye className="h-4 w-4" aria-hidden="true" />
                                            تفاصيل المتقاعد
                                        </DropdownMenuItem>
                                    )}
                                    {row.fileID && canViewFiles && (
                                        <DropdownMenuItem onSelect={() => navigate(`/files/${row.fileID}`)}>
                                            <FileText className="h-4 w-4" aria-hidden="true" />
                                            عرض ملف التقاعد
                                        </DropdownMenuItem>
                                    )}
                                    {canDelete && (
                                        <DropdownMenuItem className="text-destructive" onSelect={() => { void handleDelete(row); }}>
                                            <Trash2 className="h-4 w-4" aria-hidden="true" />
                                            حذف المتقاعد
                                        </DropdownMenuItem>
                                    )}
                                </DropdownMenuContent>
                            </DropdownMenu>
                        )}
                    </div>
                )}
            </div>
        );
    };

    const filterFields = (
        <div className="grid grid-cols-2 gap-2 lg:grid-cols-4">
            <NativeSelect
                label="الفئة"
                value={categoryId}
                onChange={setCategoryId}
                options={[
                    { value: "", label: "جميع الفئات" },
                    ...(categories ?? []).map((category) => ({ value: String(category.id), label: category.name })),
                ]}
            />
            <NativeSelect
                label="الرتبة"
                value={rankId}
                onChange={setRankId}
                options={[
                    { value: "", label: "جميع الرتب" },
                    ...(ranks ?? []).map((rank) => ({ value: String(rank.rankID), label: rank.rankName })),
                ]}
            />
            <NativeSelect
                label="الجنس"
                value={gender}
                onChange={setGender}
                options={[
                    { value: "", label: "الكل" },
                    ...GENDER_OPTIONS.map((option) => ({ value: String(option.id), label: option.name })),
                ]}
            />
            <NativeSelect
                label="المحافظة"
                value={governorate}
                onChange={setGovernorate}
                options={[
                    { value: "", label: "جميع المحافظات" },
                    ...(governorates ?? []).map((option) => ({ value: option.name, label: option.name })),
                ]}
            />
        </div>
    );
    const clearFiltersButton = hasActiveFilters ? (
        <Button variant="ghost" size="sm" onClick={clearFilters} className="text-destructive">
            <X className="h-4 w-4" aria-hidden="true" />
            مسح الفلاتر
        </Button>
    ) : null;

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    // ── Keyboard Navigation ────────────────────────────
    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setFocusedIndex(prev => {
                const next = prev + 1;
                const clamped = next < retirees.length ? next : prev;
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
            if (idx >= 0 && idx < retirees.length) {
                navigate(`/retirees/${retirees[idx].retireeID}`);
            }
        } else if (e.key === "Escape") {
            setFocusedIndex(-1);
            focusedIndexRef.current = -1;
        }
    }, [retirees, navigate]);

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
<<<<<<< HEAD
            density="compact"
            title="المتقاعدون"
            titleSuffix={
                isLoading ? (
                    <Skeleton className="h-6 w-16 rounded-full" />
                ) : (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-2.5 py-0.5 text-sm font-bold text-primary tabular-nums">
                        <Users className="w-3.5 h-3.5" aria-hidden="true" />
                        {totalCount.toLocaleString("en-US")}
                    </span>
                )
            }
=======
            title="المتقاعدون"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            description="سجل المتقاعدين مع إمكانية البحث والمباينة"
            columns={columns}
            data={retirees}
            loading={isLoading}
            searchable
            searchInHeader
<<<<<<< HEAD
            searchLabel="البحث عن متقاعد"
            searchPlaceholder="الاسم أو الرقم العسكري"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            onSearchChange={(val) => setSearch(val)}
            searchValue={search}
            onSearchKeyDown={handleSearchKeyDown}
            searchInputRef={searchInputRef}
            focusedRowIndex={focusedIndex}
<<<<<<< HEAD
            renderMobileCard={renderMobileCard}
            mobileScrollMode="page"
            rowClassName={(row) => row.isAlive
                ? "max-sm:bg-[color:color-mix(in_srgb,hsl(var(--gov-success))_8%,hsl(var(--card)))]"
                : "max-sm:bg-muted"}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            onRowClick={(row) => { if (row.fileID) navigate(`/files/${row.fileID}`); else navigate(`/retirees/${row.retireeID}`); }}
            headerActions={
                <SearchDialog
                    value={search}
                    onSelect={(val) => { setSearch(val); setPage(1); }}
                    queryKey={["retirees", "dialog-search"]}
                    fetchFn={(term) => retireeApi.getAll({ page: 1, pageSize: 100, search: term || undefined })}
                    getName={(r) => r.fullName}
                    getSubtitle={(r) => r.nationalID || r.militaryNumber || ""}
                    title="بحث بالاسم"
                    placeholder="اكتب اسم المتقاعد..."
                    hideTrigger
                />
            }
            rowKey={(row) => row.retireeID}
            pagination={{
                page,
                pageSize,
                total: totalCount,
                onPageChange: (p) => setPage(p)
            }}
        >
<<<<<<< HEAD
            <Accordion type="single" collapsible className="sm:hidden">
                <AccordionItem value="retiree-filters" className="border-0">
                    <AccordionTrigger density="compact">
                        <span className="flex items-center gap-2">
                            الفلاتر
                            {hasActiveFilters && (
                                <Badge variant="secondary">{[categoryId, rankId, gender, governorate].filter(Boolean).length} مفعّل</Badge>
                            )}
                        </span>
                    </AccordionTrigger>
                    <AccordionContent className="space-y-1 pb-0">
                        {filterFields}
                        {clearFiltersButton}
                    </AccordionContent>
                </AccordionItem>
            </Accordion>
            <div className="hidden space-y-1 sm:block">
           
                {filterFields}
=======
            {/* ── Filters Bar ───────────────────────────────── */}
            <div className="rounded-2xl border border-border/70 bg-card/50 p-3 sm:p-4 space-y-3">
                <div className="flex items-center justify-between flex-wrap gap-2">
                    <button
                        type="button"
                        onClick={() => setFiltersOpen((v) => !v)}
                        className="flex items-center gap-2 text-sm font-semibold text-foreground/80 hover:text-foreground transition-colors"
                        aria-expanded={filtersOpen}
                        aria-controls="filters-grid"
                    >
                        <Filter className="w-4 h-4" />
                        <span>الفلاتر</span>
                        {hasActiveFilters && (
                            <span className="inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 rounded-full bg-primary/15 text-primary text-xs font-bold">
                                {[categoryId, rankId, gender, governorate].filter(Boolean).length}
                            </span>
                        )}
                        <ChevronDown
                            className={cn(
                                "w-4 h-4 text-muted-foreground transition-transform duration-200",
                                filtersOpen && "rotate-180"
                            )}
                        />
                    </button>
                    {hasActiveFilters && (
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={clearFilters}
                            className="h-8 gap-1.5 text-destructive hover:text-destructive"
                        >
                            <X className="w-3.5 h-3.5" />
                            مسح الفلاتر
                        </Button>
                    )}
                </div>

                <div
                    id="filters-grid"
                    className={cn(
                        "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3",
                        !filtersOpen && "hidden"
                    )}
                >
                    {/* الفئة */}
                    <div className="space-y-1.5">
                        <label className="text-xs font-medium text-muted-foreground">الفئة</label>
                        <Select
                            value={categoryId || "all"}
                            onValueChange={(v) => setCategoryId(v === "all" ? "" : v)}
                        >
                            <SelectTrigger>
                                <SelectValue placeholder="جميع الفئات" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">جميع الفئات</SelectItem>
                                {categories?.map((c) => (
                                    <SelectItem key={c.id} value={c.id.toString()}>
                                        {c.name}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {/* الرتبة */}
                    <div className="space-y-1.5">
                        <label className="text-xs font-medium text-muted-foreground">الرتبة</label>
                        <Select
                            value={rankId || "all"}
                            onValueChange={(v) => setRankId(v === "all" ? "" : v)}
                        >
                            <SelectTrigger>
                                <SelectValue placeholder="جميع الرتب" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">جميع الرتب</SelectItem>
                                {ranks?.map((r) => (
                                    <SelectItem key={r.rankID} value={r.rankID.toString()}>
                                        {r.rankName}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {/* الجنس */}
                    <div className="space-y-1.5">
                        <label className="text-xs font-medium text-muted-foreground">الجنس</label>
                        <Select
                            value={gender || "all"}
                            onValueChange={(v) => setGender(v === "all" ? "" : v)}
                        >
                            <SelectTrigger>
                                <SelectValue placeholder="الكل" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">الكل</SelectItem>
                                {GENDER_OPTIONS.map((g) => (
                                    <SelectItem key={g.id} value={g.id.toString()}>
                                        {g.name}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {/* المحافظة */}
                    <div className="space-y-1.5">
                        <label className="text-xs font-medium text-muted-foreground">المحافظة</label>
                        <Select
                            value={governorate || "all"}
                            onValueChange={(v) => setGovernorate(v === "all" ? "" : v)}
                        >
                            <SelectTrigger>
                                <SelectValue placeholder="جميع المحافظات" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">جميع المحافظات</SelectItem>
                                {governorates?.map((g) => (
                                    <SelectItem key={g.id} value={g.name}>
                                        {g.name}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                </div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            </div>
            <BaseFormDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                title="فتح ملف تقاعد جديد"
                description={crud.selectedItem
                    ? `فتح ملف تقاعد للمتقاعد: ${crud.selectedItem.fullName}. أكمل الخطوات التالية.`
                    : "قم بإدخال بيانات ملف التقاعد الجديد في الخطوات التالية."}
                size="4xl"
                footer={null}
            >
                {crud.selectedItem && (
                    <CreateFileWizard
                        key={crud.selectedItem.retireeID}
                        initialRetireeId={crud.selectedItem.retireeID}
                        initialRetireeName={crud.selectedItem.fullName}
                        onSuccess={() => {
                            crud.closeForm();
                            refetch();
                        }}
                        onCancel={() => {
                            crud.closeForm();
                        }}
                    />
                )}
            </BaseFormDialog>
            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
