using MediatR;
using TaskBoard.Infrastructure.Auth;
using TaskBoard.Infrastructure.Cache;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInMemoryRedis _redis;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IInMemoryRedis redis)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _redis = redis;
    }

    public async Task<LoginResult?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return null;

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        var code = Guid.NewGuid().ToString("N");
        await _redis.SetAsync($"otc:{code}", user.Id.ToString(), TimeSpan.FromSeconds(60));

        return new LoginResult(code);
    }
}
