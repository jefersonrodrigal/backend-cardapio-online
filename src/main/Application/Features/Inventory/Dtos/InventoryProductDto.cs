using Application.Common.Inventory;
using Domain.Entities;

namespace Application.Features.Inventory.Dtos;

public record InventoryProductDto(
    Guid Id,
    string Name,
    string Category,
    string ImageUrl,
    bool TrackInventory,
    int StockQuantity,
    int LowStockThreshold,
    bool IsAvailable,
    string StockStatus
)
{
    public static InventoryProductDto FromProduct(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Category.ToString().ToLowerInvariant(),
            product.ImageUrl,
            product.TrackInventory,
            product.StockQuantity,
            product.LowStockThreshold,
            ProductStockStatus.IsAvailable(product),
            ProductStockStatus.GetStatus(product));
}
