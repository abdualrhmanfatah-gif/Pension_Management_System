import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Landmark, Receipt } from "lucide-react";
import { useEffect, useMemo } from "react";
import { Controller, type Resolver,useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { accountsApi } from "@/api/accounting";
import { type CreateVoucherDto,vouchersApi, type VoucherType } from "@/api/vouchers";
import {
  BaseFormDialog,
  Button,
  DateInput,
  FormField,
  FormSection,
  Input,
  NativeSelect,
  NoteBox,
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
import type { ChartOfAccountDto } from "@/types/accounting";

// ─── Schema ───────────────────────────────────────
const voucherSchema = z.object({
  voucherType: z.number().min(1).max(2),
  voucherDate: z.string().min(1, "التاريخ مطلوب"),
  payeeName: z.string().optional().or(z.literal("")),
  amount: z.coerce.number().min(0.01, "المبلغ يجب أن أكبر من صفر"),
  description: z.string().min(1, "البيان مطلوب"),
  debitAccountID: z.coerce.number().min(1, "اختر حساب المدين"),
  creditAccountID: z.coerce.number().min(1, "اختر حساب الدائن"),
  chequeNumber: z.string().optional().or(z.literal("")),
  referralSource: z.string().optional().or(z.literal("")),
});

type VoucherFormValues = z.infer<typeof voucherSchema>;

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function VoucherFormDialog({ open, onOpenChange }: Props) {
  // ─── Accounts Query ──────────────────────────────
  const { data: accounts, isLoading: accountsLoading } = useQuery({
    queryKey: queryKeys.accounting.accounts.postable,
    queryFn: () => accountsApi.getPostable(),
    enabled: open,
  });

  // ─── Form ────────────────────────────────────────
  const form = useForm<VoucherFormValues>({
    resolver: zodResolver(voucherSchema) as Resolver<VoucherFormValues>,
    defaultValues: {
      voucherType: 1,
      voucherDate: new Date().toISOString().split("T")[0],
      payeeName: "",
      amount: 0,
      description: "",
      debitAccountID: 0,
      creditAccountID: 0,
      chequeNumber: "",
      referralSource: "",
    },
  });

  const { control, register, setValue, handleSubmit, formState: { errors, isDirty }, reset } = form;

  // ─── Watch type to filter accounts ───────────────
  const voucherType = useWatch({ control, name: "voucherType" });

  const expenseAccounts = useMemo(
    () => accounts?.filter((a: ChartOfAccountDto) => a.accountCode.startsWith("3") && a.isPostable) || [],
    [accounts]
  );
  const revenueAccounts = useMemo(
    () => accounts?.filter((a: ChartOfAccountDto) => a.accountCode.startsWith("4") && a.isPostable) || [],
    [accounts]
  );
  const treasuryAccounts = useMemo(
    () => accounts?.filter((a: ChartOfAccountDto) => a.accountCode.startsWith("18") && a.isPostable) || [],
    [accounts]
  );

  const debitAccountOptions = useMemo(() => {
    const list = voucherType === 1 ? expenseAccounts : treasuryAccounts;
    return list.map((a: ChartOfAccountDto) => ({ value: String(a.accountID), label: `${a.accountCode} - ${a.accountName}` }));
  }, [voucherType, expenseAccounts, treasuryAccounts]);

  const creditAccountOptions = useMemo(() => {
    const list = voucherType === 1 ? treasuryAccounts : revenueAccounts;
    return list.map((a: ChartOfAccountDto) => ({ value: String(a.accountID), label: `${a.accountCode} - ${a.accountName}` }));
  }, [voucherType, treasuryAccounts, revenueAccounts]);

  // ─── Auto-set accounts when type changes ─────────
  useEffect(() => {
    if (!open) return;
    setValue("debitAccountID", 0);
    setValue("creditAccountID", 0);
  }, [voucherType, open, setValue]);

  // ─── Mutation ────────────────────────────────────
  const mutation = useApiMutation(
    async (values: VoucherFormValues) => {
      const dto: CreateVoucherDto = {
        voucherType: values.voucherType as VoucherType,
        voucherDate: values.voucherDate,
        payeeName: values.voucherType === 1 ? values.payeeName || undefined : undefined,
        payerName: values.voucherType === 2 ? values.payeeName || undefined : undefined,
        amount: values.amount,
        description: values.description,
        debitAccountID: values.debitAccountID,
        creditAccountID: values.creditAccountID,
        chequeNumber: values.chequeNumber || undefined,
        referralSource: values.referralSource || undefined,
      };
      return vouchersApi.create(dto);
    },
    {
      successMessage: "تم إنشاء السند بنجاح",
      errorTitle: "خطأ في إنشاء السند",
      invalidateKeys: [["vouchers"]],
      onSuccess: () => {
        onOpenChange(false);
        form.reset();
      },
    }
  );

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={voucherType === 1 ? "سند صرف" : "سند قبض"}
      description="إنشاء سند مالي جديد"
      size="lg"
      isLoading={mutation.isPending || accountsLoading}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel="إنشاء السند"
    >
      <div className="space-y-4">
        {/* ─── Type Toggle ─── */}
        <div className="flex gap-2">
          <Controller
            name="voucherType"
            control={control}
            render={({ field }) => (
              <>
                <input type="hidden" {...field} />
                <Button
                  type="button"
                  variant={field.value === 1 ? "default" : "outline"}
                  className="flex-1"
                  onClick={() => field.onChange(1)}
                >
                  سند صرف
                </Button>
                <Button
                  type="button"
                  variant={field.value === 2 ? "default" : "outline"}
                  className="flex-1"
                  onClick={() => field.onChange(2)}
                >
                  سند قبض
                </Button>
              </>
            )}
          />
        </div>

        {/* ─── بيانات السند ─── */}
        <FormSection title="بيانات السند" icon={Receipt}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="التاريخ" required error={errors.voucherDate?.message}>
              <Controller
                name="voucherDate"
                control={control}
                render={({ field }) => (
                  <DateInput
                    id={field.name}
                    value={field.value || undefined}
                    onChange={field.onChange}
                  />
                )}
              />
            </FormField>

            <FormField
              label={voucherType === 1 ? "المستفيد" : "الدافع"}
              required
              error={errors.payeeName?.message}
            >
              <Input
                {...register("payeeName")}
                placeholder={voucherType === 1 ? "اسم المستفيد" : "اسم الدافع"}
              />
            </FormField>

            <FormField label="المبلغ (ريال يمني)" required error={errors.amount?.message}>
              <Input
                type="number"
                {...register("amount")}
                placeholder="0"
                min="0"
              />
            </FormField>

            <FormField label="رقم الشيك / المرجع" error={errors.chequeNumber?.message}>
              <Input {...register("chequeNumber")} placeholder="اختياري" />
            </FormField>

            <FormField label="مصدر التوجيه" error={errors.referralSource?.message}>
              <Input {...register("referralSource")} placeholder="مصدر التوجيه (اختياري)" />
            </FormField>
          </div>

          <FormField label="البيان" required error={errors.description?.message}>
            <Input {...register("description")} placeholder="بيان السند..." />
          </FormField>
        </FormSection>

        {/* ─── الحسابات ─── */}
        <FormSection title="الحسابات المحاسبية" icon={Landmark}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField
              label={`حساب المدين (${voucherType === 1 ? "مصروف" : "خزينة"})`}
              required
              error={errors.debitAccountID?.message}
            >
              <Controller
                name="debitAccountID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    value={field.value || ""}
                    onChange={(val) => field.onChange(val ? Number(val) : 0)}
                    options={debitAccountOptions}
                    placeholder="اختر حساب المدين"
                  />
                )}
              />
            </FormField>

            <FormField
              label={`حساب الدائن (${voucherType === 1 ? "خزينة" : "إيراد"})`}
              required
              error={errors.creditAccountID?.message}
            >
              <Controller
                name="creditAccountID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    value={field.value || ""}
                    onChange={(val) => field.onChange(val ? Number(val) : 0)}
                    options={creditAccountOptions}
                    placeholder="اختر حساب الدائن"
                  />
                )}
              />
            </FormField>
          </div>

          <NoteBox type="info" title="القيد المحاسبي المتوقع">
            {voucherType === 1
              ? "مدين: مصروفات (3XXX) ← دائن: الخزينة (18XX)"
              : "مدين: الخزينة (18XX) ← دائن: إيرادات (4XXX)"}
          </NoteBox>
        </FormSection>
      </div>
    </BaseFormDialog>
  );
}
