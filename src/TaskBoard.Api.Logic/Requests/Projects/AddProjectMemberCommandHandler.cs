using MediatR;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, bool>
{
    private readonly IProjectRepository _projectRepository;

    public AddProjectMemberCommandHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<bool> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        await _projectRepository.UpsertMemberAsync(request.ProjectId, request.UserId, request.Role);
        return true;
    }
}
