<<<<<<< HEAD
=======
import { formatDate, getDocumentUrl } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type {
    AgentForPrint,
    EntitledForPrint,
    GuardianForPrint,
    RetireeDto,
    RetirementFileSummary,
    SalaryRecipientForPrint} from "@/components/domain/print/print.types";
import { PRINT_STYLES } from "@/components/domain/print/print.utils";
<<<<<<< HEAD
import { formatDate, getDocumentUrl } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

const { TB, CELL, LBL, VAL, SECTION_HEADER } = PRINT_STYLES;

const getUrl = (p: string | null | undefined) => {
    if (!p) return "";
    return getDocumentUrl(p) ?? "";
};

interface RetirementFilePrintData extends RetirementFileSummary {
    personalPhotoPath?: string | null;
    ActivePaymentBookNumber?: string | null;
    statusID?: number | null;
    fileStatusName?: string | null;
}

interface RetireePrintData extends RetireeDto {
    personalPhotoPath?: string | null;
}

interface PrintableRetirementFileReportProps {
    file: RetirementFilePrintData;
    retiree: RetireePrintData;
    entitled: EntitledForPrint[];
    agents: AgentForPrint[];
    guardians: GuardianForPrint[];
    financial: { totalAmount?: number; amount?: number; effectiveDate?: string } | null | undefined;
    recipients: SalaryRecipientForPrint[];
    paymentBooks?: { bookNumber?: string; status?: string; bookType?: string; recipientName?: string | null; issueDate?: string; expiryDate?: string }[];
}

