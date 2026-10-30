import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer } from "lucide-react";
import { useNavigate,useParams } from "react-router-dom";

import { retirementApi } from "@/api/retirement";
<<<<<<< HEAD
import { PrintSignatureBlock } from "@/components/common/PrintSignatureBlock";
import { Button } from "@/components/ui";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { formatDate } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
import type { MissingDocumentInfo } from "@/types/retirement";
=======
import type { MissingDocumentInfo } from "@/types/retirement";
import { PrintSignatureBlock } from "@/components/common/PrintSignatureBlock";
import { Button } from "@/components/ui";
import { formatDate } from "@/lib/utils";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { useAuthStore } from "@/stores/authStore";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Print Page ────────────────────────────────────────

export function DocumentPledgePrint() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const pledgeId = Number(id);
    const currentUser = useAuthStore(s => s.user);

    const TB = "border border-border print:border-black";
    const CELL = "p-2";

    const { data: pledge, isLoading, isError } = useQuery({
        queryKey: ["document-pledge", pledgeId],
        queryFn: () => retirementApi.getPledge(pledgeId),
        enabled: !!pledgeId,
    });

    if (isLoading) {
        return <div className="flex justify-center py-20 text-muted-foreground italic text-sm">جاري تحميل بيانات التعهد...</div>;
    }

    if (isError || !pledge) {
        return <div className="flex justify-center py-20 text-destructive font-bold">عذراً، لم يتم العثور على التعهد المطلوب أو حدث خطأ أثناء التحميل.</div>;
    }

    const signatures = [
        { title: "المختص", sub: currentUser?.fullName || "التوقيع والختم" },
        { title: "المتعهد", sub: pledge.pledgedByName || "بصمة/توقيع المتعهد" },
    ];

    return (
        <>
            {/* Screen-only controls */}
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => navigate(-1)}>
                    <ArrowRight className="w-5 h-5 ms-2" />
                    رجوع
                </Button>
                <div className="flex gap-2">
                    <Button onClick={() => window.print()} className="flex items-center font-bold">
                        <Printer className="w-4 h-4 ms-2" />
                        طباعة التعهد
                    </Button>
                </div>
            </div>

            {/* Printable Area */}
            <div className="w-full flex justify-center">
                <PrintLayout 
                    reportName="تعهد بإحضار وثائق" 
                    reportNumber={String(pledge.pledgeNumber || pledge.pledgeID)}
                    config={{ sectionTitle: "إدارة المعاشات" }}
                >
                    <div className="pledge-content relative z-10 px-8 pb-4 print:break-inside-avoid" dir="rtl">
                        
                        {/* 2. Person Info */}
                        <section className="mb-4">
                            <table className="w-full border-collapse text-[12px] table-fixed">
                                <tbody>
                                    <tr>
                                        <td className={`${CELL} ${TB} w-[15%] bg-slate-100 print:bg-slate-200 font-bold text-foreground print:text-black`}>اسم المتقاعد</td>
                                        <td className={`${CELL} ${TB} w-[35%] font-black text-[13px] text-foreground print:text-black`}>{pledge.retireeName || "—"}</td>
                                        <td className={`${CELL} ${TB} w-[15%] bg-slate-100 print:bg-slate-200 font-bold text-foreground print:text-black`}>رقم الملف</td>
                                        <td className={`${CELL} ${TB} w-[35%] font-black text-[13px] text-foreground print:text-black font-mono`}>{String(pledge.fileID)}</td>
                                    </tr>
                                    <tr>
                                        <td className={`${CELL} ${TB} bg-slate-100 print:bg-slate-200 font-bold text-foreground print:text-black`}>اسم المتعهد</td>
                                        <td className={`${CELL} ${TB} font-black text-[13px] text-foreground print:text-black`}>{pledge.pledgedByName || "—"}</td>
                                        <td className={`${CELL} ${TB} bg-slate-100 print:bg-slate-200 font-bold text-foreground print:text-black`}>صفة المتعهد</td>
                                        <td className={`${CELL} ${TB} font-black text-[13px] text-foreground print:text-black`}>{pledge.pledgedByType || "—"}</td>
                                    </tr>
                                    <tr>
                                        <td className={`${CELL} ${TB} bg-slate-100 print:bg-slate-200 font-bold text-foreground print:text-black`}>حالة التعهد</td>
                                        <td className={`${CELL} ${TB} font-black text-[13px] text-foreground print:text-black`} colSpan={3}>{pledge.status === "Active" ? "نشط / قيد التنفيذ" : "مستوفى"}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </section>

                        {/* 3. Pledged Documents */}
                        <section className="mb-4">
                            <h3 className="text-base font-bold mb-2 print:text-black">الوثائق الملتزم بإحضارها</h3>
                            <p className="text-[13px] leading-normal text-foreground print:text-black font-bold mb-2">
                                أتعهد بإحضار وتسليم الوثائق التالية لإدارة المعاشات في موعد أقصاه تاريخ الاستحقاق: <span className="font-mono text-amber-700 print:text-black mx-1">{formatDate(pledge.deadline)}</span>
                            </p>
                            <table className="w-full border-collapse text-[12px] table-fixed">
                                <thead className="bg-slate-100 print:bg-slate-200 text-foreground print:text-black">
                                    <tr>
                                        <th className={`${CELL} ${TB} text-center w-[10%] font-bold`}>م</th>
                                        <th className={`${CELL} ${TB} text-start w-[50%] font-bold`}>نوع الوثيقة</th>
                                        <th className={`${CELL} ${TB} text-start w-[40%] font-bold`}>صاحب الوثيقة</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {pledge.missingDocuments?.map((doc: MissingDocumentInfo, idx: number) => (
                                        <tr key={idx} className="even:bg-slate-50 print:even:bg-slate-100">
                                            <td className={`${CELL} ${TB} text-center font-mono text-foreground print:text-black font-bold`}>{idx + 1}</td>
                                            <td className={`${CELL} ${TB} font-bold text-foreground print:text-black`}>{doc.documentTypeName}</td>
                                            <td className={`${CELL} ${TB} text-foreground print:text-black font-bold`}>{getEntityLabel(doc.entityType, doc.entityName)}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </section>

                        {/* 4. Legal Declaration */}
                        <section className="mt-4 px-2">
                            <p className="text-[14px] leading-relaxed text-justify text-foreground print:text-black font-bold">
                                أقر بصحة البيانات المسجلة وأتعهد بتسليم الوثائق المطلوبة قبل انتهاء تاريخ الاستحقاق (<span className="font-mono" dir="ltr">{formatDate(pledge.deadline)}</span>). وفي حال الإخلال بهذا التعهد، يحق لإدارة المعاشات إيقاف صرف المستحقات المالية مؤقتاً لحين استكمال النواقص، دون أدنى مسؤولية قانونية عليها.
                            </p>
                        </section>

                        {/* 5. Signatures */}
                        <PrintSignatureBlock signatures={signatures} className="mt-10" />

                    </div>
                </PrintLayout>
            </div>
        </>
    );
}

function getEntityLabel(type?: string | null, name?: string | null) {
    const labels: Record<string, string> = {
        "Agent": "الوكيل",
        "Guardian": "الوصي",
        "EntitledPerson": "المستحق",
        "Retiree": "المتقاعد"
    };
    const typeLabel = type && labels[type] ? labels[type] : "";
    if (!typeLabel && !name) return "المتقاعد";
    return [typeLabel, name].filter(Boolean).join(" - ");
}
