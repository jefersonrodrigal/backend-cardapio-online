using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertTakeBlipIntegrationCommand(
    bool Enabled,
    string BotShortName,
    string AuthorizationKey,
    string WebhookUrl
) : IRequest<TakeBlipIntegrationDto>;

public class UpsertTakeBlipIntegrationValidator : AbstractValidator<UpsertTakeBlipIntegrationCommand>
{
    public UpsertTakeBlipIntegrationValidator()
    {
        RuleFor(x => x.BotShortName).MaximumLength(200);
        RuleFor(x => x.AuthorizationKey).MaximumLength(500);
        RuleFor(x => x.WebhookUrl).MaximumLength(1000);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.BotShortName).NotEmpty();
            RuleFor(x => x.AuthorizationKey).NotEmpty();
        });
    }
}

public class UpsertTakeBlipIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertTakeBlipIntegrationCommand, TakeBlipIntegrationDto>
{
    public async Task<TakeBlipIntegrationDto> Handle(UpsertTakeBlipIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.TakeBlip, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.TakeBlip };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.AccountId = cmd.BotShortName;
        integration.ApiKey = cmd.AuthorizationKey;
        integration.WebhookUrl = cmd.WebhookUrl;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new TakeBlipIntegrationDto(integration.Enabled, integration.AccountId, integration.ApiKey, integration.WebhookUrl);
    }
}
