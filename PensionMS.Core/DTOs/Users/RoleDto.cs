namespace PensionMS.Core.DTOs.Users;

public class RoleDto
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public List<TablePermissionDto> Permissions { get; set; } = new();
    public List<PermissionDto> GranularPermissions { get; set; } = new();
}

public class TablePermissionDto
{
    public int PermissionID { get; set; }
    public string TableNameAr { get; set; } = string.Empty;
    public string TableNameEn { get; set; } = string.Empty;
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}
