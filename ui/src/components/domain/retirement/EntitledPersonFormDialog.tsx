import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Heart, Percent, ShieldCheck,User } from "lucide-react";
import React from "react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
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
    NativeSelect,
    NoteBox,
    yemenPhoneSchema
} from "@/components/ui";
import { useApiMutation, useRecordNavigation } from "@/hooks";
import { ENTITLED_PERSON_KEY } from "@/hooks/queries/useEntitledPersonQuery";
import { useFileDetailsQuery } from "@/hooks/queries/useFileDetailsQuery";
import { normalizeDateInputValue } from "@/lib/date";
import { getDocumentUrl } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";

import { EntitlementAmountField } from "./EntitlementAmountField";

type EntitledFormValues = z.infer<typeof entitledSchemaRelaxed>;

export interface EntitledPersonDetail extends Omit<EntitledFormValues, "entitledAmount"> {
    entitledID: number;
    nationalIdImageUrl?: string | null;
    updatedAt?: string;
    sharePercent?: number | null;
}

interface EntitledPersonFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    fileId: number;
    initialData?: Partial<EntitledPersonDetail>;
    mode: "create" | "edit";
    onSuccess?: () => void;
    onSaveAndAddAnother?: () => void;
    retireeIsAlive?: boolean;
}

const BASE_DRAFT_KEY = "entitled_person_draft";

// ─── Validation Schema ─────────────────────────────────

const entitledSchemaRelaxed = z.object({
    fullName: z.string().trim().min(3, "الاسم الكامل مطلوب (3 أحرف على الأقل)"),
    nationalID: z.string().optional().or(z.literal("")),
    relationshipID: z.number({ message: "صلة القرابة مطلوبة" }).min(1, "صلة القرابة مطلوبة"),
    dateOfBirth: z.string().optional().or(z.literal("")),
    phone: z.union([z.literal(""), yemenPhoneSchema]).optional(),
    socialStatusID: z.number().optional().or(z.literal(0)),
    healthStatusID: z.number().optional().or(z.literal(0)),
    hasDisabilityException: z.boolean(),
    entitledAmount: z.string().optional().refine(value => !value || Number(value) > 0, "أدخل مبلغاً أكبر من صفر"),
    nationalIdImage: z.union([z.instanceof(File), z.string()]).optional(),
});

