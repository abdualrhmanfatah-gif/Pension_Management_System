<<<<<<< HEAD
﻿import type { ColumnDef } from "@tanstack/react-table";
import { useVirtualizer } from "@tanstack/react-virtual";
=======
import { useVirtualizer } from "@tanstack/react-virtual";
import type { ColumnDef } from "@tanstack/react-table";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
const ESTIMATED_ROW_HEIGHT = 48;
const MOBILE_CARD_ESTIMATED_HEIGHT = 90;
const tableNumberFormatter = new Intl.NumberFormat("ar-YE");
const paginationButtonClass = "h-11 w-11 min-h-11 min-w-11 shrink-0 rounded-md text-sm font-medium shadow-none transition-colors hover:scale-100 active:scale-100 hover:shadow-none motion-reduce:transition-none [@media(pointer:fine)]:h-9 [@media(pointer:fine)]:w-9 [@media(pointer:fine)]:min-h-9 [@media(pointer:fine)]:min-w-9";

function isNestedControl(event: React.MouseEvent<HTMLElement>): boolean {
    if (!(event.target instanceof Element)) return false;
    if (!event.currentTarget.contains(event.target)) return true;
    const control = event.target.closest(
        "button, a, input, select, textarea, summary, [role='button'], [role='link'], [contenteditable='true']"
    );
    return control !== null && control !== event.currentTarget;
}
=======
const ESTIMATED_ROW_HEIGHT = 56;
const MOBILE_CARD_ESTIMATED_HEIGHT = 90;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
    /** Keep one identifying column visible at the logical start while scrolling. */
    stickyColumnKey?: string;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    title?: React.ReactNode;
    drawerTitle?: (row: T) => string;
    drawerDescription?: (row: T) => string;
    renderDrawerContent?: (row: T) => React.ReactNode;
    renderMobileCard?: (row: T, index: number) => React.ReactNode;
    rowClassName?: (row: T) => string | undefined;
    dir?: "rtl" | "ltr";
    maxHeight?: string;
<<<<<<< HEAD
    /** Use page scrolling for short, paginated mobile lists. */
    mobileScrollMode?: "contained" | "page";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}

// ─── Mobile Card View (virtualized to prevent OOM) ──────

interface MobileCardViewProps<T> {
    data: T[];
    rowKey?: (row: T, index: number) => string | number;
    dataColumns: CustomColumnDef<T>[];
    actionsColumn?: CustomColumnDef<T>;
    renderCell: (row: T, i: number, col: CustomColumnDef<T>) => React.ReactNode;
    renderMobileCard?: (row: T, index: number) => React.ReactNode;
<<<<<<< HEAD
    rowClassName?: (row: T) => string | undefined;
    onRowClick: (row: T) => void;
    hasRowInteraction: boolean;
    maxHeight?: string;
    scrollMode: "contained" | "page";
=======
    onRowClick: (row: T) => void;
    hasRowInteraction: boolean;
    maxHeight?: string;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}

