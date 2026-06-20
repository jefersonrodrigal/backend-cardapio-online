using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertIFoodIntegrationCommand(
    bool Enabled,
    string ClientId,
    string ClientSecret,
    string MerchantId
) : IRequest<IFoodIntegrationDto>;

public class UpsertIFoodIntegrationValidator : AbstractValidator<UpsertIFoodIntegrationCommand>
{
    public UpsertIFoodIntegrationValidator()
    {
        RuleFor(x => x.ClientId).MaximumLength(200);
        RuleFor(x => x.ClientSecret).MaximumLength(500);
        RuleFor(x => x.MerchantId).MaximumLength(200);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ClientSecret).NotEmpty();
            RuleFor(x => x.MerchantId).NotEmpty();
        });
    }
}

public class UpsertIFoodIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertIFoodIntegrationCommand, IFoodIntegrationDto>
{
    public async Task<IFoodIntegrationDto> Handle(UpsertIFoodIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.IFood, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.IFood };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.ClientId = cmd.ClientId;
        integration.ClientSecret = cmd.ClientSecret;
        integration.AccountId = cmd.MerchantId;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new IFoodIntegrationDto(integration.Enabled, integration.ClientId, integration.ClientSecret, integration.AccountId);
    }
}
