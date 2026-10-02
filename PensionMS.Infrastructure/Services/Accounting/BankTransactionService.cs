using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities;
using PensionMS.Core.Entities.Accounting;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Accounting;

public interface IBankTransactionService
{
    Task<ApiResponse<List<BankTransactionDto>>> GetByBankAsync(int bankId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<List<BankTransactionDto>>> GetByAccountAsync(int accountId, DateTime? fromDate, DateTime? toDate);
    Task<ApiResponse<BankTransactionDto>> CreateAsync(CreateBankTransactionDto dto);
}

public class BankTransactionService : IBankTransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public BankTransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<BankTransactionDto>>> GetByBankAsync(int bankId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _unitOfWork.BankTransactions.Query()
            .AsNoTracking()
            .Include(t => t.Bank)
            .Include(t => t.JournalEntry)
            .Where(t => t.BankID == bankId);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.TransactionID)
            .ToListAsync();

        var dtos = transactions.Select(t => new BankTransactionDto
        {
            TransactionID = t.TransactionID,
            BankID = t.BankID,
            BankName = t.Bank?.BankName,
            JournalEntryID = t.JournalEntryID,
            EntryNumber = t.JournalEntry?.EntryNumber,
            LineID = t.LineID,
            TransactionDate = t.TransactionDate,
            TransactionType = t.TransactionType,
            Amount = t.Amount,
            Description = t.Description,
            ReferenceNo = t.ReferenceNo,
            IsReconciled = t.IsReconciled,
            ReconciledAt = t.ReconciledAt,
            BankStatementRef = t.BankStatementRef,
            CreatedAt = t.CreatedAt
        }).ToList();

        return ApiResponse<List<BankTransactionDto>>.Success(dtos);
    }

    public async Task<ApiResponse<List<BankTransactionDto>>> GetByAccountAsync(int accountId, DateTime? fromDate, DateTime? toDate)
    {
        var bank = await _unitOfWork.Banks.Query()
            .FirstOrDefaultAsync(b => b.AccountID == accountId && b.IsActive);

        if (bank == null)
            return ApiResponse<List<BankTransactionDto>>.Success(new List<BankTransactionDto>());

        return await GetByBankAsync(bank.BankID, fromDate, toDate);
    }

    public async Task<ApiResponse<BankTransactionDto>> CreateAsync(CreateBankTransactionDto dto)
    {
        var transaction = new BankTransaction
        {
            BankID = dto.BankID,
            JournalEntryID = dto.JournalEntryID ?? 0,
            LineID = dto.LineID ?? 0,
            TransactionDate = dto.TransactionDate,
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            Description = dto.Description,
            ReferenceNo = dto.ReferenceNo,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BankTransactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<BankTransactionDto>.Success(new BankTransactionDto
        {
            TransactionID = transaction.TransactionID,
            BankID = transaction.BankID,
            TransactionDate = transaction.TransactionDate,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            Description = transaction.Description,
            ReferenceNo = transaction.ReferenceNo,
            IsReconciled = transaction.IsReconciled,
            CreatedAt = transaction.CreatedAt
        }, "تم تسجيل المعاملة بنجاح");
    }
}
