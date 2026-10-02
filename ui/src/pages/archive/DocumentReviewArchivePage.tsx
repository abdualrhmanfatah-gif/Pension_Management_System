import { zodResolver } from "@hookform/resolvers/zod";
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    CheckCircle,
    Clock,
    Columns,
    Download,
    Eye,
    FileSearch,
    FileText,
    RotateCcw,
    Table,
    XCircle,
} from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";

import { archiveApi } from "@/api/archive";
import { lookupApi } from "@/api/pension";
import { SearchDialog } from "@/components/common/SearchDialog";
import { DocumentReviewPanel } from "@/components/domain/retirement/DocumentReviewPanel";
import { type DocumentItem,DocumentViewerDialog } from "@/components/domain/retirement/DocumentViewerDialog";
import {
    Badge,
    Button,
    Card,
    type CustomColumnDef,
    DataTable,
    DateInput,
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    FormField,
    Heading,
    NativeSelect,
    PageLayout,
    StatCard,
    Text,
    Textarea,
} from "@/components/ui";
import { getDocumentUrl } from "@/lib/utils";
import { cn, formatDate } from "@/lib/utils";
import type {
    DocumentReviewFilters,
    DocumentReviewItem,
    DocumentReviewStats,
} from "@/types/archive";
import {
    REVIEW_STATUS,
} from "@/types/archive";

const reviewSchema = z.object({
    reviewStatus: z.number().min(0, "حالة الفحص مطلوبة"),
    reviewNotes: z.string().min(10, "ملاحظات الفحص مطلوبة (10 أحرف على الأقل)"),
    rejectionReason: z.string().optional(),
}).refine(
    (data) => {
        if (data.reviewStatus === REVIEW_STATUS.REJECTED) {
            return data.rejectionReason && data.rejectionReason.length > 0;
        }
        return true;
    },
    {
        message: "سبب الرفض مطلوب عند رفض الوثيقة",
        path: ["rejectionReason"],
    }
);

type ReviewForm = z.infer<typeof reviewSchema>;
type ViewMode = "table" | "split";

