using AutoMapper;
using MediatR;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<ProjectDetailDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId);
        if (project == null) return null;

        var members = await _projectRepository.GetMembersAsync(request.ProjectId);
        project.Members = members.ToList();

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerId,
            project.OwnerName ?? string.Empty,
            project.IsArchived,
            _mapper.Map<IEnumerable<ProjectMemberDto>>(project.Members),
            project.DateCreated);
    }
}
