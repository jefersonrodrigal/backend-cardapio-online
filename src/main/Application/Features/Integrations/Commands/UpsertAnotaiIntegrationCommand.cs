using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertAnotaiIntegrationCommand(
    bool Enabled,
    string ApiToken,
    string AccountId,
    string WebhookUrl
) : IRequest<AnotaiIntegrationDto>;

public class UpsertAnotaiIntegrationValidator : AbstractValidator<UpsertAnotaiIntegrationCommand>
{
    public UpsertAnotaiIntegrationValidator()
    {
        RuleFor(x => x.ApiToken).MaximumLength(500);
        RuleFor(x => x.AccountId).MaximumLength(200);
        RuleFor(x => x.WebhookUrl).MaximumLength(1000);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ApiToken).NotEmpty();
            RuleFor(x => x.AccountId).NotEmpty();
        });
    }
}

public class UpsertAnotaiIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertAnotaiIntegrationCommand, AnotaiIntegrationDto>
{
    public async Task<AnotaiIntegrationDto> Handle(UpsertAnotaiIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.Anotai, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.Anotai };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.ApiKey = cmd.ApiToken;
        integration.AccountId = cmd.AccountId;
        integration.WebhookUrl = cmd.WebhookUrl;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new AnotaiIntegrationDto(integration.Enabled, integration.ApiKey, integration.AccountId, integration.WebhookUrl);
    }
}
