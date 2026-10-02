import { useQuery } from "@tanstack/react-query";
import { Edit, RotateCcw, ShieldAlert, Trash2, Wrench } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { type AssetCategoryDto,fixedAssetApi } from "@/api/fixedAssets";
import { AssetCategoryFormDialog } from "@/components/domain/assets/AssetCategoryFormDialog";
import type { CustomColumnDef } from "@/components/ui";
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useApiMutation } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { useHasPermission } from "@/stores/authStore";

export function AssetCategoriesPage() {
    const canManage = useHasPermission("Inventory.Item.Edit");

    const [editTarget, setEditTarget] = useState<AssetCategoryDto | null>(null);
    const [toggleTarget, setToggleTarget] = useState<AssetCategoryDto | null>(null);
    const [deleteTarget, setDeleteTarget] = useState<AssetCategoryDto | null>(null);
    const [resetConfirmOpen, setResetConfirmOpen] = useState(false);

    const { data: categories = [], isLoading: loading, error, refetch } = useQuery({
        queryKey: ["fixed-asset-categories"],
        queryFn: () => fixedAssetApi.getCategories(),
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل تصنيفات الأصول", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const toggleMutation = useApiMutation(
        (id: number) => fixedAssetApi.toggleCategoryActive(id),
        {
            successMessage: "تم تغيير حالة التصنيف بنجاح",
            invalidateKeys: [["fixed-asset-categories"]],
            onSuccessCallback: () => {
                setToggleTarget(null);
            }
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => fixedAssetApi.deleteCategory(id),
        {
            successMessage: "تم حذف التصنيف بنجاح",
            invalidateKeys: [["fixed-asset-categories"]],
            onSuccessCallback: () => {
                setDeleteTarget(null);
            }
        }
    );

    const resetMutation = useApiMutation<unknown, void>(
        () => fixedAssetApi.resetCategories(),
        {
            successMessage: "تم إعادة تعيين تصنيفات الأصول بنجاح",
            invalidateKeys: [["fixed-asset-categories"]],
        }
    );

    const handleEditSaved = () => {
        setEditTarget(null);
        refetch();
    };

    const columns: CustomColumnDef<AssetCategoryDto>[] = [
        {
            key: "categoryName",
            header: "تصنيف الأصل",
            render: (row) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
                        <Wrench className="w-4 h-4 text-primary" />
                    </div>
                    <div className="font-semibold text-foreground">{row.categoryName}</div>
                </div>
            )
        },
        {
            key: "assetTypeName",
            header: "النوع",
            render: (row) => (
                <span className="text-muted-foreground text-sm">{row.assetTypeName || "—"}</span>
            )
        },
        {
            key: "assetAccount",
            header: "حساب الأصل",
            render: (row) => row.assetAccountID ? (
                <div className="flex items-center gap-1.5">
                    <span className="font-mono text-xs font-medium" dir="ltr">{row.assetAccountCode}</span>
                    <span className="text-xs text-muted-foreground truncate max-w-[140px]">{row.assetAccountName}</span>
                </div>
            ) : (
                <span className="text-xs text-muted-foreground">—</span>
            )
        },
        {
            key: "accumDeprAccount",
            header: "مخصص الإهلاك",
            render: (row) => row.accumDeprAccountName ? (
                <span className="text-xs text-muted-foreground">{row.accumDeprAccountName}</span>
            ) : (
                <span className="text-xs text-muted-foreground">—</span>
            )
        },
        {
            key: "deprExpenseAccount",
            header: "مصروف الإهلاك",
            render: (row) => row.deprExpenseAccountName ? (
                <span className="text-xs text-muted-foreground">{row.deprExpenseAccountName}</span>
            ) : (
                <span className="text-xs text-muted-foreground">—</span>
            )
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isActive ? "success" : "outline"}>
                    {row.isActive ? "فعال" : "معطل"}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-1 justify-end">
                    {canManage && (
                        <Button variant="ghost" size="icon"
                            onClick={() => { setEditTarget(row); }}
                            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-primary"
                            aria-label="تعديل التصنيف"
                        >
                            <Edit className="w-4 h-4" />
                        </Button>
                    )}
                    {canManage && (
                        <Button variant="ghost" size="icon"
                            onClick={() => { setToggleTarget(row); }}
                            className={`min-h-[44px] min-w-[44px] transition-colors ${row.isActive ? "text-muted-foreground hover:text-amber-600" : "text-muted-foreground hover:text-emerald-600"}`}
                            aria-label={row.isActive ? "تعطيل التصنيف" : "تفعيل التصنيف"}
                        >
                            <ShieldAlert className="w-4 h-4" />
                        </Button>
                    )}
                    {canManage && (
                        <Button variant="ghost" size="icon"
                            onClick={() => { setDeleteTarget(row); }}
                            className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-destructive"
                            aria-label="حذف التصنيف"
                        >
                            <Trash2 className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        },
    ];

    return (
        <>
            <CrudPageLayout
                title="تصنيفات الأصول الثابتة"
                description="تصنيف الأصول حسب النوع مع الربط بالدليل المحاسبي"
                headerActions={canManage ? (
                    <Button
                        variant="outline"
                        onClick={() => setResetConfirmOpen(true)}
                        disabled={resetMutation.isPending}
                        className="gap-2"
                    >
                        <RotateCcw className="w-4 h-4" />
                        {resetMutation.isPending ? "جاري إعادة التعيين..." : "إعادة تعيين التصنيفات"}
                    </Button>
                ) : undefined}
                columns={columns}
                data={categories}
                loading={loading}
            />

            <AssetCategoryFormDialog
                open={editTarget !== null}
                onOpenChange={() => setEditTarget(null)}
                onSaved={handleEditSaved}
                category={editTarget}
            />

            <ConfirmDialog
                open={toggleTarget !== null}
                onOpenChange={() => setToggleTarget(null)}
                title={toggleTarget?.isActive ? "تعطيل التصنيف" : "تفعيل التصنيف"}
                description={`هل أنت متأكد من ${toggleTarget?.isActive ? "تعطيل" : "تفعيل"} التصنيف "${toggleTarget?.categoryName}"؟`}
                onConfirm={() => { if (toggleTarget) toggleMutation.mutate(toggleTarget.categoryID); }}
                variant={toggleTarget?.isActive ? "warning" : "default"}
                isLoading={toggleMutation.isPending}
            />
            <ConfirmDialog
                open={deleteTarget !== null}
                onOpenChange={() => setDeleteTarget(null)}
                title="تأكيد حذف التصنيف"
                description={`هل أنت متأكد من حذف التصنيف "${deleteTarget?.categoryName}"؟ لا يمكن التراجع عن هذا الإجراء.`}
                onConfirm={() => { if (deleteTarget) deleteMutation.mutate(deleteTarget.categoryID); }}
                variant="danger"
                isLoading={deleteMutation.isPending}
            />
            <ConfirmDialog
                open={resetConfirmOpen}
                onOpenChange={setResetConfirmOpen}
                title="إعادة تعيين تصنيفات الأصول"
                description="هل أنت متأكد من إعادة تعيين تصنيفات الأصول؟ سيتم حذف جميع التصنيفات وإعادة إنشائها بالحسابات الصحيحة."
                onConfirm={() => { resetMutation.mutate(undefined as never); setResetConfirmOpen(false); }}
                variant="danger"
                isLoading={resetMutation.isPending}
            />
        </>
    );
}
