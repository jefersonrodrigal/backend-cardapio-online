using Application.Common.Interfaces;
using Application.Features.AdditionalGroups.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Commands;

public record CreateAdditionalGroupCommand(
    Guid ProductId,
    string Name,
    int MinSelections = 0,
    int MaxSelections = 1,
    int SortOrder = 0
) : IRequest<AdditionalGroupDto>;

public class CreateAdditionalGroupValidator : AbstractValidator<CreateAdditionalGroupCommand>
{
    public CreateAdditionalGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinSelections).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSelections).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MaxSelections).GreaterThanOrEqualTo(x => x.MinSelections)
            .WithMessage("MaxSelections deve ser maior ou igual a MinSelections.");
    }
}

public class CreateAdditionalGroupHandler(IApplicationDbContext db)
    : IRequestHandler<CreateAdditionalGroupCommand, AdditionalGroupDto>
{
    public async Task<AdditionalGroupDto> Handle(CreateAdditionalGroupCommand cmd, CancellationToken ct)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == cmd.ProductId && p.IsActive, ct);
        if (!productExists)
            throw new ValidationException("Produto não encontrado.");

        var group = new AdditionalGroup
        {
            ProductId = cmd.ProductId,
            Name = cmd.Name,
            MinSelections = cmd.MinSelections,
            MaxSelections = cmd.MaxSelections,
            SortOrder = cmd.SortOrder
        };

        db.AdditionalGroups.Add(group);
        await db.SaveChangesAsync(ct);

        return AdditionalGroupDto.FromGroup(group);
    }
}
