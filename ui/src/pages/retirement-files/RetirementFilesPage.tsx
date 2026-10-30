import { useQuery, useQueryClient } from "@tanstack/react-query";
<<<<<<< HEAD
import { Eye, FileText, User, X } from "lucide-react";
=======
import { ChevronDown, Eye, Filter, User, X } from "lucide-react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

<<<<<<< HEAD
import { hrApi } from "@/api/hr";
import { lookupApi } from "@/api/pension";
import { retirementApi } from "@/api/retirement";
import { SearchDialog } from "@/components/common/SearchDialog";
import { DataEntryView } from "@/components/domain/retirement/DataEntryView";
import { FileStatusBadge } from "@/components/domain/retirement/FileStatusBadge";
import { RetirementFileCard } from "@/components/domain/retirement/RetirementFileCard";
import { RetirementFileDialog } from "@/components/domain/retirement/RetirementFileDialog";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
    Badge,
    Button,
    CrudPageLayout,
    type CustomColumnDef,
    NativeSelect,
    Skeleton,
} from "@/components/ui";
import { VIEW_MODE } from "@/constants";
import { queryKeys } from "@/constants/queryKeys";
import { useCreateFileIntent } from "@/constants/shortcuts";
import { useCreateShortcut,useCrudState, useRoleBasedUI } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { retirementFileCardClassName } from "@/lib/retirementFileCard";
import { MissingDocsCompletionPage } from "@/pages/documents/MissingDocsCompletionPage";
import { useHasPermission } from "@/stores/authStore";
=======
import { retirementApi } from "@/api/retirement";
import { lookupApi } from "@/api/pension";
import { hrApi } from "@/api/hr";
import { DataEntryView } from "@/components/domain/retirement/DataEntryView";
import { DocumentUploadView } from "@/components/domain/retirement/DocumentUploadView";
import { SearchDialog } from "@/components/common/SearchDialog";
import {
    Badge,
    Button,
    CrudPageLayout,
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
    type CustomColumnDef,
} from "@/components/ui";
import { RetirementFileDialog } from "@/components/domain/retirement/RetirementFileDialog";
import { VIEW_MODE } from "@/constants";
import { useCrudState, useRoleBasedUI } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { cn, getStatusColorClass } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type { RetirementFileListItem } from "@/types/retirement";

const GENDER_OPTIONS = [
    { id: 1, name: "ذكر" },
    { id: 2, name: "أنثى" },
];

/**
 * صفحة إدارة ملفات التقاعد الرئيسية
 */
export function RetirementFilesPage() {
    const navigate = useNavigate();
    const canCreate = useHasPermission("File.Create");
    const roles = useRoleBasedUI();
    const queryClient = useQueryClient();

    const crud = useCrudState<RetirementFileListItem>();
<<<<<<< HEAD
    const { openCreate } = crud;
    useCreateShortcut(crud.openCreate, canCreate);

    // Alt+N (global): open create dialog when intent arrives.
    // Data-entry view consumes it itself; document-uploader has no create.
    const { pending: createIntentPending, consume: consumeCreateIntent } = useCreateFileIntent();
    useEffect(() => {
        if (!createIntentPending) return;
        if (roles.primaryViewMode === VIEW_MODE.DATA_ENTRY) return;
        consumeCreateIntent();
        if (roles.primaryViewMode === VIEW_MODE.DOCUMENT_UPLOADER) return;
        if (canCreate) openCreate();
    }, [createIntentPending, consumeCreateIntent, roles.primaryViewMode, canCreate, openCreate]);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch();
    const [focusedIndex, setFocusedIndex] = useState(-1);
    const focusedIndexRef = useRef(-1);
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);
    const [cursor, setCursor] = useState<number | null | undefined>(undefined);
    const cursorHistory = React.useRef<number[]>([]);

    // ── Filters ──────────────────────────────────────────
    const [statusId, setStatusId] = useState<string>("");
    const [categoryId, setCategoryId] = useState<string>("");
    const [rankId, setRankId] = useState<string>("");
    const [gender, setGender] = useState<string>("");
    const [governorate, setGovernorate] = useState<string>("");
