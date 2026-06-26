using Domain.Entities;

namespace Application.Features.Orders.Dtos;

public record OrderItemDto(Guid? ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);

public record OrderDto(
    Guid Id,
    string Number,
    string ClientName,
    string ClientPhone,
    string Address,
    decimal Total,
    decimal DeliveryFee,
    string Status,
    string Date,
    string CreatedAt,
    string Source,
    string? OrderType,
    string? Note,
    IReadOnlyList<OrderItemDto> Items
);

public static class OrderDtoMapper
{
    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.Number,
            order.ClientName,
            order.ClientPhone,
            order.Address,
            order.Total,
            order.DeliveryFee,
            order.Status.ToString(),
            order.Date.ToString("yyyy-MM-dd"),
            order.CreatedAt.ToString("dd/MM HH:mm"),
            order.Source.ToString(),
            order.OrderType,
            order.Note,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.Subtotal)).ToList()
        );
}
