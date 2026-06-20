using Application.Features.Clients.Commands;
using Application.Features.Clients.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateClientCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string? search = null, CancellationToken ct = default) =>
        Ok(await sender.Send(new GetClientsQuery(page, pageSize, search), ct));

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand cmd, CancellationToken ct)
    {
        var result = await sender.Send(cmd, ct);
        return Created($"/api/Clients/{result.Id}", result);
    }
}
