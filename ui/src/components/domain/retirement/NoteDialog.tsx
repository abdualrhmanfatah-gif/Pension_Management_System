import { zodResolver } from "@hookform/resolvers/zod";
import { StickyNote } from "lucide-react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import {
  BaseFormDialog,
  FormField,
  FormSection,
  Textarea,
} from "@/components/ui";

// ─── Validation Schema ─────────────────────────────────

const noteSchema = z.object({
  content: z.string().trim().min(5, "الملاحظة يجب أن تكون 5 أحرف على الأقل"),
});

type NoteFormValues = z.infer<typeof noteSchema>;

interface NoteDialogProps {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
    onSubmit: (values: NoteFormValues) => void;
    isLoading?: boolean;
    initialData?: Partial<NoteFormValues>;
}

/**
 * Standardized Note Dialog.
 * Refactored for Batch 4: Extracted from NotesTab.tsx to a standalone component.
 */
export function NoteDialog({
    open,
    isOpen,
    onOpenChange,
    onClose,
    onSubmit,
    isLoading,
    initialData,
}: NoteDialogProps) {
    const form = useForm<NoteFormValues>({
        resolver: zodResolver(noteSchema),
        defaultValues: {
            content: "",
        }
    });

    const { register, formState: { errors, isDirty } } = form;

    return (
        <BaseFormDialog<NoteFormValues>
            open={open}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title="إضافة ملاحظة على الملف"
            description="أضف ملاحظة إدارية أو فنية لتكون مرجعاً للمستخدمين الآخرين"
            size="md"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialData || undefined}
            showGlobalError
            formErrors={errors}
            submitLabel="حفظ الملاحظة"
        >
            <div className="space-y-4">
                <FormSection title="محتوى الملاحظة" icon={StickyNote}>
                    <div className="grid grid-cols-1">
                        <FormField label="محتوى الملاحظة" required error={errors.content?.message}>
                            <Textarea 
                                {...register("content")} 
                                placeholder="اكتب الملاحظة هنا..." 
                                rows={6}
                                className="leading-relaxed"
                            />
                        </FormField>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
