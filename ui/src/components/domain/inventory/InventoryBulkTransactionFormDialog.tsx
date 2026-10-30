import { zodResolver } from "@hookform/resolvers/zod";
<<<<<<< HEAD
import { Calendar, Coins, FileText, Hash, Network, Package,Plus, StickyNote, Trash2 } from "lucide-react";
=======
import { Calendar, Coins, FileText, Hash, Network, Plus, StickyNote, Trash2, Package } from "lucide-react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useMemo } from "react";
import { type Resolver, useFieldArray, useForm } from "react-hook-form";
import * as z from "zod";

import type { ItemDto } from "@/api/inventory/items";
import { transactionApi, type TransactionDto } from "@/api/inventory/transactions";
import {
    BaseFormDialog,
    Button,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Text,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const bulkSchema = z.object({
    transactionType: z.coerce.number().int().min(0, "نوع الحركة مطلوب"),
    currency: z.string().trim().optional().nullable().or(z.literal("")),
    withdrawalOrderNumber: z.string().trim().optional().nullable().or(z.literal("")),
    invoiceNumber: z.string().trim().optional().nullable().or(z.literal("")),
    referenceNumber: z.string().trim().optional().nullable().or(z.literal("")),
    reason: z.string().trim().optional().nullable().or(z.literal("")),
    transactionDate: z.string().min(1, "تاريخ الحركة مطلوب"),
    items: z.array(z.object({
        inventoryItemID: z.coerce.number().int().positive("اختر صنفاً"),
        quantity: z.coerce.number().int().min(1, "الكمية يجب أن تكون 1 على الأقل"),
        unitCost: z.coerce.number().min(0).optional().nullable(),
    })).min(1, "يجب إضافة صنف واحد على الأقل"),
});

type BulkFormValues = z.infer<typeof bulkSchema>;

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
    items: ItemDto[];
    /** Preset the transaction type (e.g. 1 = StockOut). When set, the field is hidden. */
    presetType?: number;
    /** Called after successful save with the response data items */
    onSuccessWithResult?: (data: TransactionDto[]) => void;
}

