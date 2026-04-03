namespace TaskBoard.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo";
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public int RowVersion { get; set; } = 1;
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public List<TaskAssignment> Assignees { get; set; } = new();
    public List<TaskHistory> History { get; set; } = new();
    // Denormalized from join
    public string? ProjectName { get; set; }
    public string? CreatedByFullName { get; set; }
}
