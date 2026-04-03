using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
