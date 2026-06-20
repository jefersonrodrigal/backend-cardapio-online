namespace Application.Features.Integrations.Dtos;

public record UberEatsIntegrationDto(
    bool Enabled,
    string ClientId,
    string ClientSecret,
    string StoreId,
    string WebhookSigningSecret
);
