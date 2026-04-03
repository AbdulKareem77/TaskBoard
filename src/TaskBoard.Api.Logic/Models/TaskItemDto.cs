namespace TaskBoard.Api.Logic.Models;

public record AssigneeDto(Guid UserId, string FullName);

public record CreatedByDto(Guid UserId, string FullName);

public class TaskHistoryDto
{
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class TaskItemDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public IEnumerable<AssigneeDto> Assignees { get; set; } = Enumerable.Empty<AssigneeDto>();
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class TaskItemDetailDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public CreatedByDto CreatedBy { get; set; } = new(Guid.Empty, string.Empty);
    public IEnumerable<AssigneeDto> Assignees { get; set; } = Enumerable.Empty<AssigneeDto>();
    public IEnumerable<TaskHistoryDto> History { get; set; } = Enumerable.Empty<TaskHistoryDto>();
    public int RowVersion { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}
