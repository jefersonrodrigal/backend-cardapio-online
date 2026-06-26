using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Configuration;
using Application.Features.Clients.Commands;
using Application.Features.Clients.Dtos;
using Application.Features.Clients.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController(
    ISender sender,
    IOptions<AdminAuthOptions> adminAuthOptionsAccessor,
    IOptions<ClientAuthOptions> clientAuthOptionsAccessor) : ControllerBase
{
    private readonly AdminAuthOptions adminAuthOptions = adminAuthOptionsAccessor.Value;
    private readonly ClientAuthOptions clientAuthOptions = clientAuthOptionsAccessor.Value;

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateClientCommand cmd, CancellationToken ct)
    {
        var client = await sender.Send(cmd, ct);
        return Ok(CreateClientAuthResponse(client));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string? search = null, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetClientsQuery(page, pageSize, search), ct));

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand cmd, CancellationToken ct)
    {
        var client = await sender.Send(cmd, ct);
        var response = CreateClientAuthResponse(client);
        return Created($"/api/Clients/{client.Id}", response);
    }

    private ClientAuthResponse CreateClientAuthResponse(ClientDto client)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(clientAuthOptions.TokenExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, client.Email),
            new Claim(ClaimTypes.Role, "Client")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminAuthOptions.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: clientAuthOptions.JwtIssuer,
            audience: clientAuthOptions.JwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new ClientAuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            client
        );
    }
}

public record ClientAuthResponse(string Token, DateTime ExpiresAt, ClientDto Client);
