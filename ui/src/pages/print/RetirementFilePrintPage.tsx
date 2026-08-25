import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer } from "lucide-react";
import { useNavigate,useParams } from "react-router-dom";

import { financialEntitlementsApi } from "@/api/financial-entitlements";
import { retireeApi } from "@/api/retirees";
import { retirementApi } from "@/api/retirement";
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";

import type { NormalizableItem, SalaryRecipientForPrint } from "@/components/domain/print/print.types";
import { normalizePrintItem } from "@/components/domain/print/print.utils";
import { PrintableRetirementFileReport } from "./PrintableRetirementFileReport";

export function RetirementFilePrintPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const fileId = parseInt(id || "");

    const { data: fileData, isLoading: loadingFile, isError: errorFile } = useQuery({
        queryKey: ["retirement-files", fileId],
        queryFn: () => retirementApi.getFile(fileId),
        enabled: !!fileId
    });

    const retireeId = fileData?.retireeID;

    const { data: retiree, isLoading: loadingRetiree, isError: errorRetiree } = useQuery({
        queryKey: ["retirees", retireeId],
        queryFn: async () => {
            if (!retireeId) return null;
            return retireeApi.getById(retireeId);
        },
        enabled: !!retireeId
    });

    const { data: entitled = [], isLoading: loadingEntitled, isError: errorEntitled } = useQuery({
        queryKey: ["entitled-persons", fileId],
        queryFn: () => retirementApi.getEntitledPersons(fileId),
        enabled: !!fileId
    });

    const { data: agents = [], isLoading: loadingAgents, isError: errorAgents } = useQuery({
        queryKey: ["agents", fileId],
        queryFn: () => retirementApi.getAgents(fileId),
        enabled: !!fileId
    });

    const { data: guardians = [], isLoading: loadingGuardians, isError: errorGuardians } = useQuery({
        queryKey: ["guardians", fileId],
        queryFn: () => retirementApi.getGuardians(fileId),
        enabled: !!fileId
    });

    const { data: financial, isLoading: loadingFinancial, isError: errorFinancial } = useQuery({
        queryKey: ["active-entitlement", retireeId],
        queryFn: async () => {
            if (!retireeId) return null;
            const res = await financialEntitlementsApi.getActive(retireeId);
            return res.data;
        },
        enabled: !!retireeId
    });

    const { data: recipientsRaw = [], isLoading: loadingRecipients, isError: errorRecipients } = useQuery({
        queryKey: ["salary-recipients", fileId],
        queryFn: () => retirementApi.getRecipients(fileId),
        enabled: !!fileId
    });

    const { data: paymentBooks = [], isLoading: loadingBooks } = useQuery({
        queryKey: ["payment-books", fileId],
        queryFn: () => retirementApi.getPaymentBooks(fileId),
        enabled: !!fileId
    });

    const recipients = (recipientsRaw as unknown as NormalizableItem[]).map(r => normalizePrintItem(r))
        .filter(Boolean) as SalaryRecipientForPrint[];

    const loading = loadingFile || loadingRetiree || loadingEntitled || loadingAgents || loadingGuardians || loadingFinancial || loadingRecipients || loadingBooks;
    const hasError = errorFile || errorRetiree || errorEntitled || errorAgents || errorGuardians || errorFinancial || errorRecipients;

    if (loading) return <div className="flex justify-center py-20 text-muted-foreground italic text-sm">جاري تحميل بيانات التقرير...</div>;
    if (hasError) return <div className="p-12 text-center text-red-600 font-bold">حدث خطأ أثناء تحميل بيانات التقرير. يرجى المحاولة لاحقاً.</div>;
    if (!fileData || !retiree) return <div className="p-12 text-center font-bold">الملف غير موجود.</div>;

    return (
        <div className="bg-surface-print min-h-screen pb-20 print:bg-white print:pb-0">
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => navigate(`/files/${fileId}`)}>
                    <ArrowRight className="w-5 h-5 ms-2" />
                    رجوع للملف
                </Button>
                <div className="flex gap-2">
                    <Button onClick={() => window.print()} className="flex items-center">
                        <Printer className="w-4 h-4 ms-2" />
                        طباعة التقرير
                    </Button>
                </div>
            </div>

            <div className="w-full flex justify-center">
                <PrintLayout
                    reportNumber={`FILE-${fileId}-${new Date().getFullYear()}`}
                    attachments={`ملف التقاعد رقم ${fileId}`}
                    config={{ sectionTitle: "إدارة المعاشات" }}
                >
                    <PrintableRetirementFileReport 
                        file={{
                            ...fileData,
                            fileOpenDate: fileData.fileOpenDate ?? "",
                            statusName: fileData.fileStatusName ?? null,
                            activePaymentBookNumber: fileData.activePaymentBookNumber || paymentBooks.find((b: any) => b.status === "فعال")?.bookNumber,
                        }}
                        retiree={retiree}
                        entitled={entitled.map((e: any) => ({ ...e, nationalID: e.nationalID ?? undefined }))}
                        agents={agents.map((a: any) => ({ ...a, nationalID: a.nationalID ?? undefined }))}
                        guardians={guardians.map((g: any) => ({ ...g, nationalID: g.nationalID ?? undefined }))}
                        financial={financial}
                        recipients={recipients}
                        paymentBooks={paymentBooks}
                    />
                </PrintLayout>
            </div>
        </div>
    );
}

