namespace Application.Features.Estabelecimento.Dtos;

public record EstabelecimentoDto(
    string Name,
    string LogoUrl,
    string Category,
    string Address,
    string Whatsapp,
    string OpenTime,
    string CloseTime,
    decimal DeliveryFee,
    bool SendOrderTrackingViaWhatsApp,
    string? InstagramUrl,
    string? FacebookUrl,
    string? TikTokUrl,
    string? TwitterUrl
);
