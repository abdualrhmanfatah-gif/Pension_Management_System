"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { Calendar, FileSignature, FileText, Printer } from "lucide-react";
import { useEffect, useMemo, useRef } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

<<<<<<< HEAD
import { DocumentChecklistField } from "@/components/domain/retirement/DocumentChecklistField";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { 
  BaseFormDialog, 
  FileField, 
  FormField, 
  FormSection,
  Input, 
<<<<<<< HEAD
  NativeSelect, 
  NoteBox,
  Textarea} from "@/components/ui";
=======
  NoteBox,
  NativeSelect, 
  Textarea} from "@/components/ui";
import { DocumentChecklistField } from "@/components/domain/retirement/DocumentChecklistField";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type { MissingDocOption } from "@/utils/missing-documents";

// ─── Validation Schema ─────────────────────────────────

const pledgeSchema = z.object({
    missingDocumentIDs: z.array(z.union([z.number(), z.string()])).min(1, "يجب اختيار وثيقة واحدة على الأقل"),
    deadlineMonths: z.number({ message: "المهلة مطلوبة" }).min(1, "المهلة يجب أن تكون شهراً واحداً على الأقل"),
    pledgedByType: z.enum(["retiree", "agent", "entitled", "guardian"], {
        message: "نوع المتعهد مطلوب",
    }),
    selectedID: z.string().min(1, "يجب اختيار الشخص المتعهد"),
    reason: z.string().trim().optional().or(z.literal("")),
    pledgeFile: z.instanceof(File).optional().nullable(),
    issueOfficialPledge: z.boolean().optional(),
});

export type PledgeFormValues = z.infer<typeof pledgeSchema>;

interface PersonOption { id: number; fullName: string; }

interface PledgeDialogProps {
    open?: boolean;
    isOpen?: boolean;
    onOpenChange?: (open: boolean) => void;
    onClose?: () => void;
<<<<<<< HEAD
    onSubmit: (values: PledgeFormValues, options?: { keepOpen?: boolean }) => Promise<void> | void;
=======
    onSubmit: (values: PledgeFormValues) => Promise<void> | void;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    onSaveAndAddAnother?: () => void;
    isLoading?: boolean;
    missingDocs: MissingDocOption[];
    retireeId?: number;
    retireeName?: string;
    lookups: {
        agents: PersonOption[];
        entitleds: PersonOption[];
        guardians: PersonOption[];
    };
    initialData?: Partial<PledgeFormValues>;
    mode?: "create" | "edit";
}

/**
 * حوار إضافة تعهد إحضار وثائق - Named Export (Constraint 8)
 */
const DRAFT_KEY = "pledge_draft";

