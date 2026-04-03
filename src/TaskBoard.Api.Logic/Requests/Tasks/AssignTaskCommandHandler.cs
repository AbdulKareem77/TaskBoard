using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Events;
using TaskBoard.Infrastructure;
using TaskBoard.Infrastructure.Events;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, bool>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskHistoryRepository _taskHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssignTaskCommandHandler(
        ITaskRepository taskRepository,
        ITaskHistoryRepository taskHistoryRepository,
        IUserRepository userRepository,
        IDomainEventPublisher eventPublisher,
        IDbConnectionFactory connectionFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _taskRepository = taskRepository;
        _taskHistoryRepository = taskHistoryRepository;
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
        _connectionFactory = connectionFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        var assignee = await _userRepository.GetByIdAsync(request.AssigneeUserId);
        var assigneeName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}" : request.AssigneeUserId.ToString();

        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskItemId = request.TaskId,
            UserId = userId,
            Action = "Assigned",
            OldValue = null,
            NewValue = assigneeName,
            DateCreated = DateTime.UtcNow
        };

        var domainEvent = new TaskAssignedEvent
        {
            TaskId = request.TaskId,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeUserId,
            AssignedByUserId = userId,
            TaskTitle = task?.Title ?? string.Empty
        };

        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();
        try
        {
            await _taskRepository.UpsertAssignmentAsync(request.TaskId, request.AssigneeUserId, transaction);
            await _taskHistoryRepository.InsertAsync(history, transaction);
            await _eventPublisher.PublishAsync(domainEvent, transaction);
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
