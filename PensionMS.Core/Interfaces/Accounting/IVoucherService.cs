using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Core.Interfaces.Accounting;

public interface IVoucherService
{
    Task<ApiResponse<VoucherDto>> CreateVoucherAsync(CreateVoucherDto dto, int userId);
    Task<ApiResponse<VoucherDto>> GetVoucherByIdAsync(int id);
    Task<ApiResponse<PagedResult<VoucherDto>>> GetVouchersAsync(VoucherFilterDto filter);
    Task<ApiResponse<VoucherDto>> UpdateVoucherAsync(int id, UpdateVoucherDto dto);
    Task<ApiResponse<bool>> DeleteVoucherAsync(int id, int userId);
    Task<ApiResponse<VoucherDto>> PostVoucherAsync(int id, int userId);
    Task<ApiResponse<VoucherDto>> CancelVoucherAsync(int id, int userId);
    Task<ApiResponse<string>> GetNextVoucherNumberAsync(VoucherType type, DateTime? date = null);
    Task<ApiResponse<ImportVoucherResultDto>> ImportVouchersAsync(Stream fileStream, string fileName, int userId);
}
