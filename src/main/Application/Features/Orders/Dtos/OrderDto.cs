using Application.Common.Time;
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
    int? EstimatedPreparationMinutes,
    int? EstimatedTravelMinutes,
    int? EstimatedDeliveryMinutes,
    string? EstimatedReadyAt,
    string? EstimatedDeliveryDeadlineAt,
    string? MarkedDelayedAt,
    bool CanClientConfirmDelivery,
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
    int? EstimatedPreparationMinutes,
    int? EstimatedTravelMinutes,
    int? EstimatedDeliveryMinutes,
    string? EstimatedReadyAt,
    string? EstimatedDeliveryDeadlineAt,
    string? MarkedDelayedAt,
    bool CanClientConfirmDelivery,
    IReadOnlyList<OrderItemDto> Items
);

public record DeliveryEstimateDto(
    int EstimatedPreparationMinutes,
    int EstimatedTravelMinutes,
    int EstimatedDeliveryMinutes,
    decimal EstimatedDeliveryDistanceKm,
    string EstimatedReadyAt,
    string EstimatedDeliveryDeadlineAt,
    decimal DeliveryFee
);

public static class OrderDtoMapper
{
    private static bool CanClientConfirmDelivery(Order order) =>
        order.DeliveryStartedAt.HasValue &&
        order.Status is Domain.Enums.OrderStatus.EmEntrega or Domain.Enums.OrderStatus.EmAtraso;

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
            AppTimeZone.FormatDateTime(order.CreatedAt),
            order.Source.ToString(),
            order.OrderType,
            order.Note,
            order.EstimatedPreparationMinutes,
            order.EstimatedTravelMinutes,
            order.EstimatedDeliveryMinutes,
            AppTimeZone.FormatDateTime(order.EstimatedReadyAt),
            AppTimeZone.FormatDateTime(order.EstimatedDeliveryDeadlineAt),
            AppTimeZone.FormatDateTime(order.MarkedDelayedAt),
            CanClientConfirmDelivery(order),
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
            AppTimeZone.FormatDateTime(order.CreatedAt),
            order.Source.ToString(),
            order.OrderType,
            order.Note,
            order.EstimatedPreparationMinutes,
            order.EstimatedTravelMinutes,
            order.EstimatedDeliveryMinutes,
            AppTimeZone.FormatDateTime(order.EstimatedReadyAt),
            AppTimeZone.FormatDateTime(order.EstimatedDeliveryDeadlineAt),
            AppTimeZone.FormatDateTime(order.MarkedDelayedAt),
            CanClientConfirmDelivery(order),
            order.Items.Select(ToItemDto).ToList()
        );
}
