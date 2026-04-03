using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Domain.Events;
using TaskBoard.Infrastructure;
using TaskBoard.Infrastructure.Events;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteTaskCommandHandler(
        ITaskRepository taskRepository,
        IDomainEventPublisher eventPublisher,
        IDbConnectionFactory connectionFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _taskRepository = taskRepository;
        _eventPublisher = eventPublisher;
        _connectionFactory = connectionFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var domainEvent = new TaskDeletedEvent
        {
            TaskId = request.TaskId,
            ProjectId = request.ProjectId,
            DeletedByUserId = userId
        };

        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();
        try
        {
            await _eventPublisher.PublishAsync(domainEvent, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        await _taskRepository.DeleteAsync(request.TaskId);
        return true;
    }
}
