import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer } from "lucide-react";
import { useNavigate,useParams } from "react-router-dom";

import type { SettlementDecisionDto, SettlementItemDto } from "@/api/settlements";
import { settlementApi } from "@/api/settlements";
import { PrintSignatureBlock } from "@/components/common/PrintSignatureBlock";
import { PrintSummaryBox } from "@/components/common/PrintSummaryBox";
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { formatDate } from "@/lib/utils";

// ─── Print Page ────────────────────────────────────────

export function SettlementDecisionPrint() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const decisionId = Number(id);

    const { data: decisionResponse, isLoading: loadingDecision, isError: decisionError } = useQuery({
        queryKey: ["settlement-decision", decisionId],
        queryFn: () => settlementApi.getDecision(decisionId),
        enabled: decisionId > 0,
    });

    const { data: itemsPaged, isLoading: loadingItems, isError: itemsError } = useQuery({
        queryKey: ["settlement-decision-items", decisionId],
        queryFn: () => settlementApi.getDecisionItems(decisionId, { page: 1, pageSize: 5000 }),
        enabled: decisionId > 0,
    });

    const loading = loadingDecision || loadingItems;
    const decision: SettlementDecisionDto | undefined = decisionResponse?.data;
    const items: SettlementItemDto[] = itemsPaged?.items || [];

    if (!decisionId || isNaN(decisionId)) {
        return <div className="flex justify-center py-20 text-destructive">معرف القرار غير صالح.</div>;
    }

    if (loading) {
        return <div className="flex justify-center py-20 text-muted-foreground">جاري التحميل...</div>;
    }

    if (decisionError || itemsError) {
        return <div className="flex justify-center py-20 text-destructive">تعذر تحميل بيانات القرار</div>;
    }

    if (!decision) {
        return <div className="flex justify-center py-20 text-destructive">عذراً، لم يتم العثور على القرار المطلوب.</div>;
    }

    const totalAmount = items.reduce((sum, item) => sum + (item.processedRankSalary || 0), 0);
    const matchedCount = items.filter(i => i.retireeID).length;
    const unmatchedCount = items.filter(i => !i.retireeID).length;

    const summaryItems = [
        { label: "إجمالي القيود", value: items.length },
        { label: "مطابق (تم الربط)", value: matchedCount, color: 'success' as const },
        { label: "غير مطابق (معلق)", value: unmatchedCount, color: 'warning' as const },
    ];

    const signatures = [
        { title: "مدير إدارة التقاعد", sub: "التوقيع والختم" },
        { title: "مدير مركز الحاسب الآلي", sub: "التوقيع والختم الرسمي" },
    ];

    return (
        <>
            {/* Screen-only controls */}
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => navigate(`/settlements/${id}`)}>
                    <ArrowRight className="w-5 h-5 ms-2" />
                    رجوع للقرار
                </Button>
                <div className="flex gap-2">
                    <Button onClick={() => window.print()} className="flex items-center">
                        <Printer className="w-4 h-4 ms-2" />
                        طباعة القرار
                    </Button>
                </div>
            </div>

            {/* Printable Area Wrapper */}
            <div className="w-full flex justify-center">
                <PrintLayout 
                    reportName="قرار تسوية رتب" 
                    reportNumber={decision.decisionNumber}
                    date={decision.decisionDate}
                    config={{ sectionTitle: "إدارة المعاشات" }}
                >
                    <div className="relative z-10 px-4 pb-2" dir="rtl">
                        {/* 1. بيانات القرار */}
                        <section className="mb-4">
                            <h3 className="text-sm font-bold bg-muted print:bg-gray-100 border-e-4 border-primary-600 print:border-black px-2 py-1 mb-2 rounded-s">
                                1. بيانات القرار
                            </h3>
                            <div className="grid grid-cols-3 gap-y-2 gap-x-4 text-[11px]">
                                <PrintField label="رقم القرار" value={decision.decisionNumber} />
                                <PrintField label="تاريخ القرار" value={formatDate(decision.decisionDate)} />
                                <PrintField label="نوع القرار" value={decision.decisionType || "تسوية رتب"} />
                                <PrintField label="عدد البنود" value={String(items.length)} />
                                <PrintField label="إجمالي المبالغ" value={`${totalAmount.toLocaleString("ar-YE")} ر.ي`} />
                                <PrintField label="بواسطة" value={decision.createdByUserName || "—"} />
                            </div>
                            {decision.notes && (
                                <div className="mt-2 text-[10px] text-muted-foreground bg-muted/50 p-2 rounded border border-border">
                                    <span className="font-bold">ملاحظات: </span>{decision.notes}
                                </div>
                            )}
                        </section>

                        {/* 2. ملخص الحالة */}
                        <PrintSummaryBox items={summaryItems} />

                        {/* 3. جدول البنود */}
                        <section className="mb-4">
                            <h3 className="text-sm font-bold bg-muted print:bg-gray-100 border-e-4 border-primary-600 print:border-black px-2 py-1 mb-2 rounded-s">
                                2. كشف البنود المشمولة بالقرار ({items.length} بند)
                            </h3>
                                <div className="overflow-hidden border border-border print:border-black rounded-lg">
                                <table className="w-full text-[10px] text-end">
                                    <thead>
                                        <tr className="bg-muted/50 print:bg-gray-100 border-b border-border print:border-black font-bold">
                                            <th className="p-1.5 w-8">ت</th>
                                            <th className="p-1.5">الرقم العسكري</th>
                                            <th className="p-1.5">الاسم</th>
                                            <th className="p-1.5">الرتبة الحالية</th>
                                            <th className="p-1.5">الرتبة المعالجة</th>
                                            <th className="p-1.5">المعاش المعتمد</th>
                                            <th className="p-1.5">الحالة</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-200 print:divide-black">
                                        {items.map((item, index) => (
                                            <tr key={item.settlementItemID}>
                                                <td className="p-1.5 font-mono text-muted-foreground/60 print:text-black">{index + 1}</td>
                                                <td className="p-1.5 font-mono">{item.militaryNumber || "—"}</td>
                                                <td className="p-1.5 font-medium">{item.retireeFullName}</td>
                                                <td className="p-1.5 text-muted-foreground">{item.currentRankName}</td>
                                                <td className="p-1.5 font-bold">{item.processedRankName}</td>
                                                <td className="p-1.5 font-mono font-bold">{(item.processedRankSalary ?? 0).toLocaleString("ar-YE")} ر.ي</td>
                                                <td className="p-1.5 text-[9px]">
                                                    {item.retireeID ? "✓ مطابق" : "✗ غير مطابق"}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                    <tfoot>
                                        <tr className="bg-muted print:bg-gray-100 border-t-2 border-border print:border-black font-bold text-[11px]">
                                            <td colSpan={5} className="p-2 text-start">
                                                الإجمالي: {items.length} بند
                                            </td>
                                            <td className="p-2 font-mono">{totalAmount.toLocaleString("ar-YE")} ر.ي</td>
                                            <td className="p-2"></td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </div>
                        </section>

                        {/* 4. الاعتماد النهائي */}
                        <PrintSignatureBlock signatures={signatures} variant="boxes" />
                    </div>
                </PrintLayout>
            </div>
        </>
    );
}

function PrintField({ label, value }: { label: string; value: string }) {
    return (
        <div className="flex gap-1">
            <span className="font-semibold">{label}:</span>
            <span>{value}</span>
        </div>
    );
}
