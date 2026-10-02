import { useQuery } from "@tanstack/react-query";
import { FileText, Trash2, TrendingDown } from "lucide-react";
import { useCallback, useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { DepreciationRecordDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { DepreciationDialog } from "@/components/domain/investment/DepreciationDialog";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { Button, CrudPageLayout, type CustomColumnDef, ErrorState, useConfirm } from "@/components/ui";
import { useApiMutation, useCrudState } from "@/hooks";

interface PropertyDepreciationTabProps {
    propertyId: number;
}

/**
 * تبويب سجل استهلاك العقار
 */
export function PropertyDepreciationTab({ propertyId }: PropertyDepreciationTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<DepreciationRecordDto>();

    // جلب بيانات العقار (لأجل القيم الحالية)
    useQuery({
        queryKey: ["property", propertyId],
        queryFn: () => investmentApi.getProperty(propertyId)
    });

    // جلب سجلات الاستهلاك
    const { data: recordsData, isLoading, error } = useQuery({
        queryKey: ["property-depreciation", propertyId],
        queryFn: () => investmentApi.getDepreciation(propertyId).then(res => res || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل سجل الاستهلاك", { id: "property-depreciation" });
    }, [error]);

    const records = recordsData || [];

    // طفرة الحذف فقط
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteDepreciation(id),
        {
            invalidateKeys: [["property-depreciation", propertyId], ["property", propertyId]],
            successMessage: "تم حذف سجل الاستهلاك بنجاح",
        }
    );

    const handleDelete = useCallback(async (record: DepreciationRecordDto) => {
        const ok = await confirm({
            title: "حذف سجل الاستهلاك",
            description: "هل أنت متأكد من رغبتك في حذف هذا السجل؟ سيؤدي ذلك إلى إعادة حساب القيمة الدفترية للعقار.",
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(record.deprecID);
    }, [confirm, deleteMutation]);

    const columns = useMemo<CustomColumnDef<DepreciationRecordDto>[]>(() => [
        {
            key: "yearName",
            header: "السنة المالية",
            render: (row) => <span className="font-medium">{row.yearName}</span>
        },
        {
            key: "depreciationRate",
            header: "نسبة الاستهلاك",
            render: (row) => (
                <span className="font-semibold text-amber-600 font-mono">{row.depreciationRate}%</span>
            )
        },
        {
            key: "depreciationAmount",
            header: "مبلغ الاستهلاك",
            render: (row) => (
                <span className="text-red-500 font-medium flex items-center gap-1 font-mono">
                    <TrendingDown className="w-3 h-3" />
                    {row.depreciationAmount.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "netBookValue",
            header: "صافي القيمة الدفترية",
            render: (row) => (
                <span className="text-emerald-600 font-bold font-mono">
                    {row.netBookValue.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => crud.openViewer(row)}
                        className="text-amber-600 hover:bg-amber-50"
                        title="المرفقات والوثائق"
                    >
                        <FileText className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDelete(row)}
                        className="text-red-500 hover:text-red-700 hover:bg-red-50 transition-colors"
                        title="حذف"
                    >
                        <Trash2 className="w-4 h-4" />
                    </Button>
                </div>
            )
        }
    ], [crud, handleDelete]);

    if (error) {
        return <ErrorState message="خطأ في تحميل الإهلاك" />;
    }

    return (
        <CrudPageLayout
            title="سجل استهلاك العقار"
            description="تتبع تناقص القيمة الدفترية للأصل العقاري عبر السنوات المالية"
            createLabel="إضافة سجل"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={records}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي عمليات استهلاك لهذه الخاصية بعد."
        >
            <DepreciationDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                initialData={crud.selectedItem || undefined}
                mode={crud.isEditing ? "edit" : "create"}
                onSuccess={crud.closeForm}
                propertyId={propertyId}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="depreciation-record"
                entityId={crud.viewerTarget?.deprecID || 0}
                title={`مرفقات سجل الاستهلاك: ${crud.viewerTarget?.yearName}`}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
