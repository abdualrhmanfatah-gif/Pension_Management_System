import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Play,Printer } from "lucide-react";
import { useNavigate,useParams } from "react-router-dom";

import api from "@/api/axios";
import { ministryApi } from "@/api/ministry";
<<<<<<< HEAD
import { UnifiedRetireeStatement } from "@/components/common/UnifiedRetireeStatement";
import type { Statement } from "@/components/domain/print/print.types";
import { fetchFileDetails } from "@/components/domain/print/print.utils";
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { useApiMutation, useRoleBasedUI } from "@/hooks";
import { useConfirm } from "@/hooks/useConfirm";
=======
import { Button } from "@/components/ui/Button";
import { useConfirm } from "@/hooks/useConfirm";
import { useApiMutation, useRoleBasedUI } from "@/hooks";
import { PrintLayout } from "@/components/ui/print/PrintLayout";

import { UnifiedRetireeStatement } from "@/components/common/UnifiedRetireeStatement";
import type { Statement } from "@/components/domain/print/print.types";
import { fetchFileDetails } from "@/components/domain/print/print.utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

/**
 * صفحة مباينة بيانات المتقاعد للطباعة
 * تجمع بيانات القيد من وزارة الداخلية + ملف التقاعد (إن وجد)
 */
export function RetireeStatementPrint() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { hasPermission } = useRoleBasedUI();
    const { confirm, ConfirmDialogElement } = useConfirm();

    const { data: statement, isLoading: loadingStatement, isError } = useQuery({
        queryKey: ["retirees", id, "statement"],
        queryFn: async () => {
            const res = await api.get<{ data: Statement }>(`/retirees/${id}/statement`);
            return res.data.data;
        }
    });

    const processMutation = useApiMutation(
        (ids: number[]) => ministryApi.processBulkApprovals(ids),
        {
            successMessage: "تمت معالجة القيود بنجاح",
            invalidateKeys: [["retirees", id, "statement"]]
        }
    );

    const fileId = statement?.retirementFiles?.[0]?.fileID;

    const { data: fileDetail, isLoading: loadingFileDetail } = useQuery({
        queryKey: ["retirement-files", fileId, "details-for-statement"],
        queryFn: () => fetchFileDetails(fileId!),
        enabled: !!fileId && !!statement && statement.retirementFiles.length > 0
    });

    if (loadingStatement) return <div className="flex justify-center py-20 text-muted-foreground italic text-sm">جاري التحميل...</div>;
    if (isError || !statement) return <div className="flex justify-center py-20 text-red-600 font-bold">حدث خطأ أثناء تحميل بيانات المباينة.</div>;

    const unprocessedIds = statement.approvalItems.filter(i => !i.isProcessed).map(i => i.approvalItemID);
    const canProcess = unprocessedIds.length > 0 && hasPermission("Ministry.Manage");

    const handleProcessItems = async () => {
        if (!unprocessedIds.length) return;
        const ok = await confirm({
            title: "معالجة قيود الإحالة",
            description: `هل أنت متأكد من معالجة عدد (${unprocessedIds.length}) من قيود الإحالة لهذا المتقاعد؟`,
            confirmLabel: "نعم، معالجة",
            variant: "default"
        });
        if (ok) processMutation.mutate(unprocessedIds);
    };

    return (
        <div className="min-h-screen pb-20 print:bg-white print:pb-0">
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => navigate(`/retirees/${id}`)}>
                    <ArrowRight className="w-5 h-5 ms-2" />
                    رجوع للمتقاعد
                </Button>
                <div className="flex gap-2">
                    {canProcess && (
                        <Button
                            onClick={handleProcessItems}
                            loading={processMutation.isPending}
                            variant="success"
                            className="flex items-center"
                        >
                            <Play className="w-4 h-4 ms-2" />
                            معالجة قيود الإحالة ({unprocessedIds.length})
                        </Button>
                    )}
                    <Button onClick={() => window.print()} className="flex items-center">
                        <Printer className="w-4 h-4 ms-2" />
                        طباعة المباينة
                    </Button>
                </div>
            </div>

            <div className="w-full flex justify-center">
                <PrintLayout
                    reportNumber={`RET-${id}-${new Date().getFullYear()}`}
                    attachments={`ملف التقاعد رقم ${id}`}
                    config={{ sectionTitle: "إدارة المعاشات" }}
                >
                    <UnifiedRetireeStatement 
                        statement={statement} 
                        fileDetail={fileDetail} 
                        loadingFileDetail={loadingFileDetail}
                    />
                </PrintLayout>
            </div>
            {ConfirmDialogElement}
        </div>
    );
}
