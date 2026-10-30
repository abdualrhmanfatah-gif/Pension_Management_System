import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calendar, DollarSign, Users } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi, type LeaseContractDto } from "@/api/investment";
import {
    BaseFormDialog,
<<<<<<< HEAD
    DateInput,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    FileField,
    FormField,
    FormSection,
    Input,
<<<<<<< HEAD
=======
    DateInput,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const contractSchema = z.object({
    unitID: z.number().min(1, "يجب تحديد الوحدة المؤجرة"),
    tenantID: z.number().min(1, "يجب تحديد المستأجر"),
    startDate: z.string().min(1, "تاريخ بداية العقد مطلوب"),
    endDate: z.string().min(1, "تاريخ نهاية العقد مطلوب"),
    totalContractValue: z.number().positive("قيمة العقد يجب أن تكون رقماً موجباً"),
    paymentFrequency: z.string().min(1, "دورية السداد مطلوبة"),
    notes: z.string().trim().optional(),
    contractFile: z.instanceof(File).optional().nullable(),
    generateSchedules: z.boolean(),
});

export type ContractFormValues = z.infer<typeof contractSchema>;

// ─── Props ─────────────────────────────────────────────

interface ContractFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<LeaseContractDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
    propertyId?: number;
}

// ─── Component ─────────────────────────────────────────

