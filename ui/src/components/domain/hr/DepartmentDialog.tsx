import { zodResolver } from "@hookform/resolvers/zod";
import { Activity,FileText, Hash, Library, Type } from "lucide-react";
import { useForm } from "react-hook-form";
import * as z from "zod";

import type { DepartmentDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  FormField, 
  FormSection,
  Input, 
  NoteBox,
  Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const departmentSchema = z.object({
  code: z.string().min(1, "الكود مطلوب"),
  name: z.string().min(2, "الاسم يجب أن يكون حرفين على الأقل"),
  description: z.string().optional().nullable().or(z.literal("")),
  isActive: z.boolean(),
});

type DepartmentFormValues = z.infer<typeof departmentSchema>;

interface DepartmentModalProps {
  open?: boolean;
  isOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  onClose?: () => void;
  initialData?: DepartmentDto | null;
  /** Legacy alias for initialData */
  department?: DepartmentDto | null;
}

/**
 * Standardized DepartmentDialog.
 * Supports both open/isOpen and initialData for 100% caller compatibility.
 */

export function DepartmentDialog({
  open, 
  isOpen, 
  onOpenChange, 
  onClose, 
  initialData, 
  department 
}: DepartmentModalProps) {  const effectiveInitialData = initialData || department;
  const isEditing = !!effectiveInitialData;

  const form = useForm<DepartmentFormValues>({
    resolver: zodResolver(departmentSchema),
    defaultValues: {
      code: "",
      name: "",
      description: "",
      isActive: true,
    },
  });

  const { handleSubmit, reset, formState: { errors, isDirty } } = form;

  const mutation = useApiMutation<DepartmentDto, DepartmentFormValues>(
    (data: DepartmentFormValues) => {
      const payload = {
        ...data,
        description: data.description || undefined,
        isActive: !!data.isActive
      };
      if (isEditing) {
        return hrApi.updateDepartment(effectiveInitialData!.departmentID, payload);
      }
      return hrApi.createDepartment(payload);
    },
    {
      invalidateKeys: [["hr", "departments"], ["lookups"]],
      successMessage: isEditing ? "تم تحديث بيانات الإدارة بنجاح" : "تم إضافة الإدارة الجديدة بنجاح",
      onSuccess: () => {
        if (onOpenChange) onOpenChange(false);
        else if (onClose) onClose();
      }
    }
  );

  const onSubmit = (data: DepartmentFormValues) => {
    mutation.mutate(data);
  };

  const initialValues = effectiveInitialData ? {
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
      title={isEditing ? "تعديل بيانات الإدارة" : "إضافة إدارة جديدة"}
      description="إدارة الهيكل التنظيمي للمؤسسة"
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      resetOnOpen={false}
      reset={reset}
      initialValues={initialValues}
      showGlobalError
      formErrors={errors}
      submitLabel={isEditing ? "حفظ التعديلات" : "إضافة الإدارة"}
    >
      <div className="space-y-4">
        <FormSection title="البيانات الأساسية" icon={Library}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="كود الإدارة" error={errors.code?.message} required>
              <Input {...form.register("code")} placeholder="DEP-01" icon={Hash} />
            </FormField>

            <FormField label="اسم الإدارة" error={errors.name?.message} required>
              <Input {...form.register("name")} placeholder="اسم الإدارة بالكامل" icon={Type} />
            </FormField>
          </div>
        </FormSection>

        <FormSection title="الوصف والحالة" icon={FileText}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
            <FormField label="وصف الإدارة" error={errors.description?.message}>
              <Textarea {...form.register("description")} placeholder="وصف موجز لمهام الإدارة..." rows={3} />
            </FormField>

            <div className="flex items-center gap-3 p-4 bg-muted/30 rounded-xl border border-border mt-8">
              <Activity className="w-5 h-5 text-primary" />
              <div className="flex items-center gap-3">
                <input 
                  type="checkbox" 
                  id="isActive-dept"
                  className="min-h-[44px] min-w-[44px] w-6 h-6 text-primary border-border rounded focus:ring-primary cursor-pointer"
                  {...form.register("isActive")}
                />
                <label htmlFor="isActive-dept" className="text-sm font-bold cursor-pointer">الإدارة نشطة وتظهر في القوائم</label>
              </div>
            </div>
          </div>
        </FormSection>

        {isEditing && (
          <NoteBox type="info">
            تعديل كود الإدارة قد يؤثر على التقارير التاريخية المرتبطة بهذا الكود.
          </NoteBox>
        )}
      </div>
    </BaseFormDialog>
  );
  }
