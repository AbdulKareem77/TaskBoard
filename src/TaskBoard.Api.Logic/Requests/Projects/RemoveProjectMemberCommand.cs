using MediatR;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class RemoveProjectMemberCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
}