function MobileCardView<T>({
    data,
    rowKey,
    dataColumns,
    actionsColumn,
    renderCell,
    renderMobileCard,
<<<<<<< HEAD
    rowClassName,
    onRowClick,
    hasRowInteraction,
    maxHeight,
    scrollMode,
}: MobileCardViewProps<T>) {
    const scrollContainerRef = React.useRef<HTMLDivElement>(null);
    const isPageScroll = scrollMode === "page";
=======
    onRowClick,
    hasRowInteraction,
    maxHeight,
}: MobileCardViewProps<T>) {
    const scrollContainerRef = React.useRef<HTMLDivElement>(null);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    const mobileVirtualizer = useVirtualizer({
        count: data.length,
        getScrollElement: () => scrollContainerRef.current,
        estimateSize: () => MOBILE_CARD_ESTIMATED_HEIGHT,
        overscan: 5,
<<<<<<< HEAD
        gap: 8,
        enabled: !isPageScroll,
    });

    const virtualItems = isPageScroll
        ? data.map((_, index) => ({ index, start: 0 }))
        : mobileVirtualizer.getVirtualItems();
=======
        gap: 2,
    });

    const virtualItems = mobileVirtualizer.getVirtualItems();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const totalSize = mobileVirtualizer.getTotalSize();

    return (
        <div
            ref={scrollContainerRef}
<<<<<<< HEAD
            className={cn(isPageScroll ? "py-1" : "overflow-y-auto p-1")}
            style={isPageScroll ? undefined : { maxHeight: maxHeight ?? "calc(100dvh - 260px)" }}
        >
            <div
                className={isPageScroll ? "space-y-2" : "relative"}
                style={isPageScroll ? undefined : { height: `${totalSize}px` }}
            >
=======
            className="p-0 md:p-3 overflow-y-auto"
            style={{ maxHeight: maxHeight ?? 'calc(100dvh - 260px)' }}
        >
            <div style={{ height: `${totalSize}px`, position: 'relative' }}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                {virtualItems.map((virtualRow) => {
                    const i = virtualRow.index;
                    const row = data[i];
                    const key = rowKey ? rowKey(row, i) : `mobile-row-${i}`;

                    return (
                        <div
                            key={key}
                            data-index={virtualRow.index}
<<<<<<< HEAD
                            ref={isPageScroll ? undefined : mobileVirtualizer.measureElement}
                            style={isPageScroll ? undefined : {
=======
                            ref={mobileVirtualizer.measureElement}
                            style={{
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                position: 'absolute',
                                top: 0,
                                left: 0,
                                width: '100%',
                                transform: `translateY(${virtualRow.start}px)`,
                            }}
                            className={cn(
<<<<<<< HEAD
                                "min-w-0 bg-card rounded-xl border border-border overflow-hidden",
                                hasRowInteraction && "cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring",
                                rowClassName?.(row)
                            )}
                            onClick={(e) => {
                                if (hasRowInteraction && !isNestedControl(e)) onRowClick(row);
                            }}
                            role={hasRowInteraction ? "button" : undefined}
                            tabIndex={hasRowInteraction ? 0 : undefined}
                            onKeyDown={(e) => {
                                if (hasRowInteraction && e.target === e.currentTarget && (e.key === "Enter" || e.key === " ")) {
=======
                                "bg-card rounded-xl border border-border/80 shadow-sm overflow-hidden",
                                hasRowInteraction && "cursor-pointer active:scale-[0.99] transition-transform"
                            )}
                            onClick={() => onRowClick(row)}
                            role="button"
                            tabIndex={0}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter' || e.key === ' ') {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                            idx < dataColumns.length - 1 && "border-b border-border"
                                        )}>
                                            <span className="text-caption font-bold text-muted-foreground shrink-0 min-w-[60px] text-end">
=======
                                            idx < dataColumns.length - 1 && "border-b border-border/60"
                                        )}>
                                            <span className="text-[10px] font-bold text-muted-foreground shrink-0 min-w-[60px] text-end">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                {col.header}
                                            </span>
                                            <span className="text-xs flex-1 text-start font-medium break-words min-w-0">
                                                {renderCell(row, i, col)}
                                            </span>
                                        </div>
                                    ))}
                                    {actionsColumn && (
<<<<<<< HEAD
                                        <div className="flex justify-center items-center gap-2 px-3 py-2 bg-muted/20 border-t border-border">
=======
                                        <div className="flex justify-center items-center gap-2 px-3 py-2 bg-muted/20 border-t border-border/60">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
    stickyColumnKey,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    title,
    drawerTitle,
    drawerDescription,
    renderDrawerContent,
    renderMobileCard,
    rowClassName,
    dir = "rtl",
    maxHeight,
<<<<<<< HEAD
    mobileScrollMode = "contained",
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
        enabled: shouldVirtualize,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                        "border-b border-border px-3 py-2 text-start align-middle text-sm font-medium text-table-foreground whitespace-nowrap transition-colors motion-reduce:transition-none",
                        i === data.length - 1 && "border-b-0",
                        col.className,
                        col.key === stickyColumnKey && "sticky start-0 z-10 bg-card group-hover/table-row:bg-muted",
                        col.key === stickyColumnKey && focusedRowIndex === i && "bg-muted"
                    )}
                    style={{ width: col.size, minWidth: col.minSize, maxWidth: col.maxSize }}
=======
                        "px-3 sm:px-4 py-3.5 text-start align-middle text-sm font-medium text-table-foreground whitespace-normal sm:whitespace-nowrap transition-colors duration-300",
                        col.className
                    )}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                >
                    {renderCell(row, i, col)}
                </td>
            )),
