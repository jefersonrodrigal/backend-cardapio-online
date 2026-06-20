namespace Domain.Entities;

public class Client
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Complement { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateOnly RegisteredAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public ICollection<Order> Orders { get; set; } = [];
}
