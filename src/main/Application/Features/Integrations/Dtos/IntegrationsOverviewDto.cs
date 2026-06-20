namespace Application.Features.Integrations.Dtos;

public record IntegrationsOverviewDto(
    IFoodIntegrationDto IFood,
    AnotaiIntegrationDto Anotai,
    UberEatsIntegrationDto UberEats,
    NinetyNineFoodIntegrationDto NinetyNineFood,
    AiAgentsIntegrationDto AiAgents,
    WhatsAppIntegrationDto WhatsApp,
    TakeBlipIntegrationDto TakeBlip,
    ZenviaIntegrationDto Zenvia
);
