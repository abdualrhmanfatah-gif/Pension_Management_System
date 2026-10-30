<<<<<<< HEAD
/**
 * استكمال النواقص — صفحة مخصصة لتوثيق ورفع الوثائق الناقصة لملفات التقاعد.
 * تعرض قائمة ملفات التقاعد مع عدد النواقص ونسبة اكتمال الوثائق
 * مع إمكانية الانتقال لتفاصيل الملف لرفع وتوثيق الوثائق
 * مع إحصائيات المدخلين وحالة التوثيق
 */

import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, CheckCircle2, ChevronDown, FileText, Printer, Search, Upload, User } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import api from "@/api/axios";
import { documentsApi } from "@/api/documents";
import { SearchDialog } from "@/components/common/SearchDialog";
import { FileStatusBadge } from "@/components/domain/retirement/FileStatusBadge";
import { RetirementFileCard } from "@/components/domain/retirement/RetirementFileCard";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/Accordion";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import type { CustomColumnDef, PaginationConfig } from "@/components/ui/DataTable";
import { DataTable } from "@/components/ui/DataTable";
import { Input } from "@/components/ui/Input";
import { NativeSelect } from "@/components/ui/NativeSelect";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { retirementFileCardClassName } from "@/lib/retirementFileCard";
import { cn, getDocumentUrl } from "@/lib/utils";

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
    missingDocumentsCount: number;
    hasDocuments: boolean;
    lastUploadedByUserID: number | null;
    lastUploadedByUserName: string | null;
    statusID: number | null;
}

interface PagedResult {
    items: RetirementFileItem[];
    totalPages: number;
}

