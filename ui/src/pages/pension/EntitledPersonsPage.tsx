<<<<<<< HEAD
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
=======
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    CheckCircle,
    Edit,
    Eye,
    Plus,
    StopCircle,
    Trash2,
    Users,
} from "lucide-react";
import { useRef, useState } from "react";
<<<<<<< HEAD
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import { ReactivateEntitledPersonDialog } from "@/components/domain/retirement/ReactivateEntitledPersonDialog";
import { StopEntitledPersonDialog } from "@/components/domain/retirement/StopEntitledPersonDialog";
=======
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import type { EntitledPersonSummary } from "@/types/retirement";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    FormField,
    Input,
<<<<<<< HEAD
    StatCard,
    yemenPhoneSchema,
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import type { EntitledPersonSummary } from "@/types/retirement";
=======
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { toast } from "sonner";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

const addSchema = z.object({
    fullName: z.string().min(1, "الاسم مطلوب"),
    nationalID: z.string().min(1, "الرقم الوطني مطلوب"),
    relationshipID: z.string().min(1, "صلة القرابة مطلوبة"),
    birthDate: z.string().optional(),
<<<<<<< HEAD
    phone: yemenPhoneSchema.optional().or(z.literal("")),
=======
    phone: z.string().optional(),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
});

const editSchema = z.object({
    fullName: z.string().min(1, "الاسم مطلوب"),
    nationalID: z.string().min(1, "الرقم الوطني مطلوب"),
    relationshipID: z.string().min(1, "صلة القرابة مطلوبة"),
    birthDate: z.string().optional(),
<<<<<<< HEAD
    phone: yemenPhoneSchema.optional().or(z.literal("")),
=======
    phone: z.string().optional(),
});

const stopSchema = z.object({
    stopCode: z.string().min(1, "سبب الإيقاف مطلوب"),
    stopDate: z.string().min(1, "تاريخ الإيقاف مطلوب"),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
});

type AddForm = z.infer<typeof addSchema>;
type EditForm = z.infer<typeof editSchema>;
<<<<<<< HEAD
=======
type StopForm = z.infer<typeof stopSchema>;

const STOP_CODES = [
    { value: "AGE_LIMIT", label: "ابن >= 24 سنة" },
    { value: "DAUGHTER_MARRIED", label: "بنت متزوجة" },
    { value: "WIDOW_REMARRIED", label: "زوجة تزوجت مرة أخرى" },
    { value: "DECEASED", label: "متوفى" },
    { value: "ADMIN_DECISION", label: "قرار إداري" },
    { value: "EMPLOYED", label: "توظيف" },
];
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function EntitledPersonsPage() {
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const { searchTerm, setSearchTerm, debouncedValue } = useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);

    const [fileId] = useState<number | null>(null);
    const [showAdd, setShowAdd] = useState(false);
    const [editTarget, setEditTarget] = useState<EntitledPersonSummary | null>(
        null
    );
    const [stopTarget, setStopTarget] = useState<EntitledPersonSummary | null>(
        null
    );
    const [reactivateTarget, setReactivateTarget] =
        useState<EntitledPersonSummary | null>(null);

    // Search mode: global search across all entitled persons
    const { data: searchData = [], isLoading: isSearchLoading } = useQuery({
        queryKey: ["entitled-persons-search", debouncedValue],
        queryFn: () => retirementApi.searchEntitledPersons(debouncedValue, 50),
        enabled: debouncedValue.length > 1,
    });

    // File mode: show entitled persons for a specific file
    const { data: fileData } = useQuery({
        queryKey: ["retirement-file", fileId],
        queryFn: () => retirementApi.getFile(fileId!),
        enabled: !!fileId,
    });

    const { data: entitledData = [], isLoading: isFileLoading } = useQuery({
        queryKey: queryKeys.retirement.entitledPersons.byFile(fileId!),
        queryFn: () => retirementApi.getEntitledPersons(fileId!),
        enabled: !!fileId,
    });

    const isSearchMode = debouncedValue.length > 1;
    const displayData = isSearchMode ? searchData : (fileId ? entitledData : []);
    const isLoading = isSearchMode ? isSearchLoading : isFileLoading;

    const addForm = useForm<AddForm>({
        resolver: zodResolver(addSchema),
        defaultValues: {
            fullName: "",
            nationalID: "",
            relationshipID: "",
            birthDate: "",
            phone: "",
        },
    });

    const editForm = useForm<EditForm>({
        resolver: zodResolver(editSchema),
        defaultValues: {
            fullName: "",
            nationalID: "",
            relationshipID: "",
            birthDate: "",
            phone: "",
        },
    });

