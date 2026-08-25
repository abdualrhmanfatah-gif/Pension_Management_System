import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowDownRight, ArrowUpRight, FileText } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { TransactionDto } from "@/api/investment";
import { Badge, Button, CrudPageLayout, ErrorState } from "@/components/ui";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { TransactionDialog } from "@/components/domain/investment/TransactionDialog";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

interface TransactionsTabProps {
    projectId?: number;
}

/**
 * تبويب سجل المعاملات المالية للمشروع أو النظام
 */
export function TransactionsTab({ projectId }: TransactionsTabProps) {
    const canManage = useHasPermission("Investment.Manage");
    const crud = useCrudState<TransactionDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: transactions, isLoading, error } = useQuery({
        queryKey: projectId ? ["project-transactions", projectId] : ["transactions"],
        queryFn: () => Promise.resolve([] as TransactionDto[]) // investmentApi.getTransactions(projectId)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل المعاملات المالية", { id: "transactions" });
    }, [error]);

    const columns = useMemo(() => [
        {
            key: "direction",
            header: "النوع",
            render: (row: TransactionDto) => (
                <div className={`flex items-center justify-center w-8 h-8 rounded-full ${row.isCredit ? 'bg-emerald-50 text-emerald-600' : 'bg-red-50 text-red-600'}`}>
                    {row.isCredit ?
                        <ArrowUpRight className="w-4 h-4" /> :
                        <ArrowDownRight className="w-4 h-4" />
                    }
                </div>
            )
        },
        { key: "txnTypeName", header: "التصنيف", accessor: (row: TransactionDto) => row.txnTypeName },
        {
            key: "projectName",
            header: "المشروع/المصدر",
            render: (row: TransactionDto) => row.projectName ? (
                <span className="font-medium text-foreground">{row.projectName}</span>
            ) : (
                <span className="text-muted-foreground text-sm">{row.sourceType || "عام"}</span>
            )
        },
        {
            key: "txnDate",
            header: "التاريخ",
            render: (row: TransactionDto) => (
                <span className="text-sm font-medium">{new Date(row.txnDate).toLocaleDateString("ar-YE")}</span>
            )
        },
        {
            key: "amount",
            header: "المبلغ",
            render: (row: TransactionDto) => (
                <span className={`font-bold font-mono ${row.isCredit ? "text-emerald-600" : "text-red-600"}`}>
                    {row.isCredit ? "+" : "-"}{row.amount.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "referenceNumber",
            header: "الرقم المرجعي",
            render: (row: TransactionDto) => row.referenceNumber ? (
                <Badge variant="outline" className="font-mono text-xs font-normal">{row.referenceNumber}</Badge>
            ) : (
                <span className="text-muted-foreground text-xs">بدون رقم</span>
            )
        },
        {
            key: "actions",
            header: "الوثائق",
            render: (row: TransactionDto) => (
                <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق">
                    <FileText className="w-4 h-4 text-primary" />
                </Button>
            )
        }
    ], [crud]);

    if (error) {
        return <ErrorState message="خطأ في تحميل المعاملات" />;
    }

    return (
        <CrudPageLayout
            title="سجل المعاملات المالية"
            description="توثيق كافة التحصيلات والمصروفات المرتبطة بالاستثمارات"
            createLabel="إضافة معاملة"
            onCreateClick={canManage ? crud.openCreate : undefined}
            canCreate={canManage}
            columns={columns}
            data={transactions || []}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي معاملات مالية حتى الآن."
        >
            <TransactionDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["transactions"] }); }}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="investment-txn"
                entityId={crud.viewerTarget?.txnID || 0}
                title={`وثائق المعاملة المالية ${crud.viewerTarget?.referenceNumber ? `(${crud.viewerTarget.referenceNumber})` : ''}`}
            />
        </CrudPageLayout>
    );
}