const DOCUMENT_FILTERS = [
    { value: "all", label: "الكل" },
    { value: "noDocuments", label: "لم تُرفع" },
    { value: "missing", label: "ناقصة" },
    { value: "uploaded", label: "مرفوعة" },
] as const;

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
                <bdi dir="ltr" className="font-mono font-semibold text-foreground whitespace-nowrap">{row.fileNumber}</bdi>
            ),
        },
        {
            key: "retireeFullName",
            header: "المتقاعد",
            className: "w-[200px]",
            render: (row) => (
                <div className="flex flex-col text-start min-w-0">
                    <span className="font-bold text-sm text-foreground truncate">
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
                <FileStatusBadge statusId={row.statusID} statusName={row.fileStatusName} />
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
                        aria-label="رفع وثائق"
                    >
                        <Upload className="w-4 h-4" />
                    </Button>
                    <Button
                        size="icon"
                        variant="ghost"
                        className="min-h-[44px] min-w-[44px] text-warning"
                        onClick={(e) => { e.stopPropagation(); handlePrintMissingDocuments(row.fileID); }}
                        title="تقرير الوثائق الناقصة"
                        aria-label="تقرير الوثائق الناقصة"
                    >
                        <FileText className="w-4 h-4" />
                    </Button>
                    <Button
                        size="icon"
                        variant="ghost"
                        className="min-h-[44px] min-w-[44px] text-info"
                        onClick={(e) => { e.stopPropagation(); handlePrintDocuments(row.fileID, row.fileNumber); }}
                        title="طباعة جميع الوثائق"
                        aria-label="طباعة جميع الوثائق"
                    >
                        <Printer className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ], [navigate, handlePrintDocuments, handlePrintMissingDocuments]);

    const renderMobileCard = useCallback((row: RetirementFileItem) => (
        <RetirementFileCard file={row}>
            <div className="grid grid-cols-2 gap-2">
                <Button
                    variant="default"
                    size="sm"
                    onClick={() => navigate(`/files/${row.fileID}?tab=documents`)}
                >
                    <Upload className="h-4 w-4 shrink-0" aria-hidden="true" />
                    رفع الوثائق
                </Button>
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handlePrintDocuments(row.fileID, row.fileNumber)}
                >
                    <Printer className="h-4 w-4 shrink-0" aria-hidden="true" />
                    طباعة الوثائق
                </Button>
            </div>
        </RetirementFileCard>
    ), [navigate, handlePrintDocuments]);

    const paginationConfig = useMemo((): PaginationConfig | undefined => (
        totalPages > 1 ? {
            page,
            pageSize: 20,
            total: filesData ? (filesData.items.length < 20 && page === totalPages ? (page - 1) * 20 + filesData.items.length : totalPages * 20) : 0,
            onPageChange: (p: number) => setPage(p),
        } : undefined
    ), [page, totalPages, filesData]);

    const filterFields = (
        <div className="grid grid-cols-2 gap-2 sm:gap-3">
            <NativeSelect
                label="حالة الملف"
                value={statusId}
                onChange={(val) => { setStatusId(val); setPage(1); }}
                options={[
                    { value: "", label: "جميع الحالات" },
                    ...statuses.map(s => ({ value: String(s.statusID), label: s.statusName }))
                ]}
            />
            <NativeSelect
                label="حالة الوثائق"
                value={filterMode}
                placeholder=""
                options={[...DOCUMENT_FILTERS]}
                onChange={(value) => {
                    const filter = DOCUMENT_FILTERS.find((option) => option.value === value);
                    if (filter) setFilterMode(filter.value);
                }}
            />
            <div className="col-span-2">
                <NativeSelect
                    label="المدخل"
                    value={uploadedBy}
                    onChange={(val) => { setUploadedBy(val); setPage(1); }}
                    options={[
                        { value: "", label: "جميع المدخلين" },
                        ...uploaders.map(u => ({ value: String(u.id), label: u.name }))
                    ]}
                />
            </div>
        </div>
    );
    const activeFilterCount = Number(statusId !== "") + Number(uploadedBy !== "") + Number(filterMode !== "all");

    // ─── Render ────────────────────────────────────────────

    return (
        <div className="min-w-0 space-y-1.5 md:space-y-3" dir="rtl">
            {/* Header + Stats Toggle */}
            <div>
                <div className="flex flex-wrap items-center justify-between gap-2">
                    <div className="min-w-0">
                        <h1 className="flex items-center gap-2 text-xl font-bold text-foreground sm:text-2xl">
                            <Upload className="h-5 w-5 shrink-0 text-primary sm:h-6 sm:w-6" aria-hidden="true" />
                            رفع الوثائق
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            إجمالي الملفات: <bdi className="font-semibold tabular-nums text-foreground">{totalFiles.toLocaleString("ar-YE")}</bdi>
                        </p>
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setStatsOpen((o) => !o)}
                        aria-expanded={statsOpen}
                        aria-controls="document-upload-statistics"
                    >
                        <span>الإحصائيات</span>
                        <ChevronDown
                            aria-hidden="true"
                            className={cn(
                                "w-4 h-4 transition-transform duration-200",
                                statsOpen && "rotate-180"
                            )}
                        />
                    </Button>
                </div>

                <div
                    id="document-upload-statistics"
                    inert={!statsOpen}
                    aria-hidden={!statsOpen}
                    className={cn(
                        "grid transition-all duration-300 ease-in-out",
                        statsOpen
                            ? "grid-rows-[1fr] opacity-100"
                            : "grid-rows-[0fr] opacity-0"
                    )}
                >
                    <div className="overflow-hidden">
                        <div className="mt-2 grid grid-cols-2 gap-2 rounded-xl border border-border bg-card p-3 md:grid-cols-4 md:p-4">
                            <div className="flex items-center gap-2">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-info/10  flex items-center justify-center">
                                    <FileText className="w-3.5 h-3.5 md:w-5 md:h-5 text-info " />
                                </div>
                                <div>
                                    <p className="text-lg font-bold tabular-nums md:text-2xl">{totalFiles.toLocaleString("ar-YE")}</p>
                                    <p className="text-xs text-muted-foreground">إجمالي الملفات</p>
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-success/10  flex items-center justify-center">
                                    <CheckCircle2 className="w-3.5 h-3.5 md:w-5 md:h-5 text-success " />
                                </div>
                                <div>
                                    <p className="text-lg font-bold tabular-nums md:text-2xl">{filesWithDocuments.toLocaleString("ar-YE")}</p>
                                    <p className="text-xs text-muted-foreground">مرفوعة</p>
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-muted  flex items-center justify-center">
                                    <Upload className="w-3.5 h-3.5 md:w-5 md:h-5 text-muted-foreground " />
                                </div>
                                <div>
                                    <p className="text-lg font-bold tabular-nums md:text-2xl">{filesWithoutDocuments.toLocaleString("ar-YE")}</p>
                                    <p className="text-xs text-muted-foreground">لم تُرفع وثائق</p>
                                </div>
                            </div>
                            <div className="flex items-center gap-2">
                                <div className="w-7 h-7 md:w-10 md:h-10 rounded-lg bg-warning/10  flex items-center justify-center">
                                    <AlertTriangle className="w-3.5 h-3.5 md:w-5 md:h-5 text-warning " />
                                </div>
                                <div>
                                    <p className="text-lg font-bold tabular-nums md:text-2xl">{filesWithMissing.toLocaleString("ar-YE")}</p>
                                    <p className="text-xs text-muted-foreground">ناقصة وثائق</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Search & Filter */}
            <div className="space-y-0 sm:space-y-2">
                <div>
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
                    <Input
                        label="البحث عن ملف"
                        icon={Search}
                        placeholder="الرقم العسكري أو اسم المتقاعد"

                        value={search}
                        onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                    />
                </div>

                <Accordion type="single" collapsible className="sm:hidden">
                    <AccordionItem value="file-filters" className="border-0">
                        <AccordionTrigger density="compact">
                            <span className="flex items-center gap-2">
                                الفلاتر
                                {activeFilterCount > 0 && (
                                    <Badge variant="secondary">{activeFilterCount} مفعّل</Badge>
                                )}
                            </span>
                        </AccordionTrigger>
                        <AccordionContent className="pb-0">{filterFields}</AccordionContent>
                    </AccordionItem>
                </Accordion>
                <div className="hidden sm:block">{filterFields}</div>
            </div>

            {/* Files DataTable */}
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
                mobileScrollMode="page"
                rowKey={(row) => row.fileID}
                rowClassName={retirementFileCardClassName}
                onRowClick={(row) => navigate(`/files/${row.fileID}?tab=documents`)}
            />
        </div>
    );
=======
import { DocumentUploadView } from "@/components/domain/retirement/DocumentUploadView";

/**
 * استكمال النواقص — صفحة مخصصة لتوثيق ورفع الوثائق الناقصة لملفات التقاعد.
 * تعرض قائمة الملفات مع عدد النواقص وحالة التوثيق مع إمكانية التصفية والبحث.
 */
export function MissingDocsCompletionPage() {
    return <DocumentUploadView />;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}
