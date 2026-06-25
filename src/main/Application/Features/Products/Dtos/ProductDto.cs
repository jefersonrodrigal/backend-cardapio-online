using Application.Common.Inventory;
using Domain.Entities;

namespace Application.Features.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string ImageUrl,
    bool TrackInventory,
    int StockQuantity,
    int LowStockThreshold,
    bool IsAvailable,
    string StockStatus
)
{
    public static ProductDto FromProduct(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.ImageUrl,
            product.TrackInventory,
            product.StockQuantity,
            product.LowStockThreshold,
            ProductStockStatus.IsAvailable(product),
            ProductStockStatus.GetStatus(product));
}
