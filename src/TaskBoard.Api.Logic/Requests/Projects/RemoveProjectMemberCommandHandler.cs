using MediatR;
using TaskBoard.Infrastructure.Cache;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICacheService _cacheService;

    public RemoveProjectMemberCommandHandler(
        IProjectRepository projectRepository,
        ICacheService cacheService)
    {
        _projectRepository = projectRepository;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        await _projectRepository.RemoveMemberAsync(request.ProjectId, request.UserId);
        await _cacheService.InvalidateAsync($"project-members:{request.ProjectId}");
        return true;
    }
}
