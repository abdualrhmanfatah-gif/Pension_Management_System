import { useQuery } from "@tanstack/react-query";
import { TrendingDown } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import { fixedAssetApi, type DepreciationScheduleDto } from "@/api/fixedAssets";
import { Badge, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";
import { formatCurrency } from "@/lib/utils";

interface AssetDepreciationTabProps {
    assetId: number;
}

const MONTH_NAMES = [
    "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
    "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
];

export function AssetDepreciationTab({ assetId }: AssetDepreciationTabProps) {
    const { data: scheduleData, isLoading, error } = useQuery({
        queryKey: ["depreciation-schedule", assetId],
        queryFn: () => fixedAssetApi.getSchedule(assetId)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل جدول الإهلاك", { id: "depreciation-schedule" });
    }, [error]);

    const schedule = useMemo(() => {
        if (!scheduleData) return [];
        return Array.isArray(scheduleData) ? scheduleData : [];
    }, [scheduleData]);

    const columns = useMemo<CustomColumnDef<DepreciationScheduleDto>[]>(() => [
        {
            key: "periodMonth",
            header: "الشهر",
            render: (row) => (
                <span className="font-medium">{MONTH_NAMES[row.periodMonth - 1] || "—"}</span>
            )
        },
        {
            key: "periodYear",
            header: "السنة",
            render: (row) => <span className="font-mono">{row.periodYear}</span>
        },
        {
            key: "depreciationAmount",
            header: "مبلغ الإهلاك",
            render: (row) => (
                <span className="text-red-500 font-medium flex items-center gap-1 font-mono">
                    <TrendingDown className="w-3 h-3" />
                    {formatCurrency(row.depreciationAmount)}
                </span>
            )
        },
        {
            key: "accumulatedDepreciation",
            header: "الإهلاك المتراكم",
            render: (row) => (
                <span className="text-amber-600 font-mono">{formatCurrency(row.accumulatedDepreciation)}</span>
            )
        },
        {
            key: "netBookValue",
            header: "القيمة الدفترية",
            render: (row) => (
                <span className="text-emerald-600 font-bold font-mono">{formatCurrency(row.netBookValue)}</span>
            )
        },
        {
            key: "isPosted",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isPosted ? "success" : "warning"}>
                    {row.isPosted ? "مُرحّل" : "بانتظار الترحيل"}
                </Badge>
            )
        }
    ], []);

    if (error) {
        return (
            <ErrorState message="خطأ في تحميل جدول الإهلاك" />
        );
    }

    return (
        <CrudPageLayout
            title="جدول الإهلاك"
            description="سجل الإهلاك الشهري لهذا الأصل"
            columns={columns}
            data={schedule}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي إهلاك لهذا الأصل بعد."
            hideHeader
        />
    );
}
