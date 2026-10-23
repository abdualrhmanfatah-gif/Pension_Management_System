import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
    ArrowRight,
    CheckCircle2,
    CheckSquare,
    Clock,
    FileSpreadsheet,
    FileText,
    History,
    List,
    Pencil,
    Play,
    Plus,
    Printer,
    ShieldCheck,
    Square,
    Trash2,
    Upload,
    Users} from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { toast } from "sonner";

import { ministryApi } from "@/api/ministry";
import { ExcelImportDialog } from "@/components/domain/admin/ExcelImportDialog";
import { AddReferralItemDialog } from "@/components/domain/force/AddReferralItemDialog";
import { Card, Heading, Input, StatCard, Tabs, TabsContent,TabsList, TabsTrigger, useConfirm } from "@/components/ui";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { type CustomColumnDef,DataTable } from "@/components/ui/DataTable";
import { useApiMutation } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

import { ForceAuditTab } from "../ministry-approvals/ForceAuditTab";
import { ReconciliationTab } from "../ministry-approvals/ReconciliationTab";

// ─── Types ─────────────────────────────────────────────



interface ItemDto {
    approvalItemID: number;
    serialNumber: number;
    militaryNumber: string;
    fullName: string;
    rankID?: number;
    rankName?: string;
    categoryID?: number;
    unitName?: string;
    enrollmentDate?: string;
    retirementDate?: string;
    salary?: number;
    categoryName?: string;
    notes?: string;
    linkNumber?: string;
    isProcessed: boolean;
    retireeID?: number;
    batchNumber?: string;
    batchDate?: string;
    processedAt?: string;
    processedByUser?: string;
    gender?: number;
}

// ─── Page ──────────────────────────────────────────────