<<<<<<< HEAD
=======
    const stopForm = useForm<StopForm>({
        resolver: zodResolver(stopSchema),
        defaultValues: { stopCode: "", stopDate: "" },
    });

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const addMutation = useMutation({
        mutationFn: (data: AddForm) => {
            const formData = new FormData();
            formData.append("fullName", data.fullName);
            formData.append("nationalID", data.nationalID);
            formData.append("relationshipID", data.relationshipID);
<<<<<<< HEAD
            if (data.birthDate) formData.append("dateOfBirth", data.birthDate);
=======
            if (data.birthDate) formData.append("birthDate", data.birthDate);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (data.phone) formData.append("phone", data.phone);
            return retirementApi.createEntitledPerson(fileId!, formData);
        },
        onSuccess: () => {
            toast.success("تمت الإضافة بنجاح");
            queryClient.invalidateQueries({
                queryKey: queryKeys.retirement.entitledPersons.all,
            });
            setShowAdd(false);
            addForm.reset();
        },
        onError: () => {
            toast.error("خطأ في الإضافة");
        },
    });

    const updateMutation = useMutation({
        mutationFn: (data: { id: number; formData: FormData }) =>
            retirementApi.updateEntitledPerson(data.id, data.formData),
        onSuccess: () => {
            toast.success("تم التحديث بنجاح");
            queryClient.invalidateQueries({
                queryKey: queryKeys.retirement.entitledPersons.all,
            });
            setEditTarget(null);
            editForm.reset();
        },
        onError: () => {
            toast.error("خطأ في التحديث");
        },
    });

