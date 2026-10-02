"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { Calendar, FileText,Gavel } from "lucide-react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { documentsApi } from "@/api/documents";
import { 
  BaseFormDialog, 
  Button,
  DateInput,
  FileField, 
  FormField, 
  FormSection,
  Input,
  NoteBox,
  Textarea, 
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { cn } from "@/lib/utils";

// ─── Validation Schema ─────────────────────────────────

const actionSchema = z.object({
    actionType: z.enum(["pledge", "exemption"]),
    pledgeDeadline: z.string().optional(),
    exemptionSource: z.string().optional(),
    notes: z.string().trim().optional().or(z.literal("")),
    file: z.instanceof(File).optional().nullable(),
}).refine(data => {
    if (data.actionType === "pledge") return !!data.pledgeDeadline;
    if (data.actionType === "exemption") return !!data.exemptionSource;
    return true;
}, {
    message: "يرجى ملء كافة الحقول الإلزامية للإجراء المختار للمتابعة",
    path: ["actionType"]
});

type ActionFormValues = z.infer<typeof actionSchema>;

interface MissingDocActionModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved?: () => void;
    fileId: number;
    activeMissing: {
        missingDocumentID: number;
        documentTypeID: number;
        documentTypeName: string;
        isAutoDetected?: boolean;
    } | null;
}

/**
 * Standardized MissingDocActionDialog.
 * Refactored for Batch 2: Strictly uses BaseFormDialog, FormSection, and FileField.
 */
