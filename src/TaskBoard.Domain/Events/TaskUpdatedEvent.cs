namespace TaskBoard.Domain.Events;

public class TaskUpdatedEvent : DomainEvent
{
    public override string EventName => "TaskUpdated";
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid UpdatedByUserId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
}
