import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { CheckCircle2, Gavel, Scale, XCircle } from "lucide-react";
import { useMemo } from "react";
import { useNavigate } from "react-router-dom";

import { retirementApi } from "@/api/retirement";
import type { SettlementItemDto } from "@/api/settlements";
import { Button, CrudPageLayout, type CustomColumnDef, TabHeader } from "@/components/ui";
=======
import { CheckCircle2, Gavel,XCircle } from "lucide-react";
import { useMemo } from "react";
import { useNavigate } from "react-router-dom";

import type { SettlementItemDto } from "@/api/settlements";
import { retirementApi } from "@/api/retirement";
import { Button, CrudPageLayout, type CustomColumnDef } from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                    <div className="text-muted-foreground line-through opacity-100">{row.currentRankName}</div>
=======
                    <div className="text-muted-foreground line-through opacity-60">{row.currentRankName}</div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start">
            <TabHeader
                title="تسويات المعاش التقاعدي"
                description="سجل القرارات والتسويات المالية التي خضع لها المتقاعد"
                icon={Scale}
            />

            <CrudPageLayout
                title="تسويات المعاش"
                hideHeader={true}
                columns={columns}
                data={items}
                loading={isLoading}
                emptyMessage="لا توجد تسويات مرتبطة بهذا المتقاعد."
            />
        </div>
=======
        <CrudPageLayout
            title="تسويات المعاش"
            description="سجل القرارات والتسويات المالية التي خضع لها المتقاعد"
            columns={columns}
            data={items}
            loading={isLoading}
            emptyMessage="لا توجد تسويات مرتبطة بهذا المتقاعد."
        />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    );
}
