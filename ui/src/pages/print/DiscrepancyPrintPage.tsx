import { useQuery } from "@tanstack/react-query";
import { Printer } from "lucide-react";
import { useMemo } from "react";
import { useNavigate,useSearchParams } from "react-router-dom";

import { discrepancyApi } from "@/api/discrepancy";
import { PrintSignatureBlock } from "@/components/common/PrintSignatureBlock";
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { formatDate } from "@/lib/utils";

export function DiscrepancyPrintPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const filter = useMemo(() => ({
    pageNumber: 1,
    pageSize: 1000, // Load all for printing
    searchTerm: searchParams.get("searchTerm") || "",
    stage: searchParams.get("stage") || ""
  }), [searchParams]);

  const { data, isLoading, isError } = useQuery({
    queryKey: ["discrepancy-report-print", filter],
    queryFn: () => discrepancyApi.getReport(filter),
  });

  const signatures = [
    { title: "موظف التدقيق", sub: "................................" },
    { title: "رئيس قسم الرقابة", sub: "................................" },
    { title: "مدير إدارة التقاعد", sub: "................................" },
  ];

  if (isLoading) {
    return <div className="flex items-center justify-center min-h-screen italic text-muted-foreground text-sm">جاري تجهيز التقرير للطباعة...</div>;
  }

  if (isError || !data) {
    return <div className="flex items-center justify-center min-h-screen text-red-600 font-bold">حدث خطأ أثناء تحميل التقرير.</div>;
  }

  return (
    <>
      <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          رجوع
        </Button>
        <Button onClick={() => window.print()} className="flex items-center">
          <Printer className="w-4 h-4 ms-2" />
          طباعة التقرير
        </Button>
      </div>

      <PrintLayout reportName="مباينة بيانات المتقاعدين">
        <div className="space-y-6 text-black">
          {/* ملخص الفلاتر المطبقة */}
          <div className="flex flex-wrap gap-4 text-xs bg-slate-50 p-3 rounded-md border border-slate-200 print:hidden">
            <div className="font-bold">معايير التقرير:</div>
            {filter.searchTerm && <div>• البحث: {filter.searchTerm}</div>}
            {filter.stage && <div>• المرحلة: {
              filter.stage === "Stage 1" ? "كشف إحالة فقط" :
                filter.stage === "Stage 2" ? "تمت المعالجة" : "ملف مكتمل"
            }</div>}
            <div className="me-auto">• إجمالي السجلات: {data?.totalCount || 0}</div>
          </div>

          <section>
            <table className="w-full border-collapse border text-[11px]">
              <thead>
                <tr className="bg-slate-100">
                  <th className="border p-2 text-center w-8">#</th>
                  <th className="border p-2 text-center w-24">الرقم العسكري</th>
                  <th className="border p-2 text-start">الاسم الرباعي</th>
                  <th className="border p-2 text-start w-32">الرتبة</th>
                  <th className="border p-2 text-center w-28">تاريخ التقاعد</th>
                  <th className="border p-2 text-center w-28">المرحلة الحالية</th>
                </tr>
              </thead>
              <tbody>
                {data?.items.map((row, index) => (
                  <tr key={row.approvalItemID || index} className="hover:bg-slate-50">
                    <td className="border p-2 text-center">{index + 1}</td>
                    <td className="border p-2 text-center font-mono font-bold">{row.militaryNumber}</td>
                    <td className="border p-2 font-semibold">
                      {row.retireeFullName || row.approvalFullName || "---"}
                    </td>
                    <td className="border p-2">
                      {row.fileRankName || row.approvalRankName || "---"}
                    </td>
                    <td className="border p-2 text-center font-mono">
                      {formatDate(row.fileRetirementDate || row.approvalRetirementDate)}
                    </td>
                    <td className="border p-2 text-center">
                      {row.currentStage === "Stage 3: File Created" ? "ملف مكتمل" :
                        row.currentStage === "Stage 2: Processed" ? "تمت المعالجة" : "كشف إحالة فقط"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>

          {/* تذييل التوقيعات */}
          <PrintSignatureBlock signatures={signatures} className="mt-12 pt-8 border-t border-slate-100" />
        </div>
      </PrintLayout>
    </>
  );
}