<<<<<<< HEAD
        [columns, data.length, focusedRowIndex, renderCell, stickyColumnKey]
=======
        [columns, renderCell]
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                        "group/table-row hover:bg-muted transition-colors motion-reduce:transition-none",
                        isClickable && "cursor-pointer",
                        focusedRowIndex === i && "bg-muted ring-1 ring-inset ring-primary",
                        rowClassName?.(row),
                        extraClassName
                    )}
                    onClick={(e) => {
                        if (!isNestedControl(e)) handleRowClick(row);
                    }}
                    role={isClickable ? "button" : undefined}
                    tabIndex={isClickable ? 0 : undefined}
                    onKeyDown={(e) => {
                        if (isClickable && e.target === e.currentTarget && (e.key === "Enter" || e.key === " ")) {
=======
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
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
    const PreviousIcon = dir === "rtl" ? ChevronRight : ChevronLeft;
    const NextIcon = dir === "rtl" ? ChevronLeft : ChevronRight;
    const FirstIcon = dir === "rtl" ? ChevronsRight : ChevronsLeft;
    const LastIcon = dir === "rtl" ? ChevronsLeft : ChevronsRight;

    return (
        <div className={cn(
            "min-w-0 overflow-hidden",
            mobileScrollMode === "page" ? "sm:rounded-xl sm:border sm:border-border sm:bg-card" : "rounded-xl border border-border bg-card",
            className
        )} dir={dir}>
            {title && (
                <div className="flex items-center justify-between border-b border-border px-3 py-3">
                    <h4 className="text-sm font-black text-primary tracking-wide">
=======

    return (
        <div className={cn("space-y-4", className)} dir={dir}>
            {title && (
                <div className="flex items-center justify-between px-2">
                    <h4 className="text-sm font-black text-primary/80 tracking-wide">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        {title}
                    </h4>
                </div>
            )}
<<<<<<< HEAD
            <div className="min-w-0">
                <div
                    ref={scrollRef}
                    className="hidden overflow-auto [scrollbar-gutter:stable] [scroll-padding-block-start:3rem] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring sm:block"
                    style={{ maxHeight: maxHeight ?? "calc(100dvh - 260px)" }}
                    role="region"
                    aria-label={typeof title === "string" ? title : "بيانات الجدول"}
                    tabIndex={0}
                >
                    <table className="w-full min-w-full border-separate border-spacing-0 text-sm">
                        {/* ─── Header ────────────────────────────── */}
                        <thead className="bg-muted">
=======
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
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <tr>
                                {columns.map((col) => (
                                    <th
                                        key={col.key}
                                        scope="col"
                                        className={cn(
<<<<<<< HEAD
                                            "border-b border-border bg-muted px-3 py-2.5 text-start align-middle text-sm font-semibold text-table-foreground whitespace-nowrap",
                                            col.className,
                                            col.sortable && onSort && "p-0",
                                            stickyHeader && "sticky top-0 z-20",
                                            col.key === stickyColumnKey && "sticky start-0 z-10",
                                            stickyHeader && col.key === stickyColumnKey && "z-30"
                                        )}
                                        style={{ width: col.size, minWidth: col.minSize, maxWidth: col.maxSize }}
                                        aria-sort={
                                            col.sortable && onSort
                                                ? sortColumn === col.key
                                                    ? sortDirection === "asc"
                                                        ? "ascending"
                                                        : sortDirection === "desc"
                                                            ? "descending"
                                                            : "none"
                                                    : "none"
                                                : undefined
                                        }
                                    >
                                        {col.sortable && onSort ? (
                                            <button
                                                type="button"
                                                onClick={() => handleSort(col)}
                                                className="inline-flex min-h-11 w-full items-center gap-2 rounded-sm px-3 py-2.5 text-start text-sm font-semibold hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring"
                                                aria-label={`ترتيب حسب ${col.header}`}
                                            >
                                                {col.header}
                                                <span
                                                    className="text-muted-foreground"
=======
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
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                            </button>
                                        ) : (
                                            <span className="text-sm font-semibold">{col.header}</span>
                                        )}
=======
                                            )}
                                        </span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    </th>
                                ))}
                            </tr>
                        </thead>

                        {/* ─── Body ──────────────────────────────── */}
                        {shouldVirtualize && !loading ? (
<<<<<<< HEAD
                            <tbody className="relative">
=======
                            <tbody className="divide-y divide-border/40 relative">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                                "group/table-row hover:bg-muted transition-colors motion-reduce:transition-none",
                                                isClickable && "cursor-pointer",
                                                focusedRowIndex === virtualRow.index &&
                                                    "bg-muted ring-1 ring-inset ring-primary",
                                                rowClassName?.(row)
                                            )}
                                            onClick={(e) => {
                                                if (!isNestedControl(e)) handleRowClick(row);
                                            }}
=======
                                                "hover:bg-primary/5 transition-all duration-300 border-b border-border/40",
                                                isClickable && "cursor-pointer",
                                                focusedRowIndex === virtualRow.index &&
                                                    "bg-primary/10 ring-1 ring-primary/30"
                                            )}
                                            onClick={() => handleRowClick(row)}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            role={isClickable ? "button" : undefined}
                                            tabIndex={isClickable ? 0 : undefined}
                                            onKeyDown={(e) => {
                                                if (
                                                    isClickable &&
<<<<<<< HEAD
                                                    e.target === e.currentTarget &&
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                            <tbody>
=======
                            <tbody className="divide-y divide-border/40">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                {/* Loading state */}
                                {loading &&
                                    Array.from({ length: 5 }).map((_, i) => (
                                        <tr key={`skeleton-${i}`} aria-hidden="true">
                                            {columns.map((col) => (
<<<<<<< HEAD
                                                <td key={col.key} className="border-b border-border px-3 py-2" style={{ width: col.size, minWidth: col.minSize, maxWidth: col.maxSize }}>
=======
                                                <td key={col.key} className="px-4 py-3.5">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                                        className="h-12 w-12 opacity-100 text-primary"
=======
                                                        className="h-12 w-12 opacity-30 text-primary/40"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                            rowClassName={rowClassName}
                            onRowClick={handleRowClick}
                            hasRowInteraction={!!(onRowClick || renderDrawerContent)}
                            maxHeight={maxHeight}
                            scrollMode={mobileScrollMode}
=======
                            onRowClick={handleRowClick}
                            hasRowInteraction={!!(onRowClick || renderDrawerContent)}
                            maxHeight={maxHeight}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                    className="h-12 w-12 opacity-100 text-primary"
=======
                                    className="h-12 w-12 opacity-30 text-primary/40"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    aria-hidden="true"
                                />
                            )}
                            <p className="text-base font-bold">{emptyMessage}</p>
                        </div>
                    </div>
                )}

                {/* ─── Pagination ───────────────────────────── */}
<<<<<<< HEAD
                {pagination && (totalPages > 1 || pagination.pageSizeOptions || isCursorMode) && (
                    <div className="flex flex-col items-stretch justify-between gap-3 border-t border-border bg-card px-3 py-3 sm:flex-row sm:flex-wrap sm:items-center">
                        <div className="flex flex-wrap items-center justify-center gap-x-4 gap-y-2 text-center sm:justify-start sm:text-start">
                            <p className="text-xs font-medium text-foreground sm:text-sm" aria-live="polite">
                                عرض{" "}
                                <bdi dir="ltr" className="font-semibold tabular-nums">
                                    {tableNumberFormatter.format(pagination.total > 0
                                        ?
                                            (pagination.page - 1) * pagination.pageSize +
                                            1
                                        : 0)}
                                    {"–"}
                                    {tableNumberFormatter.format(Math.min(
                                        pagination.page * pagination.pageSize,
                                        pagination.total
                                    ))}
                                </bdi>
                                {" من "}
                                <bdi className="font-semibold tabular-nums">
                                    {tableNumberFormatter.format(pagination.total)}
                                </bdi>
                                {" سجل"}
                            </p>
                            {!isCursorMode && totalPages > 0 && (
                                <p className="text-xs text-muted-foreground">
                                    الصفحة {tableNumberFormatter.format(pagination.page)} من {tableNumberFormatter.format(totalPages)}
                                </p>
                            )}

                            {pagination.pageSizeOptions && pagination.onPageSizeChange && (
                                <div className="flex items-center gap-1.5">
                                    <span className="text-xs text-muted-foreground">حجم الصفحة</span>
=======
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
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    <select
                                        value={pagination.pageSize}
                                        onChange={(e) =>
                                            pagination.onPageSizeChange?.(Number(e.target.value))
                                        }
<<<<<<< HEAD
                                        className="h-11 min-w-16 rounded-md border border-border bg-background px-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring [@media(pointer:fine)]:h-9"
=======
                                        className="h-6 text-[10px] rounded-md border border-border/60 bg-background px-1.5 py-0.5 focus:outline-none focus:ring-2 focus:ring-primary/30"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        aria-label="عدد السجلات في الصفحة"
                                    >
                                        {pagination.pageSizeOptions.map((size) => (
                                            <option key={size} value={size}>
<<<<<<< HEAD
                                                {tableNumberFormatter.format(size)}
=======
                                                {size}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            )}
                        </div>

                        {/* Cursor mode: simplified Prev/Next */}
                        {isCursorMode ? (
<<<<<<< HEAD
                            <div className="flex items-center justify-center gap-2" role="navigation" aria-label="التنقل بين صفحات الجدول">
=======
                            <div className="flex items-center gap-1 order-1 sm:order-2">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={
                                        !pagination.onCursorPrev ||
                                        !pagination.page ||
                                        pagination.page <= 1
                                    }
                                    onClick={() => pagination.onCursorPrev?.()}
<<<<<<< HEAD
                                    className={paginationButtonClass}
                                    title="السابق"
                                    aria-label="الصفحة السابقة"
                                >
                                    <PreviousIcon className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                <span
                                    className="mx-1 text-sm font-medium tabular-nums text-foreground"
                                    aria-live="polite"
                                >
                                    الصفحة {tableNumberFormatter.format(pagination.page)}
=======
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
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                </span>

                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={!pagination.hasMore || !pagination.onCursorNext}
                                    onClick={() => pagination.onCursorNext?.()}
<<<<<<< HEAD
                                    className={paginationButtonClass}
                                    title="التالي"
                                    aria-label="الصفحة التالية"
                                >
                                    <NextIcon className="h-4 w-4" aria-hidden="true" />
=======
                                    className="h-7 w-7 text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="التالي"
                                    aria-label="الصفحة التالية"
                                >
                                    <ChevronLeft className="h-4 w-4" aria-hidden="true" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                </Button>
                            </div>
                        ) : (
                            <div
<<<<<<< HEAD
                                className="flex items-center justify-center gap-1"
=======
                                className="flex items-center gap-1 order-1 sm:order-2"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                role="navigation"
                                aria-label="التنقل بين الصفحات"
                            >
                                {/* First */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page <= 1}
                                    onClick={() => pagination.onPageChange(1)}
<<<<<<< HEAD
                                    className={paginationButtonClass}
                                    title="الصفحة الأولى"
                                    aria-label="الصفحة الأولى"
                                >
                                    <FirstIcon className="h-4 w-4" aria-hidden="true" />
=======
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة الأولى"
                                    aria-label="الصفحة الأولى"
                                >
                                    <ChevronsRight className="h-4 w-4" aria-hidden="true" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                </Button>

                                {/* Previous */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page <= 1}
                                    onClick={() => pagination.onPageChange(pagination.page - 1)}
<<<<<<< HEAD
                                    className={paginationButtonClass}
                                    title="الصفحة السابقة"
                                    aria-label="الصفحة السابقة"
                                >
                                    <PreviousIcon className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                {/* Page numbers */}
                                <div className="hidden items-center gap-1 sm:flex">
=======
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة السابقة"
                                    aria-label="الصفحة السابقة"
                                >
                                    <ChevronRight className="h-4 w-4" aria-hidden="true" />
                                </Button>

                                {/* Page numbers */}
                                <div className="flex items-center gap-0.5 mx-1">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    {getPageNumbers().map((p) => (
                                        <Button
                                            key={p}
                                            variant={p === pagination.page ? "primary" : "ghost"}
                                            size="icon"
                                            onClick={() => pagination.onPageChange(p)}
                                            className={cn(
<<<<<<< HEAD
                                                paginationButtonClass,
                                                p === pagination.page
                                                    ? "bg-primary text-primary-foreground"
                                                    : "text-foreground hover:text-primary"
=======
                                                "h-7 w-7 text-[10px] font-bold transition-all duration-300 rounded-lg",
                                                p === pagination.page
                                                    ? "bg-primary text-primary-foreground scale-105"
                                                    : "text-muted-foreground hover:text-primary hover:bg-primary/5"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            )}
                                            aria-label={`الصفحة ${p}`}
                                            aria-current={p === pagination.page ? "page" : undefined}
                                        >
<<<<<<< HEAD
                                            {tableNumberFormatter.format(p)}
=======
                                            {p}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        </Button>
                                    ))}
                                </div>

                                {/* Next */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page >= totalPages}
                                    onClick={() => pagination.onPageChange(pagination.page + 1)}
<<<<<<< HEAD
                                    className={paginationButtonClass}
                                    title="الصفحة التالية"
                                    aria-label="الصفحة التالية"
                                >
                                    <NextIcon className="h-4 w-4" aria-hidden="true" />
=======
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة التالية"
                                    aria-label="الصفحة التالية"
                                >
                                    <ChevronLeft className="h-4 w-4" aria-hidden="true" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                </Button>

                                {/* Last */}
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    disabled={pagination.page >= totalPages}
                                    onClick={() => pagination.onPageChange(totalPages)}
<<<<<<< HEAD
                                    className={paginationButtonClass}
                                    title="الصفحة الأخيرة"
                                    aria-label="الصفحة الأخيرة"
                                >
                                    <LastIcon className="h-4 w-4" aria-hidden="true" />
=======
                                    className="min-h-[28px] min-w-[28px] text-muted-foreground hover:text-primary hover:bg-primary/5 rounded-lg transition-all duration-300"
                                    title="الصفحة الأخيرة"
                                    aria-label="الصفحة الأخيرة"
                                >
                                    <ChevronsLeft className="h-4 w-4" aria-hidden="true" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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

<<<<<<< HEAD
export { DataTable };
=======
export { DataTable };
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
