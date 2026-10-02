using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class LoginHistoryService : ILoginHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public LoginHistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> GetAllAsync(int page, int pageSize, int? userId, bool? isSuccessful, string? ipAddress, DateTime? from, DateTime? to, string? search)
    {
        var query = _unitOfWork.LoginHistories.Query()
            .AsNoTracking()
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(l => l.UserID == userId.Value);
        if (isSuccessful.HasValue)
            query = query.Where(l => l.IsSuccessful == isSuccessful.Value);
        if (!string.IsNullOrEmpty(ipAddress))
            query = query.Where(l => l.IPAddress != null && l.IPAddress.Contains(ipAddress));
        if (from.HasValue)
            query = query.Where(l => l.LoginTime >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.LoginTime <= to.Value);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(l => l.AttemptedUsername.Contains(search));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoginTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.LoginID,
                l.UserID,
                userName = l.User != null ? l.User.FullName : null,
                l.AttemptedUsername,
                l.LoginTime,
                l.IPAddress,
                l.UserAgent,
                l.IsSuccessful,
                l.FailureReason,
                l.DeviceID
            })
            .ToListAsync();

        // إحصائيات سريعة
        var (todayStart, tomorrowStart) = PensionMS.Core.Time.YemenTime.PeriodUtc("today");
        var stats = new
        {
            totalAttempts = await _unitOfWork.LoginHistories.Query().AsNoTracking().CountAsync(),
            failedToday = await _unitOfWork.LoginHistories.Query().AsNoTracking()
                .Where(l => !l.IsSuccessful && l.LoginTime >= todayStart && l.LoginTime < tomorrowStart)
                .CountAsync(),
            uniqueIpsToday = await _unitOfWork.LoginHistories.Query().AsNoTracking()
                .Where(l => l.LoginTime >= todayStart && l.LoginTime < tomorrowStart && l.IPAddress != null)
                .Select(l => l.IPAddress)
                .Distinct()
                .CountAsync(),
            lockedAccounts = await _unitOfWork.Users.Query().AsNoTracking()
                .Where(u => u.IsLocked)
                .CountAsync()
        };

        return ApiResponse<object>.Success(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            stats
        });
    }

    public async Task<ApiResponse<object>> GetByIpAsync(string ip)
    {
        var attempts = await _unitOfWork.LoginHistories.Query()
            .AsNoTracking()
            .Where(l => l.IPAddress == ip)
            .OrderByDescending(l => l.LoginTime)
            .Take(100)
            .Select(l => new
            {
                l.LoginID,
                l.UserID,
                userName = l.User != null ? l.User.FullName : null,
                l.AttemptedUsername,
                l.LoginTime,
                l.IPAddress,
                l.IsSuccessful,
                l.FailureReason
            })
            .ToListAsync();

        return ApiResponse<object>.Success(attempts);
    }
}
