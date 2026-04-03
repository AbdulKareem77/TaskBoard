using MediatR;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class AssignTaskCommand : IRequest<bool>
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AssigneeUserId { get; init; }
}
