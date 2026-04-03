using MediatR;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class UnassignTaskCommand : IRequest<bool>
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AssigneeUserId { get; init; }
}
