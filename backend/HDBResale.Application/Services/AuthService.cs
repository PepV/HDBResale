using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using HDBResale.Application.Interfaces;
using HDBResale.Shared.Configuration;
using Microsoft.Extensions.Logging;

namespace HDBResale.Application.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly Dictionary<string, (string Password, string Role)> _users = new()
    {
        { "admin", ("admin123", "Admin") },
        { "user", ("user123", "User") }
    };

    public AuthService(IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
    {
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger;
    }

    public string GenerateToken(string username)
    {
        try
        {
            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            
            var user = _users.GetValueOrDefault(username);
            var role = user.Role ?? "User";
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                Issuer = _jwtSettings.Issuer ?? "HDBResaleAPI",
                Audience = _jwtSettings.Audience ?? "HDBResaleClient",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token");
            throw;
        }
    }

    public bool ValidateToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return false;

            if (string.IsNullOrEmpty(_jwtSettings.Key))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer ?? "HDBResaleAPI",
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience ?? "HDBResaleClient",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateCredentials(string username, string password)
    {
        return _users.ContainsKey(username) && _users[username].Password == password;
    }

    public string GetUserRole(string username)
    {
        if (_users.TryGetValue(username, out var user))
        {
            return user.Role;
        }
        return "User";
    }
}