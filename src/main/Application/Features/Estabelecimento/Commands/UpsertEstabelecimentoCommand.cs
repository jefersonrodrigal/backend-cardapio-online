using Application.Common.Interfaces;
using Application.Features.Estabelecimento.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Estabelecimento.Commands;

public record UpsertEstabelecimentoCommand(
    string Name,
    string LogoUrl,
    string Category,
    string Address,
    string Whatsapp,
    string OpenTime,
    string CloseTime,
    decimal DeliveryFee
) : IRequest<EstabelecimentoDto>;

public class UpsertEstabelecimentoValidator : AbstractValidator<UpsertEstabelecimentoCommand>
{
    public UpsertEstabelecimentoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Whatsapp).NotEmpty().MaximumLength(20);
        RuleFor(x => x.OpenTime).Matches(@"^\d{2}:\d{2}$");
        RuleFor(x => x.CloseTime).Matches(@"^\d{2}:\d{2}$");
    }
}

public class UpsertEstabelecimentoHandler(IApplicationDbContext db)
    : IRequestHandler<UpsertEstabelecimentoCommand, EstabelecimentoDto>
{
    public async Task<EstabelecimentoDto> Handle(UpsertEstabelecimentoCommand cmd, CancellationToken ct)
    {
        var est = await db.Estabelecimentos.FirstOrDefaultAsync(ct);

        if (est is null)
        {
            est = new Domain.Entities.Estabelecimento();
            db.Estabelecimentos.Add(est);
        }

        est.Name = cmd.Name;
        est.LogoUrl = cmd.LogoUrl;
        est.Category = cmd.Category;
        est.Address = cmd.Address;
        est.Whatsapp = cmd.Whatsapp;
        est.OpenTime = TimeOnly.Parse(cmd.OpenTime);
        est.CloseTime = TimeOnly.Parse(cmd.CloseTime);
        est.DeliveryFee = cmd.DeliveryFee >= 0 ? cmd.DeliveryFee : 0;
        est.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return new EstabelecimentoDto(
            est.Name,
            est.LogoUrl,
            est.Category,
            est.Address,
            est.Whatsapp,
            est.OpenTime.ToString("HH:mm"),
            est.CloseTime.ToString("HH:mm"),
            est.DeliveryFee
        );
    }
}
