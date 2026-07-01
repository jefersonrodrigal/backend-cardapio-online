using Application.Common.Interfaces;
using Application.Common.Time;
using Application.Features.Orders.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public record GetDeliveryEstimateQuery(string Address, string? OrderType = "Entrega", string? Neighborhood = null)
    : IRequest<DeliveryEstimateDto>;

public class GetDeliveryEstimateValidator : AbstractValidator<GetDeliveryEstimateQuery>
{
    public GetDeliveryEstimateValidator()
    {
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OrderType).MaximumLength(50);
        RuleFor(x => x.Neighborhood).MaximumLength(150);
    }
}

public class GetDeliveryEstimateHandler(
    IApplicationDbContext db,
    IDeliveryEstimateService deliveryEstimateService)
    : IRequestHandler<GetDeliveryEstimateQuery, DeliveryEstimateDto>
{
    public async Task<DeliveryEstimateDto> Handle(GetDeliveryEstimateQuery query, CancellationToken ct)
    {
        var establishment = await db.Estabelecimentos.AsNoTracking().FirstOrDefaultAsync(ct);
        var estimate = deliveryEstimateService.Calculate(establishment, query.Address, query.OrderType, DateTime.UtcNow);

        var isDelivery = string.IsNullOrEmpty(query.OrderType) ||
            query.OrderType.Equals("entrega", StringComparison.OrdinalIgnoreCase);

        var deliveryFee = 0m;
        if (isDelivery)
        {
            deliveryFee = !string.IsNullOrWhiteSpace(query.Neighborhood)
                ? await db.NeighborhoodDeliveryFees
                    .Where(n => n.IsActive && n.Neighborhood == query.Neighborhood.Trim())
                    .Select(n => (decimal?)n.Fee)
                    .FirstOrDefaultAsync(ct) ?? 0
                : 0;
        }

        return new DeliveryEstimateDto(
            estimate.PreparationMinutes,
            estimate.TravelMinutes,
            estimate.TotalMinutes,
            estimate.DistanceKm,
            AppTimeZone.FormatDateTime(estimate.EstimatedReadyAt),
            AppTimeZone.FormatDateTime(estimate.EstimatedDeliveryDeadlineAt),
            deliveryFee);
    }
}
