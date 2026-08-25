import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { ClipboardList, FileText, FileUp, User } from "lucide-react";
import { useMemo } from "react";
import { Controller, type Resolver, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import { documentsApi } from "@/api/documents";
import type { AgentSummary, EntitledPersonDto, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";

import { 
  BaseFormDialog, 
  FileField, 
  FormField, 
  FormSection,
  NativeSelect, 
  Textarea, 
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const uploadSchema = z.object({
    documentTypeID: z.coerce.number().min(1, "نوع الوثيقة مطلوب"),
    ownerEntity: z.string().optional(),
    file: z.instanceof(File, { message: "يرجى اختيار ملف صالح للمتابعة" }),
    notes: z.string().trim().optional().or(z.literal("")),
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
    // 1. Fetch doc types if not provided
    const { data: fetchedDocTypes } = useQuery({
        queryKey: ["document-types", ownerType],
        queryFn: async () => {
            if (!ownerType) return [];
            return documentsApi.getTypesByOwner(String(ownerType));
        },
        enabled: open && !!ownerType && !providedDocTypes
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
                        <FormField label="نوع الوثيقة المرفوعة" required error={errors.documentTypeID?.message}>
                            <Controller
                                name="documentTypeID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
                                        options={docTypes.map((t) => ({ value: t.id.toString(), label: t.name }))}
                                        value={field.value?.toString() || ""}
                                        onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                        placeholder="اختر نوع الوثيقة من القائمة..."
                                    />
                                )}
                            />
                        </FormField>

                        {showOwnerSelect && (
                            <FormField label="صاحب الوثيقة" error={errors.ownerEntity?.message} icon={User}>
                                <Controller
                                    name="ownerEntity"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect 
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
