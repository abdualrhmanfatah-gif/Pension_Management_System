import type { EmployeeDetailsDto } from "@/api/hr";
import { PrintSignatureBlock } from "@/components/common/PrintSignatureBlock";
import { PRINT_STYLES } from "@/components/domain/print/print.utils";
import { getDocumentUrl } from "@/lib/utils";
import { formatDate } from "@/lib/utils";

const { TB, CELL, LBL, VAL, SECTION_HEADER } = PRINT_STYLES;

interface PrintableEmployeeReportProps {
    employee: EmployeeDetailsDto;
}

const getUrl = (p: string | null | undefined) => {
    if (!p) return "";
    return getDocumentUrl(p) ?? "";
};

/**
 * تقرير تفصيلي لبيانات الموظف - الموارد البشرية
 * تم توحيده بالكامل مع معايير تقرير ملف التقاعد
 */
export function PrintableEmployeeReport({ employee }: PrintableEmployeeReportProps) {
    const signatures = [
        { title: "حرره المختص", sub: "الاسم والتوقيع" },
        { title: "ختم الإدارة العامة", sub: "الختم الرسمي", className: "opacity-100" },
        { title: "مدير إدارة الموارد البشرية", sub: "المصادقة والختم" },
    ];

    return (
        <div className="text-black w-full flex flex-col min-h-full" dir="rtl">
            {/* ═══ Photos + Title Row ═══ */}
            <div className="flex items-start mb-2 mt-2">
                <div className="flex-1" />
                <div className="flex-none flex justify-center pt-2">
                    <h1 className="text-[24px] font-[900] text-center tracking-[0.5px]">
                        استمارة بيانات موظف
                    </h1>
                </div>
                <div className="flex-1 flex gap-4 justify-end">
                    <div className="flex flex-col items-center w-16">
                        <p className="text-[9px] font-bold mb-1 border-b border-black w-full text-center pb-0.5">صورة الموظف</p>
                        {employee.profilePhotoPath ? (
                            <img 
                                src={getUrl(employee.profilePhotoPath)} 
                                alt="صورة الموظف" 
                                className="w-16 h-20 object-cover border-2 border-black rounded" 
                                onError={(e) => { (e.target as HTMLImageElement).src = "/placeholder-user.png"; }}
                            />
                        ) : (
                            <div className="w-16 h-20 border-2 border-black rounded bg-muted/50 text-muted-foreground">لا توجد</div>
                        )}
                    </div>
                </div>
            </div>

            <div className="space-y-6">
                {/* 1. البيانات الأساسية */}
                <section className={`${TB} w-full break-inside-avoid`}>
                    <div className={SECTION_HEADER}>
                        <span>أولاً: البيانات الأساسية والشخصية</span>
                    </div>
                    <table className="w-full border-collapse">
                        <tbody>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الاسم الكامل</td>
                                <td className={`${VAL} font-extrabold text-[14px]`} colSpan={3}>
                                    {employee.fullName}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الرقم الوطني</td>
                                <td className={`${VAL} w-[30%] ${TB} font-bold`}>
                                    {employee.nationalID || "—"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>الجنس</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {employee.gender === 1 ? "ذكر" : "أنثى"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>تاريخ الميلاد</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {employee.birthDate ? formatDate(employee.birthDate) : "—"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>مكان الميلاد</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {employee.birthPlace || "—"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>فصيلة الدم</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {employee.bloodGroup || "—"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>الحالة الاجتماعية</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {employee.maritalStatus || "—"}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </section>

                {/* 2. البيانات الوظيفية */}
                <section className={`${TB} w-full break-inside-avoid`}>
                    <div className={SECTION_HEADER}>
                        <span>ثانياً: البيانات الوظيفية والإدارية</span>
                    </div>
                    <table className="w-full border-collapse">
                        <tbody>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الرقم الوظيفي</td>
                                <td className={`${VAL} w-[30%] ${TB} font-mono font-bold text-[13px]`}>
                                    {employee.employeeNumber}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>حالة الخدمة</td>
                                <td className={`${VAL} w-[30%] font-bold`}>
                                    {employee.employmentStatusName}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>المسمى الوظيفي</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {employee.jobTitleName}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>تاريخ التعيين</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {formatDate(employee.hireDate)}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الإدارة / الجهة</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {employee.departmentName}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>القسم / الوحدة</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {employee.sectionName || "—"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>المؤهل الدراسي</td>
                                <td className={`${VAL}`} colSpan={3}>
                                    {employee.academicQualifications || "—"}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </section>

                {/* 3. البيانات العسكرية والطوارئ */}
                <section className={`${TB} w-full break-inside-avoid`}>
                    <div className={SECTION_HEADER}>
                        <span>ثالثاً: البيانات العسكرية وجهات الاتصال</span>
                    </div>
                    <table className="w-full border-collapse">
                        <tbody>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>الرقم العسكري</td>
                                <td className={`${VAL} w-[30%] ${TB} font-mono font-bold`}>
                                    {employee.militaryNumber || "—"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>الرتبة الحالية</td>
                                <td className={`${VAL} w-[30%] font-bold`}>
                                    {employee.rankName || "—"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>تاريخ الخدمة</td>
                                <td className={`${VAL} w-[30%] ${TB}`}>
                                    {employee.militaryServiceDate ? formatDate(employee.militaryServiceDate) : "—"}
                                </td>
                                <td className={`${LBL} w-[20%] ${TB}`}>هاتف الموظف</td>
                                <td className={`${VAL} w-[30%]`}>
                                    {employee.phone || "—"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>اتصال الطوارئ</td>
                                <td className={`${VAL}`} colSpan={3}>
                                    {employee.emergencyContactName ? `${employee.emergencyContactName} (${employee.emergencyContactPhone})` : "—"}
                                </td>
                            </tr>
                            <tr className={TB}>
                                <td className={`${LBL} w-[20%] ${TB}`}>العنوان السكني</td>
                                <td className={`${VAL}`} colSpan={3}>
                                    {employee.address || "—"}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </section>

                {/* 4. الملاحظات */}
                {employee.notes && (
                    <section className={`${TB} w-full break-inside-avoid`}>
                        <div className={SECTION_HEADER}>
                            <span>رابعاً: ملاحظات إضافية</span>
                        </div>
                        <div className={`${CELL} min-h-[60px] leading-relaxed text-[11px] text-justify`}>
                            {employee.notes}
                        </div>
                    </section>
                )}
            </div>

            {/* Spacer: pushes signature to bottom */}
            <div className="flex-1" />

            {/* ═══ Signatures Block ═══ */}
            <PrintSignatureBlock signatures={signatures} variant="default" className="mt-12 pt-4 border-t border-dashed border-black" />
        </div>
    );
}

PrintableEmployeeReport.displayName = "PrintableEmployeeReport";
