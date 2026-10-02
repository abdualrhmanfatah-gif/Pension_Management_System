import { useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, CheckCircle2, Download, FileText, Gavel, RefreshCw, Search, Trash2, Upload, XCircle } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate,useParams } from "react-router-dom";
import { toast } from "sonner";

import type { SettlementItemDto } from "@/api/settlements";
import { settlementApi } from "@/api/settlements";
import { LinkRetireeDialog } from "@/components/domain/settlements/LinkRetireeDialog";
import { Breadcrumb,Button, CrudPageLayout, EmptyState, Loader, PageLayout, StatCard, useConfirm } from "@/components/ui";
import { AlertBanner } from "@/components/ui/AlertBanner";
import { useApiMutation } from "@/hooks";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Page ──────────────────────────────────────────────

export function SettlementBatchDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const canManage = useHasPermission("Settlement.Manage");
    const canExport = useHasPermission("Export.Financial");
    const { confirm } = useConfirm();
    const queryClient = useQueryClient();
    const fileInputRef = useRef<HTMLInputElement>(null);
    const correctionFileInputRef = useRef<HTMLInputElement>(null);

    const { searchTerm, setSearchTerm, debouncedValue: debouncedSearch } = useDebouncedSearch();
    const [focusedIndex, setFocusedIndex] = useState(-1);
    const focusedIndexRef = useRef(-1);
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [itemsPage, setItemsPage] = useState(1);
    const itemsPageSize = 20;
    const [isGeneratingFailures, setIsGeneratingFailures] = useState(false);

    // ── Debounce Search ────────────────────────────────
    useEffect(() => {
        setItemsPage(1);
        setFocusedIndex(-1);
        focusedIndexRef.current = -1;
    }, [debouncedSearch]);

    const importCorrectionMutation = useApiMutation(
        async (file: File) => {
            const batchId = decision?.data?.batchID;
            if (!batchId) {
                console.error("Batch ID is missing in decision data", decision?.data);
                throw new Error("رقم الكشف غير متوفر؛ يرجى تحديث الصفحة والمحاولة مرة أخرى.");
            }
            
            return await settlementApi.importCorrection(batchId, {
                file,
                decisionNumber: decision.data.decisionNumber,
                decisionDate: decision.data.decisionDate,
                decisionType: decision.data.decisionType || undefined,
                notes: decision.data.notes || undefined,
            });
        },
        {
            successMessage: "تم استكمال الاستيراد بنجاح",
            onSuccessCallback: () => {
                setItemsPage(1);
                queryClient.invalidateQueries({ queryKey: ["settlement-decision", id] });
                queryClient.invalidateQueries({ queryKey: ["settlement-decision-items", id] });
            }
        }
    );

    // For Link Dialog
    const [linkDialogOpen, setLinkDialogOpen] = useState(false);
    const [selectedItemToLink, setSelectedItemToLink] = useState<SettlementItemDto | null>(null);

    const { data: decision, isLoading: loadingDecision, isError: decisionError } = useQuery({
        queryKey: ["settlement-decision", id],
        queryFn: () => settlementApi.getDecision(parseInt(id!)),
        enabled: !!id
    });

    const { data: itemsPaged, isLoading: loadingItems } = useQuery({
        queryKey: ["settlement-decision-items", id, { page: itemsPage, pageSize: itemsPageSize, searchTerm: debouncedSearch }],
        queryFn: () => settlementApi.getDecisionItems(parseInt(id!), { page: itemsPage, pageSize: itemsPageSize, searchTerm: debouncedSearch || undefined }),
        enabled: !!id
    });

    const items = useMemo(() => itemsPaged?.items || [], [itemsPaged]);
    const itemsTotalCount = itemsPaged?.totalCount || 0;

    const loading = loadingDecision || loadingItems;

    const deleteMutation = useApiMutation(
        async () => {
            await settlementApi.deleteDecision(parseInt(id!));
        },
        {
            successMessage: "تم حذف القرار بنجاح",
            onSuccessCallback: () => navigate("/settlements")
        }
    );

    const exportMutation = useApiMutation(
        async () => {
            const res = await settlementApi.exportDecision(parseInt(id!));
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const a = document.createElement("a");
            a.href = url;
            a.download = `settlement_decision_${id}.xlsx`;
            a.click();
            window.URL.revokeObjectURL(url);
            return res;
        },
        { successMessage: "تم تحميل ملف Excel" }
    );

    const downloadStoredFailureReport = useCallback(async () => {
        setIsGeneratingFailures(true);
        try {
            const response = await settlementApi.getFailureReport(parseInt(id!));

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `failures_report_decision_${id}.xlsx`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (error: unknown) {
            console.error("Failed to download failure report", error);
            const err = error as { response?: { data?: Blob } };
            const message = err.response?.data instanceof Blob ? await err.response.data.text() : "فشل تحميل تقرير الأخطاء";
            toast.error(message);
        } finally {
            setIsGeneratingFailures(false);
        }
    }, [id]);

    const handleFileChange = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setIsGeneratingFailures(true);
        try {
            const response = await settlementApi.exportFailures(parseInt(id!), file);

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `failures_report_decision_${id}.xlsx`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (error: unknown) {
            console.error("Failed to download error report", error);
            const err = error as { response?: { data?: Blob } };
            const message = err.response?.data instanceof Blob ? await err.response.data.text() : "فشل فحص الملف";
            toast.error(message);
        } finally {
            setIsGeneratingFailures(false);
            if (fileInputRef.current) fileInputRef.current.value = "";
        }
    }, [id, toast]);

    const handleDelete = async () => {
        if (!(await confirm({ title: "تأكيد الإجراء", description: "هل أنت متأكد من حذف هذا القرار وجميع بنوده؟", variant: "danger" }))) return;
        deleteMutation.mutate({});
    };

    const matchedCount = items.filter(i => i.retireeID).length;
    const unmatchedCount = items.filter(i => !i.retireeID).length;

    const itemColumns = [
        {
            key: "serial",
            header: "ت",
            render: (row: SettlementItemDto) => (
                <span className="font-mono text-muted-foreground">{row.serialNumber}</span>
            )
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: SettlementItemDto) => (
                <span className="font-mono">{row.militaryNumber || "-"}</span>
            )
        },
        {
            key: "name",
            header: "الاسم",
            render: (row: SettlementItemDto) => (
                <div>
                    <div className="font-medium">{row.retireeFullName}</div>
                    {row.retireeID ? (
                        <div className="text-xs text-success flex items-center gap-1 mt-0.5">
                            <CheckCircle2 className="w-4 h-4" /> مطابق
                        </div>
                    ) : (
                        <div className="text-xs text-warning flex items-center gap-1 mt-0.5">
                            <XCircle className="w-4 h-4" /> غير مطابق
                        </div>
                    )}
                </div>
            )
        },
        {
            key: "ranks",
            header: "الرتبة (الحالية ➔ الجديدة)",
            render: (row: SettlementItemDto) => (
                <div className="text-xs">
                    <span className="text-muted-foreground">{row.currentRankName}</span>
                    <span className="mx-2">➔</span>
                    <span className="font-bold text-primary">{row.processedRankName}</span>
                </div>
            )
        },
        {
            key: "salary",
            header: "المعاش المعتمد",
            render: (row: SettlementItemDto) => (
                <span className="font-mono font-bold">
                    {(row.processedRankSalary ?? 0).toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "actions",
            header: "",
            render: (row: SettlementItemDto) => (
                <div className="flex items-center gap-2">
                    {!row.retireeID && (
                        <Button
                            variant="secondary"
                            size="sm"
                            className="text-xs h-7"
                            onClick={() => {
                                setSelectedItemToLink(row);
                                setLinkDialogOpen(true);
                            }}
                        >
                            ربط يدوي
                        </Button>
                    )}
                </div>
            )
        }
    ];

    // ── Keyboard Navigation ────────────────────────────
    const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setFocusedIndex(prev => {
                const next = prev + 1;
                const clamped = next < items.length ? next : prev;
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
            if (idx >= 0 && idx < items.length) {
                const row = items[idx];
                if (!row.retireeID) {
                    setSelectedItemToLink(row);
                    setLinkDialogOpen(true);
                }
            }
        } else if (e.key === "Escape") {
            setFocusedIndex(-1);
            focusedIndexRef.current = -1;
        }
    }, [items]);

    // Scroll to focused row
    useEffect(() => {
        if (focusedIndex >= 0) {
            const row = document.querySelector(`[data-row-index="${focusedIndex}"]`)
                      || document.querySelector(`[data-index="${focusedIndex}"]`);
            if (row) {
                row.scrollIntoView({ behavior: "smooth", block: "nearest" });
            }
        }
    }, [focusedIndex]);

    if (loading) {
        return <Loader variant="fullscreen" />;
    }

    if (decisionError) {
        return <AlertBanner variant="destructive" title="تعذر تحميل بيانات القرار" />;
    }

    if (!decision?.data) {
        return <EmptyState icon={<FileText className="h-12 w-12" /> as any} title="لم يتم العثور على القرار" description="عذراً، لم يتم العثور على القرار المطلوب." />;
    }

    const dec = decision.data;

    return (
        <PageLayout className="p-4 sm:p-6">
            <Breadcrumb 
                items={[
                    { label: "قائمة التسويات", href: "/settlements" },
                    { label: `قرار رقم ${dec.decisionNumber}` }
                ]} 
            />

            <CrudPageLayout<SettlementItemDto>
                title={`قرار تسوية رقم ${dec.decisionNumber}`}
                description={`تاريخ القرار: ${formatDate(dec.decisionDate)} • النوع: ${dec.decisionType || "تسوية رتب"}`}
                icon={<Gavel className="w-6 h-6 text-primary" />}
                columns={itemColumns}
                data={items}
                loading={loadingItems}
                searchable
                searchInHeader
                searchPlaceholder="بحث بالاسم أو الرقم العسكري..."
                searchValue={searchTerm}
                onSearchChange={(val) => setSearchTerm(val)}
                onSearchKeyDown={handleSearchKeyDown}
                searchInputRef={searchInputRef}
                focusedRowIndex={focusedIndex}
                pagination={{
                    page: itemsPage,
                    pageSize: itemsPageSize,
                    total: itemsTotalCount,
                    onPageChange: (p) => setItemsPage(p)
                }}
                headerActions={
                    <>
                        <Button variant="outline" className="gap-2" onClick={() => navigate(`/settlements/${id}/print`)}>
                            <FileText className="w-4 h-4" /> طباعة القرار
                        </Button>
                        {canExport && (
                            <Button variant="outline" className="gap-2" onClick={() => exportMutation.mutate(undefined)} loading={exportMutation.isPending}>
                                <Download className="w-4 h-4" /> تصدير Excel
                            </Button>
                        )}
                        {canManage && (
                            <Button
                                variant="destructive"
                                className="gap-2"
                                onClick={handleDelete}
                                loading={deleteMutation.isPending}
                            >
                                <Trash2 className="w-4 h-4" /> حذف القرار
                            </Button>
                        )}
                    </>
                }
            >
                {/* ─── Custom Page Content ─── */}
                <div className="space-y-6">
                    {/* Proactive Data Shortage Alert */}
                    {(dec.sourceRowsCount > dec.itemCount || dec.hasFailureReport) && (
                        <AlertBanner
                            variant="warning"
                            title="تم اكتشاف نقص في البيانات المستوردة"
                            description={`ملف الـ Excel الأصلي يحتوي على ${dec.sourceRowsCount} صف، بينما تم استيراد ${dec.itemCount} بند فقط. هناك ${dec.sourceRowsCount - dec.itemCount} صف فاشل.`}
                            icon={<AlertTriangle className="h-5 w-5" />}
                            className="flex flex-col md:flex-row items-center justify-between gap-4"
                        >
                            <div className="flex items-center gap-2 flex-wrap">
                                {dec.hasFailureReport && (
                                    <Button
                                        variant="warning"
                                        size="sm"
                                        className="gap-2 font-bold"
                                        onClick={downloadStoredFailureReport}
                                        loading={isGeneratingFailures}
                                    >
                                        <Download className="w-4 h-4" />
                                        تحميل تقرير الأخطاء
                                    </Button>
                                )}
                                <input
                                    type="file"
                                    ref={correctionFileInputRef}
                                    className="hidden"
                                    accept=".xlsx,.xls"
                                    onChange={(e) => {
                                        const file = e.target.files?.[0];
                                        if (file) importCorrectionMutation.mutate(file);
                                        e.target.value = "";
                                    }}
                                />
                                <Button 
                                    variant="primary" 
                                    size="sm" 
                                    className="gap-2 font-bold"
                                    onClick={() => correctionFileInputRef.current?.click()}
                                    loading={importCorrectionMutation.isPending}
                                >
                                    <Upload className="w-4 h-4" />
                                    رفع واستكمال الاستيراد
                                </Button>
                                <input
                                    type="file"
                                    ref={fileInputRef}
                                    className="hidden"
                                    accept=".xlsx,.xls"
                                    onChange={handleFileChange}
                                />
                                <Button 
                                    variant="outline" 
                                    size="sm" 
                                    className="gap-2 border-warning/30 hover:bg-warning/10 text-warning"
                                    onClick={() => fileInputRef.current?.click()}
                                    loading={isGeneratingFailures}
                                >
                                    <Search className="w-4 h-4" />
                                    فحص ملف جديد
                                </Button>
                            </div>
                        </AlertBanner>
                    )}

                    {/* Stats Grid */}
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-2 sm:gap-4">
                        <StatCard
                            title="إجمالي الفروقات المالية"
                            value={`${(dec.totalAmount ?? 0).toLocaleString()} ر.ي`}
                            icon={<Gavel className="w-6 h-6" />}
                            variant="blue"
                        />
                        <StatCard
                            title="سجلات متطابقة (تم الربط)"
                            value={matchedCount}
                            icon={<CheckCircle2 className="w-6 h-6" />}
                            variant="emerald"
                            description={`${Math.round((matchedCount / items.length) * 100) || 0}% من إجمالي السجلات`}
                        />
                        <StatCard
                            title="سجلات معلقة (تحتاج ربط)"
                            value={unmatchedCount}
                            icon={<RefreshCw className="w-6 h-6" />}
                            variant="amber"
                            description="يتطلب ربطاً يدوياً بالمتقاعدين"
                        />
                    </div>
                </div>
            </CrudPageLayout>

            {/* Dialogs */}
            <LinkRetireeDialog
                open={linkDialogOpen}
                onOpenChange={(open) => {
                    if (!open) {
                        setLinkDialogOpen(false);
                        setSelectedItemToLink(null);
                    }
                }}
                item={selectedItemToLink}
                onLinked={() => {
                    queryClient.invalidateQueries({ queryKey: ["settlement-decision-items", id] });
                }}
            />
        </PageLayout>
    );
}
