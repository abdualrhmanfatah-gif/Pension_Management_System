import { Plus, Search } from "lucide-react";
import * as React from "react";

import { cn } from "@/lib/utils";

import { Button } from "./Button";
import { Card } from "./Card";
import { type CustomColumnDef, DataTable, type PaginationConfig } from "./DataTable";
import { Heading, Text } from "./Typography";
import { Input } from "./Input";

export interface CrudPageLayoutProps<T> {
  // Header
  title: string;
  description?: string;
  createLabel?: string;
  onCreateClick?: () => void;
  headerActions?: React.ReactNode;

  // Data Table
  columns: CustomColumnDef<T>[];
  data: T[];
  loading?: boolean;
  pagination?: PaginationConfig;
  emptyMessage?: string;
  rowKey?: (row: T, index: number) => string | number;
  onRowClick?: (row: T) => void;
  renderMobileCard?: (row: T, index: number) => React.ReactNode;

  // Sorting
  sortColumn?: string;
  sortDirection?: "asc" | "desc" | null;
  onSort?: (column: string, direction: "asc" | "desc" | null) => void;

  // Search
  searchable?: boolean;
  searchPlaceholder?: string;
  onSearchChange?: (value: string) => void;
  searchValue?: string;
  onSearchKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  searchInputRef?: React.RefObject<HTMLInputElement | null>;
  focusedRowIndex?: number;
  searchInHeader?: boolean;

  // Dialogs slot
  children?: React.ReactNode;

  // Permissions
  canCreate?: boolean;

  // Customization
  className?: string;
  icon?: React.ReactNode;
  hideHeader?: boolean;
}

function CrudPageLayoutInner<T>({
  title,
  description,
  createLabel = "إضافة جديد",
  onCreateClick,
  headerActions,
  columns,
  data,
  loading,
  pagination,
  emptyMessage,
  rowKey,
  onRowClick,
  searchable,
  searchPlaceholder = "بحث...",
  onSearchChange,
  searchValue,
  onSearchKeyDown,
  searchInputRef,
  focusedRowIndex,
  searchInHeader = false,
  sortColumn,
  sortDirection,
  onSort,
  children,
  canCreate = true,
  className,
  icon,
  hideHeader = false,
  renderMobileCard,
}: CrudPageLayoutProps<T>) {
  // ═══════════════════════════════════════════════════════
  // 1. دمج الأحداث: Escape يُفرّغ البحث (UX محسّن)
  // ═══════════════════════════════════════════════════════
  const handleSearchKeyDown = React.useCallback(
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === "Escape" && searchValue) {
        onSearchChange?.("");
        searchInputRef?.current?.blur();
      }
      onSearchKeyDown?.(e);
    },
    [onSearchChange, onSearchKeyDown, searchInputRef, searchValue]
  );

  const hasHeaderActions = React.useMemo(
    () => Boolean(headerActions) || (canCreate && onCreateClick),
    [headerActions, canCreate, onCreateClick]
  );

  const searchField = searchable ? (
    <div
      className={cn(
        "relative",
        searchInHeader
          ? "w-full rounded-lg border-2 border-primary/40 bg-primary/5 p-1 shadow-md shadow-primary/10 transition-colors focus-within:border-primary/70 focus-within:bg-primary/[0.07]"
          : "max-w-md"
      )}
    >
      <Search
        className="pointer-events-none absolute end-4 top-1/2 h-4 w-4 -translate-y-1/2 text-primary/70"
        aria-hidden="true"
      />
      <Input
        ref={searchInputRef}
        type="search"
        role="searchbox"
        aria-label="البحث في الجدول"
        placeholder={searchPlaceholder}
        value={searchValue ?? ""}
        onChange={(e) => onSearchChange?.(e.target.value)}
        onKeyDown={handleSearchKeyDown}
        className={cn(
          "h-10 pe-10 text-sm transition-colors",
          searchInHeader
            ? "border-0 bg-transparent shadow-none focus-visible:ring-0"
            : "border border-input bg-background placeholder:text-muted-foreground/70 focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1",
          "placeholder:text-muted-foreground/70"
        )}
      />
    </div>
  ) : null;

  return (
    <div
      className={cn("w-full space-y-4 md:space-y-6", className)}
      dir="rtl"
    >
      {/* ═══════════════════════════════════════════════════
          2. Header — إصلاح التجاوب + أحجام نصوص قابلة للقراءة
             (الحد الأدنى 12px بدلاً من 10px)
             مع خيار وضع البحث في المنتصف (searchInHeader)
          ═══════════════════════════════════════════════════ */}
      {!hideHeader && (
        <header
          className={
            searchInHeader
              ? "grid grid-cols-1 items-center gap-3 lg:grid-cols-[auto_minmax(0,1fr)_auto]"
              : "flex flex-row flex-wrap items-center justify-between gap-3"
          }
        >
          <div className="min-w-0">
            <div className="flex items-center gap-2">
              {icon && (
                <span className="shrink-0 text-muted-foreground" aria-hidden="true">
                  {icon}
                </span>
              )}
              <Heading
                level="h2"
                className="text-base font-semibold leading-tight sm:text-lg"
              >
                {title}
              </Heading>
            </div>
            {description && (
              <Text
                variant="description"
                className="mt-1 text-xs leading-relaxed text-muted-foreground sm:text-sm"
              >
                {description}
              </Text>
            )}
          </div>

          {searchInHeader && (
            <div className="justify-self-center w-full max-w-xl">
              {searchField}
            </div>
          )}

          <div
            className={
              searchInHeader
                ? "flex flex-wrap items-center gap-2 justify-self-end"
                : "flex flex-wrap items-center gap-2 shrink-0"
            }
          >
            {hasHeaderActions && (
              <div className="flex items-center gap-2">
                {canCreate && onCreateClick && (
                  <Button
                    type="button"
                    onClick={onCreateClick}
                    className="h-9 gap-1.5 px-3 text-xs font-medium sm:h-10 sm:px-4 sm:text-sm"
                  >
                    <Plus className="h-4 w-4" aria-hidden="true" />
                    <span>{createLabel}</span>
                  </Button>
                )}
                {headerActions}
              </div>
            )}
          </div>
        </header>
      )}

      {/* ═══════════════════════════════════════════════════
          3. Search — تصميم أنظف + أيقونة بلون muted
             + focus ring متناسق
          ═══════════════════════════════════════════════════ */}
      {searchable && !searchInHeader && searchField}

      {/* Dialogs / Stats slot */}
      {children}

      {/* ═══════════════════════════════════════════════════
          4. DataTable — إزالة padding الزائد من Card
             + shadow خفيف لعمق بصري
          ═══════════════════════════════════════════════════ */}
      <Card className="overflow-hidden border shadow-sm">
        <DataTable
          columns={columns}
          data={data}
          loading={loading}
          pagination={pagination}
          emptyMessage={emptyMessage}
          rowKey={rowKey}
          onRowClick={onRowClick}
          sortColumn={sortColumn}
          sortDirection={sortDirection}
          onSort={onSort}
          renderMobileCard={renderMobileCard}
          focusedRowIndex={focusedRowIndex}
          className="border-0"
        />
      </Card>
    </div>
  );
}


export const CrudPageLayout = React.memo(CrudPageLayoutInner) as <T>(
  props: CrudPageLayoutProps<T>
) => React.JSX.Element;