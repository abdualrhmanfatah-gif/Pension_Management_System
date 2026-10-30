import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { CalendarDays, FileText,User } from "lucide-react";
import type { SubmitHandler } from "react-hook-form";
import { Controller,useForm } from "react-hook-form";
import * as z from "zod";

import type { LeaveRequestDetailsDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
<<<<<<< HEAD
  DateInput,
  FormField,
  FormSection,
  Input,
=======
  FormField,
  FormSection,
  Input,
  DateInput,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
  NativeSelect,
  Textarea} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const leaveSchema = z.object({
  employeeID: z.string().min(1, "الموظف مطلوب"),
  leaveTypeID: z.string().min(1, "نوع الإجازة مطلوب"),
  startDate: z.string().min(1, "تاريخ البدء مطلوب"),
  endDate: z.string().min(1, "تاريخ الانتهاء مطلوب"),
  reason: z.string().min(2, "السبب مطلوب"),
  notes: z.string().trim().optional().or(z.literal("")),
}).refine(data => {
  if (!data.startDate || !data.endDate) return true;
  return new Date(data.endDate) >= new Date(data.startDate);
}, {
  message: "تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء",
  path: ["endDate"]
});

type LeaveFormValues = z.infer<typeof leaveSchema>;

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSaved?: () => void;
  initialData?: LeaveRequestDetailsDto | null;
  /** Legacy alias for initialData */
  data?: LeaveRequestDetailsDto | null;
}

/**
 * Standardized Leave Request Dialog.
 * Fixed: Unified Props API and cleaned Zod schema.
 */
export function LeaveRequestDialog({ 
  open,
  onOpenChange,
  onSaved, 
  data,
  initialData 
}: Props) {
  const leaveRequest = initialData || data;
  const isEditing = !!leaveRequest;

  // Fetch reference data
  const employeesQuery = useQuery({
    queryKey: ["hr", "employees-list-simple"],
    queryFn: () => hrApi.getEmployees({ pageSize: 1000 }),
    enabled: open
  });

  const employees = employeesQuery.data?.items || [];

  const { data: leaveTypes } = useQuery({
    queryKey: ["hr", "leave-types"],
    queryFn: () => hrApi.getLeaveTypes(),
    enabled: open
  });

  const form = useForm<LeaveFormValues>({
    resolver: zodResolver(leaveSchema),
    defaultValues: {
      employeeID: "",
      leaveTypeID: "",
      startDate: new Date().toISOString().split('T')[0],
      endDate: new Date().toISOString().split('T')[0],
      reason: "",
      notes: "",
    },
  });

  const { control, handleSubmit, reset, formState: { errors, isDirty } } = form;

  const mutation = useApiMutation<LeaveRequestDetailsDto | boolean, LeaveFormValues>(
    (values: LeaveFormValues) => {
      const payload = {
        ...values,
        employeeID: Number(values.employeeID),
        leaveTypeID: Number(values.leaveTypeID),
        notes: values.notes || undefined
      };
      return isEditing 
        ? hrApi.updateLeaveRequest(leaveRequest!.leaveRequestID, payload) 
        : hrApi.createLeaveRequest(payload);
    },
    {
      successMessage: isEditing ? "تم تحديث طلب الإجازة" : "تم تقديم طلب الإجازة بنجاح",
      invalidateKeys: [["hr", "leaves"]],
      onSuccess: () => {
        if (onSaved) onSaved();
        onOpenChange(false);
      }
    }
  );

  const onSubmit: SubmitHandler<LeaveFormValues> = (values) => {
    mutation.mutate(values);
  };

  const initialValues = leaveRequest ? {
    employeeID: String(leaveRequest.employeeID),
    leaveTypeID: String(leaveRequest.leaveTypeID),
    startDate: new Date(leaveRequest.startDate).toISOString().split('T')[0],
    endDate: new Date(leaveRequest.endDate).toISOString().split('T')[0],
    reason: leaveRequest.reason || "",
    notes: leaveRequest.notes || "",
  } : undefined;

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEditing ? "تعديل طلب إجازة" : "تقديم طلب إجازة جديد"}
      description={isEditing ? "تعديل بيانات طلب الإجازة الحالي" : "أدخل تفاصيل طلب الإجازة للموظف"}
      size="lg"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      resetOnOpen={false}
      reset={reset}
      initialValues={initialValues}
      showGlobalError
      formErrors={errors}
      submitLabel={isEditing ? "حفظ التعديلات" : "تقديم الطلب"}
    >
      <div className="space-y-4">
        <FormSection title="بيانات الموظف ونوع الإجازة" icon={User}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="الموظف" required error={errors.employeeID?.message}>
              <Controller
                name="employeeID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={employees?.map(e => ({ label: `${e.fullName} (${e.employeeNumber})`, value: String(e.employeeID) })) || []}
                    value={field.value}
                    onChange={field.onChange}
                    placeholder="اختر الموظف..."
                  />
                )}
              />
            </FormField>

            <FormField label="نوع الإجازة" required error={errors.leaveTypeID?.message}>
              <Controller
                name="leaveTypeID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={leaveTypes?.map(t => ({ label: t.name, value: String(t.leaveTypeID) })) || []}
                    value={field.value}
                    onChange={field.onChange}
                    placeholder="اختر النوع..."
                  />
                )}
              />
            </FormField>
          </div>
        </FormSection>

        <FormSection title="الجدول الزمني والسبب" icon={CalendarDays}>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <FormField label="من تاريخ" required error={errors.startDate?.message}>
              <Controller
                name="startDate"
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

            <FormField label="إلى تاريخ" required error={errors.endDate?.message}>
              <Controller
                name="endDate"
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

            <Input
              label="السبب"
              placeholder="سبب طلب الإجازة"
              {...form.register("reason")}
              error={errors.reason?.message}
              required
            />
          </div>
        </FormSection>

        <FormSection title="ملاحظات إضافية" icon={FileText}>
          <div className="grid grid-cols-1 gap-4">
            <Textarea
              placeholder="أي تفاصيل إضافية..."
              {...form.register("notes")}
              error={errors.notes?.message}
              rows={3}
            />
          </div>
        </FormSection>
      </div>
    </BaseFormDialog>
  );
}
