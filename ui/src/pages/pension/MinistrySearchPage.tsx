import { useQuery } from "@tanstack/react-query";
import { CreditCard, Layers, Play, Printer,Search, User, Users } from "lucide-react";
import { useEffect } from "react";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { toast } from "sonner";
import { useNavigate } from "react-router-dom";

import { type ForceGlobalSearchDto,ministryApi } from "@/api/ministry";
import { useHasPermission } from "@/stores/authStore";
import { SearchDialog } from "@/components/common/SearchDialog";
import { 
    Badge, 
    Button,
    Card,
    Heading,
    type CustomColumnDef, 
    DataTable, 
    Input,
    PageLayout,
    Text} from "@/components/ui";
import { useApiMutation } from "@/hooks";

export function MinistrySearchPage() {
    const navigate = useNavigate();
    const canManage = useHasPermission("Ministry.Manage", "System.Admin");
    const { searchTerm, setSearchTerm, debouncedValue: debouncedQuery } = useDebouncedSearch();

    // ── Data Fetching ──────────────────────────────────
    const { data: resultsRes, isLoading, error, refetch } = useQuery({
        queryKey: ["force", "global-search", debouncedQuery],
        queryFn: () => ministryApi.searchGlobalForce(debouncedQuery),
        enabled: debouncedQuery.length >= 3
    });

    const results = resultsRes?.data || [];

    useEffect(() => {
        if (error) toast.error("تعذر تحميل نتائج البحث", { id: "global-search" });
    }, [error]);

    // ── Mutations ─────────────────────────────────────
    const processItemMutation = useApiMutation(
        (itemId: number) => ministryApi.processItem(itemId),
        {
            successMessage: "تمت معالجة القيد بنجاح",
            onSuccess: () => refetch()
        }
    );

    // ── Table Columns ──────────────────────────────────
    const columns: CustomColumnDef<ForceGlobalSearchDto>[] = [
        {
            key: "fullName",
            header: "الاسم الكامل",
            render: (row: ForceGlobalSearchDto) => (
                <div className="flex flex-col">
                    <div className="flex items-center gap-2">
                        <User className="w-3 h-3 text-primary" />
                        <span className="font-semibold text-sm">{row.fullName}</span>
                    </div>
                    <div className="flex items-center gap-2 font-mono text-[10px] sm:text-xs text-muted-foreground mt-0.5">
                        <CreditCard className="w-2.5 h-2.5" />
                        {row.militaryId}
                    </div>
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
        {
            key: "batchName",
            header: "الكشف",
            render: (row: ForceGlobalSearchDto) => (
                <div className="flex items-center gap-2">
                    <Layers className="w-3 h-3 text-muted-foreground" />
                    <span className="text-xs text-muted-foreground">{row.batchName || "---"}</span>
                </div>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: ForceGlobalSearchDto) => (
                <Badge variant={row.isProcessed ? "success" : "warning"} className="h-5 text-[10px] sm:text-xs px-2">
                    {row.status}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: ForceGlobalSearchDto) => (
                <div className="flex items-center gap-1">
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
                    <a 
                        href={`/force/items/${row.approvalItemID}/print`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center justify-center h-7 w-7 text-blue-600 hover:bg-blue-50 rounded"
                    >
                        <Printer className="w-3.5 h-3.5" />
                    </a>
                </div>
            )
        }
    ];

    return (
        <PageLayout>
            <div className="flex flex-col gap-1">
                <div className="flex items-center justify-between gap-2">
                    <Heading level="h1">البحث عن محال</Heading>
                    <SearchDialog
                        value={searchTerm}
                        onSelect={(name) => setSearchTerm(name)}
                        queryKey={["force-global-search"]}
                        fetchFn={(term) =>
                            ministryApi
                                .searchGlobalForce(term)
                                .then((r) => ({ items: r.data || [] }))
                        }
                        getName={(item) => item.fullName}
                        getSubtitle={(item) => item.militaryId}
                        title="بحث عن محال"
                        placeholder="بحث بالاسم أو الرقم العسكري..."
                        minLength={3}
                        hideTrigger
                    />
                </div>
                <Text variant="description">
                    البحث الشامل في كافة كشوفات الإحالة إلى التقاعد
                </Text>
            </div>

            <div className="space-y-6 mt-6">
                {/* ── Search Bar ─────────────────────────────── */}
                <div className="relative max-w-2xl mx-auto">
                    <Search className="absolute end-4 top-1/2 -translate-y-1/2 text-muted-foreground h-5 w-5" />
                    <Input
                        className="pe-12 h-14 text-lg  border-primary/20 focus:border-primary transition-all rounded-2xl"
                        placeholder="ابحث بالاسم أو الرقم العسكري (3 أحرف على الأقل)..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                    />
                    {isLoading && (
                        <div className="absolute start-4 top-1/2 -translate-y-1/2">
                            <div className="w-5 h-5 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
                        </div>
                    )}
                </div>

                {/* ── Results Table ──────────────────────────── */}
                <Card className="">
                    <DataTable
                        columns={columns}
                        data={results}
                        loading={isLoading}
                        onRowClick={(row) => {
                            if (row.retireeID) navigate(`/retirees/${row.retireeID}`);
                        }}
                        emptyMessage={
                            debouncedQuery.length < 3 
                                ? "ابدأ الكتابة للبحث الشامل في كافة الكشوفات..." 
                                : "لا توجد نتائج مطابقة لبحثك."
                        }
                    />
                </Card>
            </div>
        </PageLayout>
    );
}
