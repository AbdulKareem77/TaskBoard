using MediatR;

namespace TaskBoard.Api.Logic.Requests.Auth;

public record LoginResult(string OneTimeCode);

public class LoginCommand : IRequest<LoginResult?>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
