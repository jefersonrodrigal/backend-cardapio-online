using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public record GetOrderTrackingQuery(Guid Id) : IRequest<OrderTrackingDto>;

public class GetOrderTrackingHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrderTrackingQuery, OrderTrackingDto>
{
    public async Task<OrderTrackingDto> Handle(GetOrderTrackingQuery q, CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == q.Id, ct)
            ?? throw new KeyNotFoundException("Pedido nao encontrado.");

        return order.ToTrackingDto();
    }
}
