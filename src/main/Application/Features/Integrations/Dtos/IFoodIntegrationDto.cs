namespace Application.Features.Integrations.Dtos;

public record IFoodIntegrationDto(
    bool Enabled,
    string ClientId,
    string ClientSecret,
    string MerchantId
);
