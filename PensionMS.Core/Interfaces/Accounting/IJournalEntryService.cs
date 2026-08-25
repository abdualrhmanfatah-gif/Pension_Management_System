using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IJournalEntryService
{
    Task<ApiResponse<JournalEntryDto>> CreateJournalEntryAsync(CreateJournalEntryDto dto, int userId);
    Task<ApiResponse<JournalEntryDto>> GetJournalEntryByIdAsync(int id);
    Task<ApiResponse<PagedResult<JournalEntryDto>>> GetJournalEntriesAsync(JournalEntryFilterDto filter);
    Task<ApiResponse<JournalEntryDto>> PostJournalEntryAsync(int id, int userId);
    Task<ApiResponse<JournalEntryDto>> ReverseJournalEntryAsync(int id, int userId, string reason);
    Task<FinancialOperationResultDto> ExecuteOperationAsync(FinancialOperationRequestDto request, int userId);
    Task<FinancialOperationResultDto> CreateIntegrationEntryAsync(FinancialOperationRequestDto request, int userId);
    Task<int> GetPeriodIdForDateAsync(DateTime date);
}
