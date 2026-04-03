using MediatR;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class AddProjectMemberCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
    public string Role { get; init; } = "Contributor";
}
