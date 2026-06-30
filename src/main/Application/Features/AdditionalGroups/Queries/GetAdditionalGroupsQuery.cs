using Application.Common.Interfaces;
using Application.Features.AdditionalGroups.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Queries;

public record GetAdditionalGroupsQuery(Guid ProductId) : IRequest<IReadOnlyList<AdditionalGroupDto>>;

public class GetAdditionalGroupsHandler(IApplicationDbContext db)
    : IRequestHandler<GetAdditionalGroupsQuery, IReadOnlyList<AdditionalGroupDto>>
{
    public async Task<IReadOnlyList<AdditionalGroupDto>> Handle(GetAdditionalGroupsQuery q, CancellationToken ct)
    {
        var groups = await db.AdditionalGroups
            .Include(g => g.Items)
            .Where(g => g.ProductId == q.ProductId)
            .OrderBy(g => g.SortOrder)
            .ToListAsync(ct);

        return groups.Select(AdditionalGroupDto.FromGroup).ToList();
    }
}
