import { useQuery, keepPreviousData } from "@tanstack/react-query";
import {
    Activity,
    BadgeCheck,
    Clock,
    CreditCard,
    DollarSign,
    FileSearch,
    FileText,
    FolderCheck,
    FolderOpen,
    Scale,
    Shield,
    TrendingUp,
    UserCheck,
    Users,
    Wallet,
} from "lucide-react";
import { useMemo } from "react";

import { accountingApi } from "@/api/accounting";
import api from "@/api/axios";
import { pensionStatsApi, performanceStatsApi, recentActivitiesApi } from "@/api/pension";
import { ReportBarChart, ReportLineChart, ReportPieChart } from "@/components/common/charts";
import { Card, CardContent, ErrorState, Heading, PageLayout, Skeleton, StatCard, Text } from "@/components/ui";
import { chartColors } from "@/lib/chartColors";
import { cn, formatCurrency, formatNumber } from "@/lib/utils";
import type { AccountingDashboardDto } from "@/types/accounting";
import type { DashboardStats } from "@/types/dashboard";
import type { PensionStatistics, PerformanceStatistics } from "@/types/pension";
import type { RecentActivity } from "@/types/dashboard";

// ─── Reusable Components ───────────────────────────────

interface ProgressBarProps {
    label: string;
    value: number;
    total: number;
    color: string;
}

function ProgressBar({ label, value, total, color }: ProgressBarProps) {
    const pct = total > 0 ? Math.round((value / total) * 100) : 0;
    return (
        <div className="space-y-1.5">
            <div className="flex items-center justify-between text-xs">
                <span className="font-medium text-muted-foreground">{label}</span>
                <span className="font-bold text-foreground">
                    {formatNumber(value)} <span className="text-muted-foreground">({pct}%)</span>
                </span>
            </div>
            <div className="h-2 bg-muted rounded-full overflow-hidden">
                <div
                    className="h-full rounded-full transition-all duration-700"
                    style={{ width: `${pct}%`, backgroundColor: color }}
                />
            </div>
        </div>
    );
}

// ─── Activity Table ────────────────────────────────────

function ActivityTable({ entries, isLoading }: { entries: RecentActivity[] | undefined; isLoading?: boolean }) {
    const actionStyle = (action: string) => {
        switch (action) {
            case "Create":
                return "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400";
            case "Update":
                return "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400";
            case "Delete":
                return "bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-400";
            case "StatusChange":
                return "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400";
            default:
                return "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400";
        }
    };

    const actionLabel = (action: string) => {
        switch (action) {
            case "Create": return "إنشاء";
            case "Update": return "تعديل";
            case "Delete": return "حذف";
            case "StatusChange": return "تغيير حالة";
            case "Login": return "دخول";
            case "Logout": return "خروج";
            default: return action;
        }
    };

    if (isLoading) {
        return (
            <div className="space-y-2.5">
                {Array.from({ length: 5 }).map((_, i) => (
                    <div key={i} className="flex items-center gap-3 p-3 rounded-xl bg-muted/30">
                        <Skeleton className="h-6 w-16 rounded-md" />
                        <div className="space-y-1.5 flex-1">
                            <Skeleton className="h-4 w-3/4" />
                            <Skeleton className="h-3 w-1/2" />
                        </div>
                    </div>
                ))}
            </div>
        );
    }

    if (!entries || entries.length === 0) {
        return <Text variant="description" className="text-center py-8">لا توجد نشاطات حديثة</Text>;
    }

    return (
        <div className="space-y-2.5">
            {entries.map((item) => (
                <div
                    key={item.logID}
                    className="flex items-center justify-between p-3 rounded-xl bg-muted/30 border border-border/40 hover:bg-muted/50 transition-colors"
                >
                    <div className="flex items-center gap-3">
                        <div className={cn("px-2 py-0.5 rounded-md text-caption font-bold", actionStyle(item.action))}>
                            {actionLabel(item.action)}
                        </div>
                        <div>
                            <Text variant="body-sm" className="font-bold text-foreground line-clamp-1">
                                {item.entityDisplayName || item.entityName || "عملية"}
                            </Text>
                            <Text variant="caption" className="flex items-center gap-1 mt-0.5">
                                <Clock className="w-2.5 h-2.5" />
                                {new Date(item.createdAt).toLocaleString("ar-YE")}
                                {item.userName && ` · ${item.userName}`}
                            </Text>
                        </div>
                    </div>
                </div>
            ))}
        </div>
    );
}