export function PrintableRetirementFileReport({
    file,
    retiree,
    entitled,
    agents,
    guardians,
    financial,
    recipients,
    paymentBooks = []
}: PrintableRetirementFileReportProps) {
    const user = useAuthStore(s => s.user);

    const retireePhoto = retiree.personalPhotoPath || file.personalPhotoPath;
    const activePaymentBookNumber = file.activePaymentBookNumber || file.ActivePaymentBookNumber || paymentBooks.find((b: any) => b.status === "فعال")?.bookNumber || paymentBooks[0]?.bookNumber;

    const activeRecipientRecord = recipients?.find(r => r.isActive);
    let activeRecipientName = "—";
    let recipientPhoto: string | null | undefined = null;

    if (activeRecipientRecord) {
        if (activeRecipientRecord.agentId) {
            const agent = agents?.find(a => a.agentId === activeRecipientRecord.agentId);
            activeRecipientName = agent?.fullName || activeRecipientRecord.recipientName || "وكيل غير محدد";
            recipientPhoto = agent?.personalPhotoPath;
        } else if (activeRecipientRecord.guardianId) {
            const guardian = guardians?.find(g => g.guardianId === activeRecipientRecord.guardianId);
            activeRecipientName = guardian?.fullName || activeRecipientRecord.recipientName || "وصي غير محدد";
            recipientPhoto = guardian?.personalPhotoPath;
        } else if (activeRecipientRecord.entitledId) {
            const ep = entitled?.find(e => e.entitledId === activeRecipientRecord.entitledId);
            activeRecipientName = ep?.fullName || activeRecipientRecord.recipientName || "مستحق غير محدد";
            recipientPhoto = ep?.personalPhotoPath;
        } else {
            activeRecipientName = "المتقاعد نفسه";
            recipientPhoto = retireePhoto;
        }
    } else if (activePaymentBookNumber) {
        activeRecipientName = "المتقاعد نفسه";
        recipientPhoto = retireePhoto;
    }

    return (
        <div className="text-black w-full flex flex-col min-h-full">
            {/* ═══ Photos + Title Row ═══ */}
            <div className="flex items-start mb-2 mt-2">
                <div className="flex-1" />
                <div className="flex-none flex justify-center pt-2">
                    <h1 className="text-[24px] font-[900] text-center tracking-[0.5px]">
                        تقرير ملف التقاعد
                    </h1>
                </div>
                <div className="flex-1 flex gap-4 justify-end">
                    <div className="flex flex-col items-center w-16">
                        <p className="text-[9px] font-bold mb-1 border-b border-black w-full text-center pb-0.5">صورة المتقاعد</p>
                        {retireePhoto ? (
                            <img src={getUrl(retireePhoto)} alt="صورة المتقاعد" className="w-16 h-20 object-cover border-2 border-black rounded" />
                        ) : (
<<<<<<< HEAD
                            <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 flex items-center justify-center text-[9px] text-muted-foreground">لا توجد</div>
=======
                            <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 flex items-center justify-center text-[9px] text-muted-foreground/60">لا توجد</div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        )}
                    </div>
                    <div className="flex flex-col items-center w-16">
                        <p className="text-[9px] font-bold mb-1 border-b border-black w-full text-center pb-0.5">مستلم المعاش</p>
                        {recipientPhoto ? (
                            <img src={getUrl(recipientPhoto)} alt="صورة المستلم" className="w-16 h-20 object-cover border-2 border-black rounded" />
                        ) : (
<<<<<<< HEAD
                            <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 text-muted-foreground">لا توجد</div>
=======
                            <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 text-muted-foreground/60">لا توجد</div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        )}
                    </div>
                </div>
            </div>

            <div className="space-y-6">
                {/* 1. البيانات الأساسية */}
                <section className={`${TB} w-full`}>
                    <div className={SECTION_HEADER}>
                        <span>البيانات الأساسية للمتقاعد</span>
                    </div>
                    <table className="w-full border-collapse">
                        <tbody>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الاسم الكامل</td>
                                <td className={`${VAL} font-extrabold text-[14px]`} colSpan={3}>
                                    {retiree.fullName}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الرتبة</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {file.rankName || retiree.rankName || "غير محدد"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>الرقم العسكري</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {retiree.militaryNumber || "---"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>رقم الملف</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {file.fileNumber || file.fileID}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>تاريخ فتح الملف</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {file.fileOpenDate ? formatDate(file.fileOpenDate) : "---"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الحالة الاجتماعية</td>
                                <td className={`${VAL}`} colSpan={3}>
                                    {retiree.socialStatusName || "---"}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </section>

                {/* 2. المستحقون */}
                {entitled && entitled.length > 0 && (
                    <section className={`${TB} w-full`}>
                        <div className={SECTION_HEADER}>
                            <span>المستحقون عن المتقاعد</span>
                            <span className="text-[11px] font-bold text-slate-500 me-auto">({entitled.length})</span>
                        </div>
                        <table className="w-full border-collapse text-[9px] table-fixed">
                            <thead className="bg-muted/50 text-[10px]">
                                <tr>
                                    <th className={`${CELL} ${TB} text-center w-[4%]`}>م</th>
                                    <th className={`${CELL} ${TB} text-center w-[32%]`}>الاسم الكامل</th>
                                    <th className={`${CELL} ${TB} text-center w-[12%]`}>صلة القرابة</th>
                                    <th className={`${CELL} ${TB} text-center w-[18%]`}>الحالة الاجتماعية</th>
                                    <th className={`${CELL} ${TB} text-center w-[22%]`}>الرقم الوطني</th>
                                    <th className={`${CELL} ${TB} text-center w-[12%]`}>الحالة</th>
                                </tr>
                            </thead>
                            <tbody>
                                {entitled.map((person, index) => (
                                    <tr key={person.entitledId || index} className={TB}>
                                        <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{index + 1}</td>
                                        <td className={`${CELL} ${TB} text-start font-bold break-words whitespace-normal overflow-hidden`}>{person.fullName}</td>
                                        <td className={`${CELL} ${TB} text-center break-words whitespace-normal overflow-hidden`}>{person.relationshipName || "---"}</td>
                                        <td className={`${CELL} ${TB} text-center break-words whitespace-normal overflow-hidden`}>{person.socialStatusName || "---"}</td>
                                        <td className={`${CELL} ${TB} text-center font-mono`}>{person.nationalID || "---"}</td>
                                        <td className={`${CELL} ${TB} text-center font-bold`}>
                                            {person.isActive ? "مستحق" : "غير مستحق"}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </section>
                )}

                {/* 3. الوكلاء */}
                {agents && agents.length > 0 && (
                    <section className={`${TB} w-full`}>
                        <div className={SECTION_HEADER}>
                            <span>الوكلاء المعتمدون</span>
                        </div>
                        <table className="w-full border-collapse text-[11px] table-fixed">
                            <thead className="bg-muted/50">
                                <tr>
                                    <th className={`${CELL} ${TB} text-center w-[5%]`}>م</th>
                                    <th className={`${CELL} ${TB} text-center w-[45%]`}>الاسم الكامل</th>
                                    <th className={`${CELL} ${TB} text-center w-[25%]`}>رقم الوكالة</th>
                                    <th className={`${CELL} ${TB} text-center w-[25%]`}>تاريخ الانتهاء</th>
                                </tr>
                            </thead>
                            <tbody>
                                {agents.map((agent, index) => (
                                    <tr key={agent.agentId || index} className={TB}>
                                        <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{index + 1}</td>
                                        <td className={`${CELL} ${TB} text-start font-bold`}>{agent.fullName}</td>
                                        <td className={`${CELL} ${TB} text-center font-mono`}>{agent.agencyNumber || "---"}</td>
                                        <td className={`${CELL} ${TB} text-center`}>
                                            {agent.agencyEndDate ? formatDate(agent.agencyEndDate) : "---"}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </section>
                )}

                {/* 4. الأوصياء */}
                {guardians && guardians.length > 0 && (
                    <section className={`${TB} w-full`}>
                        <div className={SECTION_HEADER}>
                            <span>الأوصياء المعتمدون</span>
                        </div>
                        <table className="w-full border-collapse text-[11px] table-fixed">
                            <thead className="bg-muted/50">
                                <tr>
                                    <th className={`${CELL} ${TB} text-center w-[5%]`}>م</th>
                                    <th className={`${CELL} ${TB} text-center w-[45%]`}>الاسم الكامل</th>
                                    <th className={`${CELL} ${TB} text-center w-[50%]`}>رقم قرار الوصاية</th>
                                </tr>
                            </thead>
                            <tbody>
                                {guardians.map((guardian, index) => (
                                    <tr key={guardian.guardianId || index} className={TB}>
                                        <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{index + 1}</td>
                                        <td className={`${CELL} ${TB} text-start font-bold`}>{guardian.fullName}</td>
                                        <td className={`${CELL} ${TB} text-center font-mono`}>{guardian.guardianshipDocNumber || "---"}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </section>
                )}

                {/* تفاصيل الصرف */}
                <section className={`${TB} w-full`}>
                    <div className={SECTION_HEADER}>
                        <span>تفاصيل الصرف والتحويل المالي</span>
                    </div>
                    <table className="w-full border-collapse">
                        <tbody>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>رقم دفتر الصرف</td>
                                <td className={`${VAL} w-[30%] ${TB} font-bold`}>
                                    {activePaymentBookNumber || "—"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>المستلم الفعلي</td>
                                <td className={`${VAL} w-[30%] font-extrabold`}>
                                    {activeRecipientName}
                                </td>
                            </tr>
                            {paymentBooks.length > 0 && paymentBooks.map((book: any, idx: number) => (
                                <tr key={idx} className={TB}>
                                    <td className={`${LBL} ${TB} w-[20%]`}>دفتر {idx + 1}</td>
                                    <td className={`${VAL} ${TB} w-[30%]`}>
                                        <span className="font-mono font-bold">{book.bookNumber || "—"}</span>
                                        <span className="mx-2">—</span>
                                        <span>{book.bookType || "—"}</span>
                                    </td>
                                    <td className={`${LBL} ${TB} w-[20%]`}>الحالة / المستلم</td>
                                    <td className={`${VAL} ${TB} w-[30%]`}>
                                        <span className={`font-bold ${book.status === "فعال" ? "text-green-700" : "text-red-700"}`}>{book.status || "—"}</span>
                                        <span className="mx-2">—</span>
                                        <span>{book.recipientName || "—"}</span>
                                    </td>
                                </tr>
                            ))}
                            {financial && (
                                <tr className={TB}>
                                    <td className={`${LBL} ${TB}`}>إجمالي المعاش التقاعدي</td>

                                    <td className={`${VAL} text-[16px] font-black text-primary ${TB}`}>
                                        {financial.totalAmount?.toLocaleString("ar-YE") || financial.amount?.toLocaleString("ar-YE") || 0} ر.ي
                                    </td>
                                    <td className={`${LBL} ${TB}`}>تاريخ السريان</td>
                                    <td className={`${VAL}`}>
                                        {financial.effectiveDate ? formatDate(financial.effectiveDate) : "---"}
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </section>

                {/* 6. جميع المستلمون */}
                {recipients && recipients.length > 0 && (
                    <section className={`${TB} w-full`}>
                        <div className={SECTION_HEADER}>
                            <span>سادساً: المستلمون المعتمدون في الملف</span>
                        </div>
                        <table className="w-full border-collapse text-[11px] table-fixed">
                            <thead className="bg-muted/50">
                                <tr>
                                    <th className={`${CELL} ${TB} text-center w-[5%]`}>م</th>
                                    <th className={`${CELL} ${TB} text-center w-[45%]`}>اسم المستلم</th>
                                    <th className={`${CELL} ${TB} text-center w-[30%]`}>نوع الاستلام</th>
                                    <th className={`${CELL} ${TB} text-center w-[20%]`}>الحالة</th>
                                </tr>
                            </thead>
                            <tbody>
                                {recipients.map((recipient, index) => (
                                    <tr key={recipient.recipientId || index} className={TB}>
                                        <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{index + 1}</td>
                                        <td className={`${CELL} ${TB} text-start font-bold`}>{recipient.recipientName}</td>
                                        <td className={`${CELL} ${TB} text-center`}>{recipient.recipientType || "مستحق مباشر"}</td>
                                        <td className={`${CELL} ${TB} text-center font-bold`}>
                                            {recipient.isActive ? "فعال" : "متوقف"}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </section>
                )}
            </div>

            {/* Spacer: pushes signature to bottom */}
            <div className="flex-1" />

            {/* ═══ التوقيعات ═══ */}
            <div className="grid grid-cols-3 gap-8 text-center mt-8 px-2 print:page-break-inside-avoid">
                <div className="col-start-3 flex flex-col items-center">
                    <p className="text-[15px] font-black mb-8">
                        المختص
                    </p>
                    <p className="text-[15px] font-extrabold text-black whitespace-nowrap">
                        {user?.fullName || 'الاســــم والتوقيـــع'}
                    </p>
                </div>
            </div>
        </div>
    );
}
