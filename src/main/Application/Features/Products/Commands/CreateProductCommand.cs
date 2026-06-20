using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name, string Description, decimal Price, string Category, string ImageUrl
) : IRequest<ProductDto>;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty();
    }
}

public class CreateProductHandler(IApplicationDbContext db)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var category = Enum.TryParse<ProductCategory>(cmd.Category, true, out var cat)
            ? cat : ProductCategory.Outro;

        var product = new Product
        {
            Name = cmd.Name,
            Description = cmd.Description,
            Price = cmd.Price,
            Category = category,
            ImageUrl = cmd.ImageUrl
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return new ProductDto(product.Id, product.Name, product.Description,
            product.Price, product.Category.ToString().ToLower(), product.ImageUrl);
    }
}
