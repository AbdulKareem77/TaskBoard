namespace TaskBoard.Domain.Events;

public class TaskCreatedEvent : DomainEvent
{
    public override string EventName => "TaskCreated";
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
}
