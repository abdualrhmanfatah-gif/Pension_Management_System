import { zodResolver } from "@hookform/resolvers/zod";
import { AlertCircle, Download,FileText, Upload } from "lucide-react";
import { useCallback,useRef, useState } from "react";
import { Controller,useForm } from "react-hook-form";
import { z } from "zod";

import { settlementApi } from "@/api/settlements";
import { Text } from "@/components/ui";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { Button } from "@/components/ui/Button";
import { FormField } from "@/components/ui/FormField";
import { DateInput,Input } from "@/components/ui/Input";
import { useApiMutation } from "@/hooks";

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved: () => void;
}

const schema = z.object({
    decisionNumber: z.string().min(1, "رقم القرار مطلوب"),
    decisionDate: z.string().min(1, "تاريخ القرار مطلوب"),
    decisionType: z.string().min(1, "نوع القرار مطلوب"),
    notes: z.string().optional().or(z.literal("")),
});

type FormValues = z.infer<typeof schema>;

export function CreateSettlementDecisionDialog({ open, onOpenChange, onSaved }: Props) {
    const decisionFileRef = useRef<HTMLInputElement>(null);
    const excelFileRef = useRef<HTMLInputElement>(null);
    const [decisionFile, setDecisionFile] = useState<File | null>(null);
    const [excelFile, setExcelFile] = useState<File | null>(null);
    const [lastImportResult, setLastImportResult] = useState<{ message: string, hasFailures: boolean } | null>(null);

    const form = useForm<FormValues>({
        resolver: zodResolver(schema),
        defaultValues: {
            decisionNumber: "",
            decisionDate: "",
            decisionType: "تسوية رتب",
            notes: ""
        }
    });

    const resetFiles = useCallback(() => {
        setDecisionFile(null);
        setExcelFile(null);
        setLastImportResult(null);
        if (decisionFileRef.current) decisionFileRef.current.value = "";
        if (excelFileRef.current) excelFileRef.current.value = "";
    }, []);

    const handleClose = useCallback(() => {
        form.reset();
        resetFiles();
        onOpenChange(false);
    }, [form, onOpenChange, resetFiles]);

    const createMutation = useApiMutation(
        async (formData: FormData) => {
            const res = await settlementApi.uploadDecision(formData);
            return res;
        },
        {
            successMessage: (data) => data.message || "تم إضافة القرار بنجاح",
            onSuccess: (data) => {
                if (data.message?.includes("فشل")) {
                    setLastImportResult({
                        message: data.message,
                        hasFailures: true
                    });
                    onSaved(); // Still refresh list for the imported items
                } else {
                    form.reset();
                    resetFiles();
                    onSaved();
                    onOpenChange(false);
                }
            }
        }
    );

    const handleCreate = useCallback(async (data: FormValues) => {
        if (!excelFile) {
            form.setError("root", { message: "يرجى اختيار كشف القوة (Excel)" });
            return;
        }

        const formData = new FormData();
        formData.append("file", excelFile);
        if (decisionFile) formData.append("decisionFile", decisionFile);
        formData.append("decisionNumber", data.decisionNumber);
        formData.append("decisionDate", data.decisionDate + "T00:00:00");
        formData.append("decisionType", data.decisionType);
        if (data.notes) formData.append("notes", data.notes);

        await createMutation.mutateAsync(formData);
    }, [createMutation, excelFile, decisionFile, form]);

    const downloadErrorReport = useCallback(async () => {
        if (!excelFile) return;
        
        try {
            const response = await settlementApi.exportFailuresForFile(excelFile);

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'import_failures_report.xlsx');
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error("Failed to download error report", error);
        }
    }, [excelFile]);

    const handleOpenChange = useCallback((isOpen: boolean) => {
        onOpenChange(isOpen);
        if (!isOpen) {
            handleClose();
        }
    }, [handleClose, onOpenChange]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={handleOpenChange}
            title="إضافة قرار تسوية جديد"
            description="أدخل بيانات قرار التسوية الصادر من الوزارة وأرفق كشف القوة المشمولة بالقرار"
            size="3xl"
            isLoading={createMutation.isPending}
            onSubmit={form.handleSubmit(handleCreate)}
            submitLabel="رفع القرار والكشف"
            submitIcon={<Upload className="w-4 h-4" />}
            cancelLabel="إلغاء"
            resetOnOpen
            reset={form.reset}
            showGlobalError
            formErrors={form.formState.errors}
        >
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mt-2">
                {lastImportResult?.hasFailures && (
                    <div className="col-span-full bg-destructive/10 border border-destructive/20 rounded-lg p-4 mb-4 flex flex-col gap-3">
                        <div className="flex items-start gap-3">
                            <AlertCircle className="w-5 h-5 text-destructive mt-0.5 shrink-0" />
                            <Text variant="label" className="text-destructive leading-relaxed">
                                {lastImportResult.message}
                            </Text>
                        </div>
                        <Button 
                            type="button" 
                            variant="outline" 
                            size="sm" 
                            className="w-fit gap-2 border-destructive/30 hover:bg-destructive/10 text-destructive"
                            onClick={downloadErrorReport}
                        >
                            <Download className="w-4 h-4" />
                            تحميل كشف الصفوف الفاشلة (Excel)
                        </Button>
                    </div>
                )}

                <FormField label="رقم القرار" required error={form.formState.errors.decisionNumber?.message}>
                    <Input
                        {...form.register("decisionNumber")}
                        placeholder="مثال: 123/2026"
                    />
                </FormField>
                <FormField label="تاريخ القرار" required error={form.formState.errors.decisionDate?.message}>
                    <Controller
                        control={form.control}
                        name="decisionDate"
                        render={({ field }) => (
                            <DateInput
                                id={field.name}
                                name={field.name}
                                value={field.value}
                                onChange={(val) => field.onChange(val || "")}
                            />
                        )}
                    />
                </FormField>

                <FormField label="نوع القرار" error={form.formState.errors.decisionType?.message}>
                    <Input
                        {...form.register("decisionType")}
                    />
                </FormField>

                <FormField label="ملاحظات (اختياري)" error={form.formState.errors.notes?.message}>
                    <Input
                        {...form.register("notes")}
                        placeholder="ملاحظات..."
                    />
                </FormField>

                <FormField label="ملف القرار (اختياري)" className="lg:col-span-2">
                    <div className="border-2 border-dashed rounded-lg p-4 text-center hover:bg-muted/50 transition-colors cursor-pointer relative h-32 flex flex-col items-center justify-center">
                        <input
                            ref={decisionFileRef}
                            type="file"
                            accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                            className="absolute inset-0 opacity-0 cursor-pointer"
                            onChange={(e) =>
                                setDecisionFile(
                                    e.target.files?.[0] || null
                                )
                            }
                        />
                        <FileText className="w-6 h-6 text-muted-foreground mx-auto mb-1" />
                        {decisionFile ? (
                            <Text variant="label" className="text-primary truncate max-w-full px-2">
                                {decisionFile.name}
                            </Text>
                        ) : (
                            <Text variant="description">
                                اختر ملف القرار (PDF، صورة، أو Word)
                            </Text>
                        )}
                    </div>
                </FormField>

                <FormField
                    label="كشف القوة المشمولة (Excel)"
                    required
                    className="lg:col-span-2"
                >
                    <div className="border-2 border-dashed rounded-lg p-4 text-center hover:bg-muted/50 transition-colors cursor-pointer relative h-32 flex flex-col items-center justify-center">
                        <input
                            ref={excelFileRef}
                            type="file"
                            accept=".xlsx,.xls"
                            className="absolute inset-0 opacity-0 cursor-pointer"
                            onChange={(e) => {
                                setExcelFile(
                                    e.target.files?.[0] || null
                                );
                            }}
                        />
                        <Upload className="w-6 h-6 text-muted-foreground mx-auto mb-1" />
                        {excelFile ? (
                            <Text variant="label" className="text-green-600 truncate max-w-full px-2">
                                {excelFile.name}
                            </Text>
                        ) : (
                            <Text variant="description">
                                اختر ملف Excel
                            </Text>
                        )}
                        <p className="text-[10px] sm:text-xs text-muted-foreground mt-1">
                            الأعمدة: م، الرقم العسكري، الرتبة الحالية، الاسم، الرتبة المعالجة
                        </p>
                    </div>
                </FormField>
            </div>
        </BaseFormDialog>
    );
}
