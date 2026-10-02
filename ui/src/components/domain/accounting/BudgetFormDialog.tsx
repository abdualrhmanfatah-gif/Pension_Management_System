import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { BarChart3 } from "lucide-react";
import { useEffect } from "react";
import { type Resolver, useForm } from "react-hook-form";

import { accountsApi, periodsApi,quartersApi } from "@/api/accounting";
import { budgetApi } from "@/api/budget";
import { BaseFormDialog, FormField, FormSection, Input, NativeSelect } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import type { ChartOfAccountDto, FiscalPeriodDto,QuarterDto } from "@/types/accounting";
import {
  type CreateBudgetItemForm,
  createBudgetItemSchema,
} from "@/types/budget";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  fiscalYearID: number | null;
  editItem?: any;
  onSuccess: () => void;
}

export function BudgetFormDialog({
  open,
  onOpenChange,
  fiscalYearID,
  editItem,
  onSuccess,
}: Props) {
  const isEditing = !!editItem;

  const form = useForm<CreateBudgetItemForm>({
    resolver: zodResolver(createBudgetItemSchema) as Resolver<CreateBudgetItemForm>,
    defaultValues: {
      accountID: 0,
      plannedAmount: 0,
      quarterID: null,
      notes: "",
    },
  });

  const { register, handleSubmit, formState: { errors, isDirty }, setValue, reset, watch } = form;

  // جلب حسابات الاستخدامات (المصروفات) - BookNumber = 3
  const { data: allAccounts } = useQuery({
    queryKey: ["postable-accounts"],
    queryFn: () => accountsApi.getPostable(),
  });

  // تصفية الحسابات لعرض حسابات الاستخدامات فقط (BookNumber = 3)
  const accounts = allAccounts?.filter((acc: ChartOfAccountDto) => {
    // حسابات الاستخدامات تبدأ بـ 3xxx
    return acc.accountCode.startsWith("3");
  });

  // جلب الفصول
  const { data: quarters } = useQuery({
    queryKey: ["quarters", fiscalYearID],
    queryFn: () => quartersApi.getAll(fiscalYearID!),
    enabled: fiscalYearID != null,
  });

  // جلب الفترات الشهرية
  const { data: periods } = useQuery({
    queryKey: ["periods", fiscalYearID],
    queryFn: () => periodsApi.getAll(fiscalYearID!),
    enabled: fiscalYearID != null,
  });

  // تعبئة النموذج عند التعديل
  useEffect(() => {
    if (open) {
      if (editItem) {
        reset({
          accountID: editItem.accountID,
          plannedAmount: editItem.plannedAmount,
          quarterID: editItem.quarterID,
          periodID: editItem.periodID,
          notes: editItem.notes || "",
        });
      } else {
        reset({
          accountID: 0,
          plannedAmount: 0,
          quarterID: null,
          periodID: null,
          notes: "",
        });
      }
    }
  }, [open, editItem, reset]);

  // إنشاء/تعديل البند
  const mutation = useApiMutation(
    async (data: CreateBudgetItemForm) => {
      const payload = {
        ...data,
        fiscalYearID: fiscalYearID!,
      };
      if (isEditing) {
        return budgetApi.update(editItem.budgetItemID, {
          plannedAmount: data.plannedAmount,
          notes: data.notes,
        });
      }
      return budgetApi.create(payload);
    },
    {
      successMessage: isEditing ? "تم التحديث بنجاح" : "تم الإنشاء بنجاح",
      errorTitle: "خطأ في الحفظ",
      invalidateKeys: [["budget-report"]],
      onSuccess: () => {
        onOpenChange(false);
        reset();
        onSuccess();
      },
    }
  );

  // تحويل الحسابات إلى خيارات
  const accountOptions = accounts?.map((acc: ChartOfAccountDto) => ({
    label: `${acc.accountCode} - ${acc.accountName}`,
    value: String(acc.accountID),
  })) ?? [];

  // تحويل الفصول إلى خيارات
  const quarterOptions = [
    { label: "لفصل السنة كاملة", value: "null" },
    ...(quarters?.map((q: QuarterDto) => ({
      label: q.quarterName,
      value: String(q.quarterID),
    })) ?? []),
  ];

  // تحويل الفترات الشهرية إلى خيارات
  const periodOptions = [
    { label: "للسنة كاملة", value: "null" },
    ...(periods?.map((p: FiscalPeriodDto) => ({
      label: p.periodName,
      value: String(p.periodID),
    })) ?? []),
  ];

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEditing ? "تعديل بند الموازنة" : "إضافة بند موازنة جديد"}
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel={isEditing ? "حفظ التعديلات" : "إضافة البند"}
    >
      <FormSection title="بيانات بند الموازنة" icon={BarChart3}>
        <FormField label="الحساب" required error={errors.accountID?.message}>
          <NativeSelect
            value={watch("accountID") || ""}
            options={accountOptions}
            onChange={(v) => setValue("accountID", Number(v), { shouldDirty: true })}
            placeholder="اختر الحساب"
          />
        </FormField>

        <FormField label="المبلغ المخطط" required error={errors.plannedAmount?.message}>
          <Input
            type="number"
            {...register("plannedAmount", { valueAsNumber: true })}
            placeholder="0"
          />
        </FormField>

        <FormField label="الفصل (اختياري)" error={errors.quarterID?.message}>
          <NativeSelect
            value={watch("quarterID") ?? "null"}
            options={quarterOptions}
            onChange={(v) => {
              setValue("quarterID", v === "null" ? null : Number(v), { shouldDirty: true });
              // عند اختيار فصل، نلغي اختيار الشهر
              if (v !== "null") {
                setValue("periodID", null, { shouldDirty: true });
              }
            }}
          />
        </FormField>

        <FormField label="الشهر (اختياري)" error={errors.periodID?.message}>
          <NativeSelect
            value={watch("periodID") ?? "null"}
            options={periodOptions}
            onChange={(v) => {
              setValue("periodID", v === "null" ? null : Number(v), { shouldDirty: true });
              // عند اختيار شهر، نلغي اختيار الفصل
              if (v !== "null") {
                setValue("quarterID", null, { shouldDirty: true });
              }
            }}
          />
        </FormField>

        <FormField label="ملاحظات" error={errors.notes?.message}>
          <Input {...register("notes")} placeholder="ملاحظات إضافية (اختياري)" />
        </FormField>
      </FormSection>
    </BaseFormDialog>
  );
}
