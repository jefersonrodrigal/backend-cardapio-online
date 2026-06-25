using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest;

public class DeleteCategoryHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand cmd, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == cmd.Id, ct)
            ?? throw new KeyNotFoundException($"Categoria {cmd.Id} nao encontrada.");

        var inUse = await db.Products.AnyAsync(p => p.Category == category.Slug && p.IsActive, ct);
        if (inUse)
            throw new InvalidOperationException("Nao e possivel excluir uma categoria que possui produtos ativos.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
}
