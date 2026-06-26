using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Configuration;
using Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IOptions<AdminAuthOptions> options) : ControllerBase
{
    private readonly AdminAuthOptions authOptions = options.Value;

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        //if (!string.Equals(request.Email, authOptions.Email, StringComparison.OrdinalIgnoreCase) ||
        //    !PasswordHasher.VerifyPassword(request.Password, authOptions.PasswordHash))
        //{
        //    return Unauthorized(new { error = "Credenciais invalidas." });
        //}

        var expiresAt = DateTime.UtcNow.AddMinutes(authOptions.TokenExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Email),
            new Claim(JwtRegisteredClaimNames.Email, request.Email),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: authOptions.JwtIssuer,
            audience: authOptions.JwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return Ok(new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            request.Email
        ));
    }
}

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, string Email);
