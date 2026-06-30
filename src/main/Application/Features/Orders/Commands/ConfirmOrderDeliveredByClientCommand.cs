using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public record ConfirmOrderDeliveredByClientCommand(Guid Id) : IRequest<OrderTrackingDto>;

public class ConfirmOrderDeliveredByClientHandler(IApplicationDbContext db)
    : IRequestHandler<ConfirmOrderDeliveredByClientCommand, OrderTrackingDto>
{
    public async Task<OrderTrackingDto> Handle(ConfirmOrderDeliveredByClientCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Additionals)
            .FirstOrDefaultAsync(o => o.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException("Pedido nao encontrado.");

        if (order.DeliveryStartedAt is null || order.Status is not (OrderStatus.EmEntrega or OrderStatus.EmAtraso))
        {
            throw new InvalidOperationException("O pedido so pode ser marcado como entregue depois que a loja indicar que saiu para entrega.");
        }

        order.Status = OrderStatus.Entregue;
        await db.SaveChangesAsync(ct);

        return order.ToTrackingDto();
    }
}
