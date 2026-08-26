using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GerenciadorDeTarefas.Application.Abstractions;
using GerenciadorDeTarefas.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorDeTarefas.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtOptions> options) : ITokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
