using PensionMS.Core.Entities;

namespace PensionMS.Core.DTOs.Salary;

// ═══════════════════════════════════════
// BATCH DTOs
// ═══════════════════════════════════════

public class CreateSalaryBatchDto
{
    public int BankID { get; set; }
    public DateTime BatchMonth { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
}

public class SalaryBatchDto
{
    public int BatchID { get; set; }
    public int BankID { get; set; }
    public string? BankName { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime BatchMonth { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserName { get; set; }
    public string? ImportedByUserName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SalaryBatchListDto
{
    public int BatchID { get; set; }
    public string? BankName { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime BatchMonth { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ═══════════════════════════════════════
// PAYMENT ITEM DTOs
// ═══════════════════════════════════════

public class SalaryPaymentItemDto
{
    public int PaymentItemID { get; set; }
    public int BatchID { get; set; }
    public string? RetireeFullName { get; set; }
    public string? FileNumber { get; set; }
    public decimal Amount { get; set; }
    public string? StatusName { get; set; }
    public string? PaymentBookNumber { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientType { get; set; }
    public string? TransactionNumber { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string? FailureReason { get; set; }
    
    public string? SenderBranch { get; set; }
    public string? ReceiverBranch { get; set; }
    public string? Currency { get; set; }
    public string? Sender { get; set; }
    public string? SalaryStatus { get; set; }
}

// ═══════════════════════════════════════
// GENERATE RESULT DTOs
// ═══════════════════════════════════════

/// <summary>
/// Result of payroll generation for a batch (from IPayrollGenerationService).
/// </summary>
public class PayrollGenerationResult
{
    public List<SalaryPaymentItem> GeneratedItems { get; set; } = new();
    public List<SkippedItemDto> SkippedItems { get; set; } = new();
}

public class GenerateItemsResultDto
{
    public int GeneratedCount { get; set; }
    public int SkippedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SkippedItemDto> SkippedItems { get; set; } = new();
}

public class SkippedItemDto
{
    public string FileNumber { get; set; } = string.Empty;
    public string RetireeFullName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

// ═══════════════════════════════════════
// STATUS UPDATE DTO
// ═══════════════════════════════════════

public class UpdateItemStatusDto
{
    public int StatusID { get; set; }
    public string? TransactionNumber { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string? FailureReason { get; set; }
}

public class ImportResultDto
{
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
