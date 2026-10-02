using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.PaymentBooks;

namespace PensionMS.Core.Interfaces;

public interface IPaymentBookService
{
    Task<ApiResponse<List<PaymentBookDto>>> GetByFileIdAsync(int fileId);
    Task<ApiResponse<PaymentBookDto>> GetByIdAsync(int id);
    Task<ApiResponse<PaymentBookDto?>> GetActiveByFileIdAsync(int fileId);
    Task<ApiResponse<bool>> CheckBookNumberExistsAsync(string bookNumber);
    Task<ApiResponse<PaymentBookDto>> CreateAsync(CreatePaymentBookDto dto, int userId);
    Task<ApiResponse<PaymentBookDto>> UpdateAsync(int id, UpdatePaymentBookDto dto, int userId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int userId);
    Task<ApiResponse<PagedResult<PaymentBookReviewDto>>> GetReviewAsync(PaymentBookReviewQueryDto query);
    Task<ApiResponse<PaymentBookStatsDto>> GetStatsAsync();
}

public class PaymentBookReviewQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public string? BookType { get; set; }
    public int? BranchId { get; set; }
    public int? ExpiringWithinDays { get; set; }
    public string? Search { get; set; }
}
