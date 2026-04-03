using System.Security.Claims;
using TaskBoard.Infrastructure.Auth;
using TaskBoard.Infrastructure.Cache;

namespace TaskBoard.Api.Middleware;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    // Paths excluded from session validation (case-insensitive)
    private static readonly string[] ExcludedPaths =
    {
        "/api/auth/login",
        "/api/auth/token",
        "/swagger"
    };

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Always pass through OPTIONS (CORS preflight) requests
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        // Check if this path is excluded — case-insensitive
        if (ExcludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Extract Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid Authorization header." });
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();

        // Validate JWT
        var jwtService = context.RequestServices.GetRequiredService<IJwtTokenService>();
        var principal = jwtService.ValidateTokenAndGetClaims(token);
        if (principal == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token." });
            return;
        }

        // Extract sessionId from JWT claims
        var sessionId = principal.FindFirst("sessionId")?.Value;
        if (string.IsNullOrEmpty(sessionId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Session ID not found in token." });
            return;
        }

        // Validate session in session store
        var sessionStore = context.RequestServices.GetRequiredService<ISessionStore>();
        var session = await sessionStore.GetSessionAsync(sessionId);

        if (session == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Session not found or expired." });
            return;
        }

        // Build ClaimsPrincipal from session data
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new("sub", session.UserId.ToString()),
            new(ClaimTypes.Email, session.Email),
            new("email", session.Email),
            new("sessionId", sessionId),
            new("roles", string.Join(",", session.Roles))
        };

        foreach (var role in session.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "SessionValidation");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }
}
