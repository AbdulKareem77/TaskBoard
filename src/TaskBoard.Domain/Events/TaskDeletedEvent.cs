namespace TaskBoard.Domain.Events;

public class TaskDeletedEvent : DomainEvent
{
    public override string EventName => "TaskDeleted";
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid DeletedByUserId { get; init; }
}
