import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
    AlertTriangle,
    FileSearch,
    FolderOpen,
    Shield,
    ShieldCheck,
    XCircle,
} from "lucide-react";
import { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import type { RetirementFileReportRow } from "@/api/retirement";
import type { MissingDocumentInfo } from "@/types/retirement";
import {
    Badge,
    BaseFormDialog,
    Button,
    Checkbox,
    CrudPageLayout,
    FormField,
    Input,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { toast } from "sonner";

const pledgeSchema = z.object({
    deadline: z.string().min(1, "الموعد النهاي مطلوب"),
    pledgedByName: z.string().min(1, "اسم الملتزم مطلوب"),
    reason: z.string().optional(),
});

const exemptionSchema = z.object({
    directiveSource: z.string().min(1, "مصدر التوجيه مطلوب"),
    exemptionDate: z.string().min(1, "تاريخ الإعفاء مطلوب"),
    notes: z.string().optional(),
});

type PledgeForm = z.infer<typeof pledgeSchema>;
type ExemptionForm = z.infer<typeof exemptionSchema>;

export function DocumentReviewPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { searchTerm, setSearchTerm, debouncedValue } =
        useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);

    const [showPledge, setShowPledge] = useState(false);
    const [showExemption, setShowExemption] = useState(false);
    const [selectedFile, setSelectedFile] =
        useState<RetirementFileReportRow | null>(null);
    const [selectedMissingDocIds, setSelectedMissingDocIds] = useState<number[]>([]);

    const pledgeForm = useForm<PledgeForm>({
        resolver: zodResolver(pledgeSchema),
        defaultValues: { deadline: "", pledgedByName: "", reason: "" },
    });

    const exemptionForm = useForm<ExemptionForm>({
        resolver: zodResolver(exemptionSchema),
        defaultValues: {
            directiveSource: "",
            exemptionDate: "",
            notes: "",
        },
    });

    const { data: summary } = useQuery({
        queryKey: ["retirement-report-summary"],
        queryFn: () =>
            retirementApi.getReportSummary({
                pageNumber: 1,
                pageSize: 1,
            }),
    });

    const { data, isLoading } = useQuery({
        queryKey: ["retirement-report", { page, search: debouncedValue }],
        queryFn: () =>
            retirementApi.getReport({
                pageNumber: page,
                pageSize: 20,
                searchTerm: debouncedValue || undefined,
            }),
    });

    const items = data?.items || [];
    const totalCount = data?.totalCount || 0;

    const { data: missingDocuments = [] } = useQuery({
        queryKey: ["missing-documents", selectedFile?.fileID],
        queryFn: () => retirementApi.getMissingDocuments(selectedFile!.fileID),
        enabled: !!selectedFile?.fileID,
    });

    const createPledgeMutation = useMutation({
        mutationFn: (data: PledgeForm) =>
            retirementApi.createPledge({
                fileID: selectedFile!.fileID,
                missingDocumentIDs: selectedMissingDocIds,
                deadline: data.deadline,
                pledgedByName: data.pledgedByName,
                pledgedByType: "Manual",
                reason: data.reason,
            }),
        onSuccess: () => {
            toast.success("تم تسجيل التعهد بنجاح");
            queryClient.invalidateQueries({
                queryKey: ["retirement-report"],
            });
            setShowPledge(false);
            pledgeForm.reset();
            setSelectedMissingDocIds([]);
        },
        onError: () => {
            toast.error("خطأ في تسجيل التعهد");
        },
    });

    const createExemptionMutation = useMutation({
        mutationFn: (data: ExemptionForm) =>
            retirementApi.createExemption({
                fileID: selectedFile!.fileID,
                missingDocumentIDs: selectedMissingDocIds,
                directiveSource: data.directiveSource,
                exemptionDate: data.exemptionDate,
                notes: data.notes,
            }),
        onSuccess: () => {
            toast.success("تم تسجيل الإعفاء بنجاح");
            queryClient.invalidateQueries({
                queryKey: ["retirement-report"],
            });
            setShowExemption(false);
            exemptionForm.reset();
            setSelectedMissingDocIds([]);
        },
        onError: () => {
            toast.error("خطأ في تسجيل الإعفاء");
        },
    });

    const columns = [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: RetirementFileReportRow) => (
                <span className="text-sm font-mono font-bold">
                    {row.fileNumber || "—"}
                </span>
            ),
        },
        {
            key: "retireeName",
            header: "اسم المتقاعد",
            render: (row: RetirementFileReportRow) => (
                <span className="font-bold">{row.retireeName || "—"}</span>
            ),
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: RetirementFileReportRow) => (
                <span className="text-sm font-mono text-primary font-bold">
                    {row.militaryNumber || "—"}
                </span>
            ),
        },
        {
            key: "rankName",
            header: "الرتبة",
            render: (row: RetirementFileReportRow) => (
                <span className="text-sm">{row.rankName || "—"}</span>
            ),
        },
        {
            key: "statusName",
            header: "الحالة",
            render: (row: RetirementFileReportRow) => (
                <Badge variant="outline">{row.statusName || "—"}</Badge>
            ),
        },
        {
            key: "missingDocumentsCount",
            header: "الوثائق الناقصة",
            render: (row: RetirementFileReportRow) => (
                <Badge
                    variant={
                        row.missingDocumentsCount > 0 ? "danger" : "success"
                    }
                >
                    {row.missingDocumentsCount}
                </Badge>
            ),
        },
        {
            key: "reviewStatusName",
            header: "حالة المراجعة",
            render: (row: RetirementFileReportRow) => (
                <Badge
                    variant={
                        row.reviewStatusName === "معتمد"
                            ? "success"
                            : row.reviewStatusName === "مرفوض"
                                ? "danger"
                                : "warning"
                    }
                >
                    {row.reviewStatusName || "—"}
                </Badge>
            ),
        },
        {
            key: "branchName",
            header: "الفرع",
            render: (row: RetirementFileReportRow) => (
                <span className="text-sm">{row.branchName || "—"}</span>
            ),
        },
        {
            key: "retirementDate",
            header: "تاريخ التقاعد",
            render: (row: RetirementFileReportRow) => (
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
            key: "isAlive",
            header: "الحالة",
            render: (row: RetirementFileReportRow) => (
                <Badge variant={row.isAlive ? "success" : "danger"}>
                    {row.isAlive ? "نشط" : "متوفى"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: RetirementFileReportRow) => (
                <div className="flex gap-1">
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="عرض الملف"
                        onClick={(e) => {
                            e.stopPropagation();
                            navigate(`/files/${row.fileID}`);
                        }}
                    >
                        <FolderOpen className="w-4 h-4" />
                    </Button>
                    {row.missingDocumentsCount > 0 && (
                        <>
                            <Button
                                variant="ghost-action"
                                size="sm"
                                title="تسجيل تعهد"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setSelectedFile(row);
                                    setShowPledge(true);
                                }}
                            >
                                <Shield className="w-4 h-4" />
                            </Button>
                            <Button
                                variant="ghost-action"
                                size="sm"
                                title="تسجيل إعفاء"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setSelectedFile(row);
                                    setShowExemption(true);
                                }}
                            >
                                <ShieldCheck className="w-4 h-4" />
                            </Button>
                        </>
                    )}
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="مراجعة البيانات الناقصة"
            description="فحص ورفع الوثائق الناقصة للملفات وتسجيل التعهدات والإعفاءات"
            icon={<FileSearch className="w-6 h-6" />}
            columns={columns}
            data={items}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث برقم الملف / اسم المتقاعد..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            onSearchKeyDown={(e) => {
                if (e.key === "ArrowDown" || e.key === "ArrowUp")
                    e.preventDefault();
            }}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.fileID}
            pagination={{
                page,
                pageSize: 20,
                total: totalCount,
                onPageChange: setPage,
            }}
        >
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي الملفات"
                    value={summary?.totalCount || 0}
                    icon={<FolderOpen className="w-5 h-5" />}
                    variant="blue"
                />
                <StatCard
                    title="ملفات ناقصة"
                    value={summary?.incompleteCount || 0}
                    icon={<AlertTriangle className="w-5 h-5" />}
                    variant="amber"
                />
                <StatCard
                    title="قيد المراجعة"
                    value={summary?.pendingReviewCount || 0}
                    icon={<FileSearch className="w-5 h-5" />}
                    variant="orange"
                />
                <StatCard
                    title="ملفات مغلقة"
                    value={summary?.closedCount || 0}
                    icon={<XCircle className="w-5 h-5" />}
                    variant="rose"
                />
            </div>

            <BaseFormDialog
                title="تسجيل تعهد"
                description={`الملف: ${selectedFile?.fileNumber || ""} — ${selectedFile?.retireeName || ""}`}
                isOpen={showPledge}
                onClose={() => {
                    setShowPledge(false);
                    pledgeForm.reset();
                    setSelectedMissingDocIds([]);
                }}
                onSubmit={pledgeForm.handleSubmit((data) =>
                    createPledgeMutation.mutate(data)
                )}
                isLoading={createPledgeMutation.isPending}
                size="md"
            >
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField
                        label="الموعد النهاي *"
                        error={pledgeForm.formState.errors.deadline?.message}
                    >
                        <Input
                            {...pledgeForm.register("deadline")}
                            type="date"
                        />
                    </FormField>
                    <FormField
                        label="اسم الملتزم *"
                        error={
                            pledgeForm.formState.errors.pledgedByName?.message
                        }
                    >
                        <Input
                            {...pledgeForm.register("pledgedByName")}
                            placeholder="اسم الملتزم"
                        />
                    </FormField>
                    <div className="sm:col-span-2">
                        <FormField label="السبب">
                            <Input
                                {...pledgeForm.register("reason")}
                                placeholder="سبب التعهد"
                            />
                        </FormField>
                    </div>
                    {missingDocuments.length > 0 && (
                        <div className="sm:col-span-2">
                            <FormField label="الوثائق الناقصة المحددة">
                                <div className="space-y-2 max-h-40 overflow-y-auto border border-input rounded-md p-3">
                                    {missingDocuments.map((doc: MissingDocumentInfo) => (
                                        <Checkbox
                                            key={doc.missingDocumentID}
                                            checked={selectedMissingDocIds.includes(doc.missingDocumentID)}
                                            onCheckedChange={(checked) => {
                                                if (checked) {
                                                    setSelectedMissingDocIds((prev) => [...prev, doc.missingDocumentID]);
                                                } else {
                                                    setSelectedMissingDocIds((prev) => prev.filter((id) => id !== doc.missingDocumentID));
                                                }
                                            }}
                                            label={doc.documentTypeName || `وثيقة #${doc.documentTypeID}`}
                                        />
                                    ))}
                                </div>
                            </FormField>
                        </div>
                    )}
                </div>
            </BaseFormDialog>

            <BaseFormDialog
                title="تسجيل إعفاء"
                description={`الملف: ${selectedFile?.fileNumber || ""} — ${selectedFile?.retireeName || ""}`}
                isOpen={showExemption}
                onClose={() => {
                    setShowExemption(false);
                    exemptionForm.reset();
                    setSelectedMissingDocIds([]);
                }}
                onSubmit={exemptionForm.handleSubmit((data) =>
                    createExemptionMutation.mutate(data)
                )}
                isLoading={createExemptionMutation.isPending}
                size="md"
            >
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField
                        label="مصدر التوجيه *"
                        error={
                            exemptionForm.formState.errors.directiveSource
                                ?.message
                        }
                    >
                        <Input
                            {...exemptionForm.register("directiveSource")}
                            placeholder="مصدر التوجيه"
                        />
                    </FormField>
                    <FormField
                        label="تاريخ الإعفاء *"
                        error={
                            exemptionForm.formState.errors.exemptionDate
                                ?.message
                        }
                    >
                        <Input
                            {...exemptionForm.register("exemptionDate")}
                            type="date"
                        />
                    </FormField>
                    <div className="sm:col-span-2">
                        <FormField label="ملاحظات">
                            <Input
                                {...exemptionForm.register("notes")}
                                placeholder="ملاحظات"
                            />
                        </FormField>
                    </div>
                    {missingDocuments.length > 0 && (
                        <div className="sm:col-span-2">
                            <FormField label="الوثائق الناقصة المحددة">
                                <div className="space-y-2 max-h-40 overflow-y-auto border border-input rounded-md p-3">
                                    {missingDocuments.map((doc: MissingDocumentInfo) => (
                                        <Checkbox
                                            key={doc.missingDocumentID}
                                            checked={selectedMissingDocIds.includes(doc.missingDocumentID)}
                                            onCheckedChange={(checked) => {
                                                if (checked) {
                                                    setSelectedMissingDocIds((prev) => [...prev, doc.missingDocumentID]);
                                                } else {
                                                    setSelectedMissingDocIds((prev) => prev.filter((id) => id !== doc.missingDocumentID));
                                                }
                                            }}
                                            label={doc.documentTypeName || `وثيقة #${doc.documentTypeID}`}
                                        />
                                    ))}
                                </div>
                            </FormField>
                        </div>
                    )}
                </div>
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
