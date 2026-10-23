import { useQuery } from "@tanstack/react-query";
import { AlertCircle, BarChart3, CheckCircle, Clock, Download, Eye, FileDown,FileText, Keyboard, Users, X, XCircle } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import api from "@/api/axios";
import { exportReportFile } from "@/api/reportExport";
import { Badge, Button, Card, CardContent, Heading, StatCard, Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui";
import type { CustomColumnDef } from "@/components/ui/DataTable";
import { DataTable } from "@/components/ui/DataTable";
import { DateInput, Input } from "@/components/ui/Input";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { addDaysToDateOnly, addMonthsToDateOnly, formatYemenDateTime, todayYemen } from "@/lib/date";

interface UploaderStats {
    userID: number;
    userName: string;
    fullName: string;
    totalUploads: number;
    uniqueFiles: number;
    lastUploadDate: string | null;
}

interface DataEntryStats {
    userID: number;
    userName: string;
    fullName: string;
    totalFilesCreated: number;
    completedFiles: number;
    pendingFiles: number;
    rejectedFiles: number;
    lastCreatedDate: string | null;
}

interface UploaderFileItem {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    createdAt: string;
    missingDocumentsCount: number;
    hasDocuments: boolean;
    lastUploadedByUserName: string | null;
    lastUploadDate: string | null;
}

interface UploaderStatsFilter {
    fromDate?: string;
    toDate?: string;
}

export function UploaderStatsView() {
    const navigate = useNavigate();
    const [filter, setFilter] = useState<UploaderStatsFilter>({});
    const [selectedPeriod, setSelectedPeriod] = useState<string>("");
    const [selectedMonth, setSelectedMonth] = useState("");
    const [customDateRange, setCustomDateRange] = useState({ from: "", to: "" });
    const [selectedUser, setSelectedUser] = useState<UploaderStats | null>(null);
    const [selectedDataEntryUser, setSelectedDataEntryUser] = useState<DataEntryStats | null>(null);
    const [activeTab, setActiveTab] = useState("uploaders");
    const [isExporting, setIsExporting] = useState(false);

    const { data: stats = [], isLoading, isError: isStatsError } = useQuery({
        queryKey: ["uploader-stats", filter],
        queryFn: async () => {
            const params = new URLSearchParams();
            if (filter.fromDate) params.append("fromDate", filter.fromDate);
            if (filter.toDate) params.append("toDate", filter.toDate);

            const res = await api.get<{ data: UploaderStats[] }>(
                `/retirement-files/uploader-stats?${params.toString()}`
            );
            return res.data.data || [];
        },
    });

    const { data: dataEntryStats = [], isLoading: loadingDataEntry, isError: isDataEntryError } = useQuery({
        queryKey: ["data-entry-stats", filter],
        queryFn: async () => {
            const params = new URLSearchParams();
            if (filter.fromDate) params.append("fromDate", filter.fromDate);
            if (filter.toDate) params.append("toDate", filter.toDate);

            const res = await api.get<{ data: DataEntryStats[] }>(
                `/retirement-files/data-entry-stats?${params.toString()}`
            );
            return res.data.data || [];
        },
    });

    const { data: userFiles = [], isLoading: loadingFiles, isError: isUserFilesError } = useQuery({
        queryKey: ["uploader-files", selectedUser?.userID, filter],
        queryFn: async () => {
            if (!selectedUser) return [];
            const params = new URLSearchParams();
            params.append("uploadedBy", String(selectedUser.userID));
            params.append("pageSize", "1000");
            if (filter.fromDate) params.append("fromDate", filter.fromDate);
            if (filter.toDate) params.append("toDate", filter.toDate);

            const res = await api.get<{ data: { items: UploaderFileItem[] } }>(
                `/retirement-files?${params.toString()}`
            );
            return res.data.data?.items || [];
        },
        enabled: !!selectedUser,
    });

    const { data: dataEntryFiles = [], isLoading: loadingDataEntryFiles, isError: isDataEntryFilesError } = useQuery({
        queryKey: ["data-entry-files", selectedDataEntryUser?.userID, filter],
        queryFn: async () => {
            if (!selectedDataEntryUser) return [];
            const params = new URLSearchParams();
            params.append("page", "1");
            params.append("pageSize", "1000");
            if (filter.fromDate) params.append("fromDate", filter.fromDate);
            if (filter.toDate) params.append("toDate", filter.toDate);

            const res = await api.get<{ data: { items: UploaderFileItem[] } }>(
                `/retirement-files/data-entry-files/${selectedDataEntryUser.userID}?${params.toString()}`
            );
            return res.data.data?.items || [];
        },
        enabled: !!selectedDataEntryUser,
    });

    // ── Reset selected users when filter changes ──
    useEffect(() => {
        setSelectedUser(null);
        setSelectedDataEntryUser(null);
    }, [filter]);

    // ── Modal: body scroll lock + Escape key ──
    const isModalOpen = !!selectedUser || !!selectedDataEntryUser;

    useEffect(() => {
        if (!isModalOpen) return;

        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === "Escape") {
                setSelectedUser(null);
                setSelectedDataEntryUser(null);
            }
        };

        document.body.style.overflow = "hidden";
        document.addEventListener("keydown", handleEscape);

        return () => {
            document.body.style.overflow = "";
            document.removeEventListener("keydown", handleEscape);
        };
    }, [isModalOpen]);

    const closeModal = useCallback(() => {
        setSelectedUser(null);
        setSelectedDataEntryUser(null);
    }, []);

    const handlePeriodChange = useCallback((period: string) => {
        setSelectedPeriod(period);
        setSelectedMonth("");
        const today = todayYemen();
        const [year, month] = today.split("-");

        if (period === "today") {
            setFilter({ fromDate: today, toDate: today });
        } else if (period === "yesterday") {
            const y = addDaysToDateOnly(today, -1);
            setFilter({ fromDate: y, toDate: y });
        } else if (period === "week") {
            setFilter({ fromDate: addDaysToDateOnly(today, -7), toDate: today });
        } else if (period === "month") {
            setFilter({ fromDate: `${year}-${month}-01`, toDate: today });
        } else if (period === "year") {
            setFilter({ fromDate: `${year}-01-01`, toDate: today });
        } else {
            setFilter({});
        }
        setCustomDateRange({ from: "", to: "" });
    }, []);

    const handleMonthChange = useCallback((month: string) => {
        if (!month) {
            handlePeriodChange("");
            return;
        }
        const fromDate = `${month}-01`;
        const toDate = addDaysToDateOnly(addMonthsToDateOnly(fromDate, 1), -1);
        if (!toDate) return;

        setSelectedMonth(month);
        setSelectedPeriod("selected-month");
        setCustomDateRange({ from: fromDate, to: toDate });
        setFilter({ fromDate, toDate });
    }, [handlePeriodChange]);

    const handleCustomDateApply = useCallback(() => {
        if (customDateRange.from && customDateRange.to) {
            if (customDateRange.from > customDateRange.to) {
                toast.error("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");
                return;
            }
            setSelectedPeriod("");
            setSelectedMonth("");
            setFilter({
                fromDate: customDateRange.from,
                toDate: customDateRange.to,
            });
        }
    }, [customDateRange]);

    const formatDate = useCallback((dateString: string | null) => {
        if (!dateString) return "---";
        const out = formatYemenDateTime(dateString, {
            year: "numeric",
            month: "short",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
        return out || "---";
    }, []);

    // ── Summary Calculations ──
    const totalUploads = stats.reduce((sum, s) => sum + s.totalUploads, 0);
    const totalUniqueFiles = stats.reduce((sum, s) => sum + s.uniqueFiles, 0);
    const totalFilesCreated = dataEntryStats.reduce((sum, s) => sum + s.totalFilesCreated, 0);
    const totalCompleted = dataEntryStats.reduce((sum, s) => sum + s.completedFiles, 0);
    const totalPending = dataEntryStats.reduce((sum, s) => sum + s.pendingFiles, 0);
    const totalRejected = dataEntryStats.reduce((sum, s) => sum + s.rejectedFiles, 0);

    const handleExportStats = useCallback(async () => {
        const uploads = activeTab === "uploaders";
        if ((uploads ? stats.length : dataEntryStats.length) === 0) {
            toast.error("لا توجد بيانات للتصدير");
            return;
        }
        setIsExporting(true);
        try {
            await exportReportFile(uploads ? "/retirement-files/uploader-stats/export" : "/retirement-files/data-entry-stats/export",
                filter, uploads ? "UploaderStats.xlsx" : "DataEntryStats.xlsx");
            toast.success("تم التصدير بنجاح");
        } catch {
            toast.error("فشل التصدير");
        } finally {
            setIsExporting(false);
        }
    }, [activeTab, filter, stats.length, dataEntryStats.length]);

    const handlePrint = useCallback(() => window.print(), []);

    // ── Uploader Stats Columns ──
    const statsColumns = useMemo<CustomColumnDef<UploaderStats>[]>(
        () => [
            {
                key: "index",
                header: "#",
                className: "w-8",
                render: (_row, index) => <span className="text-muted-foreground">{index + 1}</span>,
            },
            {
                key: "user",
                header: "المستخدم",
                className: "w-[180px] text-start",
                render: (row) => (
                    <div className="text-start">
                        <div className="font-medium text-sm">{row.fullName || row.userName}</div>
                        {row.fullName && (
                            <div className="text-[10px] text-muted-foreground">@{row.userName}</div>
                        )}
                    </div>
                ),
            },
            {
                key: "totalUploads",
                header: "عدد الوثائق",
                className: "w-[100px]",
                accessor: (row) => row.totalUploads,
                render: (row) => (
                    <Badge variant="secondary" className="font-mono text-xs">
                        {row.totalUploads}
                    </Badge>
                ),
            },
            {
                key: "uniqueFiles",
                header: "عدد الملفات",
                className: "w-[100px]",
                accessor: (row) => row.uniqueFiles,
                render: (row) => (
                    <Badge variant="outline" className="font-mono text-xs">
                        {row.uniqueFiles}
                    </Badge>
                ),
            },
            {
                key: "lastUploadDate",
                header: "آخر رفع",
                className: "w-[140px]",
                render: (row) => (
                    <span className="inline-flex items-center gap-1 text-[11px] text-muted-foreground">
                        <Clock className="w-3 h-3" />
                        {formatDate(row.lastUploadDate)}
                    </span>
                ),
            },
            {
                key: "actions",
                header: "",
                className: "w-20",
                render: (row) => (
                    <Button
                        variant="ghost"
                        size="sm"
                        className="gap-1 text-primary text-xs h-7 px-2"
                        onClick={() => setSelectedUser(row)}
                    >
                        <Eye className="w-3.5 h-3.5" />
                        استعراض
                    </Button>
                ),
            },
        ],
        [formatDate]
    );

    // ── Data Entry Stats Columns ──
    const dataEntryColumns = useMemo<CustomColumnDef<DataEntryStats>[]>(
        () => [
            {
                key: "index",
                header: "#",
                className: "w-8",
                render: (_row, index) => <span className="text-muted-foreground">{index + 1}</span>,
            },
            {
                key: "user",
                header: "المستخدم",
                className: "w-[180px] text-start",
                render: (row) => (
                    <div className="text-start">
                        <div className="font-medium text-sm">{row.fullName || row.userName}</div>
                        {row.fullName && (
                            <div className="text-[10px] text-muted-foreground">@{row.userName}</div>
                        )}
                    </div>
                ),
            },
            {
                key: "totalFilesCreated",
                header: "إجمالي الملفات",
                className: "w-[100px]",
                accessor: (row) => row.totalFilesCreated,
                render: (row) => (
                    <Badge variant="secondary" className="font-mono text-xs">
                        {row.totalFilesCreated}
                    </Badge>
                ),
            },
            {
                key: "completedFiles",
                header: "مكتملة",
                className: "w-[80px]",
                accessor: (row) => row.completedFiles,
                render: (row) => (
                    <span className="inline-flex items-center gap-1 text-[11px] text-emerald-600">
                        <CheckCircle className="w-3 h-3" />
                        {row.completedFiles}
                    </span>
                ),
            },
            {
                key: "pendingFiles",
                header: "قيد المعالجة",
                className: "w-[100px]",
                accessor: (row) => row.pendingFiles,
                render: (row) => (
                    <span className="inline-flex items-center gap-1 text-[11px] text-amber-600">
                        <AlertCircle className="w-3 h-3" />
                        {row.pendingFiles}
                    </span>
                ),
            },
            {
                key: "rejectedFiles",
                header: "مرفوضة",
                className: "w-[80px]",
                accessor: (row) => row.rejectedFiles,
                render: (row) => (
                    <span className="inline-flex items-center gap-1 text-[11px] text-red-600">
                        <XCircle className="w-3 h-3" />
                        {row.rejectedFiles}
                    </span>
                ),
            },
            {
                key: "lastCreatedDate",
                header: "آخر إدخال",
                className: "w-[140px]",
                render: (row) => (
                    <span className="inline-flex items-center gap-1 text-[11px] text-muted-foreground">
                        <Clock className="w-3 h-3" />
                        {formatDate(row.lastCreatedDate)}
                    </span>
                ),
            },
            {
                key: "actions",
                header: "",
                className: "w-20",
                render: (row) => (
                    <Button
                        variant="ghost"
                        size="sm"
                        className="gap-1 text-primary text-xs h-7 px-2"
                        onClick={() => setSelectedDataEntryUser(row)}
                    >
                        <Eye className="w-3.5 h-3.5" />
                        استعراض
                    </Button>
                ),
            },
        ],
        [formatDate]
    );

    const fileColumns = useMemo<CustomColumnDef<UploaderFileItem>[]>(
        () => [
            {
                key: "fileNumber",
                header: "رقم الملف",
                className: "w-[100px]",
                render: (row) => (
                    <span className="font-mono font-bold text-primary text-sm">{row.fileNumber}</span>
                ),
            },
            {
                key: "retireeFullName",
                header: "المتقاعد",
                className: "w-[180px]",
                render: (row) => (
                    <span className="font-medium text-sm">{row.retireeFullName}</span>
                ),
            },
            {
                key: "missingDocumentsCount",
                header: "النواقص",
                className: "w-[90px]",
                render: (row) => (
                    !row.hasDocuments
                        ? <Badge variant="danger" className="text-[10px]">لم تُرفع</Badge>
                        : (row.missingDocumentsCount ?? 0) > 0
                            ? <Badge variant="warning" className="text-[10px]">{row.missingDocumentsCount} ناقصة</Badge>
                            : <Badge variant="success" className="text-[10px]">مكتمل</Badge>
                ),
            },
            {
                key: "lastUploadDate",
                header: "تاريخ الرفع",
                className: "w-[130px]",
                render: (row) => (
                    <span className="text-[11px] text-muted-foreground">
                        {formatDate(row.lastUploadDate)}
                    </span>
                ),
            },
            {
                key: "actions",
                header: "",
                className: "w-16",
                render: (row) => (
                    <Button
                        variant="ghost"
                        size="sm"
                        className="text-primary text-xs h-7 px-2"
                        onClick={() => navigate(`/files/${row.fileID}?tab=documents`)}
                    >
                        فتح
                    </Button>
                ),
            },
        ],
        [formatDate, navigate]
    );

    const dataEntryFileColumns = useMemo<CustomColumnDef<UploaderFileItem>[]>(
        () => fileColumns.map((column) => column.key === "lastUploadDate" ? {
            ...column,
            key: "createdAt",
            header: "تاريخ إنشاء الملف",
            render: (row: UploaderFileItem) => (
                <span className="text-[11px] text-muted-foreground">
                    {formatDate(row.createdAt)}
                </span>
            ),
        } : column),
        [fileColumns, formatDate]
    );

    return (
        <>
        <div className="space-y-3 screen-only">
            {/* Header */}
            <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                    <Users className="w-5 h-5 text-primary" />
                </div>
                <div>
                    <Heading level="h2">إحصائيات المدخلين</Heading>
                    <p className="text-sm text-muted-foreground">
                        إحصائيات مدخلي البيانات والوثائق
                    </p>
                </div>
            </div>

            {/* Period Filter */}
            <Card>
                <CardContent className="flex flex-wrap items-center gap-2 px-2 py-1 sm:px-2 sm:py-1">
                    <div className="flex flex-wrap items-center gap-1.5">
                        <span className="text-sm font-medium text-muted-foreground">الفترة:</span>

                        <Button
                            variant={selectedPeriod === "today" ? "primary" : "outline"}
                            size="sm"
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("today")}
                        >
                            اليوم
                        </Button>
                        <Button
                            variant={selectedPeriod === "yesterday" ? "primary" : "outline"}
                            size="sm"
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("yesterday")}
                        >
                            أمس
                        </Button>
                        <Button
                            variant={selectedPeriod === "week" ? "primary" : "outline"}
                            size="sm"
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("week")}
                        >
                            هذا الأسبوع
                        </Button>
                        <Button
                            variant={selectedPeriod === "month" ? "primary" : "outline"}
                            size="sm"
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("month")}
                        >
                            هذا الشهر
                        </Button>
                        <Button
                            variant={selectedPeriod === "year" ? "primary" : "outline"}
                            size="sm"
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("year")}
                        >
                            هذا العام
                        </Button>
                        <Button
                            variant={!selectedPeriod && !customDateRange.from ? "primary" : "outline"}
                            size="sm"
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("")}
                        >
                            الكل
                        </Button>

                    </div>

                        <div className="flex w-full items-center gap-1.5 sm:w-auto">
                            <label htmlFor="uploader-stats-month" className="text-xs font-medium text-muted-foreground whitespace-nowrap">الشهر والسنة</label>
                            <Input
                                id="uploader-stats-month"
                                type="month"
                                value={selectedMonth}
                                onChange={(event) => handleMonthChange(event.target.value)}
                                className="w-full sm:w-36 h-8 sm:h-8"
                            />
                        </div>
                        <div className="flex w-full items-center gap-1.5 sm:w-auto">
                            <label htmlFor="uploader-stats-from" className="text-xs font-medium text-muted-foreground whitespace-nowrap">من تاريخ</label>
                            <DateInput
                                id="uploader-stats-from"
                                pickerLabel="اختيار تاريخ البداية"
                                value={customDateRange.from}
                                onChange={(v) => setCustomDateRange(prev => ({ ...prev, from: v || "" }))}
                                className="w-full sm:w-44"
                            />
                        </div>
                        <div className="flex w-full items-center gap-1.5 sm:w-auto">
                            <label htmlFor="uploader-stats-to" className="text-xs font-medium text-muted-foreground whitespace-nowrap">إلى تاريخ</label>
                            <DateInput
                                id="uploader-stats-to"
                                pickerLabel="اختيار تاريخ النهاية"
                                value={customDateRange.to}
                                onChange={(v) => setCustomDateRange(prev => ({ ...prev, to: v || "" }))}
                                className="w-full sm:w-44"
                            />
                        </div>
                        <Button
                            variant="secondary"
                            size="sm"
                            className="w-full sm:w-auto min-h-0 h-8 px-2.5"
                            onClick={handleCustomDateApply}
                            disabled={!customDateRange.from || !customDateRange.to}
                        >
                            تطبيق
                        </Button>
                </CardContent>
            </Card>

            {/* Tabs */}
            <Tabs value={activeTab} onValueChange={setActiveTab}>
                <TabsList className="w-full justify-start">
                    <TabsTrigger value="uploaders" className="gap-2">
                        <FileText className="w-4 h-4" />
                        مدخلي الوثائق
                    </TabsTrigger>
                    <TabsTrigger value="data-entry" className="gap-2">
                        <Keyboard className="w-4 h-4" />
                        مدخلي البيانات
                    </TabsTrigger>
                    <Button
                        variant="outline"
                        size="sm"
                        className="mr-auto gap-2 min-h-0 h-8 px-2.5"
                        disabled={isExporting}
                        onClick={handleExportStats}
                    >
                        <Download className="w-4 h-4" />
                        تصدير Excel
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
                        className="gap-2 min-h-0 h-8 px-2.5"
                        onClick={handlePrint}
                    >
                        <FileDown className="w-4 h-4" />
                        تصدير PDF
                    </Button>
                </TabsList>

                {/* Uploaders Tab */}
                <TabsContent value="uploaders" className="space-y-3">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-2">
                        <StatCard
                            title="إجمالي الرفعات"
                            value={totalUploads.toLocaleString()}
                            icon={<BarChart3 className="w-5 h-5" />}
                            variant="blue"
                            isLoading={isLoading}
                            className="p-2 sm:p-2.5"
                        />
                        <StatCard
                            title="عدد الملفات"
                            value={totalUniqueFiles.toLocaleString()}
                            icon={<FileText className="w-5 h-5" />}
                            variant="emerald"
                            isLoading={isLoading}
                            className="p-2 sm:p-2.5"
                        />
                        <StatCard
                            title="عدد المدخلين"
                            value={stats.length}
                            icon={<Users className="w-5 h-5" />}
                            variant="amber"
                            isLoading={isLoading}
                            className="p-2 sm:p-2.5"
                        />
                    </div>

                    {isStatsError ? (
                        <p className="text-center text-destructive py-8">فشل تحميل إحصائيات رفع الوثائق</p>
                    ) : (
                        <DataTable
                            columns={statsColumns}
                            data={stats}
                            loading={isLoading}
                            rowKey={(row) => row.userID}
                            emptyMessage="لا توجد بيانات للفترة المحددة"
                            onRowClick={(row) => setSelectedUser(row)}
                        />
                    )}
                </TabsContent>

                {/* Data Entry Tab */}
                <TabsContent value="data-entry" className="space-y-3">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-2">
                        <StatCard
                            title="إجمالي الملفات"
                            value={totalFilesCreated.toLocaleString()}
                            icon={<FileText className="w-5 h-5" />}
                            variant="blue"
                            isLoading={loadingDataEntry}
                            className="p-2 sm:p-2.5"
                        />
                        <StatCard
                            title="مكتملة"
                            value={totalCompleted.toLocaleString()}
                            icon={<CheckCircle className="w-5 h-5" />}
                            variant="emerald"
                            isLoading={loadingDataEntry}
                            className="p-2 sm:p-2.5"
                        />
                        <StatCard
                            title="قيد المعالجة"
                            value={totalPending.toLocaleString()}
                            icon={<AlertCircle className="w-5 h-5" />}
                            variant="amber"
                            isLoading={loadingDataEntry}
                            className="p-2 sm:p-2.5"
                        />
                        <StatCard
                            title="مرفوضة"
                            value={totalRejected.toLocaleString()}
                            icon={<XCircle className="w-5 h-5" />}
                            variant="destructive"
                            isLoading={loadingDataEntry}
                            className="p-2 sm:p-2.5"
                        />
                    </div>

                    {isDataEntryError ? (
                        <p className="text-center text-destructive py-8">فشل تحميل إحصائيات مدخلي البيانات</p>
                    ) : (
                        <DataTable
                            columns={dataEntryColumns}
                            data={dataEntryStats}
                            loading={loadingDataEntry}
                            rowKey={(row) => row.userID}
                            emptyMessage="لا توجد بيانات للفترة المحددة"
                            onRowClick={(row) => setSelectedDataEntryUser(row)}
                        />
                    )}
                </TabsContent>
            </Tabs>

            {/* Uploader Files Dialog */}
            {selectedUser && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={closeModal}>
                    <div className="bg-background rounded-2xl shadow-xl w-full max-w-4xl max-h-[80vh] flex flex-col mx-4" onClick={(e) => e.stopPropagation()}>
                        <div className="flex items-center justify-between p-4 border-b">
                            <div>
                                <Heading level="h3" className="text-lg font-bold">
                                    وثائق {selectedUser.fullName || selectedUser.userName}
                                </Heading>
                                <p className="text-sm text-muted-foreground">
                                    {selectedUser.uniqueFiles} ملف — {selectedUser.totalUploads} وثيقة
                                </p>
                            </div>
                            <Button
                                variant="ghost"
                                size="icon"
                                onClick={closeModal}
                            >
                                <X className="w-5 h-5" />
                            </Button>
                        </div>
                        <div className="flex-1 overflow-auto p-4">
                            {isUserFilesError ? (
                                <p className="text-center text-destructive py-8">فشل تحميل البيانات</p>
                            ) : (
                                <DataTable
                                    columns={fileColumns}
                                    data={userFiles}
                                    loading={loadingFiles}
                                    rowKey={(row) => row.fileID}
                                    emptyMessage="لا توجد ملفات لهذا المستخدم في الفترة المحددة"
                                />
                            )}
                        </div>
                    </div>
                </div>
            )}

            {/* Data Entry Files Dialog */}
            {selectedDataEntryUser && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={closeModal}>
                    <div className="bg-background rounded-2xl shadow-xl w-full max-w-4xl max-h-[80vh] flex flex-col mx-4" onClick={(e) => e.stopPropagation()}>
                        <div className="flex items-center justify-between p-4 border-b">
                            <div>
                                <Heading level="h3" className="text-lg font-bold">
                                    ملفات {selectedDataEntryUser.fullName || selectedDataEntryUser.userName}
                                </Heading>
                                <p className="text-sm text-muted-foreground">
                                    {selectedDataEntryUser.totalFilesCreated} ملف — {selectedDataEntryUser.completedFiles} مكتمل — {selectedDataEntryUser.pendingFiles} قيد المعالجة
                                </p>
                            </div>
                            <Button
                                variant="ghost"
                                size="icon"
                                onClick={closeModal}
                            >
                                <X className="w-5 h-5" />
                            </Button>
                        </div>
                        <div className="flex-1 overflow-auto p-4">
                            {isDataEntryFilesError ? (
                                <p className="text-center text-destructive py-8">فشل تحميل البيانات</p>
                            ) : (
                                <DataTable
                                    columns={dataEntryFileColumns}
                                    data={dataEntryFiles}
                                    loading={loadingDataEntryFiles}
                                    rowKey={(row) => row.fileID}
                                    emptyMessage="لا توجد ملفات لهذا المستخدم في الفترة المحددة"
                                />
                            )}
                        </div>
                    </div>
                </div>
            )}

        </div>

        {/* ═══════════════════ منطقة الطباعة الرسمية ═══════════════════ */}
        <div className="print-only">
            <PrintLayout
                reportName={activeTab === "uploaders" ? "إحصائيات مدخلي الوثائق" : "إحصائيات مدخلي البيانات"}
                attachments={filter.fromDate && filter.toDate ? `الفترة: ${filter.fromDate} — ${filter.toDate}` : ""}
                config={{ orientation: "landscape", pageSize: "A3" }}
            >
                {activeTab === "uploaders" ? (
                    <table className="w-full border-collapse text-[12px] text-black border border-black" dir="rtl">
                        <thead>
                            <tr className="bg-slate-200 border-b-2 border-black">
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "5%" }}>م</th>
                                <th className="px-3 py-2 border-e border-black text-start font-extrabold" style={{ width: "25%" }}>المستخدم</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "15%" }}>عدد الوثائق</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "15%" }}>عدد الملفات</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "20%" }}>آخر رفع</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stats.map((s, i) => (
                                <tr key={s.userID} className="border-b border-black last:border-b-0">
                                    <td className="px-2 py-2 border-e border-black text-center font-bold">{i + 1}</td>
                                    <td className="px-3 py-2 border-e border-black text-start font-bold">{s.fullName || s.userName}</td>
                                    <td className="px-2 py-2 border-e border-black text-center mono fb">{s.totalUploads}</td>
                                    <td className="px-2 py-2 border-e border-black text-center mono fb">{s.uniqueFiles}</td>
                                    <td className="px-2 py-2 border-e border-black text-center">{formatDate(s.lastUploadDate)}</td>
                                </tr>
                            ))}
                        </tbody>
                        <tfoot>
                            <tr className="border-t-3 double border-black">
                                <td colSpan={2} className="px-2 py-2 border-e border-black text-center font-black text-xs">الإجمالي</td>
                                <td className="px-2 py-2 border-e border-black text-center font-black mono">{totalUploads}</td>
                                <td className="px-2 py-2 border-e border-black text-center font-black mono">{totalUniqueFiles}</td>
                                <td className="px-2 py-2 border-e border-black"></td>
                            </tr>
                        </tfoot>
                    </table>
                ) : (
                    <table className="w-full border-collapse text-[12px] text-black border border-black" dir="rtl">
                        <thead>
                            <tr className="bg-slate-200 border-b-2 border-black">
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "4%" }}>م</th>
                                <th className="px-3 py-2 border-e border-black text-start font-extrabold" style={{ width: "20%" }}>المستخدم</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "12%" }}>إجمالي الملفات</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "10%" }}>مكتملة</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "12%" }}>قيد المعالجة</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "10%" }}>مرفوضة</th>
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold" style={{ width: "18%" }}>آخر إدخال</th>
                            </tr>
                        </thead>
                        <tbody>
                            {dataEntryStats.map((s, i) => (
                                <tr key={s.userID} className="border-b border-black last:border-b-0">
                                    <td className="px-2 py-2 border-e border-black text-center font-bold">{i + 1}</td>
                                    <td className="px-3 py-2 border-e border-black text-start font-bold">{s.fullName || s.userName}</td>
                                    <td className="px-2 py-2 border-e border-black text-center mono fb">{s.totalFilesCreated}</td>
                                    <td className="px-2 py-2 border-e border-black text-center mono fb">{s.completedFiles}</td>
                                    <td className="px-2 py-2 border-e border-black text-center mono fb">{s.pendingFiles}</td>
                                    <td className="px-2 py-2 border-e border-black text-center mono fb">{s.rejectedFiles}</td>
                                    <td className="px-2 py-2 border-e border-black text-center">{formatDate(s.lastCreatedDate)}</td>
                                </tr>
                            ))}
                        </tbody>
                        <tfoot>
                            <tr className="border-t-3 double border-black">
                                <td colSpan={2} className="px-2 py-2 border-e border-black text-center font-black text-xs">الإجمالي</td>
                                <td className="px-2 py-2 border-e border-black text-center font-black mono">{totalFilesCreated}</td>
                                <td className="px-2 py-2 border-e border-black text-center font-black mono">{totalCompleted}</td>
                                <td className="px-2 py-2 border-e border-black text-center font-black mono">{totalPending}</td>
                                <td className="px-2 py-2 border-e border-black text-center font-black mono">{totalRejected}</td>
                                <td className="px-2 py-2 border-e border-black"></td>
                            </tr>
                        </tfoot>
                    </table>
                )}
            </PrintLayout>
        </div>
        </>
    );
}
