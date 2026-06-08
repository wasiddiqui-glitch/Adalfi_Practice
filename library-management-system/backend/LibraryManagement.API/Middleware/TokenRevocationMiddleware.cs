using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Middleware;

public class TokenRevocationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Replace("Bearer ", "");
            var exists = await db.UserTokens.AnyAsync(t =>
                t.LoginProvider == "LibraryApp" &&
                t.Name == "AccessToken" &&
                t.Value == token);

            if (!exists)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked." });
                return;
            }
        }

        await next(context);
    }
}
