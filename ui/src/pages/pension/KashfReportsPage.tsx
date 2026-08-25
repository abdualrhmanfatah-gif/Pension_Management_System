import { useQuery } from "@tanstack/react-query";
import { useCallback, useMemo, useState } from "react";
import {
    BarChart3,
    FileSpreadsheet,
    FileText,
    Users
} from "lucide-react";

import {
    discrepancyApi,
    type DiscrepancyFilter,
    type RetireeDiscrepancy
} from "@/api/discrepancy";
import {
    Badge,
    Button,
    Card,
    CardContent,
    CardHeader,
    CardTitle,
    DataTable,
    Heading,
    NativeSelect,
    PageLayout,
    Skeleton,
    Text
} from "@/components/ui";
import type { CustomColumnDef } from "@/components/ui/DataTable";
import { ReportBarChart } from "@/components/common/charts/ReportBarChart";
import { ReportPieChart } from "@/components/common/charts/ReportPieChart";

export function KashfReportsPage() {
    const [filter, setFilter] = useState<DiscrepancyFilter>({
        pageNumber: 1,
        pageSize: 10
    });

    const { data: summary, isLoading: summaryLoading } = useQuery({
        queryKey: ["kashf-report-summary", filter],
        queryFn: () => discrepancyApi.getKashfReportSummary(filter)
    });

    const { data: reportData, isLoading: reportLoading } = useQuery({
        queryKey: ["kashf-report", filter],
        queryFn: () => discrepancyApi.getKashfReport(filter)
    });

    const handleFilterChange = useCallback((key: keyof DiscrepancyFilter, value: string | number | undefined) => {
        setFilter(prev => ({
            ...prev,
            [key]: value || undefined,
            pageNumber: 1
        }));
    }, []);



    const handleExport = useCallback(async (format: "excel" | "pdf") => {
        try {
            const response = await discrepancyApi.exportKashfReport(filter, format);
            const blob = new Blob([response.data], {
                type: format === "excel"
                    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    : "application/pdf"
            });
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = `kashf_report_${new Date().toISOString().split("T")[0]}.${format === "excel" ? "xlsx" : "pdf"}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error("Export failed:", error);
        }
    }, [filter]);

    const formatDate = useCallback((dateString?: string) => {
        if (!dateString) return "---";
        try {
            const d = new Date(dateString);
            if (isNaN(d.getTime())) return "---";
            const day = String(d.getDate()).padStart(2, "0");
            const month = String(d.getMonth() + 1).padStart(2, "0");
            const year = d.getFullYear();
            return `${day}/${month}/${year}`;
        } catch {
            return "---";
        }
    }, []);

    const items = useMemo(() => (reportData?.items || []) as RetireeDiscrepancy[], [reportData]);

    const columns = useMemo<CustomColumnDef<RetireeDiscrepancy>[]>(() => [
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            accessor: (row) => row.militaryNumber || "---",
            className: "text-center font-bold",
        },
        {
            key: "fullName",
            header: "الاسم الرباعي",
            accessor: (row) => row.approvalFullName || "---",
        },
        {
            key: "rank",
            header: "الرتبة",
            accessor: (row) => row.approvalRankName || "---",
            className: "text-center",
        },
        {
            key: "unitName",
            header: "الوحدة",
            accessor: (row) => row.approvalUnitName || "---",
            className: "text-sm",
        },
        {
            key: "categoryName",
            header: "الفئة",
            accessor: (row) => row.categoryName || "---",
            className: "text-center",
        },
        {
            key: "gender",
            header: "الجنس",
            accessor: (row) => row.gender === 2 ? "أنثى" : "ذكر",
            className: "text-center",
        },
        {
            key: "retirementDate",
            header: "تاريخ الإحالة",
            accessor: (row) => formatDate(row.approvalRetirementDate),
            className: "text-center",
        },
        {
            key: "status",
            header: "الحالة",
            className: "text-center",
            render: (row) => (
                <Badge
                    variant={
                        row.fileID ? "success" :
                        row.isProcessed ? "warning" : "secondary"
                    }
                >
                    {row.fileID ? "ملف مكتمل" :
                     row.isProcessed ? "تمت المعالجة" : "كشف إحالة فقط"}
                </Badge>
            ),
        },
    ], [formatDate]);

    const genderData = useMemo(() => {
        const male = summary?.byGender?.find(g => g.name === "ذكر")?.count ?? 0;
        const female = summary?.byGender?.find(g => g.name === "أنثى")?.count ?? 0;
        return [
            ...(male > 0 ? [{ name: "ذكور", count: male }] : []),
            ...(female > 0 ? [{ name: "إناث", count: female }] : [])
        ];
    }, [summary]);

    const unitData = useMemo(
        () => (summary?.byUnit || []).filter(u => u.count > 0),
        [summary]
    );

    const rankData = useMemo(
        () => (summary?.byRank || []).filter(r => r.count > 0),
        [summary]
    );

    const categoryData = useMemo(
        () => (summary?.byCategory || []).filter(c => c.count > 0),
        [summary]
    );

    if (summaryLoading) {
        return (
            <div className="space-y-4">
                <Skeleton className="h-8 w-48" />
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                    {[...Array(4)].map((_, i) => (
                        <Skeleton key={i} className="h-24 w-full" />
                    ))}
                </div>
            </div>
        );
    }

    return (
        <PageLayout>
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                        <BarChart3 className="w-5 h-5 text-primary" />
                    </div>
                    <div>
                        <Heading level="h1">تحليل كشوفات الإحالة إلى التقاعد</Heading>
                        <p className="text-sm text-muted-foreground">التقارير التفصيلية لتحليل كشوفات الإحالة إلى التقاعد</p>
                    </div>
                </div>
                <div className="flex items-center gap-2">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleExport("excel")}
                        className="gap-2"
                    >
                        <FileSpreadsheet className="w-4 h-4" />
                        تصدير Excel
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleExport("pdf")}
                        className="gap-2"
                    >
                        <FileText className="w-4 h-4" />
                        تصدير PDF
                    </Button>
                </div>
            </div>

            {/* Filters */}
            <Card className="mb-6">

                <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
                        <div className="space-y-2">
                            <Text variant="caption">الوحدة</Text>
                            <NativeSelect
                                placeholder="كل الوحدات"
                                options={
                                    summary?.byUnit?.map(u => ({ label: u.name, value: u.name })) || []
                                }
                                value={filter.unitName || ""}
                                onChange={(val) => handleFilterChange("unitName", val)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Text variant="caption">الرتبة</Text>
                            <NativeSelect
                                placeholder="كل الرتب"
                                options={
                                    summary?.byRank?.map(r => ({ label: r.name, value: r.name })) || []
                                }
                                value={filter.rankName || ""}
                                onChange={(val) => handleFilterChange("rankName", val)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Text variant="caption">الفئة</Text>
                            <NativeSelect
                                placeholder="كل الفئات"
                                options={
                                    summary?.byCategory?.map(c => ({ label: c.name, value: c.name })) || []
                                }
                                value={filter.categoryName || ""}
                                onChange={(val) => handleFilterChange("categoryName", val)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Text variant="caption">الجنس</Text>
                            <NativeSelect
                                placeholder="الكل"
                                options={[
                                    { label: "ذكر", value: "1" },
                                    { label: "أنثى", value: "2" }
                                ]}
                                value={filter.gender?.toString() || ""}
                                onChange={(val) => handleFilterChange("gender", val ? Number(val) : undefined)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Text variant="caption">حالة المعالجة</Text>
                            <NativeSelect
                                placeholder="الكل"
                                options={[
                                    { label: "معالج", value: "processed" },
                                    { label: "معلق", value: "pending" }
                                ]}
                                value={filter.processingStatus || ""}
                                onChange={(val) => handleFilterChange("processingStatus", val)}
                            />
                        </div>
                        <div className="space-y-2">
                            <Text variant="caption">حالة الملفات</Text>
                            <NativeSelect
                                placeholder="الكل"
                                options={[
                                    { label: "ملف مكتمل", value: "file_created" },
                                    { label: "تمت المعالجة فقط", value: "processed_only" },
                                    { label: "كشف إحالة فقط", value: "referral_only" }
                                ]}
                                value={filter.fileStatus || ""}
                                onChange={(val) => handleFilterChange("fileStatus", val)}
                            />
                        </div>
                    </div>
                </CardContent>
            </Card>
      {/* Summary Cards */}
            <div className="grid grid-cols-2 md:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <SummaryCard
                    title="إجمالي السجلات"
                    value={summary?.totalCount || 0}
                    icon={<Users className="w-4 h-4 text-blue-500" />}
                />
                <SummaryCard
                    title="المعالجات"
                    value={summary?.processedCount || 0}
                    icon={<BarChart3 className="w-4 h-4 text-emerald-500" />}
                    variant="success"
                />
                <SummaryCard
                    title="المعلقة"
                    value={summary?.pendingCount || 0}
                    icon={<BarChart3 className="w-4 h-4 text-amber-500" />}
                    variant="warning"
                />
                <SummaryCard
                    title="ملفات مكتملة"
                    value={summary?.fileCreatedCount || 0}
                    icon={<FileText className="w-4 h-4 text-blue-500" />}
                    variant="info"
                />
            </div>

            {/* Charts Section */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                {genderData.length > 0 ? (
                    <ReportPieChart
                        title="التوزيع حسب الجنس"
                        data={genderData}
                        nameKey="name"
                        valueKey="count"
                        innerRadius={60}
                        height={320}
                    />
                ) : (
                    <ChartEmptyCard title="التوزيع حسب الجنس" />
                )}
                <ReportBarChart
                    title="التوزيع حسب الوحدة"
                    data={unitData.map(u => ({ name: u.name, count: u.count }))}
                    xKey="name"
                    yKeys={[{ key: "count", color: "hsl(var(--primary))", label: "عدد السجلات" }]}
                    horizontal={unitData.length > 6}
                    height={320}
                />
                {rankData.length > 0 ? (
                    <ReportPieChart
                        title="التوزيع حسب الرتبة"
                        data={rankData as unknown as Record<string, unknown>[]}
                        nameKey="name"
                        valueKey="count"
                        height={320}
                    />
                ) : (
                    <ChartEmptyCard title="التوزيع حسب الرتبة" />
                )}
                <ReportBarChart
                    title="التوزيع حسب الفئة"
                    data={categoryData.map(c => ({ name: c.name, count: c.count }))}
                    xKey="name"
                    yKeys={[{ key: "count", color: "hsl(var(--primary))", label: "عدد السجلات" }]}
                    horizontal={categoryData.length > 6}
                    height={320}
                />
            </div>

            {/* Detailed Table */}
            <Card>
                <CardHeader>
                    <CardTitle className="text-sm font-medium">
                        بيانات كشوف الإحالة
                        {reportData?.totalCount ? ` (${reportData.totalCount.toLocaleString()})` : ""}
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <DataTable
                        columns={columns}
                        data={items}
                        loading={reportLoading}
                        pagination={{
                            page: filter.pageNumber,
                            pageSize: filter.pageSize,
                            total: reportData?.totalCount || 0,
                            pageSizeOptions: [10, 25, 50, 100],
                            onPageSizeChange: (pageSize) => setFilter(prev => ({ ...prev, pageSize, pageNumber: 1 })),
                            onPageChange: (page) => setFilter(prev => ({ ...prev, pageNumber: page })),
                        }}
                        emptyMessage="لا توجد بيانات"
                    />
                </CardContent>
            </Card>
        </PageLayout>
    );
}

// Summary Card Component
function SummaryCard({
    title,
    value,
    icon,
    variant = "default"
}: {
    title: string;
    value: number;
    icon: React.ReactNode;
    variant?: "default" | "success" | "warning" | "info" | "destructive";
}) {
    const variantStyles = {
        default: "bg-primary/5",
        success: "bg-emerald-500/5",
        warning: "bg-amber-500/5",
        info: "bg-blue-500/5",
        destructive: "bg-rose-500/5"
    };

    return (
        <Card className={variantStyles[variant]}>
            <CardContent className="p-4">
                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-sm font-medium text-muted-foreground">{title}</p>
                        <p className="text-lg md:text-xl lg:text-2xl font-bold mt-1">{value.toLocaleString()}</p>
                    </div>
                    <div className="w-10 h-10 rounded-lg bg-background flex items-center justify-center">
                        {icon}
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}

// Empty state for charts with no data
function ChartEmptyCard({ title }: { title: string }) {
    return (
        <Card className="mb-6">
            <CardHeader className="pb-2">
                <CardTitle className="text-sm font-bold">{title}</CardTitle>
            </CardHeader>
            <CardContent>
                <div className="flex items-center justify-center h-[220px] text-muted-foreground text-sm">
                    لا توجد بيانات لعرضها
                </div>
            </CardContent>
        </Card>
    );
}
