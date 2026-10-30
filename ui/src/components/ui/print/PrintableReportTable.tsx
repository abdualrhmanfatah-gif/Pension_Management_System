import { forwardRef } from "react";
<<<<<<< HEAD

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { cn } from "@/lib/utils";

/* ═══════════════════════════════════════════════════════════
   Types
   ═══════════════════════════════════════════════════════════ */

export interface TableCell {
  value: React.ReactNode;
  bold?: boolean;
  color?: string;
  align?: "start" | "center" | "end";
  colSpan?: number;
  className?: string;
}

export interface PrintableReportTableProps {
  headers: string[];
  rows: TableCell[][];
  totals?: TableCell[];
  caption?: string;
  captionNumber?: number;
  compact?: boolean;
  className?: string;
  /** لتفعيل التمرير الأفقي على الشاشات الصغيرة */
  scrollable?: boolean;
  /** لتفعيل تأثير hover على الصفوف */
  hoverable?: boolean;
}

/* ═══════════════════════════════════════════════════════════
   Component
   ═══════════════════════════════════════════════════════════ */

export const PrintableReportTable = forwardRef<HTMLDivElement, PrintableReportTableProps>(
  (
    {
      headers,
      rows,
      totals,
      caption,
      captionNumber,
      compact,
      className,
      scrollable = false,
      hoverable = false,
    },
    ref
  ) => {
    const cellPadding = compact ? "px-2 py-1" : "px-3 py-2";
    const headerPadding = compact ? "px-2 py-1.5" : "px-3 py-2.5";

    const tableContent = (
      <table
        className={cn(
          "w-full border-collapse border border-print",
          "text-[13px] font-semibold leading-relaxed",
          "print:w-full"
        )}
      >
        {/* ─── Caption (وصولية + معيارية) ─── */}
        {caption && (
          <caption
            className={cn(
              "caption-top text-start mb-3",
              "text-xs font-black text-brand",
              "border-b-2 border-brand pb-2",
              "flex items-center gap-2"
            )}
          >
            {captionNumber != null && (
              <span
                className={cn(
                  "inline-flex items-center justify-center",
                  "w-6 h-6 bg-brand text-white",
                  "text-[10px] font-extrabold rounded-sm shrink-0"
                )}
                aria-hidden="true"
              >
                {captionNumber}
              </span>
            )}
            {caption}
          </caption>
        )}

        {/* ─── رؤوس الأعمدة ─── */}
        <thead className="bg-table-header">
          <tr>
            {headers.map((h, i) => (
              <th
                key={i}
                scope="col"
                className={cn(
                  "border border-print",
                  "text-center font-extrabold text-[14px] text-brand",
                  "tracking-wide",
                  headerPadding
                )}
              >
                {h}
              </th>
            ))}
          </tr>
        </thead>

        {/* ─── صفوف البيانات ─── */}
        <tbody>
          {rows.map((row, ri) => (
            <tr
              key={ri}
              className={cn(
                "transition-colors duration-150",
                ri % 2 === 1 && "bg-slate-50/60",
                hoverable && "hover:bg-slate-100/80",
                "print:break-inside-avoid"
              )}
            >
              {row.map((cell, ci) => (
                <td
                  key={ci}
                  colSpan={cell.colSpan}
                  className={cn(
                    "border border-print",
                    cell.align === "start"
                      ? "text-start"
                      : cell.align === "end"
                      ? "text-end"
                      : "text-center",
                    cellPadding,
                    cell.bold && "font-black",
                    cell.className
                  )}
                  style={cell.color ? { color: cell.color } : undefined}
                >
                  {cell.value ?? "—"}
                </td>
              ))}
            </tr>
          ))}
        </tbody>

        {/* ─── صف الإجمالي ─── */}
        {totals && (
          <tfoot className="bg-table-header">
            <tr className="border-t-2 border-brand">
              {totals.map((cell, ci) => (
                <td
                  key={ci}
                  colSpan={cell.colSpan}
                  className={cn(
                    "border border-print",
                    cell.align === "start"
                      ? "text-start"
                      : cell.align === "end"
                      ? "text-end"
                      : "text-center",
                    cellPadding,
                    "text-[15px] font-black"
                  )}
                  style={cell.color ? { color: cell.color } : undefined}
                >
                  {cell.value ?? ""}
                </td>
              ))}
            </tr>
          </tfoot>
        )}
      </table>
    );

    return (
      <div ref={ref} className={cn("w-full", className)}>
        {scrollable ? (
          <div
            className="overflow-x-auto"
            role="region"
            aria-label={caption ? `جدول: ${caption}` : "جدول بيانات"}
            tabIndex={0}
          >
            {tableContent}
          </div>
        ) : (
          tableContent
        )}
      </div>
    );
  }
);

PrintableReportTable.displayName = "PrintableReportTable";