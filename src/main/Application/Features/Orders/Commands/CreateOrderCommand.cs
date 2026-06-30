using Application.Common.Interfaces;
using Application.Features.Orders.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Application.Features.Orders.Commands;

public record CreateOrderItemAdditionalRequest(Guid AdditionalItemId, int Quantity = 1);

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    IReadOnlyList<CreateOrderItemAdditionalRequest>? Additionals = null
);

public record CreateOrderCommand(
    string ClientName,
    string ClientPhone,
    string Address,
    string Source,
    IReadOnlyList<CreateOrderItemRequest> Items,
    string? Note = null,
    string? OrderType = null,
    string? TrackingBaseUrl = null
) : IRequest<OrderDto>;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClientPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TrackingBaseUrl).MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleForEach(i => i.Additionals).ChildRules(a =>
            {
                a.RuleFor(x => x.AdditionalItemId).NotEmpty();
                a.RuleFor(x => x.Quantity).GreaterThan(0);
            }).When(i => i.Additionals is { Count: > 0 });
        });
    }
}

public class CreateOrderHandler(
    IApplicationDbContext db,
    IOrderTrackingNotificationSender orderTrackingNotificationSender)
    : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var est = await db.Estabelecimentos.AsNoTracking().FirstOrDefaultAsync(ct);
        var isDelivery = string.IsNullOrEmpty(cmd.OrderType) ||
            cmd.OrderType.Equals("entrega", StringComparison.OrdinalIgnoreCase);
        var deliveryFee = isDelivery ? (est?.DeliveryFee ?? 0) : 0;

        var client = await db.Clients.FirstOrDefaultAsync(c => c.Phone == cmd.ClientPhone, ct);
        if (client is not null)
        {
            client.Name = cmd.ClientName;
        }

        var number = GenerateOrderNumber();

        var source = Enum.TryParse<OrderSource>(cmd.Source, true, out var src) ? src : OrderSource.Site;
        var requestedItems = cmd.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new CreateOrderItemRequest(
                g.Key,
                g.Sum(i => i.Quantity),
                g.SelectMany(i => i.Additionals ?? []).ToList()
            ))
            .ToList();

        var productIds = requestedItems.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => p.IsActive && productIds.Contains(p.Id))
            .ToListAsync(ct);
        var productsById = products.ToDictionary(p => p.Id);

        foreach (var item in requestedItems)
        {
            if (!productsById.ContainsKey(item.ProductId))
                throw new ValidationException($"Produto '{item.ProductId}' nao encontrado.");
        }

        // Load and validate additional items
        var allAdditionalItemIds = requestedItems
            .SelectMany(i => i.Additionals ?? [])
            .Select(a => a.AdditionalItemId)
            .Distinct()
            .ToList();

        var additionalItemsById = allAdditionalItemIds.Count > 0
            ? (await db.AdditionalItems
                .Include(i => i.Group)
                .Where(i => allAdditionalItemIds.Contains(i.Id))
                .ToListAsync(ct))
                .ToDictionary(i => i.Id)
            : [];

        foreach (var additionalId in allAdditionalItemIds)
        {
            if (!additionalItemsById.TryGetValue(additionalId, out var additionalItem))
                throw new ValidationException($"Item adicional '{additionalId}' não encontrado.");
            if (!additionalItem.IsAvailable)
                throw new ValidationException($"Item adicional '{additionalItem.Name}' não está disponível.");
        }

        // Validate group min/max per order item
        foreach (var item in requestedItems)
        {
            if (item.Additionals is not { Count: > 0 })
                continue;

            var selectedByGroup = item.Additionals
                .GroupBy(a => additionalItemsById[a.AdditionalItemId].GroupId)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Quantity));

            foreach (var (groupId, totalQty) in selectedByGroup)
            {
                var group = additionalItemsById.Values
                    .First(i => i.GroupId == groupId).Group;

                if (totalQty < group.MinSelections)
                    throw new ValidationException(
                        $"O grupo '{group.Name}' requer no mínimo {group.MinSelections} seleção(ões).");
                if (totalQty > group.MaxSelections)
                    throw new ValidationException(
                        $"O grupo '{group.Name}' permite no máximo {group.MaxSelections} seleção(ões).");
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
            OrderType = cmd.OrderType,
            DeliveryFee = deliveryFee,
            Note = cmd.Note,
            Items = requestedItems.Select(i =>
            {
                var additionals = (i.Additionals ?? [])
                    .Select(a =>
                    {
                        var additionalItem = additionalItemsById[a.AdditionalItemId];
                        return new OrderItemAdditional
                        {
                            AdditionalItemId = additionalItem.Id,
                            GroupName = additionalItem.Group.Name,
                            Name = additionalItem.Name,
                            Price = additionalItem.Price,
                            Quantity = a.Quantity
                        };
                    })
                    .ToList();

                var additionalsPrice = additionals.Sum(a => a.Price * a.Quantity);
                var product = productsById[i.ProductId];
                var unitPrice = product.IsOnPromotion && product.PromotionalPrice.HasValue
                    ? product.PromotionalPrice!.Value
                    : product.Price;

                return new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = unitPrice,
                    AdditionalsPrice = additionalsPrice,
                    Additionals = additionals
                };
            }).ToList()
        };

        order.Total = order.Items.Sum(i => i.Subtotal) + order.DeliveryFee;

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

        if (est?.SendOrderTrackingViaWhatsApp == true)
        {
            await orderTrackingNotificationSender.SendOrderTrackingLinkAsync(order, cmd.TrackingBaseUrl, ct);
        }

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
