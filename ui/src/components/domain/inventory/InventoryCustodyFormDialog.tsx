import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calendar, Hand, Package } from "lucide-react";
import { useMemo } from "react";
import { type Resolver, useForm } from "react-hook-form";
import * as z from "zod";

import { custodyApi, type CustodyDto } from "@/api/inventory/custody";
import { queryKeys } from "@/constants/queryKeys";
import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
import { hrApi, type EmployeeListItemDto } from "@/api/hr";
import {
    BaseFormDialog,
    FormField,
    FormSection,
    Input,
    NativeSelect,
    Textarea
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

const custodySchema = z.object({
    assetID: z.coerce.number().int().positive("الأصل مطلوب"),
    employeeID: z.coerce.number().int().positive("الموظف مطلوب"),
    quantity: z.coerce.number().int().min(1, "الكمية يجب أن تكون 1 على الأقل").default(1),
    assignedDate: z.string().min(1, "تاريخ التسليم مطلوب"),
    notes: z.string().trim().optional().nullable().or(z.literal("")),
});

type CustodyFormValues = z.infer<typeof custodySchema>;

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
    custody: CustodyDto | null;
}

export function InventoryCustodyFormDialog({ open, onOpenChange, onSaved }: Props) {
    const { data: assets = [] } = useQuery({
        queryKey: queryKeys.fixedAssets.simple,
        queryFn: () => fixedAssetApi.getAll()
    });

    const { data: empData } = useQuery({
        queryKey: ["hr-employees-simple"],
        queryFn: () => hrApi.getEmployees({ pageSize: 1000 })
    });
    const employees = empData?.items ?? [];

    const mutation = useApiMutation(
        (values: CustodyFormValues) => custodyApi.create(values),
        {
            successMessage: "تم تسليم العهدة بنجاح",
            onSuccessCallback: () => { onSaved(); onOpenChange(false); }
        }
    );

    const form = useForm<CustodyFormValues>({
        resolver: zodResolver(custodySchema) as Resolver<z.infer<typeof custodySchema>>,
        defaultValues: { assetID: 0, employeeID: 0, quantity: 1, assignedDate: new Date().toISOString().split('T')[0], notes: "" },
    });

    const { register, handleSubmit, reset, formState: { errors, isDirty }, setValue, watch } = form;

    const onSubmit = (values: CustodyFormValues) => mutation.mutate(values);

    const initialValues = useMemo(() => ({
        assetID: 0, employeeID: 0, quantity: 1,
        assignedDate: new Date().toISOString().split('T')[0], notes: ""
    }), []);

    const assetOptions = assets.map((a: FixedAssetDto) => ({
        value: String(a.assetID),
        label: `${a.assetName} (${a.assetNumber})`,
    }));

    const employeeOptions = employees.map((e: EmployeeListItemDto) => ({
        value: String(e.employeeID),
        label: e.fullName,
    }));

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تسليم عهدة جديدة"
            description="تسليم أصل ثابت لموظف"
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="تسليم العهدة"
        >
            <div className="space-y-4">
                <FormSection title="بيانات العهدة" icon={Hand}>
                    <div className="grid grid-cols-1 gap-4">
                        <FormField label="الأصل" required error={errors.assetID?.message}>
                            <NativeSelect
                                options={assetOptions}
                                placeholder="اختر الأصل..."
                                value={watch("assetID")}
                                onChange={(val) => setValue("assetID", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                        <FormField label="الموظف" required error={errors.employeeID?.message}>
                            <NativeSelect
                                options={employeeOptions}
                                placeholder="اختر الموظف..."
                                value={watch("employeeID")}
                                onChange={(val) => setValue("employeeID", Number(val), { shouldValidate: true })}
                            />
                        </FormField>
                        <FormField label="الكمية" required error={errors.quantity?.message}>
                            <Input {...register("quantity")} type="number" min={1} icon={Package} />
                        </FormField>
                        <FormField label="تاريخ التسليم" required error={errors.assignedDate?.message}>
                            <Input {...register("assignedDate")} type="date" icon={Calendar} />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="ملاحظات">
                    <FormField label="ملاحظات" error={errors.notes?.message}>
                        <Textarea {...register("notes")} placeholder="ملاحظات..." rows={3} />
                    </FormField>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
