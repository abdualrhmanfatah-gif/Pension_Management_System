import { useQuery } from "@tanstack/react-query";
import { Medal, Pencil, Trash2 } from "lucide-react";

import { ranksApi } from "@/api/ranks";
<<<<<<< HEAD
import { RankFormDialog } from "@/components/domain/admin/RankFormDialog";
import { Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
=======
import { Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { RankFormDialog } from "@/components/domain/admin/RankFormDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation,useCrudState } from "@/hooks";
import { formatCurrency } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface RankDto {
    rankID: number;
    rankName: string;
    rankLevel: number;
    baseSalary: number;
    createdAt: string;
}

// ─── Page ──────────────────────────────────────────────

export function RanksListPage() {
    const crud = useCrudState<RankDto>();

    const { data: ranks = [], isLoading: loading, isError, refetch } = useQuery({
        queryKey: ["ranks"],
        queryFn: () => ranksApi.getAll()
    });

    const deleteMutation = useApiMutation(
        (id: number) => ranksApi.delete(id),
        {
            invalidateKeys: [["ranks"], ["lookups"]],
            successMessage: "تم حذف الرتبة بنجاح",
            onSuccessCallback: () => crud.closeDelete(),
        }
    );

    const handleDelete = async () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.rankID);
    };

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    // ─── Columns ───────────────────────────────────────

    const columns = [
        {
            key: "rankLevel",
            header: "المستوى",
            render: (row: RankDto) => (
                <span className="font-mono text-muted-foreground">#{row.rankLevel}</span>
            ),
        },
        {
            key: "rankName",
            header: "اسم الرتبة",
            render: (row: RankDto) => (
                <div className="flex items-center gap-3">
                    <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10">
                        <Medal className="w-4 h-4 text-primary" />
                    </div>
                    <span className="font-medium text-foreground">{row.rankName}</span>
                </div>
            ),
        },
        {
            key: "baseSalary",
            header: "المعاش الإساسي",
            render: (row: RankDto) => (
                <span className="font-medium" dir="ltr">
                    {formatCurrency(row.baseSalary)}
                </span>
            ),
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: RankDto) => (
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={(e) => {
                            e.stopPropagation();
                            crud.openEdit(row);
                        }}
                        title="تعديل"
                    >
                        <Pencil className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={(e) => {
                            e.stopPropagation();
                            crud.openDelete(row);
                        }}
                        title="حذف"
                    >
                        <Trash2 className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    if (isError) return <div                     className="p-6 sm:p-12 text-center text-destructive">تعذر تحميل الرتب</div>;

    return (
        <CrudPageLayout
            title="الرتب العسكرية"
            description="إدارة سلم الرتب والمعاشات الأساسية"
            createLabel="إضافة رتبة"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={ranks}
            loading={loading}
            rowKey={(row) => row.rankID}
            emptyMessage="لا يوجد رتب مسجلة حالياً"
        >
            <RankFormDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                onSaved={handleSaved}
                rank={crud.selectedItem}
            />

            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={crud.onOpenChange}
                title="تأكيد حذف الرتبة"
                description={`هل أنت متأكد من حذف الرتبة "${crud.deleteTarget?.rankName}"؟`}
                onConfirm={handleDelete}
                variant="danger"
            />
        </CrudPageLayout>
    );
}