<<<<<<< HEAD
=======
    const stopMutation = useMutation({
        mutationFn: (data: { id: number; stopCode: string; stopDate: string }) =>
            retirementApi.stopEntitledPerson(data.id, {
                stopCode: data.stopCode,
                stopDate: data.stopDate,
            }),
        onSuccess: () => {
            toast.success("تم إيقاف الاستحقاق بنجاح");
            queryClient.invalidateQueries({
                queryKey: queryKeys.retirement.entitledPersons.all,
            });
            setStopTarget(null);
            stopForm.reset();
        },
        onError: () => {
            toast.error("خطأ في إيقاف الاستحقاق");
        },
    });

    const reactivateMutation = useMutation({
        mutationFn: (data: { id: number; reactivationCode: string }) =>
            retirementApi.reactivateEntitledPerson(data.id, {
                reactivationCode: data.reactivationCode,
            }),
        onSuccess: () => {
            toast.success("تم إعادة تفعيل الاستحقاق بنجاح");
            queryClient.invalidateQueries({
                queryKey: queryKeys.retirement.entitledPersons.all,
            });
            setReactivateTarget(null);
        },
        onError: () => {
            toast.error("خطأ في إعادة التفعيل");
        },
    });

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const deleteMutation = useMutation({
        mutationFn: (id: number) =>
            retirementApi.deleteEntitledPerson(id),
        onSuccess: () => {
            toast.success("تم الحذف بنجاح");
            queryClient.invalidateQueries({
                queryKey: queryKeys.retirement.entitledPersons.all,
            });
        },
        onError: () => {
            toast.error("خطأ في الحذف");
        },
    });

    const columns = [
        {
            key: "fullName",
            header: "الاسم",
            render: (row: any) => (
                <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                        <Users className="w-4 h-4" />
                    </div>
                    <span className="font-bold text-foreground">
                        {row.fullName}
                    </span>
                </div>
            ),
        },
        {
            key: "relationshipName",
            header: "صلة القرابة",
            render: (row: any) => (
                <Badge variant="outline">{row.relationshipName || "—"}</Badge>
            ),
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row: any) => (
                <span className="text-sm font-mono">
                    {row.nationalID || "—"}
                </span>
            ),
        },
        {
            key: "phone",
            header: "الهاتف",
            render: (row: any) => (
<<<<<<< HEAD
                <span className="text-sm">{row.phone || "—"}</span>
=======
                <span className="text-sm font-mono">{row.phone || "—"}</span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            ),
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
                <Badge
                    variant={
                        row.isDeceased
                            ? "danger"
                            : row.isActive
                                ? "success"
                                : "warning"
                    }
                >
                    {row.isDeceased
                        ? "متوفى"
                        : row.isActive
                            ? "نشط"
                            : "موقوف"}
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
                        <>
                            <Button
                                variant="ghost-action"
                                size="sm"
                                title="تعديل"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setEditTarget(row);
                                    editForm.reset({
                                        fullName: row.fullName,
                                        nationalID: row.nationalID || "",
                                        relationshipID: row.relationshipName || "",
                                        birthDate: row.dateOfBirth || "",
                                        phone: row.phone || "",
                                    });
                                }}
                            >
                                <Edit className="w-4 h-4" />
                            </Button>
                            {row.isActive && !row.isDeceased && (
                                <Button
                                    variant="ghost-action"
                                    size="sm"
                                    title="إيقاف الاستحقاق"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setStopTarget(row);
                                    }}
                                >
                                    <StopCircle className="w-4 h-4" />
                                </Button>
                            )}
                            {!row.isActive && !row.isDeceased && (
                                <Button
                                    variant="ghost-action"
                                    size="sm"
                                    title="إعادة التفعيل"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setReactivateTarget(row);
                                    }}
                                >
                                    <CheckCircle className="w-4 h-4" />
                                </Button>
                            )}
                            <Button
                                variant="ghost-action"
                                size="sm"
                                title="حذف"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    if (
                                        confirm(
                                            `هل أنت متأكد من حذف ${row.fullName}؟`
                                        )
                                    ) {
                                        deleteMutation.mutate(row.entitledID);
                                    }
                                }}
                            >
                                <Trash2 className="w-4 h-4 text-destructive" />
                            </Button>
                        </>
                    )}
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="إدارة المستحقين"
            description="عرض وإدارة جميع المستحقين (الورثة) عبر ملفات التقاعد"
            icon={<Users className="w-6 h-6" />}
            columns={columns}
            data={displayData}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="ابحث بالاسم أو الرقم الوطني أو رقم الملف أو الرقم العسكري..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row: any) => row.entitledID}
            pagination={{
                page: 1,
                pageSize: 50,
                total: displayData.length,
                onPageChange: () => { },
            }}
            emptyMessage="ابحث عن مستحق بالاسم أو الرقم الوطني أو رقم الملف"
            headerActions={
                fileId && !isSearchMode ? (
                    <Button
                        variant="primary"
                        size="sm"
                        onClick={() => setShowAdd(true)}
                    >
                        <Plus className="w-4 h-4 mr-1" />
                        إضافة مستحق
                    </Button>
                ) : undefined
            }
        >
            {/* Stats */}
            {isSearchMode && (
                <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 sm:gap-4 mb-6">
                    <StatCard
                        title="إجمالي النتائج"
                        value={searchData.length}
                        icon={<Users className="w-5 h-5" />}
                        variant="blue"
                        loading={isSearchLoading}
                    />
                    <StatCard
                        title="النشطون"
                        value={searchData.filter((s) => s.isActive && !s.isDeceased).length}
                        icon={<CheckCircle className="w-5 h-5" />}
                        variant="success"
                        loading={isSearchLoading}
                    />
                    <StatCard
                        title="الموقوفون/المتوفون"
                        value={searchData.filter((s) => !s.isActive || s.isDeceased).length}
                        icon={<Trash2 className="w-5 h-5" />}
                        variant="destructive"
                        loading={isSearchLoading}
                    />
                </div>
            )}

            {!isSearchMode && fileId && fileData && (
                <div className="bg-muted/30 rounded-lg p-4 mb-6 border border-border">
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm">
                        <div>
                            <span className="text-muted-foreground">
                                المتقاعد:
                            </span>{" "}
                            <strong>{fileData.retireeFullName || "—"}</strong>
                        </div>
                        <div>
                            <span className="text-muted-foreground">
                                رقم الملف:
                            </span>{" "}
                            <strong>{fileData.fileNumber}</strong>
                        </div>
                        <div>
                            <span className="text-muted-foreground">
                                الرقم العسكري:
                            </span>{" "}
                            <strong>
                                {fileData.militaryNumber || "—"}
                            </strong>
                        </div>
                        <div>
                            <span className="text-muted-foreground">
                                عدد المستحقين:
                            </span>{" "}
                            <strong>{entitledData.length}</strong>
                        </div>
                    </div>
                </div>
            )}

            <BaseFormDialog
                title="إضافة مستحق جديد"
                isOpen={showAdd}
                onClose={() => {
                    setShowAdd(false);
                    addForm.reset();
                }}
                onSubmit={addForm.handleSubmit((data) =>
                    addMutation.mutate(data)
                )}
                isLoading={addMutation.isPending}
                size="md"
            >
