import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle } from "lucide-react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import { 
  BaseFormDialog, 
  FormField, 
  FormSection,
  NoteBox,
  Textarea} from "@/components/ui";

// ─── Validation Schema ─────────────────────────────────

const endSuspensionSchema = z.object({
    endReason: z.string().trim().min(5, "يجب إدخال سبب الاستئناف (5 أحرف على الأقل)"),
});

export type EndSuspensionFormValues = z.infer<typeof endSuspensionSchema>;

interface EndSuspensionDialogProps {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
    onSubmit: (values: EndSuspensionFormValues) => void;
    isLoading?: boolean;
}

/**
 * حوار إنهاء إيقاف المعاش - Named Export (Constraint 8)
 */
export function EndSuspensionDialog({ 
    open, 
    isOpen,
    onOpenChange,
    onClose, 
    onSubmit, 
    isLoading 
}: EndSuspensionDialogProps) {
    const form = useForm<EndSuspensionFormValues>({
        resolver: zodResolver(endSuspensionSchema),
        defaultValues: {
            endReason: "",
        }
    });

    const { register, formState: { errors, isDirty } } = form;

    return (
        <BaseFormDialog
            open={open}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title="إنهاء إيقاف المعاش"
            description="إجراء إداري لاستئناف صرف المستحقات المالية للملف بناءً على زوال سبب الإيقاف"
            size="md"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد استئناف الصرف"
        >
            <div className="space-y-4 text-start">
                <FormSection title="سبب وتفاصيل الاستئناف" icon={CheckCircle}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="سبب الاستئناف الرسمي" required error={errors.endReason?.message}>
                            <Textarea 
                                {...register("endReason")} 
                                placeholder="يرجى توضيح سبب إعادة تفعيل الملف وصرف المستحقات..." 
                                rows={4}
                                className="leading-relaxed"
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="success" title="الأثر المالي">
                    سيتم إعادة إدراج هذا الملف في كشوفات الصرف القادمة فور اعتماد هذا الإجراء.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
