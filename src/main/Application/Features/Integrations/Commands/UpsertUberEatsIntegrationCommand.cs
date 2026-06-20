using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertUberEatsIntegrationCommand(
    bool Enabled,
    string ClientId,
    string ClientSecret,
    string StoreId,
    string WebhookSigningSecret
) : IRequest<UberEatsIntegrationDto>;

public class UpsertUberEatsIntegrationValidator : AbstractValidator<UpsertUberEatsIntegrationCommand>
{
    public UpsertUberEatsIntegrationValidator()
    {
        RuleFor(x => x.ClientId).MaximumLength(200);
        RuleFor(x => x.ClientSecret).MaximumLength(500);
        RuleFor(x => x.StoreId).MaximumLength(200);
        RuleFor(x => x.WebhookSigningSecret).MaximumLength(500);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ClientSecret).NotEmpty();
            RuleFor(x => x.StoreId).NotEmpty();
        });
    }
}

public class UpsertUberEatsIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertUberEatsIntegrationCommand, UberEatsIntegrationDto>
{
    public async Task<UberEatsIntegrationDto> Handle(UpsertUberEatsIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.UberEats, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.UberEats };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.ClientId = cmd.ClientId;
        integration.ClientSecret = cmd.ClientSecret;
        integration.AccountId = cmd.StoreId;
        integration.WebhookSecret = cmd.WebhookSigningSecret;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new UberEatsIntegrationDto(integration.Enabled, integration.ClientId, integration.ClientSecret, integration.AccountId, integration.WebhookSecret);
    }
}
