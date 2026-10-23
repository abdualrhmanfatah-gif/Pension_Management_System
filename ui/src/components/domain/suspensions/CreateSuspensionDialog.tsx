import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Ban, FileText, Landmark, Search } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import { retirementApi } from "@/api/retirement";
import {
    BaseFormDialog,
    Button,
    FormField,
    FormSection,
    Input,
    Text,
    Textarea} from "@/components/ui";

// ─── Validation Schema ─────────────────────────────────

const suspensionSchema = z.object({
    fileID: z.number({ message: "يجب اختيار ملف التقاعد" }).min(1, "يجب اختيار ملف التقاعد"),
    suspensionReason: z.string().trim().min(5, "يجب إدخال سبب الإيقاف (5 أحرف على الأقل)"),
    suspensionSource: z.string().trim().min(3, "يجب إدخال مصدر الإيقاف (الجهة أو الأمر الإداري)"),
});

export type CreateSuspensionFormValues = z.infer<typeof suspensionSchema>;

interface CreateSuspensionDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onCreated?: () => void;
}

// ─── File Search Result Type ────────────────────────────

interface FileSearchResult {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    rankName?: string;
    statusName: string;
}

/**
 * حوار إنشاء إيقاف معاش جديد — مع بحث عن الملف
 */
export function CreateSuspensionDialog({ open, onOpenChange, onCreated }: CreateSuspensionDialogProps) {
    const [searchTerm, setSearchTerm] = useState("");
    const [selectedFile, setSelectedFile] = useState<FileSearchResult | null>(null);

    const form = useForm<CreateSuspensionFormValues>({
        resolver: zodResolver(suspensionSchema),
        defaultValues: {
            fileID: 0,
            suspensionReason: "",
            suspensionSource: "",
        }
    });

    const { register, setValue, handleSubmit, formState: { errors, isDirty }, reset } = form;

    // ── Search for files ────────────────────────────────
    const { data: searchResults = [], isLoading: isSearching } = useQuery({
        queryKey: ["file-search-suspension", searchTerm],
        queryFn: async () => {
            if (searchTerm.length < 2) return [];
            const res = await retirementApi.getFiles({ page: 1, pageSize: 10, search: searchTerm });
            return (res?.items || []).map((item: any) => ({
                fileID: item.fileID,
                fileNumber: item.fileNumber,
                retireeFullName: item.retireeFullName,
                rankName: item.rankName,
                statusName: item.fileStatusName || item.statusName || "",
            })) as FileSearchResult[];
        },
        enabled: searchTerm.length >= 2,
        staleTime: 3000,
    });

    const handleFileSelect = (file: FileSearchResult) => {
        setSelectedFile(file);
        setValue("fileID", file.fileID, { shouldDirty: true, shouldValidate: true });
        setSearchTerm("");
    };

    const handleFormSubmit = async (values: CreateSuspensionFormValues) => {
        try {
            await retirementApi.createSuspension({
                fileID: values.fileID,
                suspensionReason: values.suspensionReason,
                suspensionSource: values.suspensionSource,
            } as any);
            reset();
            setSelectedFile(null);
            onOpenChange(false);
            onCreated?.();
        } catch (error: any) {
            const msg = error?.response?.data?.message || error?.message || "فشل إنشاء الإيقاف";
            alert(msg);
        }
    };

    const handleOpenChange = (isOpen: boolean) => {
        if (!isOpen) {
            reset();
            setSelectedFile(null);
            setSearchTerm("");
        }
        onOpenChange(isOpen);
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={handleOpenChange}
            title="إيقاف معاش جديد"
            description="إنشاء قرار إيقاف صرف معاش لملف تقاعد محدد"
            size="full"
            isLoading={form.formState.isSubmitting}
            isDirty={isDirty}
            onSubmit={handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تأكيد الإيقاف"
            className="border-t-4 border-t-destructive"
        >
            <div className="space-y-4 text-start">
                {/* ── اختيار الملف ── */}
                <FormSection title="اختيار ملف التقاعد" icon={FileText}>
                    {selectedFile ? (
                        <div className="flex items-center justify-between p-3 bg-destructive/5 border border-destructive rounded-xl">
                            <div className="flex items-center gap-3">
                                <div className="bg-destructive/10 p-2 rounded-lg">
                                    <FileText className="w-4 h-4 text-destructive" />
                                </div>
                                <div>
                                <Text variant="body" className="font-bold">{selectedFile.fileNumber}</Text>
                                <Text variant="caption">{selectedFile.retireeFullName} — {selectedFile.rankName}</Text>
                                </div>
                            </div>
                            <Button
                                type="button"
                                variant="link"
                                size="sm"
                                className="h-auto p-0 text-destructive"
                                onClick={() => { setSelectedFile(null); setValue("fileID", 0); }}
                            >
                                تغيير
                            </Button>
                        </div>
                    ) : (
                        <div className="space-y-2">
                            <div className="relative">
                                <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                                <Input
                                    placeholder="ابحث برقم الملف أو اسم المتقاعد..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="pr-10"
                                />
                            </div>
                            {searchResults.length > 0 && (
                                <div className="border border-border rounded-xl max-h-48 overflow-y-auto">
                                    {searchResults.map((file) => (
                                        <Button
                                            key={file.fileID}
                                            type="button"
                                            variant="ghost"
                                            className="w-full justify-start gap-3 p-3 h-auto border-b border-border last:border-0 text-start"
                                            onClick={() => handleFileSelect(file)}
                                        >
                                            <FileText className="w-4 h-4 text-muted-foreground shrink-0" />
                                            <div className="min-w-0 text-start">
                                                <Text variant="body" className="font-bold truncate">{file.fileNumber}</Text>
                                                <Text variant="caption" className="truncate">{file.retireeFullName} — {file.rankName}</Text>
                                            </div>
                                            <span className="mr-auto text-xs text-muted-foreground">{file.statusName}</span>
                                        </Button>
                                    ))}
                                </div>
                            )}
                            {searchTerm.length >= 2 && searchResults.length === 0 && !isSearching && (
                                <Text variant="description" className="text-center py-4">لا توجد نتائج مطابقة</Text>
                            )}
                        </div>
                    )}
                    {errors.fileID && <Text variant="body-sm" className="text-destructive mt-1">{errors.fileID.message}</Text>}
                </FormSection>

                {/* ── تفاصيل الإيقاف ── */}
                <FormSection title="تفاصيل قرار الإيقاف" icon={Ban}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="سبب الإيقاف القانوني" required error={errors.suspensionReason?.message}>
                            <Textarea
                                {...register("suspensionReason")}
                                placeholder="يرجى توضيح السبب التفصيلي لإيقاف صرف المعاش..."
                                rows={3}
                                className="leading-relaxed"
                            />
                        </FormField>

                        <FormField label="مصدر الإيقاف (الجهة / الأمر)" required error={errors.suspensionSource?.message}>
                            <Input
                                {...register("suspensionSource")}
                                placeholder="مثال: مدير المعاشات.."
                                icon={Landmark}
                            />
                        </FormField>
                    </div>
                </FormSection>

          
            </div>
        </BaseFormDialog>
    );
}
