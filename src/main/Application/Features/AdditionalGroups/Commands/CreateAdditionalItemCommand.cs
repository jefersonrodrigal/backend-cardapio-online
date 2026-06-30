using Application.Common.Interfaces;
using Application.Features.AdditionalGroups.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Commands;

public record CreateAdditionalItemCommand(
    Guid GroupId,
    string Name,
    decimal Price,
    int SortOrder = 0
) : IRequest<AdditionalItemDto>;

public class CreateAdditionalItemValidator : AbstractValidator<CreateAdditionalItemCommand>
{
    public CreateAdditionalItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreateAdditionalItemHandler(IApplicationDbContext db)
    : IRequestHandler<CreateAdditionalItemCommand, AdditionalItemDto>
{
    public async Task<AdditionalItemDto> Handle(CreateAdditionalItemCommand cmd, CancellationToken ct)
    {
        var groupExists = await db.AdditionalGroups.AnyAsync(g => g.Id == cmd.GroupId, ct);
        if (!groupExists)
            throw new ValidationException("Grupo de adicionais não encontrado.");

        var item = new AdditionalItem
        {
            GroupId = cmd.GroupId,
            Name = cmd.Name,
            Price = cmd.Price,
            SortOrder = cmd.SortOrder
        };

        db.AdditionalItems.Add(item);
        await db.SaveChangesAsync(ct);

        return new AdditionalItemDto(item.Id, item.Name, item.Price, item.IsAvailable, item.SortOrder);
    }
}
