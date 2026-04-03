using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class UpdateTaskCommand : IRequest<TaskItemDetailDto?>
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "Todo";
    public string? Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public int RowVersion { get; init; }
}
