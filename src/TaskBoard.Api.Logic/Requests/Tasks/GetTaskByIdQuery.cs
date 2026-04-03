using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class GetTaskByIdQuery : IRequest<TaskItemDetailDto?>
{
    public Guid ProjectId { get; init; }
    public Guid TaskId { get; init; }
}
