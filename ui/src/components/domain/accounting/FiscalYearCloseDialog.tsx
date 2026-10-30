import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
<<<<<<< HEAD
import { AlertTriangle, CheckCircle2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

import { accountsApi, closingApi, fiscalYearsApi,periodsApi } from "@/api/accounting";
import { Button, Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, Text } from "@/components/ui";
import type { ChartOfAccountDto, FiscalPeriodDto,FiscalYearSummaryDto } from "@/types/accounting";
=======
import { useState } from "react";
import { toast } from "sonner";

import { periodsApi, closingApi, accountsApi, fiscalYearsApi } from "@/api/accounting";
import type { FiscalYearSummaryDto, ChartOfAccountDto, FiscalPeriodDto } from "@/types/accounting";
import { AlertTriangle, CheckCircle2 } from "lucide-react";
import { Button, Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, Text } from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface FiscalYearCloseDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function FiscalYearCloseDialog({ open, onOpenChange }: FiscalYearCloseDialogProps) {
  const queryClient = useQueryClient();
  const [step, setStep] = useState<"close" | "opening">("close");
  const [targetAccountID, setTargetAccountID] = useState<number | null>(null);
  const [selectedFiscalYearID, setSelectedFiscalYearID] = useState<number | null>(null);
  const [targetFiscalYearID, setTargetFiscalYearID] = useState<number | null>(null);
  const [result, setResult] = useState<{ entryNumber: string; netResult: number; resultType: string } | null>(null);

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
    enabled: open,
  });

  const selectedYear = fiscalYears?.find((y: FiscalYearSummaryDto) => y.fiscalYearID === selectedFiscalYearID);

  const { data: periods } = useQuery({
    queryKey: ["periods", selectedFiscalYearID],
    queryFn: () => periodsApi.getAll(selectedFiscalYearID!),
    enabled: open && !!selectedFiscalYearID,
  });

  const { data: accounts } = useQuery({
    queryKey: ["accounts", "all"],
    queryFn: () => accountsApi.getAll(),
    enabled: open,
  });

  // Default: surplus/deficit account (3400 or similar)
  const surplusAccount = accounts?.find((a: ChartOfAccountDto) => a.accountCode.startsWith("340"));

  const closeMutation = useMutation({
    mutationFn: () => closingApi.closeYear({
      fiscalYearID: selectedFiscalYearID!,
      targetAccountID: targetAccountID ?? surplusAccount?.accountID ?? 0,
    }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      setResult({ entryNumber: data.entryNumber, netResult: data.netResult, resultType: data.resultType });
      toast.success("تم إقفال السنة المالية بنجاح");
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.message || err?.message || "فشل في إقفال السنة";
      toast.error(msg);
    },
  });

  const openingMutation = useMutation({
    mutationFn: () => closingApi.createOpeningBalances({
      newFiscalYearID: targetFiscalYearID!,
      sourceFiscalYearID: selectedFiscalYearID!,
    }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      toast.success(`تم ترحيل الأرصدة الافتتاحية: ${data.accountsCarriedForward} حساب`);
      onOpenChange(false);
      setResult(null);
      setStep("close");
      setSelectedFiscalYearID(null);
      setTargetFiscalYearID(null);
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.message || err?.message || "فشل في إنشاء الأرصدة الافتتاحية";
      toast.error(msg);
    },
  });

  const handleClose = () => {
    if (step === "close") {
      closeMutation.mutate();
    } else {
      openingMutation.mutate();
    }
  };

  const reset = () => {
    setStep("close");
    setResult(null);
    setSelectedFiscalYearID(null);
    setTargetFiscalYearID(null);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={reset}>
      <DialogContent className="sm:max-w-[500px]" dir="rtl">
        <DialogHeader>
          <DialogTitle>{step === "close" ? "إقفال السنة المالية" : "إنشاء الأرصدة الافتتاحية"}</DialogTitle>
        </DialogHeader>

        {!result ? (
          <div className="space-y-4 py-2">
            {/* Step indicator */}
            <div className="flex items-center gap-2">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${step === "close" ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"}`}>1</div>
              <div className="h-px flex-1 bg-border" />
              <div className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${step === "opening" ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"}`}>2</div>
            </div>

            {step === "close" ? (
              <>
                {/* Year selector */}
                <div className="space-y-1.5">
<<<<<<< HEAD
                  <label className="label-field">السنة المالية المراد إقفالها</label>
=======
                  <label className="text-sm font-medium">السنة المالية المراد إقفالها</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                  <Select value={String(selectedFiscalYearID ?? "")} onValueChange={(v) => setSelectedFiscalYearID(Number(v))}>
                    <SelectTrigger>
                      <SelectValue placeholder="اختر السنة المالية" />
                    </SelectTrigger>
                    <SelectContent>
                      {fiscalYears?.filter((y: FiscalYearSummaryDto) => !y.isClosed).map((fy: FiscalYearSummaryDto) => (
                        <SelectItem key={fy.fiscalYearID} value={String(fy.fiscalYearID)}>
                          {fy.yearName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {selectedFiscalYearID && (
                  <>
                    <div className="flex items-start gap-3 p-3 bg-amber-50 rounded-xl">
                      <AlertTriangle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
                      <div className="text-sm text-amber-800">
                        <p className="font-medium">إقفال السنة {selectedYear?.yearName}</p>
                        <p className="mt-1">سيتم إقفال حسابات الإيرادات والمصروفات في حساب الأرباح المحتجزة. هذا الإجراء لا يمكن التراجع عنه.</p>
                      </div>
                    </div>

                    <div className="space-y-1.5">
<<<<<<< HEAD
                      <label className="label-field">حساب الأرباح المحتجزة (الفائض/العجز)</label>
=======
                      <label className="text-sm font-medium">حساب الأرباح المحتجزة (الفائض/العجز)</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                      <Select value={String(targetAccountID ?? surplusAccount?.accountID ?? "")} onValueChange={(v) => setTargetAccountID(Number(v))}>
                        <SelectTrigger>
                          <SelectValue placeholder="اختر الحساب" />
                        </SelectTrigger>
                        <SelectContent>
                          {accounts?.filter((a: ChartOfAccountDto) => a.isPostable).map((acc: ChartOfAccountDto) => (
                            <SelectItem key={acc.accountID} value={String(acc.accountID)}>
                              {acc.accountCode} - {acc.accountName}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="text-sm text-muted-foreground bg-muted/30 p-3 rounded-xl">
                      <Text variant="body">عدد الفترات: {periods?.length ?? 0}</Text>
                      <Text variant="body">الفترات المغلقة: {periods?.filter((p: FiscalPeriodDto) => p.isClosed).length ?? 0}</Text>
                      <Text variant="body">الفترات المفتوحة: {periods?.filter((p: FiscalPeriodDto) => !p.isClosed).length ?? 0}</Text>
                    </div>
                  </>
                )}
              </>
            ) : (
              <>
                <div className="flex items-start gap-3 p-3 bg-blue-50 rounded-xl">
                  <CheckCircle2 className="w-5 h-5 text-blue-600 shrink-0 mt-0.5" />
                  <div className="text-sm text-blue-800">
                    <p className="font-medium">إنشاء الأرصدة الافتتاحية</p>
                    <p className="mt-1">سيتم ترحيل أرصدة حسابات الميزانية العمومية إلى السنة الجديدة.</p>
                  </div>
                </div>

                <div className="space-y-1.5">
<<<<<<< HEAD
                  <label className="label-field">السنة المالية المستهدفة</label>
=======
                  <label className="text-sm font-medium">السنة المالية المستهدفة</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                  <Select value={targetFiscalYearID != null ? String(targetFiscalYearID) : undefined} onValueChange={(v) => setTargetFiscalYearID(Number(v))}>
                    <SelectTrigger>
                      <SelectValue placeholder="اختر السنة المالية" />
                    </SelectTrigger>
                    <SelectContent>
                      {fiscalYears?.map((fy: FiscalYearSummaryDto) => (
                        <SelectItem key={fy.fiscalYearID} value={String(fy.fiscalYearID)}>
                          {fy.yearName} {fy.isClosed ? "(مغلقة)" : ""}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </>
            )}

            {/* Navigation buttons */}
            <div className="flex gap-2 pt-2">
              {step === "opening" && (
                <Button variant="outline" onClick={() => setStep("close")}>الرجوع</Button>
              )}
            </div>

            <DialogFooter className="gap-2">
              <Button variant="outline" onClick={reset}>إلغاء</Button>
              {step === "close" ? (
                <Button onClick={handleClose} disabled={!selectedFiscalYearID || closeMutation.isPending} className="bg-amber-600 hover:bg-amber-700">
                  {closeMutation.isPending ? "جاري الإقفال..." : "إقفال السنة"}
                </Button>
              ) : (
                <Button onClick={handleClose} disabled={!targetFiscalYearID || openingMutation.isPending}>
                  {openingMutation.isPending ? "جاري..." : "إنشاء الأرصدة"}
                </Button>
              )}
            </DialogFooter>
          </div>
        ) : (
          /* Result display */
          <div className="space-y-4 py-4">
            <div className="flex flex-col items-center gap-3">
              <div className="w-16 h-16 rounded-full bg-green-100 flex items-center justify-center">
                <CheckCircle2 className="w-8 h-8 text-green-600" />
              </div>
              <Text variant="body" className="font-medium">تم إقفال السنة المالية</Text>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 bg-muted/30 p-4 rounded-xl">
              <div>
                <span className="text-xs text-muted-foreground">رقم قيد الإقفال</span>
                <p className="font-medium font-mono" dir="ltr">{result.entryNumber}</p>
              </div>
              <div>
                <span className="text-xs text-muted-foreground">صافي النتيجة</span>
                <p className={`font-medium ${result.resultType === "فائض" ? "text-green-600" : "text-red-600"}`}>
                  {result.resultType}: {result.netResult.toLocaleString()} د.ع
                </p>
              </div>
            </div>
            <div className="flex justify-center">
              <Button onClick={() => { setStep("opening"); setResult(null); }}>
                إنشاء الأرصدة الافتتاحية
              </Button>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={reset}>إغلاق</Button>
            </DialogFooter>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
