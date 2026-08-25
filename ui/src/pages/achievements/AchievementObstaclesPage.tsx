import { useQuery } from "@tanstack/react-query";
import {
    AlertTriangle,
    CheckCircle,
    Clock,
    Eye,
    Loader2,
    Pencil,
    Plus,
    Target,
    Trash2,
    XCircle,
} from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { achievementObstacleApi } from "@/api/achievementObstacles";
import { Badge, Button, Card, DataTable, type CustomColumnDef, ErrorState, Heading, Loader, PageLayout, StatCard, Text } from "@/components/ui";
import { useApiMutation, useConfirm } from "@/hooks";
import { useAuthStore, useHasPermission } from "@/stores/authStore";
import type { AchievementObstacleDto, AchievementObstacleFilter, AchievementObstacleType, TrackingStatus } from "@/types/achievementObstacle";

import { AchievementObstacleDialog, type AchievementObstacleFormValues } from "./AchievementObstacleDialog";
import { AchievementObstacleDetailDialog } from "./AchievementObstacleDetailDialog";

// ─── Constants ─────────────────────────────────────────

const statusLabels: Record<TrackingStatus, string> = {
    New: "جديد",
    InProgress: "قيد التنفيذ",
    Completed: "مكتمل",
    Closed: "مغلق",
};

const typeLabels: Record<AchievementObstacleType, string> = {
    Achievement: "إنجاز",
    Obstacle: "معوق",
};

// ─── Component ─────────────────────────────────────────

export function AchievementObstaclesPage() {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canAdd = useHasPermission("AchievementObstacle.Add", "System.Admin");
    const canEdit = useHasPermission("AchievementObstacle.Edit", "System.Admin");
    const canDelete = useHasPermission("AchievementObstacle.Delete", "System.Admin");
    const currentUserID = useAuthStore(s => s.user?.id);

    const [filter, setFilter] = useState<AchievementObstacleFilter>({ page: 1, pageSize: 20 });
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingRecord, setEditingRecord] = useState<AchievementObstacleDto | undefined>();
    const [detailOpen, setDetailOpen] = useState(false);
    const [detailId, setDetailId] = useState<number | null>(null);

    // ─── Queries ────────────────────────────────────────

    const { data: pagedData, isLoading, error } = useQuery({
        queryKey: ["achievement-obstacles", filter],
        queryFn: () => achievementObstacleApi.list(filter),
    });

