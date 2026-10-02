import { z } from "zod";

// ═══════════════════════════════════════
// BUDGET ITEM DTOs
// ═══════════════════════════════════════

export interface BudgetItemDto {
  budgetItemID: number;
  fiscalYearID: number;
  fiscalYearName: string | null;
  quarterID: number | null;
  quarterName: string | null;
  periodID: number | null;
  periodName: string | null;
  accountID: number;
  accountCode: string;
  accountName: string;
  plannedAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  status: string;
  notes: string | null;
}

// ═══════════════════════════════════════
// BUDGET REPORT DTOs
// ═══════════════════════════════════════

export interface BudgetReportDto {
  fiscalYearID: number;
  yearName: string;
  totalPlanned: number;
  totalActual: number;
  totalVariance: number;
  totalVariancePercentage: number;
  items: BudgetItemDto[];
}

export interface QuarterlyBudgetReportDto {
  fiscalYearID: number;
  yearName: string;
  quarters: QuarterBudgetDto[];
}

export interface QuarterBudgetDto {
  quarterID: number;
  quarterName: string;
  totalPlanned: number;
  totalActual: number;
  variance: number;
  items: BudgetItemDto[];
}

// ═══════════════════════════════════════
// MONTHLY BUDGET REPORT
// ═══════════════════════════════════════

export interface MonthlyBudgetReportDto {
  fiscalYearID: number;
  yearName: string;
  months: MonthBudgetDto[];
}

export interface MonthBudgetDto {
  periodID: number;
  periodName: string;
  periodNumber: number;
  totalPlanned: number;
  totalActual: number;
  variance: number;
  items: BudgetItemDto[];
}

// ═══════════════════════════════════════
// REQUEST DTOs
// ═══════════════════════════════════════

export interface CreateBudgetItemDto {
  fiscalYearID: number;
  quarterID?: number | null;
  periodID?: number | null;
  accountID: number;
  plannedAmount: number;
  notes?: string;
}

export interface UpdateBudgetItemDto {
  plannedAmount: number;
  notes?: string;
}

export interface BudgetFilterDto {
  fiscalYearID: number;
  quarterID?: number;
  accountID?: number;
}

// ═══════════════════════════════════════
// VALIDATION SCHEMAS
// ═══════════════════════════════════════

export const createBudgetItemSchema = z.object({
  accountID: z.number().min(1, "الحساب مطلوب"),
  plannedAmount: z.number().min(0, "المبلغ يجب أن يكون موجباً"),
  quarterID: z.number().nullable().optional(),
  periodID: z.number().nullable().optional(),
  notes: z.string().optional(),
});

export type CreateBudgetItemForm = z.infer<typeof createBudgetItemSchema>;
