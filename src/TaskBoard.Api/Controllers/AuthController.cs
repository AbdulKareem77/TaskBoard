using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Logic.Requests.Auth;

namespace TaskBoard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Step 1 of login: validate credentials and return a one-time code.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (result == null)
            return Unauthorized(new { error = "Invalid email or password." });

        return Ok(new { oneTimeCode = result.OneTimeCode });
    }

    /// <summary>
    /// Step 2 of login: exchange one-time code for JWT access token.
    /// </summary>
    [HttpPost("token")]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeTokenCommand command)
    {
        var result = await _mediator.Send(command);
        if (result == null)
            return Unauthorized(new { error = "Invalid or expired code." });

        return Ok(new { accessToken = result.AccessToken, user = result.User });
    }
}
