using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Banks;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services;

public class BankService : IBankService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;

    public BankService(IUnitOfWork unitOfWork, IImmutableAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<ApiResponse<List<BankDto>>> GetAllAsync()
    {
        var banks = await _unitOfWork.Banks.Query()
            .AsNoTracking()
            .Include(b => b.Account)
            .OrderBy(b => b.BankName)
            .Select(b => new BankDto
            {
                BankID = b.BankID,
                BankName = b.BankName,
                BankCode = b.BankCode,
                AccountID = b.AccountID,
                AccountCode = b.Account != null ? b.Account.AccountCode : null,
                AccountName = b.Account != null ? b.Account.AccountName : null,
                IsActive = b.IsActive,
                BatchCount = b.SalaryPaymentBatches.Count(sp => !sp.IsDeleted),
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<BankDto>>.Success(banks);
    }

    public async Task<ApiResponse<BankDto>> GetByIdAsync(int id)
    {
        var bank = await _unitOfWork.Banks.Query()
            .AsNoTracking()
            .Include(b => b.Account)
            .Where(b => b.BankID == id)
            .Select(b => new BankDto
            {
                BankID = b.BankID,
                BankName = b.BankName,
                BankCode = b.BankCode,
                AccountID = b.AccountID,
                AccountCode = b.Account != null ? b.Account.AccountCode : null,
                AccountName = b.Account != null ? b.Account.AccountName : null,
                IsActive = b.IsActive,
                BatchCount = b.SalaryPaymentBatches.Count(sp => !sp.IsDeleted),
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (bank == null)
            return ApiResponse<BankDto>.Fail("البنك غير موجود");

        return ApiResponse<BankDto>.Success(bank);
    }

    public async Task<ApiResponse<BankDto>> CreateAsync(CreateBankDto dto, int userId)
    {
        var bank = new Bank
        {
            BankName = dto.BankName,
            BankCode = dto.BankCode,
            AccountID = dto.AccountID,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Banks.AddAsync(bank);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "Bank", bank.BankID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        var result = new BankDto
        {
            BankID = bank.BankID,
            BankName = bank.BankName,
            BankCode = bank.BankCode,
            AccountID = bank.AccountID,
            IsActive = bank.IsActive,
            BatchCount = 0,
            CreatedAt = bank.CreatedAt
        };

        return ApiResponse<BankDto>.Success(result, "تم إضافة البنك بنجاح");
    }

    public async Task<ApiResponse<BankDto>> UpdateAsync(int id, UpdateBankDto dto, int userId)
    {
        var bank = await _unitOfWork.Banks.GetByIdAsync(id);
        if (bank == null) return ApiResponse<BankDto>.Fail("البنك غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(bank);

        if (dto.BankName != null) bank.BankName = dto.BankName;
        if (dto.BankCode != null) bank.BankCode = dto.BankCode;
        if (dto.AccountID.HasValue) bank.AccountID = dto.AccountID;
        if (dto.IsActive.HasValue) bank.IsActive = dto.IsActive.Value;
        bank.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Banks.Update(bank);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Update", "Bank", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        var updatedRes = await GetByIdAsync(id);
        return updatedRes;
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var bank = await _unitOfWork.Banks.GetByIdAsync(id);
        if (bank == null) return ApiResponse<bool>.Fail("البنك غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(bank);

        // Check if bank has salary batches — deactivate instead of delete
        var hasBatches = await _unitOfWork.SalaryPaymentBatches.Query()
            .AnyAsync(b => b.BankID == id && !b.IsDeleted);

        if (hasBatches)
        {
            bank.IsActive = false;
            bank.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Banks.Update(bank);
            await _auditService.LogAsync(userId, "", "Deactivate", "Bank", id, oldValues, "Has dependencies - deactivated");
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "تم إلغاء تفعيل البنك لوجود كشوف صرف مرتبطة به");
        }
        else
        {
            _unitOfWork.Banks.Delete(bank);
            await _auditService.LogAsync(userId, "", "Delete", "Bank", id, oldValues, null);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "تم حذف البنك بنجاح");
        }
    }

    public async Task<ApiResponse<bool>> ToggleActiveAsync(int id, int userId)
    {
        var bank = await _unitOfWork.Banks.GetByIdAsync(id);
        if (bank == null) return ApiResponse<bool>.Fail("البنك غير موجود");

        var oldValues = bank.IsActive.ToString();
        bank.IsActive = !bank.IsActive;
        bank.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Banks.Update(bank);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "ToggleActive", "Bank", id, oldValues, bank.IsActive.ToString());

        return ApiResponse<bool>.Success(true, "تم تغيير حالة البنك بنجاح");
    }
}