export function BatchDetailsPage() {
    const { id: batchIdStr } = useParams<{ id: string }>();
    const batchId = Number(batchIdStr);
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();

    const canManage = useHasPermission("Ministry.Manage", "System.Admin");
    const [importOpen, setImportOpen] = useState(false);
    const [itemDialogOpen, setItemDialogOpen] = useState(false);
    const [editingItem, setEditingItem] = useState<ItemDto | null>(null);
    const { searchTerm, setSearchTerm, debouncedValue: debouncedSearch } = useDebouncedSearch();
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchParams, setSearchParams] = useSearchParams();
    const [activeTab, setActiveTab] = useState(searchParams.get("tab") || "items");
    const [selectedIds, setSelectedIds] = useState<number[]>([]);
    const [focusedIndex, setFocusedIndex] = useState(-1);
    const focusedIndexRef = useRef(-1);
    const searchInputRef = useRef<HTMLInputElement>(null);

    // ── Debounce Search ────────────────────────────────
    useEffect(() => {
        setPage(1);
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, [debouncedSearch]);

    // ─── Data Fetching ──────────────────────────────────
    const { data: batchRes, isLoading: loadingBatch, isError: batchError } = useQuery({
        queryKey: ["force", "batch", batchId],
        queryFn: () => ministryApi.getBatchById(batchId),
        enabled: !!batchId
    });

    const { data: itemsRes, isLoading: loadingItems } = useQuery({
        queryKey: ["force", "items", batchId, page, pageSize, debouncedSearch],
        queryFn: () => ministryApi.getBatchItems(batchId, page, pageSize, debouncedSearch),
        enabled: !!batchId && activeTab === "items",
        staleTime: 30000 // Cache for 30 seconds
    });

    const batch = batchRes?.data;
    const filteredItems = useMemo(() => itemsRes?.data?.items as unknown as ItemDto[] || [], [itemsRes]);
    const totalCount = itemsRes?.data?.totalCount || 0;
    const canAddItems = canManage && !!batch;

    // ─── Mutations ─────────────────────────────────────
    const bulkProcessMutation = useApiMutation(
        (ids: number[]) => ministryApi.processBulkApprovals(ids),
        {
            successMessage: "تمت المعالجة الجماعية بنجاح",
            invalidateKeys: [["force", "items"], ["force", "batch", batchId], ["force", "analytics"]],
            onSuccess: () => setSelectedIds([])
        }
    );

    const deleteBatchMutation = useApiMutation(
        (id: number) => ministryApi.deleteBatch(id),
        {
            successMessage: "تم حذف الكشف بنجاح",
            invalidateKeys: [["force", "batches"]],
            onSuccess: () => navigate("/pension/ministry-batches")
        }
    );

    const processItemMutation = useApiMutation(
        (itemId: number) => ministryApi.processItem(itemId),
        {
            successMessage: "تمت معالجة الحالة بنجاح",
            invalidateKeys: [["force", "items"], ["force", "batch", batchId], ["force", "analytics"]]
        }
    );

    const deleteItemMutation = useApiMutation(
        (itemId: number) => ministryApi.deleteManualItem(itemId),
        {
            successMessage: "تم حذف القيد بنجاح",
            invalidateKeys: [["force", "items"], ["force", "batch", batchId], ["force", "analytics"], ["force-batches"], ["ministry-batches"], ["ministry-batch-items"], ["ministry-all-items"], ["ministry-stats"]]
        }
    );

    // ── Keyboard Navigation ────────────────────────────
    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setFocusedIndex(prev => {
                const next = prev + 1;
                const clamped = next < filteredItems.length ? next : prev;
                focusedIndexRef.current = clamped;
                return clamped;
            });
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            setFocusedIndex(prev => {
                const clamped = prev > 0 ? prev - 1 : prev;
                focusedIndexRef.current = clamped;
                return clamped;
            });
        } else if (e.key === "Enter") {
            e.preventDefault();
            const idx = focusedIndexRef.current;
            if (idx >= 0 && idx < filteredItems.length) {
                const item = filteredItems[idx];
                if (!item.isProcessed && canManage) {
                    processItemMutation.mutate(item.approvalItemID);
                    setSearchTerm("");
                }
            }
        } else if (e.key === "Escape") {
            setFocusedIndex(-1);
            focusedIndexRef.current = -1;
        }
    }, [filteredItems, canManage, processItemMutation]);

    // Scroll to focused row (supports both data-row-index and data-index)
    useEffect(() => {
        if (focusedIndex >= 0) {
            const row = document.querySelector(`[data-row-index="${focusedIndex}"]`)
                      || document.querySelector(`[data-index="${focusedIndex}"]`);
            if (row) {
                row.scrollIntoView({ behavior: "smooth", block: "nearest" });
            }
        }
    }, [focusedIndex]);

    // ─── Selection Logic ───────────────────────────────
    const unprocessedItems = useMemo(() => filteredItems.filter(i => !i.isProcessed), [filteredItems]);

    const toggleSelectAll = useCallback(() => {
        if (selectedIds.length === unprocessedItems.length) {
            setSelectedIds([]);
        } else {
            setSelectedIds(unprocessedItems.map(i => i.approvalItemID));
        }
    }, [selectedIds.length, unprocessedItems]);

    const toggleSelect = useCallback((id: number) => {
        setSelectedIds(prev =>
            prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]
        );
    }, []);

    // ─── Handlers ──────────────────────────────────────
    const handleBulkProcess = useCallback(async () => {
        if (selectedIds.length === 0) return;

        const confirmed = await confirm({
            title: "تأكيد المعالجة الجماعية",
            description: `هل أنت متأكد من معالجة ${selectedIds.length} إحالة؟ سيتم إنشاء ملفات تقاعد جديدة للإحالات غير الموجودة.`
        });

        if (confirmed) {
            bulkProcessMutation.mutate(selectedIds);
        }
    }, [selectedIds, confirm, bulkProcessMutation]);

    const handleExport = useCallback(async (format: 'excel' | 'pdf') => {
        try {
            const response = await ministryApi.exportBatch(batchId, format);
            const blob = new Blob([response.data], {
                type: format === 'excel'
                    ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
                    : 'application/pdf'
            });
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `Ministry_Batch_${batchId}.${format === 'excel' ? 'xlsx' : 'pdf'}`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
            toast.info("تم بدء التنزيل");
        } catch {
            toast.error("فشل التصدير");
        }
    }, [batchId]);

    // ─── Columns ───────────────────────────────────────
    const columns = useMemo<CustomColumnDef<ItemDto>[]>(() => [
        {
            key: "selection",
            header: "",
            render: (row) => row.isProcessed ? null : (
                <Button variant="ghost" size="sm" className="p-1 h-auto w-auto" onClick={() => toggleSelect(row.approvalItemID)}>
                    {selectedIds.includes(row.approvalItemID) ? (
                        <CheckSquare className="w-4 h-4 text-primary" />
                    ) : (
                        <Square className="w-4 h-4 text-muted-foreground" />
                    )}
                </Button>
            ),
            className: "w-10"
        },
        { key: "serialNumber", header: "م", accessorKey: "serialNumber", className: "w-12 text-center font-mono text-xs text-muted-foreground" },
        {
            key: "fullName",
            header: "الاسم الكامل",
            render: (row) => (
                <div className="flex flex-col">
                    <span className="font-semibold text-sm">{row.fullName}</span>
                    <span className="text-xs font-mono text-muted-foreground">{row.militaryNumber}</span>
                </div>
            )
        },
        {
            key: "gender",
            header: "الجنس",
            render: (row) => (
                <span className="text-xs">
                    {row.gender === 1 ? "ذكر" : row.gender === 2 ? "أنثى" : "—"}
                </span>
            )
        },
        { key: "rankName", header: "الرتبة", accessorKey: "rankName", className: "text-sm" },
        { key: "unitName", header: "الوحدة", accessorKey: "unitName", className: "text-sm" },
        { key: "linkNumber", header: "الربط", accessorKey: "linkNumber", className: "text-sm font-mono" },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isProcessed ? "success" : "warning"} className="h-5 text-[10px] sm:text-xs px-2">
                    {row.isProcessed ? "معالج" : "قيد الانتظار"}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row) => (
                <div className="flex items-center gap-2 flex-wrap">
                    {canManage && !row.isProcessed && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 px-2 gap-1 text-xs text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50"
                            onClick={() => processItemMutation.mutate(row.approvalItemID)}
                            loading={processItemMutation.isPending}
                        >
                            <Play className="w-3 h-3" /> معالجة
                        </Button>
                    )}
                    {canManage && !row.isProcessed && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 px-2 gap-1 text-xs text-blue-600 hover:text-blue-700 hover:bg-blue-50"
                            onClick={() => setEditingItem(row)}
                        >
                            <Pencil className="w-3 h-3" /> تعديل
                        </Button>
                    )}
                    {canManage && !row.isProcessed && (
                        <Button
                            variant="ghost"
                            size="icon"
                            className="h-7 w-7 text-destructive hover:text-destructive hover:bg-destructive/10"
                            title="حذف"
                            onClick={() => {
                                confirm({
                                    title: "حذف القيد",
                                    description: "هل أنت متأكد من حذف هذا القيد من الكشف؟",
                                    variant: "danger"
                                }).then(ok => ok && deleteItemMutation.mutate(row.approvalItemID));
                            }}
                        >
                            <Trash2 className="w-3.5 h-3.5" />
                        </Button>
                    )}
                    {row.isProcessed && row.retireeID && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 px-2 gap-1 text-xs"
                            onClick={() => navigate(`/retirees/${row.retireeID}`)}
                        >
                            <Users className="w-3 h-3" /> الملف
                        </Button>
                    )}
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-7 w-7 text-blue-600"
                        onClick={() => window.open(`/force/items/${row.approvalItemID}/print`, '_blank')}
                    >
                        <Printer className="w-3.5 h-3.5" />
                    </Button>
                </div>
            )
        }
    ], [selectedIds, toggleSelect, processItemMutation, deleteItemMutation, navigate, canManage, confirm]);

    if (loadingBatch) return <div className="p-6 sm:p-12 text-center animate-pulse text-muted-foreground">جاري تحميل بيانات الإحالة...</div>;
    if (batchError) return <div className="p-6 sm:p-12 text-center text-destructive bg-destructive/5 rounded-2xl m-6">تعذر تحميل بيانات الإحالة</div>;
    if (!batch) return <div className="p-6 sm:p-12 text-center text-destructive bg-destructive/5 rounded-2xl m-6">الإحالة غير موجودة أو تم حذفها</div>;

    return (
        <div className="p-4 sm:p-5 space-y-4 animate-in fade-in duration-500">
            {ConfirmDialogElement}

            {/* Header */}
            <Card className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 p-4  border-border">
                <div className="flex items-center gap-4">
                    <Button variant="ghost" size="icon" className="rounded-full" onClick={() => navigate("/pension/ministry-batches")}>
                        <ArrowRight className="w-5 h-5" />
                    </Button>
                    <div>
                        <div className="flex items-center gap-2">
                            <Heading level="h1" className="text-base md:text-lg lg:text-xl font-bold text-foreground">كشف رقم: {batch.batchNumber}</Heading>
                            <Badge variant={batch.status === "مكتمل" ? "success" : "outline"} className="px-3">
                                {batch.status}
                            </Badge>
                        </div>
                        <p className="text-xs text-muted-foreground mt-0.5">
                            تاريخ الكشف: {formatDate(batch.batchDate)} | تاريخ الاستلام: {formatDate(batch.receivedDate)}
                        </p>
                    </div>
                </div>

                <div className="flex items-center gap-2">
                    {canManage && selectedIds.length > 0 && (
                        <Button
                            variant="primary"
                            className="gap-2 bg-emerald-600 hover:bg-emerald-700 animate-in zoom-in-95"
                            onClick={handleBulkProcess}
                            loading={bulkProcessMutation.isPending}
                        >
                            <ShieldCheck className="w-4 h-4" />
                            معالجة المحدد ({selectedIds.length})
                        </Button>
                    )}

                    {canManage && (
                        <div className="flex items-center gap-2">
                            <Button
                                variant="outline"
                                size="sm"
                                className="gap-2 rounded-xl text-emerald-600 border-emerald-200 hover:bg-emerald-50"
                                onClick={() => handleExport('excel')}
                            >
                                <FileSpreadsheet className="w-4 h-4" /> Excel
                            </Button>
                            <Button
                                variant="outline"
                                size="sm"
                                className="gap-2 rounded-xl text-rose-600 border-rose-200 hover:bg-rose-50"
                                onClick={() => handleExport('pdf')}
                            >
                                <FileText className="w-4 h-4" /> طباعة
                            </Button>
                        </div>
                    )}

                    {canAddItems && (
                        <Button variant="outline" className="gap-2 rounded-xl" onClick={() => setItemDialogOpen(true)}>
                            <Plus className="w-4 h-4" /> إضافة محال
                        </Button>
                    )}
                    {canManage && (
                        <Button variant="outline" className="gap-2 rounded-xl" onClick={() => setImportOpen(true)}>
                            <Upload className="w-4 h-4" /> استيراد Excel
                        </Button>
                    )}
                    {canManage && (
                        <Button variant="destructive" className="gap-2 rounded-xl" onClick={() => {
                            confirm({
                                title: "حذف الكشف",
                                description: "هل أنت متأكد من حذف هذا الكشف بالكامل؟ سيتم حذف كافة الحالات غير المعالجة.",
                                variant: "danger"
                            }).then(ok => ok && deleteBatchMutation.mutate(batch.batchID));
                        }}>
                            <Trash2 className="w-4 h-4" /> حذف
                        </Button>
                    )}
                </div>
            </Card>

            {/* Stats Overview */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <StatCard
                    title="إجمالي الإحالات"
                    value={batch.totalCount}
                    icon={<List className="w-5 h-5" />}
                    variant="blue"
                />
                <StatCard
                    title="الإحالات المعالجة"
                    value={batch.processedCount}
                    icon={<CheckCircle2 className="w-5 h-5" />}
                    variant="emerald"
                />
                <StatCard
                    title="بانتظار المعالجة"
                    value={batch.totalCount - batch.processedCount}
                    icon={<Clock className="w-5 h-5" />}
                    variant="amber"
                />
            </div>

            <Tabs value={activeTab} onValueChange={(v) => { setActiveTab(v); setSearchParams((p) => { p.set("tab", v); return p; }, { replace: true }); }} dir="rtl" className="w-full">
                <div className="flex flex-col sm:flex-row sm:items-center gap-2">
                    <div className="overflow-x-auto pb-1 scrollbar-hide shrink-0">
                        <TabsList className="bg-muted/50 p-1 rounded-xl h-11">
                            <TabsTrigger value="items" className="rounded-lg gap-2 data-[state=active]:bg-background data-[state=active]:">
                                <List className="w-4 h-4" /> الحالات في الإحالة
                            </TabsTrigger>
                            <TabsTrigger value="reconciliation" className="rounded-lg gap-2 data-[state=active]:bg-background data-[state=active]:">
                                <CheckCircle2 className="w-4 h-4" /> تحليل المطابقة
                            </TabsTrigger>
                            <TabsTrigger value="audit" className="rounded-lg gap-2 data-[state=active]:bg-background data-[state=active]:">
                                <History className="w-4 h-4" /> سجل الحوكمة
                            </TabsTrigger>
                        </TabsList>
                    </div>
                    <div className="flex-1 flex justify-center">
                        <div className="w-full sm:w-96 lg:w-[28rem]">
                            <Input
                                ref={searchInputRef}
                                type="search"
                                placeholder="بحث في الحالات..."
                                value={searchTerm}
                                onChange={e => setSearchTerm(e.target.value)}
                                onKeyDown={handleSearchKeyDown}
                            />
                        </div>
                    </div>
                </div>

                <TabsContent value="items" className="mt-4 space-y-4 animate-in slide-in-from-bottom-2 duration-300">
                    {unprocessedItems.length > 0 && (
                        <div className="flex items-center gap-2 flex-wrap">
                            <span className="text-xs text-muted-foreground">
                                {selectedIds.length > 0 ? `تم تحديد ${selectedIds.length} إحالة` : "يمكنك تحديد إحالات متعددة للمعالجة الجماعية"}
                            </span>
                            <Button variant="ghost" size="sm" onClick={toggleSelectAll} className="text-xs min-h-[44px] px-3 rounded-lg hover:bg-primary/5">
                                {selectedIds.length === unprocessedItems.length ? "إلغاء تحديد الكل" : "تحديد كافة المعلقين"}
                            </Button>
                        </div>
                    )}

                    <Card className="">
                        <DataTable
                            columns={columns}
                            data={filteredItems}
                            loading={loadingItems}
                            focusedRowIndex={focusedIndex}
                            pagination={{
                                page: page,
                                pageSize: pageSize,
                                total: totalCount,
                                onPageChange: (p) => setPage(p)
                            }}
                        />
                    </Card>
                </TabsContent>

                <TabsContent value="reconciliation" className="mt-4 animate-in slide-in-from-bottom-2 duration-300">
                    <ReconciliationTab batchId={batchId} />
                </TabsContent>

                <TabsContent value="audit" className="mt-4 animate-in slide-in-from-bottom-2 duration-300">
                    <ForceAuditTab batchId={batchId} />
                </TabsContent>
            </Tabs>

            <ExcelImportDialog
                open={importOpen}
                onOpenChange={setImportOpen}
                batchId={batchId}
                onImported={() => {
                    queryClient.invalidateQueries({ queryKey: ["force", "items", batchId] });
                    setImportOpen(false);
                }}
            />

            <AddReferralItemDialog
                open={itemDialogOpen || !!editingItem}
                onOpenChange={(open) => {
                    if (!open) {
                        setItemDialogOpen(false);
                        setEditingItem(null);
                    }
                }}
                batchId={batchId}
                item={editingItem}
            />
        </div>
    );
}
