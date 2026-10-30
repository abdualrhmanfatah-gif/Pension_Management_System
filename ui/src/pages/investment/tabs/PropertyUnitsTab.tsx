import { useQuery } from "@tanstack/react-query";
import { Edit, FileText, Trash2 } from "lucide-react";
import { useCallback, useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { PropertyUnitDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
<<<<<<< HEAD
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { UnitDialog } from "@/components/domain/investment/UnitDialog";
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState, useConfirm } from "@/components/ui";
=======
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState, useConfirm } from "@/components/ui";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { UnitDialog } from "@/components/domain/investment/UnitDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation, useCrudState } from "@/hooks";

interface PropertyUnitsTabProps {
    propertyId: number;
}

const UNIT_STATUSES = [
    { label: "شاغرة", value: "Vacant" },
    { label: "مؤجرة", value: "Leased" },
    { label: "قيد الصيانة", value: "UnderMaintenance" },
    { label: "محجوزة", value: "Reserved" },
];

/**
 * تبويب وحدات العقار (شقق، مكاتب، محلات)
 */
export function PropertyUnitsTab({ propertyId }: PropertyUnitsTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<PropertyUnitDto>();

    // جلب البيانات
    const { data: unitsData, isLoading, error } = useQuery({
        queryKey: ["property-units", propertyId],
        queryFn: () => investmentApi.getUnits(propertyId).then(res => res || [])
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل وحدات العقار", { id: "property-units" });
    }, [error]);

    const units = useMemo(() => unitsData || [], [unitsData]);

    // طفرة الحذف فقط
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteUnit(id),
        {
            invalidateKeys: [["property-units", propertyId]],
            successMessage: "تم حذف الوحدة بنجاح",
        }
    );

    const handleDelete = useCallback(async (unit: PropertyUnitDto) => {
        const ok = await confirm({
            title: "حذف الوحدة",
            description: `هل أنت متأكد من رغبتك في حذف الوحدة "${unit.unitNumber}"؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(unit.unitID);
    }, [confirm, deleteMutation]);

    const columns = useMemo<CustomColumnDef<PropertyUnitDto>[]>(() => [
        {
            key: "unitNumber",
            header: "رقم الوحدة",
            render: (row) => <span className="font-medium text-primary">{row.unitNumber}</span>
        },
        { key: "unitTypeName", header: "النوع", accessor: (row) => row.unitTypeName },
        { key: "floor", header: "الطابق", accessor: (row) => row.floor || "0" },
        {
            key: "area",
            header: "المساحة",
            render: (row) => `${row.area.toLocaleString()} م²`
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => {
                const s = row.status;
                const variant = (s === 'Available' || s === 'Vacant') ? 'success' : s === 'Leased' ? 'info' : 'warning';
                const label = UNIT_STATUSES.find(us => us.value === s)?.label || s;
                return <Badge variant={variant}>{label}</Badge>;
            }
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق والمستندات">
<<<<<<< HEAD
                        <FileText className="w-4 h-4 text-info" />
=======
                        <FileText className="w-4 h-4 text-blue-500" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => crud.openEdit(row)}>
                        <Edit className="w-4 h-4 text-muted-foreground" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => handleDelete(row)}>
<<<<<<< HEAD
                        <Trash2 className="w-4 h-4 text-danger" />
=======
                        <Trash2 className="w-4 h-4 text-red-500" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Button>
                </div>
            )
        }
    ], [crud, handleDelete]);

    if (error) {
        return <ErrorState message="خطأ في تحميل وحدات العقار" />;
    }

    return (
        <CrudPageLayout
            title="وحدات العقار"
            description="إدارة الشقق والمكاتب والمحلات التجارية التابعة لهذا العقار"
            createLabel="إضافة وحدة"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={units}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي وحدات لهذا العقار حتى الآن."
        >
            <UnitDialog
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
                entityType="property-unit"
                entityId={crud.viewerTarget?.unitID || 0}
                title={`وثائق الوحدة: ${crud.viewerTarget?.unitNumber} - ${crud.viewerTarget?.unitTypeName}`}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
