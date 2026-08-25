import { useQuery } from "@tanstack/react-query";
import { Edit, Landmark, ShieldAlert, Trash2 } from "lucide-react";
import { useEffect,useState } from "react";
import { toast } from "sonner";

import { bankApi, type BankDto } from "@/api/banks";
import { BankFormDialog } from "@/components/domain/admin/BankFormDialog";
import { AlertBanner, Badge, Button,CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useApiMutation,useCrudState } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { formatDate } from "@/lib/utils";

// ─── Page ──────────────────────────────────────────────

export function BanksListPage() {
    const crud = useCrudState<BankDto>();

    const [toggleConfirmOpen, setToggleConfirmOpen] = useState(false);
    const [bankToToggle, setBankToToggle] = useState<BankDto | null>(null);

    const { data: banks = [], isLoading: loading, error, refetch } = useQuery({
        queryKey: ["banks"],
        queryFn: () => bankApi.getAll()
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل البنوك", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const deleteMutation = useApiMutation(
        (id: number) => bankApi.delete(id),
        {
            successMessage: "تم حذف البنك بنجاح",
            invalidateKeys: [["banks"]],
            onSuccessCallback: () => {
                crud.closeDelete();
            }
        }
    );

    const toggleMutation = useApiMutation(
        (id: number) => bankApi.toggleActive(id),
        {
            successMessage: "تم تغيير حالة البنك بنجاح",
            invalidateKeys: [["banks"]],
            onSuccessCallback: () => {
                setToggleConfirmOpen(false);
                setBankToToggle(null);
            }
        }
    );

    // Handlers
    const handleDelete = () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.bankID);
    };

    const handleToggleActive = () => {
        if (!bankToToggle) return;
        toggleMutation.mutate(bankToToggle.bankID);
    };

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    // Columns
    const columns = [
        {
            key: "bankName",
            header: "اسم البنك",
            render: (row: BankDto) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <Landmark className="w-4 h-4 text-primary" />
                    </div>
                    <div>
                        <div className="font-semibold text-foreground">{row.bankName}</div>
                        {row.bankCode && (
                            <div className="text-xs text-muted-foreground font-mono mt-0.5" dir="ltr">
                                {row.bankCode}
                            </div>
                        )}
                    </div>
                </div>
            )
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: BankDto) => (
                <Badge variant={row.isActive ? "success" : "outline"}>
                    {row.isActive ? "فعال" : "معطل"}
                </Badge>
            )
        },
        {
            key: "batchCount",
            header: "كشوفات الصرف",
            render: (row: BankDto) => (
                <div className="font-mono text-muted-foreground">
                    {row.batchCount > 0 ? (
                        <span className="text-primary font-medium">{row.batchCount} كشف</span>
                    ) : (
                        "لا يوجد"
                    )}
                </div>
            )
        },
        {
            key: "createdAt",
            header: "تاريخ الإنشاء",
            render: (row: BankDto) => (
                <span className="text-sm text-muted-foreground whitespace-nowrap">
                    {formatDate(row.createdAt)}
                </span>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: BankDto) => (
                <div className="flex items-center gap-1 justify-end">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => crud.openEdit(row)}
                        className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-primary"
                        title="تعديل البنك"
                    >
                        <Edit className="w-4 h-4" />
                    </Button>

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => {
                            setBankToToggle(row);
                            setToggleConfirmOpen(true);
                        }}
                        className={`min-h-[44px] min-w-[44px] transition-colors ${row.isActive
                            ? "text-muted-foreground hover:text-amber-600"
                            : "text-muted-foreground hover:text-emerald-600"
                            }`}
                        title={row.isActive ? "تعطيل البنك" : "تفعيل البنك"}
                    >
                        <ShieldAlert className="w-4 h-4" />
                    </Button>

                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => crud.openDelete(row)}
                        className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-destructive"
                        disabled={row.batchCount > 0}
                        title={row.batchCount > 0 ? "لا يمكن حذف بنك مرتبط بكشوفات (قم بتعطيله)" : "حذف البنك"}
                    >
                        <Trash2 className="w-4 h-4" />
                    </Button>
                </div>
            )
        }
    ];

    if (error && !loading) return <AlertBanner variant="destructive" title="تعذر تحميل بيانات البنوك" />;

    return (
        <CrudPageLayout
            title="إدارة البنوك"
            description="عرض وإدارة البنوك المعتمدة لصرف المعاشات"
            createLabel="إضافة بنك جديد"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={banks}
            loading={loading}
        >
            <BankFormDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                onSaved={handleSaved}
                bank={crud.selectedItem}
            />

            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={crud.onOpenChange}
                title="تأكيد حذف البنك"
                description={`هل أنت متأكد من حذف البنك "${crud.deleteTarget?.bankName}" بشكل نهائي؟ لا يمكن التراجع عن هذا الإجراء.`}
                onConfirm={handleDelete}
                variant="danger"
                isLoading={deleteMutation.isPending}
            />

            <ConfirmDialog
                open={toggleConfirmOpen}
                onOpenChange={setToggleConfirmOpen}
                title={bankToToggle?.isActive ? "تعطيل البنك" : "تفعيل البنك"}
                description={`هل أنت متأكد من ${bankToToggle?.isActive ? "تعطيل" : "تفعيل"} البنك "${bankToToggle?.bankName}"؟`}
                onConfirm={handleToggleActive}
                variant={bankToToggle?.isActive ? "warning" : "default"}
                isLoading={toggleMutation.isPending}
            />
        </CrudPageLayout>
    );
}
