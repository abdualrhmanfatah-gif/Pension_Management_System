import { zodResolver } from "@hookform/resolvers/zod";
import { FileText, Hash, Type } from "lucide-react";
import { useCallback, useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NoteBox,
    Textarea
} from "@/components/ui";

// ─── Types ─────────────────────────────────────────────

export interface LookupItem {
    id: number;
    name: string;
    code?: string;
    description?: string;
}

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    name: z.string().min(2, "الاسم مطلوب"),
    code: z.string().optional().or(z.literal("")),
    description: z.string().optional().or(z.literal(""))
});

type FormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface LookupFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSubmit: (values: FormValues) => void;
    initialData?: LookupItem;
    isLoading?: boolean;
    lookupType?: string;
    title?: string;
}

// ─── Component ─────────────────────────────────────────

export function LookupDialog({
    open,
    onOpenChange,
    onSubmit,
    initialData,
    isLoading,
    lookupType,
    title
}: LookupFormDialogProps) {
    const isEditing = !!initialData;

    const form = useForm<FormValues>({
        resolver: zodResolver(schema) as Resolver<z.infer<typeof schema>>,
        defaultValues: useMemo(() => ({
            name: "",
            code: "",
            description: "",
        }), []),
    });

    const { register, reset, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        return {
            name: initialData.name || "",
            code: initialData.code || "",
            description: initialData.description || "",
        };
    }, [initialData]);

    const handleFormSubmit = useCallback((values: FormValues) => {
        onSubmit(values);
    }, [onSubmit]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={title || (isEditing ? `تعديل ${lookupType || ""}` : `إضافة ${lookupType || "عنصر"} جديد`)}
            size="lg"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel={isEditing ? "حفظ" : "إضافة"}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الأساسية" icon={Type}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الاسم / المسمى" error={errors.name?.message as string} required>
                            <Input {...register("name")} placeholder="أدخل المسمى..." />
                        </FormField>

                        <FormField label="الكود البرمجي (اختياري)" error={errors.code?.message as string}>
                            <Input {...register("code")} placeholder="مثال: TYPE_01" icon={Hash} className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الوصف" icon={FileText}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="ملاحظات توضيحية" error={errors.description?.message as string}>
                            <Textarea {...register("description")} rows={2} placeholder="وصف موجز لهذا العنصر..." />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    تغيير هذه البيانات قد يؤثر على تصنيفات السجلات المرتبطة بها في النظام.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
