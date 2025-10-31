using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure.General;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeChallenge.Api.Application.Services;

internal interface IAuthService
{
    string GenerateJwtToken(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

internal class AuthService(
    IClock clock,
    IEncryptionService encryptionService,
    IConfiguration configuration
) : IAuthService
{
    public string GenerateJwtToken(User user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "EmployeeChallenge";
        var audience = jwtSettings["Audience"] ?? "EmployeeChallenge";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: clock.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashPassword(string password)
    {
        var encrypted = encryptionService.PasswordEncrypt(password);
        return encrypted;
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = encryptionService.VerifyPassword(password, passwordHash);
        return result;
    }
}
