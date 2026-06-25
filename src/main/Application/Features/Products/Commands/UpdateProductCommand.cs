using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string ImageUrl,
    bool TrackInventory = false,
    int StockQuantity = 0,
    int LowStockThreshold = 0
) : IRequest<ProductDto>;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Product {cmd.Id} not found.");

        var previousStockQuantity = product.StockQuantity;

        product.Name = cmd.Name;
        product.Description = cmd.Description;
        product.Price = cmd.Price;
        product.Category = cmd.Category.ToLowerInvariant();
        product.ImageUrl = cmd.ImageUrl;
        product.TrackInventory = cmd.TrackInventory;
        product.StockQuantity = cmd.StockQuantity;
        product.LowStockThreshold = cmd.LowStockThreshold;

        if (product.TrackInventory && previousStockQuantity != product.StockQuantity)
        {
            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                Type = InventoryMovementType.Ajuste,
                Quantity = Math.Abs(product.StockQuantity - previousStockQuantity),
                BalanceBefore = previousStockQuantity,
                BalanceAfter = product.StockQuantity,
                Reason = "Ajuste realizado no cadastro do produto"
            });
        }

        await db.SaveChangesAsync(ct);

        return ProductDto.FromProduct(product);
    }
}
