namespace PensionMS.Core.DTOs.HR;

public class EmployeeListItemDto
{
    public int EmployeeID { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? SectionName { get; set; }
    public int JobTitleID { get; set; }
    public string JobTitleName { get; set; } = string.Empty;
    public string EmploymentStatusName { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}

public class RankDto
{
    public int RankID { get; set; }
    public string RankName { get; set; } = string.Empty;
    public int RankLevel { get; set; }
    public decimal BaseSalary { get; set; }
}

public class EmployeeDetailsDto
{
    public int EmployeeID { get; set; }
    public int DepartmentID { get; set; }
    public int? SectionID { get; set; }
    public int JobTitleID { get; set; }
    public int EmploymentStatusID { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BirthPlace { get; set; }
    public string? MaritalStatus { get; set; }
    public string? MilitaryNumber { get; set; }
    public int? RankID { get; set; }
    public string? RankName { get; set; }
    public DateTime? MilitaryServiceDate { get; set; }
    public string? JobPositions { get; set; }
    public string? AcademicQualifications { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? BloodGroup { get; set; }
    public string? NationalIDIssuePlace { get; set; }
    public DateTime? NationalIDIssueDate { get; set; }
    public DateTime? NationalIDExpiryDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public byte Gender { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? SectionName { get; set; }
    public string JobTitleName { get; set; } = string.Empty;
    public string EmploymentStatusName { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public bool IsInService { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEmployeeDto
{
    public int DepartmentID { get; set; }
    public int? SectionID { get; set; }
    public int JobTitleID { get; set; }
    public int EmploymentStatusID { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? BirthPlace { get; set; }
    public string? MaritalStatus { get; set; }
    public string? MilitaryNumber { get; set; }
    public int? RankID { get; set; }
    public DateTime? MilitaryServiceDate { get; set; }

    public string? JobPositions { get; set; }
    public string? AcademicQualifications { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? BloodGroup { get; set; }
    public string? NationalIDIssuePlace { get; set; }
    public DateTime? NationalIDIssueDate { get; set; }
    public DateTime? NationalIDExpiryDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public byte Gender { get; set; } // 1: Male, 2: Female
    public DateTime HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class UpdateEmployeeDto
{
    public int? DepartmentID { get; set; }
    public int? SectionID { get; set; }
    public int? JobTitleID { get; set; }
    public int? EmploymentStatusID { get; set; }
    public string? FullName { get; set; }
    public string? NationalID { get; set; }
    public string? Phone { get; set; }
    public string? BirthPlace { get; set; }
    public string? MaritalStatus { get; set; }
    public string? MilitaryNumber { get; set; }
    public int? RankID { get; set; }
    public DateTime? MilitaryServiceDate { get; set; }
    public string? JobPositions { get; set; }
    public string? AcademicQualifications { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? BloodGroup { get; set; }
    public string? NationalIDIssuePlace { get; set; }
    public DateTime? NationalIDIssueDate { get; set; }
    public DateTime? NationalIDExpiryDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public byte? Gender { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }
}