export function ContractDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess,
    propertyId
}: ContractFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.contractID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["contract", editId],
        queryFn: () => investmentApi.getContract(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as LeaseContractDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: allUnits = [], isLoading: unitsLoading } = useQuery({
        queryKey: ["all-units"],
        queryFn: () => investmentApi.getAllUnits(),
        enabled: open,
    });

    const { data: allProperties = [], isLoading: propertiesLoading } = useQuery({
        queryKey: ["properties"],
        queryFn: () => investmentApi.getAllProperties(),
        enabled: open,
    });

    const { data: tenantsRes = [], isLoading: tenantsLoading } = useQuery({
        queryKey: ["tenants"],
        queryFn: () => investmentApi.getTenants(),
        enabled: open,
    });

    // فلتر العقار المحدد محلياً
    const [selectedPropertyId, setSelectedPropertyId] = useState<string>(String(propertyId || ""));

    useEffect(() => {
        if (propertyId) setSelectedPropertyId(String(propertyId));
    }, [propertyId]);

    const propertiesOptions = allProperties.map(p => ({
        label: p.propertyName,
        value: String(p.propertyID)
    }));

    const unitsOptions = (() => {
        if (!allUnits) return [];
        const filtered = selectedPropertyId
            ? allUnits.filter(u => u.propertyID === Number(selectedPropertyId))
            : allUnits;
        return filtered.map(u => ({
            label: u.propertyName ? `${u.propertyName} - ${u.unitNumber}` : u.unitNumber,
            value: String(u.unitID)
        }));
    })();

    const tenantsOptions = tenantsRes.map(t => ({
        label: t.fullName,
        value: String(t.tenantID)
    }));

    // ─── Form ───────────────────────────────────────────
    const form = useForm<ContractFormValues>({
        resolver: zodResolver(contractSchema),
        defaultValues: useMemo(() => ({
            unitID: 0,
            tenantID: 0,
            startDate: new Date().toISOString().split('T')[0],
            endDate: "",
            totalContractValue: 0,
            paymentFrequency: "Monthly",
            notes: "",
            contractFile: null,
            generateSchedules: true,
        }), []),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;

    // ─── Initial values for BaseFormDialog reset ────────
    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            unitID: data.unitID || 0,
            tenantID: data.tenantID || 0,
            startDate: data.startDate?.split('T')[0] || "",
            endDate: data.endDate?.split('T')[0] || "",
            totalContractValue: data.totalContractValue || 0,
            paymentFrequency: data.paymentFrequency || "Monthly",
            notes: "",
            contractFile: null,
            generateSchedules: true,
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: ContractFormValues) => {
            const { contractFile, ...dto } = values;
            let res: number;
            if (mode === "create") {
                res = await investmentApi.createContract(dto);
            } else {
                res = await investmentApi.updateContract(effectiveData!.contractID!, dto) as unknown as number;
            }
            if (res && contractFile instanceof File) {
                await investmentApi.uploadSignedContract(res, contractFile);
            }
            return res;
        },
        {
            successMessage: mode === "create" ? "تم إنشاء العقد بنجاح" : "تم تحديث العقد بنجاح",
            invalidateKeys: [
                ["contracts"],
                ["property-contracts", propertyId],
                ["schedules"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: ContractFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إنشاء عقد إيجار جديد" : "تعديل عقد إيجار"}
            description="إدارة عقود الإيجار للوحدات العقارية وربطها بالمستأجرين"
            size="2xl"
            isLoading={mutation.isPending || unitsLoading || propertiesLoading || tenantsLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إنشاء العقد" : "حفظ التعديلات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="أطراف العقد" icon={Users}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="العقار">
                            <NativeSelect
                                options={propertiesOptions}
                                value={selectedPropertyId}
                                onChange={(v) => {
                                    setSelectedPropertyId(v);
                                }}
                                placeholder="كل العقارات..."
                            />
                        </FormField>
                        <FormField label="الوحدة المؤجرة" required error={errors.unitID?.message}>
                            <Controller
                                name="unitID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={unitsOptions}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر الوحدة..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="المستأجر" required error={errors.tenantID?.message}>
                            <Controller
                                name="tenantID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={tenantsOptions}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر المستأجر..."
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الشروط المالية والجدولة" icon={DollarSign}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="قيمة العقد الإجمالية" required error={errors.totalContractValue?.message}>
                            <Input type="number" {...register("totalContractValue", { valueAsNumber: true })} placeholder="0.00" className="font-mono" />
                        </FormField>
                        <FormField label="دورية السداد" required error={errors.paymentFrequency?.message}>
                            <Controller
                                name="paymentFrequency"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "شهري", value: "Monthly" },
                                            { label: "ربع سنوي", value: "Quarterly" },
                                            { label: "نصف سنوي", value: "SemiAnnually" },
                                            { label: "سنوي", value: "Annually" },
                                        ]}
                                        value={field.value}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>
                    </div>

                    {mode === "create" && (
                        <div className="mt-4 p-3 bg-muted/30 rounded-lg flex items-center gap-3">
                            <Controller
                                name="generateSchedules"
                                control={control}
                                render={({ field }) => (
                                    <input
                                        type="checkbox"
                                        id="generateSchedules"
                                        checked={field.value}
                                        onChange={field.onChange}
                                        className="w-4 h-4 rounded border-input text-primary focus:ring-primary"
                                    />
                                )}
                            />
                            <label htmlFor="generateSchedules" className="text-sm font-medium cursor-pointer select-none">
                                توليد جدول الدفعات تلقائياً بناءً على دورية السداد وقيمة العقد
                            </label>
                        </div>
                    )}
                </FormSection>

                <FormSection title="المدة الزمنية ومرفق العقد" icon={Calendar}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        <FormField label="تاريخ بداية العقد" required error={errors.startDate?.message}>
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
                        <FormField label="تاريخ نهاية العقد" required error={errors.endDate?.message}>
                            <Controller
                                name="endDate"
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
                        <div className="lg:col-span-2">
                            <FormField label="ملاحظات العقد (اختياري)" error={errors.notes?.message}>
                                <Textarea {...register("notes")} placeholder="بنود خاصة أو ملاحظات..." rows={1} />
                            </FormField>
                        </div>
                    </div>
                    <div className="grid grid-cols-1 gap-4 mt-4 text-start">
                        <FileField name="contractFile" control={control} label="نسخة العقد الممسوحة ضوئياً" size="sm" />
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
