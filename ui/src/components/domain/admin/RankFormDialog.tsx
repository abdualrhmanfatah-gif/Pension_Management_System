import { zodResolver } from "@hookform/resolvers/zod";
import { Award, DollarSign, Hash } from "lucide-react";
import { useEffect } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { ranksApi } from "@/api/ranks";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NoteBox
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const rankSchema = z.object({
    rankName: z.string().trim().min(2, "اسم الرتبة مطلوب"),
    rankLevel: z.coerce.number().int().min(1, "مستوى الرتبة مطلوب"),
    baseSalary: z.coerce.number().min(0, "المعاش الأساسي يجب أن يكون 0 أو أكثر"),
});

type RankFormValues = z.infer<typeof rankSchema>;

interface RankDto {
    rankID: number;
    rankName: string;
    rankLevel: number;
    baseSalary: number;
}

interface Props {
    open?: boolean;
    onOpenChange?: (open: boolean) => void;
    onSaved?: () => void;
    initialData?: RankDto | null;
    /** Legacy alias for initialData */
    rank?: RankDto | null;
}

/**
 * Standardized RankFormDialog.
 * Refactored to strictly use BaseFormDialog and FormSection.
 */
export function RankFormDialog({
    open,
    onOpenChange,
    onSaved,
    initialData,
    rank
}: Props) {
    const effectiveInitialData = initialData || rank;
    const isEditing = !!effectiveInitialData;

    const mutation = useApiMutation(
        (values: RankFormValues) => isEditing
            ? ranksApi.update(effectiveInitialData!.rankID, values)
            : ranksApi.create(values),
        {
            invalidateKeys: [["ranks"], ["lookups"]],
            successMessage: isEditing ? "تم تحديث الرتبة بنجاح" : "تم إضافة الرتبة بنجاح",
            onSuccessCallback: () => {
                if (onSaved) onSaved();
                if (onOpenChange) onOpenChange(false);
            }
        }
    );

    const form = useForm<RankFormValues>({
        resolver: zodResolver(rankSchema) as Resolver<z.infer<typeof rankSchema>>,
        defaultValues: { rankName: "", rankLevel: 1, baseSalary: 0 },
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty } } = form;

    const onSubmit = (values: RankFormValues) => {
        mutation.mutate(values);
    };

    const initialValues = effectiveInitialData ? {
        rankName: effectiveInitialData.rankName,
        rankLevel: effectiveInitialData.rankLevel,
        baseSalary: effectiveInitialData.baseSalary,
    } : { rankName: "", rankLevel: 1, baseSalary: 0 };

    useEffect(() => {
        if (open && effectiveInitialData) {
            reset({
                rankName: effectiveInitialData.rankName,
                rankLevel: effectiveInitialData.rankLevel,
                baseSalary: effectiveInitialData.baseSalary,
            });
        } else if (open && !effectiveInitialData) {
            reset({ rankName: "", rankLevel: 1, baseSalary: 0 });
        }
    }, [open, effectiveInitialData, reset]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEditing ? "تعديل رتبة" : "إضافة رتبة جديدة"}
            description="إدارة الرتب العسكرية والدرجات الوظيفية"
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={isEditing ? "حفظ التعديلات" : "إضافة الرتبة"}
        >
            <div className="space-y-4">
                <FormSection title="بيانات الرتبة والدرجة" icon={Award}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="md:col-span-2">
                            <FormField label="اسم الرتبة / الدرجة" error={errors.rankName?.message} required>
                                <Input {...register("rankName")} placeholder="مثال: عقيد، ملازم أول..." />
                            </FormField>
                        </div>

                        <FormField label="مستوى الأولوية (المستوى)" error={errors.rankLevel?.message} required>
                            <Input {...register("rankLevel")} type="number" min={1} icon={Hash} />
                        </FormField>

                        <FormField label="المعاش الأساسي المعتمد" error={errors.baseSalary?.message} required>
                            <Input {...register("baseSalary")} type="number" min={0} icon={DollarSign} className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    يتم استخدام المعاش الأساسي كمرجع عند احتساب المعاشات التقاعدية المرتبطة بهذه الرتبة.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
