using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Inventory.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Queries;

public record GetInventoryMovementsQuery(Guid? ProductId = null, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedResult<InventoryMovementDto>>;

public class GetInventoryMovementsHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryMovementsQuery, PaginatedResult<InventoryMovementDto>>
{
    public async Task<PaginatedResult<InventoryMovementDto>> Handle(GetInventoryMovementsQuery q, CancellationToken ct)
    {
        var query = db.InventoryMovements.Include(m => m.Product).AsQueryable();

        if (q.ProductId.HasValue)
            query = query.Where(m => m.ProductId == q.ProductId.Value);

        var total = await query.CountAsync(ct);
        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        var items = movements.Select(InventoryMovementDto.FromMovement).ToList();

        return PaginatedResult<InventoryMovementDto>.Create(items, total, q.Page, q.PageSize);
    }
}
