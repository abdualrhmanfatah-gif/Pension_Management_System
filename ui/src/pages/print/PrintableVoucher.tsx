import { forwardRef } from "react";

import type { VoucherDto } from "@/api/vouchers";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { formatCurrency, formatDate } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";

interface PrintableVoucherProps {
    voucher: VoucherDto;
}

/**
 * سند قبض — صفحة واحدة
 */
export const PrintableVoucher = forwardRef<HTMLDivElement, PrintableVoucherProps>(
    ({ voucher }, ref) => {
        const payerName = voucher.payerName || "";
        const currentUser = useAuthStore((s) => s.user);

        const summaryColumns = [
            { label: "رقم السند", value: voucher.voucherNumber, mono: true },
            { label: "التاريخ", value: formatDate(voucher.voucherDate) },
            { label: "الدافع", value: payerName || "—" },
            { label: "المبلغ", value: `${formatCurrency(voucher.amount)} ر.ي`, mono: true },
        ];

        return (
            <div ref={ref}>
                <PrintLayout
                    reportName="سند قبض"
                    reportNumber={voucher.voucherNumber}
                    date={voucher.voucherDate}
                    printOnly
                    config={{ sectionTitle: "الالإدارة المالية", pageSize: "A5", orientation: "landscape" }}
                >
                    <div dir="rtl" className="px-2 block">

                        {/* ═══ جدول الملخص ═══ */}
                        <div className="rv-grid">
                            {summaryColumns.map((col) => (
                                <div key={col.label} className="rv-cell">
                                    <div className="rv-label">{col.label}</div>
                                    <div className="rv-value" style={{ fontFamily: col.mono ? "monospace" : undefined }}>
                                        {col.value}
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* ═══ النص ═══ */}
                        <div className="rv-line">
                            <span className="whitespace-nowrap">استلمت من الأخ /</span>
                            <span className="rv-fill">{payerName}</span>
                        </div>
                        <div className="rv-line">
                            <span className="whitespace-nowrap">مبلغًا وقدره</span>
                            <span className="rv-fill font-mono text-base">
                                {formatCurrency(voucher.amount)} ر.ي
                            </span>
                        </div>
                        <div className="rv-line">
                            <span className="whitespace-nowrap">مقبل /</span>
                            <span className="rv-fill">{voucher.description || ""}</span>
                        </div>

                        {/* ═══ التذييل ═══ */}
                        <div className="rv-footer flex flex-col items-end mt-5">
                            <div className="rv-footer-line w-[300px]">
                                <span className="whitespace-nowrap font-black">اسم المستلم:</span>
                                <span className="rv-footer-fill flex-1 text-center inline-block">{currentUser?.fullName || ""}</span>
                            </div>
                            <div className="rv-footer-line w-[300px] mt-2">
                                <span className="whitespace-nowrap font-black">التوقيع:</span>
                                <span className="rv-footer-fill flex-1 inline-block"></span>
                            </div>
                        </div>

                    </div>
                </PrintLayout>
            </div>
        );
    }
);

PrintableVoucher.displayName = "PrintableVoucher";
