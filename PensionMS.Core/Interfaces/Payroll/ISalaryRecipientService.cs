using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.SalaryRecipients;

namespace PensionMS.Core.Interfaces;

public interface ISalaryRecipientService
{
    Task<ApiResponse<List<SalaryRecipientDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<SalaryRecipientDto>> GetByIdAsync(int id);
    Task<ApiResponse<SalaryRecipientDto>> CreateAsync(CreateSalaryRecipientDto dto, int userId);
    Task<ApiResponse<bool>> EndAsync(int id, EndSalaryRecipientDto dto, int userId);
    Task<ApiResponse<string>> UpdatePhotoAsync(int id, IRemoteFile photo, int userId);
}
