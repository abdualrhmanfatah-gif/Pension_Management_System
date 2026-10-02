import { zodResolver } from "@hookform/resolvers/zod";
import { FileText, Hash, Library, Type } from "lucide-react";
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

export interface AccountDto {
    accountID: number;
    accountCode: string;
    accountName: string;
    parentID?: number | null;
    description?: string;
}

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    accountCode: z.string().min(1, "كود الحساب مطلوب"),
    accountName: z.string().min(2, "اسم الحساب مطلوب"),
    parentID: z.number().optional().nullable(),
    description: z.string().optional().or(z.literal(""))
});

type FormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface AccountFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSubmit: (values: FormValues) => void;
    initialData?: AccountDto;
    isLoading?: boolean;
}

// ─── Component ─────────────────────────────────────────

export function AccountDialog({
    open,
    onOpenChange,
    onSubmit,
    initialData,
    isLoading
}: AccountFormDialogProps) {
    const isEditing = !!initialData;

    const form = useForm<FormValues>({
        resolver: zodResolver(schema) as Resolver<z.infer<typeof schema>>,
        defaultValues: useMemo(() => ({
            accountCode: "",
            accountName: "",
            parentID: null,
            description: "",
        }), []),
    });

    const { register, reset, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        return {
            accountCode: initialData.accountCode || "",
            accountName: initialData.accountName || "",
            parentID: initialData.parentID || null,
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
            title={isEditing ? "تعديل حساب" : "إضافة حساب جديد"}
            description="تعريف الحسابات المالية في دليل الحسابات"
            size="lg"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel={isEditing ? "حفظ التعديلات" : "إضافة الحساب"}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4 text-start">
                <FormSection title="هوية الحساب" icon={Library}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="كود الحساب" error={errors.accountCode?.message as string} required>
                            <Input {...register("accountCode")} placeholder="مثال: 1101" icon={Hash} className="font-mono font-bold" />
                        </FormField>

                        <FormField label="اسم الحساب" error={errors.accountName?.message as string} required>
                            <Input {...register("accountName")} placeholder="مثال: نقدية الصندوق" icon={Type} />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="تفاصيل إضافية" icon={FileText}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="وصف الحساب" error={errors.description?.message as string}>
                            <Textarea {...register("description")} rows={3} placeholder="وصف موجز لطبيعة الحساب واستخدامه..." />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    يرجى التأكد من صحة الكود والمسمى لضمان دقة التقارير المالية وميزان المراجعة.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
