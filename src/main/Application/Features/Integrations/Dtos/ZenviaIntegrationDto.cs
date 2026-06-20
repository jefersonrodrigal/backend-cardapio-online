namespace Application.Features.Integrations.Dtos;

public record ZenviaIntegrationDto(
    bool Enabled,
    string ApiToken,
    string ChannelId,
    string WebhookUrl
);
