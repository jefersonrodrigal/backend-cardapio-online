using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Clients.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Clients.Queries;

public record GetClientsQuery(int Page = 1, int PageSize = 5, string? Search = null)
    : IRequest<PaginatedResult<ClientDto>>;

public class GetClientsHandler(IApplicationDbContext db)
    : IRequestHandler<GetClientsQuery, PaginatedResult<ClientDto>>
{
    public async Task<PaginatedResult<ClientDto>> Handle(GetClientsQuery q, CancellationToken ct)
    {
        var query = db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                c.Phone.Contains(search));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(c => new
            {
                c.Id, c.Name, c.Email, c.Phone, c.ZipCode, c.Street, c.Number, c.Neighborhood, c.City, c.State, c.Complement, c.RegisteredAt,
                OrdersCount = c.Orders.Count(o => o.Status != OrderStatus.Cancelado),
                TotalSpent = c.Orders
                    .Where(o => o.Status == OrderStatus.Entregue)
                    .Sum(o => o.Total)
            })
            .ToListAsync(ct);

        var dtos = items.Select(c => new ClientDto(
            c.Id, c.Name, c.Email, c.Phone, c.ZipCode, c.Street, c.Number, c.Neighborhood, c.City, c.State, c.Complement,
            ClientAddressFormatter.Format(c.Street, c.Number, c.Neighborhood, c.City, c.State, c.ZipCode, c.Complement),
            c.RegisteredAt.ToString("dd/MM/yyyy"),
            c.OrdersCount, c.TotalSpent
        )).ToList();

        return PaginatedResult<ClientDto>.Create(dtos, total, q.Page, q.PageSize);
    }
}
