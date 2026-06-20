namespace Application.Features.Integrations.Dtos;

public record AnotaiIntegrationDto(
    bool Enabled,
    string ApiToken,
    string AccountId,
    string WebhookUrl
);
