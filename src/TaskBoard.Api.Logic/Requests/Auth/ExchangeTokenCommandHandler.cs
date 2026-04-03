using AutoMapper;
using MediatR;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Auth;
using TaskBoard.Infrastructure.Cache;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Auth;

public class ExchangeTokenCommandHandler : IRequestHandler<ExchangeTokenCommand, ExchangeTokenResult?>
{
    private readonly IInMemoryRedis _redis;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionStore _sessionStore;
    private readonly IMapper _mapper;

    public ExchangeTokenCommandHandler(
        IInMemoryRedis redis,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ISessionStore sessionStore,
        IMapper mapper)
    {
        _redis = redis;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _sessionStore = sessionStore;
        _mapper = mapper;
    }

    public async Task<ExchangeTokenResult?> Handle(ExchangeTokenCommand request, CancellationToken cancellationToken)
    {
        var otcKey = $"otc:{request.Code}";
        var userIdStr = await _redis.GetAsync(otcKey);
        if (string.IsNullOrEmpty(userIdStr))
            return null;

        // One-time use — delete the key immediately
        await _redis.DeleteAsync(otcKey);

        if (!Guid.TryParse(userIdStr, out var userId))
            return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var roles = user.Roles.Select(r => r.Name).ToList();
        var sessionId = Guid.NewGuid().ToString("N");

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email, sessionId, roles);

        var sessionData = new SessionData(
            UserId: user.Id,
            Email: user.Email,
            Roles: roles,
            CreatedAt: DateTime.UtcNow,
            ExpiresAt: DateTime.UtcNow.AddHours(1));

        await _sessionStore.CreateSessionAsync(sessionId, sessionData, TimeSpan.FromHours(1));

        var userDto = _mapper.Map<UserDto>(user);
        return new ExchangeTokenResult(token, userDto);
    }
}
