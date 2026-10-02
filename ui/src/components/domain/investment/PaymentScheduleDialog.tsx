import { zodResolver } from "@hookform/resolvers/zod";
import { Activity, Calculator } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import type { PaymentScheduleDto, UpdatePaymentStatusDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import {
    BaseFormDialog,
    DateInput,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    NoteBox} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    dueDate: z.string().min(1, "تاريخ الاستحقاق مطلوب"),
    expectedAmount: z.number().positive("المبلغ يجب أن يكون موجباً"),
    paidAmount: z.number().min(0),
    status: z.string().min(1, "الحالة مطلوبة")
});

export type PaymentScheduleFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface PaymentScheduleFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: PaymentScheduleDto | null;
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function PaymentScheduleDialog({
    open,
    onOpenChange,
    initialData,
    onSuccess
}: PaymentScheduleFormDialogProps) {
    const form = useForm<PaymentScheduleFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            dueDate: "",
            expectedAmount: 0,
            paidAmount: 0,
            status: "Pending",
        }), []),
    });

    const { register, reset, control, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        return {
            dueDate: initialData.dueDate?.split("T")[0] || "",
            expectedAmount: initialData.expectedAmount || 0,
            paidAmount: initialData.paidAmount || 0,
            status: initialData.status || "Pending",
        };
    }, [initialData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        (values: PaymentScheduleFormValues) =>
            investmentApi.updatePaymentStatus(initialData!.scheduleID, {
                paidAmount: values.paidAmount,
                status: values.status,
            } as UpdatePaymentStatusDto),
        {
            successMessage: "تم تحديث حالة الدفعة بنجاح",
            invalidateKeys: [["schedules"], ["schedules-all"], ["portfolio-summary"], ["project-summary"]],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: PaymentScheduleFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={initialData ? "تحديث حالة دفعة" : "إضافة دفعة مجدولة"}
            description="تعديل المبالغ وحالة التحصيل للدفعة"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel={initialData ? "حفظ التغييرات" : "إضافة الدفعة"}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات المالية للدفعة" icon={Calculator}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="المبلغ المتوقع (ر.ي)" error={errors.expectedAmount?.message} required>
                            <Input {...register("expectedAmount", { valueAsNumber: true })} type="number" className="font-mono font-bold" />
                        </FormField>
                        <FormField label="المبلغ المدفوع فعلياً" error={errors.paidAmount?.message}>
                            <Input {...register("paidAmount", { valueAsNumber: true })} type="number" className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الجدولة والحالة" icon={Activity}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="تاريخ الاستحقاق" error={errors.dueDate?.message} required>
                            <Controller
                                name="dueDate"
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
                        <FormField label="حالة الدفعة" error={errors.status?.message} required>
                            <Controller
                                name="status"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        value={field.value}
                                        onChange={field.onChange}
                                        options={[
                                            { label: "قيد الانتظار (Pending)", value: "Pending" },
                                            { label: "مدفوعة جزئياً (Partial)", value: "Partial" },
                                            { label: "مدفوعة بالكامل (Paid)", value: "Paid" },
                                            { label: "متأخرة (Overdue)", value: "Overdue" }
                                        ]}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    تحديث حالة الدفعة يؤثر مباشرة على تقارير التحصيل والتدفقات النقدية للمشروع.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
