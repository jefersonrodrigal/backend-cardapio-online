using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Application.Features.Orders.Commands;

public record CreateOrderItemRequest(Guid ProductId, int Quantity);

public record CreateOrderCommand(
    string ClientName,
    string ClientPhone,
    string Address,
    string Source,
    IReadOnlyList<CreateOrderItemRequest> Items,
    string? Note = null
) : IRequest<OrderDto>;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClientPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Phone == cmd.ClientPhone, ct);
        if (client is not null)
        {
            client.Name = cmd.ClientName;
        }

        var number = GenerateOrderNumber();

        var source = Enum.TryParse<OrderSource>(cmd.Source, true, out var src) ? src : OrderSource.Site;
        var requestedItems = cmd.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new CreateOrderItemRequest(g.Key, g.Sum(i => i.Quantity)))
            .ToList();
        var productIds = requestedItems.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => p.IsActive && productIds.Contains(p.Id))
            .ToListAsync(ct);
        var productsById = products.ToDictionary(p => p.Id);

        foreach (var item in requestedItems)
        {
            if (!productsById.ContainsKey(item.ProductId))
            {
                throw new ValidationException($"Produto '{item.ProductId}' nao encontrado.");
            }
        }

        var order = new Order
        {
            Number = number,
            ClientId = client?.Id,
            ClientName = cmd.ClientName,
            ClientPhone = cmd.ClientPhone,
            Address = cmd.Address,
            Source = source,
            Note = cmd.Note,
            Items = requestedItems.Select(i => new OrderItem
            {
                ProductId = productsById[i.ProductId].Id,
                ProductName = productsById[i.ProductId].Name,
                Quantity = i.Quantity,
                UnitPrice = productsById[i.ProductId].Price
            }).ToList()
        };

        order.Total = order.Items.Sum(i => i.Subtotal);

        foreach (var item in requestedItems)
        {
            var product = productsById[item.ProductId];

            if (!product.TrackInventory)
                continue;

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Estoque insuficiente para {product.Name}.");

            var balanceBefore = product.StockQuantity;
            product.StockQuantity -= item.Quantity;

            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type = InventoryMovementType.Venda,
                Quantity = item.Quantity,
                BalanceBefore = balanceBefore,
                BalanceAfter = product.StockQuantity,
                OrderId = order.Id,
                Reason = $"Pedido {order.Number}"
            });
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        return order.ToDto();
    }

    private static string GenerateOrderNumber()
    {
        Span<byte> randomBytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(randomBytes);
        var randomSuffix = Convert.ToHexString(randomBytes);
        return $"P{DateTime.UtcNow:yyMMddHHmm}{randomSuffix}";
    }
}