<<<<<<< HEAD
                <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
                    <FormField
                        label="الاسم الكامل *"
                        error={addForm.formState.errors.fullName?.message}
                        className="sm:col-span-2"
=======
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField
                        label="الاسم الكامل *"
                        error={addForm.formState.errors.fullName?.message}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    >
                        <Input
                            {...addForm.register("fullName")}
                            placeholder="الاسم"
                        />
                    </FormField>
<<<<<<< HEAD
                    <FormField label="تاريخ الميلاد" className="sm:col-span-2">
                        <Input
                            {...addForm.register("birthDate")}
                            type="date"
                        />
                    </FormField>
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <FormField
                        label="الرقم الوطني *"
                        error={addForm.formState.errors.nationalID?.message}
                    >
                        <Input
                            {...addForm.register("nationalID")}
                            placeholder="الرقم الوطني"
                        />
                    </FormField>
<<<<<<< HEAD
                    <FormField label="الهاتف">
                        <Input
                            {...addForm.register("phone")}
                            placeholder="7xxxxxxx"
                        />
                    </FormField>
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <FormField
                        label="صلة القرابة *"
                        error={
                            addForm.formState.errors.relationshipID?.message
                        }
                    >
                        <Input
                            {...addForm.register("relationshipID")}
                            placeholder="رقم العلاقة"
                        />
                    </FormField>
<<<<<<< HEAD
=======
                    <FormField label="تاريخ الميلاد">
                        <Input
                            {...addForm.register("birthDate")}
                            type="date"
                        />
                    </FormField>
                    <div className="sm:col-span-2">
                        <FormField label="الهاتف">
                            <Input
                                {...addForm.register("phone")}
                                placeholder="رقم الهاتف"
                            />
                        </FormField>
                    </div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                </div>
            </BaseFormDialog>

            <BaseFormDialog
                title="تعديل بيانات المستحق"
                description={editTarget?.fullName || ""}
                isOpen={!!editTarget}
                onClose={() => {
                    setEditTarget(null);
                    editForm.reset();
                }}
                onSubmit={editForm.handleSubmit((data) => {
                    if (editTarget) {
                        const formData = new FormData();
                        formData.append("fullName", data.fullName);
                        formData.append("nationalID", data.nationalID);
                        formData.append("relationshipID", data.relationshipID);
<<<<<<< HEAD
                        if (data.birthDate) formData.append("dateOfBirth", data.birthDate);
=======
                        if (data.birthDate) formData.append("birthDate", data.birthDate);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        if (data.phone) formData.append("phone", data.phone);
                        updateMutation.mutate({ id: editTarget.entitledID, formData });
                    }
                })}
                isLoading={updateMutation.isPending}
                size="md"
            >
<<<<<<< HEAD
                <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
                    <FormField
                        label="الاسم الكامل *"
                        error={editForm.formState.errors.fullName?.message}
                        className="sm:col-span-2"
=======
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField
                        label="الاسم الكامل *"
                        error={editForm.formState.errors.fullName?.message}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    >
                        <Input
                            {...editForm.register("fullName")}
                            placeholder="الاسم"
                        />
                    </FormField>
<<<<<<< HEAD
                    <FormField label="تاريخ الميلاد" className="sm:col-span-2">
                        <Input
                            {...editForm.register("birthDate")}
                            type="date"
                        />
                    </FormField>
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <FormField
                        label="الرقم الوطني *"
                        error={editForm.formState.errors.nationalID?.message}
                    >
                        <Input
                            {...editForm.register("nationalID")}
                            placeholder="الرقم الوطني"
                        />
                    </FormField>
