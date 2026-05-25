using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Events;
using TaskBoard.Infrastructure;
using TaskBoard.Infrastructure.Events;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskHistoryRepository _taskHistoryRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        ITaskHistoryRepository taskHistoryRepository,
        IDomainEventPublisher eventPublisher,
        IDbConnectionFactory connectionFactory,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _taskHistoryRepository = taskHistoryRepository;
        _eventPublisher = eventPublisher;
        _connectionFactory = connectionFactory;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status ?? "Todo",
            Priority = request.Priority,
            DueDate = request.DueDate,
            CreatedByUserId = userId,
            RowVersion = 1,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow,
            Assignees = new List<TaskAssignment>(),
            History = new List<TaskHistory>()
        };

        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskItemId = task.Id,
            UserId = userId,
            Action = "Created",
            OldValue = null,
            NewValue = task.Title,
            DateCreated = DateTime.UtcNow
        };

        var domainEvent = new TaskCreatedEvent
        {
            TaskId = task.Id,
            ProjectId = request.ProjectId,
            Title = task.Title,
            CreatedByUserId = userId
        };

        // Wrap in a single transaction
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();
        try
        {
            await _taskRepository.InsertAsync(task, transaction);
            await _taskHistoryRepository.InsertAsync(history, transaction);
            await _eventPublisher.PublishAsync(domainEvent, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return _mapper.Map<TaskItemDto>(task);
    }
}
