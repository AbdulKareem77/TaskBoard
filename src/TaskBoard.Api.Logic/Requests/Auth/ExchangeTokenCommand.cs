using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Auth;

public record ExchangeTokenResult(string AccessToken, UserDto User);

public class ExchangeTokenCommand : IRequest<ExchangeTokenResult?>
{
    public string Code { get; init; } = string.Empty;
}
