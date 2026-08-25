import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Activity,FileText, Hash, LayoutGrid, Library, Type } from "lucide-react";
import { Controller,useForm } from "react-hook-form";
import * as z from "zod";

import type { SectionDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  Checkbox,
  FormField, 
  FormSection,
  Input, 
  NoteBox,
  NativeSelect,
  Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const sectionSchema = z.object({
  departmentID: z.number().min(1, "الإدارة مطلوبة"),
  code: z.string().min(1, "الكود مطلوب"),
  name: z.string().min(2, "الاسم يجب أن يكون حرفين على الأقل"),
  description: z.string().optional().nullable().or(z.literal("")),
  isActive: z.boolean(),
});

type SectionFormValues = z.infer<typeof sectionSchema>;

interface SectionModalProps {
  open?: boolean;
  isOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  onClose?: () => void;
  initialData?: SectionDto | null;
  /** Legacy alias for initialData */
  section?: SectionDto | null;
}

/**
 * Standardized SectionDialog.
 * Supports both open/isOpen and initialData/section for 100% caller compatibility.
 */
export function SectionDialog({
  open, 
  isOpen, 
  onOpenChange, 
  onClose, 
  initialData, 
  section 
}: SectionModalProps) {
  const effectiveInitialData = initialData || section;
  const isEditing = !!effectiveInitialData;

  const isDialogActive = !!open || !!isOpen;

  const { data: departments = [] } = useQuery({
    queryKey: ["hr", "departments"],
    queryFn: () => hrApi.getDepartments(),
    enabled: isDialogActive,
  });

  const form = useForm<SectionFormValues>({
    resolver: zodResolver(sectionSchema),
    defaultValues: {
      departmentID: 0,
      code: "",
      name: "",
      description: "",
      isActive: true,
    },
  });

  const { control, handleSubmit, reset, watch, setValue, formState: { errors, isDirty } } = form;

  const mutation = useApiMutation<SectionDto, SectionFormValues>(
    (data: SectionFormValues) => {
      const payload = {
        ...data,
        description: data.description || undefined
      };
      if (isEditing) {
        return hrApi.updateSection(effectiveInitialData!.sectionID, payload);
      }
      return hrApi.createSection(payload);
    },
    {
      successMessage: isEditing ? "تم تحديث بيانات القسم بنجاح" : "تم إضافة القسم الجديد بنجاح",
      invalidateKeys: [["hr", "sections"], ["lookups"]],
      onSuccess: () => {
        if (onOpenChange) onOpenChange(false);
        else if (onClose) onClose();
      }
    }
  );

  const onSubmit = (data: SectionFormValues) => {
    mutation.mutate(data);
  };

  const initialValues = effectiveInitialData ? {
    departmentID: effectiveInitialData.departmentID,
    code: effectiveInitialData.code,
    name: effectiveInitialData.name,
    description: effectiveInitialData.description || "",
    isActive: effectiveInitialData.isActive,
  } : undefined;

  return (
    <BaseFormDialog
      open={open}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      onClose={onClose}
      title={isEditing ? "تعديل بيانات القسم" : "إضافة قسم جديد"}
      description="إدارة الأقسام والوحدات التابعة للإدارات"
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      resetOnOpen={false}
      reset={reset}
      initialValues={initialValues}
      showGlobalError
      formErrors={errors}
      submitLabel={isEditing ? "حفظ التعديلات" : "إضافة القسم"}
    >
      <div className="space-y-4">
        <FormSection title="التبعية الإدارية" icon={Library}>
          <div className="grid grid-cols-1 gap-4">
            <FormField label="الإدارة التابع لها" required error={errors.departmentID?.message}>
              <Controller
                name="departmentID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    value={field.value?.toString() || ""}
                    onChange={(val) => field.onChange(Number(val))}
                    options={departments.map((d) => ({
                      value: d.departmentID.toString(),
                      label: d.name,
                    }))}
                    placeholder="اختر الإدارة الأم..."
                  />
                )}
              />
            </FormField>
          </div>
        </FormSection>

        <FormSection title="البيانات الأساسية للقسم" icon={LayoutGrid}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="كود القسم" error={errors.code?.message} required>
              <Input {...form.register("code")} placeholder="SEC-01" icon={Hash} />
            </FormField>

            <FormField label="اسم القسم" error={errors.name?.message} required>
              <Input {...form.register("name")} placeholder="اسم القسم بالكامل" icon={Type} />
            </FormField>
          </div>
        </FormSection>

        <FormSection title="الوصف والحالة" icon={FileText}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
            <FormField label="وصف القسم" error={errors.description?.message}>
              <Textarea {...form.register("description")} placeholder="وصف موجز لمهام القسم..." rows={3} />
            </FormField>

            <div className="flex items-center gap-3 p-4 bg-muted/30 rounded-xl border border-border/40 mt-8">
              <Activity className="w-5 h-5 text-primary" />
              <div className="flex items-center gap-3">
                <Checkbox
                  id="isActive-sec"
                  checked={watch("isActive")}
                  onCheckedChange={(val) => setValue("isActive", val)}
                  label="القسم نشط ويظهر في القوائم"
                />
              </div>
            </div>
          </div>
        </FormSection>

        {isEditing && (
          <NoteBox type="info">
            تعديل تبعية القسم قد يؤثر على تسكين الموظفين الحاليين المرتبطين به.
          </NoteBox>
        )}
      </div>
    </BaseFormDialog>
  );
}
