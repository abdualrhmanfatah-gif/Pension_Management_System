using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IJournalEntryTypeService
{
    Task<ApiResponse<List<JournalEntryTypeDto>>> GetAllAsync();
    Task<ApiResponse<List<JournalEntryTypeDto>>> GetAllIncludingInactiveAsync();
    Task<ApiResponse<JournalEntryTypeDto>> GetByIdAsync(int id);
    Task<ApiResponse<JournalEntryTypeDto>> CreateAsync(CreateJournalEntryTypeDto dto);
    Task<ApiResponse<JournalEntryTypeDto>> UpdateAsync(int id, UpdateJournalEntryTypeDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
