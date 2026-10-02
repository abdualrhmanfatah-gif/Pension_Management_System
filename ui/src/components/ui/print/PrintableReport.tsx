import { forwardRef, type ReactNode } from "react";

import { PrintSignatureBlock, type Signature } from "@/components/common/PrintSignatureBlock";
import { type PageOrientation,type PageSize, PrintLayout } from "@/components/ui/print/PrintLayout";
import { cn } from "@/lib/utils";

import { PrintableReportTable, type TableCell } from "./PrintableReportTable";

/* ═══════════════════════════════════════════════════════════
   Types
   ═══════════════════════════════════════════════════════════ */

export interface MetadataField {
  label: string;
  value: string;
}

export interface BalanceCheck {
  isBalanced: boolean;
  difference: number;
  debitLabel?: string;
  creditLabel?: string;
  debitTotal?: number;
  creditTotal?: number;
}

export interface PrintableReportProps {
  /** عنوان التقرير الرئيسي */
  title: string;
  /** وصف إضافي */
  subtitle?: string;
  /** حقول مetadata (الفترة، السنة، التاريخ) */
  metadata?: MetadataField[];
  /** رؤوس الجدول */
  tableHeaders: string[];
  /** صفوف البيانات */
  rows: TableCell[][];
  /** صف الإجمالي */
  totals?: TableCell[];
  /** حالة التوازن (ميزان المراجعة) */
  balanceCheck?: BalanceCheck;
  /** التواقيع */
  signatures?: Signature[];
  /** ملاحظات سفلية */
  notes?: string[];
  /** محتوى إضافي قبل التواقيع */
  beforeSignatures?: ReactNode;
  /** حجم الصفحة */
  pageSize?: PageSize;
  /** اتجاه الصفحة */
  orientation?: PageOrientation;
  /** عرض المحتوى فقط بدون PrintLayout (للérbed) */
  embed?: boolean;
  /** عنوان القسم للجدول */
  tableCaption?: string;
  /** رقم القسم */
  tableCaptionNumber?: number;
  /** compact mode */
  compact?: boolean;
  /** اسم الإدارة المختصه */
  sectionTitle?: string;
}

/* ═══════════════════════════════════════════════════════════
   Component
   ═══════════════════════════════════════════════════════════ */

export const PrintableReport = forwardRef<HTMLDivElement, PrintableReportProps>(
  (
    {
      title,
      subtitle,
      metadata,
      tableHeaders,
      rows,
      totals,
      balanceCheck,
      signatures,
      notes,
      beforeSignatures,
      pageSize = "A3",
      orientation = "landscape",
      embed = false,
      tableCaption,
      tableCaptionNumber,
      compact,
      sectionTitle,
    },
    ref
  ) => {
    const content = (
      <div ref={ref} className="space-y-6 text-black" dir="rtl">
        {/* ─── رأس التقرير ─── */}
        <section className="space-y-2">
          {embed && <h2 className="text-base font-black border-b-2 border-brand pb-2 text-brand">{title}</h2>}
          {subtitle && (
            <p className="text-xs text-gray-600 font-bold">{subtitle}</p>
          )}
          {metadata && metadata.length > 0 && (
            <div className="flex flex-wrap gap-x-6 gap-y-1 text-[11px]">
              {metadata.map((m, i) => (
                <div key={i} className="flex items-center gap-1">
                  <span className="font-bold text-gray-600">{m.label}:</span>
                  <span className="font-extrabold" dir="ltr">{m.value}</span>
                </div>
              ))}
            </div>
          )}
        </section>

        {/* ─── الجدول ─── */}
        <PrintableReportTable
          headers={tableHeaders}
          rows={rows}
          totals={totals}
          caption={tableCaption}
          captionNumber={tableCaptionNumber}
          compact={compact}
        />

        {/* ─── حالة التوازن ─── */}
        {balanceCheck && (
          <section className="print:page-break-inside-avoid">
            <div
              className={cn(
                "flex items-center justify-between p-3 border-2 rounded text-sm font-bold",
                balanceCheck.isBalanced
                  ? "border-green-700 bg-green-50 text-green-800"
                  : "border-red-700 bg-red-50 text-red-800"
              )}
            >
              <div className="flex items-center gap-3">
                <span className="text-lg">{balanceCheck.isBalanced ? "✓" : "✗"}</span>
                <div>
                  <span>
                    {balanceCheck.isBalanced ? "الميزان متوازن" : "الميزان غير متوازن"}
                  </span>
                  {!balanceCheck.isBalanced && (
                    <span className="text-xs font-normal mr-2">
                      (الفرق: {balanceCheck.difference.toLocaleString("ar-YE")})
                    </span>
                  )}
                </div>
              </div>

              {balanceCheck.debitTotal != null && balanceCheck.creditTotal != null && (
                <div className="flex items-center gap-6 text-[11px]" dir="rtl">
                  <div className="flex items-center gap-1">
                    <span>{balanceCheck.debitLabel ?? "المدين"}:</span>
                    <span className="font-mono font-bold text-xs" dir="ltr">
                      {balanceCheck.debitTotal.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </span>
                  </div>
                  <div className="flex items-center gap-1">
                    <span>{balanceCheck.creditLabel ?? "الدائن"}:</span>
                    <span className="font-mono font-bold text-xs" dir="ltr">
                      {balanceCheck.creditTotal.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </span>
                  </div>
                </div>
              )}
            </div>
          </section>
        )}

        {/* ─── محتوى إضافي ─── */}
        {beforeSignatures}

        {/* ─── ملاحظات ─── */}
        {notes && notes.length > 0 && (
          <section className="print:page-break-inside-avoid">
            <h3 className="text-xs font-black mb-2">ملاحظات:</h3>
            <ul className="text-[11px] space-y-1 list-disc list-inside">
              {notes.map((note, i) => (
                <li key={i}>{note}</li>
              ))}
            </ul>
          </section>
        )}

        {/* ─── التواقيع ─── */}
        {signatures && signatures.length > 0 && (
          <PrintSignatureBlock
            signatures={signatures}
            variant="default"
          />
        )}
      </div>
    );

    if (embed) return content;

    return (
      <PrintLayout
        reportName={title}
        config={{
          pageSize,
          orientation,
          sectionTitle: sectionTitle || "",
        }}
      >
        {content}
      </PrintLayout>
    );
  }
);

PrintableReport.displayName = "PrintableReport";
