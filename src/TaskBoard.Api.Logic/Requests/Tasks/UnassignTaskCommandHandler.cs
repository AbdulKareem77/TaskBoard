using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class UnassignTaskCommandHandler : IRequestHandler<UnassignTaskCommand, bool>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskHistoryRepository _taskHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UnassignTaskCommandHandler(
        ITaskRepository taskRepository,
        ITaskHistoryRepository taskHistoryRepository,
        IUserRepository userRepository,
        IDbConnectionFactory connectionFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _taskRepository = taskRepository;
        _taskHistoryRepository = taskHistoryRepository;
        _userRepository = userRepository;
        _connectionFactory = connectionFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(UnassignTaskCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var assignee = await _userRepository.GetByIdAsync(request.AssigneeUserId);
        var assigneeName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}" : request.AssigneeUserId.ToString();

        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskItemId = request.TaskId,
            UserId = userId,
            Action = "Unassigned",
            OldValue = assigneeName,
            NewValue = null,
            DateCreated = DateTime.UtcNow
        };

        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();
        try
        {
            await _taskRepository.RemoveAssignmentAsync(request.TaskId, request.AssigneeUserId, transaction);
            await _taskHistoryRepository.InsertAsync(history, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return true;
    }
}
