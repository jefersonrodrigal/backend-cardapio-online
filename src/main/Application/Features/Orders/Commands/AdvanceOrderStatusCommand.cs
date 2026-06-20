using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public record AdvanceOrderStatusCommand(Guid Id) : IRequest<OrderDto>;

public class AdvanceOrderStatusHandler(IApplicationDbContext db)
    : IRequestHandler<AdvanceOrderStatusCommand, OrderDto>
{
    private static readonly Dictionary<OrderStatus, OrderStatus> Flow = new()
    {
        [OrderStatus.Pendente]  = OrderStatus.EmPreparo,
        [OrderStatus.EmPreparo] = OrderStatus.EmEntrega,
        [OrderStatus.EmEntrega] = OrderStatus.Entregue,
    };

    public async Task<OrderDto> Handle(AdvanceOrderStatusCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Order {cmd.Id} not found.");

        if (!Flow.TryGetValue(order.Status, out var next))
            throw new InvalidOperationException($"Cannot advance order with status {order.Status}.");

        order.Status = next;
        await db.SaveChangesAsync(ct);

        return new OrderDto(
            order.Id, order.Number, order.ClientName, order.ClientPhone, order.Address,
            order.Total, order.Status.ToString(), order.Date.ToString("yyyy-MM-dd"),
            order.CreatedAt.ToString("dd/MM HH:mm"), order.Source.ToString(), order.Note,
            order.Items.Select(i => new OrderItemDto(i.ProductName, i.Quantity, i.UnitPrice, i.Subtotal)).ToList()
        );
    }
}
