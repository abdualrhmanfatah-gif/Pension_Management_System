import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle,Percent, User } from "lucide-react";
import { useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import {
    BaseFormDialog,
    DateInput,
    FileField,
    FormField,
    FormSection,
    Input,
    NativeSelect} from "@/components/ui";
import type { LookupDto } from "@/types/retirement";

// ─── Validation Schema ─────────────────────────────────

const recipientSchema = z.object({
    recipientType: z.enum(["retiree", "agent", "entitled", "guardian"] as const, {
        message: "نوع المستلم مطلوب",
    }),
    targetID: z.string().min(1, "يجب اختيار الشخص المستلم"),

    startDate: z.string().min(1, "تاريخ بدء الصرف مطلوب"),
    notes: z.string().trim().optional().or(z.literal("")),

    // حقول استكمال البيانات (للمستحقين الموقوفين)
    nationalID: z.string().optional(),
    birthDate: z.string().optional(),
    socialStatusID: z.number().optional(),
    healthStatusID: z.number().optional(),
    personalPhoto: z.instanceof(File).optional(),
    nationalIdImage: z.instanceof(File).optional(),
});

export type RecipientFormValues = z.infer<typeof recipientSchema>;

interface RecipientOption {
    id: number;
    fullName: string;
    isEligible?: boolean;
    hasPersonalPhoto?: boolean;
    hasNationalIdDocument?: boolean;
}

interface RecipientsDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSubmit: (values: RecipientFormValues) => void;
    isLoading?: boolean;

    retireeId?: number;
    retireeName?: string;
    lookups: {
        agents: RecipientOption[];
        entitleds: RecipientOption[];
        guardians: RecipientOption[];
    };
}

/**
 * حوار إضافة مستلم المعاش جديد - تم التحديث لدعم استكمال البيانات والوثائق
 */
