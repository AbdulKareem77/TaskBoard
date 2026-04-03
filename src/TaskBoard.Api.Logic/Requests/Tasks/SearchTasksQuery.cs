using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class SearchTasksQuery : IRequest<PagedResult<TaskItemDto>>
{
    public string Q { get; init; } = string.Empty;
    public Guid? ProjectId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
