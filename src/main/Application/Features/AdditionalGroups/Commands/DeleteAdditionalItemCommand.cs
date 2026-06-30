using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Commands;

public record DeleteAdditionalItemCommand(Guid ItemId) : IRequest;

public class DeleteAdditionalItemHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteAdditionalItemCommand>
{
    public async Task Handle(DeleteAdditionalItemCommand cmd, CancellationToken ct)
    {
        var item = await db.AdditionalItems.FirstOrDefaultAsync(i => i.Id == cmd.ItemId, ct)
            ?? throw new ValidationException("Item adicional não encontrado.");

        db.AdditionalItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}
