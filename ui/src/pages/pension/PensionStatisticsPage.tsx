import { useQuery } from "@tanstack/react-query";
import {
<<<<<<< HEAD
    BadgeCheck,
    FileSearch,
    FolderOpen,
    Lock,
    PauseCircle,
    Shield,
    UserCheck,
    Users,
=======
    Users,
    FolderOpen,
    PauseCircle,
    Lock,
    FileSearch,
    UserCheck,
    BadgeCheck,
    Shield,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
} from "lucide-react";
import { useMemo } from "react";

import { pensionStatsApi } from "@/api/pension";
<<<<<<< HEAD
import { ReportBarChart,ReportPieChart } from "@/components/common/charts";
import { ErrorState, PageLayout, StatCard } from "@/components/ui";
import type { PensionStatistics } from "@/types/pension";
=======
import type { PensionStatistics } from "@/types/pension";
import { ErrorState, PageLayout, StatCard } from "@/components/ui";
import { ReportPieChart, ReportBarChart } from "@/components/common/charts";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function PensionStatisticsPage() {
    const { data: stats, isLoading, error } = useQuery<PensionStatistics>({
        queryKey: ["pension-statistics"],
        queryFn: () => pensionStatsApi.getStatistics(),
        staleTime: 5 * 60 * 1000,
    });

    const fileStatusData = useMemo(
        () =>
            (stats?.fileStatusDistribution || []).map((item) => ({
                name: item.statusName,
                value: item.count,
                percentage: item.percentage,
            })),
        [stats?.fileStatusDistribution]
    );

    const rankData = useMemo(
        () =>
            (stats?.rankDistribution || []).map((item) => ({
                name: item.rankName,
                value: item.count,
                percentage: item.percentage,
            })),
        [stats?.rankDistribution]
    );

    const governorateData = useMemo(
        () =>
            (stats?.governorateDistribution || []).map((item) => ({
                name: item.governorate,
                value: item.count,
                percentage: item.percentage,
            })),
        [stats?.governorateDistribution]
    );

    const categoryData = useMemo(
        () =>
            (stats?.categoryDistribution || []).map((item) => ({
                name: item.categoryName,
                value: item.count,
                percentage: item.percentage,
            })),
        [stats?.categoryDistribution]
    );

    return (
        <PageLayout>
            <div className="space-y-6">
                {error && <ErrorState message="تعذر تحميل إحصائيات التقاعد" />}

                {/* البطاقات الإحصائية الرئيسية */}
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                    <StatCard
                        title="إجمالي المتقاعدين"
                        value={stats?.totalRetirees ?? 0}
                        icon={<Users className="w-5 h-5" />}
                        variant="blue"
                        loading={isLoading}
                    />
                    <StatCard
                        title="الملفات النشطة"
                        value={stats?.activeFiles ?? 0}
                        icon={<FolderOpen className="w-5 h-5" />}
                        variant="emerald"
                        loading={isLoading}
                    />
                    <StatCard
                        title="ملفات موقوفة"
                        value={stats?.suspendedFiles ?? 0}
                        icon={<PauseCircle className="w-5 h-5" />}
                        variant="amber"
                        loading={isLoading}
                    />
                    <StatCard
                        title="ملفات مغلقة"
                        value={stats?.closedFiles ?? 0}
                        icon={<Lock className="w-5 h-5" />}
                        variant="rose"
                        loading={isLoading}
                    />
                    <StatCard
                        title="قيد المراجعة"
                        value={stats?.underReviewFiles ?? 0}
                        icon={<FileSearch className="w-5 h-5" />}
                        variant="purple"
                        loading={isLoading}
                    />
                </div>

                {/* بطاقات ثانوية */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <StatCard
                        title="الملفات الجديدة"
                        value={stats?.newFiles ?? 0}
                        icon={<FolderOpen className="w-5 h-5" />}
                        variant="blue"
                        loading={isLoading}
                    />
                    <StatCard
                        title="المستحقون النشطون"
                        value={stats?.activeEntitled ?? 0}
                        icon={<UserCheck className="w-5 h-5" />}
                        variant="emerald"
                        loading={isLoading}
                    />
                    <StatCard
                        title="الوكلاء النشطون"
                        value={stats?.activeAgents ?? 0}
                        icon={<BadgeCheck className="w-5 h-5" />}
                        variant="amber"
                        loading={isLoading}
                    />
                    <StatCard
                        title="الأوصياء النشطون"
                        value={stats?.activeGuardians ?? 0}
                        icon={<Shield className="w-5 h-5" />}
                        variant="purple"
                        loading={isLoading}
                    />
                </div>

                {/* الرسوم البيانية */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <ReportPieChart
                        title="توزيع حسب حالة الملفات"
                        data={fileStatusData}
                        nameKey="name"
                        valueKey="value"
                        innerRadius={60}
                        height={300}
                    />
                    <ReportBarChart
                        title="توزيع حسب الرتب العسكرية"
                        data={rankData}
                        xKey="name"
                        yKeys={[{ key: 'value', color: 'hsl(var(--primary))', label: 'العدد' }]}
                        horizontal
                        height={300}
                        showGrid={false}
                        showLegend={false}
                    />
                    <ReportBarChart
                        title="توزيع حسب المحافظة"
                        data={governorateData}
                        xKey="name"
                        yKeys={[{ key: 'value', color: 'hsl(var(--gov-success))', label: 'العدد' }]}
                        horizontal
                        height={300}
                        showGrid={false}
                        showLegend={false}
                    />
                    <ReportPieChart
                        title="توزيع حسب الفئة"
                        data={categoryData}
                        nameKey="name"
                        valueKey="value"
                        innerRadius={60}
                        height={300}
                    />
                </div>
            </div>
        </PageLayout>
    );
}