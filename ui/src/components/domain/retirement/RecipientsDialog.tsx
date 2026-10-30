import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle,Percent, User } from "lucide-react";
import { useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
<<<<<<< HEAD
import {
    BaseFormDialog,
    DateInput,
=======
import type { LookupDto } from "@/types/retirement";
import {
    BaseFormDialog,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    FileField,
    FormField,
    FormSection,
    Input,
<<<<<<< HEAD
    NativeSelect} from "@/components/ui";
import { todayYemen } from "@/lib/date";
import type { LookupDto } from "@/types/retirement";
=======
    DateInput,
    NativeSelect} from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Validation Schema ─────────────────────────────────

const recipientSchema = z.object({
    recipientType: z.enum(["retiree", "agent", "entitled", "guardian"] as const, {
        message: "نوع المستلم مطلوب",
    }),
    targetID: z.string().min(1, "يجب اختيار الشخص المستلم"),
<<<<<<< HEAD
    coveredEntitledID: z.string().optional(),
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    startDate: z.string().min(1, "تاريخ بدء الصرف مطلوب"),
    notes: z.string().trim().optional().or(z.literal("")),

    // حقول استكمال البيانات (للمستحقين الموقوفين)
    nationalID: z.string().optional(),
    birthDate: z.string().optional(),
    socialStatusID: z.number().optional(),
    healthStatusID: z.number().optional(),
    personalPhoto: z.instanceof(File).optional(),
    nationalIdImage: z.instanceof(File).optional(),
<<<<<<< HEAD
    sharePercent: z.string().optional().refine(value => !value || (Number(value) > 0 && Number(value) <= 100), "أدخل نسبة أكبر من صفر ولا تتجاوز ١٠٠٪"),
    entitledAmount: z.string().optional().refine(value => !value || Number(value) > 0, "أدخل مبلغاً أكبر من صفر"),
}).refine(values => !(values.sharePercent && values.entitledAmount), {
    message: "أدخل النسبة أو المبلغ، وليس كليهما",
    path: ["entitledAmount"],
}).refine(values => values.recipientType !== "guardian" || !!values.coveredEntitledID, {
    message: "اختر المستحق الذي ينوب عنه الوصي", path: ["coveredEntitledID"],
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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

<<<<<<< HEAD
            startDate: todayYemen(),
            notes: "",
            sharePercent: "",
            entitledAmount: "",
=======
            startDate: new Date().toISOString().split('T')[0],
            notes: "",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                                            setValue("sharePercent", "");
                                            setValue("entitledAmount", "");
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                    {recipientType === "guardian" && (
                        <FormField label="المستحق الذي ينوب عنه الوصي" required error={errors.coveredEntitledID?.message}>
                            <Controller name="coveredEntitledID" control={control} render={({ field }) => (
                                <NativeSelect value={field.value ?? ""} onChange={field.onChange}
                                    options={lookups.entitleds.map(person => ({ value: String(person.id), label: person.fullName }))}
                                    placeholder="اختر المستحق" />
                            )} />
                        </FormField>
                    )}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                </FormSection>

                {/* استكمال البيانات والوثائق للمستحقين الموقوفين */}
                {recipientType === "entitled" && selectedEntitled && (needsDataCompletion || needsPersonalPhoto || needsNationalId) && (
                    <div className="space-y-4 border border-rose-200 bg-rose-50/30 dark:border-rose-800 dark:bg-rose-950/10 rounded-2xl p-5">
                        <div className="flex items-center gap-2 text-rose-800 dark:text-rose-400 border-b border-rose-100 pb-3 mb-3">
                            <AlertTriangle className="w-5 h-5 flex-shrink-0" />
                            <div>
                                <p className="text-sm font-black">تشديد متطلبات الصرف</p>
<<<<<<< HEAD
                                <p className="text-[10px] sm:text-xs font-bold opacity-100">المستحق المختار موقوف الصرف. يجب استكمال بياناته ووثائقه أدناه لإتمام التعيين.</p>
=======
                                <p className="text-[10px] sm:text-xs font-bold opacity-80">المستحق المختار موقوف الصرف. يجب استكمال بياناته ووثائقه أدناه لإتمام التعيين.</p>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                    {recipientType === "entitled" && (
                        <div className="space-y-3 mb-4">
                            <p className="text-sm text-muted-foreground">أدخل نسبة هذا المستلم أو مبلغه الشهري. إذا تركتهما فارغين تُستخدم حصة المستحق المحفوظة.</p>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <FormField label="نسبة الاستحقاق (%)" error={errors.sharePercent?.message}>
                                    <Input type="number" min="0.01" max="100" step="0.01" inputMode="decimal"
                                        {...register("sharePercent", { onChange: event => {
                                            if (event.target.value) setValue("entitledAmount", "", { shouldDirty: true });
                                        } })} placeholder="مثال: ٢٥" />
                                </FormField>
                                <FormField label="المبلغ الشهري (ريال)" error={errors.entitledAmount?.message}>
                                    <Input type="number" min="0.01" step="0.01" inputMode="decimal"
                                        {...register("entitledAmount", { onChange: event => {
                                            if (event.target.value) setValue("sharePercent", "", { shouldDirty: true });
                                        } })} placeholder="أو أدخل المبلغ" />
                                </FormField>
                            </div>
                        </div>
                    )}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
