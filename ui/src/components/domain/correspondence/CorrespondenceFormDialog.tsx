import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { FileText, Hash,Info } from "lucide-react";
import { useEffect } from "react";
import { Controller, type SubmitHandler,useForm } from "react-hook-form";

import { correspondenceApi } from "@/api/correspondence";
import { hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  type BaseFormDialogProps, 
  DateInput,
  FormField,
  FormSection,
  Input, 
  NativeSelect, 
  Textarea} from "@/components/ui";
<<<<<<< HEAD
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
=======
import { useApiMutation } from "@/hooks";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { 
  type CorrespondenceDetailsDto,
  CorrespondenceDirection,
  correspondenceSchema, 
  type CorrespondenceTypeDto,
  type CreateCorrespondenceDto, 
  type ExternalPartyDto
} from "@/types/correspondence";

interface Props extends Pick<BaseFormDialogProps, "open" | "onOpenChange"> {
  direction: CorrespondenceDirection;
  onSaved?: () => void;
  correspondence?: CorrespondenceDetailsDto | null;
}

export function CorrespondenceFormDialog({ open, onOpenChange, direction, onSaved, correspondence }: Props) {
  const isEdit = !!correspondence;

  const form = useForm<CreateCorrespondenceDto>({
    resolver: zodResolver(correspondenceSchema),
    defaultValues: {
      direction,
      correspondenceTypeID: 0,
      sequentialNumber: 1,
      referenceNumber: "",
      correspondenceDate: new Date().toISOString().split("T")[0],
      subject: "",
      pageNumber: "",
      externalPartyID: null,
      employeeID: null,
      notes: "",
    },
  });

  const { control, handleSubmit, reset, formState: { errors, isDirty } } = form;

  // Mutation and Submit handler first
  const mutation = useApiMutation(
    (values: CreateCorrespondenceDto) => {
      if (isEdit && correspondence) {
        return correspondenceApi.update(correspondence.correspondenceID, values);
      }
      return correspondenceApi.create(values);
    },
    {
      successMessage: isEdit ? "تم تحديث المراسلة بنجاح" : "تم تسجيل المراسلة بنجاح",
      invalidateKeys: [["correspondence", "list"]],
      onSuccess: () => {
        onSaved?.();
        onOpenChange?.(false);
      }
    }
  );

  const onSubmit: SubmitHandler<CreateCorrespondenceDto> = (data) => {
    mutation.mutate(data);
  };

  // Reset values when editing
  useEffect(() => {
    if (isEdit && correspondence) {
      reset({
        direction: correspondence.direction,
        correspondenceTypeID: correspondence.correspondenceTypeID,
        sequentialNumber: correspondence.sequentialNumber,
        referenceNumber: correspondence.referenceNumber,
        correspondenceDate: correspondence.correspondenceDate.split("T")[0],
        subject: correspondence.subject,
        pageNumber: correspondence.pageNumber || "",
        externalPartyID: correspondence.externalPartyID || null,
        employeeID: correspondence.employeeID || null,
        notes: correspondence.notes || "",
      });
    }
  }, [isEdit, correspondence, reset]);

  // Lookup data
  const { data: typesRes } = useQuery({
    queryKey: queryKeys.correspondence.types,
    queryFn: () => correspondenceApi.getTypes(),
    enabled: !!open
  });

  const { data: externalPartiesRes } = useQuery({
    queryKey: queryKeys.correspondence.externalParties,
    queryFn: () => correspondenceApi.getExternalParties(),
    enabled: !!open
  });

  const { data: employees } = useQuery({
    queryKey: queryKeys.hr.employees.minimal,
    queryFn: async () => {
        const res = await hrApi.getEmployees({ pageSize: 1000 });
        return res.items;
    },
    enabled: !!open
  });

  const types = typesRes?.data || [];
  const externalParties = externalPartiesRes?.data || [];

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEdit ? "تعديل مراسلة" : (direction === CorrespondenceDirection.Outgoing ? "تسجيل صادر جديد" : "تسجيل وارد جديد")}
      description="أدخل تفاصيل القيد في السجل العام"
      size="lg"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      formErrors={errors}
    >
      <div className="space-y-6" dir="rtl">
        <FormSection title="بيانات القيد" icon={Hash}>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <FormField label="نوع المراسلة" error={errors.correspondenceTypeID?.message} required>
              <Controller
                name="correspondenceTypeID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={types.map((t: CorrespondenceTypeDto) => ({ label: t.name, value: String(t.correspondenceTypeID) }))}
                    value={String(field.value || "")}
                    onChange={(v) => field.onChange(Number(v))}
                    placeholder="اختر النوع..."
                  />
                )}
              />
            </FormField>

            <Input
              label="رقم القيد (المرجع)"
              placeholder="مثال: 07/03/25"
              {...form.register("referenceNumber")}
              error={errors.referenceNumber?.message}
              required
            />

            <FormField label="تاريخ المراسلة" error={errors.correspondenceDate?.message} required>
              <Controller
                name="correspondenceDate"
                control={control}
                render={({ field }) => (
                  <DateInput
                    value={field.value}
                    onChange={field.onChange}
                    error={errors.correspondenceDate?.message}
                  />
                )}
              />
            </FormField>

            <Input
              label="الرقم المتسلسل"
              type="number"
              {...form.register("sequentialNumber", { valueAsNumber: true })}
              error={errors.sequentialNumber?.message}
              required
            />
            
            <Input
              label="رقم الصفحة (ص)"
              placeholder="..."
              {...form.register("pageNumber")}
              error={errors.pageNumber?.message}
            />
          </div>
        </FormSection>

        <FormSection title="الموضوع والجهة" icon={FileText}>
          <div className="space-y-4">
            <Input
              label="الموضوع"
              placeholder="وصف مختصر لمحتوى المراسلة"
              {...form.register("subject")}
              error={errors.subject?.message}
              required
            />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField label="الجهة الخارجية" error={errors.externalPartyID?.message}>
                <Controller
                  name="externalPartyID"
                  control={control}
                  render={({ field }) => (
                    <NativeSelect
                      options={externalParties.map((p: ExternalPartyDto) => ({ label: p.name, value: String(p.externalPartyID) }))}
                      value={String(field.value || "")}
                      onChange={(v) => field.onChange(v ? Number(v) : null)}
                      placeholder="اختر الجهة (اختياري)..."
                    />
                  )}
                />
              </FormField>

              <FormField label="الموظف المعني" error={errors.employeeID?.message}>
                <Controller
                  name="employeeID"
                  control={control}
                  render={({ field }) => (
                    <NativeSelect
                      options={(employees || []).map(e => ({ label: e.fullName, value: String(e.employeeID) }))}
                      value={String(field.value || "")}
                      onChange={(v) => field.onChange(v ? Number(v) : null)}
                      placeholder="اختر الموظف (اختياري)..."
                    />
                  )}
                />
              </FormField>
            </div>
          </div>
        </FormSection>

        <FormSection title="ملاحظات إضافية" icon={Info}>
          <Textarea
            label="ملاحظات"
            placeholder="أي تفاصيل إضافية حول المراسلة..."
            {...form.register("notes")}
            error={errors.notes?.message}
            rows={3}
          />
        </FormSection>
      </div>
    </BaseFormDialog>
  );
}
