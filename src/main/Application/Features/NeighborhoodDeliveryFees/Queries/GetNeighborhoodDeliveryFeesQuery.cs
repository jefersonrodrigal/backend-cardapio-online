using Application.Common.Interfaces;
using Application.Features.NeighborhoodDeliveryFees.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.NeighborhoodDeliveryFees.Queries;

public record GetNeighborhoodDeliveryFeesQuery : IRequest<List<NeighborhoodDeliveryFeeDto>>;

public class GetNeighborhoodDeliveryFeesHandler(IApplicationDbContext db)
    : IRequestHandler<GetNeighborhoodDeliveryFeesQuery, List<NeighborhoodDeliveryFeeDto>>
{
    public async Task<List<NeighborhoodDeliveryFeeDto>> Handle(GetNeighborhoodDeliveryFeesQuery q, CancellationToken ct)
    {
        return await db.NeighborhoodDeliveryFees
            .OrderBy(n => n.Neighborhood)
            .Select(n => NeighborhoodDeliveryFeeDto.From(n))
            .ToListAsync(ct);
    }
}
