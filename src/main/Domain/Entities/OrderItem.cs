namespace Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AdditionalsPrice { get; set; }
    public decimal Subtotal => Quantity * (UnitPrice + AdditionalsPrice);
    public ICollection<OrderItemAdditional> Additionals { get; set; } = [];
}
