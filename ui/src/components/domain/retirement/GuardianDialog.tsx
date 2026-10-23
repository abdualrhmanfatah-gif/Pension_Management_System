"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle2, FileText, ShieldCheck, User } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef } from "react";
import { Controller,type Resolver, type ResolverOptions,useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import api from "@/api/axios";
import { retirementApi } from "@/api/retirement";
import {
    BaseFormDialog,
    DateInput,
    FileField,
    FormField,
    FormSection,
    Input,
    NoteBox,
    yemenPhoneSchema,
} from "@/components/ui";
import { useApiMutation, useRecordNavigation } from "@/hooks";
import { GUARDIAN_KEY } from "@/hooks/queries/useGuardianQuery";
import { GUARDIANS_KEY } from "@/hooks/queries/useGuardiansQuery";
import { cn } from "@/lib/utils";
import type { GuardianDto } from "@/types/retirement";

// ─── Schema ─────────────────────────────────────────────

const guardianCreateSchema = z.object({
    fullName: z.string().min(1, "اسم الوصي مطلوب"),
    nationalID: z.string().length(12, "الرقم الوطني يجب أن يتكون من 12 رقماً"),
    phone: yemenPhoneSchema.optional().or(z.literal("")),
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
    phone: yemenPhoneSchema.optional().or(z.literal("")),
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

const createEmptyDefaults = {
    fullName: "",
    nationalID: "",
    phone: "",
    guardianshipDocNumber: "",
    startDate: "",
    entitledIDs: [] as number[],
    personalPhoto: null,
    nationalIDDocument: null,
    guardianshipDocument: null,
};

export function GuardianDialog({
    open,
    onClose,
    onSaved,
    fileId,
    availableEntitleds,
    mode = "create",
    initialData
}: GuardianDialogProps) {
    // في وضع التعديل: معرّف الوصي المفتوح (لبذر فهرس التنقل)
    const editId = mode === "edit" ? initialData?.guardianID : undefined;

    // تحويل بيانات الوصي (من التفاصيل أو صف القائمة) إلى قيم النموذج
    const toFormValues = useCallback((data: GuardianDto): GuardianFormValues => ({
        fullName: data.fullName,
        nationalID: data.nationalID,
        phone: data.phone ?? "",
        guardianshipDocNumber: data.guardianshipDocNumber,
        startDate: data.startDate?.split("T")[0] ?? "",
        entitledIDs: data.minors?.filter(m => m.isActive).map(m => m.entitledID) ?? [],
    }), []);

    // وضع فعلي مرجعي — يقرأه المصحّح عند الإرسال بعد تحديد effectiveMode
    const effectiveModeRef = useRef<"create" | "edit">(mode);

    const form = useForm<GuardianFormValues>({
        resolver: ((values: GuardianFormValues, context: unknown, options: ResolverOptions<GuardianFormValues>) => {
            const schema = effectiveModeRef.current === "create" ? guardianCreateSchema : guardianEditSchema;
            const resolve = zodResolver(schema) as unknown as Resolver<GuardianFormValues>;
            return resolve(values, context, options);
        }) as Resolver<GuardianFormValues>,
        defaultValues: useMemo(
            () => (mode === "edit" && initialData ? toFormValues(initialData) : { ...createEmptyDefaults }),
            [mode, initialData, toFormValues],
        ),
    });

    const { control, register, setValue, trigger, watch, reset, formState: { errors, isDirty } } = form;

    // ─── Alt+↑/↓: التنقل بين أوصياء الملف (المنطق المشترك في useRecordNavigation) ───
    const savingRef = useRef(false);
    const { activeRecord, detailReady, effectiveMode } = useRecordNavigation<
        GuardianDto,
        GuardianDto,
        GuardianFormValues
    >({
        open,
        mode,
        editRecordId: editId,
        listKey: [GUARDIANS_KEY, fileId],
        fetchList: async () => (await retirementApi.getGuardians(fileId)) as unknown as GuardianDto[],
        getId: g => g.guardianID,
        detailKey: id => [GUARDIAN_KEY, id],
        fetchDetail: id => retirementApi.getGuardianById(id) as Promise<GuardianDto>,
        getDetailId: d => d.guardianID,
        emptyMessage: "لا يوجد أوصياء مسجلون لهذا الملف",
        emptyDefaults: createEmptyDefaults,
        toFormValues: record => toFormValues(record as GuardianDto),
        form,
        isDirty,
        isBusyRef: savingRef,
        focusField: "fullName",
    });

    // جلب البيانات الطازجة: السجل الحالي (تفاصيل أو صف القائمة)، وإلا initialData
    const effectiveData = (activeRecord ?? initialData) as GuardianDto | undefined;

    useEffect(() => {
        effectiveModeRef.current = effectiveMode;
    }, [effectiveMode]);

    const mutation = useApiMutation(
        async (formData: FormData) => {
            if (effectiveMode === "create") {
                return api.post("/guardians", formData, { headers: { "Content-Type": "multipart/form-data" } });
            } else {
                return api.put(`/guardians/${effectiveData?.guardianID}`, formData, { headers: { "Content-Type": "multipart/form-data" } });
            }
        },
        {
            invalidateKeys: [[GUARDIANS_KEY, fileId], ["retirement-file", fileId]],
            successMessage: effectiveMode === "create" ? "تم تسجيل الوصي بنجاح" : "تم تحديث بيانات الوصي بنجاح",
            onSuccess: () => {
                sessionStorage.removeItem(DRAFT_KEY);
            }
        }
    );

    // مزامنة حارس التنقل مع حالة الحفظ (يُقرأ عند الضغط فقط)
    useEffect(() => {
        savingRef.current = mutation.isPending;
    }, [mutation.isPending]);

    useEffect(() => {
        if (open && mode === "create") {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (saved) {
                try { reset(JSON.parse(saved)); } catch { /* ignore */ }
            }
        }
    }, [open, mode, reset]);
    const selectedEntitledIDs = useWatch({ control, name: "entitledIDs" }) || [];

    const allValues = watch();
    useEffect(() => {
        if (effectiveMode === "create" && isDirty) {
            const { personalPhoto: _p, nationalIDDocument: _n, guardianshipDocument: _g, ...textFields } = allValues;
            void _p; void _n; void _g;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, isDirty, effectiveMode]);

    const performSubmit = async (values: GuardianFormValues, keepOpen = false) => {
        const formData = new FormData();

        if (effectiveMode === "create") {
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
            form.reset({ ...createEmptyDefaults });
        }
    };

    const handleFormSubmit = (values: GuardianFormValues) => {
        return performSubmit(values, false);
    };

    const handleSaveAndNew = () => {
        return form.handleSubmit((values) => {
            return performSubmit(values as any, true);
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

    const initialValues = useMemo(
        () => (effectiveData && (effectiveMode === "edit" || activeRecord) ? toFormValues(effectiveData) : undefined),
        [effectiveData, effectiveMode, activeRecord, toFormValues]
    );

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={(o) => !o && onClose()}
            title={effectiveMode === "create" ? "تسجيل وصي جديد" : "تعديل بيانات الوصي"}
            description={effectiveMode === "create" ? "أدخل بيانات الوصي واختر المستحقين (القاصرين) التابعين له" : "تحديث بيانات الوصي المسجلة"}
            size="full"
            isLoading={mutation.isPending || !detailReady}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit as any)}
            onSaveAndNew={effectiveMode === "create" ? handleSaveAndNew : undefined}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={effectiveMode === "create" ? "تسجيل الوصي" : "حفظ التعديلات"}
        >
            <div className="space-y-4" dir="rtl">
                {/* اختيار المستحقين - فقط في وضع الإنشاء */}
                {effectiveMode === "create" && (
                    <FormSection title="المستحقين التابعين" icon={ShieldCheck}>
                        <FormField label="اختر القاصرين" required error={errors.entitledIDs?.message}>
                            <div className="border border-border rounded-2xl p-2 max-h-[35vh] overflow-y-auto bg-background/30 space-y-1">
                                {availableEntitleds.length === 0 ? (
                                    <div className="flex flex-col items-center justify-center h-full text-center p-6 space-y-3">
                                        <div className="p-3 bg-muted rounded-full">
                                            <ShieldCheck className="w-8 h-8 text-muted-foreground" />
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
                            <Input {...register("nationalID")} placeholder="12xxxxxxxxxx" />
                        </FormField>

                        <FormField label="رقم صك الوصاية" required={effectiveMode === "create"} error={errors.guardianshipDocNumber?.message}>
                            <Input {...register("guardianshipDocNumber")} className="font-mono font-bold" />
                        </FormField>

                        {effectiveMode === "create" && (
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
                            <Input {...register("phone")} placeholder="7xxxxxxx" />
                        </FormField>
                    </div>
                </FormSection>

                {/* المرفقات والوثائق */}
                <FormSection title="المرفقات والوثائق" icon={FileText}>
                    <NoteBox title="تنبيه" className="mb-4">
                        {effectiveMode === "create"
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