<<<<<<< HEAD
                    <FormField label="الهاتف">
                        <Input
                            {...editForm.register("phone")}
                            placeholder="7xxxxxxx"
                        />
                    </FormField>
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <FormField
                        label="صلة القرابة *"
                        error={editForm.formState.errors.relationshipID?.message}
                    >
                        <Input
                            {...editForm.register("relationshipID")}
                            placeholder="رقم العلاقة"
                        />
                    </FormField>
<<<<<<< HEAD
                </div>
            </BaseFormDialog>

            <StopEntitledPersonDialog
                open={!!stopTarget}
                onOpenChange={(open) => { if (!open) setStopTarget(null); }}
                personId={stopTarget?.entitledID ?? 0}
                targetName={stopTarget?.fullName ?? ""}
                fileId={stopTarget?.fileID ?? fileId ?? 0}
                onSuccess={() => { void queryClient.invalidateQueries({ queryKey: ["entitled-persons-search"] }); }}
            />
            <ReactivateEntitledPersonDialog
                open={!!reactivateTarget}
                onOpenChange={(open) => { if (!open) setReactivateTarget(null); }}
                personId={reactivateTarget?.entitledID ?? 0}
                targetName={reactivateTarget?.fullName ?? ""}
                fileId={reactivateTarget?.fileID ?? fileId ?? 0}
                onSuccess={() => { void queryClient.invalidateQueries({ queryKey: ["entitled-persons-search"] }); }}
            />
=======
                    <FormField label="تاريخ الميلاد">
                        <Input
                            {...editForm.register("birthDate")}
                            type="date"
                        />
                    </FormField>
                    <div className="sm:col-span-2">
                        <FormField label="الهاتف">
                            <Input
                                {...editForm.register("phone")}
                                placeholder="رقم الهاتف"
                            />
                        </FormField>
                    </div>
                </div>
            </BaseFormDialog>

            <BaseFormDialog
                title="إيقاف الاستحقاق"
                description={stopTarget?.fullName || ""}
                isOpen={!!stopTarget}
                onClose={() => {
                    setStopTarget(null);
                    stopForm.reset();
                }}
                onSubmit={stopForm.handleSubmit((data) => {
                    if (stopTarget) {
                        stopMutation.mutate({
                            id: stopTarget.entitledID,
                            stopCode: data.stopCode,
                            stopDate: data.stopDate,
                        });
                    }
                })}
                size="md"
            >
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField
                        label="سبب الإيقاف *"
                        error={stopForm.formState.errors.stopCode?.message}
                    >
                        <Select
                            value={stopForm.watch("stopCode") ?? ""}
                            onValueChange={(value) => stopForm.setValue("stopCode", value)}
                        >
                            <SelectTrigger className="w-full">
                                <SelectValue placeholder="اختر السبب" />
                            </SelectTrigger>
                            <SelectContent>
                                {STOP_CODES.map((c) => (
                                    <SelectItem key={c.value} value={c.value}>
                                        {c.label}
                                    </SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </FormField>
                    <FormField
                        label="تاريخ الإيقاف *"
                        error={stopForm.formState.errors.stopDate?.message}
                    >
                        <Input
                            {...stopForm.register("stopDate")}
                            type="date"
                        />
                    </FormField>
                </div>
            </BaseFormDialog>

            <BaseFormDialog
                title="إعادة التفعيل"
                description={reactivateTarget?.fullName || ""}
                isOpen={!!reactivateTarget}
                onClose={() => setReactivateTarget(null)}
                onSubmit={() => {
                    if (reactivateTarget) {
                        reactivateMutation.mutate({
                            id: reactivateTarget.entitledID,
                            reactivationCode: "ADMIN_DECISION",
                        });
                    }
                }}
                size="md"
            >
                <p className="text-sm text-muted-foreground">
                    هل أنت متأكد من إعادة تفعيل استحقاق{" "}
                    <strong>{reactivateTarget?.fullName}</strong>؟
                </p>
            </BaseFormDialog>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        </CrudPageLayout>
    );
}
