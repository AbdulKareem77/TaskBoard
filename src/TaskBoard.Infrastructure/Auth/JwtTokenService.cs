using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TaskBoard.Infrastructure.Auth;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string sessionId, IEnumerable<string> roles);
    ClaimsPrincipal? ValidateTokenAndGetClaims(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly int _lifetimeMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["Authentication:Secret"]
            ?? throw new InvalidOperationException("Authentication:Secret is required.");
        _issuer = configuration["Authentication:Issuer"] ?? "TaskBoard";
        _lifetimeMinutes = int.TryParse(configuration["Authentication:TokenLifetimeMinutes"], out var mins) ? mins : 60;
    }

    public string GenerateToken(Guid userId, string email, string sessionId, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var roleList = roles.ToList();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("sessionId", sessionId),
            new("roles", string.Join(",", roleList)),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: null,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_lifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateTokenAndGetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
