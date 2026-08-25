import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";
import { toast } from "sonner";
import {
    AlertTriangle,
    BarChart3,
    Clock,
    FileCheck} from "lucide-react";

import { ministryApi } from "@/api/ministry";
import { Skeleton, StatCard } from "@/components/ui";

function useForceStats() {
    const query = useQuery({
        queryKey: ["force", "analytics"],
        queryFn: () => ministryApi.getDashboardStats()
    });

    useEffect(() => {
        if (query.error) toast.error("تعذر تحميل بيانات لوحة التحليلات", { id: "force-analytics" });
    }, [query.error]);

    return query;
}

// ─── الإحصائيات (في الأعلى) ──────────────────────────
export function ForceStatsSection() {
    const { data: statsRes, isLoading, error } = useForceStats();
    const stats = statsRes?.data;

    if (error) {
        return (
            <div className="flex items-center gap-2 p-4 rounded-lg bg-destructive/10 text-destructive">
                <AlertTriangle className="w-5 h-5 shrink-0" />
                <p className="text-sm font-medium">تعذر تحميل الإحصائيات</p>
            </div>
        );
    }

    if (isLoading) {
        return (
        <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                {[...Array(4)].map((_, i) => (
                    <Skeleton key={i} className="h-32 w-full rounded-xl" />
                ))}
            </div>
        );
    }

    return (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
            <div className="hidden lg:block">
                <StatCard
                    title="إجمالي الإحالات"
                    value={stats?.totalBatches || 0}
                    icon={<BarChart3 className="h-4 w-4 text-blue-500" />}
                    description="كشوفات الإحالة إلى التقاعد المستلمة"
                    trend={{ value: 12, isPositive: true }}
                />
            </div>
            <StatCard
                title="الإحالات المعالجة"
                value={stats?.processedRecords?.toLocaleString() || 0}
                icon={<FileCheck className="h-4 w-4 text-emerald-500" />}
                description="تم ربطهم بملفات تقاعد"
                variant="success"
            />
            <StatCard
                title="الإحالات المعلقة"
                value={stats?.pendingRecords?.toLocaleString() || 0}
                icon={<Clock className="h-4 w-4 text-amber-500" />}
                description="بانتظار المراجعة والمعالجة"
                variant="warning"
            />
            <div className="hidden lg:block">
                <StatCard
                    title="فجوات المطابقة"
                    value={stats?.reconciliationGaps || 0}
                    icon={<AlertTriangle className="h-4 w-4 text-rose-500" />}
                    description="تتطلب تدخل يدوي"
                    variant="destructive"
                />
            </div>
        </div>
    );
}


