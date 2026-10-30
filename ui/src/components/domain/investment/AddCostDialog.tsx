import { zodResolver } from "@hookform/resolvers/zod";
import { Calculator, FileText,User } from "lucide-react";
<<<<<<< HEAD
import { Controller,useForm } from "react-hook-form";
=======
import { useForm, Controller } from "react-hook-form";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import * as z from "zod";

import { investmentApi } from "@/api/investment";
import { 
    BaseFormDialog, 
<<<<<<< HEAD
    DateInput,
    FormField, 
    FormSection,
    Input,
=======
    FormField, 
    FormSection,
    Input,
    DateInput,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    NoteBox,
    Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const schema = z.object({
    description: z.string().min(2, "الوصف مطلوب"),
    amount: z.number().positive("المبلغ يجب أن يكون موجباً"),
    costDate: z.string().min(1, "التاريخ مطلوب"),
    vendor: z.string().optional().or(z.literal("")),
    notes: z.string().optional().or(z.literal("")),
});

type FormValues = z.infer<typeof schema>;

interface Props {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
    ticketId: number;
}

/**
 * Standardized AddCostDialog.
 * Refactored to use BaseFormDialog and FormSection for maintenance cost tracking.
 */
export function AddCostDialog({ 
    open, 
    isOpen,
    onOpenChange,
    onClose, 
    ticketId 
}: Props) {
    const form = useForm<FormValues>({ 
        resolver: zodResolver(schema), 
        defaultValues: { 
            description: "", 
            amount: 0, 
            costDate: new Date().toISOString().split('T')[0], 
            vendor: "", 
            notes: "" 
        } 
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty } } = form;

    const mutation = useApiMutation(
        (v: FormValues) => investmentApi.addMaintenanceCost(ticketId, v),
        {
            successMessage: "تم تسجيل التكلفة بنجاح",
            invalidateKeys: [["maintenance-request", ticketId]],
            onSuccess: () => { 
                if (onOpenChange) onOpenChange(false);
                else if (onClose) onClose(); 
            }
        }
    );

    return (
        <BaseFormDialog 
            open={open}
            isOpen={isOpen} 
            onOpenChange={onOpenChange}
            onClose={onClose} 
            title="إضافة تكلفة صيانة" 
            description="تسجيل المصاريف الفعلية المرتبطة ببلاغ الصيانة"
            size="lg" 
            isLoading={mutation.isPending} 
            isDirty={isDirty}
            onSubmit={handleSubmit((v) => mutation.mutate(v))} 
            submitLabel="اعتماد التكلفة"
            resetOnOpen
            reset={reset}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4">
                <FormSection title="البيانات المالية" icon={Calculator}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="المبلغ المنصرف (ر.ي)" error={errors.amount?.message} required>
                            <Input {...register("amount", { valueAsNumber: true })} type="number" step="0.01" className="font-mono font-bold text-lg text-primary" />
                        </FormField>
                        <FormField label="تاريخ الصرف" error={errors.costDate?.message} required>
                            <Controller
                                name="costDate"
                                control={form.control}
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

                <FormSection title="تفاصيل الصرف والمورد" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="بيان التكلفة / الخدمة" error={errors.description?.message} required>
                            <Input {...register("description")} placeholder="مثال: قطع غيار، أجور أيدي عاملة..." />
                        </FormField>
                        <FormField label="اسم المورد / الفني" error={errors.vendor?.message}>
                            <Input {...register("vendor")} placeholder="الجهة المنفذة للخدمة" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الملاحظات" icon={FileText}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="ملاحظات إضافية" error={errors.notes?.message}>
                            <Textarea {...register("notes")} rows={2} placeholder="أي تفاصيل أخرى حول هذه التكلفة..." />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    سيتم ترحيل هذه التكلفة لتضاف إلى إجمالي مصاريف الصيانة الخاصة بالعقار المرتبط.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}


