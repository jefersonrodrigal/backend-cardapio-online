using Application.Features.AdditionalGroups.Commands;
using Application.Features.AdditionalGroups.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/additional-groups")]
public class AdditionalGroupsController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid productId, CancellationToken ct) =>
        Ok(await sender.Send(new GetAdditionalGroupsQuery(productId), ct));

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateAdditionalGroupRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new CreateAdditionalGroupCommand(
            productId, req.Name, req.MinSelections, req.MaxSelections, req.SortOrder), ct);
        return Created(string.Empty, result);
    }

    [Authorize]
    [HttpPut("{groupId:guid}")]
    public async Task<IActionResult> Update(Guid productId, Guid groupId, [FromBody] UpdateAdditionalGroupRequest req, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateAdditionalGroupCommand(
            groupId, req.Name, req.MinSelections, req.MaxSelections, req.SortOrder), ct));

    [Authorize]
    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> Delete(Guid productId, Guid groupId, CancellationToken ct)
    {
        await sender.Send(new DeleteAdditionalGroupCommand(groupId), ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{groupId:guid}/items")]
    public async Task<IActionResult> CreateItem(Guid productId, Guid groupId, [FromBody] CreateAdditionalItemRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new CreateAdditionalItemCommand(
            groupId, req.Name, req.Price, req.SortOrder), ct);
        return Created(string.Empty, result);
    }

    [Authorize]
    [HttpPut("{groupId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid productId, Guid groupId, Guid itemId, [FromBody] UpdateAdditionalItemRequest req, CancellationToken ct) =>
        Ok(await sender.Send(new UpdateAdditionalItemCommand(
            itemId, req.Name, req.Price, req.IsAvailable, req.SortOrder), ct));

    [Authorize]
    [HttpDelete("{groupId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid productId, Guid groupId, Guid itemId, CancellationToken ct)
    {
        await sender.Send(new DeleteAdditionalItemCommand(itemId), ct);
        return NoContent();
    }
}

public record CreateAdditionalGroupRequest(string Name, int MinSelections = 0, int MaxSelections = 1, int SortOrder = 0);
public record UpdateAdditionalGroupRequest(string Name, int MinSelections, int MaxSelections, int SortOrder);
public record CreateAdditionalItemRequest(string Name, decimal Price, int SortOrder = 0);
public record UpdateAdditionalItemRequest(string Name, decimal Price, bool IsAvailable, int SortOrder);
