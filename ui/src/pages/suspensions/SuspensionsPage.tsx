import { useQuery } from "@tanstack/react-query";
import { Ban, Calendar, CheckCircle2, FileText, Plus, Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { CreateSuspensionDialog } from "@/components/domain/suspensions/CreateSuspensionDialog";
import { Badge, Button, Card, type CustomColumnDef, DataTable, Heading, Input, Text } from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { SuspensionItem } from "@/types/retirement";

export function SuspensionsPage() {
    const navigate = useNavigate();
    const canCreate = useHasPermission("Suspension.Add", "System.Admin");

    const [page, setPage] = useState(1);
    const pageSize = 20;
    const [search, setSearch] = useState("");
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState<string>("");
    const [createOpen, setCreateOpen] = useState(false);

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(search);
            setPage(1);
        }, 500);
        return () => clearTimeout(timer);
    }, [search]);

    const { data, isLoading, error } = useQuery({
        queryKey: queryKeys.suspensions.list({ page, pageSize, search: debouncedSearch, status: statusFilter }),
        queryFn: () => retirementApi.getAllSuspensions({
            page,
            pageSize,
            search: debouncedSearch || undefined,
            status: statusFilter || undefined,
        }),
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل الإيقافات");
        }
    }, [error]);

    const suspensions = data?.items || [];
    const totalCount = data?.totalCount || 0;

    const columns: CustomColumnDef<SuspensionItem>[] = [
        {
            key: "suspensionID",
            header: "رقم",
            render: (row) => (
                <span className="font-mono font-bold text-primary">{row.suspensionID}</span>
            ),
        },
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row) => (
                <div className="flex items-center gap-1.5">
                    <FileText className="w-3.5 h-3.5 text-muted-foreground" />
                    <span className="font-mono font-bold">{row.fileNumber || "—"}</span>
                </div>
            ),
        },
        {
            key: "suspensionReason",
            header: "سبب الإيقاف",
            render: (row) => (
                <span className="text-sm">{row.suspensionReason}</span>
            ),
        },
        {
            key: "suspensionSource",
            header: "المصدر",
            render: (row) => (
                <span className="text-sm text-muted-foreground">{row.suspensionSource || "—"}</span>
            ),
        },
        {
            key: "suspensionDate",
            header: "تاريخ الإيقاف",
            render: (row) => (
                <div className="flex items-center gap-1.5 text-sm">
                    <Calendar className="w-3.5 h-3.5 text-muted-foreground" />
                    {formatDate(row.suspensionDate)}
                </div>
            ),
        },
        {
            key: "endDate",
            header: "تاريخ الرفع",
            render: (row) => (
                <div className="flex items-center gap-1.5 text-sm">
                    {row.endDate ? (
                        <>
                            <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500" />
                            {formatDate(row.endDate)}
                        </>
                    ) : (
                        <span className="text-muted-foreground">—" </span>
                    )}
                </div>
            ),
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge
                    variant={row.status === "ACTIVE" ? "destructive" : "success"}
                    className="text-xs"
                >
                    {row.status === "ACTIVE" ? (
                        <><Ban className="w-3 h-3 ml-1" /> نشط</>
                    ) : (
                        <><CheckCircle2 className="w-3 h-3 ml-1" /> منتهي</>
                    )}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "إجراء",
            render: (row) => (
                <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => navigate(`/files/${row.fileID}?tab=suspensions`)}
                    className="text-primary hover:text-primary/80"
                >
                    عرض الملف
                </Button>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <div className="bg-destructive/10 p-3 rounded-xl">
                        <Ban className="w-6 h-6 text-destructive" />
                    </div>
                    <div>
                        <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold tracking-tight">إيقاف المعاشات</Heading>
                        <Text variant="description" className="mt-1">قائمة شاملة بجميع إيقافات صرف المعاشات في النظام — {totalCount.toLocaleString("ar-YE")} سجل</Text>
                    </div>
                </div>
                {canCreate && (
                    <Button onClick={() => setCreateOpen(true)}>
                        <Plus className="w-4 h-4 ml-2" /> إيقاف جديد
                    </Button>
                )}
            </div>

            {/* ─── Filters ─── */}
            <Card className="p-4">
                <div className="flex flex-col md:flex-row gap-4">
                    <div className="relative flex-1">
                        <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                        <Input
                            placeholder="بحث برقم الملف أو اسم المتقاعد أو السبب..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            className="pr-10"
                        />
                    </div>
                    <div className="flex gap-2">
                        <Button
                            variant={statusFilter === "" ? "primary" : "outline"}
                            size="sm"
                            onClick={() => { setStatusFilter(""); setPage(1); }}
                        >
                            الكل
                        </Button>
                        <Button
                            variant={statusFilter === "ACTIVE" ? "destructive" : "outline"}
                            size="sm"
                            onClick={() => { setStatusFilter("ACTIVE"); setPage(1); }}
                        >
                            <Ban className="w-3.5 h-3.5 ml-1" /> نشط
                        </Button>
                        <Button
                            variant={statusFilter === "EXPIRED" ? "success" : "outline"}
                            size="sm"
                            onClick={() => { setStatusFilter("EXPIRED"); setPage(1); }}
                        >
                            <CheckCircle2 className="w-3.5 h-3.5 ml-1" /> منتهي
                        </Button>
                    </div>
                </div>
            </Card>

            {/* ─── Data Table ─── */}
            <Card>
                <DataTable
                    columns={columns}
                    data={suspensions}
                    loading={isLoading}
                    emptyMessage="لا توجد إيقافات مسجلة"
                    pagination={{
                        page,
                        pageSize,
                        total: totalCount,
                        onPageChange: setPage,
                    }}
                />
            </Card>

            {/* ─── Create Suspension Dialog ─── */}
            <CreateSuspensionDialog
                open={createOpen}
                onOpenChange={setCreateOpen}
                onCreated={() => { setPage(1); }}
            />
        </div>
    );
}