export function PledgeDialog({ 
    open, 
    isOpen,
    onOpenChange,
    onClose, 
    onSubmit, 
    onSaveAndAddAnother,
    isLoading,
    missingDocs,
    retireeId,
    retireeName,
    lookups,
    initialData,
    mode = "create"
}: PledgeDialogProps) {
<<<<<<< HEAD
    const freshCreateDefaults = useMemo(() => ({
        missingDocumentIDs: initialData?.missingDocumentIDs || [],
        deadlineMonths: initialData?.deadlineMonths || 3,
        pledgedByType: initialData?.pledgedByType || "retiree",
        selectedID: initialData?.selectedID || (retireeId ? String(retireeId) : ""),
        reason: initialData?.reason || "",
        pledgeFile: null,
    }), [initialData, retireeId]);

    const form = useForm<PledgeFormValues>({
        resolver: zodResolver(pledgeSchema),
        defaultValues: freshCreateDefaults,
    });

    const { control, register, setValue, watch, reset, formState: { errors, isDirty } } = form;

    useEffect(() => {
        if (open && mode === "create") {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (saved) {
                try {
                    reset(JSON.parse(saved));
                } catch { /* ignore */ }
            }
        }
    }, [open, mode, reset]);

=======
    const form = useForm<PledgeFormValues>({
        resolver: zodResolver(pledgeSchema),
        defaultValues: useMemo(() => {
            const saved = sessionStorage.getItem(DRAFT_KEY);
            if (mode === "create" && saved) {
                try {
                    return JSON.parse(saved);
                } catch { /* ignore */ }
            }
            return {
                missingDocumentIDs: initialData?.missingDocumentIDs || [],
                deadlineMonths: initialData?.deadlineMonths || 3,
                pledgedByType: initialData?.pledgedByType || "retiree",
                selectedID: initialData?.selectedID || (retireeId ? String(retireeId) : ""),
                reason: initialData?.reason || "",
                pledgeFile: null,
            };
        }, [mode, initialData, retireeId])
    });

    const { control, register, setValue, watch, formState: { errors, isDirty } } = form;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const watchedDocIDs = useWatch({ control, name: "missingDocumentIDs" });
    const selectedDocIDs = useMemo(() => watchedDocIDs || [], [watchedDocIDs]);
    const pledgedByType = useWatch({ control, name: "pledgedByType" });
    const deadlineMonths = useWatch({ control, name: "deadlineMonths" });

    // Auto-select pledger if only one document is selected and it has an owner
    useEffect(() => {
        if (mode === "create" && selectedDocIDs.length === 1) {
            const doc = missingDocs.find(d => String(d.id) === String(selectedDocIDs[0]));
            if (doc) {
                if (doc.entitledID) {
<<<<<<< HEAD
                    setValue("pledgedByType", "entitled", { shouldDirty: false });
                    setValue("selectedID", String(doc.entitledID), { shouldDirty: false });
                } else if (doc.agentID) {
                    setValue("pledgedByType", "agent", { shouldDirty: false });
                    setValue("selectedID", String(doc.agentID), { shouldDirty: false });
                } else if (doc.guardianID) {
                    setValue("pledgedByType", "guardian", { shouldDirty: false });
                    setValue("selectedID", String(doc.guardianID), { shouldDirty: false });
                } else if (doc.entityType === "Retiree") {
                    setValue("pledgedByType", "retiree", { shouldDirty: false });
                    setValue("selectedID", String(retireeId), { shouldDirty: false });
=======
                    setValue("pledgedByType", "entitled", { shouldDirty: true });
                    setValue("selectedID", String(doc.entitledID), { shouldDirty: true });
                } else if (doc.agentID) {
                    setValue("pledgedByType", "agent", { shouldDirty: true });
                    setValue("selectedID", String(doc.agentID), { shouldDirty: true });
                } else if (doc.guardianID) {
                    setValue("pledgedByType", "guardian", { shouldDirty: true });
                    setValue("selectedID", String(doc.guardianID), { shouldDirty: true });
                } else if (doc.entityType === "Retiree") {
                    setValue("pledgedByType", "retiree", { shouldDirty: true });
                    setValue("selectedID", String(retireeId), { shouldDirty: true });
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                }
            }
        }
    }, [selectedDocIDs, mode, missingDocs, setValue, retireeId]);

    const allValues = watch();
    useEffect(() => {
        if (mode === "create" && isDirty) {
            const { pledgeFile: _f, ...textFields } = allValues;
            void _f;
            sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
        }
    }, [allValues, mode, isDirty]);

    const deadlinePreview = () => {
        if (!deadlineMonths || isNaN(deadlineMonths)) return null;
        const d = new Date();
        d.setMonth(d.getMonth() + Number(deadlineMonths));
        return d.toLocaleDateString("ar-YE", { year: 'numeric', month: 'long', day: 'numeric' });
    };

    const addAnotherRef = useRef(false);

    const handleFormSubmit = async (values: PledgeFormValues) => {
<<<<<<< HEAD
        try {
            await onSubmit(values, { keepOpen: addAnotherRef.current });
        } catch (err) {
            addAnotherRef.current = false;
            throw err;
        }
        sessionStorage.removeItem(DRAFT_KEY);
        if (addAnotherRef.current) {
            form.reset(freshCreateDefaults);
=======
        await onSubmit(values);
        if (addAnotherRef.current) {
            form.reset();
            sessionStorage.removeItem(DRAFT_KEY);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            addAnotherRef.current = false;
            onSaveAndAddAnother?.();
        }
    };

    const handleSaveAndAddAnother = () => {
        addAnotherRef.current = true;
<<<<<<< HEAD
        return form.handleSubmit(handleFormSubmit, () => {
            addAnotherRef.current = false;
        })();
=======
        void form.handleSubmit(handleFormSubmit)();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    };

    return (
        <BaseFormDialog
            open={open}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title={mode === "create" ? "إضافة تعهد إحضار وثائق" : "تعديل بيانات التعهد"}
            description="الالتزام بإحضار الوثائق الناقصة خلال فترة زمنية محددة لضمان استمرار المعاملة"
            size="full"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            onSaveAndNew={mode === "create" ? handleSaveAndAddAnother : undefined}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "حفظ التعهد" : "تحديث التعهد"}
        >
            <div className="space-y-4 text-start" dir="rtl">
                <FormSection title="الوثائق المشمولة بالتعهد" icon={FileSignature}>
                    <DocumentChecklistField
                        selectedIds={selectedDocIDs}
                        documents={missingDocs}
                        onChange={(ids) => setValue("missingDocumentIDs", ids, { shouldDirty: true })}
                        error={errors.missingDocumentIDs?.message}
                        label="اختر الوثائق الناقصة"
                        emptyMessage="لا توجد وثائق ناقصة متاحة لهذا الملف"
                    />
                </FormSection>

                <FormSection title="تفاصيل المتعهد والمهلة" icon={Calendar}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        <FormField label="نوع المتعهد" required error={errors.pledgedByType?.message}>
                            <Controller
                                name="pledgedByType"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
                                        options={[
                                            { value: "retiree", label: "المتقاعد نفسه" },
                                            { value: "entitled", label: "أحد المستحقين" },
                                            { value: "agent", label: "وكيل" },
                                            { value: "guardian", label: "وصي" },
                                        ]}
                                        value={field.value}
                                        onChange={(val) => {
                                            field.onChange(val);
                                            setValue("selectedID", val === "retiree" && retireeId ? String(retireeId) : "");
                                        }}
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="اسم المتعهد" required error={errors.selectedID?.message}>
                            <Controller
                                name="selectedID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
                                        options={
                                            pledgedByType === "retiree" ? (retireeId ? [{ value: String(retireeId), label: retireeName || "صاحب الملف" }] : []) :
                                            pledgedByType === "agent" ? lookups.agents.map(a => ({ value: String(a.id), label: a.fullName })) :
                                            pledgedByType === "entitled" ? lookups.entitleds.map(e => ({ value: String(e.id), label: e.fullName })) :
                                            pledgedByType === "guardian" ? lookups.guardians.map(g => ({ value: String(g.id), label: g.fullName })) : []
                                        }
                                        value={field.value}
                                        onChange={field.onChange}
                                        disabled={!pledgedByType}
                                        placeholder="اختر الاسم..."
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="المهلة الزمنية (بالأشهر)" required error={errors.deadlineMonths?.message}>
                            <div className="space-y-2">
                                <Input 
                                    type="number" 
                                    {...register("deadlineMonths", { valueAsNumber: true })} 
                                    placeholder="مثلاً: 3"
                                    className="font-mono font-black"
                                />
                                {deadlinePreview() && (
                                    <p className="text-[10px] sm:text-xs text-primary font-bold px-1 italic">
                                        تاريخ الاستحقاق المتوقع: {deadlinePreview()}
                                    </p>
                                )}
                            </div>
                        </FormField>

                        <div className="md:col-span-1">
                            <FormField label="ملاحظات (اختياري)" error={errors.reason?.message}>
                                <Textarea {...register("reason")} placeholder="أدخل تفاصيل إضافية..." rows={1} />
                            </FormField>
                        </div>
                    </div>
                </FormSection>

                {mode === "create" && (
                    <FormSection title="المرفقات" icon={FileText}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-end">
                            <FileField 
                                name="pledgeFile" 
                                control={control} 
                                label="صورة نموذج التعهد المعتمد" 
                                hint="يرجى رفع الوثيقة الموقعة (اختياري)"
                                size="sm"
                            />
                            
<<<<<<< HEAD
                            <div className="flex items-center gap-3 p-4 rounded-2xl border border-primary bg-primary/5 hover:bg-primary/10 transition-colors cursor-pointer group mb-1">
=======
                            <div className="flex items-center gap-3 p-4 rounded-2xl border border-primary/20 bg-primary/5 hover:bg-primary/10 transition-colors cursor-pointer group mb-1">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                <input 
                                    type="checkbox" 
                                    id="issueOfficialPledge"
                                    {...register("issueOfficialPledge")}
<<<<<<< HEAD
                                    className="min-h-[44px] min-w-[44px] w-6 h-6 rounded border-primary text-primary focus:ring-primary/20 cursor-pointer"
=======
                                    className="min-h-[44px] min-w-[44px] w-6 h-6 rounded border-primary/30 text-primary focus:ring-primary/20 cursor-pointer"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                />
                                <label htmlFor="issueOfficialPledge" className="flex flex-col cursor-pointer select-none">
                                    <span className="text-sm font-black text-primary flex items-center gap-1.5">
                                        <Printer className="w-4 h-4" />
                                        إصدار تعهد رسمي
                                    </span>
                                    <span className="text-[10px] sm:text-xs text-muted-foreground font-medium">طباعة نموذج التعهد الرسمي آلياً بعد الحفظ</span>
                                </label>
                            </div>
                        </div>
                    </FormSection>
                )}

                <NoteBox type="warning" title="تنبيه قانوني">
                    سيؤدي انتهاء المهلة دون إحضار الوثائق إلى إيقاف صرف المستحقات المالية للملف آلياً بناءً على اللوائح المنظمة.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
