namespace PensionMS.Core.DTOs.Citizen;

public class CitizenProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public string? PersonalPhotoPath { get; set; }
    public string? MilitaryNumber { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class UpdateCitizenProfileDto
{
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class CitizenFileDto
{
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime OpeningDate { get; set; }
}

public class CitizenEntitledPersonDto
{
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CitizenPaymentDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
}

public class CitizenPaymentSummaryDto
{
    public decimal TotalThisYear { get; set; }
    public decimal LastPaymentAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal CurrentMonthlyEntitlement { get; set; }
}
