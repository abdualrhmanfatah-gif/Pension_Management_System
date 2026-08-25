using Microsoft.AspNetCore.Authorization;

namespace PensionMS.API.Security;

/// <summary>
/// متطلب الصلاحية المخصص الذي سيتم استخدامه في سياسات التخويل
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
