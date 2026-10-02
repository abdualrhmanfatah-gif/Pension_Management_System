import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertCircle, CheckCircle2, Plus, Trash2, Wrench } from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";

import { accountsApi, closingApi,fiscalYearsApi } from "@/api/accounting";
import { FixedAssetFormDialog } from "@/components/domain/assets/FixedAssetFormDialog";
import {
  Button,
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Input,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Text,
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { normalizeApiError } from "@/lib/errorUtils";
import type { ChartOfAccountDto, FiscalYearSummaryDto,InitialOpeningBalanceLineDto } from "@/types/accounting";

interface InitialOpeningBalanceDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function InitialOpeningBalanceDialog({ open, onOpenChange }: InitialOpeningBalanceDialogProps) {
  const queryClient = useQueryClient();
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);
  const [asOfDate, setAsOfDate] = useState(new Date().toISOString().split("T")[0]);
  const [lines, setLines] = useState<InitialOpeningBalanceLineDto[]>([]);
  const [result, setResult] = useState<{ accountsCarriedForward: number } | null>(null);
  const [showAssetForm, setShowAssetForm] = useState(false);

  const { data: fiscalYears } = useQuery({
    queryKey: queryKeys.accounting.fiscalYears,
    queryFn: () => fiscalYearsApi.getAll(),
    enabled: open,
  });

  const { data: allAccounts } = useQuery({
    queryKey: queryKeys.accounting.accounts.postable,
    queryFn: () => accountsApi.getPostable(),
    enabled: open,
  });

  const addLine = () => {
    setLines([...lines, {
      accountID: 0,
      accountCode: "",
      accountName: "",
      accountNature: "Debit",
      balance: 0,
    }]);
  };

  const addLineByCategory = (categoryPrefix: string) => {
    // Find first postable account starting with the category prefix
    const account = allAccounts?.find((a: ChartOfAccountDto) => a.accountCode.startsWith(categoryPrefix) && a.isPostable);
    if (!account) {
      toast.error(`لا توجد حسابات متاحة في هذا التصنيف`);
      return;
    }
    setLines([...lines, {
      accountID: account.accountID,
      accountCode: account.accountCode,
      accountName: account.accountName,
      accountNature: account.accountNature,
      balance: 0,
    }]);
  };

  const removeLine = (index: number) => {
    setLines(lines.filter((_, i) => i !== index));
  };

  const updateLine = (index: number, field: keyof InitialOpeningBalanceLineDto, value: string | number) => {
    const updated = [...lines];
    if (field === "accountID") {
      const account = allAccounts?.find((a: ChartOfAccountDto) => a.accountID === Number(value));
      if (account) {
        updated[index] = {
          ...updated[index],
          accountID: account.accountID,
          accountCode: account.accountCode,
          accountName: account.accountName,
          accountNature: account.accountNature,
        };
      }
    } else if (field === "balance") {
      updated[index] = { ...updated[index], balance: Number(value) };
    }
    setLines(updated);
  };

  const { totalDebit, totalCredit, isBalanced } = useMemo(() => {
    let debit = 0;
    let credit = 0;
    for (const line of lines) {
      if (line.accountID === 0 || line.balance === 0) continue;
      if (line.accountNature === "Debit") {
        if (line.balance > 0) debit += line.balance;
        else credit += Math.abs(line.balance);
      } else {
        if (line.balance > 0) credit += line.balance;
        else debit += Math.abs(line.balance);
      }
    }
    return { totalDebit: debit, totalCredit: credit, isBalanced: Math.abs(debit - credit) < 0.01 };
  }, [lines]);

  const mutation = useMutation({
    mutationFn: () => closingApi.createInitialOpeningBalances({
      fiscalYearID: fiscalYearID!,
      asOfDate,
      description: "إثبات الأرصدة الافتتاحية الأولية",
      lines: lines.filter(l => l.accountID > 0 && l.balance !== 0),
    }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.accounting.all });
      setResult({ accountsCarriedForward: data.accountsCarriedForward });
      toast.success(`تم إدخال الأرصدة الافتتاحية: ${data.accountsCarriedForward} حساب`);
    },
    onError: (err: Error) => {
      const { message } = normalizeApiError(err);
      toast.error(message || "فشل في إدخال الأرصدة الافتتاحية");
    },
  });

  const reset = () => {
    setFiscalYearID(null);
    setAsOfDate(new Date().toISOString().split("T")[0]);
    setLines([]);
    setResult(null);
    onOpenChange(false);
  };

  const handleAssetCreated = (assetData: { assetAccountID: number; acquiredCost: number; assetName: string }) => {
    // Find the asset account to add to opening balance
    const account = allAccounts?.find((a: ChartOfAccountDto) => a.accountID === assetData.assetAccountID);
    if (account && assetData.acquiredCost > 0) {
      setLines([...lines, {
        accountID: account.accountID,
        accountCode: account.accountCode,
        accountName: `${account.accountName} - ${assetData.assetName}`,
        accountNature: account.accountNature,
        balance: assetData.acquiredCost,
      }]);
      toast.success(`تمت إضافة الأصل "${assetData.assetName}" بقيمة ${assetData.acquiredCost.toLocaleString()} إلى الأرصدة`);
    }
  };

  const usedAccountIds = new Set(lines.map(l => l.accountID).filter(id => id > 0));

  return (
    <Dialog open={open} onOpenChange={reset}>
      <DialogContent className="sm:max-w-[700px] max-h-[85vh] overflow-y-auto" dir="rtl">
        <DialogHeader>
          <DialogTitle>إدخال الأرصدة الافتتاحية الأولية</DialogTitle>
        </DialogHeader>

        {result ? (
          <div className="space-y-4 py-4">
            <div className="flex flex-col items-center gap-3">
              <div className="w-16 h-16 rounded-full bg-green-100 flex items-center justify-center">
                <CheckCircle2 className="w-8 h-8 text-green-600" />
              </div>
              <Text variant="body" className="font-medium">تم إدخال الأرصدة بنجاح</Text>
            </div>
            <div className="grid grid-cols-1 gap-3 bg-muted/30 p-4 rounded-xl">
              <div>
                <span className="text-xs text-muted-foreground">عدد الحسابات</span>
                <p className="font-medium">{result.accountsCarriedForward}</p>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={reset}>إغلاق</Button>
            </DialogFooter>
          </div>
        ) : (
          <div className="space-y-4">
            {/* Year and Date */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <label className="label-field">السنة المالية</label>
                <Select value={String(fiscalYearID ?? "")} onValueChange={(v) => setFiscalYearID(Number(v))}>
                  <SelectTrigger>
                    <SelectValue placeholder="اختر السنة" />
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
              <div className="space-y-1.5">
                <label className="label-field">التاريخ</label>
                <Input type="date" value={asOfDate} onChange={(e) => setAsOfDate(e.target.value)} />
              </div>
            </div>

            {/* Warning */}
            <div className="flex items-start gap-3 p-3 bg-amber-50 rounded-xl">
              <AlertCircle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
              <div className="text-sm text-amber-800">
                <p className="font-medium">تنبيه</p>
                <p className="mt-1">يُستخدم هذا الإجراء فقط عند بدء تشغيل النظام لأول مرة. الأرصدة المُدخلة ستنشئ قيداً محاسبياً مرحلاً.</p>
              </div>
            </div>

            {/* Lines */}
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="label-field">الحسابات وأرصدتها</label>
                <div className="flex gap-2">
                  <Button type="button" variant="outline" size="sm" onClick={() => setShowAssetForm(true)}>
                    <Wrench className="w-4 h-4 ml-1" /> إضافة أصل جديد
                  </Button>
                  <Button type="button" variant="outline" size="sm" onClick={() => addLineByCategory("13")}>
                    <Plus className="w-4 h-4 ml-1" /> مخزون
                  </Button>
                  <Button type="button" variant="outline" size="sm" onClick={addLine}>
                    <Plus className="w-4 h-4 ml-1" /> حساب آخر
                  </Button>
                </div>
              </div>

              {lines.length === 0 && (
                <div className="text-center py-8 text-muted-foreground text-sm">
                  لا توجد حسابات. اضغط "إضافة حساب" للبدء.
                </div>
              )}

              {lines.map((line, index) => (
                <div key={index} className="grid grid-cols-[1fr_120px_40px] gap-2 items-end p-2 bg-muted/30 rounded-lg">
                  <div className="space-y-1">
                    {index === 0 && <label className="label-field">الحساب</label>}
                    <Select
                      value={String(line.accountID || "")}
                      onValueChange={(v) => updateLine(index, "accountID", Number(v))}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="اختر الحساب" />
                      </SelectTrigger>
                      <SelectContent>
                        {allAccounts?.filter((a: ChartOfAccountDto) => !usedAccountIds.has(a.accountID) || a.accountID === line.accountID).map((acc: ChartOfAccountDto) => (
                          <SelectItem key={acc.accountID} value={String(acc.accountID)}>
                            {acc.accountCode} - {acc.accountName}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-1">
                    {index === 0 && <label className="label-field">الرصيد</label>}
                    <Input
                      type="number"
                      step="0.01"
                      placeholder="0"
                      value={line.balance || ""}
                      onChange={(e) => updateLine(index, "balance", parseFloat(e.target.value) || 0)}
                      className="font-mono"
                    />
                  </div>
                  <div className="space-y-1">
                    {index === 0 && <label className="label-field">&nbsp;</label>}
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="text-destructive"
                      onClick={() => removeLine(index)}
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>

            {/* Summary */}
            {lines.length > 0 && (
              <div className={`flex items-center justify-between p-3 rounded-xl ${isBalanced ? "bg-green-50" : "bg-red-50"}`}>
                <div className="flex gap-4 text-sm">
                  <span>إجمالي المدين: <strong className="font-mono">{totalDebit.toLocaleString()}</strong></span>
                  <span>إجمالي الدائن: <strong className="font-mono">{totalCredit.toLocaleString()}</strong></span>
                </div>
                <span className={`text-sm font-medium ${isBalanced ? "text-green-600" : "text-red-600"}`}>
                  {isBalanced ? "متوازن" : "غير متوازن"}
                </span>
              </div>
            )}

            <DialogFooter className="gap-2">
              <Button variant="outline" onClick={reset}>إلغاء</Button>
              <Button
                onClick={() => mutation.mutate()}
                disabled={!fiscalYearID || lines.length === 0 || !isBalanced || mutation.isPending}
              >
                {mutation.isPending ? "جاري الترحيل..." : "ترحيل الأرصدة الافتتاحية"}
              </Button>
            </DialogFooter>
          </div>
        )}
      </DialogContent>
      <FixedAssetFormDialog
        open={showAssetForm}
        onOpenChange={setShowAssetForm}
        mode="create"
        skipJournalEntry
        onSuccess={(asset) => {
          if (asset) {
            handleAssetCreated({
              assetAccountID: asset.assetAccountID,
              acquiredCost: asset.acquiredCost,
              assetName: asset.assetName,
            });
          }
        }}
      />
    </Dialog>
  );
}
