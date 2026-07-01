using Application.Common.Interfaces;
using Application.Features.NeighborhoodDeliveryFees.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.NeighborhoodDeliveryFees.Commands;

public record CreateNeighborhoodDeliveryFeeCommand(string Neighborhood, decimal Fee, bool IsActive = true)
    : IRequest<NeighborhoodDeliveryFeeDto>;

public class CreateNeighborhoodDeliveryFeeValidator : AbstractValidator<CreateNeighborhoodDeliveryFeeCommand>
{
    public CreateNeighborhoodDeliveryFeeValidator()
    {
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Fee).GreaterThanOrEqualTo(0);
    }
}

public class CreateNeighborhoodDeliveryFeeHandler(IApplicationDbContext db)
    : IRequestHandler<CreateNeighborhoodDeliveryFeeCommand, NeighborhoodDeliveryFeeDto>
{
    public async Task<NeighborhoodDeliveryFeeDto> Handle(CreateNeighborhoodDeliveryFeeCommand cmd, CancellationToken ct)
    {
        var normalized = cmd.Neighborhood.Trim();

        if (await db.NeighborhoodDeliveryFees.AnyAsync(n => n.Neighborhood == normalized, ct))
            throw new InvalidOperationException($"Ja existe uma taxa configurada para o bairro '{normalized}'.");

        var entity = new NeighborhoodDeliveryFee
        {
            Neighborhood = normalized,
            Fee = cmd.Fee,
            IsActive = cmd.IsActive,
        };

        db.NeighborhoodDeliveryFees.Add(entity);
        await db.SaveChangesAsync(ct);

        return NeighborhoodDeliveryFeeDto.From(entity);
    }
}
