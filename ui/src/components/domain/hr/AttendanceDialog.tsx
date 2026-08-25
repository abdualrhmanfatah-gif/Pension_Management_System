import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Clock, User } from "lucide-react";
import { Controller,useForm } from "react-hook-form";
import * as z from "zod";

import { type AttendanceEventDto,hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  type BaseOverlayProps,
  FormField,
  FormSection,
  Input, 
  NoteBox,
  NativeSelect} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const attendanceSchema = z.object({
  employeeID: z.number().min(1, "الموظف مطلوب"),
  checkInTime: z.string().min(1, "وقت الحضور مطلوب"),
  checkOutTime: z.string().optional().nullable(),
  status: z.string().min(1, "الحالة مطلوبة"),
});

type AttendanceFormValues = z.infer<typeof attendanceSchema>;

interface Props extends BaseOverlayProps {
  onSaved?: () => void;
  initialDate?: Date;
  employeeId?: number;
}

// ─── Component ─────────────────────────────────────────


export function AttendanceDialog({
  open, 
  onOpenChange, 
  onSaved, 
  initialDate, 
  employeeId 
}: Props) {  const form = useForm<AttendanceFormValues>({
    resolver: zodResolver(attendanceSchema),
    defaultValues: {
      employeeID: employeeId || 0,
      checkInTime: "",
      checkOutTime: "",
      status: "Present",
    },
  });

  const { register, handleSubmit, control, formState: { errors, isDirty } } = form;

  const employeesDataQuery = useQuery({
    queryKey: ["hr", "employees-simple"],
    queryFn: () => hrApi.getEmployees({ pageSize: 1000 }),
    enabled: !!open
  });

  const employees = employeesDataQuery.data?.items || [];

  const mutation = useApiMutation<AttendanceEventDto, AttendanceFormValues>(
    (values: AttendanceFormValues) => {
        // Map values to CreateAttendanceEventDto
        return hrApi.recordAttendance({
            employeeID: values.employeeID,
            eventDate: values.checkInTime.split('T')[0],
            eventTime: values.checkInTime.split('T')[1],
            eventType: values.status,
            notes: `سجل يدوي - ${values.status}`
        });
    },
    {
      successMessage: "تم تسجيل الحضور بنجاح",
      invalidateKeys: [["hr", "attendance"]],
      onSuccess: () => {
        if (onSaved) onSaved();
        if (onOpenChange) onOpenChange(false);
      }
    }
  );

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="تسجيل حضور يدوي"
      description="إضافة سجل حضور لموظف في تاريخ محدد"
      size="lg"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit((v) => mutation.mutate(v))}
      resetOnOpen={false}
      reset={form.reset}
      initialValues={{
          employeeID: employeeId || 0,
          checkInTime: initialDate ? `${initialDate.toISOString().split('T')[0]}T08:00` : "",
          checkOutTime: initialDate ? `${initialDate.toISOString().split('T')[0]}T16:00` : "",
      }}
      showGlobalError
      formErrors={errors}
      submitLabel="حفظ السجل"
    >
      <div className="space-y-4">
        <FormSection title="بيانات الموظف والحالة" icon={User}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField label="الموظف" required error={errors.employeeID?.message as string}>
                    <Controller
                        name="employeeID"
                        control={control}
                        render={({ field }) => (
                            <NativeSelect
                                options={employees.map(e => ({ label: `${e.fullName} (${e.employeeNumber})`, value: String(e.employeeID) }))}
                                value={String(field.value || "")}
                                onChange={(v) => field.onChange(Number(v))}
                                placeholder="اختر الموظف..."
                            />
                        )}
                    />
                </FormField>
                <FormField label="حالة الحضور" required error={errors.status?.message as string}>
                     <Controller
                        name="status"
                        control={control}
                        render={({ field }) => (
                             <NativeSelect
                                options={[
                                    {label: "حاضر", value: "Present"},
                                    {label: "غائب", value: "Absent"},
                                    {label: "إجازة", value: "On Leave"},
                                    {label: "مهمة عمل", value: "On Duty"},
                                ]}
                                {...field}
                            />
                        )}
                    />
                </FormField>
            </div>
        </FormSection>

        <FormSection title="توقيت الحضور والانصراف" icon={Clock}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField label="وقت الحضور" required error={errors.checkInTime?.message as string}>
                    <Input type="datetime-local" {...register("checkInTime")} />
                </FormField>
                <FormField label="وقت الانصراف" error={errors.checkOutTime?.message as string}>
                    <Input type="datetime-local" {...register("checkOutTime")} />
                </FormField>
            </div>
        </FormSection>

        <NoteBox type="info">
            سيتم تسجيل هذا الحضور كسجل يدوي ويجب اعتماده من قبل مسؤول الموارد البشرية.
        </NoteBox>
      </div>
    </BaseFormDialog>
  );
  }
