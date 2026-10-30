<<<<<<< HEAD
import { AlertTriangle, CheckCircle2, FileSpreadsheet, Upload, X } from "lucide-react";
import { useCallback, useRef, useState } from "react";
import { toast } from "sonner";

import { type ImportVoucherResultDto,vouchersApi } from "@/api/vouchers";
import { Badge, Button, Dialog, DialogContent, DialogHeader, DialogTitle, Text } from "@/components/ui";
=======
import { useCallback, useRef, useState } from "react";
import { Upload, FileSpreadsheet, CheckCircle2, AlertTriangle, X } from "lucide-react";

import { vouchersApi, type ImportVoucherResultDto } from "@/api/vouchers";
import { Badge, Button, Dialog, DialogContent, DialogHeader, DialogTitle, Text } from "@/components/ui";
import { toast } from "sonner";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onImported?: () => void;
}

type ImportStep = "select" | "importing" | "result";

export function VoucherImportDialog({ open, onOpenChange, onImported }: Props) {
  const [step, setStep] = useState<ImportStep>("select");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [result, setResult] = useState<ImportVoucherResultDto | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const reset = useCallback(() => {
    setStep("select");
    setSelectedFile(null);
    setResult(null);
  }, []);

  const handleClose = useCallback(() => {
    onOpenChange(false);
    // delay reset to avoid animation issues
    setTimeout(reset, 200);
  }, [onOpenChange, reset]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setSelectedFile(file);
    }
  };

  const handleImport = async () => {
    if (!selectedFile) return;

    setStep("importing");
    try {
      const response = await vouchersApi.import(selectedFile);
      if (response.isSuccess) {
        setResult(response.data);
        setStep("result");
        toast.success(response.message || "تم الاستيراد بنجاح");
        onImported?.();
      } else {
        toast.error(response.message || "فشل في الاستيراد");
        setStep("select");
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || "خطأ في رفع الملف");
      setStep("select");
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <div className="flex items-center justify-between">
            <DialogTitle className="flex items-center gap-2">
              <FileSpreadsheet className="w-5 h-5" />
              استيراد سندات من Excel
            </DialogTitle>
            <Button variant="ghost" size="icon" onClick={handleClose}>
              <X className="w-4 h-4" />
            </Button>
          </div>
        </DialogHeader>

        {/* Step 1: Select file */}
        {step === "select" && (
          <div className="space-y-4">
            <div className="text-sm text-muted-foreground">
              <Text variant="body">يقبل ملف Excel (.xlsx) بالتنسيق التالي:</Text>
              <ul className="mt-2 space-y-1 list-disc list-inside text-xs">
                <li>العمود A: الرقم (سيُحفظ كمرجع)</li>
                <li>العمود B: التاريخ</li>
                <li>العمود C: المبلغ</li>
                <li>العمود D: جهة الاستلام (المستفيد)</li>
                <li>العمود E: جهة الأمر</li>
                <li>العمود F: البيان</li>
              </ul>
            </div>

            <input
              ref={fileInputRef}
              type="file"
              accept=".xlsx,.xls"
              onChange={handleFileChange}
              className="hidden"
            />

            <Button
              type="button"
              variant="outline"
              className="w-full border-2 border-dashed p-8 h-auto"
              onClick={() => fileInputRef.current?.click()}
            >
              <Upload className="w-8 h-8 mx-auto mb-2 text-muted-foreground" />
              {selectedFile ? (
                <div>
                  <Text variant="label">{selectedFile.name}</Text>
                  <Text variant="caption" className="mt-1">
                    {(selectedFile.size / 1024).toFixed(1)} KB
                  </Text>
                </div>
              ) : (
                <div>
                  <Text variant="label">اضغط لاختيار ملف</Text>
                  <Text variant="caption" className="mt-1">أو اسحب وأفلت الملف هنا</Text>
                </div>
              )}
            </Button>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={handleClose}>إلغاء</Button>
              <Button onClick={handleImport} disabled={!selectedFile}>
                <Upload className="w-4 h-4 me-1" />
                استيراد
              </Button>
            </div>
          </div>
        )}

        {/* Step 2: Importing */}
        {step === "importing" && (
          <div className="py-8 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full mx-auto mb-4" />
            <Text variant="description">جاري استيراد السندات...</Text>
          </div>
        )}

        {/* Step 3: Result */}
        {step === "result" && result && (
          <div className="space-y-4">
            {/* Summary */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 text-center">
              <div className="p-3 bg-blue-50 rounded-lg">
                <Text variant="body" className="font-bold text-blue-700">{result.totalRows}</Text>
                <Text variant="caption">إجمالي الصفوف</Text>
              </div>
              <div className="p-3 bg-green-50 rounded-lg">
                <Text variant="body" className="font-bold text-green-700">{result.successCount}</Text>
                <Text variant="caption">تم الاستيراد</Text>
              </div>
              <div className="p-3 bg-orange-50 rounded-lg">
                <Text variant="body" className="font-bold text-orange-700">{result.skipCount}</Text>
                <Text variant="caption">تم التخطي</Text>
              </div>
            </div>

            {/* Errors list */}
            {result.errors.length > 0 && (
              <div className="max-h-48 overflow-y-auto border rounded-lg">
                <div className="p-2 bg-muted/50 text-xs font-medium flex items-center gap-1">
                  <AlertTriangle className="w-3 h-3" />
                  تفاصيل الأخطاء
                </div>
                {result.errors.map((err, i) => (
                  <div key={i} className="px-3 py-1.5 text-xs border-t flex items-center gap-2">
                    <Badge variant="outline" className="shrink-0">صف {err.rowNumber}</Badge>
                    <span>{err.message}</span>
                  </div>
                ))}
              </div>
            )}

            {/* Success message */}
            {result.skipCount === 0 && (
              <div className="flex items-center gap-2 p-3 bg-green-50 text-green-700 rounded-lg text-sm">
                <CheckCircle2 className="w-4 h-4" />
                تم استيراد جميع السندات بنجاح
              </div>
            )}

            <div className="flex justify-end">
              <Button onClick={handleClose}>إغلاق</Button>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
