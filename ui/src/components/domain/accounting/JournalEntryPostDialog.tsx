import { AlertTriangle } from "lucide-react";

import { journalEntriesApi } from "@/api/accounting";
import { BaseFormDialog, Text } from "@/components/ui";
import { useApiMutation } from "@/hooks";

interface JournalEntryPostDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  entryId: number;
  entryNumber: string;
}

export function JournalEntryPostDialog({ open, onOpenChange, entryId, entryNumber }: JournalEntryPostDialogProps) {
  const mutation = useApiMutation(
    () => journalEntriesApi.post(entryId),
    {
      successMessage: "تم ترحيل القيد بنجاح",
      errorTitle: "خطأ في الترحيل",
      invalidateKeys: [["journal-entries"], ["vouchers"], ["vouchers-quarter"]],
      onSuccess: () => onOpenChange(false),
    }
  );

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="تأكيد ترحيل القيد"
      size="sm"
      isLoading={mutation.isPending}
      onSubmit={() => mutation.mutate(undefined as void)}
      submitLabel="ترحيل القيد"
      submitVariant="warning"
      submitIcon={<AlertTriangle className="w-4 h-4" />}
    >
      <div className="flex flex-col items-center gap-4 py-4">
        <div className="w-14 h-14 rounded-full bg-amber-100 flex items-center justify-center">
          <AlertTriangle className="w-7 h-7 text-amber-600" />
        </div>
        <p className="text-center">
          هل أنت متأكد من ترحيل القيد <span className="font-medium font-mono" dir="ltr">{entryNumber}</span>؟
        </p>
        <Text variant="description" className="text-center">
          بعد الترحيل لا يمكن تعديل القيد. يمكنك عكسه لاحقاً إذا لزم الأمر.
        </Text>
      </div>
    </BaseFormDialog>
  );
}
