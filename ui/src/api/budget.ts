<<<<<<< HEAD
import type { ApiResponse } from "@/types/api";
import type {
  BudgetFilterDto,
  BudgetItemDto,
  BudgetReportDto,
  CreateBudgetItemDto,
  MonthlyBudgetReportDto,
  QuarterlyBudgetReportDto,
  UpdateBudgetItemDto,
} from "@/types/budget";

import api from "./axios";

=======
import api from "./axios";

import type { ApiResponse } from "@/types/api";

import type {
  BudgetItemDto,
  BudgetReportDto,
  QuarterlyBudgetReportDto,
  MonthlyBudgetReportDto,
  CreateBudgetItemDto,
  UpdateBudgetItemDto,
  BudgetFilterDto,
} from "@/types/budget";

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
// ═══════════════════════════════════════
// CRUD Operations
// ═══════════════════════════════════════

export const budgetApi = {
  getItems: (filter: BudgetFilterDto) =>
    api.get<ApiResponse<BudgetItemDto[]>>("/budget/items", { params: filter })
      .then((r) => r.data.data),

  getById: (id: number) =>
    api.get<ApiResponse<BudgetItemDto>>(`/budget/items/${id}`)
      .then((r) => r.data.data),

  create: (data: CreateBudgetItemDto) =>
    api.post<ApiResponse<BudgetItemDto>>("/budget/items", data)
      .then((r) => r.data.data),

  update: (id: number, data: UpdateBudgetItemDto) =>
    api.put<ApiResponse<BudgetItemDto>>(`/budget/items/${id}`, data)
      .then((r) => r.data.data),

  delete: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/budget/items/${id}`)
      .then((r) => r.data.data),

  bulkCreate: (items: CreateBudgetItemDto[]) =>
    api.post<ApiResponse<boolean>>("/budget/items/bulk", items)
      .then((r) => r.data.data),

  // ═══════════════════════════════════════
  // SEED BUDGET
  // ═══════════════════════════════════════

  seedBudget: (fiscalYearId: number) =>
    api.post<ApiResponse<boolean>>(`/budget/seed/${fiscalYearId}`)
      .then((r) => r.data),

  // ═══════════════════════════════════════
  // REPORTS
  // ═══════════════════════════════════════

  getAnnualReport: (fiscalYearId: number) =>
    api.get<ApiResponse<BudgetReportDto>>(`/budget/reports/annual/${fiscalYearId}`)
      .then((r) => r.data.data),

  getQuarterlyReport: (fiscalYearId: number) =>
    api.get<ApiResponse<QuarterlyBudgetReportDto>>(`/budget/reports/quarterly/${fiscalYearId}`)
      .then((r) => r.data.data),

  getQuarterReport: (quarterId: number) =>
    api.get<ApiResponse<BudgetReportDto>>(`/budget/reports/quarter/${quarterId}`)
      .then((r) => r.data.data),

  getMonthlyReport: (fiscalYearId: number) =>
    api.get<ApiResponse<MonthlyBudgetReportDto>>(`/budget/reports/monthly/${fiscalYearId}`)
      .then((r) => r.data.data),
};
