namespace Application.Features.Orders.Dtos;

public record OrderItemDto(string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);

public record OrderDto(
    Guid Id,
    string Number,
    string ClientName,
    string ClientPhone,
    string Address,
    decimal Total,
    string Status,
    string Date,
    string CreatedAt,
    string Source,
    string? Note,
    IReadOnlyList<OrderItemDto> Items
);
