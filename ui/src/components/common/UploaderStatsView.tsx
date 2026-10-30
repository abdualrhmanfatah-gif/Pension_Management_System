import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { AlertCircle, BarChart3, CheckCircle, Clock, Download, Eye, FileDown,FileText, Keyboard, Users, X, XCircle } from "lucide-react";
=======
import { BarChart3, Calendar, Clock, FileText, Eye, Users, X, Keyboard, CheckCircle, XCircle, AlertCircle, Download, FileDown } from "lucide-react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import api from "@/api/axios";
<<<<<<< HEAD
import { exportReportFile } from "@/api/reportExport";
import { Badge, Button, Card, CardContent, Heading, StatCard, Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui";
import type { CustomColumnDef } from "@/components/ui/DataTable";
import { DataTable } from "@/components/ui/DataTable";
import { DateInput, Input } from "@/components/ui/Input";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { addDaysToDateOnly, addMonthsToDateOnly, formatYemenDateTime, todayYemen } from "@/lib/date";
=======
import { Badge, Button, Card, CardContent, DateInput, Heading, StatCard, Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui";
import type { CustomColumnDef } from "@/components/ui/DataTable";
import { DataTable } from "@/components/ui/DataTable";
import { createWorkbook, buildReportHeader, buildTableHeader, styleDataCell, altRowFill, buildTotalsRow, buildReportFooter, exportWorkbook, autoFitColumns } from "@/components/domain/print/excel.utils";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
    createdAt: string;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
    const [selectedMonth, setSelectedMonth] = useState("");
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const [customDateRange, setCustomDateRange] = useState({ from: "", to: "" });
    const [selectedUser, setSelectedUser] = useState<UploaderStats | null>(null);
    const [selectedDataEntryUser, setSelectedDataEntryUser] = useState<DataEntryStats | null>(null);
    const [activeTab, setActiveTab] = useState("uploaders");
<<<<<<< HEAD
    const [isExporting, setIsExporting] = useState(false);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
<<<<<<< HEAD
        setSelectedMonth("");
        const today = todayYemen();
        const [year, month] = today.split("-");
=======
        const now = new Date();
        const today = now.toISOString().split("T")[0];
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        if (period === "today") {
            setFilter({ fromDate: today, toDate: today });
        } else if (period === "yesterday") {
<<<<<<< HEAD
            const y = addDaysToDateOnly(today, -1);
            setFilter({ fromDate: y, toDate: y });
        } else if (period === "week") {
            setFilter({ fromDate: addDaysToDateOnly(today, -7), toDate: today });
        } else if (period === "month") {
            setFilter({ fromDate: `${year}-${month}-01`, toDate: today });
        } else if (period === "year") {
            setFilter({ fromDate: `${year}-01-01`, toDate: today });
=======
            const yesterday = new Date(now);
            yesterday.setDate(yesterday.getDate() - 1);
            const y = yesterday.toISOString().split("T")[0];
            setFilter({ fromDate: y, toDate: y });
        } else if (period === "week") {
            const weekAgo = new Date(now);
            weekAgo.setDate(weekAgo.getDate() - 7);
            setFilter({ fromDate: weekAgo.toISOString().split("T")[0], toDate: today });
        } else if (period === "month") {
            const monthAgo = new Date(now);
            monthAgo.setMonth(monthAgo.getMonth() - 1);
            setFilter({ fromDate: monthAgo.toISOString().split("T")[0], toDate: today });
        } else if (period === "year") {
            const yearAgo = new Date(now);
            yearAgo.setFullYear(yearAgo.getFullYear() - 1);
            setFilter({ fromDate: yearAgo.toISOString().split("T")[0], toDate: today });
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        } else {
            setFilter({});
        }
        setCustomDateRange({ from: "", to: "" });
    }, []);

