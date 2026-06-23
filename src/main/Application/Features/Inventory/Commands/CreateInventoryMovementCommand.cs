using Application.Common.Interfaces;
using Application.Features.Inventory.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public record CreateInventoryMovementCommand(
    Guid ProductId,
    string Type,
    int Quantity = 0,
    int? NewQuantity = null,
    string Reason = ""
) : IRequest<InventoryProductDto>;

public class CreateInventoryMovementValidator : AbstractValidator<CreateInventoryMovementCommand>
{
    public CreateInventoryMovementValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => !IsAdjustment(x.Type));
        RuleFor(x => x.NewQuantity).NotNull().GreaterThanOrEqualTo(0).When(x => IsAdjustment(x.Type));
        RuleFor(x => x.Reason).MaximumLength(300);
    }

    private static bool IsAdjustment(string type) =>
        string.Equals(type, "ajuste", StringComparison.OrdinalIgnoreCase);
}

public class CreateInventoryMovementHandler(IApplicationDbContext db)
    : IRequestHandler<CreateInventoryMovementCommand, InventoryProductDto>
{
    public async Task<InventoryProductDto> Handle(CreateInventoryMovementCommand cmd, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == cmd.ProductId && p.IsActive, ct)
            ?? throw new KeyNotFoundException($"Product {cmd.ProductId} not found.");

        if (!product.TrackInventory)
            throw new InvalidOperationException("Ative o controle de estoque do produto antes de registrar movimentacoes.");

        var type = ParseType(cmd.Type);
        if (type is InventoryMovementType.Venda or InventoryMovementType.Cancelamento)
            throw new InvalidOperationException("Movimentacoes de venda e cancelamento sao geradas pelos pedidos.");

        var balanceBefore = product.StockQuantity;
        var balanceAfter = type switch
        {
            InventoryMovementType.Entrada => balanceBefore + cmd.Quantity,
            InventoryMovementType.Perda => balanceBefore - cmd.Quantity,
            InventoryMovementType.Ajuste => cmd.NewQuantity!.Value,
            _ => balanceBefore
        };

        if (balanceAfter < 0)
            throw new InvalidOperationException($"Estoque insuficiente para registrar {type.ToString().ToLowerInvariant()} de {product.Name}.");

        var movementQuantity = type == InventoryMovementType.Ajuste
            ? Math.Abs(balanceAfter - balanceBefore)
            : cmd.Quantity;

        if (movementQuantity <= 0)
            throw new InvalidOperationException("A movimentacao precisa alterar a quantidade em estoque.");

        product.StockQuantity = balanceAfter;

        db.InventoryMovements.Add(new InventoryMovement
        {
            ProductId = product.Id,
            Type = type,
            Quantity = movementQuantity,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Reason = string.IsNullOrWhiteSpace(cmd.Reason)
                ? GetDefaultReason(type)
                : cmd.Reason.Trim()
        });

        await db.SaveChangesAsync(ct);

        return InventoryProductDto.FromProduct(product);
    }

    private static InventoryMovementType ParseType(string type)
    {
        var normalized = type.Trim();

        if (string.Equals(normalized, "entrada", StringComparison.OrdinalIgnoreCase))
            return InventoryMovementType.Entrada;

        if (string.Equals(normalized, "perda", StringComparison.OrdinalIgnoreCase))
            return InventoryMovementType.Perda;

        if (string.Equals(normalized, "ajuste", StringComparison.OrdinalIgnoreCase))
            return InventoryMovementType.Ajuste;

        throw new InvalidOperationException($"Tipo de movimentacao '{type}' invalido.");
    }

    private static string GetDefaultReason(InventoryMovementType type) =>
        type switch
        {
            InventoryMovementType.Entrada => "Entrada manual de estoque",
            InventoryMovementType.Perda => "Perda manual de estoque",
            InventoryMovementType.Ajuste => "Ajuste manual de estoque",
            _ => "Movimentacao manual de estoque"
        };
}
