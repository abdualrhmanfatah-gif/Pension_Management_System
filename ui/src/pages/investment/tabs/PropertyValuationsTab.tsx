import { useQuery } from "@tanstack/react-query";
import { Download, FileText, Trash2, TrendingUp } from "lucide-react";
import { useCallback, useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { ValuationDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { Button, CrudPageLayout, type CustomColumnDef, ErrorState, useConfirm } from "@/components/ui";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { ValuationDialog } from "@/components/domain/investment/ValuationDialog";
import { useApiMutation, useCrudState } from "@/hooks";

interface PropertyValuationsTabProps {
    propertyId: number;
}

/**
 * تبويب سجل التقييمات العقارية
 */
export function PropertyValuationsTab({ propertyId }: PropertyValuationsTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<ValuationDto>();

    // جلب البيانات
    const { data: valuationsData, isLoading, error } = useQuery({
        queryKey: ["property-valuations", propertyId],
        queryFn: () => investmentApi.getValuations(propertyId).then(res => res || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل التقييمات", { id: "property-valuations" });
    }, [error]);

    const valuations = valuationsData || [];

    // طفرة الحذف فقط
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteValuation(id),
        {
            invalidateKeys: [
                ["property-valuations", propertyId],
                ["property", propertyId]
            ],
            successMessage: "تم حذف التقييم بنجاح",
        }
    );

    const handleDelete = useCallback(async (valuation: ValuationDto) => {
        const ok = await confirm({
            title: "حذف التقييم",
            description: "هل أنت متأكد من رغبتك في حذف هذا التقييم؟ لا يمكن التراجع عن هذه العملية.",
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(valuation.valuationID);
    }, [confirm, deleteMutation]);

    const columns = useMemo<CustomColumnDef<ValuationDto>[]>(() => [
        {
            key: "valuationDate",
            header: "تاريخ التقييم",
            render: (row) => (
                <span className="font-medium font-mono">{new Date(row.valuationDate).toLocaleDateString("ar-YE")}</span>
            )
        },
        {
            key: "marketValue",
            header: "القيمة السوقية",
            render: (row) => (
                <span className="font-bold text-emerald-600 flex items-center gap-1 font-mono">
                    <TrendingUp className="w-3 h-3" />
                    {row.marketValue.toLocaleString()} ر.ي
                </span>
            )
        },
        { key: "valuedBy", header: "المُقيّم", accessor: (row) => row.valuedBy },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق والمستندات">
                        <FileText className="w-4 h-4 text-blue-500" />
                    </Button>
                    {row.valuationReportPath && (
                        <a
                            href={`${import.meta.env.VITE_API_BASE_URL}/${row.valuationReportPath}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="p-2 hover:bg-muted rounded-md text-primary transition-colors"
                            title="تحميل التقرير"
                        >
                            <Download className="w-4 h-4" />
                        </a>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => handleDelete(row)} title="حذف">
                        <Trash2 className="w-4 h-4 text-red-500" />
                    </Button>
                </div>
            )
        }
    ], [crud, handleDelete]);

    if (error) {
        return <ErrorState message="خطأ في تحميل تقييمات العقار" />;
    }

    return (
        <CrudPageLayout
            title="سجل التقييمات العقارية"
            description="متابعة تطور القيمة السوقية للأصل العقاري بناءً على تقييمات فنية دورية"
            createLabel="إضافة تقييم"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={valuations}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي تقييمات لهذا العقار حتى الآن."
        >
            <ValuationDialog
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
                entityType="property-valuation"
                entityId={crud.viewerTarget?.valuationID || 0}
                title={`وثائق التقييم: ${crud.viewerTarget && new Date(crud.viewerTarget.valuationDate).toLocaleDateString("ar-YE")}`}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
