<<<<<<< HEAD
=======
import { formatDate, getDocumentUrl } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
import { Heading } from "@/components/ui";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type {
    FileDetailsForPrint,
    Statement} from "@/components/domain/print/print.types";
import { PRINT_STYLES } from "@/components/domain/print/print.utils";
<<<<<<< HEAD
import { Heading } from "@/components/ui";
import { formatDate, getDocumentUrl } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

const { TB, CELL, LBL, VAL, SECTION_HEADER } = PRINT_STYLES;

const getUrl = (p: string | null | undefined) => {
    if (!p) return "";
    return getDocumentUrl(p) ?? "";
};

interface UnifiedRetireeStatementProps {
    statement: Statement;
    fileDetail?: FileDetailsForPrint | null;
    loadingFileDetail?: boolean;
    title?: string;
}

export function UnifiedRetireeStatement({
    statement,
    fileDetail,
    loadingFileDetail,
    title = "بيانات المتقاعد"
}: UnifiedRetireeStatementProps) {
    const { retiree, approvalItems, retirementFiles, settlements, entitlements } = statement;
    const hasFile = retirementFiles.length > 0;
    const user = useAuthStore(s => s.user);

    const activeRecipient = fileDetail?.recipients?.find((r) => r.isActive);
    const activeRecipientName = activeRecipient?.recipientName ?? "—";

    let activeRecipientPhotoPath: string | null | undefined = null;
    if (activeRecipient) {
        if (activeRecipient.agentId) {
            activeRecipientPhotoPath = fileDetail?.agents?.find(a => a.agentId == activeRecipient.agentId)?.personalPhotoPath;
        } else if (activeRecipient.guardianId) {
            activeRecipientPhotoPath = fileDetail?.guardians?.find(g => g.guardianId == activeRecipient.guardianId)?.personalPhotoPath;
        } else if (activeRecipient.entitledId) {
            activeRecipientPhotoPath = fileDetail?.entitled?.find(e => e.entitledId == activeRecipient.entitledId)?.personalPhotoPath;
        } else {
            activeRecipientPhotoPath = fileDetail?.file?.personalPhotoPath;
        }
    }


    const getGuardianNameForEntitled = (entitledId: number): string => {
        if (!fileDetail?.guardians?.length) return "—";
        const g = fileDetail.guardians.find(
            (guardian) => guardian.minors?.some((m) => m.entitledId === entitledId && m.isActive)
        );
        return g ? g.fullName : "—";
    };

    return (
        <div className="text-black w-full flex flex-col min-h-full">
            {/* ═══ Photos + Title Row ═══ */}
            <div className="flex items-start mb-2 mt-2">
                <div className="flex-1" />
                <div className="flex-none flex justify-center pt-2">
                    <Heading level="h1" className="text-[24px] font-[900] text-center tracking-[0.5px]">
                        {title}
                    </Heading>
                </div>
                <div className="flex-1 flex gap-4 justify-end">
                    {hasFile && fileDetail && !loadingFileDetail && (
                        <>
                            <div className="flex flex-col items-center w-16">
                                <p className="text-[9px] font-bold mb-1 border-b border-black w-full text-center pb-0.5">صورة المتقاعد</p>
                                {fileDetail.file.personalPhotoPath ? (
                                    <img src={getUrl(fileDetail.file.personalPhotoPath)} alt="صورة المتقاعد" className="w-16 h-20 object-cover border-2 border-black rounded" loading="lazy" />
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
                                {activeRecipientPhotoPath ? (
                                    <img src={getUrl(activeRecipientPhotoPath)} alt="صورة الوكيل أو الوصي" className="w-16 h-20 object-cover border-2 border-black rounded" loading="lazy" />
                                ) : (
<<<<<<< HEAD
                                    <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 flex items-center justify-center text-[9px] text-muted-foreground">لا توجد</div>
=======
                                    <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 flex items-center justify-center text-[9px] text-muted-foreground/60">لا توجد</div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                )}
                            </div>
                        </>
                    )}
                </div>
            </div>

            {/* ═══ 1. البيانات الأساسية ═══ */}
            <section className={`${TB} w-full`}>
                <div className={SECTION_HEADER}>
                    <span>البيانات الأساسية للمستفيد</span>
                </div>
                <table className="w-full border-collapse">
                    <tbody>
                        {/* Row 1: Full name spanning entire width */}
                        <tr className={TB}>
                            <td className={`${LBL} w-[20%] ${TB}`}>{hasFile ? "الاسم الكامل" : "اسم القيد"}</td>
                            <td className={`${VAL} font-extrabold text-[14px]`} colSpan={5}>
                                {retiree.fullName || "—"}
                            </td>
                        </tr>
                        {/* Row 2: Military number + National ID */}
                        <tr className={TB}>
                            <td className={`${LBL} w-[20%] ${TB}`}>الرقم العسكري</td>
                            <td className={`${VAL} w-[30%] ${TB} font-bold`} colSpan={2}>
                                {retiree.militaryNumber || "—"}
                            </td>
                            <td className={`${LBL} w-[20%] ${TB}`}>الرقم الوطني</td>
                            <td className={`${VAL} w-[30%] font-bold`} colSpan={2}>
                                {retiree.nationalID || "غ/م"}
                            </td>
                        </tr>
                        {hasFile ? (
                            <>
                                {/* Row 3: Gender + Life status + Rank */}
                                <tr className={TB}>
                                    <td className={`${LBL} ${TB}`}>الجنس</td>
                                    <td className={`${VAL} ${TB}`}>
                                        {retiree.gender === 1 ? "ذكر" : retiree.gender === 2 ? "أنثى" : "—"}
                                    </td>
                                    <td className={`${LBL} ${TB}`}>حالة الحياة</td>
                                    <td className={`${VAL} ${TB}`}>
                                        {retiree.isAlive ? "على قيد الحياة" : retiree.deathDate ? `متوفى (${formatDate(retiree.deathDate)})` : "—"}
                                    </td>
                                    <td className={`${LBL} ${TB}`}>الفئة</td>
                                    <td className={VAL}>
                                        {retiree.categoryName || "—"}
                                    </td>
                                </tr>
                                {/* Row 4: Phone + Governorate + Social status */}
                                <tr className={TB}>
                                    <td className={`${LBL} ${TB}`}>الهاتف</td>
                                    <td className={`${VAL} ${TB}`}>{retiree.phone || "—"}</td>
                                    <td className={`${LBL} ${TB}`}>المحافظة</td>
                                    <td className={`${VAL} ${TB}`}>{retiree.governorate || "—"}</td>
                                    <td className={`${LBL} ${TB}`}>الحالة الاجتماعية</td>
                                    <td className={VAL}>{retiree.socialStatusName || "—"}</td>
                                </tr>
                                {/* Row 5: Health status + Unit */}
                                <tr className={TB}>
                                    <td className={`${LBL} ${TB}`}>الحالة الصحية</td>
                                    <td className={`${VAL} ${TB}`}>{retiree.healthStatusName || "—"}</td>
                                    <td className={`${LBL} ${TB}`}>الوحدة</td>
                                    <td className={`${VAL}`} colSpan={3}>{retiree.unitName || "—"}</td>
                                </tr>
                            </>
                        ) : (
                            /* No-file case: show rank + retirement date + unit + category */
                            <>
                                <tr className={TB}>
                                    <td className={`${LBL} ${TB}`}>الرتبة</td>
                                    <td className={`${VAL} ${TB} font-bold`}>
                                        {approvalItems[0]?.rankName || "—"}
                                    </td>
                                    <td className={`${LBL} ${TB}`}>تاريخ الإحالة</td>
                                    <td className={`${VAL} ${TB}`}>
                                        {approvalItems[0]?.retirementDate ? formatDate(approvalItems[0].retirementDate) : "—"}
                                    </td>
                                    <td className={`${LBL} ${TB}`}>الفئة</td>
                                    <td className={VAL}>
                                        {approvalItems[0]?.categoryName || "—"}
                                    </td>
                                </tr>
                                <tr className={TB}>
                                    <td className={`${LBL} ${TB}`}>الوحدة</td>
                                    <td className={`${VAL} ${TB}`} colSpan={3}>
                                        {approvalItems[0]?.unitName || "—"}
                                    </td>
                                    <td className={`${LBL} ${TB}`}>المعاش</td>
                                    <td className={`${VAL} font-extrabold`}>
                                        {approvalItems[0]?.salary?.toLocaleString("ar-YE") || "—"} ر.ي
                                    </td>
                                </tr>
                            </>
                        )}
                    </tbody>
                </table>
            </section>



            {/* ═══ 3. الاستحقاقات المالية ═══ */}
            {entitlements.length > 0 && (
                <section className={`${TB} w-full`}>
                    <div className={SECTION_HEADER}>
                        <span>الاستحقاقات المالية المعتمدة</span>
                    </div>
                    <div className="overflow-x-auto">
                    <table className="w-full border-collapse text-[11px] table-fixed">
                        <thead className="bg-muted/50">
                            <tr>
                                <th className={`${CELL} ${TB} text-center w-[5%]`}>م</th>
                                <th className={`${CELL} ${TB} text-center w-[25%]`}>الرتبة</th>
                                <th className={`${CELL} ${TB} text-center w-[30%]`}>المبلغ المستحق</th>
                                <th className={`${CELL} ${TB} text-center w-[20%]`}>من تاريخ</th>
                                <th className={`${CELL} ${TB} text-center w-[20%]`}>إلى تاريخ</th>
                            </tr>
                        </thead>
                        <tbody>
                            {entitlements.map((e, idx) => (
                                <tr key={e.entitlementID} className={TB}>
                                    <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{idx + 1}</td>
                                    <td className={`${CELL} ${TB} text-center font-bold`}>{e.rankName || "—"}</td>
                                    <td className={`${CELL} ${TB} text-center font-extrabold`}>{e.amount.toLocaleString("ar-YE")} ر.ي</td>
                                    <td className={`${CELL} ${TB} text-center`}>{formatDate(e.effectiveDate)}</td>
                                    <td className={`${CELL} ${TB} text-center`}>{e.endDate ? formatDate(e.endDate) : "مستمر"}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    </div>
                </section>
            )}

            {/* ═══ 4. تفاصيل الصرف + المستحقون ═══ */}
            {hasFile && fileDetail && !loadingFileDetail && (
                <>
                    <section className={`${TB} w-full`}>
                        <div className={SECTION_HEADER}>
                            <span>تفاصيل الصرف</span>
                        </div>
                        <table className="w-full border-collapse">
                            <tbody>
                                <tr className={TB}>
                                    <td className={`${LBL} w-[20%] ${TB}`}>رقم دفتر الصرف الحالي</td>
                                    <td className={`${VAL} w-[30%] ${TB} font-bold`}>
                                        {fileDetail.file.activePaymentBookNumber || "—"}
                                    </td>
                                    <td className={`${LBL} w-[20%] ${TB}`}>المستلم الفعلي</td>
                                    <td className={`${VAL} w-[30%] font-extrabold`}>
                                        {activeRecipientName}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </section>

                    {/* Heirs */}
                    {fileDetail.entitled.length > 0 && (
                        <section className={`${TB} w-full`}>
                            <div className={SECTION_HEADER}>
                                <span>المستحقون والورثة</span>
                                <span className="text-[11px] font-bold text-slate-500 me-auto">({fileDetail.entitled.length})</span>
                            </div>
                            <div className="overflow-x-auto">
                            <table className="w-full border-collapse text-[10px] table-fixed">
                                <thead className="bg-muted/50 text-[11px]">
                                    <tr>
                                        <th className={`${CELL} ${TB} text-center w-[5%]`}>م</th>
                                        <th className={`${CELL} ${TB} text-center w-[30%]`}>الاسم الكامل</th>
                                        <th className={`${CELL} ${TB} text-center w-[20%]`}>الصلة</th>
                                        <th className={`${CELL} ${TB} text-center w-[20%]`}>الحالة الاجتماعية</th>
                                        <th className={`${CELL} ${TB} text-center w-[25%]`}>الوصي (إن وجد)</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {fileDetail.entitled.map((e, idx) => (
                                        <tr key={e.entitledId} className={TB}>
                                            <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{idx + 1}</td>
                                            <td className={`${CELL} ${TB} text-center font-bold break-words`}>{e.fullName}</td>
                                            <td className={`${CELL} ${TB} text-center break-words`}>{e.relationshipName || "—"}</td>
                                            <td className={`${CELL} ${TB} text-center break-words`}>{e.socialStatusName || "—"}</td>
                                            <td className={`${CELL} ${TB} text-center text-slate-600 break-words`}>{getGuardianNameForEntitled(e.entitledId ?? 0)}</td>
                                        </tr>
                                    ))}
                            </tbody>
                        </table>
                        </div>
                    </section>
                )}
                </>
            )}

            {/* ═══ التسويات ═══ */}
            {settlements.length > 0 && (
                <section className={`${TB} w-full`}>
                    <div className={SECTION_HEADER}>
                        <span>التسويات</span>
                        <span className="text-[11px] font-bold text-slate-500 me-auto">({settlements.length})</span>
                    </div>
                    <div className="overflow-x-auto">
                    <table className="w-full border-collapse text-[11px] table-fixed">
                        <thead className="bg-muted/50">
                            <tr>
                                <th className={`${CELL} ${TB} text-center w-[5%]`}>م</th>
                                <th className={`${CELL} ${TB} text-center w-[12%]`}>رقم القرار</th>
                                <th className={`${CELL} ${TB} text-center w-[15%]`}>نوع التسوية</th>
                                <th className={`${CELL} ${TB} text-center w-[12%]`}>التاريخ</th>
                                <th className={`${CELL} ${TB} text-center w-[13%]`}>من رتبة</th>
                                <th className={`${CELL} ${TB} text-center w-[13%]`}>إلى رتبة</th>
                                <th className={`${CELL} ${TB} text-center w-[30%]`}>الملاحظات</th>
                            </tr>
                        </thead>
                        <tbody>
                            {settlements.map((s, idx) => (
                                <tr key={s.settlementItemID} className={TB}>
                                    <td className={`${CELL} ${TB} text-center font-mono text-slate-500`}>{idx + 1}</td>
                                    <td className={`${CELL} ${TB} text-center font-bold`}>{s.decisionNumber || "—"}</td>
                                    <td className={`${CELL} ${TB} text-center`}>{s.decisionType || "تسوية رتبة"}</td>
                                    <td className={`${CELL} ${TB} text-center`}>{s.decisionDate ? formatDate(s.decisionDate) : formatDate(s.createdAt)}</td>
                                    <td className={`${CELL} ${TB} text-center text-slate-600`}>{s.currentRankName || "—"}</td>
                                    <td className={`${CELL} ${TB} text-center font-bold`}>{s.processedRankName || "—"}</td>
                                    <td className={`${CELL} ${TB} text-[11px] max-w-[160px] sm:max-w-none`}>{s.notes || "—"}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    </div>
                </section>
            )}

            {/* Spacer: pushes signature to bottom */}
            <div className="flex-1" />

            {/* ═══ التوقيعات ═══ */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-8 text-center mt-8 px-2 print:page-break-inside-avoid">
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