<<<<<<< HEAD
=======
    const [filtersOpen, setFiltersOpen] = useState<boolean>(false);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const filters = useMemo(() => ({
        statusId: statusId ? Number(statusId) : undefined,
        categoryId: categoryId ? Number(categoryId) : undefined,
        rankId: rankId ? Number(rankId) : undefined,
        gender: gender ? Number(gender) : undefined,
        governorate: governorate || undefined,
    }), [statusId, categoryId, rankId, gender, governorate]);

    const hasActiveFilters = !!(statusId || categoryId || rankId || gender || governorate);

    const clearFilters = () => {
        setStatusId("");
        setCategoryId("");
        setRankId("");
        setGender("");
        setGovernorate("");
    };



    // ── Debounce Search ────────────────────────────────
    useEffect(() => {
        setPage(1);
        setCursor(undefined);
        cursorHistory.current = [];
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, [debouncedSearch, statusId, categoryId, rankId, gender, governorate]);

    // ── Lookup data for filters ─────────────────────────
    const { data: statuses } = useQuery({
        queryKey: ["retirement-files", "statuses"],
        queryFn: () => retirementApi.getStatuses(),
        staleTime: 5 * 60_000,
    });
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

    const { data: filesData, isLoading, error: queryError } = useQuery({
        queryKey: [...queryKeys.retirement.files.all, page, debouncedSearch, cursor, filters],
        queryFn: () => retirementApi.getFiles({ page, pageSize: 25, search: debouncedSearch || undefined, cursor, ...filters })
    });

    React.useEffect(() => {
        if (queryError) {
            toast.error("فشل تحميل الملفات", {
                description: queryError instanceof Error ? queryError.message : "حدث خطأ غير متوقع"
            });
        }
    }, [queryError]);

    const onCursorNext = React.useCallback(() => {
        if (filesData?.nextCursor) {
            cursorHistory.current.push(filesData.nextCursor);
            setCursor(filesData.nextCursor);
            setPage((p) => p + 1);
        }
    }, [filesData?.nextCursor]);

    const onCursorPrev = React.useCallback(() => {
        const prev = cursorHistory.current.pop();
        setCursor(prev ?? undefined);
        setPage((p) => Math.max(1, p - 1));
    }, []);

    const onSearchChange = React.useCallback((val: string) => {
        setSearch(val);
<<<<<<< HEAD
    }, [setSearch]);
=======
    }, []);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const pagination = React.useMemo(() => filesData ? {
        page: filesData.page,
        pageSize: filesData.pageSize,
        total: filesData.totalCount,
        onPageChange: (p: number) => { setPage(p); setCursor(undefined); cursorHistory.current = []; },
        cursor,
        hasMore: filesData.hasMore,
        onCursorNext: cursor !== undefined ? onCursorNext : undefined,
        onCursorPrev: cursor !== undefined ? onCursorPrev : undefined,
    } : undefined, [filesData, cursor, onCursorNext, onCursorPrev]);


    const columns = useMemo<CustomColumnDef<RetirementFileListItem>[]>(() => [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row) => <span className="font-mono font-black text-primary">{row.fileNumber}</span>,
            sortable: true
        },
        {
            key: "retireeFullName",
            header: "المتقاعد",
            render: (row) => (
                <div className="flex flex-col text-start">
                    <span 
<<<<<<< HEAD
                        className="font-bold text-sm flex items-center gap-1.5 text-foreground"
=======
                        className={cn(
                            "font-bold text-sm flex items-center gap-1.5",
                            getStatusColorClass(row.statusID)
                        )}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    >
                        <User className="w-3.5 h-3.5 text-muted-foreground" /> {row.retireeFullName}
                    </span>
                    <span className="text-[10px] sm:text-xs text-muted-foreground pe-5">{row.rankName} — {row.nationalID}</span>
                </div>
            )
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row) => (
                <span className="text-sm font-mono font-bold text-primary">
                    {row.militaryNumber || <span className="text-muted-foreground italic">—</span>}
                </span>
            ),
        },
        {
            key: "statusName",
            header: "الحالة",
            render: (row) => (
                <div className="flex items-center gap-2">
<<<<<<< HEAD
                    <FileStatusBadge statusId={row.statusID} statusName={row.fileStatusName} />
                    {row.reviewStatus === 1 && (
                        <Badge variant="warning" className="font-bold">
=======
                    <Badge
                        variant={
                            row.fileStatusName?.includes("نشط") ? "success" :
                            row.fileStatusName?.includes("معلق") || row.fileStatusName?.includes("ناقص") ? "warning" :
                            "outline"
                        }
                        className="font-bold"
                    >
                        {row.fileStatusName}
                    </Badge>
                    {row.reviewStatus === 1 && (
                        <Badge variant="warning" className="font-bold animate-pulse">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            مراجعة
                        </Badge>
                    )}
                </div>
            )
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex justify-center">
                    <Button
                        size="sm"
                        variant="ghost-action"
                        onClick={() => navigate(`/files/${row.fileID}`)}
                    >
                        <Eye className="w-4 h-4" /> عرض الملف
                    </Button>
                </div>
            ),
        },
    ], [navigate]);

    // ── Keyboard Navigation ────────────────────────────
    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        const items = filesData?.items || [];
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
                navigate(`/files/${items[idx].fileID}`);
            }
        } else if (e.key === "Escape") {
            setFocusedIndex(-1);
            focusedIndexRef.current = -1;
        }
    }, [filesData?.items, navigate]);

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

