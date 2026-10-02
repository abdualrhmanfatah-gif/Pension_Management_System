import { zodResolver } from "@hookform/resolvers/zod";
import { Barcode, Hash, MapPin, Package, StickyNote, Wrench } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { itemApi, type ItemDto } from "@/api/inventory/items";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const itemSchema = z.object({
    itemName: z.string().trim().min(2, "اسم الصنف مطلوب"),
    itemCode: z.string().trim().min(1, "كود الصنف مطلوب"),
    description: z.string().trim().optional().nullable().or(z.literal("")),
    unit: z.string().trim().min(1, "وحدة القياس مطلوبة"),
    quantity: z.coerce.number().int().min(0, "الكمية لا يمكن أن تكون سالبة").default(0),
    minimumQuantity: z.coerce.number().int().min(0).default(0),
    condition: z.coerce.number().int().min(0).default(0),
    location: z.string().trim().optional().nullable().or(z.literal("")),
    serialNumber: z.string().trim().optional().nullable().or(z.literal("")),
    notes: z.string().trim().optional().nullable().or(z.literal("")),
});

type ItemFormValues = z.infer<typeof itemSchema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    item: ItemDto | null;
}

export function InventoryItemFormDialog({ open, onOpenChange, onSaved, item }: Props) {
    const isEdit = !!item;

    const mutation = useApiMutation(
        (values: ItemFormValues) => isEdit
            ? itemApi.update(item!.inventoryItemID, values)
            : itemApi.create(values),
        {
            successMessage: isEdit ? "تم تحديث الصنف بنجاح" : "تم إضافة الصنف الجديد بنجاح",
            onSuccessCallback: () => { onSaved(); onOpenChange(false); }
        }
    );

    const form = useForm<ItemFormValues>({
        resolver: zodResolver(itemSchema) as Resolver<z.infer<typeof itemSchema>>,
        defaultValues: {
            itemName: "", itemCode: "", description: "",
            unit: "قطعة", quantity: 0, minimumQuantity: 0, condition: 0, location: "",
            serialNumber: "", notes: ""
        },
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty }, setValue, watch } = form;

    const onSubmit = (values: ItemFormValues) => mutation.mutate(values);

    const initialValues = useMemo(() => {
        if (!item) return {
            itemName: "", itemCode: "", description: "",
            unit: "قطعة", quantity: 0, minimumQuantity: 0, condition: 0, location: "",
            serialNumber: "", notes: ""
        };
        return {
            itemName: item.itemName,
            itemCode: item.itemCode,
            description: item.description || "",
            unit: item.unit,
            quantity: item.quantity,
            minimumQuantity: item.minimumQuantity,
            condition: item.condition,
            location: item.location || "",
            serialNumber: item.serialNumber || "",
            notes: item.notes || "",
        };
    }, [item]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEdit ? "تعديل الصنف" : "إضافة صنف جديد"}
            description="إدارة الأصول والمخزون"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen={false}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={isEdit ? "حفظ التعديلات" : "إضافة الصنف"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الأساسية" icon={Package}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="اسم الصنف" required error={errors.itemName?.message}>
                            <Input {...register("itemName")} placeholder="اسم الصنف" icon={Package} />
                        </FormField>
                        <FormField label="كود الصنف" required error={errors.itemCode?.message}>
                            <Input {...register("itemCode")} placeholder="رقم فريد" icon={Hash} className="font-mono" />
                        </FormField>
                        <FormField label="وحدة القياس" required error={errors.unit?.message}>
                            <Input {...register("unit")} placeholder="قطعة، علبة، رزمة..." />
                        </FormField>
                        <FormField label="حالة الصنف" error={errors.condition?.message}>
                            <NativeSelect
                                options={[
                                    { value: "0", label: "جديد" },
                                    { value: "1", label: "مستعمل" },
                                ]}
                                value={watch("condition")}
                                onChange={(val) => setValue("condition", Number(val), { shouldValidate: true })}
                                icon={Wrench}
                            />
                        </FormField>
                        <FormField label="الكمية الحالية" error={errors.quantity?.message}>
                            <Input {...register("quantity")} type="number" icon={Package} />
                        </FormField>
                        <FormField label="الحد الأدنى" error={errors.minimumQuantity?.message}>
                            <Input {...register("minimumQuantity")} type="number" />
                        </FormField>
                        <FormField label="الموقع" error={errors.location?.message}>
                            <Input {...register("location")} placeholder="مستودع، مكتب..." icon={MapPin} />
                        </FormField>
                        <FormField label="الرقم التسلسلي" error={errors.serialNumber?.message}>
                            <Input {...register("serialNumber")} placeholder="للأجهزة" icon={Barcode} className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="ملاحظات" icon={StickyNote}>
                    <FormField label="الوصف" error={errors.description?.message}>
                        <Textarea {...register("description")} placeholder="وصف الصنف..." rows={2} />
                    </FormField>
                    <FormField label="ملاحظات" error={errors.notes?.message}>
                        <Textarea {...register("notes")} placeholder="ملاحظات إضافية..." rows={2} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
