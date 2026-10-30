import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { ClipboardList, Download, Eye, FileText, Settings,User } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { Controller, type Resolver, useForm } from "react-hook-form";
import { z } from "zod";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import { DocumentProcessDialog } from "@/components/domain/retirement/DocumentProcessDialog";
=======
import { ClipboardList, FileText, User, Eye, Download, Settings } from "lucide-react";
import { useMemo, useEffect, useRef, useState } from "react";
import { Controller, type Resolver, useForm } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import { documentsApi } from "@/api/documents";
import type { AgentSummary, EntitledPersonDto, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";
import type { DocumentItem } from "@/types/retirement";
import { DocumentProcessDialog } from "@/components/domain/retirement/DocumentProcessDialog";
import { getDocumentUrl } from "@/lib/utils";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  BaseFormDialog,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  DateInput,
  FormField,
  FormSection,
  NativeSelect,
<<<<<<< HEAD
  Text,
  Textarea,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { getDocumentUrl } from "@/lib/utils";
import type { AgentSummary, EntitledPersonDto, EntitledPersonSummary, GuardianSummary } from "@/types/retirement";
import type { DocumentItem } from "@/types/retirement";
=======
  Textarea,
  Text,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Validation Schema ─────────────────────────────────

const editSchema = z.object({
    documentTypeID: z.coerce.number().min(1, "نوع الوثيقة مطلوب"),
    ownerEntity: z.string().optional(),
<<<<<<< HEAD
    documentNumber: z.string().max(50, "رقم الوثيقة يتجاوز الحد الأقصى (50 حرف)").optional().or(z.literal("")),
    issueDate: z.string().optional().or(z.literal("")),
    expiryDate: z.string().optional().or(z.literal("")),
    notes: z.string().max(2000, "الملاحظات تتجاوز الحد الأقصى (2000 حرف)").optional().or(z.literal("")),
}).refine((data) => !data.issueDate || !data.expiryDate || data.expiryDate >= data.issueDate, {
    message: "تاريخ الانتهاء لا يمكن أن يسبق تاريخ الإصدار",
    path: ["expiryDate"],
=======
    documentNumber: z.string().optional().or(z.literal("")),
    issueDate: z.string().optional().or(z.literal("")),
    expiryDate: z.string().optional().or(z.literal("")),
    notes: z.string().optional().or(z.literal("")),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
});

type EditFormValues = z.infer<typeof editSchema>;

interface EditDocumentDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    document: DocumentItem | null;
    fileId?: number;
    onSaved?: () => void;
}

function buildOwnerValue(doc: DocumentItem): string {
    if (doc.agentID) return `agent_${doc.agentID}`;
    if (doc.guardianID) return `guardian_${doc.guardianID}`;
    if (doc.entitledID) return `entitled_${doc.entitledID}`;
    return "retiree";
}

const IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif"];

function isImageDocument(doc: DocumentItem | null): boolean {
    if (!doc?.fileName) return false;
    const lower = doc.fileName.toLowerCase();
    return IMAGE_EXTENSIONS.some((ext) => lower.endsWith(ext));
}

