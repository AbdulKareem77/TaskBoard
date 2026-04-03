namespace TaskBoard.Domain.Entities;

public class TaskComment
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}
