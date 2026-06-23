using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Inventory.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Queries;

public record GetInventoryQuery(int Page = 1, int PageSize = 20, string? Status = null, string? Search = null)
    : IRequest<PaginatedResult<InventoryProductDto>>;

public class GetInventoryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryQuery, PaginatedResult<InventoryProductDto>>
{
    public async Task<PaginatedResult<InventoryProductDto>> Handle(GetInventoryQuery q, CancellationToken ct)
    {
        var query = db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim();
            query = query.Where(p => p.Name.Contains(search));
        }

        query = q.Status?.Trim().ToLowerInvariant() switch
        {
            "tracked" => query.Where(p => p.TrackInventory),
            "untracked" => query.Where(p => !p.TrackInventory),
            "out" => query.Where(p => p.TrackInventory && p.StockQuantity <= 0),
            "low" => query.Where(p =>
                p.TrackInventory &&
                p.StockQuantity > 0 &&
                p.LowStockThreshold > 0 &&
                p.StockQuantity <= p.LowStockThreshold),
            "available" => query.Where(p => !p.TrackInventory || p.StockQuantity > 0),
            _ => query
        };

        var total = await query.CountAsync(ct);
        var products = await query
            .OrderBy(p => p.TrackInventory ? p.StockQuantity : int.MaxValue)
            .ThenBy(p => p.Name)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        var items = products.Select(InventoryProductDto.FromProduct).ToList();

        return PaginatedResult<InventoryProductDto>.Create(items, total, q.Page, q.PageSize);
    }
}
