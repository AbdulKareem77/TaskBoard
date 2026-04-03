namespace TaskBoard.Domain.Entities;

public class TaskHistory
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime DateCreated { get; set; }
    // Denormalized for display
    public string UserName { get; set; } = string.Empty;
}
