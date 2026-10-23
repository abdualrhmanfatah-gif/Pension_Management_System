/**
 * استكمال النواقص — صفحة مخصصة لتوثيق ورفع الوثائق الناقصة لملفات التقاعد.
 * تعرض قائمة ملفات التقاعد مع عدد النواقص ونسبة اكتمال الوثائق
 * مع إمكانية الانتقال لتفاصيل الملف لرفع وتوثيق الوثائق
 * مع إحصائيات المدخلين وحالة التوثيق
 */

import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, CheckCircle2, ChevronDown, Eye, FileText, Printer, Search, Upload, User } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import api from "@/api/axios";
import { documentsApi } from "@/api/documents";
import { SearchDialog } from "@/components/common/SearchDialog";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import type { CustomColumnDef, PaginationConfig } from "@/components/ui/DataTable";
import { DataTable } from "@/components/ui/DataTable";
import { Input } from "@/components/ui/Input";
import { NativeSelect } from "@/components/ui/NativeSelect";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { cn, getDocumentUrl,getStatusColorClass } from "@/lib/utils";

function sanitizeHtml(str: string): string {
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

interface FileStatus {
    statusID: number;
    statusName: string;
}

interface RetirementFileItem {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    nationalID: string | null;
    rankName: string | null;
    fileStatusName: string | null;
    fileStatusColor: string | null;
    missingDocumentsCount: number;
    hasDocuments: boolean;
    lastUploadedByUserID: number | null;
    lastUploadedByUserName: string | null;
    statusID: number;
}

interface PagedResult {
    items: RetirementFileItem[];
    totalPages: number;
}

export function MissingDocsCompletionPage() {
    const navigate = useNavigate();
    const { searchTerm: search, setSearchTerm: setSearch, debouncedValue: debouncedSearch } = useDebouncedSearch("");
    const [filterMode, setFilterMode] = useState<"all" | "missing" | "uploaded" | "noDocuments">("all");
    const [statusId, setStatusId] = useState<string>("");
    const [uploadedBy, setUploadedBy] = useState<string>("");
    const [page, setPage] = useState(1);
    const [statsOpen, setStatsOpen] = useState(() => typeof window !== "undefined" ? window.innerWidth >= 768 : true);

    // ─── Fetch Statuses (React Query) ───────────────────────
    const { data: statuses = [] } = useQuery({
        queryKey: ["file-statuses"],
        queryFn: async () => {
            const res = await api.get<{ data: FileStatus[] }>("/file-statuses");
            return res.data.data || [];
        },
        staleTime: 5 * 60 * 1000,
    });

    // ─── Fetch Global Stats (React Query) ──────────────────
    const { data: stats } = useQuery({
        queryKey: ["document-upload-stats"],
        queryFn: async () => {
            const res = await api.get<{ data: { totalFiles: number; filesWithDocuments: number; filesWithoutDocuments: number; filesWithMissing: number; totalMissingDocuments: number } }>("/retirement-files/document-upload-stats");
            return res.data.data;
        },
        staleTime: 2 * 60 * 1000,
    });

    // ─── Fetch Files (React Query) ───────────────────────

    const { data: filesData, isLoading: loading } = useQuery({
        queryKey: ["retirement-files", "document-upload", page, debouncedSearch, statusId, uploadedBy],
        queryFn: async () => {
            const res = await api.get<{ data: PagedResult }>("/retirement-files", {
                params: {
                    page,
                    pageSize: 20,
                    search: debouncedSearch || undefined,
                    statusId: statusId || undefined,
                    uploadedBy: uploadedBy || undefined,
                },
            });
            return res.data.data;
        },
    });

    const files = filesData?.items || [];
    const totalPages = filesData?.totalPages || 1;

    // ─── Fetch All Uploaders (React Query) ──────────────────
    const { data: uploadersData = [] } = useQuery({
        queryKey: ["uploader-stats"],
        queryFn: async () => {
            const res = await api.get<{ data: { userID: number; fullName: string; userName: string }[] }>("/retirement-files/uploader-stats");
            return res.data.data || [];
        },
        staleTime: 5 * 60 * 1000,
    });

    const uploaders = useMemo(
        () => uploadersData.map((u) => ({ id: u.userID, name: u.fullName || u.userName })),
        [uploadersData]
    );

    // ─── Filter ────────────────────────────────────────────

    const filteredFiles = filterMode === "missing"
        ? files.filter(f => (f.missingDocumentsCount ?? 0) > 0)
        : filterMode === "uploaded"
            ? files.filter(f => f.hasDocuments === true)
            : filterMode === "noDocuments"
                ? files.filter(f => !f.hasDocuments)
                : files;

    // ─── Stats (from global endpoint) ─────────────────────

    const filesWithMissing = stats?.filesWithMissing ?? 0;
    const filesWithDocuments = stats?.filesWithDocuments ?? 0;
    const filesWithoutDocuments = stats?.filesWithoutDocuments ?? 0;
    const totalFiles = stats?.totalFiles ?? 0;

    // ─── Print All Documents ──────────────────────────────

    const handlePrintDocuments = useCallback(async (fileId: number, fileNumber: string) => {
        try {
            const documents = await documentsApi.getByFileId(fileId);
            if (!documents.length) { toast.info("لا توجد وثائق للطباعة"); return; }

            const docsHtml = documents.filter(d => d.filePath).map((doc) => {
                const url = getDocumentUrl(doc.filePath!) ?? "";
                const safeName = sanitizeHtml(doc.documentTypeName);
                const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(doc.filePath!);
                if (isImage) return `<div class="doc-page"><img src="${url}" alt="${safeName}" onerror="this.parentElement.innerHTML='<p style=color:red>فشل تحميل الصورة</p>'" /><p class="doc-title">${safeName}</p></div>`;
                const isPdf = /\.pdf$/i.test(doc.filePath!);
                if (isPdf) return `<div class="doc-page"><iframe src="${url}" title="${safeName}"></iframe><p class="doc-title">${safeName}</p></div>`;
                return `<div class="doc-page"><a href="${url}" target="_blank">${safeName} — افتح الملف</a></div>`;
            }).join("\n");

            const safeFileNumber = sanitizeHtml(fileNumber);
            const blob = new Blob([`<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head><meta charset="UTF-8"><title>طباعة وثائق ${safeFileNumber}</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:Arial,sans-serif;padding:20px;background:#fff}
.doc-page{page-break-after:always;text-align:center;margin-bottom:20px}
.doc-page:last-child{page-break-after:auto}
.doc-page img{max-width:100%;max-height:90vh;object-fit:contain}
.doc-page iframe{width:100%;height:90vh;border:none}
.doc-title{margin-top:10px;font-size:14px;color:#666}
@media print{.doc-page{page-break-after:always}.doc-page:last-child{page-break-after:auto}}
</style></head>
<body><h2 style="text-align:center;margin-bottom:20px;">وثائق الملف التقاعدي — ${safeFileNumber}</h2>
${docsHtml}
<script>
let loaded=0,total=document.querySelectorAll('img,iframe').length;
function check(){loaded++;if(loaded>=total)setTimeout(function(){window.print()},500)}
document.querySelectorAll('img').forEach(function(e){if(e.complete)check();else{e.onload=check;e.onerror=check}});
document.querySelectorAll('iframe').forEach(function(e){e.onload=check;e.onerror=check});
if(total===0)window.print();
<\/script></body></html>`], { type: "text/html" });
            const blobUrl = URL.createObjectURL(blob);
            try {
                const printWindow = window.open(blobUrl, "_blank", "width=900,height=700");
                if (!printWindow) { toast.warning("يرجى السماح للنافذة المنبثقة"); return; }
                printWindow.onload = () => URL.revokeObjectURL(blobUrl);
            } catch {
                URL.revokeObjectURL(blobUrl);
                toast.error("فشل فتح نافذة الطباعة");
            }
        } catch {
            toast.error("فشل تحميل الوثائق للطباعة");
        }
    }, []);

    // ─── Print Missing Documents Report ───────────────────

    const handlePrintMissingDocuments = useCallback((fileId: number, _fileNumber?: string, _retireeFullName?: string) => {
        const printWindow = window.open(`/files/${fileId}/missing-docs-print`, "_blank", "width=900,height=700");
        if (!printWindow) {
            toast.warning("يرجى السماح للنافذة المنبثقة");
        }
    }, []);

    // ─── Columns ────────────────────────────────────────────

    const columns = useMemo<CustomColumnDef<RetirementFileItem>[]>(() => [
        {
            key: "fileNumber",
            header: "رقم الملف",
            className: "w-[90px]",
            render: (row) => (
                <span className={cn("font-mono font-black text-primary truncate block", !row.hasDocuments && "text-danger ")}>{row.fileNumber}</span>
            ),
        },
        {
            key: "retireeFullName",
            header: "المتقاعد",
            className: "w-[200px]",
            render: (row) => (
                <div className="flex flex-col text-start min-w-0">
                    <span className={cn("font-bold text-sm truncate", getStatusColorClass(row.statusID), !row.hasDocuments && "text-danger ")}>
                        <User className="w-3.5 h-3.5 inline ms-1 text-muted-foreground shrink-0" />
                        {row.retireeFullName}
                    </span>
                    {row.nationalID && (
                        <span className="text-[10px] sm:text-xs text-muted-foreground truncate">
                            {row.rankName ? `${row.rankName} — ` : ""}{row.nationalID}
                        </span>
                    )}
                </div>
            ),
        },
        {
            key: "fileStatusName",
            header: "الحالة",
            className: "w-[100px]",
            render: (row) => (
                <Badge variant={
                    row.fileStatusColor === "success" ? "success" :
                        row.fileStatusColor === "warning" ? "warning" :
                            row.fileStatusColor === "destructive" ? "danger" : "outline"
                } className={cn("font-bold text-[10px] sm:text-xs", !row.hasDocuments && "border-danger ")}>
                    {row.fileStatusName || "—"}
                </Badge>
            ),
        },
        {
            key: "missingDocumentsCount",
            header: "النواقص",
            className: "w-[90px]",
            render: (row) => (
                !row.hasDocuments
                    ? <Badge variant="danger" className="text-xs">لم تُرفع</Badge>
                    : (row.missingDocumentsCount ?? 0) > 0
                        ? <Badge variant="warning" className="text-xs">{row.missingDocumentsCount} ناقصة</Badge>
                        : <Badge variant="success" className="text-xs">مكتمل</Badge>
            ),
        },
        {
            key: "lastUploadedByUserName",
            header: "المدخل",
            className: "w-[130px]",
            render: (row) => row.lastUploadedByUserName ? (
                <span className="text-xs text-muted-foreground flex items-center gap-1 truncate">
                    <User className="w-3 h-3 shrink-0" />
                    {row.lastUploadedByUserName}
                </span>
            ) : (
                <span className="text-xs text-danger  italic font-medium">— لم يُدخل بعد</span>
            ),
        },
        {
            key: "actions",
            header: "إجراءات",
            className: "w-[130px]",
            render: (row) => (
                <div className="flex items-center justify-center gap-1">
                    <Button
                        size="icon"
                        variant="ghost"
                        className="min-h-[44px] min-w-[44px] text-primary"
                        onClick={() => navigate(`/files/${row.fileID}?tab=documents`)}
                        title="رفع وثائق"
                    >
                        <Upload className="w-4 h-4" />
                    </Button>
                    <Button
                        size="icon"
                        variant="ghost"
                        className="min-h-[44px] min-w-[44px] text-warning"
                        onClick={(e) => { e.stopPropagation(); handlePrintMissingDocuments(row.fileID); }}
                        title="تقرير الوثائق الناقصة"
                    >
                        <FileText className="w-4 h-4" />
                    </Button>
                    <Button
                        size="icon"
                        variant="ghost"
                        className="min-h-[44px] min-w-[44px] text-info"
                        onClick={(e) => { e.stopPropagation(); handlePrintDocuments(row.fileID, row.fileNumber); }}
                        title="طباعة جميع الوثائق"
                    >
                        <Printer className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ], [navigate, handlePrintDocuments, handlePrintMissingDocuments]);

    const renderMobileCard = useCallback((row: RetirementFileItem) => (
        <div className={cn("p-1.5", !row.hasDocuments && "bg-card ")}>
            {/* Line 1: icon + file number + name (right), badges (left) */}
            <div className="flex items-center justify-between gap-2">
                <div className="flex items-center gap-1.5 min-w-0 flex-1">
                    <div className={`w-7 h-7 rounded-lg flex items-center justify-center shrink-0 ${!row.hasDocuments
                        ? "bg-danger/10 "
                        : (row.missingDocumentsCount ?? 0) > 0
                            ? "bg-warning/10 "
                            : "bg-success/10 "
                        }`}>
                        {!row.hasDocuments
                            ? <AlertTriangle className="w-3.5 h-3.5 text-danger " />
                            : (row.missingDocumentsCount ?? 0) > 0
                                ? <AlertTriangle className="w-3.5 h-3.5 text-warning " />
                                : <CheckCircle2 className="w-3.5 h-3.5 text-success " />
                        }
                    </div>
                    <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-1 min-w-0">
                            <span className={cn("font-mono font-bold text-[13px] shrink-0", !row.hasDocuments && "text-danger ")}>{row.fileNumber}</span>
                            <span className="text-[11px] text-muted-foreground truncate">{row.retireeFullName}</span>
                        </div>
                        <div className="flex items-center gap-0.5 flex-wrap">
                            <Badge variant={
                                row.fileStatusColor === "success" ? "success" :
                                    row.fileStatusColor === "warning" ? "warning" :
                                        row.fileStatusColor === "destructive" ? "danger" : "outline"
                            } className={cn("text-[8px] h-3.5 px-1", !row.hasDocuments && "border-danger ")}>
                                {row.fileStatusName || "—"}
                            </Badge>
                            {!row.hasDocuments && (
                                <Badge variant="danger" className="text-[8px] h-3.5 px-1">لم تُرفع</Badge>
                            )}
                            {row.hasDocuments && (row.missingDocumentsCount ?? 0) > 0 && (
                                <Badge variant="warning" className="text-[8px] h-3.5 px-1">
                                    {row.missingDocumentsCount} ناقصة
                                </Badge>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Line 2: uploader + action buttons */}
            <div className="flex items-center justify-between gap-2 mt-1 pt-1 border-t border-border/60">
                <div className="min-w-0 flex-1 truncate">
                    {!row.hasDocuments ? (
                        <p className="text-[10px] text-danger  flex items-center gap-1 font-medium truncate">
                            <AlertTriangle className="w-3 h-3 shrink-0" />
                            لم تُرفع أي وثائق بعد
                        </p>
                    ) : row.lastUploadedByUserName ? (
                        <p className="text-[10px] text-muted-foreground flex items-center gap-1 truncate">
                            <User className="w-3 h-3 shrink-0" />
                            {row.lastUploadedByUserName}
                        </p>
                    ) : (
                        <p className="text-[10px] text-danger  italic font-medium truncate" dir="rtl">
                            لم يُدخل بعد
                        </p>
                    )}
                </div>
                <div className="flex items-center gap-1 shrink-0">
                    <Button
                        variant="link"
                        size="sm"
                        className="h-auto p-0 text-[10px] text-warning "
                        onClick={(e) => { e.stopPropagation(); handlePrintMissingDocuments(row.fileID, row.fileNumber, row.retireeFullName); }}
                    >
                        <FileText className="w-3 h-3 ms-0.5" />
                        تقرير النواقص
                    </Button>
                    <span className="text-muted-foreground">·</span>
                    <Button
                        variant="link"
                        size="sm"
                        className="h-auto p-0 text-[10px] text-info "
                        onClick={(e) => { e.stopPropagation(); handlePrintDocuments(row.fileID, row.fileNumber); }}
                    >
                        <Printer className="w-3 h-3 ms-0.5" />
                        طباعة الوثائق
                    </Button>
                    <span className="text-muted-foreground">·</span>
                    <Button
                        variant="link"
                        size="sm"
                        className="h-auto p-0 text-[10px] text-primary dark:text-primary font-semibold"
                        onClick={(e) => { e.stopPropagation(); navigate(`/files/${row.fileID}`); }}
                    >
                        <Eye className="w-3 h-3 ms-0.5" />
                        استعراض الملف
                    </Button>
                </div>
            </div>
        </div>
    ), [handlePrintDocuments, handlePrintMissingDocuments]);

    const paginationConfig = useMemo((): PaginationConfig | undefined => (
        totalPages > 1 ? {
            page,
            pageSize: 20,
            total: filesData ? (filesData.items.length < 20 && page === totalPages ? (page - 1) * 20 + filesData.items.length : totalPages * 20) : 0,
            onPageChange: (p: number) => setPage(p),
        } : undefined
    ), [page, totalPages, filesData]);

    // ─── Render ────────────────────────────────────────────

    return (
        <div className="space-y-2 md:space-y-6 animate-fade-in" dir="rtl">
            {/* Header + Stats Toggle */}
            <div className="rounded-xl border bg-card overflow-hidden">
                <div className="flex items-center justify-between px-3 py-2 md:px-5 md:py-3">
                    <h1 className="text-sm sm:text-2xl font-bold text-foreground flex items-center gap-1.5">
                        <Upload className="w-4 h-4 sm:w-7 sm:h-7 text-primary" />
                        رفع الوثائق
                    </h1>
                    <button
                        type="button"
                        onClick={() => setStatsOpen((o) => !o)}
                        className="flex items-center gap-2 text-xs sm:text-sm font-bold text-muted-foreground hover:text-foreground transition-colors"
                    >
                        <span>الإحصائيات</span>
                        <span className="text-[10px] sm:text-xs font-normal">({totalFiles} ملف)</span>
                        <ChevronDown
                            className={cn(
                                "w-4 h-4 transition-transform duration-200",
                                statsOpen && "rotate-180"
                            )}
                        />
                    </button>
                </div>

                <div
                    className={cn(
                        "grid transition-all duration-300 ease-in-out",
                        statsOpen
                            ? "grid-rows-[1fr] opacity-100"
                            : "grid-rows-[0fr] opacity-0"
                    )}
                >
                    <div className="overflow-hidden">
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-1.5 md:gap-4 px-3 pb-3 md:px-5 md:pb-4">
                            <div className="rounded-xl border bg-background p-1.5 md:p-4 flex items-center gap-1.5 md:gap-3">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-info/10  flex items-center justify-center">
                                    <FileText className="w-3.5 h-3.5 md:w-5 md:h-5 text-info " />
                                </div>
                                <div>
                                    <p className="text-base md:text-2xl font-bold">{totalFiles}</p>
                                    <p className="text-[9px] md:text-xs text-muted-foreground">إجمالي الملفات</p>
                                </div>
                            </div>
                            <div className="rounded-xl border bg-background p-1.5 md:p-4 flex items-center gap-1.5 md:gap-3">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-success/10  flex items-center justify-center">
                                    <CheckCircle2 className="w-3.5 h-3.5 md:w-5 md:h-5 text-success " />
                                </div>
                                <div>
                                    <p className="text-base md:text-2xl font-bold">{filesWithDocuments}</p>
                                    <p className="text-[9px] md:text-xs text-muted-foreground">مرفوعة</p>
                                </div>
                            </div>
                            <div className="rounded-xl border bg-background p-1.5 md:p-4 flex items-center gap-1.5 md:gap-3">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-muted  flex items-center justify-center">
                                    <Upload className="w-3.5 h-3.5 md:w-5 md:h-5 text-muted-foreground " />
                                </div>
                                <div>
                                    <p className="text-base md:text-2xl font-bold">{filesWithoutDocuments}</p>
                                    <p className="text-[9px] md:text-xs text-muted-foreground">لم تُرفع وثائق</p>
                                </div>
                            </div>
                            <div className="rounded-xl border bg-background p-1.5 md:p-4 flex items-center gap-1.5 md:gap-3">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-warning/10  flex items-center justify-center">
                                    <AlertTriangle className="w-3.5 h-3.5 md:w-5 md:h-5 text-warning " />
                                </div>
                                <div>
                                    <p className="text-base md:text-2xl font-bold">{filesWithMissing}</p>
                                    <p className="text-[9px] md:text-xs text-muted-foreground">ناقصة وثائق</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Search & Filter */}
            <div className="flex flex-col gap-1.5 md:gap-3">
                <div className="flex items-end gap-2 flex-nowrap">
                    <SearchDialog
                        value={search}
                        onSelect={(val) => { setSearch(val); setPage(1); }}
                        queryKey={["retirement-files", "dialog-search"]}
                        fetchFn={async (term) => {
                            const res = await api.get<{ data: { items: RetirementFileItem[] } }>("/retirement-files", {
                                params: { page: 1, pageSize: 100, search: term || undefined },
                            });
                            return { items: res.data.data.items };
                        }}
                        getName={(f) => f.retireeFullName}
                        getSubtitle={(f) => f.fileNumber}
                        title="بحث بالاسم"
                        placeholder="اكتب الرقم العسكري أو الرقم العسكري أو الرقم العسكري أو اسم المتقاعد..."
                        hideTrigger
                    />
                    <div className="relative flex-1 min-w-0">
                        <Search className="absolute end-3 top-1/2 -translate-y-1/2 w-5 h-5 text-primary" />
                        <Input
                            className="pe-11 h-11 sm:h-12 text-sm sm:text-base font-medium border-2 border-primary/30 bg-primary/5 focus:border-primary focus:ring-primary/20 shadow-sm"
                            placeholder="بحث بالاسم، رقم الملف، أو الرقم العسكري..."
                            value={search}
                            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                        />
                    </div>
                </div>

                <div className="grid grid-cols-2 gap-1.5">
                    <NativeSelect
                        value={statusId}
                        onChange={(val) => { setStatusId(val); setPage(1); }}
                        className="!h-8 sm:!h-9 flex-1 min-w-0 text-[11px] sm:text-xs"
                        options={[
                            { value: "", label: "جميع الحالات" },
                            ...statuses.map(s => ({ value: String(s.statusID), label: s.statusName }))
                        ]}
                    />

                    <NativeSelect
                        value={uploadedBy}
                        onChange={(val) => { setUploadedBy(val); setPage(1); }}
                        className="!h-8 sm:!h-9 flex-1 min-w-0 text-[11px] sm:text-xs"
                        options={[
                            { value: "", label: "جميع المدخلين" },
                            ...uploaders.map(u => ({ value: String(u.id), label: u.name }))
                        ]}
                    />
                </div>

                <div className="grid grid-cols-4 gap-1.5">
                    <Button
                        variant={filterMode === "noDocuments" ? "destructive" : "outline"}
                        size="sm"
                        className="!min-h-0 !h-8 sm:!h-9 text-[10px] sm:text-xs px-1.5 sm:px-3"
                        onClick={() => setFilterMode("noDocuments")}
                    >
                        لم تُرفع
                    </Button>
                    <Button
                        variant={filterMode === "missing" ? "warning" : "outline"}
                        size="sm"
                        className="!min-h-0 !h-8 sm:!h-9 text-[10px] sm:text-xs px-1.5 sm:px-3"
                        onClick={() => setFilterMode("missing")}
                    >
                        ناقصة
                    </Button>
                    <Button
                        variant={filterMode === "uploaded" ? "default" : "outline"}
                        size="sm"
                        className="!min-h-0 !h-8 sm:!h-9 text-[10px] sm:text-xs px-1.5 sm:px-3"
                        onClick={() => setFilterMode("uploaded")}
                    >
                        مرفوعة
                    </Button>
                    <Button
                        variant={filterMode === "all" ? "default" : "outline"}
                        size="sm"
                        className="!min-h-0 !h-8 sm:!h-9 text-[10px] sm:text-xs px-1.5 sm:px-3"
                        onClick={() => setFilterMode("all")}
                    >
                        الكل
                    </Button>
                </div>
            </div>

            {/* Files DataTable */}
            <Card className="border bg-card p-0">
                <DataTable
                    columns={columns}
                    data={filteredFiles}
                    loading={loading}
                    pagination={paginationConfig}
                    emptyMessage={
                        filterMode === "missing"
                            ? "لا توجد ملفات ناقصة الوثائق"
                            : filterMode === "uploaded"
                                ? "لا توجد ملفات بها وثائق مرفوعة"
                                : filterMode === "noDocuments"
                                    ? "لا توجد ملفات لم تُرفع وثائقها نهائياً"
                                    : "لا توجد ملفات"
                    }
                    renderMobileCard={renderMobileCard}
                    onRowClick={(row) => navigate(`/files/${row.fileID}?tab=documents`)}
                    rowClassName={(row) => !row.hasDocuments ? "bg-card" : undefined}
                    className="border-none rounded-none"
                />
            </Card>
        </div>
    );
}
