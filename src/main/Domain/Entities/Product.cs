namespace Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsOnPromotion { get; set; }
    public decimal? PromotionalPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackInventory { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = [];
}
