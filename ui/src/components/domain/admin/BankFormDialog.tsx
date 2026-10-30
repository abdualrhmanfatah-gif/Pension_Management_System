import { zodResolver } from "@hookform/resolvers/zod";
<<<<<<< HEAD
import { useQuery } from "@tanstack/react-query";
import { Activity, Hash, Landmark } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { accountsApi } from "@/api/accounting";
import { bankApi, type BankDto } from "@/api/banks";
=======
import { Activity, Hash, Landmark } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import { useQuery } from "@tanstack/react-query";
import * as z from "zod";

import { bankApi, type BankDto } from "@/api/banks";
import { accountsApi } from "@/api/accounting";
import type { ChartOfAccountDto } from "@/types/accounting";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
<<<<<<< HEAD
    NativeSelect,
    NoteBox} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import type { ChartOfAccountDto } from "@/types/accounting";
=======
    NoteBox,
    NativeSelect
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Validation Schema ─────────────────────────────────

const bankSchema = z.object({
    bankName: z.string().trim().min(2, "اسم البنك مطلوب"),
    bankCode: z.string().trim().optional().nullable().or(z.literal("")),
    accountID: z.number().nullable().optional(),
    isActive: z.boolean(),
});

type BankFormValues = z.infer<typeof bankSchema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    bank: BankDto | null;
}

/**
 * Standardized BankFormDialog.
 * Refactored to reach 100% architectural compliance.
 */
export function BankFormDialog({ open, onOpenChange, onSaved, bank }: Props) {
    const isEdit = !!bank;

    const { data: accounts = [] } = useQuery({
        queryKey: ["accounts", "postable"],
        queryFn: () => accountsApi.getPostable(),
        enabled: open,
    });

    // تصفية الحسابات البنكية (تبدأ بـ 18 - الأموال الجاهزة)
    const bankAccounts = useMemo(() => {
        return accounts.filter((a: ChartOfAccountDto) => a.accountCode.startsWith("18"));
    }, [accounts]);

    const mutation = useApiMutation(
        (values: BankFormValues) => isEdit
            ? bankApi.update(bank!.bankID, values)
            : bankApi.create(values),
        {
            successMessage: isEdit ? "تم تحديث بيانات البنك بنجاح" : "تم إضافة البنك الجديد بنجاح",
            onSuccessCallback: () => {
                onSaved();
                onOpenChange(false);
            }
        }
    );

    const form = useForm<BankFormValues>({
        resolver: zodResolver(bankSchema) as Resolver<z.infer<typeof bankSchema>>,
        defaultValues: {
            bankName: "",
            bankCode: "",
            accountID: null,
            isActive: true,
        },
    });

    const { register, handleSubmit, reset, watch, setValue, formState: { errors, isDirty } } = form;

    const onSubmit = (values: BankFormValues) => {
        mutation.mutate(values);
    };

    const initialValues = useMemo(() => {
        if (!bank) return { bankName: "", bankCode: "", accountID: null, isActive: true };
        return {
            bankName: bank.bankName,
            bankCode: bank.bankCode || "",
            accountID: bank.accountID || null,
            isActive: bank.isActive,
        };
    }, [bank]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEdit ? "تعديل بيانات البنك" : "إضافة بنك جديد"}
            description="إدارة الجهات المصرفية المعتمدة لصرف المعاشات"
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={isEdit ? "حفظ التعديلات" : "إضافة البنك"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الأساسية للبنك" icon={Landmark}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="اسم البنك / المصرف" required error={errors.bankName?.message}>
                            <Input
                                {...register("bankName")}
                                placeholder="مثال: البنك المركزي، بنك الكريمي..."
                                icon={Landmark}
                            />
                        </FormField>

                        <FormField label="رمز البنك (Bank Code)" error={errors.bankCode?.message}>
                            <Input
                                {...register("bankCode")}
                                placeholder="مثال: CACB"
                                icon={Hash}
                                dir="ltr"
                                className="font-mono uppercase"
                            />
                        </FormField>

                        <FormField label="الحساب المحاسبي المرتبط" error={errors.accountID?.message}>
                            <NativeSelect
                                value={String(watch("accountID") || "")}
                                options={[
                                    { label: "اختر الحساب...", value: "" },
                                    ...bankAccounts.map((a: ChartOfAccountDto) => ({
                                        label: `${a.accountCode} - ${a.accountName}`,
                                        value: String(a.accountID)
                                    }))
                                ]}
                                onChange={(v) => setValue("accountID", v ? Number(v) : null)}
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
                        <input
                            type="checkbox"
                            id="bank-active-check"
                            {...register("isActive")}
                            className="h-5 w-5 rounded border-border text-primary focus:ring-primary accent-primary cursor-pointer"
                        />
                        <label htmlFor="bank-active-check" className="text-sm font-bold text-foreground cursor-pointer">
                            البنك نشط (يسمح بإجراء التحويلات المالية إليه)
                        </label>
                    </div>
                </FormSection>

                <NoteBox type="info">
                    تأكد من اختيار الحساب المحاسبي الصحيح للبنك لضمان تسجيل القيود البنكية بشكل صحيح.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
