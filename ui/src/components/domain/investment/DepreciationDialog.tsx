import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calculator, Calendar, TrendingDown } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import api from "@/api/axios";
import { type DepreciationRecordDto,investmentApi } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    NoteBox} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    fiscalYearID: z.number().min(1, "يجب تحديد السنة المالية"),
    depreciationRate: z.number().min(0).max(100, "نسبة الاستهلاك يجب أن تكون بين 0 و 100"),
    depreciationAmount: z.number().positive("مبلغ الاستهلاك يجب أن يكون موجباً"),
});

export type DepreciationFormValues = z.infer<typeof schema>;

// ─── Types ─────────────────────────────────────────────

interface FiscalYear { id: number; name: string; }

// ─── Props ─────────────────────────────────────────────

interface DepreciationFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<DepreciationRecordDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
    propertyId: number;
}

// ─── Component ─────────────────────────────────────────

export function DepreciationDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess,
    propertyId
}: DepreciationFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.deprecID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["depreciation-record", editId],
        queryFn: () => investmentApi.getDepreciation(propertyId).then(d => d.find(x => x.deprecID === editId)),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as DepreciationRecordDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: fiscalYears = [] } = useQuery({
        queryKey: ["lookups", "fiscal-years"],
        queryFn: () => api.get<FiscalYear[]>("/lookups/fiscal-years").then(res => res.data),
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<DepreciationFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            fiscalYearID: 0,
            depreciationRate: 0,
            depreciationAmount: 0,
        }), []),
    });

    const { register, reset, control, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            fiscalYearID: data.fiscalYearID || 0,
            depreciationRate: data.depreciationRate || 0,
            depreciationAmount: data.depreciationAmount || 0,
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: DepreciationFormValues) => {
            if (mode === "create") {
                return investmentApi.createDepreciation({ ...values, propertyID: propertyId });
            }
            return investmentApi.createDepreciation({ ...values, propertyID: propertyId });
        },
        {
            successMessage: mode === "create" ? "تم إضافة سجل الاستهلاك بنجاح" : "تم تحديث سجل الاستهلاك",
            invalidateKeys: [
                ["property-depreciation", propertyId],
                ["property", propertyId],
                ["portfolio-summary"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: DepreciationFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "تسجيل استهلاك أصل" : "تعديل سجل استهلاك"}
            description="إثبات الانخفاض الدوري في قيمة الأصول العقارية للسنة المالية"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "تسجيل الاستهلاك" : "حفظ التعديلات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات المالية والزمنية" icon={Calendar}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="السنة المالية" required error={errors.fiscalYearID?.message}>
                            <Controller
                                name="fiscalYearID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={fiscalYears.map(y => ({ label: y.name, value: String(y.id) }))}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر السنة..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="نسبة الاستهلاك (%)" required error={errors.depreciationRate?.message}>
                            <Input {...register("depreciationRate", { valueAsNumber: true })} type="number" step="0.1" icon={TrendingDown} className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="تفاصيل القيمة" icon={Calculator}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="مبلغ الاستهلاك المحتسب (ر.ي)" error={errors.depreciationAmount?.message} required>
                            <Input {...register("depreciationAmount", { valueAsNumber: true })} type="number" className="font-mono font-bold text-destructive" />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="warning">
                    يؤثر تسجيل الاستهلاك مباشرة على صافي القيمة الدفترية للأصل في التقارير المالية.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
