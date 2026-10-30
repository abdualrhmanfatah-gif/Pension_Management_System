import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calculator, User } from "lucide-react";
import { useCallback, useMemo } from "react";
<<<<<<< HEAD
import { Controller,useForm } from "react-hook-form";
=======
import { useForm, Controller } from "react-hook-form";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import * as z from "zod";

import { investmentApi, type ValuationDto } from "@/api/investment";
import {
    BaseFormDialog,
<<<<<<< HEAD
    DateInput,
    FormField,
    FormSection,
    Input,
=======
    FormField,
    FormSection,
    Input,
    DateInput,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    NoteBox,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    valuationDate: z.string().min(1, "تاريخ التقييم مطلوب"),
    marketValue: z.number().positive("القيمة السوقية مطلوبة"),
    valuedBy: z.string().min(3, "اسم المُقيّم مطلوب"),
    notes: z.string().optional().or(z.literal("")),
});

export type ValuationFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface ValuationFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<ValuationDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
    propertyId: number;
}

// ─── Component ─────────────────────────────────────────

export function ValuationDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess,
    propertyId
}: ValuationFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.valuationID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["valuation", editId],
        queryFn: () => investmentApi.getValuations(propertyId).then(v => v.find(x => x.valuationID === editId)),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as ValuationDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Form ───────────────────────────────────────────
    const form = useForm<ValuationFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            valuationDate: new Date().toISOString().split("T")[0],
            marketValue: 0,
            valuedBy: "",
            notes: "",
        }), []),
    });

    const { register, reset, control, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            valuationDate: data.valuationDate?.split("T")[0] || "",
            marketValue: data.marketValue || 0,
            valuedBy: data.valuedBy || "",
            notes: "",
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: ValuationFormValues) => {
            if (mode === "create") {
                return investmentApi.createValuation({ ...values, propertyID: propertyId });
            }
            // No update API for valuations — create only
            return investmentApi.createValuation({ ...values, propertyID: propertyId });
        },
        {
            successMessage: mode === "create" ? "تم إضافة التقييم بنجاح" : "تم تحديث التقييم بنجاح",
            invalidateKeys: [
                ["property-valuations", propertyId],
                ["property", propertyId],
                ["properties"],
                ["portfolio-summary"],
                ["project-summary"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: ValuationFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إضافة تقييم عقاري جديد" : "تعديل تقييم العقار"}
            description="توثيق القيمة السوقية الحالية للأصل العقاري بناءً على تقييم فني"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إضافة التقييم" : "حفظ التعديلات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="القيمة المالية والجدولة" icon={Calculator}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="القيمة السوقية المعتمدة" error={errors.marketValue?.message} required>
                            <Input {...register("marketValue", { valueAsNumber: true })} type="number" className="font-mono font-bold text-lg text-primary" />
                        </FormField>
                        <FormField label="تاريخ التقييم" error={errors.valuationDate?.message} required>
                            <Controller
                                name="valuationDate"
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

                <FormSection title="بيانات المُقيّم والملاحظات" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="المُقيّم / جهة التقييم" error={errors.valuedBy?.message} required>
                            <Input {...register("valuedBy")} placeholder="اسم الخبير أو الشركة المثمنة" />
                        </FormField>
                        <FormField label="ملاحظات إضافية" error={errors.notes?.message}>
                            <Textarea {...register("notes")} rows={2} placeholder="تفاصيل حول حالة العقار أو معايير التقييم..." />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    يتم استخدام هذا التقييم لتحديث إجمالي قيمة المحفظة الاستثمارية واحتساب نسب النمو.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
