import { useQuery } from "@tanstack/react-query";
import { Edit, Package, Trash2 } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";

import { itemApi, type ItemDto } from "@/api/inventory/items";
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useApiMutation, useCrudState } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import { INVENTORY_ITEM_STATUS, INVENTORY_ITEM_CONDITION } from "@/constants/inventory";

import { InventoryItemFormDialog } from "@/components/domain/inventory/InventoryItemFormDialog";

export function InventoryItemsPage() {
    const crud = useCrudState<ItemDto>();
    const canCreate = useHasPermission("Inventory.Item.Create");
    const canEdit = useHasPermission("Inventory.Item.Edit");
    const canDelete = useHasPermission("Inventory.Item.Delete");

    const { data: items = [], isLoading: loading, error, refetch } = useQuery({
        queryKey: ["inventory-items"],
        queryFn: () => itemApi.getAll()
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل الأصناف", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const deleteMutation = useApiMutation(
        (id: number) => itemApi.delete(id),
        {
            successMessage: "تم حذف الصنف بنجاح",
            invalidateKeys: [["inventory-items"]],
            onSuccessCallback: () => crud.closeDelete()
        }
    );

    const handleDelete = () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.inventoryItemID);
    };

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    const columns = [
        {
            key: "itemName",
            header: "اسم الصنف",
            render: (row: ItemDto) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <Package className="w-4 h-4 text-primary" />
                    </div>
                    <div>
                        <div className="font-semibold text-foreground">{row.itemName}</div>
                        <div className="text-xs text-muted-foreground font-mono mt-0.5" dir="ltr">
                            {row.itemCode}
                        </div>
                    </div>
                </div>
            )
        },
        {
            key: "unit",
            header: "الوحدة",
            render: (row: ItemDto) => <span className="text-muted-foreground">{row.unit}</span>
        },
        {
            key: "quantity",
            header: "الكمية",
            render: (row: ItemDto) => (
                <div className={`font-mono font-medium ${row.quantity <= row.minimumQuantity ? "text-amber-600" : "text-foreground"}`}>
                    {row.quantity}
                    {row.quantity <= row.minimumQuantity && row.minimumQuantity > 0 && (
                        <span className="text-xs text-amber-500 block">أقل من الحد الأدنى</span>
                    )}
                </div>
            )
        },
        {
            key: "condition",
            header: "الحالة الفنية",
            render: (row: ItemDto) => {
                const c = INVENTORY_ITEM_CONDITION[row.condition] ?? INVENTORY_ITEM_CONDITION[0];
                return <Badge variant={c.variant}>{c.label}</Badge>;
            }
        },
        {
            key: "status",
            header: "حالة التداول",
            render: (row: ItemDto) => {
                const s = INVENTORY_ITEM_STATUS[row.status] ?? INVENTORY_ITEM_STATUS[0];
                return <Badge variant={s.variant}>{s.label}</Badge>;
            }
        },
        {
            key: "createdAt",
            header: "تاريخ الإنشاء",
            render: (row: ItemDto) => (
                <span className="text-sm text-muted-foreground whitespace-normal sm:whitespace-nowrap">
                    {formatDate(row.createdAt)}
                </span>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: ItemDto) => (
                <div className="flex items-center gap-1 justify-end">
                    {canEdit && (
                        <Button variant="ghost" size="icon"
                            onClick={() => crud.openEdit(row)}
                            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-primary"
                            aria-label="تعديل الصنف"
                        >
                            <Edit className="w-4 h-4" />
                        </Button>
                    )}
                    {canDelete && (
                        <Button variant="ghost" size="icon"
                            onClick={() => crud.openDelete(row)}
                            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-destructive"
                            aria-label="حذف الصنف"
                        >
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    return (
        <CrudPageLayout
            title="إدارة الأصناف"
            description="عرض وإدارة الأصول والمخزون (أجهزة، أثاث، مستلزمات، مركبات)"
            createLabel={canCreate ? "إضافة صنف جديد" : undefined}
            onCreateClick={canCreate ? crud.openCreate : undefined}
            columns={columns}
            data={items}
            loading={loading}
        >
            <InventoryItemFormDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                onSaved={handleSaved}
                item={crud.selectedItem}
            />
            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={crud.onOpenChange}
                title="تأكيد حذف الصنف"
                description={`هل أنت متأكد من حذف الصنف "${crud.deleteTarget?.itemName}"؟ لا يمكن التراجع عن هذا الإجراء.`}
                onConfirm={handleDelete}
                variant="danger"
                isLoading={deleteMutation.isPending}
            />
        </CrudPageLayout>
    );
}
