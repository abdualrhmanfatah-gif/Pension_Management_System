import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, type Resolver,type SubmitHandler, useForm } from "react-hook-form";
import * as z from "zod";

import { financialEntitlementsApi } from "@/api/financial-entitlements";
import { 
    BaseFormDialog, 
    FormField, 
    Input,
    DateInput,
    NativeSelect, 
    Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { type CreateEntitlementForm,createEntitlementSchema } from "@/types/financial-entitlements";

interface EntitlementFormDialogProps {
    open: boolean;
    onClose: () => void;
    onSaved: () => void;
    retireeId: number;
}

/**
 * حوار تعريف استحقاق مالي جديد لمتقاعد
 */
export function EntitlementFormDialog({ 
    open, 
    onClose, 
    onSaved, 
    retireeId 
}: EntitlementFormDialogProps) {
    const form = useForm<CreateEntitlementForm>({
        resolver: zodResolver(createEntitlementSchema) as Resolver<z.infer<typeof createEntitlementSchema>>,
        defaultValues: {
            amount: 0,
            effectiveDate: new Date().toISOString().split("T")[0],
            source: "تسوية رتبة",
            notes: "",
        }
    });

    const { register, handleSubmit, formState: { errors, isDirty } } = form;

    const createMutation = useApiMutation(
        (values: CreateEntitlementForm) => financialEntitlementsApi.create({
            ...values,
            retireeID: retireeId,
        }),
        {
            successMessage: "تم تعريف الاستحقاق المالي بنجاح",
            onSuccessCallback: () => {
                onSaved();
                onClose();
            }
        }
    );

    const onSubmit: SubmitHandler<CreateEntitlementForm> = (values) => {
        createMutation.mutate(values);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={(o) => !o && onClose()}
            title="تعريف استحقاق مالي جديد"
            size="lg"
            isLoading={createMutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            reset={form.reset}
            resetOnOpen
        >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 py-2 text-start">
                <FormField label="المبلغ الشهري (ر.ي)" required error={errors.amount?.message}>
                    <Input 
                        type="number" 
                        step="0.01"
                        {...register("amount", { valueAsNumber: true })} 
                        className="font-mono"
                    />
                </FormField>

                <FormField label="تاريخ السريان" required error={errors.effectiveDate?.message}>
                    <Controller
                        name="effectiveDate"
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

                <FormField label="المصدر" required error={errors.source?.message}>
                    <Controller
                        name="source"
                        control={form.control}
                        render={({ field }) => (
                            <NativeSelect
                                options={[
                                    { value: "تسوية رتبة", label: "تسوية رتبة" },
                                    { value: "ترقية", label: "ترقية" },
                                    { value: "قرار خاص", label: "قرار خاص" },
                                    { value: "أخرى", label: "أخرى" },
                                ]}
                                value={field.value}
                                onChange={field.onChange}
                            />
                        )}
                    />
                </FormField>

                <div className="md:col-span-2">
                    <FormField label="ملاحظات" error={errors.notes?.message}>
                        <Textarea 
                            {...register("notes")} 
                            placeholder="أي تفاصيل إضافية حول سبب أو مصدر هذا الاستحقاق..."
                            rows={3}
                        />
                    </FormField>
                </div>
            </div>
        </BaseFormDialog>
    );
}
