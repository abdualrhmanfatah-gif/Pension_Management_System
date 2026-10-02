import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit, Eye, Trash2 } from "lucide-react";
import { useCallback, useEffect, useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi, type PortfolioDto } from "@/api/investment";
import { PortfolioDialog } from "@/components/domain/investment/PortfolioDialog";
import {
    Button,
    CrudPageLayout,
    type CustomColumnDef,
    useConfirm} from "@/components/ui";
import { useApiMutation,useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

/**
 * صفحة المحافظ الاستثمارية
 * تعرض قائمة بكافة المحافظ الاستثمارية التابعة للصندوق
 */
export function PortfoliosPage() {
    const navigate = useNavigate();
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canManage = useHasPermission("Investment.Manage");

    const crud = useCrudState<PortfolioDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: portfoliosRes, isLoading, error } = useQuery({
        queryKey: ["portfolios"],
        queryFn: () => investmentApi.getPortfolios()
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل المحافظ الاستثمارية", { id: "portfolios" });
    }, [error]);

    const portfolios = portfoliosRes || [];

    // طفرة حذف
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deletePortfolio(id),
        {
            successMessage: "تم حذف المحفظة بنجاح",
            invalidateKeys: [["portfolios"]],
        }
    );

    const handleDelete = useCallback(async (portfolio: PortfolioDto) => {
        const ok = await confirm({
            title: "حذف المحفظة الاستثمارية",
            description: `هل أنت متأكد من رغبتك في حذف المحفظة "${portfolio.name}"؟ سيتم حذف جميع المشاريع والمعاملات المرتبطة بهذه المحفظة.`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(portfolio.portfolioID);
    }, [confirm, deleteMutation]);

    // تعريف الأعمدة
    const columns = useMemo<CustomColumnDef<PortfolioDto>[]>(() => [
        {
            key: "portfolioID",
            header: "رقم المحفظة",
            render: (row) => <span className="font-mono text-xs text-muted-foreground">{row.portfolioID}</span>
        },
        {
            key: "name",
            header: "اسم المحفظة",
            render: (row) => <span className="font-bold">{row.name}</span>
        },
        { key: "managerName", header: "المدير المسؤول", accessor: (row) => row.managerName },
        {
            key: "totalTargetCapital",
            header: "رأس المال المستهدف",
            render: (row) => (
                <span className="font-semibold text-emerald-600">
                    {row.totalTargetCapital.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "createdAt",
            header: "تاريخ الإنشاء",
            render: (row) => <span className="text-sm">{new Date(row.createdAt).toLocaleDateString("ar-YE")}</span>
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <Link
                        to={`/investment/portfolios/${row.portfolioID}`}
                        className="p-2 hover:bg-muted rounded-md text-primary transition-colors"
                        title="عرض التفاصيل"
                    >
                        <Eye className="w-4 h-4" />
                    </Link>
                    {canManage && (
                        <>
                            <Button variant="ghost" size="sm" className="text-blue-600" title="تعديل" onClick={(e) => { e.stopPropagation(); crud.openEdit(row); }}>
                                <Edit className="w-4 h-4" />
                            </Button>
                            <Button variant="ghost" size="sm" className="text-red-600" title="حذف" onClick={(e) => { e.stopPropagation(); handleDelete(row); }}>
                                <Trash2 className="w-4 h-4" />
                            </Button>
                        </>
                    )}
                </div>
            )
        }
    ], [canManage, crud, handleDelete]);

    return (
        <CrudPageLayout
            title="المحافظ الاستثمارية"
            description="إدارة جميع محافظ صندوق التقاعد الاستثمارية"
            createLabel="محفظة جديدة"
            onCreateClick={canManage ? crud.openCreate : undefined}
            canCreate={canManage}
            columns={columns}
            data={portfolios}
            loading={isLoading}
            emptyMessage="لم يتم إنشاء أي محافظ استثمارية بعد."
            onRowClick={(row) => navigate(`/investment/portfolios/${row.portfolioID}`)}
        >
            <PortfolioDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem || undefined}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["portfolios"] }); }}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
