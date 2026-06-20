using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertZenviaIntegrationCommand(
    bool Enabled,
    string ApiToken,
    string ChannelId,
    string WebhookUrl
) : IRequest<ZenviaIntegrationDto>;

public class UpsertZenviaIntegrationValidator : AbstractValidator<UpsertZenviaIntegrationCommand>
{
    public UpsertZenviaIntegrationValidator()
    {
        RuleFor(x => x.ApiToken).MaximumLength(500);
        RuleFor(x => x.ChannelId).MaximumLength(200);
        RuleFor(x => x.WebhookUrl).MaximumLength(1000);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ApiToken).NotEmpty();
            RuleFor(x => x.ChannelId).NotEmpty();
        });
    }
}

public class UpsertZenviaIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertZenviaIntegrationCommand, ZenviaIntegrationDto>
{
    public async Task<ZenviaIntegrationDto> Handle(UpsertZenviaIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.Zenvia, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.Zenvia };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.ApiKey = cmd.ApiToken;
        integration.AccountId = cmd.ChannelId;
        integration.WebhookUrl = cmd.WebhookUrl;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new ZenviaIntegrationDto(integration.Enabled, integration.ApiKey, integration.AccountId, integration.WebhookUrl);
    }
}
