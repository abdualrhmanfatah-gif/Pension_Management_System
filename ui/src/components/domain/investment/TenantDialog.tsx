"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { MapPin, User } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import * as z from "zod";

import { investmentApi, type TenantDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NoteBox,
    NativeSelect
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const tenantSchema = z.object({
    fullName: z.string().trim().min(3, "الاسم الكامل يجب أن يكون 3 أحرف على الأقل"),
    tenantType: z.enum(["Individual", "Company", "Merchant"]),
    nationalID: z.string().trim().optional(),
    commercialRegister: z.string().trim().optional(),
    phone: z.string().min(7, "رقم الهاتف غير صحيح"),
    address: z.string().trim().optional(),
}).superRefine((data, ctx) => {
    if (data.tenantType === "Individual" && (!data.nationalID || data.nationalID.length < 5)) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "الرقم الوطني مطلوب",
            path: ["nationalID"]
        });
    }
    if ((data.tenantType === "Company" || data.tenantType === "Merchant") && (!data.commercialRegister || data.commercialRegister.length < 3)) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "رقم السجل التجاري مطلوب",
            path: ["commercialRegister"]
        });
    }
});

export type TenantFormValues = z.infer<typeof tenantSchema>;

// ─── Props ─────────────────────────────────────────────

interface TenantFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<TenantDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function TenantDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess
}: TenantFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.tenantID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["tenant", editId],
        queryFn: () => investmentApi.getTenant(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as TenantDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Form ───────────────────────────────────────────
    const form = useForm<TenantFormValues>({
        resolver: zodResolver(tenantSchema),
        defaultValues: useMemo(() => ({
            fullName: "",
            tenantType: "Individual" as const,
            nationalID: "",
            commercialRegister: "",
            phone: "",
            address: "",
        }), []),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;
    const selectedType = useWatch({ control, name: "tenantType" });

    // ─── Initial values for BaseFormDialog reset ────────
    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            fullName: data.fullName || "",
            tenantType: (data.tenantType === "Corporate" ? "Company" : data.tenantType === "Merchant" ? "Merchant" : "Individual") as "Individual" | "Company" | "Merchant",
            nationalID: data.nationalIDOrCR || "",
            commercialRegister: data.nationalIDOrCR || "",
            phone: data.phone || "",
            address: (data as TenantDto & { address?: string }).address || "",
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: TenantFormValues) => {
            if (mode === "create") {
                return investmentApi.createTenant(values);
            }
            return investmentApi.updateTenant(effectiveData!.tenantID!, values);
        },
        {
            successMessage: mode === "create" ? "تم إضافة المستأجر بنجاح" : "تم تحديث بيانات المستأجر",
            invalidateKeys: [
                ["tenants"],
                ["tenant", editId],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: TenantFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إضافة مستأجر جديد" : "تعديل بيانات المستأجر"}
            description="إدارة بيانات المستأجرين وربطهم بالعقود بشكل آمن"
            size="3xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "حفظ المستأجر" : "تحديث البيانات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات التعريفية" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        <FormField label="الاسم الكامل" required error={errors.fullName?.message}>
                            <Input {...register("fullName")} />
                        </FormField>
                        <FormField label="نوع المستأجر" required>
                            <Controller
                                name="tenantType"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[{ label: "فرد", value: "Individual" }, { label: "شركة", value: "Company" }, { label: "تاجر", value: "Merchant" }]}
                                        value={field.value}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>
                        {selectedType === "Individual" ? (
                            <FormField label="الرقم الوطني" required error={errors.nationalID?.message}>
                                <Input {...register("nationalID")} className="font-mono" />
                            </FormField>
                        ) : (
                            <FormField label="السجل التجاري" required error={errors.commercialRegister?.message}>
                                <Input {...register("commercialRegister")} className="font-mono" />
                            </FormField>
                        )}
                        <FormField label="رقم الهاتف" required error={errors.phone?.message}>
                            <Input {...register("phone")} className="font-mono" dir="ltr" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="العنوان" icon={MapPin}>
                    <FormField label="العنوان التفصيلي" error={errors.address?.message}>
                        <Input {...register("address")} placeholder="المحافظة، المدينة، الشارع..." />
                    </FormField>
                </FormSection>

                <NoteBox type="info">
                    يجب مطابقة بيانات المستأجر مع الوثائق الرسمية لضمان صحة العقود.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
