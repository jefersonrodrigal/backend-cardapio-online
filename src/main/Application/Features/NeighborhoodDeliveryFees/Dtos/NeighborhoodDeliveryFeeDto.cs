using Domain.Entities;

namespace Application.Features.NeighborhoodDeliveryFees.Dtos;

public record NeighborhoodDeliveryFeeDto(int Id, string Neighborhood, decimal Fee, bool IsActive)
{
    public static NeighborhoodDeliveryFeeDto From(NeighborhoodDeliveryFee n) =>
        new(n.Id, n.Neighborhood, n.Fee, n.IsActive);
}
