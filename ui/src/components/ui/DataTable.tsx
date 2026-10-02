import type { ColumnDef } from "@tanstack/react-table";
import { useVirtualizer } from "@tanstack/react-virtual";
import {
    ArrowDown,
    ArrowUp,
    ArrowUpDown,
    ChevronLeft,
    ChevronRight,
    ChevronsLeft,
    ChevronsRight,
    Inbox,
} from "lucide-react";
import * as React from "react";

import { cn } from "@/lib/utils";

import { Button } from "./Button";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
} from "./Sheet";
import { Skeleton } from "./Skeleton";

const ROW_VIRTUALIZATION_THRESHOLD = 200;
const ESTIMATED_ROW_HEIGHT = 56;
const MOBILE_CARD_ESTIMATED_HEIGHT = 90;

// ─── Types ─────────────────────────────────────────────

export type SortDirection = "asc" | "desc" | null;

export interface PaginationConfig {
    page: number;
    pageSize: number;
    total: number;
    onPageChange: (page: number) => void;
    pageSizeOptions?: number[];
    onPageSizeChange?: (pageSize: number) => void;
    cursor?: number | null;
    hasMore?: boolean;
    onCursorNext?: () => void;
    onCursorPrev?: () => void;
}

export type CustomColumnDef<T> = ColumnDef<T, unknown> & {
    key: string;
    header: string;
    sortable?: boolean;
    className?: string;
    accessor?: (row: T) => string | number | boolean | React.ReactNode;
    render?: (row: T, index: number) => React.ReactNode;
};

export interface DataTableProps<T> {
    columns: CustomColumnDef<T>[];
    data: T[];
    loading?: boolean;
    pagination?: PaginationConfig;
    emptyMessage?: string;
    emptyIcon?: React.ReactNode;
    sortColumn?: string;
    sortDirection?: SortDirection;
    onSort?: (column: string, direction: SortDirection) => void;
    rowKey?: (row: T, index: number) => string | number;
    onRowClick?: (row: T) => void;
    focusedRowIndex?: number;
    className?: string;
    stickyHeader?: boolean;
    title?: React.ReactNode;
    drawerTitle?: (row: T) => string;
    drawerDescription?: (row: T) => string;
    renderDrawerContent?: (row: T) => React.ReactNode;
    renderMobileCard?: (row: T, index: number) => React.ReactNode;
    rowClassName?: (row: T) => string | undefined;
    dir?: "rtl" | "ltr";
    maxHeight?: string;
}

// ─── Mobile Card View (virtualized to prevent OOM) ──────

interface MobileCardViewProps<T> {
    data: T[];
    rowKey?: (row: T, index: number) => string | number;
    dataColumns: CustomColumnDef<T>[];
    actionsColumn?: CustomColumnDef<T>;
    renderCell: (row: T, i: number, col: CustomColumnDef<T>) => React.ReactNode;
    renderMobileCard?: (row: T, index: number) => React.ReactNode;
    onRowClick: (row: T) => void;
    hasRowInteraction: boolean;
    maxHeight?: string;
}

