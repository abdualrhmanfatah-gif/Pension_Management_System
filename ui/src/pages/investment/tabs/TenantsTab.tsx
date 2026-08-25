import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Edit, Eye, FileText, Star, Trash2 } from "lucide-react";
import { useCallback,useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi,type TenantDto } from "@/api/investment";
import { 
    Badge, 
    BaseFormDialog,
    Button, 
    CrudPageLayout,
    type CustomColumnDef, 
    ErrorState,
    useConfirm} from "@/components/ui";
import { TenantDialog } from "@/components/domain/investment/TenantDialog";
import { BADGE_VARIANT } from "@/constants";
import { useApiMutation, useCrudState } from "@/hooks";

import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";

export function TenantsTab() {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const queryClient = useQueryClient();
    const crud = useCrudState<TenantDto>();
    const navigate = useNavigate();

    // جلب المستأجرين
    const { data: tenantsRes, isLoading, error } = useQuery({
        queryKey: ["tenants"],
        queryFn: () => investmentApi.getTenants()
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل المستأجرين", { id: "tenants" });
    }, [error]);

    const tenants = tenantsRes || [];

    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteTenant(id),
        {
            successMessage: "تم حذف المستأجر بنجاح",
            onSuccess: () => queryClient.invalidateQueries({ queryKey: ["tenants"] })
        }
    );

    const handleDelete = useCallback(async (item: TenantDto) => {
        const ok = await confirm({
            title: "حذف مستأجر",
            description: `هل أنت متأكد من حذف المستأجر "${item.fullName}"؟ سيتم حذف كافة العقود المرتبطة به.`,
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(item.tenantID);
    }, [confirm, deleteMutation]);

    const getTypeLabel = (type?: string) => {
        switch (type) {
            case "Individual": return "فرد";
            case "Corporate": return "شركة";
            case "Merchant": return "تاجر";
            case "Government": return "حكومي";
            default: return type || "غير محدد";
        }
    };

    const columns = useMemo<CustomColumnDef<TenantDto>[]>(() => [
        { key: "fullName", header: "اسم المستأجر", accessor: (row: TenantDto) => row.fullName },
        {
            key: "tenantType",
            header: "النوع",
            render: (row: TenantDto) => <Badge variant={BADGE_VARIANT.OUTLINE}>{getTypeLabel(row.tenantType)}</Badge>
        },
        { key: "phone", header: "رقم التواصل", accessor: (row: TenantDto) => row.phone || "—" },
        {
            key: "ratingScore",
            header: "التقييم",
            render: (row: TenantDto) => (
                <div className="flex items-center gap-1">
                    <span className="font-medium font-mono">{row.ratingScore}</span>
                    <Star className="w-4 h-4 text-amber-500 fill-amber-500" />
                </div>
            )
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row: TenantDto) => (
                <div className="flex items-center gap-1 justify-center">
                    <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0" onClick={() => crud.openViewer(row)}>
                        <FileText className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0" onClick={() => navigate(`/investment/leasing/tenants/${row.tenantID}`)}>
                        <Eye className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0 text-blue-600" onClick={() => crud.openEdit(row)}>
                        <Edit className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0 text-destructive" onClick={() => handleDelete(row)}>
                        <Trash2 className="w-4 h-4" />
                    </Button>
                </div>
            )
        }
    ], [crud, handleDelete, navigate]);

    if (error) {
        return <ErrorState message="خطأ في تحميل المستأجرين" />;
    }

    return (
        <CrudPageLayout
            title="إدارة المستأجرين"
            description="قاعدة بيانات المستأجرين (أفراد، شركات، تجار) وتصنيفاتهم الائتمانية"
            createLabel="إضافة مستأجر"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={tenants}
            loading={isLoading}
        >
            <TenantDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                mode={crud.isEditing ? "edit" : "create"}
                initialData={crud.selectedItem || undefined}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: ["tenants"] }); }}
            />

            <BaseFormDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.onOpenChange}
                title={`الوثائق والمستندات: ${crud.viewerTarget?.fullName}`}
            >
                <InvestmentDocumentsManager
                    entityType="tenant"
                    entityId={crud.viewerTarget?.tenantID || 0}
                />
            </BaseFormDialog>

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
