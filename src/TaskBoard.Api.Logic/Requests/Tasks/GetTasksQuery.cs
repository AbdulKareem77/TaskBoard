using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class GetTasksQuery : IRequest<PagedResult<TaskItemDto>>
{
    public Guid ProjectId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Status { get; init; }
}
