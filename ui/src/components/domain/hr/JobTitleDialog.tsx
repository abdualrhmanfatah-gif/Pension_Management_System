import { zodResolver } from "@hookform/resolvers/zod";
import { Activity,Briefcase, FileText, Hash, Type } from "lucide-react";
import { useForm } from "react-hook-form";
import * as z from "zod";

import type { JobTitleDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  FormField, 
  FormSection,
  Input, 
  NoteBox,
  Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const jobTitleSchema = z.object({
  code: z.string().min(1, "الكود مطلوب"),
  name: z.string().min(2, "الاسم يجب أن يكون حرفين على الأقل"),
  description: z.string().optional().nullable().or(z.literal("")),
  isActive: z.boolean(),
});

type JobTitleFormValues = z.infer<typeof jobTitleSchema>;

interface JobTitleModalProps {
  open?: boolean;
  isOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  onClose?: () => void;
  initialData?: JobTitleDto | null;
  /** Legacy alias for initialData */
  jobTitle?: JobTitleDto | null;
}

/**
 * Standardized JobTitleDialog.
 * Supports both open/isOpen and initialData/jobTitle for 100% caller compatibility.
 */

export function JobTitleDialog({
  open, 
  isOpen, 
  onOpenChange, 
  onClose, 
  initialData, 
  jobTitle 
}: JobTitleModalProps) {  const effectiveInitialData = initialData || jobTitle;
  const isEditing = !!effectiveInitialData;

  const form = useForm<JobTitleFormValues>({
    resolver: zodResolver(jobTitleSchema),
    defaultValues: {
      code: "",
      name: "",
      description: "",
      isActive: true,
    },
  });

  const { handleSubmit, reset, formState: { errors, isDirty } } = form;

  const mutation = useApiMutation<JobTitleDto, JobTitleFormValues>(
    (data: JobTitleFormValues) => {
      const payload = {
        ...data,
        description: data.description || undefined
      };
      if (isEditing) {
        return hrApi.updateJobTitle(effectiveInitialData!.jobTitleID, payload);
      }
      return hrApi.createJobTitle(payload);
    },
    {
      successMessage: isEditing ? "تم تحديث المسمى الوظيفي بنجاح" : "تم إضافة المسمى الوظيفي بنجاح",
      invalidateKeys: [["hr", "jobTitles"], ["lookups"]],
      onSuccess: () => {
        if (onOpenChange) onOpenChange(false);
        else if (onClose) onClose();
      }
    }
  );

  const onSubmit = (data: JobTitleFormValues) => {
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
      title={isEditing ? "تعديل مسمى وظيفي" : "إضافة مسمى وظيفي جديد"}
      description="إدارة قوائم المسميات الوظيفية المعتمدة"
      size="md"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      resetOnOpen={false}
      reset={reset}
      initialValues={initialValues}
      showGlobalError
      formErrors={errors}
      submitLabel={isEditing ? "حفظ التعديلات" : "إضافة المسمى"}
    >
      <div className="space-y-4">
        <FormSection title="البيانات الأساسية" icon={Briefcase}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="كود المسمى" error={errors.code?.message} required>
              <Input {...form.register("code")} placeholder="JT-01" icon={Hash} />
            </FormField>

            <FormField label="المسمى الوظيفي" error={errors.name?.message} required>
              <Input {...form.register("name")} placeholder="اسم المسمى الوظيفي" icon={Type} />
            </FormField>
          </div>
        </FormSection>

        <FormSection title="الوصف والحالة" icon={FileText}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-start">
            <FormField label="الوصف" error={errors.description?.message}>
              <Textarea {...form.register("description")} placeholder="وصف المسمى الوظيفي ومهامه..." rows={3} />
            </FormField>

            <div className="flex items-center gap-3 p-4 bg-muted/30 rounded-xl border border-border mt-8">
              <Activity className="w-5 h-5 text-primary" />
              <div className="flex items-center gap-3">
                <input 
                  type="checkbox" 
                  id="isActive-jt"
                  className="w-5 h-5 text-primary border-border rounded focus:ring-primary cursor-pointer"
                  {...form.register("isActive")}
                />
                <label htmlFor="isActive-jt" className="text-sm font-bold cursor-pointer">المسمى نشط ويظهر في القوائم</label>
              </div>
            </div>
          </div>
        </FormSection>

        {isEditing && (
          <NoteBox type="info">
            تعديل اسم المسمى سيؤثر على ظهوره في كشوفات الموظفين الحالية والمستقبلية.
          </NoteBox>
        )}
      </div>
    </BaseFormDialog>
  );
  }
