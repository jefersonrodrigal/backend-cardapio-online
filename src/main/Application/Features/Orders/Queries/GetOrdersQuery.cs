using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Orders.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 5,
    DateOnly? Date = null,
    string? Search = null,
    bool ActiveOnly = false)
    : IRequest<PaginatedResult<OrderDto>>;

public class GetOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery q, CancellationToken ct)
    {
        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 1000);
        var query = db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Additionals)
            .AsQueryable();

        if (q.Date.HasValue)
        {
            query = query.Where(o => o.Date == q.Date.Value);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim();
            query = query.Where(o => o.ClientName.Contains(search));
        }

        if (q.ActiveOnly)
        {
            query = query.Where(o => o.Status != OrderStatus.Entregue && o.Status != OrderStatus.Cancelado);
        }

        var total = await query.CountAsync(ct);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = orders.Select(o => o.ToDto()).ToList();

        return PaginatedResult<OrderDto>.Create(dtos, total, page, pageSize);
    }
}
