using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Product {cmd.Id} not found.");

        product.IsActive = false; // soft delete
        await db.SaveChangesAsync(ct);
    }
}
