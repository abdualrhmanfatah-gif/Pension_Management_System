/**
 * DataEntryView - واجهة مخصصة لمدخل البيانات
 * تركز على إنشاء الملفات وإدخال/مراجعة البيانات الأساسية فقط
 */

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2,Clock, FileText, Plus, Search } from "lucide-react";
import { useEffect,useState } from "react";
import { useNavigate } from "react-router-dom";

import api from "@/api/axios";
import { retirementApi } from "@/api/retirement";
import { CreateFileWizard } from "@/components/domain/retirement/CreateFileWizard";
import { FileStatusBadge } from "@/components/domain/retirement/FileStatusBadge";
import { RetirementFileCard } from "@/components/domain/retirement/RetirementFileCard";
import { Input, Text } from "@/components/ui";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { DataTable } from "@/components/ui/DataTable";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/Dialog";
import { useCreateFileIntent } from "@/constants/shortcuts";
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

    const { data: statuses } = useQuery({
        queryKey: ["retirement-files", "statuses"],
        queryFn: () => retirementApi.getStatuses(),
        staleTime: 5 * 60_000,
    });
    // The data-entry endpoint omits statusID; resolve against the status lookup.
    const resolveStatusId = (file: RetirementFileListItem) => file.statusID
        ?? statuses?.find((status) => status.statusName === file.fileStatusName)?.statusID;

    // Create dialog
    const [createOpen, setCreateOpen] = useState(false);

    // Alt+N (global): this view owns the create dialog for data-entry role.
    const { pending: createIntentPending, consume: consumeCreateIntent } = useCreateFileIntent();
    useEffect(() => {
        if (!createIntentPending) return;
        consumeCreateIntent();
        if (canCreate) setCreateOpen(true);
    }, [createIntentPending, consumeCreateIntent, canCreate]);

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
                    <FileStatusBadge statusId={resolveStatusId(row)} statusName={row.fileStatusName} />
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
        <div className="min-w-0 space-y-1.5 md:space-y-3" dir="rtl">
            {/* Header */}
            <div className="flex flex-wrap items-center justify-between gap-2">
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
            <div className="min-w-0 max-w-xl">
                <Input
                    type="search"
                    label="البحث عن ملف"
                    icon={Search}
                    placeholder="الاسم أو الرقم الوطني أو رقم الملف"
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
                <div>
                    <DataTable
                        columns={columns}
                        data={files}
                        loading={loading}
                        rowKey={(row) => row.fileID}
                        mobileScrollMode="page"
                        onRowClick={(row) => navigate(`/files/${row.fileID}`)}
                        renderMobileCard={(row) => (
                            <RetirementFileCard file={{ ...row, statusID: resolveStatusId(row) }}>
                                <Button variant="outline" size="sm" className="w-full" onClick={() => navigate(`/files/${row.fileID}`)}>
                                    <CheckCircle2 className="h-4 w-4" aria-hidden="true" />
                                    استكمال البيانات
                                </Button>
                            </RetirementFileCard>
                        )}
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