function MobileCardView<T>({
    data,
    rowKey,
    dataColumns,
    actionsColumn,
    renderCell,
    renderMobileCard,
    onRowClick,
    hasRowInteraction,
    maxHeight,
}: MobileCardViewProps<T>) {
    const scrollContainerRef = React.useRef<HTMLDivElement>(null);

    const mobileVirtualizer = useVirtualizer({
        count: data.length,
        getScrollElement: () => scrollContainerRef.current,
        estimateSize: () => MOBILE_CARD_ESTIMATED_HEIGHT,
        overscan: 5,
        gap: 2,
    });

    const virtualItems = mobileVirtualizer.getVirtualItems();
    const totalSize = mobileVirtualizer.getTotalSize();

    return (
        <div
            ref={scrollContainerRef}
            className="p-0 md:p-3 overflow-y-auto"
            style={{ maxHeight: maxHeight ?? 'calc(100dvh - 260px)' }}
        >
            <div style={{ height: `${totalSize}px`, position: 'relative' }}>
                {virtualItems.map((virtualRow) => {
                    const i = virtualRow.index;
                    const row = data[i];
                    const key = rowKey ? rowKey(row, i) : `mobile-row-${i}`;

                    return (
                        <div
                            key={key}
                            data-index={virtualRow.index}
                            ref={mobileVirtualizer.measureElement}
                            style={{
                                position: 'absolute',
                                top: 0,
                                left: 0,
                                width: '100%',
                                transform: `translateY(${virtualRow.start}px)`,
                            }}
                            className={cn(
                                "bg-card rounded-xl border border-border/80 shadow-sm overflow-hidden",
                                hasRowInteraction && "cursor-pointer active:scale-[0.99] transition-transform"
                            )}
                            onClick={() => onRowClick(row)}
                            role="button"
                            tabIndex={0}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter' || e.key === ' ') {
                                    e.preventDefault();
                                    onRowClick(row);
                                }
                            }}
                        >
                            {renderMobileCard ? (
                                renderMobileCard(row, i)
                            ) : (
                                <div>
                                    {dataColumns.map((col, idx) => (
                                        <div key={col.key} className={cn(
                                            "flex items-center gap-2 px-3 py-2",
                                            idx < dataColumns.length - 1 && "border-b border-border/60"
                                        )}>
                                            <span className="text-[10px] font-bold text-muted-foreground shrink-0 min-w-[60px] text-end">
                                                {col.header}
                                            </span>
                                            <span className="text-xs flex-1 text-start font-medium break-words min-w-0">
                                                {renderCell(row, i, col)}
                                            </span>
                                        </div>
                                    ))}
                                    {actionsColumn && (
                                        <div className="flex justify-center items-center gap-2 px-3 py-2 bg-muted/20 border-t border-border/60">
                                            {renderCell(row, i, actionsColumn)}
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

// ─── DataTable ─────────────────────────────────────────

function DataTable<T>({
    columns,
    data: unsafeData,
    loading = false,
    pagination,
    emptyMessage = "لا توجد بيانات",
    emptyIcon,
    sortColumn,
    sortDirection,
    onSort,
    rowKey,
    onRowClick,
    focusedRowIndex,
    className,
    stickyHeader = true,
    title,
    drawerTitle,
    drawerDescription,
    renderDrawerContent,
    renderMobileCard,
    rowClassName,
    dir = "rtl",
    maxHeight,
}: DataTableProps<T>) {
    const data = React.useMemo(() => {
        if (!unsafeData) return [];
        return Array.isArray(unsafeData) ? unsafeData : [];
    }, [unsafeData]);

    const [selectedRow, setSelectedRow] = React.useState<T | null>(null);
    const [drawerOpen, setDrawerOpen] = React.useState(false);

    const actionsColumn = React.useMemo(
        () => columns.find((c) => c.key === "actions"),
        [columns]
    );
    const dataColumns = React.useMemo(
        () => columns.filter((c) => c.key !== "actions"),
        [columns]
    );

    const scrollRef = React.useRef<HTMLDivElement>(null);
    const shouldVirtualize = data.length > ROW_VIRTUALIZATION_THRESHOLD;

    const rowVirtualizer = useVirtualizer({
        count: data.length,
        getScrollElement: () => scrollRef.current,
        estimateSize: () => ESTIMATED_ROW_HEIGHT,
        overscan: 10,
    });

    // Scroll to focused row when it changes
    React.useEffect(() => {
        if (
            focusedRowIndex !== undefined &&
            focusedRowIndex >= 0 &&
            shouldVirtualize
        ) {
            rowVirtualizer.scrollToIndex(focusedRowIndex, { align: "center" });
        }
    }, [focusedRowIndex, shouldVirtualize, rowVirtualizer]);

    const handleRowClick = React.useCallback(
        (row: T) => {
            if (renderDrawerContent) {
                setSelectedRow(row);
                setDrawerOpen(true);
            }
            onRowClick?.(row);
        },
        [renderDrawerContent, onRowClick]
    );

    const handleSort = React.useCallback(
        (column: CustomColumnDef<T>) => {
            if (!column.sortable || !onSort) return;

            let direction: SortDirection = "asc";
            if (sortColumn === column.key) {
                if (sortDirection === "asc") direction = "desc";
                else if (sortDirection === "desc") direction = null;
            }

            onSort(column.key, direction);
        },
        [sortColumn, sortDirection, onSort]
    );

    const totalPages = React.useMemo(() => {
        if (!pagination) return 0;
        return Math.ceil(pagination.total / pagination.pageSize);
    }, [pagination]);

    const getPageNumbers = React.useCallback((): number[] => {
        if (!pagination) return [];
        const pages: number[] = [];
        const maxVisible = 5;
        let start = Math.max(1, pagination.page - Math.floor(maxVisible / 2));
        const end = Math.min(totalPages, start + maxVisible - 1);
        start = Math.max(1, end - maxVisible + 1);

        for (let i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    }, [pagination, totalPages]);

    const renderCell = React.useCallback(
        (row: T, i: number, col: CustomColumnDef<T>) => {
            if (col.render) return col.render(row, i);
            if (col.accessor) return col.accessor(row);
            const value = (row as Record<string, unknown>)[col.key];
            return value as React.ReactNode;
        },
        []
    );

    const renderRowCells = React.useCallback(
        (row: T, i: number) =>
            columns.map((col) => (
                <td
                    key={col.key}
                    className={cn(
                        "px-3 sm:px-4 py-3.5 text-start align-middle text-sm font-medium text-table-foreground whitespace-normal sm:whitespace-nowrap transition-colors duration-300",
                        col.className
                    )}
                >
                    {renderCell(row, i, col)}
                </td>
            )),
        [columns, renderCell]
    );

    const renderTr = React.useCallback(
        (row: T, i: number, extraClassName?: string) => {
            const key = rowKey ? rowKey(row, i) : `row-${i}`;
            const isClickable = !!(onRowClick || renderDrawerContent);

            return (
                <tr
                    key={key}
                    data-row-index={i}
                    className={cn(
                        "hover:bg-primary/5 hover:scale-[1.001] transition-all duration-300 border-b border-border/40 last:border-0",
                        isClickable && "cursor-pointer",
                        focusedRowIndex === i && "bg-primary/10 ring-1 ring-primary/30",
                        rowClassName?.(row),
                        extraClassName
                    )}
                    onClick={() => handleRowClick(row)}
                    role={isClickable ? "button" : undefined}
                    tabIndex={isClickable ? 0 : undefined}
                    onKeyDown={(e) => {
                        if (isClickable && (e.key === "Enter" || e.key === " ")) {
                            e.preventDefault();
                            handleRowClick(row);
                        }
                    }}
                >
                    {renderRowCells(row, i)}
                </tr>
            );
        },
        [
            rowKey,
            onRowClick,
            renderDrawerContent,
            focusedRowIndex,
            rowClassName,
            handleRowClick,
            renderRowCells,
        ]
    );

    const virtualItems = rowVirtualizer.getVirtualItems();
    const totalSize = rowVirtualizer.getTotalSize();

    const isCursorMode =
        pagination &&
        pagination.cursor !== undefined &&
        pagination.cursor !== null;

    return (
        <div className={cn("space-y-4", className)} dir={dir}>
            {title && (
                <div className="flex items-center justify-between px-2">
                    <h4 className="text-sm font-black text-primary/80 tracking-wide">
                        {title}
                    </h4>
                </div>
            )}
            <div className="rounded-2xl border border-border/70 overflow-hidden bg-card transition-all duration-300">
                <div
                    ref={scrollRef}
                    className="overflow-y-auto overflow-x-auto hidden sm:block"
                    style={{ maxHeight: maxHeight ?? "calc(100dvh - 260px)" }}
                >
                    <table className="w-full text-sm min-w-full sm:min-w-[680px] border-collapse">
                        {/* ─── Header ────────────────────────────── */}
                        <thead
                            className={cn(
                                "bg-muted border-b border-border/80",
                                stickyHeader && "sticky top-0 z-10"
                            )}
                        >
                            <tr>
                                {columns.map((col) => (
                                    <th
                                        key={col.key}
                                        scope="col"
                                        className={cn(
                                            "px-3 sm:px-4 py-3.5 text-start font-bold text-table-foreground whitespace-normal sm:whitespace-nowrap bg-muted border-b border-border/60 transition-colors duration-200",
                                            col.sortable &&
                                                "cursor-pointer select-none hover:text-primary hover:bg-muted/40",
                                            col.className
                                        )}
                                        onClick={() => handleSort(col)}
                                        aria-sort={
                                            sortColumn === col.key
                                                ? sortDirection === "asc"
                                                    ? "ascending"
                                                    : sortDirection === "desc"
                                                        ? "descending"
                                                        : "none"
                                                : undefined
                                        }
                                    >
                                        <span className="inline-flex items-center gap-2 text-xs sm:text-[13px] font-extrabold tracking-wide">
                                            {col.header}
                                            {col.sortable && (
                                                <span
                                                    className="text-muted-foreground/40"
                                                    aria-hidden="true"
                                                >
                                                    {sortColumn === col.key &&
                                                        sortDirection === "asc" ? (
                                                        <ArrowUp className="h-4 w-4 text-primary" />
                                                    ) : sortColumn === col.key &&
                                                        sortDirection === "desc" ? (
                                                        <ArrowDown className="h-4 w-4 text-primary" />
                                                    ) : (
                                                        <ArrowUpDown className="h-4 w-4" />
                                                    )}
                                                </span>
                                            )}
                                        </span>
                                    </th>
                                ))}
                            </tr>
                        </thead>

                        {/* ─── Body ──────────────────────────────── */}
                        {shouldVirtualize && !loading ? (
                            <tbody className="divide-y divide-border/40 relative">
                                {virtualItems.length > 0 &&
                                    virtualItems[0].start > 0 && (
                                        <tr aria-hidden="true">
                                            <td
                                                colSpan={columns.length}
                                                style={{
                                                    height: `${virtualItems[0].start}px`,
                                                    padding: 0,
                                                    border: 0,
                                                }}
                                            />
                                        </tr>
                                    )}
                                {virtualItems.map((virtualRow) => {
                                    const row = data[virtualRow.index];
                                    const key = rowKey
                                        ? rowKey(row, virtualRow.index)
                                        : `vrow-${virtualRow.index}`;
                                    const isClickable = !!(onRowClick || renderDrawerContent);

                                    return (
                                        <tr
                                            key={key}
                                            data-index={virtualRow.index}
                                            data-row-index={virtualRow.index}
                                            ref={rowVirtualizer.measureElement}
                                            className={cn(
                                                "hover:bg-primary/5 transition-all duration-300 border-b border-border/40",
                                                isClickable && "cursor-pointer",
                                                focusedRowIndex === virtualRow.index &&
                                                    "bg-primary/10 ring-1 ring-primary/30"
                                            )}
                                            onClick={() => handleRowClick(row)}
                                            role={isClickable ? "button" : undefined}
                                            tabIndex={isClickable ? 0 : undefined}
                                            onKeyDown={(e) => {
                                                if (
                                                    isClickable &&
                                                    (e.key === "Enter" || e.key === " ")
                                                ) {
                                                    e.preventDefault();
                                                    handleRowClick(row);
                                                }
                                            }}
                                        >
                                            {renderRowCells(row, virtualRow.index)}
                                        </tr>
                                    );
                                })}
                                {virtualItems.length > 0 &&
                                    virtualItems[virtualItems.length - 1].end < totalSize && (
                                        <tr aria-hidden="true">
                                            <td
                                                colSpan={columns.length}
                                                style={{
                                                    height: `${totalSize - virtualItems[virtualItems.length - 1].end}px`,
                                                    padding: 0,
                                                    border: 0,
                                                }}
                                            />
                                        </tr>
                                    )}
                            </tbody>
                        ) : (
                            <tbody className="divide-y divide-border/40">
                                {/* Loading state */}
                                {loading &&
                                    Array.from({ length: 5 }).map((_, i) => (
                                        <tr key={`skeleton-${i}`} aria-hidden="true">
                                            {columns.map((col) => (
                                                <td key={col.key} className="px-4 py-3.5">
                                                    <Skeleton className="h-4 w-full max-w-[200px]" />
                                                </td>
                                            ))}
                                        </tr>
                                    ))}

                                {/* Empty state */}
                                {!loading && data.length === 0 && (
                                    <tr>
                                        <td
                                            colSpan={columns.length}
                                            className="px-4 py-20 text-center bg-transparent"
                                        >
                                            <div
                                                className="flex flex-col items-center gap-3 text-muted-foreground"
                                                role="status"
                                                aria-live="polite"
                                            >
                                                {emptyIcon || (
                                                    <Inbox
                                                        className="h-12 w-12 opacity-30 text-primary/40"
                                                        aria-hidden="true"
                                                    />
                                                )}
                                                <p className="text-base font-bold">{emptyMessage}</p>
                                                {!emptyIcon && (
                                                    <p className="text-xs text-muted-foreground mt-1">
                                                        لم يتم العثور على سجلات تطابق معايير البحث
                                                    </p>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                )}

                                {/* Data rows */}
                                {!loading && data.map((row, i) => renderTr(row, i))}
                            </tbody>
                        )}
                    </table>
                </div>

                {/* ─── Mobile Card View (virtualized) ──────────── */}
                {!loading && data.length > 0 && (
                    <div className="block sm:hidden">
                        <MobileCardView
                            data={data}
                            rowKey={rowKey}
                            dataColumns={dataColumns}
                            actionsColumn={actionsColumn}
                            renderCell={renderCell}
                            renderMobileCard={renderMobileCard}
                            onRowClick={handleRowClick}
                            hasRowInteraction={!!(onRowClick || renderDrawerContent)}
                            maxHeight={maxHeight}
                        />
                    </div>
                )}

                {/* ─── Empty state (visible on mobile when no cards rendered) ─── */}
                {!loading && data.length === 0 && (
                    <div className="block sm:hidden px-4 py-16 text-center bg-transparent">
                        <div
                            className="flex flex-col items-center gap-3 text-muted-foreground"
                            role="status"
                            aria-live="polite"
                        >
                            {emptyIcon || (
                                <Inbox
                                    className="h-12 w-12 opacity-30 text-primary/40"
                                    aria-hidden="true"
                                />
                            )}
                            <p className="text-base font-bold">{emptyMessage}</p>
                        </div>
                    </div>
                )}

                {/* ─── Pagination ───────────────────────────── */}
                {pagination && (totalPages > 1 || pagination.pageSizeOptions) && (
                    <div className="flex flex-col sm:flex-row items-center justify-between gap-2 px-3 sm:px-4 py-2 bg-muted/10 border-t border-border/60">
                        <div className="flex items-center gap-2 order-2 sm:order-1">
                            <p className="text-[10px] text-muted-foreground font-medium whitespace-nowrap sm:whitespace-normal break-words">
                                عرض{" "}
                                <span className="font-semibold text-foreground">
                                    {pagination.total > 0
                                        ? (
                                            (pagination.page - 1) * pagination.pageSize +
                                            1
                                        ).toLocaleString()
                                        : 0}
                                </span>
                                {" - "}
                                <span className="font-semibold text-foreground">
                                    {Math.min(
                                        pagination.page * pagination.pageSize,
                                        pagination.total
                                    ).toLocaleString()}
                                </span>
                                {" من "}
                                <span className="font-semibold text-foreground">
                                    {pagination.total.toLocaleString()}
                                </span>
                                {" سجل"}
                            </p>

                            {pagination.pageSizeOptions && pagination.onPageSizeChange && (
                                <div className="flex items-center gap-1.5">
                                    <span className="text-[10px] text-muted-foreground">عرض</span>
                                    <select
                                        value={pagination.pageSize}
                                        onChange={(e) =>
                                            pagination.onPageSizeChange?.(Number(e.target.value))
                                        }
                                        className="h-6 text-[10px] rounded-md border border-border/60 bg-background px-1.5 py-0.5 focus:outline-none focus:ring-2 focus:ring-primary/30"
                                        aria-label="عدد السجلات في الصفحة"
                                    >
                                        {pagination.pageSizeOptions.map((size) => (
                                            <option key={size} value={size}>
                                                {size}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            )}
                        </div>

                        {/* Cursor mode: simplified Prev/Next */}
                        {isCursorMode ? (
                            <div className="flex items-center gap-1 order-1 sm:order-2">
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={
                                        !pagination.onCursorPrev ||
                                        !pagination.page ||
                                        pagination.page <= 1
                                    }
                                    onClick={() => pagination.onCursorPrev?.()}
                                    className="h-7 w-7 text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="السابق"
                                    aria-label="الصفحة السابقة"
                                >
                                    <ChevronRight className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                <span
                                    className="text-[10px] text-muted-foreground font-medium mx-1"
                                    aria-live="polite"
                                >
                                    الصفحة {pagination.page}
                                </span>

                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={!pagination.hasMore || !pagination.onCursorNext}
                                    onClick={() => pagination.onCursorNext?.()}
                                    className="h-7 w-7 text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="التالي"
                                    aria-label="الصفحة التالية"
                                >
                                    <ChevronLeft className="h-4 w-4" aria-hidden="true" />
                                </Button>
                            </div>
                        ) : (
                            <div
                                className="flex items-center gap-1 order-1 sm:order-2"
                                role="navigation"
                                aria-label="التنقل بين الصفحات"
                            >
                                {/* First */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page <= 1}
                                    onClick={() => pagination.onPageChange(1)}
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة الأولى"
                                    aria-label="الصفحة الأولى"
                                >
                                    <ChevronsRight className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                {/* Previous */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page <= 1}
                                    onClick={() => pagination.onPageChange(pagination.page - 1)}
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة السابقة"
                                    aria-label="الصفحة السابقة"
                                >
                                    <ChevronRight className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                {/* Page numbers */}
                                <div className="flex items-center gap-0.5 mx-1">
                                    {getPageNumbers().map((p) => (
                                        <Button
                                            key={p}
                                            variant={p === pagination.page ? "primary" : "ghost"}
                                            size="icon"
                                            onClick={() => pagination.onPageChange(p)}
                                            className={cn(
                                                "h-7 w-7 text-[10px] font-bold transition-all duration-300 rounded-lg",
                                                p === pagination.page
                                                    ? "bg-primary text-primary-foreground scale-105"
                                                    : "text-muted-foreground hover:text-primary hover:bg-primary/5"
                                            )}
                                            aria-label={`الصفحة ${p}`}
                                            aria-current={p === pagination.page ? "page" : undefined}
                                        >
                                            {p}
                                        </Button>
                                    ))}
                                </div>

                                {/* Next */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page >= totalPages}
                                    onClick={() => pagination.onPageChange(pagination.page + 1)}
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة التالية"
                                    aria-label="الصفحة التالية"
                                >
                                    <ChevronLeft className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                {/* Last */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page >= totalPages}
                                    onClick={() => pagination.onPageChange(totalPages)}
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة الأخيرة"
                                    aria-label="الصفحة الأخيرة"
                                >
                                    <ChevronsLeft className="h-4 w-4" aria-hidden="true" />
                                </Button>
                            </div>
                        )}
                    </div>
                )}
            </div>

            {/* ─── Quick View Drawer ─────────────────────── */}
            {renderDrawerContent && selectedRow && (
                <Sheet open={drawerOpen} onOpenChange={setDrawerOpen}>
                    <SheetContent
                        side={dir === "rtl" ? "left" : "right"}
                        className="sm:max-w-md border-s"
                    >
                        <SheetHeader className="mb-6">
                            <SheetTitle>
                                {drawerTitle ? drawerTitle(selectedRow) : "تفاصيل السجل"}
                            </SheetTitle>
                            {drawerDescription && (
                                <SheetDescription>
                                    {drawerDescription(selectedRow)}
                                </SheetDescription>
                            )}
                        </SheetHeader>
                        <div className="mt-4 overflow-y-auto max-h-[calc(100dvh-180px)] custom-scrollbar px-1">
                            {renderDrawerContent(selectedRow)}
                        </div>
                    </SheetContent>
                </Sheet>
            )}
        </div>
    );
}

export { DataTable };