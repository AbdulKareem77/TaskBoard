using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Logic.Requests.Dashboard;

namespace TaskBoard.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _mediator.Send(new GetDashboardQuery());
        return Ok(dashboard);
    }
}
