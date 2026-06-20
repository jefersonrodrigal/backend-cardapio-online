namespace Application.Features.Integrations.Dtos;

public record AiAgentsIntegrationDto(
    bool Enabled,
    string Provider,
    string ApiKey,
    string Model,
    string AssistantId,
    string WebhookUrl
);
