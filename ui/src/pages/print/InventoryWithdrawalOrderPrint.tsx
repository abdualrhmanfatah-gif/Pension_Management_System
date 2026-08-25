import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";

import { transactionApi } from "@/api/inventory/transactions";
import { PrintSignatureBlock } from "@/components/common/PrintSignatureBlock";
import { Button } from "@/components/ui";
import { formatDate } from "@/lib/utils";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { useAuthStore } from "@/stores/authStore";

export function InventoryWithdrawalOrderPrint() {
    const { orderNumber } = useParams<{ orderNumber: string }>();
    const navigate = useNavigate();
    const currentUser = useAuthStore(s => s.user);

    const TB = "border border-border print:border-black";
    const CELL = "p-2";

    const { data: transactions, isLoading, isError } = useQuery({
        queryKey: ["inventory-withdrawal-order", orderNumber],
        queryFn: () => transactionApi.getByWithdrawalOrder(orderNumber!),
        enabled: !!orderNumber,
    });

    if (isLoading) {
        return <div className="flex justify-center py-20 text-muted-foreground italic text-sm">جاري تحميل الأمر...</div>;
    }

    if (isError || !transactions || transactions.length === 0) {
        return <div className="flex justify-center py-20 text-destructive font-bold">لم يتم العثور على أمر الصرف المطلوب.</div>;
    }

    const first = transactions[0];
    const totalCost = transactions.reduce((sum, t) => sum + (t.unitCost ?? 0) * t.quantity, 0);

    const signatures = [
        { title: "المستلم", sub: "الاسم / التوقيع" },
        { title: "المصدر", sub: currentUser?.fullName || "موظف المخازن" },
        { title: "مدير الإدارة", sub: "الاسم / التوقيع" },
    ];

    return (
        <>
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => navigate(-1)}>
                    <ArrowRight className="w-5 h-5 ms-2" /> رجوع
                </Button>
                <div className="flex gap-2">
                    <Button onClick={() => window.print()} className="flex items-center font-bold">
                        <Printer className="w-4 h-4 ms-2" /> طباعة الأمر
                    </Button>
                </div>
            </div>

            <div className="w-full flex justify-center">
                <PrintLayout
                    reportName="أمر صرف مخزني"
                    reportNumber={orderNumber}
                    date={first.transactionDate}
                    config={{ sectionTitle: "إدارة المخازن والأصول" }}
                >
                    <div className="px-8 pb-4">
                        <section className="mb-4">
                            <table className="w-full border-collapse text-[12px] table-fixed">
                                <tbody>
                                    <tr>
                                        <td className={`${CELL} ${TB} w-[15%] bg-slate-100 print:bg-slate-200 font-bold`}>رقم الأمر</td>
                                        <td className={`${CELL} ${TB} w-[35%] font-black text-[13px] font-mono`} dir="ltr">{orderNumber}</td>
                                        <td className={`${CELL} ${TB} w-[15%] bg-slate-100 print:bg-slate-200 font-bold`}>تاريخ الأمر</td>
                                        <td className={`${CELL} ${TB} w-[35%] font-black text-[13px] font-mono`}>{formatDate(first.transactionDate)}</td>
                                    </tr>
                                    {first.reason && (
                                        <tr>
                                            <td className={`${CELL} ${TB} bg-slate-100 print:bg-slate-200 font-bold`}>سبب الصرف</td>
                                            <td className={`${CELL} ${TB} font-black text-[13px]`} colSpan={3}>{first.reason}</td>
                                        </tr>
                                    )}
                                    <tr>
                                        <td className={`${CELL} ${TB} bg-slate-100 print:bg-slate-200 font-bold`}>المصدر</td>
                                        <td className={`${CELL} ${TB} font-black text-[13px]`} colSpan={3}>{currentUser?.fullName || "—"}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </section>

                        <section className="mb-4">
                            <h3 className="text-base font-bold mb-2 print:text-black border-b border-black pb-1">الأصناف المصروفة</h3>
                            <table className="w-full border-collapse text-[12px] table-fixed">
                                <thead className="bg-slate-100 print:bg-slate-200">
                                    <tr>
                                        <th className={`${CELL} ${TB} text-center w-[8%] font-bold`}>م</th>
                                        <th className={`${CELL} ${TB} text-center w-[15%] font-bold`}>رمز الصنف</th>
                                        <th className={`${CELL} ${TB} text-start w-[32%] font-bold`}>اسم الصنف</th>
                                        <th className={`${CELL} ${TB} text-center w-[15%] font-bold`}>الكمية</th>
                                        <th className={`${CELL} ${TB} text-center w-[15%] font-bold`}>تكلفة الوحدة</th>
                                        <th className={`${CELL} ${TB} text-center w-[15%] font-bold`}>الإجمالي</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {transactions.map((t, idx) => (
                                        <tr key={t.inventoryTransactionID} className="even:bg-slate-50 print:even:bg-slate-100">
                                            <td className={`${CELL} ${TB} text-center font-mono font-bold`}>{idx + 1}</td>
                                            <td className={`${CELL} ${TB} text-center font-mono font-bold`}>{t.itemCode}</td>
                                            <td className={`${CELL} ${TB} font-bold`}>{t.itemName}</td>
                                            <td className={`${CELL} ${TB} text-center font-mono font-bold`}>{t.quantity}</td>
                                            <td className={`${CELL} ${TB} text-center font-mono font-bold`}>{t.unitCost != null ? `${t.unitCost.toFixed(2)} ${t.currency}` : "—"}</td>
                                            <td className={`${CELL} ${TB} text-center font-mono font-bold`}>{t.unitCost != null ? `${(t.unitCost * t.quantity).toFixed(2)} ${t.currency}` : "—"}</td>
                                        </tr>
                                    ))}
                                </tbody>
                                <tfoot>
                                    <tr className="bg-slate-100 print:bg-slate-200 font-bold text-[13px]">
                                        <td colSpan={3} className={`${CELL} ${TB} text-start`}>الإجمالي العام</td>
                                        <td className={`${CELL} ${TB} text-center font-mono`}>{transactions.reduce((s, t) => s + t.quantity, 0)}</td>
                                        <td colSpan={2} className={`${CELL} ${TB} text-center font-mono`}>{totalCost.toFixed(2)} {first.currency}</td>
                                    </tr>
                                </tfoot>
                            </table>
                        </section>

                        <PrintSignatureBlock signatures={signatures} className="mt-10" />
                    </div>
                </PrintLayout>
            </div>
        </>
    );
}
