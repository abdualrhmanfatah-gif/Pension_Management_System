import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Users, Eye, UserX } from "lucide-react";
import { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import { salaryRecipientApi, type SalaryRecipientDto } from "@/api/salary-recipients";
import {
    Badge,
    CrudPageLayout,
    Heading,
    StatCard,
    Button,
    InfoPair,
    BaseFormDialog,
} from "@/components/ui";
import { getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { toast } from "sonner";

export function SalaryRecipientsPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [fileId, setFileId] = useState<number | null>(null);
    const [search, setSearch] = useState("");
    const [selected, setSelected] = useState<SalaryRecipientDto | null>(null);

    const { data: recipients = [], isLoading } = useQuery({
        queryKey: ["salary-recipients-by-file", fileId],
        queryFn: () => salaryRecipientApi.getByFile(fileId!).then(r => r.data || []),
        enabled: !!fileId,
    });

    const endMutation = useMutation({
        mutationFn: ({ id, endDate }: { id: number; endDate: string }) =>
            salaryRecipientApi.end(id, { endDate }),
        onSuccess: () => {
            toast.success("تم إنهاء المستلم بنجاح");
            queryClient.invalidateQueries({ queryKey: ["salary-recipients-by-file"] });
        },
        onError: () => {
            toast.error("خطأ في إنهاء المستلم");
        },
    });

    const handleSearch = () => {
        const id = parseInt(search);
        if (!isNaN(id) && id > 0) {
            setFileId(id);
        } else {
            toast.error("أدخل رقم ملف صحيح");
        }
    };

    const activeCount = recipients.filter((r: SalaryRecipientDto) => r.isActive).length;
    const retiredCount = recipients.filter((r: SalaryRecipientDto) => r.recipientType === "متقاعد").length;
    const agentCount = recipients.filter((r: SalaryRecipientDto) => r.recipientType === "وكيل").length;

    const columns = [
        {
            key: "recipientName",
            header: "اسم المستلم",
            render: (row: SalaryRecipientDto) => (
                <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-primary overflow-hidden">
                        {getPhotoUrl(resolvePersonalPhotoPath(row)) ? (
                            <img src={getPhotoUrl(resolvePersonalPhotoPath(row))!} alt="" className="w-full h-full object-cover" loading="lazy" />
                        ) : (
                            <Users className="w-4 h-4" />
                        )}
                    </div>
                    <span className="font-bold text-foreground">{row.recipientName}</span>
                </div>
            ),
        },
        {
            key: "recipientType",
            header: "النوع",
            render: (row: SalaryRecipientDto) => {
                const variants: Record<string, "info" | "warning" | "success"> = {
                    "متقاعد": "info",
                    "وكيل": "warning",
                    "مستحق": "info",
                    "وصي": "success",
                };
                return (
                    <Badge variant={variants[row.recipientType] || "outline"}>
                        {row.recipientType}
                    </Badge>
                );
            },
        },
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: SalaryRecipientDto) => (
                <Button
                    variant="link"
                    size="sm"
                    className="h-auto p-0 text-sm font-bold"
                    onClick={(e) => {
                        e.stopPropagation();
                        navigate(`/files/${row.fileID}`);
                    }}
                >
                    {row.fileNumber || "—"}
                </Button>
            ),
        },
        {
            key: "startDate",
            header: "تاريخ البدء",
            render: (row: SalaryRecipientDto) => (
                <span className="text-sm">
                    {row.startDate ? new Date(row.startDate).toLocaleDateString("ar-SA") : "—"}
                </span>
            ),
        },
        {
            key: "endDate",
            header: "تاريخ الانتهاء",
            render: (row: SalaryRecipientDto) => (
                <span className="text-sm">
                    {row.endDate ? new Date(row.endDate).toLocaleDateString("ar-SA") : "—"}
                </span>
            ),
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: SalaryRecipientDto) => (
                <Badge variant={row.isActive ? "success" : "destructive"}>
                    {row.isActive ? "نشط" : "غير نشط"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: SalaryRecipientDto) => (
                <div className="flex gap-1">
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="عرض التفاصيل"
                        onClick={(e) => {
                            e.stopPropagation();
                            setSelected(row);
                        }}
                    >
                        <Eye className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="مستلمو المعاش"
            description="عرض إدارة جميع مستلمي المعاش عبر ملفات التقاعد"
            icon={<Users className="w-6 h-6" />}
            columns={columns}
            data={fileId ? recipients : []}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="أدخل رقم الملف واضغط Enter..."
            searchValue={search}
            onSearchChange={setSearch}
            onSearchKeyDown={(e) => {
                if (e.key === "Enter") handleSearch();
            }}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.recipientID}
            pagination={{
                page: 1,
                pageSize: 50,
                total: recipients.length,
                onPageChange: () => {},
            }}
            emptyMessage="أدخل رقم الملف لعرض مستلمي المعاش"
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي المستلمين"
                    value={recipients.length}
                    icon={<Users className="w-5 h-5" />}
                    variant="blue"
                    loading={isLoading}
                />
                <StatCard
                    title="النشطون"
                    value={activeCount}
                    icon={<Users className="w-5 h-5" />}
                    variant="success"
                    loading={isLoading}
                />
                <StatCard
                    title="المتقاعدون"
                    value={retiredCount}
                    icon={<Users className="w-5 h-5" />}
                    variant="info"
                    loading={isLoading}
                />
                <StatCard
                    title="الوكلاء"
                    value={agentCount}
                    icon={<Users className="w-5 h-5" />}
                    variant="warning"
                    loading={isLoading}
                />
            </div>

            {/* View Dialog */}
            <BaseFormDialog
                title="تفاصيل المستلم"
                isOpen={!!selected}
                onClose={() => setSelected(null)}
                size="md"
                submitLabel="إغلاق"
                onSubmit={() => setSelected(null)}
            >
                {selected && (
                    <div className="space-y-4">
                        <div className="flex items-center gap-4">
                            <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center text-primary overflow-hidden">
                                {getPhotoUrl(resolvePersonalPhotoPath(selected)) ? (
                                    <img src={getPhotoUrl(resolvePersonalPhotoPath(selected))!} alt="" className="w-full h-full object-cover" loading="lazy" />
                                ) : (
                                    <Users className="w-8 h-8" />
                                )}
                            </div>
                            <div>
                                <Heading level="h3" className="text-lg font-bold">{selected.recipientName}</Heading>
                                <Badge variant="info">{selected.recipientType}</Badge>
                            </div>
                        </div>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <InfoPair label="رقم الملف" value={selected.fileNumber || "—"} />
                            <InfoPair
                                label="تاريخ البدء"
                                value={selected.startDate ? new Date(selected.startDate).toLocaleDateString("ar-SA") : "—"}
                            />
                            <InfoPair
                                label="تاريخ الانتهاء"
                                value={selected.endDate ? new Date(selected.endDate).toLocaleDateString("ar-SA") : "—"}
                            />
                            <InfoPair
                                label="الحالة"
                                value={
                                    <Badge variant={selected.isActive ? "success" : "destructive"}>
                                        {selected.isActive ? "نشط" : "غير نشط"}
                                    </Badge>
                                }
                            />
                        </div>
                        {selected.notes && (
                            <div>
                                <span className="text-sm text-muted-foreground">ملاحظات:</span>
                                <p className="text-sm mt-1">{selected.notes}</p>
                            </div>
                        )}
                        <div className="flex gap-2 pt-4">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => navigate(`/files/${selected.fileID}`)}
                            >
                                عرض الملف
                            </Button>
                            {selected.isActive && (
                                <Button
                                    variant="destructive"
                                    size="sm"
                                    onClick={() => {
                                        const endDate = prompt("أدخل تاريخ الانتهاء (YYYY-MM-DD):");
                                        if (endDate) {
                                            endMutation.mutate({ id: selected.recipientID, endDate });
                                            setSelected(null);
                                        }
                                    }}
                                >
                                    <UserX className="w-4 h-4 mr-1" />
                                    إنهاء المستلم
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
