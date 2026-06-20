namespace Application.Features.Integrations.Dtos;

public record WhatsAppIntegrationDto(
    bool Enabled,
    string PhoneNumberId,
    string BusinessAccountId,
    string AccessToken,
    string AppSecret,
    string VerifyToken
);
