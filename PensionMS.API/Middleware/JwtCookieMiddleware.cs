namespace PensionMS.API.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;

    public JwtCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
        {
            var jwtCookie = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(jwtCookie))
            {
                context.Request.Headers.Authorization = $"Bearer {jwtCookie}";
            }
        }

        await _next(context);
    }
}
