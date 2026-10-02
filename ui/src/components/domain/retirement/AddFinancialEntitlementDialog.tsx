import { zodResolver } from "@hookform/resolvers/zod";
import { Controller,useForm } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { DateInput,Input, Textarea } from "@/components/ui";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { FormField } from "@/components/ui/FormField";
import { useApiMutation } from "@/hooks";

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    retireeId: number;
}

const schema = z.object({
    amount: z.number().positive("المبلغ يجب أن يكون أكبر من صفر"),
    effectiveDate: z.string().min(1, "تاريخ السريان مطلوب"),
    notes: z.string().optional().or(z.literal("")),
});

type FormValues = z.infer<typeof schema>;

export function AddFinancialEntitlementDialog({ open, onOpenChange, onSaved, retireeId }: Props) {
    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            amount: 0,
            effectiveDate: "",
            notes: "",
        },
    });

    const mutation = useApiMutation(
        (data: FormValues) => api.post("/financial-entitlements", {
            ...data,
            retireeID: retireeId,
            notes: data.notes || null,
        }),
        {
            successMessage: "تم إنشاء الاستحقاق المالي بنجاح",
            onSuccess: () => {
                onSaved();
                onOpenChange(false);
            }
        }
    );

    const onSubmit = (data: FormValues) => {
        mutation.mutate(data);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إنشاء استحقاق مالي جديد"
            size="full"
            isLoading={mutation.isPending}
            onSubmit={form.handleSubmit(onSubmit)}
            submitLabel="إنشاء"
            resetOnOpen
            reset={form.reset}
            showGlobalError
            formErrors={form.formState.errors}
        >
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 py-2">
                <FormField label="المبلغ (ر.ي)" required error={form.formState.errors.amount?.message}>
                    <Input
                        {...form.register("amount", { valueAsNumber: true })}
                        type="number"
                        required
                        placeholder="أدخل المبلغ"
                        dir="ltr"
                    />
                </FormField>
                <FormField label="تاريخ السريان" required error={form.formState.errors.effectiveDate?.message}>
                    <Controller
                        name="effectiveDate"
                        control={form.control}
                        render={({ field }) => (
                            <DateInput
                                id={field.name}
                                value={field.value || undefined}
                                onChange={field.onChange}
                                required
                            />
                        )}
                    />
                </FormField>
                <FormField label="ملاحظات" error={form.formState.errors.notes?.message}>
                    <Textarea
                        {...form.register("notes")}
                        placeholder="ملاحظات اختيارية"
                    />
                </FormField>
                <div className="col-span-full p-3 rounded-lg bg-amber-50 dark:bg-amber-950/20 text-amber-800 dark:text-amber-200 text-sm">
                    سيتم إنهاء الاستحقاق الساري الحالي تلقائياً عند إنشاء استحقاق جديد
                </div>
            </div>
        </BaseFormDialog>
    );
}
