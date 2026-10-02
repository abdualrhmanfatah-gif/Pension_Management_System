import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { BedDouble, Building2, DollarSign, Square } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi, type PropertyUnitDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const unitSchema = z.object({
    propertyID: z.number().min(1, "يجب تحديد العقار"),
    unitNumber: z.string().trim().min(1, "رقم الوحدة مطلوب"),
    unitTypeID: z.number().min(1, "نوع الوحدة مطلوب"),
    floor: z.number().int("الطابق يجب أن يكون رقماً صحيحاً"),
    area: z.number().positive("المساحة يجب أن تكون رقماً موجباً"),
    bedrooms: z.number().int().min(0, "عدد الغرف لا يمكن أن يكون سالباً"),
    rentAmount: z.number().min(0, "مبلغ الإيجار لا يمكن أن يكون سالباً"),
    status: z.string().min(1, "حالة الوحدة مطلوبة"),
    notes: z.string().trim().optional(),
});

export type UnitFormValues = z.infer<typeof unitSchema>;

// ─── Constants ─────────────────────────────────────────

const UNIT_STATUSES = [
    { label: "شاغرة", value: "Vacant" },
    { label: "مؤجرة", value: "Leased" },
    { label: "قيد الصيانة", value: "UnderMaintenance" },
    { label: "محجوزة", value: "Reserved" },
];

// ─── Props ─────────────────────────────────────────────

interface UnitFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<PropertyUnitDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
    propertyId: number;
}

// ─── Component ─────────────────────────────────────────

export function UnitDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess,
    propertyId
}: UnitFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.unitID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["property-unit", editId],
        queryFn: () => investmentApi.getUnits(propertyId).then(units => units.find(u => u.unitID === editId)),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as PropertyUnitDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: unitTypes = [] } = useQuery({
        queryKey: ["unit-types-lookup"],
        queryFn: () => investmentApi.getUnitTypes(),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<UnitFormValues>({
        resolver: zodResolver(unitSchema),
        defaultValues: useMemo(() => ({
            propertyID: propertyId,
            unitNumber: "",
            unitTypeID: 0,
            floor: 0,
            area: 0,
            bedrooms: 0,
            rentAmount: 0,
            status: "Vacant",
            notes: "",
        }), [propertyId]),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;

    // ─── Initial values for BaseFormDialog reset ────────
    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        const extended = data as PropertyUnitDto & { bedrooms?: number; rentAmount?: number };
        return {
            propertyID: data.propertyID || propertyId,
            unitNumber: data.unitNumber || "",
            unitTypeID: data.unitTypeID || 0,
            floor: typeof data.floor === 'string' ? parseInt(data.floor) || 0 : data.floor || 0,
            area: data.area || 0,
            bedrooms: extended.bedrooms || 0,
            rentAmount: extended.rentAmount || 0,
            status: data.status || "Vacant",
            notes: "",
        };
    }, [effectiveData, propertyId]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: UnitFormValues) => {
            if (mode === "create") {
                return investmentApi.createUnit({ ...values, propertyID: propertyId });
            }
            return investmentApi.updateUnit(effectiveData!.unitID!, values);
        },
        {
            successMessage: mode === "create" ? "تمت إضافة الوحدة بنجاح" : "تم تحديث الوحدة بنجاح",
            invalidateKeys: [
                ["property-units", propertyId],
                ["property", propertyId],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: UnitFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إضافة وحدة جديدة" : "تعديل بيانات وحدة"}
            description="إدارة الوحدات السكنية أو التجارية داخل أصل عقاري"
            size="3xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إضافة الوحدة" : "حفظ التعديلات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات الأساسية للوحدة" icon={Building2}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="رقم/اسم الوحدة" required error={errors.unitNumber?.message}>
                            <Input {...register("unitNumber")} placeholder="مثال: شقة 101" />
                        </FormField>
                        <FormField label="نوع الوحدة" required error={errors.unitTypeID?.message}>
                            <Controller
                                name="unitTypeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={unitTypes.map(p => ({ label: p.name, value: String(p.id) }))}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر النوع..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="حالة الوحدة" required error={errors.status?.message}>
                            <Controller
                                name="status"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={UNIT_STATUSES}
                                        value={field.value}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="المواصفات الفنية" icon={Square}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="الطابق" required error={errors.floor?.message}>
                            <Input type="number" {...register("floor", { valueAsNumber: true })} placeholder="0" />
                        </FormField>
                        <FormField label="المساحة (م²)" required error={errors.area?.message}>
                            <Input type="number" {...register("area", { valueAsNumber: true })} placeholder="0.00" />
                        </FormField>
                        <FormField label="عدد غرف النوم" required error={errors.bedrooms?.message}>
                            <Input type="number" {...register("bedrooms", { valueAsNumber: true })} placeholder="0" icon={BedDouble} />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="البيانات الإيجارية" icon={DollarSign}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="مبلغ الإيجار الشهري (ر.ي)" required error={errors.rentAmount?.message}>
                            <Input type="number" {...register("rentAmount", { valueAsNumber: true })} placeholder="0.00" className="font-mono" />
                        </FormField>
                    </div>
                    <FormField label="ملاحظات (اختياري)" error={errors.notes?.message} className="mt-4">
                        <Textarea {...register("notes")} placeholder="أي تفاصيل إضافية حول حالة الوحدة..." rows={2} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
