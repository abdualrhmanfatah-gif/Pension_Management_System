import { useQuery } from "@tanstack/react-query";
import {
    BarChart3,
    ChevronDown,
    FileSpreadsheet,
    FileText,
    Users,
} from "lucide-react";
import { useCallback, useMemo, useState } from "react";

import { lookupApi } from "@/api/pension";
import {
    type NameValuePair,
    retirementApi,
    type RetirementFileReportFilter,
    type RetirementFileReportRow,
} from "@/api/retirement";
import {
    Badge,
    Button,
    Card,
    CardContent,
    CardHeader,
    CardTitle,
    type CustomColumnDef,
    DataTable,
    NativeSelect,
    PageHeader,
    PageLayout,
    Skeleton,
    StatCard,
    Text,
} from "@/components/ui";

export function RetirementFileReportsPage() {
    const initialFilter = useMemo<RetirementFileReportFilter>(
        () => ({ pageNumber: 1, pageSize: 10 }),
        []
    );

    const [filter, setFilter] = useState<RetirementFileReportFilter>(initialFilter);
    const [activeSection, setActiveSection] = useState<string | null>(null);
    const [exporting, setExporting] = useState<"excel" | "pdf" | null>(null);

    const { data: allBranches = [] } = useQuery({
        queryKey: ["lookup-branches"],
        queryFn: () => lookupApi.getBranches(),
    });

    const { data: allGovernorates = [] } = useQuery({
        queryKey: ["lookup-governorates"],
        queryFn: () => lookupApi.getGovernorates(),
    });

    const { data: summary, isLoading: summaryLoading } = useQuery({
        queryKey: ["retirement-file-report-summary", filter],
        queryFn: () => retirementApi.getReportSummary(filter),
    });

    const { data: reportData, isLoading: reportLoading } = useQuery({
        queryKey: ["retirement-file-report", filter],
        queryFn: () => retirementApi.getReport(filter),
    });

    const handleFilterChange = useCallback(
        (key: keyof RetirementFileReportFilter, value: string | number | undefined) => {
            setFilter((prev) => ({
                ...prev,
                [key]: value === "" || value === undefined || (typeof value === "number" && Number.isNaN(value)) ? undefined : value,
                pageNumber: 1,
            }));
        },
        []
    );

    const handleExport = useCallback(
        async (format: "excel" | "pdf") => {
            if (exporting) return;
            try {
                setExporting(format);
                const response = await retirementApi.exportReport(filter, format);
                const blob = new Blob([response.data], {
                    type: format === "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/pdf",
                });
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement("a");
                link.href = url;
                link.download = `retirement_files_report_${new Date().toISOString().split("T")[0]}.${format === "excel" ? "xlsx" : "pdf"}`;
                document.body.appendChild(link);
                link.click();
                link.remove();
                window.URL.revokeObjectURL(url);
            } catch (error) {
                console.error("Export failed:", error);
            } finally {
                setExporting(null);
            }
        },
        [exporting, filter]
    );

    const formatDate = useCallback((dateString?: string) => {
        if (!dateString) return "---";
        const date = new Date(dateString);
        if (Number.isNaN(date.getTime())) return "---";
        return new Intl.DateTimeFormat("ar", { day: "2-digit", month: "2-digit", year: "numeric" }).format(date);
    }, []);

    const items = useMemo<RetirementFileReportRow[]>(() => reportData?.items ?? [], [reportData]);

    const mergedBranches = useMemo(() => {
        const countMap = new Map((summary?.byBranch ?? []).map((item) => [item.name, item.count]));
        return allBranches.map((branch) => ({ ...branch, count: countMap.get(branch.name) ?? 0 }));
    }, [allBranches, summary?.byBranch]);

    const columns = useMemo<CustomColumnDef<RetirementFileReportRow>[]>(() => [
        { key: "fileNumber", header: "رقم الملف", accessor: (row) => row.fileNumber || "---", className: "text-center font-bold text-primary whitespace-nowrap" },
        { key: "retireeName", header: "المتقاعد", accessor: (row) => row.retireeName || "---", className: "font-medium min-w-[180px]" },
        { key: "militaryNumber", header: "الرقم العسكري", accessor: (row) => row.militaryNumber || "---", className: "text-center font-mono whitespace-nowrap" },
        { key: "rankName", header: "الرتبة", accessor: (row) => row.rankName || "---", className: "text-center whitespace-nowrap" },
        { key: "branchName", header: "الفرع", accessor: (row) => row.branchName || "---", className: "whitespace-nowrap" },
        { key: "governorateName", header: "المحافظة", accessor: (row) => row.governorateName || "---", className: "whitespace-nowrap" },
        { key: "gender", header: "الجنس", accessor: (row) => row.gender === 2 ? "أنثى" : "ذكر", className: "text-center whitespace-nowrap" },
        { key: "socialStatusName", header: "الحالة الاجتماعية", accessor: (row) => row.socialStatusName || "---", className: "text-center whitespace-nowrap" },
        { key: "retirementDate", header: "تاريخ التقاعد", accessor: (row) => formatDate(row.retirementDate), className: "text-center whitespace-nowrap" },
        {
            key: "statusName", header: "حالة الملفات", className: "text-center whitespace-nowrap",
            render: (row) => {
                const status = row.statusName || "---";
                const variant = status.includes("نشط") ? "success" : status.includes("معلق") || status.includes("ناقص") ? "warning" : "secondary";
                return <Badge variant={variant}>{status}</Badge>;
            },
        },
        { key: "missingDocumentsCount", header: "مستندات ناقصة", accessor: (row) => row.missingDocumentsCount ?? 0, className: "text-center font-bold" },
    ], [formatDate]);

    if (summaryLoading) {
        return (
            <PageLayout>
                <div className="space-y-6">
                    <Skeleton className="h-20 w-full rounded-2xl" />
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                        {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-28 rounded-xl" />)}
                    </div>
                    <Skeleton className="h-48 rounded-xl" />
                    <Skeleton className="h-[420px] rounded-xl" />
                </div>
            </PageLayout>
        );
    }

    return (
        <PageLayout>
            {/* Header */}
            <PageHeader
                title="تقارير ملفات التقاعد"
                description="تحليل شامل وإحصائيات تفصيلية لملفات التقاعد"
                icon={<BarChart3 className="h-6 w-6 text-primary" />}
                actions={
                    <>
                        <Button variant="outline" size="sm" disabled={exporting !== null} onClick={() => handleExport("excel")} className="gap-2">
                            <FileSpreadsheet className="h-4 w-4 text-emerald-600" />
                            {exporting === "excel" ? "جاري التصدير..." : "تصدير Excel"}
                        </Button>
                        <Button variant="outline" size="sm" disabled={exporting !== null} onClick={() => handleExport("pdf")} className="gap-2">
                            <FileText className="h-4 w-4 text-red-600" />
                            {exporting === "pdf" ? "جاري التصدير..." : "تصدير PDF"}
                        </Button>
                    </>
                }
            />

            {/* Summary KPIs */}
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <StatCard title="إجمالي الملفات" value={summary?.totalCount ?? 0} icon={<Users className="h-4 w-4" />} variant="blue" />
                <StatCard title="الملفات النشطة" value={summary?.activeCount ?? 0} icon={<BarChart3 className="h-4 w-4" />} variant="emerald" />
                <StatCard title="الملفات المعلقة" value={summary?.suspendedCount ?? 0} icon={<BarChart3 className="h-4 w-4" />} variant="amber" />
                <StatCard title="بانتظار المراجعة" value={summary?.pendingReviewCount ?? 0} icon={<FileText className="h-4 w-4" />} variant="rose" />
            </div>

            {/* Filters */}
            <Card>
      
                <CardContent className="p-5">
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-7">
                        <FilterField label="الفرع">
                            <NativeSelect
                                placeholder="كل الفروع"
                                options={mergedBranches.map((b) => ({ label: `${b.name} (${b.count})`, value: String(b.id) }))}
                                value={filter.branchId?.toString() ?? ""}
                                onChange={(v) => handleFilterChange("branchId", v ? Number(v) : undefined)}
                            />
                        </FilterField>
                        <FilterField label="المحافظة">
                            <NativeSelect
                                placeholder="كل المحافظات"
                                options={allGovernorates.map((g) => ({ label: g.name, value: g.name }))}
                                value={filter.governorate ?? ""}
                                onChange={(v) => handleFilterChange("governorate", v || undefined)}
                            />
                        </FilterField>
                        <FilterField label="الجنس">
                            <NativeSelect
                                placeholder="الكل"
                                options={[{ label: "ذكر", value: "1" }, { label: "أنثى", value: "2" }]}
                                value={filter.gender?.toString() ?? ""}
                                onChange={(v) => handleFilterChange("gender", v ? Number(v) : undefined)}
                            />
                        </FilterField>
                        <FilterField label="حالة المراجعة">
                            <NativeSelect
                                placeholder="الكل"
                                options={[{ label: "بانتظار المراجعة", value: "1" }, { label: "تمت المراجعة", value: "2" }, { label: "مرفوض", value: "3" }]}
                                value={filter.reviewStatus?.toString() ?? ""}
                                onChange={(v) => handleFilterChange("reviewStatus", v ? Number(v) : undefined)}
                            />
                        </FilterField>
                        <FilterField label="الرتبة">
                            <NativeSelect placeholder="كل الرتب" options={[]} value="" onChange={() => undefined} />
                        </FilterField>
                        <FilterField label="الحالة الاجتماعية">
                            <NativeSelect placeholder="كل الحالات" options={[]} value="" onChange={() => undefined} />
                        </FilterField>
                        <FilterField label="حالة الملفات">
                            <NativeSelect placeholder="كل الحالات" options={[]} value="" onChange={() => undefined} />
                        </FilterField>
                    </div>
                </CardContent>
            </Card>

            {/* Data Table */}
            <Card>
                <CardHeader className="border-b">
                    <div className="flex items-center justify-between">
                        <div>
                            <CardTitle className="text-base">بيانات ملفات التقاعد</CardTitle>
                            <p className="mt-1 text-xs text-muted-foreground">عرض الملفات المطابقة للفلاتر المحددة</p>
                        </div>
                        <Badge variant="secondary">{(reportData?.totalCount ?? 0).toLocaleString("ar")} ملف</Badge>
                    </div>
                </CardHeader>
                <CardContent className="p-0">
                    <div className="overflow-x-auto">
                        <DataTable
                            columns={columns}
                            data={items}
                            loading={reportLoading}
                            pagination={{
                                page: filter.pageNumber,
                                pageSize: filter.pageSize,
                                total: reportData?.totalCount ?? 0,
                                onPageChange: (page) => setFilter((prev) => ({ ...prev, pageNumber: page })),
                            }}
                            emptyMessage="لا توجد ملفات مطابقة للمعايير المحددة"
                        />
                    </div>
                </CardContent>
            </Card>

            {/* Report Sections */}
            <div className="space-y-4">
                <ReportSection title="حسب الرتبة" data={summary?.byRank ?? []} isActive={activeSection === "rank"} onToggle={() => setActiveSection((c) => c === "rank" ? null : "rank")} />
                <ReportSection title="حسب الفرع" data={summary?.byBranch ?? []} isActive={activeSection === "branch"} onToggle={() => setActiveSection((c) => c === "branch" ? null : "branch")} />
                <ReportSection title="حسب المحافظة" data={summary?.byGovernorate ?? []} isActive={activeSection === "governorate"} onToggle={() => setActiveSection((c) => c === "governorate" ? null : "governorate")} />
                <ReportSection title="حسب الجنس" data={summary?.byGender ?? []} isActive={activeSection === "gender"} onToggle={() => setActiveSection((c) => c === "gender" ? null : "gender")} />
                <ReportSection title="حسب حالة الملفات" data={summary?.byStatus ?? []} isActive={activeSection === "status"} onToggle={() => setActiveSection((c) => c === "status" ? null : "status")} />
                <ReportSection title="حسب الحالة الاجتماعية" data={summary?.bySocialStatus ?? []} isActive={activeSection === "socialStatus"} onToggle={() => setActiveSection((c) => c === "socialStatus" ? null : "socialStatus")} />
            </div>
        </PageLayout>
    );
}

function FilterField({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div className="space-y-2">
            <Text variant="caption" className="font-medium">{label}</Text>
            {children}
        </div>
    );
}

function ReportSection({ title, data, isActive, onToggle }: { title: string; data: NameValuePair[]; isActive: boolean; onToggle: () => void }) {
    const total = data.reduce((sum, item) => sum + (item.count ?? 0), 0);

    return (
        <Card>
            <button type="button" onClick={onToggle} className="w-full text-right" aria-expanded={isActive}>
                <CardHeader className="transition-colors hover:bg-muted/40">
                    <div className="flex items-center justify-between gap-4">
                        <div className="flex items-center gap-3">
                            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
                                <BarChart3 className="h-4 w-4 text-primary" />
                            </div>
                            <div>
                                <CardTitle className="text-sm font-semibold">{title}</CardTitle>
                                <p className="mt-1 text-xs text-muted-foreground">توزيع الملفات</p>
                            </div>
                        </div>
                        <div className="flex items-center gap-2">
                            <Badge variant="outline">{data.length.toLocaleString("ar")} عنصر</Badge>
                            <Badge variant="secondary">{total.toLocaleString("ar")}</Badge>
                            <ChevronDown className={`h-4 w-4 text-muted-foreground transition-transform ${isActive ? "rotate-180" : ""}`} />
                        </div>
                    </div>
                </CardHeader>
            </button>
            {isActive && (
                <CardContent className="border-t bg-muted/10 p-4">
                    {data.length === 0 ? (
                        <div className="py-8 text-center text-sm text-muted-foreground">لا توجد بيانات</div>
                    ) : (
                        <div className="space-y-1">
                            {data.map((item, index) => {
                                const count = item.count ?? 0;
                                const percentage = total > 0 ? (count / total) * 100 : 0;
                                return (
                                    <div key={`${item.name}-${index}`} className="rounded-lg p-3 transition-colors hover:bg-background">
                                        <div className="mb-2 flex items-center justify-between gap-4">
                                            <span className="text-sm font-medium">{item.name || "---"}</span>
                                            <div className="flex items-center gap-3">
                                                <span className="font-mono text-sm font-bold">{count.toLocaleString("ar")}</span>
                                                <span className="w-12 text-left text-xs text-muted-foreground">{Math.round(percentage)}%</span>
                                            </div>
                                        </div>
                                        <div className="h-2 overflow-hidden rounded-full bg-muted">
                                            <div className="h-full rounded-full bg-primary transition-all duration-500" style={{ width: `${Math.min(percentage, 100)}%` }} />
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </CardContent>
            )}
        </Card>
    );
}
