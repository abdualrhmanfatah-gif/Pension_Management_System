import { zodResolver } from "@hookform/resolvers/zod";
import { FileCheck, FileText, Type } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

<<<<<<< HEAD
import { type ApprovalRequestDto,investmentApi } from "@/api/investment";
=======
import { investmentApi, type ApprovalRequestDto } from "@/api/investment";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
<<<<<<< HEAD
    NativeSelect,
    NoteBox,
=======
    NoteBox,
    NativeSelect,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    entityType: z.string().min(1, "نوع الكيان مطلوب"),
    entityID: z.number().min(1, "معرف الكيان مطلوب"),
    requestNote: z.string().min(5, "يرجى كتابة شرح للطلب"),
});

export type ApprovalFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface ApprovalRequestFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: ApprovalRequestDto | null;
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function ApprovalRequestDialog({
    open,
    onOpenChange,
    initialData,
    onSuccess
}: ApprovalRequestFormDialogProps) {
    const form = useForm<ApprovalFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            entityType: "Project",
            entityID: 0,
            requestNote: "",
        }), []),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        if (!initialData) return undefined;
        return {
            entityType: initialData.entityType || "Project",
            entityID: initialData.entityID || 0,
            requestNote: "",
        };
    }, [initialData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        (values: ApprovalFormValues) => investmentApi.createApproval(values),
        {
            successMessage: "تم إرسال طلب الاعتماد بنجاح",
            invalidateKeys: [["investment-approvals"]],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: ApprovalFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={initialData ? "تعديل طلب الموافقة" : "إنشاء طلب موافقة جديد"}
            description="إرسال طلب رسمي للإدارة لاعتماد إجراء أو مصروف مالي"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            submitLabel="إرسال الطلب"
            resetOnOpen
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
        >
            <div className="space-y-4 text-start">
                <FormSection title="تفاصيل الكيان" icon={Type}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="نوع الكيان" required error={errors.entityType?.message}>
                            <Controller
                                name="entityType"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "مشروع استثماري", value: "Project" },
                                            { label: "أصل عقاري", value: "Property" },
                                            { label: "عقد إيجار", value: "Lease" },
                                            { label: "عملية مالية", value: "Transaction" },
                                        ]}
                                        value={field.value}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="رقم المعرف (ID)" required error={errors.entityID?.message}>
                            <Input {...register("entityID", { valueAsNumber: true })} type="number" placeholder="مثال: 101" className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="المسوغات والملاحظات" icon={FileText}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="شرح الطلب" required error={errors.requestNote?.message}>
                            <Textarea {...register("requestNote")} rows={4} placeholder="يرجى توضيح سبب طلب الاعتماد والتفاصيل اللازمة..." />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info" icon={FileCheck}>
                    سيتم تحويل هذا الطلب آلياً إلى صاحب الصلاحية المعني حسب مصفوفة الصلاحيات المعتمدة.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
