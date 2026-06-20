namespace Application.Features.Integrations.Dtos;

public record TakeBlipIntegrationDto(
    bool Enabled,
    string BotShortName,
    string AuthorizationKey,
    string WebhookUrl
);
