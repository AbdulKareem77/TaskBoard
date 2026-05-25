using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Logic.Requests.Projects;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;

    public ProjectsController(IMediator mediator, IUserRepository userRepository)
    {
        _mediator = mediator;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] bool includeArchived = false)
    {
        var projects = await _mediator.Send(new GetProjectsQuery { IncludeArchived = includeArchived });
        return Ok(projects);
    }

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetProject(Guid projectId)
    {
        var project = await _mediator.Send(new GetProjectByIdQuery { ProjectId = projectId });
        if (project == null)
            return NotFound(new { error = "Project not found." });

        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        var project = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
    }

    [HttpPost("{projectId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return NotFound(new { error = $"No user found with email '{request.Email}'." });

        var command = new AddProjectMemberCommand
        {
            ProjectId = projectId,
            UserId = user.Id,
            Role = request.Role
        };
        await _mediator.Send(command);
        return Ok(new { success = true });
    }

    [HttpDelete("{projectId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
    {
        var command = new RemoveProjectMemberCommand
        {
            ProjectId = projectId,
            UserId = userId
        };
        await _mediator.Send(command);
        return Ok(new { success = true });
    }

    [HttpPost("{projectId:guid}/archive")]
    public async Task<IActionResult> ArchiveProject(Guid projectId)
    {
        var project = await _mediator.Send(new ArchiveProjectCommand { ProjectId = projectId });
        return Ok(project);
    }

    [HttpPost("{projectId:guid}/unarchive")]
    public async Task<IActionResult> UnarchiveProject(Guid projectId)
    {
        var project = await _mediator.Send(new UnarchiveProjectCommand { ProjectId = projectId });
        return Ok(project);
    }
}

public record AddMemberRequest(string Email, string Role);
