using Application.Features.Integrations.Commands;
using Application.Features.Integrations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class IntegrationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await sender.Send(new GetIntegrationsQuery(), ct));

    [HttpPut("ifood")]
    public async Task<IActionResult> UpsertIFood([FromBody] UpsertIFoodIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("anotai")]
    public async Task<IActionResult> UpsertAnotai([FromBody] UpsertAnotaiIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("ubereats")]
    public async Task<IActionResult> UpsertUberEats([FromBody] UpsertUberEatsIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("99food")]
    public async Task<IActionResult> UpsertNinetyNineFood([FromBody] UpsertNinetyNineFoodIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("aiagents")]
    public async Task<IActionResult> UpsertAiAgents([FromBody] UpsertAiAgentsIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("whatsapp")]
    public async Task<IActionResult> UpsertWhatsApp([FromBody] UpsertWhatsAppIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("takeblip")]
    public async Task<IActionResult> UpsertTakeBlip([FromBody] UpsertTakeBlipIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));

    [HttpPut("zenvia")]
    public async Task<IActionResult> UpsertZenvia([FromBody] UpsertZenviaIntegrationCommand cmd, CancellationToken ct) =>
        Ok(await sender.Send(cmd, ct));
}
