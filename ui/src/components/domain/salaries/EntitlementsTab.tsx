import { useQuery, useQueryClient } from "@tanstack/react-query";
import { DollarSign,Plus, X } from "lucide-react";
import { useState } from "react";

import { type FinancialEntitlementDto,financialEntitlementsApi } from "@/api/financial-entitlements";
import {
    Badge,
    Button,
    type CustomColumnDef,
    DataTable,
    Heading,
    Text,
    useConfirm
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { formatCurrency,formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

import { EntitlementFormDialog } from "./EntitlementFormDialog";

interface EntitlementsTabProps {
    retireeId: number;
}

/**
 * تبويب الاستحقاقات المالية لمتقاعد
 */
export function EntitlementsTab({ retireeId }: EntitlementsTabProps) {
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canEdit = useHasPermission("File.Edit.FinancialData");
    const [isFormOpen, setIsFormOpen] = useState(false);

    // جلب الاستحقاقات
    const { data: response, isLoading } = useQuery({
        queryKey: ["financial-entitlements", retireeId],
        queryFn: () => financialEntitlementsApi.getByRetiree(retireeId)
    });

    const entitlements = response?.data || [];

    // طفرة إنهاء الاستحقاق
    const endMutation = useApiMutation(
        ({ id, endDate }: { id: number, endDate: string }) => financialEntitlementsApi.end(id, { endDate }),
        {
            successMessage: "تم إنهاء الاستحقاق بنجاح",
            onSuccessCallback: () => {
                queryClient.invalidateQueries({ queryKey: ["financial-entitlements", retireeId] });
            }
        }
    );

    const handleEnd = async (id: number) => {
        const ok = await confirm({
            title: "تأكيد إنهاء الاستحقاق",
            description: "هل أنت متأكد من إنهاء هذا الاستحقاق؟ سيؤدي ذلك إلى توقف صرف المعاشات لهذا الملف ما لم يتم تعريف استحقاق جديد.",
            variant: "danger"
        });
        if (ok) {
            endMutation.mutate({ id, endDate: new Date().toISOString().split("T")[0] });
        }
    };

    const columns: CustomColumnDef<FinancialEntitlementDto>[] = [
        {
            key: "amount",
            header: "المبلغ الشهري",
            render: (row) => <span className="font-mono font-bold text-primary">{formatCurrency(row.amount)}</span>,
        },
        {
            key: "effectiveDate",
            header: "تاريخ السريان",
            render: (row) => <span className="font-mono text-xs">{formatDate(row.effectiveDate)}</span>,
        },
        {
            key: "endDate",
            header: "تاريخ الانتهاء",
            render: (row) => row.endDate ? <span className="font-mono text-xs text-muted-foreground">{formatDate(row.endDate)}</span> : <span className="text-xs text-muted-foreground italic">غير محدد</span>,
        },
        {
            key: "notes",
            header: "ملاحظات",
            render: (row) => <span className="text-xs text-muted-foreground line-clamp-1 max-w-[200px] sm:max-w-[300px]" title={row.notes || ""}>{row.notes || "---"}</span>,
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isActive ? "success" : "outline"} className="font-bold">
                    {row.isActive ? "ساري" : "منتهي"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row) => row.isActive && canEdit && (
                <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleEnd(row.entitlementID)}
                    className="text-destructive hover:text-destructive hover:bg-destructive/10 px-2"
                    loading={endMutation.isPending && endMutation.variables?.id === row.entitlementID}
                >
                    <X className="w-4 h-4 me-1" /> إنهاء
                </Button>
            ),
        }
    ];

    return (
        <div className="space-y-6 animate-in fade-in duration-500">
            {ConfirmDialogElement}

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="space-y-1 text-start">
                    <Heading level="h3" className="text-lg font-black flex items-center gap-2 text-foreground tracking-tight">
                        <DollarSign className="w-5 h-5 text-emerald-600" /> الاستحقاقات المالية
                    </Heading>
                    <Text variant="caption" className="font-medium">سجل المبالغ المالية المعتمدة للصرف الشهري</Text>
                </div>

                {canEdit && (
                    <Button onClick={() => setIsFormOpen(true)} className="gap-2 rounded-2xl h-11 font-bold bg-emerald-600 hover:bg-emerald-700">
                        <Plus className="w-4 h-4" /> تعريف استحقاق جديد
                    </Button>
                )}
            </div>

            <div className="bg-card border rounded-2xl overflow-hidden">
                <DataTable
                    columns={columns}
                    data={entitlements}
                    loading={isLoading}
                    emptyMessage="لا يوجد استحقاقات مالية مسجلة حالياً."
                />
            </div>

            <EntitlementFormDialog
                open={isFormOpen}
                onClose={() => setIsFormOpen(false)}
                onSaved={() => queryClient.invalidateQueries({ queryKey: ["financial-entitlements", retireeId] })}
                retireeId={retireeId}
            />
        </div>
    );
}
