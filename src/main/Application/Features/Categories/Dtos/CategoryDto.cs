using Domain.Entities;

namespace Application.Features.Categories.Dtos;

public record CategoryDto(int Id, string Slug, string Name, int SortOrder)
{
    public static CategoryDto FromCategory(Category c) => new(c.Id, c.Slug, c.Name, c.SortOrder);
}