export function EntitledPersonFormDialog({
    open,
    onOpenChange,
    fileId,
    initialData,
    mode,
    onSuccess,
    onSaveAndAddAnother,
    retireeIsAlive: retireeIsAliveProp
}: EntitledPersonFormDialogProps) {
    const queryClient = useQueryClient();
    const userId = useAuthStore(s => s.user?.id);
    const DRAFT_KEY = useMemo(() => `${BASE_DRAFT_KEY}_${userId || "anon"}_${fileId}`, [userId, fileId]);

    // في وضع التعديل: جلب البيانات من الخادم بدلاً من الاعتماد على initialData القديمة
    const editId = mode === "edit" ? initialData?.entitledID : undefined;

    // ─── Lookups Query ──────────────────────────────────
    const { data: lookups } = useQuery({
        queryKey: ["entitled-lookups"],
        queryFn: async () => {
            const [rRes, ssRes, hsRes] = await Promise.all([
                api.get<{ data: { id: number; name: string }[] }>("/lookups/relationships"),
                api.get<{ data: { id: number; name: string }[] }>("/lookups/social-statuses"),
                api.get<{ data: { id: number; name: string }[] }>("/lookups/health-statuses"),
            ]);
            return {
                relationships: rRes.data.data || [],
                socialStatuses: ssRes.data.data || [],
                healthStatuses: hsRes.data.data || [],
            };
        },
        staleTime: 5 * 60 * 1000, // 5 minutes
    });

    // ─── Fetch File/Retiree Details for Auto-completion ───
    const { file } = useFileDetailsQuery(fileId);
    const retireeIsAlive = retireeIsAliveProp ?? file?.isAlive ?? true;

    const createEmptyDefaults = useMemo(() => ({
        fullName: "",
        nationalID: "",
        relationshipID: 0,
        dateOfBirth: "",
        phone: "",
        socialStatusID: 0,
        healthStatusID: 0,
        hasDisabilityException: false,
        entitledAmount: undefined,
    }), []);

    // تحويل بيانات مستحق (من التفاصيل أو صف القائمة) إلى قيم النموذج
    const toFormValues = useCallback((data: EntitledPersonDetail | undefined) => {
        if (!data) return undefined;
        return {
            ...data,
            dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split('T')[0] : "",
            relationshipID: data.relationshipID || 0,
            socialStatusID: data.socialStatusID || 0,
            healthStatusID: data.healthStatusID || 0,
            entitledAmount: undefined,
            // Map existing image URL to the form field if no new file is selected
            nationalIdImage: (() => {
                const url = data.nationalIdImageUrl;
                if (!url) return undefined;
                if (url.startsWith("http")) return url;
                return getDocumentUrl(url) ?? url;
            })(),
        };
    }, []);

    const form = useForm<EntitledFormValues>({
        resolver: (values, context, options) => {
            // نستخدم المخطط المرن دائماً عند الإضافة/التعديل في ملف الورثة
            // التشديد يكون فقط عند تعيين الوريث كمستلم معاش

            return zodResolver(entitledSchemaRelaxed)(values, context, options);
        },
        defaultValues: createEmptyDefaults,
    });

    useEffect(() => {
        if (open && mode === "create") {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (saved) {
                try { form.reset(JSON.parse(saved)); } catch { /* ignore */ }
            }
        }
    }, [open, mode, DRAFT_KEY, form]);

    const { control, register, setValue, watch, formState: { errors, isDirty } } = form;

    // ─── Alt+↑/↓: التنقل بين مستحقي الملف (المنطق المشترك في useRecordNavigation) ───
    const savingRef = React.useRef(false);
    const { activeRecord, detailReady, effectiveMode } = useRecordNavigation<
        EntitledPersonDetail,
        EntitledPersonDetail,
        EntitledFormValues
    >({
        open,
        mode,
        editRecordId: editId,
        listKey: ["entitled-persons", fileId],
        fetchList: async () => (await retirementApi.getEntitledPersons(fileId)) as unknown as EntitledPersonDetail[],
        getId: r => r.entitledID,
        detailKey: id => [ENTITLED_PERSON_KEY, id],
        fetchDetail: id => retirementApi.getEntitledPersonById(id) as unknown as Promise<EntitledPersonDetail>,
        getDetailId: d => d.entitledID,
        emptyMessage: "لا يوجد ورثة مسجلون لهذا الملف",
        emptyDefaults: createEmptyDefaults,
        toFormValues: record => toFormValues(record) as EntitledFormValues,
        form,
        isDirty,
        isBusyRef: savingRef,
        focusField: "relationshipID",
    });

    // استخدم السجل الحالي (تفاصيل طازجة أو صف القائمة)، وإلا initialData
    const effectiveData = (activeRecord ?? initialData) as EntitledPersonDetail | undefined;

    // ─── Default health status to "سليم" ─────────────────
    const currentHealthStatusID = useWatch({ control, name: "healthStatusID" });
    useEffect(() => {
        if (!lookups || effectiveMode !== "create") return;
        if (currentHealthStatusID && currentHealthStatusID !== 0) return;
        const salim = lookups.healthStatuses.find(h => h.name.trim() === "سليم");
        if (salim) {
            setValue("healthStatusID", salim.id, { shouldDirty: false });
        }
    }, [lookups, currentHealthStatusID, effectiveMode, setValue]);

    // Save draft on changes (create mode only — لا يُحفظ مستحق محمّل عبر التنقل في مسودة الإنشاء)
    const allValues = watch();
    useEffect(() => {
        if (effectiveMode === "create" && isDirty) {
            const { nationalIdImage: _, ...textFields } = allValues;
            void _;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, effectiveMode, isDirty]);

    const [now] = useState(() => Date.now());
    const dobValue = useWatch({ control, name: "dateOfBirth" });
    const relationshipID = useWatch({ control, name: "relationshipID" });
    const currentSocialStatusID = useWatch({ control, name: "socialStatusID" });

    const isMinor = useMemo(() => {
        if (!dobValue) return false;
        const dob = new Date(dobValue);
        if (isNaN(dob.getTime())) return false;
        const ageDifMs = now - dob.getTime();
        const ageDate = new Date(Math.abs(ageDifMs));
        return (ageDate.getUTCFullYear() - 1970) < 18;
    }, [dobValue, now]);

    const isAdult = useMemo(() => {
        if (!dobValue) return false;
        const dob = new Date(dobValue);
        if (isNaN(dob.getTime())) return false;
        const ageDifMs = now - dob.getTime();
        const ageDate = new Date(Math.abs(ageDifMs));
        return (ageDate.getUTCFullYear() - 1970) >= 18;
    }, [dobValue, now]);

    // ─── Inference Logic ────────────────────────────────
    useEffect(() => {
        if (!lookups || !relationshipID) return;

        const rel = lookups.relationships.find(r => r.id === Number(relationshipID));
        let newSocialStatusID = currentSocialStatusID ?? 0;

        if (rel) {
            const relName = rel.name.trim();
            if (["ابن", "أبنه", "ابنة", "أخ", "أخت", "حفيد", "حفيدة"].includes(relName)) {
                const singleStatus = lookups.socialStatuses.find(s => s.name.includes("أعزب") || s.name.includes("عزباء"));
                if (singleStatus) newSocialStatusID = singleStatus.id;
            } else if (["زوجة", "زوج"].includes(relName)) {
                if (retireeIsAlive) {
                    // المتقاعد حي → الزوج/الزوجة متزوج
                    const marriedStatus = lookups.socialStatuses.find(s => s.name.includes("متزوج"));
                    if (marriedStatus) newSocialStatusID = marriedStatus.id;
                } else {
                    // المتقاعد متوفى → الزوج/الزوجة أرمل
                    const widowStatus = lookups.socialStatuses.find(s => s.name.includes("أرمل"));
                    if (widowStatus) newSocialStatusID = widowStatus.id;
                    else {
                        const marriedStatus = lookups.socialStatuses.find(s => s.name.includes("متزوج"));
                        if (marriedStatus) newSocialStatusID = marriedStatus.id;
                    }
                }
            }
        }

        if (dobValue && isMinor) {
            const singleStatus = lookups.socialStatuses.find(s => s.name.includes("أعزب") || s.name.includes("عزباء"));
            if (singleStatus) newSocialStatusID = singleStatus.id;
        }

        if (newSocialStatusID !== 0 && newSocialStatusID !== currentSocialStatusID) {
            setValue("socialStatusID", newSocialStatusID, { shouldDirty: true });
        }
    }, [relationshipID, dobValue, lookups, isMinor, retireeIsAlive, setValue]);

    const addAnotherRef = React.useRef(false);

    // ─── Mutation Logic ────────────────────────────────
    const mutation = useApiMutation(async (values: EntitledFormValues) => {
        if (effectiveMode === "create") {
            const formData = new FormData();
            formData.append("fileID", fileId.toString());
            formData.append("fullName", values.fullName);
            formData.append("nationalID", values.nationalID || "");
            formData.append("relationshipID", values.relationshipID.toString());
            if (values.dateOfBirth) {
                // DateOfBirth في الـAPI من نوع DateOnly: يقبل "YYYY-MM-DD" فقط،
                // و toISOString() يرسل طابعًا زمنيًا كاملًا فيرفضه الربط.
                const dateOfBirth = normalizeDateInputValue(values.dateOfBirth);
                if (dateOfBirth) formData.append("dateOfBirth", dateOfBirth);
            }
            if (values.phone) formData.append("phone", values.phone);
            formData.append("socialStatusID", (values.socialStatusID || 0).toString());
            formData.append("healthStatusID", (values.healthStatusID || 0).toString());
            formData.append("hasDisabilityException", values.hasDisabilityException.toString());
            if (values.entitledAmount) formData.append("entitledAmount", values.entitledAmount);

            if (values.nationalIdImage instanceof File) {
                formData.append("nationalIdImage", values.nationalIdImage);
            }

            return api.post("/entitled-persons", formData, {
                headers: { "Content-Type": undefined }
            });
        } else {
            const formData = new FormData();
            formData.append("fullName", values.fullName);
            formData.append("nationalID", values.nationalID || "");
            formData.append("relationshipID", values.relationshipID.toString());
            if (values.dateOfBirth) {
                // DateOnly يقبل "YYYY-MM-DD" فقط — لا طابعًا زمنيًا.
                const dateOfBirth = normalizeDateInputValue(values.dateOfBirth);
                if (dateOfBirth) formData.append("dateOfBirth", dateOfBirth);
            }
            formData.append("phone", values.phone || "");
            formData.append("socialStatusID", (values.socialStatusID || 0).toString());
            formData.append("healthStatusID", (values.healthStatusID || 0).toString());
            if (values.hasDisabilityException !== undefined) formData.append("hasDisabilityException", values.hasDisabilityException.toString());
            if (values.entitledAmount) formData.append("entitledAmount", values.entitledAmount);
            if (values.nationalIdImage instanceof File) formData.append("nationalIdImage", values.nationalIdImage);
            if (effectiveData?.updatedAt) formData.append("updatedAt", effectiveData.updatedAt);

            return api.put(`/entitled-persons/${effectiveData?.entitledID}`, formData, {
                headers: { "Content-Type": undefined }
            });
        }
    }, {
        successMessage: effectiveMode === "create" ? "تم إضافة المستحق بنجاح" : "تم تحديث بيانات المستحق",
        errorTitle: "خطأ في الحفظ",
        invalidateKeys: [
            ["entitled-persons", fileId],
            ["documents", fileId],
            ["missing-documents", fileId],
            ["retirement-file"]
        ],
        onSuccess: async () => {
            // ملاحظة: لا تنتظر أي عملية شبكة قبل تحديث حالة النموذج.
            // onSuccess هنا يُشغَّل من داخل onSuccess الخاص بـ useApiMutation،
            // وهو غير مُنتظَر هناك؛ لذلك أي await قبل إعادة التعيين/الإغلاق
            // يجعل التصفير يحدث بعد انتهاء طفرة الحفظ (isDirty تبقى true لحظياً).
            sessionStorage.removeItem(DRAFT_KEY);
            if (addAnotherRef.current) {
                form.reset({ ...createEmptyDefaults });
                addAnotherRef.current = false;
                onSuccess?.();
                onSaveAndAddAnother?.();
                // إعادة تثبيت focus على حقل الصلة بعد إعادة التعيين
                // لأن autoFocus لا يعمل مرة أخرى بعد إعادة تعيين النموذج
                setTimeout(() => form.setFocus("relationshipID"), 150);
            } else {
                onOpenChange(false);
                if (effectiveMode === "create") {
                    form.reset({ ...createEmptyDefaults });
                }
                onSuccess?.();
            }
            // تحديث فوري للقائمة بعد أن أصبح النموذج نظيفاً
            await queryClient.refetchQueries({ queryKey: ["entitled-persons", fileId] });
        },
        onError: async (error: any) => {
            addAnotherRef.current = false;
            const msg = error?.response?.data?.message || error?.message || "";
            if (typeof msg === "string" && msg.includes("مستخدم آخر")) {
                await queryClient.refetchQueries({ queryKey: ["entitled-persons", fileId] });
            }
        }
    });

    // مزامنة حارس التنقل مع حالة الحفظ (يُقرأ عند الضغط فقط)
    useEffect(() => {
        savingRef.current = mutation.isPending;
    }, [mutation.isPending]);

    const handleFormSubmit = React.useCallback((values: EntitledFormValues) => {
        return mutation.mutateAsync(values).catch(() => undefined);
    }, [mutation]);

    const handleSaveAndAddAnother = React.useCallback(() => {
        addAnotherRef.current = true;
        return form.handleSubmit(handleFormSubmit, () => {
            addAnotherRef.current = false;
        })();
    }, [form, handleFormSubmit]);

    const initialValues = useMemo(
        () => toFormValues(effectiveData),
        [toFormValues, effectiveData]
    );

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={effectiveMode === "create" ? "إضافة وريث جديد" : "تعديل بيانات الوريث"}
            description={effectiveMode === "create" ? "أدخل بيانات الوريث في الملف التقاعدي" : "تحديث بيانات الوريث المعتمدة"}
            size="full"
            isLoading={mutation.isPending || !detailReady}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            onSaveAndNew={effectiveMode === "create" ? handleSaveAndAddAnother : undefined}
            resetOnOpen={true}
            reset={form.reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={effectiveMode === "create" ? "إضافة الوريث" : "حفظ التعديلات"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الشخصية والقرابة" icon={User}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-12 gap-4">
                        <FormField label="صلة القرابة" required error={errors.relationshipID?.message} className="lg:col-span-2">
                            <Controller
                                name="relationshipID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        ref={field.ref}
                                        options={lookups?.relationships.map(r => ({ value: r.id.toString(), label: r.name })) || []}
                                        value={field.value || ""}
                                        onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                        placeholder="اختر الصلة..."
                                        autoFocus
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="الاسم الكامل للوريث" required error={errors.fullName?.message} className="lg:col-span-3">
                            <Input
                                {...register("fullName")}
                                onBlur={(e) => {
                                    register("fullName").onBlur(e);
                                    const val = e.target.value.trim();
                                    if (val && file?.retireeFullName && file?.gender === 1) {
                                        const rel = lookups?.relationships.find(r => r.id === Number(relationshipID));
                                        if (rel) {
                                            const relName = rel.name.trim();
                                            if (["ابن", "أبنه", "ابنة", "بنت"].includes(relName)) {
                                                // إكمال الاسم فقط إذا لم يحتوِ اسم المتقاعد بالفعل
                                                if (!val.includes(file.retireeFullName)) {
                                                    setValue("fullName", `${val} ${file.retireeFullName}`, { shouldDirty: true, shouldValidate: true });
                                                }
                                            } else if (["أخ", "أخت"].includes(relName)) {
                                                // إكمال الاسم باسم الأب (الجزء الثاني من اسم المتقاعد)
                                                if (!val.includes(file.retireeFullName)) {
                                                    const nameParts = file.retireeFullName.trim().split(/\s+/);
                                                    if (nameParts.length > 1) {
                                                        const fathersName = nameParts.slice(1).join(" ");
                                                        setValue("fullName", `${val} ${fathersName}`, { shouldDirty: true, shouldValidate: true });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }}
                                placeholder="الاسم الرباعي واللقب"
                            />
                        </FormField>

                        <FormField label="تاريخ الميلاد" required={!retireeIsAlive} error={errors.dateOfBirth?.message} className="lg:col-span-2">
                            <Controller
                                name="dateOfBirth"
                                control={control}
                                render={({ field }) => (
                                    <DateInput
                                        ref={field.ref}
                                        id={field.name}
                                        value={field.value ?? ""}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="الرقم الوطني / الجواز" required={!retireeIsAlive && !isMinor} error={errors.nationalID?.message} className="lg:col-span-3">
                            <Input
                                {...register("nationalID")}
                                placeholder={isMinor || retireeIsAlive ? "اختياري" : "أدخل الرقم الوطني"}
                            />
                        </FormField>

                        <FormField label="رقم الهاتف" error={errors.phone?.message} className="lg:col-span-2">
                            <Input {...register("phone")} placeholder="7xxxxxxx" />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="الحالة الاجتماعية والصحية" icon={Heart}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                        <FormField label="الحالة الاجتماعية" required={!retireeIsAlive} error={errors.socialStatusID?.message}>
                            <Controller
                                name="socialStatusID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        ref={field.ref}
                                        options={lookups?.socialStatuses.map(s => ({ value: s.id.toString(), label: s.name })) || []}
                                        value={field.value || ""}
                                        onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                        placeholder={retireeIsAlive ? "اختياري" : "اختر الحالة..."}
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="الحالة الصحية" required={!retireeIsAlive} error={errors.healthStatusID?.message}>
                            <Controller
                                name="healthStatusID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        ref={field.ref}
                                        options={lookups?.healthStatuses.map(h => ({ value: h.id.toString(), label: h.name })) || []}
                                        value={field.value || ""}
                                        onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                        placeholder={retireeIsAlive ? "اختياري" : "اختر الحالة..."}
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="الاستثناءات" error={errors.hasDisabilityException?.message}>
                            <label
                                htmlFor="hasDisabilityException"
                                className="flex items-center gap-2.5 h-11 sm:h-10 px-3 bg-primary/5 hover:bg-primary/10 border border-primary rounded-md w-full cursor-pointer transition-colors"
                            >
                                <ShieldCheck className="w-4 h-4 text-primary shrink-0" />
                                <input
                                    type="checkbox"
                                    id="hasDisabilityException"
                                    className="w-4 h-4 text-primary border-border rounded focus:ring-primary cursor-pointer transition-all"
                                    {...register("hasDisabilityException")}
                                />
                                <span className="text-xs font-bold text-primary hover:text-primary transition-colors select-none">
                                    استثناء عجز / عالة
                                </span>
                            </label>
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="حصة المعاش" icon={Percent}>
                    <Controller name="entitledAmount" control={control} render={({ field, fieldState }) => (
                        <EntitlementAmountField retireeId={file?.retireeID} value={field.value} onChange={field.onChange}
                            onBlur={field.onBlur} inputRef={field.ref} savedSharePercent={effectiveData?.sharePercent} error={fieldState.error?.message} />
                    )} />
                </FormSection>

                {isAdult && (
                    <FormSection title="الهوية الوطنية" icon={User}>
                        <FileField
                            name="nationalIdImage"
                            control={control}
                            label="صورة الهوية الوطنية"
                            accept="image/*,.pdf"
                            size="sm"
                            required={!retireeIsAlive}
                        />
                    </FormSection>
                )}

                {retireeIsAlive && effectiveMode === "create" && (
                    <NoteBox type="info" title="ملاحظة">
                        المتقاعد على قيد الحياة — سيُضاف الوريث بحالة <strong>موقوف</strong> حتى تسجيل وفاة المتقاعد.
                        يمكنك إكمال بياناته لاحقاً.
                    </NoteBox>
                )}
            </div>
        </BaseFormDialog>
    );
}
