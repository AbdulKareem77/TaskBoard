using MediatR;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Api.Logic.Shared.Authorization;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class ArchiveProjectCommand : IRequest<ProjectDto>, IRequirePermission
{
    public Guid ProjectId { get; init; }
    public string Permission => "ProjectManage";
}
