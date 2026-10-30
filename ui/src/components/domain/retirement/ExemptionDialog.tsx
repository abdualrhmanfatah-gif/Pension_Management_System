"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { FileText,Gavel, ShieldCheck } from "lucide-react";
<<<<<<< HEAD
import { Controller,useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { DocumentChecklistField } from "@/components/domain/retirement/DocumentChecklistField";
import {
  BaseFormDialog,
  DateInput,
=======
import { useForm, useWatch, Controller } from "react-hook-form";
import { z } from "zod";

import {
  BaseFormDialog,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  FileField,
  FormField,
  FormSection,
  Input,
<<<<<<< HEAD
  NoteBox,
  Textarea,
} from "@/components/ui";
=======
  DateInput,
  NoteBox,
  Textarea,
} from "@/components/ui";
import { DocumentChecklistField } from "@/components/domain/retirement/DocumentChecklistField";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type { MissingDocOption } from "@/utils/missing-documents";

// ─── Validation Schema ─────────────────────────────────

const exemptionSchema = z.object({
  missingDocumentIDs: z.array(z.union([z.number(), z.string()])).min(1, "يجب اختيار وثيقة واحدة على الأقل"),
  directiveSource: z.string().trim().min(3, "مصدر التوجيه مطلوب"),
  exemptionDate: z.string().min(1, "تاريخ الإعفاء مطلوب"),
  notes: z.string().trim().optional().or(z.literal("")),
  exemptionFile: z.instanceof(File).optional().nullable(),
});

export type ExemptionFormValues = z.infer<typeof exemptionSchema>;

interface ExemptionDialogProps {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
    onSubmit: (values: ExemptionFormValues) => void;
    isLoading?: boolean;
    mode: "create" | "edit";
    missingDocs: MissingDocOption[];
    initialData?: Partial<ExemptionFormValues>;
}

/**
 * حوار تسجيل إعفاء جديد - Named Export (Constraint 8)
 */
export function ExemptionDialog({
    open,
    isOpen,
    onOpenChange,
    onClose,
    onSubmit,
    isLoading,
    mode,
    missingDocs,
    initialData
}: ExemptionDialogProps) {
    const form = useForm<ExemptionFormValues>({
        resolver: zodResolver(exemptionSchema),
        defaultValues: {
            missingDocumentIDs: initialData?.missingDocumentIDs || [],
            directiveSource: initialData?.directiveSource || "",
            exemptionDate: initialData?.exemptionDate || new Date().toISOString().split('T')[0],
            notes: initialData?.notes || "",
            exemptionFile: null,
        }
    });

    const { control, register, setValue, formState: { errors, isDirty } } = form;
    const selectedDocIDs = useWatch({ control, name: "missingDocumentIDs" }) || [];

    return (
        <BaseFormDialog
            open={open}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title={mode === "create" ? "تسجيل إعفاء جديد" : "تعديل بيانات الإعفاء"}
            description="إسقاط المطالبة بوثيقة ناقصة بناءً على توجيه رسمي"
            size="full"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إضافة الإعفاء" : "حفظ التعديلات"}
        >
            <div className="space-y-4 text-start" dir="rtl">
                <FormSection title="الوثائق المعفاة" icon={ShieldCheck}>
                    <DocumentChecklistField
                        selectedIds={selectedDocIDs}
                        documents={missingDocs}
                        onChange={(ids) => setValue("missingDocumentIDs", ids, { shouldDirty: true })}
                        error={errors.missingDocumentIDs?.message}
                        label="اختر الوثائق من قائمة النواقص"
                    />
                </FormSection>

                <FormSection title="تفاصيل توجيه الإعفاء" icon={Gavel}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="مصدر التوجيه الرسمي" required error={errors.directiveSource?.message}>
                            <Input {...register("directiveSource")} placeholder="مثال: مدير الصندوق، الوزير..." />
                        </FormField>
                        <FormField label="تاريخ صدور التوجيه" required error={errors.exemptionDate?.message}>
                            <Controller
                                name="exemptionDate"
                                control={control}
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
                    </div>
                </FormSection>
                
                <FormSection title="ملاحظات ومرفقات الإعفاء" icon={FileText}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="ملاحظات (اختياري)" error={errors.notes?.message}>
                            <Textarea {...register("notes")} placeholder="تفاصيل إضافية حول سبب الإعفاء..." rows={1} />
                        </FormField>
                        <FileField name="exemptionFile" control={control} label="مرفق التوجيه (اختياري)" size="sm" />
                    </div>
                </FormSection>

                 <NoteBox type="warning" title="الأثر القانوني">
                    بموجب هذا الإجراء، سيتم إسقاط المطالبة بهذه الوثائق بشكل نهائي من الملف التقاعدي.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
