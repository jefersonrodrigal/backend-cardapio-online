using Application.Common.Interfaces;
using Application.Features.AdditionalGroups.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AdditionalGroups.Commands;

public record UpdateAdditionalGroupCommand(
    Guid GroupId,
    string Name,
    int MinSelections,
    int MaxSelections,
    int SortOrder
) : IRequest<AdditionalGroupDto>;

public class UpdateAdditionalGroupValidator : AbstractValidator<UpdateAdditionalGroupCommand>
{
    public UpdateAdditionalGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinSelections).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSelections).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MaxSelections).GreaterThanOrEqualTo(x => x.MinSelections)
            .WithMessage("MaxSelections deve ser maior ou igual a MinSelections.");
    }
}

public class UpdateAdditionalGroupHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateAdditionalGroupCommand, AdditionalGroupDto>
{
    public async Task<AdditionalGroupDto> Handle(UpdateAdditionalGroupCommand cmd, CancellationToken ct)
    {
        var group = await db.AdditionalGroups
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == cmd.GroupId, ct)
            ?? throw new ValidationException("Grupo de adicionais não encontrado.");

        group.Name = cmd.Name;
        group.MinSelections = cmd.MinSelections;
        group.MaxSelections = cmd.MaxSelections;
        group.SortOrder = cmd.SortOrder;

        await db.SaveChangesAsync(ct);

        return AdditionalGroupDto.FromGroup(group);
    }
}
