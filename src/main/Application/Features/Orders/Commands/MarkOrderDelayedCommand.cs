using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public record MarkOrderDelayedCommand(Guid Id) : IRequest<OrderDto>;

public class MarkOrderDelayedHandler(IApplicationDbContext db)
    : IRequestHandler<MarkOrderDelayedCommand, OrderDto>
{
    public async Task<OrderDto> Handle(MarkOrderDelayedCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Order {cmd.Id} not found.");

        if (order.Status is OrderStatus.Entregue or OrderStatus.Cancelado)
            throw new InvalidOperationException($"Cannot mark order with status {order.Status} as delayed.");

        order.Status = OrderStatus.EmAtraso;
        order.MarkedDelayedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return order.ToDto();
    }
}
