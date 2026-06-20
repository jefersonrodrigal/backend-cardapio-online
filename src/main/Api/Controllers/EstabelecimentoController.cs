using Application.Features.Estabelecimento.Commands;
using Application.Features.Estabelecimento.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstabelecimentoController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await sender.Send(new GetEstabelecimentoQuery(), ct));

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertEstabelecimentoCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));
}