export function RecipientsDialog({
    open,
    onOpenChange,
    onSubmit,
    isLoading,

    retireeId,
    retireeName,
    lookups
}: RecipientsDialogProps) {
    const form = useForm<RecipientFormValues>({
        resolver: zodResolver(recipientSchema),
        defaultValues: {
            recipientType: "retiree",
            targetID: retireeId ? String(retireeId) : "",

            startDate: new Date().toISOString().split('T')[0],
            notes: "",
        }
    });

    const { control, register, setValue, formState: { errors, isDirty } } = form;
    const recipientType = useWatch({ control, name: "recipientType" });
    const targetID = useWatch({ control, name: "targetID" });

    // جلب بيانات الجداول المرجعية التكميلية
    const { data: socialStatuses = [] } = useQuery({
        queryKey: ["social-statuses"],
        queryFn: () => retirementApi.getSocialStatuses(),
        enabled: open && recipientType === "entitled"
    });

    const { data: healthStatuses = [] } = useQuery({
        queryKey: ["health-statuses"],
        queryFn: () => retirementApi.getHealthStatuses(),
        enabled: open && recipientType === "entitled"
    });

    // فحص متطلبات الاستكمال للمستحق المختار
    const selectedEntitled = useMemo(() => {
        if (recipientType !== "entitled" || !targetID) return null;
        return lookups.entitleds.find(e => String(e.id) === targetID) || null;
    }, [recipientType, targetID, lookups.entitleds]);

    const needsDataCompletion = selectedEntitled != null && selectedEntitled.isEligible === false;
    const needsPersonalPhoto = selectedEntitled != null && !selectedEntitled.hasPersonalPhoto;
    const needsNationalId = selectedEntitled != null && !selectedEntitled.hasNationalIdDocument;

    const handleFormSubmit = (values: RecipientFormValues) => {
        onSubmit(values);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إضافة مستلم المعاش جديد"
            description="تحديد الشخص المخول باستلام المستحقات المالية"
            size="full"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="اعتماد المستلم"
        >
            <div className="space-y-4 text-start">
                <FormSection title="تصنيف المستلم" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="نوع المستلم" required error={errors.recipientType?.message}>
                            <Controller
                                name="recipientType"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { value: "retiree", label: "المتقاعد نفسه" },
                                            { value: "entitled", label: "أحد المستحقين (الورثة)" },
                                            { value: "agent", label: "وكيل شرعي" },
                                            { value: "guardian", label: "وصي" },
                                        ]}
                                        value={field.value}
                                        onChange={(val) => {
                                            field.onChange(val);
                                            setValue("targetID", val === "retiree" && retireeId ? String(retireeId) : "");
                                            // Reset completion fields
                                            setValue("nationalID", undefined);
                                            setValue("birthDate", undefined);
                                            setValue("socialStatusID", undefined);
                                            setValue("healthStatusID", undefined);
                                        }}
                                        autoFocus
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="الشخص المختار" required error={errors.targetID?.message}>
                            <Controller
                                name="targetID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={
                                            recipientType === "retiree" ? (retireeId ? [{ value: String(retireeId), label: retireeName || "صاحب الملف" }] : []) :
                                                recipientType === "agent" ? lookups.agents.map(a => ({ value: String(a.id), label: a.fullName })) :
                                                    recipientType === "entitled" ? lookups.entitleds.map(e => ({
                                                        value: String(e.id),
                                                        label: `${e.fullName} ${e.isEligible === false ? "(موقوف - يتطلب استكمال)" : ""}`
                                                    })) :
                                                        recipientType === "guardian" ? lookups.guardians.map(g => ({ value: String(g.id), label: g.fullName })) : []
                                        }
                                        value={field.value}
                                        onChange={(val) => {
                                            field.onChange(val);
                                            // Reset data fields
                                            setValue("nationalID", undefined);
                                            setValue("birthDate", undefined);
                                        }}
                                        disabled={!recipientType}
                                        placeholder="ابحث عن الاسم..."
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                {/* استكمال البيانات والوثائق للمستحقين الموقوفين */}
                {recipientType === "entitled" && selectedEntitled && (needsDataCompletion || needsPersonalPhoto || needsNationalId) && (
                    <div className="space-y-4 border border-rose-200 bg-rose-50/30 dark:border-rose-800 dark:bg-rose-950/10 rounded-2xl p-5">
                        <div className="flex items-center gap-2 text-rose-800 dark:text-rose-400 border-b border-rose-100 pb-3 mb-3">
                            <AlertTriangle className="w-5 h-5 flex-shrink-0" />
                            <div>
                                <p className="text-sm font-black">تشديد متطلبات الصرف</p>
                                <p className="text-[10px] sm:text-xs font-bold opacity-80">المستحق المختار موقوف الصرف. يجب استكمال بياناته ووثائقه أدناه لإتمام التعيين.</p>
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <FormField label="الرقم الوطني" required error={errors.nationalID?.message}>
                                <Input {...register("nationalID")} placeholder="11 رقم" />
                            </FormField>
                            <FormField label="تاريخ الميلاد" required error={errors.birthDate?.message}>
                                <Controller
                                    name="birthDate"
                                    control={control}
                                    render={({ field }) => (
                                        <DateInput
                                            id={field.name}
                                            value={field.value || undefined}
                                            onChange={field.onChange}
                                        />
                                    )}
                                />
                            </FormField>
                            <FormField label="الحالة الاجتماعية" required error={errors.socialStatusID?.message}>
                                <Controller
                                    name="socialStatusID"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            value={field.value?.toString() || ""}
                                            onChange={(val) => field.onChange(parseInt(val))}
                                            options={socialStatuses.map((s: LookupDto) => ({ value: s.id.toString(), label: s.name }))}
                                        />
                                    )}
                                />
                            </FormField>
                            <FormField label="الحالة الصحية" required error={errors.healthStatusID?.message}>
                                <Controller
                                    name="healthStatusID"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            value={field.value?.toString() || ""}
                                            onChange={(val) => field.onChange(parseInt(val))}
                                            options={healthStatuses.map((h: LookupDto) => ({ value: h.id.toString(), label: h.name }))}
                                        />
                                    )}
                                />
                            </FormField>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border-t border-rose-100 pt-4">
                            <FileField
                                name="personalPhoto"
                                control={control}
                                label="صورة شخصية 4×6"
                                required={needsPersonalPhoto}
                                size="sm"
                            />
                            <FileField
                                name="nationalIdImage"
                                control={control}
                                label="صورة الهوية الوطنية"
                                required={needsNationalId}
                                size="sm"
                            />
                        </div>
                    </div>
                )}

                <FormSection title="تفاصيل الاستحقاق المالي" icon={Percent}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="تاريخ بدء الاستلام" required error={errors.startDate?.message}>
                            <Controller
                                name="startDate"
                                control={control}
                                render={({ field }) => (
                                    <DateInput
                                        id={field.name}
                                        value={field.value || undefined}
                                        onChange={field.onChange}
                                        className="font-mono"
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="ملاحظات إضافية" error={errors.notes?.message}>
                            <Input
                                {...register("notes")}
                                placeholder="أدخل أي ملاحظات..."
                            />
                        </FormField>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
