import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Activity, BookOpen, CircleDollarSign, FolderTree, Hash, Percent, Wrench } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

<<<<<<< HEAD
import { accountsApi } from "@/api/accounting";
import { type AssetCategoryDto,fixedAssetApi } from "@/api/fixedAssets";
=======
import { fixedAssetApi, type AssetCategoryDto } from "@/api/fixedAssets";
import { accountsApi } from "@/api/accounting";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    BaseFormDialog,
    Checkbox,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Textarea
} from "@/components/ui";
<<<<<<< HEAD
import { queryKeys } from "@/constants/queryKeys";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation } from "@/hooks";

const schema = z.object({
    categoryName: z.string().trim().min(2, "اسم التصنيف مطلوب"),
    categoryCode: z.string().trim().optional().nullable().or(z.literal("")),
    description: z.string().trim().optional().nullable().or(z.literal("")),
    assetTypeID: z.coerce.number().min(1, "نوع الأصل مطلوب"),
    depreciationRate: z.coerce.number().min(0.01, "نسبة الإهلاك مطلوبة"),
    salvagePercent: z.coerce.number().min(0).max(100),
    assetAccountID: z.coerce.number().optional().nullable().or(z.literal(0)),
    accumDeprAccountID: z.coerce.number().optional().nullable().or(z.literal(0)),
    deprExpenseAccountID: z.coerce.number().optional().nullable().or(z.literal(0)),
    isActive: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    category: AssetCategoryDto | null;
}

export function AssetCategoryFormDialog({ open, onOpenChange, onSaved, category }: Props) {
    const { data: assetTypes = [] } = useQuery({
        queryKey: queryKeys.fixedAssets.types,
        queryFn: () => fixedAssetApi.getTypes(),
    });

    const { data: accounts = [] } = useQuery({
        queryKey: queryKeys.accounting.accounts.postable,
        queryFn: () => accountsApi.getPostable(),
    });

    const mutation = useApiMutation(
        (values: FormValues) => {
            const payload = {
                ...values,
                assetAccountID: values.assetAccountID && values.assetAccountID > 0 ? values.assetAccountID : null,
                accumDeprAccountID: values.accumDeprAccountID && values.accumDeprAccountID > 0 ? values.accumDeprAccountID : null,
                deprExpenseAccountID: values.deprExpenseAccountID && values.deprExpenseAccountID > 0 ? values.deprExpenseAccountID : null,
            };
            return fixedAssetApi.updateCategory(category!.categoryID, payload);
        },
        {
            successMessage: "تم تحديث تصنيف الأصل بنجاح",
            onSuccessCallback: () => {
                onSaved();
                onOpenChange(false);
            }
        }
    );

    const form = useForm<FormValues>({
        resolver: zodResolver(schema) as Resolver<z.infer<typeof schema>>,
        defaultValues: {
            categoryName: "", categoryCode: "", description: "",
            assetTypeID: 0, depreciationRate: 20, salvagePercent: 5,
            assetAccountID: 0, accumDeprAccountID: 0, deprExpenseAccountID: 0,
            isActive: true,
        },
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty }, setValue, watch } = form;
    const onSubmit = (values: FormValues) => mutation.mutate(values);

    const initialValues = useMemo(() => {
        if (!category) return {
            categoryName: "", categoryCode: "", description: "",
            assetTypeID: 0, depreciationRate: 20, salvagePercent: 5,
            assetAccountID: 0, accumDeprAccountID: 0, deprExpenseAccountID: 0,
            isActive: true,
        };
        return {
            categoryName: category.categoryName,
            categoryCode: category.categoryCode || "",
            description: category.description || "",
            assetTypeID: category.assetTypeID,
            depreciationRate: category.depreciationRate,
            salvagePercent: category.salvagePercent,
            assetAccountID: category.assetAccountID || 0,
            accumDeprAccountID: category.accumDeprAccountID || 0,
            deprExpenseAccountID: category.deprExpenseAccountID || 0,
            isActive: category.isActive,
        };
    }, [category]);

    const typeOptions = assetTypes.map((t: any) => ({
        value: String(t.assetTypeID),
        label: t.typeName,
    }));

    const accountOptions = [
        { value: "0", label: "بدون حساب محاسبي" },
        ...accounts.map((a: any) => ({
            value: String(a.accountID),
            label: `${a.accountCode} — ${a.accountName}`,
        })),
    ];

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تعديل تصنيف الأصل"
            description="تحديث بيانات تصنيف الأصول الثابتة والحسابات المرتبطة"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="حفظ التعديلات"
        >
            <div className="space-y-4">
                <FormSection title="بيانات التصنيف" icon={Wrench}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="اسم التصنيف" required error={errors.categoryName?.message}>
                            <Input
                                {...register("categoryName")}
                                placeholder="مثال: أجهزة حاسوب"
                                icon={FolderTree}
                            />
                        </FormField>
                        <FormField label="رمز التصنيف" error={errors.categoryCode?.message}>
                            <Input
                                {...register("categoryCode")}
                                placeholder="مثال: FA-IT"
                                icon={Hash}
                                className="font-mono"
                            />
                        </FormField>
                        <FormField label="نوع الأصل" required error={errors.assetTypeID?.message}>
                            <NativeSelect
                                options={typeOptions}
                                placeholder="اختر نوع الأصل..."
                                value={watch("assetTypeID") ?? ""}
                                onChange={(val) => setValue("assetTypeID", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                        <FormField label="نسبة الإهلاك السنوية (%)" required error={errors.depreciationRate?.message}>
                            <Input
                                type="number"
                                step="0.01"
                                {...register("depreciationRate")}
                                icon={BookOpen}
                                placeholder="مثال: 20"
                            />
                        </FormField>
                        <FormField label="نسبة قيمة التخلي %" error={errors.salvagePercent?.message}>
                            <Input
                                type="number"
                                step="any"
                                {...register("salvagePercent")}
                                icon={Percent}
                            />
                        </FormField>
                    </div>
                    <FormField label="الوصف" error={errors.description?.message}>
                        <Textarea
                            {...register("description")}
                            placeholder="وصف التصنيف..."
                            rows={2}
                        />
                    </FormField>
                </FormSection>

                <FormSection title="الحسابات المحاسبية" icon={CircleDollarSign}>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FormField label="حساب الأصل" error={errors.assetAccountID?.message}>
                            <NativeSelect
                                options={accountOptions}
                                placeholder="اختر الحساب..."
                                value={watch("assetAccountID") ?? ""}
                                onChange={(val) => setValue("assetAccountID", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                        <FormField label="مخصص الإهلاك" error={errors.accumDeprAccountID?.message}>
                            <NativeSelect
                                options={accountOptions}
                                placeholder="اختر الحساب..."
                                value={watch("accumDeprAccountID") ?? ""}
                                onChange={(val) => setValue("accumDeprAccountID", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                        <FormField label="مصروف الإهلاك" error={errors.deprExpenseAccountID?.message}>
                            <NativeSelect
                                options={accountOptions}
                                placeholder="اختر الحساب..."
                                value={watch("deprExpenseAccountID") ?? ""}
                                onChange={(val) => setValue("deprExpenseAccountID", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الحالة الإدارية" icon={Activity}>
<<<<<<< HEAD
                    <div className="flex items-center gap-3 p-4 bg-muted/30 rounded-2xl border border-border">
=======
                    <div className="flex items-center gap-3 p-4 bg-muted/30 rounded-2xl border border-border/40">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <Checkbox
                            id="acat-active-check"
                            checked={watch("isActive")}
                            onCheckedChange={(checked) => setValue("isActive", checked === true)}
                        />
                        <label htmlFor="acat-active-check" className="text-sm font-bold text-foreground cursor-pointer">
                            التصنيف نشط
                        </label>
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
