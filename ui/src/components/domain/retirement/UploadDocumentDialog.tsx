import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { ClipboardList, FileText, FileUp, User } from "lucide-react";
<<<<<<< HEAD
import { useId, useMemo } from "react";
import { Controller, type Resolver, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
=======
import { useMemo } from "react";
import { Controller, type Resolver, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import { documentsApi } from "@/api/documents";
import type { AgentSummary, EntitledPersonDto, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { 
  BaseFormDialog, 
  FileField, 
  FormField, 
  FormSection,
  NativeSelect, 
  Textarea, 
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
<<<<<<< HEAD
import type { AgentSummary, EntitledPersonDto, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Validation Schema ─────────────────────────────────

const uploadSchema = z.object({
    documentTypeID: z.coerce.number().min(1, "نوع الوثيقة مطلوب"),
    ownerEntity: z.string().optional(),
    file: z.instanceof(File, { message: "يرجى اختيار ملف صالح للمتابعة" }),
<<<<<<< HEAD
    // م33: حدود مطابقة للواجهة الخلفية
    notes: z.string().trim().max(2000, "الملاحظات تتجاوز الحد الأقصى (2000 حرف)").optional().or(z.literal("")),
=======
    notes: z.string().trim().optional().or(z.literal("")),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
});

type UploadFormValues = z.infer<typeof uploadSchema>;

interface UploadDocumentModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved?: () => void;
    fileId?: number;
    docTypes?: { id: number; name: string }[];
    defaultTypeId?: number;
    missingEntityIds?: { retireeID?: number; agentID?: number; guardianID?: number; entitledID?: number; pledgeID?: number; exemptionID?: number };
    // New props for cross-module usage
    ownerType?: number;
    entityName?: string;
    onSubmit?: (values: UploadFormValues) => Promise<void>;
    isLoading?: boolean;
    /** Portal the dialog to document.body. Default: true. Set false when used outside Sheet/Dialog nesting. */
    portal?: boolean;
}

/**
 * Standardized Document Upload Dialog.
 * Refactored for 100% compliance: Uses BaseFormDialog, FormSection, and FileField.
 */
export function UploadDocumentDialog({ 
    open, 
    onOpenChange, 
    onSaved,
    fileId,
    docTypes: providedDocTypes,
    defaultTypeId,
    missingEntityIds,
    ownerType,
    entityName,
    onSubmit,
    isLoading: providedIsLoading,
    portal = true
}: UploadDocumentModalProps) {
<<<<<<< HEAD
    const documentTypeInputId = useId();
    const ownerInputId = useId();
    // 1. Fetch doc types if not provided
    const { data: fetchedDocTypes } = useQuery({
        queryKey: ["document-types", ownerType ?? "all"],
        queryFn: async () => {
            // General retirement uploads use the same lookup as EditDocumentDialog.
            if (ownerType === undefined) return documentsApi.getTypes();
            return documentsApi.getTypesByOwner(String(ownerType));
        },
        enabled: open && !providedDocTypes
=======
    // 1. Fetch doc types if not provided
    const { data: fetchedDocTypes } = useQuery({
        queryKey: ["document-types", ownerType],
        queryFn: async () => {
            if (!ownerType) return [];
            return documentsApi.getTypesByOwner(String(ownerType));
        },
        enabled: open && !!ownerType && !providedDocTypes
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    });

    const docTypes = providedDocTypes || fetchedDocTypes || [];

    // Fetch file entities if it's a general upload (no missingEntityIds provided) and fileId is available
    const { data: fileEntities, isLoading: loadingEntities } = useQuery({
        queryKey: ["file-entities", fileId],
        queryFn: async () => {
            if (!fileId) return [];
            const [agents, guardians, entitled] = await Promise.all([
                retirementApi.getAgents(fileId).catch((): AgentSummary[] => []),
                retirementApi.getGuardians(fileId).catch((): GuardianSummary[] => []),
                retirementApi.getEntitledPersons(fileId).catch((): EntitledPersonDto[] => [])
            ]);
            
            const entities = [
                { value: "retiree", label: "المتقاعد (صاحب الملف)" }
            ];
            
            agents.forEach((a: AgentSummary) => entities.push({ value: `agent_${a.agentID}`, label: `الوكيل: ${a.fullName}` }));
            guardians.forEach((g: GuardianSummary) => entities.push({ value: `guardian_${g.guardianID}`, label: `الوصي: ${g.fullName}` }));
            entitled.forEach((e: EntitledPersonSummary) => entities.push({ value: `entitled_${e.entitledID}`, label: `المستفيد: ${e.fullName}` }));
            
            return entities;
        },
        enabled: open && !!fileId && !missingEntityIds
    });

    const showOwnerSelect = !!fileId && !missingEntityIds;

    const mutation = useApiMutation(
        async (formData: FormData) => {
            return documentsApi.upload(formData);
        },
        {
            invalidateKeys: [["documents"], fileId ? ["retirement-file", fileId] : []],
            successMessage: "تم رفع الوثيقة بنجاح",
        }
    );

    const form = useForm<UploadFormValues>({
        resolver: zodResolver(uploadSchema) as Resolver<z.infer<typeof uploadSchema>>,
        defaultValues: {
            documentTypeID: defaultTypeId || 0,
            ownerEntity: "retiree",
            file: undefined as unknown as File,
            notes: "",
        }
    });

    const { control, register, handleSubmit, reset, formState: { errors, isDirty } } = form;
    const documentTypeID = useWatch({ control, name: "documentTypeID" });

    const initialValues = useMemo(() => ({
        documentTypeID: defaultTypeId || 0,
        ownerEntity: "retiree",
        file: undefined as unknown as File,
        notes: "",
    }), [defaultTypeId]);

    const handleUploadSubmit = async (values: UploadFormValues) => {
        if (onSubmit) {
            await onSubmit(values);
            onOpenChange(false);
            reset();
            return;
        }

        const formData = new FormData();
        formData.append("File", values.file);
<<<<<<< HEAD
        formData.append("DocumentTypeID", String(values.documentTypeID));
        if (values.notes) formData.append("Notes", values.notes);

        // مالك واحد فقط. إرسال RetirementFileID مع معرّف كيان معًا يجعل الخدمة
        // تحسم الأولوية للكيان (DocumentService يحل الكيان قبل الملف) بينما قيد
        // CK_Document_OwnerType يشترط عمود المالك المطابق لنوع الوثيقة.
        let ownerSent = false;
        const appendIfPresent = (key: string, value: number | undefined | null) => {
            if (!value) return;
            formData.append(key, String(value));
            ownerSent = true;
        };

        if (missingEntityIds) {
            appendIfPresent("RetireeID", missingEntityIds.retireeID);
            appendIfPresent("AgentID", missingEntityIds.agentID);
            appendIfPresent("GuardianID", missingEntityIds.guardianID);
            appendIfPresent("EntitledPersonID", missingEntityIds.entitledID);
            appendIfPresent("PledgeID", missingEntityIds.pledgeID);
            appendIfPresent("ExemptionID", missingEntityIds.exemptionID);
        } else if (values.ownerEntity && values.ownerEntity !== "retiree") {
            const [type, id] = values.ownerEntity.split("_");
            if (id) {
                if (type === "agent") formData.append("AgentID", id);
                if (type === "guardian") formData.append("GuardianID", id);
                if (type === "entitled") formData.append("EntitledPersonID", id);
                ownerSent = !!(type === "agent" || type === "guardian" || type === "entitled");
            }
        }

        if (!ownerSent && fileId) {
            formData.append("RetirementFileID", String(fileId));
=======
        
        if (fileId) formData.append("RetirementFileID", String(fileId));
        formData.append("DocumentTypeID", String(values.documentTypeID));
        if (values.notes) formData.append("Notes", values.notes);
        
        if (missingEntityIds) {
            if (missingEntityIds.retireeID) formData.append("RetireeID", String(missingEntityIds.retireeID));
            if (missingEntityIds.agentID) formData.append("AgentID", String(missingEntityIds.agentID));
            if (missingEntityIds.guardianID) formData.append("GuardianID", String(missingEntityIds.guardianID));
            if (missingEntityIds.entitledID) formData.append("EntitledPersonID", String(missingEntityIds.entitledID));
            if (missingEntityIds.pledgeID) formData.append("PledgeID", String(missingEntityIds.pledgeID));
            if (missingEntityIds.exemptionID) formData.append("ExemptionID", String(missingEntityIds.exemptionID));
        } else if (values.ownerEntity && values.ownerEntity !== "retiree") {
            const [type, id] = values.ownerEntity.split("_");
            if (type === "agent" && id) formData.append("AgentID", id);
            if (type === "guardian" && id) formData.append("GuardianID", id);
            if (type === "entitled" && id) formData.append("EntitledPersonID", id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        }

        await mutation.mutateAsync(formData);
        
        onSaved?.();
        onOpenChange(false);
        reset();
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="رفع وثيقة ثبوتية"
            description={entityName ? `إرفاق المستندات والوثائق لـ: ${entityName}` : "إرفاق المستندات والوثائق المؤيدة للملف التقاعدي"}
            size="full"
            portal={portal}
            isLoading={providedIsLoading || mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(handleUploadSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="بدء الرفع والاعتماد"
        >
            <div className="space-y-3 sm:space-y-4">
                <FormSection title="تصنيف الوثيقة" icon={ClipboardList}>
                    <div className="w-full max-w-3xl grid grid-cols-1 sm:grid-cols-2 gap-4">
<<<<<<< HEAD
                        <FormField htmlFor={documentTypeInputId} label="نوع الوثيقة المرفوعة" required error={errors.documentTypeID?.message}>
=======
                        <FormField label="نوع الوثيقة المرفوعة" required error={errors.documentTypeID?.message}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <Controller
                                name="documentTypeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
<<<<<<< HEAD
                                        id={documentTypeInputId}
                                        ref={field.ref}
                                        aria-labelledby={`${documentTypeInputId}-label`}
                                        aria-invalid={!!errors.documentTypeID || undefined}
                                        aria-describedby={errors.documentTypeID ? `${documentTypeInputId}-error` : undefined}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        options={docTypes.map((t) => ({ value: t.id.toString(), label: t.name }))}
                                        value={field.value?.toString() || ""}
                                        onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                        placeholder="اختر نوع الوثيقة من القائمة..."
                                    />
                                )}
                            />
                        </FormField>

                        {showOwnerSelect && (
<<<<<<< HEAD
                            <FormField htmlFor={ownerInputId} label="صاحب الوثيقة" error={errors.ownerEntity?.message} icon={User}>
=======
                            <FormField label="صاحب الوثيقة" error={errors.ownerEntity?.message} icon={User}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                <Controller
                                    name="ownerEntity"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect 
<<<<<<< HEAD
                                            id={ownerInputId}
                                            ref={field.ref}
                                            aria-labelledby={`${ownerInputId}-label`}
                                            aria-invalid={!!errors.ownerEntity || undefined}
                                            aria-describedby={errors.ownerEntity ? `${ownerInputId}-error` : undefined}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            options={fileEntities || [{ value: "retiree", label: "المتقاعد (صاحب الملف)" }]}
                                            value={field.value || "retiree"}
                                            onChange={field.onChange}
                                            disabled={loadingEntities}
                                        />
                                    )}
                                />
                            </FormField>
                        )}
                    </div>
                </FormSection>

                <FormSection title="الملف والمرفقات" icon={FileUp}>
                    <div className="grid grid-cols-1 gap-3 sm:gap-4">
                        <FileField 
                            name="file" 
                            control={control} 
                            label={documentTypeID === 1 ? "الهوية الوطنية" : "اختيار ملف الوثيقة"} 
                            hint="PDF, PNG, JPG (الحد الأقصى 10MB)"
                            required 
                            accept=".pdf,image/*" 
                            size="sm"
                        />
                        
                        <div>
                            <FormField label="ملاحظات توضيحية (اختياري)" error={errors.notes?.message} icon={FileText}>
                                <Textarea 
                                    {...register("notes")} 
                                    placeholder="أضف أي تفاصيل إضافية..." 
                                    className="min-h-[70px] sm:min-h-[80px] leading-relaxed"
                                />
                            </FormField>
                        </div>
                    </div>
                </FormSection>

       
            </div>
        </BaseFormDialog>
    );
}
