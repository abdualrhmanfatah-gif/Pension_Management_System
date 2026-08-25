import { useQuery } from "@tanstack/react-query";
import { ArrowLeftFromLine, Hand, Trash2, User } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";

import { custodyApi, type CustodyDto } from "@/api/inventory/custody";
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useApiMutation, useCrudState } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import { INVENTORY_CUSTODY_STATUS } from "@/constants/inventory";

import { InventoryCustodyFormDialog } from "@/components/domain/inventory/InventoryCustodyFormDialog";
import { InventoryCustodyReturnDialog } from "@/components/domain/inventory/InventoryCustodyReturnDialog";

export function InventoryCustodyPage() {
    const crud = useCrudState<CustodyDto>();
    const canCreate = useHasPermission("Inventory.Custody.Create");
    const canReturn = useHasPermission("Inventory.Custody.Return");
    const canDelete = useHasPermission("Inventory.Custody.Delete");

    const { data: custodies = [], isLoading: loading, error, refetch } = useQuery({
        queryKey: ["inventory-custody"],
        queryFn: () => custodyApi.getAll()
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل العهد", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const deleteMutation = useApiMutation(
        (id: number) => custodyApi.delete(id),
        {
            successMessage: "تم حذف العهدة بنجاح",
            invalidateKeys: [["inventory-custody"]],
            onSuccessCallback: () => crud.closeDelete()
        }
    );

    const handleDelete = () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.inventoryCustodyID);
    };

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    const handleReturned = () => {
        crud.closeForm();
        refetch();
    };

    const columns = [
        {
            key: "assetNumber",
            header: "رقم الأصل",
            render: (row: CustodyDto) => (
                <div className="font-mono text-sm text-center" dir="ltr">{row.assetNumber}</div>
            )
        },
        {
            key: "assetName",
            header: "اسم الأصل",
            render: (row: CustodyDto) => (
                <div className="flex items-center gap-2">
                    <div className="w-7 h-7 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <Hand className="w-3.5 h-3.5 text-primary" />
                    </div>
                    <span className="font-medium text-sm">{row.assetName}</span>
                </div>
            )
        },
        {
            key: "employeeName",
            header: "الموظف",
            render: (row: CustodyDto) => (
                <div className="flex items-center gap-2">
                    <User className="w-4 h-4 text-muted-foreground" />
                    <span>{row.employeeName}</span>
                </div>
            )
        },
        {
            key: "quantity",
            header: "الكمية",
            render: (row: CustodyDto) => (
                <span className="font-mono">{row.quantity}</span>
            )
        },
        {
            key: "assignedDate",
            header: "تاريخ التسليم",
            render: (row: CustodyDto) => (
                <span className="text-sm text-muted-foreground whitespace-normal sm:whitespace-nowrap">
                    {formatDate(row.assignedDate)}
                </span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: CustodyDto) => {
                const s = INVENTORY_CUSTODY_STATUS[row.status] ?? INVENTORY_CUSTODY_STATUS[0];
                return <Badge variant={s.variant}>{s.label}</Badge>;
            }
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: CustodyDto) => (
                <div className="flex items-center gap-1 justify-end">
                    {canReturn && row.status === 0 && (
                        <Button variant="ghost" size="icon"
                            onClick={() => crud.openEdit(row)}
                            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-emerald-600"
                            aria-label="إرجاع العهدة"
                        >
                            <ArrowLeftFromLine className="w-4 h-4" />
                        </Button>
                    )}
                    {canDelete && (
                        <Button variant="ghost" size="icon"
                            onClick={() => crud.openDelete(row)}
                            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-destructive"
                            aria-label="حذف العهدة"
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
            title="العهد الشخصية"
            description="إدارة العهد المسلمة للموظفين (أجهزة، أدوات، مواد)"
            createLabel={canCreate ? "تسليم عهدة جديدة" : undefined}
            onCreateClick={canCreate ? crud.openCreate : undefined}
            columns={columns}
            data={custodies}
            loading={loading}
        >
            <InventoryCustodyFormDialog
                open={crud.isFormOpen && !crud.selectedItem}
                onOpenChange={crud.onOpenChange}
                onSaved={handleSaved}
                custody={null}
            />
            <InventoryCustodyReturnDialog
                open={crud.isFormOpen && !!crud.selectedItem}
                onOpenChange={crud.onOpenChange}
                onSaved={handleReturned}
                custody={crud.selectedItem}
            />
            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={crud.onOpenChange}
                title="تأكيد حذف العهدة"
                description={`هل أنت متأكد من حذف العهدة؟ لا يمكن التراجع عن هذا الإجراء.`}
                onConfirm={handleDelete}
                variant="danger"
                isLoading={deleteMutation.isPending}
            />
        </CrudPageLayout>
    );
}
