using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

/// <summary>
/// خدمة الموازنة — التخطيط والمتابعة
/// </summary>
public interface IBudgetService
{
    // ═══════════════════════════════════════
    // CRUD
    // ═══════════════════════════════════════
    Task<ApiResponse<List<BudgetItemDto>>> GetBudgetItemsAsync(BudgetFilterDto filter);
    Task<ApiResponse<BudgetItemDto>> GetBudgetItemByIdAsync(int id);
    Task<ApiResponse<BudgetItemDto>> CreateBudgetItemAsync(CreateBudgetItemDto dto, int userId);
    Task<ApiResponse<BudgetItemDto>> UpdateBudgetItemAsync(int id, UpdateBudgetItemDto dto, int userId);
    Task<ApiResponse<bool>> DeleteBudgetItemAsync(int id, int userId);

    // ═══════════════════════════════════════
    // BULK OPERATIONS
    // ═══════════════════════════════════════
    Task<ApiResponse<bool>> BulkCreateBudgetItemsAsync(List<CreateBudgetItemDto> items, int userId);

    // ═══════════════════════════════════════
    // SEED BUDGET
    // ═══════════════════════════════════════
    Task<ApiResponse<bool>> SeedBudgetAsync(int fiscalYearId, int userId);

    // ═══════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════
    Task<ApiResponse<BudgetReportDto>> GetBudgetReportAsync(int fiscalYearId);
    Task<ApiResponse<QuarterlyBudgetReportDto>> GetQuarterlyBudgetReportAsync(int fiscalYearId);
    Task<ApiResponse<BudgetReportDto>> GetQuarterBudgetReportAsync(int quarterId);
    Task<ApiResponse<MonthlyBudgetReportDto>> GetMonthlyBudgetReportAsync(int fiscalYearId);
}
