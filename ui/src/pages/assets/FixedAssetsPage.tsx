import { useQuery } from "@tanstack/react-query";
import { Edit, Monitor, PlayCircle, Trash2 } from "lucide-react";
<<<<<<< HEAD
// Icon mapping from string name to component
import { Building2, LandPlot, Laptop, Sofa, Truck } from "lucide-react";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useEffect } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";

import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
<<<<<<< HEAD
import { FixedAssetFormDialog } from "@/components/domain/assets/FixedAssetFormDialog";
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { ASSET_STATUS_CONFIG } from "@/constants/assets";
import { ASSET_TYPE_ICONS } from "@/constants/assets";
=======
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation, useCrudState } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { formatCurrency, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
<<<<<<< HEAD
=======
import { ASSET_STATUS_CONFIG } from "@/constants/assets";
import { ASSET_TYPE_ICONS } from "@/constants/assets";

import { FixedAssetFormDialog } from "@/components/domain/assets/FixedAssetFormDialog";

// Icon mapping from string name to component
import { Building2, LandPlot, Laptop, Sofa, Truck } from "lucide-react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
const ICON_MAP: Record<string, React.ReactNode> = {
    Laptop: <Laptop className="w-4 h-4" />,
    Truck: <Truck className="w-4 h-4" />,
    Building2: <Building2 className="w-4 h-4" />,
    LandPlot: <LandPlot className="w-4 h-4" />,
    Sofa: <Sofa className="w-4 h-4" />,
};

export function FixedAssetsPage() {
    const crud = useCrudState<FixedAssetDto>();
    const canCreate = useHasPermission("Inventory.Item.Create");
    const canEdit = useHasPermission("Inventory.Item.Edit");
    const canDelete = useHasPermission("Inventory.Item.Delete");

    const { data: assets = [], isLoading: loading, error } = useQuery({
        queryKey: ["fixed-assets"],
        queryFn: () => fixedAssetApi.getAll()
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل الأصول", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const deleteMutation = useApiMutation(
        (id: number) => fixedAssetApi.delete(id),
        {
            successMessage: "تم حذف الأصل بنجاح",
            invalidateKeys: [["fixed-assets"]],
            onSuccessCallback: () => crud.closeDelete()
        }
    );

    const handleDelete = () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.assetID);
    };

    const columns = [
        {
            key: "assetNumber",
            header: "رقم الأصل",
            render: (row: FixedAssetDto) => (
                <div className="font-semibold text-foreground font-mono text-center" dir="ltr">{row.assetNumber}</div>
            )
        },
        {
            key: "assetName",
            header: "اسم الأصل",
            render: (row: FixedAssetDto) => (
                <div className="flex items-center gap-2">
                    <div className="w-7 h-7 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0 text-primary">
                        {ICON_MAP[ASSET_TYPE_ICONS[row.assetTypeID]] || <Monitor className="w-3.5 h-3.5" />}
                    </div>
                    <Link
                        to={`/assets/fixed/${row.assetID}`}
                        className="text-sm font-medium text-primary hover:underline"
                    >
                        {row.assetName}
                    </Link>
                </div>
            )
        },
        {
            key: "assetTypeName",
            header: "النوع",
            render: (row: FixedAssetDto) => (
                <Badge variant="secondary" className="text-xs">{row.assetTypeName || "—"}</Badge>
            )
        },
        {
            key: "acquiredCost",
            header: "التكلفة",
            render: (row: FixedAssetDto) => (
                <span className="font-mono text-sm text-center" dir="ltr">{formatCurrency(row.acquiredCost)}</span>
            )
        },
        {
            key: "quantity",
            header: "الكمية",
            render: (row: FixedAssetDto) => (
                <span className="font-mono text-center">{row.quantity}</span>
            )
        },
        {
            key: "netBookValue",
            header: "القيمة الدفترية",
            render: (row: FixedAssetDto) => (
                <span className="font-mono text-primary font-semibold text-sm text-center" dir="ltr">{formatCurrency(row.netBookValue)}</span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: FixedAssetDto) => {
                const s = ASSET_STATUS_CONFIG[row.status] || { label: "—", variant: "outline" as const };
                return <Badge variant={s.variant} className="text-xs">{s.label}</Badge>;
            }
        },
        {
            key: "purchaseDate",
            header: "تاريخ الشراء",
            render: (row: FixedAssetDto) => <span className="text-sm text-center" dir="ltr">{formatDate(row.purchaseDate)}</span>
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row: FixedAssetDto) => (
                <div className="flex items-center justify-center gap-1">
                    {canEdit && (
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => crud.openEdit(row)}
                            aria-label="تعديل"
                        >
                            <Edit className="h-4 w-4" />
                        </Button>
                    )}
                    {canDelete && (
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => crud.openDelete(row)}
                            aria-label="حذف"
                            className="text-destructive hover:text-destructive"
                        >
                            <Trash2 className="h-4 w-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    return (
        <CrudPageLayout
            title="الأصول الثابتة"
            description={`إدارة الأصول الثابتة — ${assets.length} أصل`}
            createLabel={canCreate ? "+ أصل جديد" : undefined}
            onCreateClick={canCreate ? crud.openCreate : undefined}
            headerActions={
                <Link to="/assets/depreciation-run">
                    <Button variant="outline" className="gap-2">
                        <PlayCircle className="w-4 h-4" />
                        الإهلاك
                    </Button>
                </Link>
            }
            columns={columns}
            data={assets}
            loading={loading}
        >
            <FixedAssetFormDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                initialData={crud.selectedItem || undefined}
                mode={crud.isEditing ? "edit" : "create"}
                onSuccess={crud.closeForm}
            />

            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={crud.onOpenChange}
                title="حذف الأصل"
                description={`هل أنت متأكد من حذف الأصل "${crud.deleteTarget?.assetName}"؟ هذا الإجراء لا يمكن التراجع عنه.`}
                onConfirm={handleDelete}
                variant="danger"
                isLoading={deleteMutation.isPending}
            />
        </CrudPageLayout>
    );
}
