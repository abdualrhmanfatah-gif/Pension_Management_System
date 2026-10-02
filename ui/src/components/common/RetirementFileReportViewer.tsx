import { useQuery } from "@tanstack/react-query";
import { BarChart3, FileSpreadsheet, FileText, Users } from "lucide-react";
import { useCallback, useMemo, useState } from "react";

import { reportsApi } from "@/api/reports";
import type { RetirementFileReportFilter } from "@/api/retirement";
import type { NameValuePair, RetirementFileReportRow } from "@/api/retirement";
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
    Skeleton
} from "@/components/ui";
import type { CustomColumnDef } from "@/components/ui/DataTable";

interface RetirementFileReportViewerProps {
    className?: string;
}

export function RetirementFileReportViewer({ className }: RetirementFileReportViewerProps) {
    const [filter, setFilter] = useState<RetirementFileReportFilter>({
        pageNumber: 1,
        pageSize: 10
    } as RetirementFileReportFilter);

    const [activeSection, setActiveSection] = useState<string | null>(null);

    const { data: summary, isLoading: summaryLoading } = useQuery({
        queryKey: ["retirement-file-report-summary", filter],
        queryFn: () => reportsApi.getRetirementFileReportSummary(filter)
    });

    const { data: reportData, isLoading: reportLoading } = useQuery({
        queryKey: ["retirement-file-report", filter],
        queryFn: () => reportsApi.getRetirementFileReport(filter)
    });

    const handleFilterChange = useCallback((key: keyof RetirementFileReportFilter, value: string | number | undefined) => {
        setFilter(prev => ({
            ...prev,
            [key]: value || undefined,
            pageNumber: 1
        } as RetirementFileReportFilter));
    }, []);

    const handleExport = useCallback(async (format: "excel" | "pdf") => {
        try {
            await reportsApi.exportRetirementFileReport(filter, format);
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

    const items = useMemo(() => (reportData?.items || []) as RetirementFileReportRow[], [reportData]);

    const columns = useMemo<CustomColumnDef<RetirementFileReportRow>[]>(() => [
        {
            key: "fileNumber",
            header: "رقم الملف",
            accessor: (row) => row.fileNumber || "---",
            className: "text-center font-bold text-primary",
        },
        {
            key: "retireeName",
            header: "المتقاعد",
            accessor: (row) => row.retireeName || "---",
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            accessor: (row) => row.militaryNumber || "---",
            className: "text-center font-mono",
        },
        {
            key: "rankName",
            header: "الرتبة",
            accessor: (row) => row.rankName || "---",
            className: "text-center",
        },
        {
            key: "branchName",
            header: "الفرع",
            accessor: (row) => row.branchName || "---",
            className: "text-sm",
        },
        {
            key: "gender",
            header: "الجنس",
            accessor: (row) => row.gender === 2 ? "أنثى" : "ذكر",
            className: "text-center",
        },
        {
            key: "socialStatusName",
            header: "الحالة الاجتماعية",
            accessor: (row) => row.socialStatusName || "---",
            className: "text-center",
        },
        {
            key: "retirementDate",
            header: "تاريخ التقاعد",
            accessor: (row) => formatDate(row.retirementDate),
            className: "text-center",
        },
        {
            key: "statusName",
            header: "حالة الملفات",
            className: "text-center",
            render: (row) => (
                <Badge
                    variant={
                        row.statusName?.includes("نشط") ? "success" :
                        row.statusName?.includes("معلق") || row.statusName?.includes("ناقص") ? "warning" :
                        "secondary"
                    }
                >
                    {row.statusName || "---"}
                </Badge>
            ),
        },
        {
            key: "missingDocumentsCount",
            header: "مستندات ناقصة",
            accessor: (row) => row.missingDocumentsCount,
            className: "text-center",
        },
    ], [formatDate]);

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
        <div className={className}>
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                        <BarChart3 className="w-5 h-5 text-primary" />
                    </div>
                    <div>
                        <Heading level="h2">تقارير ملفات التقاعد</Heading>
                        <p className="text-sm text-muted-foreground">تحليل وتقارير شاملة لملفات التقاعد</p>
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
                            <label className="label-filter">الرتبة</label>
                            <NativeSelect
                                placeholder="كل الرتب"
                                options={
                                    summary?.byRank?.map((r: NameValuePair) => ({ label: r.name, value: r.name })) || []
                                }
                                value={filter.rankId?.toString() || ""}
                                onChange={(val) => handleFilterChange("rankId", val ? Number(val) : undefined)}
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="label-filter">الفرع</label>
                            <NativeSelect
                                placeholder="كل الفروع"
                                options={
                                    summary?.byBranch?.map((b: NameValuePair) => ({ label: b.name, value: b.name })) || []
                                }
                                value={filter.branchId?.toString() || ""}
                                onChange={(val) => handleFilterChange("branchId", val ? Number(val) : undefined)}
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="label-filter">الجنس</label>
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
                            <label className="label-filter">حالة المراجعة</label>
                            <NativeSelect
                                placeholder="الكل"
                                options={[
                                    { label: "بانتظار المراجعة", value: "1" },
                                    { label: "تمت المراجعة", value: "2" },
                                    { label: "مرفوض", value: "3" }
                                ]}
                                value={filter.reviewStatus?.toString() || ""}
                                onChange={(val) => handleFilterChange("reviewStatus", val ? Number(val) : undefined)}
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="label-filter">الحالة الاجتماعية</label>
                            <NativeSelect
                                placeholder="الكل"
                                options={
                                    summary?.bySocialStatus?.map((s: NameValuePair) => ({ label: s.name, value: s.name })) || []
                                }
                                value={filter.socialStatusId?.toString() || ""}
                                onChange={(val) => handleFilterChange("socialStatusId", val ? Number(val) : undefined)}
                            />
                        </div>
                        <div className="space-y-2">
                            <label className="label-filter">حالة الملفات</label>
                            <NativeSelect
                                placeholder="الكل"
                                options={
                                    summary?.byStatus?.map((s: NameValuePair) => ({ label: s.name, value: s.name })) || []
                                }
                                value={filter.statusId?.toString() || ""}
                                onChange={(val) => handleFilterChange("statusId", val ? Number(val) : undefined)}
                            />
                        </div>
                    </div>
                </CardContent>
            </Card>

            {/* Detailed Table */}
            <Card className="mb-6">
                <CardHeader className="pb-3">
                    <div className="flex items-center justify-between">
                        <CardTitle className="text-sm font-medium">البيانات التفصيلية</CardTitle>
                        <Badge variant="outline">
                            {reportData?.totalCount || 0} سجل
                        </Badge>
                    </div>
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
                            onPageChange: (page) => setFilter(prev => ({ ...prev, pageNumber: page } as RetirementFileReportFilter)),
                        }}
                        emptyMessage="لا توجد بيانات"
                    />
                </CardContent>
            </Card>

            {/* Summary Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <SummaryCard
                    title="إجمالي الملفات"
                    value={summary?.totalCount || 0}
                    icon={<Users className="w-4 h-4 text-blue-500" />}
                />
                <SummaryCard
                    title="نشطة"
                    value={summary?.activeCount || 0}
                    icon={<BarChart3 className="w-4 h-4 text-emerald-500" />}
                    variant="success"
                />
                <SummaryCard
                    title="معلقة"
                    value={summary?.suspendedCount || 0}
                    icon={<BarChart3 className="w-4 h-4 text-amber-500" />}
                    variant="warning"
                />
                <SummaryCard
                    title="بانتظار المراجعة"
                    value={summary?.pendingReviewCount || 0}
                    icon={<FileText className="w-4 h-4 text-rose-500" />}
                    variant="destructive"
                />
            </div>

            {/* Report Sections */}
            <div className="space-y-6">
                {/* By Rank */}
                <ReportSection
                    title="حسب الرتبة"
                    data={summary?.byRank || []}
                    isActive={activeSection === "rank"}
                    onToggle={() => setActiveSection(activeSection === "rank" ? null : "rank")}
                />

                {/* By Branch */}
                <ReportSection
                    title="حسب الفرع"
                    data={summary?.byBranch || []}
                    isActive={activeSection === "branch"}
                    onToggle={() => setActiveSection(activeSection === "branch" ? null : "branch")}
                />

                {/* By Gender */}
                <ReportSection
                    title="حسب الجنس"
                    data={summary?.byGender || []}
                    isActive={activeSection === "gender"}
                    onToggle={() => setActiveSection(activeSection === "gender" ? null : "gender")}
                />

                {/* By Status */}
                <ReportSection
                    title="حسب حالة الملفات"
                    data={summary?.byStatus || []}
                    isActive={activeSection === "status"}
                    onToggle={() => setActiveSection(activeSection === "status" ? null : "status")}
                />

                {/* By Social Status */}
                <ReportSection
                    title="حسب الحالة الاجتماعية"
                    data={summary?.bySocialStatus || []}
                    isActive={activeSection === "socialStatus"}
                    onToggle={() => setActiveSection(activeSection === "socialStatus" ? null : "socialStatus")}
                />
            </div>
        </div>
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
    variant?: "default" | "success" | "warning" | "destructive";
}) {
    const variantStyles = {
        default: "bg-primary/5",
        success: "bg-emerald-500/5",
        warning: "bg-amber-500/5",
        destructive: "bg-rose-500/5"
    };

    return (
        <Card className={variantStyles[variant]}>
            <CardContent className="p-4">
                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-sm font-medium text-muted-foreground">{title}</p>
                        <p className="text-2xl font-bold mt-1">{value.toLocaleString()}</p>
                    </div>
                    <div className="w-10 h-10 rounded-lg bg-background flex items-center justify-center">
                        {icon}
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}

// Report Section Component
function ReportSection({
    title,
    data,
    isActive,
    onToggle
}: {
    title: string;
    data: NameValuePair[];
    isActive: boolean;
    onToggle: () => void;
}) {
    const total = data.reduce((sum, item) => sum + item.count, 0);

    return (
        <Card>
            <CardHeader
                className="pb-3 cursor-pointer hover:bg-muted/50 transition-colors"
                onClick={onToggle}
            >
                <div className="flex items-center justify-between">
                    <CardTitle className="text-sm font-medium">{title}</CardTitle>
                    <div className="flex items-center gap-3">
                        <Badge variant="outline">{data.length} عنصر</Badge>
                        <Badge variant="secondary">{total.toLocaleString()} إجمالي</Badge>
                    </div>
                </div>
            </CardHeader>
            {isActive && (
                <CardContent>
                    <div className="space-y-2">
                        {data.map((item) => (
                            <div key={item.name} className="flex items-center justify-between py-2 border-b last:border-0">
                                <span className="text-sm">{item.name}</span>
                                <div className="flex items-center gap-3">
                                    <div className="w-32 bg-muted rounded-full h-2">
                                        <div
                                            className="bg-primary h-2 rounded-full transition-all duration-500"
                                            style={{ width: `${total > 0 ? (item.count / total) * 100 : 0}%` }}
                                        />
                                    </div>
                                    <span className="text-sm font-mono w-12 text-left">{item.count}</span>
                                    <span className="text-xs text-muted-foreground w-12 text-left">
                                        {total > 0 ? `${Math.round((item.count / total) * 100)}%` : "0%"}
                                    </span>
                                </div>
                            </div>
                        ))}
                    </div>
                </CardContent>
            )}
        </Card>
    );
}
