using Application.Features.Inventory.Commands;
using Application.Features.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default) =>
        Ok(await sender.Send(new GetInventoryQuery(page, pageSize, status, search), ct));

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await sender.Send(new GetInventoryMovementsQuery(productId, page, pageSize), ct));

    [HttpPost("movements")]
    public async Task<IActionResult> CreateMovement([FromBody] CreateInventoryMovementCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));
}
