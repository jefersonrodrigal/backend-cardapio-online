namespace Application.Features.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string ImageUrl
);
