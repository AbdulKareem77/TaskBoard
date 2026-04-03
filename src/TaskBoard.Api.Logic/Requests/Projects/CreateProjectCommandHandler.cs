using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Projects;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            IsArchived = false,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow,
            MemberCount = 1,
            TaskCount = 0
        };

        await _projectRepository.InsertAsync(project);
        // Add creator as Manager
        await _projectRepository.UpsertMemberAsync(project.Id, userId, "Manager");

        project.OwnerName = "Me"; // Will be loaded from DB on next fetch

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.OwnerName ?? string.Empty,
            project.MemberCount,
            project.TaskCount,
            project.IsArchived,
            project.DateCreated);
    }
}
