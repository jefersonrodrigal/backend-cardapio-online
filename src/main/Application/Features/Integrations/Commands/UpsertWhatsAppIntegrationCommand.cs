using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public record UpsertWhatsAppIntegrationCommand(
    bool Enabled,
    string PhoneNumberId,
    string BusinessAccountId,
    string AccessToken,
    string AppSecret,
    string VerifyToken
) : IRequest<WhatsAppIntegrationDto>;

public class UpsertWhatsAppIntegrationValidator : AbstractValidator<UpsertWhatsAppIntegrationCommand>
{
    public UpsertWhatsAppIntegrationValidator()
    {
        RuleFor(x => x.PhoneNumberId).MaximumLength(200);
        RuleFor(x => x.BusinessAccountId).MaximumLength(200);
        RuleFor(x => x.AccessToken).MaximumLength(1000);
        RuleFor(x => x.AppSecret).MaximumLength(500);
        RuleFor(x => x.VerifyToken).MaximumLength(200);
        When(x => x.Enabled, () =>
        {
            RuleFor(x => x.PhoneNumberId).NotEmpty();
            RuleFor(x => x.BusinessAccountId).NotEmpty();
            RuleFor(x => x.AccessToken).NotEmpty();
        });
    }
}

public class UpsertWhatsAppIntegrationHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertWhatsAppIntegrationCommand, WhatsAppIntegrationDto>
{
    public async Task<WhatsAppIntegrationDto> Handle(UpsertWhatsAppIntegrationCommand cmd, CancellationToken ct)
    {
        var integration = await db.Integrations
            .FirstOrDefaultAsync(i => i.Provider == IntegrationProvider.WhatsApp, ct);

        if (integration is null)
        {
            integration = new Integration { Provider = IntegrationProvider.WhatsApp };
            db.Integrations.Add(integration);
        }

        integration.Enabled = cmd.Enabled;
        integration.PhoneNumberId = cmd.PhoneNumberId;
        integration.AccountId = cmd.BusinessAccountId;
        integration.AccessToken = cmd.AccessToken;
        integration.AppSecret = cmd.AppSecret;
        integration.VerifyToken = cmd.VerifyToken;
        integration.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new WhatsAppIntegrationDto(integration.Enabled, integration.PhoneNumberId, integration.AccountId, integration.AccessToken, integration.AppSecret, integration.VerifyToken);
    }
}
