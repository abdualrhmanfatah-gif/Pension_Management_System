import { forwardRef } from "react";

import type { VoucherDto } from "@/api/vouchers";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { formatCurrency } from "@/lib/utils";

interface PrintablePaymentOrderProps {
    voucher: VoucherDto;
}

/** الموقّعون الثابتون على أمر الصرف */
const SIGNATORIES = [
    { rank: "عقيد", name: "منصور اليماني", title: "رئيس قسم الحسابات" },
    { rank: "نقيب", name: "خالد صالح بن صالح", title: "قسم المصروفات والإيرادات" },

    { rank: "عقيد ركن", name: "شامخ عبدالرحمن قتاح", title: "المدير المالي" },
    { rank: "عميد ركن", name: "جمال القطيبي", title: "المدير العام" },
] as const;

const ARABIC_MONTHS = [
    "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
    "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
];

const QUARTER_NAMES = ["", "الأول", "الثاني", "الثالث", "الرابع"];

/**
 * أمر صرف من الصندوق
 */
export const PrintablePaymentOrder = forwardRef<HTMLDivElement, PrintablePaymentOrderProps>(
    ({ voucher }, ref) => {
        const isPayment = voucher.voucherType === 1;
        const netAmount = voucher.amount;
        const expenseMonth = voucher.voucherDate ? ARABIC_MONTHS[new Date(voucher.voucherDate).getMonth()] : "";
        const recipientName = isPayment ? voucher.payeeName : voucher.payerName;

        /* ── أعمدة الجدول (من اليمين لليسار في RTL) ── */
        const summaryColumns = [
            { label: "المبلغ", value: formatCurrency(voucher.amount), mono: true },
            { label: "الضريبة", value: formatCurrency(0), mono: true },
            { label: "صافي المبلغ", value: formatCurrency(netAmount), mono: true },
            { label: "نوع الإنفاق", value: voucher.debitAccountName || "ميزانية تشغيلية", mono: false },
        ];

        return (
            <div ref={ref}>
                <PrintLayout
                    reportName="أمر صرف من الصندوق"
                    reportNumber={String(voucher.voucherID)}
                    attachments={voucher.journalEntryNumber || "—"}
                    date={voucher.voucherDate}
                    config={{ sectionTitle: "نظام إدارة صندوق التقاعد الأمني" }}
                >
                    <div dir="rtl" className="px-3 flex flex-col">

                        {/* ═══ جدول الملخص (سطر واحد) ═══ */}
                        <div className="po2-grid">
                            {summaryColumns.map((col) => (
                                <div key={col.label} className="po2-cell">
                                    <div className="po2-label">{col.label}</div>
                                    <div
                                        className="po2-value"
                                        style={{ fontFamily: col.mono ? "monospace" : undefined }}
                                    >
                                        {col.value}
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* ═══ المخاطَب ═══ */}
                        <div className="flex justify-between text-2xl font-black mt-5 mb-4 pe-5 ps-[100px]">
                            <span>الأخ / أمين الصندوق</span>
                            <span>المحترم</span>
                        </div>

                        {/* ═══ نص الأمر ═══ */}
                        <div className="mb-2">

                            {/* السطر 1: يتم صرف صافي المبلغ وقدره _____ ريال يمني */}
                            <div className="po2-line">
                                <span className="whitespace-nowrap">يتم صرف صافي المبلغ وقدره</span>
                                <span className="po2-fill font-mono text-xl">
                                    &nbsp;{formatCurrency(netAmount)}&nbsp;
                                </span>
                                <span className="whitespace-nowrap">ريال يمني</span>
                            </div>

                            {/* السطر 2: للأخ/ للأخوة / _____ مقابل _____ */}
                            <div className="po2-line">
                                <span className="whitespace-nowrap">للأخ/ للأخوة /</span>
                                <span className="po2-fill flex-[2]">
                                    &nbsp;{recipientName || ""}&nbsp;
                                </span>
                                <span className="whitespace-nowrap">مقابل</span>
                                <span className="po2-fill flex-[2]">
                                    &nbsp;{voucher.description || ""}&nbsp;
                                </span>
                            </div>

                            {/* السطر 3: بموجب الأوليات المرفقة عدد ( ) مع أخذ استلام */}
                            <div className="text-base font-semibold mb-2 flex items-baseline gap-1.5 flex-wrap">
                                <span>بموجب الأوليات المرفقة عدد</span>
                                <span className="inline-block font-mono font-black px-1 text-center ltr" dir="ltr">
                                    ({voucher.chequeNumber || ""})
                                </span>
                                <span>مع أخذ استلام بذلك بحسب النظام.</span>
                            </div>

                            {/* السطر 4: ليتم صرفه من الميزانية التشغيلية للفصل (رقم الفصل) لشهر ( ) من شيك رقم ( ) */}
                            <div className="po2-line">
                                <span className="whitespace-nowrap">ليتم صرفه من الميزانية التشغيلية للفصل (</span>
                                <span className="po2-fill max-w-[70px] text-center">
                                    &nbsp;{(() => {
                                        if (!voucher.voucherDate) return "...";
                                        const month = new Date(voucher.voucherDate).getMonth();
                                        const quarter = Math.floor(month / 3) + 1;
                                        return QUARTER_NAMES[quarter] || "...";
                                    })()}&nbsp;
                                </span>
                                <span className="whitespace-nowrap">) لشهر (</span>
                                <span className="po2-fill max-w-[80px] text-center">
                                    &nbsp;{expenseMonth}&nbsp;
                                </span>
                                <span className="whitespace-nowrap">) من شيك رقم (</span>
                                <span className="po2-fill max-w-[140px] text-center font-mono">
                                    &nbsp;{voucher.chequeNumber || ""}&nbsp;
                                </span>
                                <span className="whitespace-nowrap">)</span>
                            </div>
                            {/* ═══ مرسل للتنفيذ ═══ */}
                            <div className="text-center text-[17px] font-black mb-3.5">
                                مرسل للتنفيذ،
                            </div>
                        </div>

                        {/* مسافة لدفع التوقيعات للأسفل */}
                        <div className="min-h-[60px]" />
                      

                        {/* ═══ التوقيعات (5 موقّعين) ═══ */}
                        <div className="po2-sigs">
                            {SIGNATORIES.map((sig) => (
                                <div key={sig.name} className="po2-sig-person">
                                    <div className="po2-sig-rank">{sig.rank} /</div>
                                    <div className="po2-sig-name">{sig.name}</div>
                                    <div className="po2-sig-title">{sig.title}</div>
                                </div>
                            ))}
                        </div>

                        {/* مسافة لدفع قسم الاستلام لأسفل */}
                        <div className="flex-1 min-h-[40px]" />

                        {/* ═══ قسم الاستلام ═══ */}
                        <div className="po2-receipt">
                            <div className="po2-receipt-line">
                                <span className="whitespace-nowrap font-black">
                                    استلمت من الأخ/ أمين الصندوق مبلغ وقدره:
                                </span>
                                <div className="po2-receipt-fill" />
                            </div>
                            <div className="po2-receipt-line">
                                <span className="whitespace-nowrap">اسم المستلم:</span>
                                <div className="po2-receipt-fill" />
                            </div>
                            <div className="po2-receipt-line">
                                <span className="whitespace-nowrap">جهة العمل:</span>
                                <div className="po2-receipt-fill" />
                            </div>
                            <div className="po2-receipt-line">
                                <span className="whitespace-nowrap">التوقيع/ البصمة:</span>
                                <div className="po2-receipt-fill" />
                            </div>
                        </div>

                    </div>
                </PrintLayout>
            </div>
        );
    }
);

PrintablePaymentOrder.displayName = "PrintablePaymentOrder";
