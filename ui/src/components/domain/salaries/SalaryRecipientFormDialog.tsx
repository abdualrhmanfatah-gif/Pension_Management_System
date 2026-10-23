import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle } from "lucide-react";
import { useMemo } from "react";
import { Controller, type Resolver,useForm, useWatch } from "react-hook-form";
import * as z from "zod";

import { retirementApi } from "@/api/retirement";
import { type CreateSalaryRecipientDto,salaryRecipientApi } from "@/api/salary-recipients";
import { EntitlementAmountField } from "@/components/domain/retirement/EntitlementAmountField";
import {
    BaseFormDialog,
    DateInput,
    FileField,
    FormField,
    Input,
    NativeSelect} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
import { useAgentsQuery, useEntitledPersonsQuery, useGuardiansQuery } from "@/hooks/queries";
import { todayYemen } from "@/lib/date";
import type { LookupDto } from "@/types/retirement";
import type { AgentSummary, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";
import { type CreateSalaryRecipientForm,createSalaryRecipientSchema } from "@/types/salary-recipients";

interface SalaryRecipientFormDialogProps {
    open: boolean;
    onClose: () => void;
    onSaved: () => void;
    fileId: number;
    retireeId?: number;
    retireeName?: string;
}

/**
 * حوار إضافة مستلم المعاش جديد
 */
export function SalaryRecipientFormDialog({
    open,
    onClose,
    onSaved,
    fileId,
    retireeId,
    retireeName,
}: SalaryRecipientFormDialogProps) {
    const form = useForm<CreateSalaryRecipientForm>({
        resolver: zodResolver(createSalaryRecipientSchema) as Resolver<z.infer<typeof createSalaryRecipientSchema>>,
        defaultValues: {
            recipientType: "متقاعد",
            retireeID: retireeId,
            startDate: todayYemen(),
        }
    });

    const { control, register, handleSubmit, setValue, formState: { errors, isDirty } } = form;
    const recipientType = useWatch({ control, name: "recipientType" });
    const selectedEntitledId = useWatch({ control, name: "entitledID" });

    const { data: agentsData = [] } = useAgentsQuery(fileId);
    const { data: entitledsData = [] } = useEntitledPersonsQuery(fileId);
    const { data: guardiansData = [] } = useGuardiansQuery(fileId);

    // تصفية الكيانات النشطة:
    // بالنسبة للمستحقين -> نظهر الجميع (طالما غير متوفى) للسماح باستكمال بياناتهم عند التعيين كمستلم
    const agents = useMemo(() =>
        (agentsData as AgentSummary[]).filter(a => a.isActive && !a.isDeleted),
        [agentsData]);

    const entitleds = useMemo(() =>
        (entitledsData as EntitledPersonSummary[]).filter(e => !e.isDeceased && !e.isDeleted),
        [entitledsData]);

    const guardians = useMemo(() =>
        (guardiansData as GuardianSummary[]).filter(g => g.isActive && !g.isDeleted),
        [guardiansData]);

    // جلب بيانات الجداول المرجعية التكميلية
    const { data: socialStatuses = [] } = useQuery({
        queryKey: queryKeys.lookups.socialStatuses,
        queryFn: () => retirementApi.getSocialStatuses(),
        enabled: open && recipientType === "مستحق"
    });

    const { data: healthStatuses = [] } = useQuery({
        queryKey: queryKeys.lookups.healthStatuses,
        queryFn: () => retirementApi.getHealthStatuses(),
        enabled: open && recipientType === "مستحق"
    });

    // تحديد البيانات والوثائق الناقصة للمستحق المختار
    const selectedEntitled = useMemo(() => {
        if (recipientType !== "مستحق" || !selectedEntitledId) return null;
        return entitleds.find(e => e.entitledID === selectedEntitledId) ?? null;
    }, [recipientType, selectedEntitledId, entitleds]);

    const needsDataCompletion = selectedEntitled != null && !selectedEntitled.isEligible;
    const needsPersonalPhoto = selectedEntitled != null && !selectedEntitled.hasPersonalPhoto;
    const needsNationalId = selectedEntitled != null && !selectedEntitled.hasNationalIdDocument;

    const createMutation = useApiMutation(
        (data: CreateSalaryRecipientDto) => salaryRecipientApi.create(data),
        {
            successMessage: "تم إضافة مستلم المعاش بنجاح وتفعيل المستحق",
            onSuccessCallback: () => {
                onSaved();
                onClose();
            }
        }
    );

    const onSubmit = (values: CreateSalaryRecipientForm) => {
        const dto: CreateSalaryRecipientDto = {
            fileID: fileId,
            startDate: values.startDate,
            notes: values.notes,
            retireeID: values.recipientType === "متقاعد" ? retireeId : undefined,
            agentID: values.recipientType === "وكيل" ? values.agentID : undefined,
            entitledID: values.recipientType === "مستحق" || values.recipientType === "وصي" ? values.entitledID : undefined,
            guardianID: values.recipientType === "وصي" ? values.guardianID : undefined,
            personalPhoto: values.personalPhoto,
            nationalIdImage: values.nationalIdImage,
            // بيانات تكميلية
            nationalID: values.nationalID,
            birthDate: values.birthDate,
            socialStatusID: values.socialStatusID,
            healthStatusID: values.healthStatusID,
            entitledAmount: values.recipientType === "مستحق" && values.entitledAmount ? Number(values.entitledAmount) : undefined,
        };
        createMutation.mutate(dto);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={(o) => !o && onClose()}
            title="إضافة مستلم المعاش"
            size="lg"
            isLoading={createMutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            reset={form.reset}
            resetOnOpen
            initialValues={{
                recipientType: "متقاعد",
                retireeID: retireeId,
                startDate: new Date().toISOString().split("T")[0],
            }}
        >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 py-2 text-start">
                <FormField label="نوع المستلم" required error={errors.recipientType?.message}>
                    <Controller
                        name="recipientType"
                        control={control}
                        render={({ field }) => (
                            <NativeSelect
                                value={field.value}
                                onChange={(val) => {
                                    field.onChange(val);
                                    // Reset other IDs and completion data
                                    setValue("retireeID", val === "متقاعد" ? retireeId : undefined);
                                    setValue("agentID", undefined);
                                    setValue("entitledID", undefined);
                                    setValue("guardianID", undefined);
                                    setValue("personalPhoto", undefined);
                                    setValue("nationalIdImage", undefined);
                                    setValue("nationalID", undefined);
                                    setValue("birthDate", undefined);
                                    setValue("socialStatusID", undefined);
                                    setValue("healthStatusID", undefined);
                                    setValue("entitledAmount", undefined);
                                }}
                                options={[
                                    { value: "متقاعد", label: "متقاعد" },
                                    { value: "وكيل", label: "وكيل" },
                                    { value: "مستحق", label: "مستحق" },
                                    { value: "وصي", label: "وصي" },
                                ]}
                            />
                        )}
                    />
                </FormField>

                {recipientType === "متقاعد" && (
                    <FormField label="اسم المتقاعد" required>
                        <Input value={retireeName} disabled className="bg-muted" />
                    </FormField>
                )}

                {recipientType === "وكيل" && (
                    <FormField label="اختر الوكيل" required error={errors.agentID?.message}>
                        <Controller
                            name="agentID"
                            control={control}
                            render={({ field }) => (
                                <NativeSelect
                                    value={field.value?.toString() || ""}
                                    onChange={(val) => field.onChange(parseInt(val))}
                                    options={agents.map((a) => ({ value: a.agentID.toString(), label: a.fullName }))}
                                    placeholder="اختر من وكلاء الملف"
                                />
                            )}
                        />
                    </FormField>
                )}

                {recipientType === "مستحق" && (
                    <FormField label="اختر المستحق" required error={errors.entitledID?.message}>
                        <Controller
                            name="entitledID"
                            control={control}
                            render={({ field }) => (
                                <NativeSelect
                                    value={field.value?.toString() || ""}
                                    onChange={(val) => {
                                        field.onChange(parseInt(val));
                                        setValue("entitledAmount", undefined);
                                        // Reset file and data fields when changing entitled person
                                        setValue("personalPhoto", undefined);
                                        setValue("nationalIdImage", undefined);
                                        setValue("nationalID", undefined);
                                        setValue("birthDate", undefined);
                                        setValue("socialStatusID", undefined);
                                        setValue("healthStatusID", undefined);
                                    }}
                                    options={entitleds.map((e) => ({
                                        value: e.entitledID.toString(),
                                        label: `${e.fullName} ${!e.isEligible ? "(موقوف - يتطلب استكمال)" : ""}`
                                    }))}
                                    placeholder="اختر من ورثة المتقاعد"
                                />
                            )}
                        />
                    </FormField>
                )}

                {recipientType === "وصي" && (
                    <FormField label="اختر الوصي" required error={errors.guardianID?.message}>
                        <Controller
                            name="guardianID"
                            control={control}
                            render={({ field }) => (
                                <NativeSelect
                                    value={field.value?.toString() || ""}
                                    onChange={(val) => field.onChange(parseInt(val))}
                                    options={guardians.map((g) => ({ value: g.guardianID.toString(), label: g.fullName }))}
                                    placeholder="اختر من الأوصياء المسجلين"
                                />
                            )}
                        />
                    </FormField>
                )}

                {recipientType === "وصي" && (
                    <FormField label="المستحق الذي ينوب عنه الوصي" required error={errors.entitledID?.message}>
                        <Controller name="entitledID" control={control} render={({ field }) => (
                            <NativeSelect value={field.value ?? ""} onChange={value => field.onChange(Number(value))}
                                options={entitleds.map(person => ({ value: String(person.entitledID), label: person.fullName }))}
                                placeholder="اختر المستحق" />
                        )} />
                    </FormField>
                )}
                <FormField label="تاريخ البدء" required error={errors.startDate?.message as string}>
                    <Controller
                        name="startDate"
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

                {recipientType === "مستحق" && (
                    <div className="md:col-span-2 space-y-3">
                        <Controller name="entitledAmount" control={control} render={({ field, fieldState }) => (
                            <EntitlementAmountField retireeId={retireeId} value={field.value} onChange={field.onChange}
                                onBlur={field.onBlur} inputRef={field.ref} savedSharePercent={selectedEntitled?.sharePercent} error={fieldState.error?.message} />
                        )} />
                    </div>
                )}

                {/* استكمال البيانات والوثائق للمستحقين الموقوفين */}
                {recipientType === "مستحق" && selectedEntitled && (needsDataCompletion || needsPersonalPhoto || needsNationalId) && (
                    <div className="md:col-span-2 space-y-4 border border-rose-200 bg-rose-50/30 dark:border-rose-800 dark:bg-rose-950/10 rounded-2xl p-5 mt-2">
                        <div className="flex items-center gap-2 text-rose-800 dark:text-rose-400 border-b border-rose-100 pb-3 mb-3">
                            <AlertTriangle className="w-5 h-5 flex-shrink-0" />
                            <div>
                                <p className="text-sm font-black">تشديد متطلبات الصرف</p>
                                <p className="text-[10px] sm:text-xs font-bold opacity-100">المستحق المختار موقوف الصرف حالياً. يجب استكمال بياناته ووثائقه الإلزامية أدناه لإتمام التعيين.</p>
                            </div>
                        </div>

                        {/* قسم البيانات الأساسية */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <FormField label="الرقم الوطني" required error={errors.nationalID?.message}>
                                <Input {...register("nationalID")} placeholder="الرقم الوطني المكون من 11 رقم" />
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

                        {/* قسم الوثائق */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border-t border-rose-100 pt-4">
                            <div className="space-y-1">
                                <label className="label-field">صورة شخصية 4×6</label>
                                <FileField
                                    name="personalPhoto"
                                    control={control}
                                    label="اضغط للرفع"
                                    size="sm"
                                    accept="image/jpeg,image/png,image/jpg"
                                />
                            </div>

                            <div className="space-y-1">
                                <label className="label-field">صورة الهوية الوطنية</label>
                                <FileField
                                    name="nationalIdImage"
                                    control={control}
                                    label="اضغط للرفع"
                                    size="sm"
                                    accept="image/jpeg,image/png,image/jpg,application/pdf"
                                />
                            </div>
                        </div>
                    </div>
                )}

                <div className="md:col-span-2">
                    <FormField label="ملاحظات" error={errors.notes?.message}>
                        <Input {...register("notes")} placeholder="أي ملاحظات إضافية..." />
                    </FormField>
                </div>
            </div>
        </BaseFormDialog>
    );
}
