using Application.Common.Interfaces;
using Application.Features.Integrations.Dtos;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Queries;

public record GetIntegrationsQuery : IRequest<IntegrationsOverviewDto>;

public class GetIntegrationsHandler(IApplicationDbContext db)
    : IRequestHandler<GetIntegrationsQuery, IntegrationsOverviewDto>
{
    public async Task<IntegrationsOverviewDto> Handle(GetIntegrationsQuery request, CancellationToken ct)
    {
        var integrations = await db.Integrations.AsNoTracking().ToListAsync(ct);

        Integration Find(IntegrationProvider provider) =>
            integrations.FirstOrDefault(i => i.Provider == provider) ?? new Integration { Provider = provider };

        var iFood = Find(IntegrationProvider.IFood);
        var anotai = Find(IntegrationProvider.Anotai);
        var uberEats = Find(IntegrationProvider.UberEats);
        var ninetyNineFood = Find(IntegrationProvider.NinetyNineFood);
        var aiAgents = Find(IntegrationProvider.AiAgents);
        var whatsApp = Find(IntegrationProvider.WhatsApp);
        var takeBlip = Find(IntegrationProvider.TakeBlip);
        var zenvia = Find(IntegrationProvider.Zenvia);

        return new IntegrationsOverviewDto(
            new IFoodIntegrationDto(iFood.Enabled, iFood.ClientId, iFood.ClientSecret, iFood.AccountId),
            new AnotaiIntegrationDto(anotai.Enabled, anotai.ApiKey, anotai.AccountId, anotai.WebhookUrl),
            new UberEatsIntegrationDto(uberEats.Enabled, uberEats.ClientId, uberEats.ClientSecret, uberEats.AccountId, uberEats.WebhookSecret),
            new NinetyNineFoodIntegrationDto(ninetyNineFood.Enabled, ninetyNineFood.ClientId, ninetyNineFood.ClientSecret, ninetyNineFood.AccountId, ninetyNineFood.WebhookUrl),
            new AiAgentsIntegrationDto(aiAgents.Enabled, aiAgents.AiProvider, aiAgents.ApiKey, aiAgents.Model, aiAgents.AssistantId, aiAgents.WebhookUrl),
            new WhatsAppIntegrationDto(whatsApp.Enabled, whatsApp.PhoneNumberId, whatsApp.AccountId, whatsApp.AccessToken, whatsApp.AppSecret, whatsApp.VerifyToken),
            new TakeBlipIntegrationDto(takeBlip.Enabled, takeBlip.AccountId, takeBlip.ApiKey, takeBlip.WebhookUrl),
            new ZenviaIntegrationDto(zenvia.Enabled, zenvia.ApiKey, zenvia.AccountId, zenvia.WebhookUrl)
        );
    }
}
