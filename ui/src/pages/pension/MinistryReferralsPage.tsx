import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    ArrowLeft,
    CheckCircle,
    ChevronDown,
    ClipboardList,
    Eye,
    Filter,
    List,
    Pencil,
    Play,
    Plus,
    Trash2,
    Users,
    X,
    XCircle,
} from "lucide-react";
import { useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { hrApi } from "@/api/hr";
import {
    ministryApi,
    type MinistryApprovalBatchDto,
    type MinistryApprovalItemDto,
} from "@/api/ministry";
import { lookupApi } from "@/api/pension";
import { SearchDialog } from "@/components/common/SearchDialog";
import { AddReferralItemDialog } from "@/components/domain/force/AddReferralItemDialog";
import {
    Badge,
    Button,
    CrudPageLayout,
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
    StatCard,
    useConfirm,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { cn } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

type ViewMode = "batches" | "all-items";

const GENDER_OPTIONS = [
    { id: 1, name: "ذكر" },
    { id: 2, name: "أنثى" },
];

export function MinistryReferralsPage() {
    const navigate = useNavigate();
    const canManage = useHasPermission("Ministry.Manage", "System.Admin");
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
    const [itemDialogOpen, setItemDialogOpen] = useState(false);
    const [editingItem, setEditingItem] =
        useState<MinistryApprovalItemDto | null>(null);
    const { confirm, ConfirmDialogElement } = useConfirm();

    // ── Additional Filters (Rank, Unit, Category, Gender, Governorate) ──
    const [categoryId, setCategoryId] = useState<string>("");
    const [rankId, setRankId] = useState<string>("");
    const [gender, setGender] = useState<string>("");
    const [governorate, setGovernorate] = useState<string>("");
    const [unitName, setUnitName] = useState<string>("");
    const [filtersOpen, setFiltersOpen] = useState<boolean>(false);

    const hasActiveFilters = !!(categoryId || rankId || gender || governorate || unitName);

    const clearFilters = () => {
        setCategoryId("");
        setRankId("");
        setGender("");
        setGovernorate("");
        setUnitName("");
        setPage(1);
    };

    // ── Lookup Queries ─────────────────────────────────
    const { data: categories } = useQuery({
        queryKey: ["lookups", "retiree-categories"],
        queryFn: () => lookupApi.getCategories(),
        staleTime: 5 * 60_000,
    });
    const { data: governorates } = useQuery({
        queryKey: ["lookups", "governorates"],
        queryFn: () => lookupApi.getGovernorates(),
        staleTime: 5 * 60_000,
    });
    const { data: ranks } = useQuery({
        queryKey: ["hr", "ranks"],
        queryFn: () => hrApi.getRanks(),
        staleTime: 5 * 60_000,
    });

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
            gender,
        ],
        queryFn: () =>
            ministryApi.getBatchItems(
                selectedBatch!.batchID,
                page,
                20,
                debouncedValue || undefined,
                gender ? Number(gender) : undefined
            ),
        enabled: !!selectedBatch,
    });

    const { data: allItemsData, isLoading: allItemsLoading } = useQuery({
        queryKey: [
            "ministry-all-items",
            page,
            debouncedValue,
            processedFilter,
            gender,
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
                        : undefined,
                gender ? Number(gender) : undefined
            ),
        enabled: viewMode === "all-items" && !selectedBatch,
    });

    const items = selectedBatch
        ? itemsData?.data?.items || []
        : allItemsData?.data?.items || [];
    const itemsTotal = selectedBatch
        ? itemsData?.data?.totalCount || 0
        : allItemsData?.data?.totalCount || 0;

    const canAddItems = canManage && !!selectedBatch;

    const availableUnits = useMemo(() => {
        const set = new Set<string>();
        items.forEach((item) => {
            if (item.unitName) set.add(item.unitName);
        });
        return Array.from(set);
    }, [items]);

    const filteredItems = useMemo(() => {
        let result = items;
        if (processedFilter === "processed") {
            result = result.filter((i) => i.isProcessed);
        } else if (processedFilter === "pending") {
            result = result.filter((i) => !i.isProcessed);
        }

        if (categoryId) {
            const cat = categories?.find((c) => c.id.toString() === categoryId);
            if (cat) {
                result = result.filter((i) => 
                    i.categoryName?.trim().toLowerCase() === cat.name?.trim().toLowerCase() ||
                    (i as any).categoryId?.toString() === categoryId
                );
            }
        }
        if (rankId) {
            const rk = ranks?.find((r) => r.rankID.toString() === rankId);
            if (rk) {
                result = result.filter((i) => 
                    i.rankName?.trim().toLowerCase() === rk.rankName?.trim().toLowerCase() ||
                    (i as any).rankId?.toString() === rankId
                );
            }
        }
        if (governorate) {
            result = result.filter((i) => 
                i.notes?.includes(governorate) || 
                (i as any).governorate?.toString() === governorate ||
                (i as any).governorateName?.toString() === governorate
            );
        }
        if (unitName) {
            result = result.filter((i) => 
                i.unitName?.trim().toLowerCase() === unitName.trim().toLowerCase()
            );
        }

        return result;
    }, [items, processedFilter, categoryId, rankId, governorate, unitName, categories, ranks]);

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

    const deleteItemMutation = useApiMutation(
        (itemId: number) => ministryApi.deleteManualItem(itemId),
        {
            successMessage: "تم حذف القيد بنجاح",
            invalidateKeys: [
                ["ministry-batch-items"],
                ["ministry-all-items"],
                ["ministry-stats"],
                ["ministry-batches"],
                ["force", "items"],
                ["force", "batch"],
                ["force", "analytics"],
                ["force-batches"],
            ],
        }
    );

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
            key: "gender",
            header: "الجنس",
            render: (row: MinistryApprovalItemDto) => {
                const isFemale = row.gender === 2;
                const isMale = row.gender === 1;
                return (
                    <span className="text-xs">
                        {isFemale ? "أنثى" : isMale ? "ذكر" : "—"}
                    </span>
                );
            },
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
            header: "تاريخ الالتحاق",
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
            header: "المعاش",
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
                    {canManage && !row.isProcessed && (
                        <Button
                            variant="ghost-action"
                            size="sm"
                            title="تعديل"
                            onClick={(e) => {
                                e.stopPropagation();
                                setEditingItem(row);
                            }}
                        >
                            <Pencil className="w-4 h-4" />
                        </Button>
                    )}
                    {canManage && !row.isProcessed && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 px-2 gap-1 text-xs text-destructive hover:text-destructive hover:bg-destructive/10 dark:hover:bg-destructive/20"
                            title="حذف"
                            onClick={(e) => {
                                e.stopPropagation();
                                confirm({
                                    title: "حذف القيد",
                                    description:
                                        "هل أنت متأكد من حذف هذا القيد من الكشف؟",
                                    variant: "danger",
                                }).then(
                                    (ok) =>
                                        ok &&
                                        deleteItemMutation.mutate(
                                            row.approvalItemID
                                        )
                                );
                            }}
                        >
                            <Trash2 className="w-3.5 h-3.5" />
                        </Button>
                    )}
                    {canManage && !row.isProcessed && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 px-2 gap-1 text-xs text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/50"
                            title="تعليم كمعالج"
                            onClick={(e) => {
                                e.stopPropagation();
                                processMutation.mutate(row.approvalItemID);
                            }}
                            loading={processMutation.isPending && processMutation.variables === row.approvalItemID}
                        >
                            <Play className="w-3.5 h-3.5 me-1" /> معالجة
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
                        <span>المعاش:</span>
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
                {canManage && !row.isProcessed && (
                    <Button 
                        variant="ghost" 
                        size="sm" 
                        className="flex-1 text-xs text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/50"
                        onClick={() => processMutation.mutate(row.approvalItemID)}
                        loading={processMutation.isPending && processMutation.variables === row.approvalItemID}
                    >
                        <Play className="w-3.5 h-3.5 me-1" /> معالجة
                    </Button>
                )}
            </div>
        </div>
    );

    const filtersElement = (
        <div className="rounded-2xl border border-border bg-card/50 p-3 sm:p-4 space-y-3 mb-4">
            <div className="flex items-center justify-between flex-wrap gap-2">
                <button
                    type="button"
                    onClick={() => setFiltersOpen((v) => !v)}
                    className="flex items-center gap-2 text-sm font-semibold text-foreground hover:text-foreground transition-colors"
                    aria-expanded={filtersOpen}
                    aria-controls="filters-grid"
                >
                    <Filter className="w-4 h-4" />
                    <span>الفلاتر</span>
                    {hasActiveFilters && (
                        <span className="inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 rounded-full bg-primary/15 text-primary text-xs font-bold">
                            {[categoryId, rankId, gender, governorate, unitName].filter(Boolean).length}
                        </span>
                    )}
                    <ChevronDown
                        className={cn(
                            "w-4 h-4 text-muted-foreground transition-transform duration-200",
                            filtersOpen && "rotate-180"
                        )}
                    />
                </button>
                {hasActiveFilters && (
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={clearFilters}
                        className="h-8 gap-1.5 text-destructive hover:text-destructive"
                    >
                        <X className="w-3.5 h-3.5" />
                        مسح الفلاتر
                    </Button>
                )}
            </div>

            <div
                id="filters-grid"
                className={cn(
                    "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3",
                    !filtersOpen && "hidden"
                )}
            >
                {/* الرتبة */}
                <div className="space-y-1.5">
                    <label className="label-filter">الرتبة</label>
                    <Select
                        value={rankId || "all"}
                        onValueChange={(v) => setRankId(v === "all" ? "" : v)}
                    >
                        <SelectTrigger>
                            <SelectValue placeholder="جميع الرتب" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">جميع الرتب</SelectItem>
                            {ranks?.map((r) => (
                                <SelectItem key={r.rankID} value={r.rankID.toString()}>
                                    {r.rankName}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                {/* الوحدة */}
                <div className="space-y-1.5">
                    <label className="label-filter">الوحدة</label>
                    <Select
                        value={unitName || "all"}
                        onValueChange={(v) => setUnitName(v === "all" ? "" : v)}
                    >
                        <SelectTrigger>
                            <SelectValue placeholder="جميع الوحدات" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">جميع الوحدات</SelectItem>
                            {availableUnits.map((u) => (
                                <SelectItem key={u} value={u}>
                                    {u}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                {/* الفئة */}
                <div className="space-y-1.5">
                    <label className="label-filter">الفئة</label>
                    <Select
                        value={categoryId || "all"}
                        onValueChange={(v) => setCategoryId(v === "all" ? "" : v)}
                    >
                        <SelectTrigger>
                            <SelectValue placeholder="جميع الفئات" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">جميع الفئات</SelectItem>
                            {categories?.map((c) => (
                                <SelectItem key={c.id} value={c.id.toString()}>
                                    {c.name}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                {/* الجنس */}
                <div className="space-y-1.5">
                    <label className="label-filter">الجنس</label>
                    <Select
                        value={gender || "all"}
                        onValueChange={(v) => {
                            setGender(v === "all" ? "" : v);
                            setPage(1);
                        }}
                    >
                        <SelectTrigger>
                            <SelectValue placeholder="الكل" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">الكل</SelectItem>
                            {GENDER_OPTIONS.map((g) => (
                                <SelectItem key={g.id} value={g.id.toString()}>
                                    {g.name}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                {/* المحافظة */}
                <div className="space-y-1.5">
                    <label className="label-filter">المحافظة</label>
                    <Select
                        value={governorate || "all"}
                        onValueChange={(v) => setGovernorate(v === "all" ? "" : v)}
                    >
                        <SelectTrigger>
                            <SelectValue placeholder="جميع المحافظات" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">جميع المحافظات</SelectItem>
                            {governorates?.map((g) => (
                                <SelectItem key={g.id} value={g.name}>
                                    {g.name}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>
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
                        {canAddItems && (
                            <Button
                                variant="primary"
                                size="sm"
                                onClick={() => setItemDialogOpen(true)}
                            >
                                <Plus className="w-4 h-4 mr-1" />
                                إضافة محال
                            </Button>
                        )}
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
            >
                {filtersElement}
                {ConfirmDialogElement}
                <AddReferralItemDialog
                    open={itemDialogOpen || !!editingItem}
                    onOpenChange={(open) => {
                        if (!open) {
                            setItemDialogOpen(false);
                            setEditingItem(null);
                        }
                    }}
                    batchId={selectedBatch.batchID}
                    item={editingItem}
                />
            </CrudPageLayout>
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
            >
                {filtersElement}
            </CrudPageLayout>
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
