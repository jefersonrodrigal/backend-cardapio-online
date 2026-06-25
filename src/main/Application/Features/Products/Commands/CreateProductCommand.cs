using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Category,
    string ImageUrl,
    bool TrackInventory = false,
    int StockQuantity = 0,
    int LowStockThreshold = 0
) : IRequest<ProductDto>;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductHandler(IApplicationDbContext db)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = new Product
        {
            Name = cmd.Name,
            Description = cmd.Description,
            Price = cmd.Price,
            Category = cmd.Category.ToLowerInvariant(),
            ImageUrl = cmd.ImageUrl,
            TrackInventory = cmd.TrackInventory,
            StockQuantity = cmd.StockQuantity,
            LowStockThreshold = cmd.LowStockThreshold
        };

        db.Products.Add(product);

        if (product.TrackInventory && product.StockQuantity > 0)
        {
            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type = InventoryMovementType.Entrada,
                Quantity = product.StockQuantity,
                BalanceBefore = 0,
                BalanceAfter = product.StockQuantity,
                Reason = "Estoque inicial do produto"
            });
        }

        await db.SaveChangesAsync(ct);

        return ProductDto.FromProduct(product);
    }
}
