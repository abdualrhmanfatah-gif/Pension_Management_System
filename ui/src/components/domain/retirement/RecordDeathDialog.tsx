import { zodResolver } from "@hookform/resolvers/zod";
import { Calendar } from "lucide-react";
import { useForm, Controller } from "react-hook-form";
import { z } from "zod";

import { 
    BaseFormDialog, 
    FormField, 
    FormSection,
    DateInput,
    NoteBox
} from "@/components/ui";

// ─── Validation Schema ─────────────────────────────────

const recordDeathSchema = z.object({
    deathDate: z.string().min(1, "تاريخ الوفاة مطلوب"),
});

export type RecordDeathFormValues = z.infer<typeof recordDeathSchema>;

interface RecordDeathDialogProps {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
    onSubmit: (values: RecordDeathFormValues) => void;
    isLoading?: boolean;
    retireeName: string;
}

/**
 * حوار تسجيل الوفاة للمتقاعد
 */
export function RecordDeathDialog({ 
    open, 
    isOpen,
    onOpenChange,
    onClose,
    onSubmit, 
    isLoading,
    retireeName
}: RecordDeathDialogProps) {
    const form = useForm<RecordDeathFormValues>({
        resolver: zodResolver(recordDeathSchema),
        defaultValues: {
            deathDate: new Date().toISOString().split('T')[0],
        }
    });

    const {  formState: { errors, isDirty } } = form;

    return (
        <BaseFormDialog
            open={open}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title="تسجيل وفاة المتقاعد"
            description={`أنت بصدد تسجيل حالة وفاة للمتقاعد: ${retireeName}`}
            size="md"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد وتسجيل الوفاة"
            className="border-t-4 border-t-destructive"
        >
            <div className="space-y-4 text-start">
                <FormSection title="تفاصيل الوفاة" icon={Calendar}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="تاريخ الوفاة" required error={errors.deathDate?.message}>
                            <Controller
                                name="deathDate"
                                control={form.control}
                                render={({ field }) => (
                                    <DateInput
                                        id={field.name}
                                        value={field.value || undefined}
                                        onChange={field.onChange}
                                        className="font-mono font-bold"
                                        max={new Date().toISOString().split('T')[0]}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="warning" title="الإجراءات التلقائية عند التسجيل">
                    <ul className="list-disc list-inside space-y-1 text-sm mt-2">
                        <li>سيتم إيقاف الملف مؤقتاً لاستكمال إجراءات الورثة.</li>
                        <li>سيتم إيقاف المتقاعد كمستلم صرف وإلغاء دفتر الصرف النشط.</li>
                        <li>ستُسجَّل وثيقة (حصر الورثة) و(شهادة الوفاة) كنواقص مطلوبة.</li>
                        <li>سيتم فحص بيانات الورثة — المكتملون يُفعَّلون تلقائياً والناقصون يبقون موقوفين.</li>
                    </ul>
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
