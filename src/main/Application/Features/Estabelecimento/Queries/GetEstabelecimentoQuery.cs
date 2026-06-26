using Application.Common.Interfaces;
using Application.Features.Estabelecimento.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Estabelecimento.Queries;

public record GetEstabelecimentoQuery : IRequest<EstabelecimentoDto>;

public class GetEstabelecimentoHandler(IApplicationDbContext db)
    : IRequestHandler<GetEstabelecimentoQuery, EstabelecimentoDto>
{
    public async Task<EstabelecimentoDto> Handle(GetEstabelecimentoQuery request, CancellationToken ct)
    {
        var est = await db.Estabelecimentos.FirstOrDefaultAsync(ct)
            ?? new Domain.Entities.Estabelecimento
            {
                Name = "Cardapio Online",
                LogoUrl = string.Empty,
                Category = "hamburgueria",
                Address = string.Empty,
                Whatsapp = string.Empty,
                OpenTime = new TimeOnly(18, 0),
                CloseTime = new TimeOnly(22, 0)
            };

        return new EstabelecimentoDto(
            est.Name,
            est.LogoUrl,
            est.Category,
            est.Address,
            est.Whatsapp,
            est.OpenTime.ToString("HH:mm"),
            est.CloseTime.ToString("HH:mm"),
            est.DeliveryFee
        );
    }
}
