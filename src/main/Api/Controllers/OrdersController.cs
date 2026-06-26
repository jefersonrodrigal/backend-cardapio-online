using System.Security.Claims;
using Api.Configuration;
using Application.Features.Orders.Commands;
using Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string? date = null, CancellationToken ct = default)
    {
        DateOnly? parsedDate = DateOnly.TryParse(date, out var d) ? d : null;
        return Ok(await sender.Send(new GetOrdersQuery(page, pageSize, parsedDate), ct));
    }

    [Authorize(AuthenticationSchemes = ClientAuthOptions.AuthenticationScheme)]
    [HttpGet("client")]
    public async Task<IActionResult> GetClientOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
        {
            return Unauthorized(new { error = "Sessao do cliente invalida." });
        }

        return Ok(await sender.Send(new GetClientOrdersQuery(clientId, page, pageSize), ct));
    }

    [AllowAnonymous]
    [HttpGet("track/{id:guid}")]
    public async Task<IActionResult> GetTracking(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetOrderTrackingQuery(id), ct));

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct)
    {
        var result = await sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetTracking), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}/advance")]
    public async Task<IActionResult> Advance(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new AdvanceOrderStatusCommand(id), ct));

    [Authorize]
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new CancelOrderCommand(id), ct));
}
