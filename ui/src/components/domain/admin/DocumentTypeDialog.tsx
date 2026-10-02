import { zodResolver } from "@hookform/resolvers/zod";
import { Activity,FileText, HardDrive, Settings2 } from "lucide-react";
import { useMemo } from "react";
import { Controller, type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { documentTypesApi } from "@/api/document-types";
import { 
    BaseFormDialog, 
    Checkbox,
    FormField, 
    FormSection,
    Input,
    NativeSelect,
    NoteBox
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const docTypeSchema = z.object({
    typeName: z.string().trim().min(3, "اسم النوع مطلوب (3 أحرف على الأقل)"),
    ownerType: z.coerce.number().min(0).optional(),
    isRequired: z.boolean(),
    hasExpiry: z.boolean(),
    maxFileSize: z.coerce.number().min(1, "الحجم الأقصى يجب أن يكون 1 ميجابايت على الأقل"),
    allowedExtensions: z.string().trim().min(1, "الامتدادات المسموحة مطلوبة"),
    isActive: z.boolean(),
});

type DocTypeFormValues = z.infer<typeof docTypeSchema>;

interface DocumentTypeDto {
    documentTypeID: number;
    typeName: string;
    ownerType: number | null;
    isRequired: boolean;
    hasExpiry: boolean;
    maxFileSize: number | null;
    allowedExtensions: string | null;
    isActive: boolean;
}

const OWNER_TYPE_OPTIONS = [
    { value: "1", label: "ملف تقاعدي" },
    { value: "2", label: "متقاعد" },
    { value: "3", label: "مستحق" },
    { value: "4", label: "وكيل" },
    { value: "5", label: "ولي" },
    { value: "6", label: "رهن مستندات" },
    { value: "7", label: "إعفاء مستندات" },
    { value: "15", label: "مشروع استثماري" },
    { value: "16", label: "عقار استثماري" },
    { value: "17", label: "وحدة عقارية" },
    { value: "18", label: "عقد إيجار" },
    { value: "19", label: "مستأجر" },
    { value: "20", label: "محفظة استثمارية" },
    { value: "21", label: "طلب صيانة" },
    { value: "22", label: "أمر عمل" },
    { value: "25", label: "تقييم عقاري" },
    { value: "27", label: "إهلاك عقاري" },
    { value: "28", label: "دفعة إيجار" },
    { value: "30", label: "الميزانية التشغيلية" },
    { value: "31", label: "خطة استثمارية" },
    { value: "32", label: "معاملة استثمارية" },
    { value: "33", label: "تقييم مخاطر" },
    { value: "40", label: "موظف" },
    { value: "41", label: "مراسلة" },
];

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    docType: DocumentTypeDto | null;
}

/**
 * Standardized DocumentTypeDialog.
 * Refactored for 100% architectural compliance.
 */
export function DocumentTypeDialog({ open, onOpenChange, onSaved, docType }: Props) {
    const isEditing = !!docType;

    const mutation = useApiMutation(
        (values: DocTypeFormValues) => isEditing 
            ? documentTypesApi.update(docType!.documentTypeID, values)
            : documentTypesApi.create(values),
        {
            invalidateKeys: [["document-types"], ["lookups"]],
            successMessage: isEditing ? "تم تحديث نوع الوثيقة بنجاح" : "تم إضافة نوع الوثيقة الجديد بنجاح",
            onSuccessCallback: () => {
                onSaved();
                onOpenChange(false);
            }
        }
    );

    const form = useForm<DocTypeFormValues>({
        resolver: zodResolver(docTypeSchema) as Resolver<z.infer<typeof docTypeSchema>>,
        defaultValues: {
            typeName: "",
            ownerType: undefined,
            isRequired: false,
            hasExpiry: false,
            maxFileSize: 5,
            allowedExtensions: ".pdf,.jpg,.png",
            isActive: true,
        },
    });

    const { register, handleSubmit, reset, control, watch, setValue, formState: { errors, isDirty } } = form;

    const onSubmit = (values: DocTypeFormValues) => {
        mutation.mutate(values);
    };

    const initialValues = useMemo(() => {
        if (!docType) return {
            typeName: "",
            ownerType: undefined,
            isRequired: false,
            hasExpiry: false,
            maxFileSize: 5,
            allowedExtensions: ".pdf,.jpg,.png",
            isActive: true,
        };
        return {
            typeName: docType.typeName,
            ownerType: docType.ownerType ?? undefined,
            isRequired: docType.isRequired,
            hasExpiry: docType.hasExpiry,
            maxFileSize: docType.maxFileSize || 5,
            allowedExtensions: docType.allowedExtensions || ".pdf,.jpg,.png",
            isActive: docType.isActive,
        };
    }, [docType]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEditing ? "تعديل نوع الوثيقة" : "تعريف نوع وثيقة جديد"}
            description="إدارة تصنيفات الوثائق ومتطلباتها التقنية في النظام"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={isEditing ? "حفظ التعديلات" : "إضافة النوع"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الأساسية" icon={FileText}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="اسم نوع الوثيقة" required error={errors.typeName?.message}>
                            <Input 
                                {...register("typeName")} 
                                placeholder="مثال: البطاقة الشخصية، قرار التعيين..." 
                                icon={FileText}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الجهة المالكة" icon={Settings2}>
                    <div className="w-full max-w-xs">
                        <Controller
                            name="ownerType"
                            control={control}
                            render={({ field }) => (
                                <NativeSelect
                                    label="الجهة"
                                    value={field.value?.toString() || ""}
                                    onChange={(val) => field.onChange(val ? Number(val) : undefined)}
                                    options={OWNER_TYPE_OPTIONS}
                                    placeholder="عام (بدون جهة)"
                                />
                            )}
                        />
                    </div>
                </FormSection>

                <FormSection title="القيود والمتطلبات التقنية" icon={HardDrive}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الحجم الأقصى للملف (MB)" required error={errors.maxFileSize?.message}>
                            <Input 
                                {...register("maxFileSize")} 
                                type="number" 
                                min={1} 
                                icon={HardDrive}
                                dir="ltr"
                            />
                        </FormField>

                        <FormField label="الامتدادات المسموح بها" required error={errors.allowedExtensions?.message}>
                            <Input 
                                {...register("allowedExtensions")} 
                                placeholder=".pdf,.jpg,.png" 
                                icon={Settings2} 
                                dir="ltr"
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="خصائص الوثيقة والحالة" icon={Activity}>
                    <div className="space-y-4">
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <label className="flex items-center gap-3 p-4 bg-muted/30 rounded-2xl border border-border/40 hover:bg-muted/50 transition-colors cursor-pointer">
                                <Checkbox
                                    id="is-required-check"
                                    checked={watch("isRequired")}
                                    onCheckedChange={(checked) => setValue("isRequired", !!checked)}
                                />
                                <span className="text-sm font-bold text-foreground cursor-pointer">
                                    وثيقة إلزامية
                                </span>
                            </label>

                            <label className="flex items-center gap-3 p-4 bg-muted/30 rounded-2xl border border-border/40 hover:bg-muted/50 transition-colors cursor-pointer">
                                <Checkbox
                                    id="has-expiry-check"
                                    checked={watch("hasExpiry")}
                                    onCheckedChange={(checked) => setValue("hasExpiry", !!checked)}
                                />
                                <span className="text-sm font-bold text-foreground cursor-pointer">
                                    تطلب تاريخ انتهاء
                                </span>
                            </label>
                        </div>

                        <label className="flex items-center gap-3 p-4 bg-primary/5 rounded-2xl border border-primary/20 cursor-pointer">
                            <Checkbox
                                id="is-active-check"
                                checked={watch("isActive")}
                                onCheckedChange={(checked) => setValue("isActive", !!checked)}
                            />
                            <span className="text-sm font-bold text-primary cursor-pointer">
                                النوع نشط (متاح للاستخدام في الملفات)
                            </span>
                        </label>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    تنبيه: التعديلات على القيود التقنية (مثل الحجم والامتدادات) ستطبق على الملفات التي سيتم رفعها مستقبلاً فقط.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}