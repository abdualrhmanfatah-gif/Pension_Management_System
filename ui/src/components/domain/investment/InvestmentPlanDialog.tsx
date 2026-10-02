import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Activity, Calendar, DollarSign } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import api from "@/api/axios";
import { investmentApi, type InvestmentPlanDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Types ─────────────────────────────────────────────

interface FiscalYear { id: number; name: string; }

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    fiscalYearID: z.number().min(1, "يجب تحديد السنة المالية"),
    plannedAmount: z.number().positive("المبلغ المخطط يجب أن يكون موجباً"),
    actualSpent: z.number().min(0, "المبلغ المنفق يجب أن يكون موجباً أو صفراً").optional(),
});

export type InvestmentPlanFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface InvestmentPlanFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: InvestmentPlanDto | null;
    mode: "create" | "edit";
    projectId: number;
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function InvestmentPlanDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    projectId,
    onSuccess
}: InvestmentPlanFormDialogProps) {
    // ─── Lookups ────────────────────────────────────────
    const { data: fiscalYears = [] } = useQuery({
        queryKey: ["lookups", "fiscal-years"],
        queryFn: () => api.get<FiscalYear[]>("/lookups/fiscal-years").then(res => res.data),
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<InvestmentPlanFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            fiscalYearID: 0,
            plannedAmount: 0,
            actualSpent: 0,
        }), []),
    });

    const { register, reset, control, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        return {
            fiscalYearID: initialData.fiscalYearID || 0,
            plannedAmount: initialData.plannedAmount || 0,
            actualSpent: initialData.actualSpent || 0,
        };
    }, [initialData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: InvestmentPlanFormValues) => {
            if (mode === "create") {
                return investmentApi.createInvestmentPlan({ ...values, projectID: projectId });
            }
            return investmentApi.updateInvestmentPlan(initialData!.planID, {
                ...values,
                planID: initialData!.planID,
                projectID: projectId,
            });
        },
        {
            successMessage: mode === "create" ? "تمت إضافة خطة الموازنة بنجاح" : "تم تحديث خطة الموازنة بنجاح",
            invalidateKeys: [["project-plans", projectId], ["project-summary", projectId]],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: InvestmentPlanFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إضافة خطة موازنة سنوية" : "تعديل خطة الموازنة"}
            description="تحديد الميزانية المخططة للسنة المالية"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel={mode === "create" ? "إضافة الخطة" : "حفظ التعديلات"}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
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
                        <FormField label="المبلغ المخطط (ر.ي)" required error={errors.plannedAmount?.message}>
                            <Input {...register("plannedAmount", { valueAsNumber: true })} type="number" className="font-mono" icon={DollarSign} />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الإنفاق الفعلي" icon={Activity}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="إجمالي المنفق فعلياً (ر.ي)" error={errors.actualSpent?.message}>
                            <Input {...register("actualSpent", { valueAsNumber: true })} type="number" className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
