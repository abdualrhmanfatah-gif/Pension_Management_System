import { zodResolver } from "@hookform/resolvers/zod";
<<<<<<< HEAD
import { Controller,useForm } from "react-hook-form";
=======
import { useForm, Controller } from "react-hook-form";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { z } from "zod";

import api from "@/api/axios";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { FormField } from "@/components/ui/FormField";
import { DateInput } from "@/components/ui/Input";
import { useApiMutation } from "@/hooks";

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    entitlementId: number | null;
}

const schema = z.object({
    endDate: z.string().min(1, "تاريخ الانتهاء مطلوب"),
});

type FormValues = z.infer<typeof schema>;

export function EndFinancialEntitlementDialog({ open, onOpenChange, onSaved, entitlementId }: Props) {
    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            endDate: "",
        },
    });

    const mutation = useApiMutation(
        (data: FormValues) => api.put(`/financial-entitlements/${entitlementId}/end`, data),
        {
            successMessage: "تم إنهاء الاستحقاق",
            onSuccess: () => {
                onSaved();
                onOpenChange(false);
            },
        }
    );

    const onSubmit = (data: FormValues) => {
        if (!entitlementId) return;
        mutation.mutate(data);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إنهاء الاستحقاق المالي"
            size="md"
            isLoading={mutation.isPending}
            onSubmit={form.handleSubmit(onSubmit)}
            submitLabel="إنهاء الاستحقاق"
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={form.formState.errors}
        >
            <div className="grid grid-cols-1 gap-4 py-2">
                <FormField label="تاريخ الانتهاء" required error={form.formState.errors.endDate?.message}>
                    <Controller
                        name="endDate"
                        control={form.control}
                        render={({ field }) => (
                            <DateInput
                                id={field.name}
                                value={field.value || undefined}
                                onChange={field.onChange}
                            />
                        )}
                    />
                </FormField>
            </div>
        </BaseFormDialog>
    );
}
