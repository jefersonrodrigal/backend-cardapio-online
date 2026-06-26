using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pendente;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderSource Source { get; set; }
    public string? OrderType { get; set; }
    public decimal DeliveryFee { get; set; }
    public string? Note { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
}
