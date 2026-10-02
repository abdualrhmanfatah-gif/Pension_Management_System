using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Entities.Accounting;

namespace PensionMS.Core.Entities;

/// <summary>
/// البنوك
/// </summary>
public class Bank
{
    public int BankID { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string? BankCode { get; set; }

    /// <summary>رقم الحساب البنكي</summary>
    [MaxLength(50)]
    public string? BankAccountNo { get; set; }

    /// <summary>رمز السويفت</summary>
    [MaxLength(20)]
    public string? SwiftCode { get; set; }

    /// <summary>معرف الحساب في الدليل المحاسبي (حساب 181)</summary>
    public int? AccountID { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AccountID))]
    public virtual ChartOfAccount? Account { get; set; }

    public virtual ICollection<SalaryPaymentBatch> SalaryPaymentBatches { get; set; } = new List<SalaryPaymentBatch>();
    public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
}
