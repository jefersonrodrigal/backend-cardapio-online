using Application.Features.NeighborhoodDeliveryFees.Commands;
using Application.Features.NeighborhoodDeliveryFees.Dtos;
using Application.Features.NeighborhoodDeliveryFees.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NeighborhoodDeliveryFeesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<NeighborhoodDeliveryFeeDto>>> GetAll(CancellationToken ct) =>
        Ok(await sender.Send(new GetNeighborhoodDeliveryFeesQuery(), ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<NeighborhoodDeliveryFeeDto>> Create([FromBody] CreateNeighborhoodDeliveryFeeCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<NeighborhoodDeliveryFeeDto>> Update(int id, [FromBody] UpdateNeighborhoodDeliveryFeeBody body, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateNeighborhoodDeliveryFeeCommand(id, body.Neighborhood, body.Fee, body.IsActive), ct));

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteNeighborhoodDeliveryFeeCommand(id), ct);
        return NoContent();
    }
}

public record UpdateNeighborhoodDeliveryFeeBody(string Neighborhood, decimal Fee, bool IsActive);
