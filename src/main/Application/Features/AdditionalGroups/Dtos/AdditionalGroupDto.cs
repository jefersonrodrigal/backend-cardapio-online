using Domain.Entities;

namespace Application.Features.AdditionalGroups.Dtos;

public record AdditionalItemDto(Guid Id, string Name, decimal Price, bool IsAvailable, int SortOrder);

public record AdditionalGroupDto(
    Guid Id,
    string Name,
    int MinSelections,
    int MaxSelections,
    int SortOrder,
    IReadOnlyList<AdditionalItemDto> Items
)
{
    public static AdditionalGroupDto FromGroup(AdditionalGroup g) =>
        new(
            g.Id,
            g.Name,
            g.MinSelections,
            g.MaxSelections,
            g.SortOrder,
            g.Items.OrderBy(i => i.SortOrder).Select(i => new AdditionalItemDto(i.Id, i.Name, i.Price, i.IsAvailable, i.SortOrder)).ToList()
        );
}