<<<<<<< HEAD
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

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const handleCustomDateApply = useCallback(() => {
        if (customDateRange.from && customDateRange.to) {
            if (customDateRange.from > customDateRange.to) {
                toast.error("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");
                return;
            }
            setSelectedPeriod("");
<<<<<<< HEAD
            setSelectedMonth("");
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            setFilter({
                fromDate: customDateRange.from,
                toDate: customDateRange.to,
            });
        }
    }, [customDateRange]);

    const formatDate = useCallback((dateString: string | null) => {
        if (!dateString) return "---";
<<<<<<< HEAD
        const out = formatYemenDateTime(dateString, {
            year: "numeric",
            month: "short",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
        return out || "---";
=======
        try {
            const d = new Date(dateString);
            if (isNaN(d.getTime())) return "---";
            return d.toLocaleDateString("ar-YE", {
                year: "numeric",
                month: "short",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit",
            });
        } catch {
            return "---";
        }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }, []);

    // ── Summary Calculations ──
    const totalUploads = stats.reduce((sum, s) => sum + s.totalUploads, 0);
    const totalUniqueFiles = stats.reduce((sum, s) => sum + s.uniqueFiles, 0);
    const totalFilesCreated = dataEntryStats.reduce((sum, s) => sum + s.totalFilesCreated, 0);
    const totalCompleted = dataEntryStats.reduce((sum, s) => sum + s.completedFiles, 0);
    const totalPending = dataEntryStats.reduce((sum, s) => sum + s.pendingFiles, 0);
    const totalRejected = dataEntryStats.reduce((sum, s) => sum + s.rejectedFiles, 0);

<<<<<<< HEAD
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
=======
    // ── Export Uploaders Stats to Excel ──
    const handleExportUploaders = useCallback(async () => {
        if (stats.length === 0) {
            toast.error("لا توجد بيانات للتصدير");
            return;
        }

        try {
            const { wb, ws } = createWorkbook("إحصائيات مدخلي الوثائق");
            const cols = 5;

            // Build header
            const dataStartRow = await buildReportHeader(ws, "إحصائيات مدخلي الوثائق", cols, [
                { label: "الفترة", value: filter.fromDate && filter.toDate ? `${filter.fromDate} - ${filter.toDate}` : "الكل" },
            ]);

            // Table headers
            buildTableHeader(ws, dataStartRow, ["#", "المستخدم", "عدد الوثائق", "عدد الملفات", "آخر رفع"]);

            // Data rows
            let currentRow = dataStartRow + 1;
            stats.forEach((stat, index) => {
                const row = ws.getRow(currentRow);
                row.height = 24;

                row.getCell(1).value = index + 1;
                styleDataCell(row.getCell(1), { align: "center" });
                altRowFill(row.getCell(1), index);

                row.getCell(2).value = stat.fullName || stat.userName;
                styleDataCell(row.getCell(2), { align: "start" });
                altRowFill(row.getCell(2), index);

                row.getCell(3).value = stat.totalUploads;
                styleDataCell(row.getCell(3), { align: "center" });
                altRowFill(row.getCell(3), index);

                row.getCell(4).value = stat.uniqueFiles;
                styleDataCell(row.getCell(4), { align: "center" });
                altRowFill(row.getCell(4), index);

                row.getCell(5).value = stat.lastUploadDate ? formatDate(stat.lastUploadDate) : "---";
                styleDataCell(row.getCell(5), { align: "center" });
                altRowFill(row.getCell(5), index);

                currentRow++;
            });

            // Totals row
            buildTotalsRow(ws, currentRow, [
                { value: "الإجمالي", bold: true, colSpan: 2 },
                undefined,
                { value: totalUploads },
                { value: totalUniqueFiles },
                undefined,
            ]);

            // Footer
            buildReportFooter(ws, cols);

            // Auto-fit columns
            autoFitColumns(ws, { 1: 6, 2: 25, 3: 15, 4: 15, 5: 20 });

            // Export
            const filename = `إحصائيات_مدخلي_الوثائق_${new Date().toISOString().split("T")[0]}`;
            await exportWorkbook(wb, filename);
            toast.success("تم التصدير بنجاح");
        } catch (error) {
            console.error("Export error:", error);
            toast.error("فشل التصدير");
        }
    }, [stats, filter, totalUploads, totalUniqueFiles, formatDate]);

    // ── Export Data Entry Stats to Excel ──
    const handleExportDataEntry = useCallback(async () => {
        if (dataEntryStats.length === 0) {
            toast.error("لا توجد بيانات للتصدير");
            return;
        }

        try {
            const { wb, ws } = createWorkbook("إحصائيات مدخلي البيانات");
            const cols = 7;

            // Build header
            const dataStartRow = await buildReportHeader(ws, "إحصائيات مدخلي البيانات", cols, [
                { label: "الفترة", value: filter.fromDate && filter.toDate ? `${filter.fromDate} - ${filter.toDate}` : "الكل" },
            ]);

            // Table headers
            buildTableHeader(ws, dataStartRow, ["#", "المستخدم", "إجمالي الملفات", "مكتملة", "قيد المعالجة", "مرفوضة", "آخر إدخال"]);

            // Data rows
            let currentRow = dataStartRow + 1;
            dataEntryStats.forEach((stat, index) => {
                const row = ws.getRow(currentRow);
                row.height = 24;

                row.getCell(1).value = index + 1;
                styleDataCell(row.getCell(1), { align: "center" });
                altRowFill(row.getCell(1), index);

                row.getCell(2).value = stat.fullName || stat.userName;
                styleDataCell(row.getCell(2), { align: "start" });
                altRowFill(row.getCell(2), index);

                row.getCell(3).value = stat.totalFilesCreated;
                styleDataCell(row.getCell(3), { align: "center" });
                altRowFill(row.getCell(3), index);

                row.getCell(4).value = stat.completedFiles;
                styleDataCell(row.getCell(4), { align: "center" });
                altRowFill(row.getCell(4), index);

                row.getCell(5).value = stat.pendingFiles;
                styleDataCell(row.getCell(5), { align: "center" });
                altRowFill(row.getCell(5), index);

                row.getCell(6).value = stat.rejectedFiles;
                styleDataCell(row.getCell(6), { align: "center" });
                altRowFill(row.getCell(6), index);

                row.getCell(7).value = stat.lastCreatedDate ? formatDate(stat.lastCreatedDate) : "---";
                styleDataCell(row.getCell(7), { align: "center" });
                altRowFill(row.getCell(7), index);

                currentRow++;
            });

            // Totals row
            buildTotalsRow(ws, currentRow, [
                { value: "الإجمالي", bold: true, colSpan: 2 },
                undefined,
                { value: totalFilesCreated },
                { value: totalCompleted },
                { value: totalPending },
                { value: totalRejected },
                undefined,
            ]);

            // Footer
            buildReportFooter(ws, cols);

            // Auto-fit columns
            autoFitColumns(ws, { 1: 6, 2: 25, 3: 15, 4: 12, 5: 14, 6: 12, 7: 20 });

            // Export
            const filename = `إحصائيات_مدخلي_البيانات_${new Date().toISOString().split("T")[0]}`;
            await exportWorkbook(wb, filename);
            toast.success("تم التصدير بنجاح");
        } catch (error) {
            console.error("Export error:", error);
            toast.error("فشل التصدير");
        }
    }, [dataEntryStats, filter, totalFilesCreated, totalCompleted, totalPending, totalRejected, formatDate]);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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

<<<<<<< HEAD
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
=======
    return (
        <>
        <div className="space-y-6 screen-only">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                <CardContent className="flex flex-wrap items-center gap-2 px-2 py-1 sm:px-2 sm:py-1">
                    <div className="flex flex-wrap items-center gap-1.5">
=======
                <CardContent className="p-4">
                    <div className="flex flex-wrap items-center gap-3">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <span className="text-sm font-medium text-muted-foreground">الفترة:</span>

                        <Button
                            variant={selectedPeriod === "today" ? "primary" : "outline"}
                            size="sm"
<<<<<<< HEAD
                            className="min-h-0 h-8 px-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            onClick={() => handlePeriodChange("today")}
                        >
                            اليوم
                        </Button>
                        <Button
                            variant={selectedPeriod === "yesterday" ? "primary" : "outline"}
                            size="sm"
<<<<<<< HEAD
                            className="min-h-0 h-8 px-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            onClick={() => handlePeriodChange("yesterday")}
                        >
                            أمس
                        </Button>
                        <Button
                            variant={selectedPeriod === "week" ? "primary" : "outline"}
                            size="sm"
<<<<<<< HEAD
                            className="min-h-0 h-8 px-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            onClick={() => handlePeriodChange("week")}
                        >
                            هذا الأسبوع
                        </Button>
                        <Button
                            variant={selectedPeriod === "month" ? "primary" : "outline"}
                            size="sm"
<<<<<<< HEAD
                            className="min-h-0 h-8 px-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            onClick={() => handlePeriodChange("month")}
                        >
                            هذا الشهر
                        </Button>
                        <Button
                            variant={selectedPeriod === "year" ? "primary" : "outline"}
                            size="sm"
<<<<<<< HEAD
                            className="min-h-0 h-8 px-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            onClick={() => handlePeriodChange("year")}
                        >
                            هذا العام
                        </Button>
                        <Button
                            variant={!selectedPeriod && !customDateRange.from ? "primary" : "outline"}
                            size="sm"
<<<<<<< HEAD
                            className="min-h-0 h-8 px-2.5"
                            onClick={() => handlePeriodChange("")}
=======
                            onClick={() => { setFilter({}); setSelectedPeriod(""); }}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        >
                            الكل
                        </Button>

<<<<<<< HEAD
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
=======
                        <div className="mr-4 flex items-center gap-2">
                            <Calendar className="w-4 h-4 text-muted-foreground" />
                            <DateInput
                                value={customDateRange.from}
                                onChange={(v) => setCustomDateRange(prev => ({ ...prev, from: v || "" }))}
                                placeholder="من"
                            />
                            <span className="text-muted-foreground">إلى</span>
                            <DateInput
                                value={customDateRange.to}
                                onChange={(v) => setCustomDateRange(prev => ({ ...prev, to: v || "" }))}
                                placeholder="إلى"
                            />
                            <Button
                                variant="secondary"
                                size="sm"
                                onClick={handleCustomDateApply}
                                disabled={!customDateRange.from || !customDateRange.to}
                            >
                                تطبيق
                            </Button>
                        </div>
                    </div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                        className="mr-auto gap-2 min-h-0 h-8 px-2.5"
                        disabled={isExporting}
                        onClick={handleExportStats}
=======
                        className="mr-auto gap-2"
                        onClick={activeTab === "uploaders" ? handleExportUploaders : handleExportDataEntry}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    >
                        <Download className="w-4 h-4" />
                        تصدير Excel
                    </Button>
                    <Button
                        variant="outline"
                        size="sm"
<<<<<<< HEAD
                        className="gap-2 min-h-0 h-8 px-2.5"
=======
                        className="gap-2"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        onClick={handlePrint}
                    >
                        <FileDown className="w-4 h-4" />
                        تصدير PDF
                    </Button>
                </TabsList>

                {/* Uploaders Tab */}
<<<<<<< HEAD
                <TabsContent value="uploaders" className="space-y-3">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-2">
=======
                <TabsContent value="uploaders" className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <StatCard
                            title="إجمالي الرفعات"
                            value={totalUploads.toLocaleString()}
                            icon={<BarChart3 className="w-5 h-5" />}
                            variant="blue"
                            isLoading={isLoading}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        />
                        <StatCard
                            title="عدد الملفات"
                            value={totalUniqueFiles.toLocaleString()}
                            icon={<FileText className="w-5 h-5" />}
                            variant="emerald"
                            isLoading={isLoading}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        />
                        <StatCard
                            title="عدد المدخلين"
                            value={stats.length}
                            icon={<Users className="w-5 h-5" />}
                            variant="amber"
                            isLoading={isLoading}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                <TabsContent value="data-entry" className="space-y-3">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-2">
=======
                <TabsContent value="data-entry" className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <StatCard
                            title="إجمالي الملفات"
                            value={totalFilesCreated.toLocaleString()}
                            icon={<FileText className="w-5 h-5" />}
                            variant="blue"
                            isLoading={loadingDataEntry}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        />
                        <StatCard
                            title="مكتملة"
                            value={totalCompleted.toLocaleString()}
                            icon={<CheckCircle className="w-5 h-5" />}
                            variant="emerald"
                            isLoading={loadingDataEntry}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        />
                        <StatCard
                            title="قيد المعالجة"
                            value={totalPending.toLocaleString()}
                            icon={<AlertCircle className="w-5 h-5" />}
                            variant="amber"
                            isLoading={loadingDataEntry}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        />
                        <StatCard
                            title="مرفوضة"
                            value={totalRejected.toLocaleString()}
                            icon={<XCircle className="w-5 h-5" />}
                            variant="destructive"
                            isLoading={loadingDataEntry}
<<<<<<< HEAD
                            className="p-2 sm:p-2.5"
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                    columns={dataEntryFileColumns}
=======
                                    columns={fileColumns}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
