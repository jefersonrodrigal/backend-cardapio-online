using Domain.Entities;

namespace Application.Common.Interfaces;

public record DeliveryEstimate(
    int PreparationMinutes,
    int TravelMinutes,
    int TotalMinutes,
    decimal DistanceKm,
    DateTime EstimatedReadyAt,
    DateTime EstimatedDeliveryDeadlineAt
);

public interface IDeliveryEstimateService
{
    DeliveryEstimate Calculate(
        Estabelecimento? establishment,
        string deliveryAddress,
        string? orderType,
        DateTime createdAtUtc);
}
