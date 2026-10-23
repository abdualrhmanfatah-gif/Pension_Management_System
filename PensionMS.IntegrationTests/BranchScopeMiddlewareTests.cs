using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PensionMS.API.Middleware;
using PensionMS.Core.Exceptions;

namespace PensionMS.IntegrationTests;

public class BranchScopeMiddlewareTests
{
    [Fact]
    public async Task MissingBranch_ReturnsForbiddenFromExceptionMiddleware()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new BranchScopeAccessException(),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        using var services = new ServiceCollection().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }
}
