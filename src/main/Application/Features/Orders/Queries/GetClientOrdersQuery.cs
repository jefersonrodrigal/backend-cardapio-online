using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Orders.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public record GetClientOrdersQuery(Guid ClientId, int Page = 1, int PageSize = 10)
    : IRequest<PaginatedResult<OrderDto>>;

public class GetClientOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetClientOrdersQuery, PaginatedResult<OrderDto>>
{
    public async Task<PaginatedResult<OrderDto>> Handle(GetClientOrdersQuery q, CancellationToken ct)
    {
        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 50);
        var query = db.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Additionals)
            .Where(o => o.ClientId == q.ClientId);

        var total = await query.CountAsync(ct);
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PaginatedResult<OrderDto>.Create(
            orders.Select(o => o.ToDto()).ToList(),
            total,
            page,
            pageSize
        );
    }
}
