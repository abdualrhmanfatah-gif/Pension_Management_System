import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
<<<<<<< HEAD
import { Controller,useForm } from "react-hook-form";
=======
import { useForm, Controller } from "react-hook-form";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import * as z from "zod";

import { hrApi, type ServiceHistoryEventDto } from "@/api/hr";
import { 
    BaseFormDialog, 
<<<<<<< HEAD
    DateInput,
    FormField, 
    Input,
=======
    FormField, 
    Input,
    DateInput,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    NativeSelect 
} from "@/components/ui";
import { HrEventTypeConstants } from "@/constants/hr-event-types";
import { useApiMutation } from "@/hooks";

const schema = z.object({
    eventType: z.string().min(1, "نوع الحدث مطلوب"),
    eventDate: z.string().min(1, "التاريخ مطلوب"),
    summary: z.string().min(5, "يجب كتابة ملخص للحدث (5 أحرف على الأقل)"),
});

type FormValues = z.infer<typeof schema>;

interface ServiceHistoryDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    employeeId: number;
    event?: ServiceHistoryEventDto | null;
    onSuccess?: () => void;
}

export function ServiceHistoryDialog({ open, onOpenChange, employeeId, event, onSuccess }: ServiceHistoryDialogProps) {
    const isEdit = !!event;

    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            eventType: HrEventTypeConstants.JOB_HISTORY,
            eventDate: new Date().toISOString().split('T')[0],
            summary: "",
        }
    });

    useEffect(() => {
        if (event && open) {
            form.reset({
                eventType: event.eventType,
                eventDate: event.eventDate.split('T')[0],
                summary: event.summary,
            });
        } else if (!isEdit && open) {
            form.reset({
                eventType: HrEventTypeConstants.JOB_HISTORY,
                eventDate: new Date().toISOString().split('T')[0],
                summary: "",
            });
        }
    }, [event, open, isEdit, form]);

    const mutation = useApiMutation(
        (data: FormValues) => {
            const payload = { ...data, employeeID: employeeId };
            return isEdit && event
                ? hrApi.updateHistoryEvent(event.serviceHistoryEventID, payload)
                : hrApi.createHistoryEvent(payload);
        },
        {
            successMessage: isEdit ? "تم تحديث الحدث بنجاح" : "تم تسجيل الحدث في السجل الوظيفي بنجاح",
            invalidateKeys: [["hr", "history", employeeId]],
            onSuccess: () => {
                onOpenChange(false);
                form.reset();
                onSuccess?.();
            }
        }
    );

    const eventTypes = [
        { label: "سجل وظيفي", value: HrEventTypeConstants.JOB_HISTORY },
        { label: "ترقية", value: HrEventTypeConstants.PROMOTION },
        { label: "تغيير مسمى وظيفي", value: HrEventTypeConstants.JOB_CHANGE },
        { label: "أخرى", value: HrEventTypeConstants.OTHER },
    ];

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEdit ? "تعديل حدث في السجل الوظيفي" : "إضافة حدث للسجل الوظيفي"}
            description={isEdit ? "تعديل تفاصيل الحدث المسجل مسبقاً" : "تسجيل ترقية أو تغيير في المسار الوظيفي للموظف"}
            isLoading={mutation.isPending}
            isDirty={form.formState.isDirty}
            onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
            resetOnOpen={true}
            reset={form.reset}
        >
            <div className="space-y-4 text-start">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField label="نوع الحدث" required error={form.formState.errors.eventType?.message}>
                        <NativeSelect
                            value={form.watch("eventType")}
                            options={eventTypes}
                            onChange={(v) => form.setValue("eventType", v, { shouldDirty: true })}
                        />
                    </FormField>

                    <FormField label="تاريخ الحدث" required error={form.formState.errors.eventDate?.message}>
                        <Controller
                            name="eventDate"
                            control={form.control}
                            render={({ field }) => (
                                <DateInput
                                    id={field.name}
                                    value={field.value || undefined}
                                    onChange={field.onChange}
                                />
                            )}
                        />
                    </FormField>
                </div>

                <FormField label="تفاصيل الحدث / البيان" required error={form.formState.errors.summary?.message}>
                    <Input 
                        {...form.register("summary")} 
                        placeholder="مثال: ترقية من رتبة ملازم إلى نقيب بموجب القرار الإداري..."
                    />
                </FormField>
            </div>
        </BaseFormDialog>
    );
}
