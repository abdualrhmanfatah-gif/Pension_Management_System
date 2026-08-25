import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Building, Calendar, DollarSign, FileText } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi, type ProjectDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    DateInput,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const projectSchema = z.object({
    portfolioID: z.number().min(1, "يجب تحديد المحفظة"),
    typeID: z.number().min(1, "يجب تحديد نوع المشروع"),
    name: z.string().trim().min(3, "اسم المشروع مطلوب"),
    description: z.string().trim().optional(),
    plannedBudget: z.number().positive("الميزانية يجب أن تكون رقماً موجباً"),
    expectedROI: z.number().min(0, "العائد المتوقع يجب أن يكون موجباً"),
    startDate: z.string().min(1, "تاريخ البدء مطلوب"),
    endDate: z.string().optional(),
});

type ProjectFormValues = z.infer<typeof projectSchema>;

// ─── Props ─────────────────────────────────────────────

interface ProjectFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<ProjectDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function ProjectDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess
}: ProjectFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.projectID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["project", editId],
        queryFn: () => investmentApi.getProject(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as ProjectDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: portfolios = [] } = useQuery({
        queryKey: ["portfolios-lookup"],
        queryFn: () => investmentApi.getPortfolios(),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<ProjectFormValues>({
        resolver: zodResolver(projectSchema),
        defaultValues: useMemo(() => ({
            portfolioID: 0,
            typeID: 1,
            name: "",
            description: "",
            plannedBudget: 0,
            expectedROI: 0,
            startDate: new Date().toISOString().split("T")[0],
            endDate: "",
        }), []),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            portfolioID: data.portfolioID || 0,
            typeID: data.typeID || 1,
            name: data.name || "",
            plannedBudget: data.plannedBudget || 0,
            expectedROI: data.expectedROI || 0,
            startDate: data.startDate?.split("T")[0] || "",
            endDate: data.endDate?.split("T")[0] || "",
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: ProjectFormValues) => {
            if (mode === "create") {
                return investmentApi.createProject(values);
            }
            return investmentApi.updateProject(effectiveData!.projectID!, {
                ...values,
                status: effectiveData?.status || "Active",
            });
        },
        {
            successMessage: mode === "create" ? "تم إنشاء المشروع بنجاح" : "تم تحديث المشروع بنجاح",
            invalidateKeys: [
                ["projects"],
                ["project", editId],
                ["project-summary"],
                ["portfolios"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: ProjectFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إنشاء مشروع جديد" : "تعديل مشروع استثماري"}
            description="إدارة المشاريع الاستثمارية ضمن المحافظ المعتمدة"
            size="3xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إنشاء المشروع" : "حفظ التغييرات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات الأساسية للمشروع" icon={Building}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="المحفظة الاستثمارية" required error={errors.portfolioID?.message}>
                            <Controller
                                name="portfolioID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={portfolios.map(p => ({ label: p.name, value: String(p.portfolioID) }))}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر المحفظة..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="نوع المشروع" required error={errors.typeID?.message}>
                            <Controller
                                name="typeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "سكني", value: "1" },
                                            { label: "تجاري", value: "2" },
                                            { label: "صناعي", value: "3" },
                                        ]}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر النوع..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="اسم المشروع" required error={errors.name?.message}>
                            <Input {...register("name")} placeholder="مثال: بناء برج النخيل السكني" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="البيانات المالية" icon={DollarSign}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الميزانية المخططة (ر.ي)" required error={errors.plannedBudget?.message}>
                            <Input type="number" {...register("plannedBudget", { valueAsNumber: true })} placeholder="0.00" className="font-mono" />
                        </FormField>
                        <FormField label="العائد المتوقع (%)" required error={errors.expectedROI?.message}>
                            <Input type="number" step="0.1" {...register("expectedROI", { valueAsNumber: true })} placeholder="0.0" className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الجدول الزمني" icon={Calendar}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="تاريخ البدء" required error={errors.startDate?.message}>
                            <Controller
                                name="startDate"
                                control={control}
                                render={({ field }) => (
                                    <DateInput
                                        id={field.name}
                                        value={field.value || undefined}
                                        onChange={field.onChange}
                                        className="font-mono"
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="تاريخ الانتهاء المتوقع" error={errors.endDate?.message}>
                            <Controller
                                name="endDate"
                                control={control}
                                render={({ field }) => (
                                    <DateInput
                                        id={field.name}
                                        value={field.value || undefined}
                                        onChange={field.onChange}
                                        className="font-mono"
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="تفاصيل إضافية" icon={FileText}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="وصف المشروع" error={errors.description?.message}>
                            <Textarea {...register("description")} placeholder="وصف موجز للمشروع وأهدافه..." rows={3} />
                        </FormField>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
