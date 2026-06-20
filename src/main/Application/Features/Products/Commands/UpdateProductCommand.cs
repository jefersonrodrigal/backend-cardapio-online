using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public record UpdateProductCommand(
    Guid Id, string Name, string Description, decimal Price, string Category, string ImageUrl
) : IRequest<ProductDto>;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public class UpdateProductHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Product {cmd.Id} not found.");

        var category = Enum.TryParse<ProductCategory>(cmd.Category, true, out var cat)
            ? cat : product.Category;

        product.Name = cmd.Name;
        product.Description = cmd.Description;
        product.Price = cmd.Price;
        product.Category = category;
        product.ImageUrl = cmd.ImageUrl;

        await db.SaveChangesAsync(ct);

        return new ProductDto(product.Id, product.Name, product.Description,
            product.Price, product.Category.ToString().ToLower(), product.ImageUrl);
    }
}
