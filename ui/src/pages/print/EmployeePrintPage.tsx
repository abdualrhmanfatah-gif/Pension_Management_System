import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer } from "lucide-react";
import { useNavigate,useParams } from "react-router-dom";

import { hrApi } from "@/api/hr";
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";

import { PrintableEmployeeReport } from "./PrintableEmployeeReport";

/**
 * صفحة طباعة استمارة بيانات موظف
 * هيكلية مطابقة تماماً لـ RetirementFilePrintPage
 */
export function EmployeePrintPage() {
    const { employeeId } = useParams<{ employeeId: string }>();
    const navigate = useNavigate();
    const id = parseInt(employeeId || "");

    const { data: employee, isLoading, isError } = useQuery({
        queryKey: ["hr-employee", id],
        queryFn: () => hrApi.getEmployeeById(id),
        enabled: !!id,
    });

    if (isLoading) return <div className="flex justify-center py-20 text-muted-foreground italic text-sm">جاري تحميل بيانات التقرير...</div>;
    
    if (isError) return <div className="p-12 text-center text-red-600 font-bold">حدث خطأ أثناء تحميل بيانات التقرير. يرجى المحاولة لاحقاً.</div>;

    if (!employee) return <div className="p-12 text-center font-bold">الموظف غير موجود.</div>;

    return (
        <div className="bg-slate-200/60 min-h-screen pb-20 print:bg-white print:pb-0">
            {/* Screen-only controls */}
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => navigate(`/hr/employees/${id}`)}>
                    <ArrowRight className="w-5 h-5 ms-2" />
                    رجوع للملف
                </Button>
                <div className="flex gap-2">
                    <Button onClick={() => window.print()} className="flex items-center">
                        <Printer className="w-4 h-4 ms-2" />
                        طباعة الاستمارة
                    </Button>
                </div>
            </div>

            <div className="w-full flex justify-center">
                <PrintLayout
                    reportNumber={`EMP-${id}-${new Date().getFullYear()}`}
                    attachments={`ملف الموظف رقم ${id}`}
                    config={{ sectionTitle: "إدارة الشؤون الإدارية" }}
                >
                    <PrintableEmployeeReport employee={employee} />
                </PrintLayout>
            </div>
        </div>
    );
}