export function DocumentReviewArchivePage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    // ─── State ────────────────────────────────────────────
    const [page, setPage] = useState(1);
    const pageSize = 20;
    const [search, setSearch] = useState("");
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [ownerType, setOwnerType] = useState<number | undefined>(undefined);
    const [documentTypeId, setDocumentTypeId] = useState<number | undefined>(undefined);
    const [reviewStatus, setReviewStatus] = useState<number | undefined>(undefined);
    const [dateFrom, setDateFrom] = useState<string | undefined>(undefined);
    const [dateTo, setDateTo] = useState<string | undefined>(undefined);
    const [branchId, setBranchId] = useState<number | undefined>(undefined);

    const [showReviewDialog, setShowReviewDialog] = useState(false);
    const [selectedDocument, setSelectedDocument] = useState<DocumentReviewItem | null>(null);
    const [showViewerDialog, setShowViewerDialog] = useState(false);
    const [viewingDocument, setViewingDocument] = useState<DocumentItem | null>(null);
    const [viewingFileId, setViewingFileId] = useState<number | undefined>(undefined);

    const [viewMode, setViewMode] = useState<ViewMode>("table");
    const [splitSelected, setSplitSelected] = useState<DocumentReviewItem | null>(null);

    // ─── Debounce Search ──────────────────────────────────
    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(search);
        }, 500);
        return () => clearTimeout(timer);
    }, [search]);

    // ─── Form ─────────────────────────────────────────────
    const reviewForm = useForm<ReviewForm>({
        resolver: zodResolver(reviewSchema),
        defaultValues: {
            reviewStatus: REVIEW_STATUS.PENDING,
            reviewNotes: "",
            rejectionReason: "",
        },
    });

    // ─── Queries ──────────────────────────────────────────
    const filters: Omit<DocumentReviewFilters, "page"> = {
        ownerType,
        documentTypeId,
        reviewStatus,
        branchId,
        dateFrom,
        dateTo,
        searchTerm: debouncedSearch || undefined,
        pageSize,
    };

    const {
        data: infiniteData,
        isLoading: isLoadingInfinite,
        fetchNextPage,
        hasNextPage,
        isFetchingNextPage,
    } = useInfiniteQuery({
        queryKey: ["archive", "review-documents-infinite", filters],
        queryFn: ({ pageParam = 1 }) =>
            archiveApi.getReviewDocuments({ ...filters, page: pageParam }),
        getNextPageParam: (lastPage) =>
            lastPage.page * lastPage.pageSize < lastPage.totalCount
                ? lastPage.page + 1
                : undefined,
        initialPageParam: 1,
    });

    const allDocuments = infiniteData?.pages.flatMap((p) => p.items) ?? [];
    const totalCount = infiniteData?.pages[0]?.totalCount ?? 0;

    const { data: tableData, isLoading: isLoadingTable } = useQuery({
        queryKey: ["archive", "review-documents", { ...filters, page }],
        queryFn: () => archiveApi.getReviewDocuments({ ...filters, page }),
    });

    const tableDocuments = tableData?.items ?? [];

    const { data: stats, isLoading: statsLoading } = useQuery<DocumentReviewStats>({
        queryKey: ["archive", "review-stats"],
        queryFn: async () => {
            const result = await archiveApi.getReviewStats();
            return result as unknown as DocumentReviewStats;
        },
    });

    const { data: documentTypes = [] } = useQuery({
        queryKey: ["document-types"],
        queryFn: () => archiveApi.getDocumentTypes(),
    });

    const { data: branches = [] } = useQuery({
        queryKey: ["branches"],
        queryFn: () => lookupApi.getBranches(),
    });

    // ─── Mutations ────────────────────────────────────────
    const reviewMutation = useMutation({
        mutationFn: (data: { documentId: number; reviewStatus: number; reviewNotes: string; rejectionReason?: string }) =>
            archiveApi.reviewDocument(data.documentId, {
                reviewStatus: data.reviewStatus,
                reviewNotes: data.reviewNotes,
                rejectionReason: data.rejectionReason,
            }),
        onSuccess: () => {
            toast.success("تم حفظ نتيجة الفحص بنجاح");
            queryClient.invalidateQueries({ queryKey: ["archive", "review-documents"] });
            queryClient.invalidateQueries({ queryKey: ["archive", "review-stats"] });
            setShowReviewDialog(false);
            setSelectedDocument(null);
            reviewForm.reset();
        },
        onError: () => {
            toast.error("خطأ في حفظ نتيجة الفحص");
        },
    });

    // ─── Handlers ─────────────────────────────────────────
    const handleReview = (doc: DocumentReviewItem) => {
        setSelectedDocument(doc);
        reviewForm.setValue("reviewStatus", doc.reviewStatus || REVIEW_STATUS.PENDING);
        reviewForm.setValue("reviewNotes", doc.reviewNotes || "");
        reviewForm.setValue("rejectionReason", doc.rejectionReason || "");
        setShowReviewDialog(true);
    };

    const handleViewDocument = (doc: DocumentReviewItem) => {
        setViewingDocument({
            documentID: doc.documentId,
            documentTypeID: doc.documentTypeId,
            documentTypeName: doc.documentTypeName,
            fileName: doc.fileName,
            filePath: doc.filePath,
            notes: doc.notes,
            uploadedByUser: doc.uploadedBy,
            createdAt: doc.createdAt,
        });
        setViewingFileId(doc.retirementFileId);
        setShowViewerDialog(true);
    };

    const handleSubmitReview = (data: ReviewForm) => {
        if (!selectedDocument) return;
        reviewMutation.mutate({
            documentId: selectedDocument.documentId,
            reviewStatus: data.reviewStatus,
            reviewNotes: data.reviewNotes,
            rejectionReason: data.rejectionReason,
        });
    };

    const clearFilters = () => {
        setSearch("");
        setDebouncedSearch("");
        setOwnerType(undefined);
        setDocumentTypeId(undefined);
        setReviewStatus(undefined);
        setBranchId(undefined);
        setDateFrom(undefined);
        setDateTo(undefined);
    };

    const getReviewBadge = (status: number) => {
        switch (status) {
            case REVIEW_STATUS.APPROVED:
                return <Badge variant="success">معتمدة</Badge>;
            case REVIEW_STATUS.REJECTED:
                return <Badge variant="destructive">مرفوضة</Badge>;
            case REVIEW_STATUS.PENDING:
            default:
                return <Badge variant="warning">بانتظار الفحص</Badge>;
        }
    };

    const getFileUrl = (filePath: string) => {
        return getDocumentUrl(filePath) ?? filePath;
    };

    // ─── Columns ──────────────────────────────────────────
    const columns: CustomColumnDef<DocumentReviewItem>[] = [
        {
            key: "documentId",
            header: "الرقم",
            render: (row) => (
                <span className="font-mono font-bold text-primary">{row.documentId}</span>
            ),
        },
        {
            key: "fileName",
            header: "اسم الملف",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-muted-foreground" />
                    <span className="text-sm font-medium truncate max-w-[200px]">{row.fileName}</span>
                </div>
            ),
        },
        {
            key: "documentTypeName",
            header: "نوع الوثيقة",
            render: (row) => (
                <span className="text-sm">{row.documentTypeName}</span>
            ),
        },
        {
            key: "ownerType",
            header: "القسم",
            render: (row) => (
                <Badge variant="outline">{row.ownerTypeName}</Badge>
            ),
        },
        {
            key: "retireeName",
            header: "رقم الملف",
            render: (row) => (
                <span className="text-sm font-medium">
                    {row.retireeName || row.fileNumber || "—"}
                </span>
            ),
        },
        {
            key: "uploadedBy",
            header: "رفع بواسطة",
            render: (row) => (
                <span className="text-sm text-muted-foreground">{row.uploadedBy}</span>
            ),
        },
        {
            key: "createdAt",
            header: "تاريخ الرفع",
            render: (row) => (
                <span className="text-sm">{formatDate(row.createdAt)}</span>
            ),
        },
        {
            key: "notes",
            header: "ملاحظات",
            render: (row) => (
                <span className="text-sm text-muted-foreground truncate max-w-[150px] block" title={row.notes || ""}>
                    {row.notes || "—"}
                </span>
            ),
        },
        {
            key: "reviewStatus",
            header: "حالة الفحص",
            render: (row) => getReviewBadge(row.reviewStatus),
        },
        {
            key: "actions",
            header: "",
            render: (row) => (
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="sm"
                        title="عرض الوثيقة"
                        onClick={(e) => {
                            e.stopPropagation();
                            handleViewDocument(row);
                        }}
                    >
                        <Eye className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
                        title="تحميل"
                        onClick={(e) => {
                            e.stopPropagation();
                            const link = document.createElement("a");
                            link.href = getFileUrl(row.filePath);
                            link.download = row.fileName;
                            link.click();
                        }}
                    >
                        <Download className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="فحص الوثيقة"
                        onClick={(e) => {
                            e.stopPropagation();
                            handleReview(row);
                        }}
                    >
                        <FileSearch className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <PageLayout>
            <Heading title="فحص الوثائق" />

            {/* Stats Cards */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي الوثائق"
                    value={stats?.totalDocuments ?? 0}
                    icon={<FileText className="w-6 h-6" />}
                    variant="blue"
                    isLoading={statsLoading}
                />
                <StatCard
                    title="بانتظار الفحص"
                    value={stats?.pendingReview ?? 0}
                    icon={<Clock className="w-6 h-6" />}
                    variant="warning"
                    isLoading={statsLoading}
                />
                <StatCard
                    title="معتمدة"
                    value={stats?.approvedDocuments ?? 0}
                    icon={<CheckCircle className="w-6 h-6" />}
                    variant="success"
                    isLoading={statsLoading}
                />
                <StatCard
                    title="مرفوضة"
                    value={stats?.rejectedDocuments ?? 0}
                    icon={<XCircle className="w-6 h-6" />}
                    variant="destructive"
                    isLoading={statsLoading}
                />
            </div>

            {/* Filters */}
            <Card className="p-4 mb-6">
                <div className="flex flex-nowrap gap-3 items-end">
                    <SearchDialog
                        value={search}
                        onSelect={setSearch}
                        queryKey={["archive", "dialog-search"]}
                        fetchFn={(term) => archiveApi.getReviewDocuments({ searchTerm: term || undefined, page: 1, pageSize: 50 })}
                        getName={(doc) => doc.retireeName || doc.entityName || doc.fileName}
                        getSubtitle={(doc) => doc.fileNumber || doc.documentTypeName}
                        title="بحث في الوثائق"
                        placeholder="بحث بالاسم أو الرقم..."
                    />

                    <NativeSelect
                        className="w-36 shrink-0"
                        value={branchId ?? ""}
                        placeholder=""
                        onChange={(val) => {
                            setBranchId(val ? Number(val) : undefined);
                            
                        }}
                        options={[
                            { value: "", label: "جميع الفروع" },
                            ...branches.map((b) => ({
                                value: b.id.toString(),
                                label: b.name,
                            })),
                        ]}
                    />

                    <NativeSelect
                        className="w-36 shrink-0"
                        value={ownerType ?? ""}
                        placeholder=""
                        onChange={(val) => {
                            setOwnerType(val ? Number(val) : undefined);
                            
                        }}
                        options={[
                            { value: "", label: "جميع الأقسام" },
                            { value: "1", label: "ملفات التقاعد" },
                            { value: "2", label: "المتقاعدون" },
                            { value: "3", label: "المستحقون" },
                            { value: "4", label: "الوكلاء" },
                            { value: "5", label: "الأوصياء" },
                            { value: "15", label: "مشاريع الاستثمار" },
                            { value: "40", label: "الموظفون" },
                            { value: "41", label: "المراسلات" },
                        ]}
                    />

                    <NativeSelect
                        className="w-36 shrink-0"
                        value={documentTypeId ?? ""}
                        placeholder=""
                        onChange={(val) => {
                            setDocumentTypeId(val ? Number(val) : undefined);
                            
                        }}
                        options={[
                            { value: "", label: "جميع أنواع الوثائق" },
                            ...documentTypes.map((t) => ({
                                value: t.documentTypeId.toString(),
                                label: t.typeName,
                            })),
                        ]}
                    />

                    <NativeSelect
                        className="w-40 shrink-0"
                        value={reviewStatus ?? ""}
                        placeholder=""
                        onChange={(val) => {
                            setReviewStatus(val !== "" ? Number(val) : undefined);
                            
                        }}
                        options={[
                            { value: "", label: "جميع حالات الفحص" },
                            { value: "1", label: "بانتظار الفحص" },
                            { value: "2", label: "معتمدة" },
                            { value: "3", label: "مرفوضة" },
                        ]}
                    />

                    <DateInput
                        label="من تاريخ"
                        className="w-36 shrink-0"
                        value={dateFrom || ""}
                        onChange={(val) => {
                            setDateFrom(val || undefined);
                            
                        }}
                        placeholder="من تاريخ"
                    />

                    <DateInput
                        label="إلى تاريخ"
                        className="w-36 shrink-0"
                        value={dateTo || ""}
                        onChange={(val) => {
                            setDateTo(val || undefined);
                            
                        }}
                        placeholder="إلى تاريخ"
                    />

                    <Button variant="ghost" size="icon" className="shrink-0" onClick={clearFilters} title="إعادة تعيين">
                        <RotateCcw className="w-4 h-4" />
                    </Button>

                    {/* View Mode Toggle */}
                    <div className="flex items-center border rounded-lg overflow-hidden">
                        <Button
                            variant={viewMode === "table" ? "default" : "ghost"}
                            size="sm"
                            className="rounded-none border-0"
                            onClick={() => setViewMode("table")}
                        >
                            <Table className="w-4 h-4 ml-1" />
                            جدول
                        </Button>
                        <Button
                            variant={viewMode === "split" ? "default" : "ghost"}
                            size="sm"
                            className="rounded-none border-0"
                            onClick={() => setViewMode("split")}
                        >
                            <Columns className="w-4 h-4 ml-1" />
                            مقسم
                        </Button>
                    </div>
                </div>
            </Card>

            {/* ═══ Table View ═══ */}
            {viewMode === "table" && (
                <Card>
                    <DataTable
                        columns={columns}
                        data={tableDocuments}
                        loading={isLoadingTable}
                        emptyMessage="لا توجد وثائق"
                        rowKey={(row, index) => row.documentId ?? index}
                        pagination={{
                            page,
                            pageSize,
                            total: totalCount,
                            onPageChange: setPage,
                        }}
                    />
                </Card>
            )}

            {/* ═══ Split View ═══ */}
            {viewMode === "split" && (
                <div className="flex gap-4 h-[calc(100vh-280px)] min-h-[500px]">
                    {/* Left Panel — Document List */}
                    <Card className="w-[380px] shrink-0 flex flex-col overflow-hidden">
                        <div className="p-3 border-b bg-muted/30">
                            <Text variant="label" className="text-muted-foreground">
                                {totalCount} وثيقة
                            </Text>
                        </div>
                        <div
                            className="flex-1 min-h-0 overflow-y-auto"
                            onScroll={(e) => {
                                const { scrollHeight, scrollTop, clientHeight } = e.currentTarget;
                                if (scrollHeight - scrollTop - clientHeight < 100 && hasNextPage && !isFetchingNextPage) {
                                    fetchNextPage();
                                }
                            }}
                        >
                            {isLoadingInfinite && (
                                <div className="p-8 text-center text-sm text-muted-foreground">
                                    جاري التحميل...
                                </div>
                            )}
                            {!isLoadingInfinite && allDocuments.length === 0 && (
                                <div className="p-8 text-center text-sm text-muted-foreground">
                                    لا توجد وثائق
                                </div>
                            )}
                            {allDocuments.map((doc) => (
                                <div
                                    key={doc.documentId}
                                    className={cn(
                                        "p-3 cursor-pointer transition-colors hover:bg-muted/50 border-b",
                                        splitSelected?.documentId === doc.documentId && "bg-primary/5 border-r-2 border-primary"
                                    )}
                                    onClick={() => {
                                        setSplitSelected(doc);
                                        reviewForm.setValue("reviewStatus", doc.reviewStatus || REVIEW_STATUS.PENDING);
                                        reviewForm.setValue("reviewNotes", doc.reviewNotes || "");
                                        reviewForm.setValue("rejectionReason", doc.rejectionReason || "");
                                    }}
                                >
                                        <div className="flex items-start gap-3">
                                            <div className="shrink-0 mt-0.5">
                                                <FileText className="w-5 h-5 text-muted-foreground" />
                                            </div>
                                            <div className="flex-1 min-w-0">
                                                <Text variant="label">{doc.documentTypeName} — {doc.entityName || doc.retireeName || "—"}</Text>
                                                <Text variant="caption" className="mt-1">{doc.uploadedBy}</Text>
                                                <div className="flex items-center gap-2 mt-1.5">
                                                    {getReviewBadge(doc.reviewStatus)}
                                                    <span className="text-[10px] text-muted-foreground">
                                                        {formatDate(doc.createdAt)}
                                                    </span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            {isFetchingNextPage && (
                                <div className="p-4 text-center text-sm text-muted-foreground">
                                    جاري تحميل المزيد...
                                </div>
                            )}
                            {hasNextPage === false && allDocuments.length > 0 && (
                                <div className="p-4 text-center text-xs text-muted-foreground">
                                    تم تحميل جميع الوثائق
                                </div>
                            )}
                        </div>
                    </Card>

                    {/* Right Panel — Document Details */}
                    <DocumentReviewPanel
                        document={splitSelected}
                        onViewDocument={handleViewDocument}
                        onRefresh={() => {
                            queryClient.invalidateQueries({ queryKey: ["archive", "review-documents"] });
                            queryClient.invalidateQueries({ queryKey: ["archive", "review-stats"] });
                        }}
                    />
                </div>
            )}

            {/* Review Dialog (for table mode) */}
            <Dialog open={showReviewDialog} onOpenChange={setShowReviewDialog}>
                <DialogContent className="max-w-lg">
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2">
                            <FileSearch className="w-5 h-5" />
                            فحص الوثيقة
                        </DialogTitle>
                    </DialogHeader>

                    <div className="flex flex-col gap-4">
                        {/* Document Info */}
                        <div className="p-3 bg-muted/50 rounded-lg">
                            <div className="flex items-center gap-2 mb-2">
                                <FileText className="w-4 h-4 text-muted-foreground" />
                                <span className="text-sm font-medium">{selectedDocument?.fileName}</span>
                            </div>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm">
                                <div>
                                    <span className="text-muted-foreground">النوع:</span>
                                    <span className="mr-2">{selectedDocument?.documentTypeName}</span>
                                </div>
                                <div>
                                    <span className="text-muted-foreground">رفع بواسطة:</span>
                                    <span className="mr-2">{selectedDocument?.uploadedBy}</span>
                                </div>
                            </div>
                        </div>

                        {/* View Document Button */}
                        {selectedDocument?.filePath && (
                            <Button
                                variant="outline"
                                className="w-full"
                                onClick={() => {
                                    setShowReviewDialog(false);
                                    handleViewDocument(selectedDocument);
                                }}
                            >
                                <Eye className="w-4 h-4 ml-2" />
                                عرض الوثيقة
                            </Button>
                        )}

                        {/* Review Form */}
                        <FormField label="حالة الفحص *" error={reviewForm.formState.errors.reviewStatus?.message}>
                            <NativeSelect
                                value={reviewForm.watch("reviewStatus").toString()}
                                onChange={(val) => reviewForm.setValue("reviewStatus", Number(val))}
                                options={[
                                    { value: "1", label: "بانتظار الفحص" },
                                    { value: "2", label: "معتمدة" },
                                    { value: "3", label: "مرفوضة" },
                                ]}
                            />
                        </FormField>

                        <FormField label="ملاحظات الفحص *" error={reviewForm.formState.errors.reviewNotes?.message}>
                            <Textarea
                                {...reviewForm.register("reviewNotes")}
                                placeholder="أدخل ملاحظات الفحص..."
                                rows={4}
                            />
                        </FormField>

                        {reviewForm.watch("reviewStatus") === REVIEW_STATUS.REJECTED && (
                            <FormField label="سبب الرفض *" error={reviewForm.formState.errors.rejectionReason?.message}>
                                <Textarea
                                    {...reviewForm.register("rejectionReason")}
                                    placeholder="أدخل سبب رفض الوثيقة..."
                                    rows={3}
                                />
                            </FormField>
                        )}

                        {/* File Link */}
                        {selectedDocument?.retirementFileId && (
                            <div className="p-3 bg-muted/50 rounded-lg">
                                <span className="text-sm text-muted-foreground">مرفق لملف: </span>
                                <Button
                                    variant="link"
                                    size="sm"
                                    className="p-0 h-auto"
                                    onClick={() => {
                                        navigate(`/files/${selectedDocument.retirementFileId}`);
                                    }}
                                >
                                    {selectedDocument.fileNumber || `ملف #${selectedDocument.retirementFileId}`}
                                </Button>
                            </div>
                        )}

                        {/* Action Buttons */}
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                className="flex-1"
                                onClick={() => {
                                    setShowReviewDialog(false);
                                    setSelectedDocument(null);
                                    reviewForm.reset();
                                }}
                            >
                                إلغاء
                            </Button>
                            <Button
                                className="flex-1"
                                onClick={reviewForm.handleSubmit(handleSubmitReview)}
                                disabled={reviewMutation.isPending}
                            >
                                {reviewMutation.isPending ? "جاري الحفظ..." : "حفظ نتيجة الفحص"}
                            </Button>
                        </div>
                    </div>
                </DialogContent>
            </Dialog>

            {/* Document Viewer Dialog */}
            <DocumentViewerDialog
                open={showViewerDialog}
                onOpenChange={setShowViewerDialog}
                document={viewingDocument}
                fileId={viewingFileId}
                onUploaded={() => {
                    queryClient.invalidateQueries({ queryKey: ["archive", "review-documents"] });
                    queryClient.invalidateQueries({ queryKey: ["archive", "review-stats"] });
                    setShowViewerDialog(false);
                }}
                onRefresh={() => {
                    queryClient.invalidateQueries({ queryKey: ["archive", "review-documents"] });
                    queryClient.invalidateQueries({ queryKey: ["archive", "review-stats"] });
                }}
            />
        </PageLayout>
    );
}
