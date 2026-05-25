using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public GetProjectsQueryHandler(
        IProjectRepository projectRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdStr = user?.FindFirst("sub")?.Value
                     ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var rolesClaimValue = user?.FindFirst("roles")?.Value ?? string.Empty;
        var isAdmin = rolesClaimValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Any(r => r.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));

        var projects = await _projectRepository.GetByUserIdAsync(userId, isAdmin, request.IncludeArchived);
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }
}
