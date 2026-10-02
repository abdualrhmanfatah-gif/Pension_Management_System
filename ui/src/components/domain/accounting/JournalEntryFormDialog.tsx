import { useQuery } from "@tanstack/react-query";
import { Pencil, PlusCircle, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import { accountsApi, fiscalYearsApi,journalEntriesApi, journalEntryTypesApi, periodsApi } from "@/api/accounting";
import {
  BaseFormDialog,
  Button,
  DateInput,
  FormField,
  FormSection,
  Input,
  NativeSelect,
  Text,
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
import { formatCurrency } from "@/lib/utils";
import type { ChartOfAccountDto, CreateJournalEntryLineDto, FiscalPeriodDto, FiscalYearSummaryDto, JournalEntryTypeDto } from "@/types/accounting";

// ─── Types ────────────────────────────────────────
interface LineEntry {
  key: string;
  accountID: number;
  accountName: string;
  accountCode: string;
  debit: number;
  credit: number;
  description: string;
}

let lineKeyCounter = 0;
function newLineKey() {
  return `line_${++lineKeyCounter}`;
}

function createEmptyLine(): LineEntry {
  return { key: newLineKey(), accountID: 0, accountName: "", accountCode: "", debit: 0, credit: 0, description: "" };
}

// ─── Component ────────────────────────────────────
interface JournalEntryFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function JournalEntryFormDialog({ open, onOpenChange }: JournalEntryFormDialogProps) {
  // ─── State (lines must be state — dynamic rows) ──
  const [entryDate, setEntryDate] = useState(new Date().toISOString().split("T")[0]);
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);
  const [periodID, setPeriodID] = useState<number>(0);
  const [journalEntryTypeID, setJournalEntryTypeID] = useState<number>(0);
  const [description, setDescription] = useState("");
  const [lines, setLines] = useState<LineEntry[]>([createEmptyLine(), createEmptyLine()]);

  // ─── Queries ─────────────────────────────────────
  const { data: fiscalYears } = useQuery({
    queryKey: queryKeys.accounting.fiscalYears,
    queryFn: () => fiscalYearsApi.getAll(),
    enabled: open,
  });

  const { data: periods } = useQuery({
    queryKey: queryKeys.accounting.periods.byYear(fiscalYearID!),
    queryFn: () => periodsApi.getAll(fiscalYearID!),
    enabled: open && fiscalYearID != null,
  });

  const { data: activePeriod } = useQuery({
    queryKey: queryKeys.accounting.periods.active(fiscalYearID!),
    queryFn: () => periodsApi.getActive(fiscalYearID!),
    enabled: open && fiscalYearID != null,
  });

  const { data: postableAccounts } = useQuery({
    queryKey: queryKeys.accounting.accounts.postable,
    queryFn: () => accountsApi.getPostable(),
    enabled: open,
  });

  const { data: journalEntryTypes } = useQuery({
    queryKey: queryKeys.accounting.journalEntryTypes,
    queryFn: () => journalEntryTypesApi.getAll(),
    enabled: open,
  });

  // ─── Reset on open ───────────────────────────────
  useEffect(() => {
    if (open) {
      const today = new Date().toISOString().split("T")[0];
      setEntryDate(today);
      setPeriodID(0);
      setFiscalYearID(null);
      setJournalEntryTypeID(0);
      setDescription("");
      setLines([createEmptyLine(), createEmptyLine()]);
    }
  }, [open]);

  // ─── Auto-match fiscal year & period from date ───
  useEffect(() => {
    if (!open || !entryDate || !fiscalYears || fiscalYears.length === 0) return;

    const date = new Date(entryDate);

    // 1) Find the fiscal year that contains this date
    const matchedYear = fiscalYears.find(
      (y: FiscalYearSummaryDto) => date >= new Date(y.startDate) && date <= new Date(y.endDate)
    );

    if (matchedYear && matchedYear.fiscalYearID !== fiscalYearID) {
      setFiscalYearID(matchedYear.fiscalYearID);
      setPeriodID(0); // reset period so the next effect picks it
      return; // wait for periods to load before matching period
    }

    // 2) Find the period that contains this date
    if (matchedYear && periods && periods.length > 0 && periodID === 0) {
      const matchedPeriod = periods.find(
        (p: FiscalPeriodDto) => date >= new Date(p.startDate) && date <= new Date(p.endDate)
      );
      if (matchedPeriod) {
        setPeriodID(matchedPeriod.periodID);
      } else {
        // Date falls inside the year but outside all periods — pick active or first
        setPeriodID(activePeriod?.periodID ?? periods[0].periodID);
      }
    }
  }, [open, entryDate, fiscalYears, periods, activePeriod, fiscalYearID, periodID]);

  // ─── Account options ─────────────────────────────
  const accountOptions = useMemo(
    () => postableAccounts?.map((a: ChartOfAccountDto) => ({ value: String(a.accountID), label: `${a.accountCode} - ${a.accountName}` })) || [],
    [postableAccounts]
  );

  const periodOptions = useMemo(
    () => periods?.map((p: FiscalPeriodDto) => ({ value: String(p.periodID), label: p.periodName })) || [],
    [periods]
  );

  const journalEntryTypeOptions = useMemo(
    () => journalEntryTypes?.map((t: JournalEntryTypeDto) => ({ value: String(t.journalEntryTypeID), label: `${t.code} - ${t.typeName}` })) || [],
    [journalEntryTypes]
  );

  // ─── Computed ────────────────────────────────────
  const totalDebit = lines.reduce((s, l) => s + (l.debit || 0), 0);
  const totalCredit = lines.reduce((s, l) => s + (l.credit || 0), 0);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.01;

  // ─── Line operations ─────────────────────────────
  const addLine = () => setLines([...lines, createEmptyLine()]);

  const removeLine = (key: string) => {
    if (lines.length <= 2) return;
    setLines(lines.filter((l) => l.key !== key));
  };

  const updateLine = (key: string, field: keyof LineEntry, value: unknown) => {
    setLines(
      lines.map((l) => {
        if (l.key !== key) return l;
        const updated = { ...l, [field]: value };
        if (field === "debit" && Number(value) > 0) updated.credit = 0;
        if (field === "credit" && Number(value) > 0) updated.debit = 0;
        return updated;
      })
    );
  };

  const selectAccount = (key: string, accountID: string) => {
    const id = Number(accountID);
    const acc = postableAccounts?.find((a: ChartOfAccountDto) => a.accountID === id);
    setLines(
      lines.map((l) =>
        l.key === key ? { ...l, accountID: id, accountName: acc?.accountName ?? "", accountCode: acc?.accountCode ?? "" } : l
      )
    );
  };

  // ─── Mutation ────────────────────────────────────
  const mutation = useApiMutation(
    async () => {
      const linesDto: CreateJournalEntryLineDto[] = lines.map((l) => ({
        accountID: l.accountID,
        debit: l.debit || 0,
        credit: l.credit || 0,
        description: l.description || null,
      }));
      return journalEntriesApi.create({
        entryDate,
        periodID: periodID!,
        journalEntryTypeID,
        description,
        lines: linesDto,
      });
    },
    {
      successMessage: "تم إنشاء القيد اليومي بنجاح",
      errorTitle: "خطأ في إنشاء القيد",
      invalidateKeys: [["journal-entries"]],
      onSuccess: () => {
        onOpenChange(false);
      },
    }
  );

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="إضافة قيد يومي يدوي"
      description="إنشاء قيد يومي جديد ببنود متعددة"
      size="2xl"
      isLoading={mutation.isPending}
      isDirty={!!description.trim() || lines.some((l) => l.accountID > 0)}
      onSubmit={() => mutation.mutate(undefined as void)}
      resetOnOpen
      submitLabel="حفظ القيد"
      submitIcon={<Pencil className="w-4 h-4" />}
    >
      <div className="space-y-4">
        {/* ─── Header fields ─── */}
        <FormSection title="بيانات القيد" icon={Pencil}>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <FormField label="تاريخ القيد" required>
              <DateInput value={entryDate} onChange={(val) => setEntryDate(val || "")} />
            </FormField>

            <FormField label="السنة المالية" required>
              <NativeSelect
                value={fiscalYearID || ""}
                onChange={(val) => {
                  setFiscalYearID(val ? Number(val) : null);
                  setPeriodID(0);
                }}
                options={fiscalYears?.map((y: FiscalYearSummaryDto) => ({ value: String(y.fiscalYearID), label: y.yearName })) || []}
                placeholder="اختر السنة"
              />
            </FormField>

            <FormField label="الفترة المحاسبية" required>
              <NativeSelect
                value={periodID || ""}
                onChange={(val) => setPeriodID(val ? Number(val) : 0)}
                options={periodOptions}
                placeholder="اختر الفترة"
              />
            </FormField>

            <FormField label="نوع القيد" required>
              <NativeSelect
                value={journalEntryTypeID || ""}
                onChange={(val) => setJournalEntryTypeID(val ? Number(val) : 0)}
                options={journalEntryTypeOptions}
                placeholder="اختر نوع القيد"
              />
            </FormField>
          </div>

          <FormField label="بيان القيد" required>
            <Input
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="شرح سبب القيد..."
            />
          </FormField>
        </FormSection>

        {/* ─── Lines table ─── */}
        <FormSection
          title="بنود القيد"
          icon={null}
          badge={lines.length}
        >
          <div className="flex justify-end mb-3">
            <Button
              type="button"
              variant="link"
              size="sm"
              className="h-auto p-0 gap-1"
              onClick={addLine}
            >
              <PlusCircle className="w-3.5 h-3.5" />
              إضافة بند
            </Button>
          </div>

          <div className="border rounded-xl overflow-hidden">
            <table className="w-full text-sm" role="table" aria-label="إدخال القيود اليومية">
              <thead className="bg-muted/50 sticky top-0 z-10">
                <tr>
                  <th className="text-end p-2 w-8">#</th>
                  <th className="text-end p-2">الحساب</th>
                  <th className="text-end p-2">البيان</th>
                  <th className="text-start p-2 w-28">مدين</th>
                  <th className="text-start p-2 w-28">دائن</th>
                  <th className="p-2 w-8"></th>
                </tr>
              </thead>
              <tbody>
                {lines.map((line, idx) => (
                  <tr key={line.key} className="border-t border-border/50">
                    <td className="p-1.5 text-xs text-muted-foreground text-center">{idx + 1}</td>
                    <td className="p-1.5">
                      <NativeSelect
                        value={line.accountID || ""}
                        onChange={(val) => selectAccount(line.key, val)}
                        options={accountOptions}
                        placeholder="اختر حساب"
                        className="h-9 text-xs"
                      />
                    </td>
                    <td className="p-1.5">
                      <Input
                        className="h-9 text-xs"
                        placeholder="بيان (اختياري)"
                        value={line.description}
                        onChange={(e) => updateLine(line.key, "description", e.target.value)}
                      />
                    </td>
                    <td className="p-1.5">
                      <Input
                        type="number"
                        className="h-9 text-xs text-left font-mono ltr"
                        dir="ltr"
                        placeholder="0.00"
                        value={line.debit || ""}
                        onChange={(e) => updateLine(line.key, "debit", Number(e.target.value))}
                      />
                    </td>
                    <td className="p-1.5">
                      <Input
                        type="number"
                        className="h-9 text-xs text-left font-mono ltr"
                        dir="ltr"
                        placeholder="0.00"
                        value={line.credit || ""}
                        onChange={(e) => updateLine(line.key, "credit", Number(e.target.value))}
                      />
                    </td>
                    <td className="p-1.5">
                      {lines.length > 2 && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          className="h-7 w-7 text-destructive hover:bg-destructive/10"
                          onClick={() => removeLine(line.key)}
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-muted/30 font-medium">
                <tr>
                  <td colSpan={3} className="p-2 text-start">الإجمالي</td>
                  <td className="p-2 text-start font-mono">{formatCurrency(totalDebit)}</td>
                  <td className="p-2 text-start font-mono">{formatCurrency(totalCredit)}</td>
                  <td></td>
                </tr>
              </tfoot>
            </table>
          </div>

          {!isBalanced && lines.length >= 2 && (
            <Text variant="body-sm" className="text-destructive mt-2">
              غير متوازن — إجمالي المدين ({formatCurrency(totalDebit)}) لا يساوي إجمالي الدائن ({formatCurrency(totalCredit)})
            </Text>
          )}
        </FormSection>
      </div>
    </BaseFormDialog>
  );
}
