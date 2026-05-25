using AutoMapper;
using MediatR;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public ArchiveProjectCommandHandler(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId)
            ?? throw new KeyNotFoundException("Project not found.");

        await _projectRepository.SetArchivedAsync(request.ProjectId, isArchived: true);

        project.IsArchived = true;
        return _mapper.Map<ProjectDto>(project);
    }
}

public class UnarchiveProjectCommandHandler : IRequestHandler<UnarchiveProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public UnarchiveProjectCommandHandler(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(UnarchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId)
            ?? throw new KeyNotFoundException("Project not found.");

        await _projectRepository.SetArchivedAsync(request.ProjectId, isArchived: false);

        project.IsArchived = false;
        return _mapper.Map<ProjectDto>(project);
    }
}
