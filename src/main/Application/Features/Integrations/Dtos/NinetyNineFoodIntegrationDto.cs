namespace Application.Features.Integrations.Dtos;

public record NinetyNineFoodIntegrationDto(
    bool Enabled,
    string ClientId,
    string ClientSecret,
    string StoreId,
    string WebhookUrl
);
