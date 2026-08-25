import { zodResolver } from "@hookform/resolvers/zod";
import { FileText, RefreshCcw, ShieldCheck } from "lucide-react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import {
    BaseFormDialog,
    FileField,
    FormField,
    FormSection,
    NoteBox,
    NativeSelect} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const reactivateSchema = z.object({
    reactivationCode: z.string().min(1, "سبب إعادة التفعيل مطلوب"),
    document: z.instanceof(File).optional().nullable(),
    civilServiceProof: z.instanceof(File).optional().nullable(),
});

type ReactivateFormValues = z.infer<typeof reactivateSchema>;

interface ReactivateEntitledPersonDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    personId: number;
    targetName: string;
    fileId: number;
    onSuccess?: () => void;
}

export function ReactivateEntitledPersonDialog({ open, onOpenChange, personId, targetName, fileId, onSuccess }: ReactivateEntitledPersonDialogProps) {
    const form = useForm<ReactivateFormValues>({
        resolver: zodResolver(reactivateSchema),
        defaultValues: {
            reactivationCode: "",
            document: null,
            civilServiceProof: null,
        }
    });

    const { control, formState: { errors, isDirty } } = form;
    const reactivationCode = useWatch({ control, name: "reactivationCode" });

    const mutation = useApiMutation(async (values: ReactivateFormValues) => {
        const formData = new FormData();
        formData.append("reactivationCode", values.reactivationCode);
        if (values.document) formData.append("document", values.document);
        if (values.civilServiceProof) formData.append("civilServiceProof", values.civilServiceProof);

        return api.put(`/entitled-persons/${personId}/reactivate`, formData, {
            headers: { "Content-Type": "multipart/form-data" },
        });
    }, {
        successMessage: "تم إعادة تفعيل الصرف بنجاح",
        invalidateKeys: [["entitled-persons", fileId], ["retirement-file"]],
        onSuccess: () => {
            onOpenChange(false);
            form.reset();
            onSuccess?.();
        }
    });

    const handleFormSubmit = (values: ReactivateFormValues) => {
        mutation.mutate(values);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={`إعادة تفعيل صرف الوريث: ${targetName}`}
            description="طلب استئناف صرف المستحقات المالية المتوقفة بناءً على زوال سبب الإيقاف"
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد إعادة التفعيل"
        >
            <div className="space-y-4">
                <FormSection title="تصنيف طلب إعادة التفعيل" icon={RefreshCcw}>
                    <FormField label="سبب إعادة التفعيل" required error={errors.reactivationCode?.message}>
                        <Controller
                            name="reactivationCode"
                            control={control}
                            render={({ field }) => (
                                <NativeSelect
                                    options={[
                                        { value: "DIVORCED", label: "طلاق (للابنة/الأرملة)" },
                                        { value: "HUSBAND_DECEASED", label: "وفاة الزوج" },
                                        { value: "DISABILITY_REPORT", label: "تقرير طبي (عجز)" },
                                        { value: "ADMIN_DECISION", label: "قرار إداري" },
                                    ]}
                                    value={field.value}
                                    onChange={field.onChange}
                                    placeholder="اختر سبباً..."
                                />
                            )}
                        />
                    </FormField>
                </FormSection>

                {(reactivationCode === "DIVORCED" || reactivationCode === "DISABILITY_REPORT" || reactivationCode === "HUSBAND_DECEASED") && (
                    <FormSection title="الوثائق الثبوتية المطلوبة" icon={FileText}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <FileField
                                name="document"
                                control={control}
                                label={
                                    reactivationCode === "DIVORCED" ? "وثيقة الطلاق الرسمية" :
                                        reactivationCode === "DISABILITY_REPORT" ? "التقرير الطبي المعتمد" :
                                            "شهادة وفاة الزوج"
                                }
                                required
                                size="sm"
                            />

                            {reactivationCode === "HUSBAND_DECEASED" && (
                                <FileField
                                    name="civilServiceProof"
                                    control={control}
                                    label="إثبات عدم تقاضي معاش (الأحوال المدنية)"

                                    required
                                    size="sm"
                                />
                            )}
                        </div>
                    </FormSection>
                )}

                {reactivationCode === "ADMIN_DECISION" ? (
                    <NoteBox type="info" title="توجيه إداري">
                        يرجى التأكد من إرفاق القرار الإداري في تبويب "الوثائق" العام للملف لضمان اكتمال الأوليات القانونية.
                    </NoteBox>
                ) : (
                    <NoteBox type="success" title="جاهزية الطلب" icon={ShieldCheck}>
                        بمجرد اكتمال رفع الوثائق المطلوبة، سيتم إرسال الطلب للمراجعة المالية لإعادة إدراج الوريث في كشوفات الصرف.
                    </NoteBox>
                )}
            </div>
        </BaseFormDialog>
    );
}
