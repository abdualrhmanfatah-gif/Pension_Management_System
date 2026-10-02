using System.Collections.Generic;

namespace PensionMS.Core.DTOs.Users;


public class SetUserPermissionsDto
{
    public List<UserPermissionItemDto> Permissions { get; set; } = new();
}

public class UserPermissionItemDto
{
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; } = true;
}
