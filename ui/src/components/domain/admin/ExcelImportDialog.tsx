import { FileSpreadsheet, Upload, X } from "lucide-react";
<<<<<<< HEAD
import { type MouseEvent,useCallback,useRef, useState } from "react";
=======
import { useCallback,useRef, useState, type MouseEvent } from "react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

import api from "@/api/axios";
import {
    BaseFormDialog,
    Button,
    FormSection,
    Text,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";

interface ApiResponse<T> { isSuccess: boolean; data: T; message?: string; }

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onImported: (count: number) => void;
    batchId: number;
}

/**
 * Standardized ExcelImportDialog.
 * Features built-in drag-and-drop and validation logic.
 */
export function ExcelImportDialog({ open, onOpenChange, onImported, batchId }: Props) {
    const [file, setFile] = useState<File | null>(null);
    const [error, setError] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const mutation = useApiMutation(
        (formData: FormData) => api.post<ApiResponse<number>>(`/ministry-approvals/batches/${batchId}/import`, formData).then(r => r.data),
        {
            successMessage: (res: ApiResponse<number>) => `تم استيراد ${res.data} سجل بنجاح من ملف Excel`,
            onSuccessCallback: (res: ApiResponse<number>) => {
                onImported(res.data);
                onOpenChange(false);
            },
            onError: (err: unknown) => {
                const { message } = normalizeApiError(err);
                setError(message || "فشل استيراد البيانات، يرجى مراجعة صياغة الملف");
            }
        }
    );

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            const selected = e.target.files[0];
            validateAndSetFile(selected);
        }
    };

    const validateAndSetFile = (f: File) => {
        const lower = f.name.toLowerCase();
        const isValid = lower.endsWith(".xlsx") || lower.endsWith(".xls");

        if (!isValid) {
            setFile(null);
            setError("يجب اختيار ملف Excel بصيغة .xlsx أو .xls فقط للمتابعة.");
        } else {
            setFile(f);
            setError(null);
        }
    };

    const handleUpload = async () => {
        if (!file) {
            setError("يرجى اختيار ملف للمتابعة");
            return;
        }

        setError(null);
        const formData = new FormData();
        formData.append("File", file);
        mutation.mutate(formData);
    };

    const handleReset = useCallback(() => {
        setFile(null);
        setError(null);
    }, []);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="استيراد كشوفات الإحالة "
            description="رفع ملف البيانات الجماعي لمعالجة الدفعات المالية"
            size="md"
            isLoading={mutation.isPending}
            onSubmit={handleUpload}
            submitLabel="بدء الاستيراد الآن"
            resetOnOpen
            reset={handleReset}
        >
            <div className="space-y-4">
                <FormSection title="اختيار ملف البيانات" icon={FileSpreadsheet}>
                    <div
<<<<<<< HEAD
                        className={`border-2 border-dashed rounded-2xl p-10 text-center transition-all cursor-pointer relative group ${file ? "border-primary bg-primary/5" : "border-border hover:border-primary hover:bg-muted/30"
=======
                        className={`border-2 border-dashed rounded-2xl p-10 text-center transition-all cursor-pointer relative group ${file ? "border-primary bg-primary/5" : "border-border/60 hover:border-primary/50 hover:bg-muted/30"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            }`}
                        role="button"
                        tabIndex={0}
                        aria-label="ارفع ملف Excel"
                        onDragOver={(e) => e.preventDefault()}
                        onDrop={(e) => {
                            e.preventDefault();
                            if (e.dataTransfer.files && e.dataTransfer.files[0]) {
                                validateAndSetFile(e.dataTransfer.files[0]);
                            }
                        }}
                        onKeyDown={(e) => {
                            if (e.key === "Enter" || e.key === " ") {
                                e.preventDefault();
                                fileInputRef.current?.click();
                            }
                        }}
                    >
                        <input
                            key={file ? "has-file" : "no-file"}
                            ref={fileInputRef}
                            type="file"
                            accept=".xlsx, .xls"
                            className={file
                                ? "hidden"
                                : "absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
                            }
                            onChange={handleFileChange}
                        />

                        {file ? (
                            <div className="flex flex-col items-center gap-3">
                                <div className="p-4 bg-green-100 rounded-2xl">
                                    <FileSpreadsheet className="w-10 h-10 text-green-600" />
                                </div>
                                <div className="space-y-1">
                                    <Text variant="body" className="font-bold">{file.name}</Text>
                                    <Text variant="caption" className="font-mono">
                                        {(file.size / 1024).toFixed(1)} KB
                                    </Text>
                                </div>
                                <Button
                                    type="button"
                                    variant="destructive"
                                    size="sm"
                                    className="mt-2"
                                    onClick={(e: MouseEvent) => {
                                        e.stopPropagation();
                                        setFile(null);
                                    }}
                                >
                                    <X className="w-4 h-4 me-2" /> إزالة الملف الحالي
                                </Button>
                            </div>
                        ) : (
                            <div className="flex flex-col items-center gap-4">
                                <div className="p-4 bg-muted rounded-2xl group-hover:bg-primary/10 group-hover:scale-110 transition-all duration-300">
                                    <Upload className="w-10 h-10 text-muted-foreground group-hover:text-primary" />
                                </div>
                                <div className="space-y-1">
                                    <Text variant="body" className="font-bold">اضغط لاختيار ملف أو قم بالسحب والإفلات</Text>
                                    <Text variant="caption">
                                        يدعم صيغ Excel فقط (.xlsx, .xls)
                                    </Text>
                                </div>
                            </div>
                        )}
                    </div>
                </FormSection>

                {error && (
<<<<<<< HEAD
                    <div className="rounded-2xl bg-destructive/10 border border-destructive p-4 flex items-start gap-3 animate-in fade-in slide-in-from-top-2">
=======
                    <div className="rounded-2xl bg-destructive/10 border border-destructive/20 p-4 flex items-start gap-3 animate-in fade-in slide-in-from-top-2">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <div className="p-1.5 bg-destructive/10 rounded-lg">
                            <X className="w-4 h-4 text-destructive" />
                        </div>
                        <p role="alert" aria-live="polite" className="text-sm text-destructive font-medium pt-0.5">{error}</p>
                    </div>
                )}

          
            </div>
        </BaseFormDialog>
    );
}
