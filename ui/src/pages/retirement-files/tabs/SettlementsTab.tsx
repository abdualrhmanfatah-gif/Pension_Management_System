import { useQuery } from "@tanstack/react-query";
import { CheckCircle2, Gavel,XCircle } from "lucide-react";
import { useMemo } from "react";
import { useNavigate } from "react-router-dom";

import { retirementApi } from "@/api/retirement";
import type { SettlementItemDto } from "@/api/settlements";
import { Button, CrudPageLayout, type CustomColumnDef } from "@/components/ui";
import { formatDate } from "@/lib/utils";

interface SettlementsTabProps {
    retireeId: number;
}

/**
 * تبويب سجل تسويات المعاش المعتمدة للمتقاعد
 */
export function SettlementsTab({ retireeId }: SettlementsTabProps) {
    const navigate = useNavigate();

    // جلب البيانات
    const { data: items = [], isLoading } = useQuery({
        queryKey: ["settlements", retireeId],
        queryFn: () => retirementApi.getSettlements(retireeId)
    });

    const columns = useMemo<CustomColumnDef<SettlementItemDto>[]>(() => [
        {
            key: "serial",
            header: "ت",
            render: (row) => <span className="font-mono text-muted-foreground text-xs">{row.serialNumber}</span>
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row) => <span className="font-mono text-xs">{row.militaryNumber || "-"}</span>
        },
        {
            key: "name",
            header: "الاسم",
            render: (row) => (
                <div className="text-start">
                    <div className="font-bold text-sm">{row.retireeFullName}</div>
                    {row.retireeID ? (
                        <div className="text-[10px] sm:text-xs text-emerald-600 flex items-center gap-1 mt-0.5 font-medium">
                            <CheckCircle2 className="w-3 h-3" /> مطابق للنظام
                        </div>
                    ) : (
                        <div className="text-[10px] sm:text-xs text-destructive flex items-center gap-1 mt-0.5 font-medium">
                            <XCircle className="w-3 h-3" /> غير مرتبط
                        </div>
                    )}
                </div>
            )
        },
        {
            key: "ranks",
            header: "المعالجة",
            render: (row) => (
                <div className="text-start text-xs">
                    <div className="text-muted-foreground line-through opacity-60">{row.currentRankName}</div>
                    <div className="font-bold text-primary">{row.processedRankName}</div>
                </div>
            )
        },
        {
            key: "salary",
            header: "المعاش المعتمد",
            render: (row) => <span className="font-mono font-bold">{row.processedRankSalary.toLocaleString("ar-YE")} ر.ي</span>
        },
        {
            key: "decision",
            header: "القرار",
            render: (row) => (
                row.decisionID ? (
                    <Button
                        variant="ghost"
                        size="sm"
                        className="gap-1.5 font-mono text-xs min-h-[44px]"
                        onClick={(e) => { e.stopPropagation(); navigate(`/settlements/${row.decisionID}`); }}
                    >
                        <Gavel className="w-3 h-3" /> {row.decisionNumber}
                    </Button>
                ) : (
                    <span className="text-muted-foreground text-xs">—</span>
                )
            )
        },
        {
            key: "createdAt",
            header: "تاريخ الإنشاء",
            render: (row) => <span className="text-xs text-muted-foreground font-mono">{formatDate(row.createdAt)}</span>
        }
    ], [navigate]);

    return (
        <CrudPageLayout
            title="تسويات المعاش"
            description="سجل القرارات والتسويات المالية التي خضع لها المتقاعد"
            columns={columns}
            data={items}
            loading={isLoading}
            emptyMessage="لا توجد تسويات مرتبطة بهذا المتقاعد."
        />
    );
}
