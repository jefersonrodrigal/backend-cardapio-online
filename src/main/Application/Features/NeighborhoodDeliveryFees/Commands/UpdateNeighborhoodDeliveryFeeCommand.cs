using Application.Common.Interfaces;
using Application.Features.NeighborhoodDeliveryFees.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.NeighborhoodDeliveryFees.Commands;

public record UpdateNeighborhoodDeliveryFeeCommand(int Id, string Neighborhood, decimal Fee, bool IsActive)
    : IRequest<NeighborhoodDeliveryFeeDto>;

public class UpdateNeighborhoodDeliveryFeeValidator : AbstractValidator<UpdateNeighborhoodDeliveryFeeCommand>
{
    public UpdateNeighborhoodDeliveryFeeValidator()
    {
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Fee).GreaterThanOrEqualTo(0);
    }
}

public class UpdateNeighborhoodDeliveryFeeHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateNeighborhoodDeliveryFeeCommand, NeighborhoodDeliveryFeeDto>
{
    public async Task<NeighborhoodDeliveryFeeDto> Handle(UpdateNeighborhoodDeliveryFeeCommand cmd, CancellationToken ct)
    {
        var entity = await db.NeighborhoodDeliveryFees.FirstOrDefaultAsync(n => n.Id == cmd.Id, ct)
            ?? throw new InvalidOperationException($"Taxa de bairro com id {cmd.Id} nao encontrada.");

        var normalized = cmd.Neighborhood.Trim();

        if (await db.NeighborhoodDeliveryFees.AnyAsync(n => n.Neighborhood == normalized && n.Id != cmd.Id, ct))
            throw new InvalidOperationException($"Ja existe uma taxa configurada para o bairro '{normalized}'.");

        entity.Neighborhood = normalized;
        entity.Fee = cmd.Fee;
        entity.IsActive = cmd.IsActive;

        await db.SaveChangesAsync(ct);

        return NeighborhoodDeliveryFeeDto.From(entity);
    }
}
