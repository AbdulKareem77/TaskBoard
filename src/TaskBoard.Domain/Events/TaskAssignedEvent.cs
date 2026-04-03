namespace TaskBoard.Domain.Events;

public class TaskAssignedEvent : DomainEvent
{
    public override string EventName => "TaskAssigned";
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AssigneeId { get; init; }
    public Guid AssignedByUserId { get; init; }
    public string TaskTitle { get; init; } = string.Empty;
}
