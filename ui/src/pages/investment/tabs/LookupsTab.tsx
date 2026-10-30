import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit, Trash2 } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";

import type { InvestmentLookupDto } from "@/api/investment";
<<<<<<< HEAD
import { LookupDialog } from "@/components/domain/investment/LookupDialog";
import { Badge, Button, CrudPageLayout, ErrorState } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import type { CustomColumnDef } from "@/components/ui/DataTable";
=======
import { Badge, Button, CrudPageLayout, ErrorState } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import type { CustomColumnDef } from "@/components/ui/DataTable";
import { LookupDialog } from "@/components/domain/investment/LookupDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation,useCrudState } from "@/hooks";

interface LookupsTabProps {
    queryKey: string;
    fetchFn: () => Promise<InvestmentLookupDto[]>;
    createFn: (data: Partial<InvestmentLookupDto>) => Promise<number>;
    updateFn?: (id: number, data: Partial<InvestmentLookupDto>) => Promise<boolean>;
    deleteFn?: (id: number) => Promise<boolean>;
    title: string;
    modalTitle: string;
    showIsCredit?: boolean;
}

export function LookupsTab({
    queryKey,
    fetchFn,
    createFn,
    updateFn,
    deleteFn,
    title,
    modalTitle,
    showIsCredit
}: LookupsTabProps) {
    const queryClient = useQueryClient();
    const crud = useCrudState<InvestmentLookupDto>();

    const { data: itemsRes, isLoading, error } = useQuery({
        queryKey: [queryKey],
        queryFn: fetchFn
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل البيانات", { id: queryKey });
    }, [error]);

    const items = itemsRes || [];

    const createMutation = useApiMutation(
        createFn,
        {
            successMessage: "تم الإضافة بنجاح",
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: [queryKey] });
                crud.closeForm();
            }
        }
    );

    const updateMutation = useApiMutation(
        ({ id, data }: { id: number; data: Partial<InvestmentLookupDto> }) =>
            updateFn ? updateFn(id, data) : Promise.resolve(true),
        {
            successMessage: "تم التحديث بنجاح",
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: [queryKey] });
                crud.closeForm();
            }
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => deleteFn ? deleteFn(id) : Promise.resolve(true),
        {
            successMessage: "تم الحذف بنجاح",
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: [queryKey] });
                crud.closeDelete();
            }
        }
    );

    const handleSubmit = async (values: Partial<InvestmentLookupDto>) => {
        if (crud.isEditing && updateFn) {
            await updateMutation.mutateAsync({ id: crud.selectedItem!.id, data: values });
        } else {
            await createMutation.mutateAsync(values);
        }
    };

    const columns: CustomColumnDef<InvestmentLookupDto>[] = [
        { key: "id", header: "الرقم", accessor: (row) => row.id },
        { key: "name", header: "الاسم", accessor: (row) => row.name },
        { key: "code", header: "الرمز", accessor: (row) => row.code || "" },
        { key: "description", header: "الوصف", accessor: (row) => row.description || "" }
    ];

    if (showIsCredit) {
        columns.push({
            key: "isCredit",
            header: "النوع المالي",
            render: (row) => row.isCredit ?
                <Badge variant="success">دائن</Badge> :
                <Badge variant="danger">مدين</Badge>
        });
    }

    columns.push({
        key: "actions",
        header: "الإجراءات",
        render: (row) => (
            <div className="flex items-center gap-2">
                {updateFn && (
                    <Button variant="ghost" size="icon" onClick={() => crud.openEdit(row)}>
                        <Edit className="w-4 h-4 text-muted-foreground" />
                    </Button>
                )}
                {deleteFn && (
                    <Button variant="ghost" size="icon" onClick={() => crud.openDelete(row)}>
<<<<<<< HEAD
                        <Trash2 className="w-4 h-4 text-danger" />
=======
                        <Trash2 className="w-4 h-4 text-red-500" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Button>
                )}
            </div>
        )
    });

    if (error) {
        return <ErrorState message="خطأ في تحميل البيانات المرجعية" />;
    }

    return (
        <CrudPageLayout
            title={title}
            createLabel="إضافة جديد"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={items}
            loading={isLoading}
        >
            <LookupDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                onSubmit={handleSubmit}
                initialData={crud.selectedItem || undefined}
                isLoading={createMutation.isPending || updateMutation.isPending}
                title={modalTitle}
            />

            {deleteFn && (
                <ConfirmDialog
                    open={crud.isDeleteOpen}
                    onOpenChange={(o) => !o && crud.closeDelete()}
                    onConfirm={async () => { if (crud.deleteTarget) await deleteMutation.mutateAsync(crud.deleteTarget.id); }}
                    title="حذف العنصر"
                    description="هل أنت متأكد من رغبتك في حذف هذا العنصر؟ لا يمكن التراجع عن هذا الإجراء."
                />
            )}
        </CrudPageLayout>
    );
}
