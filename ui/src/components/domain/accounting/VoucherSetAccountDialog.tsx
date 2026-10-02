import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Controller, type Resolver,useForm } from "react-hook-form";
import { z } from "zod";

import { accountsApi } from "@/api/accounting";
import { vouchersApi } from "@/api/vouchers";
import { BaseFormDialog, FormField, NativeSelect } from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
import type { ChartOfAccountDto } from "@/types/accounting";

const schema = z.object({
  debitAccountID: z.coerce.number().min(1, "اختر حساب المصروف"),
});

type FormValues = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  voucherId: number;
  currentAccountId?: number | null;
}

export function VoucherSetAccountDialog({ open, onOpenChange, voucherId, currentAccountId }: Props) {
  const { data: accounts, isLoading } = useQuery({
    queryKey: queryKeys.accounting.accounts.postable,
    queryFn: () => accountsApi.getPostable(),
    enabled: open,
  });

  const form = useForm<FormValues>({
    resolver: zodResolver(schema) as Resolver<FormValues>,
    defaultValues: { debitAccountID: currentAccountId || 0 },
  });

  const { control, handleSubmit, formState: { errors, isDirty }, reset } = form;

  const expenseAccounts = (accounts || [])
    .filter((a: ChartOfAccountDto) => a.accountCode.startsWith("3") && a.isPostable)
    .map((a: ChartOfAccountDto) => ({ value: String(a.accountID), label: `${a.accountCode} - ${a.accountName}` }));

  const mutation = useApiMutation(
    async (values: FormValues) => {
      return vouchersApi.update(voucherId, { debitAccountID: values.debitAccountID });
    },
    {
      successMessage: "تم تحديث حساب المصروف بنجاح",
      errorTitle: "خطأ في التحديث",
      invalidateKeys: [["vouchers"]],
      onSuccess: () => onOpenChange(false),
    }
  );

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="تحديد حساب المصروف"
      description="اختر حساب المصروف لهذا السند"
      size="md"
      isLoading={mutation.isPending || isLoading}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel="حفظ"
    >
      <FormField label="حساب المصروف" required error={errors.debitAccountID?.message}>
        <Controller
          name="debitAccountID"
          control={control}
          render={({ field }) => (
            <NativeSelect
              value={field.value || ""}
              onChange={(val) => field.onChange(val ? Number(val) : 0)}
              options={expenseAccounts}
              placeholder="اختر حساب المصروف"
            />
          )}
        />
      </FormField>
    </BaseFormDialog>
  );
}
