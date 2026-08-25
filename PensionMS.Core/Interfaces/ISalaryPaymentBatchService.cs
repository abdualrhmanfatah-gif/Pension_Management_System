using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Salary;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// خدمة كشوفات صرف المعاشات
/// </summary>
public interface ISalaryPaymentBatchService
{
    /// <summary>
    /// جلب جميع الكشوفات (مع pagination)
    /// </summary>
    Task<ApiResponse<PagedResult<SalaryBatchListDto>>> GetAllAsync(int page, int pageSize, string? status = null);

    /// <summary>
    /// جلب تفاصيل كشف
    /// </summary>
    Task<ApiResponse<SalaryBatchDto>> GetByIdAsync(int id);

    /// <summary>
    /// إنشاء كشف صرف جديد
    /// </summary>
    Task<ApiResponse<SalaryBatchDto>> CreateAsync(CreateSalaryBatchDto dto, int userId);

    /// <summary>
    /// توليد عناصر الصرف تلقائياً
    /// </summary>
    Task<ApiResponse<GenerateItemsResultDto>> GenerateItemsAsync(int batchId, int userId);

    /// <summary>
    /// اعتماد كشف الصرف
    /// </summary>
    Task<ApiResponse<bool>> ApproveAsync(int batchId, int userId);

    /// <summary>
    /// حذف كشف (Draft فقط)
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int batchId, int userId);

    /// <summary>
    /// جلب عناصر صرف كشف معين
    /// </summary>
    Task<ApiResponse<List<SalaryPaymentItemDto>>> GetItemsByBatchIdAsync(int batchId);

    /// <summary>
    /// جلب عناصر صرف ملف تقاعد معين
    /// </summary>
    Task<ApiResponse<List<SalaryPaymentItemDto>>> GetItemsByFileIdAsync(int fileId);

    /// <summary>
    /// تحديث حالة عنصر صرف
    /// </summary>
    Task<ApiResponse<bool>> UpdateItemStatusAsync(int itemId, UpdateItemStatusDto dto);

    /// <summary>
    /// تصدير كشف صرف إلى Excel
    /// </summary>
    Task<byte[]> ExportBatchToExcelAsync(int batchId);

    /// <summary>
    /// استيراد نتائج المعاملات من Excel
    /// </summary>
    Task<ApiResponse<ImportResultDto>> ImportTransactionResultsAsync(int batchId, Stream excelStream);
}
