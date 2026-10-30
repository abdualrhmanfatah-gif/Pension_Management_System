import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { AlertTriangle,ArrowRight, FileText, Printer } from "lucide-react";
import { useCallback,useMemo } from "react";
import { useNavigate, useParams,useSearchParams } from "react-router-dom";

import {
  accountsApi,
  fiscalYearsApi,
  periodsApi,
  reportsApi,
=======
import { ArrowRight, Printer, FileText, AlertTriangle } from "lucide-react";
import { useNavigate, useSearchParams, useParams } from "react-router-dom";
import { useMemo, useCallback } from "react";

import {
  reportsApi,
  periodsApi,
  fiscalYearsApi,
  accountsApi,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
} from "@/api/accounting";
import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
import { Button } from "@/components/ui/Button";
import { PrintableReport } from "@/components/ui/print/PrintableReport";
import type { TableCell } from "@/components/ui/print/PrintableReportTable";
<<<<<<< HEAD
import { chartColors } from "@/lib/chartColors";
import { formatCurrency } from "@/lib/utils";
import type {
  AccountStatementResultDto,
  BalanceCheckResult,
  BalanceSheetDto,
  BudgetReportResultDto,
  CashFlowStatementDto,
  CostCenterReportDto,
  CumulativePnLReportDto,
  CurrentOperationsReportDto,
  DepreciationReportDto,
  GeneralLedgerDto,
  IncomeStatementDto,
  JournalRegisterDto,
  JournalRegisterItemDto,
  LiabilitiesReportDto,
  ProvisionsReportDto,
  QuarterlyReportDto,
  SubledgerReportResultDto,
  TrialBalanceItemDto,
  YearOverYearReportDto,
=======
import { formatCurrency } from "@/lib/utils";
import { chartColors } from "@/lib/chartColors";
import type {
  TrialBalanceItemDto,
  JournalRegisterItemDto,
  JournalRegisterDto,
  AccountStatementResultDto,
  GeneralLedgerDto,
  BalanceSheetDto,
  IncomeStatementDto,
  CashFlowStatementDto,
  QuarterlyReportDto,
  BudgetReportResultDto,
  CurrentOperationsReportDto,
  LiabilitiesReportDto,
  YearOverYearReportDto,
  CumulativePnLReportDto,
  CostCenterReportDto,
  ProvisionsReportDto,
  DepreciationReportDto,
  SubledgerReportResultDto,
  BalanceCheckResult,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
} from "@/types/accounting";

/* ═══════════════════════════════════════════════════════════
   Helpers
   ═══════════════════════════════════════════════════════════ */

const fc = (n: number) => formatCurrency(n);
const isNum = (n: number) => n !== 0;

/* ═══════════════════════════════════════════════════════════
   Report Registry — كل التقارير في مكان واحد
   ═══════════════════════════════════════════════════════════ */

interface ReportConfig<TData = unknown> {
  title: string;
  headers: string[];
  pageSize?: "A4" | "A3" | "A5" | "letter";
  orientation?: "portrait" | "landscape";
  buildRows: (data: TData) => TableCell[][];
  buildTotals?: (data: TData) => TableCell[] | undefined;
  buildMetadata?: (data: TData, deps: ReportDeps) => { label: string; value: string }[];
  buildBalanceCheck?: (data: TData) => BalanceCheckResult;
}

interface ReportDeps {
  yearName: string;
  periodName: string;
  accountName?: { accountCode: string; accountName: string };
  fromDate?: string;
  toDate?: string;
  date?: string;
}

const REPORT_REGISTRY: Record<string, ReportConfig<any>> = {
  "trial-balance": {
    title: "ميزان المراجعة",
    headers: ["#", "الكود", "الحساب", "افتتاحي مدين", "افتتاحي دائن", "حركة مدين", "حركة دائن", "ختامي مدين", "ختامي دائن", "الرصيد"],
    pageSize: "A3",
    orientation: "landscape",
    buildRows: (data: TrialBalanceItemDto[]) => data.map((item: TrialBalanceItemDto, i: number) => [
      { value: i + 1 },
      { value: item.accountCode, bold: true, className: "font-mono" },
      { value: item.accountName, bold: !item.isPostable, align: "start" },
      { value: isNum(item.openingDebit) ? fc(item.openingDebit) : "—" },
      { value: isNum(item.openingCredit) ? fc(item.openingCredit) : "—" },
      { value: isNum(item.movementDebit) ? fc(item.movementDebit) : "—" },
      { value: isNum(item.movementCredit) ? fc(item.movementCredit) : "—" },
      { value: isNum(item.closingDebit) ? fc(item.closingDebit) : "—" },
      { value: isNum(item.closingCredit) ? fc(item.closingCredit) : "—" },
      { value: item.balance > 0 ? fc(item.balance) : "—", bold: true, color: item.balanceType === "Debit" ? chartColors.primary : chartColors.success },
    ]),
    buildTotals: (data: TrialBalanceItemDto[]) => {
      const postable = data.filter((d) => d.isPostable);
      const sum = (key: keyof TrialBalanceItemDto) => postable.reduce((s, i) => s + ((i[key] as number) || 0), 0);
      const totalDebit = sum("totalDebit");
      const totalCredit = sum("totalCredit");
      return [
        { value: "الإجمالي", bold: true, colSpan: 2 },
        { value: fc(sum("openingDebit")), bold: true },
        { value: fc(sum("openingCredit")), bold: true },
        { value: fc(sum("movementDebit")), bold: true },
        { value: fc(sum("movementCredit")), bold: true },
        { value: fc(sum("closingDebit")), bold: true },
        { value: fc(sum("closingCredit")), bold: true },
        { value: `${fc(totalDebit)} / ${fc(totalCredit)}`, bold: true },
      ];
    },
    buildBalanceCheck: (data: TrialBalanceItemDto[]) => {
      const postable = data.filter((d) => d.isPostable);
      const sum = (key: keyof TrialBalanceItemDto) => postable.reduce((s, i) => s + ((i[key] as number) || 0), 0);
      const totalDebit = sum("totalDebit");
      const totalCredit = sum("totalCredit");
      return {
        isBalanced: Math.abs(totalDebit - totalCredit) < 0.01,
        difference: Math.abs(totalDebit - totalCredit),
        debitTotal: totalDebit,
        creditTotal: totalCredit,
      };
    },
  },

  "journal-register": {
    title: "سجل القيود اليومية",
    headers: ["التاريخ", "رقم القيد", "البيان", "المصدر", "الحالة", "بواسطة", "مدين", "دائن"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: JournalRegisterDto) => data.entries.map((entry: JournalRegisterItemDto) => [
      { value: new Date(entry.entryDate).toLocaleDateString("ar-YE") },
      { value: entry.entryNumber, className: "font-mono" },
      { value: entry.description, align: "start" },
      { value: entry.sourceModule === "Manual" ? "يدوي" : entry.sourceModule },
      { value: entry.status === 1 ? "مسودة" : entry.status === 2 ? "مرحّل" : "ملغي", color: entry.status === 2 ? chartColors.success : entry.status === 3 ? chartColors.danger : chartColors.warning, bold: true },
      { value: entry.createdByUserName },
      { value: entry.totalDebit > 0 ? fc(entry.totalDebit) : "—" },
      { value: entry.totalCredit > 0 ? fc(entry.totalCredit) : "—" },
    ]),
    buildTotals: (data) => [
      { value: "الإجمالي", bold: true, colSpan: 6 },
      { value: fc(data.grandTotalDebit), bold: true },
      { value: fc(data.grandTotalCredit), bold: true },
    ],
    buildBalanceCheck: (data) => ({
      isBalanced: Math.abs(data.grandTotalDebit - data.grandTotalCredit) < 0.01,
      difference: Math.abs(data.grandTotalDebit - data.grandTotalCredit),
      debitTotal: data.grandTotalDebit,
      creditTotal: data.grandTotalCredit,
    }),
  },

  "account-statement": {
    title: "كشف حساب",
    headers: ["التاريخ", "رقم القيد", "البيان", "مدين", "دائن", "الرصيد"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: AccountStatementResultDto) => (data.items || []).map((item) => [
      { value: new Date(item.entryDate).toLocaleDateString("ar-YE") },
      { value: item.entryNumber, className: "font-mono" },
      { value: item.description, align: "start" as const },
      { value: item.debit > 0 ? fc(item.debit) : "—" },
      { value: item.credit > 0 ? fc(item.credit) : "—" },
      { value: fc(item.runningBalance), bold: true },
    ]),
    buildTotals: (data: AccountStatementResultDto) => {
      const lastBalance = data.items?.[data.items.length - 1]?.runningBalance ?? 0;
      return [{ value: "الرصيد النهائي", bold: true, colSpan: 4 }, { value: fc(lastBalance), bold: true }];
    },
  },

  "general-ledger": {
    title: "دفتر الأستاذ العام",
    headers: ["التاريخ", "رقم القيد", "البيان", "مركز التكلفة", "مدين", "دائن", "الرصيد"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: GeneralLedgerDto) => data.entries.map((entry) => [
      { value: new Date(entry.entryDate).toLocaleDateString("ar-YE") },
      { value: entry.entryNumber, className: "font-mono" },
      { value: entry.description, align: "start" as const },
      { value: entry.costCenterName || "—" },
      { value: entry.debit > 0 ? fc(entry.debit) : "—" },
      { value: entry.credit > 0 ? fc(entry.credit) : "—" },
      { value: fc(entry.runningBalance), bold: true },
    ]),
    buildTotals: (data: GeneralLedgerDto) => [
      { value: "الإجمالي", bold: true, colSpan: 3 },
      { value: fc(data.totalDebit), bold: true },
      { value: fc(data.totalCredit), bold: true },
      { value: fc(data.closingBalance), bold: true },
    ],
    buildMetadata: (data: GeneralLedgerDto, deps: ReportDeps) => [
      ...deps.yearName ? [{ label: "السنة المالية", value: deps.yearName }] : [],
      ...deps.accountName ? [{ label: "الحساب", value: `${deps.accountName.accountCode} - ${deps.accountName.accountName}` }] : [],
      ...deps.fromDate && deps.toDate ? [{ label: "من", value: deps.fromDate }, { label: "إلى", value: deps.toDate }] : [],
      { label: "الرصيد الافتتاحي", value: fc(data.openingBalance) },
    ],
  },

  "fixed-assets": {
    title: "الأصول الثابتة",
    headers: ["رقم الأصل", "اسم الأصل", "التصنيف", "تاريخ الشراء", "التكلفة", "نسبة الإهلاك", "الإهلاك المتراكم", "القيمة الدفترية", "الحالة"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: FixedAssetDto[]) => data.filter((a) => a.status === 0).map((item) => [
      { value: item.assetNumber, className: "font-mono" },
      { value: item.assetName, align: "start" as const },
      { value: item.categoryName || "—" },
      { value: item.inServiceDate ? new Date(item.inServiceDate).toLocaleDateString("ar-YE") : "—" },
      { value: fc(item.acquiredCost) },
      { value: `${item.depreciationRate}%` },
      { value: fc(item.accumulatedDepreciation), color: chartColors.danger },
      { value: fc(item.netBookValue), bold: true, color: chartColors.success },
      { value: "نشط", color: chartColors.success, bold: true },
    ]),
    buildTotals: (data: FixedAssetDto[]) => {
      const active = data.filter((a) => a.status === 0);
      const totalCost = active.reduce((s, a) => s + a.acquiredCost, 0);
      const totalAccum = active.reduce((s, a) => s + a.accumulatedDepreciation, 0);
      const totalNet = active.reduce((s, a) => s + a.netBookValue, 0);
      return [
        { value: "الإجمالي", bold: true, colSpan: 3 },
        { value: "" },
        { value: fc(totalCost), bold: true },
        { value: "" },
        { value: fc(totalAccum), bold: true, color: chartColors.danger },
        { value: fc(totalNet), bold: true, color: chartColors.success },
        { value: "" },
      ];
    },
  },

  "balance-sheet": {
    title: "الميزانية العمومية",
    headers: ["#", "كود الحساب", "اسم الحساب", "المبلغ"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: BalanceSheetDto) => {
      type TreeNode = { accountCode: string; accountName: string; balance: number; children?: TreeNode[] };
      const flattenTree = (items: TreeNode[], list: TreeNode[] = []): TreeNode[] => {
        items.forEach(item => { list.push(item); if (item.children) flattenTree(item.children, list); });
        return list;
      };
      const all = [
        ...flattenTree(data.assets || []).map((a) => ({ ...a, section: "الأصول" })),
        ...flattenTree(data.liabilities || []).map((l) => ({ ...l, section: "الخصوم" })),
        ...flattenTree(data.equity || []).map((e) => ({ ...e, section: "حقوق الملكية" })),
      ];
      return all.map((item, i) => [
        { value: i + 1 },
        { value: item.accountCode, className: "font-mono" },
        { value: item.accountName, align: "start" as const },
        { value: fc(item.balance), bold: true },
      ]);
    },
    buildTotals: (data: BalanceSheetDto) => [
      { value: "إجمالي الأصول", bold: true, colSpan: 3 },
      { value: fc(data.totalAssets), bold: true, color: chartColors.primary },
    ],
    buildBalanceCheck: (data: BalanceSheetDto) => ({ isBalanced: data.isBalanced, difference: 0 }),
  },

  "income-statement": {
    title: "قائمة الدخل",
    headers: ["#", "الحساب", "المبلغ"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: IncomeStatementDto) => {
      type LineItem = { accountName: string; accountLevel: number; amount: number; children?: LineItem[] };
      const all = [
        ...(data.revenues || []).map((r) => ({ ...r, section: "الإيرادات" })),
        ...(data.expenses || []).map((e) => ({ ...e, section: "المصروفات" })),
      ];
      return all.map((item: LineItem & { section: string }, i) => [
        { value: i + 1 },
        { value: item.accountName, align: "start" as const, bold: item.accountLevel <= 1 },
        { value: fc(item.amount), bold: item.accountLevel <= 1 },
      ]);
    },
    buildTotals: (data: IncomeStatementDto) => [
      { value: "صافي الدخل", bold: true, colSpan: 1 },
      { value: fc(data.netIncome), bold: true, color: data.netIncome >= 0 ? chartColors.success : chartColors.danger },
    ],
  },

  "cash-flow": {
    title: "التدفقات النقدية",
    headers: ["البيان", "المبلغ"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: CashFlowStatementDto) => [
      [{ value: "التدفقات التشغيلية", bold: true }, { value: fc(data.netOperatingCashFlow), bold: true, color: data.netOperatingCashFlow >= 0 ? chartColors.success : chartColors.danger }],
      [{ value: "التدفقات الاستثمارية", bold: true }, { value: fc(data.netInvestingCashFlow), bold: true, color: data.netInvestingCashFlow >= 0 ? chartColors.success : chartColors.danger }],
      [{ value: "التدفقات التمويلية", bold: true }, { value: fc(data.netFinancingCashFlow), bold: true, color: data.netFinancingCashFlow >= 0 ? chartColors.success : chartColors.danger }],
      [{ value: "صافي التدفق النقدي", bold: true }, { value: fc(data.netCashFlow), bold: true, color: data.netCashFlow >= 0 ? chartColors.success : chartColors.danger }],
      [{ value: "الرصيد النهائي", bold: true }, { value: fc(data.endingCashBalance), bold: true }],
    ],
  },

  "quarterly": {
    title: "التقرير الفصلي",
    headers: ["الفصل", "إجمالي التمويل", "إجمالي الإنفاق", "الرصيد المتبقي"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: QuarterlyReportDto) => (data.quarters || []).map((q) => [
      { value: `الفصل ${q.quarterNumber}` },
      { value: fc(q.totalAllocated), bold: true },
      { value: fc(q.totalSpent), bold: true },
      { value: fc(q.remainingBalance), bold: true, color: q.remainingBalance >= 0 ? chartColors.success : chartColors.danger },
    ]),
    buildTotals: (data: QuarterlyReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: fc(data.totalAllocated || 0), bold: true },
      { value: fc(data.totalSpent || 0), bold: true },
      { value: fc(data.totalRemaining || 0), bold: true, color: chartColors.primary },
    ],
  },

  "budget": {
    title: "تقرير الموازنة",
    headers: ["كود الحساب", "اسم الحساب", "الموازنة", "الفعلي", "الانحراف", "النسبة"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: BudgetReportResultDto) => (data.items || []).map((item) => [
      { value: item.accountCode, className: "font-mono" },
      { value: item.accountName, align: "start" as const },
      { value: fc(item.budgetAmount), bold: true },
      { value: fc(item.actualAmount), bold: true },
      { value: fc(item.variance), bold: true, color: item.variance >= 0 ? chartColors.success : chartColors.danger },
      { value: `${item.variancePercent?.toFixed(1) || 0}%` },
    ]),
    buildTotals: (data: BudgetReportResultDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: "" },
      { value: fc(data.totalBudget || 0), bold: true },
      { value: fc(data.totalActual || 0), bold: true },
      { value: fc(data.totalVariance || 0), bold: true },
      { value: "" },
    ],
  },

  "current-operations": {
    title: "العمليات الجارية",
    headers: ["كود الحساب", "اسم الحساب", "الافتتاحي", "مدين", "دائن", "الختامي"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: CurrentOperationsReportDto) => (data.items || []).map((item) => [
      { value: item.accountCode, className: "font-mono" },
      { value: item.accountName, align: "start" as const },
      { value: fc(item.openingBalance), bold: true },
      { value: fc(item.debit), bold: true },
      { value: fc(item.credit), bold: true },
      { value: fc(item.closingBalance), bold: true, color: item.closingBalance >= 0 ? chartColors.success : chartColors.danger },
    ]),
    buildTotals: (data: CurrentOperationsReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: "" },
      { value: fc(data.totalOpening || 0), bold: true },
      { value: fc(data.totalDebit || 0), bold: true },
      { value: fc(data.totalCredit || 0), bold: true },
      { value: fc(data.totalClosing || 0), bold: true },
    ],
  },

  "liabilities": {
    title: "الديون والالتزامات",
    headers: ["كود الحساب", "اسم الحساب", "تاريخ الاستحقاق", "المبلغ", "الحالة"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: LiabilitiesReportDto) => (data.items || []).map((item) => [
      { value: item.accountCode, className: "font-mono" },
      { value: item.accountName, align: "start" as const },
      { value: item.dueDate ? new Date(item.dueDate).toLocaleDateString("ar-YE") : "—" },
      { value: fc(item.amount), bold: true, color: chartColors.danger },
      { value: item.status || "—" },
    ]),
    buildTotals: (data: LiabilitiesReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 3 },
      { value: fc(data.totalAmount || 0), bold: true, color: chartColors.danger },
      { value: "" },
    ],
  },

  "year-over-year": {
    title: "المقارنة السنوية",
    headers: ["اسم الحساب", "المبلغ (السنة 1)", "المبلغ (السنة 2)", "التغيير", "النسبة"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: YearOverYearReportDto) => (data.items || []).map((item) => [
      { value: item.accountName, align: "start" as const },
      { value: fc(item.year1Amount), bold: true },
      { value: fc(item.year2Amount), bold: true },
      { value: fc(item.change), bold: true, color: item.change >= 0 ? chartColors.success : chartColors.danger },
      { value: `${item.changePercent?.toFixed(1) || 0}%` },
    ]),
    buildTotals: (data: YearOverYearReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: fc(data.totalYear1 || 0), bold: true },
      { value: fc(data.totalYear2 || 0), bold: true },
      { value: fc(data.totalChange || 0), bold: true },
      { value: "" },
    ],
  },

  "cumulative-pnl": {
    title: "الأرباح والخسائر المتراكم",
    headers: ["كود الحساب", "اسم الحساب", "المبلغ"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: CumulativePnLReportDto) => (data.items || []).map((item) => [
      { value: item.accountCode, className: "font-mono" },
      { value: item.accountName, align: "start" as const },
      { value: fc(item.amount), bold: true, color: item.amount >= 0 ? chartColors.success : chartColors.danger },
    ]),
    buildTotals: (data: CumulativePnLReportDto) => [
      { value: "صافي الدخل المتراكم", bold: true, colSpan: 1 },
      { value: "" },
      { value: fc(data.netIncome || 0), bold: true, color: (data.netIncome || 0) >= 0 ? chartColors.success : chartColors.danger },
    ],
  },

  "cost-centers": {
    title: "مراكز التكلفة",
    headers: ["كود المركز", "اسم المركز", "الموازنة", "الفعلي", "الانحراف"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: CostCenterReportDto) => (data.items || []).map((item) => [
      { value: item.costCenterCode, className: "font-mono" },
      { value: item.costCenterName, align: "start" as const },
      { value: fc(item.budget), bold: true },
      { value: fc(item.actual), bold: true },
      { value: fc(item.variance), bold: true, color: item.variance >= 0 ? chartColors.success : chartColors.danger },
    ]),
    buildTotals: (data: CostCenterReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: "" },
      { value: fc(data.totalBudget || 0), bold: true },
      { value: fc(data.totalActual || 0), bold: true },
      { value: fc(data.totalVariance || 0), bold: true },
    ],
  },

  "provisions": {
    title: "المخصصات والاحتياطيات",
    headers: ["كود الحساب", "اسم الحساب", "الرصيد الافتتاحي", "الإضافات", "الاستخدامات", "الرصيد الختامي"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: ProvisionsReportDto) => (data.items || []).map((item) => [
      { value: item.accountCode, className: "font-mono" },
      { value: item.accountName, align: "start" as const },
      { value: fc(item.openingBalance), bold: true },
      { value: fc(item.additions), bold: true },
      { value: fc(item.utilizations), bold: true },
      { value: fc(item.closingBalance), bold: true, color: chartColors.primary },
    ]),
    buildTotals: (data: ProvisionsReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: "" },
      { value: fc(data.totalOpening || 0), bold: true },
      { value: fc(data.totalAdditions || 0), bold: true },
      { value: fc(data.totalUtilizations || 0), bold: true },
      { value: fc(data.totalClosing || 0), bold: true, color: chartColors.primary },
    ],
  },

  "depreciation": {
    title: "تقرير الإهلاك",
    headers: ["رقم الأصل", "اسم الأصل", "التكلفة", "نسبة الإهلاك", "الإهلاك المتراكم", "الإهلاك الحالي", "القيمة الدفترية"],
    pageSize: "A4",
    orientation: "landscape",
    buildRows: (data: DepreciationReportDto) => (data.items || []).map((item) => [
      { value: item.assetNumber, className: "font-mono" },
      { value: item.assetName, align: "start" as const },
      { value: fc(item.acquiredCost), bold: true },
      { value: `${item.depreciationRate}%` },
      { value: fc(item.accumulatedDepreciation), color: chartColors.danger },
      { value: fc(item.currentDepreciation), bold: true },
      { value: fc(item.netBookValue), bold: true, color: chartColors.success },
    ]),
    buildTotals: (data: DepreciationReportDto) => [
      { value: "الإجمالي", bold: true, colSpan: 1 },
      { value: "" },
      { value: fc(data.totalCost || 0), bold: true },
      { value: "" },
      { value: fc(data.totalAccumulated || 0), bold: true, color: chartColors.danger },
      { value: fc(data.totalCurrent || 0), bold: true },
      { value: fc(data.totalNetBookValue || 0), bold: true, color: chartColors.success },
    ],
  },

  "subledger": {
    title: "الأستاذ المساعد",
    headers: ["التاريخ", "المرجع", "البيان", "مدين", "دائن", "الرصيد"],
    pageSize: "A4",
    orientation: "portrait",
    buildRows: (data: SubledgerReportResultDto) => (data.entries || []).map((item) => [
      { value: new Date(item.entryDate).toLocaleDateString("ar-YE") },
      { value: item.entryNumber || "—" },
      { value: item.description || "", align: "start" as const },
      { value: item.debit > 0 ? fc(item.debit) : "—" },
      { value: item.credit > 0 ? fc(item.credit) : "—" },
      { value: fc(item.runningBalance), bold: true },
    ]),
    buildTotals: (data: SubledgerReportResultDto) => [
      { value: "الإجمالي", bold: true, colSpan: 3 },
      { value: fc(data.totalDebit || 0), bold: true },
      { value: fc(data.totalCredit || 0), bold: true },
      { value: fc(data.closingBalance || 0), bold: true },
    ],
    buildMetadata: (data: SubledgerReportResultDto, deps: ReportDeps) => [
      ...deps.yearName ? [{ label: "السنة المالية", value: deps.yearName }] : [],
      ...deps.fromDate && deps.toDate ? [{ label: "من", value: deps.fromDate }, { label: "إلى", value: deps.toDate }] : [],
      { label: "الرصيد الافتتاحي", value: fc(data.openingBalance || 0) },
    ],
  },
};

