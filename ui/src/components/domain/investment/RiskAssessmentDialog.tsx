import { zodResolver } from "@hookform/resolvers/zod";
import { AlertTriangle, FileText, ListFilter, ShieldAlert } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi } from "@/api/investment";
import {
    BaseFormDialog,
<<<<<<< HEAD
    DateInput,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    NoteBox,
=======
    FormField,
    FormSection,
    Input,
    DateInput,
    NoteBox,
    NativeSelect,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    riskType: z.string().min(1, "نوع المخاطرة مطلوب"),
    riskScore: z.number().int().min(1).max(5),
    riskDescription: z.string().min(5, "الوصف مطلوب"),
    mitigationPlan: z.string().optional(),
    responsiblePerson: z.string().optional(),
    assessedDate: z.string().min(1, "تاريخ التقييم مطلوب"),
});

export type RiskFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface RiskAssessmentFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: RiskFormValues | null;
    projectId: number;
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function RiskAssessmentDialog({
    open,
    onOpenChange,
    initialData,
    projectId,
    onSuccess
}: RiskAssessmentFormDialogProps) {
    const form = useForm<RiskFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            riskType: "Financial",
            riskScore: 3,
            riskDescription: "",
            mitigationPlan: "",
            responsiblePerson: "",
            assessedDate: new Date().toISOString().split("T")[0],
        }), []),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        return initialData;
    }, [initialData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        (values: RiskFormValues) => investmentApi.createRisk({ ...values, projectID: projectId }),
        {
            successMessage: "تم تسجيل تقييم المخاطر بنجاح",
            invalidateKeys: [
                ["project-risks", projectId],
                ["investment-risks"],
                ["project-summary", projectId],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: RiskFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={initialData ? "تعديل تقييم المخاطر" : "إضافة تقييم مخاطر جديد"}
            description="تحليل المخاطر المحتملة للمشروع الاستثمارية وسبل معالجتها"
            size="3xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel={initialData ? "حفظ التعديلات" : "إضافة التقييم"}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4">
                <FormSection title="تعريف الخطر وتقييمه" icon={ShieldAlert}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        <FormField label="نوع المخاطرة" error={errors.riskType?.message} required>
                            <Controller
                                name="riskType"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        value={field.value}
                                        onChange={field.onChange}
                                        options={[
                                            { label: "مالي (Financial)", value: "Financial" },
                                            { label: "تشغيلي (Operational)", value: "Operational" },
                                            { label: "قانوني (Legal)", value: "Legal" },
                                            { label: "سوقي (Market)", value: "Market" }
                                        ]}
                                        icon={ListFilter}
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="تاريخ التقييم" error={errors.assessedDate?.message} required>
                            <Controller
                                name="assessedDate"
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
                        <FormField label="درجة الخطورة (1-5)" error={errors.riskScore?.message} required>
                            <Controller
                                name="riskScore"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        value={field.value.toString()}
                                        onChange={(v) => field.onChange(parseInt(v))}
                                        options={[
                                            { label: "1 - منخفض جداً", value: "1" },
                                            { label: "2 - منخفض", value: "2" },
                                            { label: "3 - متوسط", value: "3" },
                                            { label: "4 - مرتفع", value: "4" },
                                            { label: "5 - حرج", value: "5" }
                                        ]}
                                        icon={AlertTriangle}
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="الشخص المسؤول" error={errors.responsiblePerson?.message}>
                            <Input {...register("responsiblePerson")} placeholder="اسم مدير المخاطر أو المسؤول" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="التحليل والتحوط" icon={FileText}>
                    <div className="space-y-4 text-start">
                        <FormField label="وصف الخطر بالتفصيل" error={errors.riskDescription?.message} required>
                            <Textarea {...register("riskDescription")} rows={3} placeholder="شرح طبيعة الخطر وتأثيره المحتمل..." />
                        </FormField>
                        <FormField label="خطة التخفيف / المعالجة" error={errors.mitigationPlan?.message}>
                            <Textarea {...register("mitigationPlan")} rows={3} placeholder="الإجراءات المتخذة لتقليل أثر الخطر..." />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="warning">
                    تقييم المخاطر يساهم في حماية المحفظة الاستثمارية ويجب تحديثه عند حدوث أي متغيرات جوهرية.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
