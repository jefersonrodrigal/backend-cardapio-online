using Domain.Entities;

namespace Application.Common.Inventory;

public static class ProductStockStatus
{
    public static bool IsAvailable(Product product) =>
        !product.TrackInventory || product.StockQuantity > 0;

    public static string GetStatus(Product product)
    {
        if (!product.TrackInventory)
            return "untracked";

        if (product.StockQuantity <= 0)
            return "out";

        if (product.LowStockThreshold > 0 && product.StockQuantity <= product.LowStockThreshold)
            return "low";

        return "available";
    }
}
