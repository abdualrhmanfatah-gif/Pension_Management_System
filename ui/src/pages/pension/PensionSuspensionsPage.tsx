import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye, Inbox, Loader2,PauseCircle, PlayCircle, Search } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    EmptyState,
    FormField,
    Input,
    NativeSelect,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import type { RetirementFileListItem } from "@/types/retirement";

const suspendSchema = z.object({
    fileID: z.string().min(1, "اختر ملف المتقاعد"),
    suspensionReason: z.string().min(1, "سبب الإيقاف مطلوب"),
    suspensionSource: z.string().min(1, "مصدر الإيقاف مطلوب"),
});

type SuspendForm = z.infer<typeof suspendSchema>;

interface Suspension {
    suspensionID: number;
    retireeName: string;
    nationalID: string;
    militaryNumber: string;
    fileNumber: string;
    suspensionDate: string;
    resumeDate: string;
    suspensionReason: string;
    status: string;
}

export function PensionSuspensionsPage() {
    const queryClient = useQueryClient();
    const { searchTerm, setSearchTerm } = useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [statusFilter, setStatusFilter] = useState("all");
    const [fromDate, setFromDate] = useState("");
    const [toDate, setToDate] = useState("");
    const [showSuspendDialog, setShowSuspendDialog] = useState(false);
    const [selected, setSelected] = useState<Suspension | null>(null);
    const [fileSearch, setFileSearch] = useState("");
    const [debouncedFileSearch, setDebouncedFileSearch] = useState("");
    const [selectedFile, setSelectedFile] = useState<RetirementFileListItem | null>(null);
    const [page, setPage] = useState(1);
    const pageSize = 20;

    useEffect(() => {
        const timer = setTimeout(() => setDebouncedFileSearch(fileSearch), 500);
        return () => clearTimeout(timer);
    }, [fileSearch]);

    const suspendForm = useForm<SuspendForm>({
        resolver: zodResolver(suspendSchema),
    });

    const { data: fileResults, isLoading: isSearchingFiles } = useQuery({
        queryKey: ["files-search-suspend", debouncedFileSearch],
        queryFn: () => retirementApi.getFiles({ page: 1, pageSize: 10, search: debouncedFileSearch }),
        enabled: showSuspendDialog && debouncedFileSearch.length > 1,
    });

    const { data, isLoading } = useQuery({
        queryKey: ["pension-suspensions", page, pageSize, statusFilter, searchTerm, fromDate, toDate],
        queryFn: () => retirementApi.getAllSuspensions({
            page,
            pageSize,
            status: statusFilter === "all" ? undefined : statusFilter === "active" ? "Active" : undefined,
            search: searchTerm || undefined,
            fromDate: fromDate || undefined,
            toDate: toDate || undefined,
        }),
    });

    const items: Suspension[] = Array.isArray(data?.items) ? (data.items as unknown as Suspension[]) : Array.isArray(data) ? (data as unknown as Suspension[]) : [];
    const totalCount = data?.totalCount ?? items.length;

    const filteredItems = items.filter((item) => {
        const matchesStatus = statusFilter === "all" || (statusFilter === "active" && item.status === "Active") || (statusFilter === "inactive" && item.status !== "Active");
        return matchesStatus;
    });

    const activeCount = items.filter((i) => i.status === "Active").length;

    const suspendMutation = useMutation({
        mutationFn: (formData: SuspendForm) =>
            retirementApi.createSuspension({
                fileID: Number(formData.fileID),
                suspensionReason: formData.suspensionReason,
                suspensionSource: formData.suspensionSource,
            }),
        onSuccess: () => {
            toast.success("تم إيقاف المعاش بنجاح");
            setShowSuspendDialog(false);
            suspendForm.reset();
            setFileSearch("");
            setDebouncedFileSearch("");
            setSelectedFile(null);
            queryClient.invalidateQueries({ queryKey: ["pension-suspensions"] });
        },
        onError: () => toast.error("خطأ في الإيقاف"),
    });

    const resumeMutation = useMutation({
        mutationFn: (id: number) => retirementApi.endSuspension(id, { endReason: "إعادة التفعيل" }),
        onSuccess: () => {
            toast.success("تم إعادة التفعيل بنجاح");
            queryClient.invalidateQueries({ queryKey: ["pension-suspensions"] });
        },
        onError: () => toast.error("خطأ في إعادة التفعيل"),
    });

    const columns = [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: Suspension) => <span className="font-mono font-bold text-xs">{row.fileNumber || "—"}</span>,
        },
        {
            key: "retireeName",
            header: "المتقاعد",
            render: (row: Suspension) => (
                <div>
                    <p className="font-medium text-sm">{row.retireeName}</p>
                    <p className="text-xs text-muted-foreground">{row.nationalID}</p>
                </div>
            ),
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: Suspension) => <span className="text-sm font-mono font-bold text-primary">{row.militaryNumber || "—"}</span>,
        },
        {
            key: "suspensionDate",
            header: "تاريخ الإيقاف",
            render: (row: Suspension) => <span className="text-sm">{row.suspensionDate ? new Date(row.suspensionDate).toLocaleDateString("ar-SA") : "—"}</span>,
        },
        {
            key: "resumeDate",
            header: "تاريخ الاستئناف",
            render: (row: Suspension) => <span className="text-sm">{row.resumeDate ? new Date(row.resumeDate).toLocaleDateString("ar-SA") : "—"}</span>,
        },
        {
            key: "suspensionReason",
            header: "السبب",
            render: (row: Suspension) => <span className="text-sm">{row.suspensionReason || "—"}</span>,
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: Suspension) => <Badge variant={row.status === "Active" ? "warning" : "success"}>{row.status === "Active" ? "موقوف" : "نشط"}</Badge>,
        },
        {
            key: "actions",
            header: "",
            render: (row: Suspension) => (
                <div className="flex gap-1">
                    <Button variant="ghost-action" size="sm" onClick={(e) => { e.stopPropagation(); setSelected(row); }}>
                        <Eye className="w-4 h-4" />
                    </Button>
                    {row.status === "Active" && (
                        <Button variant="ghost-action" size="sm" onClick={(e) => { e.stopPropagation(); resumeMutation.mutate(row.suspensionID); }}>
                            <PlayCircle className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="إيقاف وإعادة المعاش"
            description="إدارة إيقافات واعادة تفعيل المعاشات"
            icon={<PauseCircle className="w-6 h-6" />}
            columns={columns}
            data={filteredItems}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث بالاسم أو رقم الملف أو الرقم العسكري..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.suspensionID}
            pagination={{
                page,
                pageSize,
                total: totalCount,
                onPageChange: setPage,
            }}
            onCreateClick={() => {
                suspendForm.reset();
                setFileSearch("");
                setDebouncedFileSearch("");
                setSelectedFile(null);
                setShowSuspendDialog(true);
            }}
            headerActions={
                <div className="flex items-center gap-2">
                    <NativeSelect
                        value={statusFilter}
                        onChange={setStatusFilter}
                        options={[
                            { value: "all", label: "الكل" },
                            { value: "active", label: "موقوف" },
                            { value: "inactive", label: "نشط" },
                        ]}
                        placeholder="الحالة"
                    />
                    <Input
                        type="date"
                        value={fromDate}
                        onChange={(e) => setFromDate(e.target.value)}
                        className="w-36"
                        placeholder="من تاريخ"
                    />
                    <Input
                        type="date"
                        value={toDate}
                        onChange={(e) => setToDate(e.target.value)}
                        className="w-36"
                        placeholder="إلى تاريخ"
                    />
                </div>
            }
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 sm:gap-4 mb-6">
                <StatCard title="إجمالي الإيقافات" value={items.length} icon={<PauseCircle className="w-5 h-5" />} variant="blue" loading={isLoading} />
                <StatCard title="موقوف حالياً" value={activeCount} icon={<PauseCircle className="w-5 h-5" />} variant="warning" loading={isLoading} />
                <StatCard title="تمت الاستئنافة" value={items.length - activeCount} icon={<PlayCircle className="w-5 h-5" />} variant="success" loading={isLoading} />
            </div>

            {/* View Dialog */}
            <BaseFormDialog
                title="تفاصيل الإيقاف"
                isOpen={!!selected}
                onClose={() => setSelected(null)}
                size="md"
                submitLabel="إغلاق"
                onSubmit={() => setSelected(null)}
            >
                {selected && (
                    <div className="space-y-3 text-sm">
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">المتقاعد:</span>
                            <span className="font-medium">{selected.retireeName}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">رقم الملف:</span>
                            <span className="font-mono font-bold">{selected.fileNumber}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">تاريخ الإيقاف:</span>
                            <span className="font-medium">{new Date(selected.suspensionDate).toLocaleDateString("ar-SA")}</span>
                        </div>
                        {selected.resumeDate && (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">تاريخ الاستئناف:</span>
                                <span className="font-medium">{new Date(selected.resumeDate).toLocaleDateString("ar-SA")}</span>
                            </div>
                        )}
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">السبب:</span>
                            <span className="font-medium">{selected.suspensionReason}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">الحالة:</span>
                            <Badge variant={selected.status === "Active" ? "warning" : "success"}>{selected.status === "Active" ? "موقوف" : "نشط"}</Badge>
                        </div>
                    </div>
                )}
            </BaseFormDialog>

            {/* Suspend Dialog */}
            <BaseFormDialog
                title="إيقاف معاش"
                isOpen={showSuspendDialog}
                onClose={() => setShowSuspendDialog(false)}
                onSubmit={suspendForm.handleSubmit((d) => suspendMutation.mutate(d))}
                isLoading={suspendMutation.isPending}
                size="lg"
            >
                <input type="hidden" {...suspendForm.register("fileID")} value={selectedFile?.fileID?.toString() || ""} />
                <div className="space-y-4">
                    {/* File Search */}
                    <FormField label="البحث عن الملف *" error={suspendForm.formState.errors.fileID?.message}>
                        <div className="relative">
                            <Search className="absolute end-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                            <Input
                                placeholder="ابحث بالاسم أو رقم الملف أو الرقم العسكري..."
                                className="pe-9"
                                value={fileSearch}
                                onChange={(e) => { setFileSearch(e.target.value); setSelectedFile(null); suspendForm.setValue("fileID", ""); }}
                                autoComplete="off"
                            />
                        </div>
                    </FormField>

                    {/* Selected File Display */}
                    {selectedFile && (
                        <div className="bg-primary/5 border border-primary/20 rounded-lg p-3 flex items-center gap-3">
                            <div className="w-10 h-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-bold shrink-0">
                                {selectedFile.retireeFullName?.charAt(0)}
                            </div>
                            <div className="flex-1 text-sm">
                                <p className="font-bold">{selectedFile.retireeFullName}</p>
                                <div className="flex gap-3 text-xs text-muted-foreground">
                                    {selectedFile.fileNumber && <span>رقم الملف: <span className="font-mono" dir="ltr">{selectedFile.fileNumber}</span></span>}
                                    {selectedFile.militaryNumber && <span>الرقم العسكري: <span className="font-mono" dir="ltr">{selectedFile.militaryNumber}</span></span>}
                                </div>
                            </div>
                            <Button variant="ghost" size="sm" onClick={() => { setSelectedFile(null); setFileSearch(""); suspendForm.setValue("fileID", ""); }}>×</Button>
                        </div>
                    )}

                    {/* Search Results */}
                    {!selectedFile && debouncedFileSearch.length > 1 && (
                        <div className="max-h-48 overflow-y-auto border rounded-lg divide-y bg-card">
                            {isSearchingFiles ? (
                                <div className="flex items-center gap-2 p-4 text-muted-foreground text-sm justify-center">
                                    <Loader2 className="w-4 h-4 animate-spin" /> جاري البحث...
                                </div>
                            ) : fileResults?.items?.length === 0 ? (
                                <EmptyState icon={Inbox} title="لا توجد نتائج متطابقة" />
                            ) : (
                                fileResults?.items.map((file: RetirementFileListItem) => (
                                    <div
                                        key={file.fileID}
                                        onClick={() => {
                                            setSelectedFile(file);
                                            setFileSearch(file.retireeFullName);
                                            suspendForm.setValue("fileID", file.fileID.toString());
                                        }}
                                        className="p-3 flex items-center gap-3 cursor-pointer hover:bg-muted/40 transition-colors"
                                    >
                                        <div className="w-8 h-8 rounded-full bg-secondary text-secondary-foreground flex items-center justify-center font-bold text-xs shrink-0">
                                            {file.retireeFullName?.charAt(0)}
                                        </div>
                                        <div className="flex-1 text-sm">
                                            <p className="font-medium">{file.retireeFullName}</p>
                                            <div className="flex gap-3 text-xs text-muted-foreground">
                                                {file.fileNumber && <span className="font-mono" dir="ltr">{file.fileNumber}</span>}
                                                {file.militaryNumber && <span className="font-mono" dir="ltr">{file.militaryNumber}</span>}
                                            </div>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    )}

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <FormField label="مصدر الإيقاف *" error={suspendForm.formState.errors.suspensionSource?.message}>
                            <Input {...suspendForm.register("suspensionSource")} placeholder="مصدر الإيقاف" />
                        </FormField>
                        <FormField label="سبب الإيقاف *" error={suspendForm.formState.errors.suspensionReason?.message}>
                            <Input {...suspendForm.register("suspensionReason")} placeholder="سبب الإيقاف" />
                        </FormField>
                    </div>
                </div>
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
