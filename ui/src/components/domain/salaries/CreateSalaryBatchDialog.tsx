import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { type Resolver,useForm, useWatch } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import * as z from "zod";

import api from "@/api/axios";
import { salariesApi } from "@/api/salaries";
import { 
    BaseFormDialog, 
    FormField, 
    Input, 
    NativeSelect} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { queryKeys } from "@/constants/queryKeys";

const schema = z.object({
    bankID: z.string().min(1, "يرجى اختيار البنك"),
    batchMonth: z.string().min(1, "يرجى اختيار شهر الصرف"),
    notes: z.string().optional(),
});

type FormData = z.infer<typeof schema>;

interface LookupDto {
    id: number;
    name: string;
}

interface CreateSalaryBatchDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved?: () => void;
}

/**
 * حوار إنشاء كشف صرف معاشات جديد
 */
export function CreateSalaryBatchDialog({ open, onOpenChange, onSaved }: CreateSalaryBatchDialogProps) {
    const navigate = useNavigate();
    
    const form = useForm<FormData>({
        resolver: zodResolver(schema) as Resolver<z.infer<typeof schema>>,
        defaultValues: {
            bankID: "",
            batchMonth: "",
            notes: "",
        }
    });

    const { control, register, setValue, formState: { errors, isDirty } } = form;
    const bankID = useWatch({ control, name: "bankID" });

    // تحميل البنوك
    const { data: banks = [] } = useQuery({
        queryKey: queryKeys.lookups.banks,
        queryFn: () => api.get<{ data: LookupDto[] }>("/lookups/banks").then(res => res.data.data),
        enabled: open
    });

    const createMutation = useApiMutation(
        (values: FormData) => salariesApi.create({
            bankID: parseInt(values.bankID),
            batchMonth: values.batchMonth + "-01",
            notes: values.notes,
        }),
        {
            successMessage: "تم إنشاء كشف الصرف بنجاح",
            invalidateKeys: [["salary-batches"]],
            onSuccessCallback: (data) => {
                form.reset();
                onSaved?.();
                onOpenChange(false);
                navigate(`/salaries/${data.batchID}`);
            }
        }
    );

    const onSubmit = (values: FormData) => {
        createMutation.mutate(values);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إنشاء كشف صرف جديد"
            size="2xl"
            isLoading={createMutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onSubmit)}
            reset={form.reset}
            resetOnOpen
            submitLabel="إنشاء والانتقال للتفاصيل"
        >
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 py-2">
                <FormField 
                    label="البنك" 
                    required 
                    error={errors.bankID?.message}
                >
                    <NativeSelect
                        value={bankID}
                        onChange={(val: string) => setValue("bankID", val, { shouldValidate: true })}
                        options={banks.map((b) => ({ value: String(b.id), label: b.name }))}
                        placeholder="اختر البنك"
                    />
                </FormField>

                <FormField 
                    label="شهر الصرف" 
                    required 
                    error={errors.batchMonth?.message}
                >
                    <Input
                        type="month"
                        {...register("batchMonth")}
                    />
                </FormField>

                <FormField 
                    label="ملاحظات" 
                    error={errors.notes?.message}
                >
                    <Input
                        {...register("notes")}
                        placeholder="ملاحظات اختيارية"
                    />
                </FormField>
            </div>
        </BaseFormDialog>
    );
}
