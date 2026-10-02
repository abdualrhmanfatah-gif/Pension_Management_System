namespace PensionMS.Core.DTOs.Branches;

public class BranchDto
{
    public int PensionBranchID { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? Governorate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public bool IsActive { get; set; }
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBranchDto
{
    public string BranchName { get; set; } = string.Empty;
    public string? Governorate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
}

public class UpdateBranchDto
{
    public string? BranchName { get; set; }
    public string? Governorate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public bool? IsActive { get; set; }
}
