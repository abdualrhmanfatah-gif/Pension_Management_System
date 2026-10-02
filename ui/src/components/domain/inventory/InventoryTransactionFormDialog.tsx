import { zodResolver } from "@hookform/resolvers/zod";
import { Calendar, Coins, FileText, Hash, Network, StickyNote } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { transactionApi } from "@/api/inventory/transactions";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const transactionSchema = z.object({
    transactionType: z.coerce.number().int().min(0, "نوع الحركة مطلوب"),
    quantity: z.coerce.number().int().min(1, "الكمية يجب أن تكون 1 على الأقل"),
    unitCost: z.coerce.number().min(0).optional().nullable(),
    currency: z.string().trim().optional().nullable().or(z.literal("")),
    withdrawalOrderNumber: z.string().trim().optional().nullable().or(z.literal("")),
    invoiceNumber: z.string().trim().optional().nullable().or(z.literal("")),
    referenceNumber: z.string().trim().optional().nullable().or(z.literal("")),
    reason: z.string().trim().optional().nullable().or(z.literal("")),
    transactionDate: z.string().min(1, "تاريخ الحركة مطلوب"),
});

type TransactionFormValues = z.infer<typeof transactionSchema>;

const typeOptions = [
    { value: "0", label: "إدخال (شراء)" },
    { value: "1", label: "إخراج (صرف)" },
    { value: "2", label: "نقل" },
    { value: "3", label: "إتلاف" },
    { value: "4", label: "تسوية جرد" },
];

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    transaction: unknown;
    itemId: number;
}

export function InventoryTransactionFormDialog({ open, onOpenChange, onSaved, itemId }: Props) {
    const mutation = useApiMutation(
        (values: TransactionFormValues) => transactionApi.create({
            ...values,
            inventoryItemID: itemId,
        }),
        {
            successMessage: "تم إضافة الحركة بنجاح",
            onSuccessCallback: () => { onSaved(); onOpenChange(false); }
        }
    );

    const form = useForm<TransactionFormValues>({
        resolver: zodResolver(transactionSchema) as Resolver<z.infer<typeof transactionSchema>>,
        defaultValues: {
            transactionType: 0, quantity: 1, unitCost: null, currency: "SAR",
            withdrawalOrderNumber: "", invoiceNumber: "",
            referenceNumber: "", reason: "",
            transactionDate: new Date().toISOString().split('T')[0]
        },
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty }, setValue, watch } = form;

    const onSubmit = (values: TransactionFormValues) => mutation.mutate(values);

    const initialValues = useMemo(() => ({
        transactionType: 0, quantity: 1, unitCost: null, currency: "SAR",
        withdrawalOrderNumber: "", invoiceNumber: "",
        referenceNumber: "", reason: "",
        transactionDate: new Date().toISOString().split('T')[0]
    }), []);

    const transactionType = watch("transactionType");

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إضافة حركة مخزون"
            description="تسجيل حركة على الصنف المحدد"
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen={false}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="تسجيل الحركة"
        >
            <div className="space-y-4">
                <FormSection title="بيانات الحركة">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="نوع الحركة" required error={errors.transactionType?.message}>
                            <NativeSelect
                                options={typeOptions}
                                placeholder="اختر النوع..."
                                value={form.watch("transactionType")}
                                onChange={(val) => setValue("transactionType", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                        <FormField label="الكمية" required error={errors.quantity?.message}>
                            <Input {...register("quantity")} type="number" min={1} />
                        </FormField>
                        <FormField label="تكلفة الوحدة" error={errors.unitCost?.message}>
                            <Input {...register("unitCost")} type="number" step="0.01" min={0} placeholder="0.00" icon={Coins} />
                        </FormField>
                        <FormField label="العملة" error={errors.currency?.message}>
                            <Input {...register("currency")} placeholder="SAR" maxLength={3} icon={Network} className="font-mono" />
                        </FormField>
                        {transactionType === 1 && (
                            <FormField label="رقم أمر الصرف" error={errors.withdrawalOrderNumber?.message}>
                                <Input {...register("withdrawalOrderNumber")} placeholder="رقم أمر الصرف" icon={FileText} className="font-mono" />
                            </FormField>
                        )}
                        <FormField label="رقم الفاتورة" error={errors.invoiceNumber?.message}>
                            <Input {...register("invoiceNumber")} placeholder="رقم فاتورة الشراء" icon={FileText} className="font-mono" />
                        </FormField>
                        <FormField label="الرقم المرجعي" error={errors.referenceNumber?.message}>
                            <Input {...register("referenceNumber")} placeholder="أمر شراء، محضر..." icon={Hash} className="font-mono" />
                        </FormField>
                        <FormField label="تاريخ الحركة" required error={errors.transactionDate?.message}>
                            <Input {...register("transactionDate")} type="date" icon={Calendar} />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="السبب" icon={StickyNote}>
                    <FormField label="السبب" error={errors.reason?.message}>
                        <Textarea {...register("reason")} placeholder="سبب الحركة..." rows={2} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