export function InventoryBulkTransactionFormDialog({ open, onOpenChange, onSaved, items, presetType, onSuccessWithResult }: Props) {

    const mutation = useApiMutation(
        (values: BulkFormValues) => transactionApi.createBulk({
            transactionType: Number(values.transactionType),
            currency: values.currency || undefined,
            withdrawalOrderNumber: values.withdrawalOrderNumber || undefined,
            invoiceNumber: values.invoiceNumber || undefined,
            referenceNumber: values.referenceNumber || undefined,
            reason: values.reason || undefined,
            transactionDate: values.transactionDate,
            items: values.items.map(i => ({ inventoryItemID: Number(i.inventoryItemID), quantity: Number(i.quantity), unitCost: i.unitCost ?? undefined })),
        }),
        {
            successMessage: "تم إضافة الحركات بنجاح",
            onSuccessCallback: (data: any) => {
                onSaved();
                onOpenChange(false);
                const responseData = data?.data as TransactionDto[] | undefined;
                if (responseData && responseData.length > 0 && onSuccessWithResult) {
                    onSuccessWithResult(responseData);
                }
            }
        }
    );

    const form = useForm<BulkFormValues>({
        resolver: zodResolver(bulkSchema) as Resolver<z.infer<typeof bulkSchema>>,
        defaultValues: {
            transactionType: presetType ?? 0, currency: "SAR",
            withdrawalOrderNumber: "", invoiceNumber: "",
            referenceNumber: "", reason: "",
            transactionDate: new Date().toISOString().split('T')[0],
            items: [{ inventoryItemID: 0, quantity: 1, unitCost: null }],
        },
    });

    const { register, handleSubmit, reset, control, formState: { errors, isDirty }, setValue, watch } = form;
    const { fields, append, remove } = useFieldArray({ control, name: "items" });
    const itemOptions = items.map(i => ({ value: String(i.inventoryItemID), label: `${i.itemName} (${i.itemCode}) - ${i.quantity}` }));

    const onSubmit = (values: BulkFormValues) => mutation.mutate(values);

    const initialValues = useMemo(() => ({
        transactionType: presetType ?? 0, currency: "SAR",
        withdrawalOrderNumber: "", invoiceNumber: "",
        referenceNumber: "", reason: "",
        transactionDate: new Date().toISOString().split('T')[0],
        items: [{ inventoryItemID: 0, quantity: 1, unitCost: null }],
    }), [presetType]);

    const currentType = Number(watch("transactionType"));
    const isStockIn = currentType === 0;
    const isStockOut = currentType === 1;

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={
                isStockIn ? "إضافة حركة شراء متعددة" :
                isStockOut ? "إضافة حركة صرف متعددة" :
                "إضافة حركة مخزون متعددة"
            }
            description="تسجيل حركة مخزون لأكثر من صنف في وقت واحد"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={presetType === 1 ? "إصدار أمر الصرف" : "تسجيل الحركات"}
        >
            <div className="space-y-4">
                <FormSection title="بيانات الحركة">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {presetType == null && (
                            <FormField label="نوع الحركة" required error={errors.transactionType?.message}>
                                <NativeSelect
                                    options={typeOptions}
                                    value={currentType}
                                    onChange={(val) => setValue("transactionType", Number(val), { shouldValidate: true })}
                                />
                            </FormField>
                        )}
                        <FormField label="تاريخ الحركة" required error={errors.transactionDate?.message}>
                            <Input {...register("transactionDate")} type="date" icon={Calendar} />
                        </FormField>
                        {isStockIn && (
                            <>
                                <FormField label="رقم فاتورة الشراء" error={errors.invoiceNumber?.message}>
                                    <Input {...register("invoiceNumber")} placeholder="رقم فاتورة الشراء" icon={FileText} className="font-mono" />
                                </FormField>
                                <FormField label="العملة" error={errors.currency?.message}>
                                    <Input {...register("currency")} placeholder="SAR" maxLength={3} icon={Network} className="font-mono" />
                                </FormField>
                            </>
                        )}
                        {isStockOut && (
                            <>
                                <FormField label="رقم أمر الصرف" error={errors.withdrawalOrderNumber?.message}>
                                    <Input {...register("withdrawalOrderNumber")} placeholder="رقم أمر الصرف" icon={FileText} className="font-mono" />
                                </FormField>
                                <FormField label="الرقم المرجعي" error={errors.referenceNumber?.message}>
                                    <Input {...register("referenceNumber")} placeholder="أمر صرف، إذن..." icon={Hash} className="font-mono" />
                                </FormField>
                            </>
                        )}
                    </div>
                </FormSection>

                <FormSection
                    title={isStockIn ? "الأصناف المشتراة" : "الأصناف"}
                    icon={Package}
                >
                    {fields.map((field, index) => (
                        <div key={field.id} className="flex items-start gap-2 p-3 border rounded-lg">
                            <div className={`flex-1 grid grid-cols-1 ${isStockIn ? 'md:grid-cols-3' : 'md:grid-cols-2'} gap-3`}>
                                <FormField label="الصنف" required error={errors.items?.[index]?.inventoryItemID?.message}>
                                    <NativeSelect
                                        options={itemOptions}
                                        placeholder="اختر صنف..."
                                        value={watch(`items.${index}.inventoryItemID`)}
                                        onChange={(val) => setValue(`items.${index}.inventoryItemID`, Number(val), { shouldValidate: true })}
                                    />
                                </FormField>
                                <FormField label="الكمية" required error={errors.items?.[index]?.quantity?.message}>
                                    <Input {...register(`items.${index}.quantity`)} type="number" min={1} />
                                </FormField>
                                {isStockIn && (
                                    <FormField label="تكلفة الوحدة" error={errors.items?.[index]?.unitCost?.message}>
                                        <Input {...register(`items.${index}.unitCost`)} type="number" step="0.01" min={0} placeholder="0.00" icon={Coins} />
                                    </FormField>
                                )}
                            </div>
                            {fields.length > 1 && (
                                <Button variant="ghost" size="icon" onClick={() => remove(index)}
<<<<<<< HEAD
                                    className="mt-6 text-destructive hover:text-destructive min-h-[44px] min-w-[44px]">
=======
                                    className="mt-6 text-destructive hover:text-destructive/80 min-h-[44px] min-w-[44px]">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    <Trash2 className="w-4 h-4" />
                                </Button>
                            )}
                        </div>
                    ))}
                    <Button variant="outline" size="sm" onClick={() => append({ inventoryItemID: 0, quantity: 1, unitCost: null })} className="gap-1.5">
                        <Plus className="w-4 h-4" /> إضافة صنف آخر
                    </Button>
                    {errors.items?.root && (
                        <Text variant="body" className="text-destructive mt-1">{errors.items.root.message}</Text>
                    )}
                </FormSection>

                {currentType >= 2 && (
                    <FormSection title="المستندات" icon={FileText}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <FormField label="رقم الفاتورة" error={errors.invoiceNumber?.message}>
                                <Input {...register("invoiceNumber")} placeholder="رقم المستند" icon={FileText} className="font-mono" />
                            </FormField>
                            <FormField label="الرقم المرجعي" error={errors.referenceNumber?.message}>
                                <Input {...register("referenceNumber")} placeholder="رقم مرجعي" icon={Hash} className="font-mono" />
                            </FormField>
                            <FormField label="العملة" error={errors.currency?.message}>
                                <Input {...register("currency")} placeholder="SAR" maxLength={3} icon={Network} className="font-mono" />
                            </FormField>
                        </div>
                    </FormSection>
                )}

                <FormSection title="سبب الحركة" icon={StickyNote}>
                    <FormField label="السبب" error={errors.reason?.message}>
                        <Textarea {...register("reason")} placeholder={
                            isStockIn ? "سبب الإدخال / فاتورة شراء رقم..." :
                            isStockOut ? "سبب الإخراج / أمر صرف رقم..." :
                            "سبب الحركة..."
                        } rows={2} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