const { data: stats } = useQuery({
        queryKey: ["achievement-obstacles-stats", filter.departmentID, filter.employeeID],
        queryFn: () => achievementObstacleApi.stats(filter),
    });

    // ─── Mutations ──────────────────────────────────────

    const createMutation = useApiMutation(
        (values: Record<string, unknown>) => achievementObstacleApi.create(values as Parameters<typeof achievementObstacleApi.create>[0]),
        {
            successMessage: "تم إنشاء السجل بنجاح",
            invalidateKeys: [["achievement-obstacles"], ["achievement-obstacles-stats"]],
            onSuccessCallback: () => setDialogOpen(false),
        }
    );

    const updateMutation = useApiMutation(
        ({ id, ...values }: { id: number } & Record<string, unknown>) =>
            achievementObstacleApi.update(id, values as Parameters<typeof achievementObstacleApi.update>[1]),
        {
            successMessage: "تم تحديث السجل بنجاح",
            invalidateKeys: [["achievement-obstacles"], ["achievement-obstacles-stats"]],
            onSuccessCallback: () => { setDialogOpen(false); setEditingRecord(undefined); },
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => achievementObstacleApi.remove(id),
        {
            successMessage: "تم حذف السجل بنجاح",
            invalidateKeys: [["achievement-obstacles"], ["achievement-obstacles-stats"]],
        }
    );

    // ─── Handlers ───────────────────────────────────────

    const handleDelete = async (record: AchievementObstacleDto) => {
        const ok = await confirm({
            title: "حذف السجل",
            description: `هل أنت متأكد من حذف السجل ${record.number}؟`,
            variant: "danger",
        });
        if (ok) deleteMutation.mutate(record.id);
    };

    const handleEdit = (record: AchievementObstacleDto) => {
        setEditingRecord(undefined);
        setDialogOpen(false);
        setTimeout(() => {
            setEditingRecord(record);
            setDialogOpen(true);
        }, 0);
    };

    const handleSubmit = (values: AchievementObstacleFormValues) => {
        const data = { ...values, ownerUserID: currentUserID ?? 0 };
        if (editingRecord) {
            updateMutation.mutate({ id: editingRecord.id, ...data });
        } else {
            createMutation.mutate(data);
        }
    };

    // ─── Effects ────────────────────────────────────────

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل البيانات");
    }, [error]);

    // ─── Loading / Error ────────────────────────────────

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-[60dvh]">
                <Loader variant="page" />
            </div>
        );
    }

    if (error) {
        return (
            <PageLayout>
                <ErrorState message="تعذر تحميل البيانات" />
            </PageLayout>
        );
    }

    const items = pagedData?.items || [];

    return (
        <PageLayout>
            {/* ── Header ──────────────────────────── */}
            <div className="flex items-center justify-between flex-wrap gap-3">
                <div>
                    <Heading level="h1">الإنجازات والمعوقات</Heading>
                    <Text variant="description" className="mt-1">تسجيل وتتبع الإنجازات والمعوقات</Text>
                </div>
                {canAdd && (
                    <Button onClick={() => { setEditingRecord(undefined); setDialogOpen(true); }} className="gap-1.5">
                        <Plus className="w-4 h-4" /> سجل جديد
                    </Button>
                )}
            </div>

            {/* ── Stats ───────────────────────────── */}
            {stats && (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                    <StatCard title="الإنجازات" value={stats.totalAchievements} icon={<Target />} variant="emerald" />
                    <StatCard title="المعوقات" value={stats.totalObstacles} icon={<AlertTriangle />} variant="orange" />
                    <StatCard title="مفتوحة" value={stats.totalOpen} icon={<Clock />} variant="blue" />
                    <StatCard title="قيد التنفيذ" value={stats.totalInProgress} icon={<Loader2 />} variant="amber" />
                    <StatCard title="مكتملة" value={stats.totalCompleted} icon={<CheckCircle />} variant="emerald" />
                    <StatCard title="مغلقة" value={stats.totalClosed} icon={<XCircle />} variant="muted" />
                </div>
            )}

            {/* ── Table ───────────────────────────── */}
            <Card className="p-5">
                {items.length === 0 ? (
                    <div className="border-2 border-dashed rounded-xl py-16 text-center text-muted-foreground bg-muted/10">
                        <Target className="w-12 h-12 mx-auto mb-3 opacity-20" />
                        لا توجد سجلات إنجازات أو معوقات
                    </div>
                ) : (
                    <DataTable
                        columns={[
                            { key: "number", header: "الرقم", render: (row) => <span className="font-mono text-xs">{row.number}</span> },
                            { key: "type", header: "النوع", render: (row) => <Badge variant={row.type === "Achievement" ? "success" : "destructive"}>{typeLabels[row.type]}</Badge> },
                            { key: "title", header: "العنوان", render: (row) => <span className="font-medium max-w-[200px] truncate block">{row.title}</span> },
                            { key: "status", header: "الحالة", render: (row) => <Badge variant={row.status === "Completed" ? "success" : row.status === "Closed" ? "ghost" : row.status === "InProgress" ? "warning" : "info"}>{statusLabels[row.status]}</Badge> },
                            { key: "departmentName", header: "القسم", render: (row) => <span className="text-xs text-muted-foreground">{row.departmentName || "-"}</span> },
                            { key: "createdAt", header: "التاريخ", render: (row) => <span className="text-xs text-muted-foreground">{new Date(row.createdAt).toLocaleDateString("ar-YE")}</span> },
                            ...((canEdit || canDelete) ? [{ key: "actions", header: "إجراءات", render: (row: AchievementObstacleDto) => <div className="flex items-center gap-1"><Button size="icon" variant="ghost" onClick={() => { setDetailId(row.id); setDetailOpen(true); }}><Eye className="w-4 h-4" /></Button>{canEdit && <Button size="icon" variant="ghost" onClick={() => handleEdit(row)}><Pencil className="w-4 h-4" /></Button>}{canDelete && <Button size="icon" variant="ghost" className="text-destructive" onClick={() => handleDelete(row)}><Trash2 className="w-4 h-4" /></Button>}</div> }] : []),
                        ] as CustomColumnDef<AchievementObstacleDto>[]}
                        data={items}
                        rowKey={(row) => row.id}
                    />
                )}

                {/* ── Pagination ────────────────────── */}
                {pagedData && pagedData.totalCount > pagedData.pageSize && (
                    <div className="flex items-center justify-between mt-4 pt-4 border-t border-border">
                        <Text variant="small" className="text-muted-foreground">
                            عرض {(pagedData.page - 1) * pagedData.pageSize + 1} - {Math.min(pagedData.page * pagedData.pageSize, pagedData.totalCount)} من {pagedData.totalCount}
                        </Text>
                        <div className="flex gap-2">
                            <Button
                                size="sm"
                                variant="outline"
                                disabled={pagedData.page <= 1}
                                onClick={() => setFilter(f => ({ ...f, page: (f.page || 1) - 1 }))}
                            >
                                السابق
                            </Button>
                            <Button
                                size="sm"
                                variant="outline"
                                disabled={pagedData.page >= (pagedData.totalPages ?? 0)}
                                onClick={() => setFilter(f => ({ ...f, page: (f.page || 1) + 1 }))}
                            >
                                التالي
                            </Button>
                        </div>
                    </div>
                )}
            </Card>

            {/* ── Dialog ──────────────────────────── */}
            <AchievementObstacleDialog
                key={editingRecord?.id ?? "new"}
                isOpen={dialogOpen}
                onOpenChange={(open) => {
                    setDialogOpen(open);
                    if (!open) setEditingRecord(undefined);
                }}
                onSubmit={handleSubmit}
                isLoading={createMutation.isPending || updateMutation.isPending}
                initialData={editingRecord}
            />

            {ConfirmDialogElement}

            {/* -- Detail Dialog -- */}
            <AchievementObstacleDetailDialog
                open={detailOpen}
                onOpenChange={(open) => { setDetailOpen(open); if (!open) setDetailId(null); }}
                recordId={detailId}
            />
        </PageLayout>
    );
}
