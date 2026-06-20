using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertNinetyNineFoodIntegrationCommand(
    bool Enabled,
    string ClientId,
    string ClientSecret,
    string StoreId,
    string WebhookUrl
) : IRequest<NinetyNineFoodIntegrationDto>;

public class UpsertNinetyNineFoodIntegrationValidator : AbstractValidator<UpsertNinetyNineFoodIntegrationCommand>
{
    public UpsertNinetyNineFoodIntegrationValidator()
    {
        RuleFor(x => x.ClientId).MaximumLength(200);
        RuleFor(x => x.ClientSecret).MaximumLength(500);
        RuleFor(x => x.StoreId).MaximumLength(200);
        RuleFor(x => x.WebhookUrl).MaximumLength(1000);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ClientSecret).NotEmpty();
            RuleFor(x => x.StoreId).NotEmpty();
        });
    }
}

public class UpsertNinetyNineFoodIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertNinetyNineFoodIntegrationCommand, NinetyNineFoodIntegrationDto>
{
    public async Task<NinetyNineFoodIntegrationDto> Handle(UpsertNinetyNineFoodIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.NinetyNineFood, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.NinetyNineFood };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.ClientId = cmd.ClientId;
        integration.ClientSecret = cmd.ClientSecret;
        integration.AccountId = cmd.StoreId;
        integration.WebhookUrl = cmd.WebhookUrl;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new NinetyNineFoodIntegrationDto(integration.Enabled, integration.ClientId, integration.ClientSecret, integration.AccountId, integration.WebhookUrl);
    }
}
