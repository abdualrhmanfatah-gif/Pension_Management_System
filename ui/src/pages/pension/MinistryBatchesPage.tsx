import { useQuery } from "@tanstack/react-query";
import { ChevronLeft, FileText, Plus,Trash2 } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import api from "@/api/axios";
import { ministryApi } from "@/api/ministry";
import { CreateForceBatchDialog } from "@/components/domain/force/CreateForceBatchDialog";
import { Badge, Button, CrudPageLayout, Heading, PageLayout, Text, useConfirm } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { formatDate } from "@/lib/utils";
import { ForceStatsSection } from "@/pages/ministry-approvals/ForceAnalyticsDashboard";

// ─── Types ─────────────────────────────────────────────

interface BatchDto {
    batchID: number;
    batchNumber: string;
    batchDate: string;
    totalCount: number;
    processedCount: number;
    status: string;
    importedByUser: string;
    createdAt: string;
}

// ─── Page ──────────────────────────────────────────────

export function MinistryBatchesPage() {
    const navigate = useNavigate();
    const { confirm, ConfirmDialogElement } = useConfirm();
    const [createOpen, setCreateOpen] = useState(false);

    const { data: batches = [], isLoading: loading, refetch, error } = useQuery({
        queryKey: ["force-batches"],
        queryFn: async () => {
            const res = await api.get<{ data: BatchDto[] }>("/ministry-approvals/batches");
            return res.data.data || [];
        }
    });

    const lastErrorRef = useRef<string | null>(null);

    useEffect(() => {
        if (error && lastErrorRef.current !== error.message) {
            lastErrorRef.current = error.message;
            toast.error("فشل في تحميل كشوفات القوة", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const deleteMutation = useApiMutation(
        (id: number) => ministryApi.deleteBatch(id),
        {
            successMessage: "تم حذف الكشف بنجاح",
            invalidateKeys: [["force-batches"]],
        }
    );

    const handleDelete = async (batchId: number) => {
        const confirmed = await confirm({
            title: "تأكيد الإجراء",
            description: "هل أنت متأكد من حذف هذا الكشف؟",
            variant: "danger",
        });
        if (!confirmed) return;

        deleteMutation.mutate(batchId);
    };

    const columns = [
        {
            key: "batchNumber",
            header: "رقم الكشف",
            render: (row: BatchDto) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                        <FileText className="w-4 h-4" />
                    </div>
                    <div>
                        <p className="font-medium text-foreground">{row.batchNumber}</p>
                        <p className="text-xs text-muted-foreground">{formatDate(row.batchDate)}</p>
                    </div>
                </div>
            )
        },
        {
            key: "counts",
            header: "الإحالات",
            render: (row: BatchDto) => (
                <div className="flex flex-col text-sm">
                    <span className="font-medium">{row.totalCount} إحالة</span>
                    <span className="text-xs text-muted-foreground">تمت معالجة {row.processedCount}</span>
                </div>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: BatchDto) => (
                <Badge variant={row.status === "جديد" ? "outline" : row.status === "مكتمل" ? "success" : "warning"}>
                    {row.status}
                </Badge>
            )
        },
        {
            key: "user",
            header: "بواسطة",
            render: (row: BatchDto) => <span className="text-sm text-muted-foreground">{row.importedByUser}</span>
        },
        {
            key: "actions",
            header: "",
            render: (row: BatchDto) => (
                <div className="flex items-center gap-1">
                    <Button variant="ghost" size="sm" onClick={() => navigate(`/force/batches/${row.batchID}`)}>
                        التفاصيل <ChevronLeft className="w-4 h-4 me-1 rotate-180" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="text-destructive hover:text-destructive hover:bg-destructive/10 min-h-[44px] min-w-[44px]"
                        onClick={(e) => { e.stopPropagation(); handleDelete(row.batchID); }}
                    >
                        <Trash2 className="w-4 h-4" />
                    </Button>
                </div>
            )
        }
    ];

    return (
        <PageLayout>
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div className="flex flex-col gap-1">
                    <Heading level="h1">إدارة كشوفات الإحاله</Heading>
                    <Text variant="description">
                        كشوفات الإحالة إلى التقاعد الواردة من الوزارة والمصادقة عليها
                    </Text>
                </div>
                <Button onClick={() => setCreateOpen(true)}
                    className="rounded-md px-4 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 shrink-0"
                    variant="default">
                    <Plus className="w-4 h-4 ml-2" />
                    إحالة جديدة
                </Button>
            </div>

            {/* ─── الإحصائيات (في الأعلى) ─── */}
            <div className="mt-6">
                <ForceStatsSection />
            </div>

            {/* ─── الجدول ─── */}
            <div className="mt-6">
                <CrudPageLayout
                    title=""
                    description=""
                    columns={columns}
                    data={batches}
                    loading={loading}
                    emptyMessage="لا يوجد إحالات مسجلة"
                    onRowClick={(row) => navigate(`/force/batches/${row.batchID}`)}
                >
                    <CreateForceBatchDialog
                        open={createOpen}
                        onOpenChange={setCreateOpen}
                        onSuccess={refetch}
                    />
                    {ConfirmDialogElement}
                </CrudPageLayout>
            </div>

            {/* ─── توزيع العمل (أسفل الجدول) ─── */}
            <div className="mt-6">
                {/* تابع - قيد التطوير */}
            </div>
        </PageLayout>
    );
}
