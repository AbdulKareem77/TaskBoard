using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class GetTaskCommentsQueryHandler : IRequestHandler<GetTaskCommentsQuery, IEnumerable<CommentDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskCommentRepository _commentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public GetTaskCommentsQueryHandler(
        ITaskRepository taskRepository,
        ITaskCommentRepository commentRepository,
        IProjectRepository projectRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _commentRepository = commentRepository;
        _projectRepository = projectRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CommentDto>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(request.ProjectId, request.TaskId);

        var comments = await _commentRepository.GetByTaskIdAsync(request.TaskId);
        return _mapper.Map<IEnumerable<CommentDto>>(comments);
    }

    private async Task EnsureAccessAsync(Guid projectId, Guid taskId)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.ProjectId != projectId)
            throw new KeyNotFoundException("Task not found.");

        var roles = GetCurrentRoles();
        var isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        if (!isAdmin && !await _projectRepository.IsMemberAsync(projectId, userId))
            throw new UnauthorizedAccessException("You do not have access to this task.");
    }

    private Guid GetCurrentUserId()
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }

    private HashSet<string> GetCurrentRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.Claims
            .Where(c => c.Type == "roles" || c.Type == System.Security.Claims.ClaimTypes.Role)
            .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(r => r.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
