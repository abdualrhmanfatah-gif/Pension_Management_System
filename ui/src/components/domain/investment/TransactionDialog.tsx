import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { CreditCard, FileText, Hash } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi } from "@/api/investment";
import {
    BaseFormDialog,
    DateInput,
    FormField,
    FormSection,
    Input,
    NativeSelect
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    txnDate: z.string().min(1, "تاريخ الحركة مطلوب"),
    txnTypeID: z.number().min(1, "يجب تحديد نوع الحركة"),
    amount: z.number().positive("المبلغ يجب أن يكون موجباً"),
    isCredit: z.boolean(),
    referenceNumber: z.string().optional().or(z.literal("")),
    sourceType: z.string().optional().or(z.literal(""))
});

type FormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface TransactionFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function TransactionDialog({
    open,
    onOpenChange,
    onSuccess
}: TransactionFormDialogProps) {
    // ─── Lookups ────────────────────────────────────────
    const { data: txnTypes = [] } = useQuery({
        queryKey: ["transaction-types-lookup"],
        queryFn: () => investmentApi.getTransactionTypes(),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            txnDate: new Date().toISOString().split("T")[0],
            txnTypeID: 0,
            amount: 0,
            isCredit: false,
            referenceNumber: "",
            sourceType: "",
        }), []),
    });

    const { register, reset, control, formState: { errors, isDirty } } = form;

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: FormValues) => {
            return investmentApi.createTransaction({
                txnTypeID: values.txnTypeID,
                txnDate: values.txnDate,
                amount: values.amount,
                isCredit: values.isCredit,
                referenceNumber: values.referenceNumber || undefined,
                sourceType: values.sourceType || undefined,
            });
        },
        {
            successMessage: "تم تسجيل الحركة المالية بنجاح",
            invalidateKeys: [
                ["transactions"],
                ["portfolio-summary"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: FormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تسجيل حركة مالية جديدة"
            description="توثيق التحصيلات أو المصروفات الاستثمارية"
            size="xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel="تسجيل الحركة"
            resetOnOpen
            reset={reset}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات الأساسية للحركة" icon={CreditCard}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="المبلغ (ر.ي)" error={errors.amount?.message} required>
                            <Input {...register("amount", { valueAsNumber: true })} type="number" step="0.01" className="font-mono font-bold text-lg text-primary" />
                        </FormField>
                        <FormField label="تاريخ الحركة" error={errors.txnDate?.message} required>
                            <Controller
                                name="txnDate"
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

                <FormSection title="التصنيف والنوع" icon={Hash}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="نوع الحركة" required error={errors.txnTypeID?.message}>
                            <Controller
                                name="txnTypeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={txnTypes.map(t => ({ label: t.name, value: String(t.id) }))}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر النوع..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="اتجاه الحركة" required>
                            <Controller
                                name="isCredit"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "إيداع / تحصيل (Credit)", value: "true" },
                                            { label: "صرف / دفع (Debit)", value: "false" }
                                        ]}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(v === "true")}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="تفاصيل إضافية" icon={FileText}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الرقم المرجعي (اختياري)" error={errors.referenceNumber?.message}>
                            <Input {...register("referenceNumber")} placeholder="رقم الشيك أو الحوالة" />
                        </FormField>
                        <FormField label="المصدر / البيان" error={errors.sourceType?.message}>
                            <Input {...register("sourceType")} placeholder="جهة الإيداع أو الصرف" />
                        </FormField>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
