using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Logic.Requests.Reports;
using TaskBoard.Infrastructure.Cache;

namespace TaskBoard.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IInMemoryRedis _redis;

    public ReportsController(IMediator mediator, IInMemoryRedis redis)
    {
        _mediator = mediator;
        _redis = redis;
    }

    [HttpPost("project-summary")]
    public async Task<IActionResult> RequestReport([FromBody] RequestReportRequest request)
    {
        var command = new RequestReportCommand { ProjectId = request.ProjectId };
        var result = await _mediator.Send(command);
        return Accepted(new { reportId = result.ReportId, status = result.Status });
    }

    [HttpGet("{reportId:guid}")]
    public async Task<IActionResult> GetReport(Guid reportId)
    {
        var key = $"report:{reportId}";
        var reportJson = await _redis.GetAsync(key);

        if (reportJson == null)
            return NotFound(new { error = "Report not found or still being generated.", reportId });

        return Content(reportJson, "application/json");
    }
}

public record RequestReportRequest(Guid ProjectId);
