import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    ArrowLeft,
    CheckCircle,
    ClipboardList,
    Eye,
    List,
    Plus,
    Users,
    XCircle,
} from "lucide-react";
import { useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import {
    ministryApi,
    type MinistryApprovalBatchDto,
    type MinistryApprovalItemDto,
} from "@/api/ministry";
import { SearchDialog } from "@/components/common/SearchDialog";
import {
    Badge,
    Button,
    CrudPageLayout,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";

type ViewMode = "batches" | "all-items";

export function MinistryReferralsPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { searchTerm, setSearchTerm, debouncedValue } =
        useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);

    const [viewMode, setViewMode] = useState<ViewMode>("all-items");
    const [selectedBatch, setSelectedBatch] =
        useState<MinistryApprovalBatchDto | null>(null);
    const [processedFilter, setProcessedFilter] = useState<
        "all" | "processed" | "pending"
    >("all");

    const { data: stats } = useQuery({
        queryKey: ["ministry-stats"],
        queryFn: () => ministryApi.getDashboardStats(),
    });

    const { data: batchesData, isLoading: batchesLoading } = useQuery({
        queryKey: ["ministry-batches"],
        queryFn: () => ministryApi.getBatches(),
    });

    const batches = batchesData?.data || [];

    const { data: itemsData, isLoading: itemsLoading } = useQuery({
        queryKey: [
            "ministry-batch-items",
            selectedBatch?.batchID,
            page,
            debouncedValue,
        ],
        queryFn: () =>
            ministryApi.getBatchItems(
                selectedBatch!.batchID,
                page,
                20,
                debouncedValue || undefined
            ),
        enabled: !!selectedBatch,
    });

    const { data: allItemsData, isLoading: allItemsLoading } = useQuery({
        queryKey: [
            "ministry-all-items",
            page,
            debouncedValue,
            processedFilter,
        ],
        queryFn: () =>
            ministryApi.getAllItems(
                page,
                20,
                debouncedValue || undefined,
                processedFilter === "processed"
                    ? true
                    : processedFilter === "pending"
                        ? false
                        : undefined
            ),
        enabled: viewMode === "all-items" && !selectedBatch,
    });

    const items = selectedBatch
        ? itemsData?.data?.items || []
        : allItemsData?.data?.items || [];
    const itemsTotal = selectedBatch
        ? itemsData?.data?.totalCount || 0
        : allItemsData?.data?.totalCount || 0;

    const filteredItems = useMemo(() => {
        if (viewMode === "all-items") return items;
        if (processedFilter === "all") return items;
        if (processedFilter === "processed")
            return items.filter((i) => i.isProcessed);
        return items.filter((i) => !i.isProcessed);
    }, [items, processedFilter, viewMode]);

    const processMutation = useMutation({
        mutationFn: (itemId: number) => ministryApi.processItem(itemId),
        onSuccess: () => {
            toast.success("تم التعليق كمعالج");
            queryClient.invalidateQueries({
                queryKey: ["ministry-batch-items"],
            });
            queryClient.invalidateQueries({
                queryKey: ["ministry-all-items"],
            });
            queryClient.invalidateQueries({ queryKey: ["ministry-stats"] });
        },
        onError: () => {
            toast.error("خطأ في المعالجة");
        },
    });

    const batchColumns = [
        {
            key: "batchNumber",
            header: "رقم الكشف",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm font-mono font-bold">
                    {row.batchNumber}
                </span>
            ),
        },
        {
            key: "batchDate",
            header: "تاريخ الكشف",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm">
                    {row.batchDate
                        ? new Date(row.batchDate).toLocaleDateString("ar-SA")
                        : "—"}
                </span>
            ),
        },
        {
            key: "receivedDate",
            header: "تاريخ الاستلام",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm">
                    {row.receivedDate
                        ? new Date(row.receivedDate).toLocaleDateString("ar-SA")
                        : "—"}
                </span>
            ),
        },
        {
            key: "totalCount",
            header: "العدد الإجمالي",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm font-medium">{row.totalCount}</span>
            ),
        },
        {
            key: "processedCount",
            header: "المعالج",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm">{row.processedCount}</span>
            ),
        },
        {
            key: "remaining",
            header: "المتبقي",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm font-bold text-destructive">
                    {row.totalCount - row.processedCount}
                </span>
            ),
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: MinistryApprovalBatchDto) => {
                const variant =
                    row.status === "Processed"
                        ? "success"
                        : row.status === "Partial"
                            ? "warning"
                            : "outline";
                return <Badge variant={variant}>{row.status}</Badge>;
            },
        },
        {
            key: "importedByUser",
            header: "رافع الكشف",
            render: (row: MinistryApprovalBatchDto) => (
                <span className="text-sm">{row.importedByUser || "—"}</span>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: MinistryApprovalBatchDto) => (
                <Button
                    variant="ghost-action"
                    size="sm"
                    title="عرض البنود"
                    onClick={(e) => {
                        e.stopPropagation();
                        setSelectedBatch(row);
                        setPage(1);
                        setSearchTerm("");
                    }}
                >
                    <Eye className="w-4 h-4" />
                </Button>
            ),
        },
    ];

    const renderMobileBatchCard = (row: MinistryApprovalBatchDto) => (
        <div className="p-3 space-y-2">
            <div className="flex items-center justify-between">
                <span className="font-mono font-bold text-sm">{row.batchNumber}</span>
                <Badge variant={row.status === "Processed" ? "success" : row.status === "Partial" ? "warning" : "outline"}>
                    {row.status}
                </Badge>
            </div>
            <div className="text-xs text-muted-foreground space-y-1">
                <div className="flex justify-between">
                    <span>التاريخ:</span>
                    <span>{row.batchDate ? new Date(row.batchDate).toLocaleDateString("ar-SA") : "—"}</span>
                </div>
                <div className="flex justify-between">
                    <span>العدد:</span>
                    <span className="font-medium">{row.totalCount}</span>
                </div>
                <div className="flex justify-between">
                    <span>المعالج:</span>
                    <span>{row.processedCount}</span>
                </div>
                <div className="flex justify-between">
                    <span>المتبقي:</span>
                    <span className="font-bold text-destructive">{row.totalCount - row.processedCount}</span>
                </div>
            </div>
            <Button variant="ghost-action" size="sm" className="w-full text-xs"
                onClick={() => { setSelectedBatch(row); setPage(1); setSearchTerm(""); }}>
                <Eye className="w-3.5 h-3.5 mr-1" /> عرض البنود
            </Button>
        </div>
    );

    const itemColumns = [
        {
            key: "batchNumber",
            header: "رقم الكشف",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm font-mono">
                    {row.batchNumber || "—"}
                </span>
            ),
        },
        {
            key: "serialNumber",
            header: "التسلسلي",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm font-mono">{row.serialNumber}</span>
            ),
        },
        {
            key: "fullName",
            header: "الاسم",
            render: (row: MinistryApprovalItemDto) => (
                <span className="font-bold">{row.fullName}</span>
            ),
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm font-mono text-primary font-bold">
                    {row.militaryNumber}
                </span>
            ),
        },
        {
            key: "rankName",
            header: "الرتبة",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm">{row.rankName || "—"}</span>
            ),
        },
        {
            key: "unitName",
            header: "الوحدة",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm">{row.unitName || "—"}</span>
            ),
        },
        {
            key: "enrollmentDate",
            header: "تاريخ التكليف",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm">
                    {row.enrollmentDate
                        ? new Date(row.enrollmentDate).toLocaleDateString(
                            "ar-SA"
                        )
                        : "—"}
                </span>
            ),
        },
        {
            key: "retirementDate",
            header: "تاريخ التقاعد",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm">
                    {row.retirementDate
                        ? new Date(row.retirementDate).toLocaleDateString(
                            "ar-SA"
                        )
                        : "—"}
                </span>
            ),
        },
        {
            key: "salary",
            header: "الراتب",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm font-mono">
                    {row.salary?.toLocaleString() || "—"}
                </span>
            ),
        },
        {
            key: "categoryName",
            header: "الفئة",
            render: (row: MinistryApprovalItemDto) => (
                <Badge variant="outline">{row.categoryName || "—"}</Badge>
            ),
        },
        {
            key: "linkNumber",
            header: "رقم الربط",
            render: (row: MinistryApprovalItemDto) => (
                <span className="text-sm font-mono">
                    {row.linkNumber || "—"}
                </span>
            ),
        },
        {
            key: "isProcessed",
            header: "الحالة",
            render: (row: MinistryApprovalItemDto) => (
                <Badge variant={row.isProcessed ? "success" : "warning"}>
                    {row.isProcessed ? "معالج" : "غير معالج"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: MinistryApprovalItemDto) => (
                <div className="flex gap-1">
                    {row.retireeID ? (
                        <Button
                            variant="ghost-action"
                            size="sm"
                            title="عرض المتقاعد"
                            onClick={(e) => {
                                e.stopPropagation();
                                navigate(`/retirees/${row.retireeID}`);
                            }}
                        >
                            <Eye className="w-4 h-4" />
                        </Button>
                    ) : (
                        <Button
                            variant="ghost-action"
                            size="sm"
                            title="إنشاء ملف تقاعد"
                            onClick={(e) => {
                                e.stopPropagation();
                                navigate(
                                    `/files?referralItemId=${row.approvalItemID}`
                                );
                            }}
                        >
                            <Plus className="w-4 h-4" />
                        </Button>
                    )}
                    {!row.isProcessed && (
                        <Button
                            variant="ghost-action"
                            size="sm"
                            title="تعليم كمعالج"
                            onClick={(e) => {
                                e.stopPropagation();
                                processMutation.mutate(row.approvalItemID);
                            }}
                        >
                            <CheckCircle className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            ),
        },
    ];

    const renderMobileItemCard = (row: MinistryApprovalItemDto) => (
        <div className="p-3 space-y-2">
            <div className="flex items-center justify-between">
                <span className="font-bold text-sm">{row.fullName}</span>
                <Badge variant={row.isProcessed ? "success" : "warning"}>
                    {row.isProcessed ? "معالج" : "غير معالج"}
                </Badge>
            </div>
            <div className="text-xs text-muted-foreground space-y-1">
                <div className="flex justify-between">
                    <span>الرقم العسكري:</span>
                    <span className="font-mono font-bold text-primary">{row.militaryNumber}</span>
                </div>
                <div className="flex justify-between">
                    <span>الرتبة:</span>
                    <span>{row.rankName || "—"}</span>
                </div>
                <div className="flex justify-between">
                    <span>الوحدة:</span>
                    <span>{row.unitName || "—"}</span>
                </div>
                {row.salary && (
                    <div className="flex justify-between">
                        <span>الراتب:</span>
                        <span className="font-mono">{row.salary.toLocaleString()}</span>
                    </div>
                )}
            </div>
            <div className="flex gap-1 pt-1 border-t">
                {row.retireeID ? (
                    <Button variant="ghost-action" size="sm" className="flex-1 text-xs"
                        onClick={() => navigate(`/retirees/${row.retireeID}`)}>
                        <Eye className="w-3.5 h-3.5 mr-1" /> عرض
                    </Button>
                ) : (
                    <Button variant="ghost-action" size="sm" className="flex-1 text-xs"
                        onClick={() => navigate(`/files?referralItemId=${row.approvalItemID}`)}>
                        <Plus className="w-3.5 h-3.5 mr-1" /> إنشاء ملف
                    </Button>
                )}
                {!row.isProcessed && (
                    <Button variant="ghost-action" size="sm" className="flex-1 text-xs"
                        onClick={() => processMutation.mutate(row.approvalItemID)}>
                        <CheckCircle className="w-3.5 h-3.5 mr-1" /> معالج
                    </Button>
                )}
            </div>
        </div>
    );

    if (selectedBatch) {
        return (
            <CrudPageLayout
                title={`بنود الكشف: ${selectedBatch.batchNumber}`}
                description={`تاريخ الكشف: ${new Date(selectedBatch.batchDate).toLocaleDateString("ar-SA")} — العدد: ${selectedBatch.totalCount}`}
                icon={<ClipboardList className="w-6 h-6" />}
                columns={itemColumns}
                data={filteredItems}
                loading={itemsLoading}
                searchable
                searchInHeader
                searchPlaceholder="بحث بالاسم / الرقم العسكري..."
                searchValue={searchTerm}
                onSearchChange={setSearchTerm}
                onSearchKeyDown={(e) => {
                    if (e.key === "ArrowDown" || e.key === "ArrowUp")
                        e.preventDefault();
                }}
                searchInputRef={searchInputRef}
                rowKey={(row) => row.approvalItemID}
                renderMobileCard={renderMobileItemCard}
                onRowClick={(row) => {
                    if (row.retireeID) navigate(`/retirees/${row.retireeID}`);
                    else navigate(`/files?referralItemId=${row.approvalItemID}`);
                }}
                pagination={{
                    page,
                    pageSize: 20,
                    total: itemsTotal,
                    onPageChange: setPage,
                }}
                headerActions={
                    <div className="flex flex-wrap gap-2 items-center">
                        <SearchDialog
                            value={searchTerm}
                            onSelect={(name) => {
                                setSearchTerm(name);
                                setPage(1);
                            }}
                            queryKey={["ministry-items-search"]}
                            fetchFn={(term) =>
                                ministryApi
                                    .getAllItems(1, 100, term)
                                    .then((r) => ({ items: r.data?.items || [] }))
                            }
                            getName={(item) => item.fullName}
                            getSubtitle={(item) => item.militaryNumber}
                            title="بحث عن محال"
                            placeholder="بحث بالاسم أو الرقم العسكري..."
                            hideTrigger
                        />
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                                setSelectedBatch(null);
                                setPage(1);
                            }}
                        >
                            <ArrowLeft className="w-4 h-4 mr-1" />
                            العودة
                        </Button>
                        <div className="flex gap-1">
                            <Button
                                variant={
                                    processedFilter === "all"
                                        ? "primary"
                                        : "outline"
                                }
                                size="sm"
                                onClick={() => setProcessedFilter("all")}
                            >
                                الكل
                            </Button>
                            <Button
                                variant={
                                    processedFilter === "processed"
                                        ? "primary"
                                        : "outline"
                                }
                                size="sm"
                                onClick={() => setProcessedFilter("processed")}
                            >
                                معالج
                            </Button>
                            <Button
                                variant={
                                    processedFilter === "pending"
                                        ? "primary"
                                        : "outline"
                                }
                                size="sm"
                                onClick={() => setProcessedFilter("pending")}
                            >
                                غير معالج
                            </Button>
                        </div>
                    </div>
                }
            />
        );
    }

    if (viewMode === "all-items") {
        return (
            <CrudPageLayout
                title="جميع المحالين"
                description="عرض جميع المحالين الى التقاعد عبر جميع الكشوفات"
                icon={<Users className="w-6 h-6" />}
                columns={itemColumns}
                data={filteredItems}
                loading={allItemsLoading}
                searchable
                searchInHeader
                searchPlaceholder="بحث بالاسم / الرقم العسكري..."
                searchValue={searchTerm}
                onSearchChange={setSearchTerm}
                onSearchKeyDown={(e) => {
                    if (e.key === "ArrowDown" || e.key === "ArrowUp")
                        e.preventDefault();
                }}
                searchInputRef={searchInputRef}
                rowKey={(row) => row.approvalItemID}
                renderMobileCard={renderMobileItemCard}
                onRowClick={(row) => {
                    if (row.retireeID) navigate(`/retirees/${row.retireeID}`);
                    else navigate(`/files?referralItemId=${row.approvalItemID}`);
                }}
                pagination={{
                    page,
                    pageSize: 20,
                    total: itemsTotal,
                    onPageChange: setPage,
                }}
                headerActions={
                    <div className="flex flex-wrap gap-2 items-center">
                        <SearchDialog
                            value={searchTerm}
                            onSelect={(name) => {
                                setSearchTerm(name);
                                setPage(1);
                            }}
                            queryKey={["ministry-all-items-search"]}
                            fetchFn={(term) =>
                                ministryApi
                                    .getAllItems(1, 100, term)
                                    .then((r) => ({ items: r.data?.items || [] }))
                            }
                            getName={(item) => item.fullName}
                            getSubtitle={(item) => item.militaryNumber}
                            title="بحث عن محال"
                            placeholder="بحث بالاسم أو الرقم العسكري..."
                            hideTrigger
                        />
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                                setViewMode("batches");
                                setPage(1);
                                setSearchTerm("");
                                setProcessedFilter("all");
                            }}
                        >
                            <ClipboardList className="w-4 h-4 mr-1" />
                            الكشوفات
                        </Button>
                        <div className="flex gap-1">
                            <Button
                                variant={
                                    processedFilter === "all"
                                        ? "primary"
                                        : "outline"
                                }
                                size="sm"
                                onClick={() => {
                                    setProcessedFilter("all");
                                    setPage(1);
                                }}
                            >
                                الكل
                            </Button>
                            <Button
                                variant={
                                    processedFilter === "processed"
                                        ? "primary"
                                        : "outline"
                                }
                                size="sm"
                                onClick={() => {
                                    setProcessedFilter("processed");
                                    setPage(1);
                                }}
                            >
                                معالج
                            </Button>
                            <Button
                                variant={
                                    processedFilter === "pending"
                                        ? "primary"
                                        : "outline"
                                }
                                size="sm"
                                onClick={() => {
                                    setProcessedFilter("pending");
                                    setPage(1);
                                }}
                            >
                                غير معالج
                            </Button>
                        </div>
                    </div>
                }
            />
        );
    }

    return (
        <CrudPageLayout
            title="فحص المحالين إلى التقاعد"
            description="إدارة كشوفات والمحالين الى التقاعد"
            icon={<ClipboardList className="w-6 h-6" />}
            columns={batchColumns}
            data={batches}
            loading={batchesLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث برقم الكشف..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            onSearchKeyDown={(e) => {
                if (e.key === "ArrowDown" || e.key === "ArrowUp")
                    e.preventDefault();
            }}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.batchID}
            renderMobileCard={renderMobileBatchCard}
            pagination={{
                page: 1,
                pageSize: 50,
                total: batches.length,
                onPageChange: () => { },
            }}
            headerActions={
                <Button
                    variant="primary"
                    size="sm"
                    onClick={() => {
                        setViewMode("all-items");
                        setPage(1);
                        setSearchTerm("");
                        setProcessedFilter("all");
                    }}
                >
                    <List className="w-4 h-4 mr-1" />
                    جميع المحالين
                </Button>
            }
        >
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي الكشوفات"
                    value={stats?.data?.totalBatches || 0}
                    icon={<ClipboardList className="w-5 h-5" />}
                    variant="blue"
                />
                <StatCard
                    title="السجلات المعالجة"
                    value={stats?.data?.processedRecords || 0}
                    icon={<CheckCircle className="w-5 h-5" />}
                    variant="emerald"
                />
                <StatCard
                    title="السجلات المعلقة"
                    value={stats?.data?.pendingRecords || 0}
                    icon={<XCircle className="w-5 h-5" />}
                    variant="amber"
                />
                <StatCard
                    title="فروق المطابقة"
                    value={stats?.data?.reconciliationGaps || 0}
                    icon={<Users className="w-5 h-5" />}
                    variant="rose"
                />
            </div>
        </CrudPageLayout>
    );
}
