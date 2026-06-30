namespace Domain.Entities;

public class OrderItemAdditional
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;
    public Guid? AdditionalItemId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}