export function EditDocumentDialog({
    open,
    onOpenChange,
    document,
    fileId,
    onSaved,
}: EditDocumentDialogProps) {
    // 1. Fetch doc types (all active types)
    const { data: fetchedDocTypes } = useQuery({
        queryKey: ["document-types", "lookup"],
        queryFn: () => documentsApi.getTypes(),
        enabled: open,
    });

    const docTypes = fetchedDocTypes || [];

    // 2. Fetch file entities
    const { data: fileEntities, isLoading: loadingEntities } = useQuery({
        queryKey: ["file-entities", fileId],
        queryFn: async () => {
            if (!fileId) return [];
            const [agents, guardians, entitled] = await Promise.all([
                retirementApi.getAgents(fileId).catch((): AgentSummary[] => []),
                retirementApi.getGuardians(fileId).catch((): GuardianSummary[] => []),
                retirementApi.getEntitledPersons(fileId).catch((): EntitledPersonDto[] => []),
            ]);

            const entities = [
                { value: "retiree", label: "المتقاعد (صاحب الملف)" }
            ];

            agents.forEach((a: AgentSummary) => entities.push({ value: `agent_${a.agentID}`, label: `الوكيل: ${a.fullName}` }));
            guardians.forEach((g: GuardianSummary) => entities.push({ value: `guardian_${g.guardianID}`, label: `الوصي: ${g.fullName}` }));
            entitled.forEach((e: EntitledPersonSummary) => entities.push({ value: `entitled_${e.entitledID}`, label: `المستفيد: ${e.fullName}` }));

            return entities;
        },
        enabled: open && !!fileId,
    });

    // 3. Mutation
    const mutation = useApiMutation(
        async (values: EditFormValues) => {
            if (!document) return;

            const formData = new FormData();
            formData.append("DocumentTypeID", String(values.documentTypeID));
<<<<<<< HEAD
            // Notes يُرسل دائماً: الفارغ يعني مسح الملاحظة (null في الخادم = لم يُرسل)
            formData.append("Notes", values.notes ?? "");
=======
            if (values.notes) formData.append("Notes", values.notes);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (values.documentNumber) formData.append("DocumentNumber", values.documentNumber);
            if (values.issueDate) formData.append("IssueDate", values.issueDate);
            if (values.expiryDate) formData.append("ExpiryDate", values.expiryDate);

            if (values.ownerEntity && values.ownerEntity !== "retiree") {
                const [type, id] = values.ownerEntity.split("_");
                if (type === "agent" && id) formData.append("AgentID", id);
                if (type === "guardian" && id) formData.append("GuardianID", id);
                if (type === "entitled" && id) formData.append("EntitledPersonID", id);
<<<<<<< HEAD
            } else {
                // المالك المتقاعد: يُرسل صراحة لتفادي التخلي عن المالك عند التعديل
                if (document.retireeID) formData.append("RetireeID", String(document.retireeID));
                else if (fileId) formData.append("RetirementFileID", String(fileId));
=======
            } else if (values.ownerEntity === "retiree") {
                if (fileId) formData.append("RetirementFileID", String(fileId));
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            }

            return documentsApi.update(document.documentID, formData);
        },
        {
            invalidateKeys: [["documents"], fileId ? ["retirement-file", fileId] : []],
            successMessage: "تم تحديث الوثيقة بنجاح",
        }
    );

    // 4. Form
    const form = useForm<EditFormValues>({
        resolver: zodResolver(editSchema) as Resolver<z.infer<typeof editSchema>>,
        defaultValues: {
            documentTypeID: 0,
            ownerEntity: "retiree",
            documentNumber: "",
            issueDate: "",
            expiryDate: "",
            notes: "",
        },
    });

    const { control, register, handleSubmit, reset, formState: { errors, isDirty } } = form;
    const [processOpen, setProcessOpen] = useState(false);

    // 5. Pre-fill when document changes
    const initialValues = useMemo(() => {
        if (!document) return {
            documentTypeID: 0,
            ownerEntity: "retiree",
            documentNumber: "",
            issueDate: "",
            expiryDate: "",
            notes: "",
        };
        return {
            documentTypeID: document.documentTypeID,
            ownerEntity: buildOwnerValue(document),
            documentNumber: document.documentNumber ?? "",
            issueDate: document.issueDate ?? "",
            expiryDate: document.expiryDate ?? "",
            notes: document.notes ?? "",
        };
    }, [document]);

    // 6. Reset form directly when dialog opens with document
    const prevOpenRef = useRef(false);
    useEffect(() => {
        if (open && !prevOpenRef.current && document) {
            reset(initialValues);
        }
        prevOpenRef.current = open;
    }, [open, document, initialValues, reset]);

    const handleEditSubmit = async (values: EditFormValues) => {
        await mutation.mutateAsync(values);
        onSaved?.();
        onOpenChange(false);
        reset();
    };

    const documentUrl = useMemo(() => {
        if (!document?.filePath) return null;
        return getDocumentUrl(document.filePath);
    }, [document?.filePath]);

    return (
        <>
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تعديل وثيقة"
            description={document ? `تعديل بيانات: ${document.documentTypeName}` : "تعديل بيانات الوثيقة"}
            size="full"
            portal
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(handleEditSubmit)}
            resetOnOpen={false}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel="حفظ التعديلات"
        >
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Right: Form Data */}
                <div className="space-y-4 order-2 lg:order-1">
                    <FormSection title="تصنيف الوثيقة" icon={ClipboardList}>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <FormField label="نوع الوثيقة" required error={errors.documentTypeID?.message}>
                                <Controller
                                    name="documentTypeID"
                                    control={control}
                                    render={({ field }) => (
                                        <NativeSelect
                                            options={docTypes.map((t) => ({ value: t.id.toString(), label: t.name }))}
                                            value={field.value?.toString() || ""}
                                            onChange={(val) => field.onChange(val ? Number(val) : 0)}
                                            placeholder="اختر نوع الوثيقة..."
                                        />
                                    )}
                                />
                            </FormField>

                            {fileId && (
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

                    <FormSection title="بيانات الوثيقة" icon={FileText}>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <FormField label="تاريخ الإصدار" error={errors.issueDate?.message}>
                                <Controller
                                    name="issueDate"
                                    control={control}
                                    render={({ field }) => (
                                        <DateInput
                                            value={field.value || ""}
                                            onChange={field.onChange}
                                            placeholder="YYYY-MM-DD"
                                        />
                                    )}
                                />
                            </FormField>

                            <FormField label="تاريخ الانتهاء" error={errors.expiryDate?.message}>
                                <Controller
                                    name="expiryDate"
                                    control={control}
                                    render={({ field }) => (
                                        <DateInput
                                            value={field.value || ""}
                                            onChange={field.onChange}
                                            placeholder="YYYY-MM-DD"
                                        />
                                    )}
                                />
                            </FormField>
                        </div>

                        <FormField label="ملاحظات" error={errors.notes?.message} icon={FileText}>
                            <Textarea
                                {...register("notes")}
                                placeholder="أضف أي تفاصيل إضافية..."
                                className="min-h-[70px] sm:min-h-[80px] leading-relaxed"
                            />
                        </FormField>
                    </FormSection>
                </div>

                {/* Left: Document Preview */}
                <div className="order-1 lg:order-2 lg:sticky lg:top-0 lg:self-start">
                    {documentUrl ? (
                        <Card className="overflow-hidden">
                            <CardHeader className="pb-3 flex flex-row items-center justify-between">
                                <CardTitle className="text-sm flex items-center gap-2">
                                    <FileText className="h-4 w-4" />
                                    الوثيقة المرفقة
                                </CardTitle>
                                <div className="flex items-center gap-1">
                                    {isImageDocument(document) && (
                                        <Button
                                            type="button"
                                            variant="ghost"
                                            size="icon"
                                            className="h-7 w-7"
                                            onClick={() => setProcessOpen(true)}
                                            title="معالجة الصورة"
                                        >
                                            <Settings className="h-4 w-4" />
                                        </Button>
                                    )}
                                    <a
                                        href={documentUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="inline-flex items-center justify-center h-7 w-7 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
                                        title="عرض بحجم كامل"
                                    >
                                        <Eye className="h-4 w-4" />
                                    </a>
                                    <a
                                        href={documentUrl}
                                        download={document?.fileName}
                                        className="inline-flex items-center justify-center h-7 w-7 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
                                        title="تحميل"
                                    >
                                        <Download className="h-4 w-4" />
                                    </a>
                                </div>
                            </CardHeader>
                            <CardContent className="pt-0">
                                {isImageDocument(document) ? (
                                    <img
                                        src={documentUrl}
                                        alt={document?.fileName || "الوثيقة"}
                                        className="w-full rounded-md border object-contain"
                                        style={{ maxHeight: "calc(100dvh - 300px)", minHeight: 300 }}
                                    />
                                ) : (
                                    <iframe
                                        src={documentUrl}
                                        title={document?.fileName || "الوثيقة"}
                                        className="w-full rounded-md border"
                                        style={{ height: "calc(100dvh - 300px)", minHeight: 300 }}
                                    />
                                )}
                                {document?.fileName && (
                                    <Text variant="caption" className="text-center mt-2 truncate">
                                        {document.fileName}
                                    </Text>
                                )}
                            </CardContent>
                        </Card>
                    ) : (
                        <Card className="border-dashed">
                            <CardContent className="flex flex-col items-center justify-center py-16 text-muted-foreground">
<<<<<<< HEAD
                                <FileText className="h-12 w-12 mb-3 opacity-100" />
=======
                                <FileText className="h-12 w-12 mb-3 opacity-40" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                <Text variant="body">لا توجد وثيقة مرفقة</Text>
                            </CardContent>
                        </Card>
                    )}
                </div>
            </div>
        </BaseFormDialog>

        {document && (
            <DocumentProcessDialog
                open={processOpen}
                onOpenChange={setProcessOpen}
                document={{
                    documentID: document.documentID,
                    documentTypeID: document.documentTypeID,
                    documentTypeName: document.documentTypeName,
                    fileName: document.fileName,
                    filePath: document.filePath,
                    notes: document.notes,
                    uploadedByUser: document.uploadedByUser,
                    createdAt: document.createdAt,
                }}
                fileId={fileId || 0}
                onSaved={onSaved}
            />
        )}
        </>
    );
}
