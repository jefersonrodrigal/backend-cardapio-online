using Application.Common.Interfaces;
using Application.Common.Time;
using Application.Features.Orders.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public record GetDeliveryEstimateQuery(string Address, string? OrderType = "Entrega") : IRequest<DeliveryEstimateDto>;

public class GetDeliveryEstimateValidator : AbstractValidator<GetDeliveryEstimateQuery>
{
    public GetDeliveryEstimateValidator()
    {
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.OrderType).MaximumLength(50);
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

        return new DeliveryEstimateDto(
            estimate.PreparationMinutes,
            estimate.TravelMinutes,
            estimate.TotalMinutes,
            estimate.DistanceKm,
            AppTimeZone.FormatDateTime(estimate.EstimatedReadyAt),
            AppTimeZone.FormatDateTime(estimate.EstimatedDeliveryDeadlineAt));
    }
}
