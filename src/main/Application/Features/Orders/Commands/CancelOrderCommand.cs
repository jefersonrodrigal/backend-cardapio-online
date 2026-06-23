using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Entities;
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

        var productIds = order.Items
            .Where(i => i.ProductId.HasValue)
            .Select(i => i.ProductId!.Value)
            .Distinct()
            .ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var item in order.Items)
        {
            if (!item.ProductId.HasValue || !products.TryGetValue(item.ProductId.Value, out var product))
                continue;

            if (!product.TrackInventory)
                continue;

            var balanceBefore = product.StockQuantity;
            product.StockQuantity += item.Quantity;

            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type = InventoryMovementType.Cancelamento,
                Quantity = item.Quantity,
                BalanceBefore = balanceBefore,
                BalanceAfter = product.StockQuantity,
                OrderId = order.Id,
                Reason = $"Cancelamento do pedido {order.Number}"
            });
        }

        order.Status = OrderStatus.Cancelado;
        await db.SaveChangesAsync(ct);

        return order.ToDto();
    }
}
