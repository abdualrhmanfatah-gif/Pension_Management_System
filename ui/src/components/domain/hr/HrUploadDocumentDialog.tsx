import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { ClipboardList, FileText,FileUp } from "lucide-react";
import { useMemo } from "react";
import { Controller, type Resolver,useForm } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  FileField, 
  FormField, 
  FormSection,
  NoteBox,
  NativeSelect, 
  Textarea, 
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const hrUploadSchema = z.object({
    documentTypeID: z.coerce.number().min(1, "نوع الوثيقة مطلوب"),
    file: z.instanceof(File, { message: "يرجى اختيار ملف صالح للمتابعة" }),
    notes: z.string().trim().optional().or(z.literal("")),
});

type HrUploadFormValues = z.infer<typeof hrUploadSchema>;

interface HrUploadDocumentDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    employeeId: number;
    employeeName?: string;
    onSaved?: () => void;
}

const DOCUMENT_OWNER_TYPE_EMPLOYEE = 40;

/**
 * Dialog for uploading documents for HR employees.
 * Compliant with project architecture and standards.
 */
export function HrUploadDocumentDialog({ 
    open, 
    onOpenChange,
    employeeId,
    employeeName,
    onSaved
}: HrUploadDocumentDialogProps) {
    // 1. Fetch doc types for employees (ownerType 40)
    const { data: docTypes = [] } = useQuery({
        queryKey: ["document-types", DOCUMENT_OWNER_TYPE_EMPLOYEE],
        queryFn: async () => {
            const res = await api.get<{ data: { documentTypeID: number; typeName: string }[] }>(`/document-types/by-owner/${DOCUMENT_OWNER_TYPE_EMPLOYEE}`);
            return res.data.data.map(t => ({ id: t.documentTypeID, name: t.typeName }));
        },
        enabled: open
    });

    const mutation = useApiMutation(
        (formData: FormData) => hrApi.uploadDocument(formData),
        {
            invalidateKeys: [["hr", "documents", employeeId]],
            successMessage: "تم رفع الوثيقة بنجاح",
            onSuccess: () => {
                onSaved?.();
                onOpenChange(false);
                reset();
            }
        }
    );

    const form = useForm<HrUploadFormValues>({
        resolver: zodResolver(hrUploadSchema) as Resolver<HrUploadFormValues>,
        defaultValues: {
            documentTypeID: 0,
            file: undefined as unknown as File,
            notes: "",
        }
    });

    const { control, register, handleSubmit, reset, formState: { errors, isDirty } } = form;
    
    const initialValues = useMemo(() => ({
        documentTypeID: 0,
        file: undefined as unknown as File,
        notes: "",
    }), []);

    const handleUploadSubmit = async (values: HrUploadFormValues) => {
        const formData = new FormData();
        formData.append("File", values.file);
        formData.append("EmployeeID", String(employeeId));
        formData.append("DocumentTypeID", String(values.documentTypeID));
        if (values.notes) formData.append("Notes", values.notes);
        
        await mutation.mutateAsync(formData);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="رفع وثيقة للموظف"
            description={employeeName ? `إرفاق مستندات الموارد البشرية لـ: ${employeeName}` : "إرفاق مستندات الموارد البشرية لملف الموظف"}
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(handleUploadSubmit)}
            resetOnOpen={false}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="بدء الرفع والاعتماد"
        >
            <div className="space-y-4">
                <FormSection title="تصنيف الوثيقة" icon={ClipboardList}>
                    <div className="max-w-md">
                        <FormField label="نوع الوثيقة المرفوعة" required error={errors.documentTypeID?.message}>
                            <Controller
                                name="documentTypeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
                                        options={docTypes.map((t) => ({ value: t.id.toString(), label: t.name }))}
                                        value={field.value?.toString() || ""}
                                        onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                        placeholder="اختر نوع الوثيقة من القائمة..."
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الملف والمرفقات" icon={FileUp}>
                    <div className="grid grid-cols-1 gap-4">
                        <FileField 
                            name="file" 
                            control={control} 
                            label="اختيار ملف الوثيقة" 
                            hint="PDF, PNG, JPG (الحد الأقصى 10MB)"
                            required 
                            accept=".pdf,image/*" 
                            size="sm"
                        />
                        
                        <div>
                            <FormField label="ملاحظات توضيحية (اختياري)" error={errors.notes?.message} icon={FileText}>
                                <Textarea 
                                    {...register("notes")} 
                                    placeholder="أضف أي تفاصيل إضافية..." 
                                    className="min-h-[80px] leading-relaxed"
                                />
                            </FormField>
                        </div>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    سيتم تخزين الوثيقة في الأرشيف الإلكتروني للموظف وربطها بسجله الوظيفي تلقائياً.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
