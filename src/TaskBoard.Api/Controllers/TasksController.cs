using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Logic.Requests.Tasks;

namespace TaskBoard.Api.Controllers;

[ApiController]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> GetTasks(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new GetTasksQuery
        {
            ProjectId = projectId,
            Page = page,
            PageSize = pageSize,
            Status = status
        });
        return Ok(result);
    }

    [HttpGet("api/projects/{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> GetTask(Guid projectId, Guid taskId)
    {
        var task = await _mediator.Send(new GetTaskByIdQuery { ProjectId = projectId, TaskId = taskId });
        if (task == null)
            return NotFound(new { error = "Task not found." });

        return Ok(task);
    }

    [HttpPost("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskRequest request)
    {
        var command = new CreateTaskCommand
        {
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate
        };
        var task = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { projectId = projectId, taskId = task.Id }, task);
    }

    [HttpPut("api/projects/{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand
        {
            TaskId = taskId,
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            RowVersion = request.RowVersion
        };
        var result = await _mediator.Send(command);
        if (result == null)
            return Conflict(new { error = "Concurrency conflict. The task was modified by another user." });

        return Ok(result);
    }

    [HttpPost("api/projects/{projectId:guid}/tasks/{taskId:guid}/assign")]
    public async Task<IActionResult> AssignTask(Guid projectId, Guid taskId, [FromBody] AssignTaskRequest request)
    {
        var command = new AssignTaskCommand
        {
            TaskId = taskId,
            ProjectId = projectId,
            AssigneeUserId = request.UserId
        };
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    [HttpDelete("api/projects/{projectId:guid}/tasks/{taskId:guid}/assign/{userId:guid}")]
    public async Task<IActionResult> UnassignTask(Guid projectId, Guid taskId, Guid userId)
    {
        var command = new UnassignTaskCommand
        {
            TaskId = taskId,
            ProjectId = projectId,
            AssigneeUserId = userId
        };
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    [HttpGet("api/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
    public async Task<IActionResult> GetTaskComments(Guid projectId, Guid taskId)
    {
        var comments = await _mediator.Send(new GetTaskCommentsQuery
        {
            ProjectId = projectId,
            TaskId = taskId
        });
        return Ok(comments);
    }

    [HttpPost("api/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
    public async Task<IActionResult> CreateTaskComment(
        Guid projectId,
        Guid taskId,
        [FromBody] CreateTaskCommentRequest request)
    {
        var comment = await _mediator.Send(new CreateTaskCommentCommand
        {
            ProjectId = projectId,
            TaskId = taskId,
            Content = request.Content
        });
        return CreatedAtAction(nameof(GetTaskComments), new { projectId, taskId }, comment);
    }

    [HttpDelete("api/projects/{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
    {
        var command = new DeleteTaskCommand
        {
            TaskId = taskId,
            ProjectId = projectId
        };
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    [HttpGet("api/tasks/search")]
    public async Task<IActionResult> SearchTasks(
        [FromQuery] string q = "",
        [FromQuery] Guid? projectId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new SearchTasksQuery
        {
            Q = q,
            ProjectId = projectId,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }
}

public record CreateTaskRequest(
    string Title,
    string? Description,
    string? Status,
    string? Priority,
    DateTime? DueDate);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    string Status,
    string? Priority,
    DateTime? DueDate,
    int RowVersion);

public record AssignTaskRequest(Guid UserId);

public record CreateTaskCommentRequest(string Content);
