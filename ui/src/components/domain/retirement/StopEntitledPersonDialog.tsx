import { zodResolver } from "@hookform/resolvers/zod";
import { AlertTriangle, FileText } from "lucide-react";
import { Controller,useForm } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { 
  BaseFormDialog, 
  FileField, 
  FormField, 
  FormSection,
  NativeSelect, 
  NoteBox,
  Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const stopSchema = z.object({
    stopCode: z.string().min(1, "سبب الإيقاف مطلوب"),
    notes: z.string().trim().optional().or(z.literal("")),
    document: z.instanceof(File).optional().nullable(),
});

type StopFormValues = z.infer<typeof stopSchema>;

interface StopEntitledPersonDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    personId: number;
    targetName: string;
    fileId: number;
    onSuccess?: () => void;
}

export function StopEntitledPersonDialog({ 
    open, 
    onOpenChange,
    personId,
    targetName,
    fileId,
    onSuccess
}: StopEntitledPersonDialogProps) {
    const form = useForm<StopFormValues>({
        resolver: zodResolver(stopSchema),
        defaultValues: {
            stopCode: "",
            notes: "",
            document: null,
        }
    });

    const { control, register, formState: { errors, isDirty } } = form;

    const mutation = useApiMutation(async (values: StopFormValues) => {
        const formData = new FormData();
        formData.append("stopCode", values.stopCode);
        if (values.notes) formData.append("notes", values.notes);
        if (values.document) formData.append("document", values.document);

        return api.put(`/entitled-persons/${personId}/stop`, formData, {
            headers: { "Content-Type": "multipart/form-data" },
        });
    }, {
        successMessage: "تم إيقاف الصرف بنجاح",
        invalidateKeys: [["entitled-persons", fileId], ["retirement-file"]],
        onSuccess: () => {
            onOpenChange(false);
            form.reset();
            onSuccess?.();
        }
    });

    const handleFormSubmit = (values: StopFormValues) => {
        mutation.mutate(values);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={`إيقاف صرف الوريث: ${targetName}`}
            description="إجراء قانوني لإيقاف صرف المستحقات المالية بناءً على أسباب محددة"
            size="full"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد إيقاف الصرف"
            className="border-t-4 border-t-destructive"
        >
            <div className="space-y-4">
                <FormSection title="تصنيف الإيقاف" icon={AlertTriangle}>
                    <div className="max-w-md">
                        <FormField label="سبب الإيقاف القانوني" required error={errors.stopCode?.message}>
                            <Controller
                                name="stopCode"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
                                        options={[
                                            { value: "EMPLOYED", label: "التحق بوظيفة" },
                                            { value: "DAUGHTER_MARRIED", label: "زواج الابنة/الأخت" },
                                            { value: "WIDOW_REMARRIED", label: "زواج الأرملة" },
                                            { value: "AGE_LIMIT", label: "بلوغ السن القانوني" },
                                            { value: "DECEASED", label: "وفاة المستحق" },
                                            { value: "ADMIN_DECISION", label: "قرار إداري" },
                                        ]}
                                        value={field.value}
                                        onChange={field.onChange}
                                        placeholder="اختر سبب الإيقاف..."
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="البيانات والمستندات المؤيدة" icon={FileText}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="ملاحظات تفصيلية" error={errors.notes?.message}>
                            <Textarea 
                                {...register("notes")} 
                                placeholder="أدخل تفاصيل إضافية حول قرار الإيقاف..." 
                                className="min-h-[100px] leading-relaxed"
                            />
                        </FormField>

                        <FileField 
                            name="document" 
                            control={control} 
                            label="الوثيقة الثبوتية (اختياري)" 
                            hint="مثل شهادة وفاة، عقد زواج، أو تقرير إداري"
                            size="sm"
                        />
                    </div>
                </FormSection>

                <NoteBox type="warning" title="تنبيه مالي">
                    بمجرد تأكيد هذا الإجراء، سيتم حجب المستحقات المالية لهذا الوريث في كشوفات الصرف القادمة آلياً.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
