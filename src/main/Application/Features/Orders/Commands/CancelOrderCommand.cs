using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands;

public record CancelOrderCommand(Guid Id) : IRequest<OrderDto>;

public class CancelOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CancelOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Order {cmd.Id} not found.");

        if (order.Status is OrderStatus.Entregue or OrderStatus.Cancelado)
            throw new InvalidOperationException($"Cannot cancel order with status {order.Status}.");

        order.Status = OrderStatus.Cancelado;
        await db.SaveChangesAsync(ct);

        return new OrderDto(
            order.Id, order.Number, order.ClientName, order.ClientPhone, order.Address,
            order.Total, order.Status.ToString(), order.Date.ToString("yyyy-MM-dd"),
            order.CreatedAt.ToString("dd/MM HH:mm"), order.Source.ToString(), order.Note,
            order.Items.Select(i => new OrderItemDto(i.ProductName, i.Quantity, i.UnitPrice, i.Subtotal)).ToList()
        );
    }
}
