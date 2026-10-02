import { useQuery } from "@tanstack/react-query";
import { ArrowLeftRight, Clock, Info, Plus, Shield, Trash2, User } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";

import { type ForceAuditLogDto, ministryApi } from "@/api/ministry";
import { Badge, Card, type CustomColumnDef, DataTable, ErrorState, Loader } from "@/components/ui";

interface ForceAuditTabProps {
    batchId: number;
}

const actionConfig: Record<string, { icon: React.ReactNode; variant: "success" | "info" | "destructive" | "ghost" }> = {
    "إنشاء / رفع": { icon: <Plus className="w-3 h-3" />, variant: "success" },
    "تعديل / معالجة": { icon: <ArrowLeftRight className="w-3 h-3" />, variant: "info" },
    "حذف": { icon: <Trash2 className="w-3 h-3" />, variant: "destructive" },
};

function ActionBadge({ type }: { type: string }) {
    const cfg = actionConfig[type] || { icon: <Shield className="w-3 h-3" />, variant: "ghost" as const };
    return (
        <Badge variant={cfg.variant} className="gap-1">
            {cfg.icon}
            {type}
        </Badge>
    );
}

function StatusTransition({ oldStatus, newStatus }: { oldStatus?: string; newStatus?: string }) {
    if (!oldStatus && !newStatus) return null;
    return (
        <span className="text-xs text-muted-foreground">
            <span className="text-amber-600 font-medium">{oldStatus || "—"}</span>
            <span className="mx-1.5 text-muted-foreground/50">→</span>
            <span className="text-emerald-600 font-medium">{newStatus || "—"}</span>
        </span>
    );
}

export function ForceAuditTab({ batchId }: ForceAuditTabProps) {
    const { data: auditRes, isLoading, error } = useQuery({
        queryKey: ["force", "audit", batchId],
        queryFn: () => ministryApi.getBatchAuditTrail(batchId)
    });

    const logs = auditRes?.data || [];

    useEffect(() => {
        if (error) toast.error("تعذر تحميل بيانات تدقيق القوة", { id: "force-audit" });
    }, [error]);

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center h-[60dvh] gap-4" dir="rtl">
                <Loader variant="page" text="جاري تحميل سجل التدقيق..." />
            </div>
        );
    }

    if (error) {
        return <ErrorState message="تعذر تحميل بيانات تدقيق القوة" />;
    }

    const columns: CustomColumnDef<ForceAuditLogDto>[] = [
        {
            key: "actionDate",
            header: "التاريخ والوقت",
            className: "w-44",
            render: (row) => (
                <div className="flex items-center gap-2 text-xs">
                    <Clock className="w-3 h-3 text-muted-foreground shrink-0" />
                    <span>{new Date(row.actionDate).toLocaleString("ar-YE")}</span>
                </div>
            )
        },
        {
            key: "userName",
            header: "المستخدم",
            className: "w-36",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <div className="w-6 h-6 rounded-full bg-primary/10 flex items-center justify-center text-primary shrink-0">
                        <User className="w-3 h-3" />
                    </div>
                    <span className="text-sm font-medium truncate">{row.userName}</span>
                </div>
            )
        },
        {
            key: "actionType",
            header: "نوع الإجراء",
            className: "w-40",
            render: (row) => <ActionBadge type={row.actionType} />
        },
        {
            key: "statusTransition",
            header: "تغيير الحالة",
            className: "w-48",
            render: (row) => <StatusTransition oldStatus={row.oldStatus} newStatus={row.newStatus} />
        },
        {
            key: "notes",
            header: "التفاصيل",
            render: (row) => (
                <div className="text-xs text-muted-foreground leading-relaxed" title={row.notes}>
                    {row.notes || "—"}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-4">
            <div className="flex items-center gap-2 p-3 bg-blue-50/50 rounded-xl border border-blue-100 text-blue-800">
                <Info className="w-4 h-4 shrink-0" />
                <p className="text-xs font-medium">سجل الحوكمة: يتتبع كافة العمليات التي تمت على هذا الكشف والحالات التابعة له لضمان الشفافية.</p>
            </div>

            <Card className="">
                <DataTable
                    columns={columns}
                    data={logs}
                    loading={isLoading}
                    emptyMessage="لا يوجد سجل عمليات لهذا الكشف حتى الآن."
                />
            </Card>
        </div>
    );
}
