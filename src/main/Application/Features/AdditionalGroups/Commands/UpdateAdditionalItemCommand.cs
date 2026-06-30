using Application.Common.Interfaces;
using Application.Features.AdditionalGroups.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Commands;

public record UpdateAdditionalItemCommand(
    Guid ItemId,
    string Name,
    decimal Price,
    bool IsAvailable,
    int SortOrder
) : IRequest<AdditionalItemDto>;

public class UpdateAdditionalItemValidator : AbstractValidator<UpdateAdditionalItemCommand>
{
    public UpdateAdditionalItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class UpdateAdditionalItemHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateAdditionalItemCommand, AdditionalItemDto>
{
    public async Task<AdditionalItemDto> Handle(UpdateAdditionalItemCommand cmd, CancellationToken ct)
    {
        var item = await db.AdditionalItems.FirstOrDefaultAsync(i => i.Id == cmd.ItemId, ct)
            ?? throw new ValidationException("Item adicional não encontrado.");

        item.Name = cmd.Name;
        item.Price = cmd.Price;
        item.IsAvailable = cmd.IsAvailable;
        item.SortOrder = cmd.SortOrder;

        await db.SaveChangesAsync(ct);

        return new AdditionalItemDto(item.Id, item.Name, item.Price, item.IsAvailable, item.SortOrder);
    }
}
