import { useQuery } from "@tanstack/react-query";
import { Edit, Eye, MapPin, Trash2 } from "lucide-react";
import { useCallback, useEffect, useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi, type PropertyDto } from "@/api/investment";
<<<<<<< HEAD
import { PropertyDialog } from "@/components/domain/investment/PropertyDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Button,
    CrudPageLayout,
    type CustomColumnDef,
    useConfirm} from "@/components/ui";
<<<<<<< HEAD
=======
import { PropertyDialog } from "@/components/domain/investment/PropertyDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation, useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

/**
 * صفحة العقارات والأصول
 * تعرض قائمة بكافة العقارات والمجمعات الاستثمارية
 */
export function PropertiesPage() {
    const navigate = useNavigate();
    const canAdd = useHasPermission("Investment.Add");
    const canEdit = useHasPermission("Investment.Edit");
    const canDelete = useHasPermission("Investment.Delete");
    const { confirm, ConfirmDialogElement } = useConfirm();

    const crud = useCrudState<PropertyDto>();

    // جلب العقارات
    const { data: propertiesRes, isLoading, error } = useQuery({
        queryKey: ["properties"],
        queryFn: () => investmentApi.getProperties()
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل العقارات", { id: "properties" });
    }, [error]);

    const properties = propertiesRes || [];

    // طفرة الحذف فقط (الإضافة والتعديل الآن داخل الحوار)
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteProperty(id),
        {
            invalidateKeys: [["properties"]],
            successMessage: "تم حذف العقار بنجاح",
        }
    );

    const handleDelete = useCallback(async (property: PropertyDto) => {
        const ok = await confirm({
            title: "حذف العقار",
            description: `هل أنت متأكد من رغبتك في حذف العقار "${property.propertyName}"؟ سيتم حذف جميع البيانات المرتبطة به نهائياً.`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(property.propertyID);
    }, [confirm, deleteMutation]);

    // تعريف الأعمدة
    const columns = useMemo<CustomColumnDef<PropertyDto>[]>(() => [
        {
            key: "propertyID",
            header: "رقم العقار",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.propertyID}</span>
        },
        {
            key: "propertyName",
            header: "اسم العقار",
            render: (row) => <span className="font-bold">{row.propertyName}</span>
        },
        {
            key: "projectName",
            header: "المشروع",
            accessor: (row) => row.projectName || "—"
        },
        { key: "propertyTypeName", header: "النوع", accessor: (row) => row.propertyTypeName },
        { key: "locationCity", header: "المدينة", accessor: (row) => row.locationCity || "—" },
        {
            key: "totalArea",
            header: "المساحة",
            render: (row) => `${row.totalArea.toLocaleString()} م²`
        },
        {
            key: "currentMarketValue",
            header: "القيمة السوقية",
            render: (row) => (
<<<<<<< HEAD
                <span className="font-semibold text-success">
=======
                <span className="font-semibold text-emerald-600">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    {row.currentMarketValue.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "googleMapsUrl",
            header: "الموقع",
            render: (row) => row.googleMapsUrl ? (
                <a
                    href={row.googleMapsUrl}
                    target="_blank"
                    rel="noreferrer"
<<<<<<< HEAD
                    className="inline-flex items-center text-info hover:underline gap-1"
=======
                    className="inline-flex items-center text-blue-600 hover:underline gap-1"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    onClick={(e) => e.stopPropagation()}
                >
                    <MapPin className="w-4 h-4" /> خريطة
                </a>
            ) : "—"
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <Link
                        to={`/investment/properties/${row.propertyID}`}
                        className="p-2 hover:bg-muted rounded-md text-primary transition-colors"
                        title="التفاصيل"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <Eye className="w-4 h-4" />
                    </Link>
                    {canEdit && (
<<<<<<< HEAD
                        <Button variant="ghost" size="sm" className="text-info" title="تعديل" onClick={(e) => { e.stopPropagation(); crud.openEdit(row); }}>
=======
                        <Button variant="ghost" size="sm" className="text-blue-600" title="تعديل" onClick={(e) => { e.stopPropagation(); crud.openEdit(row); }}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <Edit className="w-4 h-4" />
                        </Button>
                    )}
                    {canDelete && (
<<<<<<< HEAD
                        <Button variant="ghost" size="sm" className="text-danger" title="حذف" onClick={(e) => { e.stopPropagation(); handleDelete(row); }}>
=======
                        <Button variant="ghost" size="sm" className="text-red-600" title="حذف" onClick={(e) => { e.stopPropagation(); handleDelete(row); }}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ], [canEdit, canDelete, crud, handleDelete]);

    return (
        <CrudPageLayout
            title="العقارات والأصول"
            description="إدارة وتتبع العقارات والمجمعات الاستثمارية التابعة للصندوق"
            createLabel="إضافة عقار"
            onCreateClick={canAdd ? crud.openCreate : undefined}
            canCreate={canAdd}
            columns={columns}
            data={properties}
            loading={isLoading}
            onRowClick={(row) => navigate(`/investment/properties/${row.propertyID}`)}
        >
            <PropertyDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                initialData={crud.selectedItem || undefined}
                mode={crud.isEditing ? "edit" : "create"}
                onSuccess={crud.closeForm}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
