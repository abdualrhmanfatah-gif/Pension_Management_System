using System;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface ILoginHistoryService
{
    Task<ApiResponse<object>> GetAllAsync(int page, int pageSize, int? userId, bool? isSuccessful, string? ipAddress, DateTime? from, DateTime? to, string? search);
    Task<ApiResponse<object>> GetByIpAsync(string ip);
}
