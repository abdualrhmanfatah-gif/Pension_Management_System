import { useQuery } from "@tanstack/react-query";
import { Download, Edit, FileText, Trash2 } from "lucide-react";
import { useCallback, useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { LeaseContractDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
<<<<<<< HEAD
import { ContractDialog } from "@/components/domain/investment/ContractDialog";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState, useConfirm } from "@/components/ui";
=======
import { Badge, Button, CrudPageLayout, type CustomColumnDef, ErrorState, useConfirm } from "@/components/ui";
import { ContractDialog } from "@/components/domain/investment/ContractDialog";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation, useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

interface ContractsTabProps {
    propertyId?: number;
    tenantId?: number;
}

/**
 * تبويب عقود الإيجار
 */
export function ContractsTab({ propertyId, tenantId }: ContractsTabProps) {
    const canManage = useHasPermission("Investment.Manage");
    const { confirm, ConfirmDialogElement } = useConfirm();
    const crud = useCrudState<LeaseContractDto>();

    // جلب البيانات
    const { data: contractsRes, isLoading, error } = useQuery({
        queryKey: propertyId ? ["contracts", propertyId] : ["contracts"],
        queryFn: () => investmentApi.getContracts(propertyId)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل العقود", { id: "contracts" });
    }, [error]);

    const contracts = (Array.isArray(contractsRes) ? contractsRes : []).filter(c => !tenantId || c.tenantID === tenantId);

    // طفرة الحذف فقط
    const deleteMutation = useApiMutation(
        (id: number) => investmentApi.deleteContract(id),
        {
            invalidateKeys: [["contracts"], ["contracts", propertyId]],
            successMessage: "تم حذف العقد بنجاح",
        }
    );

    const handleDelete = useCallback(async (contract: LeaseContractDto) => {
        const ok = await confirm({
            title: "حذف العقد",
            description: "هل أنت متأكد من رغبتك في حذف هذا العقد؟ سيتم حذف جميع جداول الدفع المرتبطة به.",
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(contract.contractID);
    }, [confirm, deleteMutation]);

    const getStatusColor = (status: string): "success" | "warning" | "danger" | "outline" => {
        switch (status) {
            case "Active": return "success";
            case "Draft": return "outline";
            case "Expired": return "warning";
            case "Terminated": return "danger";
            default: return "outline";
        }
    };

    const getStatusLabel = (status: string) => {
        switch (status) {
            case "Active": return "نشط";
            case "Draft": return "مسودة";
            case "Expired": return "منتهي";
            case "Terminated": return "مفسوخ";
            default: return status;
        }
    };

    const columns = useMemo<CustomColumnDef<LeaseContractDto>[]>(() => [
        { key: "tenantName", header: "المستأجر", accessor: (row) => row.tenantName },
        {
            key: "unitInfo",
            header: "العقار والوحدة",
            render: (row) => (
                <div className="text-start">
                    <div className="font-medium">{row.propertyName}</div>
                    <div className="text-xs text-muted-foreground">{row.unitNumber}</div>
                </div>
            )
        },
        {
            key: "dates",
            header: "الفترة",
            render: (row) => (
                <div className="text-xs text-start">
                    <div>{new Date(row.startDate).toLocaleDateString("ar-YE")}</div>
                    <div className="text-muted-foreground">إلى {new Date(row.endDate).toLocaleDateString("ar-YE")}</div>
                </div>
            )
        },
        {
            key: "totalContractValue",
            header: "قيمة العقد",
            render: (row) => (
                <span className="font-mono font-semibold">
                    {row.totalContractValue.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={getStatusColor(row.status)}>
                    {getStatusLabel(row.status)}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2">
                    {row.signedContractPath && (
                        <a
                            href={`${import.meta.env.VITE_API_BASE_URL}/${row.signedContractPath}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="p-2 hover:bg-muted rounded-md text-primary transition-colors"
                            title="تنزيل العقد"
                        >
                            <Download className="w-4 h-4" />
                        </a>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق والمستندات">
<<<<<<< HEAD
                        <FileText className="w-4 h-4 text-info" />
=======
                        <FileText className="w-4 h-4 text-blue-500" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Button>
                    {canManage && (
                        <>
                            <Button variant="ghost" size="icon" onClick={() => crud.openEdit(row)} title="تعديل">
                                <Edit className="w-4 h-4 text-muted-foreground" />
                            </Button>
                            <Button variant="ghost" size="icon" onClick={() => handleDelete(row)} title="حذف">
<<<<<<< HEAD
                                <Trash2 className="w-4 h-4 text-danger" />
=======
                                <Trash2 className="w-4 h-4 text-red-500" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            </Button>
                        </>
                    )}
                </div>
            )
        }
    ], [canManage, crud, handleDelete]);

    if (error) {
        return <ErrorState message="خطأ في تحميل العقود" />;
    }

    return (
        <CrudPageLayout
            title="إدارة العقود"
            description="تنظيم عقود الإيجار والالتزامات المالية للمستأجرين"
            onCreateClick={canManage ? crud.openCreate : undefined}
            createLabel="إنشاء عقد"
            columns={columns}
            data={contracts}
            loading={isLoading}
            emptyMessage="لم يتم العثور على أي عقود إيجار مسجلة."
        >
            <ContractDialog
                open={crud.isFormOpen}
                onOpenChange={crud.onOpenChange}
                initialData={crud.selectedItem || undefined}
                mode={crud.isEditing ? "edit" : "create"}
                onSuccess={crud.closeForm}
                propertyId={propertyId}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="lease-contract"
                entityId={crud.viewerTarget?.contractID || 0}
                title={`وثائق العقد: ${crud.viewerTarget?.tenantName} - ${crud.viewerTarget?.propertyName}`}
            />

            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
