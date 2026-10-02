import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeftFromLine, Calendar, StickyNote } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { custodyApi, type CustodyDto } from "@/api/inventory/custody";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const returnSchema = z.object({
    returnedDate: z.string().min(1, "تاريخ الإرجاع مطلوب"),
    status: z.coerce.number().int().min(1, "حالة الإرجاع مطلوبة"),
    notes: z.string().trim().optional().nullable().or(z.literal("")),
});

type ReturnFormValues = z.infer<typeof returnSchema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    custody: CustodyDto | null;
}

const statusOptions = [
    { value: "1", label: "تم الإرجاع" },
    { value: "2", label: "تالف" },
    { value: "3", label: "مفقود" },
];

export function InventoryCustodyReturnDialog({ open, onOpenChange, onSaved, custody }: Props) {
    const mutation = useApiMutation(
        (values: ReturnFormValues) => custodyApi.return(custody!.inventoryCustodyID, values),
        {
            successMessage: "تم إرجاع العهدة بنجاح",
            onSuccessCallback: () => { onSaved(); onOpenChange(false); }
        }
    );

    const form = useForm<ReturnFormValues>({
        resolver: zodResolver(returnSchema) as Resolver<z.infer<typeof returnSchema>>,
        defaultValues: { returnedDate: new Date().toISOString().split('T')[0], status: 1, notes: "" },
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty }, setValue } = form;

    const onSubmit = (values: ReturnFormValues) => mutation.mutate(values);

    const initialValues = useMemo(() => ({
        returnedDate: new Date().toISOString().split('T')[0],
        status: 1,
        notes: ""
    }), []);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إرجاع العهدة"
            description={custody ? `إرجاع: ${custody.assetName} - ${custody.employeeName}` : ""}
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="إرجاع العهدة"
        >
            <div className="space-y-4">
                <FormSection title="بيانات الإرجاع" icon={ArrowLeftFromLine}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="تاريخ الإرجاع" required error={errors.returnedDate?.message}>
                            <Input {...register("returnedDate")} type="date" icon={Calendar} />
                        </FormField>
                        <FormField label="حالة الإرجاع" required error={errors.status?.message}>
                            <NativeSelect
                                options={statusOptions}
                                placeholder="اختر حالة الإرجاع..."
                                value={form.watch("status")}
                                onChange={(val) => setValue("status", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="ملاحظات" icon={StickyNote}>
                    <FormField label="ملاحظات" error={errors.notes?.message}>
                        <Textarea {...register("notes")} placeholder="ملاحظات..." rows={3} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
