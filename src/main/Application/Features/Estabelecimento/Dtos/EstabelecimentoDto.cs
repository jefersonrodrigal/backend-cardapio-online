namespace Application.Features.Estabelecimento.Dtos;

public record EstabelecimentoDto(
    string Name,
    string LogoUrl,
    string Category,
    string Address,
    string Whatsapp,
    string OpenTime,
    string CloseTime
);
