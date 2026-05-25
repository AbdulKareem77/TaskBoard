using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class CreateTaskCommentCommandHandler : IRequestHandler<CreateTaskCommentCommand, CommentDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskCommentRepository _commentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public CreateTaskCommentCommandHandler(
        ITaskRepository taskRepository,
        ITaskCommentRepository commentRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _commentRepository = commentRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<CommentDto> Handle(CreateTaskCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null || task.ProjectId != request.ProjectId)
            throw new KeyNotFoundException("Task not found.");

        var roles = GetCurrentRoles();
        var isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        if (!isAdmin && !await _projectRepository.IsMemberAsync(request.ProjectId, userId))
            throw new UnauthorizedAccessException("You do not have access to this task.");

        var user = await _userRepository.GetByIdAsync(userId);
        var authorName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskItemId = request.TaskId,
            UserId = userId,
            Content = request.Content.Trim(),
            DateCreated = DateTime.UtcNow,
            AuthorFullName = authorName
        };

        await _commentRepository.InsertAsync(comment);
        return _mapper.Map<CommentDto>(comment);
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