<<<<<<< HEAD
    const filterFields = (
        <div className="grid grid-cols-2 gap-2 lg:grid-cols-5">
            <NativeSelect label="حالة الملف" value={statusId} onChange={setStatusId}
                options={[{ value: "", label: "جميع الحالات" }, ...(statuses ?? []).map(s => ({ value: String(s.statusID), label: s.statusName }))]} />
            <NativeSelect label="الفئة" value={categoryId} onChange={setCategoryId}
                options={[{ value: "", label: "جميع الفئات" }, ...(categories ?? []).map(c => ({ value: String(c.id), label: c.name }))]} />
            <NativeSelect label="الرتبة" value={rankId} onChange={setRankId}
                options={[{ value: "", label: "جميع الرتب" }, ...(ranks ?? []).map(r => ({ value: String(r.rankID), label: r.rankName }))]} />
            <NativeSelect label="الجنس" value={gender} onChange={setGender}
                options={[{ value: "", label: "الكل" }, ...GENDER_OPTIONS.map(g => ({ value: String(g.id), label: g.name }))]} />
            <div className="col-span-2 lg:col-span-1">
                <NativeSelect label="المحافظة" value={governorate} onChange={setGovernorate}
                    options={[{ value: "", label: "جميع المحافظات" }, ...(governorates ?? []).map(g => ({ value: g.name, label: g.name }))]} />
            </div>
        </div>
    );
    const clearFiltersButton = hasActiveFilters ? (
        <Button variant="ghost" size="sm" onClick={clearFilters} className="text-destructive">
            <X className="h-4 w-4" aria-hidden="true" />مسح الفلاتر
        </Button>
    ) : null;

    // معالجة العرض حسب الصلاحية
    if (roles.primaryViewMode === VIEW_MODE.DATA_ENTRY) return <DataEntryView />;
    if (roles.primaryViewMode === VIEW_MODE.DOCUMENT_UPLOADER) return <MissingDocsCompletionPage />;

    return (
        <div className="min-w-0">
            <CrudPageLayout
                density="compact"
                title="ملفات التقاعد"
                titleSuffix={
                    isLoading ? (
                        <Skeleton className="h-6 w-16 rounded-full" />
                    ) : (
                        <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-2.5 py-0.5 text-sm font-bold text-primary tabular-nums">
                            <FileText className="w-3.5 h-3.5" aria-hidden="true" />
                            {(filesData?.totalCount || 0).toLocaleString("en-US")}
                        </span>
                    )
                }
                description="ملفات تقاعد مسجلة في النظام"
=======
    // معالجة العرض حسب الصلاحية
    if (roles.primaryViewMode === VIEW_MODE.DATA_ENTRY) return <DataEntryView />;
    if (roles.primaryViewMode === VIEW_MODE.DOCUMENT_UPLOADER) return <DocumentUploadView />;

    return (
        <div className="space-y-6">
            <CrudPageLayout
                title="ملفات التقاعد"
                description={`${filesData?.totalCount || 0} ملف تقاعد مسجل في النظام`}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                createLabel="فتح ملف جديد"
                onCreateClick={canCreate ? crud.openCreate : undefined}
                columns={columns}
                data={filesData?.items || []}
                loading={isLoading}
                searchable
                searchInHeader
<<<<<<< HEAD
                searchLabel="البحث عن ملف"
                searchPlaceholder="الاسم أو الرقم العسكري أو رقم الملف"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                onSearchChange={onSearchChange}
                searchValue={search}
                onSearchKeyDown={handleSearchKeyDown}
                searchInputRef={searchInputRef}
                focusedRowIndex={focusedIndex}
                onRowClick={(row) => navigate(`/files/${row.fileID}`)}
                pagination={pagination}
                emptyMessage="لم يتم العثور على أي ملفات تقاعد."
<<<<<<< HEAD
                rowKey={(row) => row.fileID}
                mobileScrollMode="page"
                rowClassName={retirementFileCardClassName}
                renderMobileCard={(item) => (
                    <RetirementFileCard file={item}>
                        {(item.militaryNumber || item.rankName || item.reviewStatus === 1) && (
                            <div className="flex min-w-0 items-center gap-2 text-xs text-muted-foreground">
                                {item.militaryNumber && <p className="shrink-0 whitespace-nowrap">العسكري: <bdi dir="ltr" className="font-mono font-semibold text-foreground">{item.militaryNumber}</bdi></p>}
                                {item.rankName && <p className="min-w-0 flex-1 truncate" title={item.rankName}>{item.rankName}</p>}
                                {item.reviewStatus === 1 && <Badge variant="warning" className="shrink-0">مراجعة</Badge>}
                            </div>
                        )}
                    </RetirementFileCard>
                )}
            >
                <SearchDialog
                    value={search}
                    onSelect={(val) => { setSearch(val); setPage(1); }}
                    queryKey={["retirement-files", "dialog-search"]}
                    fetchFn={(term) => retirementApi.getFiles({ page: 1, pageSize: 100, search: term || undefined })}
                    getName={(f) => f.retireeFullName}
                    getSubtitle={(f) => f.fileNumber}
                    title="بحث بالاسم"
                    placeholder="اكتب الرقم العسكري أو اسم المتقاعد..."
                    hideTrigger
                />
                <Accordion type="single" collapsible className="sm:hidden">
                    <AccordionItem value="file-filters" className="border-0">
                        <AccordionTrigger density="compact">
                            <span className="flex items-center gap-2">
                                الفلاتر
                                {hasActiveFilters && <Badge variant="secondary">{[statusId, categoryId, rankId, gender, governorate].filter(Boolean).length} مفعّل</Badge>}
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
                    {clearFiltersButton}
=======
                headerActions={
                    <SearchDialog
                        value={search}
                        onSelect={(val) => { setSearch(val); setPage(1); }}
                        queryKey={["retirement-files", "dialog-search"]}
                        fetchFn={(term) => retirementApi.getFiles({ page: 1, pageSize: 100, search: term || undefined })}
                        getName={(f) => f.retireeFullName}
                        getSubtitle={(f) => f.fileNumber}
                        title="بحث بالاسم"
                        placeholder="اكتب اسم المتقاعد أو رقم الملف..."
                        hideTrigger
                    />
                }
                renderMobileCard={(item) => (
                    <div className="p-3 space-y-1.5">
                        <div className="flex items-center justify-between">
                            <span className="font-mono font-black text-primary text-xs">{item.fileNumber}</span>
                            <Badge
                                variant={
                                    item.fileStatusName?.includes("نشط") ? "success" :
                                    item.fileStatusName?.includes("معلق") || item.fileStatusName?.includes("ناقص") ? "warning" :
                                    "outline"
                                }
                                className="text-[8px] h-3.5 font-bold"
                            >
                                {item.fileStatusName}
                            </Badge>
                        </div>
                        <div className="flex items-center gap-1 flex-wrap">
                            <span className={cn("font-bold text-xs flex items-center gap-1", getStatusColorClass(item.statusID))}>
                                <User className="w-3 h-3 text-muted-foreground" /> {item.retireeFullName}
                            </span>
                            <span className="text-[10px] text-muted-foreground">— {item.rankName}</span>
                        </div>
                        <div className="flex items-center justify-between text-[10px] text-muted-foreground">
                            <span>الرقم العسكري: <span className="font-mono font-bold text-primary">{item.militaryNumber || "—"}</span></span>
                            {item.reviewStatus === 1 && (
                                <Badge variant="warning" className="text-[8px] h-3.5 animate-pulse">مراجعة</Badge>
                            )}
                        </div>
                        <div className="flex justify-end">
                            <Button size="sm" variant="ghost" className="min-h-[28px] h-7 text-[10px] px-2 gap-1 text-primary" onClick={() => navigate(`/files/${item.fileID}`)}>
                                <Eye className="w-3 h-3" /> عرض الملف
                            </Button>
                        </div>
                    </div>
                )}
            >
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
                                    {[statusId, categoryId, rankId, gender, governorate].filter(Boolean).length}
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
                            "grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3",
                            !filtersOpen && "hidden"
                        )}
                    >
                        {/* الحالة */}
                        <div className="space-y-1.5">
                            <label className="text-xs font-medium text-muted-foreground">الحالة</label>
                            <Select value={statusId || "all"} onValueChange={(v) => setStatusId(v === "all" ? "" : v)}>
                                <SelectTrigger>
                                    <SelectValue placeholder="جميع الحالات" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">جميع الحالات</SelectItem>
                                    {statuses?.map((s) => (
                                        <SelectItem key={s.statusID} value={s.statusID.toString()}>
                                            {s.statusName}
                                        </SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {/* الفئة */}
                        <div className="space-y-1.5">
                            <label className="text-xs font-medium text-muted-foreground">الفئة</label>
                            <Select value={categoryId || "all"} onValueChange={(v) => setCategoryId(v === "all" ? "" : v)}>
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
                            <Select value={rankId || "all"} onValueChange={(v) => setRankId(v === "all" ? "" : v)}>
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
                            <Select value={gender || "all"} onValueChange={(v) => setGender(v === "all" ? "" : v)}>
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
                            <Select value={governorate || "all"} onValueChange={(v) => setGovernorate(v === "all" ? "" : v)}>
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

                <RetirementFileDialog
                    open={crud.isFormOpen}
                    onOpenChange={crud.closeForm}
                    onCreated={(id) => navigate(`/files/${id}`)}
                    onSuccess={async () => { await queryClient.invalidateQueries({ queryKey: queryKeys.retirement.files.all }); }}
                />
            </CrudPageLayout>
        </div>
    );
}

