using MediatR;
using Microsoft.AspNetCore.Http;

namespace TaskBoard.Api.Logic.Shared.Authorization;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequirePermission permissionRequest)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst("sub")?.Value ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException($"Authentication is required to perform this action (Permission: {permissionRequest.Permission}).");
            }

            // Role-based permission check.
            // JWT Bearer maps the "roles" JWT claim to ClaimTypes.Role at runtime,
            // so we check both the raw "roles" claim (set by SessionValidationMiddleware)
            // and ClaimTypes.Role (set by JWT Bearer auth middleware).
            var userRoles = user?.Claims
                .Where(c => c.Type == "roles" || c.Type == System.Security.Claims.ClaimTypes.Role)
                .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(r => r.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var hasPermission = permissionRequest.Permission switch
            {
                "TaskDelete" => userRoles.Contains("Admin") || userRoles.Contains("Manager"),
                "ProjectManage" => userRoles.Contains("Admin") || userRoles.Contains("Manager"),
                "MemberManage" => userRoles.Contains("Admin") || userRoles.Contains("Manager"),
                _ => userRoles.Contains("Admin")
            };

            if (!hasPermission)
            {
                throw new UnauthorizedAccessException($"You do not have permission to perform this action (Permission: {permissionRequest.Permission}).");
            }
        }

        return await next();
    }
}
