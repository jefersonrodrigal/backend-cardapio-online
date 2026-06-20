using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertAiAgentsIntegrationCommand(
    bool Enabled,
    string Provider,
    string ApiKey,
    string Model,
    string AssistantId,
    string WebhookUrl
) : IRequest<AiAgentsIntegrationDto>;

public class UpsertAiAgentsIntegrationValidator : AbstractValidator<UpsertAiAgentsIntegrationCommand>
{
    public UpsertAiAgentsIntegrationValidator()
    {
        RuleFor(x => x.Provider).MaximumLength(50);
        RuleFor(x => x.ApiKey).MaximumLength(500);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.AssistantId).MaximumLength(200);
        RuleFor(x => x.WebhookUrl).MaximumLength(1000);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.Provider).NotEmpty();
            RuleFor(x => x.ApiKey).NotEmpty();
        });
    }
}

public class UpsertAiAgentsIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertAiAgentsIntegrationCommand, AiAgentsIntegrationDto>
{
    public async Task<AiAgentsIntegrationDto> Handle(UpsertAiAgentsIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.AiAgents, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.AiAgents };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.AiProvider = cmd.Provider;
        integration.ApiKey = cmd.ApiKey;
        integration.Model = cmd.Model;
        integration.AssistantId = cmd.AssistantId;
        integration.WebhookUrl = cmd.WebhookUrl;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new AiAgentsIntegrationDto(integration.Enabled, integration.AiProvider, integration.ApiKey, integration.Model, integration.AssistantId, integration.WebhookUrl);
    }
}
