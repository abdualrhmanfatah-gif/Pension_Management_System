import { zodResolver } from "@hookform/resolvers/zod";
import { Layers } from "lucide-react";
import { useEffect } from "react";
import { type Resolver, useForm } from "react-hook-form";
import { z } from "zod";

import { costCentersApi } from "@/api/accounting";
<<<<<<< HEAD
import { BaseFormDialog, FormField, FormSection, Input } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import type { CostCenterDto } from "@/types/accounting";
=======
import type { CostCenterDto } from "@/types/accounting";
import { BaseFormDialog, FormField, FormSection, Input } from "@/components/ui";
import { useApiMutation } from "@/hooks";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Schema ───────────────────────────────────────
const schema = z.object({
  code: z.string().min(1, "الكود مطلوب"),
  name: z.string().min(1, "الاسم مطلوب"),
});

type FormValues = z.infer<typeof schema>;

// ─── Props ────────────────────────────────────────
interface CostCenterFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  costCenter?: CostCenterDto | null;
  parentCostCenter?: CostCenterDto | null;
}

// ─── Component ────────────────────────────────────
export function CostCenterFormDialog({ open, onOpenChange, costCenter, parentCostCenter }: CostCenterFormDialogProps) {
  const isEditing = !!costCenter;

  const form = useForm<FormValues>({
    resolver: zodResolver(schema) as Resolver<FormValues>,
    defaultValues: { code: "", name: "" },
  });

  const { register, handleSubmit, formState: { errors, isDirty }, reset } = form;

  // ─── Reset on open ───────────────────────────────
  useEffect(() => {
    if (open) {
      if (costCenter) {
        reset({ code: costCenter.code, name: costCenter.name });
      } else {
        reset({ code: "", name: "" });
      }
    }
  }, [open, costCenter, reset]);

  // ─── Mutation ────────────────────────────────────
  const mutation = useApiMutation(
    async (data: FormValues) => {
      if (isEditing) {
        return costCentersApi.update(costCenter!.costCenterID, {
          ...data,
          parentCostCenterID: parentCostCenter?.costCenterID ?? null,
          isActive: costCenter?.isActive ?? true,
        });
      }
      return costCentersApi.create({
        ...data,
        parentCostCenterID: parentCostCenter?.costCenterID ?? null,
      });
    },
    {
      successMessage: isEditing ? "تم تحديث مركز التكلفة بنجاح" : "تم إنشاء مركز التكلفة بنجاح",
      errorTitle: "خطأ في الحفظ",
      invalidateKeys: [["cost-centers"]],
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
      title={isEditing ? "تعديل مركز تكلفة" : "إضافة مركز تكلفة"}
      description={parentCostCenter ? `تحت: ${parentCostCenter.code} - ${parentCostCenter.name}` : undefined}
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      formErrors={errors}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen
      reset={reset}
      submitLabel={isEditing ? "حفظ التعديلات" : "إنشاء المركز"}
    >
      <FormSection title="بيانات مركز التكلفة" icon={Layers}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="الكود" required error={errors.code?.message}>
            <Input {...register("code")} placeholder="مثال: CC-001" />
          </FormField>
          <FormField label="الاسم" required error={errors.name?.message}>
            <Input {...register("name")} placeholder="اسم مركز التكلفة" />
          </FormField>
        </div>
      </FormSection>
    </BaseFormDialog>
  );
}
