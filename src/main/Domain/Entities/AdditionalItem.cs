namespace Domain.Entities;

public class AdditionalItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public AdditionalGroup Group { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int SortOrder { get; set; }
}
