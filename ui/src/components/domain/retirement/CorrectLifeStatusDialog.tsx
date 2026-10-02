import { zodResolver } from "@hookform/resolvers/zod";
import { ClipboardCheck } from "lucide-react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";

import { BaseFormDialog, FormField, FormSection, NativeSelect, NoteBox, Textarea } from "@/components/ui";

const schema = z.object({
    reason: z.string().trim().min(10, "سبب التصحيح يجب ألا يقل عن 10 أحرف"),
    healthStatusID: z.number().int().min(1, "اختر الحالة الصحية").max(3, "اختر الحالة الصحية"),
});

export type CorrectLifeStatusValues = z.infer<typeof schema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSubmit: (values: CorrectLifeStatusValues) => void;
    isLoading: boolean;
    retireeName: string;
}

export function CorrectLifeStatusDialog({ open, onOpenChange, onSubmit, isLoading, retireeName }: Props) {
    const form = useForm<CorrectLifeStatusValues>({
        resolver: zodResolver(schema),
        defaultValues: { reason: "", healthStatusID: 1 },
    });
    const { errors, isDirty } = form.formState;

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تصحيح حالة حياة المتقاعد"
            description={`تصحيح وفاة مسجلة خطأً للمتقاعد: ${retireeName}`}
            size="md"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد التصحيح"
        >
            <div className="space-y-4 text-start">
                <FormSection title="بيانات التصحيح" icon={ClipboardCheck}>
                    <div className="space-y-4">
                        <FormField label="الحالة الصحية الصحيحة" required error={errors.healthStatusID?.message}>
                            <Controller
                                name="healthStatusID"
                                control={form.control}
                                render={({ field }) => (
                                    <NativeSelect
                                        id={field.name}
                                        value={field.value}
                                        onChange={(value) => field.onChange(Number(value))}
                                        options={[
                                            { value: "1", label: "سليم" },
                                            { value: "2", label: "عاجز جزئياً" },
                                            { value: "3", label: "عاجز كلياً" },
                                        ]}
                                        placeholder=""
                                        error={errors.healthStatusID?.message}
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="سبب التصحيح" required error={errors.reason?.message}>
                            <Textarea {...form.register("reason")} rows={3} placeholder="اذكر سبب تصحيح حالة الوفاة والمرجع الإداري" />
                        </FormField>
                    </div>
                </FormSection>
                <NoteBox type="warning" title="ما سيحدث بعد التصحيح">
                    سيُمسح تاريخ الوفاة، ويُوقف استحقاق الورثة. سينتقل الملف للمراجعة ويبقى الصرف موقوفاً حتى الاعتماد، ولن تُعاد دفاتر الصرف الملغاة تلقائياً.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
