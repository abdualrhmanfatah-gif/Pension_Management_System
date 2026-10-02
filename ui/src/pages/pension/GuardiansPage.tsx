import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye, ShieldCheck, Trash2, Users } from "lucide-react";
import { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    Heading,
    InfoPair,
    StatCard,
    useConfirm,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import type { GuardianSummary } from "@/types/retirement";

export function GuardiansPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const { searchTerm, setSearchTerm, debouncedValue } = useDebouncedSearch();
    const [fileId] = useState<number | null>(null);
    const [selected, setSelected] = useState<GuardianSummary | null>(null);
    const { confirm, ConfirmDialogElement } = useConfirm();

    // Search mode: global search across all guardians
    const { data: searchData = [], isLoading: isSearchLoading } = useQuery({
        queryKey: ["guardians-search", debouncedValue],
        queryFn: () => retirementApi.searchGuardians(debouncedValue, 50),
        enabled: debouncedValue.length > 1,
    });

    // File mode: show guardians for a specific file
    const { data: guardians = [], isLoading: isFileLoading } = useQuery({
        queryKey: ["guardians-by-file", fileId],
        queryFn: () => retirementApi.getGuardians(fileId!),
        enabled: !!fileId,
    });

    const isSearchMode = debouncedValue.length > 1;
    const displayData = isSearchMode ? searchData : (fileId ? guardians : []);
    const isLoading = isSearchMode ? isSearchLoading : isFileLoading;

    const endMutation = useMutation({
        mutationFn: (id: number) =>
            retirementApi.endGuardianship(id, { reason: "إنهاء وصاية" }),
        onSuccess: () => {
            toast.success("تم إنهاء الوصاية بنجاح");
            queryClient.invalidateQueries({ queryKey: ["guardians-by-file"] });
        },
        onError: () => {
            toast.error("خطأ في إنهاء الوصاية");
        },
    });


    const activeCount = displayData.filter((g: any) => g.isActive).length;

    const columns = [
        {
            key: "fullName",
            header: "الاسم",
            render: (row: any) => (
                <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-primary overflow-hidden">
                        {getPhotoUrl(resolvePersonalPhotoPath(row)) ? (
                            <img src={getPhotoUrl(resolvePersonalPhotoPath(row))!} alt="" className="w-full h-full object-cover" loading="lazy" />
                        ) : (
                            <ShieldCheck className="w-4 h-4" />
                        )}
                    </div>
                    <span className="font-bold text-foreground">{row.fullName}</span>
                </div>
            ),
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row: any) => <span className="text-sm font-mono">{row.nationalID || "—"}</span>,
        },
        {
            key: "guardianshipDocNumber",
            header: "رقم وثيقة الوصاية",
            render: (row: any) => <span className="text-sm">{row.guardianshipDocNumber || "—"}</span>,
        },
        {
            key: "phone",
            header: "الهاتف",
            render: (row: any) => <span className="text-sm">{row.phone || "—"}</span>,
        },
        ...(isSearchMode ? [
            {
                key: "fileNumber",
                header: "رقم الملف",
                render: (row: any) => (
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
                key: "retireeMilitaryNumber",
                header: "الرقم العسكري",
                render: (row: any) => <span className="text-sm font-mono">{row.retireeMilitaryNumber || "—"}</span>,
            },
        ] : []),
        {
            key: "isActive",
            header: "الحالة",
            render: (row: any) => (
                <Badge variant={row.isActive ? "success" : "destructive"}>
                    {row.isActive ? "نشط" : "غير نشط"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: any) => (
                <div className="flex gap-1">
                    {isSearchMode ? (
                        <Button
                            variant="ghost-action"
                            size="sm"
                            title="عرض الملف"
                            onClick={(e) => {
                                e.stopPropagation();
                                navigate(`/files/${row.fileID}`);
                            }}
                        >
                            <Eye className="w-4 h-4" />
                        </Button>
                    ) : (
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
                    )}
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="إدارة الأوصياء"
            description="عرض وإدارة جميع الأوصياء عبر ملفات التقاعد"
            icon={<ShieldCheck className="w-6 h-6" />}
            columns={columns}
            data={displayData}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="ابحث بالاسم أو الرقم الوطني أو رقم الملف أو الرقم العسكري..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row: any) => row.guardianID}
            pagination={{
                page: 1,
                pageSize: 50,
                total: displayData.length,
                onPageChange: () => {},
            }}
            emptyMessage="ابحث عن وصي بالاسم أو الرقم الوطني أو رقم الملف"

        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title={isSearchMode ? "إجمالي النتائج" : "إجمالي الأوصياء"}
                    value={displayData.length}
                    icon={<ShieldCheck className="w-5 h-5" />}
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
                    title="غير النشطين"
                    value={displayData.length - activeCount}
                    icon={<Trash2 className="w-5 h-5" />}
                    variant="destructive"
                    loading={isLoading}
                />
            </div>

            {/* View Dialog */}
            <BaseFormDialog
                title="تفاصيل الوصي"
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
                                    <ShieldCheck className="w-8 h-8" />
                                )}
                            </div>
                            <div>
                                <Heading level="h3" className="text-lg font-bold">{selected.fullName}</Heading>
                                <p className="text-sm text-muted-foreground">{selected.nationalID}</p>
                            </div>
                        </div>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <InfoPair label="رقم وثيقة الوصاية" value={selected.guardianshipDocNumber || "—"} />
                            <InfoPair label="رقم الهاتف" value={selected.phone || "—"} />
                            <InfoPair
                                label="الحالة"
                                value={
                                    <Badge variant={selected.isActive ? "success" : "destructive"}>
                                        {selected.isActive ? "نشط" : "غير نشط"}
                                    </Badge>
                                }
                            />
                        </div>
                        <div className="flex gap-2 pt-4">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => navigate(`/files/${fileId}`)}
                            >
                                عرض الملف
                            </Button>
                            {selected.isActive && (
                                <Button
                                    variant="destructive"
                                    size="sm"
                                    onClick={async () => {
                                        const ok = await confirm({
                                            title: "هل أنت متأكد من إنهاء الوصاية؟",
                                            variant: "danger",
                                        });
                                        if (ok) {
                                            endMutation.mutate(selected.guardianID);
                                            setSelected(null);
                                        }
                                    }}
                                >
                                    إنهاء الوصاية
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            </BaseFormDialog>
            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
