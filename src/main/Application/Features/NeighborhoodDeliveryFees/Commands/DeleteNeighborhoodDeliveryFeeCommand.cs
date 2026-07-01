using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.NeighborhoodDeliveryFees.Commands;

public record DeleteNeighborhoodDeliveryFeeCommand(int Id) : IRequest;

public class DeleteNeighborhoodDeliveryFeeHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteNeighborhoodDeliveryFeeCommand>
{
    public async Task Handle(DeleteNeighborhoodDeliveryFeeCommand cmd, CancellationToken ct)
    {
        var entity = await db.NeighborhoodDeliveryFees.FirstOrDefaultAsync(n => n.Id == cmd.Id, ct)
            ?? throw new InvalidOperationException($"Taxa de bairro com id {cmd.Id} nao encontrada.");

        db.NeighborhoodDeliveryFees.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
