using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Products.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries;

public record GetProductsQuery(int Page = 1, int PageSize = 5, string? Category = null)
    : IRequest<PaginatedResult<ProductDto>>;

public class GetProductsHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery q, CancellationToken ct)
    {
        var query = db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(q.Category))
        {
            var slug = q.Category.ToLowerInvariant();
            query = query.Where(p => p.Category == slug);
        }

        var total = await query.CountAsync(ct);
        var products = await query
            .Include(p => p.AdditionalGroups)
                .ThenInclude(g => g.Items)
            .OrderBy(p => p.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        var items = products.Select(ProductDto.FromProduct).ToList();

        return PaginatedResult<ProductDto>.Create(items, total, q.Page, q.PageSize);
    }
}
