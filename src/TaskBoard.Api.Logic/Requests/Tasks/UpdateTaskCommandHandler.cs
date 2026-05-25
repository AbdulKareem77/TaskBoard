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

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskItemDetailDto?>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskHistoryRepository _taskHistoryRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandler(
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

    public async Task<TaskItemDetailDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        // Load existing task
        var existingTask = await _taskRepository.GetByIdAsync(request.TaskId);
        if (existingTask == null)
            return null;

        var oldStatus = existingTask.Status;

        // Prepare update
        existingTask.Title = request.Title;
        existingTask.Description = request.Description;
        existingTask.Status = request.Status;
        existingTask.Priority = request.Priority;
        existingTask.DueDate = request.DueDate;
        existingTask.RowVersion = request.RowVersion; // used as ExpectedVersion

        var rowsAffected = await _taskRepository.UpdateAsync(existingTask);
        if (rowsAffected == 0)
        {
            // Concurrency conflict
            return null;
        }

        var action = oldStatus != request.Status ? "StatusChanged" : "Updated";
        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskItemId = request.TaskId,
            UserId = userId,
            Action = action,
            OldValue = oldStatus != request.Status ? oldStatus : null,
            NewValue = oldStatus != request.Status ? request.Status : null,
            DateCreated = DateTime.UtcNow
        };

        var domainEvent = new TaskUpdatedEvent
        {
            TaskId = request.TaskId,
            ProjectId = request.ProjectId,
            UpdatedByUserId = userId,
            OldStatus = oldStatus,
            NewStatus = request.Status
        };

        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();
        try
        {
            await _taskHistoryRepository.InsertAsync(history, transaction);
            await _eventPublisher.PublishAsync(domainEvent, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        // Reload updated task
        var updatedTask = await _taskRepository.GetByIdAsync(request.TaskId);
        if (updatedTask == null) return null;

        return _mapper.Map<TaskItemDetailDto>(updatedTask);
    }
}
