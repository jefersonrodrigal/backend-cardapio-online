using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Commands;

public record DeleteAdditionalGroupCommand(Guid GroupId) : IRequest;

public class DeleteAdditionalGroupHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteAdditionalGroupCommand>
{
    public async Task Handle(DeleteAdditionalGroupCommand cmd, CancellationToken ct)
    {
        var group = await db.AdditionalGroups.FirstOrDefaultAsync(g => g.Id == cmd.GroupId, ct)
            ?? throw new ValidationException("Grupo de adicionais não encontrado.");

        db.AdditionalGroups.Remove(group);
        await db.SaveChangesAsync(ct);
    }
}
