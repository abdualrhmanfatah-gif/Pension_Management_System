import { zodResolver } from "@hookform/resolvers/zod";
import { AlertTriangle } from "lucide-react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import { journalEntriesApi } from "@/api/accounting";
import { BaseFormDialog, FormField, Text, Textarea } from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Schema ───────────────────────────────────────
const reverseSchema = z.object({
  reason: z.string().min(1, "سبب العكس مطلوب"),
});

type ReverseFormValues = z.infer<typeof reverseSchema>;

// ─── Component ────────────────────────────────────
interface JournalEntryReverseDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  entryId: number;
  entryNumber: string;
}

export function JournalEntryReverseDialog({ open, onOpenChange, entryId, entryNumber }: JournalEntryReverseDialogProps) {
  const form = useForm<ReverseFormValues>({
    resolver: zodResolver(reverseSchema),
    defaultValues: { reason: "" },
  });

  const { register, handleSubmit, formState: { errors, isDirty }, reset } = form;

  const mutation = useApiMutation(
    (values: ReverseFormValues) => journalEntriesApi.reverse(entryId, values.reason),
    {
      successMessage: "تم عكس القيد بنجاح",
      errorTitle: "خطأ في عكس القيد",
      invalidateKeys: [["journal-entries"], ["vouchers"], ["vouchers-quarter"]],
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
      title="عكس القيد اليومي"
      description={`عكس القيد ${entryNumber}`}
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel="تأكيد العكس"
      submitVariant="destructive"
      submitIcon={<AlertTriangle className="w-4 h-4" />}
    >
      <div className="flex flex-col items-center gap-4">
        <div className="w-14 h-14 rounded-full bg-destructive/10 flex items-center justify-center">
          <AlertTriangle className="w-7 h-7 text-destructive" />
        </div>
        <Text variant="description" className="text-center">
          سيتم إنشاء قيد عكسي بقيم معاكسة للقيد الأصلي.
        </Text>
      </div>

      <FormField label="سبب العكس" required error={errors.reason?.message}>
        <Textarea
          {...register("reason")}
          placeholder="اذكر سبب العكس..."
          className="resize-none"
          rows={3}
        />
      </FormField>
    </BaseFormDialog>
  );
}
