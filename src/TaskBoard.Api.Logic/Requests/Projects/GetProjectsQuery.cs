using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class GetProjectsQuery : IRequest<IEnumerable<ProjectDto>>
{
}
