/**
 * DataEntryView - واجهة مخصصة لمدخل البيانات
 * تركز على إنشاء الملفات وإدخال/مراجعة البيانات الأساسية فقط
 */

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2,Clock, FileText, Plus, Search } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

import api from "@/api/axios";
import { CreateFileWizard } from "@/components/domain/retirement/CreateFileWizard";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { DataTable } from "@/components/ui/DataTable";
import { Text } from "@/components/ui";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/Dialog";
import { cn, getStatusColorClass } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

interface RetirementFileListItem {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    nationalID: string | null;
    fileStatusName: string | null;
    fileOpenDate: string | null;
    needsDataCompletion?: boolean;
    statusID?: number;
}

interface PagedResult {
    items: RetirementFileListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
}

export function DataEntryView() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const canCreate = useHasPermission("File.Create");

    const [search, setSearch] = useState("");
    const [page, setPage] = useState(1);
    const pageSize = 20;

    // Create dialog
    const [createOpen, setCreateOpen] = useState(false);

    // ─── Fetch Files (React Query) ───────────────────────

    const { data: filesData, isLoading: loading } = useQuery({
        queryKey: ["retirement-files", "data-entry", page, search],
        queryFn: async () => {
            try {
                const res = await api.get<{ data: PagedResult }>("/retirement-files/for-data-entry", {
                    params: { page, pageSize, search: search || undefined },
                });
                return res.data.data;
            } catch {
                const res = await api.get<{ data: PagedResult }>("/retirement-files", {
                    params: { page, pageSize, search: search || undefined, statusId: 1 },
                });
                return res.data.data;
            }
        },
    });

    const files = filesData?.items ?? [];
    const totalCount = filesData?.totalCount ?? 0;

    // ─── Status Badge Color ────────────────────────────────

    const getStatusVariant = (statusName: string | null): "outline" | "success" | "warning" | "danger" => {
        if (!statusName) return "outline";
        const lower = statusName.toLowerCase();
        if (lower.includes("جديد") || lower.includes("مفتوح")) return "outline";
        if (lower.includes("مكتمل") || lower.includes("نشط")) return "success";
        if (lower.includes("معلق") || lower.includes("ناقص")) return "warning";
        if (lower.includes("مغلق") || lower.includes("موقوف")) return "danger";
        return "outline";
    };

    // ─── Columns ───────────────────────────────────────────

    const totalPages = Math.ceil(totalCount / pageSize);

    const columns = [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: RetirementFileListItem) => (
                <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                        <FileText className="w-4 h-4" />
                    </div>
                    <span className="font-medium text-foreground font-mono">{row.fileNumber}</span>
                </div>
            ),
        },
        {
            key: "retireeFullName",
            header: "المتقاعد",
            render: (row: RetirementFileListItem) => (
                <div>
                    <Text variant="label" className={cn(
                        getStatusColorClass(row.statusID)
                    )}>
                        {row.retireeFullName}
                    </Text>
                    {row.nationalID && (
                        <Text variant="caption" className="font-mono">{row.nationalID}</Text>
                    )}
                </div>
            ),
        },
        {
            key: "fileStatusName",
            header: "الحالة",
            render: (row: RetirementFileListItem) => (
                <div className="flex items-center gap-2">
                    <Badge variant={getStatusVariant(row.fileStatusName)}>
                        {row.fileStatusName || "غير محدد"}
                    </Badge>
                    {row.needsDataCompletion && (
                        <Badge variant="warning" className="text-xs">
                            <Clock className="w-3 h-3 ms-1" />
                            يحتاج استكمال
                        </Badge>
                    )}
                </div>
            ),
        },
        {
            key: "fileOpenDate",
            header: "تاريخ الفتح",
            render: (row: RetirementFileListItem) => (
                <span className="text-sm text-muted-foreground">
                    {row.fileOpenDate
                        ? new Date(row.fileOpenDate).toLocaleDateString("ar-SA")
                        : "—"}
                </span>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: RetirementFileListItem) => (
                <Button variant="ghost" size="md" onClick={() => navigate(`/files/${row.fileID}`)}>
                    <CheckCircle2 className="w-4 h-4 ms-1" />
                    استكمال البيانات
                </Button>
            ),
        },
    ];

    // ─── Render ────────────────────────────────────────────

    return (
        <div className="space-y-6 animate-fade-in" dir="rtl">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-lg sm:text-2xl font-bold text-foreground">إدخال البيانات</h1>
                    <Text variant="description" className="mt-1">
                        إنشاء ملفات تقاعد جديدة واستكمال البيانات الأساسية
                    </Text>
                </div>
                {canCreate && (
                    <Button onClick={() => setCreateOpen(true)}>
                        <Plus className="w-4 h-4" />
                        فتح ملف جديد
                    </Button>
                )}
            </div>

            {/* Search */}
            <div className="relative flex-1 min-w-0 sm:min-w-[280px] max-w-md">
                <Search className="absolute end-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                    type="text"
                    className="w-full rounded-lg border px-10 py-2.5 bg-background focus:ring-2 focus:ring-primary/20 outline-none"
                    placeholder="بحث بالاسم، الرقم الوطني، أو رقم الملف..."
                    value={search}
                    onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                    aria-label="البحث في ملفات التقاعد"
                />
            </div>

            {/* Table or Empty State */}
            {!loading && files.length === 0 && !search ? (
                <div className="flex flex-col items-center justify-center p-6 sm:p-12 text-center border rounded-xl bg-card border-dashed animate-in fade-in duration-500">
                    <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center mb-4">
                        <CheckCircle2 className="w-8 h-8 text-primary" />
                    </div>
                    <h3 className="text-xl font-bold text-foreground mb-2">عمل رائع! لا توجد ملفات معلقة</h3>
                    <Text variant="body" className="max-w-sm mb-6">لقد قمت بإنجاز جميع مهام البيانات الأساسية للملفات المكلفة. يمكنك إنشاء ملف جديد متى شئت.</Text>
                    {canCreate && (
                        <Button onClick={() => setCreateOpen(true)}>
                            <Plus className="w-4 h-4 ms-2" />
                            فتح ملف تقاعد جديد
                        </Button>
                    )}
                </div>
            ) : (
                <div className="rounded-xl border bg-card overflow-hidden min-h-[250px] sm:min-h-[400px]">
                    <DataTable
                        columns={columns}
                        data={files}
                        loading={loading}
                        rowKey={(row) => row.fileID}
                        emptyMessage="لا توجد ملفات تطابق البحث"
                    />
                </div>
            )}

            {/* Pagination */}
            {totalPages > 1 && (
                <div className="flex items-center justify-center gap-2 flex-wrap">
                    <Button variant="outline" size="md" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>
                        السابق
                    </Button>
                    <span className="text-sm text-muted-foreground">
                        صفحة {page} من {totalPages} ({totalCount} ملف)
                    </span>
                    <Button variant="outline" size="md" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>
                        التالي
                    </Button>
                </div>
            )}

            {/* ─── Create Dialog (Wizard) ───────────────── */}
            <Dialog open={createOpen} onOpenChange={setCreateOpen}>
                <DialogContent size="full">
                    <DialogHeader>
                        <DialogTitle>فتح ملف تقاعد جديد</DialogTitle>
                        <DialogDescription>
                            أدخل بيانات ملف التقاعد الجديد لاستكمال عملية الإنشاء.
                        </DialogDescription>
                    </DialogHeader>
                    <CreateFileWizard
                        onSuccess={() => {
                            setCreateOpen(false);
                            queryClient.invalidateQueries({ queryKey: ["retirement-files"] });
                        }}
                        onCancel={() => setCreateOpen(false)}
                    />
                </DialogContent>
            </Dialog>
        </div>
    );
}
