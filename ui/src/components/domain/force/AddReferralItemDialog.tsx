import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { UserPlus } from "lucide-react";
import { useCallback } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";

import { ministryApi } from "@/api/ministry";
import { lookupApi } from "@/api/pension";
import { ranksApi } from "@/api/ranks";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { FormField } from "@/components/ui/FormField";
import { DateInput, Input, Textarea } from "@/components/ui/Input";
import { NativeSelect } from "@/components/ui/NativeSelect";
import { useApiMutation } from "@/hooks";

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    batchId: number;
}

const schema = z.object({
    militaryNumber: z.string().min(1, "الرقم العسكري مطلوب"),
    fullName: z.string().min(3, "الاسم الكامل يجب أن يكون 3 أحرف على الأقل"),
    rankID: z.string().optional(),
    categoryID: z.string().optional(),
    gender: z.string().optional(),
    unitName: z.string().optional().or(z.literal("")),
    enrollmentDate: z.string().optional().or(z.literal("")),
    retirementDate: z.string().optional().or(z.literal("")),
    salary: z.string().optional().or(z.literal("")),
    linkNumber: z.string().optional().or(z.literal("")),
    notes: z.string().optional().or(z.literal("")),
});

type FormValues = z.infer<typeof schema>;

export function AddReferralItemDialog({ open, onOpenChange, batchId }: Props) {
    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            militaryNumber: "",
            fullName: "",
            rankID: "",
            categoryID: "",
            gender: "1",
            unitName: "",
            enrollmentDate: "",
            retirementDate: "",
            salary: "",
            linkNumber: "",
            notes: "",
        },
    });

    const { data: ranks = [] } = useQuery({
        queryKey: ["ranks"],
        queryFn: () => ranksApi.getAll(),
    });

    const { data: categories = [] } = useQuery({
        queryKey: ["retiree-categories"],
        queryFn: () => lookupApi.getCategories(),
    });

    const handleClose = useCallback(() => {
        form.reset();
        onOpenChange(false);
    }, [form, onOpenChange]);

    const handleOpenChange = useCallback(
        (isOpen: boolean) => {
            onOpenChange(isOpen);
            if (!isOpen) {
                handleClose();
            }
        },
        [handleClose, onOpenChange]
    );

    const addMutation = useApiMutation(
        (payload: {
            militaryNumber: string;
            fullName: string;
            rankID?: number | null;
            categoryID?: number | null;
            gender?: number;
            unitName?: string;
            enrollmentDate?: string | null;
            retirementDate?: string | null;
            salary?: number | null;
            linkNumber?: string;
            notes?: string;
        }) => ministryApi.addManualItem(batchId, payload),
        {
            successMessage: "تم إضافة المحال بنجاح",
            invalidateKeys: [
                ["force", "items"],
                ["force", "batch", batchId],
                ["force", "analytics"],
            ],
            onSuccess: () => {
                form.reset();
                onOpenChange(false);
            },
        }
    );

    const handleSubmit = useCallback(
        (data: FormValues) => {
            addMutation.mutate({
                militaryNumber: data.militaryNumber.trim(),
                fullName: data.fullName.trim(),
                rankID: data.rankID ? Number(data.rankID) : null,
                categoryID: data.categoryID ? Number(data.categoryID) : null,
                gender: data.gender ? Number(data.gender) : 1,
                unitName: data.unitName?.trim() || undefined,
                enrollmentDate: data.enrollmentDate || null,
                retirementDate: data.retirementDate || null,
                salary: data.salary ? Number(data.salary) : null,
                linkNumber: data.linkNumber?.trim() || undefined,
                notes: data.notes?.trim() || undefined,
            });
        },
        [addMutation]
    );

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={handleOpenChange}
            title="إضافة محال"
            description="إدخال بيانات محال واحد إلى هذا الكشف"
            size="3xl"
            isLoading={addMutation.isPending}
            onSubmit={form.handleSubmit(handleSubmit)}
            submitLabel="إضافة المحال"
            submitIcon={<UserPlus className="w-4 h-4" />}
            cancelLabel="إلغاء"
            resetOnOpen
            reset={form.reset}
            showGlobalError
            formErrors={form.formState.errors}
        >
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-2">
                <FormField label="الرقم العسكري" required error={form.formState.errors.militaryNumber?.message}>
                    <Input
                        {...form.register("militaryNumber")}
                        placeholder="مثال: 123456"
                        dir="ltr"
                    />
                </FormField>

                <FormField label="الاسم الكامل" required error={form.formState.errors.fullName?.message} className="lg:col-span-2">
                    <Input
                        {...form.register("fullName")}
                        placeholder="الاسم الرباعي للمحال"
                    />
                </FormField>

                <FormField label="الرتبة" error={form.formState.errors.rankID?.message}>
                    <Controller
                        control={form.control}
                        name="rankID"
                        render={({ field }) => (
                            <NativeSelect
                                id={field.name}
                                value={field.value || ""}
                                onChange={(val) => field.onChange(val || "")}
                                placeholder="اختر الرتبة..."
                                options={ranks.map((r) => ({
                                    value: String(r.rankID),
                                    label: r.rankName,
                                }))}
                            />
                        )}
                    />
                </FormField>

                <FormField label="الفئة التقاعدية" error={form.formState.errors.categoryID?.message}>
                    <Controller
                        control={form.control}
                        name="categoryID"
                        render={({ field }) => (
                            <NativeSelect
                                id={field.name}
                                value={field.value || ""}
                                onChange={(val) => field.onChange(val || "")}
                                placeholder="اختر الفئة..."
                                options={categories.map((c) => ({
                                    value: String(c.id),
                                    label: c.name,
                                }))}
                            />
                        )}
                    />
                </FormField>

                <FormField label="الجنس" error={form.formState.errors.gender?.message}>
                    <Controller
                        control={form.control}
                        name="gender"
                        render={({ field }) => (
                            <NativeSelect
                                id={field.name}
                                value={field.value || "1"}
                                onChange={(val) => field.onChange(val || "1")}
                                options={[
                                    { value: "1", label: "ذكر" },
                                    { value: "2", label: "أنثى" },
                                ]}
                            />
                        )}
                    />
                </FormField>

                <FormField label="الوحدة" error={form.formState.errors.unitName?.message}>
                    <Input
                        {...form.register("unitName")}
                        placeholder="الوحدة أو الجهة"
                    />
                </FormField>

                <FormField label="تاريخ الالتحاق" error={form.formState.errors.enrollmentDate?.message}>
                    <Controller
                        control={form.control}
                        name="enrollmentDate"
                        render={({ field }) => (
                            <DateInput
                                id={field.name}
                                name={field.name}
                                value={field.value || undefined}
                                onChange={(val) => field.onChange(val || "")}
                            />
                        )}
                    />
                </FormField>

                <FormField label="تاريخ الإحالة" error={form.formState.errors.retirementDate?.message}>
                    <Controller
                        control={form.control}
                        name="retirementDate"
                        render={({ field }) => (
                            <DateInput
                                id={field.name}
                                name={field.name}
                                value={field.value || undefined}
                                onChange={(val) => field.onChange(val || "")}
                            />
                        )}
                    />
                </FormField>

                <FormField label="الراتب" error={form.formState.errors.salary?.message}>
                    <Input
                        {...form.register("salary")}
                        type="number"
                        placeholder="0"
                        dir="ltr"
                    />
                </FormField>

                <FormField label="رقم الربط" error={form.formState.errors.linkNumber?.message}>
                    <Input
                        {...form.register("linkNumber")}
                        placeholder="رقم الربط إن وجد"
                        dir="ltr"
                    />
                </FormField>

                <FormField label="ملاحظات" error={form.formState.errors.notes?.message} className="md:col-span-2 lg:col-span-3">
                    <Textarea
                        {...form.register("notes")}
                        placeholder="ملاحظات إضافية..."
                    />
                </FormField>
            </div>
        </BaseFormDialog>
    );
}