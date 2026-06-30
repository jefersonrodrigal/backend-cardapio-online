using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Common.Delivery;

public partial class DefaultDeliveryEstimateService : IDeliveryEstimateService
{
    private const int DefaultPreparationMinutes = 30;
    private const int DefaultDeliverySafetyMarginMinutes = 10;
    private const decimal DefaultDistanceKm = 6m;

    public DeliveryEstimate Calculate(
        Estabelecimento? establishment,
        string deliveryAddress,
        string? orderType,
        DateTime createdAtUtc)
    {
        var preparationMinutes = Clamp(
            establishment?.PreparationTimeMinutes ?? DefaultPreparationMinutes,
            5,
            240);

        var isDelivery = IsDelivery(orderType);
        var distanceKm = isDelivery
            ? EstimateDistanceKm(establishment?.Address, deliveryAddress)
            : 0m;

        var travelMinutes = isDelivery
            ? CalculateTravelMinutes(establishment, distanceKm)
            : 0;

        var totalMinutes = preparationMinutes + travelMinutes;

        return new DeliveryEstimate(
            preparationMinutes,
            travelMinutes,
            totalMinutes,
            distanceKm,
            createdAtUtc.AddMinutes(preparationMinutes),
            createdAtUtc.AddMinutes(totalMinutes));
    }

    private static int CalculateTravelMinutes(Estabelecimento? establishment, decimal distanceKm)
    {
        var safetyMarginMinutes = Clamp(
            establishment?.DeliverySafetyMarginMinutes ?? DefaultDeliverySafetyMarginMinutes,
            0,
            90);

        var averageSpeedKmH = CalculateAverageDeliverySpeedKmH(distanceKm);
        var travelMinutes = (int)Math.Ceiling((double)distanceKm / averageSpeedKmH * 60);
        return Math.Max(5, travelMinutes) + safetyMarginMinutes;
    }

    private static int CalculateAverageDeliverySpeedKmH(decimal distanceKm) =>
        distanceKm switch
        {
            <= 3m => 18,
            <= 8m => 24,
            <= 15m => 32,
            _ => 40
        };

    private static decimal EstimateDistanceKm(string? establishmentAddress, string deliveryAddress)
    {
        var originZipCode = ExtractZipCode(establishmentAddress);
        var destinationZipCode = ExtractZipCode(deliveryAddress);

        if (string.IsNullOrEmpty(destinationZipCode))
        {
            return DefaultDistanceKm;
        }

        if (string.IsNullOrEmpty(originZipCode))
        {
            return DefaultDistanceKm;
        }

        if (originZipCode == destinationZipCode)
        {
            return 1.5m;
        }

        var commonPrefixLength = originZipCode
            .Zip(destinationZipCode, (origin, destination) => origin == destination)
            .TakeWhile(isEqual => isEqual)
            .Count();

        return commonPrefixLength switch
        {
            >= 5 => 3m,
            4 => 5m,
            3 => 8m,
            2 => 12m,
            1 => 18m,
            _ => 25m
        };
    }

    private static string ExtractZipCode(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        var match = ZipCodePattern().Match(address);
        return match.Success ? Regex.Replace(match.Value, @"\D", string.Empty) : string.Empty;
    }

    private static bool IsDelivery(string? orderType) =>
        string.IsNullOrWhiteSpace(orderType) ||
        orderType.Equals("Entrega", StringComparison.OrdinalIgnoreCase) ||
        orderType.Equals("entrega", StringComparison.OrdinalIgnoreCase);

    private static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

    [GeneratedRegex(@"\b\d{5}-?\d{3}\b")]
    private static partial Regex ZipCodePattern();
}
