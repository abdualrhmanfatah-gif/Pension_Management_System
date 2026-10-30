using PensionMS.Core.Interfaces;

namespace PensionMS.Core.DTOs.PaymentBooks;

public class PaymentBookDto
{
    public int PaymentBookID { get; set; }
    public int FileID { get; set; }
    public string BookNumber { get; set; } = string.Empty;
<<<<<<< HEAD
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
=======
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string Status { get; set; } = string.Empty;
    public string BookType { get; set; } = "إلكتروني";
    public string? LegacyColor { get; set; }
    public string? ChangeReason { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientType { get; set; }
    public string? Notes { get; set; }
    public int? IssuingBranchID { get; set; }
    public string? IssuingBranchName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PendingReviewStatus { get; set; }
}

public class CreatePaymentBookDto
{
    public int FileID { get; set; }
    public string BookNumber { get; set; } = string.Empty;
<<<<<<< HEAD
    public DateOnly IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
=======
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string BookType { get; set; } = "إلكتروني";
    public string? LegacyColor { get; set; }
    public string? ChangeReason { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientType { get; set; }
    public string? Notes { get; set; }
    public int? IssuingBranchID { get; set; }
    public IRemoteFile? BookImage { get; set; }

    // Auto Salary Recipient Link
    public bool AutoCreateSalaryRecipient { get; set; } = true;
    public int? RetireeID { get; set; }
    public int? AgentID { get; set; }
    public int? EntitledID { get; set; }
    public int? GuardianID { get; set; }
}

public class UpdatePaymentBookDto
{
    public string? BookNumber { get; set; }
<<<<<<< HEAD
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
=======
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string? Status { get; set; }
    public string? LegacyColor { get; set; }
    public string? ChangeReason { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientType { get; set; }
    public string? Notes { get; set; }
    public int? IssuingBranchID { get; set; }
}

public class PaymentBookReviewDto
{
    public int PaymentBookID { get; set; }
    public string BookNumber { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string? RecipientType { get; set; }
    public string? FileNumber { get; set; }
    public string? RetireeName { get; set; }
    public int? RetireeId { get; set; }
    public string? NationalID { get; set; }
    public string? MilitaryNumber { get; set; }
    public string? Phone { get; set; }
    public string BookType { get; set; } = string.Empty;
    public string? LegacyColor { get; set; }
<<<<<<< HEAD
    public DateOnly IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
=======
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    public string Status { get; set; } = string.Empty;
    public string? IssuingBranchName { get; set; }
    public string? ChangeReason { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public bool IsExpiringSoon { get; set; }
    public List<string> BookImagePaths { get; set; } = new();
}

public class PaymentBookStatsDto
{
    public int TotalActive { get; set; }
    public int TotalExpired { get; set; }
    public int TotalLost { get; set; }
    public int TotalDamaged { get; set; }
    public int ExpiringWithin30Days { get; set; }
}
