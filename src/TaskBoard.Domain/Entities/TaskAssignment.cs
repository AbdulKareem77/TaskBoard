namespace TaskBoard.Domain.Entities;

public class TaskAssignment
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateAssigned { get; set; }
    // Denormalized for display
    public string FullName { get; set; } = string.Empty;
}
