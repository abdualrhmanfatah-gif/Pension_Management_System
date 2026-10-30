import { zodResolver } from "@hookform/resolvers/zod";
import { BookOpen } from "lucide-react";
import { useEffect } from "react";
import { type Resolver, useForm } from "react-hook-form";
import { z } from "zod";

import { accountsApi } from "@/api/accounting";
<<<<<<< HEAD
import { BaseFormDialog, FormField, FormSection, Input, NativeSelect } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import type { AccountCategory,ChartOfAccountDto, UpdateChartOfAccountDto } from "@/types/accounting";
import { ACCOUNT_CATEGORY, ACCOUNT_CATEGORY_AR } from "@/types/accounting";
=======
import type { ChartOfAccountDto, UpdateChartOfAccountDto, AccountCategory } from "@/types/accounting";
import { ACCOUNT_CATEGORY, ACCOUNT_CATEGORY_AR } from "@/types/accounting";
import { BaseFormDialog, FormField, FormSection, Input, NativeSelect } from "@/components/ui";
import { useApiMutation } from "@/hooks";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Schema ───────────────────────────────────────
const schema = z.object({
  accountCode: z.string().min(1, "كود الحساب مطلوب"),
  accountName: z.string().min(1, "اسم الحساب مطلوب"),
  accountNature: z.enum(["Debit", "Credit"]),
  isPostable: z.boolean().default(false),
  accountCategory: z.string().default("Asset"),
  description: z.string().max(500).nullable().optional(),
});

type FormValues = z.infer<typeof schema>;

// ─── Props ────────────────────────────────────────
interface AccountFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account?: ChartOfAccountDto | null;
  parentAccount?: ChartOfAccountDto | null;
}

// ─── Component ────────────────────────────────────
export function AccountFormDialog({ open, onOpenChange, account, parentAccount }: AccountFormDialogProps) {
  const isEditing = !!account;

  const form = useForm<FormValues>({
    resolver: zodResolver(schema) as Resolver<FormValues>,
    defaultValues: {
      accountCode: "",
      accountName: "",
      accountNature: parentAccount?.accountNature === "Credit" ? "Credit" : "Debit",
      isPostable: !!parentAccount,
      accountCategory: "Asset",
      description: "",
    },
  });

  const { register, handleSubmit, formState: { errors, isDirty }, setValue, reset } = form;

  // ─── Reset on open ───────────────────────────────
  useEffect(() => {
    if (open) {
      if (account) {
        reset({
          accountCode: account.accountCode,
          accountName: account.accountName,
          accountNature: account.accountNature as "Debit" | "Credit",
          isPostable: account.isPostable,
          accountCategory: account.accountCategory,
          description: account.description || "",
        });
      } else {
        reset({
          accountCode: "",
          accountName: "",
          accountNature: parentAccount?.accountNature === "Credit" ? "Credit" : "Debit",
          isPostable: !!parentAccount,
          accountCategory: "Asset",
          description: "",
        });
      }
    }
  }, [open, account, parentAccount, reset]);

  // ─── Mutation ────────────────────────────────────
  const mutation = useApiMutation(
    async (data: FormValues) => {
      if (isEditing) {
        const updateData: UpdateChartOfAccountDto = {
          accountCode: data.accountCode,
          accountName: data.accountName,
          accountNature: data.accountNature,
          parentAccountID: parentAccount?.accountID ?? null,
          isPostable: data.isPostable,
          accountCategory: data.accountCategory as AccountCategory,
          description: data.description || null,
          isActive: account?.isActive ?? true,
        };
        return accountsApi.update(account!.accountID, updateData);
      }
      return accountsApi.create({
        ...data,
        parentAccountID: parentAccount?.accountID ?? null,
        accountCategory: data.accountCategory as AccountCategory,
        description: data.description || null,
      });
    },
    {
      successMessage: isEditing ? "تم تحديث الحساب بنجاح" : "تم إنشاء الحساب بنجاح",
      errorTitle: "خطأ في الحفظ",
      invalidateKeys: [["accounts"]],
      onSuccess: () => {
        onOpenChange(false);
        reset();
      },
    }
  );

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEditing ? "تعديل حساب" : "إضافة حساب جديد"}
      description={parentAccount ? `تحت: ${parentAccount.accountCode} - ${parentAccount.accountName}` : undefined}
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel={isEditing ? "حفظ التعديلات" : "إنشاء الحساب"}
    >
      <FormSection title="بيانات الحساب" icon={BookOpen}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="كود الحساب" required error={errors.accountCode?.message}>
            <Input {...register("accountCode")} placeholder="مثال: 1110" />
          </FormField>

          <FormField label="طبيعة الحساب" required error={errors.accountNature?.message}>
            <NativeSelect
              value={form.watch("accountNature")}
              options={[
                { label: "مدين", value: "Debit" },
                { label: "دائن", value: "Credit" },
              ]}
              onChange={(v) => setValue("accountNature", v as "Debit" | "Credit")}
            />
          </FormField>
        </div>

        <FormField label="اسم الحساب" required error={errors.accountName?.message}>
          <Input {...register("accountName")} placeholder="اسم الحساب بالعربية" />
        </FormField>

        <FormField label="تصنيف الحساب" required error={errors.accountCategory?.message}>
          <NativeSelect
            value={form.watch("accountCategory")}
            options={Object.entries(ACCOUNT_CATEGORY).map(([, value]) => ({
              label: ACCOUNT_CATEGORY_AR[value as keyof typeof ACCOUNT_CATEGORY_AR],
              value: value,
            }))}
            onChange={(v) => setValue("accountCategory", v)}
          />
        </FormField>

        <FormField label="الوصف" error={errors.description?.message}>
          <Input {...register("description")} placeholder="وصف مختصر للحساب (اختياري)" />
        </FormField>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={form.watch("isPostable")}
            onChange={(e) => setValue("isPostable", e.target.checked)}
            className="rounded border-border"
          />
          <span className="text-sm">حساب قابل للترحيل (مستوى تفصيلي)</span>
        </label>
      </FormSection>
    </BaseFormDialog>
  );
}