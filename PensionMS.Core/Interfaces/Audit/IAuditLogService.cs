using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IAuditLogService
{
    Task<ApiResponse<object>> GetAllAsync(int page, int pageSize, string? action, int? userId, int? tableId, DateTime? from, DateTime? to, string? search);
    Task<ApiResponse<object>> GetImmutableAsync(int page, int pageSize, string? action, string? entityType, int? actorId, DateTime? from, DateTime? to);
    Task<ApiResponse<List<string>>> GetDistinctActionsAsync();
    Task<ApiResponse<object>> GetTablesAsync();
    Task<ApiResponse<object>> GetByRecordAsync(int tableId, int recordId);
}
