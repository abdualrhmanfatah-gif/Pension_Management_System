import { zodResolver } from "@hookform/resolvers/zod";
import { Calendar } from "lucide-react";
import { useEffect } from "react";
<<<<<<< HEAD
import { Controller,type Resolver, useForm } from "react-hook-form";
=======
import { type Resolver, useForm, Controller } from "react-hook-form";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { z } from "zod";

import { fiscalYearsApi } from "@/api/accounting";
import { BaseFormDialog, DateInput, FormField, FormSection, Input } from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Schema ───────────────────────────────────────
const schema = z.object({
  yearName: z.string().min(1, "اسم السنة مطلوب").regex(/^\d{4}$/, "يجب أن يكون سنة (مثل 2027)"),
  startDate: z.string().min(1, "تاريخ البداية مطلوب"),
  endDate: z.string().min(1, "تاريخ النهاية مطلوب"),
  totalBudget: z.coerce.number().min(0, "الميزانية يجب أن تكون صفر أو أكثر"),
});

type FormValues = z.infer<typeof schema>;

// ─── Props ────────────────────────────────────────
interface FiscalYearFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

// ─── Component ────────────────────────────────────
export function FiscalYearFormDialog({ open, onOpenChange }: FiscalYearFormDialogProps) {
  const form = useForm<FormValues>({
    resolver: zodResolver(schema) as Resolver<FormValues>,
    defaultValues: {
      yearName: "",
      startDate: "",
      endDate: "",
      totalBudget: 0,
    },
  });

  const { register, handleSubmit, formState: { errors, isDirty }, reset, watch, setValue, control } = form;
  const yearName = watch("yearName");

  // ─── Auto-fill dates when year changes ──────────
  useEffect(() => {
    if (open) {
      reset({ yearName: "", startDate: "", endDate: "", totalBudget: 0 });
    }
  }, [open, reset]);

  useEffect(() => {
    if (yearName && /^\d{4}$/.test(yearName)) {
      setValue("startDate", `${yearName}-01-01`);
      setValue("endDate", `${yearName}-12-31`);
    }
  }, [yearName, setValue]);

  // ─── Mutation ────────────────────────────────────
  const mutation = useApiMutation(
    async (data: FormValues) => {
      return fiscalYearsApi.create(data);
    },
    {
      successMessage: "تم إنشاء السنة المالية والفترات والفصول بنجاح",
      errorTitle: "خطأ في الإنشاء",
      invalidateKeys: [["fiscal-years"], ["periods"], ["quarters"]],
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
      title="إضافة سنة مالية جديدة"
      description="ستُنشأ تلقائياً 12 فترة شهرية و4 فصول مالية"
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel="إنشاء السنة المالية"
      submitIcon={<Calendar className="w-4 h-4" />}
    >
      <FormSection title="بيانات السنة المالية" icon={Calendar}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="اسم السنة المالية" required error={errors.yearName?.message}>
            <Input {...register("yearName")} placeholder="مثال: 2027" maxLength={4} />
          </FormField>
          <FormField label="الميزانية الإجمالية" error={errors.totalBudget?.message}>
            <Input {...register("totalBudget")} type="number" min={0} placeholder="0" />
          </FormField>
          <FormField label="تاريخ البداية" required error={errors.startDate?.message}>
            <Controller
              control={control}
              name="startDate"
              render={({ field }) => (
                <DateInput value={field.value} onChange={field.onChange} />
              )}
            />
          </FormField>
          <FormField label="تاريخ النهاية" required error={errors.endDate?.message}>
            <Controller
              control={control}
              name="endDate"
              render={({ field }) => (
                <DateInput value={field.value} onChange={field.onChange} />
              )}
            />
          </FormField>
        </div>
      </FormSection>
    </BaseFormDialog>
  );
}
