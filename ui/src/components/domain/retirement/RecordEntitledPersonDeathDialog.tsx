import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import { BaseFormDialog, DateInput, FileField, FormField } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { todayYemen } from "@/lib/date";

const schema = z.object({
    deathDate: z.string().min(1, "تاريخ الوفاة مطلوب"),
    deathCertificate: z.instanceof(File, { message: "شهادة الوفاة مطلوبة" }),
});
type Values = z.infer<typeof schema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    personId: number;
    personName: string;
    fileId: number;
}

export function RecordEntitledPersonDeathDialog({ open, onOpenChange, personId, personName, fileId }: Props) {
    const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { deathDate: todayYemen() } });
    const mutation = useApiMutation((values: Values) => retirementApi.recordEntitledPersonDeath(personId, values), {
        successMessage: "تم تسجيل الوفاة بنجاح",
        invalidateKeys: [["entitled-persons", fileId], ["retirement-file", fileId], ["person-documents", fileId], ["entitled-persons-search"]],
        onSuccess: () => { onOpenChange(false); form.reset(); },
    });

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تسجيل وفاة المستحق"
            description={personName}
            onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
            isLoading={mutation.isPending}
            isDirty={form.formState.isDirty}
            resetOnOpen
            reset={form.reset}
            showGlobalError
            formErrors={form.formState.errors}
            submitLabel="تأكيد وتسجيل الوفاة"
            size="md"
        >
            <div className="space-y-4">
                <FormField label="تاريخ الوفاة" required error={form.formState.errors.deathDate?.message}>
                    <Controller name="deathDate" control={form.control} render={({ field }) => (
                        <DateInput value={field.value} onChange={field.onChange} max={todayYemen()} />
                    )} />
                </FormField>
                <FileField name="deathCertificate" control={form.control} label="شهادة الوفاة" required />
            </div>
        </BaseFormDialog>
    );
}
