using Domain.Entities;

namespace Application.Features.Inventory.Dtos;

public record InventoryMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Type,
    int Quantity,
    int BalanceBefore,
    int BalanceAfter,
    string Reason,
    Guid? OrderId,
    string CreatedAt
)
{
    public static InventoryMovementDto FromMovement(InventoryMovement movement) =>
        new(
            movement.Id,
            movement.ProductId,
            movement.Product.Name,
            movement.Type.ToString().ToLowerInvariant(),
            movement.Quantity,
            movement.BalanceBefore,
            movement.BalanceAfter,
            movement.Reason,
            movement.OrderId,
            movement.CreatedAt.ToString("dd/MM HH:mm"));
}
