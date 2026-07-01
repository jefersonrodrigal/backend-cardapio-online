namespace Domain.Entities;

public class NeighborhoodDeliveryFee
{
    public int Id { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public bool IsActive { get; set; } = true;
}
