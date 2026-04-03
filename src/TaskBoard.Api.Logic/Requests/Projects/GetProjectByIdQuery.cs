using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class GetProjectByIdQuery : IRequest<ProjectDetailDto?>
{
    public Guid ProjectId { get; init; }
}
