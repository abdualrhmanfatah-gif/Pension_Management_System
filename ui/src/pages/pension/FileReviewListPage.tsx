import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    CheckCircle,
    ChevronDown,
    ChevronUp,
    Clock,
    Eye,
    FileSearch,
    ThumbsDown,
    ThumbsUp,
    User,
    XCircle,
} from "lucide-react";
import { useCallback, useId,useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { SearchDialog } from "@/components/common/SearchDialog";
import {
    Badge,
    Button,
    CrudPageLayout,
    Dialog,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    NativeSelect,
    StatCard,
    Textarea,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import type { PendingReviewFile } from "@/types/retirement";

// ─── Reject Dialog ──────────────────────────────────────

function RejectDialog({
    isOpen,
    onClose,
    onConfirm,
    fileName,
    isLoading,
}: {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: (reason: string) => void;
    fileName: string;
    isLoading: boolean;
}) {
    const [reason, setReason] = useState("");
    const textareaId = useId();

    const handleSubmit = useCallback(() => {
        const trimmed = reason.trim();
        if (!trimmed) return;
        onConfirm(trimmed);
        setReason("");
    }, [reason, onConfirm]);

    const handleClose = useCallback(() => {
        setReason("");
        onClose();
    }, [onClose]);

    return (
        <Dialog
            open={isOpen}
            onOpenChange={(open: boolean) => {
                if (!open) handleClose();
            }}
        >
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="text-right text-sm font-black flex items-center gap-2">
                        <XCircle className="w-4 h-4 text-destructive" />
                        رفض الملف: {fileName}
                    </DialogTitle>
                </DialogHeader>
                <div className="space-y-4 py-2">
                    <Textarea
                        id={textareaId}
                        label="سبب الرفض *"
                        className="min-h-[100px] text-xs"
                        placeholder="اكتب سبب الرفض بالتفصيل..."
                        value={reason}
                        onChange={(e) => setReason(e.target.value)}
                        disabled={isLoading}
                        error={!reason.trim() ? "السبب مطلوب" : undefined}
                    />
                </div>
                <DialogFooter className="gap-2 sm:gap-0">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={handleClose}
                        disabled={isLoading}
                    >
                        إلغاء
                    </Button>
                    <Button
                        variant="destructive"
                        size="sm"
                        onClick={handleSubmit}
                        disabled={!reason.trim() || isLoading}
                        loading={isLoading}
                    >
                        <XCircle className="w-3.5 h-3.5 mr-1" />
                        تأكيد الرفض
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

// ─── Approve Dialog ─────────────────────────────────────

function ApproveDialog({
    isOpen,
    onClose,
    onConfirm,
    fileName,
    isLoading,
}: {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: (notes: string) => void;
    fileName: string;
    isLoading: boolean;
}) {
    const [notes, setNotes] = useState("");
    const textareaId = useId();

    const handleSubmit = useCallback(() => {
        onConfirm(notes.trim());
        setNotes("");
    }, [notes, onConfirm]);

    const handleClose = useCallback(() => {
        setNotes("");
        onClose();
    }, [onClose]);

    return (
        <Dialog
            open={isOpen}
            onOpenChange={(open: boolean) => {
                if (!open) handleClose();
            }}
        >
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="text-right text-sm font-black flex items-center gap-2">
                        <ThumbsUp className="w-4 h-4 text-emerald-600" />
                        اعتماد الملف: {fileName}
                    </DialogTitle>
                </DialogHeader>
                <div className="space-y-4 py-2">
                    <Textarea
                        id={textareaId}
                        label="ملاحظات (اختياري)"
                        className="min-h-[80px] text-xs"
                        placeholder="أضف ملاحظات على الاعتماد إن وجدت..."
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                        disabled={isLoading}
                    />
                </div>
                <DialogFooter className="gap-2 sm:gap-0">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={handleClose}
                        disabled={isLoading}
                    >
                        إلغاء
                    </Button>
                    <Button
                        variant="success"
                        size="sm"
                        onClick={handleSubmit}
                        disabled={isLoading}
                        loading={isLoading}
                    >
                        <CheckCircle className="w-3.5 h-3.5 mr-1" />
                        تأكيد الاعتماد
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

// ─── Main Page ──────────────────────────────────────────

export function FileReviewListPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { searchTerm, setSearchTerm, debouncedValue } =
        useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);

    const [showFilters, setShowFilters] = useState(false);
    const [filters, setFilters] = useState<{
        status?: "pending" | "approved" | "rejected";
    }>({});

    // ─── Dialog State ──────────────────────────────────

    const [rejectDialog, setRejectDialog] = useState<{
        isOpen: boolean;
        fileId: number | null;
        fileName: string;
    }>({ isOpen: false, fileId: null, fileName: "" });

    const [approveDialog, setApproveDialog] = useState<{
        isOpen: boolean;
        fileId: number | null;
        fileName: string;
    }>({ isOpen: false, fileId: null, fileName: "" });

    // ─── Queries ─────────────────────────────────────────

    const { data: pendingFiles = [], isLoading } = useQuery({
        queryKey: ["pending-reviews"],
        queryFn: () => retirementApi.getPendingReviews(),
    });

    // ─── Filtered Data (Client-side) ─────────────────────

    const filteredItems = useMemo(() => {
        let items = pendingFiles;

        if (debouncedValue) {
            const q = debouncedValue.toLowerCase();
            items = items.filter(
                (f: PendingReviewFile) =>
                    f.fileNumber.toLowerCase().includes(q) ||
                    f.retireeFullName.toLowerCase().includes(q) ||
                    f.nationalID?.toLowerCase().includes(q)
            );
        }

        if (filters.status === "pending") {
            items = items.filter((f: PendingReviewFile) => f.reviewStatus === 0);
        } else if (filters.status === "approved") {
            items = items.filter((f: PendingReviewFile) => f.reviewStatus === 1);
        } else if (filters.status === "rejected") {
            items = items.filter((f: PendingReviewFile) => f.reviewStatus === 2);
        }

        return items;
    }, [pendingFiles, debouncedValue, filters.status]);

    const paginatedItems = useMemo(() => {
        const start = (page - 1) * 20;
        return filteredItems.slice(start, start + 20);
    }, [filteredItems, page]);

    // ─── Stats ───────────────────────────────────────────

    const stats = useMemo(() => {
        const total = pendingFiles.length;
        const pending = pendingFiles.filter((f: PendingReviewFile) => f.reviewStatus === 0).length;
        const approved = pendingFiles.filter((f: PendingReviewFile) => f.reviewStatus === 1).length;
        const rejected = pendingFiles.filter((f: PendingReviewFile) => f.reviewStatus === 2).length;
        return { total, pending, approved, rejected };
    }, [pendingFiles]);

    // ─── Mutations ───────────────────────────────────────

    const approveMutation = useMutation({
        mutationFn: ({ fileId, notes }: { fileId: number; notes?: string }) =>
            retirementApi.approveReview(fileId, notes),
        onSuccess: () => {
            toast.success("تم الاعتماد بنجاح");
            queryClient.invalidateQueries({ queryKey: ["pending-reviews"] });
            setApproveDialog({ isOpen: false, fileId: null, fileName: "" });
        },
        onError: () => {
            toast.error("خطأ في الاعتماد");
        },
    });

    const rejectMutation = useMutation({
        mutationFn: ({ fileId, reason }: { fileId: number; reason: string }) =>
            retirementApi.rejectReview(fileId, reason),
        onSuccess: () => {
            toast.success("تم الرفض");
            queryClient.invalidateQueries({ queryKey: ["pending-reviews"] });
            setRejectDialog({ isOpen: false, fileId: null, fileName: "" });
        },
        onError: () => {
            toast.error("خطأ في الرفض");
        },
    });

    // ─── Handlers ────────────────────────────────────────

    const handleApproveClick = useCallback(
        (row: PendingReviewFile) => {
            setApproveDialog({
                isOpen: true,
                fileId: row.fileID,
                fileName: `${row.fileNumber} — ${row.retireeFullName}`,
            });
        },
        []
    );

    const handleApproveConfirm = useCallback(
        (notes: string) => {
            if (approveDialog.fileId === null) return;
            approveMutation.mutate({
                fileId: approveDialog.fileId,
                notes: notes || undefined,
            });
        },
        [approveDialog.fileId, approveMutation]
    );

    const handleRejectClick = useCallback(
        (row: PendingReviewFile) => {
            setRejectDialog({
                isOpen: true,
                fileId: row.fileID,
                fileName: `${row.fileNumber} — ${row.retireeFullName}`,
            });
        },
        []
    );

    const handleRejectConfirm = useCallback(
        (reason: string) => {
            if (rejectDialog.fileId === null) return;
            rejectMutation.mutate({
                fileId: rejectDialog.fileId,
                reason,
            });
        },
        [rejectDialog.fileId, rejectMutation]
    );

    // ─── Columns ─────────────────────────────────────────

    const columns = [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: PendingReviewFile) => (
                <span className="font-mono font-bold text-primary">
                    {row.fileNumber}
                </span>
            ),
        },
        {
            key: "retireeFullName",
            header: "اسم المتقاعد",
            render: (row: PendingReviewFile) => (
                <span className="font-bold">{row.retireeFullName}</span>
            ),
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row: PendingReviewFile) => (
                <span className="text-sm font-mono">
                    {row.nationalID || "—"}
                </span>
            ),
        },
        {
            key: "reviewRequestedAt",
            header: "تاريخ الطلب",
            render: (row: PendingReviewFile) => (
                <span className="text-sm flex items-center gap-1">
                    <Clock className="w-3 h-3 text-muted-foreground" />
                    {row.reviewRequestedAt
                        ? new Date(row.reviewRequestedAt).toLocaleDateString("ar-EG")
                        : "—"}
                </span>
            ),
        },
        {
            key: "reviewRequestedByUserName",
            header: "مُطلب المراجعة",
            render: (row: PendingReviewFile) => (
                <span className="text-xs flex items-center gap-1 text-muted-foreground">
                    {row.reviewRequestedByUserName ? (
                        <>
                            <User className="w-3 h-3" />
                            {row.reviewRequestedByUserName}
                        </>
                    ) : "—"}
                </span>
            ),
        },
        {
            key: "reviewNotes",
            header: "ملاحظات",
            render: (row: PendingReviewFile) => (
                <span className="text-sm text-muted-foreground truncate max-w-[200px] block">
                    {row.reviewNotes || "—"}
                </span>
            ),
        },
        {
            key: "reviewStatus",
            header: "الحالة",
            render: (row: PendingReviewFile) => {
                if (row.reviewStatus === 1) {
                    return <Badge variant="success">تمت الموافقة</Badge>;
                }
                if (row.reviewStatus === 2) {
                    return <Badge variant="danger">مرفوض</Badge>;
                }
                return <Badge variant="warning">قيد المراجعة</Badge>;
            },
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row: PendingReviewFile) => (
                <div className="flex gap-1 items-center">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="text-blue-600 dark:text-blue-400 hover:bg-blue-500/15 hover:text-blue-700 dark:hover:text-blue-300"
                        title="مراجعة الملف"
                        onClick={() => navigate(`/files/${row.fileID}/review`)}
                    >
                        <Eye className="w-4 h-4 shrink-0" />
                    </Button>
                    {row.reviewStatus === 0 && (
                        <>
                            <Button
                                variant="ghost"
                                size="sm"
                                className="text-emerald-600 dark:text-emerald-400 hover:bg-emerald-500/15 hover:text-emerald-700 dark:hover:text-emerald-300"
                                title="اعتماد"
                                onClick={() => handleApproveClick(row)}
                                disabled={approveMutation.isPending}
                            >
                                <ThumbsUp className="w-4 h-4 shrink-0" />
                            </Button>
                            <Button
                                variant="ghost"
                                size="sm"
                                className="text-red-600 dark:text-red-400 hover:bg-red-500/15 hover:text-red-700 dark:hover:text-red-300"
                                title="رفض"
                                onClick={() => handleRejectClick(row)}
                                disabled={rejectMutation.isPending}
                            >
                                <ThumbsDown className="w-4 h-4 shrink-0" />
                            </Button>
                        </>
                    )}
                </div>
            ),
        },
    ];

    // ─── Render ──────────────────────────────────────────

    return (
        <CrudPageLayout
            title="مراجعة بيانات ملفات التقاعد"
            description="عرض والاطلاع على الملفات المنتظرة للمراجعة والاعتماد"
            icon={<FileSearch className="w-6 h-6" />}
            columns={columns}
            data={paginatedItems}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث برقم الملف / اسم المتقاعد / الرقم الوطني..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            onSearchKeyDown={(e) => {
                if (e.key === "ArrowDown" || e.key === "ArrowUp")
                    e.preventDefault();
            }}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.fileID}
            onRowClick={(row) => navigate(`/files/${row.fileID}`)}
            pagination={{
                page,
                pageSize: 20,
                total: filteredItems.length,
                onPageChange: setPage,
            }}
            headerActions={
                <div className="flex gap-2">
                    <SearchDialog
                        value={searchTerm}
                        onSelect={(val) => { setSearchTerm(val); setPage(1); }}
                        queryKey={["pending-reviews", "dialog-search"]}
                        fetchFn={async (term) => {
                            const q = term.toLowerCase();
                            return {
                                items: pendingFiles.filter(
                                    (f: PendingReviewFile) =>
                                        f.fileNumber.toLowerCase().includes(q) ||
                                        f.retireeFullName.toLowerCase().includes(q) ||
                                        f.nationalID?.toLowerCase().includes(q)
                                ),
                            };
                        }}
                        getName={(r) => r.retireeFullName}
                        getSubtitle={(r) => r.fileNumber}
                        title="بحث بالاسم"
                        placeholder="اكتب اسم المتقاعد أو رقم الملف..."
                        hideTrigger
                    />
                    <Button
                        variant={showFilters ? "primary" : "outline"}
                        size="sm"
                        onClick={() => setShowFilters(!showFilters)}
                    >
                        فلاتر متقدمة
                        {showFilters ? (
                            <ChevronUp className="w-5 h-5 mr-1" />
                        ) : (
                            <ChevronDown className="w-5 h-5 mr-1" />
                        )}
                    </Button>
                </div>
            }
        >
            {/* Stats Cards */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي الملفات"
                    value={stats.total}
                    icon={<FileSearch className="w-5 h-5" />}
                    variant="blue"
                    loading={isLoading}
                />
                <StatCard
                    title="قيد المراجعة"
                    value={stats.pending}
                    icon={<Clock className="w-5 h-5" />}
                    variant="warning"
                    loading={isLoading}
                />
                <StatCard
                    title="تمت الموافقة"
                    value={stats.approved}
                    icon={<CheckCircle className="w-5 h-5" />}
                    variant="emerald"
                    loading={isLoading}
                />
                <StatCard
                    title="مرفوض"
                    value={stats.rejected}
                    icon={<XCircle className="w-5 h-5" />}
                    variant="rose"
                    loading={isLoading}
                />
            </div>

            {/* Advanced Filters */}
            {showFilters && (
                <div className="bg-muted/30 rounded-lg p-4 mb-6 border border-border">
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                        <NativeSelect
                            label="الحالة"
                            value={filters.status || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    status: val as "pending" | "approved" | "rejected" | undefined,
                                }))
                            }
                            options={[
                                { value: "pending", label: "قيد المراجعة" },
                                { value: "approved", label: "تمت الموافقة" },
                                { value: "rejected", label: "مرفوض" },
                            ]}
                            placeholder="الكل"
                        />
                        <div className="flex items-end">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() =>
                                    setFilters({
                                        status: undefined,
                                    })
                                }
                            >
                                مسح الفلاتر
                            </Button>
                        </div>
                    </div>
                </div>
            )}

            {/* Dialogs */}
            <ApproveDialog
                isOpen={approveDialog.isOpen}
                onClose={() =>
                    setApproveDialog({ isOpen: false, fileId: null, fileName: "" })
                }
                onConfirm={handleApproveConfirm}
                fileName={approveDialog.fileName}
                isLoading={approveMutation.isPending}
            />

            <RejectDialog
                isOpen={rejectDialog.isOpen}
                onClose={() =>
                    setRejectDialog({ isOpen: false, fileId: null, fileName: "" })
                }
                onConfirm={handleRejectConfirm}
                fileName={rejectDialog.fileName}
                isLoading={rejectMutation.isPending}
            />
        </CrudPageLayout>
    );
}
