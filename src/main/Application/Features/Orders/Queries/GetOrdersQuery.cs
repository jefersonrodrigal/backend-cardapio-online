using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Orders.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries;

public record GetOrdersQuery(int Page = 1, int PageSize = 5, DateOnly? Date = null)
    : IRequest<PaginatedResult<OrderDto>>;

public class GetOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery q, CancellationToken ct)
    {
        var query = db.Orders.Include(o => o.Items).AsQueryable();

        if (q.Date.HasValue)
            query = query.Where(o => o.Date == q.Date.Value);

        var total = await query.CountAsync(ct);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        var dtos = orders.Select(o => new OrderDto(
            o.Id, o.Number, o.ClientName, o.ClientPhone, o.Address, o.Total,
            o.Status.ToString(), o.Date.ToString("yyyy-MM-dd"),
            o.CreatedAt.ToString("dd/MM HH:mm"), o.Source.ToString(), o.Note,
            o.Items.Select(i => new OrderItemDto(i.ProductName, i.Quantity, i.UnitPrice, i.Subtotal)).ToList()
        )).ToList();

        return PaginatedResult<OrderDto>.Create(dtos, total, q.Page, q.PageSize);
    }
}
