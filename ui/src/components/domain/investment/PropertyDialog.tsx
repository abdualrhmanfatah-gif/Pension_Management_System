import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { DollarSign, Home, Map } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi, type PropertyDetailDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const propertySchema = z.object({
    projectID: z.number().optional(),
    propertyTypeID: z.number().min(1, "نوع العقار مطلوب"),
    propertyName: z.string().trim().min(3, "اسم العقار مطلوب"),
    locationCity: z.string().trim().min(2, "المدينة مطلوبة"),
    address: z.string().trim().min(5, "العنوان التفصيلي مطلوب"),
    totalArea: z.number().positive("المساحة يجب أن تكون رقماً موجباً"),
    initialValue: z.number().positive("القيمة الأولية يجب أن تكون رقماً موجباً"),
    currentMarketValue: z.number().positive("القيمة السوقية يجب أن تكون رقماً موجباً"),
    googleMapsUrl: z.string().url("رابط الخريطة غير صحيح").or(z.literal("")).optional(),
    titleDeedNumber: z.string().optional(),
});

type PropertyFormValues = z.infer<typeof propertySchema>;

// ─── Props ─────────────────────────────────────────────

interface PropertyFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<PropertyDetailDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function PropertyDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess
}: PropertyFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.propertyID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["property", editId],
        queryFn: () => investmentApi.getProperty(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as PropertyDetailDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: projects = [] } = useQuery({
        queryKey: ["projects-lookup"],
        queryFn: () => investmentApi.getProjects(),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    const { data: propertyTypes = [] } = useQuery({
        queryKey: ["property-types-lookup"],
        queryFn: () => investmentApi.getPropertyTypes(),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<PropertyFormValues>({
        resolver: zodResolver(propertySchema),
        defaultValues: useMemo(() => ({
            projectID: 0,
            propertyName: "",
            propertyTypeID: 0,
            locationCity: "",
            address: "",
            totalArea: 0,
            initialValue: 0,
            currentMarketValue: 0,
            googleMapsUrl: "",
            titleDeedNumber: ""
        }), []),
    });

    const { register, control, formState: { errors, isDirty } } = form;

    // ─── Initial values for BaseFormDialog reset ────────
    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            projectID: data.projectID || 0,
            propertyTypeID: data.propertyTypeID || 0,
            propertyName: data.propertyName || "",
            locationCity: data.locationCity || "",
            address: (data as PropertyDetailDto).address || "",
            totalArea: data.totalArea || 0,
            initialValue: data.initialValue || 0,
            currentMarketValue: data.currentMarketValue || 0,
            googleMapsUrl: data.googleMapsUrl || "",
            titleDeedNumber: data.titleDeedNumber || "",
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: PropertyFormValues) => {
            const payload = {
                ...values,
                projectID: values.projectID === 0 ? undefined : values.projectID,
                status: effectiveData?.status || "Active",
            };
            if (mode === "create") {
                return investmentApi.createProperty(payload);
            }
            return investmentApi.updateProperty(effectiveData!.propertyID!, payload);
        },
        {
            successMessage: mode === "create" ? "تم إضافة العقار بنجاح" : "تم تحديث العقار بنجاح",
            invalidateKeys: [
                ["properties"],
                ["property", editId],
                ["portfolio-summary"],
                ["project-summary"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: PropertyFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إضافة عقار جديد" : "تعديل بيانات عقار"}
            description="إدارة الأصول العقارية ضمن المشاريع الاستثمارية"
            size="3xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إضافة العقار" : "حفظ التعديلات"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات التعريفية للعقار" icon={Home}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        <FormField label="المشروع التابع له" error={errors.projectID?.message}>
                            <Controller
                                name="projectID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "بدون مشروع (عقار مستقل)", value: "0" },
                                            ...projects.map(p => ({ label: p.name, value: String(p.projectID) }))
                                        ]}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر المشروع..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="اسم العقار" required error={errors.propertyName?.message}>
                            <Input {...register("propertyName")} placeholder="مثال: عمارة اللؤلؤة" />
                        </FormField>
                        <FormField label="نوع العقار" required error={errors.propertyTypeID?.message}>
                            <Controller
                                name="propertyTypeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={propertyTypes.map(p => ({ label: p.name, value: String(p.id) }))}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر النوع..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="رقم سند الملكية" error={errors.titleDeedNumber?.message}>
                            <Input {...register("titleDeedNumber")} placeholder="رقم السند/الصك" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="العنوان والموقع" icon={Map}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="المدينة/المحافظة" required error={errors.locationCity?.message}>
                            <Input {...register("locationCity")} placeholder="مثال: صنعاء، عدن..." />
                        </FormField>
                        <FormField label="العنوان التفصيلي" required error={errors.address?.message}>
                            <Input {...register("address")} placeholder="اسم الشارع، رقم المبنى..." />
                        </FormField>
                        <FormField label="رابط Google Maps" error={errors.googleMapsUrl?.message}>
                            <Input {...register("googleMapsUrl")} placeholder="https://maps.google.com/..." dir="ltr" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="البيانات المالية والمساحة" icon={DollarSign}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="المساحة الإجمالية (م²)" required error={errors.totalArea?.message}>
                            <Input type="number" {...register("totalArea", { valueAsNumber: true })} placeholder="0.00" className="font-mono" />
                        </FormField>
                        <FormField label="القيمة الأولية/سعر الشراء" required error={errors.initialValue?.message}>
                            <Input type="number" {...register("initialValue", { valueAsNumber: true })} placeholder="0.00" className="font-mono" />
                        </FormField>
                        <FormField label="القيمة السوقية الحالية" required error={errors.currentMarketValue?.message}>
                            <Input type="number" {...register("currentMarketValue", { valueAsNumber: true })} placeholder="0.00" className="font-mono" />
                        </FormField>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
