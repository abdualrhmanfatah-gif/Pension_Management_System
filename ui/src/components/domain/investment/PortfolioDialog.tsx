import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Activity, Briefcase, DollarSign, User } from "lucide-react";
import { useCallback, useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";

import api from "@/api/axios";
import { investmentApi, type PortfolioDto } from "@/api/investment";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NoteBox,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const portfolioSchema = z.object({
    name: z.string().trim().min(3, "اسم المحفظة يجب أن يكون 3 أحرف على الأقل"),
    description: z.string().trim().optional(),
    managerUserID: z.number().min(1, "يجب تحديد المدير المسؤول"),
    totalTargetCapital: z.number().positive("رأس المال المستهدف يجب أن يكون مبلغاً موجباً"),
    status: z.enum(["Active", "Inactive", "Closed"]),
});

type PortfolioFormValues = z.infer<typeof portfolioSchema>;

// ─── Types ─────────────────────────────────────────────



// ─── Props ─────────────────────────────────────────────

interface PortfolioFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<PortfolioDto>;
    mode: "create" | "edit";
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function PortfolioDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    onSuccess
}: PortfolioFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.portfolioID : undefined;
    const { data: freshData } = useQuery({
        queryKey: ["portfolio", editId],
        queryFn: () => investmentApi.getPortfolio(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as PortfolioDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: users = [] } = useQuery({
        queryKey: ["users-lookup"],
        queryFn: () => api.get<{ id: number; fullName: string }[]>("/users").then(r => r.data.map(u => ({ id: u.id, name: u.fullName }))),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<PortfolioFormValues>({
        resolver: zodResolver(portfolioSchema),
        defaultValues: useMemo(() => ({
            name: "",
            description: "",
            managerUserID: 0,
            totalTargetCapital: 0,
            status: "Active" as const,
        }), []),
    });

    const { register, control, reset, formState: { errors, isDirty } } = form;

    const initialValues = useMemo(() => {
        const data = effectiveData;
        if (!data) return undefined;
        return {
            name: data.name || "",
            description: data.description || "",
            managerUserID: data.managerUserID || 0,
            totalTargetCapital: data.totalTargetCapital || 0,
            status: (data as any).status || "Active",
        };
    }, [effectiveData]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        async (values: PortfolioFormValues) => {
            if (mode === "create") {
                return investmentApi.createPortfolio(values);
            }
            return investmentApi.updatePortfolio(effectiveData!.portfolioID!, {
                ...values,
                status: (effectiveData as any)?.status || "Active",
            });
        },
        {
            successMessage: mode === "create" ? "تم إنشاء المحفظة بنجاح" : "تم تحديث المحفظة بنجاح",
            invalidateKeys: [
                ["portfolios"],
                ["portfolio", editId],
                ["portfolio-summary"],
            ],
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.();
            },
        }
    );

    const handleFormSubmit = useCallback((values: PortfolioFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إنشاء محفظة جديدة" : "تعديل محفظة استثمارية"}
            description="إدارة المحافظ الاستثمارية الكبرى وتحديد رؤوس الأموال المستهدفة"
            size="xl"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إنشاء المحفظة" : "حفظ التغييرات"}
        >
            <div className="space-y-4 text-start">
                <FormSection title="البيانات الأساسية للمحفظة" icon={Briefcase}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="اسم المحفظة الاستثمارية" required error={errors.name?.message}>
                            <Input
                                {...register("name")}
                                placeholder="مثال: محفظة التطوير العقاري السكني"
                            />
                        </FormField>
                        <FormField label="وصف أهداف المحفظة" error={errors.description?.message}>
                            <Textarea
                                {...register("description")}
                                placeholder="أدخل تفاصيل حول الأهداف الاستثمارية لهذه المحفظة..."
                                className="min-h-[80px]"
                                rows={2}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="البيانات المالية والإدارية" icon={DollarSign}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="رأس المال المستهدف (ر.ي)" required error={errors.totalTargetCapital?.message}>
                            <Input
                                type="number"
                                step="1000"
                                {...register("totalTargetCapital", { valueAsNumber: true })}
                                placeholder="0.00"
                                className="font-mono text-lg"
                            />
                        </FormField>
                        <FormField label="المدير المسؤول" required error={errors.managerUserID?.message}>
                            <Controller
                                name="managerUserID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={users.map(u => ({ label: u.name, value: String(u.id) }))}
                                        value={String(field.value)}
                                        onChange={(v) => field.onChange(Number(v))}
                                        placeholder="اختر المدير..."
                                        icon={User}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الحالة التشغيلية" icon={Activity}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="حالة المحفظة" required error={errors.status?.message}>
                            <Controller
                                name="status"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "نشطة", value: "Active" },
                                            { label: "غير نشطة", value: "Inactive" },
                                            { label: "مغلقة", value: "Closed" }
                                        ]}
                                        value={field.value}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="warning">
                    تنبيه: سيؤدي إغلاق المحفظة (Closed) إلى إيقاف إمكانية إضافة مشاريع جديدة إليها مستقبلاً.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
