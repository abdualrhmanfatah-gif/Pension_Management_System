using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Core.DTOs.Accounting;

public class CreateVoucherDto
{
    public VoucherType VoucherType { get; set; }
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
    public string? PayeeName { get; set; }
    public string? PayerName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DebitAccountID { get; set; }
    public int CreditAccountID { get; set; }
    public string? ChequeNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ReferralSource { get; set; }
}

public class UpdateVoucherDto
{
    public DateTime? VoucherDate { get; set; }
    public string? PayeeName { get; set; }
    public string? PayerName { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public int? DebitAccountID { get; set; }
    public int? CreditAccountID { get; set; }
    public string? ChequeNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ReferralSource { get; set; }
}

public class VoucherDto
{
    public int VoucherID { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public string VoucherTypeName => VoucherType == VoucherType.Payment ? "سند صرف" : "سند قبض";
    public VoucherStatus Status { get; set; }
    public string StatusName => Status switch
    {
        VoucherStatus.Draft => "مسودة",
        VoucherStatus.Posted => "مرحّل",
        VoucherStatus.Cancelled => "ملغي",
        _ => "غير معروف"
    };
    public DateTime VoucherDate { get; set; }
    public string? PayeeName { get; set; }
    public string? PayerName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    // الحسابات
    public int? DebitAccountID { get; set; }
    public string? DebitAccountCode { get; set; }
    public string DebitAccountName { get; set; } = string.Empty;
    public int CreditAccountID { get; set; }
    public string CreditAccountCode { get; set; } = string.Empty;
    public string CreditAccountName { get; set; } = string.Empty;

    public string? ChequeNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ReferralSource { get; set; }
    public int? JournalEntryID { get; set; }
    public string? JournalEntryNumber { get; set; }

    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class VoucherFilterDto
{
    public VoucherType? VoucherType { get; set; }
    public VoucherStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// ═══════════════════════════════════════
// IMPORT DTOs
// ═══════════════════════════════════════

public class ImportVoucherResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int SkipCount { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = new();
    public List<int> CreatedVoucherIds { get; set; } = new();
}

public class ImportErrorDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}
