import { useQuery } from "@tanstack/react-query";
import { Building2, MapPin, Pencil, Phone, Trash2 } from "lucide-react";

import { branchesApi } from "@/api/branches";
<<<<<<< HEAD
import { BranchDialog } from "@/components/domain/admin/BranchDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { Badge, Button, CrudPageLayout, Text } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useApiMutation,useCrudState } from "@/hooks";

<<<<<<< HEAD
=======
import { BranchDialog } from "@/components/domain/admin/BranchDialog";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
// ─── Types ─────────────────────────────────────────────

interface BranchDto {
    pensionBranchID: number;
    branchName: string;
    governorate: string | null;
    address: string | null;
    phone: string | null;
    managerName: string | null;
    managerPhone: string | null;
    isActive: boolean;
    usersCount: number;
    createdAt: string;
}

// ─── Page ──────────────────────────────────────────────

export function BranchesListPage() {
    const crud = useCrudState<BranchDto>();

    const { data: branches = [], isLoading: loading, isError, refetch } = useQuery({
        queryKey: ["branches"],
        queryFn: () => branchesApi.getAll()
    });

    const deleteMutation = useApiMutation(
        (id: number) => branchesApi.delete(id),
        {
            invalidateKeys: [["branches"], ["lookups"]],
            successMessage: "تم حذف الفرع بنجاح",
            onSuccessCallback: () => crud.closeDelete(),
        }
    );

    const toggleActiveMutation = useApiMutation(
        (branch: BranchDto) => branchesApi.toggleActive(branch.pensionBranchID),
        {
            invalidateKeys: [["branches"], ["lookups"]],
            successMessage: "تم تحديث حالة الفرع بنجاح",
        }
    );

    const handleDelete = async () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.pensionBranchID);
    };

    const handleToggleActive = (branch: BranchDto) => {
        toggleActiveMutation.mutate(branch);
    };

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    // ─── Columns ───────────────────────────────────────

    const columns = [
        {
            key: "branchName",
            header: "اسم الفرع",
            render: (row: BranchDto) => (
                <div className="flex items-center gap-3">
                    <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10">
                        <Building2 className="w-4 h-4 text-primary" />
                    </div>
                    <div>
                        <p className="font-medium text-foreground">{row.branchName}</p>
                        <Text variant="caption">{row.governorate}</Text>
                    </div>
                </div>
            ),
        },
        {
            key: "manager",
            header: "المدير",
            render: (row: BranchDto) => (
                <div className="text-sm">
                    <Text variant="body">{row.managerName || "—"}</Text>
                    {row.managerPhone && (
                        <Text variant="caption" dir="ltr">
                            {row.managerPhone}
                        </Text>
                    )}
                </div>
            ),
        },
        {
            key: "contact",
            header: "الاتصال والموقع",
            render: (row: BranchDto) => (
                <div className="text-sm space-y-1">
                    {row.phone && (
                        <div className="flex items-center gap-1 text-muted-foreground">
                            <Phone className="w-3 h-3" />
                            <span dir="ltr">{row.phone}</span>
                        </div>
                    )}
                    {row.address && (
                        <div className="flex items-center gap-1 text-muted-foreground">
                            <MapPin className="w-3 h-3" />
                            <span>{row.address}</span>
                        </div>
                    )}
                </div>
            ),
        },
        {
            key: "usersCount",
            header: "الموظفين",
            render: (row: BranchDto) => (
                <Badge variant="outline">{row.usersCount} موظف</Badge>
            ),
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: BranchDto) => (
                <div
                    className="cursor-pointer min-h-[44px] min-w-[44px] flex items-center justify-center"
                    onClick={() => handleToggleActive(row)}
                    title="انقر لتغيير الحالة"
                >
                    <Badge variant={row.isActive ? "success" : "danger"}>
                        {row.isActive ? "نشط" : "معطل"}
                    </Badge>
                </div>
            ),
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: BranchDto) => (
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

    if (isError) return <div                     className="p-6 sm:p-12 text-center text-destructive">تعذر تحميل بيانات الفروع</div>;

    return (
        <CrudPageLayout
            title="إدارة الفروع"
            description="إدارة فروع صندوق التقاعد ومعلوماتها"
            createLabel="إضافة فرع"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={branches}
            loading={loading}
            rowKey={(row) => row.pensionBranchID}
            emptyMessage="لا يوجد فروع حالياً"
        >
            <BranchDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                onSaved={handleSaved}
                branch={crud.selectedItem}
            />

            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={(o) => !o && crud.closeDelete()}
                title="تأكيد حذف الفرع"
                description={`هل أنت متأكد من حذف الفرع "${crud.deleteTarget?.branchName}"؟ لا يمكن التراجع عن هذا الإجراء.`}
                onConfirm={handleDelete}
                variant="danger"
            />
        </CrudPageLayout>
    );
}
