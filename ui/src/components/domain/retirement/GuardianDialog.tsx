"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle2, FileText, ShieldCheck, User } from "lucide-react";
import { useEffect, useMemo } from "react";
import { useForm, useWatch, Controller } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import {
    BaseFormDialog,
    FileField,
    FormField,
    FormSection,
    Input,
    DateInput,
    NoteBox,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { useGuardianQuery } from "@/hooks/queries/useGuardianQuery";
import type { GuardianDto } from "@/types/retirement";
import { cn } from "@/lib/utils";

// ─── Schema ─────────────────────────────────────────────

const guardianCreateSchema = z.object({
    fullName: z.string().min(1, "اسم الوصي مطلوب"),
    nationalID: z.string().length(12, "الرقم الوطني يجب أن يتكون من 12 رقماً"),
    phone: z.string().optional(),
    guardianshipDocNumber: z.string().min(1, "رقم صك الوصاية مطلوب"),
    startDate: z.string().min(1, "تاريخ بدء الوصاية مطلوب"),
    entitledIDs: z.array(z.number()).min(1, "يجب اختيار مستحق واحد على الأقل"),
    personalPhoto: z.instanceof(File).optional().nullable(),
    nationalIDDocument: z.instanceof(File).optional().nullable(),
    guardianshipDocument: z.instanceof(File).optional().nullable(),
});

const guardianEditSchema = z.object({
    fullName: z.string().min(1, "اسم الوصي مطلوب"),
    nationalID: z.string().length(12, "الرقم الوطني يجب أن يتكون من 12 رقماً"),
    phone: z.string().optional(),
    guardianshipDocNumber: z.string().optional(),
    startDate: z.string().optional(),
    entitledIDs: z.array(z.number()).optional(),
    personalPhoto: z.instanceof(File).optional().nullable(),
    nationalIDDocument: z.instanceof(File).optional().nullable(),
    guardianshipDocument: z.instanceof(File).optional().nullable(),
});

export type GuardianFormValues = z.infer<typeof guardianCreateSchema>;

interface GuardianDialogProps {
    open: boolean;
    onClose: () => void;
    onSaved?: () => void;
    fileId: number;
    availableEntitleds: { entitledID: number; fullName: string }[];
    mode?: "create" | "edit";
    initialData?: GuardianDto;
}

const DRAFT_KEY = "guardian_draft";

export function GuardianDialog({
    open,
    onClose,
    onSaved,
    fileId,
    availableEntitleds,
    mode = "create",
    initialData
}: GuardianDialogProps) {
    // جلب البيانات الطازجة في وضع التعديل
    const editId = mode === "edit" ? initialData?.guardianID : undefined;
    const { data: freshData } = useGuardianQuery(editId, !!editId && open);

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData;
        return initialData;
    }, [mode, freshData, initialData]);

    const mutation = useApiMutation(
        async (formData: FormData) => {
            if (mode === "create") {
                return api.post("/guardians", formData, { headers: { "Content-Type": "multipart/form-data" } });
            } else {
                return api.put(`/guardians/${effectiveData?.guardianID}`, formData, { headers: { "Content-Type": "multipart/form-data" } });
            }
        },
        {
            invalidateKeys: [["guardians"], ["retirement-file", fileId]],
            successMessage: mode === "create" ? "تم تسجيل الوصي بنجاح" : "تم تحديث بيانات الوصي بنجاح",
            onSuccess: () => {
                sessionStorage.removeItem(DRAFT_KEY);
            }
        }
    );

    const form = useForm<GuardianFormValues>({
        resolver: zodResolver(mode === "create" ? guardianCreateSchema : guardianEditSchema) as any,
        defaultValues: useMemo(() => {
            if (mode === "edit" && effectiveData) {
                return {
                    fullName: effectiveData.fullName,
                    nationalID: effectiveData.nationalID,
                    phone: effectiveData.phone ?? "",
                    guardianshipDocNumber: effectiveData.guardianshipDocNumber,
                    startDate: effectiveData.startDate?.split("T")[0] ?? "",
                    entitledIDs: effectiveData.minors?.filter(m => m.isActive).map(m => m.entitledID) ?? [],
                };
            }
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (saved) {
                try {
                    return JSON.parse(saved);
                } catch { /* ignore */ }
            }
            return {
                fullName: "",
                nationalID: "",
                phone: "",
                guardianshipDocNumber: "",
                startDate: "",
                entitledIDs: [],
                personalPhoto: null,
                nationalIDDocument: null,
                guardianshipDocument: null,
            };
        }, [mode, effectiveData])
    });

    const { control, register, setValue, trigger, watch, formState: { errors, isDirty } } = form;
    const selectedEntitledIDs = useWatch({ control, name: "entitledIDs" }) || [];

    const allValues = watch();
    useEffect(() => {
        if (mode === "create" && isDirty) {
            const { personalPhoto: _p, nationalIDDocument: _n, guardianshipDocument: _g, ...textFields } = allValues;
            void _p; void _n; void _g;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, isDirty, mode]);

    const performSubmit = async (values: GuardianFormValues, keepOpen = false) => {
        const formData = new FormData();

        if (mode === "create") {
            formData.append("fileID", fileId.toString());
            values.entitledIDs?.forEach((id: number) => formData.append("entitledIDs", id.toString()));
            formData.append("startDate", (values.startDate ?? "") + "T00:00:00");
        }

        formData.append("fullName", values.fullName);
        formData.append("nationalID", values.nationalID);
        if (values.phone) formData.append("phone", values.phone);
        if (values.guardianshipDocNumber) formData.append("guardianshipDocNumber", values.guardianshipDocNumber);

        if (values.personalPhoto) formData.append("personalPhoto", values.personalPhoto);
        if (values.nationalIDDocument) formData.append("nationalIDDocument", values.nationalIDDocument);
        if (values.guardianshipDocument) formData.append("guardianshipDocument", values.guardianshipDocument);

        await mutation.mutateAsync(formData);

        onSaved?.();

        if (!keepOpen) {
            onClose();
        } else {
            form.reset();
        }
    };

    const handleFormSubmit = (values: GuardianFormValues) => {
        performSubmit(values, false);
    };

    const handleSaveAndNew = () => {
        form.handleSubmit((values) => {
            performSubmit(values as any, true);
        })();
    };

    const toggleEntitled = (id: number) => {
        if (selectedEntitledIDs.includes(id)) {
            setValue("entitledIDs", selectedEntitledIDs.filter(i => i !== id), { shouldDirty: true });
        } else {
            setValue("entitledIDs", [...selectedEntitledIDs, id], { shouldDirty: true });
        }
        trigger("entitledIDs");
    };

    const initialValues = useMemo(() => {
        if (mode !== "edit" || !effectiveData) return undefined;
        return {
            fullName: effectiveData.fullName,
            nationalID: effectiveData.nationalID,
            phone: effectiveData.phone ?? "",
            guardianshipDocNumber: effectiveData.guardianshipDocNumber,
            startDate: effectiveData.startDate?.split("T")[0] ?? "",
            entitledIDs: effectiveData.minors?.filter(m => m.isActive).map(m => m.entitledID) ?? [],
        };
    }, [mode, effectiveData]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={(o) => !o && onClose()}
            title={mode === "create" ? "تسجيل وصي جديد" : "تعديل بيانات الوصي"}
            description={mode === "create" ? "أدخل بيانات الوصي واختر المستحقين (القاصرين) التابعين له" : "تحديث بيانات الوصي المسجلة"}
            size="full"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit as any)}
            onSaveAndNew={mode === "create" ? handleSaveAndNew : undefined}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "تسجيل الوصي" : "حفظ التعديلات"}
        >
            <div className="space-y-4" dir="rtl">
                {/* اختيار المستحقين - فقط في وضع الإنشاء */}
                {mode === "create" && (
                    <FormSection title="المستحقين التابعين" icon={ShieldCheck}>
                        <FormField label="اختر القاصرين" required error={errors.entitledIDs?.message}>
                            <div className="border border-border/40 rounded-2xl p-2 max-h-[35vh] overflow-y-auto bg-background/30 space-y-1">
                                {availableEntitleds.length === 0 ? (
                                    <div className="flex flex-col items-center justify-center h-full text-center p-6 space-y-3">
                                        <div className="p-3 bg-muted rounded-full">
                                            <ShieldCheck className="w-8 h-8 text-muted-foreground/40" />
                                        </div>
                                        <p className="text-xs text-muted-foreground leading-relaxed">لا يوجد قاصرين متاحين بدون أوصياء حالياً</p>
                                    </div>
                                ) : (
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
                                        {availableEntitleds.map(ep => {
                                            const isSelected = selectedEntitledIDs.includes(ep.entitledID);
                                            return (
                                                <div
                                                    key={ep.entitledID}
                                                    onClick={() => toggleEntitled(ep.entitledID)}
                                                    className={cn(
                                                        "flex items-center justify-between p-3 rounded-xl border-2 cursor-pointer transition-all duration-200",
                                                        isSelected
                                                            ? "border-primary bg-primary/5"
                                                            : "hover:bg-muted/50 border-transparent text-muted-foreground"
                                                    )}
                                                >
                                                    <span className={cn("text-sm font-semibold", isSelected && "text-primary")}>
                                                        {ep.fullName}
                                                    </span>
                                                    {isSelected && <CheckCircle2 className="w-4 h-4 text-primary animate-in zoom-in-50" />}
                                                </div>
                                            );
                                        })}
                                    </div>
                                )}
                            </div>
                        </FormField>
                    </FormSection>
                )}

                {/* بيانات الوصي الأساسية */}
                <FormSection title="بيانات الوصي الأساسية" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        <FormField label="اسم الوصي الكامل" required error={errors.fullName?.message} className="md:col-span-2">
                            <Input {...register("fullName")} placeholder="الاسم الرباعي واللقب" autoFocus />
                        </FormField>

                        <FormField label="الرقم الوطني" required error={errors.nationalID?.message}>
                            <Input {...register("nationalID")} className="font-mono font-bold" placeholder="12xxxxxxxxxx" />
                        </FormField>

                        <FormField label="رقم صك الوصاية" required={mode === "create"} error={errors.guardianshipDocNumber?.message}>
                            <Input {...register("guardianshipDocNumber")} className="font-mono font-bold" />
                        </FormField>

                        {mode === "create" && (
                            <FormField label="تاريخ بدء الوصاية" required error={errors.startDate?.message}>
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
                        )}

                        <FormField label="رقم الهاتف" error={errors.phone?.message}>
                            <Input {...register("phone")} className="font-mono" placeholder="7xxxxxxx" />
                        </FormField>
                    </div>
                </FormSection>

                {/* المرفقات والوثائق */}
                <FormSection title="المرفقات والوثائق" icon={FileText}>
                    <NoteBox title="تنبيه" className="mb-4">
                        {mode === "create"
                            ? "يرجى التأكد من وضوح الوثائق الممسوحة ضوئياً لضمان سرعة قبول الملف."
                            : "اترك الحقول فارغة إذا كنت لا تريد تغيير المرفقات الحالية."
                        }
                    </NoteBox>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <FileField
                            name="personalPhoto"
                            control={control as any}
                            label="الصورة الشخصية"
                            accept="image/*"
                            size="sm"
                        />
                        <FileField
                            name="nationalIDDocument"
                            control={control as any}
                            label="الهوية الوطنية"
                            size="sm"
                        />
                        <FileField
                            name="guardianshipDocument"
                            control={control as any}
                            label="صك الوصاية"
                            size="sm"
                        />
                    </div>
                </FormSection>
            </div>
        </BaseFormDialog>
    );
}