/* ═══════════════════════════════════════════════════════════
   Data Fetching Hook
   ═══════════════════════════════════════════════════════════ */

function useReportData(reportType: string, params: Record<string, any>) {
  const { fiscalYearId, periodId, fromDate, toDate, accountId, date, year1Id, year2Id, quarterId } = params;

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: periods } = useQuery({
    queryKey: ["periods", fiscalYearId],
    queryFn: () => periodsApi.getAll(fiscalYearId),
    enabled: !!fiscalYearId,
  });

  const { data: accounts } = useQuery({
    queryKey: ["accounts", "all"],
    queryFn: () => accountsApi.getAll(),
  });

  const queries = {
    "trial-balance": useQuery({
      queryKey: ["reports", "trial-balance", fiscalYearId, periodId, fromDate, toDate],
      queryFn: () => reportsApi.trialBalance({ fiscalYearId, periodId, fromDate, toDate }),
      enabled: reportType === "trial-balance",
    }),
    "journal-register": useQuery({
      queryKey: ["reports", "journal-register", fiscalYearId, fromDate, toDate],
      queryFn: () => reportsApi.journalRegister({ fiscalYearId, fromDate, toDate }),
      enabled: reportType === "journal-register",
    }),
    "account-statement": useQuery({
      queryKey: ["reports", "account-statement", accountId, fiscalYearId, fromDate, toDate],
      queryFn: () => reportsApi.accountStatement({ accountId, fiscalYearId, fromDate, toDate }),
      enabled: reportType === "account-statement" && !!accountId,
    }),
    "general-ledger": useQuery({
      queryKey: ["reports", "general-ledger", accountId, fromDate, toDate],
      queryFn: () => reportsApi.generalLedger({ accountId, fromDate, toDate }),
      enabled: reportType === "general-ledger" && !!accountId,
    }),
    "balance-sheet": useQuery({
      queryKey: ["reports", "balance-sheet", date, fiscalYearId],
      queryFn: () => reportsApi.balanceSheet(date, fiscalYearId),
      enabled: reportType === "balance-sheet" && !!date,
    }),
    "income-statement": useQuery({
      queryKey: ["reports", "income-statement", fiscalYearId],
      queryFn: () => reportsApi.incomeStatement(fiscalYearId),
      enabled: reportType === "income-statement" && !!fiscalYearId,
    }),
    "cash-flow": useQuery({
      queryKey: ["reports", "cash-flow", fiscalYearId],
      queryFn: () => reportsApi.cashFlow(fiscalYearId),
      enabled: reportType === "cash-flow" && !!fiscalYearId,
    }),
    "fixed-assets": useQuery({
      queryKey: ["fixed-assets"],
      queryFn: () => fixedAssetApi.getAll(),
      enabled: reportType === "fixed-assets",
    }),
    "quarterly": useQuery({
      queryKey: ["reports", "quarterly", fiscalYearId, quarterId],
      queryFn: () => reportsApi.quarterlyReport({ fiscalYearId, quarterId }),
      enabled: reportType === "quarterly",
    }),
    "budget": useQuery({
      queryKey: ["reports", "budget", fiscalYearId],
      queryFn: () => reportsApi.budgetReport({ fiscalYearId }),
      enabled: reportType === "budget" && !!fiscalYearId,
    }),
    "current-operations": useQuery({
      queryKey: ["reports", "current-operations", fiscalYearId],
      queryFn: () => reportsApi.currentOperations({ fiscalYearId }),
      enabled: reportType === "current-operations" && !!fiscalYearId,
    }),
    "liabilities": useQuery({
      queryKey: ["reports", "liabilities", fiscalYearId],
      queryFn: () => reportsApi.liabilitiesReport({ fiscalYearId }),
      enabled: reportType === "liabilities" && !!fiscalYearId,
    }),
    "year-over-year": useQuery({
      queryKey: ["reports", "year-over-year", year1Id, year2Id],
      queryFn: () => reportsApi.yearOverYear({ year1Id, year2Id }),
      enabled: reportType === "year-over-year",
    }),
    "cumulative-pnl": useQuery({
      queryKey: ["reports", "cumulative-pnl", fiscalYearId],
      queryFn: () => reportsApi.cumulativePnL({ fiscalYearId }),
      enabled: reportType === "cumulative-pnl" && !!fiscalYearId,
    }),
    "cost-centers": useQuery({
      queryKey: ["reports", "cost-centers", fiscalYearId],
      queryFn: () => reportsApi.costCentersReport({ fiscalYearId }),
      enabled: reportType === "cost-centers" && !!fiscalYearId,
    }),
    "provisions": useQuery({
      queryKey: ["reports", "provisions", fiscalYearId],
      queryFn: () => reportsApi.provisionsReport({ fiscalYearId }),
      enabled: reportType === "provisions" && !!fiscalYearId,
    }),
    "depreciation": useQuery({
      queryKey: ["reports", "depreciation", fiscalYearId],
      queryFn: () => reportsApi.depreciationReport({ fiscalYearId }),
      enabled: reportType === "depreciation" && !!fiscalYearId,
    }),
    "subledger": useQuery({
      queryKey: ["reports", "subledger", accountId],
      queryFn: () => reportsApi.subledger({ accountId }),
      enabled: reportType === "subledger" && !!accountId,
    }),
  };

  const activeQuery = queries[reportType as keyof typeof queries];
  const isLoading = activeQuery?.isLoading ?? false;
  const isError = activeQuery?.isError ?? false;
  const error = activeQuery?.error;
  const data = activeQuery?.data;

  return {
    fiscalYears,
    periods,
    accounts,
    data,
    isLoading,
    isError,
    error,
  };
}

