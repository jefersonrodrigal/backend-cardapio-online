namespace Domain.Entities;

public class AdditionalGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int MinSelections { get; set; } = 0;
    public int MaxSelections { get; set; } = 1;
    public int SortOrder { get; set; }
    public ICollection<AdditionalItem> Items { get; set; } = [];
}
