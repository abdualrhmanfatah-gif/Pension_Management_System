namespace PensionMS.Core.DTOs.Banks;

public class BankDto
{
    public int BankID { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? BankAccountNo { get; set; }
    public string? SwiftCode { get; set; }
    public int? AccountID { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public bool IsActive { get; set; }
    public int BatchCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateBankDto
{
    public string BankName { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? BankAccountNo { get; set; }
    public string? SwiftCode { get; set; }
    public int? AccountID { get; set; }
}

public class UpdateBankDto
{
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public string? BankAccountNo { get; set; }
    public string? SwiftCode { get; set; }
    public int? AccountID { get; set; }
    public bool? IsActive { get; set; }
}
