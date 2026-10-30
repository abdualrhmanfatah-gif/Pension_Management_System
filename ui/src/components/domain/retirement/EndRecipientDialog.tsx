import { zodResolver } from "@hookform/resolvers/zod";
import { Calendar } from "lucide-react";
import { useForm, Controller } from "react-hook-form";
import { z } from "zod";

import {
    BaseFormDialog,
    FormField,
    FormSection,
    DateInput,
    NoteBox,
    Textarea} from "@/components/ui";

// ─── Validation Schema ─────────────────────────────────

const endRecipientSchema = z.object({
    endDate: z.string().min(1, "تاريخ انتهاء الصرف مطلوب"),
    notes: z.string().trim().min(3, "يجب إدخال سبب الإنهاء (3 أحرف على الأقل)"),
});

export type EndRecipientFormValues = z.infer<typeof endRecipientSchema>;

interface EndRecipientDialogProps {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
    onSubmit: (values: EndRecipientFormValues) => void;
    isLoading?: boolean;
    recipientName: string;
}

/**
 * حوار إنهاء حالة مستلم المعاش - Named Export (Constraint 8)
 */
export function EndRecipientDialog({
    open,
    isOpen,
    onOpenChange,
    onClose,
    onSubmit,
    isLoading,
    recipientName
}: EndRecipientDialogProps) {
    const form = useForm<EndRecipientFormValues>({
        resolver: zodResolver(endRecipientSchema),
        defaultValues: {
            endDate: new Date().toISOString().split('T')[0],
            notes: "",
        }
    });

    const { register, formState: { errors, isDirty } } = form;

    return (
        <BaseFormDialog
            open={open}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title="إنهاء حالة مستلم المعاش"
            description={`أنت بصدد إيقاف صرف المستحقات المالية لـ: ${recipientName}`}
            size="md"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد الإنهاء"
            className="border-t-4 border-t-destructive"
        >
            <div className="space-y-4 text-start">
                <FormSection title="تفاصيل الإنهاء" icon={Calendar}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="تاريخ انتهاء الاستحقاق" required error={errors.endDate?.message}>
                            <Controller
                                name="endDate"
                                control={form.control}
                                render={({ field }) => (
                                    <DateInput
                                        id={field.name}
                                        value={field.value || undefined}
                                        onChange={field.onChange}
                                        className="font-mono font-bold"
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="سبب الإنهاء / ملاحظات إدارية" required error={errors.notes?.message}>
                            <Textarea
                                {...register("notes")}
                                placeholder="يرجى توضيح سبب إيقاف الصرف لهذا المستلم..."
                                rows={2}
                                className="leading-relaxed"
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="warning" title="تنبيه الإيقاف">
                    سيتم إيقاف تحويل المبالغ المالية لهذا المستلم بدءاً من التاريخ المحدد. هذا الإجراء سيؤثر على كشوفات الصرف القادمة للملف.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
