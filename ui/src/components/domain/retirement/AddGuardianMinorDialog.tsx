import { zodResolver } from "@hookform/resolvers/zod";
import { Controller,useForm } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { 
    BaseFormDialog, 
<<<<<<< HEAD
    DateInput,
    FormField, 
=======
    FormField, 
    DateInput,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    NativeSelect 
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    entitledID: z.string().min(1, "يجب اختيار المستحق القاصر"),
    startDate: z.string().min(1, "تاريخ بدء الوصاية مطلوب"),
});

type FormValues = z.infer<typeof schema>;

interface EntitledOption {
    entitledID: number;
    fullName: string;
}

interface AddGuardianMinorDialogProps {
    open: boolean;
    onClose: () => void;
    onSaved: () => void;
    guardianId: number;
    availableEntitleds: EntitledOption[];
}

/**
 * حوار إضافة مستحق إضافي تحت وصاية قائمة - Named Export (Constraint 8)
 */
export function AddGuardianMinorDialog({ 
    open, 
    onClose, 
    onSaved, 
    guardianId, 
    availableEntitleds 
}: AddGuardianMinorDialogProps) {
    
    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            entitledID: "",
            startDate: new Date().toISOString().split('T')[0],
        }
    });

    const { handleSubmit, control, reset, formState: { errors, isDirty } } = form;

    const mutation = useApiMutation(
        (data: { entitledID: number; startDate: string }) => 
            api.put(`/guardians/${guardianId}/add-minor`, data),
        {
            successMessage: "تم إضافة القاصر للوصي بنجاح",
            onSuccess: () => {
                onSaved();
                onClose();
                reset();
            }
        }
    );

    const onSubmit = (values: FormValues) => {
        mutation.mutate({
            entitledID: Number(values.entitledID),
            startDate: values.startDate + "T00:00:00"
        });
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={(o) => !o && onClose()}
            title="إضافة مستحق للوصي"
            description="ربط مستحق (قاصر) إضافي تحت وصاية هذا الشخص"
            size="full"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen
            reset={reset}
            showGlobalError
            formErrors={errors}
            submitLabel="إضافة"
        >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2 text-start" dir="rtl">
                <FormField label="اختر المستحق القاصر" required error={errors.entitledID?.message}>
                    <Controller
                        name="entitledID"
                        control={control}
                        render={({ field }) => (
                            <NativeSelect 
                                value={field.value}
                                onChange={field.onChange}
                                options={availableEntitleds.map(e => ({ value: e.entitledID.toString(), label: e.fullName }))}
                                placeholder="اختر من القائمة..."
                            />
                        )}
                    />
                </FormField>
                <FormField label="تاريخ بدء الوصاية" required error={errors.startDate?.message}>
                    <Controller
                        name="startDate"
                        control={control}
                        render={({ field }) => (
                            <DateInput
                                id={field.name}
                                value={field.value || undefined}
                                onChange={field.onChange}
                                className="font-mono text-start"
                            />
                        )}
                    />
                </FormField>
            </div>
        </BaseFormDialog>
    );
}