/* ═══════════════════════════════════════════════════════════
   Page Component
   ═══════════════════════════════════════════════════════════ */

const DEFAULT_SIGNATURES = [
  { title: "رئيس قسم الحسابات", sub: "رائد / خالد صالح بن صالح" },
  { title: "رئيس قسم المصروفات والإيرادات", sub: "عقيد / منصور عبد الله اليماني" },
  { title: "المدير المالي", sub: "عقيدركن / شامخ عبد الرحمن فتاح" },
  { title: "المدير العام التنفيذي", sub: "عميد ركن/ جمال فضل القطيبي" },
];

export default function AccountingReportPrintPage() {
  const navigate = useNavigate();
  const { reportType } = useParams<{ reportType: string }>();
  const [searchParams] = useSearchParams();

  const params = useMemo(() => ({
    fiscalYearId: searchParams.get("fiscalYearId") ? Number(searchParams.get("fiscalYearId")) : undefined,
    periodId: searchParams.get("periodId") ? Number(searchParams.get("periodId")) : undefined,
    fromDate: searchParams.get("fromDate") ?? undefined,
    toDate: searchParams.get("toDate") ?? undefined,
    accountId: searchParams.get("accountId") ? Number(searchParams.get("accountId")) : undefined,
    date: searchParams.get("date") ?? undefined,
    year1Id: searchParams.get("year1Id") ? Number(searchParams.get("year1Id")) : undefined,
    year2Id: searchParams.get("year2Id") ? Number(searchParams.get("year2Id")) : undefined,
    quarterId: searchParams.get("quarterId") ? Number(searchParams.get("quarterId")) : undefined,
  }), [searchParams]);

  const { data, fiscalYears, periods, accounts, isLoading, isError, error } = useReportData(reportType ?? "", params);

  const config = REPORT_REGISTRY[reportType ?? ""];
  const yearName = fiscalYears?.find((y: { fiscalYearID: number; yearName: string }) => y.fiscalYearID === params.fiscalYearId)?.yearName ?? "";
  const periodName = periods?.find((p: { periodID: number; periodName: string }) => p.periodID === params.periodId)?.periodName ?? "";
  const accountName = accounts?.find((a: { accountID: number; accountCode: string; accountName: string }) => a.accountID === params.accountId);

  const handlePrint = useCallback(() => window.print(), []);

  /* ── Memoized report data ── */
  const reportData = useMemo(() => {
    if (!config || !data) return null;

    const deps: ReportDeps = {
      yearName,
      periodName,
      accountName: accountName ? { accountCode: accountName.accountCode, accountName: accountName.accountName } : undefined,
      fromDate: params.fromDate,
      toDate: params.toDate,
      date: params.date,
    };

    const rows = config.buildRows(data);
    const totals = config.buildTotals?.(data);
    const balanceCheck = config.buildBalanceCheck?.(data);
    const extraMetadata = config.buildMetadata?.(data, deps) ?? [];

    const metadata: { label: string; value: string }[] = [
      ...(yearName ? [{ label: "السنة المالية", value: yearName }] : []),
      ...(periodName ? [{ label: "الفترة", value: periodName }] : []),
      ...(accountName ? [{ label: "الحساب", value: `${accountName.accountCode} - ${accountName.accountName}` }] : []),
      ...(params.fromDate && params.toDate ? [{ label: "من", value: params.fromDate }, { label: "إلى", value: params.toDate }] : []),
      ...(params.date ? [{ label: "حتى تاريخ", value: params.date }] : []),
      ...extraMetadata,
    ];

    return { rows, totals, metadata, balanceCheck };
  }, [config, data, yearName, periodName, accountName, params]);

  /* ── Loading ── */
  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen gap-4 text-muted-foreground">
        <div className="w-10 h-10 border-4 border-primary border-t-transparent rounded-full animate-spin" />
        <p className="text-lg font-medium">جاري تحميل البيانات...</p>
      </div>
    );
  }

  /* ── Error ── */
  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen gap-4 p-6">
        <AlertTriangle className="w-16 h-16 text-destructive" />
        <h2 className="text-xl font-bold text-destructive">حدث خطأ أثناء تحميل البيانات</h2>
        <p className="text-muted-foreground text-center max-w-md">
          {error instanceof Error ? error.message : "يرجى التحقق من الاتصال والمحاولة مرة أخرى"}
        </p>
        <Button variant="outline" onClick={() => navigate(-1)}>
          <ArrowRight className="w-4 h-4 ms-2" />
          رجوع
        </Button>
      </div>
    );
  }

  /* ── No config ── */
  if (!config) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen gap-4 p-6">
        <FileText className="w-16 h-16 text-muted-foreground" />
        <h2 className="text-xl font-bold">تقرير غير معروف</h2>
        <p className="text-muted-foreground">نوع التقرير "{reportType}" غير مدعوم</p>
        <Button variant="outline" onClick={() => navigate(-1)}>
          <ArrowRight className="w-4 h-4 ms-2" />
          رجوع
        </Button>
      </div>
    );
  }

  /* ── Empty data ── */
  if (!reportData || reportData.rows.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen gap-4 p-6">
        <FileText className="w-16 h-16 text-muted-foreground" />
        <h2 className="text-xl font-bold">لا توجد بيانات</h2>
        <p className="text-muted-foreground">لا توجد بيانات متاحة لهذا التقرير في الفترة المحددة</p>
        <Button variant="outline" onClick={() => navigate(-1)}>
          <ArrowRight className="w-4 h-4 ms-2" />
          رجوع
        </Button>
      </div>
    );
  }

  return (
    <>
      {/* ─── Toolbar (شاشة فقط) ─── */}
      <div className="print:hidden sticky top-0 z-50 bg-white/95 backdrop-blur border-b shadow-sm">
        <div className="max-w-7xl mx-auto flex items-center justify-between px-6 py-3">
          <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
            <ArrowRight className="w-4 h-4 ms-2" />
            رجوع
          </Button>

          <div className="flex items-center gap-2">
            <FileText className="w-5 h-5 text-primary" />
            <span className="font-bold text-sm">{config.title}</span>
            {reportData.metadata.length > 0 && (
              <span className="text-muted-foreground text-xs hidden sm:inline">
                — {reportData.metadata.map(m => m.value).join(" | ")}
              </span>
            )}
          </div>

          <Button size="sm" onClick={handlePrint} className="gap-2">
            <Printer className="w-4 h-4" />
            <span className="hidden sm:inline">طباعة</span>
          </Button>
        </div>
      </div>

      {/* ─── منطقة الطباعة ─── */}
      <div className="w-full flex justify-center p-4 print:p-0">
        <PrintableReport
          title={config.title}
          metadata={reportData.metadata}
          tableHeaders={config.headers}
          rows={reportData.rows}
          totals={reportData.totals}
          balanceCheck={reportData.balanceCheck}
          signatures={DEFAULT_SIGNATURES}
          sectionTitle="الالإدارة المالية"
          pageSize={config.pageSize ?? "A4"}
          orientation={config.orientation ?? "portrait"}
        />
      </div>
    </>
  );
}