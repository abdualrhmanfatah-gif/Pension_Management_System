import { zodResolver } from "@hookform/resolvers/zod";
import { Target } from "lucide-react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
    Textarea,
} from "@/components/ui";
import { hrApi } from "@/api/hr";
import { useQuery } from "@tanstack/react-query";
import type { AchievementObstacleDto, AchievementObstacleType, TrackingStatus } from "@/types/achievementObstacle";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    type: z.enum(["Achievement", "Obstacle"]),
    title: z.string().trim().min(3, "العنوان يجب أن يكون 3 أحرف على الأقل"),
    description: z.string().optional(),
    status: z.enum(["New", "InProgress", "Completed", "Closed"]),
    departmentID: z.number().optional(),
    employeeID: z.number().optional(),
});

export type AchievementObstacleFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface AchievementObstacleDialogProps {
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onSubmit: (values: AchievementObstacleFormValues) => void;
    isLoading?: boolean;
    initialData?: Partial<AchievementObstacleDto>;
}

// ─── Lookup Data ───────────────────────────────────────

const typeOptions: { value: AchievementObstacleType; label: string }[] = [
    { value: "Achievement", label: "إنجاز" },
    { value: "Obstacle", label: "معوق" },
];

const statusOptions: { value: TrackingStatus; label: string }[] = [
    { value: "New", label: "جديد" },
    { value: "InProgress", label: "قيد التنفيذ" },
    { value: "Completed", label: "مكتمل" },
    { value: "Closed", label: "مغلق" },
];

export function AchievementObstacleDialog({
    isOpen,
    onOpenChange,
    onSubmit,
    isLoading,
    initialData,
}: AchievementObstacleDialogProps) {
    const isEdit = !!initialData?.id;

    // ─── Data Loading ─────────────────────────────────

    const { data: departments } = useQuery({
        queryKey: ["departments-select"],
        queryFn: () => hrApi.getDepartments(),
        staleTime: 5 * 60_000,
    });

    const { data: employeesData } = useQuery({
        queryKey: ["employees-select"],
        queryFn: () => hrApi.getEmployees({ pageSize: 100 }),
        staleTime: 5 * 60_000,
    });

    const employees = employeesData?.items || [];

    // ─── Form ─────────────────────────────────────────

    const form = useForm<AchievementObstacleFormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            type: initialData?.type || "Achievement",
            title: initialData?.title || "",
            description: initialData?.description || "",
            status: initialData?.status || "New",
            departmentID: initialData?.departmentID,
            employeeID: initialData?.employeeID,
        },
    });

    const { register, formState: { errors, isDirty }, watch, setValue } = form;

    return (
        <BaseFormDialog<AchievementObstacleFormValues>
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            title={isEdit ? "تعديل سجل الإنجاز/المعوق" : "إنشاء سجل إنجاز/معوق"}
            description={isEdit ? "تعديل بيانات السجل" : "أدخل بيانات السجل الجديد"}
            size="lg"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialData || undefined}
            showGlobalError
            formErrors={errors}
            submitLabel={isEdit ? "حفظ التعديلات" : "إنشاء السجل"}
        >
            <div className="space-y-4">
                <FormSection title="بيانات السجل" icon={Target}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="النوع" required error={errors.type?.message}>
                            <Select value={watch("type")} onValueChange={(v) => setValue("type", v as AchievementObstacleType)}>
                                <SelectTrigger><SelectValue /></SelectTrigger>
                                <SelectContent>
                                    {typeOptions.map(o => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                                </SelectContent>
                            </Select>
                        </FormField>

                        <FormField label="العنوان" required error={errors.title?.message}>
                            <Input {...register("title")} placeholder="عنوان السجل" />
                        </FormField>

                        <FormField label="الحالة" required error={errors.status?.message}>
                            <Select value={watch("status")} onValueChange={(v) => setValue("status", v as TrackingStatus)}>
                                <SelectTrigger><SelectValue /></SelectTrigger>
                                <SelectContent>
                                    {statusOptions.map(o => <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>)}
                                </SelectContent>
                            </Select>
                        </FormField>

                        <FormField label="القسم" error={errors.departmentID?.message}>
                            <Select
                                value={watch("departmentID")?.toString() || ""}
                                onValueChange={(v) => setValue("departmentID", v ? Number(v) : undefined)}
                            >
                                <SelectTrigger><SelectValue placeholder="اختر القسم" /></SelectTrigger>
                                <SelectContent>
                                    {departments?.map(d => <SelectItem key={d.departmentID} value={d.departmentID.toString()}>{d.name}</SelectItem>)}
                                </SelectContent>
                            </Select>
                        </FormField>

                        <FormField label="الموظف المسؤول" error={errors.employeeID?.message}>
                            <Select
                                value={watch("employeeID")?.toString() || ""}
                                onValueChange={(v) => setValue("employeeID", v ? Number(v) : undefined)}
                            >
                                <SelectTrigger><SelectValue placeholder="اختر الموظف" /></SelectTrigger>
                                <SelectContent>
                                    {employees.map(e => <SelectItem key={e.employeeID} value={e.employeeID.toString()}>{e.fullName}</SelectItem>)}
                                </SelectContent>
                            </Select>
                        </FormField>
                    </div>

                    <FormField label="الوصف" error={errors.description?.message}>
                        <Textarea {...register("description")} placeholder="وصف تفصيلي..." rows={3} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
