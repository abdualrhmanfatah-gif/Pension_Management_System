import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Controller,useForm } from "react-hook-form";
import * as z from "zod";

import type { CreateAdministrativeOrderDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import {
  BaseFormDialog,
  DateInput,
  FormField,
  Input,
  NativeSelect
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const orderSchema = z.object({
  employeeID: z.number().min(1, "الموظف مطلوب"),
  administrativeOrderTypeID: z.number().min(1, "نوع القرار مطلوب"),
  orderNumber: z.string().min(1, "رقم الأمر مطلوب"),
  orderDate: z.string().min(1, "تاريخ الأمر مطلوب"),
  effectiveDate: z.string().optional(),
  subject: z.string().min(3, "الموضوع مطلوب"),
  notes: z.string().optional(),
});

type OrderFormValues = z.infer<typeof orderSchema>;

interface CreateOrderDialogProps {
  open: boolean;
  onClose: () => void;
}

export function CreateOrderDialog({ open, onClose }: CreateOrderDialogProps) {
  const form = useForm<OrderFormValues>({
    resolver: zodResolver(orderSchema),
    defaultValues: {
      employeeID: 0,
      administrativeOrderTypeID: 0,
      orderNumber: "",
      orderDate: new Date().toISOString().split("T")[0],
      subject: "",
    }
  });

  const { register, handleSubmit, formState: { errors, isDirty } } = form;

  const { data: employees = [] } = useQuery({
    queryKey: ["hr", "employees-lookup"],
    queryFn: () => hrApi.getEmployees({ pageSize: 100 }).then(r => r.items),
    enabled: open
  });

  // Mock types for now since I don't have a lookup for order types in hrApi yet
  const orderTypes = [
    { id: 1, name: "قرار تعيين" },
    { id: 2, name: "قرار ترقية" },
    { id: 3, name: "قرار نقل" },
    { id: 4, name: "قرار إجازة" },
  ];

  const mutation = useApiMutation(
    (data: CreateAdministrativeOrderDto) => hrApi.createOrder(data),
    {
      successMessage: "تم إصدار الأمر الإداري بنجاح",
      invalidateKeys: [["hr", "orders"]],
      onSuccess: () => {
        onClose();
        form.reset();
      }
    }
  );

  const onSubmit = (values: OrderFormValues) => {
    mutation.mutate(values as CreateAdministrativeOrderDto);
  };

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={(o) => !o && onClose()}
      title="إصدار أمر إداري جديد"
      size="lg"
      isLoading={mutation.isPending}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      resetOnOpen={true}
      reset={form.reset}
    >
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 py-2 text-start">
        <FormField label="الموظف المعني" required error={errors.employeeID?.message}>
          <Controller
            name="employeeID"
            control={form.control}
            render={({ field }) => (
              <NativeSelect
                options={employees.map(e => ({ value: e.employeeID.toString(), label: e.fullName }))}
                value={field.value.toString()}
                onChange={(val) => field.onChange(Number(val))}
                placeholder="اختر الموظف..."
              />
            )}
          />
        </FormField>

        <FormField label="نوع القرار" required error={errors.administrativeOrderTypeID?.message}>
          <Controller
            name="administrativeOrderTypeID"
            control={form.control}
            render={({ field }) => (
              <NativeSelect
                options={orderTypes.map(t => ({ value: t.id.toString(), label: t.name }))}
                value={field.value.toString()}
                onChange={(val) => field.onChange(Number(val))}
                placeholder="اختر النوع..."
              />
            )}
          />
        </FormField>

        <FormField label="رقم الأمر" required error={errors.orderNumber?.message}>
          <Input {...register("orderNumber")} placeholder="مثلاً: 2024/001" />
        </FormField>

        <FormField label="تاريخ الأمر" required error={errors.orderDate?.message}>
          <Controller
            name="orderDate"
            control={form.control}
            render={({ field }) => (
              <DateInput
                value={field.value}
                onChange={field.onChange}
                error={errors.orderDate?.message}
              />
            )}
          />
        </FormField>

        <FormField label="تاريخ النفاذ" error={errors.effectiveDate?.message}>
          <Controller
            name="effectiveDate"
            control={form.control}
            render={({ field }) => (
              <DateInput
                value={field.value}
                onChange={field.onChange}
                error={errors.effectiveDate?.message}
              />
            )}
          />
        </FormField>

        <FormField label="الموضوع" required error={errors.subject?.message}>
          <Input {...register("subject")} placeholder="ملخص موضوع القرار" />
        </FormField>

        <div className="md:col-span-2">
          <FormField label="ملاحظات" error={errors.notes?.message}>
            <Input {...register("notes")} placeholder="أي ملاحظات إضافية..." />
          </FormField>
        </div>
      </div>
    </BaseFormDialog>
  );
}
