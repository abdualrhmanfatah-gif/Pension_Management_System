import { zodResolver } from "@hookform/resolvers/zod";
import { Activity,Building2, MapPin, UserCheck } from "lucide-react";
import { useMemo } from "react";
import { Controller,useForm } from "react-hook-form";
import * as z from "zod";

import { branchesApi } from "@/api/branches";
import {
    BaseFormDialog,
    type BaseOverlayProps,
    FormField,
    FormSection,
    Input,
    NativeSelect,    NoteBox} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const branchSchema = z.object({
    branchName: z.string().trim().min(3, "اسم الفرع يجب أن يكون 3 أحرف على الأقل"),
    governorate: z.string().trim().min(2, "المحافظة مطلوبة"),
    address: z.string().trim().optional().nullable().or(z.literal("")),
    phone: z.string().trim().optional().nullable().or(z.literal("")),
    managerName: z.string().trim().optional().nullable().or(z.literal("")),
    managerPhone: z.string().trim().optional().nullable().or(z.literal("")),
    isActive: z.boolean(),
});

type BranchFormValues = z.infer<typeof branchSchema>;

// ─── Types ─────────────────────────────────────────────

interface BranchDto {
    pensionBranchID: number;
    branchName: string;
    governorate: string | null;
    address: string | null;
    phone: string | null;
    managerName: string | null;
    managerPhone: string | null;
    isActive: boolean;
}

interface Props extends BaseOverlayProps {
    onSaved: () => void;
    branch?: BranchDto | null;
}

// ─── Component ─────────────────────────────────────────

export function BranchDialog({
    open,
    onOpenChange,
    onSaved,
    branch
}: Props) {
    const isEditing = !!branch;

    const mutation = useApiMutation(
        (values: BranchFormValues) => isEditing 
            ? branchesApi.update(branch!.pensionBranchID, values)
            : branchesApi.create(values),
        {
            invalidateKeys: [["branches"], ["lookups"]],
            successMessage: isEditing ? "تم تحديث بيانات الفرع بنجاح" : "تم إضافة الفرع الجديد بنجاح",
            onSuccess: () => {
                onSaved();
                onOpenChange?.(false);
            }
        }
    );

    const form = useForm<BranchFormValues>({
        resolver: zodResolver(branchSchema),
        defaultValues: {
            branchName: "",
            governorate: "",
            address: "",
            phone: "",
            managerName: "",
            managerPhone: "",
            isActive: true,
        },
    });

    const { register, handleSubmit, reset, control, formState: { errors, isDirty } } = form;

    const onSubmit = (values: BranchFormValues) => {
        mutation.mutate(values);
    };

    const initialValues = useMemo(() => {
        if (!branch) return undefined;
        return {
            branchName: branch.branchName,
            governorate: branch.governorate || "",
            address: branch.address || "",
            phone: branch.phone || "",
            managerName: branch.managerName || "",
            managerPhone: branch.managerPhone || "",
            isActive: branch.isActive,
        };
    }, [branch]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEditing ? "تعديل بيانات الفرع" : "إضافة فرع جديد"}
            description="إدارة الفروع الإدارية والجغرافية للصندوق"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={isEditing ? "حفظ التعديلات" : "إضافة الفرع"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الأساسية للفرع" icon={Building2}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="اسم الفرع" required error={errors.branchName?.message}>
                            <Input placeholder="مثال: فرع عدن الرئيسي" {...register("branchName")} />
                        </FormField>

                        <FormField label="المحافظة / المدينة" required error={errors.governorate?.message}>
                            <Input placeholder="أدخل المحافظة" {...register("governorate")} icon={MapPin} />
                        </FormField>

                        <FormField label="هاتف التواصل" error={errors.phone?.message}>
                            <Input placeholder="7xxxxxxx" {...register("phone")} />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الإدارة والموقع" icon={UserCheck}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="اسم مدير الفرع" error={errors.managerName?.message}>
                            <Input placeholder="الاسم الكامل للمدير" {...register("managerName")} />
                        </FormField>

                        <FormField label="هاتف المدير" error={errors.managerPhone?.message}>
                            <Input placeholder="7xxxxxxx" {...register("managerPhone")} />
                        </FormField>

                        <div className="md:col-span-2">
                            <FormField label="العنوان التفصيلي" error={errors.address?.message}>
                                <Input placeholder="الشارع، الحي، المعالم القريبة" {...register("address")} />
                            </FormField>
                        </div>
                    </div>
                </FormSection>

                <FormSection title="الحالة التشغيلية" icon={Activity}>
                    <div className="max-w-md">
                        <FormField label="حالة الفرع" required error={errors.isActive?.message}>
                            <Controller
                                name="isActive"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "نشط (يعمل)", value: "true" },
                                            { label: "غير نشط (متوقف)", value: "false" }
                                        ]}
                                        value={String(field.value)}
                                        onChange={(val) => field.onChange(val === "true")}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="warning">
                    تنبيه: تعطيل الفرع قد يؤثر على إمكانية الوصول للبيانات المرتبطة به من قبل المستخدمين المحليين في هذا النطاق.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