export function MissingDocActionDialog({ 
    open, 
    onOpenChange, 
    onSaved,
    fileId,
    activeMissing
}: MissingDocActionModalProps) {
    const mutation = useApiMutation(
        async (values: ActionFormValues) => {
            if (!activeMissing) return;

            let missingId = activeMissing.missingDocumentID;
            if (activeMissing.isAutoDetected) {
                const res = await api.post<{ data: { missingDocumentID: number } }>("/missing-documents", {
                    fileID: fileId,
                    documentTypeID: activeMissing.documentTypeID,
                });
                missingId = res.data.data.missingDocumentID;
            }

            if (values.actionType === "pledge") {
                const pRes = await api.post<{ data: { pledgeID: number } }>("/document-pledges", {
                    fileID: fileId,
                    missingDocumentIDs: [missingId],
                    deadline: values.pledgeDeadline,
                });
                if (values.file && pRes.data.data) {
                    const formData = new FormData();
                    formData.append("File", values.file);
                    formData.append("RetirementFileID", String(fileId));
                    formData.append("DocumentTypeID", "9"); // Type for Pledge
                    formData.append("PledgeID", String(pRes.data.data.pledgeID));
                    await documentsApi.upload(formData);
                }
            } else {
                const eRes = await api.post<{ data: { exemptionID: number } }>("/document-exemptions", {
                    fileID: fileId,
                    missingDocumentIDs: [missingId],
                    directiveSource: values.exemptionSource,
                    exemptionDate: new Date().toISOString(),
                });
                if (values.file && eRes.data.data) {
                    const formData = new FormData();
                    formData.append("File", values.file);
                    formData.append("RetirementFileID", String(fileId));
                    formData.append("DocumentTypeID", "10"); // Type for Exemption
                    formData.append("ExemptionID", String(eRes.data.data.exemptionID));
                    await documentsApi.upload(formData);
                }
            }
        },
        {
            invalidateKeys: [["missing-documents"], ["documents"], ["retirement-file", fileId]],
            successMessage: "تم تنفيذ الإجراء بنجاح",
        }
    );

    const form = useForm<ActionFormValues>({
        resolver: zodResolver(actionSchema),
        defaultValues: {
            actionType: "pledge",
            pledgeDeadline: "",
            exemptionSource: "",
            notes: "",
            file: null,
        }
    });

    const { control, register, setValue, formState: { errors, isDirty } } = form;
    const actionType = useWatch({ control, name: "actionType" });

    const handleActionSubmit = async (values: ActionFormValues) => {
        await mutation.mutateAsync(values);
        onSaved?.();
        onOpenChange(false);
        form.reset();
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="اتخاذ إجراء على وثيقة ناقصة"
            description={`الوثيقة المطلوبة: ${activeMissing?.documentTypeName || ""}`}
            size="full"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleActionSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تنفيذ الإجراء واعتماده"
        >
            <div className="space-y-4">
                <FormSection title="تحديد نوع الإجراء القانوني" icon={Gavel}>
                    <FormField label="الإجراء المختار" required error={errors.actionType?.message}>
                        <Controller
                            name="actionType"
                            control={control}
                            render={({ field }) => (
                                <div className="flex gap-4">
                                    <Button 
                                        type="button"
                                        variant={field.value === "pledge" ? "primary" : "outline"}
                                        className={cn(
                                            "flex-1 h-14 rounded-2xl flex flex-col items-center justify-center gap-1 transition-all",
                                            field.value === "pledge" && "ring-2 ring-primary ring-offset-2"
                                        )}
                                        onClick={() => setValue("actionType", "pledge", { shouldDirty: true })}
                                    >
                                        <span className="font-bold">تعهد</span>
                                        <span className="text-[10px] sm:text-xs opacity-70">التزام بإحضار الوثيقة لاحقاً</span>
                                    </Button>
                                    <Button 
                                        type="button"
                                        variant={field.value === "exemption" ? "secondary" : "outline"}
                                        className={cn(
                                            "flex-1 h-14 rounded-2xl flex flex-col items-center justify-center gap-1 transition-all",
                                            field.value === "exemption" && "ring-2 ring-secondary ring-offset-2"
                                        )}
                                        onClick={() => setValue("actionType", "exemption", { shouldDirty: true })}
                                    >
                                        <span className="font-bold">إعفاء</span>
                                        <span className="text-[10px] sm:text-xs opacity-70">إسقاط المطالبة بالوثيقة</span>
                                    </Button>
                                </div>
                            )}
                        />
                    </FormField>
                </FormSection>

                <FormSection 
                    title={actionType === "pledge" ? "تفاصيل التعهد الزمني" : "بيانات إعفاء الوثيقة"} 
                    icon={actionType === "pledge" ? Calendar : Gavel}
                >
                    <div className="grid grid-cols-1 md:grid-cols-2 dlg-gap">
                        {actionType === "pledge" ? (
                            <FormField label="تاريخ المهلة النهائية" required error={errors.pledgeDeadline?.message}>
                                <Controller
                                    name="pledgeDeadline"
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
                        ) : (
                            <FormField label="جهة توجيه الإعفاء المعتمدة" required error={errors.exemptionSource?.message}>
                                <Input {...register("exemptionSource")} placeholder="مثلاً: مدير الدائرة المالية، الوزير المختص..." />
                            </FormField>
                        )}

                        <FormField label="ملاحظات وسياق الإجراء" error={errors.notes?.message}>
                            <Textarea 
                                {...register("notes")} 
                                placeholder="أدخل تفاصيل إضافية حول سبب هذا الإجراء..." 
                                className="min-h-[100px]"
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الوثيقة الثبوتية (اختياري)" icon={FileText}>
                    <FileField 
                        name="file" 
                        control={control} 
                        label={actionType === "pledge" ? "صورة نموذج التعهد المعتمد" : "صورة التوجيه الرسمي بالإعفاء"} 
                        size="sm"
                    />
                </FormSection>

                <NoteBox type="info" title="أثر الإجراء">
                    {actionType === "pledge" 
                        ? "سيسمح هذا التعهد باستمرار معاملة الملف مالياً حتى حلول تاريخ المهلة المختار." 
                        : "الإعفاء يسقط المطالبة بهذه الوثيقة نهائياً من قائمة النواقص لهذا الملف."}
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
