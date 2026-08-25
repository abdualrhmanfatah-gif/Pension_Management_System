import { useQuery } from "@tanstack/react-query";
import {
ArrowRight, CheckCircle, CheckCircle2,
DollarSign, Download,     Play, Trash2, Upload, User, Users} from "lucide-react";
import { useMemo, useState } from "react";
import { Link,useNavigate, useParams } from "react-router-dom";

import { salariesApi } from "@/api/salaries";
import {
    AlertBanner,
    Badge,
    Breadcrumb,
    Button,
    type CustomColumnDef,
    DataTable,
    Heading,
    Loader,
    StatCard,
    Text,
    useConfirm} from "@/components/ui";
import { BATCH_STATUS } from "@/constants";
import { useApiMutation } from "@/hooks";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { SalaryBatchItem } from "@/types/salaries";

interface GenerateResult {
    generatedCount: number;
    skippedCount: number;
    totalAmount: number;
    skippedItems: { fileNumber: string; retireeFullName: string; reason: string }[];
}

interface ImportResult {
    updatedCount: number;
    skippedCount: number;
    errors: string[];
}

/**
 * صفحة تفاصيل كشف صرف المعاشات
 */
export function SalaryBatchDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const batchId = Number(id);
    const navigate = useNavigate();
    const { confirm, ConfirmDialogElement } = useConfirm();

    const canApprove = useHasPermission("SalaryBatch.Approve");
    const canDelete = useHasPermission("SalaryBatch.Manage");
    const canExport = useHasPermission("SalaryBatch.Export");
    const canManage = useHasPermission("SalaryBatch.Manage");

    const [generateResult, setGenerateResult] = useState<GenerateResult | null>(null);
    const [importResult, setImportResult] = useState<ImportResult | null>(null);
    const [page, setPage] = useState(1);
    const pageSize = 10;

    const { data: batch, isLoading: isBatchLoading, isError: batchError } = useQuery({
        queryKey: ["salary-batch", batchId],
        queryFn: () => salariesApi.getById(batchId),
        enabled: !!batchId,
    });

    const { data: itemsData, isLoading: isItemsLoading } = useQuery({
        queryKey: ["salary-batch-items", batchId, page],
        queryFn: () => salariesApi.getItems(batchId, { page, pageSize }),
        enabled: !!batchId,
    });

    const generateMutation = useApiMutation(
        () => salariesApi.generate(batchId),
        {
            invalidateKeys: [["salary-batch", batchId], ["salary-batch-items", batchId]],
            successMessage: (res) => `تم توليد ${res.data.generatedCount} عنصر صرف`,
            onSuccessCallback: (res) => {
                setPage(1);
                setGenerateResult(res.data as GenerateResult);
            }
        }
    );

    const approveMutation = useApiMutation(
        () => salariesApi.approve(batchId),
        {
            invalidateKeys: [["salary-batch", batchId]],
            successMessage: "تم اعتماد كشف الصرف",
        }
    );

    const deleteMutation = useApiMutation(
        () => salariesApi.delete(batchId),
        {
            successMessage: "تم حذف كشف الصرف",
            onSuccessCallback: () => navigate("/salaries"),
        }
    );

    const exportMutation = useApiMutation(
        async () => {
            const res = await salariesApi.exportItems(batchId);
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const a = document.createElement("a");
            a.href = url;
            a.download = `salary_batch_${batchId}.xlsx`;
            a.click();
            window.URL.revokeObjectURL(url);
            return res;
        },
        { successMessage: "تم تحميل ملف Excel" }
    );

    const importMutation = useApiMutation(
        (file: File) => salariesApi.importResults(batchId, file),
        {
            invalidateKeys: [["salary-batch", batchId], ["salary-batch-items", batchId]],
            successMessage: (res) => `تم تحديث ${res.data.updatedCount} عنصر`,
            onSuccessCallback: (res) => {
                setPage(1);
                setImportResult(res.data as ImportResult);
            }
        }
    );

    const handleApprove = () => approveMutation.mutate(undefined);

    const handleDelete = async () => {
        const ok = await confirm({
            title: "حذف كشف الصرف",
            description: "هل أنت متأكد من حذف هذا الكشف؟ لا يمكن التراجع عن هذا الإجراء.",
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(undefined);
    };

    const handleImport = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        setImportResult(null);
        importMutation.mutate(file, {
            onSettled: () => { if (e.target) e.target.value = ""; }
        });
    };

    const columns = useMemo<CustomColumnDef<SalaryBatchItem>[]>(() => [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row) => <span className="font-mono text-primary">{row.fileNumber || "—"}</span>
        },
        {
            key: "retireeName",
            header: "المتقاعد",
            accessor: (row) => row.retireeName || "—"
        },
        {
            key: "amount",
            header: "المبلغ",
            render: (row) => <span className="font-mono font-semibold">{row.amount.toLocaleString()} ر.ي</span>
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => {
                const status = row.status || "";
                let variant: "success" | "warning" | "danger" | "info" | "outline" = "outline";
                if (status.includes("استلام")) variant = "success";
                else if (status.includes("قيد")) variant = "warning";
                else if (status.includes("فشل")) variant = "danger";
                else if (status.includes("إرسال")) variant = "info";
                return <Badge variant={variant}>{status || "—"}</Badge>;
            }
        },
        { key: "paymentBookNumber", header: "دفتر الصرف", accessor: (row) => row.paymentBookNumber || "—" },
        {
            key: "transferNumber",
            header: "رقم الحوالة",
            render: (row) => <span className="font-mono" dir="ltr">{row.transferNumber || "—"}</span>
        }
    ], []);

    if (isBatchLoading) return <Loader variant="fullscreen" />;
    if (batchError) return <AlertBanner variant="destructive" title="تعذر تحميل بيانات الكشف" />;
    if (!batch) return null;

    const isDraft = batch.status === BATCH_STATUS.DRAFT;

    return (
        <div className="w-full space-y-6 animate-in fade-in duration-500" dir="rtl">
            <Breadcrumb items={[
                { label: "لوحة التحكم", href: "/" },
                { label: "كشوفات الصرف", href: "/salaries" },
                { label: batch.batchNumber || `كشف #${batch.batchID}` },
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4 flex-wrap">
                    <Link to="/salaries" className="p-2 hover:bg-muted rounded-full transition-colors">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div className="text-start">
                        <Heading level="h3" className="font-bold text-lg md:text-xl lg:text-2xl tracking-tight">{batch.batchNumber || `كشف #${batch.batchID}`}</Heading>
                        <Text variant="description" className="mt-1">
                            {batch.bankName} — {new Date(batch.batchMonth).toLocaleDateString("ar-YE", { year: "numeric", month: "long" })}
                        </Text>
                    </div>
                </div>

                <div className="flex items-center gap-2 flex-wrap">
                    <Badge variant={batch.status === BATCH_STATUS.APPROVED ? "success" : "warning"} className="text-sm px-3 py-1">
                        {batch.status}
                    </Badge>

                    {batch.totalCount > 0 && canExport && (
                        <Button variant="outline" onClick={() => exportMutation.mutate(undefined)} loading={exportMutation.isPending}>
                            <Download className="w-4 h-4 ms-1" /> تصدير
                        </Button>
                    )}

                    {batch.status === BATCH_STATUS.APPROVED && canManage && (
                        <label className="inline-flex items-center gap-1 px-3 py-2 text-sm font-medium border rounded-md cursor-pointer hover:bg-muted transition-colors">
                            <Upload className="w-4 h-4 ms-1" />
                            {importMutation.isPending ? "جاري الاستيراد..." : "استيراد نتائج"}
                            <input type="file" accept=".xlsx,.xls" className="hidden" onChange={handleImport} disabled={importMutation.isPending} />
                        </label>
                    )}

                    {isDraft && canManage && (
                        <>
                            <Button variant="outline" onClick={() => generateMutation.mutate(undefined)} loading={generateMutation.isPending}>
                                <Play className="w-4 h-4 ms-1" /> توليد
                            </Button>
                            {canApprove && (
                                <Button onClick={handleApprove} loading={approveMutation.isPending} disabled={batch.totalCount === 0}>
                                    <CheckCircle className="w-4 h-4 ms-1" /> اعتماد
                                </Button>
                            )}
                            {canDelete && (
                                <Button variant="destructive" onClick={handleDelete} loading={deleteMutation.isPending}>
                                    <Trash2 className="w-4 h-4 ms-1" /> حذف
                                </Button>
                            )}
                        </>
                    )}
                </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 sm:gap-4">
                <StatCard title="عدد المستفيدين" value={batch.totalCount} icon={<Users className="w-4 h-4 text-primary" />} />
                <StatCard title="الإجمالي" value={`${batch.totalAmount.toLocaleString()} ر.ي`} icon={<DollarSign className="w-4 h-4 text-primary" />} variant="blue" />
                <StatCard title="أنشأ بواسطة" value={batch.importedByUserName || "—"} icon={<User className="w-4 h-4 text-primary" />} variant="purple" />
                <StatCard title="اعتمد بواسطة" value={batch.approvedByUserName || "—"} description={batch.approvedAt ? formatDate(batch.approvedAt) : undefined} icon={<CheckCircle2 className="w-4 h-4 text-primary" />} variant="emerald" />
            </div>

            {generateResult && (
                <div className="border rounded-xl p-4 bg-emerald-500/5 dark:bg-emerald-950/20">
                    <Heading level="h3" className="font-semibold text-emerald-800 dark:text-emerald-200 mb-3">
                        ✅ نتيجة التوليد: {generateResult.generatedCount} مُضاف — {generateResult.skippedCount} مستبعد
                    </Heading>
                    {generateResult.skippedItems.length > 0 && (
                        <DataTable
                            data={generateResult.skippedItems}
                            columns={[
                                { key: "fileNumber", header: "رقم الملف", accessor: (r) => r.fileNumber },
                                { key: "retireeFullName", header: "المتقاعد", accessor: (r) => r.retireeFullName },
                                { key: "reason", header: "السبب", render: (r) => <Badge variant="warning">{r.reason}</Badge> }
                            ]}
                        />
                    )}
                </div>
            )}

            {importResult && (
                <div className="border rounded-xl p-4 bg-blue-500/5 dark:bg-blue-950/20">
                    <Heading level="h3" className="font-semibold text-blue-800 dark:text-blue-200 mb-2">
                        📥 نتيجة الاستيراد: {importResult.updatedCount} مُحدّث — {importResult.skippedCount} مستبعد
                    </Heading>
                    {importResult.errors.length > 0 && (
                        <ul className="text-sm text-red-600 dark:text-red-400 space-y-1 max-h-32 overflow-y-auto">
                            {importResult.errors.map((err, i) => <li key={i}>• {err}</li>)}
                        </ul>
                    )}
                </div>
            )}

            <DataTable
                title={`عناصر الصرف (${itemsData?.totalCount || 0})`}
                data={itemsData?.items || []}
                columns={columns}
                loading={isItemsLoading}
                pagination={{
                    page: page,
                    pageSize,
                    total: itemsData?.totalCount || 0,
                    onPageChange: setPage
                }}
                emptyMessage={isDraft ? "اضغط \"توليد\" لإنشاء العناصر تلقائياً" : "لا توجد عناصر صرف"}
            />

            {ConfirmDialogElement}
        </div>
    );
}
