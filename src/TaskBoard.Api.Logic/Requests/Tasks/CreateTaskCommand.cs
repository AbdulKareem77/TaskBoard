using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class CreateTaskCommand : IRequest<TaskItemDto>
{
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Status { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Priority { get; init; }
}
