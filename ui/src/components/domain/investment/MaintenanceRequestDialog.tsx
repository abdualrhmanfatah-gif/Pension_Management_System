import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { AlertOctagon, Building2, FileText, Wrench } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { investmentApi, type MaintenanceRequestDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    NoteBox,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    propertyID: z.number().min(1, "العقار مطلوب"),
    unitID: z.number().optional().nullable(),
    description: z.string().min(5, "وصف المشكلة مطلوب (5 أحرف على الأقل)"),
    priority: z.string().min(1, "الأولوية مطلوبة"),
});

export type MaintenanceFormValues = z.infer<typeof schema>;

// ─── Props ─────────────────────────────────────────────

interface MaintenanceRequestFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<MaintenanceRequestDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
    propertyId?: number;
}

// ─── Component ─────────────────────────────────────────

export function MaintenanceRequestDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess,
    propertyId
}: MaintenanceRequestFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.requestID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["maintenance-request", editId],
        queryFn: () => investmentApi.getRequest(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as MaintenanceRequestDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const effectivePropertyId = effectiveData?.propertyID || propertyId || 0;

    const { data: properties = [] } = useQuery({
        queryKey: ["investment", "properties-simple"],
        queryFn: () => investmentApi.getProperties(),
        enabled: open,
    });

    const { data: units = [] } = useQuery({
        queryKey: ["investment", "units", effectivePropertyId],
        queryFn: () => investmentApi.getUnits(effectivePropertyId),
        enabled: open && !!effectivePropertyId,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<MaintenanceFormValues>({
        resolver: zodResolver(schema) as Resolver<z.infer<typeof schema>>,
        defaultValues: useMemo(() => ({
            propertyID: propertyId || 0,
            unitID: null,
            description: "",
            priority: "Medium",
        }), [propertyId]),
    });

    const { register, control, watch, reset, formState: { errors, isDirty } } = form;
    const selectedPropertyID = watch("propertyID");

    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            propertyID: data.propertyID || propertyId || 0,
            unitID: data.unitID || null,
            description: data.description || "",
            priority: data.priority || "Medium",
        };
    }, [effectiveData, propertyId]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: MaintenanceFormValues) => {
            if (mode === "create") {
                return investmentApi.createRequest({ ...values, propertyID: values.propertyID || propertyId });
            }
            return investmentApi.updateRequest(effectiveData!.requestID!, values);
        },
        {
            successMessage: mode === "create" ? "تم تسجيل بلاغ الصيانة بنجاح" : "تم تحديث بلاغ الصيانة",
            invalidateKeys: [
                ["property-maintenance", (propertyId as number) ?? 0],
                ["maintenance-requests-all"],
                ["maintenance-request", editId],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: MaintenanceFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "تسجيل بلاغ صيانة جديد" : "تعديل بلاغ صيانة"}
            description="إرسال طلب صيانة أو إصلاح لمرافق العقارات التابعة للصندوق"
            size="xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "تسجيل البلاغ" : "حفظ التعديلات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="تحديد الموقع" icon={Building2}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="العقار المرتبط" error={errors.propertyID?.message} required>
                            <Controller
                                name="propertyID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        options={properties.map((p) => ({
                                            value: String(p.propertyID),
                                            label: p.propertyName
                                        }))}
                                        placeholder="اختر العقار..."
                                    />
                                )}
                            />
                        </FormField>
                        <FormField label="الوحدة (اختياري)" error={errors.unitID?.message}>
                            <Controller
                                name="unitID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        value={field.value ? String(field.value) : ""}
                                        onChange={(v) => field.onChange(v ? Number(v) : null)}
                                        options={
                                            units.map((u) => ({
                                                value: String(u.unitID),
                                                label: u.unitNumber || `Unit ${u.unitID}`,
                                            }))
                                        }
                                        placeholder="اختر الوحدة..."
                                        disabled={!selectedPropertyID}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="تفاصيل المشكلة والأولوية" icon={Wrench}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="درجة الأهمية" error={errors.priority?.message} required>
                            <Controller
                                name="priority"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        {...field}
                                        options={[
                                            { value: "Low", label: "منخفض" },
                                            { value: "Medium", label: "متوسط" },
                                            { value: "High", label: "مرتفع" },
                                            { value: "Urgent", label: "عاجل جداً" }
                                        ]}
                                        icon={AlertOctagon}
                                    />
                                )}
                            />
                        </FormField>

                        <div className="md:col-span-2">
                            <FormField label="وصف العطل / الطلب" error={errors.description?.message} required icon={FileText}>
                                <Textarea
                                    {...register("description")}
                                    rows={2}
                                    placeholder="يرجى وصف المشكلة بالتفصيل وموقعها الدقيق في العقار..."
                                    className="leading-relaxed"
                                />
                            </FormField>
                        </div>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    سيتم إرسال هذا البلاغ مباشرة إلى قسم الصيانة والخدمات الفنية للمراجعة والتكليف.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