// ─── Dashboard Page ────────────────────────────────────

export function DashboardPage() {
    // الاستعلام 1: إحصائيات لوحة التحكم الأساسية والميزانية
    const { 
        data, 
        isLoading: isLoadingStats, 
        isError: isErrorStats, 
        refetch: refetchStats 
    } = useQuery<DashboardStats>({
        queryKey: ["dashboard-stats"],
        queryFn: async () => {
            const res = await api.get<{ success: boolean; data: DashboardStats }>("/dashboard/stats");
            return res.data.data;
        },
        staleTime: 5 * 60_000,
        refetchInterval: () =>
            typeof document !== "undefined" && document.visibilityState === "hidden" ? false : 5 * 60_000,
        refetchOnWindowFocus: false,
        placeholderData: keepPreviousData,
    });

    // الاستعلام 2: إحصائيات المتقاعدين والملفات
    const { 
        data: pensionStats, 
        isLoading: isLoadingPension, 
        isError: isErrorPension,
        refetch: refetchPension
    } = useQuery<PensionStatistics>({
        queryKey: ["pension-statistics"],
        queryFn: () => pensionStatsApi.getStatistics(),
        staleTime: 5 * 60_000,
        refetchOnWindowFocus: false,
        placeholderData: keepPreviousData,
    });

    // الاستعلام 3: البيانات المالية للوحة تحكم الحسابات
    const { 
        data: accountingDashboard, 
        isLoading: isLoadingAccounting, 
        isError: isErrorAccounting,
        refetch: refetchAccounting
    } = useQuery<AccountingDashboardDto>({
        queryKey: ["accounting-dashboard"],
        queryFn: () => accountingApi.getDashboard(),
        staleTime: 5 * 60_000,
        refetchOnWindowFocus: false,
        placeholderData: keepPreviousData,
    });

    // الاستعلام 4: إحصائيات الأداء الشهري
    const { 
        data: performanceStats, 
        isLoading: isLoadingPerformance, 
        isError: isErrorPerformance,
        refetch: refetchPerformance
    } = useQuery<PerformanceStatistics>({
        queryKey: ["performance-statistics"],
        queryFn: () => performanceStatsApi.getStatistics(),
        staleTime: 5 * 60_000,
        refetchOnWindowFocus: false,
        placeholderData: keepPreviousData,
    });

    // الاستعلام 5: آخر عمليات النظام
    const { 
        data: recentActivities, 
        isLoading: isLoadingActivities, 
        refetch: refetchActivities
    } = useQuery<RecentActivity[]>({
        queryKey: ["recent-activities"],
        queryFn: () => recentActivitiesApi.getRecent(),
        staleTime: 60_000,
        refetchOnWindowFocus: false,
        placeholderData: keepPreviousData,
    });

    // ─── الحسابات المشتقة الميمية (useMemo) ────────────────

    const fileStatusData = useMemo(
        () =>
            (pensionStats?.fileStatusDistribution || [])
                .filter(item => item.count > 0)
                .map((item) => ({
                    name: item.statusName,
                    value: item.count,
                    percentage: item.percentage,
                    color: item.color,
                })),
        [pensionStats?.fileStatusDistribution]
    );

    const rankData = useMemo(
        () =>
            (pensionStats?.rankDistribution || []).map((item) => ({
                name: item.rankName,
                value: item.count,
                percentage: item.percentage,
            })),
        [pensionStats?.rankDistribution]
    );

    const categoryData = useMemo(
        () =>
            (pensionStats?.categoryDistribution || []).map((item) => ({
                name: item.categoryName,
                value: item.count,
                percentage: item.percentage,
            })),
        [pensionStats?.categoryDistribution]
    );

    const governorateData = useMemo(
        () =>
            (pensionStats?.governorateDistribution || []).map((item) => ({
                name: item.governorate,
                value: item.count,
                percentage: item.percentage,
            })),
        [pensionStats?.governorateDistribution]
    );

    const budgetTotal = data?.totalBudgetAmount ?? 0;
    const budgetUsed = data?.usedBudgetAmount ?? 0;
    const budgetPctRaw = budgetTotal > 0 ? Math.round((budgetUsed / budgetTotal) * 100) : 0;
    const budgetPct = Math.min(budgetPctRaw, 100);
    const budgetRemaining = budgetTotal - budgetUsed;
    const isOverBudget = budgetRemaining < 0;

    const totalRetirees = (data && data.totalRetirees > 0) ? data.totalRetirees : (pensionStats?.totalRetirees ?? 0);
    const activeFiles = (data && data.activeFilesCount > 0) ? data.activeFilesCount : (pensionStats?.activeFiles ?? 0);
    const newFiles = pensionStats?.newFiles ?? 0;
    const underReview = pensionStats?.underReviewFiles ?? 0;

    const totalBatches = (data?.draftBatchesCount ?? 0) + (data?.approvedBatchesCount ?? 0);
    const totalJournalEntries = accountingDashboard?.totalJournalEntries ?? 0;
    const safeJournalTotal = totalJournalEntries > 0 ? totalJournalEntries : 1;

    const handleRetrySection = (section: "stats" | "pension" | "accounting" | "performance" | "activities") => {
        switch (section) {
            case "stats": refetchStats(); break;
            case "pension": refetchPension(); break;
            case "accounting": refetchAccounting(); break;
            case "performance": refetchPerformance(); break;
            case "activities": refetchActivities(); break;
        }
    };

    return (
        <PageLayout className="text-start">
            <div className="space-y-6">
                {/* ═══════════════════════════════════════════════ */}
                {/* SECTION 1: Main KPIs                           */}
                {/* ═══════════════════════════════════════════════ */}
                {isErrorStats || isErrorPension ? (
                    <ErrorState
                        message="تعذر تحميل المؤشرات الرئيسية"
                        onRetry={() => handleRetrySection("stats")}
                    />
                ) : (
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-7 gap-4">
                        <StatCard
                            title="إجمالي المتقاعدين"
                            value={totalRetirees}
                            icon={<Users className="w-4 h-4" />}
                            variant="blue"
                            loading={isLoadingStats || isLoadingPension}
                            className="p-3"
                        />
                        <StatCard
                            title="الملفات النشطة"
                            value={activeFiles}
                            icon={<FolderOpen className="w-4 h-4" />}
                            variant="emerald"
                            loading={isLoadingStats || isLoadingPension}
                            className="p-3"
                        />
                        <StatCard
                            title="قيد المراجعة"
                            value={underReview}
                            icon={<FileSearch className="w-4 h-4" />}
                            variant="purple"
                            loading={isLoadingPension}
                            className="p-3"
                        />
                        <StatCard
                            title="الملفات الجديدة"
                            value={newFiles}
                            icon={<FolderCheck className="w-4 h-4" />}
                            variant="amber"
                            loading={isLoadingPension}
                            className="p-3"
                        />
                        <StatCard
                            title="المستحقون النشطون"
                            value={pensionStats?.activeEntitled ?? 0}
                            icon={<UserCheck className="w-4 h-4" />}
                            variant="teal"
                            loading={isLoadingPension}
                            className="p-3"
                        />
                        <StatCard
                            title="الوكلاء النشطون"
                            value={pensionStats?.activeAgents ?? 0}
                            icon={<BadgeCheck className="w-4 h-4" />}
                            variant="orange"
                            loading={isLoadingPension}
                            className="p-3"
                        />
                        <StatCard
                            title="الأوصياء النشطون"
                            value={pensionStats?.activeGuardians ?? 0}
                            icon={<Shield className="w-4 h-4" />}
                            variant="muted"
                            loading={isLoadingPension}
                            className="p-3 hidden md:block"
                        />
                    </div>
                )}

                {/* ═══════════════════════════════════════════════ */}
                {/* SECTION 2: Performance + File Status            */}
                {/* ═══════════════════════════════════════════════ */}
                {isErrorPension || isErrorPerformance ? (
                    <ErrorState
                        message="تعذر تحميل الرسوم البيانية"
                        onRetry={() => handleRetrySection("pension")}
                    />
                ) : isLoadingPension || isLoadingPerformance ? (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 items-stretch">
                        <Skeleton className="h-[300px] w-full rounded-xl" />
                        <Skeleton className="h-[300px] w-full rounded-xl lg:col-span-2" />
                    </div>
                ) : (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 items-stretch">
                        <ReportPieChart
                            title="حالة الملفات"
                            data={fileStatusData}
                            nameKey="name"
                            valueKey="value"
                            showLegend
                            formatValue={(v) => formatNumber(v)}
                            minPercent={5}
                        />
                        <div className="lg:col-span-2">
                            <ReportLineChart
                                title="إحصائيات الأداء الشهري"
                                data={performanceStats?.monthlyData ?? []}
                                xKey="month"
                                lines={[
                                    { key: "retirees", color: "hsl(346, 84%, 60%)", label: "المتقاعدين" },
                                    { key: "files", color: "hsl(142, 76%, 36%)", label: "ملفات التقاعد" },
                                 
                                ]}
                                height={280}
                                area
                                showGrid
                                showLegend
                                formatValue={(v) => formatNumber(v)}
                            />
                        </div>
                    </div>
                )}

                {/* ═══════════════════════════════════════════════ */}
                {/* SECTION 3: Distribution Charts                  */}
                {/* ═══════════════════════════════════════════════ */}
                {isErrorPension ? (
                    <ErrorState
                        message="تعذر تحميل توزيعات المتقاعدين"
                        onRetry={() => handleRetrySection("pension")}
                    />
                ) : isLoadingPension ? (
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 items-stretch">
                        <Skeleton className="h-[300px] w-full rounded-xl" />
                        <Skeleton className="h-[300px] w-full rounded-xl" />
                    </div>
                ) : (
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 items-stretch">
                        <ReportPieChart
                            title="المتقاعدين حسب الفئة"
                            data={categoryData}
                            nameKey="name"
                            valueKey="value"
                            showLegend
                            formatValue={(v) => formatNumber(v)}
                            minPercent={4}
                        />
                        <ReportPieChart
                            title="المتقاعدين حسب الرتبة"
                            data={rankData}
                            nameKey="name"
                            valueKey="value"
                            showLegend
                            formatValue={(v) => formatNumber(v)}
                            minPercent={4}
                        />
                    </div>
                )}

                {/* ═══════════════════════════════════════════════ */}
                {/* SECTION 4: Governorate Distribution             */}
                {/* ═══════════════════════════════════════════════ */}
                {isErrorPension ? (
                    <ErrorState
                        message="تعذر تحميل توزيع المحافظات"
                        onRetry={() => handleRetrySection("pension")}
                    />
                ) : isLoadingPension ? (
                    <Skeleton className="h-[400px] w-full rounded-xl" />
                ) : (
                    <ReportBarChart
                        data={governorateData}
                        xKey="name"
                        yKeys={[{ key: "value", label: "عدد المتقاعدين", color: chartColors.primary }]}
                        title="توزيع المتقاعدين حسب المحافظة"
                        height={400}
                        showGrid={false}
                        showLegend={false}
                        formatValue={(v) => formatNumber(v)}
                        maxItems={12}
                    />
                )}

                {/* ═══════════════════════════════════════════════ */}
                {/* SECTION 5: Budget & Financial Details           */}
                {/* ═══════════════════════════════════════════════ */}
                {isErrorStats || isErrorAccounting ? (
                    <ErrorState
                        message="تعذر تحميل البيانات المالية"
                        onRetry={() => handleRetrySection("accounting")}
                    />
                ) : isLoadingStats || isLoadingAccounting ? (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
                        <Skeleton className="h-[200px] w-full rounded-xl" />
                        <div className="lg:col-span-2 grid grid-cols-2 gap-4">
                            <Skeleton className="h-[120px] w-full rounded-xl" />
                            <Skeleton className="h-[120px] w-full rounded-xl" />
                            <Skeleton className="h-[120px] w-full rounded-xl" />
                            <Skeleton className="h-[120px] w-full rounded-xl" />
                        </div>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
                        {/* Budget Utilization */}
                        <Card className="border border-border/60 shadow-sm hover:shadow-md transition-shadow">
                            <CardContent className="p-5">
                                <div className="flex items-center gap-3 mb-5">
                                    <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-muted text-muted-foreground">
                                        <Wallet className="w-4 h-4" />
                                    </div>
                                    <div>
                                        <Heading level="h3" className="text-sm font-bold text-foreground">استهلاك الميزانية</Heading>
                                        <Text variant="caption">نسبة الصرف من الميزانية الإجمالية</Text>
                                    </div>
                                </div>
                                <div className="space-y-4">
                                    <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                                        <div>
                                            <Text variant="caption">الميزانية الإجمالية</Text>
                                            <p className="text-lg font-black text-foreground">{formatCurrency(budgetTotal)}</p>
                                        </div>
                                        <div className="sm:text-left">
                                            <Text variant="caption">المتبقي</Text>
                                            <p className={cn(
                                                "text-lg font-black",
                                                isOverBudget ? "text-rose-600" : "text-emerald-600"
                                            )}>
                                                {formatCurrency(budgetRemaining)}
                                            </p>
                                        </div>
                                    </div>
                                    <div className="relative h-4 bg-muted rounded-full overflow-hidden">
                                        <div
                                            className={cn(
                                                "h-full rounded-full transition-all duration-1000",
                                                budgetPct > 90 ? "bg-rose-500" : budgetPct > 70 ? "bg-amber-500" : "bg-emerald-500"
                                            )}
                                            style={{ width: `${budgetPct}%` }}
                                        />
                                    </div>
                                    <div className="flex items-center justify-between text-xs text-muted-foreground">
                                        <span>المصروف: {formatCurrency(budgetUsed)}</span>
                                        <span className="font-bold text-foreground">{budgetPct}%</span>
                                        {isOverBudget && (
                                            <span className="font-bold text-rose-600">
                                                تجاوز: {formatCurrency(-budgetRemaining)}
                                            </span>
                                        )}
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Financial Summary Cards */}
                        <div className="lg:col-span-2 grid grid-cols-2 sm:grid-cols-2 gap-4">
                            <StatCard
                                title="إجمالي الإيرادات"
                                value={formatCurrency(accountingDashboard?.totalRevenue ?? 0)}
                                icon={<TrendingUp className="w-4 h-4" />}
                                variant="emerald"
                                loading={isLoadingAccounting}
                                className="p-3"
                            />
                            <StatCard
                                title="إجمالي المصروفات"
                                value={formatCurrency(accountingDashboard?.totalExpenses ?? 0)}
                                icon={<CreditCard className="w-4 h-4" />}
                                variant="rose"
                                loading={isLoadingAccounting}
                                className="p-3"
                            />
                            <StatCard
                                title="صافي الدخل"
                                value={formatCurrency(accountingDashboard?.netIncome ?? 0)}
                                icon={<DollarSign className="w-4 h-4" />}
                                variant="blue"
                                loading={isLoadingAccounting}
                                className="p-3"
                            />
                            <StatCard
                                title="القيود المحاسبية"
                                value={accountingDashboard?.totalJournalEntries ?? 0}
                                icon={<FileText className="w-4 h-4" />}
                                variant="purple"
                                loading={isLoadingAccounting}
                                className="p-3"
                            />
                        </div>
                    </div>
                )}

                {/* ═══════════════════════════════════════════════ */}
                {/* SECTION 6: Accounting + Activity Feed           */}
                {/* ═══════════════════════════════════════════════ */}
                <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
                    {/* Accounting Details */}
                    <div className="lg:col-span-2 space-y-4">
                        <Card className="border border-border/60 shadow-sm hover:shadow-md transition-shadow">
                            <CardContent className="p-5">
                                <div className="flex items-center gap-3 mb-5">
                                    <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-muted text-muted-foreground">
                                        <Scale className="w-4 h-4" />
                                    </div>
                                    <div>
                                        <Heading level="h3" className="text-sm font-bold text-foreground">تفاصيل القيود</Heading>
                                        <p className="text-caption text-muted-foreground">حالة القيود المحاسبية</p>
                                    </div>
                                </div>
                                {isErrorAccounting ? (
                                    <ErrorState
                                        message="تعذر تحميل تفاصيل القيود"
                                        onRetry={() => handleRetrySection("accounting")}
                                    />
                                ) : totalJournalEntries === 0 ? (
                                    <Text variant="description" className="text-center py-6">لا توجد قيود محاسبية</Text>
                                ) : (
                                    <div className="space-y-3">
                                        <ProgressBar
                                            label="مرحل"
                                            value={accountingDashboard?.postedEntries ?? 0}
                                            total={safeJournalTotal}
                                            color={chartColors.success}
                                        />
                                        <ProgressBar
                                            label="مسودة"
                                            value={accountingDashboard?.draftEntries ?? 0}
                                            total={safeJournalTotal}
                                            color={chartColors.warning}
                                        />
                                        <ProgressBar
                                            label="معكوس"
                                            value={accountingDashboard?.reversedEntries ?? 0}
                                            total={safeJournalTotal}
                                            color={chartColors.danger}
                                        />
                                    </div>
                                )}
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mt-4 pt-4 border-t border-border/40">
                                    <div className="text-center p-3 rounded-lg bg-muted/30">
                                        <p className="text-lg font-black text-foreground">
                                            {accountingDashboard?.openPeriods ?? 0}
                                        </p>
                                        <p className="text-caption text-muted-foreground">فترات مفتوحة</p>
                                    </div>
                                    <div className="text-center p-3 rounded-lg bg-muted/30">
                                        <p className="text-lg font-black text-foreground">
                                            {accountingDashboard?.closedPeriods ?? 0}
                                        </p>
                                        <p className="text-caption text-muted-foreground">فترات مغلقة</p>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Batch Status */}
                        <Card className="border border-border/60 shadow-sm hover:shadow-md transition-shadow">
                            <CardContent className="p-5">
                                <div className="flex items-center gap-3 mb-5">
                                    <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-muted text-muted-foreground">
                                        <CreditCard className="w-4 h-4" />
                                    </div>
                                    <div>
                                        <Heading level="h3" className="text-sm font-bold text-foreground">حالة الكشوف</Heading>
                                        <p className="text-caption text-muted-foreground">الكشوف المعتمدة والمسودة</p>
                                    </div>
                                </div>
                                {totalBatches === 0 ? (
                                    <Text variant="description" className="text-center py-6">لا توجد كشوفات</Text>
                                ) : (
                                    <div className="space-y-3">
                                        <ProgressBar
                                            label="كشوف مسودة"
                                            value={data?.draftBatchesCount ?? 0}
                                            total={totalBatches}
                                            color={chartColors.warning}
                                        />
                                        <ProgressBar
                                            label="كشوف معتمدة"
                                            value={data?.approvedBatchesCount ?? 0}
                                            total={totalBatches}
                                            color={chartColors.success}
                                        />
                                    </div>
                                )}
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mt-4 pt-4 border-t border-border/40">
                                    <div className="text-center p-3 rounded-lg bg-muted/30">
                                        <p className="text-lg font-black text-foreground">
                                            {data?.pendingSettlementsCount ?? 0}
                                        </p>
                                        <p className="text-caption text-muted-foreground">تسويات معلقة</p>
                                    </div>
                                    <div className="text-center p-3 rounded-lg bg-muted/30">
                                        <p className="text-lg font-black text-foreground">
                                            {data?.approvedSettlementsCount ?? 0}
                                        </p>
                                        <p className="text-caption text-muted-foreground">تسويات معتمدة</p>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                    </div>

                    {/* Activity Feed */}
                    <div className="lg:col-span-3">
                        <Card className="border border-border/60 shadow-sm hover:shadow-md transition-shadow">
                            <CardContent className="p-5">
                                <div className="flex items-center gap-3 mb-5">
                                    <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-muted text-muted-foreground">
                                        <Activity className="w-4 h-4" />
                                    </div>
                                    <div>
                                        <Heading level="h3" className="text-sm font-bold text-foreground">آخر النشاطات</Heading>
                                        <p className="text-caption text-muted-foreground">تتبع آخر العمليات على النظام</p>
                                    </div>
                                </div>
                                <ActivityTable
                                    entries={recentActivities}
                                    isLoading={isLoadingActivities}
                                />
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </div>
        </PageLayout>
    );
}
