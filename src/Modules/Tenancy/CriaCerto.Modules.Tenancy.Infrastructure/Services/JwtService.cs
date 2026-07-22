using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CriaCerto.Modules.Tenancy.Application.Abstractions;
using CriaCerto.Modules.Tenancy.Application.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CriaCerto.Modules.Tenancy.Infrastructure.Services;

public sealed class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"] ?? "CriaCertoSuperSecretKeyThatIsAtLeast32BytesLong!";
        _issuer = configuration["Jwt:Issuer"] ?? "CriaCerto";
        _audience = configuration["Jwt:Audience"] ?? "CriaCertoClient";
    }

    public string GenerateToken(User user, Tenant tenant)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("FullName", user.FullName),
            new Claim("TenantId", tenant.Id.ToString()),
            new Claim("TenantName", tenant.Name),
            new Claim("SubscribedPlan", tenant.SubscribedPlan)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
