using Domain.Entities;

namespace Application.Features.Orders.Dtos;

public record OrderItemAdditionalDto(string GroupName, string Name, decimal Price, int Quantity);

public record OrderItemDto(
    Guid? ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal AdditionalsPrice,
    decimal Subtotal,
    IReadOnlyList<OrderItemAdditionalDto> Additionals
);

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

public record OrderTrackingDto(
    Guid Id,
    string Number,
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
    private static OrderItemDto ToItemDto(OrderItem i) =>
        new(
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.AdditionalsPrice,
            i.Subtotal,
            i.Additionals.Select(a => new OrderItemAdditionalDto(a.GroupName, a.Name, a.Price, a.Quantity)).ToList()
        );

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
            order.Items.Select(ToItemDto).ToList()
        );

    public static OrderTrackingDto ToTrackingDto(this Order order) =>
        new(
            order.Id,
            order.Number,
            order.Address,
            order.Total,
            order.DeliveryFee,
            order.Status.ToString(),
            order.Date.ToString("yyyy-MM-dd"),
            order.CreatedAt.ToString("dd/MM HH:mm"),
            order.Source.ToString(),
            order.OrderType,
            order.Note,
            order.Items.Select(ToItemDto).ToList()
        );
}
