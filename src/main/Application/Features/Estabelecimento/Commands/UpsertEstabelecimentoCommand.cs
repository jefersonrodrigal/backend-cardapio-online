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
    bool SendOrderTrackingViaWhatsApp,
    int PreparationTimeMinutes,
    int DeliverySafetyMarginMinutes,
    string? InstagramUrl,
    string? FacebookUrl,
    string? TikTokUrl,
    string? TwitterUrl
) : IRequest<EstabelecimentoDto>;

public class UpsertEstabelecimentoValidator : AbstractValidator<UpsertEstabelecimentoCommand>
{
    public UpsertEstabelecimentoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Whatsapp).NotEmpty().MaximumLength(20);
        RuleFor(x => x.OpenTime).Matches(@"^\d{2}:\d{2}$");
        RuleFor(x => x.CloseTime).Matches(@"^\d{2}:\d{2}$");
        RuleFor(x => x.PreparationTimeMinutes).InclusiveBetween(5, 240);
        RuleFor(x => x.DeliverySafetyMarginMinutes).InclusiveBetween(0, 90);
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
        est.SendOrderTrackingViaWhatsApp = cmd.SendOrderTrackingViaWhatsApp;
        est.PreparationTimeMinutes = cmd.PreparationTimeMinutes;
        est.DeliverySafetyMarginMinutes = cmd.DeliverySafetyMarginMinutes;
        est.InstagramUrl = string.IsNullOrWhiteSpace(cmd.InstagramUrl) ? null : cmd.InstagramUrl;
        est.FacebookUrl = string.IsNullOrWhiteSpace(cmd.FacebookUrl) ? null : cmd.FacebookUrl;
        est.TikTokUrl = string.IsNullOrWhiteSpace(cmd.TikTokUrl) ? null : cmd.TikTokUrl;
        est.TwitterUrl = string.IsNullOrWhiteSpace(cmd.TwitterUrl) ? null : cmd.TwitterUrl;
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
            est.SendOrderTrackingViaWhatsApp,
            est.PreparationTimeMinutes,
            est.DeliverySafetyMarginMinutes,
            est.InstagramUrl,
            est.FacebookUrl,
            est.TikTokUrl,
            est.TwitterUrl
        );
    }
}
