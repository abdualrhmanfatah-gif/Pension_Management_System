using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PensionMS.API.Security;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Security;

namespace PensionMS.IntegrationTests;

public class GlobalAdministrationScopeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GlobalAdministration_RequiresAnExplicitCentralAssignment(bool central)
    {
        var permissions = new Mock<IScopedPermissionResolver>();
        permissions.Setup(p => p.HasPermissionAsync(7, "System.Admin")).ReturnsAsync(true);
        permissions.Setup(p => p.GetAllowedScopeAsync(7, "System.Admin"))
            .ReturnsAsync(central ? PermissionScope.All : PermissionScope.Restricted([1]));
        using var services = new ServiceCollection().AddSingleton(permissions.Object).BuildServiceProvider();
        var requirement = new PermissionRequirement("System.Admin");
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "7")], "test"));
        var context = new AuthorizationHandlerContext([requirement], user,
            new DefaultHttpContext { RequestServices = services });
        await new PermissionHandler(services, NullLogger<PermissionHandler>.Instance).HandleAsync(context);
        Assert.Equal(central, context.HasSucceeded);
    }
}
