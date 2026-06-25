using Application.Features.Categories.Commands;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetCategoriesQuery(), ct));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
        => Ok(await sender.Send(cmd, ct));

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryBody body, CancellationToken ct)
        => Ok(await sender.Send(new UpdateCategoryCommand(id, body.Name, body.SortOrder), ct));

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}

public record UpdateCategoryBody(string Name, int SortOrder);
