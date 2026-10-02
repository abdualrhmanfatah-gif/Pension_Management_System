import { useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, DollarSign, Plus,XCircle } from "lucide-react";
import { useEffect, useMemo,useState } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { AddFinancialEntitlementDialog } from "@/components/domain/retirement/AddFinancialEntitlementDialog";
import { EndFinancialEntitlementDialog } from "@/components/domain/retirement/EndFinancialEntitlementDialog";
import { Badge, Button, Card, type CustomColumnDef, DataTable, Heading, LoadingIndicator } from "@/components/ui";
import { useCreateShortcut } from "@/hooks";
import { cn, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Types ──────────────────────────────────────────────

export interface FinancialEntitlement {
    entitlementID: number;
    retireeID: number;
    retireeFullName?: string;
    amount: number;
    effectiveDate: string;
    endDate?: string | null;
    isActive: boolean;
    notes?: string | null;
    createdAt: string;
}

interface FinancialEntitlementsTabProps {
    fileId: number;
    retireeId: number;
}

/**
 * تبويب الاستحقاقات المالية للمتقاعد
 */
export function FinancialEntitlementsTab({ retireeId }: FinancialEntitlementsTabProps) {
    const queryClient = useQueryClient();

    const canCreate = useHasPermission("File.Edit.FinancialData");
    const canEnd = useHasPermission("File.Edit.FinancialData");

    // حالة الحوارات
    const [showCreate, setShowCreate] = useState(false);
    const [endingId, setEndingId] = useState<number | null>(null);

    // جلب البيانات
    const { data: entitlements = [], isLoading, error } = useQuery({
        queryKey: ["financial-entitlements", retireeId],
        queryFn: () => retirementApi.getFinancialEntitlements(retireeId).then(data => data as unknown as FinancialEntitlement[])
    });

    const columns = useMemo<CustomColumnDef<FinancialEntitlement>[]>(() => [
        {
            key: "amount",
            header: "المبلغ",
            render: (row) => (
                <span className="font-mono font-bold text-primary text-lg">
                    {row.amount.toLocaleString("ar-YE")} <span className="text-[10px] sm:text-xs text-muted-foreground">ر.ي</span>
                </span>
            )
        },
        {
            key: "effectiveDate",
            header: "تاريخ السريان",
            render: (row) => <span className="text-sm font-mono">{formatDate(row.effectiveDate)}</span>
        },
        {
            key: "endDate",
            header: "تاريخ الانتهاء",
            render: (row) => row.endDate ? <span className="text-sm font-mono text-amber-600">{formatDate(row.endDate)}</span> : "—"
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isActive ? "success" : "outline"}>
                    {row.isActive ? "ساري" : "منتهي"}
                </Badge>
            )
        },
        {
            key: "notes",
            header: "ملاحظات",
            accessor: (row) => row.notes || "—"
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (row.isActive && canEnd) ? (
                <div className="text-center">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:bg-destructive/10 gap-1"
                        onClick={() => setEndingId(row.entitlementID)}
                    >
                        <XCircle className="w-4 h-4" /> إنهاء
                    </Button>
                </div>
            ) : null
        }
    ], [canEnd]);

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل الاستحقاقات المالية");
    }, [error]);

    const handleOpenCreate = () => setShowCreate(true);

    useCreateShortcut(handleOpenCreate, canCreate);

    if (error) return (
        <div className="py-12 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل الاستحقاقات المالية</p>
        </div>
    );

    if (isLoading) return <LoadingIndicator message="جاري تحميل المستحقات المالية..." />;

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start">
            <Card className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 sm:gap-4 bg-muted/30 p-3 sm:p-5">
                <div className="space-y-0.5 sm:space-y-1">
                    <Heading level="h3" className="font-bold flex items-center gap-1.5 sm:gap-2 text-sm sm:text-base">
                        <DollarSign className="w-4 h-4 sm:w-5 sm:h-5 text-emerald-600" /> الاستحقاقات المالية
                    </Heading>
                    <p className="text-xs sm:text-sm text-muted-foreground hidden sm:block">سجل الاستحقاقات والمبالغ المضافة للمعاش</p>
                </div>
                
                {canCreate && (
                    <Button size="sm" onClick={() => setShowCreate(true)} className="gap-1.5  h-8 sm:h-auto text-xs sm:text-sm px-3 sm:px-4">
                        <Plus className="w-3.5 h-3.5 sm:w-4 sm:h-4" /> إضافة استحقاق
                    </Button>
                )}
            </Card>

            <DataTable
                columns={columns}
                data={entitlements}
                loading={isLoading}
                emptyMessage="لا توجد استحقاقات مالية مسجلة"
                renderMobileCard={(item) => (
                    <div className="p-3 space-y-2">
                        <div className="flex items-center justify-between">
                            <span className="font-mono font-bold text-primary text-base">
                                {item.amount.toLocaleString("ar-YE")} <span className="text-[10px] text-muted-foreground">ر.ي</span>
                            </span>
                            <span className={cn("w-2 h-2 rounded-full shrink-0", item.isActive ? "bg-emerald-500" : "bg-muted-foreground")} title={item.isActive ? "ساري" : "منتهي"} />
                        </div>
                        <div className="text-[10px] text-muted-foreground space-y-0.5">
                            <div>السريان: <span className="font-mono">{formatDate(item.effectiveDate)}</span></div>
                            {item.endDate && <div>الانتهاء: <span className="font-mono text-amber-600">{formatDate(item.endDate)}</span></div>}
                            {item.notes && <div>ملاحظات: {item.notes}</div>}
                        </div>
                        {item.isActive && canEnd && (
                            <div className="flex justify-end">
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="text-destructive hover:bg-destructive/10 gap-1 min-h-[32px] h-8 text-xs px-2"
                                    onClick={() => setEndingId(item.entitlementID)}
                                >
                                    <XCircle className="w-3.5 h-3.5" /> إنهاء
                                </Button>
                            </div>
                        )}
                    </div>
                )}
            />

            <AddFinancialEntitlementDialog
                open={showCreate}
                onOpenChange={setShowCreate}
                onSaved={() => queryClient.invalidateQueries({ queryKey: ["financial-entitlements", retireeId] })}
                retireeId={retireeId}
            />

            <EndFinancialEntitlementDialog
                open={!!endingId}
                onOpenChange={(open) => !open && setEndingId(null)}
                onSaved={() => queryClient.invalidateQueries({ queryKey: ["financial-entitlements", retireeId] })}
                entitlementId={endingId}
            />
        </div>
    );
}
